using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.API.DTOs;
using RestaurantCRM.API.Helpers;
using RestaurantCRM.API.Services;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhaoSatController : ControllerBase
    {
        private readonly RestaurantCRM.BusinessLogic.Services.IRestaurantService _service;

        public KhaoSatController(RestaurantCRM.BusinessLogic.Services.IRestaurantService service)
        {
            _service = service;
        }

        /// <summary>
        /// Khách được tham gia khi khảo sát không giới hạn đối tượng (công khai)
        /// hoặc khách có trong danh sách DOI_TUONG_KHAO_SAT của khảo sát.
        /// </summary>
        private static bool DuocThamGia(KhaoSat ks, string? maKhachHang) =>
            !ks.DoiTuongKhaoSat.Any() ||
            (!string.IsNullOrEmpty(maKhachHang) && ks.DoiTuongKhaoSat.Any(d => d.MaKhachHang == maKhachHang));

        // POST: api/KhaoSat/TraLoi - Nhận phiếu trả lời khảo sát từ khách hàng
        [HttpPost("TraLoi")]
        public async Task<IActionResult> SubmitTraLoi(SurveyAnswerRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.MaKhaoSat) || string.IsNullOrWhiteSpace(request.MaKhachHang))
            {
                return BadRequest("Thông tin khảo sát hoặc khách hàng không hợp lệ.");
            }

            var loiKhach = await KhachHangValidator.KiemTraHoatDongAsync(_service, request.MaKhachHang);
            if (loiKhach != null) return BadRequest(loiKhach);

            var khaoSat = await _service.Query<KhaoSat>()
                .Include(k => k.DoiTuongKhaoSat)
                .Include(k => k.CauHoiKhaoSat)
                    .ThenInclude(c => c.TuyChonCauHoi)
                .FirstOrDefaultAsync(k => k.MaKhaoSat == request.MaKhaoSat && k.TrangThai == "Open");
            if (khaoSat == null)
            {
                return BadRequest("Khảo sát không tồn tại hoặc đã đóng.");
            }
            if (khaoSat.LaSoThich)
            {
                return BadRequest("Hồ sơ sở thích được cập nhật ở mục \"Sở thích của tôi\", không gửi như khảo sát.");
            }
            if (!DuocThamGia(khaoSat, request.MaKhachHang))
            {
                return BadRequest("Bạn không thuộc đối tượng của khảo sát này.");
            }

            var daGui = await _service.Query<PhieuTraLoi>()
            .AnyAsync(p => p.MaKhaoSat == request.MaKhaoSat && p.MaKhachHang == request.MaKhachHang);
            if (daGui) return BadRequest("Bạn đã hoàn thành khảo sát này rồi.");

            // Kiểm tra toàn bộ câu trả lời trước khi lưu; sai ở đâu thì không lưu gì
            var (loiTraLoi, cauTraLois) = KiemTraCauTraLoi(khaoSat, request.Answers);
            if (loiTraLoi != null) return BadRequest(loiTraLoi);

            var phieuTraLoi = new PhieuTraLoi
            {
                MaPhieuTraLoi = "PTL" + Guid.NewGuid().ToString().Substring(0, 7).ToUpper(),
                MaKhaoSat = request.MaKhaoSat,
                MaKhachHang = request.MaKhachHang,
                NgayTraLoi = DateTime.Now
            };

            _service.Add(phieuTraLoi);

            foreach (var (maCauHoi, maTuyChon, noiDung) in cauTraLois)
            {
                _service.Add(new CauTraLoi
                {
                    MaCauTraLoi = "CTL" + Guid.NewGuid().ToString().Substring(0, 7).ToUpper(),
                    MaPhieuTraLoi = phieuTraLoi.MaPhieuTraLoi,
                    MaCauHoi = maCauHoi,
                    MaTuyChon = maTuyChon,
                    NoiDungTuDien = noiDung
                });
            }

            // Đánh dấu hoàn thành đối tượng khảo sát nếu khách hàng có trong danh sách
            var doiTuong = khaoSat.DoiTuongKhaoSat.FirstOrDefault(d => d.MaKhachHang == request.MaKhachHang);
            if (doiTuong != null)
            {
                doiTuong.DaHoanThanh = true;
            }

            try
            {
                await _service.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (DbErrorHelper.IsUniqueViolation(ex))
            {
                // Gửi 2 lần cùng lúc: lần sau vướng UNIQUE(maKhaoSat, maKhachHang)
                return BadRequest("Bạn đã hoàn thành khảo sát này rồi.");
            }

            return Ok(new { message = "Gửi phản hồi thành công" });
        }

        private const int DoDaiTuLuanToiDa = 1000;   // khớp ô nhập trên Web

        /// <summary>
        /// Kiểm tra phiếu trả lời. Trả về lỗi đầu tiên, hoặc danh sách dòng CAU_TRA_LOI cần lưu.
        /// Trắc nghiệm: bắt buộc ít nhất 1 đáp án; câu không cho chọn nhiều thì đúng 1; đáp án phải thuộc đúng câu,
        /// không trùng; mỗi đáp án là 1 dòng. Tự luận: không có tùy chọn, trim, tối đa 1000 ký tự; để trống thì không lưu dòng.
        /// </summary>
        private static (string? Loi, List<(string MaCauHoi, string? MaTuyChon, string? NoiDung)> CauTraLois)
            KiemTraCauTraLoi(KhaoSat khaoSat, List<AnswerItem>? answers)
        {
            var ketQua = new List<(string, string?, string?)>();
            if (answers == null || answers.Count == 0)
                return ("Vui lòng trả lời các câu hỏi của khảo sát.", ketQua);

            var cauHois = khaoSat.CauHoiKhaoSat.ToDictionary(c => c.MaCauHoi);
            var daTraLoi = new HashSet<string>();

            foreach (var answer in answers)
            {
                var maCauHoi = answer?.MaCauHoi?.Trim();
                if (string.IsNullOrEmpty(maCauHoi) || !cauHois.TryGetValue(maCauHoi, out var cauHoi))
                    return ("Câu trả lời không thuộc khảo sát này.", ketQua);
                if (!daTraLoi.Add(maCauHoi))
                    return ($"Câu hỏi \"{cauHoi.NoiDungCauHoi}\" bị trả lời trùng.", ketQua);

                // Gom MaTuyChon (kiểu cũ) và MaTuyChons (chọn nhiều)
                var dsChon = new List<string>();
                if (!string.IsNullOrWhiteSpace(answer!.MaTuyChon)) dsChon.Add(answer.MaTuyChon.Trim());
                if (answer.MaTuyChons != null)
                    dsChon.AddRange(answer.MaTuyChons.Where(m => !string.IsNullOrWhiteSpace(m)).Select(m => m.Trim()));

                if (AdminKhaoSatService.ChuanHoaLoai(cauHoi.LoaiCauHoi) == AdminKhaoSatService.TracNghiem)
                {
                    if (dsChon.Count == 0)
                        return ($"Vui lòng chọn đáp án cho câu hỏi \"{cauHoi.NoiDungCauHoi}\".", ketQua);
                    if (dsChon.Distinct().Count() != dsChon.Count)
                        return ($"Câu hỏi \"{cauHoi.NoiDungCauHoi}\" có đáp án bị chọn trùng.", ketQua);
                    if (!cauHoi.ChonNhieu && dsChon.Count > 1)
                        return ($"Câu hỏi \"{cauHoi.NoiDungCauHoi}\" chỉ được chọn 1 đáp án.", ketQua);
                    foreach (var maTuyChon in dsChon)
                    {
                        if (!cauHoi.TuyChonCauHoi.Any(t => t.MaTuyChon == maTuyChon))
                            return ($"Đáp án không hợp lệ cho câu hỏi \"{cauHoi.NoiDungCauHoi}\".", ketQua);
                        ketQua.Add((maCauHoi, maTuyChon, null));
                    }
                }
                else
                {
                    if (dsChon.Count > 0)
                        return ($"Câu hỏi \"{cauHoi.NoiDungCauHoi}\" là câu tự luận, không có lựa chọn.", ketQua);
                    var noiDung = answer.NoiDungTuDien?.Trim();
                    if (noiDung?.Length > DoDaiTuLuanToiDa)
                        return ($"Câu trả lời cho \"{cauHoi.NoiDungCauHoi}\" tối đa {DoDaiTuLuanToiDa} ký tự.", ketQua);
                    if (!string.IsNullOrEmpty(noiDung))
                        ketQua.Add((maCauHoi, null, noiDung));
                }
            }

            var thieu = cauHois.Values.FirstOrDefault(c =>
                AdminKhaoSatService.ChuanHoaLoai(c.LoaiCauHoi) == AdminKhaoSatService.TracNghiem && !daTraLoi.Contains(c.MaCauHoi));
            if (thieu != null)
                return ($"Vui lòng trả lời câu hỏi \"{thieu.NoiDungCauHoi}\".", ketQua);

            return (null, ketQua);
        }
        // GET: api/KhaoSat/DanhSach?maKhachHang=KH01
        [HttpGet("DanhSach")]
        public async Task<IActionResult> GetDanhSach([FromQuery] string? maKhachHang)
        {
            var dsKhaoSat = (await _service.Query<KhaoSat>().ChiKhaoSatThuong()
                .Include(k => k.CauHoiKhaoSat)
                .Include(k => k.DoiTuongKhaoSat)
                .Where(k => k.TrangThai == "Open")
                .OrderByDescending(k => k.NgayTao)
                .ToListAsync())
                .Where(k => DuocThamGia(k, maKhachHang))
                .ToList();

            var daTraLoi = new HashSet<string>();
            if (!string.IsNullOrEmpty(maKhachHang))
            {
                var ds = await _service.Query<PhieuTraLoi>()
                    .Where(p => p.MaKhachHang == maKhachHang)
                    .Select(p => p.MaKhaoSat)
                    .ToListAsync();
                daTraLoi = ds.Where(x => x != null).Select(x => x!).ToHashSet();
            }

            return Ok(dsKhaoSat.Select(k => new
            {
                k.MaKhaoSat,
                k.TieuDe,
                SoCauHoi = k.CauHoiKhaoSat.Count,
                DaTraLoi = daTraLoi.Contains(k.MaKhaoSat)
            }));
        }
        // GET: api/KhaoSat/LichSu?maKhachHang=KH01
        [HttpGet("LichSu")]
        public async Task<IActionResult> GetLichSu([FromQuery] string maKhachHang)
        {
            var phieus = await _service.Query<PhieuTraLoi>()
                .Where(p => p.MaKhachHang == maKhachHang
                            && !(p.MaKhaoSatNavigation != null && p.MaKhaoSatNavigation.LaSoThich))
                .OrderByDescending(p => p.NgayTraLoi)
                .ToListAsync();

            var maKs = phieus.Select(p => p.MaKhaoSat).Distinct().ToList();
            var tieuDe = await _service.Query<KhaoSat>()
                .Where(k => maKs.Contains(k.MaKhaoSat))
                .ToDictionaryAsync(k => k.MaKhaoSat, k => k.TieuDe);

            return Ok(phieus.Select(p => new
            {
                p.MaKhaoSat,
                TieuDe = tieuDe.GetValueOrDefault(p.MaKhaoSat ?? "") ?? "(Khảo sát đã xóa)",
                NgayGui = p.NgayTraLoi
            }));
        }
        // GET: api/KhaoSat/{id}?maKhachHang=
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, [FromQuery] string? maKhachHang)
        {
            var khaoSat = await _service.Query<KhaoSat>()
                .Include(k => k.CauHoiKhaoSat).ThenInclude(c => c.TuyChonCauHoi)
                .Include(k => k.DoiTuongKhaoSat)
                .FirstOrDefaultAsync(k => k.MaKhaoSat == id && k.TrangThai == "Open" && !k.LaSoThich);

            if (khaoSat == null || !DuocThamGia(khaoSat, maKhachHang))
                return NotFound("Khảo sát không tồn tại, đã đóng hoặc bạn không thuộc đối tượng khảo sát.");

            return Ok(new
            {
                khaoSat.MaKhaoSat,
                khaoSat.TieuDe,
                CauHoi = AdminKhaoSatService.SapXep(khaoSat.CauHoiKhaoSat, c => c.MaCauHoi).Select(c => new
                {
                    c.MaCauHoi,
                    c.NoiDungCauHoi,
                    c.LoaiCauHoi,
                    c.ChonNhieu,
                    TuyChon = AdminKhaoSatService.SapXep(c.TuyChonCauHoi, t => t.MaTuyChon).Select(t => new { t.MaTuyChon, t.NoiDungTuyChon })
                })
            });
        }

        // ── HỒ SƠ SỞ THÍCH (khách tự khai báo) ───────────────────────────────

        // GET: api/KhaoSat/SoThich?maKhachHang= - Danh mục sở thích + lựa chọn hiện tại của khách
        [HttpGet("SoThich")]
        public async Task<IActionResult> GetSoThich([FromQuery] string? maKhachHang)
        {
            var bo = await SoThichService.LayBoSoThichAsync(_service);
            if (bo == null) return Ok(new { coHoSo = false });

            PhieuTraLoi? phieu = null;
            if (!string.IsNullOrWhiteSpace(maKhachHang))
            {
                phieu = await _service.Query<PhieuTraLoi>()
                    .Include(p => p.CauTraLoi)
                    .FirstOrDefaultAsync(p => p.MaKhaoSat == bo.MaKhaoSat && p.MaKhachHang == maKhachHang);
            }
            var cauTraLois = phieu?.CauTraLoi.ToList() ?? new List<CauTraLoi>();
            var daChon = cauTraLois.Where(c => c.MaTuyChon != null).Select(c => c.MaTuyChon!).ToHashSet();
            var cauMon = SoThichService.TimCauMonYeuThich(bo);

            return Ok(new
            {
                coHoSo = true,
                bo.MaKhaoSat,
                bo.TieuDe,
                coMonYeuThich = cauMon != null,
                maCauMonYeuThich = cauMon?.MaCauHoi,
                daKhaiBao = phieu != null,
                ngayCapNhat = phieu?.NgayTraLoi,
                cauHoi = AdminKhaoSatService.SapXep(bo.CauHoiKhaoSat, c => c.MaCauHoi).Select(c => new
                {
                    c.MaCauHoi,
                    c.NoiDungCauHoi,
                    LoaiCauHoi = AdminKhaoSatService.ChuanHoaLoai(c.LoaiCauHoi) ?? c.LoaiCauHoi,
                    c.ChonNhieu,
                    laMonYeuThich = c == cauMon,
                    laDiUng = SoThichService.LaCauDiUng(c.NoiDungCauHoi),
                    // Món ngừng bán chỉ hiện khi khách đang chọn (để còn bỏ chọn được)
                    tuyChon = AdminKhaoSatService.SapXep(c.TuyChonCauHoi, t => t.MaTuyChon)
                        .Where(t => t.MaMonNavigation == null || !SoThichService.NgungBan(t.MaMonNavigation.TrangThai) || daChon.Contains(t.MaTuyChon))
                        .Select(t => new
                        {
                            t.MaTuyChon,
                            t.NoiDungTuyChon,
                            t.MaMon,
                            tenMon = t.MaMonNavigation?.TenMon,
                            duongDanAnh = t.MaMonNavigation?.DuongDanAnh,
                            ngungBan = t.MaMonNavigation != null && SoThichService.NgungBan(t.MaMonNavigation.TrangThai),
                            daChon = daChon.Contains(t.MaTuyChon)
                        }),
                    noiDung = cauTraLois.FirstOrDefault(x => x.MaCauHoi == c.MaCauHoi && x.MaTuyChon == null)?.NoiDungTuDien
                })
            });
        }

        // GET: api/KhaoSat/SoThich/MonYeuThich?maKhachHang= - Các món khách đang ♥ (để tô nút ♥ trên trang món)
        [HttpGet("SoThich/MonYeuThich")]
        public async Task<IActionResult> GetMonYeuThich([FromQuery] string? maKhachHang)
        {
            var bo = await SoThichService.LayBoSoThichAsync(_service);
            var cauMon = bo == null ? null : SoThichService.TimCauMonYeuThich(bo);
            if (cauMon == null) return Ok(new { coMonYeuThich = false, maMons = new List<string>() });

            var maMons = string.IsNullOrWhiteSpace(maKhachHang)
                ? new List<string>()
                : await _service.Query<CauTraLoi>()
                    .Where(c => c.MaCauHoi == cauMon.MaCauHoi && c.MaTuyChonNavigation != null && c.MaTuyChonNavigation.MaMon != null
                                && c.MaPhieuTraLoiNavigation != null && c.MaPhieuTraLoiNavigation.MaKhachHang == maKhachHang)
                    .Select(c => c.MaTuyChonNavigation!.MaMon!)
                    .Distinct()
                    .ToListAsync();
            return Ok(new { coMonYeuThich = true, maMons });
        }

        // PUT: api/KhaoSat/SoThich - Lưu (ghi đè) hồ sơ sở thích của khách (R8, R9)
        [HttpPut("SoThich")]
        public async Task<IActionResult> LuuSoThich([FromBody] SoThichLuuRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.MaKhachHang))
                return BadRequest("Thiếu mã khách hàng.");
            var maKhachHang = request.MaKhachHang.Trim();
            var loiKhach = await KhachHangValidator.KiemTraHoatDongAsync(_service, maKhachHang);
            if (loiKhach != null) return BadRequest(loiKhach);

            var bo = await SoThichService.LayBoSoThichAsync(_service);
            if (bo == null) return BadRequest("Nhà hàng chưa mở hồ sơ sở thích.");

            var phieu = await _service.Query<PhieuTraLoi>()
                .Include(p => p.CauTraLoi)
                .FirstOrDefaultAsync(p => p.MaKhaoSat == bo.MaKhaoSat && p.MaKhachHang == maKhachHang);
            var daChonTruoc = phieu?.CauTraLoi.Where(c => c.MaTuyChon != null).Select(c => c.MaTuyChon!).ToHashSet() ?? new HashSet<string>();

            // Kiểm tra toàn bộ trước khi ghi (R9). Mọi câu đều không bắt buộc (R7).
            var cauHois = bo.CauHoiKhaoSat.ToDictionary(c => c.MaCauHoi);
            var cauMon = SoThichService.TimCauMonYeuThich(bo);
            var dong = new List<(string MaCauHoi, string? MaTuyChon, string? NoiDung)>();
            var daGap = new HashSet<string>();
            foreach (var item in request.CauTraLoi ?? new List<SoThichCauTraLoi>())
            {
                var maCauHoi = item?.MaCauHoi?.Trim();
                if (string.IsNullOrEmpty(maCauHoi) || !cauHois.TryGetValue(maCauHoi, out var cauHoi))
                    return BadRequest("Câu trả lời không thuộc hồ sơ sở thích.");
                if (!daGap.Add(maCauHoi))
                    return BadRequest($"Câu \"{cauHoi.NoiDungCauHoi}\" bị gửi trùng.");

                var ds = (item!.MaTuyChons ?? new List<string>())
                    .Where(m => !string.IsNullOrWhiteSpace(m)).Select(m => m.Trim()).ToList();

                if (AdminKhaoSatService.ChuanHoaLoai(cauHoi.LoaiCauHoi) == AdminKhaoSatService.TracNghiem)
                {
                    if (ds.Distinct().Count() != ds.Count)
                        return BadRequest($"Câu \"{cauHoi.NoiDungCauHoi}\" có đáp án bị chọn trùng.");
                    if (!cauHoi.ChonNhieu && ds.Count > 1)
                        return BadRequest($"Câu \"{cauHoi.NoiDungCauHoi}\" chỉ được chọn 1 đáp án.");
                    if (cauHoi == cauMon && ds.Count > SoThichService.SoMonYeuThichToiDa)
                        return BadRequest($"Chỉ được chọn tối đa {SoThichService.SoMonYeuThichToiDa} món yêu thích.");
                    foreach (var ma in ds)
                    {
                        var tc = cauHoi.TuyChonCauHoi.FirstOrDefault(t => t.MaTuyChon == ma);
                        if (tc == null)
                            return BadRequest($"Đáp án không hợp lệ cho câu \"{cauHoi.NoiDungCauHoi}\".");
                        // Món ngừng bán: giữ được nếu đã chọn từ trước, không chọn mới được (R10)
                        if (tc.MaMonNavigation != null && !SoThichService.DangKinhDoanh(tc.MaMonNavigation.TrangThai) && !daChonTruoc.Contains(ma))
                            return BadRequest($"Món \"{tc.MaMonNavigation.TenMon}\" đã ngừng bán, không thể thêm vào yêu thích.");
                        dong.Add((maCauHoi, ma, null));
                    }
                }
                else
                {
                    if (ds.Count > 0)
                        return BadRequest($"Câu \"{cauHoi.NoiDungCauHoi}\" là câu tự luận, không có lựa chọn.");
                    var noiDung = item.NoiDung?.Trim();
                    if (noiDung?.Length > SoThichService.DoDaiGhiChuToiDa)
                        return BadRequest($"Câu \"{cauHoi.NoiDungCauHoi}\" tối đa {SoThichService.DoDaiGhiChuToiDa} ký tự.");
                    if (!string.IsNullOrEmpty(noiDung))
                        dong.Add((maCauHoi, null, noiDung));
                }
            }

            // Ghi đè trên chính phiếu của khách, trong 1 transaction (R8)
            try
            {
                using var transaction = await _service.BeginTransactionAsync();
                if (phieu != null)
                {
                    _service.RemoveRange(phieu.CauTraLoi.ToList());
                    if (dong.Count == 0)
                        _service.Remove(phieu);   // không còn gì: quay về "chưa khai báo"
                    else
                        phieu.NgayTraLoi = DateTime.Now;
                    // Lưu phần xóa trước để không vướng UNIQUE (maPhieuTraLoi, maTuyChon) khi thêm lại cùng đáp án
                    await _service.SaveChangesAsync();
                }

                if (dong.Count > 0)
                {
                    if (phieu == null)
                    {
                        phieu = new PhieuTraLoi
                        {
                            MaPhieuTraLoi = SoThichService.MaPhieuMoi(),
                            MaKhaoSat = bo.MaKhaoSat,
                            MaKhachHang = maKhachHang,
                            NgayTraLoi = DateTime.Now
                        };
                        _service.Add(phieu);
                    }
                    foreach (var (maCauHoi, maTuyChon, noiDung) in dong)
                    {
                        _service.Add(new CauTraLoi
                        {
                            MaCauTraLoi = SoThichService.MaCauTraLoiMoi(),
                            MaPhieuTraLoi = phieu.MaPhieuTraLoi,
                            MaCauHoi = maCauHoi,
                            MaTuyChon = maTuyChon,
                            NoiDungTuDien = noiDung
                        });
                    }
                    await _service.SaveChangesAsync();
                }
                await transaction.CommitAsync();
            }
            catch (DbUpdateException ex) when (DbErrorHelper.IsUniqueViolation(ex))
            {
                return BadRequest("Hồ sơ sở thích vừa được cập nhật ở nơi khác, vui lòng tải lại trang và thử lại.");
            }

            return Ok(new
            {
                message = dong.Count == 0 ? "Đã xóa hồ sơ sở thích của bạn." : "Đã lưu sở thích của bạn.",
                daKhaiBao = dong.Count > 0,
                ngayCapNhat = dong.Count > 0 ? phieu?.NgayTraLoi : null
            });
        }

        // PUT: api/KhaoSat/SoThich/MonYeuThich/{maMon} - Bật / tắt ♥ một món (R10)
        [HttpPut("SoThich/MonYeuThich/{maMon}")]
        public async Task<IActionResult> DatMonYeuThich(string maMon, [FromBody] MonYeuThichRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.MaKhachHang))
                return BadRequest("Thiếu mã khách hàng.");
            var maKhachHang = request.MaKhachHang.Trim();
            var loiKhach = await KhachHangValidator.KiemTraHoatDongAsync(_service, maKhachHang);
            if (loiKhach != null) return BadRequest(loiKhach);

            var bo = await SoThichService.LayBoSoThichAsync(_service);
            var cauMon = bo == null ? null : SoThichService.TimCauMonYeuThich(bo);
            if (bo == null || cauMon == null)
                return BadRequest("Nhà hàng chưa mở mục \"Món yêu thích\".");

            var mon = await _service.Query<MonAn>().FirstOrDefaultAsync(m => m.MaMon == maMon);
            if (mon == null) return NotFound("Không tìm thấy món ăn.");

            var tuyChon = cauMon.TuyChonCauHoi.FirstOrDefault(t => t.MaMon == maMon);
            var phieu = await _service.Query<PhieuTraLoi>()
                .Include(p => p.CauTraLoi)
                .FirstOrDefaultAsync(p => p.MaKhaoSat == bo.MaKhaoSat && p.MaKhachHang == maKhachHang);
            var dongHienCo = tuyChon == null ? null : phieu?.CauTraLoi.FirstOrDefault(c => c.MaTuyChon == tuyChon.MaTuyChon);

            if (request.YeuThich)
            {
                if (dongHienCo != null) return Ok(new { yeuThich = true, message = "Món đã có trong danh sách yêu thích." });
                if (!SoThichService.DangKinhDoanh(mon.TrangThai))
                    return BadRequest("Món này đã ngừng bán, không thể thêm vào yêu thích.");
                int soMon = phieu?.CauTraLoi.Count(c => c.MaCauHoi == cauMon.MaCauHoi && c.MaTuyChon != null) ?? 0;
                if (soMon >= SoThichService.SoMonYeuThichToiDa)
                    return BadRequest($"Bạn chỉ được lưu tối đa {SoThichService.SoMonYeuThichToiDa} món yêu thích.");

                // Chưa có đáp án cho món này: tự tạo trong câu "Món yêu thích"
                if (tuyChon == null)
                    tuyChon = await SoThichService.TaoDapAnMonAsync(_service, cauMon, mon);
                if (phieu == null)
                {
                    phieu = new PhieuTraLoi
                    {
                        MaPhieuTraLoi = SoThichService.MaPhieuMoi(),
                        MaKhaoSat = bo.MaKhaoSat,
                        MaKhachHang = maKhachHang
                    };
                    _service.Add(phieu);
                }
                phieu.NgayTraLoi = DateTime.Now;
                _service.Add(new CauTraLoi
                {
                    MaCauTraLoi = SoThichService.MaCauTraLoiMoi(),
                    MaPhieuTraLoi = phieu.MaPhieuTraLoi,
                    MaCauHoi = cauMon.MaCauHoi,
                    MaTuyChon = tuyChon.MaTuyChon
                });
            }
            else
            {
                if (dongHienCo == null || phieu == null) return Ok(new { yeuThich = false, message = "Món không có trong danh sách yêu thích." });
                _service.Remove(dongHienCo);
                // Phiếu rỗng thì xóa luôn: khách quay về "chưa khai báo" (R8)
                if (phieu.CauTraLoi.Count == 1)
                    _service.Remove(phieu);
                else
                    phieu.NgayTraLoi = DateTime.Now;
            }

            try
            {
                await _service.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (DbErrorHelper.IsUniqueViolation(ex))
            {
                return BadRequest("Thao tác vừa được thực hiện ở nơi khác, vui lòng tải lại trang và thử lại.");
            }

            return Ok(new
            {
                yeuThich = request.YeuThich,
                message = request.YeuThich ? "Đã thêm vào món yêu thích." : "Đã bỏ khỏi món yêu thích."
            });
        }
    }
}

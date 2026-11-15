using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.API.DTOs;
using RestaurantCRM.API.Helpers;
using RestaurantCRM.API.Services;

namespace RestaurantCRM.API.Controllers
{
    /// <summary>
    /// Quản trị khảo sát: khảo sát, câu hỏi, tùy chọn, mở/đóng, thống kê.
    /// Giá trị lưu trong CSDL phải khớp ràng buộc CHECK của script Database/RestaurantCRM.sql:
    ///   KHAO_SAT.trangThai         : 'Draft' | 'Open' | 'Closed'
    ///   CAU_HOI_KHAO_SAT.loaiCauHoi : 'Option' (trắc nghiệm) | 'Text' (tự luận)
    /// API vẫn nhận các tên cũ ('Active', 'TracNghiem', 'TuLuan') và tự quy đổi.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AdminKhaoSatController : ControllerBase
    {
        public const string Nhap = AdminKhaoSatService.Nhap;
        public const string DangMo = AdminKhaoSatService.DangMo;
        public const string DaDong = AdminKhaoSatService.DaDong;
        public const string TracNghiem = AdminKhaoSatService.TracNghiem;
        public const string TuLuan = AdminKhaoSatService.TuLuan;

        private readonly RestaurantCRM.BusinessLogic.Services.IRestaurantService _service;

        public AdminKhaoSatController(RestaurantCRM.BusinessLogic.Services.IRestaurantService service)
        {
            _service = service;
        }

        // ── KHẢO SÁT ─────────────────────────────────────────────────────────

        // GET: api/AdminKhaoSat?search=&trangThai= - Danh sách khảo sát
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? trangThai)
        {
            // Bộ hồ sơ sở thích có màn hình riêng, không nằm trong danh sách khảo sát thường
            var query = _service.Query<KhaoSat>().ChiKhaoSatThuong();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(k => k.MaKhaoSat.Contains(s) || (k.TieuDe != null && k.TieuDe.Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                var tt = AdminKhaoSatService.ChuanHoaTrangThai(trangThai);
                if (tt == null) return BadRequest("Trạng thái lọc không hợp lệ. Chấp nhận: Draft, Open, Closed.");
                query = query.Where(k => k.TrangThai == tt);
            }

            var khaoSats = await query
                .OrderByDescending(k => k.NgayTao)
                .Select(k => new
                {
                    k.MaKhaoSat,
                    k.TieuDe,
                    k.TrangThai,
                    k.NgayTao,
                    k.MaNhanVien,
                    NguoiTao = k.MaNhanVienNavigation != null ? k.MaNhanVienNavigation.HoTen : null,
                    SoCauHoi = k.CauHoiKhaoSat.Count(),
                    SoNguoiTraLoi = k.PhieuTraLoi.Count(),
                    SoDoiTuong = k.DoiTuongKhaoSat.Count()
                })
                .ToListAsync();

            return Ok(khaoSats);
        }

        // GET: api/AdminKhaoSat/{id} - Chi tiết khảo sát kèm câu hỏi và tùy chọn (dùng cho sửa và xem trước)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var khaoSat = await _service.Query<KhaoSat>()
                .Include(k => k.CauHoiKhaoSat)
                    .ThenInclude(c => c.TuyChonCauHoi)
                        .ThenInclude(t => t.MaMonNavigation)
                .FirstOrDefaultAsync(k => k.MaKhaoSat == id);

            if (khaoSat == null)
            {
                return NotFound("Không tìm thấy khảo sát.");
            }

            var soPhieu = await _service.Query<PhieuTraLoi>().CountAsync(p => p.MaKhaoSat == id);
            var soDoiTuongKs = await _service.Query<DoiTuongKhaoSat>().CountAsync(d => d.MaKhaoSat == id);
            var cauHoiDaCoTraLoi = await _service.Query<CauTraLoi>()
                .Where(c => c.MaCauHoiNavigation != null && c.MaCauHoiNavigation.MaKhaoSat == id)
                .Select(c => c.MaCauHoi)
                .Distinct()
                .ToListAsync();
            var tuyChonDaChon = (await _service.Query<CauTraLoi>()
                .Where(c => c.MaTuyChon != null && c.MaCauHoiNavigation != null && c.MaCauHoiNavigation.MaKhaoSat == id)
                .Select(c => c.MaTuyChon!)
                .Distinct()
                .ToListAsync()).ToHashSet();

            var result = new
            {
                khaoSat.MaKhaoSat,
                khaoSat.TieuDe,
                khaoSat.TrangThai,
                khaoSat.NgayTao,
                khaoSat.MaNhanVien,
                khaoSat.LaSoThich,
                SoNguoiTraLoi = soPhieu,
                DaCoPhieu = soPhieu > 0,
                SoDoiTuong = soDoiTuongKs,
                CauHoi = AdminKhaoSatService.SapXep(khaoSat.CauHoiKhaoSat, c => c.MaCauHoi).Select(c => new
                {
                    c.MaCauHoi,
                    c.NoiDungCauHoi,
                    LoaiCauHoi = AdminKhaoSatService.ChuanHoaLoai(c.LoaiCauHoi) ?? c.LoaiCauHoi,
                    c.ChonNhieu,
                    LaCauMonYeuThich = SoThichService.LaCauMonYeuThich(c),
                    DaCoTraLoi = cauHoiDaCoTraLoi.Contains(c.MaCauHoi),
                    TuyChon = AdminKhaoSatService.SapXep(c.TuyChonCauHoi, t => t.MaTuyChon).Select(t => new
                    {
                        t.MaTuyChon,
                        t.NoiDungTuyChon,
                        t.MaMon,
                        TenMon = t.MaMonNavigation?.TenMon,
                        NgungBan = t.MaMonNavigation != null && SoThichService.NgungBan(t.MaMonNavigation.TrangThai),
                        DaCoNguoiChon = tuyChonDaChon.Contains(t.MaTuyChon)
                    })
                })
            };

            return Ok(result);
        }

        // POST: api/AdminKhaoSat - Tạo khảo sát mới (trạng thái Nháp), có thể kèm danh sách câu hỏi
        [HttpPost]
        public async Task<IActionResult> Create(KhaoSatCreateRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.TieuDe))
                return BadRequest("Tiêu đề khảo sát không được để trống.");
            if (request.TieuDe.Trim().Length > 200)
                return BadRequest("Tiêu đề khảo sát tối đa 200 ký tự.");

            var cauHois = request.CauHois ?? new List<CauHoiCreateRequest>();
            foreach (var ch in cauHois)
            {
                var loi = AdminKhaoSatService.KiemTraCauHoi(ch.NoiDungCauHoi, ch.LoaiCauHoi, ch.TuyChons, ch.ChonNhieu);
                if (loi != null) return BadRequest(loi);
            }

            string? maNhanVien = string.IsNullOrWhiteSpace(request.MaNhanVien) ? null : request.MaNhanVien.Trim();
            if (maNhanVien != null && !await _service.Query<NhanVien>().AnyAsync(n => n.MaNhanVien == maNhanVien))
                maNhanVien = null;

            var khaoSat = new KhaoSat
            {
                MaKhaoSat = await MaTiepTheoAsync<KhaoSat>("KS", k => k.MaKhaoSat.StartsWith("KS"), k => k.MaKhaoSat),
                TieuDe = request.TieuDe.Trim(),
                MaNhanVien = maNhanVien,
                TrangThai = Nhap,
                NgayTao = DateTime.Now
            };
            _service.Add(khaoSat);

            int soCH = await SoTiepTheoAsync<CauHoiKhaoSat>("CH", c => c.MaCauHoi.StartsWith("CH"), c => c.MaCauHoi);
            int soTC = await SoTiepTheoAsync<TuyChonCauHoi>("TC", t => t.MaTuyChon.StartsWith("TC"), t => t.MaTuyChon);

            foreach (var reqCauHoi in cauHois)
            {
                var loai = AdminKhaoSatService.ChuanHoaLoai(reqCauHoi.LoaiCauHoi)!;
                var cauHoi = new CauHoiKhaoSat
                {
                    MaCauHoi = $"CH{soCH++:D2}",
                    MaKhaoSat = khaoSat.MaKhaoSat,
                    NoiDungCauHoi = reqCauHoi.NoiDungCauHoi.Trim(),
                    LoaiCauHoi = loai,
                    ChonNhieu = loai == TracNghiem && reqCauHoi.ChonNhieu
                };
                _service.Add(cauHoi);

                if (loai == TracNghiem)
                {
                    foreach (var tc in AdminKhaoSatService.LamSachTuyChon(reqCauHoi.TuyChons))
                    {
                        _service.Add(new TuyChonCauHoi { MaTuyChon = $"TC{soTC++:D2}", MaCauHoi = cauHoi.MaCauHoi, NoiDungTuyChon = tc });
                    }
                }
            }

            await _service.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = khaoSat.MaKhaoSat }, new { message = "Tạo khảo sát thành công.", MaKhaoSat = khaoSat.MaKhaoSat });
        }

        // PUT: api/AdminKhaoSat/{id} - Cập nhật tiêu đề khảo sát
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] KhaoSatUpdateRequest request)
        {
            var khaoSat = await _service.FindAsync<KhaoSat>(id);
            if (khaoSat == null)
                return NotFound("Không tìm thấy khảo sát.");

            if (request == null || string.IsNullOrWhiteSpace(request.TieuDe))
                return BadRequest("Tiêu đề không được để trống.");
            if (request.TieuDe.Trim().Length > 200)
                return BadRequest("Tiêu đề khảo sát tối đa 200 ký tự.");

            khaoSat.TieuDe = request.TieuDe.Trim();
            await _service.SaveChangesAsync();

            return Ok(new { message = "Cập nhật khảo sát thành công." });
        }

        // PUT: api/AdminKhaoSat/{id}/status - Mở / đóng / chuyển về nháp. Body: "Open" | "Closed" | "Draft"
        [HttpPut("{id}/status")]
        public async Task<IActionResult> ChangeStatus(string id, [FromBody] string status)
        {
            var khaoSat = await _service.Query<KhaoSat>()
                .Include(k => k.CauHoiKhaoSat)
                    .ThenInclude(c => c.TuyChonCauHoi)
                .FirstOrDefaultAsync(k => k.MaKhaoSat == id);
            if (khaoSat == null)
            {
                return NotFound("Không tìm thấy khảo sát.");
            }
            if (khaoSat.LaSoThich)
                return BadRequest(SoThichService.ThongBaoKhongDoiTrangThai);

            var moi = AdminKhaoSatService.ChuanHoaTrangThai(status);
            if (moi == null)
            {
                return BadRequest("Trạng thái không hợp lệ. Chấp nhận: Draft, Open, Closed.");
            }

            if (moi == DangMo)
            {
                // Chỉ mở khi khảo sát đã đủ nội dung để khách trả lời
                if (!khaoSat.CauHoiKhaoSat.Any())
                    return BadRequest("Khảo sát chưa có câu hỏi nào, không thể mở.");
                var thieu = khaoSat.CauHoiKhaoSat.FirstOrDefault(c => AdminKhaoSatService.ChuanHoaLoai(c.LoaiCauHoi) == TracNghiem && c.TuyChonCauHoi.Count < 2);
                if (thieu != null)
                    return BadRequest($"Câu hỏi \"{thieu.NoiDungCauHoi}\" là trắc nghiệm nhưng có ít hơn 2 tùy chọn.");
            }
            else if (moi == Nhap && await _service.Query<PhieuTraLoi>().AnyAsync(p => p.MaKhaoSat == id))
            {
                return BadRequest("Khảo sát đã có người trả lời, không thể chuyển về nháp. Bạn có thể đóng khảo sát.");
            }

            khaoSat.TrangThai = moi;
            await _service.SaveChangesAsync();

            string thongBao = moi == DangMo ? "Đã mở khảo sát." : moi == DaDong ? "Đã đóng khảo sát." : "Đã chuyển khảo sát về nháp.";
            return Ok(new { message = thongBao, trangThai = moi });
        }

        // DELETE: api/AdminKhaoSat/{id} - Xóa khảo sát (tự động đóng nếu đã có phiếu trả lời, hoặc xóa vĩnh viễn nếu chưa có)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var khaoSat = await _service.FindAsync<KhaoSat>(id);
            if (khaoSat == null)
            {
                return NotFound("Không tìm thấy khảo sát.");
            }
            if (khaoSat.LaSoThich)
                return BadRequest(SoThichService.ThongBaoKhongXoaDuoc);

            bool hasAnswers =await _service.Query<PhieuTraLoi>().AnyAsync(p => p.MaKhaoSat == id);
            if (hasAnswers)
            {
                if (khaoSat.TrangThai == DaDong)
                {
                    return BadRequest("Khảo sát này đã ở trạng thái 'Đóng' và đã có người tham gia trả lời. Hệ thống bảo lưu toàn bộ dữ liệu thống kê và không xóa khỏi cơ sở dữ liệu.");
                }

                // Tự động đóng khảo sát
                khaoSat.TrangThai = DaDong;
                _service.Update(khaoSat);
                await _service.SaveChangesAsync();

                return Ok(new { message = "Khảo sát đã có người tham gia trả lời. Hệ thống đã tự động Đóng khảo sát để bảo toàn dữ liệu thống kê.", trangThai = DaDong });
            }

            var cauHois = await _service.Query<CauHoiKhaoSat>().Include(c => c.TuyChonCauHoi).Where(c => c.MaKhaoSat == id).ToListAsync();
            foreach (var ch in cauHois)
            {
                _service.RemoveRange(ch.TuyChonCauHoi);
            }
            _service.RemoveRange(cauHois);

            var doiTuongs = await _service.Query<DoiTuongKhaoSat>().Where(d => d.MaKhaoSat == id).ToListAsync();
            _service.RemoveRange(doiTuongs);

            _service.Remove(khaoSat);
            await _service.SaveChangesAsync();

            return Ok(new { message = "Đã xóa vĩnh viễn khảo sát khỏi hệ thống." });
        }

        // ── CÂU HỎI ──────────────────────────────────────────────────────────
        // Bộ hồ sơ sở thích (R3): được thêm / sửa câu hỏi, đáp án dù đã có phiếu; vẫn không được xóa đáp án
        // đã có khách chọn, xóa câu hỏi đã có câu trả lời, đổi loại câu hỏi đã có câu trả lời.

        // POST: api/AdminKhaoSat/{id}/CauHoi - Thêm câu hỏi vào khảo sát
        [HttpPost("{id}/CauHoi")]
        public async Task<IActionResult> AddCauHoi(string id, [FromBody] CauHoiUpsertRequest request)
        {
            var khaoSat = await _service.FindAsync<KhaoSat>(id);
            if (khaoSat == null)
                return NotFound("Không tìm thấy khảo sát.");
            var lyDoKhoa = await LyDoKhoaNoiDungAsync(id, chanKhiCoPhieu: true);
            if (lyDoKhoa != null) return BadRequest(lyDoKhoa);

            if (request == null) return BadRequest("Dữ liệu câu hỏi không hợp lệ.");
            bool laCauMon = request.LaCauMonYeuThich;
            string? loi;
            List<MonAn> mons = new();
            if (laCauMon)
            {
                (loi, mons) = await KiemTraCauMonYeuThichAsync(khaoSat, null, request);
                if (loi != null) return BadRequest(loi);
            }
            else
            {
                loi = AdminKhaoSatService.KiemTraCauHoi(request.NoiDung, request.LoaiCauHoi, request.TuyChons, request.ChonNhieu);
                if (loi != null) return BadRequest(loi);
            }

            var loai = laCauMon ? TracNghiem : AdminKhaoSatService.ChuanHoaLoai(request.LoaiCauHoi)!;
            var maCauHoi = await MaTiepTheoAsync<CauHoiKhaoSat>("CH", c => c.MaCauHoi.StartsWith("CH"), c => c.MaCauHoi);
            _service.Add(new CauHoiKhaoSat
            {
                MaCauHoi = maCauHoi,
                MaKhaoSat = id,
                NoiDungCauHoi = request.NoiDung.Trim(),
                LoaiCauHoi = loai,
                ChonNhieu = laCauMon || (loai == TracNghiem && request.ChonNhieu)
            });

            if (loai == TracNghiem)
            {
                int soTC = await SoTiepTheoAsync<TuyChonCauHoi>("TC", t => t.MaTuyChon.StartsWith("TC"), t => t.MaTuyChon);
                if (laCauMon)
                {
                    foreach (var m in mons)
                        _service.Add(new TuyChonCauHoi { MaTuyChon = $"TC{soTC++:D2}", MaCauHoi = maCauHoi, NoiDungTuyChon = SoThichService.TenDapAnMon(m), MaMon = m.MaMon });
                }
                else
                {
                    foreach (var nd in AdminKhaoSatService.LamSachTuyChon(request.TuyChons))
                        _service.Add(new TuyChonCauHoi { MaTuyChon = $"TC{soTC++:D2}", MaCauHoi = maCauHoi, NoiDungTuyChon = nd });
                }
            }

            await _service.SaveChangesAsync();
            return Ok(new { message = "Thêm câu hỏi thành công.", MaCauHoi = maCauHoi });
        }

        // PUT: api/AdminKhaoSat/{id}/CauHoi/{cauHoiId} - Sửa câu hỏi (nội dung, loại, chọn nhiều, danh sách tùy chọn)
        // Tùy chọn được đối chiếu theo nội dung (câu món yêu thích: theo mã món): giữ tùy chọn trùng, thêm tùy chọn mới,
        // xóa tùy chọn bị bỏ (không xóa được tùy chọn đã có người chọn). Đổi chữ một tùy chọn: dùng PUT .../TuyChon/{id}.
        [HttpPut("{id}/CauHoi/{cauHoiId}")]
        public async Task<IActionResult> UpdateCauHoi(string id, string cauHoiId, [FromBody] CauHoiUpsertRequest request)
        {
            var khaoSat = await _service.FindAsync<KhaoSat>(id);
            var cauHoi = await _service.Query<CauHoiKhaoSat>()
                .Include(c => c.TuyChonCauHoi)
                .FirstOrDefaultAsync(c => c.MaCauHoi == cauHoiId && c.MaKhaoSat == id);

            if (khaoSat == null || cauHoi == null)
                return NotFound("Không tìm thấy câu hỏi.");
            if (request == null) return BadRequest("Dữ liệu câu hỏi không hợp lệ.");

            bool laCauMonCu = cauHoi.TuyChonCauHoi.Any(t => t.MaMon != null);
            bool laCauMon = request.LaCauMonYeuThich;
            if (laCauMonCu && !laCauMon)
                return BadRequest("Không thể bỏ vai trò \"Món yêu thích\" của câu hỏi này. Nếu chưa có ai chọn, hãy xóa câu hỏi.");

            string? loi;
            List<MonAn> mons = new();
            if (laCauMon)
            {
                (loi, mons) = await KiemTraCauMonYeuThichAsync(khaoSat, cauHoiId, request);
                if (loi != null) return BadRequest(loi);
            }
            else
            {
                loi = AdminKhaoSatService.KiemTraCauHoi(request.NoiDung, request.LoaiCauHoi, request.TuyChons, request.ChonNhieu);
                if (loi != null) return BadRequest(loi);
            }

            var lyDoKhoa = await LyDoKhoaNoiDungAsync(id, chanKhiCoPhieu: false);
            if (lyDoKhoa != null) return BadRequest(lyDoKhoa);

            var loaiMoi = laCauMon ? TracNghiem : AdminKhaoSatService.ChuanHoaLoai(request.LoaiCauHoi)!;
            var loaiCu = AdminKhaoSatService.ChuanHoaLoai(cauHoi.LoaiCauHoi);
            bool chonNhieuMoi = laCauMon || (loaiMoi == TracNghiem && request.ChonNhieu);
            bool daCoTraLoi = await _service.Query<CauTraLoi>().AnyAsync(ct => ct.MaCauHoi == cauHoiId);
            if (daCoTraLoi && loaiMoi != loaiCu)
                return BadRequest("Câu hỏi đã có người trả lời nên không thể đổi loại câu hỏi.");

            // Tắt chọn nhiều: không phiếu nào được đang chọn quá 1 đáp án ở câu này
            if (cauHoi.ChonNhieu && !chonNhieuMoi)
            {
                bool coPhieuNhieuDapAn = await _service.Query<CauTraLoi>()
                    .Where(ct => ct.MaCauHoi == cauHoiId && ct.MaTuyChon != null)
                    .GroupBy(ct => ct.MaPhieuTraLoi)
                    .Where(g => g.Count() > 1)
                    .AnyAsync();
                if (coPhieuNhieuDapAn)
                    return BadRequest("Đã có phiếu chọn nhiều hơn 1 đáp án ở câu này nên không thể tắt \"Cho chọn nhiều đáp án\".");
            }

            // Xác định tùy chọn cần xóa / thêm
            List<TuyChonCauHoi> canXoa;
            List<TuyChonCauHoi> themMoi = new();
            if (laCauMon)
            {
                var maMonGiu = mons.Select(m => m.MaMon!).ToHashSet();
                canXoa = cauHoi.TuyChonCauHoi.Where(t => t.MaMon == null || !maMonGiu.Contains(t.MaMon)).ToList();
                var daCoMon = cauHoi.TuyChonCauHoi.Where(t => t.MaMon != null).Select(t => t.MaMon!).ToHashSet();
                foreach (var m in mons.Where(m => !daCoMon.Contains(m.MaMon!)))
                    themMoi.Add(new TuyChonCauHoi { MaCauHoi = cauHoiId, NoiDungTuyChon = SoThichService.TenDapAnMon(m), MaMon = m.MaMon });
            }
            else
            {
                var giuLai = loaiMoi == TracNghiem ? AdminKhaoSatService.LamSachTuyChon(request.TuyChons) : new List<string>();
                canXoa = cauHoi.TuyChonCauHoi.Where(t => !giuLai.Contains((t.NoiDungTuyChon ?? "").Trim())).ToList();
                var daCo = cauHoi.TuyChonCauHoi.Select(t => (t.NoiDungTuyChon ?? "").Trim()).ToList();
                foreach (var nd in giuLai.Where(t => !daCo.Contains(t)))
                    themMoi.Add(new TuyChonCauHoi { MaCauHoi = cauHoiId, NoiDungTuyChon = nd });
            }

            // Khảo sát thường đã có phiếu: không được sửa nội dung câu hỏi, đổi chọn nhiều hay thêm tùy chọn mới
            // (vẫn được bỏ tùy chọn chưa ai chọn — kiểm tra bên dưới). Hồ sơ sở thích thì được (R3).
            bool doiNoiDung = (cauHoi.NoiDungCauHoi ?? "").Trim() != request.NoiDung!.Trim();
            if (doiNoiDung || themMoi.Count > 0 || cauHoi.ChonNhieu != chonNhieuMoi)
            {
                lyDoKhoa = await LyDoKhoaNoiDungAsync(id, chanKhiCoPhieu: true);
                if (lyDoKhoa != null) return BadRequest(lyDoKhoa);
            }

            if (canXoa.Count > 0)
            {
                var maXoa = canXoa.Select(t => t.MaTuyChon).ToList();
                var daChon = await _service.Query<CauTraLoi>().Where(ct => ct.MaTuyChon != null && maXoa.Contains(ct.MaTuyChon)).Select(ct => ct.MaTuyChon).FirstOrDefaultAsync();
                if (daChon != null)
                {
                    var ten = canXoa.First(t => t.MaTuyChon == daChon).NoiDungTuyChon;
                    return BadRequest($"Tùy chọn \"{ten}\" đã có người chọn, không thể xóa.");
                }
                _service.RemoveRange(canXoa);
            }

            if (themMoi.Count > 0)
            {
                int soTC = await SoTiepTheoAsync<TuyChonCauHoi>("TC", t => t.MaTuyChon.StartsWith("TC"), t => t.MaTuyChon);
                foreach (var tc in themMoi)
                {
                    tc.MaTuyChon = $"TC{soTC++:D2}";
                    _service.Add(tc);
                }
            }

            cauHoi.NoiDungCauHoi = request.NoiDung.Trim();
            cauHoi.LoaiCauHoi = loaiMoi;
            cauHoi.ChonNhieu = chonNhieuMoi;

            await _service.SaveChangesAsync();
            return Ok(new { message = "Cập nhật câu hỏi thành công." });
        }

        // DELETE: api/AdminKhaoSat/{id}/CauHoi/{cauHoiId} - Xóa câu hỏi
        [HttpDelete("{id}/CauHoi/{cauHoiId}")]
        public async Task<IActionResult> DeleteCauHoi(string id, string cauHoiId)
        {
            var cauHoi = await _service.Query<CauHoiKhaoSat>()
                .Include(c => c.TuyChonCauHoi)
                .FirstOrDefaultAsync(c => c.MaCauHoi == cauHoiId && c.MaKhaoSat == id);

            if (cauHoi == null)
                return NotFound("Không tìm thấy câu hỏi.");

            var lyDoKhoa = await LyDoKhoaNoiDungAsync(id, chanKhiCoPhieu: false);
            if (lyDoKhoa != null) return BadRequest(lyDoKhoa);

            bool daCoCauTraLoi = await _service.Query<CauTraLoi>().AnyAsync(ct => ct.MaCauHoi == cauHoiId);
            if (daCoCauTraLoi)
                return BadRequest("Câu hỏi này đã có câu trả lời, không thể xóa.");

            _service.RemoveRange(cauHoi.TuyChonCauHoi);
            _service.Remove(cauHoi);
            await _service.SaveChangesAsync();

            return Ok(new { message = "Xóa câu hỏi thành công." });
        }

        // ── TÙY CHỌN CÂU HỎI ────────────────────────────────────────────────

        // PUT: api/AdminKhaoSat/{id}/CauHoi/{cauHoiId}/TuyChon/{tuyChonId} - Sửa chữ của tùy chọn
        [HttpPut("{id}/CauHoi/{cauHoiId}/TuyChon/{tuyChonId}")]
        public async Task<IActionResult> UpdateTuyChon(string id, string cauHoiId, string tuyChonId, [FromBody] TuyChonUpsertRequest request)
        {
            bool cauHoiTonTai = await _service.Query<CauHoiKhaoSat>()
                .AnyAsync(c => c.MaCauHoi == cauHoiId && c.MaKhaoSat == id);
            if (!cauHoiTonTai)
                return NotFound("Không tìm thấy câu hỏi.");

            var tuyChon = await _service.Query<TuyChonCauHoi>()
                .Include(t => t.MaMonNavigation)
                .FirstOrDefaultAsync(t => t.MaTuyChon == tuyChonId && t.MaCauHoi == cauHoiId);
            if (tuyChon == null)
                return NotFound("Không tìm thấy tùy chọn.");

            var lyDoKhoa = await LyDoKhoaNoiDungAsync(id, chanKhiCoPhieu: true);
            if (lyDoKhoa != null) return BadRequest(lyDoKhoa);

            if (request == null)
                return BadRequest("Nội dung tùy chọn không được để trống.");
            var maMon = string.IsNullOrWhiteSpace(request.MaMon) ? null : request.MaMon.Trim();
            if (maMon != null && maMon != tuyChon.MaMon)
                return BadRequest("Không thể đổi món của đáp án. Hãy thêm đáp án mới cho món khác.");

            string? nd = string.IsNullOrWhiteSpace(request.NoiDungTuyChon) ? null : request.NoiDungTuyChon.Trim();
            if (nd == null && tuyChon.MaMonNavigation != null)
                nd = SoThichService.TenDapAnMon(tuyChon.MaMonNavigation);
            if (nd == null)
                return BadRequest("Nội dung tùy chọn không được để trống.");
            if (nd.Length > 255)
                return BadRequest("Nội dung tùy chọn tối đa 255 ký tự.");

            tuyChon.NoiDungTuyChon = nd;
            await _service.SaveChangesAsync();

            return Ok(new { message = "Cập nhật tùy chọn thành công." });
        }

        // ── HỒ SƠ SỞ THÍCH ──────────────────────────────────────────────────

        // GET: api/AdminKhaoSat/SoThich - Mã bộ hồ sơ sở thích (404 nếu chưa tạo)
        [HttpGet("SoThich")]
        public async Task<IActionResult> GetBoSoThich()
        {
            var ma = await SoThichService.LayMaBoSoThichAsync(_service);
            if (ma == null) return NotFound("Chưa có hồ sơ sở thích.");
            return Ok(new { MaKhaoSat = ma });
        }

        // POST: api/AdminKhaoSat/SoThich - Tạo bộ hồ sơ sở thích nếu chưa có (R1). Có rồi thì trả bộ hiện có.
        [HttpPost("SoThich")]
        public async Task<IActionResult> CreateBoSoThich([FromQuery] string? maNhanVien)
        {
            var daCo = await SoThichService.LayMaBoSoThichAsync(_service);
            if (daCo != null) return Ok(new { message = "Hồ sơ sở thích đã có sẵn.", MaKhaoSat = daCo });

            string? maNV = string.IsNullOrWhiteSpace(maNhanVien) ? null : maNhanVien.Trim();
            if (maNV != null && !await _service.Query<NhanVien>().AnyAsync(n => n.MaNhanVien == maNV))
                maNV = null;

            var khaoSat = new KhaoSat
            {
                MaKhaoSat = await MaTiepTheoAsync<KhaoSat>("KS", k => k.MaKhaoSat.StartsWith("KS"), k => k.MaKhaoSat),
                TieuDe = "Hồ sơ sở thích",
                MaNhanVien = maNV,
                TrangThai = DangMo,
                NgayTao = DateTime.Now,
                LaSoThich = true
            };
            _service.Add(khaoSat);
            try
            {
                await _service.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (DbErrorHelper.IsUniqueViolation(ex))
            {
                // Tạo cùng lúc ở 2 nơi: vướng UNIQUE có điều kiện WHERE laSoThich = 1
                _service.Remove(khaoSat);
                var ma = await SoThichService.LayMaBoSoThichAsync(_service);
                if (ma != null) return Ok(new { message = "Hồ sơ sở thích đã có sẵn.", MaKhaoSat = ma });
                return BadRequest("Không thể tạo hồ sơ sở thích, vui lòng thử lại.");
            }

            return Ok(new { message = "Đã tạo hồ sơ sở thích.", MaKhaoSat = khaoSat.MaKhaoSat });
        }

        // GET: api/AdminKhaoSat/SoThich/KhachHang - Đáp án mà từng khách đã khai báo (dùng cho lọc đối tượng, phân tích)
        [HttpGet("SoThich/KhachHang")]
        public async Task<IActionResult> GetSoThichKhachHang()
        {
            var ds = await _service.Query<CauTraLoi>()
                .Where(c => c.MaTuyChon != null && c.MaPhieuTraLoiNavigation != null
                            && c.MaPhieuTraLoiNavigation.MaKhachHang != null
                            && c.MaPhieuTraLoiNavigation.MaKhaoSatNavigation != null
                            && c.MaPhieuTraLoiNavigation.MaKhaoSatNavigation.LaSoThich)
                .Select(c => new { MaKhachHang = c.MaPhieuTraLoiNavigation!.MaKhachHang!, MaTuyChon = c.MaTuyChon!, c.MaCauHoi })
                .ToListAsync();

            // Khách đã khai báo = có phiếu sở thích (kể cả phiếu chỉ có câu tự luận)
            var khachCoPhieu = await _service.Query<PhieuTraLoi>()
                .Where(p => p.MaKhachHang != null && p.MaKhaoSatNavigation != null && p.MaKhaoSatNavigation.LaSoThich)
                .Select(p => new { MaKhachHang = p.MaKhachHang!, MaCauHois = p.CauTraLoi.Select(c => c.MaCauHoi).Distinct().ToList() })
                .ToListAsync();

            var theoKhach = ds.GroupBy(d => d.MaKhachHang).ToDictionary(g => g.Key, g => g.Select(x => x.MaTuyChon).Distinct().ToList());
            return Ok(khachCoPhieu.Select(k => new
            {
                k.MaKhachHang,
                MaTuyChons = theoKhach.GetValueOrDefault(k.MaKhachHang) ?? new List<string>(),
                // Các câu khách có trả lời (mẫu số % theo từng câu)
                MaCauHois = k.MaCauHois.Where(m => m != null).ToList()
            }));
        }

        // ── ĐỐI TƯỢNG KHẢO SÁT ──────────────────────────────────────────────
        // Không có dòng DOI_TUONG_KHAO_SAT nào = khảo sát công khai cho tất cả khách hàng.

        // GET: api/AdminKhaoSat/{id}/DoiTuong - Danh sách khách hàng được mời
        [HttpGet("{id}/DoiTuong")]
        public async Task<IActionResult> GetDoiTuong(string id)
        {
            if (!await _service.Query<KhaoSat>().AnyAsync(k => k.MaKhaoSat == id))
                return NotFound("Không tìm thấy khảo sát.");

            var daTraLoi = (await _service.Query<PhieuTraLoi>()
                .Where(p => p.MaKhaoSat == id && p.MaKhachHang != null)
                .Select(p => p.MaKhachHang!)
                .ToListAsync()).ToHashSet();

            var ds = await _service.Query<DoiTuongKhaoSat>()
                .Where(d => d.MaKhaoSat == id && d.MaKhachHang != null)
                .Select(d => new
                {
                    MaKhachHang = d.MaKhachHang!,
                    HoTen = d.MaKhachHangNavigation != null ? d.MaKhachHangNavigation.HoTen : null,
                    SoDienThoai = d.MaKhachHangNavigation != null ? d.MaKhachHangNavigation.SoDienThoai : null,
                    d.DaHoanThanh
                })
                .ToListAsync();

            return Ok(ds.Select(d => new
            {
                d.MaKhachHang,
                d.HoTen,
                d.SoDienThoai,
                DaHoanThanh = d.DaHoanThanh == true || daTraLoi.Contains(d.MaKhachHang)
            }));
        }

        // GET: api/AdminKhaoSat/{id}/NguoiDaTraLoi - Mã các khách đã nộp phiếu (lấy từ PHIEU_TRA_LOI, kể cả khi khảo sát công khai)
        [HttpGet("{id}/NguoiDaTraLoi")]
        public async Task<IActionResult> GetNguoiDaTraLoi(string id)
        {
            if (!await _service.Query<KhaoSat>().AnyAsync(k => k.MaKhaoSat == id))
                return NotFound("Không tìm thấy khảo sát.");

            var ds = await _service.Query<PhieuTraLoi>()
                .Where(p => p.MaKhaoSat == id && p.MaKhachHang != null)
                .Select(p => p.MaKhachHang!)
                .Distinct()
                .ToListAsync();

            return Ok(ds);
        }

        // PUT: api/AdminKhaoSat/{id}/DoiTuong - Thay toàn bộ danh sách đối tượng. Danh sách rỗng = tất cả khách hàng.
        [HttpPut("{id}/DoiTuong")]
        public async Task<IActionResult> SetDoiTuong(string id, [FromBody] DoiTuongUpdateRequest request)
        {
            var khaoSat = await _service.FindAsync<KhaoSat>(id);
            if (khaoSat == null)
                return NotFound("Không tìm thấy khảo sát.");
            if (khaoSat.LaSoThich)
                return BadRequest(SoThichService.ThongBaoKhongDatDoiTuong);
            if (khaoSat.TrangThai == DaDong)
                return BadRequest("Khảo sát đã đóng, không thể thay đổi đối tượng.");

            var yeuCau = (request?.MaKhachHangs ?? new List<string>())
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Select(m => m.Trim())
                .Distinct()
                .ToList();

            var hopLe = yeuCau.Count == 0
                ? new HashSet<string>()
                : (await _service.Query<KhachHang>()
                    .Where(k => yeuCau.Contains(k.MaKhachHang))
                    .Select(k => k.MaKhachHang)
                    .ToListAsync()).ToHashSet();

            var daTraLoi = (await _service.Query<PhieuTraLoi>()
                .Where(p => p.MaKhaoSat == id && p.MaKhachHang != null)
                .Select(p => p.MaKhachHang!)
                .ToListAsync()).ToHashSet();

            // Danh sách giới hạn phải bao gồm mọi khách đã nộp phiếu. Kiểm tra trên phiếu thật (không dựa vào
            // DOI_TUONG hiện có) để chặn cả trường hợp chuyển từ công khai sang giới hạn.
            if (hopLe.Count > 0)
            {
                var thieu = daTraLoi.Where(m => !hopLe.Contains(m)).OrderBy(m => m).ToList();
                if (thieu.Count > 0)
                {
                    var viDu = thieu.Take(3).ToList();
                    var tenViDu = await _service.Query<KhachHang>()
                        .Where(k => viDu.Contains(k.MaKhachHang))
                        .Select(k => k.HoTen ?? k.MaKhachHang)
                        .ToListAsync();
                    return BadRequest($"Có {thieu.Count} khách đã trả lời khảo sát nhưng không nằm trong danh sách mới " +
                                      $"(ví dụ: {string.Join(", ", tenViDu)}). Danh sách đối tượng phải bao gồm tất cả người đã trả lời.");
                }
            }

            var hienCo = await _service.Query<DoiTuongKhaoSat>()
                .Where(d => d.MaKhaoSat == id)
                .ToListAsync();

            var canXoa = hienCo.Where(d => d.MaKhachHang == null || !hopLe.Contains(d.MaKhachHang)).ToList();
            _service.RemoveRange(canXoa);

            foreach (var d in hienCo.Except(canXoa))
                d.DaHoanThanh = daTraLoi.Contains(d.MaKhachHang!);

            var daCo = hienCo.Except(canXoa).Select(d => d.MaKhachHang!).ToHashSet();
            var themMoi = hopLe.Where(m => !daCo.Contains(m)).OrderBy(m => m).ToList();
            if (themMoi.Count > 0)
            {
                int so = await SoTiepTheoAsync<DoiTuongKhaoSat>("DT", d => d.MaDoiTuong.StartsWith("DT"), d => d.MaDoiTuong);
                foreach (var ma in themMoi)
                {
                    _service.Add(new DoiTuongKhaoSat
                    {
                        MaDoiTuong = $"DT{so++:D3}",
                        MaKhaoSat = id,
                        MaKhachHang = ma,
                        DaHoanThanh = daTraLoi.Contains(ma)
                    });
                }
            }

            try
            {
                await _service.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (DbErrorHelper.IsUniqueViolation(ex))
            {
                // 2 lần lưu danh sách đối tượng cùng lúc
                return BadRequest("Danh sách đối tượng vừa được thay đổi ở nơi khác, vui lòng tải lại và thử lại.");
            }

            string thongBao = hopLe.Count == 0
                ? "Khảo sát áp dụng cho tất cả khách hàng."
                : $"Đã lưu {hopLe.Count} khách hàng là đối tượng khảo sát.";
            return Ok(new { message = thongBao, soDoiTuong = hopLe.Count });
        }

        // ── THỐNG KÊ ─────────────────────────────────────────────────────────

        // GET: api/AdminKhaoSat/{id}/results - Thống kê: số phiếu, số lượt chọn + tỷ lệ từng tùy chọn, câu trả lời tự luận
        [HttpGet("{id}/results")]
        public async Task<IActionResult> GetResults(string id)
        {
            var khaoSat = await _service.Query<KhaoSat>()
                .Include(k => k.CauHoiKhaoSat)
                    .ThenInclude(c => c.TuyChonCauHoi)
                .FirstOrDefaultAsync(k => k.MaKhaoSat == id);

            if (khaoSat == null)
            {
                return NotFound("Không tìm thấy khảo sát.");
            }

            var tongSoPhieu = await _service.Query<PhieuTraLoi>().CountAsync(p => p.MaKhaoSat == id);
            var soDoiTuong = await _service.Query<DoiTuongKhaoSat>().CountAsync(d => d.MaKhaoSat == id);

            // Lấy toàn bộ câu trả lời của khảo sát một lần rồi đếm trong bộ nhớ
            var cauTraLois = await _service.Query<CauTraLoi>()
                .Where(c => c.MaPhieuTraLoiNavigation != null && c.MaPhieuTraLoiNavigation.MaKhaoSat == id)
                .Select(c => new KhaoSatAnswerItem
                {
                    MaCauHoi = c.MaCauHoi,
                    MaTuyChon = c.MaTuyChon,
                    MaPhieuTraLoi = c.MaPhieuTraLoi,
                    NoiDungTuDien = c.NoiDungTuDien,
                    TenKhachHang = c.MaPhieuTraLoiNavigation!.MaKhachHangNavigation != null ? c.MaPhieuTraLoiNavigation.MaKhachHangNavigation.HoTen : null,
                    NgayTraLoi = c.MaPhieuTraLoiNavigation.NgayTraLoi
                })
                .ToListAsync();

            var cauHoiResults = AdminKhaoSatService.AggregateQuestionResults(khaoSat.CauHoiKhaoSat, cauTraLois);

            return Ok(new
            {
                khaoSat.MaKhaoSat,
                khaoSat.TieuDe,
                khaoSat.TrangThai,
                TongSoNguoiTraLoi = tongSoPhieu,
                SoDoiTuong = soDoiTuong,
                ChiTiet = cauHoiResults
            });
        }

        // ── Hàm hỗ trợ ───────────────────────────────────────────────────────

        private const string ThongBaoDaDong = "Khảo sát đã đóng, không thể sửa nội dung.";
        private const string ThongBaoDaCoPhieu = "Khảo sát đã có người trả lời, không thể sửa nội dung để tránh sai lệch kết quả. Hãy tạo khảo sát mới.";

        /// <summary>
        /// Lý do không được sửa nội dung khảo sát (câu hỏi / tùy chọn), hoặc null nếu được sửa.
        /// Khảo sát đã đóng: chặn mọi thao tác. Khảo sát đã có phiếu: chỉ chặn khi <paramref name="chanKhiCoPhieu"/> = true
        /// (thêm câu hỏi, sửa nội dung câu hỏi, thêm / sửa tùy chọn).
        /// </summary>
        private async Task<string?> LyDoKhoaNoiDungAsync(string maKhaoSat, bool chanKhiCoPhieu)
        {
            var ks = await _service.Query<KhaoSat>()
                .Where(k => k.MaKhaoSat == maKhaoSat)
                .Select(k => new { k.TrangThai, k.LaSoThich })
                .FirstOrDefaultAsync();
            if (ks == null) return null;
            if (AdminKhaoSatService.ChuanHoaTrangThai(ks.TrangThai) == DaDong) return ThongBaoDaDong;
            // Hồ sơ sở thích là danh mục sống: được thêm / sửa dù đã có phiếu (R3)
            if (ks.LaSoThich) return null;
            if (chanKhiCoPhieu && await _service.Query<PhieuTraLoi>().AnyAsync(p => p.MaKhaoSat == maKhaoSat))
                return ThongBaoDaCoPhieu;
            return null;
        }

        /// <summary>
        /// Kiểm tra câu "Món yêu thích" (R4): chỉ ở hồ sơ sở thích, tối đa 1 câu, có ít nhất 1 món hợp lệ.
        /// Trả về lỗi hoặc danh sách món (giữ thứ tự, bỏ trùng).
        /// </summary>
        private async Task<(string? Loi, List<MonAn> Mons)> KiemTraCauMonYeuThichAsync(KhaoSat khaoSat, string? maCauHoiHienTai, CauHoiUpsertRequest request)
        {
            var mons = new List<MonAn>();
            if (!khaoSat.LaSoThich)
                return ("Chỉ hồ sơ sở thích mới có câu \"Món yêu thích\".", mons);

            var loi = AdminKhaoSatService.KiemTraCauHoi(request.NoiDung, TracNghiem, null, chonNhieu: true, boQuaTuyChon: true);
            if (loi != null) return (loi, mons);

            bool daCoCauKhac = await _service.Query<CauHoiKhaoSat>()
                .AnyAsync(c => c.MaKhaoSat == khaoSat.MaKhaoSat && c.MaCauHoi != maCauHoiHienTai
                               && c.TuyChonCauHoi.Any(t => t.MaMon != null));
            if (daCoCauKhac)
                return ("Hồ sơ sở thích đã có câu \"Món yêu thích\". Mỗi hồ sơ chỉ có 1 câu loại này.", mons);

            var maMons = (request.MaMons ?? new List<string>())
                .Where(m => !string.IsNullOrWhiteSpace(m)).Select(m => m.Trim()).Distinct().ToList();
            if (maMons.Count == 0)
                return ("Câu \"Món yêu thích\" cần ít nhất 1 món.", mons);

            var timThay = await _service.Query<MonAn>().Where(m => m.MaMon != null && maMons.Contains(m.MaMon)).ToListAsync();
            var thieu = maMons.FirstOrDefault(m => !timThay.Any(x => x.MaMon == m));
            if (thieu != null)
                return ($"Không tìm thấy món \"{thieu}\".", mons);

            mons = maMons.Select(m => timThay.First(x => x.MaMon == m)).ToList();
            return (null, mons);
        }

        private async Task<int> SoTiepTheoAsync<TEntity>(string prefix,
            System.Linq.Expressions.Expression<Func<TEntity, bool>> loc,
            System.Linq.Expressions.Expression<Func<TEntity, string>> chonMa) where TEntity : class
        {
            var codes = await _service.ListAsync(loc, chonMa);
            int max = 0;
            foreach (var code in codes)
            {
                if (code.Length > prefix.Length && code.Length <= prefix.Length + 6 &&
                    int.TryParse(code.Substring(prefix.Length), out int n) && n > max)
                    max = n;
            }
            return max + 1;
        }

        private async Task<string> MaTiepTheoAsync<TEntity>(string prefix,
            System.Linq.Expressions.Expression<Func<TEntity, bool>> loc,
            System.Linq.Expressions.Expression<Func<TEntity, string>> chonMa) where TEntity : class
        {
            return $"{prefix}{await SoTiepTheoAsync(prefix, loc, chonMa):D2}";
        }
    }
}
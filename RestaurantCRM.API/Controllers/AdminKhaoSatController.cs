using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.API.DTOs;
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
            var query = _service.Query<KhaoSat>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(k => k.MaKhaoSat.Contains(s) || (k.TieuDe != null && k.TieuDe.Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                var tt = ChuanHoaTrangThai(trangThai);
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
                    SoNguoiTraLoi = k.PhieuTraLoi.Count()
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
                .FirstOrDefaultAsync(k => k.MaKhaoSat == id);

            if (khaoSat == null)
            {
                return NotFound("Không tìm thấy khảo sát.");
            }

            var soPhieu = await _service.Query<PhieuTraLoi>().CountAsync(p => p.MaKhaoSat == id);
            var cauHoiDaCoTraLoi = await _service.Query<CauTraLoi>()
                .Where(c => c.MaCauHoiNavigation != null && c.MaCauHoiNavigation.MaKhaoSat == id)
                .Select(c => c.MaCauHoi)
                .Distinct()
                .ToListAsync();

            var result = new
            {
                khaoSat.MaKhaoSat,
                khaoSat.TieuDe,
                khaoSat.TrangThai,
                khaoSat.NgayTao,
                khaoSat.MaNhanVien,
                SoNguoiTraLoi = soPhieu,
                CauHoi = SapXep(khaoSat.CauHoiKhaoSat, c => c.MaCauHoi).Select(c => new
                {
                    c.MaCauHoi,
                    c.NoiDungCauHoi,
                    LoaiCauHoi = ChuanHoaLoai(c.LoaiCauHoi) ?? c.LoaiCauHoi,
                    DaCoTraLoi = cauHoiDaCoTraLoi.Contains(c.MaCauHoi),
                    TuyChon = SapXep(c.TuyChonCauHoi, t => t.MaTuyChon).Select(t => new
                    {
                        t.MaTuyChon,
                        t.NoiDungTuyChon
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
                var loi = KiemTraCauHoi(ch.NoiDungCauHoi, ch.LoaiCauHoi, ch.TuyChons);
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
                var loai = ChuanHoaLoai(reqCauHoi.LoaiCauHoi)!;
                var cauHoi = new CauHoiKhaoSat
                {
                    MaCauHoi = $"CH{soCH++:D2}",
                    MaKhaoSat = khaoSat.MaKhaoSat,
                    NoiDungCauHoi = reqCauHoi.NoiDungCauHoi.Trim(),
                    LoaiCauHoi = loai
                };
                _service.Add(cauHoi);

                if (loai == TracNghiem)
                {
                    foreach (var tc in LamSachTuyChon(reqCauHoi.TuyChons))
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

            var moi = ChuanHoaTrangThai(status);
            if (moi == null)
            {
                return BadRequest("Trạng thái không hợp lệ. Chấp nhận: Draft, Open, Closed.");
            }

            if (moi == DangMo)
            {
                // Chỉ mở khi khảo sát đã đủ nội dung để khách trả lời
                if (!khaoSat.CauHoiKhaoSat.Any())
                    return BadRequest("Khảo sát chưa có câu hỏi nào, không thể mở.");
                var thieu = khaoSat.CauHoiKhaoSat.FirstOrDefault(c => ChuanHoaLoai(c.LoaiCauHoi) == TracNghiem && c.TuyChonCauHoi.Count < 2);
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

        // DELETE: api/AdminKhaoSat/{id} - Xóa khảo sát (chỉ khi chưa có phiếu trả lời)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var khaoSat = await _service.FindAsync<KhaoSat>(id);
            if (khaoSat == null)
            {
                return NotFound("Không tìm thấy khảo sát.");
            }

            // CHECK của KHAO_SAT không có giá trị 'Deleted' nên không xóa mềm được.
            // Khảo sát đã có phiếu trả lời phải giữ lại để còn thống kê -> chỉ cho đóng.
            if (await _service.Query<PhieuTraLoi>().AnyAsync(p => p.MaKhaoSat == id))
            {
                return BadRequest("Khảo sát đã có phiếu trả lời nên không thể xóa (cần giữ dữ liệu thống kê). Hãy đóng khảo sát thay thế.");
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

            return Ok(new { message = "Xóa khảo sát thành công." });
        }

        // ── CÂU HỎI ──────────────────────────────────────────────────────────

        // POST: api/AdminKhaoSat/{id}/CauHoi - Thêm câu hỏi vào khảo sát
        [HttpPost("{id}/CauHoi")]
        public async Task<IActionResult> AddCauHoi(string id, [FromBody] CauHoiUpsertRequest request)
        {
            var khaoSat = await _service.FindAsync<KhaoSat>(id);
            if (khaoSat == null)
                return NotFound("Không tìm thấy khảo sát.");
            if (khaoSat.TrangThai == DaDong)
                return BadRequest("Khảo sát đã đóng, không thể thêm câu hỏi.");

            var loi = KiemTraCauHoi(request?.NoiDung, request?.LoaiCauHoi, request?.TuyChons);
            if (loi != null) return BadRequest(loi);

            var loai = ChuanHoaLoai(request!.LoaiCauHoi)!;
            var maCauHoi = await MaTiepTheoAsync<CauHoiKhaoSat>("CH", c => c.MaCauHoi.StartsWith("CH"), c => c.MaCauHoi);
            _service.Add(new CauHoiKhaoSat
            {
                MaCauHoi = maCauHoi,
                MaKhaoSat = id,
                NoiDungCauHoi = request.NoiDung.Trim(),
                LoaiCauHoi = loai
            });

            if (loai == TracNghiem)
            {
                int soTC = await SoTiepTheoAsync<TuyChonCauHoi>("TC", t => t.MaTuyChon.StartsWith("TC"), t => t.MaTuyChon);
                foreach (var nd in LamSachTuyChon(request.TuyChons))
                {
                    _service.Add(new TuyChonCauHoi { MaTuyChon = $"TC{soTC++:D2}", MaCauHoi = maCauHoi, NoiDungTuyChon = nd });
                }
            }

            await _service.SaveChangesAsync();
            return Ok(new { message = "Thêm câu hỏi thành công.", MaCauHoi = maCauHoi });
        }

        // PUT: api/AdminKhaoSat/{id}/CauHoi/{cauHoiId} - Sửa câu hỏi (nội dung, loại, danh sách tùy chọn)
        // Tùy chọn được đối chiếu theo nội dung: giữ tùy chọn trùng, thêm tùy chọn mới, xóa tùy chọn bị bỏ
        // (không xóa được tùy chọn đã có người chọn).
        [HttpPut("{id}/CauHoi/{cauHoiId}")]
        public async Task<IActionResult> UpdateCauHoi(string id, string cauHoiId, [FromBody] CauHoiUpsertRequest request)
        {
            var cauHoi = await _service.Query<CauHoiKhaoSat>()
                .Include(c => c.TuyChonCauHoi)
                .FirstOrDefaultAsync(c => c.MaCauHoi == cauHoiId && c.MaKhaoSat == id);

            if (cauHoi == null)
                return NotFound("Không tìm thấy câu hỏi.");

            var loi = KiemTraCauHoi(request?.NoiDung, request?.LoaiCauHoi, request?.TuyChons);
            if (loi != null) return BadRequest(loi);

            var loaiMoi = ChuanHoaLoai(request!.LoaiCauHoi)!;
            var loaiCu = ChuanHoaLoai(cauHoi.LoaiCauHoi);
            bool daCoTraLoi = await _service.Query<CauTraLoi>().AnyAsync(ct => ct.MaCauHoi == cauHoiId);
            if (daCoTraLoi && loaiMoi != loaiCu)
                return BadRequest("Câu hỏi đã có người trả lời nên không thể đổi loại câu hỏi.");

            var giuLai = loaiMoi == TracNghiem ? LamSachTuyChon(request.TuyChons) : new List<string>();
            var canXoa = cauHoi.TuyChonCauHoi.Where(t => !giuLai.Contains((t.NoiDungTuyChon ?? "").Trim())).ToList();
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

            var daCo = cauHoi.TuyChonCauHoi.Select(t => (t.NoiDungTuyChon ?? "").Trim()).ToList();
            var themMoi = giuLai.Where(t => !daCo.Contains(t)).ToList();
            if (themMoi.Count > 0)
            {
                int soTC = await SoTiepTheoAsync<TuyChonCauHoi>("TC", t => t.MaTuyChon.StartsWith("TC"), t => t.MaTuyChon);
                foreach (var nd in themMoi)
                    _service.Add(new TuyChonCauHoi { MaTuyChon = $"TC{soTC++:D2}", MaCauHoi = cauHoiId, NoiDungTuyChon = nd });
            }

            cauHoi.NoiDungCauHoi = request.NoiDung.Trim();
            cauHoi.LoaiCauHoi = loaiMoi;

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

            bool daCoCauTraLoi = await _service.Query<CauTraLoi>().AnyAsync(ct => ct.MaCauHoi == cauHoiId);
            if (daCoCauTraLoi)
                return BadRequest("Câu hỏi này đã có câu trả lời, không thể xóa.");

            _service.RemoveRange(cauHoi.TuyChonCauHoi);
            _service.Remove(cauHoi);
            await _service.SaveChangesAsync();

            return Ok(new { message = "Xóa câu hỏi thành công." });
        }

        // ── TÙY CHỌN CÂU HỎI ────────────────────────────────────────────────

        // POST: api/AdminKhaoSat/{id}/CauHoi/{cauHoiId}/TuyChon - Thêm tùy chọn
        [HttpPost("{id}/CauHoi/{cauHoiId}/TuyChon")]
        public async Task<IActionResult> AddTuyChon(string id, string cauHoiId, [FromBody] TuyChonUpsertRequest request)
        {
            var cauHoi = await _service.Query<CauHoiKhaoSat>()
                .Include(c => c.TuyChonCauHoi)
                .FirstOrDefaultAsync(c => c.MaCauHoi == cauHoiId && c.MaKhaoSat == id);

            if (cauHoi == null)
                return NotFound("Không tìm thấy câu hỏi.");

            if (ChuanHoaLoai(cauHoi.LoaiCauHoi) == TuLuan)
                return BadRequest("Câu hỏi tự luận không cần tùy chọn.");

            if (request == null || string.IsNullOrWhiteSpace(request.NoiDungTuyChon))
                return BadRequest("Nội dung tùy chọn không được để trống.");
            var nd = request.NoiDungTuyChon.Trim();
            if (nd.Length > 255)
                return BadRequest("Nội dung tùy chọn tối đa 255 ký tự.");
            if (cauHoi.TuyChonCauHoi.Any(t => (t.NoiDungTuyChon ?? "").Trim() == nd))
                return BadRequest("Tùy chọn này đã tồn tại trong câu hỏi.");

            var tuyChon = new TuyChonCauHoi
            {
                MaTuyChon = await MaTiepTheoAsync<TuyChonCauHoi>("TC", t => t.MaTuyChon.StartsWith("TC"), t => t.MaTuyChon),
                MaCauHoi = cauHoiId,
                NoiDungTuyChon = nd
            };
            _service.Add(tuyChon);
            await _service.SaveChangesAsync();

            return Ok(new { message = "Thêm tùy chọn thành công.", MaTuyChon = tuyChon.MaTuyChon });
        }

        // PUT: api/AdminKhaoSat/{id}/CauHoi/{cauHoiId}/TuyChon/{tuyChonId} - Sửa tùy chọn
        [HttpPut("{id}/CauHoi/{cauHoiId}/TuyChon/{tuyChonId}")]
        public async Task<IActionResult> UpdateTuyChon(string id, string cauHoiId, string tuyChonId, [FromBody] TuyChonUpsertRequest request)
        {
            bool cauHoiTonTai = await _service.Query<CauHoiKhaoSat>()
                .AnyAsync(c => c.MaCauHoi == cauHoiId && c.MaKhaoSat == id);
            if (!cauHoiTonTai)
                return NotFound("Không tìm thấy câu hỏi.");

            var tuyChon = await _service.Query<TuyChonCauHoi>()
                .FirstOrDefaultAsync(t => t.MaTuyChon == tuyChonId && t.MaCauHoi == cauHoiId);
            if (tuyChon == null)
                return NotFound("Không tìm thấy tùy chọn.");

            if (request == null || string.IsNullOrWhiteSpace(request.NoiDungTuyChon))
                return BadRequest("Nội dung tùy chọn không được để trống.");
            if (request.NoiDungTuyChon.Trim().Length > 255)
                return BadRequest("Nội dung tùy chọn tối đa 255 ký tự.");

            tuyChon.NoiDungTuyChon = request.NoiDungTuyChon.Trim();
            await _service.SaveChangesAsync();

            return Ok(new { message = "Cập nhật tùy chọn thành công." });
        }

        // DELETE: api/AdminKhaoSat/{id}/CauHoi/{cauHoiId}/TuyChon/{tuyChonId} - Xóa tùy chọn
        [HttpDelete("{id}/CauHoi/{cauHoiId}/TuyChon/{tuyChonId}")]
        public async Task<IActionResult> DeleteTuyChon(string id, string cauHoiId, string tuyChonId)
        {
            bool cauHoiTonTai = await _service.Query<CauHoiKhaoSat>()
                .AnyAsync(c => c.MaCauHoi == cauHoiId && c.MaKhaoSat == id);
            if (!cauHoiTonTai)
                return NotFound("Không tìm thấy câu hỏi.");

            var tuyChon = await _service.Query<TuyChonCauHoi>()
                .FirstOrDefaultAsync(t => t.MaTuyChon == tuyChonId && t.MaCauHoi == cauHoiId);
            if (tuyChon == null)
                return NotFound("Không tìm thấy tùy chọn.");

            bool daCoLuaChon = await _service.Query<CauTraLoi>()
                .AnyAsync(ct => ct.MaTuyChon == tuyChonId);
            if (daCoLuaChon)
                return BadRequest("Tùy chọn này đã có người chọn, không thể xóa.");

            _service.Remove(tuyChon);
            await _service.SaveChangesAsync();

            return Ok(new { message = "Xóa tùy chọn thành công." });
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

        /// <summary>Quy đổi trạng thái về giá trị hợp lệ của CSDL (Draft / Open / Closed).</summary>
        public static string? ChuanHoaTrangThai(string? s) => AdminKhaoSatService.ChuanHoaTrangThai(s);

        /// <summary>Quy đổi loại câu hỏi về giá trị hợp lệ của CSDL (Option / Text).</summary>
        public static string? ChuanHoaLoai(string? s) => AdminKhaoSatService.ChuanHoaLoai(s);

        private static string? KiemTraCauHoi(string? noiDung, string? loaiCauHoi, List<string>? tuyChons) =>
            AdminKhaoSatService.KiemTraCauHoi(noiDung, loaiCauHoi, tuyChons);

        private static List<string> LamSachTuyChon(List<string>? tuyChons) =>
            AdminKhaoSatService.LamSachTuyChon(tuyChons);

        /// <summary>Sắp theo mã tự nhiên: CH2 trước CH10.</summary>
        private static IEnumerable<T> SapXep<T>(IEnumerable<T> items, Func<T, string> key) =>
            AdminKhaoSatService.SapXep(items, key);

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
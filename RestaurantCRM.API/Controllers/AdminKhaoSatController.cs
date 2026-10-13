using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminKhaoSatController : ControllerBase
    {
        private readonly RestaurantCrmContext _context;

        public AdminKhaoSatController(RestaurantCrmContext context)
        {
            _context = context;
        }

        // GET: api/AdminKhaoSat - Lấy danh sách khảo sát
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var khaoSats = await _context.KhaoSat
                .Where(k => k.TrangThai != "Deleted")
                .OrderBy(k => k.MaKhaoSat)
                .Select(k => new
                {
                    k.MaKhaoSat,
                    k.TieuDe,
                    k.TrangThai,
                    k.NgayTao,
                    k.MaNhanVien,
                    SoNguoiTraLoi = _context.PhieuTraLoi.Count(p => p.MaKhaoSat == k.MaKhaoSat)
                })
                .ToListAsync();

            return Ok(khaoSats);
        }

        // GET: api/AdminKhaoSat/{id} - Lấy chi tiết khảo sát bao gồm câu hỏi
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var khaoSat = await _context.KhaoSat
                .Include(k => k.CauHoiKhaoSat)
                    .ThenInclude(c => c.TuyChonCauHoi)
                .FirstOrDefaultAsync(k => k.MaKhaoSat == id && k.TrangThai != "Deleted");

            if (khaoSat == null)
            {
                return NotFound("Không tìm thấy khảo sát.");
            }

            var result = new
            {
                khaoSat.MaKhaoSat,
                khaoSat.TieuDe,
                khaoSat.TrangThai,
                khaoSat.NgayTao,
                khaoSat.MaNhanVien,
                CauHoi = khaoSat.CauHoiKhaoSat.Select(c => new
                {
                    c.MaCauHoi,
                    c.NoiDungCauHoi,
                    c.LoaiCauHoi,
                    TuyChon = c.TuyChonCauHoi.Select(t => new
                    {
                        t.MaTuyChon,
                        t.NoiDungTuyChon
                    })
                })
            };

            return Ok(result);
        }

        // POST: api/AdminKhaoSat - Tạo khảo sát mới
        [HttpPost]
        public async Task<IActionResult> Create(KhaoSatCreateRequest request)
        {
            var khaoSat = new KhaoSat
            {
                MaKhaoSat = "KS" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                TieuDe = request.TieuDe,
                MaNhanVien = request.MaNhanVien,
                TrangThai = "Draft",
                NgayTao = DateTime.Now
            };

            _context.KhaoSat.Add(khaoSat);

            foreach (var reqCauHoi in request.CauHois)
            {
                var cauHoi = new CauHoiKhaoSat
                {
                    MaCauHoi = "CH" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    MaKhaoSat = khaoSat.MaKhaoSat,
                    NoiDungCauHoi = reqCauHoi.NoiDungCauHoi,
                    LoaiCauHoi = reqCauHoi.LoaiCauHoi
                };
                _context.CauHoiKhaoSat.Add(cauHoi);

                if (reqCauHoi.TuyChons != null && reqCauHoi.TuyChons.Any())
                {
                    foreach (var tc in reqCauHoi.TuyChons)
                    {
                        var tuyChon = new TuyChonCauHoi
                        {
                            MaTuyChon = "TC" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                            MaCauHoi = cauHoi.MaCauHoi,
                            NoiDungTuyChon = tc
                        };
                        _context.TuyChonCauHoi.Add(tuyChon);
                    }
                }
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = khaoSat.MaKhaoSat }, new { message = "Tạo khảo sát thành công", MaKhaoSat = khaoSat.MaKhaoSat });
        }

        // PUT: api/AdminKhaoSat/{id}/status - Thay đổi trạng thái (Kích hoạt/Đóng)
        [HttpPut("{id}/status")]
        public async Task<IActionResult> ChangeStatus(string id, [FromBody] string status)
        {
            var khaoSat = await _context.KhaoSat.FindAsync(id);
            if (khaoSat == null || khaoSat.TrangThai == "Deleted")
            {
                return NotFound("Không tìm thấy khảo sát.");
            }

            var validStatuses = new[] { "Draft", "Active", "Closed" };
            if (!validStatuses.Contains(status))
            {
                return BadRequest("Trạng thái không hợp lệ.");
            }

            khaoSat.TrangThai = status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật trạng thái thành công" });
        }

        // DELETE: api/AdminKhaoSat/{id} - Xóa khảo sát (hoặc ẩn)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var khaoSat = await _context.KhaoSat.FindAsync(id);
            if (khaoSat == null || khaoSat.TrangThai == "Deleted")
            {
                return NotFound("Không tìm thấy khảo sát.");
            }

            var hasAnswers = await _context.PhieuTraLoi.AnyAsync(p => p.MaKhaoSat == id);
            
            if (hasAnswers)
            {
                // Soft delete
                khaoSat.TrangThai = "Deleted";
            }
            else
            {
                // Hard delete
                var cauHois = await _context.CauHoiKhaoSat.Where(c => c.MaKhaoSat == id).ToListAsync();
                foreach (var ch in cauHois)
                {
                    var tuyChons = await _context.TuyChonCauHoi.Where(t => t.MaCauHoi == ch.MaCauHoi).ToListAsync();
                    _context.TuyChonCauHoi.RemoveRange(tuyChons);
                }
                _context.CauHoiKhaoSat.RemoveRange(cauHois);
                _context.KhaoSat.Remove(khaoSat);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa khảo sát thành công" });
        }

        // PUT: api/AdminKhaoSat/{id} - Cập nhật tiêu đề khảo sát
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] KhaoSatUpdateRequest request)
        {
            var khaoSat = await _context.KhaoSat.FindAsync(id);
            if (khaoSat == null || khaoSat.TrangThai == "Deleted")
                return NotFound("Không tìm thấy khảo sát.");

            if (string.IsNullOrWhiteSpace(request.TieuDe))
                return BadRequest("Tiêu đề không được để trống.");

            khaoSat.TieuDe = request.TieuDe.Trim();
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật khảo sát thành công." });
        }

        // ── CÂU HỎI ──────────────────────────────────────────────────────────

        // POST: api/AdminKhaoSat/{id}/CauHoi - Thêm câu hỏi vào khảo sát
        [HttpPost("{id}/CauHoi")]
        public async Task<IActionResult> AddCauHoi(string id, [FromBody] CauHoiUpsertRequest request)
        {
            var khaoSat = await _context.KhaoSat.FindAsync(id);
            if (khaoSat == null || khaoSat.TrangThai == "Deleted")
                return NotFound("Không tìm thấy khảo sát.");

            if (string.IsNullOrWhiteSpace(request.NoiDung))
                return BadRequest("Nội dung câu hỏi không được để trống.");

            var validLoai = new[] { "TracNghiem", "TuLuan" };
            if (!validLoai.Contains(request.LoaiCauHoi))
                return BadRequest("Loại câu hỏi không hợp lệ. Chỉ chấp nhận 'TracNghiem' hoặc 'TuLuan'.");

            if (request.LoaiCauHoi == "TracNghiem" && (request.TuyChons == null || request.TuyChons.Count < 2))
                return BadRequest("Câu hỏi trắc nghiệm cần ít nhất 2 tùy chọn.");

            var maCauHoi = "CH" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            var cauHoi = new CauHoiKhaoSat
            {
                MaCauHoi = maCauHoi,
                MaKhaoSat = id,
                NoiDungCauHoi = request.NoiDung.Trim(),
                LoaiCauHoi = request.LoaiCauHoi
            };
            _context.CauHoiKhaoSat.Add(cauHoi);

            if (request.LoaiCauHoi == "TracNghiem" && request.TuyChons != null)
            {
                foreach (var nd in request.TuyChons.Where(t => !string.IsNullOrWhiteSpace(t)))
                {
                    _context.TuyChonCauHoi.Add(new TuyChonCauHoi
                    {
                        MaTuyChon = "TC" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                        MaCauHoi = maCauHoi,
                        NoiDungTuyChon = nd.Trim()
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Thêm câu hỏi thành công.", MaCauHoi = maCauHoi });
        }

        // PUT: api/AdminKhaoSat/{id}/CauHoi/{cauHoiId} - Sửa câu hỏi
        [HttpPut("{id}/CauHoi/{cauHoiId}")]
        public async Task<IActionResult> UpdateCauHoi(string id, string cauHoiId, [FromBody] CauHoiUpsertRequest request)
        {
            var cauHoi = await _context.CauHoiKhaoSat
                .Include(c => c.TuyChonCauHoi)
                .FirstOrDefaultAsync(c => c.MaCauHoi == cauHoiId && c.MaKhaoSat == id);

            if (cauHoi == null)
                return NotFound("Không tìm thấy câu hỏi.");

            if (string.IsNullOrWhiteSpace(request.NoiDung))
                return BadRequest("Nội dung câu hỏi không được để trống.");

            var validLoai = new[] { "TracNghiem", "TuLuan" };
            if (!validLoai.Contains(request.LoaiCauHoi))
                return BadRequest("Loại câu hỏi không hợp lệ. Chỉ chấp nhận 'TracNghiem' hoặc 'TuLuan'.");

            if (request.LoaiCauHoi == "TracNghiem" && (request.TuyChons == null || request.TuyChons.Count < 2))
                return BadRequest("Câu hỏi trắc nghiệm cần ít nhất 2 tùy chọn.");

            cauHoi.NoiDungCauHoi = request.NoiDung.Trim();
            cauHoi.LoaiCauHoi = request.LoaiCauHoi;

            // Xóa toàn bộ tùy chọn cũ rồi thêm lại (replace strategy)
            _context.TuyChonCauHoi.RemoveRange(cauHoi.TuyChonCauHoi);

            if (request.LoaiCauHoi == "TracNghiem" && request.TuyChons != null)
            {
                foreach (var nd in request.TuyChons.Where(t => !string.IsNullOrWhiteSpace(t)))
                {
                    _context.TuyChonCauHoi.Add(new TuyChonCauHoi
                    {
                        MaTuyChon = "TC" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                        MaCauHoi = cauHoiId,
                        NoiDungTuyChon = nd.Trim()
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật câu hỏi thành công." });
        }

        // DELETE: api/AdminKhaoSat/{id}/CauHoi/{cauHoiId} - Xóa câu hỏi
        [HttpDelete("{id}/CauHoi/{cauHoiId}")]
        public async Task<IActionResult> DeleteCauHoi(string id, string cauHoiId)
        {
            var cauHoi = await _context.CauHoiKhaoSat
                .Include(c => c.TuyChonCauHoi)
                .FirstOrDefaultAsync(c => c.MaCauHoi == cauHoiId && c.MaKhaoSat == id);

            if (cauHoi == null)
                return NotFound("Không tìm thấy câu hỏi.");

            // Kiểm tra đã có câu trả lời chưa
            bool daCoCauTraLoi = await _context.CauTraLoi.AnyAsync(ct => ct.MaCauHoi == cauHoiId);
            if (daCoCauTraLoi)
                return BadRequest("Câu hỏi này đã có câu trả lời, không thể xóa.");

            _context.TuyChonCauHoi.RemoveRange(cauHoi.TuyChonCauHoi);
            _context.CauHoiKhaoSat.Remove(cauHoi);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa câu hỏi thành công." });
        }

        // ── TÙY CHỌN CÂU HỎI ────────────────────────────────────────────────

        // POST: api/AdminKhaoSat/{id}/CauHoi/{cauHoiId}/TuyChon - Thêm tùy chọn
        [HttpPost("{id}/CauHoi/{cauHoiId}/TuyChon")]
        public async Task<IActionResult> AddTuyChon(string id, string cauHoiId, [FromBody] TuyChonUpsertRequest request)
        {
            var cauHoi = await _context.CauHoiKhaoSat
                .FirstOrDefaultAsync(c => c.MaCauHoi == cauHoiId && c.MaKhaoSat == id);

            if (cauHoi == null)
                return NotFound("Không tìm thấy câu hỏi.");

            if (cauHoi.LoaiCauHoi == "TuLuan")
                return BadRequest("Câu hỏi tự luận không cần tùy chọn.");

            if (string.IsNullOrWhiteSpace(request.NoiDungTuyChon))
                return BadRequest("Nội dung tùy chọn không được để trống.");

            var tuyChon = new TuyChonCauHoi
            {
                MaTuyChon = "TC" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                MaCauHoi = cauHoiId,
                NoiDungTuyChon = request.NoiDungTuyChon.Trim()
            };
            _context.TuyChonCauHoi.Add(tuyChon);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm tùy chọn thành công.", MaTuyChon = tuyChon.MaTuyChon });
        }

        // PUT: api/AdminKhaoSat/{id}/CauHoi/{cauHoiId}/TuyChon/{tuyChonId} - Sửa tùy chọn
        [HttpPut("{id}/CauHoi/{cauHoiId}/TuyChon/{tuyChonId}")]
        public async Task<IActionResult> UpdateTuyChon(string id, string cauHoiId, string tuyChonId, [FromBody] TuyChonUpsertRequest request)
        {
            // Kiểm tra câu hỏi thuộc đúng khảo sát
            bool cauHoiTonTai = await _context.CauHoiKhaoSat
                .AnyAsync(c => c.MaCauHoi == cauHoiId && c.MaKhaoSat == id);
            if (!cauHoiTonTai)
                return NotFound("Không tìm thấy câu hỏi.");

            var tuyChon = await _context.TuyChonCauHoi
                .FirstOrDefaultAsync(t => t.MaTuyChon == tuyChonId && t.MaCauHoi == cauHoiId);
            if (tuyChon == null)
                return NotFound("Không tìm thấy tùy chọn.");

            if (string.IsNullOrWhiteSpace(request.NoiDungTuyChon))
                return BadRequest("Nội dung tùy chọn không được để trống.");

            tuyChon.NoiDungTuyChon = request.NoiDungTuyChon.Trim();
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật tùy chọn thành công." });
        }

        // DELETE: api/AdminKhaoSat/{id}/CauHoi/{cauHoiId}/TuyChon/{tuyChonId} - Xóa tùy chọn
        [HttpDelete("{id}/CauHoi/{cauHoiId}/TuyChon/{tuyChonId}")]
        public async Task<IActionResult> DeleteTuyChon(string id, string cauHoiId, string tuyChonId)
        {
            bool cauHoiTonTai = await _context.CauHoiKhaoSat
                .AnyAsync(c => c.MaCauHoi == cauHoiId && c.MaKhaoSat == id);
            if (!cauHoiTonTai)
                return NotFound("Không tìm thấy câu hỏi.");

            var tuyChon = await _context.TuyChonCauHoi
                .FirstOrDefaultAsync(t => t.MaTuyChon == tuyChonId && t.MaCauHoi == cauHoiId);
            if (tuyChon == null)
                return NotFound("Không tìm thấy tùy chọn.");

            // Không cho xóa nếu đã có ai chọn
            bool daCoLuaChon = await _context.CauTraLoi
                .AnyAsync(ct => ct.MaTuyChon == tuyChonId);
            if (daCoLuaChon)
                return BadRequest("Tùy chọn này đã có người chọn, không thể xóa.");

            _context.TuyChonCauHoi.Remove(tuyChon);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa tùy chọn thành công." });
        }

        // GET: api/AdminKhaoSat/{id}/results - Thống kê kết quả khảo sát
        [HttpGet("{id}/results")]
        public async Task<IActionResult> GetResults(string id)
        {
            var khaoSat = await _context.KhaoSat
                .Include(k => k.CauHoiKhaoSat)
                    .ThenInclude(c => c.TuyChonCauHoi)
                .FirstOrDefaultAsync(k => k.MaKhaoSat == id);

            if (khaoSat == null)
            {
                return NotFound("Không tìm thấy khảo sát.");
            }

            var tongSoPhieu = await _context.PhieuTraLoi.CountAsync(p => p.MaKhaoSat == id);

            var cauHoiResults = new List<object>();

            foreach (var ch in khaoSat.CauHoiKhaoSat)
            {
                // Chuẩn hoá về 2 giá trị tiếng Anh để Form dễ xử lý
                bool laTuLuan = ch.LoaiCauHoi == "TuLuan" || ch.LoaiCauHoi == "Tự luận";
                string loaiChuan = laTuLuan ? "TuLuan" : "TracNghiem";

                if (!laTuLuan)
                {
                    var thongKeTuyChon = new List<object>();
                    foreach (var tc in ch.TuyChonCauHoi)
                    {
                        var soNguoiChon = await _context.CauTraLoi
                            .CountAsync(ctl => ctl.MaCauHoi == ch.MaCauHoi && ctl.MaTuyChon == tc.MaTuyChon);

                        thongKeTuyChon.Add(new
                        {
                            tc.MaTuyChon,
                            tc.NoiDungTuyChon,
                            SoNguoiChon = soNguoiChon,
                            TyLe = tongSoPhieu > 0
                                ? Math.Round((double)soNguoiChon / tongSoPhieu * 100, 2)
                                : 0.0
                        });
                    }
                    cauHoiResults.Add(new
                    {
                        ch.MaCauHoi,
                        ch.NoiDungCauHoi,
                        LoaiCauHoi = loaiChuan,
                        KetQuaTN   = thongKeTuyChon,
                        KetQuaTuLuan = (object)null
                    });
                }
                else
                {
                    var cauTraLois = await _context.CauTraLoi
                        .Where(ctl => ctl.MaCauHoi == ch.MaCauHoi && ctl.NoiDungTuDien != null)
                        .Select(ctl => ctl.NoiDungTuDien)
                        .ToListAsync();

                    cauHoiResults.Add(new
                    {
                        ch.MaCauHoi,
                        ch.NoiDungCauHoi,
                        LoaiCauHoi   = loaiChuan,
                        KetQuaTN     = (object)null,
                        KetQuaTuLuan = cauTraLois
                    });
                }
            }

            return Ok(new
            {
                khaoSat.MaKhaoSat,
                khaoSat.TieuDe,
                TongSoNguoiTraLoi = tongSoPhieu,
                ChiTiet = cauHoiResults
            });
        }
    }
}

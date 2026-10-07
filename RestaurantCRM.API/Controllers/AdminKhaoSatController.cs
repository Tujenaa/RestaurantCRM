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
                .OrderByDescending(k => k.NgayTao)
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
                if (ch.LoaiCauHoi == "Trắc nghiệm")
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
                            TyLe = tongSoPhieu > 0 ? Math.Round((double)soNguoiChon / tongSoPhieu * 100, 2) : 0
                        });
                    }
                    cauHoiResults.Add(new { ch.MaCauHoi, ch.NoiDungCauHoi, ch.LoaiCauHoi, KetQua = thongKeTuyChon });
                }
                else
                {
                     // Câu hỏi tự luận
                     var cauTraLois = await _context.CauTraLoi
                        .Where(ctl => ctl.MaCauHoi == ch.MaCauHoi && ctl.NoiDungTuDien != null)
                        .Select(ctl => ctl.NoiDungTuDien)
                        .ToListAsync();
                        
                     cauHoiResults.Add(new { ch.MaCauHoi, ch.NoiDungCauHoi, ch.LoaiCauHoi, KetQua = cauTraLois });
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

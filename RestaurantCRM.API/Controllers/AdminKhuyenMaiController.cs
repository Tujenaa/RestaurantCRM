using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminKhuyenMaiController : ControllerBase
    {
        private readonly RestaurantCRMContext _context;

        public AdminKhuyenMaiController(RestaurantCRMContext context)
        {
            _context = context;
        }

        // GET: api/AdminKhuyenMai - Lấy danh sách chương trình khuyến mãi.
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var khuyenMais = await _context.ChuongTrinhKhuyenMais
                .OrderByDescending(k => k.NgayBatDau)
                .Select(k => new
                {
                    k.MaChuongTrinh,
                    k.TenChuongTrinh,
                    k.LoaiKhuyenMai,
                    k.LoaiGiam,
                    k.GiaTriGiam,
                    k.GiaTriDonToiThieu,
                    k.NgayBatDau,
                    k.NgayKetThuc,
                    k.SoLuong,
                    k.TrangThai
                })
                .ToListAsync();

            return Ok(khuyenMais);
        }

        // GET: api/AdminKhuyenMai/LoaiKhuyenMai - Lấy danh sách các loại khuyến mãi.
        [HttpGet("LoaiKhuyenMai")]
        public IActionResult GetLoaiKhuyenMai()
        {
            var loaiKhuyenMais = new[] { "Voucher", "GiamGiaMon" };
            return Ok(loaiKhuyenMais);
        }

        // GET: api/AdminKhuyenMai/LoaiGiam - Lấy danh sách các loại giảm giá.
        [HttpGet("LoaiGiam")]
        public IActionResult GetLoaiGiam()
        {
            var loaiGiams = new[] { "%", "VND" };
            return Ok(loaiGiams);
        }

        // GET: api/AdminKhuyenMai/{id} - Lấy chi tiết chương trình khuyến mãi.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var khuyenMai = await _context.ChuongTrinhKhuyenMais
                .Include(k => k.ChiTietKhuyenMaiMons)
                    .ThenInclude(c => c.MaMonNavigation)
                .FirstOrDefaultAsync(k => k.MaChuongTrinh == id);

            if (khuyenMai == null)
            {
                return NotFound("Không tìm thấy chương trình khuyến mãi.");
            }

            var result = new
            {
                khuyenMai.MaChuongTrinh,
                khuyenMai.TenChuongTrinh,
                khuyenMai.LoaiKhuyenMai,
                khuyenMai.LoaiGiam,
                khuyenMai.GiaTriGiam,
                khuyenMai.GiaTriDonToiThieu,
                khuyenMai.NgayBatDau,
                khuyenMai.NgayKetThuc,
                khuyenMai.SoLuong,
                khuyenMai.TrangThai,
                DanhSachMon = khuyenMai.ChiTietKhuyenMaiMons.Select(c => new
                {
                    c.MaChiTietKm,
                    c.MaMon,
                    TenMon = c.MaMonNavigation?.TenMon,
                    c.SoLuongApDung
                })
            };

            return Ok(result);
        }

        // POST: api/AdminKhuyenMai - Thêm chương trình khuyến mãi mới.
        [HttpPost]
        public async Task<IActionResult> Create(KhuyenMaiCreateRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var khuyenMai = new ChuongTrinhKhuyenMai
                {
                    MaChuongTrinh = "KM" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    TenChuongTrinh = request.TenChuongTrinh,
                    LoaiKhuyenMai = request.LoaiKhuyenMai,
                    LoaiGiam = request.LoaiGiam,
                    GiaTriGiam = request.GiaTriGiam,
                    GiaTriDonToiThieu = request.GiaTriDonToiThieu,
                    NgayBatDau = request.NgayBatDau,
                    NgayKetThuc = request.NgayKetThuc,
                    SoLuong = request.SoLuong,
                    TrangThai = "Active"
                };

                _context.ChuongTrinhKhuyenMais.Add(khuyenMai);

                if (request.DanhSachMon != null && request.DanhSachMon.Any())
                {
                    foreach (var item in request.DanhSachMon)
                    {
                        var chiTiet = new ChiTietKhuyenMaiMon
                        {
                            MaChiTietKm = "CTKM" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper(),
                            MaChuongTrinh = khuyenMai.MaChuongTrinh,
                            MaMon = item.MaMon,
                            SoLuongApDung = item.SoLuongApDung
                        };
                        _context.ChiTietKhuyenMaiMons.Add(chiTiet);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetById), new { id = khuyenMai.MaChuongTrinh }, new { message = "Thêm khuyến mãi thành công.", MaChuongTrinh = khuyenMai.MaChuongTrinh });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi hệ thống: " + ex.Message);
            }
        }

        // PUT: api/AdminKhuyenMai/{id}/Status - Cập nhật trạng thái khuyến mãi.
        [HttpPut("{id}/Status")]
        public async Task<IActionResult> UpdateStatus(string id, KhuyenMaiStatusRequest request)
        {
            var khuyenMai = await _context.ChuongTrinhKhuyenMais.FindAsync(id);
            if (khuyenMai == null)
            {
                return NotFound("Không tìm thấy chương trình khuyến mãi.");
            }

            var validStatuses = new[] { "Active", "Inactive", "Expired" };
            if (!validStatuses.Contains(request.Status))
            {
                return BadRequest("Trạng thái không hợp lệ. (Chỉ cho phép Active, Inactive, Expired).");
            }

            khuyenMai.TrangThai = request.Status;
            await _context.SaveChangesAsync();

            return Ok($"Đã cập nhật trạng thái khuyến mãi thành {request.Status}.");
        }

        // DELETE: api/AdminKhuyenMai/{id} - Xóa một chương trình khuyến mãi.
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var khuyenMai = await _context.ChuongTrinhKhuyenMais
                .Include(k => k.DonHangs)
                .Include(k => k.ChiTietKhuyenMaiMons)
                .FirstOrDefaultAsync(k => k.MaChuongTrinh == id);

            if (khuyenMai == null)
            {
                return NotFound("Không tìm thấy chương trình khuyến mãi.");
            }

            if (khuyenMai.DonHangs.Any())
            {
                return BadRequest("Chương trình khuyến mãi này đã được sử dụng trong Đơn hàng. Không thể xóa để bảo toàn dữ liệu lịch sử. Vui lòng chuyển trạng thái thành Inactive hoặc Expired.");
            }

            // Xóa các chi tiết trước
            if (khuyenMai.ChiTietKhuyenMaiMons.Any())
            {
                _context.ChiTietKhuyenMaiMons.RemoveRange(khuyenMai.ChiTietKhuyenMaiMons);
            }

            _context.ChuongTrinhKhuyenMais.Remove(khuyenMai);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

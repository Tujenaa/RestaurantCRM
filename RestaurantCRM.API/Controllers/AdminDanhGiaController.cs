using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminDanhGiaController : ControllerBase
    {
        private readonly RestaurantCRMContext _context;

        public AdminDanhGiaController(RestaurantCRMContext context)
        {
            _context = context;
        }

        // GET: api/AdminDanhGia - Lấy toàn bộ danh sách đánh giá
        [HttpGet]
        public async Task<IActionResult> GetAllDanhGia()
        {
            var danhGias = await _context.DanhGias
                .Include(d => d.MaKhachHangNavigation)
                .Include(d => d.MaMonNavigation)
                .Include(d => d.TraLoiDanhGias)
                .OrderByDescending(d => d.NgayDanhGia)
                .Select(d => new
                {
                    d.MaDanhGia,
                    d.MaMon,
                    TenMon = d.MaMonNavigation != null ? d.MaMonNavigation.TenMon : null,
                    d.SoSao,
                    d.NoiDung,
                    d.NgayDanhGia,
                    TenKhachHang = d.MaKhachHangNavigation != null ? d.MaKhachHangNavigation.HoTen : "Khách ẩn danh",
                    TongSoTraLoi = d.TraLoiDanhGias.Count,
                    HoiThoai = d.TraLoiDanhGias.OrderBy(t => t.NgayGui).Select(t => new
                    {
                        t.MaTraLoi,
                        t.NguoiGui,
                        t.NoiDung,
                        t.NgayGui,
                        NguoiTraLoi = t.NguoiGui == "KhachHang" && t.MaKhachHangNavigation != null ? t.MaKhachHangNavigation.HoTen : 
                                      (t.NguoiGui == "NhanVien" && t.MaNhanVienNavigation != null ? t.MaNhanVienNavigation.HoTen : "Ẩn danh")
                    })
                })
                .ToListAsync();

            return Ok(danhGias);
        }

        // POST: api/AdminDanhGia/{id}/Reply - Nhân viên phản hồi đánh giá
        [HttpPost("{id}/Reply")]
        public async Task<IActionResult> CreateReply(string id, ReplyRequest request)
        {
            var danhGia = await _context.DanhGias.FindAsync(id);
            if (danhGia == null)
            {
                return NotFound("Không tìm thấy đánh giá.");
            }

            var traLoi = new TraLoiDanhGia
            {
                MaTraLoi = "TL" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                MaDanhGia = id,
                NguoiGui = "NhanVien",
                MaKhachHang = null,
                MaNhanVien = request.NguoiGuiId,
                NoiDung = request.NoiDung,
                NgayGui = DateTime.Now
            };

            _context.TraLoiDanhGias.Add(traLoi);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Gửi phản hồi thành công", maTraLoi = traLoi.MaTraLoi });
        }

        // DELETE: api/AdminDanhGia/{id} - Xóa đánh giá (và các phản hồi liên quan)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDanhGia(string id)
        {
            var danhGia = await _context.DanhGias
                .Include(d => d.TraLoiDanhGias)
                .FirstOrDefaultAsync(d => d.MaDanhGia == id);

            if (danhGia == null)
            {
                return NotFound("Không tìm thấy đánh giá.");
            }

            if (danhGia.TraLoiDanhGias.Any())
            {
                _context.TraLoiDanhGias.RemoveRange(danhGia.TraLoiDanhGias);
            }

            _context.DanhGias.Remove(danhGia);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

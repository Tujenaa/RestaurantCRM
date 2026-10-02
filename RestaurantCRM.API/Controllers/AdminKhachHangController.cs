using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminKhachHangController : ControllerBase
    {
        private readonly RestaurantCRMContext _context;

        public AdminKhachHangController(RestaurantCRMContext context)
        {
            _context = context;
        }

        // GET: api/AdminKhachHang - Lấy danh sách khách hàng.
        [HttpGet]
        public async Task<IActionResult> GetKhachHangs()
        {
            var khachHangs = await _context.KhachHangs
                .Select(k => new
                {
                    k.MaKhachHang,
                    k.HoTen,
                    k.SoDienThoai,
                    k.Email,
                    k.NgaySinh,
                    k.GioiTinh,
                    k.SoThich,
                    k.TrangThai
                })
                .ToListAsync();

            return Ok(khachHangs);
        }

        // GET: api/AdminKhachHang/{id} - Lấy chi tiết thông tin khách hàng.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetKhachHang(string id)
        {
            var khachHang = await _context.KhachHangs
                .Include(k => k.DonHangs)
                .Where(k => k.MaKhachHang == id)
                .Select(k => new
                {
                    k.MaKhachHang,
                    k.HoTen,
                    k.SoDienThoai,
                    k.Email,
                    k.NgaySinh,
                    k.GioiTinh,
                    k.SoThich,
                    k.TrangThai,
                    LichSuDonHang = k.DonHangs.Select(d => new
                    {
                        d.MaDonHang,
                        d.NgayDat,
                        TongTien = d.TongThanhToan,
                        d.TrangThai
                    })
                })
                .FirstOrDefaultAsync();

            if (khachHang == null)
            {
                return NotFound("Không tìm thấy khách hàng.");
            }

            return Ok(khachHang);
        }

        // POST: api/AdminKhachHang - Thêm một khách hàng mới.
        [HttpPost]
        public async Task<IActionResult> CreateKhachHang(AdminCreateKhachHangRequest request)
        {
            if (!string.IsNullOrEmpty(request.Email) && await _context.KhachHangs.AnyAsync(k => k.Email == request.Email))
            {
                return BadRequest("Email đã được sử dụng.");
            }
            if (!string.IsNullOrEmpty(request.SoDienThoai) && await _context.KhachHangs.AnyAsync(k => k.SoDienThoai == request.SoDienThoai))
            {
                return BadRequest("Số điện thoại đã được sử dụng.");
            }

            var newMaKhachHang = "KH" + DateTime.Now.Ticks.ToString().Substring(8, 6);

            var khachHang = new KhachHang
            {
                MaKhachHang = newMaKhachHang,
                HoTen = request.HoTen,
                SoDienThoai = request.SoDienThoai,
                Email = request.Email,
                NgaySinh = request.NgaySinh,
                GioiTinh = request.GioiTinh,
                MatKhau = BCrypt.Net.BCrypt.HashPassword("123456"), // Mật khẩu mặc định
                TrangThai = "Active"
            };

            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm khách hàng thành công.", maKhachHang = khachHang.MaKhachHang });
        }

        // PUT: api/AdminKhachHang/{id}/Status - Cập nhật trạng thái khóa/mở khóa tài khoản khách hàng.
        [HttpPut("{id}/Status")]
        public async Task<IActionResult> UpdateStatus(string id, AdminLockKhachHangRequest request)
        {
            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null)
            {
                return NotFound("Không tìm thấy khách hàng.");
            }

            if (request.TrangThai != "Active" && request.TrangThai != "Locked")
            {
                return BadRequest("Trạng thái không hợp lệ. Chỉ chấp nhận 'Active' hoặc 'Locked'.");
            }

            khachHang.TrangThai = request.TrangThai;
            await _context.SaveChangesAsync();

            return Ok($"Đã cập nhật trạng thái khách hàng thành {request.TrangThai}.");
        }

        // DELETE: api/AdminKhachHang/{id} - Xóa một khách hàng.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKhachHang(string id)
        {
            var khachHang = await _context.KhachHangs
                .Include(k => k.DonHangs)
                .Include(k => k.DanhGia)
                .Include(k => k.PhanHois)
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (khachHang == null)
            {
                return NotFound("Không tìm thấy khách hàng.");
            }

            if (khachHang.DonHangs.Any() || khachHang.DanhGia.Any() || khachHang.PhanHois.Any())
            {
                return BadRequest("Khách hàng này đã phát sinh giao dịch (Đơn hàng, Đánh giá hoặc Phản hồi). Không thể xóa dữ liệu để bảo toàn lịch sử hệ thống. Vui lòng sử dụng tính năng 'Khóa tài khoản' thay thế.");
            }

            _context.KhachHangs.Remove(khachHang);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

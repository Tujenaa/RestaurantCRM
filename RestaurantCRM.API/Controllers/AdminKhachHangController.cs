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
        private readonly RestaurantCrmContext _context;

        public AdminKhachHangController(RestaurantCrmContext context)
        {
            _context = context;
        }

        // GET: api/AdminKhachHang - Lấy danh sách khách hàng (hỗ trợ tìm kiếm theo từ khóa).
        [HttpGet]
        public async Task<IActionResult> GetKhachHangs([FromQuery] string? search)
        {
            var query = _context.KhachHang.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(k =>
                    (k.MaKhachHang != null && k.MaKhachHang.Contains(s)) ||
                    (k.HoTen != null && k.HoTen.Contains(s)) ||
                    (k.SoDienThoai != null && k.SoDienThoai.Contains(s)) ||
                    (k.Email != null && k.Email.Contains(s)));
            }

            var khachHangs = await query
                .OrderBy(k => k.MaKhachHang)
                .Select(k => new
                {
                    k.MaKhachHang,
                    k.HoTen,
                    k.SoDienThoai,
                    k.Email,
                    k.NgaySinh,
                    k.GioiTinh,
                    k.DiaChi,
                    k.TrangThai
                })
                .ToListAsync();

            return Ok(khachHangs);
        }

        // GET: api/AdminKhachHang/{id} - Lấy chi tiết thông tin khách hàng.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetKhachHang(string id)
        {
            var khachHang = await _context.KhachHang
                .Include(k => k.HoaDon)
                .Where(k => k.MaKhachHang == id)
                .Select(k => new
                {
                    k.MaKhachHang,
                    k.HoTen,
                    k.SoDienThoai,
                    k.Email,
                    k.NgaySinh,
                    k.GioiTinh,
                    k.DiaChi,
                    k.TrangThai,
                    LichSuHoaDon = k.HoaDon.Select(d => new
                    {
                        d.MaHoaDon,
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
            if (!string.IsNullOrEmpty(request.Email) && await _context.KhachHang.AnyAsync(k => k.Email == request.Email))
            {
                return BadRequest("Email đã được sử dụng.");
            }
            if (!string.IsNullOrEmpty(request.SoDienThoai) && await _context.KhachHang.AnyAsync(k => k.SoDienThoai == request.SoDienThoai))
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
                DiaChi = request.DiaChi,
                MatKhau = BCrypt.Net.BCrypt.HashPassword("123456"), // Mật khẩu mặc định
                TrangThai = !string.IsNullOrEmpty(request.TrangThai) ? request.TrangThai : "Active"
            };

            _context.KhachHang.Add(khachHang);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm khách hàng thành công.", maKhachHang = khachHang.MaKhachHang });
        }

        // PUT: api/AdminKhachHang/{id} - Cập nhật thông tin khách hàng.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateKhachHang(string id, AdminUpdateKhachHangRequest request)
        {
            var khachHang = await _context.KhachHang.FindAsync(id);
            if (khachHang == null)
            {
                return NotFound("Không tìm thấy khách hàng.");
            }

            if (!string.IsNullOrEmpty(request.Email) &&
                await _context.KhachHang.AnyAsync(k => k.Email == request.Email && k.MaKhachHang != id))
            {
                return BadRequest("Email đã được sử dụng bởi khách hàng khác.");
            }
            if (!string.IsNullOrEmpty(request.SoDienThoai) &&
                await _context.KhachHang.AnyAsync(k => k.SoDienThoai == request.SoDienThoai && k.MaKhachHang != id))
            {
                return BadRequest("Số điện thoại đã được sử dụng bởi khách hàng khác.");
            }

            if (!string.IsNullOrWhiteSpace(request.HoTen))
                khachHang.HoTen = request.HoTen.Trim();
            khachHang.SoDienThoai = request.SoDienThoai?.Trim();
            khachHang.Email = request.Email?.Trim();
            khachHang.NgaySinh = request.NgaySinh;
            khachHang.GioiTinh = request.GioiTinh;
            khachHang.DiaChi = request.DiaChi;
            if (!string.IsNullOrEmpty(request.TrangThai))
                khachHang.TrangThai = request.TrangThai;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thông tin khách hàng thành công." });
        }

        // PUT: api/AdminKhachHang/{id}/Status - Cập nhật trạng thái khóa/mở khóa tài khoản khách hàng.
        [HttpPut("{id}/Status")]
        public async Task<IActionResult> UpdateStatus(string id, AdminLockKhachHangRequest request)
        {
            var khachHang = await _context.KhachHang.FindAsync(id);
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
            var khachHang = await _context.KhachHang
                .Include(k => k.HoaDon)
                .Include(k => k.DanhGia)
                .Include(k => k.PhanHoi)
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (khachHang == null)
            {
                return NotFound("Không tìm thấy khách hàng.");
            }

            if (khachHang.HoaDon.Any() || khachHang.DanhGia.Any() || khachHang.PhanHoi.Any())
            {
                return BadRequest("Khách hàng này đã phát sinh giao dịch (Đơn hàng, Đánh giá hoặc Phản hồi). Không thể xóa dữ liệu để bảo toàn lịch sử hệ thống. Vui lòng sử dụng tính năng 'Khóa tài khoản' thay thế.");
            }

            _context.KhachHang.Remove(khachHang);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

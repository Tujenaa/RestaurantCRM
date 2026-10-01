using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhachHangController : ControllerBase
    {
        private readonly RestaurantCRMContext _context;

        public KhachHangController(RestaurantCRMContext context)
        {
            _context = context;
        }

        // POST: api/KhachHang/DangKy - Đăng ký tài khoản khách hàng mới
        [HttpPost("DangKy")]
        public async Task<IActionResult> DangKy(RegisterRequest request)
        {
            if (await _context.KhachHangs.AnyAsync(k => k.Email == request.Email && !string.IsNullOrEmpty(request.Email)))
            {
                return BadRequest("Email đã được sử dụng.");
            }
            if (await _context.KhachHangs.AnyAsync(k => k.SoDienThoai == request.SoDienThoai && !string.IsNullOrEmpty(request.SoDienThoai)))
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
                MatKhau = request.MatKhau, // TODO: Cần Hash password khi đưa vào thực tế
                TrangThai = "Active"
            };

            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công", maKhachHang = khachHang.MaKhachHang });
        }

        // POST: api/KhachHang/DangNhap - Xác thực đăng nhập khách hàng
        [HttpPost("DangNhap")]
        public async Task<IActionResult> DangNhap(LoginRequest request)
        {
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k =>
                (k.Email == request.EmailOrPhone || k.SoDienThoai == request.EmailOrPhone) &&
                k.MatKhau == request.MatKhau);

            if (khachHang == null)
            {
                return Unauthorized("Email/Số điện thoại hoặc mật khẩu không đúng.");
            }

            if (khachHang.TrangThai != "Active")
            {
                return BadRequest("Tài khoản đã bị khóa.");
            }

            // TODO: Tạo JWT Token thật khi cài đặt JWT Auth
            var fakeToken = "jwt_token_placeholder_for_" + khachHang.MaKhachHang;

            return Ok(new
            {
                message = "Đăng nhập thành công",
                token = fakeToken,
                khachHang = new
                {
                    khachHang.MaKhachHang,
                    khachHang.HoTen,
                    khachHang.Email,
                    khachHang.SoDienThoai
                }
            });
        }

        // GET: api/KhachHang/ThongTin/{id} - Lấy thông tin chi tiết của khách hàng
        // Lưu ý: Sau khi có JWT, ta sẽ lấy ID từ token thay vì truyền qua URL
        [HttpGet("ThongTin/{id}")]
        public async Task<IActionResult> ThongTin(string id)
        {
            var khachHang = await _context.KhachHangs
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (khachHang == null)
            {
                return NotFound("Không tìm thấy thông tin khách hàng.");
            }

            return Ok(new
            {
                khachHang.MaKhachHang,
                khachHang.HoTen,
                khachHang.Email,
                khachHang.SoDienThoai,
                khachHang.NgaySinh,
                khachHang.GioiTinh,
                khachHang.SoThich
            });
        }

        // PUT: api/KhachHang/ThongTin/{id} - Cập nhật thông tin cá nhân khách hàng
        [HttpPut("ThongTin/{id}")]
        public async Task<IActionResult> CapNhatThongTin(string id, KhachHang updatedKhachHang)
        {
            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null)
            {
                return NotFound("Không tìm thấy thông tin khách hàng.");
            }

            khachHang.HoTen = updatedKhachHang.HoTen;
            khachHang.Email = updatedKhachHang.Email;
            khachHang.SoDienThoai = updatedKhachHang.SoDienThoai;
            khachHang.NgaySinh = updatedKhachHang.NgaySinh;
            khachHang.GioiTinh = updatedKhachHang.GioiTinh;
            khachHang.SoThich = updatedKhachHang.SoThich;

            await _context.SaveChangesAsync();

            return Ok("Cập nhật thông tin thành công.");
        }
    }
}
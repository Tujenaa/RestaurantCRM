using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;
using RestaurantCRM.API.DTOs;
using RestaurantCRM.API.Helpers;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhachHangController : ControllerBase
    {
        private readonly RestaurantCrmContext _context;

        public KhachHangController(RestaurantCrmContext context)
        {
            _context = context;
        }

        // POST: api/KhachHang/DangKy - Đăng ký tài khoản khách hàng mới
        [HttpPost("DangKy")]
        public async Task<IActionResult> DangKy(RegisterRequest request)
        {
            if (await _context.KhachHang.AnyAsync(k => k.Email == request.Email && !string.IsNullOrEmpty(request.Email)))
            {
                return BadRequest("Email đã được sử dụng.");
            }
            if (await _context.KhachHang.AnyAsync(k => k.SoDienThoai == request.SoDienThoai && !string.IsNullOrEmpty(request.SoDienThoai)))
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
                MatKhau = BCrypt.Net.BCrypt.HashPassword(request.MatKhau),
                TrangThai = "Active"
            };

            _context.KhachHang.Add(khachHang);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công", maKhachHang = khachHang.MaKhachHang });
        }

        // POST: api/KhachHang/DangNhap - Xác thực đăng nhập khách hàng
        [HttpPost("DangNhap")]
        public async Task<IActionResult> DangNhap(LoginRequest request)
        {
            var khachHang = await _context.KhachHang.FirstOrDefaultAsync(k =>
                (k.Email == request.EmailOrPhone || k.SoDienThoai == request.EmailOrPhone));

            if (khachHang == null || !PasswordVerifier.Verify(request.MatKhau, khachHang.MatKhau))
            {
                return Unauthorized("Email/Số điện thoại hoặc mật khẩu không đúng.");
            }

            if (khachHang.TrangThai != "Active")
            {
                return BadRequest("Tài khoản đã bị khóa.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("Chuoi_Bao_Mat_Bi_Mat_Cua_Nha_Hang_CRM_123456");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, khachHang.MaKhachHang),
                    new Claim(ClaimTypes.Name, khachHang.HoTen ?? ""),
                    new Claim(ClaimTypes.Role, "KhachHang")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return Ok(new
            {
                message = "Đăng nhập thành công",
                token = jwtToken,
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
            var khachHang = await _context.KhachHang
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
        public async Task<IActionResult> CapNhatThongTin(string id, UpdateProfileRequest request)
        {
            if (!string.IsNullOrEmpty(request.Email) && await _context.KhachHang.AnyAsync(k => k.Email == request.Email && k.MaKhachHang != id))
            {
                return BadRequest("Email đã được sử dụng bởi người khác.");
            }
            if (!string.IsNullOrEmpty(request.SoDienThoai) && await _context.KhachHang.AnyAsync(k => k.SoDienThoai == request.SoDienThoai && k.MaKhachHang != id))
            {
                return BadRequest("Số điện thoại đã được sử dụng bởi người khác.");
            }

            var khachHang = await _context.KhachHang.FindAsync(id);
            if (khachHang == null)
            {
                return NotFound("Không tìm thấy thông tin khách hàng.");
            }

            khachHang.HoTen = request.HoTen;
            khachHang.Email = request.Email;
            khachHang.SoDienThoai = request.SoDienThoai;
            khachHang.NgaySinh = request.NgaySinh;
            khachHang.GioiTinh = request.GioiTinh;
            khachHang.SoThich = request.SoThich;

            await _context.SaveChangesAsync();

            return Ok("Cập nhật thông tin thành công.");
        }

        // PUT: api/KhachHang/DoiMatKhau/{id} - Đổi mật khẩu
        [HttpPut("DoiMatKhau/{id}")]
        public async Task<IActionResult> DoiMatKhau(string id, ChangePasswordRequest request)
        {
            var khachHang = await _context.KhachHang.FindAsync(id);
            if (khachHang == null)
            {
                return NotFound("Không tìm thấy thông tin khách hàng.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.MatKhauCu, khachHang.MatKhau))
            {
                return BadRequest("Mật khẩu cũ không chính xác.");
            }

            khachHang.MatKhau = BCrypt.Net.BCrypt.HashPassword(request.MatKhauMoi);
            await _context.SaveChangesAsync();

            return Ok("Đổi mật khẩu thành công.");
        }
    }
}

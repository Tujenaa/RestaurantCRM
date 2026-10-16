using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.DTOs;
using RestaurantCRM.API.Helpers;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminAuthController : ControllerBase
    {
        private readonly RestaurantCrmContext _context;

        public AdminAuthController(RestaurantCrmContext context)
        {
            _context = context;
        }

        // POST: api/AdminAuth/Login - Đăng nhập cho hệ thống (Admin & Nhân viên)
        [HttpPost("Login")]
        public async Task<IActionResult> Login(AdminLoginRequest request)
        {
            var nhanVien = await _context.NhanVien
                .Include(n => n.MaVaiTroNavigation)
                .FirstOrDefaultAsync(n => n.TenDangNhap == request.TenDangNhap);

            if (nhanVien != null && PasswordVerifier.Verify(request.MatKhau, nhanVien.MatKhau))
            {
                if (nhanVien.TrangThai != "Active")
                {
                    return BadRequest("Tài khoản đã bị khóa hoặc ngừng hoạt động.");
                }

                return Ok(new
                {
                    message = "Đăng nhập thành công",
                    token = "fake_jwt_token_for_" + nhanVien.MaNhanVien,
                    role = nhanVien.MaVaiTroNavigation?.TenVaiTro ?? "UnknownRole",
                    roleId = nhanVien.MaVaiTro,
                    userInfo = new
                    {
                        Id = nhanVien.MaNhanVien,
                        HoTen = nhanVien.HoTen,
                        Username = nhanVien.TenDangNhap
                    }
                });
            }

            return Unauthorized("Tên đăng nhập hoặc mật khẩu không đúng.");
        }
    }
}

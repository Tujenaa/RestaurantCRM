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
        private readonly RestaurantCRMContext _context;

        public AdminAuthController(RestaurantCRMContext context)
        {
            _context = context;
        }

        // POST: api/AdminAuth/Login - Đăng nhập đa bảng dành cho Admin và Nhân viên
        [HttpPost("Login")]
        public async Task<IActionResult> Login(AdminLoginRequest request)
        {
            // 1. Kiểm tra trong bảng TaiKhoanAdmin trước (Quyền cao nhất)
            var admin = await _context.TaiKhoanAdmins
                .FirstOrDefaultAsync(a => a.TenDangNhap == request.TenDangNhap);

            if (admin != null && PasswordVerifier.Verify(request.MatKhau, admin.MatKhau))
            {
                // TODO: Thay thế bằng JWT thật
                return Ok(new
                {
                    message = "Đăng nhập thành công với quyền Admin tối cao",
                    token = "fake_jwt_token_for_" + admin.MaAdmin,
                    role = "SuperAdmin",
                    userInfo = new
                    {
                        Id = admin.MaAdmin,
                        HoTen = admin.HoTen,
                        Username = admin.TenDangNhap
                    }
                });
            }

            // 2. Nếu không phải Admin, kiểm tra trong bảng NhanVien
            var nhanVien = await _context.NhanViens
                .Include(n => n.MaVaiTroNavigation)
                .FirstOrDefaultAsync(n => n.TenDangNhap == request.TenDangNhap);

            if (nhanVien != null && PasswordVerifier.Verify(request.MatKhau, nhanVien.MatKhau))
            {
                if (nhanVien.TrangThai != "Active")
                {
                    return BadRequest("Tài khoản nhân viên đã bị khóa hoặc ngừng hoạt động.");
                }

                // TODO: Thay thế bằng JWT thật
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

            // 3. Không tìm thấy ở cả 2 bảng
            return Unauthorized("Tên đăng nhập hoặc mật khẩu không đúng.");
        }
    }
}

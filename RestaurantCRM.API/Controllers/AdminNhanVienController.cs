using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.DTOs;
using RestaurantCRM.API.Helpers;
using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.BusinessLogic.Services;
using System.Security.Cryptography;
using System.Text;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminNhanVienController : ControllerBase
    {
        private readonly IRestaurantService _service;

        public AdminNhanVienController(IRestaurantService service)
        {
            _service = service;
        }

        // GET: api/AdminNhanVien - Lấy danh sách nhân viên có tìm kiếm & lọc
        [HttpGet]
        public async Task<IActionResult> GetNhanViens([FromQuery] string? search, [FromQuery] string? maVaiTro, [FromQuery] string? trangThai)
        {
            var query = _service.Query<NhanVien>()
                .Include(n => n.MaVaiTroNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLowerInvariant();
                query = query.Where(n =>
                    (n.MaNhanVien != null && n.MaNhanVien.ToLower().Contains(s)) ||
                    (n.HoTen != null && n.HoTen.ToLower().Contains(s)) ||
                    (n.TenDangNhap != null && n.TenDangNhap.ToLower().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(maVaiTro))
            {
                query = query.Where(n => n.MaVaiTro == maVaiTro);
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(n => n.TrangThai == trangThai);
            }

            var nhanViens = await query
                .OrderBy(n => n.MaNhanVien)
                .ToListAsync();

            var result = nhanViens.Select(n => new NhanVienDto
            {
                MaNhanVien = n.MaNhanVien,
                MaVaiTro = n.MaVaiTro,
                TenVaiTro = n.MaVaiTroNavigation?.TenVaiTro ?? n.MaVaiTro,
                TenDangNhap = n.TenDangNhap,
                HoTen = n.HoTen,
                TrangThai = n.TrangThai,
                Permissions = PermissionCatalog.ParsePermissions(n.MaVaiTroNavigation?.MoTa, n.MaVaiTroNavigation?.TenVaiTro)
            }).ToList();

            return Ok(result);
        }

        // GET: api/AdminNhanVien/{id} - Lấy chi tiết nhân viên
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNhanVien(string id)
        {
            var n = await _service.Query<NhanVien>()
                .Include(x => x.MaVaiTroNavigation)
                .FirstOrDefaultAsync(x => x.MaNhanVien == id);

            if (n == null) return NotFound("Không tìm thấy nhân viên.");

            return Ok(new NhanVienDto
            {
                MaNhanVien = n.MaNhanVien,
                MaVaiTro = n.MaVaiTro,
                TenVaiTro = n.MaVaiTroNavigation?.TenVaiTro ?? n.MaVaiTro,
                TenDangNhap = n.TenDangNhap,
                HoTen = n.HoTen,
                TrangThai = n.TrangThai,
                Permissions = PermissionCatalog.ParsePermissions(n.MaVaiTroNavigation?.MoTa, n.MaVaiTroNavigation?.TenVaiTro)
            });
        }

        // POST: api/AdminNhanVien - Thêm mới tài khoản nhân viên
        [HttpPost]
        public async Task<IActionResult> CreateNhanVien([FromBody] CreateNhanVienRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TenDangNhap) || string.IsNullOrWhiteSpace(request.MatKhau) || string.IsNullOrWhiteSpace(request.HoTen))
            {
                return BadRequest("Họ tên, tên đăng nhập và mật khẩu là bắt buộc.");
            }

            var username = request.TenDangNhap.Trim().ToLowerInvariant();
            var existsUsername = await _service.Query<NhanVien>().AnyAsync(n => n.TenDangNhap != null && n.TenDangNhap.ToLower() == username);
            if (existsUsername)
            {
                return BadRequest($"Tên đăng nhập '{request.TenDangNhap}' đã tồn tại.");
            }

            // Tự sinh mã NV nếu không cung cấp
            string maNV = request.MaNhanVien?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(maNV))
            {
                var count = await _service.Query<NhanVien>().CountAsync();
                maNV = $"NV{(count + 1):D2}";
                while (await _service.Query<NhanVien>().AnyAsync(n => n.MaNhanVien == maNV))
                {
                    count++;
                    maNV = $"NV{(count + 1):D2}";
                }
            }
            else
            {
                if (await _service.Query<NhanVien>().AnyAsync(n => n.MaNhanVien == maNV))
                {
                    return BadRequest($"Mã nhân viên '{maNV}' đã tồn tại.");
                }
            }

            // Băm mật khẩu dạng MD5 khớp hệ thống hiện tại
            var md5Pass = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(request.MatKhau.Trim()))).ToLowerInvariant();

            var entity = new NhanVien
            {
                MaNhanVien = maNV,
                TenDangNhap = username,
                MatKhau = md5Pass,
                HoTen = request.HoTen.Trim(),
                MaVaiTro = request.MaVaiTro,
                TrangThai = string.IsNullOrWhiteSpace(request.TrangThai) ? "Active" : request.TrangThai
            };

            _service.Add(entity);
            await _service.SaveChangesAsync();

            return Ok(new { message = "Tạo tài khoản nhân viên thành công", maNhanVien = entity.MaNhanVien });
        }

        // PUT: api/AdminNhanVien/{id} - Cập nhật thông tin nhân viên
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNhanVien(string id, [FromBody] UpdateNhanVienRequest request)
        {
            var entity = await _service.Query<NhanVien>().FirstOrDefaultAsync(n => n.MaNhanVien == id);
            if (entity == null) return NotFound("Không tìm thấy nhân viên.");

            entity.HoTen = request.HoTen.Trim();
            entity.MaVaiTro = request.MaVaiTro;
            if (!string.IsNullOrWhiteSpace(request.TrangThai))
            {
                entity.TrangThai = request.TrangThai;
            }

            _service.Update(entity);
            await _service.SaveChangesAsync();
            return Ok(new { message = "Cập nhật nhân viên thành công." });
        }

        // PUT: api/AdminNhanVien/{id}/doi-mat-khau - Đổi/Đặt lại mật khẩu
        [HttpPut("{id}/doi-mat-khau")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] NhanVienChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MatKhauMoi))
            {
                return BadRequest("Mật khẩu mới không được để trống.");
            }

            var entity = await _service.Query<NhanVien>().FirstOrDefaultAsync(n => n.MaNhanVien == id);
            if (entity == null) return NotFound("Không tìm thấy nhân viên.");

            var md5Pass = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(request.MatKhauMoi.Trim()))).ToLowerInvariant();
            entity.MatKhau = md5Pass;

            _service.Update(entity);
            await _service.SaveChangesAsync();
            return Ok(new { message = "Đổi mật khẩu thành công." });
        }

        // DELETE: api/AdminNhanVien/{id} - Vô hiệu hóa hoặc xóa tài khoản
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNhanVien(string id)
        {
            var entity = await _service.Query<NhanVien>()
                .Include(n => n.HoaDon)
                .Include(n => n.PhieuNhapHang)
                .FirstOrDefaultAsync(n => n.MaNhanVien == id);

            if (entity == null) return NotFound("Không tìm thấy nhân viên.");

            if (entity.TenDangNhap?.ToLower() == "admin")
            {
                return BadRequest("Không thể xóa tài khoản admin quản trị tối cao của hệ thống.");
            }

            // Nếu đã phát sinh hóa đơn hoặc phiếu nhập, chuyển trạng thái Inactive thay vì xóa cứng
            if (entity.HoaDon.Count > 0 || entity.PhieuNhapHang.Count > 0)
            {
                entity.TrangThai = "Inactive";
                _service.Update(entity);
                await _service.SaveChangesAsync();
                return Ok(new { message = "Tài khoản đã có dữ liệu giao dịch hóa đơn/phiếu nhập, hệ thống đã tự động chuyển sang trạng thái Ngừng hoạt động (Inactive)." });
            }

            _service.Remove(entity);
            await _service.SaveChangesAsync();
            return Ok(new { message = "Đã xóa tài khoản nhân viên thành công." });
        }
    }
}

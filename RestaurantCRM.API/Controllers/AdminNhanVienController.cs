
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.DTOs;
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
                    .ThenInclude(v => v!.VaiTroQuyen)
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
                .OrderByDescending(n => n.MaNhanVien)
                .ToListAsync();

            var result = nhanViens.Select(n => new NhanVienDto
            {
                MaNhanVien = n.MaNhanVien,
                MaVaiTro = n.MaVaiTro,
                TenVaiTro = n.MaVaiTroNavigation?.TenVaiTro ?? n.MaVaiTro,
                TenDangNhap = n.TenDangNhap,
                HoTen = n.HoTen,
                TrangThai = n.TrangThai,
                // Quyền chỉ lấy từ VAI_TRO_QUYEN, giống lúc đăng nhập
                Permissions = n.MaVaiTroNavigation?.VaiTroQuyen?.Select(q => q.MaQuyen).Distinct().ToList() ?? new List<string>()
            }).ToList();

            return Ok(result);
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

            // Mã NV luôn do hệ thống tự sinh: số lớn nhất trong các mã NVxx hiện có + 1
            var existingCodes = await _service.Query<NhanVien>()
                .Where(n => n.MaNhanVien.StartsWith("NV"))
                .Select(n => n.MaNhanVien)
                .ToListAsync();
            int next = existingCodes
                .Select(c => int.TryParse(c.Substring(2), out var num) ? num : 0)
                .DefaultIfEmpty(0)
                .Max() + 1;
            string maNV = $"NV{next:D2}";
            while (await _service.Query<NhanVien>().AnyAsync(n => n.MaNhanVien == maNV))
            {
                next++;
                maNV = $"NV{next:D2}";
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

        // DELETE: api/AdminNhanVien/{id} - Vô hiệu hóa / Tạm khóa tài khoản nhân viên (Bảo lưu dữ liệu lịch sử)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNhanVien(string id)
        {
            var entity = await _service.Query<NhanVien>()
                .FirstOrDefaultAsync(n => n.MaNhanVien == id);

            if (entity == null) return NotFound("Không tìm thấy nhân viên.");

            if (entity.TenDangNhap?.ToLower() == "admin")
            {
                return BadRequest("Không thể khóa tài khoản admin quản trị tối cao của hệ thống.");
            }

            // Nếu tài khoản đã ở trạng thái Inactive (tạm khóa)
            if (string.Equals(entity.TrangThai, "Inactive", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Tài khoản nhân viên này đã ở trạng thái tạm khóa (Inactive). Hệ thống bảo lưu toàn bộ dữ liệu lịch sử và không xóa khỏi cơ sở dữ liệu.");
            }

            // Thực hiện vô hiệu hóa: chuyển trạng thái sang Inactive, khóa quyền truy cập hệ thống
            entity.TrangThai = "Inactive";
            _service.Update(entity);
            await _service.SaveChangesAsync();

            return Ok(new { message = "Đã tạm khóa tài khoản nhân viên thành công (chuyển sang trạng thái Inactive, vô hiệu hóa quyền đăng nhập)." });
        }
    }
}

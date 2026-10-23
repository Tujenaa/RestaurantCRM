using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.DTOs;
using RestaurantCRM.API.Helpers;
using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.BusinessLogic.Services;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminVaiTroController : ControllerBase
    {
        private readonly IRestaurantService _service;

        public AdminVaiTroController(IRestaurantService service)
        {
            _service = service;
        }

        // GET: api/AdminVaiTro/Permissions - Danh mục tất cả quyền có thể cấp
        [HttpGet("Permissions")]
        public IActionResult GetAvailablePermissions()
        {
            var result = PermissionCatalog.AllPermissions.Select(p => new PermissionDefinitionDto
            {
                Code = p.Code,
                Name = p.Name,
                Group = p.Group,
                Description = p.Description
            }).ToList();

            return Ok(result);
        }

        // GET: api/AdminVaiTro - Danh sách vai trò kèm số lượng nhân viên và permissions
        [HttpGet]
        public async Task<IActionResult> GetVaiTros()
        {
            var vaiTros = await _service.Query<VaiTro>()
                .Include(v => v.NhanVien)
                .OrderBy(v => v.MaVaiTro)
                .ToListAsync();

            var result = vaiTros.Select(v => new VaiTroDto
            {
                MaVaiTro = v.MaVaiTro,
                TenVaiTro = v.TenVaiTro,
                MoTa = v.MoTa,
                SoLuongNhanVien = v.NhanVien.Count,
                Permissions = PermissionCatalog.ParsePermissions(v.MoTa, v.TenVaiTro)
            }).ToList();

            return Ok(result);
        }

        // GET: api/AdminVaiTro/{id} - Chi tiết vai trò
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVaiTro(string id)
        {
            var v = await _service.Query<VaiTro>()
                .Include(x => x.NhanVien)
                .FirstOrDefaultAsync(x => x.MaVaiTro == id);

            if (v == null) return NotFound("Không tìm thấy vai trò.");

            return Ok(new VaiTroDto
            {
                MaVaiTro = v.MaVaiTro,
                TenVaiTro = v.TenVaiTro,
                MoTa = v.MoTa,
                SoLuongNhanVien = v.NhanVien.Count,
                Permissions = PermissionCatalog.ParsePermissions(v.MoTa, v.TenVaiTro)
            });
        }

        // POST: api/AdminVaiTro - Tạo vai trò mới
        [HttpPost]
        public async Task<IActionResult> CreateVaiTro([FromBody] SaveVaiTroRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MaVaiTro) || string.IsNullOrWhiteSpace(request.TenVaiTro))
            {
                return BadRequest("Mã vai trò và tên vai trò không được để trống.");
            }

            var ma = request.MaVaiTro.Trim().ToUpperInvariant();
            var exists = await _service.Query<VaiTro>().AnyAsync(v => v.MaVaiTro == ma);
            if (exists)
            {
                return BadRequest($"Mã vai trò '{ma}' đã tồn tại trong hệ thống.");
            }

            var entity = new VaiTro
            {
                MaVaiTro = ma,
                TenVaiTro = request.TenVaiTro.Trim(),
                MoTa = PermissionCatalog.FormatPermissions(request.Permissions)
            };

            _service.Add(entity);
            await _service.SaveChangesAsync();
            return Ok(new { message = "Tạo vai trò thành công", maVaiTro = entity.MaVaiTro });
        }

        // PUT: api/AdminVaiTro/{id} - Cập nhật vai trò & danh sách quyền
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVaiTro(string id, [FromBody] SaveVaiTroRequest request)
        {
            var entity = await _service.Query<VaiTro>().FirstOrDefaultAsync(v => v.MaVaiTro == id);
            if (entity == null) return NotFound("Không tìm thấy vai trò cần cập nhật.");

            entity.TenVaiTro = request.TenVaiTro.Trim();
            entity.MoTa = PermissionCatalog.FormatPermissions(request.Permissions);

            _service.Update(entity);
            await _service.SaveChangesAsync();
            return Ok(new { message = "Cập nhật vai trò và phân quyền thành công" });
        }

        // DELETE: api/AdminVaiTro/{id} - Xóa vai trò (nếu không có nhân viên)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVaiTro(string id)
        {
            var entity = await _service.Query<VaiTro>()
                .Include(v => v.NhanVien)
                .FirstOrDefaultAsync(v => v.MaVaiTro == id);

            if (entity == null) return NotFound("Không tìm thấy vai trò.");

            if (entity.NhanVien.Count > 0)
            {
                return BadRequest($"Không thể xóa vai trò này vì đang có {entity.NhanVien.Count} nhân viên được gán.");
            }

            if (entity.MaVaiTro == "VT01" || entity.TenVaiTro?.ToLower() == "admin")
            {
                return BadRequest("Không thể xóa vai trò Quản trị viên (Admin) mặc định của hệ thống.");
            }

            _service.Remove(entity);
            await _service.SaveChangesAsync();
            return Ok(new { message = "Đã xóa vai trò thành công." });
        }
    }
}

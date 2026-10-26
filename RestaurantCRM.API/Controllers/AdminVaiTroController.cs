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
        public async Task<IActionResult> GetAvailablePermissions()
        {
            try
            {
                var dbPermissions = await _service.Query<Quyen>()
                    .OrderBy(q => q.NhomQuyen)
                    .ThenBy(q => q.MaQuyen)
                    .ToListAsync();

                if (dbPermissions != null && dbPermissions.Count > 0)
                {
                    return Ok(dbPermissions.Select(p => new PermissionDefinitionDto
                    {
                        Code = p.MaQuyen,
                        Name = p.TenQuyen,
                        Group = p.NhomQuyen,
                        Description = p.MoTa ?? ""
                    }).ToList());
                }
            }
            catch
            {
                // Fallback nếu bảng QUYEN chưa được khởi tạo
            }

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
                .Include(v => v.VaiTroQuyen)
                .OrderBy(v => v.MaVaiTro)
                .ToListAsync();

            var result = vaiTros.Select(v =>
            {
                List<string> perms;
                if (v.VaiTroQuyen != null && v.VaiTroQuyen.Count > 0)
                {
                    perms = v.VaiTroQuyen.Select(x => x.MaQuyen).Distinct().ToList();
                }
                else
                {
                    perms = PermissionCatalog.ParsePermissions(v.MoTa, v.TenVaiTro);
                }

                return new VaiTroDto
                {
                    MaVaiTro = v.MaVaiTro,
                    TenVaiTro = v.TenVaiTro,
                    MoTa = v.MoTa,
                    SoLuongNhanVien = v.NhanVien.Count,
                    Permissions = perms
                };
            }).ToList();

            return Ok(result);
        }

        // GET: api/AdminVaiTro/{id} - Chi tiết vai trò
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVaiTro(string id)
        {
            var v = await _service.Query<VaiTro>()
                .Include(x => x.NhanVien)
                .Include(x => x.VaiTroQuyen)
                .FirstOrDefaultAsync(x => x.MaVaiTro == id);

            if (v == null) return NotFound("Không tìm thấy vai trò.");

            List<string> perms;
            if (v.VaiTroQuyen != null && v.VaiTroQuyen.Count > 0)
            {
                perms = v.VaiTroQuyen.Select(x => x.MaQuyen).Distinct().ToList();
            }
            else
            {
                perms = PermissionCatalog.ParsePermissions(v.MoTa, v.TenVaiTro);
            }

            return Ok(new VaiTroDto
            {
                MaVaiTro = v.MaVaiTro,
                TenVaiTro = v.TenVaiTro,
                MoTa = v.MoTa,
                SoLuongNhanVien = v.NhanVien.Count,
                Permissions = perms
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

            // Thêm các quyền vào bảng VAI_TRO_QUYEN
            if (request.Permissions != null && request.Permissions.Count > 0)
            {
                foreach (var p in request.Permissions.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
                {
                    _service.Add(new VaiTroQuyen
                    {
                        MaVaiTro = ma,
                        MaQuyen = p.Trim().ToUpperInvariant(),
                        NgayTao = DateTime.Now
                    });
                }
            }

            await _service.SaveChangesAsync();
            return Ok(new { message = "Tạo vai trò thành công", maVaiTro = entity.MaVaiTro });
        }

        // PUT: api/AdminVaiTro/{id} - Cập nhật vai trò & danh sách quyền
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVaiTro(string id, [FromBody] SaveVaiTroRequest request)
        {
            var entity = await _service.Query<VaiTro>()
                .Include(v => v.VaiTroQuyen)
                .FirstOrDefaultAsync(v => v.MaVaiTro == id);

            if (entity == null) return NotFound("Không tìm thấy vai trò cần cập nhật.");

            entity.TenVaiTro = request.TenVaiTro.Trim();
            entity.MoTa = PermissionCatalog.FormatPermissions(request.Permissions);

            // Đồng bộ quan hệ trong bảng VAI_TRO_QUYEN
            var existingPerms = entity.VaiTroQuyen.ToList();
            var newPermCodes = new HashSet<string>(
                (request.Permissions ?? new List<string>())
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p.Trim().ToUpperInvariant()),
                StringComparer.OrdinalIgnoreCase
            );

            // 1. Xóa những quyền bị bỏ chọn
            foreach (var ep in existingPerms)
            {
                if (!newPermCodes.Contains(ep.MaQuyen))
                {
                    _service.Remove(ep);
                }
            }

            // 2. Thêm những quyền mới được chọn
            var existingCodes = new HashSet<string>(existingPerms.Select(p => p.MaQuyen), StringComparer.OrdinalIgnoreCase);
            foreach (var np in newPermCodes)
            {
                if (!existingCodes.Contains(np))
                {
                    _service.Add(new VaiTroQuyen
                    {
                        MaVaiTro = entity.MaVaiTro,
                        MaQuyen = np,
                        NgayTao = DateTime.Now
                    });
                }
            }

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
                .Include(v => v.VaiTroQuyen)
                .FirstOrDefaultAsync(v => v.MaVaiTro == id);

            if (entity == null) return NotFound("Không tìm thấy vai trò.");

            if (entity.NhanVien.Count > 0)
            {
                return BadRequest($"Không thể xóa vai trò này vì đang có {entity.NhanVien.Count} nhân viên được gán.");
            }

            if (entity.MaVaiTro == "VT01" || string.Equals(entity.TenVaiTro, "admin", StringComparison.OrdinalIgnoreCase) || string.Equals(entity.TenVaiTro, "Hệ thống", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Không thể xóa vai trò Quản trị viên (Hệ thống) mặc định của hệ thống.");
            }

            _service.Remove(entity);
            await _service.SaveChangesAsync();
            return Ok(new { message = "Đã xóa vai trò thành công." });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.DTOs;
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
            // Danh mục quyền chỉ lấy từ bảng QUYEN
            var dbPermissions = await _service.Query<Quyen>()
                .OrderBy(q => q.NhomQuyen)
                .ThenBy(q => q.MaQuyen)
                .ToListAsync();

            return Ok(dbPermissions.Select(p => new PermissionDefinitionDto
            {
                Code = p.MaQuyen,
                Name = p.TenQuyen,
                Group = p.NhomQuyen,
                Description = p.MoTa ?? ""
            }).ToList());
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

            var result = vaiTros.Select(v => new VaiTroDto
            {
                MaVaiTro = v.MaVaiTro,
                TenVaiTro = v.TenVaiTro,
                MoTa = v.MoTa,
                SoLuongNhanVien = v.NhanVien.Count,
                // Quyền chỉ lấy từ VAI_TRO_QUYEN; không có dòng nào thì không có quyền nào
                Permissions = v.VaiTroQuyen?.Select(q => q.MaQuyen).Distinct().ToList() ?? new List<string>()
            }).ToList();

            return Ok(result);
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
                MoTa = string.IsNullOrWhiteSpace(request.MoTa) ? null : request.MoTa.Trim()
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
            // Cột mô tả giữ đúng nghĩa: chỉ đổi khi client gửi mô tả mới
            if (request.MoTa != null)
            {
                entity.MoTa = string.IsNullOrWhiteSpace(request.MoTa) ? null : request.MoTa.Trim();
            }

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
            return Ok(new { message = "Đã xóa vĩnh viễn vai trò khỏi hệ thống." });
        }
    }
}

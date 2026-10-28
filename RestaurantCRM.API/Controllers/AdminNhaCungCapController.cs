using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminNhaCungCapController : ControllerBase
    {
        private readonly RestaurantCRM.BusinessLogic.Services.IRestaurantService _service;

        public AdminNhaCungCapController(RestaurantCRM.BusinessLogic.Services.IRestaurantService service)
        {
            _service = service;
        }

        // GET: api/AdminNhaCungCap - Lấy danh sách nhà cung cấp
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? trangThai = null)
        {
            var query = _service.Query<NhaCungCap>();
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                if (string.Equals(trangThai, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(n => n.TrangThai == null || n.TrangThai == "" || n.TrangThai == "Active");
                }
                else
                {
                    query = query.Where(n => n.TrangThai == trangThai);
                }
            }
            var nhaCungCaps = await query.ToListAsync();
            return Ok(nhaCungCaps);
        }

        // GET: api/AdminNhaCungCap/{id} - Lấy chi tiết nhà cung cấp
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var nhaCungCap = await _service.FindAsync<NhaCungCap>(id);
            if (nhaCungCap == null)
            {
                return NotFound("Không tìm thấy nhà cung cấp.");
            }
            return Ok(nhaCungCap);
        }

        // POST: api/AdminNhaCungCap - Thêm nhà cung cấp mới
        [HttpPost]
        public async Task<IActionResult> Create(NhaCungCap nhaCungCap)
        {
            if (string.IsNullOrWhiteSpace(nhaCungCap.MaNhaCungCap))
            {
                nhaCungCap.MaNhaCungCap = await RestaurantCRM.BusinessLogic.Services.CodeGenerator.GenerateMaNhaCungCapAsync(_service);
            }
            if (string.IsNullOrWhiteSpace(nhaCungCap.TrangThai))
            {
                nhaCungCap.TrangThai = "Active";
            }
            _service.Add(nhaCungCap);
            await _service.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = nhaCungCap.MaNhaCungCap }, nhaCungCap);
        }

        // PUT: api/AdminNhaCungCap/{id} - Cập nhật nhà cung cấp
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, NhaCungCap nhaCungCap)
        {
            if (id != nhaCungCap.MaNhaCungCap)
            {
                return BadRequest("ID không khớp.");
            }

            var existing = await _service.FindAsync<NhaCungCap>(id);
            if (existing == null)
            {
                return NotFound("Không tìm thấy nhà cung cấp.");
            }

            existing.TenNhaCungCap = nhaCungCap.TenNhaCungCap;
            existing.SoDienThoai = nhaCungCap.SoDienThoai;
            existing.DiaChi = nhaCungCap.DiaChi;
            existing.Email = nhaCungCap.Email;
            if (!string.IsNullOrWhiteSpace(nhaCungCap.TrangThai))
            {
                existing.TrangThai = nhaCungCap.TrangThai;
            }

            try
            {
                await _service.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NhaCungCapExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/AdminNhaCungCap/{id} - Xóa nhà cung cấp
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var nhaCungCap = await _service.FindAsync<NhaCungCap>(id);
            if (nhaCungCap == null)
            {
                return NotFound("Không tìm thấy nhà cung cấp.");
            }

            var hasPhieuNhap = await _service.Query<PhieuNhapHang>().AnyAsync(p => p.MaNhaCungCap == id);
            if (hasPhieuNhap)
            {
                nhaCungCap.TrangThai = "Hidden";
                _service.Update(nhaCungCap);
                await _service.SaveChangesAsync();

                return Ok(new { message = "NCC đã có phiếu nhập nên sẽ ẩn NCC", isHidden = true });
            }

            _service.Remove(nhaCungCap);
            await _service.SaveChangesAsync();

            return Ok(new { message = "Xóa thành công", isHidden = false });
        }

        private bool NhaCungCapExists(string id)
        {
            return _service.Query<NhaCungCap>().Any(e => e.MaNhaCungCap == id);
        }
    }
}

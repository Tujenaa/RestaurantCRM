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
        public async Task<IActionResult> GetAll()
        {
            var nhaCungCaps = await _service.Query<NhaCungCap>().ToListAsync();
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

            _service.Update(nhaCungCap);

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
                return NotFound();
            }

            var hasPhieuNhap = await _service.Query<PhieuNhapHang>().AnyAsync(p => p.MaNhaCungCap == id);
            if (hasPhieuNhap)
            {
                return BadRequest("Không thể xóa nhà cung cấp này vì đã có lịch sử nhập hàng.");
            }

            _service.Remove(nhaCungCap);
            await _service.SaveChangesAsync();

            return Ok(new { message = "Xóa thành công" });
        }

        private bool NhaCungCapExists(string id)
        {
            return _service.Query<NhaCungCap>().Any(e => e.MaNhaCungCap == id);
        }
    }
}

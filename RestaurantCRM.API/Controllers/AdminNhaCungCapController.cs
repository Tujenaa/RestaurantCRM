using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminNhaCungCapController : ControllerBase
    {
        private readonly RestaurantCrmContext _context;

        public AdminNhaCungCapController(RestaurantCrmContext context)
        {
            _context = context;
        }

        // GET: api/AdminNhaCungCap - Lấy danh sách nhà cung cấp
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var nhaCungCaps = await _context.NhaCungCap.ToListAsync();
            return Ok(nhaCungCaps);
        }

        // GET: api/AdminNhaCungCap/{id} - Lấy chi tiết nhà cung cấp
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var nhaCungCap = await _context.NhaCungCap.FindAsync(id);
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
                nhaCungCap.MaNhaCungCap = await Helpers.CodeGenerator.GenerateMaNhaCungCapAsync(_context);
            }
            _context.NhaCungCap.Add(nhaCungCap);
            await _context.SaveChangesAsync();

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

            _context.Entry(nhaCungCap).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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
            var nhaCungCap = await _context.NhaCungCap.FindAsync(id);
            if (nhaCungCap == null)
            {
                return NotFound();
            }

            var hasPhieuNhap = await _context.PhieuNhapHang.AnyAsync(p => p.MaNhaCungCap == id);
            if (hasPhieuNhap)
            {
                return BadRequest("Không thể xóa nhà cung cấp này vì đã có lịch sử nhập hàng.");
            }

            _context.NhaCungCap.Remove(nhaCungCap);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa thành công" });
        }

        private bool NhaCungCapExists(string id)
        {
            return _context.NhaCungCap.Any(e => e.MaNhaCungCap == id);
        }
    }
}

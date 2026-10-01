using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MonAnController : ControllerBase
    {
        private readonly RestaurantCRMContext _context;

        public MonAnController(RestaurantCRMContext context)
        {
            _context = context;
        }

        // GET: api/MonAn - Lấy danh sách tất cả món ăn (chỉ hiển thị trạng thái Active).
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MonAn>>> GetMonAns()
        {
            return await _context.MonAns
                .Where(m => m.TrangThai == "Active")
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/MonAn/TheoLoai/{maLoai} - Lấy danh sách món ăn theo loại (chỉ hiển thị trạng thái Active).
        [HttpGet("TheoLoai/{maLoai}")]
        public async Task<ActionResult<IEnumerable<MonAn>>> GetMonAnsByLoai(string maLoai)
        {
            return await _context.MonAns
                .Where(m => m.MaLoaiMon == maLoai && m.TrangThai == "Active")
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/MonAn/{id} - Lấy thông tin chi tiết một món ăn.
        [HttpGet("{id}")]
        public async Task<ActionResult<MonAn>> GetMonAn(string id)
        {
            var monAn = await _context.MonAns.FindAsync(id);

            if (monAn == null)
            {
                return NotFound();
            }

            return monAn;
        }

        // POST: api/MonAn - Thêm một món ăn mới.
        [HttpPost]
        public async Task<ActionResult<MonAn>> PostMonAn(MonAn monAn)
        {
            _context.MonAns.Add(monAn);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (MonAnExists(monAn.MaMon))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction(nameof(GetMonAn), new { id = monAn.MaMon }, monAn);
        }

        // PUT: api/MonAn/{id} - Cập nhật thông tin món ăn.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMonAn(string id, MonAn monAn)
        {
            if (id != monAn.MaMon)
            {
                return BadRequest();
            }

            _context.Entry(monAn).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MonAnExists(id))
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

        // DELETE: api/MonAn/{id} - Xóa một món ăn.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMonAn(string id)
        {
            var monAn = await _context.MonAns.FindAsync(id);
            if (monAn == null)
            {
                return NotFound();
            }

            _context.MonAns.Remove(monAn);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MonAnExists(string id)
        {
            return _context.MonAns.Any(e => e.MaMon == id);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminLoaiMonController : ControllerBase
    {
        private readonly RestaurantCrmContext _context;

        public AdminLoaiMonController(RestaurantCrmContext context)
        {
            _context = context;
        }

        // GET: api/AdminLoaiMon - Lấy danh sách toàn bộ loại món.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoaiMon>>> GetLoaiMons()
        {
            return await _context.LoaiMon.ToListAsync();
        }

        // GET: api/AdminLoaiMon/{id} - Lấy thông tin chi tiết một loại món.
        [HttpGet("{id}")]
        public async Task<ActionResult<LoaiMon>> GetLoaiMon(string id)
        {
            var loaiMon = await _context.LoaiMon.FindAsync(id);

            if (loaiMon == null)
            {
                return NotFound("Không tìm thấy loại món.");
            }

            return loaiMon;
        }

        // POST: api/AdminLoaiMon - Thêm một loại món mới.
        [HttpPost]
        public async Task<ActionResult<LoaiMon>> PostLoaiMon(LoaiMon loaiMon)
        {
            loaiMon.MaLoaiMon = "LM" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            
            _context.LoaiMon.Add(loaiMon);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLoaiMon), new { id = loaiMon.MaLoaiMon }, loaiMon);
        }

        // PUT: api/AdminLoaiMon/{id} - Cập nhật thông tin loại món.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLoaiMon(string id, LoaiMon loaiMon)
        {
            if (id != loaiMon.MaLoaiMon)
            {
                return BadRequest("Mã loại món không khớp.");
            }

            _context.Entry(loaiMon).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoaiMonExists(id))
                {
                    return NotFound("Không tìm thấy loại món để cập nhật.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/AdminLoaiMon/{id} - Xóa cứng một loại món (Kiểm tra ràng buộc món ăn).
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoaiMon(string id)
        {
            var loaiMon = await _context.LoaiMon
                .Include(lm => lm.MonAn)
                .FirstOrDefaultAsync(lm => lm.MaLoaiMon == id);
            
            if (loaiMon == null)
            {
                return NotFound("Không tìm thấy loại món để xóa.");
            }

            if (loaiMon.MonAn.Any())
            {
                return BadRequest("Không thể xóa loại món này vì vẫn còn các món ăn thuộc loại này. Vui lòng chuyển các món ăn sang loại khác trước khi xóa.");
            }

            _context.LoaiMon.Remove(loaiMon);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LoaiMonExists(string id)
        {
            return _context.LoaiMon.Any(e => e.MaLoaiMon == id);
        }
    }
}

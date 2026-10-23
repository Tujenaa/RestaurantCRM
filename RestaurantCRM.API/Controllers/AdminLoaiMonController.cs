using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminLoaiMonController : ControllerBase
    {
        private readonly RestaurantCRM.BusinessLogic.Services.IRestaurantService _service;

        public AdminLoaiMonController(RestaurantCRM.BusinessLogic.Services.IRestaurantService service)
        {
            _service = service;
        }

        // GET: api/AdminLoaiMon - Lấy danh sách toàn bộ loại món.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoaiMon>>> GetLoaiMons()
        {
            return await _service.Query<LoaiMon>().ToListAsync();
        }

        // GET: api/AdminLoaiMon/{id} - Lấy thông tin chi tiết một loại món.
        [HttpGet("{id}")]
        public async Task<ActionResult<LoaiMon>> GetLoaiMon(string id)
        {
            var loaiMon = await _service.FindAsync<LoaiMon>(id);

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
            if (string.IsNullOrWhiteSpace(loaiMon.MaLoaiMon))
            {
                loaiMon.MaLoaiMon = await RestaurantCRM.BusinessLogic.Services.CodeGenerator.GenerateMaLoaiMonAsync(_service);
            }
            
            _service.Add(loaiMon);
            await _service.SaveChangesAsync();

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

            _service.Update(loaiMon);

            try
            {
                await _service.SaveChangesAsync();
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
            var loaiMon = await _service.Query<LoaiMon>()
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

            _service.Remove(loaiMon);
            await _service.SaveChangesAsync();

            return NoContent();
        }

        private bool LoaiMonExists(string id)
        {
            return _service.Query<LoaiMon>().Any(e => e.MaLoaiMon == id);
        }
    }
}

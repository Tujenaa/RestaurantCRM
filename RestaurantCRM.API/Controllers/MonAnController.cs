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
        private readonly RestaurantCrmContext _context;

        public MonAnController(RestaurantCrmContext context)
        {
            _context = context;
        }

        // GET: api/MonAn - Lấy danh sách món ăn (hỗ trợ tìm kiếm theo tên và lọc theo loại món)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MonAn>>> GetMonAns([FromQuery] string? search = null, [FromQuery] string? maLoai = null)
        {
            var query = _context.MonAn
                .Where(m => m.TrangThai == "Active" || m.TrangThai == "InStock" || (m.TrangThai != "Discontinued" && m.TrangThai != "Inactive"))
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(maLoai) && maLoai != "Tất cả")
            {
                query = query.Where(m => m.MaLoaiMon == maLoai);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.TenMon != null && m.TenMon.Contains(search));
            }

            return await query.ToListAsync();
        }

        // GET: api/MonAn/TheoLoai/{maLoai} - Lấy danh sách món ăn theo loại
        [HttpGet("TheoLoai/{maLoai}")]
        public async Task<ActionResult<IEnumerable<MonAn>>> GetMonAnsByLoai(string maLoai)
        {
            return await _context.MonAn
                .Where(m => m.MaLoaiMon == maLoai && (m.TrangThai == "Active" || m.TrangThai == "InStock" || (m.TrangThai != "Discontinued" && m.TrangThai != "Inactive")))
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/MonAn/{id} - Lấy thông tin chi tiết một món ăn.
        [HttpGet("{id}")]
        public async Task<ActionResult<MonAn>> GetMonAn(string id)
        {
            var monAn = await _context.MonAn.FindAsync(id);

            if (monAn == null)
            {
                return NotFound();
            }

            return monAn;
        }



        private bool MonAnExists(string id)
        {
            return _context.MonAn.Any(e => e.MaMon == id);
        }
    }
}

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

        // GET: api/MonAn - Lấy danh sách tất cả món ăn (chỉ hiển thị trạng thái Active).
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MonAn>>> GetMonAns()
        {
            return await _context.MonAn
                .Where(m => m.TrangThai == "Active")
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/MonAn/TheoLoai/{maLoai} - Lấy danh sách món ăn theo loại (chỉ hiển thị trạng thái Active).
        [HttpGet("TheoLoai/{maLoai}")]
        public async Task<ActionResult<IEnumerable<MonAn>>> GetMonAnsByLoai(string maLoai)
        {
            return await _context.MonAn
                .Where(m => m.MaLoaiMon == maLoai && m.TrangThai == "Active")
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

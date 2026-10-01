using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoaiMonController : ControllerBase
    {
        private readonly RestaurantCRMContext _context;

        public LoaiMonController(RestaurantCRMContext context)
        {
            _context = context;
        }

        // GET: api/LoaiMon - Lấy danh sách tất cả loại món ăn
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoaiMon>>> GetLoaiMons()
        {
            return await _context.LoaiMons
                .AsNoTracking()
                .ToListAsync();
        }
    }
}

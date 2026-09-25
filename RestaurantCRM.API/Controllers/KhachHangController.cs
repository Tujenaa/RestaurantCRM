using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhachHangController : ControllerBase
    {
        private readonly RestaurantCRMContext _context;

        public KhachHangController(RestaurantCRMContext context)
        {
            _context = context;
        }

        // GET: api/KhachHang
        [HttpGet]
        public async Task<IActionResult> GetKhachHangs()
        {
            var khachHangs = await _context.KhachHangs
                .AsNoTracking()
                .ToListAsync();

            return Ok(khachHangs);
        }
    }
}
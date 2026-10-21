using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoaiMonController : ControllerBase
    {
        private readonly RestaurantCRM.BusinessLogic.Services.IRestaurantService _service;

        public LoaiMonController(RestaurantCRM.BusinessLogic.Services.IRestaurantService service)
        {
            _service = service;
        }

        // GET: api/LoaiMon - Lấy danh sách tất cả loại món ăn
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoaiMon>>> GetLoaiMons()
        {
            return await _service.Query<LoaiMon>()
                .AsNoTracking()
                .ToListAsync();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminKhoController : ControllerBase
    {
        private readonly RestaurantCRM.BusinessLogic.Services.IRestaurantService _service;

        public AdminKhoController(RestaurantCRM.BusinessLogic.Services.IRestaurantService service)
        {
            _service = service;
        }

        // GET: api/AdminKho - Danh sách tồn kho nguyên vật liệu
        [HttpGet]
        public async Task<IActionResult> GetTonKho()
        {
            var ds = await _service.Query<NguyenLieu>()
                .OrderBy(nl => nl.SoLuongTon)
                .Select(nl => new
                {
                    MaNguyenLieu = nl.MaNguyenLieu,
                    TenNguyenLieu = nl.TenNguyenLieu,
                    DonViTinh = nl.DonViTinh,
                    SoLuongTon = nl.SoLuongTon ?? 0,
                    GiaNhapGanNhat = nl.GiaNhapGanNhat ?? 0,
                    TrangThai = nl.TrangThai ?? "Active",
                    CanhBao = (nl.SoLuongTon ?? 0) <= 0 ? "HetHang" : (nl.SoLuongTon ?? 0) <= 10 ? "SapHet" : "DuHang",
                    // Tương thích ngược với client cũ
                    MaMon = nl.MaNguyenLieu,
                    TenMon = nl.TenNguyenLieu
                })
                .ToListAsync();

            return Ok(ds);
        }
    }
}

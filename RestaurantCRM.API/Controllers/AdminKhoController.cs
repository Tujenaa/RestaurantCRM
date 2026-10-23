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

        // GET: api/AdminKho/canh-bao - Lấy các nguyên vật liệu sắp hết hoặc đã hết (<= 10)
        [HttpGet("canh-bao")]
        public async Task<IActionResult> GetCanhBao([FromQuery] decimal nguong = 10)
        {
            var ds = await _service.Query<NguyenLieu>()
                .Where(nl => (nl.SoLuongTon ?? 0) <= nguong)
                .OrderBy(nl => nl.SoLuongTon)
                .Select(nl => new
                {
                    MaNguyenLieu = nl.MaNguyenLieu,
                    TenNguyenLieu = nl.TenNguyenLieu,
                    DonViTinh = nl.DonViTinh,
                    SoLuongTon = nl.SoLuongTon ?? 0,
                    GiaNhapGanNhat = nl.GiaNhapGanNhat ?? 0,
                    TrangThai = nl.TrangThai ?? "Active",
                    CanhBao = (nl.SoLuongTon ?? 0) <= 0 ? "HetHang" : "SapHet",
                    // Tương thích ngược
                    MaMon = nl.MaNguyenLieu,
                    TenMon = nl.TenNguyenLieu
                })
                .ToListAsync();

            return Ok(ds);
        }

        // PUT: api/AdminKho/dieu-chinh/{id} - Điều chỉnh / kiểm kê số lượng tồn kho nguyên liệu
        [HttpPut("dieu-chinh/{id}")]
        public async Task<IActionResult> DieuChinhTonKho(string id, [FromBody] DieuChinhTonKhoRequest request)
        {
            var nl = await _service.FindAsync<NguyenLieu>(id);
            if (nl == null)
            {
                return NotFound("Không tìm thấy nguyên vật liệu.");
            }

            nl.SoLuongTon = request.SoLuongMoi;
            await _service.SaveChangesAsync();
            return Ok(new { message = "Điều chỉnh tồn kho thành công", soLuongMoi = nl.SoLuongTon });
        }
    }

    public class DieuChinhTonKhoRequest
    {
        public decimal SoLuongMoi { get; set; }
        public string? GhiChu { get; set; }
    }
}

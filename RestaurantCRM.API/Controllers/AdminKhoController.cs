using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminKhoController : ControllerBase
    {
        private readonly RestaurantCrmContext _context;

        public AdminKhoController(RestaurantCrmContext context)
        {
            _context = context;
        }

        // GET: api/AdminKho - Danh sách tồn kho của tất cả món ăn
        [HttpGet]
        public async Task<IActionResult> GetTonKho()
        {
            var ds = await _context.MonAn
                .Include(m => m.MaLoaiMonNavigation)
                .OrderBy(m => m.SoLuong)
                .Select(m => new
                {
                    m.MaMon,
                    m.TenMon,
                    MaLoaiMon = m.MaLoaiMon,
                    TenLoaiMon = m.MaLoaiMonNavigation != null ? m.MaLoaiMonNavigation.TenLoaiMon : null,
                    m.DonGia,
                    SoLuongTon = m.SoLuong ?? 0,
                    TrangThai = m.TrangThai ?? "InStock",
                    CanhBao = (m.SoLuong ?? 0) <= 0 ? "HetHang" : (m.SoLuong ?? 0) <= 10 ? "SapHet" : "DuHang"
                })
                .ToListAsync();

            return Ok(ds);
        }

        // GET: api/AdminKho/canh-bao - Lấy các mặt hàng sắp hết hoặc đã hết (<= 10)
        [HttpGet("canh-bao")]
        public async Task<IActionResult> GetCanhBao([FromQuery] int nguong = 10)
        {
            var ds = await _context.MonAn
                .Include(m => m.MaLoaiMonNavigation)
                .Where(m => (m.SoLuong ?? 0) <= nguong)
                .OrderBy(m => m.SoLuong)
                .Select(m => new
                {
                    m.MaMon,
                    m.TenMon,
                    TenLoaiMon = m.MaLoaiMonNavigation != null ? m.MaLoaiMonNavigation.TenLoaiMon : null,
                    m.DonGia,
                    SoLuongTon = m.SoLuong ?? 0,
                    TrangThai = m.TrangThai ?? "InStock",
                    CanhBao = (m.SoLuong ?? 0) <= 0 ? "HetHang" : "SapHet"
                })
                .ToListAsync();

            return Ok(ds);
        }

        // PUT: api/AdminKho/dieu-chinh/{id} - Điều chỉnh / kiểm kê số lượng tồn kho
        [HttpPut("dieu-chinh/{id}")]
        public async Task<IActionResult> DieuChinhTonKho(string id, [FromBody] DieuChinhTonKhoRequest request)
        {
            var monAn = await _context.MonAn.FindAsync(id);
            if (monAn == null)
            {
                return NotFound("Không tìm thấy mặt hàng.");
            }

            monAn.SoLuong = request.SoLuongMoi;
            if (monAn.TrangThai != "Discontinued")
            {
                if (monAn.SoLuong <= 0)
                {
                    monAn.TrangThai = "OutOfStock";
                }
                else if (monAn.TrangThai == "OutOfStock")
                {
                    monAn.TrangThai = "InStock";
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Điều chỉnh tồn kho thành công", soLuongMoi = monAn.SoLuong });
        }
    }

    public class DieuChinhTonKhoRequest
    {
        public int SoLuongMoi { get; set; }
        public string? GhiChu { get; set; }
    }
}

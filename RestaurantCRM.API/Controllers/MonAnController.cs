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
                .Include(m => m.KmTheoSp)
                    .ThenInclude(k => k.MaChuongTrinhNavigation)
                .Where(m => m.TrangThai == "Active" || m.TrangThai == "InStock" || (m.TrangThai != "Discontinued" && m.TrangThai != "Inactive" && m.TrangThai != "OutOfStock"))
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(maLoai) && maLoai != "Tất cả")
            {
                query = query.Where(m => m.MaLoaiMon == maLoai);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.TenMon != null && m.TenMon.Contains(search));
            }

            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);
            var list = await query.ToListAsync();
            return Ok(list.Select(m => {
                var activeKm = m.KmTheoSp.FirstOrDefault(k => k.MaChuongTrinhNavigation != null && k.MaChuongTrinhNavigation.NgayBatDau <= today && k.MaChuongTrinhNavigation.NgayKetThuc >= today);
                double donGia = m.DonGia ?? 0;
                double giaSauGiam = donGia;
                if (activeKm != null) {
                    giaSauGiam = donGia - (activeKm.TienGiam ?? 0) - (donGia * (activeKm.PhanTramGiam ?? 0) / 100.0);
                }
                return new {
                    m.MaMon, m.MaLoaiMon, m.TenMon, m.MoTa, m.DuongDanAnh, m.DonGia, m.SoLuong, m.TrangThai,
                    DonGiaSauGiam = Math.Max(0, giaSauGiam)
                };
            }));
        }

        // GET: api/MonAn/TheoLoai/{maLoai} - Lấy danh sách món ăn theo loại
        [HttpGet("TheoLoai/{maLoai}")]
        public async Task<ActionResult<IEnumerable<MonAn>>> GetMonAnsByLoai(string maLoai)
        {
            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);
            var list = await _context.MonAn
                .Include(m => m.KmTheoSp)
                    .ThenInclude(k => k.MaChuongTrinhNavigation)
                .Where(m => m.MaLoaiMon == maLoai && (m.TrangThai == "Active" || m.TrangThai == "InStock" || (m.TrangThai != "Discontinued" && m.TrangThai != "Inactive" && m.TrangThai != "OutOfStock")))
                .AsNoTracking()
                .ToListAsync();

            return Ok(list.Select(m => {
                var activeKm = m.KmTheoSp.FirstOrDefault(k => k.MaChuongTrinhNavigation != null && k.MaChuongTrinhNavigation.NgayBatDau <= today && k.MaChuongTrinhNavigation.NgayKetThuc >= today);
                double donGia = m.DonGia ?? 0;
                double giaSauGiam = donGia;
                if (activeKm != null) {
                    giaSauGiam = donGia - (activeKm.TienGiam ?? 0) - (donGia * (activeKm.PhanTramGiam ?? 0) / 100.0);
                }
                return new {
                    m.MaMon, m.MaLoaiMon, m.TenMon, m.MoTa, m.DuongDanAnh, m.DonGia, m.SoLuong, m.TrangThai,
                    DonGiaSauGiam = Math.Max(0, giaSauGiam)
                };
            }));
        }

        // GET: api/MonAn/{id} - Lấy thông tin chi tiết một món ăn.
        [HttpGet("{id}")]
        public async Task<ActionResult<MonAn>> GetMonAn(string id)
        {
            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);
            var monAn = await _context.MonAn
                .Include(m => m.KmTheoSp)
                    .ThenInclude(k => k.MaChuongTrinhNavigation)
                .FirstOrDefaultAsync(m => m.MaMon == id);

            if (monAn == null)
            {
                return NotFound();
            }

            var activeKm = monAn.KmTheoSp.FirstOrDefault(k => k.MaChuongTrinhNavigation != null && k.MaChuongTrinhNavigation.NgayBatDau <= today && k.MaChuongTrinhNavigation.NgayKetThuc >= today);
            double donGia = monAn.DonGia ?? 0;
            double giaSauGiam = donGia;
            if (activeKm != null) {
                giaSauGiam = donGia - (activeKm.TienGiam ?? 0) - (donGia * (activeKm.PhanTramGiam ?? 0) / 100.0);
            }

            return Ok(new {
                monAn.MaMon, monAn.MaLoaiMon, monAn.TenMon, monAn.MoTa, monAn.DuongDanAnh, monAn.DonGia, monAn.SoLuong, monAn.TrangThai,
                DonGiaSauGiam = Math.Max(0, giaSauGiam)
            });
        }



        private bool MonAnExists(string id)
        {
            return _context.MonAn.Any(e => e.MaMon == id);
        }
    }
}

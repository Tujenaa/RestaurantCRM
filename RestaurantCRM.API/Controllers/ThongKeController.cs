using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThongKeController : ControllerBase
    {
        private readonly RestaurantCRMContext _context;

        public ThongKeController(RestaurantCRMContext context)
        {
            _context = context;
        }

        // GET: api/ThongKe/TongQuan - Lấy thống kê tổng quan trong ngày (Đơn, Doanh thu, Khách hàng)
        [HttpGet("TongQuan")]
        public async Task<IActionResult> GetTongQuan()
        {
            var startOfDay = DateTime.Today;
            var endOfDay = startOfDay.AddDays(1);

            var donHangsHomNay = await _context.DonHangs
                .Where(d => d.NgayDat >= startOfDay && d.NgayDat < endOfDay)
                .ToListAsync();

            var tongDon = donHangsHomNay.Count;
            
            var doanhThuHomNay = donHangsHomNay
                .Where(d => d.TrangThai == "Completed" || d.TrangThai == "Hoàn thành")
                .Sum(d => d.TongThanhToan ?? 0);

            var tongKhachHang = await _context.KhachHangs.CountAsync();

            return Ok(new
            {
                TongDonHomNay = tongDon,
                DoanhThuHomNay = doanhThuHomNay,
                TongKhachHang = tongKhachHang
            });
        }

        // GET: api/ThongKe/DoanhThuTheoThang - Lấy dữ liệu vẽ biểu đồ doanh thu theo năm
        [HttpGet("DoanhThuTheoThang")]
        public async Task<IActionResult> GetDoanhThuTheoThang([FromQuery] int nam)
        {
            if (nam <= 0) nam = DateTime.Now.Year;

            var startDate = new DateTime(nam, 1, 1);
            var endDate = new DateTime(nam + 1, 1, 1);

            var donHangsTrongNam = await _context.DonHangs
                .Where(d => d.NgayDat >= startDate && d.NgayDat < endDate 
                            && (d.TrangThai == "Completed" || d.TrangThai == "Hoàn thành"))
                .Select(d => new { d.NgayDat, d.TongThanhToan })
                .ToListAsync();

            var doanhThuTheoThang = Enumerable.Range(1, 12).Select(thang => new
            {
                Thang = thang,
                DoanhThu = donHangsTrongNam
                    .Where(d => d.NgayDat.HasValue && d.NgayDat.Value.Month == thang)
                    .Sum(d => d.TongThanhToan ?? 0)
            }).ToList();

            return Ok(doanhThuTheoThang);
        }

        // GET: api/ThongKe/MonBanChay - Lấy danh sách các món ăn bán chạy nhất
        [HttpGet("MonBanChay")]
        public async Task<IActionResult> GetMonBanChay([FromQuery] int top = 5)
        {
            var query = await _context.ChiTietDonHangs
                .Where(c => c.MaDonHangNavigation != null 
                            && (c.MaDonHangNavigation.TrangThai == "Completed" || c.MaDonHangNavigation.TrangThai == "Hoàn thành"))
                .Select(c => new { c.MaMon, TenMon = c.MaMonNavigation.TenMon, c.SoLuong })
                .ToListAsync();

            var monBanChay = query
                .GroupBy(c => new { c.MaMon, c.TenMon })
                .Select(g => new
                {
                    MaMon = g.Key.MaMon,
                    TenMon = g.Key.TenMon,
                    TongSoLuongBan = g.Sum(c => c.SoLuong ?? 0)
                })
                .OrderByDescending(x => x.TongSoLuongBan)
                .Take(top)
                .ToList();

            return Ok(monBanChay);
        }
    }
}

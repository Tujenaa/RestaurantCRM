using RestaurantCRM.BusinessLogic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThongKeController : ControllerBase
    {
        private readonly RestaurantCRM.BusinessLogic.Services.IRestaurantService _service;

        public ThongKeController(RestaurantCRM.BusinessLogic.Services.IRestaurantService service)
        {
            _service = service;
        }

        // GET: api/ThongKe/TongQuan - Lấy thống kê tổng quan trong ngày (Đơn, Doanh thu, Khách hàng)
        [HttpGet("TongQuan")]
        public async Task<IActionResult> GetTongQuan()
        {
            var startOfDay = DateTime.Today;
            var endOfDay = startOfDay.AddDays(1);

            var hoaDonsHomNay = await _service.Query<HoaDon>()
                .Where(d => d.NgayDat >= startOfDay && d.NgayDat < endOfDay)
                .ToListAsync();

            var tongDon = hoaDonsHomNay.Count;
            
            var doanhThuHomNay = hoaDonsHomNay
                .Where(d => d.TrangThai == "Completed" || d.TrangThai == "Hoàn thành")
                .Sum(d => d.TongThanhToan ?? 0);

            var tongKhachHang = await _service.Query<KhachHang>().CountAsync();

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

            var hoaDonsTrongNam = await _service.Query<HoaDon>()
                .Where(d => d.NgayDat >= startDate && d.NgayDat < endDate 
                            && (d.TrangThai == "Completed" || d.TrangThai == "Hoàn thành"))
                .Select(d => new { d.NgayDat, d.TongThanhToan })
                .ToListAsync();

            var doanhThuTheoThang = Enumerable.Range(1, 12).Select(thang => new
            {
                Thang = thang,
                DoanhThu = hoaDonsTrongNam
                    .Where(d => d.NgayDat.HasValue && d.NgayDat.Value.Month == thang)
                    .Sum(d => d.TongThanhToan ?? 0)
            }).ToList();

            return Ok(doanhThuTheoThang);
        }

        // GET: api/ThongKe/MonBanChay - Lấy danh sách các món ăn bán chạy nhất
        [HttpGet("MonBanChay")]
        public async Task<IActionResult> GetMonBanChay([FromQuery] int top = 5)
        {
            var query = await _service.Query<ChiTietHoaDon>()
                .Where(c => c.MaHoaDonNavigation != null 
                            && (c.MaHoaDonNavigation.TrangThai == "Completed" || c.MaHoaDonNavigation.TrangThai == "Hoàn thành"))
                .Select(c => new { c.MaMon, TenMon = c.MaMonNavigation != null ? c.MaMonNavigation.TenMon : "Không xác định", c.SoLuong })
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

        // GET: api/ThongKe/DashboardMetrics - Lấy tổng hợp số liệu theo thời gian thực cho Dashboard
        [HttpGet("DashboardMetrics")]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            var now = DateTime.Now;
            var startOfDay = DateTime.Today;
            var endOfDay = startOfDay.AddDays(1);
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            // 1. Phân hệ Admin
            var soMonAn = await _service.Query<MonAn>().CountAsync();
            var soNhaCungCap = await _service.Query<NhaCungCap>().CountAsync();
            var soPhieuNhapThangNay = await _service.Query<PhieuNhapHang>()
                .CountAsync(p => p.NgayNhap >= startOfMonth);
            var soTaiKhoan = await _service.Query<NhanVien>().CountAsync();

            // 2. Phân hệ Bán hàng
            var hoaDonsHomNay = await _service.Query<HoaDon>()
                .Where(d => d.NgayDat >= startOfDay && d.NgayDat < endOfDay)
                .ToListAsync();

            var doanhThuHomNay = hoaDonsHomNay
                .Where(d => d.TrangThai == "Completed" || d.TrangThai == "Hoàn thành")
                .Sum(d => d.TongThanhToan ?? 0);

            var donChoXuLy = await _service.Query<HoaDon>()
                .CountAsync(d => d.TrangThai == "Pending" || d.TrangThai == "Chờ xử lý");

            var donDangGiao = await _service.Query<HoaDon>()
                .CountAsync(d => d.TrangThai == "Delivering" || d.TrangThai == "Đang giao" 
                              || d.TrangThai == "Preparing" || d.TrangThai == "Đang chuẩn bị");

            var donHoanThanhHomNay = hoaDonsHomNay
                .Count(d => d.TrangThai == "Completed" || d.TrangThai == "Hoàn thành");

            // 3. Phân hệ CRM
            var tongKhachHang = await _service.Query<KhachHang>().CountAsync();
            var danhGiaHienThi = _service.Query<DanhGia>().Where(d => d.TrangThai);
            var diemDanhGiaTb = await danhGiaHienThi.AnyAsync()
                ? await danhGiaHienThi.AverageAsync(d => (double?)d.SoSao) ?? 5.0
                : 5.0;
            var tongDanhGia = await danhGiaHienThi.CountAsync();
            var khaoSatDangMo = await _service.Query<KhaoSat>()
                .CountAsync(k => k.TrangThai == "Đang diễn ra" || k.TrangThai == "Hoạt động" || k.TrangThai == "Active");

            return Ok(new
            {
                // Admin
                SoMonAn = soMonAn,
                SoNhaCungCap = soNhaCungCap,
                SoPhieuNhapThangNay = soPhieuNhapThangNay,
                SoTaiKhoan = soTaiKhoan,

                // Bán hàng
                DoanhThuHomNay = doanhThuHomNay,
                DonChoXuLy = donChoXuLy,
                DonDangGiao = donDangGiao,
                DonHoanThanhHomNay = donHoanThanhHomNay,

                // CRM
                TongKhachHang = tongKhachHang,
                DiemDanhGiaTb = Math.Round(diemDanhGiaTb, 1),
                TongDanhGia = tongDanhGia,
                KhaoSatDangMo = khaoSatDangMo
            });
        }
    }
}

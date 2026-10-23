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

        // GET: api/ThongKe/BaoCaoDoanhThu - Báo cáo doanh thu & lợi nhuận chuyên sâu theo khoảng ngày
        [HttpGet("BaoCaoDoanhThu")]
        public async Task<IActionResult> GetBaoCaoDoanhThu([FromQuery] DateTime? tuNgay, [FromQuery] DateTime? denNgay)
        {
            var startDate = (tuNgay ?? DateTime.Today.AddDays(-6)).Date;
            var endDate = (denNgay ?? DateTime.Today).Date.AddDays(1); // Lấy đến hết ngày kết thúc

            // Lấy tất cả hóa đơn trong khoảng thời gian kèm chi tiết món và định lượng
            var hoaDons = await _service.Query<HoaDon>()
                .Include(h => h.ChiTietHoaDon)
                    .ThenInclude(ct => ct.MaMonNavigation)
                        .ThenInclude(m => m.DinhLuong)
                            .ThenInclude(dl => dl.MaNguyenLieuNavigation)
                .Where(h => h.NgayDat >= startDate && h.NgayDat < endDate)
                .OrderBy(h => h.NgayDat)
                .ToListAsync();

            var donHoanThanh = hoaDons.Where(h => h.TrangThai == "Completed" || h.TrangThai == "Hoàn thành").ToList();
            var donHuy = hoaDons.Where(h => h.TrangThai == "Cancelled" || h.TrangThai == "Đã hủy" || h.TrangThai == "Cancel").ToList();
            var donDangXuLy = hoaDons.Where(h => !donHoanThanh.Contains(h) && !donHuy.Contains(h)).ToList();

            // 1. Doanh thu thực thu
            double tongDoanhThu = donHoanThanh.Sum(h => h.TongThanhToan ?? 0);

            // 2. Tiền hàng niêm yết (Gross Sales)
            double tongTienHangGoc = donHoanThanh.Sum(h => h.ChiTietHoaDon.Sum(ct => ((ct.DonGiaGoc ?? ct.DonGiaSauGiam ?? 0) * (ct.SoLuong ?? 1))));

            // 3. Tiền giảm giá theo món & Voucher
            double tongTienGiamMon = donHoanThanh.Sum(h => h.ChiTietHoaDon.Sum(ct => ct.TienGiamMon ?? 0));
            double tongTienGiamVoucher = donHoanThanh.Sum(h => h.TienGiamVoucher ?? 0);
            double tongGiamGia = tongTienGiamMon + tongTienGiamVoucher;

            // 4. Tính giá vốn nguyên liệu (COGS) của các đơn hoàn tất
            double tongGiaVon = 0;
            var monAnStats = new Dictionary<string, (string TenMon, int SoLuong, double DoanhThu, double GiaVon)>();

            foreach (var hd in donHoanThanh)
            {
                foreach (var ct in hd.ChiTietHoaDon)
                {
                    var mon = ct.MaMonNavigation;
                    int sl = ct.SoLuong ?? 1;
                    double doanhThuMon = ct.ThanhTien ?? ((ct.DonGiaSauGiam ?? ct.DonGiaGoc ?? 0) * sl);

                    double giaVon1Mon = 0;
                    if (mon != null && mon.DinhLuong.Any())
                    {
                        giaVon1Mon = mon.DinhLuong.Sum(dl => (double)dl.SoLuongCan * (dl.MaNguyenLieuNavigation?.GiaNhapGanNhat ?? 0));
                    }
                    double giaVonChiTiet = giaVon1Mon * sl;
                    tongGiaVon += giaVonChiTiet;

                    string maMon = ct.MaMon ?? "Khac";
                    string tenMon = ct.TenMon ?? (mon?.TenMon ?? "Không xác định");
                    if (monAnStats.ContainsKey(maMon))
                    {
                        var cur = monAnStats[maMon];
                        monAnStats[maMon] = (tenMon, cur.SoLuong + sl, cur.DoanhThu + doanhThuMon, cur.GiaVon + giaVonChiTiet);
                    }
                    else
                    {
                        monAnStats[maMon] = (tenMon, sl, doanhThuMon, giaVonChiTiet);
                    }
                }
            }

            double loiNhuanGop = tongDoanhThu - tongGiaVon;
            double tyLeLoiNhuan = tongDoanhThu > 0 ? (loiNhuanGop / tongDoanhThu) * 100 : 0;
            int soDonHoanThanh = donHoanThanh.Count;
            double giaTriDonTb = soDonHoanThanh > 0 ? tongDoanhThu / soDonHoanThanh : 0;

            // 5. Thống kê theo ngày để vẽ biểu đồ
            var daysCount = (endDate.Date - startDate.Date).Days;
            var doanhThuTheoNgay = new List<object>();

            for (int i = 0; i < daysCount; i++)
            {
                var curDate = startDate.AddDays(i);
                var nextDate = curDate.AddDays(1);
                var donTrongNgay = donHoanThanh.Where(h => h.NgayDat >= curDate && h.NgayDat < nextDate).ToList();

                double dtNgay = donTrongNgay.Sum(h => h.TongThanhToan ?? 0);
                int soDonNgay = donTrongNgay.Count;

                string thuStr = curDate.DayOfWeek switch
                {
                    DayOfWeek.Monday => "T2",
                    DayOfWeek.Tuesday => "T3",
                    DayOfWeek.Wednesday => "T4",
                    DayOfWeek.Thursday => "T5",
                    DayOfWeek.Friday => "T6",
                    DayOfWeek.Saturday => "T7",
                    DayOfWeek.Sunday => "CN",
                    _ => ""
                };

                doanhThuTheoNgay.Add(new
                {
                    Ngay = curDate.ToString("yyyy-MM-dd"),
                    NgayHienThi = curDate.ToString("dd/MM"),
                    Thu = thuStr,
                    DoanhThu = dtNgay,
                    SoDon = soDonNgay
                });
            }

            // 6. Top món sinh lời & doanh thu cao nhất
            var topMon = monAnStats.Values
                .OrderByDescending(x => x.DoanhThu)
                .Take(10)
                .Select(x => new
                {
                    x.TenMon,
                    x.SoLuong,
                    x.DoanhThu,
                    x.GiaVon,
                    LoiNhuan = x.DoanhThu - x.GiaVon,
                    TyLeLai = x.DoanhThu > 0 ? Math.Round(((x.DoanhThu - x.GiaVon) / x.DoanhThu) * 100, 1) : 0
                })
                .ToList();

            return Ok(new
            {
                TuNgay = startDate.ToString("yyyy-MM-dd"),
                DenNgay = endDate.AddDays(-1).ToString("yyyy-MM-dd"),
                TongDoanhThu = Math.Round(tongDoanhThu, 0),
                TongTienHangGoc = Math.Round(tongTienHangGoc, 0),
                TongTienGiamMon = Math.Round(tongTienGiamMon, 0),
                TongTienGiamVoucher = Math.Round(tongTienGiamVoucher, 0),
                TongGiamGia = Math.Round(tongGiamGia, 0),
                TongGiaVon = Math.Round(tongGiaVon, 0),
                LoiNhuanGop = Math.Round(loiNhuanGop, 0),
                TyLeLoiNhuan = Math.Round(tyLeLoiNhuan, 1),
                TongDonHang = hoaDons.Count,
                DonHoanThanh = soDonHoanThanh,
                DonHuy = donHuy.Count,
                DonDangXuLy = donDangXuLy.Count,
                GiaTriDonTb = Math.Round(giaTriDonTb, 0),
                DoanhThuTheoNgay = doanhThuTheoNgay,
                TopMonBanChay = topMon
            });
        }
    }
}

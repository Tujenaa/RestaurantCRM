using RestaurantCRM.BusinessLogic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestaurantCRM.API.DTOs;

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

            // 1. Phân hệ Hệ thống
            var soMonAn = await _service.Query<MonAn>().CountAsync();
            var soNhaCungCap = await _service.Query<NhaCungCap>().CountAsync();
            var soPhieuNhapThangNay = await _service.Query<PhieuNhapHang>()
                .CountAsync(p => p.NgayNhap >= startOfMonth);
            var soTaiKhoan = await _service.Query<NhanVien>().CountAsync();

            // 2. Phân hệ Vận Hành
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

            // 3. Phân hệ Khách hàng
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
                // Hệ thống
                SoMonAn = soMonAn,
                SoNhaCungCap = soNhaCungCap,
                SoPhieuNhapThangNay = soPhieuNhapThangNay,
                SoTaiKhoan = soTaiKhoan,

                // Vận Hành
                DoanhThuHomNay = doanhThuHomNay,
                DonChoXuLy = donChoXuLy,
                DonDangGiao = donDangGiao,
                DonHoanThanhHomNay = donHoanThanhHomNay,

                // Khách hàng
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

        // GET: api/ThongKe/PhanTichKhachHang - Báo cáo phân tích cơ cấu độ tuổi, giới tính và sở thích món ăn
        [HttpGet("PhanTichKhachHang")]
        public async Task<IActionResult> GetPhanTichKhachHang()
        {
            var khachHangs = await _service.Query<KhachHang>()
                .Select(k => new { k.MaKhachHang, k.NgaySinh, k.GioiTinh })
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.Today);
            int tongKhachHang = khachHangs.Count;

            // 1. Phân tích nhóm tuổi
            int duoi18 = 0;
            int tu18Den24 = 0;
            int tu25Den34 = 0;
            int tu35Den50 = 0;
            int tren50 = 0;
            int chuaCapNhatTuoi = 0;
            int tongTuoi = 0;
            int coNgaySinh = 0;

            foreach (var k in khachHangs)
            {
                if (!k.NgaySinh.HasValue)
                {
                    chuaCapNhatTuoi++;
                    continue;
                }

                var dob = k.NgaySinh.Value;
                int tuoi = today.Year - dob.Year;
                if (dob > today.AddYears(-tuoi)) tuoi--;

                if (tuoi < 0 || tuoi > 130)
                {
                    chuaCapNhatTuoi++;
                    continue;
                }

                tongTuoi += tuoi;
                coNgaySinh++;

                if (tuoi < 18) duoi18++;
                else if (tuoi <= 24) tu18Den24++;
                else if (tuoi <= 34) tu25Den34++;
                else if (tuoi <= 50) tu35Den50++;
                else tren50++;
            }

            double RoundPct(int count) => tongKhachHang > 0 ? Math.Round((double)count / tongKhachHang * 100, 1) : 0;

            var dsDoTuoi = new List<NhomDoTuoiDto>
            {
                new NhomDoTuoiDto { TenNhom = "< 18", MoTa = "Học sinh / Dưới 18 tuổi", SoLuong = duoi18, TyLe = RoundPct(duoi18) },
                new NhomDoTuoiDto { TenNhom = "18 - 24", MoTa = "Sinh viên / Giới trẻ", SoLuong = tu18Den24, TyLe = RoundPct(tu18Den24) },
                new NhomDoTuoiDto { TenNhom = "25 - 34", MoTa = "Nhân viên văn phòng / Trẻ", SoLuong = tu25Den34, TyLe = RoundPct(tu25Den34) },
                new NhomDoTuoiDto { TenNhom = "35 - 50", MoTa = "Trung niên / Gia đình", SoLuong = tu35Den50, TyLe = RoundPct(tu35Den50) },
                new NhomDoTuoiDto { TenNhom = "> 50", MoTa = "Lớn tuổi", SoLuong = tren50, TyLe = RoundPct(tren50) },
                new NhomDoTuoiDto { TenNhom = "Chưa rõ", MoTa = "Chưa cập nhật ngày sinh", SoLuong = chuaCapNhatTuoi, TyLe = RoundPct(chuaCapNhatTuoi) }
            };

            // 2. Phân tích giới tính
            var genderGroups = khachHangs
                .GroupBy(k => string.IsNullOrWhiteSpace(k.GioiTinh) ? "Khác / Chưa rõ" : k.GioiTinh.Trim())
                .Select(g => new GioiTinhStatDto
                {
                    TenGioiTinh = g.Key,
                    SoLuong = g.Count(),
                    TyLe = RoundPct(g.Count())
                })
                .OrderByDescending(x => x.SoLuong)
                .ToList();

            // 3. Phân tích sở thích món ăn (lấy từ các hóa đơn hoàn thành)
            var chiTiets = await _service.Query<ChiTietHoaDon>()
                .Where(c => c.MaHoaDonNavigation != null &&
                           (c.MaHoaDonNavigation.TrangThai == "Completed" || c.MaHoaDonNavigation.TrangThai == "Hoàn thành"))
                .Select(c => new
                {
                    c.MaMon,
                    TenMon = c.TenMon ?? (c.MaMonNavigation != null ? c.MaMonNavigation.TenMon : "Món chưa xác định"),
                    TenLoaiMon = (c.MaMonNavigation != null && c.MaMonNavigation.MaLoaiMonNavigation != null)
                        ? c.MaMonNavigation.MaLoaiMonNavigation.TenLoaiMon
                        : "Khác",
                    SoLuong = c.SoLuong ?? 1,
                    ThanhTien = c.ThanhTien ?? ((c.DonGiaSauGiam ?? c.DonGiaGoc ?? 0) * (c.SoLuong ?? 1))
                })
                .ToListAsync();

            int tongSoLuongMonGoi = chiTiets.Sum(c => c.SoLuong);

            var topMonUaThich = chiTiets
                .GroupBy(c => new { c.MaMon, c.TenMon, c.TenLoaiMon })
                .Select(g =>
                {
                    int sl = g.Sum(x => x.SoLuong);
                    double doanhThu = g.Sum(x => x.ThanhTien);
                    double tyLe = tongSoLuongMonGoi > 0 ? Math.Round((double)sl / tongSoLuongMonGoi * 100, 1) : 0;
                    return new MonUaThichDto
                    {
                        MaMon = g.Key.MaMon ?? string.Empty,
                        TenMon = g.Key.TenMon ?? "Không xác định",
                        TenLoaiMon = g.Key.TenLoaiMon ?? "Khác",
                        SoLuongGoi = sl,
                        DoanhThu = Math.Round(doanhThu, 0),
                        TyLeUaThich = tyLe
                    };
                })
                .OrderByDescending(x => x.SoLuongGoi)
                .Take(12)
                .ToList();

            var response = new PhanTichKhachHangResponse
            {
                TongKhachHang = tongKhachHang,
                KhachHangCoNgaySinh = coNgaySinh,
                DoTuoiTrungBinh = coNgaySinh > 0 ? Math.Round((double)tongTuoi / coNgaySinh, 1) : 0,
                DoTuoi = dsDoTuoi,
                GioiTinh = genderGroups,
                TopMonUaThich = topMonUaThich
            };

            return Ok(response);
        }

        // GET: api/ThongKe/BaoCaoDoanhThuTheoQuy - Báo cáo tổng hợp chu kỳ Quý/Năm theo yêu cầu đồ án
        [HttpGet("BaoCaoDoanhThuTheoQuy")]
        public async Task<IActionResult> GetBaoCaoDoanhThuTheoQuy([FromQuery] int nam)
        {
            if (nam <= 0) nam = DateTime.Now.Year;

            // 1. Lấy danh sách các năm có hóa đơn phát sinh
            var danhSachNamHoaDon = await _service.Query<HoaDon>()
                .Where(h => h.NgayDat.HasValue)
                .Select(h => h.NgayDat.Value.Year)
                .Distinct()
                .ToListAsync();

            var danhSachNamPhieuNhap = await _service.Query<PhieuNhapHang>()
                .Where(p => p.NgayNhap.HasValue)
                .Select(p => p.NgayNhap.Value.Year)
                .Distinct()
                .ToListAsync();

            var allYears = danhSachNamHoaDon.Union(danhSachNamPhieuNhap).Union(new[] { nam, nam - 1, nam - 2 }).Distinct().OrderByDescending(y => y).ToList();

            // 2. Lấy dữ liệu hóa đơn hoàn tất trong năm chọn
            var startDate = new DateTime(nam, 1, 1);
            var endDate = new DateTime(nam + 1, 1, 1);

            var hoaDonsTrongNam = await _service.Query<HoaDon>()
                .Include(h => h.MaNhanVienNavigation)
                    .ThenInclude(nv => nv.MaVaiTroNavigation)
                .Include(h => h.ChiTietHoaDon)
                    .ThenInclude(ct => ct.MaMonNavigation)
                        .ThenInclude(m => m.MaLoaiMonNavigation)
                .Include(h => h.ChiTietHoaDon)
                    .ThenInclude(ct => ct.MaMonNavigation)
                        .ThenInclude(m => m.DinhLuong)
                            .ThenInclude(dl => dl.MaNguyenLieuNavigation)
                .Where(h => h.NgayDat >= startDate && h.NgayDat < endDate 
                            && (h.TrangThai == "Completed" || h.TrangThai == "Hoàn thành"))
                .ToListAsync();

            // Lấy phiếu nhập hàng trong năm chọn
            var phieuNhapsTrongNam = await _service.Query<PhieuNhapHang>()
                .Where(p => p.NgayNhap >= startDate && p.NgayNhap < endDate)
                .ToListAsync();

            // 3. Phân chia theo Quý
            Func<DateTime?, int> getQuarter = dt => dt.HasValue ? ((dt.Value.Month - 1) / 3) + 1 : 1;

            double thuQ1 = hoaDonsTrongNam.Where(h => getQuarter(h.NgayDat) == 1).Sum(h => h.TongThanhToan ?? 0);
            double thuQ2 = hoaDonsTrongNam.Where(h => getQuarter(h.NgayDat) == 2).Sum(h => h.TongThanhToan ?? 0);
            double thuQ3 = hoaDonsTrongNam.Where(h => getQuarter(h.NgayDat) == 3).Sum(h => h.TongThanhToan ?? 0);
            double thuQ4 = hoaDonsTrongNam.Where(h => getQuarter(h.NgayDat) == 4).Sum(h => h.TongThanhToan ?? 0);
            double tongThuNam = thuQ1 + thuQ2 + thuQ3 + thuQ4;

            // Chi phí: Ưu tiên lấy từ phiếu nhập hàng; nếu quý đó không có phiếu nhập thì tính từ COGS định lượng món
            Func<int, double> getChiQuy = q =>
            {
                var chiNhap = phieuNhapsTrongNam.Where(p => getQuarter(p.NgayNhap) == q).Sum(p => p.TongTien ?? 0);
                if (chiNhap > 0) return chiNhap;

                // Tính giá vốn từ đơn bán
                var donQuy = hoaDonsTrongNam.Where(h => getQuarter(h.NgayDat) == q);
                double cogs = 0;
                foreach (var hd in donQuy)
                {
                    foreach (var ct in hd.ChiTietHoaDon)
                    {
                        var mon = ct.MaMonNavigation;
                        if (mon != null && mon.DinhLuong.Any())
                        {
                            var gv1 = mon.DinhLuong.Sum(dl => (double)dl.SoLuongCan * (dl.MaNguyenLieuNavigation?.GiaNhapGanNhat ?? 0));
                            cogs += gv1 * (ct.SoLuong ?? 1);
                        }
                    }
                }
                return cogs;
            };

            double chiQ1 = getChiQuy(1);
            double chiQ2 = getChiQuy(2);
            double chiQ3 = getChiQuy(3);
            double chiQ4 = getChiQuy(4);
            double tongChiNam = chiQ1 + chiQ2 + chiQ3 + chiQ4;

            double lnQ1 = thuQ1 - chiQ1;
            double lnQ2 = thuQ2 - chiQ2;
            double lnQ3 = thuQ3 - chiQ3;
            double lnQ4 = thuQ4 - chiQ4;
            double tongLnNam = tongThuNam - tongChiNam;

            double tyLeLnQ1 = thuQ1 > 0 ? (lnQ1 / thuQ1) * 100 : 0;
            double tyLeLnQ2 = thuQ2 > 0 ? (lnQ2 / thuQ2) * 100 : 0;
            double tyLeLnQ3 = thuQ3 > 0 ? (lnQ3 / thuQ3) * 100 : 0;
            double tyLeLnQ4 = thuQ4 > 0 ? (lnQ4 / thuQ4) * 100 : 0;
            double tyLeLnNam = tongThuNam > 0 ? (tongLnNam / tongThuNam) * 100 : 0;

            // BẢNG 1: TÀI CHÍNH THEO QUÝ
            var bangTaiChinh = new List<DongTaiChinhQuyDto>
            {
                new DongTaiChinhQuyDto { ChiTieu = "💰 Tổng thu (Doanh thu)", Quy1 = Math.Round(thuQ1, 0), Quy2 = Math.Round(thuQ2, 0), Quy3 = Math.Round(thuQ3, 0), Quy4 = Math.Round(thuQ4, 0), TongNam = Math.Round(tongThuNam, 0), DinhDang = "Tien" },
                new DongTaiChinhQuyDto { ChiTieu = "📦 Tổng chi (Nhập kho / Chi phí)", Quy1 = Math.Round(chiQ1, 0), Quy2 = Math.Round(chiQ2, 0), Quy3 = Math.Round(chiQ3, 0), Quy4 = Math.Round(chiQ4, 0), TongNam = Math.Round(tongChiNam, 0), DinhDang = "Tien" },
                new DongTaiChinhQuyDto { ChiTieu = "📈 Lợi nhuận thuần", Quy1 = Math.Round(lnQ1, 0), Quy2 = Math.Round(lnQ2, 0), Quy3 = Math.Round(lnQ3, 0), Quy4 = Math.Round(lnQ4, 0), TongNam = Math.Round(tongLnNam, 0), DinhDang = "Tien" },
                new DongTaiChinhQuyDto { ChiTieu = "🏷️ Tỷ suất lợi nhuận (%)", Quy1 = Math.Round(tyLeLnQ1, 1), Quy2 = Math.Round(tyLeLnQ2, 1), Quy3 = Math.Round(tyLeLnQ3, 1), Quy4 = Math.Round(tyLeLnQ4, 1), TongNam = Math.Round(tyLeLnNam, 1), DinhDang = "PhanTram" }
            };

            // BẢNG 2: DOANH THU THEO NHÂN VIÊN
            var tatCaNhanVien = await _service.Query<NhanVien>()
                .Include(nv => nv.MaVaiTroNavigation)
                .ToListAsync();

            var bangNhanVien = new List<DongNhanVienQuyDto>();
            foreach (var nv in tatCaNhanVien)
            {
                var donCuaNv = hoaDonsTrongNam.Where(h => h.MaNhanVien == nv.MaNhanVien).ToList();
                double nvQ1 = donCuaNv.Where(h => getQuarter(h.NgayDat) == 1).Sum(h => h.TongThanhToan ?? 0);
                double nvQ2 = donCuaNv.Where(h => getQuarter(h.NgayDat) == 2).Sum(h => h.TongThanhToan ?? 0);
                double nvQ3 = donCuaNv.Where(h => getQuarter(h.NgayDat) == 3).Sum(h => h.TongThanhToan ?? 0);
                double nvQ4 = donCuaNv.Where(h => getQuarter(h.NgayDat) == 4).Sum(h => h.TongThanhToan ?? 0);
                double nvTong = nvQ1 + nvQ2 + nvQ3 + nvQ4;

                bangNhanVien.Add(new DongNhanVienQuyDto
                {
                    MaNhanVien = nv.MaNhanVien,
                    TenNhanVien = !string.IsNullOrEmpty(nv.HoTen) ? nv.HoTen : nv.MaNhanVien,
                    ChucVu = nv.MaVaiTroNavigation?.TenVaiTro ?? "Nhân viên",
                    Quy1 = Math.Round(nvQ1, 0),
                    Quy2 = Math.Round(nvQ2, 0),
                    Quy3 = Math.Round(nvQ3, 0),
                    Quy4 = Math.Round(nvQ4, 0),
                    TongNam = Math.Round(nvTong, 0),
                    TyLeDongGop = tongThuNam > 0 ? Math.Round((nvTong / tongThuNam) * 100, 1) : 0
                });
            }

            // Nếu có đơn không gán nhân viên
            var donKhongNv = hoaDonsTrongNam.Where(h => string.IsNullOrEmpty(h.MaNhanVien)).ToList();
            if (donKhongNv.Any())
            {
                double vQ1 = donKhongNv.Where(h => getQuarter(h.NgayDat) == 1).Sum(h => h.TongThanhToan ?? 0);
                double vQ2 = donKhongNv.Where(h => getQuarter(h.NgayDat) == 2).Sum(h => h.TongThanhToan ?? 0);
                double vQ3 = donKhongNv.Where(h => getQuarter(h.NgayDat) == 3).Sum(h => h.TongThanhToan ?? 0);
                double vQ4 = donKhongNv.Where(h => getQuarter(h.NgayDat) == 4).Sum(h => h.TongThanhToan ?? 0);
                double vTong = vQ1 + vQ2 + vQ3 + vQ4;
                bangNhanVien.Add(new DongNhanVienQuyDto
                {
                    MaNhanVien = "ONLINE",
                    TenNhanVien = "Khách đặt trực tuyến / Khác",
                    ChucVu = "Hệ thống",
                    Quy1 = Math.Round(vQ1, 0),
                    Quy2 = Math.Round(vQ2, 0),
                    Quy3 = Math.Round(vQ3, 0),
                    Quy4 = Math.Round(vQ4, 0),
                    TongNam = Math.Round(vTong, 0),
                    TyLeDongGop = tongThuNam > 0 ? Math.Round((vTong / tongThuNam) * 100, 1) : 0
                });
            }

            bangNhanVien = bangNhanVien.OrderByDescending(x => x.TongNam).ToList();
            for (int i = 0; i < bangNhanVien.Count; i++)
            {
                bangNhanVien[i].Stt = i + 1;
            }

            string nvXuatSac = bangNhanVien.FirstOrDefault(n => n.TongNam > 0)?.TenNhanVien ?? "Chưa có";
            double dtNvXuatSac = bangNhanVien.FirstOrDefault(n => n.TongNam > 0)?.TongNam ?? 0;

            // BẢNG 3: DOANH THU THEO MÓN ĂN (KÈM SO SÁNH 3 NĂM: nam-2, nam-1, nam)
            int nam1 = nam - 2;
            int nam2 = nam - 1;
            int nam3 = nam;

            var start3Years = new DateTime(nam1, 1, 1);
            var end3Years = new DateTime(nam3 + 1, 1, 1);

            var chiTiet3Nam = await _service.Query<ChiTietHoaDon>()
                .Include(ct => ct.MaMonNavigation)
                    .ThenInclude(m => m.MaLoaiMonNavigation)
                .Include(ct => ct.MaHoaDonNavigation)
                .Where(ct => ct.MaHoaDonNavigation != null 
                             && ct.MaHoaDonNavigation.NgayDat >= start3Years 
                             && ct.MaHoaDonNavigation.NgayDat < end3Years
                             && (ct.MaHoaDonNavigation.TrangThai == "Completed" || ct.MaHoaDonNavigation.TrangThai == "Hoàn thành"))
                .ToListAsync();

            var tatCaMonAn = await _service.Query<MonAn>()
                .Include(m => m.MaLoaiMonNavigation)
                .ToListAsync();

            var bangMonAn = new List<DongMonAnDoanhThuDto>();
            foreach (var mon in tatCaMonAn)
            {
                var ctMon = chiTiet3Nam.Where(c => c.MaMon == mon.MaMon).ToList();
                int slTong = ctMon.Sum(c => c.SoLuong ?? 1);

                // Theo quý của năm chọn
                var ctNamNay = ctMon.Where(c => c.MaHoaDonNavigation.NgayDat.Value.Year == nam).ToList();
                double mQ1 = ctNamNay.Where(c => getQuarter(c.MaHoaDonNavigation.NgayDat) == 1)
                                     .Sum(c => c.ThanhTien ?? ((c.DonGiaSauGiam ?? c.DonGiaGoc ?? 0) * (c.SoLuong ?? 1)));
                double mQ2 = ctNamNay.Where(c => getQuarter(c.MaHoaDonNavigation.NgayDat) == 2)
                                     .Sum(c => c.ThanhTien ?? ((c.DonGiaSauGiam ?? c.DonGiaGoc ?? 0) * (c.SoLuong ?? 1)));
                double mQ3 = ctNamNay.Where(c => getQuarter(c.MaHoaDonNavigation.NgayDat) == 3)
                                     .Sum(c => c.ThanhTien ?? ((c.DonGiaSauGiam ?? c.DonGiaGoc ?? 0) * (c.SoLuong ?? 1)));
                double mQ4 = ctNamNay.Where(c => getQuarter(c.MaHoaDonNavigation.NgayDat) == 4)
                                     .Sum(c => c.ThanhTien ?? ((c.DonGiaSauGiam ?? c.DonGiaGoc ?? 0) * (c.SoLuong ?? 1)));
                double mTongNamNay = mQ1 + mQ2 + mQ3 + mQ4;

                // Theo 3 năm
                double mNam1 = ctMon.Where(c => c.MaHoaDonNavigation.NgayDat.Value.Year == nam1)
                                    .Sum(c => c.ThanhTien ?? ((c.DonGiaSauGiam ?? c.DonGiaGoc ?? 0) * (c.SoLuong ?? 1)));
                double mNam2 = ctMon.Where(c => c.MaHoaDonNavigation.NgayDat.Value.Year == nam2)
                                    .Sum(c => c.ThanhTien ?? ((c.DonGiaSauGiam ?? c.DonGiaGoc ?? 0) * (c.SoLuong ?? 1)));
                double mNam3 = mTongNamNay;

                double tongDoanhThuMon = mNam1 + mNam2 + mNam3;

                bangMonAn.Add(new DongMonAnDoanhThuDto
                {
                    MaMon = mon.MaMon,
                    TenMon = mon.TenMon ?? "Không xác định",
                    DanhMuc = mon.MaLoaiMonNavigation?.TenLoaiMon ?? "Khác",
                    TongSoLuongBan = slTong,
                    Quy1 = Math.Round(mQ1, 0),
                    Quy2 = Math.Round(mQ2, 0),
                    Quy3 = Math.Round(mQ3, 0),
                    Quy4 = Math.Round(mQ4, 0),
                    DoanhThuNam1 = Math.Round(mNam1, 0),
                    DoanhThuNam2 = Math.Round(mNam2, 0),
                    DoanhThuNam3 = Math.Round(mNam3, 0),
                    TongDoanhThu = Math.Round(mTongNamNay, 0), // Ưu tiên doanh thu năm chọn
                    TyLeDongGop = tongThuNam > 0 ? Math.Round((mTongNamNay / tongThuNam) * 100, 1) : 0
                });
            }

            bangMonAn = bangMonAn.OrderByDescending(x => x.TongDoanhThu).ToList();
            for (int i = 0; i < bangMonAn.Count; i++)
            {
                bangMonAn[i].Stt = i + 1;
            }

            string monBanChayNhat = bangMonAn.FirstOrDefault(m => m.TongDoanhThu > 0)?.TenMon ?? "Chưa có";
            double dtMonBanChay = bangMonAn.FirstOrDefault(m => m.TongDoanhThu > 0)?.TongDoanhThu ?? 0;

            var result = new BaoCaoDoanhThuTheoQuyDto
            {
                Nam = nam,
                DanhSachNam = allYears,
                TongThuNam = Math.Round(tongThuNam, 0),
                TongChiNam = Math.Round(tongChiNam, 0),
                LoiNhuanNam = Math.Round(tongLnNam, 0),
                TyLeLoiNhuanNam = Math.Round(tyLeLnNam, 1),
                TongDonNam = hoaDonsTrongNam.Count,
                NhanVienXuatSac = nvXuatSac,
                DoanhThuNhanVienXuatSac = Math.Round(dtNvXuatSac, 0),
                MonBanChayNhat = monBanChayNhat,
                DoanhThuMonBanChayNhat = Math.Round(dtMonBanChay, 0),
                BangTaiChinhQuy = bangTaiChinh,
                BangNhanVien = bangNhanVien,
                BangMonAn = bangMonAn,
                CacNamSoSanhMonAn = new List<int> { nam1, nam2, nam3 }
            };

            return Ok(result);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HoaDonController : ControllerBase
    {
        private readonly RestaurantCrmContext _context;

        public HoaDonController(RestaurantCrmContext context)
        {
            _context = context;
        }

        // POST: api/HoaDon - Tạo đơn hàng mới
        [HttpPost]
        public async Task<IActionResult> CreateOrder(OrderRequest request)
        {
            if (request.Items == null || !request.Items.Any())
            {
                return BadRequest("Đơn hàng phải có ít nhất 1 món.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var maHoaDon = "DH" + DateTime.Now.Ticks.ToString().Substring(8, 6);

                var isOnline = string.Equals(request.PhuongThucThanhToan, "online", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(request.PhuongThucThanhToan, "transfer", StringComparison.OrdinalIgnoreCase);

                var hoaDon = new HoaDon
                {
                    MaHoaDon = maHoaDon,
                    MaKhachHang = request.MaKhachHang,
                    TenKhachHang = request.TenKhachHang,
                    SoDienThoai = request.SoDienThoai,
                    DiaChiGiao = request.DiaChiGiao,
                    NgayDat = DateTime.Now,
                    TrangThai = isOnline ? "Confirmed" : "Pending", // Đã thanh toán online thì đơn chuyển sang đã xác nhận
                    TongTienHang = 0,
                    TongThanhToan = 0,
                    TienGiamVoucher = 0
                };

                _context.HoaDon.Add(hoaDon);

                double tongTienHang = 0;
                var today = DateOnly.FromDateTime(DateTime.Now);

                // Lấy tất cả các món đang được sale hợp lệ
                var activeDishPromos = await _context.KmTheoSp
                    .Include(c => c.MaChuongTrinhNavigation)
                    .Where(c => c.MaChuongTrinhNavigation != null
                             && (c.MaChuongTrinhNavigation.NgayBatDau == null || c.MaChuongTrinhNavigation.NgayBatDau <= today)
                             && (c.MaChuongTrinhNavigation.NgayKetThuc == null || c.MaChuongTrinhNavigation.NgayKetThuc >= today))
                    .ToListAsync();

                foreach (var item in request.Items)
                {
                    var monAn = await _context.MonAn.FindAsync(item.MaMon);
                    if (monAn != null && monAn.DonGia.HasValue)
                    {
                        if (monAn.SoLuong == null || monAn.SoLuong < item.SoLuong)
                        {
                            return BadRequest($"Món ăn '{monAn.TenMon}' không đủ số lượng phục vụ (Chỉ còn: {monAn.SoLuong ?? 0}).");
                        }

                        // Trừ tồn kho món ăn
                        monAn.SoLuong -= item.SoLuong;
                        if (monAn.SoLuong <= 0 && monAn.TrangThai != "Discontinued")
                        {
                            monAn.TrangThai = "OutOfStock";
                        }

                        double donGiaGoc = monAn.DonGia.Value;
                        double donGiaSauGiam = donGiaGoc;
                        double tienGiamMon = 0;
                        string? maChuongTrinhKm = null;
                        int soLuongGiam = 0;

                        // Kiểm tra xem món này có đang Sale không
                        var promo = activeDishPromos.FirstOrDefault(p => p.MaMon == item.MaMon);
                        if (promo != null && promo.MaChuongTrinhNavigation != null)
                        {
                            maChuongTrinhKm = promo.MaKmsp;
                            
                            soLuongGiam = item.SoLuong; // Không còn quota

                            if (soLuongGiam > 0)
                            {
                                double discountPerItem = 0;
                                if (promo.PhanTramGiam.HasValue && promo.PhanTramGiam.Value > 0)
                                {
                                    discountPerItem = donGiaGoc * promo.PhanTramGiam.Value / 100.0;
                                }
                                else if (promo.TienGiam.HasValue && promo.TienGiam.Value > 0)
                                {
                                    discountPerItem = promo.TienGiam.Value;
                                }
                                
                                donGiaSauGiam = donGiaGoc - discountPerItem;
                                if (donGiaSauGiam < 0) donGiaSauGiam = 0;
                                
                                tienGiamMon = discountPerItem * soLuongGiam;
                            }
                        }

                        double thanhTien = (donGiaSauGiam * soLuongGiam) + (donGiaGoc * (item.SoLuong - soLuongGiam));

                        var maChiTiet = "CT" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                        var chiTiet = new ChiTietHoaDon
                        {
                            MaChiTiet = maChiTiet,
                            MaHoaDon = maHoaDon,
                            MaMon = item.MaMon,
                            MaKmsp = maChuongTrinhKm,
                            SoLuong = item.SoLuong,
                            DonGiaGoc = donGiaGoc,
                            DonGiaSauGiam = soLuongGiam > 0 ? donGiaSauGiam : donGiaGoc,
                            TienGiamMon = tienGiamMon,
                            ThanhTien = thanhTien
                        };

                        _context.ChiTietHoaDon.Add(chiTiet);
                        tongTienHang += thanhTien;
                    }
                }

                hoaDon.TongTienHang = tongTienHang;
                hoaDon.TongThanhToan = tongTienHang;

                // Áp dụng Voucher (nếu có)
                if (!string.IsNullOrEmpty(request.MaKmvoucher))
                {
                    var voucher = await _context.KmTheoVoucher
                        .Include(v => v.MaChuongTrinhNavigation)
                        .FirstOrDefaultAsync(v => v.MaKmvoucher == request.MaKmvoucher || v.MaVoucher == request.MaKmvoucher);

                    if (voucher != null && voucher.MaChuongTrinhNavigation != null)
                    {
                        var ct = voucher.MaChuongTrinhNavigation;
                        if ((ct.NgayBatDau == null || ct.NgayBatDau <= today) &&
                            (ct.NgayKetThuc == null || ct.NgayKetThuc >= today) &&
                            (voucher.GiaTriDonToiThieu == null || hoaDon.TongTienHang >= voucher.GiaTriDonToiThieu))
                        {
                            hoaDon.MaKmvoucher = voucher.MaKmvoucher;
                            
                            double tienGiam = 0;
                            if (voucher.PhanTramGiam.HasValue && voucher.PhanTramGiam.Value > 0)
                            {
                                tienGiam = tongTienHang * voucher.PhanTramGiam.Value / 100.0;
                            }
                            else if (voucher.TienGiam.HasValue && voucher.TienGiam.Value > 0)
                            {
                                tienGiam = voucher.TienGiam.Value;
                            }

                            hoaDon.TienGiamVoucher = tienGiam;
                            hoaDon.TongThanhToan = tongTienHang - tienGiam;
                            if (hoaDon.TongThanhToan < 0) hoaDon.TongThanhToan = 0;
                        }
                    }
                }

                // Lưu lại thanh toán nếu online
                if (isOnline)
                {
                    var thanhToan = new ThanhToan
                    {
                        MaThanhToan = "TT" + DateTime.Now.Ticks.ToString().Substring(8, 6),
                        MaHoaDon = maHoaDon,
                        PhuongThuc = "Online",
                        TrangThai = "Paid",
                        NgayThanhToan = DateTime.Now,
                        SoTien = hoaDon.TongThanhToan
                    };
                    _context.ThanhToan.Add(thanhToan);
                }

                // Lưu lại lịch sử trạng thái
                var lichSu = new LichSuTrangThai
                {
                    MaLichSu = "LS" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    MaHoaDon = maHoaDon,
                    TrangThai = isOnline ? "Confirmed" : "Pending",
                    ThoiGian = DateTime.Now,
                    GhiChu = isOnline 
                        ? "Đã thanh toán online thành công qua ngân hàng. Đơn hàng đã được xác nhận." 
                        : "Đơn hàng mới được tạo (Thanh toán COD khi nhận món)"
                };
                _context.LichSuTrangThai.Add(lichSu);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Đặt hàng thành công", maHoaDon = hoaDon.MaHoaDon });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi hệ thống: " + ex.Message + " | Inner: " + ex.InnerException?.Message);
            }
        }

        // PUT: api/HoaDon/{id}/HuyDon - Khách hàng hủy đơn (chỉ khi Pending)
        [HttpPut("{id}/HuyDon")]
        public async Task<IActionResult> CancelOrder(string id, [FromQuery] string maKhachHang)
        {
            var hoaDon = await _context.HoaDon
                .Include(d => d.ChiTietHoaDon)
                .FirstOrDefaultAsync(d => d.MaHoaDon == id && d.MaKhachHang == maKhachHang);

            if (hoaDon == null)
                return NotFound("Không tìm thấy đơn hàng của bạn.");

            if (hoaDon.TrangThai != "Pending")
                return BadRequest("Đơn hàng đã được xác nhận hoặc đang xử lý, không thể tự hủy.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Hoàn lại kho món ăn
                foreach (var chiTiet in hoaDon.ChiTietHoaDon)
                {
                    var monAn = await _context.MonAn.FindAsync(chiTiet.MaMon);
                    if (monAn != null && monAn.SoLuong.HasValue)
                    {
                        monAn.SoLuong += chiTiet.SoLuong;
                        if (monAn.SoLuong > 0 && monAn.TrangThai == "OutOfStock")
                            monAn.TrangThai = "InStock";
                    }
                    }
                hoaDon.TrangThai = "Cancelled";
                
                var lichSu = new LichSuTrangThai
                {
                    MaLichSu = "LS" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    MaHoaDon = id,
                    TrangThai = "Cancelled",
                    ThoiGian = DateTime.Now,
                    GhiChu = "Khách hàng tự hủy đơn"
                };
                _context.LichSuTrangThai.Add(lichSu);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok("Hủy đơn hàng thành công.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi hệ thống: " + ex.Message);
            }
        }

        // GET: api/HoaDon/LichSu/{maKhachHang} - Lấy lịch sử đơn hàng của khách
        [HttpGet("LichSu/{maKhachHang}")]
        public async Task<IActionResult> GetLichSuHoaDon(string maKhachHang)
        {
            var hoaDons = await _context.HoaDon
                .Where(d => d.MaKhachHang == maKhachHang)
                .OrderByDescending(d => d.NgayDat)
                .AsNoTracking()
                .Select(d => new 
                {
                    d.MaHoaDon,
                    d.NgayDat,
                    d.TrangThai,
                    d.TongThanhToan,
                    SoMon = d.ChiTietHoaDon.Sum(c => c.SoLuong)
                })
                .ToListAsync();

            return Ok(hoaDons);
        }

        // GET: api/HoaDon/{id} - Lấy chi tiết một đơn hàng
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChiTietHoaDon(string id)
        {
            var hoaDon = await _context.HoaDon
                .Include(d => d.ChiTietHoaDon)
                    .ThenInclude(c => c.MaMonNavigation)
                .Include(d => d.ThanhToan)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaHoaDon == id);

            if (hoaDon == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            var result = new
            {
                hoaDon.MaHoaDon,
                hoaDon.NgayDat,
                hoaDon.TrangThai,
                hoaDon.DiaChiGiao,
                hoaDon.TongTienHang,
                hoaDon.TienGiamVoucher,
                hoaDon.TongThanhToan,
                PhuongThucThanhToan = hoaDon.ThanhToan.FirstOrDefault()?.PhuongThuc ?? "COD",
                ChiTiet = hoaDon.ChiTietHoaDon.Select(c => new
                {
                    c.MaMon,
                    TenMon = c.MaMonNavigation?.TenMon,
                    c.SoLuong,
                    c.DonGiaGoc,
                    c.DonGiaSauGiam,
                    c.TienGiamMon,
                    c.ThanhTien
                })
            };

            return Ok(result);
        }
    }
}

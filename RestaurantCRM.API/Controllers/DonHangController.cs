using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonHangController : ControllerBase
    {
        private readonly RestaurantCRMContext _context;

        public DonHangController(RestaurantCRMContext context)
        {
            _context = context;
        }

        // POST: api/DonHang - Tạo đơn hàng mới
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
                var maDonHang = "DH" + DateTime.Now.Ticks.ToString().Substring(8, 6);

                var donHang = new DonHang
                {
                    MaDonHang = maDonHang,
                    MaKhachHang = request.MaKhachHang,
                    DiaChiGiao = request.DiaChiGiao,
                    NgayDat = DateTime.Now,
                    TrangThai = "Pending", // Đang chờ xác nhận
                    TongTienHang = 0,
                    TongThanhToan = 0,
                    TienGiamVoucher = 0
                };

                _context.DonHangs.Add(donHang);

                double tongTienHang = 0;
                var today = DateOnly.FromDateTime(DateTime.Now);

                // Lấy tất cả các món đang được sale hợp lệ
                var activeDishPromos = await _context.ChiTietKhuyenMaiMons
                    .Include(c => c.MaChuongTrinhNavigation)
                    .Where(c => c.MaChuongTrinhNavigation != null && c.MaChuongTrinhNavigation.TrangThai == "Active"
                             && c.MaChuongTrinhNavigation.LoaiKhuyenMai == "GiamGiaMon"
                             && (c.MaChuongTrinhNavigation.NgayBatDau == null || c.MaChuongTrinhNavigation.NgayBatDau <= today)
                             && (c.MaChuongTrinhNavigation.NgayKetThuc == null || c.MaChuongTrinhNavigation.NgayKetThuc >= today)
                             && (c.MaChuongTrinhNavigation.SoLuong == null || c.MaChuongTrinhNavigation.SoLuong > 0))
                    .ToListAsync();

                foreach (var item in request.Items)
                {
                    var monAn = await _context.MonAns.FindAsync(item.MaMon);
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
                            maChuongTrinhKm = promo.MaChuongTrinh;
                            
                            // Giảm tối đa SoLuongApDung trên 1 đơn. Nếu SoLuongApDung null thì giảm toàn bộ
                            soLuongGiam = promo.SoLuongApDung.HasValue 
                                ? Math.Min(item.SoLuong, promo.SoLuongApDung.Value) 
                                : item.SoLuong;
                            
                            // Xét Quota của quỹ Khuyến mãi tổng (SoLuong của ChuongTrinhKhuyenMai)
                            if (promo.MaChuongTrinhNavigation.SoLuong.HasValue)
                            {
                                soLuongGiam = Math.Min(soLuongGiam, promo.MaChuongTrinhNavigation.SoLuong.Value);
                                // Trừ quota của chương trình khuyến mãi
                                promo.MaChuongTrinhNavigation.SoLuong -= soLuongGiam;
                            }

                            if (soLuongGiam > 0)
                            {
                                double discountPerItem = 0;
                                if (promo.MaChuongTrinhNavigation.LoaiGiam == "%" && promo.MaChuongTrinhNavigation.GiaTriGiam.HasValue)
                                {
                                    discountPerItem = donGiaGoc * promo.MaChuongTrinhNavigation.GiaTriGiam.Value / 100.0;
                                }
                                else if (promo.MaChuongTrinhNavigation.LoaiGiam == "VND" && promo.MaChuongTrinhNavigation.GiaTriGiam.HasValue)
                                {
                                    discountPerItem = promo.MaChuongTrinhNavigation.GiaTriGiam.Value;
                                }
                                
                                donGiaSauGiam = donGiaGoc - discountPerItem;
                                if (donGiaSauGiam < 0) donGiaSauGiam = 0;
                                
                                tienGiamMon = discountPerItem * soLuongGiam;
                            }
                        }

                        double thanhTien = (donGiaSauGiam * soLuongGiam) + (donGiaGoc * (item.SoLuong - soLuongGiam));

                        var maChiTiet = "CT" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                        var chiTiet = new ChiTietDonHang
                        {
                            MaChiTiet = maChiTiet,
                            MaDonHang = maDonHang,
                            MaMon = item.MaMon,
                            MaChuongTrinhKm = maChuongTrinhKm,
                            SoLuong = item.SoLuong,
                            DonGiaGoc = donGiaGoc,
                            DonGiaSauGiam = soLuongGiam > 0 ? donGiaSauGiam : donGiaGoc,
                            TienGiamMon = tienGiamMon,
                            ThanhTien = thanhTien
                        };

                        _context.ChiTietDonHangs.Add(chiTiet);
                        tongTienHang += thanhTien;
                    }
                }

                donHang.TongTienHang = tongTienHang;
                donHang.TongThanhToan = tongTienHang;

                // Áp dụng Voucher (nếu có)
                if (!string.IsNullOrEmpty(request.MaChuongTrinhVoucher))
                {
                    var voucher = await _context.ChuongTrinhKhuyenMais
                        .FirstOrDefaultAsync(v => v.MaChuongTrinh == request.MaChuongTrinhVoucher 
                                               && v.TrangThai == "Active" 
                                               && v.LoaiKhuyenMai == "Voucher");

                    if (voucher != null)
                    {
                        if ((voucher.NgayBatDau == null || voucher.NgayBatDau <= today) &&
                            (voucher.NgayKetThuc == null || voucher.NgayKetThuc >= today) &&
                            (voucher.SoLuong == null || voucher.SoLuong > 0) &&
                            (voucher.GiaTriDonToiThieu == null || donHang.TongTienHang >= voucher.GiaTriDonToiThieu))
                        {
                            donHang.MaChuongTrinhVoucher = voucher.MaChuongTrinh;
                            
                            double tienGiam = 0;
                            if (voucher.LoaiGiam == "%" && voucher.GiaTriGiam.HasValue)
                            {
                                tienGiam = tongTienHang * voucher.GiaTriGiam.Value / 100.0;
                            }
                            else if (voucher.LoaiGiam == "VND" && voucher.GiaTriGiam.HasValue)
                            {
                                tienGiam = voucher.GiaTriGiam.Value;
                            }

                            donHang.TienGiamVoucher = tienGiam;
                            donHang.TongThanhToan = tongTienHang - tienGiam;
                            if (donHang.TongThanhToan < 0) donHang.TongThanhToan = 0;

                            // Trừ quota voucher
                            if (voucher.SoLuong.HasValue)
                            {
                                voucher.SoLuong -= 1;
                            }
                        }
                    }
                }

                // Lưu lại lịch sử
                var lichSu = new LichSuTrangThai
                {
                    MaLichSu = "LS" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    MaDonHang = maDonHang,
                    TrangThai = "Pending",
                    ThoiGian = DateTime.Now,
                    GhiChu = "Đơn hàng mới được tạo"
                };
                _context.LichSuTrangThais.Add(lichSu);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Đặt hàng thành công", maDonHang = donHang.MaDonHang });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi hệ thống: " + ex.Message);
            }
        }

        // PUT: api/DonHang/{id}/HuyDon - Khách hàng hủy đơn (chỉ khi Pending)
        [HttpPut("{id}/HuyDon")]
        public async Task<IActionResult> CancelOrder(string id, [FromQuery] string maKhachHang)
        {
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.MaDonHang == id && d.MaKhachHang == maKhachHang);

            if (donHang == null)
                return NotFound("Không tìm thấy đơn hàng của bạn.");

            if (donHang.TrangThai != "Pending")
                return BadRequest("Đơn hàng đã được xác nhận hoặc đang xử lý, không thể tự hủy.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Hoàn lại kho món ăn
                foreach (var chiTiet in donHang.ChiTietDonHangs)
                {
                    var monAn = await _context.MonAns.FindAsync(chiTiet.MaMon);
                    if (monAn != null && monAn.SoLuong.HasValue)
                    {
                        monAn.SoLuong += chiTiet.SoLuong;
                        if (monAn.SoLuong > 0 && monAn.TrangThai == "OutOfStock")
                            monAn.TrangThai = "InStock";
                    }

                    // Hoàn lại kho GiamGiaMon
                    if (!string.IsNullOrEmpty(chiTiet.MaChuongTrinhKm))
                    {
                        var kmMon = await _context.ChuongTrinhKhuyenMais.FindAsync(chiTiet.MaChuongTrinhKm);
                        if (kmMon != null && kmMon.SoLuong.HasValue)
                        {
                            // Tính lại số lượng món được giảm để cộng vào kho Khuyến mãi
                            int soLuongGiam = 0;
                            if (chiTiet.DonGiaGoc > chiTiet.DonGiaSauGiam && chiTiet.DonGiaGoc.HasValue && chiTiet.DonGiaSauGiam.HasValue && chiTiet.TienGiamMon.HasValue)
                            {
                                var discountPerItem = chiTiet.DonGiaGoc.Value - chiTiet.DonGiaSauGiam.Value;
                                soLuongGiam = (int)Math.Round(chiTiet.TienGiamMon.Value / discountPerItem);
                            }
                            kmMon.SoLuong += soLuongGiam;
                        }
                    }
                }

                // Hoàn lại Voucher
                if (!string.IsNullOrEmpty(donHang.MaChuongTrinhVoucher))
                {
                    var voucher = await _context.ChuongTrinhKhuyenMais.FindAsync(donHang.MaChuongTrinhVoucher);
                    if (voucher != null && voucher.SoLuong.HasValue)
                    {
                        voucher.SoLuong += 1;
                    }
                }

                donHang.TrangThai = "Cancelled";
                
                var lichSu = new LichSuTrangThai
                {
                    MaLichSu = "LS" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    MaDonHang = id,
                    TrangThai = "Cancelled",
                    ThoiGian = DateTime.Now,
                    GhiChu = "Khách hàng tự hủy đơn"
                };
                _context.LichSuTrangThais.Add(lichSu);

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

        // GET: api/DonHang/LichSu/{maKhachHang} - Lấy lịch sử đơn hàng của khách
        [HttpGet("LichSu/{maKhachHang}")]
        public async Task<IActionResult> GetLichSuDonHang(string maKhachHang)
        {
            var donHangs = await _context.DonHangs
                .Where(d => d.MaKhachHang == maKhachHang)
                .OrderByDescending(d => d.NgayDat)
                .AsNoTracking()
                .Select(d => new 
                {
                    d.MaDonHang,
                    d.NgayDat,
                    d.TrangThai,
                    d.TongThanhToan,
                    SoMon = d.ChiTietDonHangs.Sum(c => c.SoLuong)
                })
                .ToListAsync();

            return Ok(donHangs);
        }

        // GET: api/DonHang/{id} - Lấy chi tiết một đơn hàng
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChiTietDonHang(string id)
        {
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.MaMonNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDonHang == id);

            if (donHang == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            var result = new
            {
                donHang.MaDonHang,
                donHang.NgayDat,
                donHang.TrangThai,
                donHang.DiaChiGiao,
                donHang.TongTienHang,
                donHang.TienGiamVoucher,
                donHang.TongThanhToan,
                ChiTiet = donHang.ChiTietDonHangs.Select(c => new
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

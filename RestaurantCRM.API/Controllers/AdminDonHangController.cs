using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminDonHangController : ControllerBase
    {
        private readonly RestaurantCRMContext _context;

        public AdminDonHangController(RestaurantCRMContext context)
        {
            _context = context;
        }

        // GET: api/AdminDonHang - Lấy toàn bộ danh sách đơn hàng
        [HttpGet]
        public async Task<IActionResult> GetAllDonHangs()
        {
            var donHangs = await _context.DonHangs
                .Include(d => d.MaKhachHangNavigation)
                .Include(d => d.MaNhanVienNavigation)
                .OrderByDescending(d => d.NgayDat)
                .AsNoTracking()
                .Select(d => new
                {
                    d.MaDonHang,
                    d.NgayDat,
                    TenKhachHang = d.MaKhachHangNavigation != null ? d.MaKhachHangNavigation.HoTen : "Khách vãng lai",
                    d.TrangThai,
                    d.TongTienHang,
                    d.TongThanhToan,
                    d.MaNhanVien,
                    TenNhanVien = d.MaNhanVienNavigation != null ? d.MaNhanVienNavigation.HoTen : null
                })
                .ToListAsync();

            return Ok(donHangs);
        }

        // GET: api/AdminDonHang/{id} - Lấy chi tiết đơn hàng
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChiTietDonHang(string id)
        {
            var donHang = await _context.DonHangs
                .Include(d => d.MaKhachHangNavigation)
                .Include(d => d.MaNhanVienNavigation)
                .Include(d => d.MaChuongTrinhVoucherNavigation)
                .Include(d => d.ThanhToans)
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
                KhachHang = donHang.MaKhachHangNavigation != null ? new { donHang.MaKhachHangNavigation.HoTen, donHang.MaKhachHangNavigation.SoDienThoai } : null,
                NhanVienPhuTrach = donHang.MaNhanVienNavigation != null ? donHang.MaNhanVienNavigation.HoTen : null,
                KhuyenMai = donHang.MaChuongTrinhVoucherNavigation != null ? donHang.MaChuongTrinhVoucherNavigation.TenChuongTrinh : null,
                ThanhToan = donHang.ThanhToans.Select(t => new { t.PhuongThuc, t.NgayThanhToan, t.SoTien, t.TrangThai }),
                ChiTiet = donHang.ChiTietDonHangs.Select(c => new
                {
                    c.MaMon,
                    TenMon = c.MaMonNavigation?.TenMon,
                    c.SoLuong,
                    c.DonGiaSauGiam,
                    c.ThanhTien
                })
            };

            return Ok(result);
        }

        // GET: api/AdminDonHang/{id}/LichSuTrangThai - Xem chi tiết lịch sử cập nhật trạng thái của đơn hàng
        [HttpGet("{id}/LichSuTrangThai")]
        public async Task<IActionResult> GetLichSuTrangThai(string id)
        {
            var lichSu = await _context.LichSuTrangThais
                .Where(l => l.MaDonHang == id)
                .OrderByDescending(l => l.ThoiGian)
                .Select(l => new
                {
                    l.MaLichSu,
                    l.TrangThai,
                    l.ThoiGian,
                    l.GhiChu
                })
                .ToListAsync();

            return Ok(lichSu);
        }

        // PUT: api/AdminDonHang/{id}/TrangThai - Cập nhật trạng thái mới cho đơn hàng và lưu log
        [HttpPut("{id}/TrangThai")]
        public async Task<IActionResult> UpdateTrangThaiDonHang(string id, UpdateOrderStatusRequest request)
        {
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.MaDonHang == id);

            if (donHang == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            var trangThaiCu = donHang.TrangThai;

            // Logic hoàn kho: Chỉ cộng lại số lượng nếu đơn vừa chuyển sang Hủy và trạng thái cũ là Pending hoặc Processing
            if ((request.TrangThaiMoi == "Cancelled" || request.TrangThaiMoi == "Đã hủy" || request.TrangThaiMoi == "Hủy") && 
                (trangThaiCu == "Pending" || trangThaiCu == "Processing" || trangThaiCu == "Preparing" || trangThaiCu == "Đang chờ xác nhận" || trangThaiCu == "Đang xử lý"))
            {
                foreach (var chiTiet in donHang.ChiTietDonHangs)
                {
                    var monAn = await _context.MonAns.FindAsync(chiTiet.MaMon);
                    if (monAn != null && monAn.SoLuong.HasValue)
                    {
                        monAn.SoLuong += chiTiet.SoLuong;

                        // Nếu hoàn lại kho làm số lượng > 0 và món đang ở trạng thái hết hàng (không phải Discontinued), mở lại InStock
                        if (monAn.SoLuong > 0 && monAn.TrangThai == "OutOfStock")
                        {
                            monAn.TrangThai = "InStock";
                        }
                    }
                    
                    // Hoàn lại kho GiamGiaMon
                    if (!string.IsNullOrEmpty(chiTiet.MaChuongTrinhKm))
                    {
                        var kmMon = await _context.ChuongTrinhKhuyenMais.FindAsync(chiTiet.MaChuongTrinhKm);
                        if (kmMon != null && kmMon.SoLuong.HasValue)
                        {
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
            }

            // Gán nhân viên tiếp nhận đơn (nếu có MaNhanVien gửi lên và đơn chưa có người nhận)
            if (!string.IsNullOrEmpty(request.MaNhanVien) && donHang.MaNhanVien == null && request.TrangThaiMoi == "Preparing")
            {
                donHang.MaNhanVien = request.MaNhanVien;
            }

            // Cập nhật trạng thái mới
            donHang.TrangThai = request.TrangThaiMoi;

            // Ghi log vào bảng LichSuTrangThai
            var lichSu = new LichSuTrangThai
            {
                MaLichSu = "LS" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                MaDonHang = id,
                TrangThai = request.TrangThaiMoi,
                ThoiGian = DateTime.Now,
                GhiChu = request.GhiChu
            };
            
            _context.LichSuTrangThais.Add(lichSu);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật trạng thái thành công" });
        }
    }
}

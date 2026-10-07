using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminHoaDonController : ControllerBase
    {
        private readonly RestaurantCrmContext _context;

        public AdminHoaDonController(RestaurantCrmContext context)
        {
            _context = context;
        }

        // GET: api/AdminHoaDon - Lấy toàn bộ danh sách đơn hàng
        [HttpGet]
        public async Task<IActionResult> GetAllHoaDons()
        {
            var hoaDons = await _context.HoaDon
                .Include(d => d.MaKhachHangNavigation)
                .Include(d => d.MaNhanVienNavigation)
                .OrderByDescending(d => d.NgayDat)
                .AsNoTracking()
                .Select(d => new
                {
                    d.MaHoaDon,
                    d.NgayDat,
                    TenKhachHang = d.MaKhachHangNavigation != null ? d.MaKhachHangNavigation.HoTen : "Khách vãng lai",
                    d.TrangThai,
                    d.TongTienHang,
                    d.TongThanhToan,
                    d.MaNhanVien,
                    TenNhanVien = d.MaNhanVienNavigation != null ? d.MaNhanVienNavigation.HoTen : null
                })
                .ToListAsync();

            return Ok(hoaDons);
        }

        // GET: api/AdminHoaDon/{id} - Lấy chi tiết đơn hàng
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChiTietHoaDon(string id)
        {
            var hoaDon = await _context.HoaDon
                .Include(d => d.MaKhachHangNavigation)
                .Include(d => d.MaNhanVienNavigation)
                .Include(d => d.MaKmvoucherNavigation)
                    .ThenInclude(v => v.MaChuongTrinhNavigation)
                .Include(d => d.ThanhToan)
                .Include(d => d.ChiTietHoaDon)
                    .ThenInclude(c => c.MaMonNavigation)
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
                KhachHang = hoaDon.MaKhachHangNavigation != null ? new { hoaDon.MaKhachHangNavigation.HoTen, hoaDon.MaKhachHangNavigation.SoDienThoai } : null,
                NhanVienPhuTrach = hoaDon.MaNhanVienNavigation != null ? hoaDon.MaNhanVienNavigation.HoTen : null,
                KhuyenMai = hoaDon.MaKmvoucherNavigation?.MaChuongTrinhNavigation?.TenChuongTrinh,
                ThanhToan = hoaDon.ThanhToan.Select(t => new { t.PhuongThuc, t.NgayThanhToan, t.SoTien, t.TrangThai }),
                ChiTiet = hoaDon.ChiTietHoaDon.Select(c => new
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

        // GET: api/AdminHoaDon/{id}/LichSuTrangThai - Xem chi tiết lịch sử cập nhật trạng thái của đơn hàng
        [HttpGet("{id}/LichSuTrangThai")]
        public async Task<IActionResult> GetLichSuTrangThai(string id)
        {
            var lichSu = await _context.LichSuTrangThai
                .Where(l => l.MaHoaDon == id)
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

        // PUT: api/AdminHoaDon/{id}/TrangThai - Cập nhật trạng thái mới cho đơn hàng và lưu log
        [HttpPut("{id}/TrangThai")]
        public async Task<IActionResult> UpdateTrangThaiHoaDon(string id, UpdateOrderStatusRequest request)
        {
            var hoaDon = await _context.HoaDon
                .Include(d => d.ChiTietHoaDon)
                .FirstOrDefaultAsync(d => d.MaHoaDon == id);

            if (hoaDon == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            var trangThaiCu = hoaDon.TrangThai;

            // Logic hoàn kho: Chỉ cộng lại số lượng nếu đơn vừa chuyển sang Hủy và trạng thái cũ là Pending hoặc Processing
            if ((request.TrangThaiMoi == "Cancelled" || request.TrangThaiMoi == "Đã hủy" || request.TrangThaiMoi == "Hủy") && 
                (trangThaiCu == "Pending" || trangThaiCu == "Processing" || trangThaiCu == "Preparing" || trangThaiCu == "Đang chờ xác nhận" || trangThaiCu == "Đang xử lý"))
            {
                foreach (var chiTiet in hoaDon.ChiTietHoaDon)
                {
                    var monAn = await _context.MonAn.FindAsync(chiTiet.MaMon);
                    if (monAn != null && monAn.SoLuong.HasValue)
                    {
                        monAn.SoLuong += chiTiet.SoLuong;

                        // Nếu hoàn lại kho làm số lượng > 0 và món đang ở trạng thái hết hàng (không phải Discontinued), mở lại InStock
                        if (monAn.SoLuong > 0 && monAn.TrangThai == "OutOfStock")
                        {
                            monAn.TrangThai = "InStock";
                        }
                    }
                    
                }
            }

            // Gán nhân viên tiếp nhận đơn (nếu có MaNhanVien gửi lên và đơn chưa có người nhận)
            if (!string.IsNullOrEmpty(request.MaNhanVien) && hoaDon.MaNhanVien == null && request.TrangThaiMoi == "Preparing")
            {
                hoaDon.MaNhanVien = request.MaNhanVien;
            }

            // Cập nhật trạng thái mới
            hoaDon.TrangThai = request.TrangThaiMoi;

            // Ghi log vào bảng LichSuTrangThai
            var lichSu = new LichSuTrangThai
            {
                MaLichSu = "LS" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                MaHoaDon = id,
                TrangThai = request.TrangThaiMoi,
                ThoiGian = DateTime.Now,
                GhiChu = request.GhiChu
            };
            
            _context.LichSuTrangThai.Add(lichSu);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật trạng thái thành công" });
        }
    }
}

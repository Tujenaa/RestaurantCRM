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
                .OrderByDescending(d => d.NgayDat)
                .AsNoTracking()
                .Select(d => new
                {
                    d.MaDonHang,
                    d.NgayDat,
                    TenKhachHang = d.MaKhachHangNavigation != null ? d.MaKhachHangNavigation.HoTen : "Khách vãng lai",
                    d.TrangThai,
                    d.TongTienHang,
                    d.TongThanhToan
                })
                .ToListAsync();

            return Ok(donHangs);
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
                (trangThaiCu == "Pending" || trangThaiCu == "Processing" || trangThaiCu == "Đang chờ xác nhận" || trangThaiCu == "Đang xử lý"))
            {
                foreach (var chiTiet in donHang.ChiTietDonHangs)
                {
                    var monAn = await _context.MonAns.FindAsync(chiTiet.MaMon);
                    if (monAn != null && monAn.SoLuong.HasValue)
                    {
                        monAn.SoLuong += chiTiet.SoLuong;
                    }
                }
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

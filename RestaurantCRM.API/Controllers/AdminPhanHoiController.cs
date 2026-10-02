using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminPhanHoiController : ControllerBase
    {
        private readonly RestaurantCRMContext _context;

        public AdminPhanHoiController(RestaurantCRMContext context)
        {
            _context = context;
        }

        // GET: api/AdminPhanHoi - Lấy toàn bộ danh sách phản hồi (có thể lọc theo trạng thái)
        [HttpGet]
        public async Task<IActionResult> GetAllPhanHoi([FromQuery] string? trangThai)
        {
            var query = _context.PhanHois
                .Include(p => p.MaKhachHangNavigation)
                .Include(p => p.MaNhanVienNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(p => p.TrangThai == trangThai);
            }

            var phanHois = await query
                .OrderByDescending(p => p.NgayPhanHoi)
                .Select(p => new
                {
                    p.MaPhanHoi,
                    p.NoiDung,
                    p.DanhGia,
                    p.NgayPhanHoi,
                    p.TrangThai,
                    TenKhachHang = p.MaKhachHangNavigation != null ? p.MaKhachHangNavigation.HoTen : "Khách ẩn danh",
                    NhanVienPhuTrach = p.MaNhanVienNavigation != null ? p.MaNhanVienNavigation.HoTen : "Chưa phân công"
                })
                .ToListAsync();

            return Ok(phanHois);
        }

        // GET: api/AdminPhanHoi/{id} - Lấy chi tiết phản hồi (Kèm hội thoại)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChiTietPhanHoi(string id)
        {
            var phanHoi = await _context.PhanHois
                .Include(p => p.MaKhachHangNavigation)
                .Include(p => p.MaNhanVienNavigation)
                .Include(p => p.TraLoiPhanHois)
                    .ThenInclude(t => t.MaKhachHangNavigation)
                .Include(p => p.TraLoiPhanHois)
                    .ThenInclude(t => t.MaNhanVienNavigation)
                .FirstOrDefaultAsync(p => p.MaPhanHoi == id);

            if (phanHoi == null)
            {
                return NotFound("Không tìm thấy phản hồi.");
            }

            var result = new
            {
                phanHoi.MaPhanHoi,
                phanHoi.NoiDung,
                phanHoi.DanhGia,
                phanHoi.NgayPhanHoi,
                phanHoi.TrangThai,
                TenKhachHang = phanHoi.MaKhachHangNavigation != null ? phanHoi.MaKhachHangNavigation.HoTen : "Khách ẩn danh",
                NhanVienPhuTrach = phanHoi.MaNhanVienNavigation != null ? phanHoi.MaNhanVienNavigation.HoTen : "Chưa phân công",
                TongSoTraLoi = phanHoi.TraLoiPhanHois.Count,
                HoiThoai = phanHoi.TraLoiPhanHois.OrderBy(t => t.NgayGui).Select(t => new
                {
                    t.MaTraLoi,
                    t.NguoiGui,
                    t.NoiDung,
                    t.NgayGui,
                    NguoiTraLoi = t.NguoiGui == "KhachHang" && t.MaKhachHangNavigation != null ? t.MaKhachHangNavigation.HoTen : 
                                  (t.NguoiGui == "NhanVien" && t.MaNhanVienNavigation != null ? t.MaNhanVienNavigation.HoTen : "Ẩn danh")
                })
            };

            return Ok(result);
        }

        // PUT: api/AdminPhanHoi/{id}/TrangThai - Cập nhật trạng thái xử lý phản hồi
        [HttpPut("{id}/TrangThai")]
        public async Task<IActionResult> UpdateTrangThai(string id, PhanHoiStatusUpdateRequest request)
        {
            var phanHoi = await _context.PhanHois.FindAsync(id);
            if (phanHoi == null)
            {
                return NotFound("Không tìm thấy phản hồi.");
            }

            var validStatuses = new[] { "Pending", "InProgress", "Resolved", "Closed" };
            if (!validStatuses.Contains(request.TrangThai))
            {
                return BadRequest("Trạng thái không hợp lệ. (Chỉ cho phép Pending, InProgress, Resolved, Closed).");
            }

            phanHoi.TrangThai = request.TrangThai;

            // Nếu truyền mã nhân viên lên thì gán nhân viên phụ trách
            if (!string.IsNullOrEmpty(request.MaNhanVien))
            {
                phanHoi.MaNhanVien = request.MaNhanVien;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Đã cập nhật trạng thái phản hồi thành {request.TrangThai}." });
        }

        // POST: api/AdminPhanHoi/{id}/Reply - Nhân viên nhắn tin trả lời khách hàng trong Ticket
        [HttpPost("{id}/Reply")]
        public async Task<IActionResult> CreateReply(string id, ReplyRequest request)
        {
            var phanHoi = await _context.PhanHois.FindAsync(id);
            if (phanHoi == null)
            {
                return NotFound("Không tìm thấy phản hồi.");
            }

            var traLoi = new TraLoiPhanHoi
            {
                MaTraLoi = "TP" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                MaPhanHoi = id,
                NguoiGui = "NhanVien",
                MaKhachHang = null,
                MaNhanVien = request.NguoiGuiId,
                NoiDung = request.NoiDung,
                NgayGui = DateTime.Now
            };

            // Có thể tự động đổi trạng thái sang InProgress khi nhân viên bắt đầu reply
            if (phanHoi.TrangThai == "Pending")
            {
                phanHoi.TrangThai = "InProgress";
            }

            _context.TraLoiPhanHois.Add(traLoi);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Gửi phản hồi thành công", maTraLoi = traLoi.MaTraLoi });
        }
    }
}

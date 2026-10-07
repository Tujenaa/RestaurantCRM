using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhanHoiController : ControllerBase
    {
        private readonly RestaurantCrmContext _context;

        public PhanHoiController(RestaurantCrmContext context)
        {
            _context = context;
        }

        // POST: api/PhanHoi - Khách tạo phản hồi mới (Ticket)
        [HttpPost]
        public async Task<IActionResult> CreatePhanHoi(PhanHoiCreateRequest request)
        {
            var phanHoi = new PhanHoi
            {
                MaPhanHoi = "PH" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                MaKhachHang = request.MaKhachHang,
                NoiDung = request.NoiDung,
                DanhGia = request.DanhGia,
                NgayPhanHoi = DateTime.Now,
                TrangThai = "Pending"
            };

            _context.PhanHoi.Add(phanHoi);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Gửi phản hồi thành công", maPhanHoi = phanHoi.MaPhanHoi });
        }

        // GET: api/PhanHoi/KhachHang/{maKhachHang} - Lấy danh sách phản hồi của khách
        [HttpGet("KhachHang/{maKhachHang}")]
        public async Task<IActionResult> GetPhanHoiByKhachHang(string maKhachHang)
        {
            var phanHois = await _context.PhanHoi
                .Where(p => p.MaKhachHang == maKhachHang)
                .OrderByDescending(p => p.NgayPhanHoi)
                .Select(p => new
                {
                    p.MaPhanHoi,
                    p.NoiDung,
                    p.DanhGia,
                    p.NgayPhanHoi,
                    p.TrangThai
                })
                .ToListAsync();

            return Ok(phanHois);
        }

        // GET: api/PhanHoi/{id} - Lấy chi tiết phản hồi (Kèm hội thoại)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChiTietPhanHoi(string id)
        {
            var phanHoi = await _context.PhanHoi
                .Include(p => p.MaNhanVienNavigation)
                .Include(p => p.TraLoiPhanHoi)
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
                NhanVienPhuTrach = phanHoi.MaNhanVienNavigation != null ? phanHoi.MaNhanVienNavigation.HoTen : "Chưa phân công",
                HoiThoai = phanHoi.TraLoiPhanHoi.OrderBy(t => t.NgayGui).Select(t => new
                {
                    t.MaTraLoi,
                    t.NguoiGui,
                    t.NoiDung,
                    t.NgayGui
                })
            };

            return Ok(result);
        }

        // POST: api/PhanHoi/{id}/Reply - Khách hàng nhắn thêm thông tin trong phản hồi
        [HttpPost("{id}/Reply")]
        public async Task<IActionResult> CreateReply(string id, ReplyRequest request)
        {
            var phanHoi = await _context.PhanHoi.FindAsync(id);
            if (phanHoi == null)
            {
                return NotFound("Không tìm thấy phản hồi.");
            }

            var traLoi = new TraLoiPhanHoi
            {
                MaTraLoi = "TP" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                MaPhanHoi = id,
                NguoiGui = "KhachHang",
                MaKhachHang = request.NguoiGuiId,
                MaNhanVien = null,
                NoiDung = request.NoiDung,
                NgayGui = DateTime.Now
            };

            _context.TraLoiPhanHoi.Add(traLoi);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Gửi trả lời thành công", maTraLoi = traLoi.MaTraLoi });
        }
    }
}

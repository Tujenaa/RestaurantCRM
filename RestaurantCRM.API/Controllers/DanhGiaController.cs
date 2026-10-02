using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DanhGiaController : ControllerBase
    {
        private readonly RestaurantCRMContext _context;

        public DanhGiaController(RestaurantCRMContext context)
        {
            _context = context;
        }

        // GET: api/DanhGia/MonAn/{maMon} - Lấy danh sách đánh giá của một món ăn
        [HttpGet("MonAn/{maMon}")]
        public async Task<IActionResult> GetDanhGiaByMonAn(string maMon)
        {
            var danhGias = await _context.DanhGias
                .Where(d => d.MaMon == maMon)
                .Include(d => d.MaKhachHangNavigation)
                .Include(d => d.TraLoiDanhGias)
                .OrderByDescending(d => d.NgayDanhGia)
                .Select(d => new
                {
                    d.MaDanhGia,
                    d.SoSao,
                    d.NoiDung,
                    d.NgayDanhGia,
                    TenKhachHang = d.MaKhachHangNavigation != null ? d.MaKhachHangNavigation.HoTen : "Khách ẩn danh",
                    HoiThoai = d.TraLoiDanhGias.OrderBy(t => t.NgayGui).Select(t => new
                    {
                        t.MaTraLoi,
                        t.NguoiGui,
                        t.NoiDung,
                        t.NgayGui,
                        NguoiTraLoi = t.NguoiGui == "KhachHang" && t.MaKhachHangNavigation != null ? t.MaKhachHangNavigation.HoTen : 
                                      (t.NguoiGui == "NhanVien" && t.MaNhanVienNavigation != null ? t.MaNhanVienNavigation.HoTen : "Ẩn danh")
                    })
                })
                .ToListAsync();

            return Ok(danhGias);
        }

        // POST: api/DanhGia - Gửi đánh giá cho món ăn
        [HttpPost]
        public async Task<IActionResult> CreateDanhGia(ReviewRequest request)
        {
            if (request.SoSao < 1 || request.SoSao > 5)
            {
                return BadRequest("Số sao phải từ 1 đến 5.");
            }

            var danhGia = new DanhGia
            {
                MaDanhGia = "DG" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                MaKhachHang = request.MaKhachHang,
                MaMon = request.MaMon,
                MaDonHang = request.MaDonHang,
                SoSao = request.SoSao,
                NoiDung = request.NoiDung,
                NgayDanhGia = DateTime.Now
            };

            _context.DanhGias.Add(danhGia);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đánh giá thành công", maDanhGia = danhGia.MaDanhGia });
        }

        // POST: api/DanhGia/{id}/Reply - Khách hàng phản hồi lại một đánh giá
        [HttpPost("{id}/Reply")]
        public async Task<IActionResult> CreateReply(string id, ReplyRequest request)
        {
            var danhGia = await _context.DanhGias.FindAsync(id);
            if (danhGia == null)
            {
                return NotFound("Không tìm thấy đánh giá.");
            }

            var traLoi = new TraLoiDanhGia
            {
                MaTraLoi = "TL" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                MaDanhGia = id,
                NguoiGui = "KhachHang",
                MaKhachHang = request.NguoiGuiId,
                MaNhanVien = null,
                NoiDung = request.NoiDung,
                NgayGui = DateTime.Now
            };

            _context.TraLoiDanhGias.Add(traLoi);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Gửi phản hồi thành công", maTraLoi = traLoi.MaTraLoi });
        }
    }
}

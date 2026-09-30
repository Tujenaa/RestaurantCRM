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
                .OrderByDescending(d => d.NgayDanhGia)
                .Select(d => new
                {
                    d.MaDanhGia,
                    d.SoSao,
                    d.NoiDung,
                    d.NgayDanhGia,
                    TenKhachHang = d.MaKhachHangNavigation != null ? d.MaKhachHangNavigation.HoTen : "Khách ẩn danh"
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
    }
}

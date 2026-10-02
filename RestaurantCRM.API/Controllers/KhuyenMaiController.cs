using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhuyenMaiController : ControllerBase
    {
        private readonly RestaurantCRMContext _context;

        public KhuyenMaiController(RestaurantCRMContext context)
        {
            _context = context;
        }

        // GET: api/KhuyenMai/Vouchers - Lấy danh sách Voucher hợp lệ
        [HttpGet("Vouchers")]
        public async Task<IActionResult> GetVouchers()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var vouchers = await _context.ChuongTrinhKhuyenMais
                .Where(k => k.TrangThai == "Active" 
                         && k.LoaiKhuyenMai == "Voucher"
                         && (k.NgayBatDau == null || k.NgayBatDau <= today)
                         && (k.NgayKetThuc == null || k.NgayKetThuc >= today)
                         && (k.SoLuong == null || k.SoLuong > 0))
                .Select(k => new
                {
                    k.MaChuongTrinh,
                    k.TenChuongTrinh,
                    k.LoaiGiam,
                    k.GiaTriGiam,
                    k.GiaTriDonToiThieu,
                    k.NgayBatDau,
                    k.NgayKetThuc,
                    k.SoLuong
                })
                .ToListAsync();

            return Ok(vouchers);
        }

        // GET: api/KhuyenMai/GiamGiaMon - Lấy danh sách khuyến mãi món hợp lệ
        [HttpGet("GiamGiaMon")]
        public async Task<IActionResult> GetGiamGiaMon()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var giamGiaMons = await _context.ChuongTrinhKhuyenMais
                .Include(k => k.ChiTietKhuyenMaiMons)
                .Where(k => k.TrangThai == "Active" 
                         && k.LoaiKhuyenMai == "GiamGiaMon"
                         && (k.NgayBatDau == null || k.NgayBatDau <= today)
                         && (k.NgayKetThuc == null || k.NgayKetThuc >= today)
                         && (k.SoLuong == null || k.SoLuong > 0))
                .Select(k => new
                {
                    k.MaChuongTrinh,
                    k.TenChuongTrinh,
                    k.LoaiGiam,
                    k.GiaTriGiam,
                    k.NgayBatDau,
                    k.NgayKetThuc,
                    k.SoLuong,
                    DanhSachMon = k.ChiTietKhuyenMaiMons.Select(c => new
                    {
                        c.MaMon,
                        c.SoLuongApDung
                    })
                })
                .ToListAsync();

            return Ok(giamGiaMons);
        }
    }
}

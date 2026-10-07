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
        private readonly RestaurantCrmContext _context;

        public KhuyenMaiController(RestaurantCrmContext context)
        {
            _context = context;
        }

        // GET: api/KhuyenMai/Vouchers - Lấy danh sách Voucher hợp lệ
        [HttpGet("Vouchers")]
        public async Task<IActionResult> GetVouchers()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var vouchers = await _context.KmTheoVoucher
                .Include(v => v.MaChuongTrinhNavigation)
                .Where(v => v.MaChuongTrinhNavigation != null 
                         && (v.MaChuongTrinhNavigation.NgayBatDau == null || v.MaChuongTrinhNavigation.NgayBatDau <= today)
                         && (v.MaChuongTrinhNavigation.NgayKetThuc == null || v.MaChuongTrinhNavigation.NgayKetThuc >= today))
                .Select(v => new
                {
                    v.MaKmvoucher,
                    v.MaVoucher,
                    TenChuongTrinh = v.MaChuongTrinhNavigation.TenChuongTrinh,
                    v.PhanTramGiam,
                    v.TienGiam,
                    v.GiaTriDonToiThieu,
                    v.MaChuongTrinhNavigation.NgayBatDau,
                    v.MaChuongTrinhNavigation.NgayKetThuc
                })
                .ToListAsync();

            return Ok(vouchers);
        }

        // GET: api/KhuyenMai/GiamGiaMon - Lấy danh sách khuyến mãi món hợp lệ
        [HttpGet("GiamGiaMon")]
        public async Task<IActionResult> GetGiamGiaMon()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var giamGiaMons = await _context.KmTheoSp
                .Include(k => k.MaChuongTrinhNavigation)
                .Where(k => k.MaChuongTrinhNavigation != null 
                         && (k.MaChuongTrinhNavigation.NgayBatDau == null || k.MaChuongTrinhNavigation.NgayBatDau <= today)
                         && (k.MaChuongTrinhNavigation.NgayKetThuc == null || k.MaChuongTrinhNavigation.NgayKetThuc >= today))
                .Select(k => new
                {
                    k.MaKmsp,
                    TenChuongTrinh = k.MaChuongTrinhNavigation.TenChuongTrinh,
                    k.PhanTramGiam,
                    k.TienGiam,
                    k.MaChuongTrinhNavigation.NgayBatDau,
                    k.MaChuongTrinhNavigation.NgayKetThuc,
                    k.MaMon
                })
                .ToListAsync();

            return Ok(giamGiaMons);
        }
    }
}

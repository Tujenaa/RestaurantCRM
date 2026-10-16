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
                    v.GiamToiDa,
                    v.MaChuongTrinhNavigation.NgayBatDau,
                    v.MaChuongTrinhNavigation.NgayKetThuc
                })
                .ToListAsync();

            return Ok(vouchers);
        }

        // GET: api/KhuyenMai/KiemTraVoucher?code=...&subtotal=...
        [HttpGet("KiemTraVoucher")]
        public async Task<IActionResult> KiemTraVoucher([FromQuery] string code, [FromQuery] double subtotal)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest(new { valid = false, message = "Vui lòng nhập mã giảm giá." });

            var today = DateOnly.FromDateTime(DateTime.Now);
            var voucher = await _context.KmTheoVoucher
                .Include(v => v.MaChuongTrinhNavigation)
                .FirstOrDefaultAsync(v => v.MaVoucher == code.Trim().ToUpper() || v.MaKmvoucher == code.Trim().ToUpper());

            if (voucher == null || voucher.MaChuongTrinhNavigation == null)
                return Ok(new { valid = false, message = "Mã voucher không tồn tại." });

            var ct = voucher.MaChuongTrinhNavigation;
            if (ct.NgayBatDau.HasValue && today < ct.NgayBatDau.Value)
                return Ok(new { valid = false, message = "Chương trình khuyến mãi chưa bắt đầu." });

            if (ct.NgayKetThuc.HasValue && today > ct.NgayKetThuc.Value)
                return Ok(new { valid = false, message = "Mã voucher đã hết hạn sử dụng." });

            if (voucher.GiaTriDonToiThieu.HasValue && subtotal < voucher.GiaTriDonToiThieu.Value)
                return Ok(new { valid = false, message = $"Mã này chỉ áp dụng cho đơn hàng từ {voucher.GiaTriDonToiThieu.Value:N0} đ trở lên." });

            double discount = 0;
            if (voucher.PhanTramGiam.HasValue && voucher.PhanTramGiam.Value > 0)
            {
                discount = subtotal * voucher.PhanTramGiam.Value / 100.0;
                if (voucher.GiamToiDa.HasValue && voucher.GiamToiDa.Value > 0 && discount > voucher.GiamToiDa.Value)
                {
                    discount = voucher.GiamToiDa.Value;
                }
            }
            else if (voucher.TienGiam.HasValue && voucher.TienGiam.Value > 0)
            {
                discount = Math.Min(subtotal, voucher.TienGiam.Value);
            }

            return Ok(new
            {
                valid = true,
                maVoucher = voucher.MaVoucher,
                maKmvoucher = voucher.MaKmvoucher,
                tenChuongTrinh = ct.TenChuongTrinh,
                phanTramGiam = voucher.PhanTramGiam,
                tienGiam = voucher.TienGiam,
                giamToiDa = voucher.GiamToiDa,
                giaTriGiamThucTe = discount,
                tongTienSauGiam = Math.Max(0, subtotal - discount),
                message = $"Áp dụng thành công! Giảm {discount:N0} đ."
            });
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

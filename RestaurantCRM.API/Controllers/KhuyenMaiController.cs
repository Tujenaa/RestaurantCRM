using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhuyenMaiController : ControllerBase
    {
        private readonly RestaurantCRM.BusinessLogic.Services.IRestaurantService _service;

        public KhuyenMaiController(RestaurantCRM.BusinessLogic.Services.IRestaurantService service)
        {
            _service = service;
        }

        // GET: api/KhuyenMai/Vouchers - Lấy danh sách Voucher hợp lệ
        [HttpGet("Vouchers")]
        public async Task<IActionResult> GetVouchers()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var vouchers = await _service.Query<KmTheoVoucher>()
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

        // GET: api/KhuyenMai/UsedVouchers/{maKhachHang} - Lấy danh sách voucher khách hàng đã từng sử dụng
        [HttpGet("UsedVouchers/{maKhachHang}")]
        public async Task<IActionResult> GetUsedVouchers(string maKhachHang)
        {
            if (string.IsNullOrWhiteSpace(maKhachHang))
            {
                return Ok(new List<string>());
            }

            var usedOrders = await _service.Query<HoaDon>()
                .Where(h => h.MaKhachHang == maKhachHang && !string.IsNullOrEmpty(h.MaKmvoucher) && h.TrangThai != "Cancelled")
                .Include(h => h.MaKmvoucherNavigation)
                .Select(h => new
                {
                    h.MaKmvoucher,
                    MaVoucher = h.MaKmvoucherNavigation != null ? h.MaKmvoucherNavigation.MaVoucher : null
                })
                .ToListAsync();

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in usedOrders)
            {
                if (!string.IsNullOrWhiteSpace(item.MaKmvoucher))
                    result.Add(item.MaKmvoucher.Trim().ToUpperInvariant());
                if (!string.IsNullOrWhiteSpace(item.MaVoucher))
                    result.Add(item.MaVoucher.Trim().ToUpperInvariant());
            }

            return Ok(result.ToList());
        }

        // GET: api/KhuyenMai/KiemTraVoucher?code=...&subtotal=...&maKhachHang=...
        [HttpGet("KiemTraVoucher")]
        public async Task<IActionResult> KiemTraVoucher([FromQuery] string code, [FromQuery] double subtotal, [FromQuery] string? maKhachHang = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest(new { valid = false, message = "Vui lòng nhập mã giảm giá." });

            var today = DateOnly.FromDateTime(DateTime.Now);
            var voucher = await _service.Query<KmTheoVoucher>()
                .Include(v => v.MaChuongTrinhNavigation)
                .FirstOrDefaultAsync(v => v.MaVoucher == code.Trim().ToUpper() || v.MaKmvoucher == code.Trim().ToUpper());

            if (voucher == null || voucher.MaChuongTrinhNavigation == null)
                return Ok(new { valid = false, message = "Mã voucher không tồn tại." });

            if (!string.IsNullOrWhiteSpace(maKhachHang))
            {
                var isUsed = await _service.Query<HoaDon>()
                    .AnyAsync(h => h.MaKhachHang == maKhachHang 
                                && (h.MaKmvoucher == voucher.MaKmvoucher || (voucher.MaVoucher != null && h.MaKmvoucher == voucher.MaVoucher)) 
                                && h.TrangThai != "Cancelled");
                if (isUsed)
                {
                    return Ok(new { valid = false, message = $"Bạn đã từng sử dụng mã {code.Trim().ToUpper()} này rồi. Mỗi khách hàng chỉ được áp dụng 1 lần." });
                }
            }

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

            var giamGiaMons = await _service.Query<KmTheoSp>()
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

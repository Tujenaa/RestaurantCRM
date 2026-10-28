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
                .Include(v => v.MaLoaiMonNavigation)
                .Include(v => v.MaMonNavigation)
                .Include(v => v.HoaDon)
                .Where(v => v.MaChuongTrinhNavigation != null 
                         && (v.ChoPhepCongDon == null || v.ChoPhepCongDon == true)
                         && (v.MaVoucher == null || !v.MaVoucher.StartsWith("[DA_HUY]_"))
                         && ((v.PhanTramGiam.HasValue && v.PhanTramGiam > 0) || (v.TienGiam.HasValue && v.TienGiam > 0))
                         && (!v.SoLuongToiThieu.HasValue || v.SoLuongToiThieu <= 0 || v.HoaDon.Count(h => h.TrangThai != "Cancelled") < v.SoLuongToiThieu.Value)
                         && (v.MaChuongTrinhNavigation.NgayBatDau == null || v.MaChuongTrinhNavigation.NgayBatDau <= today)
                         && (v.MaChuongTrinhNavigation.NgayKetThuc == null || v.MaChuongTrinhNavigation.NgayKetThuc >= today))
                .Select(v => new
                {
                    v.MaKmvoucher,
                    v.MaVoucher,
                    TenChuongTrinh = v.MaChuongTrinhNavigation!.TenChuongTrinh,
                    v.PhanTramGiam,
                    v.TienGiam,
                    v.GiaTriDonToiThieu,
                    v.GiamToiDa,
                    v.PhamViApDung,
                    v.MaLoaiMon,
                    TenLoaiMon = v.MaLoaiMonNavigation != null ? v.MaLoaiMonNavigation.TenLoaiMon : null,
                    v.MaMon,
                    TenMon = v.MaMonNavigation != null ? v.MaMonNavigation.TenMon : null,
                    v.SoLuongToiThieu,
                    v.ChoPhepCongDon,
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
            var voucher = await _service.Query<KmTheoVoucher>()
                .Include(v => v.MaChuongTrinhNavigation)
                .Include(v => v.MaLoaiMonNavigation)
                .Include(v => v.MaMonNavigation)
                .Include(v => v.HoaDon)
                .FirstOrDefaultAsync(v => v.MaVoucher == code.Trim().ToUpper() || v.MaKmvoucher == code.Trim().ToUpper());

            if (voucher == null || voucher.MaChuongTrinhNavigation == null)
                return Ok(new { valid = false, message = "Mã voucher không tồn tại." });

            if (voucher.ChoPhepCongDon == false || (voucher.MaVoucher != null && voucher.MaVoucher.StartsWith("[DA_HUY]_")))
                return Ok(new { valid = false, message = "Mã voucher này đã bị ẩn hoặc hủy áp dụng." });

            if (voucher.SoLuongToiThieu.HasValue && voucher.SoLuongToiThieu.Value > 0)
            {
                int daDung = voucher.HoaDon.Count(h => h.TrangThai != "Cancelled");
                if (daDung >= voucher.SoLuongToiThieu.Value)
                {
                    return Ok(new { valid = false, message = "Mã voucher này đã hết lượt sử dụng." });
                }
            }

            if ((!voucher.PhanTramGiam.HasValue || voucher.PhanTramGiam <= 0) && (!voucher.TienGiam.HasValue || voucher.TienGiam <= 0))
                return Ok(new { valid = false, message = "Voucher không còn hiệu lực giảm giá." });

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
                giaTriDonToiThieu = voucher.GiaTriDonToiThieu,
                giamToiDa = voucher.GiamToiDa,
                phamViApDung = voucher.PhamViApDung ?? "TOAN_DON",
                maLoaiMon = voucher.MaLoaiMon,
                tenLoaiMon = voucher.MaLoaiMonNavigation?.TenLoaiMon,
                maMon = voucher.MaMon,
                tenMon = voucher.MaMonNavigation?.TenMon,
                soLuongToiThieu = voucher.SoLuongToiThieu ?? 1,
                choPhepCongDon = voucher.ChoPhepCongDon ?? true,
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
                         && ((k.PhanTramGiam.HasValue && k.PhanTramGiam > 0) || (k.TienGiam.HasValue && k.TienGiam > 0))
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

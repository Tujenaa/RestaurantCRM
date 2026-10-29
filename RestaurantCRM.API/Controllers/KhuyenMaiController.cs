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

        // GET: api/KhuyenMai/Vouchers - Lấy danh sách Voucher hợp lệ từ cả 3 bảng
        [HttpGet("Vouchers")]
        public async Task<IActionResult> GetVouchers()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            // 1. Voucher toàn đơn hàng
            var vDon = await _service.Query<KmTheoDonHang>()
                .Include(v => v.MaChuongTrinhNavigation)
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
                    MaKmvoucher = v.MaKMDonHang,
                    v.MaVoucher,
                    TenChuongTrinh = v.MaChuongTrinhNavigation.TenChuongTrinh,
                    v.PhanTramGiam,
                    v.TienGiam,
                    v.GiaTriDonToiThieu,
                    v.GiamToiDa,
                    PhamViApDung = "TOAN_DON",
                    MaLoaiMon = (string?)null,
                    TenLoaiMon = (string?)null,
                    MaMon = (string?)null,
                    TenMon = (string?)null,
                    v.SoLuongToiThieu,
                    v.ChoPhepCongDon,
                    v.MaChuongTrinhNavigation.NgayBatDau,
                    v.MaChuongTrinhNavigation.NgayKetThuc
                })
                .ToListAsync();

            // 2. Voucher theo loại món
            var vLoai = await _service.Query<KmTheoLoai>()
                .Include(v => v.MaChuongTrinhNavigation)
                .Include(v => v.MaLoaiMonNavigation)
                .Include(v => v.ChiTietHoaDon)
                .Where(v => v.MaChuongTrinhNavigation != null
                         && (v.ChoPhepCongDon == null || v.ChoPhepCongDon == true)
                         && (v.MaVoucher == null || !v.MaVoucher.StartsWith("[DA_HUY]_"))
                         && ((v.PhanTramGiam.HasValue && v.PhanTramGiam > 0) || (v.TienGiam.HasValue && v.TienGiam > 0))
                         && (!v.SoLuongToiThieu.HasValue || v.SoLuongToiThieu <= 0 || v.ChiTietHoaDon.Count(c => c.MaHoaDonNavigation != null && c.MaHoaDonNavigation.TrangThai != "Cancelled") < v.SoLuongToiThieu.Value)
                         && (v.MaChuongTrinhNavigation.NgayBatDau == null || v.MaChuongTrinhNavigation.NgayBatDau <= today)
                         && (v.MaChuongTrinhNavigation.NgayKetThuc == null || v.MaChuongTrinhNavigation.NgayKetThuc >= today))
                .Select(v => new
                {
                    MaKmvoucher = v.MaKMLoai,
                    v.MaVoucher,
                    TenChuongTrinh = v.MaChuongTrinhNavigation.TenChuongTrinh,
                    v.PhanTramGiam,
                    v.TienGiam,
                    v.GiaTriDonToiThieu,
                    v.GiamToiDa,
                    PhamViApDung = "THEO_LOAI",
                    v.MaLoaiMon,
                    TenLoaiMon = v.MaLoaiMonNavigation != null ? v.MaLoaiMonNavigation.TenLoaiMon : null,
                    MaMon = (string?)null,
                    TenMon = (string?)null,
                    v.SoLuongToiThieu,
                    v.ChoPhepCongDon,
                    v.MaChuongTrinhNavigation.NgayBatDau,
                    v.MaChuongTrinhNavigation.NgayKetThuc
                })
                .ToListAsync();

            // 3. Voucher theo món ăn
            var vMon = await _service.Query<KmTheoSp>()
                .Include(v => v.MaChuongTrinhNavigation)
                .Include(v => v.MaMonNavigation)
                .Include(v => v.ChiTietHoaDon)
                .Where(v => v.MaChuongTrinhNavigation != null
                         && !string.IsNullOrEmpty(v.MaVoucher)
                         && (v.ChoPhepCongDon == null || v.ChoPhepCongDon == true)
                         && !v.MaVoucher.StartsWith("[DA_HUY]_")
                         && ((v.PhanTramGiam.HasValue && v.PhanTramGiam > 0) || (v.TienGiam.HasValue && v.TienGiam > 0))
                         && (!v.SoLuongToiThieu.HasValue || v.SoLuongToiThieu <= 0 || v.ChiTietHoaDon.Count(c => c.MaHoaDonNavigation != null && c.MaHoaDonNavigation.TrangThai != "Cancelled") < v.SoLuongToiThieu.Value)
                         && (v.MaChuongTrinhNavigation.NgayBatDau == null || v.MaChuongTrinhNavigation.NgayBatDau <= today)
                         && (v.MaChuongTrinhNavigation.NgayKetThuc == null || v.MaChuongTrinhNavigation.NgayKetThuc >= today))
                .Select(v => new
                {
                    MaKmvoucher = v.MaKmsp,
                    v.MaVoucher,
                    TenChuongTrinh = v.MaChuongTrinhNavigation.TenChuongTrinh,
                    v.PhanTramGiam,
                    v.TienGiam,
                    GiaTriDonToiThieu = (double?)null,
                    v.GiamToiDa,
                    PhamViApDung = "THEO_MON",
                    MaLoaiMon = (string?)null,
                    TenLoaiMon = (string?)null,
                    v.MaMon,
                    TenMon = v.MaMonNavigation != null ? v.MaMonNavigation.TenMon : null,
                    v.SoLuongToiThieu,
                    v.ChoPhepCongDon,
                    v.MaChuongTrinhNavigation.NgayBatDau,
                    v.MaChuongTrinhNavigation.NgayKetThuc
                })
                .ToListAsync();

            var all = vDon.Concat(vLoai).Concat(vMon).ToList();
            return Ok(all);
        }

        // GET: api/KhuyenMai/KiemTraVoucher?code=...&subtotal=...
        [HttpGet("KiemTraVoucher")]
        public async Task<IActionResult> KiemTraVoucher([FromQuery] string code, [FromQuery] double subtotal)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest(new { valid = false, message = "Vui lòng nhập mã giảm giá." });

            var today = DateOnly.FromDateTime(DateTime.Now);
            var searchCode = code.Trim().ToUpper();

            // 1. Tìm trong KM_THEO_DON_HANG
            var vDon = await _service.Query<KmTheoDonHang>()
                .Include(v => v.MaChuongTrinhNavigation)
                .Include(v => v.HoaDon)
                .FirstOrDefaultAsync(v => v.MaVoucher == searchCode || v.MaKMDonHang == searchCode);

            if (vDon != null)
            {
                var ct = vDon.MaChuongTrinhNavigation;
                if (ct == null) return Ok(new { valid = false, message = "Chương trình khuyến mãi không tồn tại." });
                if (vDon.ChoPhepCongDon == false || (vDon.MaVoucher != null && vDon.MaVoucher.StartsWith("[DA_HUY]_")))
                    return Ok(new { valid = false, message = "Mã voucher này đã bị ẩn hoặc hủy áp dụng." });

                if (vDon.SoLuongToiThieu.HasValue && vDon.SoLuongToiThieu.Value > 0)
                {
                    int daDung = vDon.HoaDon.Count(h => h.TrangThai != "Cancelled");
                    if (daDung >= vDon.SoLuongToiThieu.Value)
                        return Ok(new { valid = false, message = "Mã voucher này đã hết lượt sử dụng." });
                }

                if ((!vDon.PhanTramGiam.HasValue || vDon.PhanTramGiam <= 0) && (!vDon.TienGiam.HasValue || vDon.TienGiam <= 0))
                    return Ok(new { valid = false, message = "Voucher không còn hiệu lực giảm giá." });

                if (ct.NgayBatDau.HasValue && today < ct.NgayBatDau.Value)
                    return Ok(new { valid = false, message = "Chương trình khuyến mãi chưa bắt đầu." });

                if (ct.NgayKetThuc.HasValue && today > ct.NgayKetThuc.Value)
                    return Ok(new { valid = false, message = "Mã voucher đã hết hạn sử dụng." });

                if (vDon.GiaTriDonToiThieu.HasValue && subtotal < vDon.GiaTriDonToiThieu.Value)
                    return Ok(new { valid = false, message = $"Mã này chỉ áp dụng cho đơn hàng từ {vDon.GiaTriDonToiThieu.Value:N0} đ trở lên." });

                double discount = 0;
                if (vDon.PhanTramGiam.HasValue && vDon.PhanTramGiam.Value > 0)
                {
                    discount = subtotal * vDon.PhanTramGiam.Value / 100.0;
                    if (vDon.GiamToiDa.HasValue && vDon.GiamToiDa.Value > 0 && discount > vDon.GiamToiDa.Value)
                        discount = vDon.GiamToiDa.Value;
                }
                else if (vDon.TienGiam.HasValue && vDon.TienGiam.Value > 0)
                {
                    discount = Math.Min(subtotal, vDon.TienGiam.Value);
                }

                return Ok(new
                {
                    valid = true,
                    maVoucher = vDon.MaVoucher,
                    maKmvoucher = vDon.MaKMDonHang,
                    tenChuongTrinh = ct.TenChuongTrinh,
                    phanTramGiam = vDon.PhanTramGiam,
                    tienGiam = vDon.TienGiam,
                    giaTriDonToiThieu = vDon.GiaTriDonToiThieu,
                    giamToiDa = vDon.GiamToiDa,
                    phamViApDung = "TOAN_DON",
                    maLoaiMon = (string?)null,
                    tenLoaiMon = (string?)null,
                    maMon = (string?)null,
                    tenMon = (string?)null,
                    soLuongToiThieu = vDon.SoLuongToiThieu ?? 1,
                    choPhepCongDon = vDon.ChoPhepCongDon ?? true,
                    giaTriGiamThucTe = discount,
                    tongTienSauGiam = Math.Max(0, subtotal - discount),
                    message = $"Áp dụng thành công! Giảm {discount:N0} đ."
                });
            }

            // 2. Tìm trong KM_THEO_LOAI
            var vLoai = await _service.Query<KmTheoLoai>()
                .Include(v => v.MaChuongTrinhNavigation)
                .Include(v => v.MaLoaiMonNavigation)
                .Include(v => v.ChiTietHoaDon)
                .FirstOrDefaultAsync(v => v.MaVoucher == searchCode || v.MaKMLoai == searchCode);

            if (vLoai != null)
            {
                var ct = vLoai.MaChuongTrinhNavigation;
                if (ct == null) return Ok(new { valid = false, message = "Chương trình khuyến mãi không tồn tại." });
                if (vLoai.ChoPhepCongDon == false || (vLoai.MaVoucher != null && vLoai.MaVoucher.StartsWith("[DA_HUY]_")))
                    return Ok(new { valid = false, message = "Mã voucher này đã bị ẩn hoặc hủy áp dụng." });

                if (vLoai.SoLuongToiThieu.HasValue && vLoai.SoLuongToiThieu.Value > 0)
                {
                    int daDung = vLoai.ChiTietHoaDon.Count(c => c.MaHoaDonNavigation != null && c.MaHoaDonNavigation.TrangThai != "Cancelled");
                    if (daDung >= vLoai.SoLuongToiThieu.Value)
                        return Ok(new { valid = false, message = "Mã voucher này đã hết lượt sử dụng." });
                }

                if ((!vLoai.PhanTramGiam.HasValue || vLoai.PhanTramGiam <= 0) && (!vLoai.TienGiam.HasValue || vLoai.TienGiam <= 0))
                    return Ok(new { valid = false, message = "Voucher không còn hiệu lực giảm giá." });

                if (ct.NgayBatDau.HasValue && today < ct.NgayBatDau.Value)
                    return Ok(new { valid = false, message = "Chương trình khuyến mãi chưa bắt đầu." });

                if (ct.NgayKetThuc.HasValue && today > ct.NgayKetThuc.Value)
                    return Ok(new { valid = false, message = "Mã voucher đã hết hạn sử dụng." });

                return Ok(new
                {
                    valid = true,
                    maVoucher = vLoai.MaVoucher,
                    maKmvoucher = vLoai.MaKMLoai,
                    tenChuongTrinh = ct.TenChuongTrinh,
                    phanTramGiam = vLoai.PhanTramGiam,
                    tienGiam = vLoai.TienGiam,
                    giaTriDonToiThieu = vLoai.GiaTriDonToiThieu,
                    giamToiDa = vLoai.GiamToiDa,
                    phamViApDung = "THEO_LOAI",
                    maLoaiMon = vLoai.MaLoaiMon,
                    tenLoaiMon = vLoai.MaLoaiMonNavigation?.TenLoaiMon,
                    maMon = (string?)null,
                    tenMon = (string?)null,
                    soLuongToiThieu = vLoai.SoLuongToiThieu ?? 1,
                    choPhepCongDon = vLoai.ChoPhepCongDon ?? true,
                    giaTriGiamThucTe = 0,
                    tongTienSauGiam = subtotal,
                    message = $"Mã voucher hợp lệ áp dụng cho nhóm món '{vLoai.MaLoaiMonNavigation?.TenLoaiMon ?? vLoai.MaLoaiMon}'."
                });
            }

            // 3. Tìm trong KM_THEO_SP
            var vMonItem = await _service.Query<KmTheoSp>()
                .Include(v => v.MaChuongTrinhNavigation)
                .Include(v => v.MaMonNavigation)
                .Include(v => v.ChiTietHoaDon)
                .FirstOrDefaultAsync(v => v.MaVoucher == searchCode || v.MaKmsp == searchCode);

            if (vMonItem != null)
            {
                var ct = vMonItem.MaChuongTrinhNavigation;
                if (ct == null) return Ok(new { valid = false, message = "Chương trình khuyến mãi không tồn tại." });
                if (vMonItem.ChoPhepCongDon == false || (vMonItem.MaVoucher != null && vMonItem.MaVoucher.StartsWith("[DA_HUY]_")))
                    return Ok(new { valid = false, message = "Mã voucher này đã bị ẩn hoặc hủy áp dụng." });

                if (vMonItem.SoLuongToiThieu.HasValue && vMonItem.SoLuongToiThieu.Value > 0)
                {
                    int daDung = vMonItem.ChiTietHoaDon.Count(c => c.MaHoaDonNavigation != null && c.MaHoaDonNavigation.TrangThai != "Cancelled");
                    if (daDung >= vMonItem.SoLuongToiThieu.Value)
                        return Ok(new { valid = false, message = "Mã voucher này đã hết lượt sử dụng." });
                }

                if ((!vMonItem.PhanTramGiam.HasValue || vMonItem.PhanTramGiam <= 0) && (!vMonItem.TienGiam.HasValue || vMonItem.TienGiam <= 0))
                    return Ok(new { valid = false, message = "Voucher không còn hiệu lực giảm giá." });

                if (ct.NgayBatDau.HasValue && today < ct.NgayBatDau.Value)
                    return Ok(new { valid = false, message = "Chương trình khuyến mãi chưa bắt đầu." });

                if (ct.NgayKetThuc.HasValue && today > ct.NgayKetThuc.Value)
                    return Ok(new { valid = false, message = "Mã voucher đã hết hạn sử dụng." });

                return Ok(new
                {
                    valid = true,
                    maVoucher = vMonItem.MaVoucher,
                    maKmvoucher = vMonItem.MaKmsp,
                    tenChuongTrinh = ct.TenChuongTrinh,
                    phanTramGiam = vMonItem.PhanTramGiam,
                    tienGiam = vMonItem.TienGiam,
                    giamToiDa = vMonItem.GiamToiDa,
                    phamViApDung = "THEO_MON",
                    maLoaiMon = (string?)null,
                    tenLoaiMon = (string?)null,
                    maMon = vMonItem.MaMon,
                    tenMon = vMonItem.MaMonNavigation?.TenMon,
                    soLuongToiThieu = vMonItem.SoLuongToiThieu ?? 1,
                    choPhepCongDon = vMonItem.ChoPhepCongDon ?? true,
                    giaTriGiamThucTe = 0,
                    tongTienSauGiam = subtotal,
                    message = $"Mã voucher hợp lệ áp dụng cho món '{vMonItem.MaMonNavigation?.TenMon ?? vMonItem.MaMon}'."
                });
            }

            return Ok(new { valid = false, message = "Mã voucher không tồn tại." });
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

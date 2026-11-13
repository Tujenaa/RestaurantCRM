using Microsoft.AspNetCore.Mvc;

namespace RestaurantCRM.Web.Helpers;

/// <summary>
/// Xử lý khi API báo tài khoản khách đã bị khóa: xóa Session và đưa khách về trang đăng nhập.
/// </summary>
public static class TaiKhoanBiKhoaHelper
{
    /// <summary>Thông báo API trả về khi khách bị khóa (KhachHangValidator.TaiKhoanBiKhoa bên API).</summary>
    public const string ThongBao = "Tài khoản đã bị khóa.";

    /// <summary>Lỗi từ API có phải là "tài khoản đã bị khóa" không (bỏ qua dấu nháy JSON, khoảng trắng, tiền tố).</summary>
    public static bool LaTaiKhoanBiKhoa(string? loi) =>
        !string.IsNullOrWhiteSpace(loi) && loi.Trim().Trim('"').EndsWith(ThongBao, StringComparison.Ordinal);

    /// <summary>Xóa Session và chuyển về trang đăng nhập kèm thông báo.</summary>
    public static IActionResult DangXuatVaVeDangNhap(this Controller controller)
    {
        controller.HttpContext.Session.Clear();
        controller.TempData["ThongBao"] = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ nhà hàng để được hỗ trợ.";
        return controller.RedirectToAction("DangNhap", "TaiKhoan");
    }
}

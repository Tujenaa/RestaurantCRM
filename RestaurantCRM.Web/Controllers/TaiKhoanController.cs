using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;

namespace RestaurantCRM.Web.Controllers;

public class TaiKhoanController : Controller
{
    [HttpGet]
    public IActionResult DangNhap() => View("Login");

    [HttpGet]
    public IActionResult Index(string tab = "login")
    {
        ViewBag.Tab = tab;
        return View(new KhachHangViewModel { HoTen = "Nguyễn Văn An", SoDienThoai = "0901234567", Email = "a.nguyen@email.com", GioiTinh = "Nam", DiaChi = "12 Lê Lợi, Quận 1, TP. Hồ Chí Minh", SoDonHang = 12 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DangNhap(string lienHe, string matKhau)
    {
        TempData["ThongBao"] = "Đây là giao diện minh họa; đăng nhập chưa kết nối API xác thực.";
        return RedirectToAction(nameof(Index), new { tab = "login" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DangKy(string hoTen, string soDienThoai, string? email, string matKhau)
    {
        if (String.IsNullOrWhiteSpace(hoTen) || String.IsNullOrWhiteSpace(soDienThoai) || String.IsNullOrWhiteSpace(matKhau))
            TempData["ThongBao"] = "Vui lòng nhập họ tên, số điện thoại và mật khẩu.";
        else
            TempData["ThongBao"] = "Thông tin đã được nhận trong giao diện demo; chưa tạo tài khoản trên máy chủ.";
        return RedirectToAction(nameof(Index), new { tab = "register" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CapNhat(KhachHangViewModel model)
    {
        TempData["ThongBao"] = "Thông tin chỉ được hiển thị trong bản demo; chưa lưu lên máy chủ.";
        return RedirectToAction(nameof(Index), new { tab = "profile" });
    }
}

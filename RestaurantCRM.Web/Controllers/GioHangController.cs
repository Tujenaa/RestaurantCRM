using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;
using RestaurantCRM.Web.Services;

namespace RestaurantCRM.Web.Controllers;

public class GioHangController(GioHangService gioHangService, DonHangService donHangService) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(BuildPage());

    [HttpGet]
    public IActionResult Checkout()
    {
        var cart = gioHangService.Get(HttpContext.Session);
        if (cart.Items.Count == 0) return RedirectToAction(nameof(Index));
        return View(new CheckoutPageViewModel { GioHang = cart });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Them(ThemGioHangViewModel model)
    {
        if (ModelState.IsValid) gioHangService.Add(HttpContext.Session, model.MaMon, model.SoLuong);
        TempData["ThongBao"] = "Đã cập nhật giỏ hàng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CapNhat(CapNhatSoLuongViewModel model)
    {
        if (ModelState.IsValid) gioHangService.SetQuantity(HttpContext.Session, model.MaMon, model.SoLuong);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Voucher(ApDungVoucherViewModel model)
    {
        var ok = gioHangService.ApplyVoucher(HttpContext.Session, model.MaVoucher);
        TempData["ThongBao"] = ok ? "Đã áp dụng voucher LAUPHO20." : "Mã voucher không hợp lệ. Dùng LAUPHO20 để nhận giảm 20.000đ.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DatHang(CheckoutPageViewModel page)
    {
        var model = page.DatHang;
        var cart = gioHangService.Get(HttpContext.Session);
        if (cart.Items.Count == 0)
        {
            TempData["ThongBao"] = "Giỏ hàng đang trống.";
            return RedirectToAction(nameof(Index));
        }
        if (!ModelState.IsValid) return View("Checkout", new CheckoutPageViewModel { GioHang = cart, DatHang = model });

        var id = donHangService.Create(model, cart.TongThanhToan, cart.Items);
        gioHangService.Clear(HttpContext.Session);
        TempData["DonHangMoi"] = id;
        return RedirectToAction("Index", "DonHang");
    }

    private GioHangPageViewModel BuildPage() => new() { GioHang = gioHangService.Get(HttpContext.Session) };
}

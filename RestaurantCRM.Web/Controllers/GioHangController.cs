using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;
using RestaurantCRM.Web.Services;

namespace RestaurantCRM.Web.Controllers;

public class GioHangController : Controller
{
    private readonly GioHangService _gioHangService;
    private readonly DonHangService _donHangService;

    public GioHangController(GioHangService gioHangService, DonHangService donHangService)
    {
        _gioHangService = gioHangService;
        _donHangService = donHangService;
    }

    [HttpGet]
    public IActionResult Index() => View(BuildPage());

    [HttpGet]
    public IActionResult Checkout()
    {
        var token = HttpContext.Session.GetString("Token");
        if (string.IsNullOrEmpty(token))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập tài khoản trước khi tiến hành đặt hàng và thanh toán.";
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var cart = _gioHangService.Get(HttpContext.Session);
        if (cart.Items.Count == 0) return RedirectToAction(nameof(Index));

        // Pre-fill user information if logged in
        var hoTen = HttpContext.Session.GetString("HoTen") ?? "";
        var phone = HttpContext.Session.GetString("SoDienThoai") ?? "";

        var datHang = new DatHangViewModel
        {
            HoTen = hoTen,
            SoDienThoai = phone
        };

        return View(new CheckoutPageViewModel { GioHang = cart, DatHang = datHang });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Them(ThemGioHangViewModel model, [FromHeader(Name = "Referer")] string referer)
    {
        if (ModelState.IsValid)
        {
            _gioHangService.Add(HttpContext.Session, model.MaMon, model.SoLuong);
            TempData["ThongBao"] = "Đã thêm món vào giỏ hàng.";
        }
        
        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        if (isAjax)
        {
            TempData.Remove("ThongBao");
            var cart = _gioHangService.Get(HttpContext.Session);
            var cartCount = cart.Items.Sum(x => x.SoLuong);
            return Json(new { success = true, message = "Đã thêm món vào giỏ hàng.", cartCount = cartCount });
        }

        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CapNhat(CapNhatSoLuongViewModel model)
    {
        if (ModelState.IsValid)
        {
            _gioHangService.SetQuantity(HttpContext.Session, model.MaMon, model.SoLuong);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Voucher(ApDungVoucherViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.MaVoucher))
        {
            _gioHangService.ApplyVoucher(HttpContext.Session, null);
            TempData["ThongBao"] = "Đã gỡ mã ưu đãi khỏi đơn hàng.";
            TempData["IsError"] = false;
        }
        else
        {
            var result = _gioHangService.ApplyVoucher(HttpContext.Session, model.MaVoucher);
            TempData["ThongBao"] = result.Message;
            TempData["IsError"] = !result.Success;
        }

        if (string.Equals(model.ReturnUrl, "checkout", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Checkout");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DatHang(CheckoutPageViewModel page)
    {
        var token = HttpContext.Session.GetString("Token");
        if (string.IsNullOrEmpty(token))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập tài khoản trước khi tiến hành đặt hàng và thanh toán.";
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var model = page.DatHang;
        var cart = _gioHangService.Get(HttpContext.Session);

        if (cart.Items.Count == 0)
        {
            TempData["ThongBao"] = "Giỏ hàng đang trống, vui lòng chọn món trước khi đặt.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            return View("Checkout", new CheckoutPageViewModel { GioHang = cart, DatHang = model });
        }

        var customerId = HttpContext.Session.GetString("MaKhachHang") ?? "KH01";
        var voucherCode = cart.MaVoucher;

        var result = await _donHangService.CreateOrderAsync(model, customerId, voucherCode, cart.Items);

        if (!result.Success)
        {
            ModelState.AddModelError("", result.ErrorMessage ?? "Đặt hàng thất bại. Vui lòng kiểm tra lại số lượng hoặc món ăn trong giỏ.");
            ViewBag.ErrorMessage = result.ErrorMessage;
            return View("Checkout", new CheckoutPageViewModel { GioHang = cart, DatHang = model });
        }

        _gioHangService.Clear(HttpContext.Session);
        TempData["DonHangMoi"] = result.OrderId;

        if (!string.IsNullOrWhiteSpace(voucherCode))
        {
            var used = HttpContext.Session.GetString("UsedVouchers");
            HttpContext.Session.SetString("UsedVouchers", string.IsNullOrEmpty(used) ? voucherCode : used + "," + voucherCode);
        }

        var isOnline = string.Equals(model.PhuongThucThanhToan, "transfer", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(model.PhuongThucThanhToan, "online", StringComparison.OrdinalIgnoreCase);

        TempData["ThongBao"] = isOnline
            ? $"🎉 Đã thanh toán online thành công! Mã đơn của bạn là #{result.OrderId}. Nhà hàng đang chuẩn bị món ăn và tiến hành giao ngay!"
            : $"Đặt hàng thành công! Mã đơn của bạn là #{result.OrderId}. Bạn sẽ thanh toán khi nhận hàng (COD).";

        return RedirectToAction("Index", "DonHang");
    }

    private GioHangPageViewModel BuildPage() => new() { GioHang = _gioHangService.Get(HttpContext.Session) };
}

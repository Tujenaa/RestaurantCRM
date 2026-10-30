using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;
using RestaurantCRM.Web.Services;

namespace RestaurantCRM.Web.Controllers;

public class GioHangController : Controller
{
    private readonly GioHangService _gioHangService;
    private readonly DonHangService _donHangService;
    private readonly IHttpClientFactory _httpClientFactory;

    public GioHangController(GioHangService gioHangService, DonHangService donHangService, IHttpClientFactory httpClientFactory)
    {
        _gioHangService = gioHangService;
        _donHangService = donHangService;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await BuildPageAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var token = HttpContext.Session.GetString("Token");
        if (string.IsNullOrEmpty(token))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập tài khoản trước khi tiến hành đặt hàng và thanh toán.";
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var customerId = HttpContext.Session.GetString("MaKhachHang");
        var cart = _gioHangService.Get(HttpContext.Session, customerId);
        if (cart.Items.Count == 0) return RedirectToAction(nameof(Index));

        // Kiểm tra tính khả dụng & số lượng món ăn ngay từ bước tiến hành đặt hàng
        var availability = await _gioHangService.CheckCartAvailabilityAsync(cart.Items);
        if (!availability.IsValid)
        {
            TempData["ThongBao"] = availability.ErrorMessage;
            TempData["IsError"] = true;
            return RedirectToAction(nameof(Index));
        }

        // Pre-fill user information if logged in
        var hoTen = HttpContext.Session.GetString("HoTen") ?? "";
        var phone = HttpContext.Session.GetString("SoDienThoai") ?? "";
        var defaultAddress = HttpContext.Session.GetString("DiaChiGiaoHang") ?? "";

        if (string.IsNullOrWhiteSpace(defaultAddress))
        {
            try
            {
                if (!string.IsNullOrEmpty(customerId))
                {
                    var client = _httpClientFactory.CreateClient("ApiClient");
                    var response = await client.GetAsync($"/api/KhachHang/ThongTin/{Uri.EscapeDataString(customerId)}");
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonStr = await response.Content.ReadAsStringAsync();
                        using var doc = System.Text.Json.JsonDocument.Parse(jsonStr);
                        if (doc.RootElement.TryGetProperty("diaChi", out var pAddr) && pAddr.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            var addr = pAddr.GetString() ?? "";
                            if (!string.IsNullOrWhiteSpace(addr))
                            {
                                defaultAddress = addr;
                                HttpContext.Session.SetString("DiaChiGiaoHang", defaultAddress);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        ViewBag.DiaChiMacDinh = defaultAddress;

        var datHang = new DatHangViewModel
        {
            HoTen = hoTen,
            SoDienThoai = phone,
            DiaChiGiao = defaultAddress
        };

        var vouchers = await _gioHangService.GetAvailableVouchersAsync();
        return View(new CheckoutPageViewModel { GioHang = cart, DatHang = datHang, AvailableVouchers = vouchers });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Them(ThemGioHangViewModel model, [FromHeader(Name = "Referer")] string referer)
    {
        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        if (!ModelState.IsValid)
        {
            if (isAjax)
            {
                var cart = _gioHangService.Get(HttpContext.Session);
                return Json(new { success = false, message = "Thông tin món ăn không hợp lệ.", cartCount = cart.Items.Sum(x => x.SoLuong) });
            }
            if (!string.IsNullOrEmpty(referer)) return Redirect(referer);
            return RedirectToAction(nameof(Index));
        }

        _gioHangService.Add(HttpContext.Session, model.MaMon, model.SoLuong);
        TempData["ThongBao"] = "Đã thêm món vào giỏ hàng.";
        
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
    public async Task<IActionResult> Voucher(ApDungVoucherViewModel model)
    {
        var customerId = HttpContext.Session.GetString("MaKhachHang");
        if (string.IsNullOrWhiteSpace(model.MaVoucher))
        {
            await _gioHangService.ApplyVoucherAsync(HttpContext.Session, null, customerId);
            TempData["ThongBao"] = "Đã gỡ mã ưu đãi khỏi đơn hàng.";
            TempData["IsError"] = false;
        }
        else
        {
            var result = await _gioHangService.ApplyVoucherAsync(HttpContext.Session, model.MaVoucher, customerId);
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

        var customerId = HttpContext.Session.GetString("MaKhachHang");
        var model = page.DatHang;
        var cart = _gioHangService.Get(HttpContext.Session, customerId);

        if (cart.Items.Count == 0)
        {
            TempData["ThongBao"] = "Giỏ hàng đang trống, vui lòng chọn món trước khi đặt.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            var vouchers = await _gioHangService.GetAvailableVouchersAsync();
            return View("Checkout", new CheckoutPageViewModel { GioHang = cart, DatHang = model, AvailableVouchers = vouchers });
        }

        // Kiểm tra lại tính khả dụng & tồn kho trước khi gửi tạo đơn
        var availability = await _gioHangService.CheckCartAvailabilityAsync(cart.Items);
        if (!availability.IsValid)
        {
            ModelState.AddModelError("", availability.ErrorMessage ?? "Một số món trong giỏ hàng hiện không đủ số lượng để phục vụ.");
            ViewBag.ErrorMessage = availability.ErrorMessage;
            var vouchers = await _gioHangService.GetAvailableVouchersAsync();
            return View("Checkout", new CheckoutPageViewModel { GioHang = cart, DatHang = model, AvailableVouchers = vouchers });
        }

        string orderCustomerId = string.IsNullOrWhiteSpace(customerId) ? "KH01" : customerId;
        var voucherCode = cart.MaVoucher;

        var result = await _donHangService.CreateOrderAsync(model, orderCustomerId, voucherCode, cart.Items);

        if (!result.Success)
        {
            ModelState.AddModelError("", result.ErrorMessage ?? "Đặt hàng thất bại. Vui lòng kiểm tra lại số lượng hoặc món ăn trong giỏ.");
            ViewBag.ErrorMessage = result.ErrorMessage;
            var vouchers = await _gioHangService.GetAvailableVouchersAsync();
            return View("Checkout", new CheckoutPageViewModel { GioHang = cart, DatHang = model, AvailableVouchers = vouchers });
        }

        _gioHangService.Clear(HttpContext.Session, orderCustomerId);
        TempData["DonHangMoi"] = result.OrderId;

        var isOnline = string.Equals(model.PhuongThucThanhToan, "transfer", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(model.PhuongThucThanhToan, "online", StringComparison.OrdinalIgnoreCase);

        TempData["ThongBao"] = isOnline
            ? $"🎉 Đã thanh toán online thành công! Mã đơn của bạn là #{result.OrderId}. Nhà hàng đang chuẩn bị món ăn và tiến hành giao ngay!"
            : $"Đặt hàng thành công! Mã đơn của bạn là #{result.OrderId}. Bạn sẽ thanh toán khi nhận hàng (COD).";

        return RedirectToAction("Index", "DonHang");
    }

    private async Task<GioHangPageViewModel> BuildPageAsync()
    {
        var customerId = HttpContext.Session.GetString("MaKhachHang");
        var cart = _gioHangService.Get(HttpContext.Session, customerId);
        var vouchers = await _gioHangService.GetAvailableVouchersAsync();
        return new GioHangPageViewModel
        {
            GioHang = cart,
            AvailableVouchers = vouchers
        };
    }
}

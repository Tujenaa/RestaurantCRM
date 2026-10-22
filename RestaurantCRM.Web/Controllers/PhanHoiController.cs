using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;
using RestaurantCRM.Web.Services;

namespace RestaurantCRM.Web.Controllers;

public class PhanHoiController(MonAnService monAnService, PhanHoiService phanHoiService, DonHangService donHangService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] string? maDonHang = null, [FromQuery] string? maMon = null)
    {
        var model = await BuildModelAsync(new PhanHoiViewModel
        {
            MaDonHang = maDonHang,
            MaMon = maMon,
            CheDoDanhGia = string.IsNullOrEmpty(maMon) ? "DonHang" : "TungMon"
        });

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Gui(PhanHoiViewModel model)
    {
        string? customerId = HttpContext.Session.GetString("MaKhachHang") ?? "KH01";
        string? customerName = HttpContext.Session.GetString("HoTen") ?? "Khách hàng Lẩu Phố";

        if (string.IsNullOrWhiteSpace(model.NoiDung))
        {
            ModelState.AddModelError(nameof(model.NoiDung), "Vui lòng nhập nội dung đánh giá của bạn.");
        }

        // Tùy theo chế độ: "TungMon" (chọn 1 món cụ thể) hay "DonHang" (tất cả các món trong đơn hàng)
        if (model.CheDoDanhGia == "TungMon")
        {
            if (string.IsNullOrWhiteSpace(model.MaMon))
            {
                ModelState.AddModelError(nameof(model.MaMon), "Vui lòng chọn món ăn muốn đánh giá.");
            }
        }

        if (!ModelState.IsValid)
        {
            var reloadModel = await BuildModelAsync(model);
            return View("Index", reloadModel);
        }

        if (model.CheDoDanhGia == "TungMon" && !string.IsNullOrWhiteSpace(model.MaMon))
        {
            var dish = monAnService.Find(model.MaMon) ?? await monAnService.GetByIdAsync(model.MaMon);
            string tenMon = dish?.TenMon ?? "Món ngon Lẩu Phố";
            await phanHoiService.AddReviewAsync(model.MaMon, tenMon, model.DanhGia, model.NoiDung, model.MaDonHang, customerName, customerId);
            TempData["ThongBao"] = $"Cảm ơn bạn đã gửi đánh giá cho món '{tenMon}'!";
        }
        else
        {
            // Chế độ đánh giá cho toàn bộ đơn hàng / tất cả các món trong đơn
            DonHangViewModel? order = null;
            if (!string.IsNullOrWhiteSpace(model.MaDonHang))
            {
                order = await donHangService.GetOrderDetailAsync(model.MaDonHang);
            }

            if (order != null && order.ChiTiet.Count > 0)
            {
                foreach (var item in order.ChiTiet)
                {
                    await phanHoiService.AddReviewAsync(item.MaMon, item.TenMon, model.DanhGia, model.NoiDung, model.MaDonHang, customerName, customerId);
                }
                TempData["ThongBao"] = $"Cảm ơn bạn đã đánh giá toàn bộ {order.ChiTiet.Count} món trong đơn hàng #{model.MaDonHang}!";
            }
            else
            {
                // Fallback nếu không có đơn cụ thể
                var firstDish = monAnService.GetAll().FirstOrDefault();
                string maMon = firstDish?.MaMon ?? "M01";
                string tenMon = firstDish?.TenMon ?? "Đơn hàng Lẩu Phố";
                await phanHoiService.AddReviewAsync(maMon, tenMon, model.DanhGia, model.NoiDung, model.MaDonHang, customerName, customerId);
                TempData["ThongBao"] = "Cảm ơn bạn đã gửi đánh giá thành công!";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PhanHoiLai(string maDanhGia, string noiDung, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(maDanhGia) || string.IsNullOrWhiteSpace(noiDung))
        {
            TempData["Loi"] = "Nội dung phản hồi không được để trống.";
            return Redirect(returnUrl ?? Url.Action(nameof(Index))!);
        }

        string customerName = HttpContext.Session.GetString("HoTen") ?? "Khách hàng";
        string customerId = HttpContext.Session.GetString("MaKhachHang") ?? "KH01";

        await phanHoiService.AddReplyAsync(maDanhGia, noiDung.Trim(), customerName, customerId);
        TempData["ThongBao"] = "Đã gửi phản hồi thành công!";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<PhanHoiViewModel> BuildModelAsync(PhanHoiViewModel? model = null)
    {
        model ??= new PhanHoiViewModel();
        model.MonAns = monAnService.GetAll();
        model.PhanHois = await phanHoiService.GetRecentAsync();

        string? customerId = HttpContext.Session.GetString("MaKhachHang");
        model.DonHangs = await donHangService.GetOrdersForCustomerAsync(customerId, "done");

        if (string.IsNullOrEmpty(model.MaDonHang) && model.DonHangs.Count > 0)
        {
            model.MaDonHang = model.DonHangs[0].MaDonHang;
        }

        if (!string.IsNullOrEmpty(model.MaDonHang))
        {
            model.DonHangHienTai = await donHangService.GetOrderDetailAsync(model.MaDonHang);
        }

        return model;
    }
}

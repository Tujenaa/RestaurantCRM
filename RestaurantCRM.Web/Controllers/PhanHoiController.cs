using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;
using RestaurantCRM.Web.Services;

namespace RestaurantCRM.Web.Controllers;

public class PhanHoiController(MonAnService monAnService, PhanHoiService phanHoiService, DonHangService donHangService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString("Token");
        var customerId = HttpContext.Session.GetString("MaKhachHang");
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(customerId))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập để xem lịch sử đánh giá của bạn.";
            return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = Url.Action("Index", "PhanHoi") });
        }

        var model = new PhanHoiViewModel
        {
            PhanHois = await phanHoiService.GetByKhachHangAsync(customerId)
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> DanhGia([FromQuery] string? maDonHang = null, [FromQuery] string? maMon = null)
    {
        var token = HttpContext.Session.GetString("Token");
        var customerId = HttpContext.Session.GetString("MaKhachHang");
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(customerId))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập để gửi đánh giá.";
            return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = Request.Path + Request.QueryString });
        }

        // Lấy danh sách các đơn hàng đã hoàn tất của khách hàng
        var doneOrders = await donHangService.GetOrdersForCustomerAsync(customerId, "done");
        if (doneOrders.Count == 0)
        {
            TempData["Loi"] = "Bạn chỉ có thể đánh giá sau khi đã có đơn hàng giao hoàn tất tại nhà hàng.";
            return RedirectToAction("Index", "DonHang");
        }

        // Nếu mã đơn truyền vào không thuộc đơn hoàn tất của khách, mặc định lấy đơn đầu tiên
        if (string.IsNullOrEmpty(maDonHang) || !doneOrders.Any(o => o.MaDonHang == maDonHang))
        {
            maDonHang = doneOrders[0].MaDonHang;
        }

        var model = await BuildModelAsync(new PhanHoiViewModel
        {
            MaDonHang = maDonHang,
            MaMon = maMon,
            CheDoDanhGia = string.IsNullOrEmpty(maMon) ? "DonHang" : "TungMon"
        }, customerId, doneOrders);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Gui(PhanHoiViewModel model)
    {
        var token = HttpContext.Session.GetString("Token");
        var customerId = HttpContext.Session.GetString("MaKhachHang");
        var customerName = HttpContext.Session.GetString("HoTen") ?? "Khách hàng Lẩu Phố";

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(customerId))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập để gửi đánh giá.";
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        // Bắt buộc phải có mã đơn hàng
        if (string.IsNullOrWhiteSpace(model.MaDonHang))
        {
            ModelState.AddModelError(nameof(model.MaDonHang), "Vui lòng chọn đơn hàng bạn muốn đánh giá.");
        }

        // Kiểm tra đơn hàng có thực sự thuộc khách hàng và đã hoàn tất không
        var doneOrders = await donHangService.GetOrdersForCustomerAsync(customerId, "done");
        var matchedOrder = doneOrders.FirstOrDefault(o => o.MaDonHang == model.MaDonHang);
        if (matchedOrder == null)
        {
            ModelState.AddModelError(nameof(model.MaDonHang), "Đơn hàng không hợp lệ hoặc chưa hoàn tất.");
        }

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
            var reloadModel = await BuildModelAsync(model, customerId, doneOrders);
            return View("DanhGia", reloadModel);
        }

        DonHangViewModel? orderDetail = await donHangService.GetOrderDetailAsync(model.MaDonHang!);

        if (model.CheDoDanhGia == "TungMon" && !string.IsNullOrWhiteSpace(model.MaMon))
        {
            var orderItem = orderDetail?.ChiTiet.FirstOrDefault(c => c.MaMon == model.MaMon);
            var dish = monAnService.Find(model.MaMon) ?? await monAnService.GetByIdAsync(model.MaMon);
            string tenMon = orderItem?.TenMon ?? dish?.TenMon ?? "Món ngon Lẩu Phố";

            await phanHoiService.AddReviewAsync(model.MaMon, tenMon, model.DanhGia, model.NoiDung, model.MaDonHang, customerName, customerId);
            TempData["ThongBao"] = $"Cảm ơn bạn đã gửi đánh giá cho món '{tenMon}' trong đơn #{model.MaDonHang}!";
        }
        else
        {
            // Chế độ đánh giá cho toàn bộ đơn hàng
            if (orderDetail != null && orderDetail.ChiTiet.Count > 0)
            {
                foreach (var item in orderDetail.ChiTiet)
                {
                    await phanHoiService.AddReviewAsync(item.MaMon, item.TenMon, model.DanhGia, model.NoiDung, model.MaDonHang, customerName, customerId);
                }
                TempData["ThongBao"] = $"Cảm ơn bạn đã đánh giá toàn bộ {orderDetail.ChiTiet.Count} món trong đơn hàng #{model.MaDonHang}!";
            }
            else
            {
                var firstDish = monAnService.GetAll().FirstOrDefault();
                string maMon = firstDish?.MaMon ?? "M01";
                string tenMon = firstDish?.TenMon ?? "Đơn hàng Lẩu Phố";
                await phanHoiService.AddReviewAsync(maMon, tenMon, model.DanhGia, model.NoiDung, model.MaDonHang, customerName, customerId);
                TempData["ThongBao"] = $"Cảm ơn bạn đã gửi đánh giá cho đơn hàng #{model.MaDonHang}!";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PhanHoiLai(string maDanhGia, string noiDung, string? returnUrl = null)
    {
        var token = HttpContext.Session.GetString("Token");
        var customerId = HttpContext.Session.GetString("MaKhachHang");
        var customerName = HttpContext.Session.GetString("HoTen") ?? "Khách hàng";

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(customerId))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập để gửi phản hồi.";
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        if (string.IsNullOrWhiteSpace(maDanhGia) || string.IsNullOrWhiteSpace(noiDung))
        {
            TempData["Loi"] = "Nội dung phản hồi không được để trống.";
            return Redirect(returnUrl ?? Url.Action(nameof(Index))!);
        }

        await phanHoiService.AddReplyAsync(maDanhGia, noiDung.Trim(), customerName, customerId);
        TempData["ThongBao"] = "Đã gửi phản hồi thành công!";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<PhanHoiViewModel> BuildModelAsync(PhanHoiViewModel model, string customerId, IReadOnlyList<DonHangViewModel> doneOrders)
    {
        model.DonHangs = doneOrders;
        model.MonAns = monAnService.GetAll();

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

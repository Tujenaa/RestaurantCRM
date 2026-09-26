using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;
using RestaurantCRM.Web.Services;

namespace RestaurantCRM.Web.Controllers;

public class PhanHoiController(MonAnService monAnService, PhanHoiService phanHoiService, DonHangService donHangService) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(BuildModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Gui(PhanHoiViewModel model)
    {
        if (!ModelState.IsValid) return View("Index", BuildModel(model));
        var dish = monAnService.Find(model.MaMon);
        if (dish is null)
        {
            ModelState.AddModelError(nameof(model.MaMon), "Vui lòng chọn món trong thực đơn.");
            return View("Index", BuildModel(model));
        }
        phanHoiService.Add(dish.TenMon, model.DanhGia, model.NoiDung);
        TempData["ThongBao"] = "Cảm ơn bạn đã gửi phản hồi.";
        return RedirectToAction(nameof(Index));
    }

    private PhanHoiViewModel BuildModel(PhanHoiViewModel? model = null)
    {
        model ??= new PhanHoiViewModel();
        model.MonAns = monAnService.GetAll();
        model.PhanHois = phanHoiService.GetRecent();
        model.DonHangs = donHangService.GetForDemoCustomer("done");
        if (String.IsNullOrEmpty(model.MaDonHang)) model.MaDonHang = model.DonHangs.FirstOrDefault()?.MaDonHang ?? "";
        return model;
    }
}

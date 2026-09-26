using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;

namespace RestaurantCRM.Web.Controllers;

public class KhaoSatController : Controller
{
    [HttpGet]
    public IActionResult Index(bool form = false)
    {
        ViewBag.ShowForm = form;
        return View(new KhaoSatViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Gui(KhaoSatViewModel model)
    {
        if (model.YeuToQuanTam.Count == 0) ModelState.AddModelError(nameof(model.YeuToQuanTam), "Vui lòng chọn ít nhất một yếu tố.");
        if (!ModelState.IsValid) { ViewBag.ShowForm = true; return View("Index", model); }
        TempData["ThongBao"] = "Cảm ơn bạn đã hoàn thành khảo sát demo.";
        return RedirectToAction(nameof(Index));
    }
}

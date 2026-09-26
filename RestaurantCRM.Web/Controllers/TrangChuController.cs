using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;
using RestaurantCRM.Web.Services;

namespace RestaurantCRM.Web.Controllers;

public class TrangChuController(MonAnService monAnService) : Controller
{
    public IActionResult Index(string? loai, string? q, bool menu = false)
    {
        var all = monAnService.GetAll();
        var selected = all.AsEnumerable();
        if (!String.IsNullOrWhiteSpace(loai) && loai != "Tất cả") selected = selected.Where(x => x.LoaiMon == loai);
        if (!String.IsNullOrWhiteSpace(q)) selected = selected.Where(x => x.TenMon.Contains(q, StringComparison.CurrentCultureIgnoreCase));
        return View(new MonAnDanhSachViewModel { MonAns = selected.ToList(), MonNoiBat = all.Take(4).ToList(), LoaiMons = monAnService.GetCategories(), LoaiDangChon = loai, TuKhoa = q, MenuMode = menu });
    }
}

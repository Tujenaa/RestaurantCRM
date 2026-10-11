using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;
using RestaurantCRM.Web.Services;

namespace RestaurantCRM.Web.Controllers;

public class TrangChuController : Controller
{
    private readonly MonAnService _monAnService;

    public TrangChuController(MonAnService monAnService)
    {
        _monAnService = monAnService;
    }

    public async Task<IActionResult> Index(string? loai, string? q, bool menu = false)
    {
        if (menu)
        {
            return RedirectToAction("Index", "Menu", new { loai, q });
        }

        var all = await _monAnService.GetAllAsync(loai, q);
        var categories = await _monAnService.GetCategoriesAsync();

        return View(new MonAnDanhSachViewModel
        {
            MonAns = all,
            MonNoiBat = all.Where(d => d.ConHang).Take(6).ToList(),
            DanhSachLoaiMon = categories,
            LoaiMons = categories.Select(c => c.TenLoaiMon).ToList(),
            LoaiDangChon = loai,
            TuKhoa = q,
            MenuMode = false
        });
    }
}

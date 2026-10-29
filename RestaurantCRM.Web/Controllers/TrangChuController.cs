using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;
using RestaurantCRM.Web.Services;

namespace RestaurantCRM.Web.Controllers;

public class TrangChuController : Controller
{
    private readonly MonAnService _monAnService;
    private readonly PhanHoiService _phanHoiService;

    public TrangChuController(MonAnService monAnService, PhanHoiService phanHoiService)
    {
        _monAnService = monAnService;
        _phanHoiService = phanHoiService;
    }

    public async Task<IActionResult> Index(string? loai, string? q, bool menu = false)
    {
        if (menu)
        {
            return RedirectToAction("Index", "Menu", new { loai, q });
        }

        var all = await _monAnService.GetAllAsync(loai, q);
        var categories = await _monAnService.GetCategoriesAsync();
        var monNoiBat = all.Where(d => d.ConHang).Take(6).ToList();

        foreach (var d in monNoiBat)
        {
            var summary = await _phanHoiService.GetDishRatingSummaryAsync(d.MaMon);
            d.DiemDanhGiaTrungBinh = summary.AverageRating;
            d.SoLuongDanhGia = summary.ReviewCount;
        }

        return View(new MonAnDanhSachViewModel
        {
            MonAns = all,
            MonNoiBat = monNoiBat,
            DanhSachLoaiMon = categories,
            LoaiMons = categories.Select(c => c.TenLoaiMon).ToList(),
            LoaiDangChon = loai,
            TuKhoa = q,
            MenuMode = false
        });
    }
}

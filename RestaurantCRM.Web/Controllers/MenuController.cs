using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;
using RestaurantCRM.Web.Services;

namespace RestaurantCRM.Web.Controllers;

[Route("[controller]")]
public class MenuController : Controller
{
    private readonly MonAnService _monAnService;

    public MenuController(MonAnService monAnService)
    {
        _monAnService = monAnService;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index([FromQuery] string? loai = null, [FromQuery] string? q = null)
    {
        var categories = await _monAnService.GetCategoriesAsync();
        var dishes = await _monAnService.GetAllAsync(loai, q);

        var viewModel = new MonAnDanhSachViewModel
        {
            MonAns = dishes,
            MonNoiBat = dishes.Where(d => d.ConHang).Take(4).ToList(),
            DanhSachLoaiMon = categories,
            LoaiMons = categories.Select(c => c.TenLoaiMon).ToList(),
            LoaiDangChon = loai,
            TuKhoa = q,
            MenuMode = true
        };

        return View(viewModel);
    }

    [HttpGet("ChiTiet/{id}")]
    public async Task<IActionResult> ChiTiet(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return RedirectToAction(nameof(Index));
        }

        var dish = await _monAnService.GetByIdAsync(id);
        if (dish == null)
        {
            TempData["ThongBao"] = "Không tìm thấy món ăn yêu cầu.";
            return RedirectToAction(nameof(Index));
        }

        // Get related dishes in the same category
        var related = await _monAnService.GetAllAsync(dish.MaLoaiMon);
        var filteredRelated = related.Where(d => !string.Equals(d.MaMon, id, StringComparison.OrdinalIgnoreCase)).Take(4).ToList();

        var viewModel = new MonAnChiTietViewModel
        {
            MonAn = dish,
            MonLienQuan = filteredRelated
        };

        return View(viewModel);
    }

    [HttpGet("QuickView/{id}")]
    public async Task<IActionResult> QuickView(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        var dish = await _monAnService.GetByIdAsync(id);
        if (dish == null)
        {
            return NotFound();
        }

        return PartialView("_MonAnQuickView", dish);
    }
}

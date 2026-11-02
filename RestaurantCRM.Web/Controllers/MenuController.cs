using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;
using RestaurantCRM.Web.Services;

namespace RestaurantCRM.Web.Controllers;

[Route("[controller]")]
public class MenuController : Controller
{
    private readonly MonAnService _monAnService;
    private readonly PhanHoiService _phanHoiService;

    public MenuController(MonAnService monAnService, PhanHoiService phanHoiService)
    {
        _monAnService = monAnService;
        _phanHoiService = phanHoiService;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index([FromQuery] string? loai = null, [FromQuery] string? q = null, [FromQuery] string? sort = null, [FromQuery] int page = 1)
    {
        if (page < 1) page = 1;
        const int pageSize = 8;

        var categories = await _monAnService.GetCategoriesAsync();
        var dishes = await _monAnService.GetAllAsync(loai, q, sort);
        var availableDishes = dishes.Where(d => d.ConHang).ToList();

        int totalDishes = availableDishes.Count;
        int totalPages = (int)Math.Ceiling((double)totalDishes / pageSize);
        if (totalPages < 1) totalPages = 1;
        if (page > totalPages) page = totalPages;

        var pagedDishes = availableDishes
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Cập nhật số sao trung bình thực tế chỉ cho 8 món hiển thị trên trang hiện tại
        foreach (var d in pagedDishes)
        {
            var summary = await _phanHoiService.GetDishRatingSummaryAsync(d.MaMon);
            d.DiemDanhGiaTrungBinh = summary.AverageRating;
            d.SoLuongDanhGia = summary.ReviewCount;
        }

        // Xác định tên danh mục đang chọn (nếu có)
        string? tenLoaiHienTai = null;
        if (!string.IsNullOrWhiteSpace(loai) && loai != "all")
        {
            var matchedCat = categories.FirstOrDefault(c => string.Equals(c.MaLoaiMon, loai, StringComparison.OrdinalIgnoreCase) || string.Equals(c.TenLoaiMon, loai, StringComparison.OrdinalIgnoreCase));
            tenLoaiHienTai = matchedCat?.TenLoaiMon ?? loai;
        }

        var viewModel = new MonAnDanhSachViewModel
        {
            MonAns = pagedDishes,
            MonNoiBat = availableDishes.Take(4).ToList(),
            DanhSachLoaiMon = categories,
            LoaiMons = categories.Select(c => c.TenLoaiMon).ToList(),
            LoaiDangChon = loai,
            TenLoaiHienTai = tenLoaiHienTai,
            TuKhoa = q,
            SortDangChon = sort,
            MenuMode = true,
            CurrentPage = page,
            PageSize = pageSize,
            TotalDishes = totalDishes,
            TotalPages = totalPages
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

        // Lấy danh sách đánh giá thực tế và số sao trung bình của món
        var reviews = await _phanHoiService.GetByMonAnAsync(id);
        var summary = await _phanHoiService.GetDishRatingSummaryAsync(id);
        dish.DiemDanhGiaTrungBinh = summary.AverageRating;
        dish.SoLuongDanhGia = summary.ReviewCount;

        // Get related dishes in the same category
        var related = await _monAnService.GetAllAsync(dish.MaLoaiMon);
        var filteredRelated = related.Where(d => d.ConHang && !string.Equals(d.MaMon, id, StringComparison.OrdinalIgnoreCase)).Take(4).ToList();

        var viewModel = new MonAnChiTietViewModel
        {
            MonAn = dish,
            MonLienQuan = filteredRelated,
            DanhGias = reviews
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

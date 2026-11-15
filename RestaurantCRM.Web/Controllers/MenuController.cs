using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Helpers;
using RestaurantCRM.Web.Models;
using RestaurantCRM.Web.Services;

namespace RestaurantCRM.Web.Controllers;

[Route("[controller]")]
public class MenuController : Controller
{
    private readonly MonAnService _monAnService;
    private readonly PhanHoiService _phanHoiService;
    private readonly KhaoSatService _khaoSatService;

    public MenuController(MonAnService monAnService, PhanHoiService phanHoiService, KhaoSatService khaoSatService)
    {
        _monAnService = monAnService;
        _phanHoiService = phanHoiService;
        _khaoSatService = khaoSatService;
    }

    private string? MaKhachHang => HttpContext.Session.GetString("MaKhachHang");

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(
        [FromQuery] string? loai = null,
        [FromQuery] string? q = null,
        [FromQuery] string? sort = null,
        [FromQuery] string? mucGia = null,
        [FromQuery] bool khuyenMai = false,
        [FromQuery] int page = 1)
    {
        if (page < 1) page = 1;
        const int pageSize = 8;

        var categories = await _monAnService.GetCategoriesAsync();
        // Tải toàn bộ món theo loại rồi lọc thông minh (không dấu + khoảng giá + khuyến mãi)
        var dishes = await _monAnService.GetAllAsync(loai, null, null);
        var availableDishes = dishes.Where(d => d.ConHang).ToList();

        if (!string.IsNullOrWhiteSpace(q))
        {
            availableDishes = availableDishes
                .Where(d => MatchesSearch(q, d.TenMon))   // chỉ tìm theo tên món
                .ToList();
        }

        if (khuyenMai)
        {
            availableDishes = availableDishes
                .Where(d => d.DonGiaSauGiam > 0 && d.DonGiaSauGiam < d.DonGia)
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(mucGia) && mucGia != "all")
        {
            availableDishes = availableDishes.Where(d =>
            {
                decimal price = d.DonGiaSauGiam > 0 ? d.DonGiaSauGiam : d.DonGia;
                return mucGia switch
                {
                    "duoi50" => price < 50000m,
                    "50-150" => price >= 50000m && price <= 150000m,
                    "150-300" => price > 150000m && price <= 300000m,
                    "tren300" => price > 300000m,
                    _ => true
                };
            }).ToList();
        }

        if (!string.IsNullOrWhiteSpace(sort))
        {
            availableDishes = sort switch
            {
                "price_asc" => availableDishes.OrderBy(d => d.DonGiaSauGiam > 0 ? d.DonGiaSauGiam : d.DonGia).ToList(),
                "price_desc" => availableDishes.OrderByDescending(d => d.DonGiaSauGiam > 0 ? d.DonGiaSauGiam : d.DonGia).ToList(),
                "name_asc" => availableDishes.OrderBy(d => d.TenMon).ToList(),
                _ => availableDishes
            };
        }

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
            MucGiaDangChon = mucGia,
            ChiKhuyenMai = khuyenMai,
            MenuMode = true,
            CurrentPage = page,
            PageSize = pageSize,
            TotalDishes = totalDishes,
            TotalPages = totalPages
        };

        return View(viewModel);
    }

    // Cùng quy tắc tìm kiếm với app (khớp đầu chữ, gõ có dấu thì khớp đúng dấu) – xem SearchHelper
    internal static string RemoveDiacritics(string? text) => SearchHelper.RemoveDiacritics(text);

    internal static bool MatchesSearch(string? query, params string?[] fields) => SearchHelper.MatchesSearch(query, fields);

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
        var summary = PhanHoiService.TinhTomTatDiem(reviews);
        dish.DiemDanhGiaTrungBinh = summary.AverageRating;
        dish.SoLuongDanhGia = summary.ReviewCount;

        // Get related dishes in the same category
        var related = await _monAnService.GetAllAsync(dish.MaLoaiMon);
        var filteredRelated = related.Where(d => d.ConHang && !string.Equals(d.MaMon, id, StringComparison.OrdinalIgnoreCase)).Take(4).ToList();
        foreach (var d in filteredRelated)
        {
            var tomTat = await _phanHoiService.GetDishRatingSummaryAsync(d.MaMon);
            d.DiemDanhGiaTrungBinh = tomTat.AverageRating;
            d.SoLuongDanhGia = tomTat.ReviewCount;
        }

        // Nút ♥: chỉ hiện khi nhà hàng đã có câu "Món yêu thích" trong hồ sơ sở thích
        var yeuThich = new MonYeuThichViewModel();
        try
        {
            yeuThich = await _khaoSatService.GetMonYeuThichAsync(MaKhachHang);
        }
        catch (HttpRequestException)
        {
            // Không lấy được thì ẩn nút ♥, trang món vẫn hiển thị bình thường
        }

        var viewModel = new MonAnChiTietViewModel
        {
            MonAn = dish,
            MonLienQuan = filteredRelated,
            DanhGias = reviews,
            CoNutYeuThich = yeuThich.CoMonYeuThich,
            DaYeuThich = yeuThich.MaMons.Contains(dish.MaMon, StringComparer.OrdinalIgnoreCase),
            DaDangNhap = !string.IsNullOrEmpty(MaKhachHang)
        };

        return View(viewModel);
    }

    // POST: /Menu/YeuThich/{maMon} (AJAX) - Bật / tắt ♥ món. Chưa đăng nhập: 401 kèm đường dẫn đăng nhập.
    [HttpPost("YeuThich/{maMon}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> YeuThich(string maMon, [FromForm] bool yeuThich)
    {
        var maKh = MaKhachHang;
        if (string.IsNullOrEmpty(maKh))
        {
            var quayLai = Url.Action(nameof(ChiTiet), new { id = maMon });
            return StatusCode(401, new { dangNhapUrl = Url.Action("DangNhap", "TaiKhoan", new { returnUrl = quayLai }) });
        }

        try
        {
            var (ok, message) = await _khaoSatService.DatMonYeuThichAsync(maKh, maMon, yeuThich);
            if (!ok && TaiKhoanBiKhoaHelper.LaTaiKhoanBiKhoa(message))
            {
                HttpContext.Session.Clear();
                TempData["ThongBao"] = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ nhà hàng để được hỗ trợ.";
                return StatusCode(401, new { dangNhapUrl = Url.Action("DangNhap", "TaiKhoan") });
            }
            return ok ? Ok(new { yeuThich, message }) : BadRequest(new { message });
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, new { message = "Không kết nối được đến máy chủ API." });
        }
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

        var summary = await _phanHoiService.GetDishRatingSummaryAsync(id);
        dish.DiemDanhGiaTrungBinh = summary.AverageRating;
        dish.SoLuongDanhGia = summary.ReviewCount;

        return PartialView("_MonAnQuickView", dish);
    }
}

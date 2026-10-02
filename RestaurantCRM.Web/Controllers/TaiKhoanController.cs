using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestaurantCRM.Web.Controllers;

public class TaiKhoanController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public TaiKhoanController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult DangNhap() => View("Login");

    [HttpGet]
    public IActionResult Index(string tab = "login")
    {
        ViewBag.Tab = tab;
        // Check if user is logged in
        if (HttpContext.Session.GetString("Token") != null)
        {
            var hoTen = HttpContext.Session.GetString("HoTen");
            var email = HttpContext.Session.GetString("Email");
            var soDienThoai = HttpContext.Session.GetString("SoDienThoai");
            
            return View(new KhachHangViewModel { 
                HoTen = hoTen ?? String.Empty,
                SoDienThoai = soDienThoai ?? String.Empty,
                Email = email ?? String.Empty,
                GioiTinh = "Nam", // Placeholder, you may fetch from API
                DiaChi = "12 Lê Lợi, Quận 1, TP. Hồ Chí Minh", // Placeholder
                SoDonHang = 12 
            });
        }

        return View(new KhachHangViewModel { HoTen = "Nguyễn Văn An", SoDienThoai = "0901234567", Email = "a.nguyen@email.com", GioiTinh = "Nam", DiaChi = "12 Lê Lợi, Quận 1, TP. Hồ Chí Minh", SoDonHang = 12 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap(string lienHe, string matKhau)
    {
        if (String.IsNullOrWhiteSpace(lienHe) || String.IsNullOrEmpty(matKhau))
        {
            TempData["ThongBao"] = "Vui lòng nhập email/số điện thoại và mật khẩu.";
            return RedirectToAction(nameof(DangNhap));
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var payload = new { EmailOrPhone = lienHe.Trim(), MatKhau = matKhau };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/KhachHang/DangNhap", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseString);
                var result = document.RootElement;

                if (!result.TryGetProperty("token", out var tokenProperty) ||
                    !result.TryGetProperty("khachHang", out var khachHang) ||
                    !khachHang.TryGetProperty("maKhachHang", out var idProperty) ||
                    !khachHang.TryGetProperty("hoTen", out var nameProperty))
                {
                    TempData["ThongBao"] = "API trả về thông tin đăng nhập không hợp lệ.";
                    return RedirectToAction(nameof(DangNhap));
                }

                HttpContext.Session.SetString("Token", tokenProperty.GetString() ?? "");
                HttpContext.Session.SetString("MaKhachHang", idProperty.GetString() ?? "");
                HttpContext.Session.SetString("HoTen", nameProperty.GetString() ?? "");

                if (khachHang.TryGetProperty("email", out var emailProp) && emailProp.ValueKind == JsonValueKind.String)
                {
                    HttpContext.Session.SetString("Email", emailProp.GetString() ?? "");
                }
                if (khachHang.TryGetProperty("soDienThoai", out var phoneProp) && phoneProp.ValueKind == JsonValueKind.String)
                {
                    HttpContext.Session.SetString("SoDienThoai", phoneProp.GetString() ?? "");
                }

                return RedirectToAction("Index", "TrangChu");
            }

            var error = await response.Content.ReadAsStringAsync();
            TempData["ThongBao"] = String.IsNullOrWhiteSpace(error)
                ? "Email/số điện thoại hoặc mật khẩu không đúng."
                : error.Trim('"');
            return RedirectToAction(nameof(DangNhap));
        }
        catch (HttpRequestException)
        {
            TempData["ThongBao"] = "Không kết nối được API. Hãy chạy RestaurantCRM.API rồi thử lại.";
            return RedirectToAction(nameof(DangNhap));
        }
        catch (JsonException)
        {
            TempData["ThongBao"] = "API trả về dữ liệu không hợp lệ.";
            return RedirectToAction(nameof(DangNhap));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangKy(string hoTen, string soDienThoai, string? email, string matKhau)
    {
        if (String.IsNullOrWhiteSpace(hoTen) || String.IsNullOrWhiteSpace(soDienThoai) || String.IsNullOrWhiteSpace(matKhau))
        {
            TempData["ThongBao"] = "Vui lòng nhập họ tên, số điện thoại và mật khẩu.";
            return RedirectToAction(nameof(Index), new { tab = "register" });
        }

        var client = _httpClientFactory.CreateClient("ApiClient");
        var payload = new { HoTen = hoTen, SoDienThoai = soDienThoai, Email = email, MatKhau = matKhau };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsync("/api/KhachHang/DangKy", content);
        }
        catch (HttpRequestException)
        {
            TempData["ThongBao"] = "Không kết nối được API. Hãy chạy RestaurantCRM.API rồi thử lại.";
            return RedirectToAction(nameof(Index), new { tab = "register" });
        }

        if (response.IsSuccessStatusCode)
        {
            TempData["ThongBao"] = "Đăng ký thành công, vui lòng đăng nhập.";
            return RedirectToAction(nameof(Index), new { tab = "login" });
        }
        else
        {
            var errorResponse = await response.Content.ReadAsStringAsync();
            TempData["ThongBao"] = "Đăng ký thất bại: " + errorResponse;
            return RedirectToAction(nameof(Index), new { tab = "register" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CapNhat(KhachHangViewModel model)
    {
        TempData["ThongBao"] = "Thông tin chỉ được hiển thị trong bản demo; chưa lưu lên máy chủ.";
        return RedirectToAction(nameof(Index), new { tab = "profile" });
    }
}

using RestaurantCRM.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;
using RestaurantCRM.Web.Services;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestaurantCRM.Web.Controllers;

public class TaiKhoanController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GioHangService _gioHangService;

    public TaiKhoanController(IHttpClientFactory httpClientFactory, GioHangService gioHangService)
    {
        _httpClientFactory = httpClientFactory;
        _gioHangService = gioHangService;
    }

    [HttpGet]
    public IActionResult DangNhap([FromQuery] string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View("Login");
    }

    [HttpGet]
    public IActionResult DangKy() => View("Register");

    [HttpGet]
    public async Task<IActionResult> Index(string tab = "profile")
    {
        // Check if user is logged in
        var token = HttpContext.Session.GetString("Token");
        var maKhachHang = HttpContext.Session.GetString("MaKhachHang");
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(maKhachHang))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập để xem thông tin tài khoản.";
            return RedirectToAction(nameof(DangNhap));
        }

        ViewBag.Tab = tab;
        var hoTen = HttpContext.Session.GetString("HoTen") ?? "Khách hàng";
        var email = HttpContext.Session.GetString("Email") ?? "";
        var soDienThoai = HttpContext.Session.GetString("SoDienThoai") ?? "";

        var model = new KhachHangViewModel
        {
            HoTen = hoTen,
            SoDienThoai = soDienThoai,
            Email = email,
            GioiTinh = "Không muốn tiết lộ",
            DiaChi = "Chưa cập nhật",
            SoDonHang = 0
        };

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/KhachHang/ThongTin/{Uri.EscapeDataString(maKhachHang)}");
            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonStr);
                var root = doc.RootElement;
                if (root.TryGetProperty("hoTen", out var pName) && pName.ValueKind == JsonValueKind.String) model.HoTen = pName.GetString() ?? model.HoTen;
                if (root.TryGetProperty("soDienThoai", out var pPhone) && pPhone.ValueKind == JsonValueKind.String) model.SoDienThoai = pPhone.GetString() ?? model.SoDienThoai;
                if (root.TryGetProperty("email", out var pEmail))
                {
                    var apiEmail = (pEmail.ValueKind == JsonValueKind.String) ? pEmail.GetString() : null;
                    model.Email = !string.IsNullOrWhiteSpace(apiEmail) ? apiEmail : (HttpContext.Session.GetString("Email") ?? "");
                    if (!string.IsNullOrWhiteSpace(model.Email)) HttpContext.Session.SetString("Email", model.Email);
                }
                if (root.TryGetProperty("gioiTinh", out var pGender) && pGender.ValueKind == JsonValueKind.String) model.GioiTinh = pGender.GetString() ?? "Không muốn tiết lộ";
                if (root.TryGetProperty("ngaySinh", out var pDob) && pDob.ValueKind == JsonValueKind.String && DateTime.TryParse(pDob.GetString(), out var dob)) model.NgaySinh = dob;
                if (root.TryGetProperty("diaChi", out var pAddr) && pAddr.ValueKind == JsonValueKind.String)
                {
                    var addr = pAddr.GetString();
                    if (!string.IsNullOrWhiteSpace(addr))
                    {
                        model.DiaChi = addr;
                        HttpContext.Session.SetString("DiaChiGiaoHang", addr);
                    }
                }
                if (string.IsNullOrWhiteSpace(model.DiaChi))
                {
                    model.DiaChi = HttpContext.Session.GetString("DiaChiGiaoHang") ?? "";
                }
            }
        }
        catch
        {
            model.DiaChi = HttpContext.Session.GetString("DiaChiGiaoHang") ?? "";
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult DangXuat()
    {
        var customerId = HttpContext.Session.GetString("MaKhachHang");
        if (!string.IsNullOrWhiteSpace(customerId))
        {
            _gioHangService.PreserveCartBeforeLogout(HttpContext.Session, customerId);
        }
        HttpContext.Session.Clear();
        TempData["ThongBao"] = "Đã đăng xuất tài khoản thành công.";
        return RedirectToAction("Index", "TrangChu");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap(string lienHe, string matKhau, [FromQuery] string? returnUrl = null)
    {
        if (String.IsNullOrWhiteSpace(lienHe) || String.IsNullOrEmpty(matKhau))
        {
            TempData["ThongBao"] = "Vui lòng nhập email/số điện thoại và mật khẩu.";
            return RedirectToAction(nameof(DangNhap), new { returnUrl });
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
                    return RedirectToAction(nameof(DangNhap), new { returnUrl });
                }

                var customerId = idProperty.GetString() ?? "";
                HttpContext.Session.SetString("Token", tokenProperty.GetString() ?? "");
                HttpContext.Session.SetString("MaKhachHang", customerId);
                HttpContext.Session.SetString("HoTen", nameProperty.GetString() ?? "");

                if (khachHang.TryGetProperty("email", out var emailProp) && emailProp.ValueKind == JsonValueKind.String)
                {
                    HttpContext.Session.SetString("Email", emailProp.GetString() ?? "");
                }
                if (khachHang.TryGetProperty("soDienThoai", out var phoneProp) && phoneProp.ValueKind == JsonValueKind.String)
                {
                    HttpContext.Session.SetString("SoDienThoai", phoneProp.GetString() ?? "");
                }
                if (khachHang.TryGetProperty("diaChi", out var addrProp) && addrProp.ValueKind == JsonValueKind.String)
                {
                    var addr = addrProp.GetString();
                    if (!string.IsNullOrWhiteSpace(addr))
                    {
                        HttpContext.Session.SetString("DiaChiGiaoHang", addr);
                    }
                }

                // Khôi phục và hợp nhất các món trong giỏ hàng đã lưu của khách hàng
                _gioHangService.RestoreCartOnLogin(HttpContext.Session, customerId);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "TrangChu");
            }

            var error = await response.Content.ReadAsStringAsync();
            TempData["ThongBao"] = String.IsNullOrWhiteSpace(error)
                ? "Email/số điện thoại hoặc mật khẩu không đúng."
                : error.Trim('"');
            return RedirectToAction(nameof(DangNhap), new { returnUrl });
        }
        catch (HttpRequestException)
        {
            TempData["ThongBao"] = "Không kết nối được API. Hãy chạy RestaurantCRM.API rồi thử lại.";
            return RedirectToAction(nameof(DangNhap), new { returnUrl });
        }
        catch (JsonException)
        {
            TempData["ThongBao"] = "API trả về dữ liệu không hợp lệ.";
            return RedirectToAction(nameof(DangNhap), new { returnUrl });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangKy(string hoTen, string soDienThoai, string? email, string matKhau, DateTime? ngaySinh, string? gioiTinh)
    {
        if (String.IsNullOrWhiteSpace(hoTen) || String.IsNullOrWhiteSpace(soDienThoai) || String.IsNullOrWhiteSpace(matKhau))
        {
            TempData["ThongBao"] = "Vui lòng nhập họ tên, số điện thoại và mật khẩu.";
            return RedirectToAction(nameof(DangKy));
        }

        var client = _httpClientFactory.CreateClient("ApiClient");
        DateOnly? ns = ngaySinh.HasValue ? DateOnly.FromDateTime(ngaySinh.Value) : null;
        var payload = new
        {
            HoTen = hoTen.Trim(),
            SoDienThoai = soDienThoai.Trim(),
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            MatKhau = matKhau,
            NgaySinh = ns,
            GioiTinh = string.IsNullOrWhiteSpace(gioiTinh) ? null : gioiTinh.Trim()
        };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsync("/api/KhachHang/DangKy", content);
        }
        catch (HttpRequestException)
        {
            TempData["ThongBao"] = "Không kết nối được API. Hãy đảm bảo RestaurantCRM.API đang chạy rồi thử lại.";
            return RedirectToAction(nameof(DangKy));
        }

        if (response.IsSuccessStatusCode)
        {
            TempData["ThongBao"] = "🎉 Tạo tài khoản thành công! Vui lòng đăng nhập bằng số điện thoại/email vừa đăng ký.";
            return RedirectToAction(nameof(DangNhap));
        }
        else
        {
            var errorResponse = await response.Content.ReadAsStringAsync();
            TempData["ThongBao"] = "Đăng ký không thành công: " + (string.IsNullOrWhiteSpace(errorResponse) ? "Vui lòng kiểm tra lại thông tin." : errorResponse.Trim('"'));
            return RedirectToAction(nameof(DangKy));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CapNhat(KhachHangViewModel model)
    {
        var maKhachHang = HttpContext.Session.GetString("MaKhachHang");
        if (string.IsNullOrEmpty(maKhachHang))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập lại.";
            return RedirectToAction(nameof(DangNhap));
        }

        if (!ModelState.IsValid)
        {
            var loi = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));
            TempData["ThongBao"] = "Cập nhật thất bại: " + (loi ?? "Dữ liệu không hợp lệ.");
            return RedirectToAction(nameof(Index), new { tab = "profile" });
        }

        var cleanEmail = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        var cleanSoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai) ? null : model.SoDienThoai.Trim();
        var cleanGioiTinh = string.IsNullOrWhiteSpace(model.GioiTinh) ? null : model.GioiTinh.Trim();

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            DateOnly? ngaySinh = model.NgaySinh.HasValue ? DateOnly.FromDateTime(model.NgaySinh.Value) : null;

            // Gửi đủ các trường; địa chỉ lấy từ dữ liệu hiện có của khách trên hệ thống (không lấy từ Session).
            // Ô nào để trống thì API giữ nguyên giá trị cũ.
            var hienTai = await LayThongTinHienTaiAsync(client, maKhachHang);
            if (cleanEmail == null && cleanSoDienThoai == null
                && string.IsNullOrWhiteSpace(hienTai?.Email) && string.IsNullOrWhiteSpace(hienTai?.SoDienThoai))
            {
                TempData["ThongBao"] = "Cập nhật thất bại: Vui lòng nhập ít nhất email hoặc số điện thoại.";
                return RedirectToAction(nameof(Index), new { tab = "profile" });
            }

            var payload = new
            {
                HoTen = model.HoTen?.Trim(),
                SoDienThoai = cleanSoDienThoai,
                Email = cleanEmail,
                NgaySinh = ngaySinh,
                GioiTinh = cleanGioiTinh,
                DiaChi = hienTai?.DiaChi
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"/api/KhachHang/ThongTin/{Uri.EscapeDataString(maKhachHang)}", content);

            if (response.IsSuccessStatusCode)
            {
                HttpContext.Session.SetString("HoTen", model.HoTen?.Trim() ?? "");
                if (cleanEmail != null) HttpContext.Session.SetString("Email", cleanEmail);
                if (cleanSoDienThoai != null) HttpContext.Session.SetString("SoDienThoai", cleanSoDienThoai);

                TempData["ThongBao"] = "🎉 Cập nhật thông tin thành công!";
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                if (TaiKhoanBiKhoaHelper.LaTaiKhoanBiKhoa(err)) return this.DangXuatVaVeDangNhap();
                TempData["ThongBao"] = "Cập nhật thất bại: " + (string.IsNullOrWhiteSpace(err) ? "Lỗi không xác định." : err.Trim('"'));
            }
        }
        catch (HttpRequestException)
        {
            TempData["ThongBao"] = "Không kết nối được đến máy chủ API.";
        }

        return RedirectToAction(nameof(Index), new { tab = "profile" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiMatKhau(string matKhauCu, string matKhauMoi, string? matKhauXacNhan)
    {
        var maKhachHang = HttpContext.Session.GetString("MaKhachHang");
        if (string.IsNullOrEmpty(maKhachHang))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập lại để đổi mật khẩu.";
            return RedirectToAction(nameof(DangNhap));
        }

        if (string.IsNullOrWhiteSpace(matKhauCu))
        {
            TempData["ThongBao"] = "Vui lòng nhập mật khẩu hiện tại.";
            return RedirectToAction(nameof(Index), new { tab = "profile" });
        }

        if (string.IsNullOrWhiteSpace(matKhauMoi) || matKhauMoi.Length < 6)
        {
            TempData["ThongBao"] = "Mật khẩu mới phải có ít nhất 6 ký tự.";
            return RedirectToAction(nameof(Index), new { tab = "profile" });
        }

        if (!string.IsNullOrEmpty(matKhauXacNhan) && matKhauMoi != matKhauXacNhan)
        {
            TempData["ThongBao"] = "Mật khẩu xác nhận không khớp.";
            return RedirectToAction(nameof(Index), new { tab = "profile" });
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var payload = new
            {
                matKhauCu = matKhauCu,
                matKhauMoi = matKhauMoi
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"/api/KhachHang/DoiMatKhau/{Uri.EscapeDataString(maKhachHang)}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["ThongBao"] = "🎉 Đổi mật khẩu thành công!";
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                if (TaiKhoanBiKhoaHelper.LaTaiKhoanBiKhoa(err)) return this.DangXuatVaVeDangNhap();
                TempData["ThongBao"] = "Đổi mật khẩu thất bại: " + (string.IsNullOrWhiteSpace(err) ? "Mật khẩu cũ không chính xác." : err.Trim('"'));
            }
        }
        catch (HttpRequestException)
        {
            TempData["ThongBao"] = "Không kết nối được đến máy chủ API.";
        }

        return RedirectToAction(nameof(Index), new { tab = "profile" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LuuDiaChi(string diaChi)
    {
        var maKhachHang = HttpContext.Session.GetString("MaKhachHang");
        if (string.IsNullOrEmpty(maKhachHang))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập lại.";
            return RedirectToAction(nameof(DangNhap));
        }

        var cleanAddress = (diaChi ?? "").Trim();
        if (cleanAddress.Length == 0)
        {
            TempData["ThongBao"] = "Vui lòng nhập địa chỉ giao hàng.";
            return RedirectToAction(nameof(Index), new { tab = "addresses" });
        }
        if (cleanAddress.Length > 255)
        {
            TempData["ThongBao"] = "Địa chỉ giao hàng tối đa 255 ký tự.";
            return RedirectToAction(nameof(Index), new { tab = "addresses" });
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var payload = new { DiaChi = cleanAddress };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"/api/KhachHang/DiaChi/{Uri.EscapeDataString(maKhachHang)}", content);

            if (response.IsSuccessStatusCode)
            {
                HttpContext.Session.SetString("DiaChiGiaoHang", cleanAddress);
                TempData["ThongBao"] = "🎉 Cập nhật địa chỉ giao hàng thành công!";
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                if (TaiKhoanBiKhoaHelper.LaTaiKhoanBiKhoa(err)) return this.DangXuatVaVeDangNhap();
                TempData["ThongBao"] = "Cập nhật địa chỉ thất bại: " + (string.IsNullOrWhiteSpace(err) ? "Lỗi không xác định." : err.Trim('"'));
            }
        }
        catch (HttpRequestException)
        {
            TempData["ThongBao"] = "Không kết nối được đến máy chủ API.";
        }

        return RedirectToAction(nameof(Index), new { tab = "addresses" });
    }

    private sealed class ThongTinKhachHangApi
    {
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
    }

    /// <summary>Lấy thông tin hiện có của khách từ API (null nếu không lấy được).</summary>
    private static async Task<ThongTinKhachHangApi?> LayThongTinHienTaiAsync(HttpClient client, string maKhachHang)
    {
        try
        {
            var response = await client.GetAsync($"/api/KhachHang/ThongTin/{Uri.EscapeDataString(maKhachHang)}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ThongTinKhachHangApi>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}

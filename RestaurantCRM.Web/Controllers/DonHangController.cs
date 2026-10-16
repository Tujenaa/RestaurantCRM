using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;
using RestaurantCRM.Web.Services;

namespace RestaurantCRM.Web.Controllers;

public class DonHangController : Controller
{
    private readonly DonHangService _donHangService;
    private readonly GioHangService _gioHangService;

    public DonHangController(DonHangService donHangService, GioHangService gioHangService)
    {
        _donHangService = donHangService;
        _gioHangService = gioHangService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string trangThai = "all")
    {
        var token = HttpContext.Session.GetString("Token");
        if (string.IsNullOrEmpty(token))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập để xem danh sách đơn hàng của bạn.";
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var customerId = HttpContext.Session.GetString("MaKhachHang") ?? "KH01";
        var orders = await _donHangService.GetOrdersForCustomerAsync(customerId, trangThai);

        return View(new DonHangDanhSachViewModel
        {
            DonHangs = orders,
            TrangThaiDangChon = trangThai
        });
    }

    [HttpGet]
    public async Task<IActionResult> ChiTiet(string id)
    {
        var token = HttpContext.Session.GetString("Token");
        if (string.IsNullOrEmpty(token))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập để xem chi tiết đơn hàng.";
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        if (string.IsNullOrWhiteSpace(id)) return RedirectToAction(nameof(Index));

        var order = await _donHangService.GetOrderDetailAsync(id);
        if (order == null)
        {
            TempData["ThongBao"] = "Không tìm thấy thông tin đơn hàng.";
            return RedirectToAction(nameof(Index));
        }

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HuyDon(string id)
    {
        var token = HttpContext.Session.GetString("Token");
        if (string.IsNullOrEmpty(token))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập để thực hiện thao tác này.";
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var customerId = HttpContext.Session.GetString("MaKhachHang") ?? "KH01";
        var result = await _donHangService.CancelOrderAsync(id, customerId);

        TempData["ThongBao"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MuaLai(string id)
    {
        var token = HttpContext.Session.GetString("Token");
        if (string.IsNullOrEmpty(token))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập để đặt lại đơn hàng.";
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var order = await _donHangService.GetOrderDetailAsync(id);
        if (order is null || order.ChiTiet.Count == 0)
        {
            TempData["ThongBao"] = "Không tìm thấy chi tiết đơn hàng để đặt lại.";
        }
        else
        {
            // Xóa sạch giỏ hàng cũ trước khi nạp món từ đơn trước để giữ nguyên đúng số lượng, không bị cộng dồn tăng thêm
            _gioHangService.Clear(HttpContext.Session);

            foreach (var item in order.ChiTiet)
            {
                _gioHangService.Add(HttpContext.Session, item.MaMon, item.SoLuong);
            }
            TempData["ThongBao"] = $"Đã tải lại đúng các món từ đơn #{id} vào giỏ hàng.";
        }
        return RedirectToAction("Index", "GioHang");
    }
}

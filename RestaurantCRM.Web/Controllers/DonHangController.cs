using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;
using RestaurantCRM.Web.Services;

namespace RestaurantCRM.Web.Controllers;

public class DonHangController(DonHangService donHangService, GioHangService gioHangService) : Controller
{
    public IActionResult Index(string trangThai = "all") => View(new DonHangDanhSachViewModel { DonHangs = donHangService.GetForDemoCustomer(trangThai), TrangThaiDangChon = trangThai });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MuaLai(string id)
    {
        var order = donHangService.Find(id);
        if (order is null) TempData["ThongBao"] = "Không tìm thấy đơn hàng demo này.";
        else
        {
            foreach (var item in order.ChiTiet) gioHangService.Add(HttpContext.Session, item.MaMon, item.SoLuong);
            TempData["ThongBao"] = "Đã thêm món từ đơn hàng vào giỏ.";
        }
        return RedirectToAction("Index", "GioHang");
    }
}

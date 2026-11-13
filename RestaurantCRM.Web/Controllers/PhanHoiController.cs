using RestaurantCRM.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;
using RestaurantCRM.Web.Services;

namespace RestaurantCRM.Web.Controllers;

public class PhanHoiController(MonAnService monAnService, PhanHoiService phanHoiService, DonHangService donHangService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var token = HttpContext.Session.GetString("Token");
        var customerId = HttpContext.Session.GetString("MaKhachHang");
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(customerId))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập để xem lịch sử đánh giá của bạn.";
            return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = Url.Action("Index", "PhanHoi") });
        }

        var model = new PhanHoiViewModel
        {
            PhanHois = await phanHoiService.GetByKhachHangAsync(customerId)
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> DanhGia([FromQuery] string? maDonHang = null)
    {
        var token = HttpContext.Session.GetString("Token");
        var customerId = HttpContext.Session.GetString("MaKhachHang");
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(customerId))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập để gửi đánh giá.";
            return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = Request.Path + Request.QueryString });
        }

        // Lấy danh sách các đơn hàng đã hoàn tất của khách hàng
        var doneOrders = await donHangService.GetOrdersForCustomerAsync(customerId, "done");
        if (doneOrders.Count == 0)
        {
            TempData["Loi"] = "Bạn chỉ có thể đánh giá sau khi đã có đơn hàng giao hoàn tất tại nhà hàng.";
            return RedirectToAction("Index", "DonHang");
        }

        // Nếu mã đơn truyền vào đã được đánh giá rồi: chuyển sang xem chi tiết đánh giá
        if (!string.IsNullOrEmpty(maDonHang))
        {
            var targetOrder = doneOrders.FirstOrDefault(o => o.MaDonHang == maDonHang);
            if (targetOrder != null && targetOrder.DaDanhGia)
            {
                TempData["ThongBao"] = $"Đơn hàng #{maDonHang} đã được đánh giá trước đó.";
                return RedirectToAction(nameof(ChiTiet), new { maHoaDon = maDonHang });
            }
        }

        // Danh sách các đơn chưa từng được đánh giá
        var unreviewedOrders = doneOrders.Where(o => !o.DaDanhGia).ToList();
        if (unreviewedOrders.Count == 0)
        {
            TempData["ThongBao"] = "Tất cả các đơn hàng đã hoàn tất của bạn đều đã được đánh giá. Cảm ơn bạn!";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrEmpty(maDonHang) || !unreviewedOrders.Any(o => o.MaDonHang == maDonHang))
        {
            maDonHang = unreviewedOrders[0].MaDonHang;
        }

        var orderDetail = await donHangService.GetOrderDetailAsync(maDonHang);

        var model = new PhanHoiViewModel
        {
            MaDonHang = maDonHang,
            DanhGia = 5,
            NoiDung = "",
            DonHangHienTai = orderDetail,
            DonHangs = unreviewedOrders,
            ChiTietMonDanhGia = (orderDetail?.ChiTiet ?? new()).Select(c => new DishReviewInputItem
            {
                MaMon = c.MaMon,
                TenMon = c.TenMon,
                DuongDanAnh = c.DuongDanAnh,
                DanhGia = 5,
                NoiDung = "",
                ChamRieng = false
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Gui(PhanHoiViewModel model)
    {
        var token = HttpContext.Session.GetString("Token");
        var customerId = HttpContext.Session.GetString("MaKhachHang");

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(customerId))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập để gửi đánh giá.";
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        // Bắt buộc phải có mã đơn hàng
        if (string.IsNullOrWhiteSpace(model.MaDonHang))
        {
            ModelState.AddModelError(nameof(model.MaDonHang), "Vui lòng chọn đơn hàng bạn muốn đánh giá.");
        }

        // 1. Kiểm tra đơn hàng có thực sự thuộc khách hàng và đã hoàn tất không
        var doneOrders = await donHangService.GetOrdersForCustomerAsync(customerId, "done");
        var matchedOrder = doneOrders.FirstOrDefault(o => o.MaDonHang == model.MaDonHang);
        if (matchedOrder == null)
        {
            ModelState.AddModelError(nameof(model.MaDonHang), "Đơn hàng không hợp lệ hoặc chưa hoàn tất.");
        }

        // 2. Chặn đánh giá lại lần 2: Đơn hàng chỉ được đánh giá 1 lần duy nhất qua form tạo mới
        bool daDanhGia = await phanHoiService.IsOrderReviewedAsync(model.MaDonHang);
        if (daDanhGia)
        {
            TempData["ThongBao"] = $"Đơn hàng #{model.MaDonHang} đã có đánh giá. Bạn có thể xem hoặc chỉnh sửa tại đây.";
            return RedirectToAction(nameof(ChiTiet), new { maHoaDon = model.MaDonHang });
        }

        if (!ModelState.IsValid)
        {
            model.DonHangs = doneOrders.Where(o => !o.DaDanhGia).ToList();
            model.DonHangHienTai = await donHangService.GetOrderDetailAsync(model.MaDonHang!);
            if (model.ChiTietMonDanhGia.Count == 0 && model.DonHangHienTai != null)
            {
                model.ChiTietMonDanhGia = (model.DonHangHienTai.ChiTiet ?? new()).Select(c => new DishReviewInputItem
                {
                    MaMon = c.MaMon,
                    TenMon = c.TenMon,
                    DuongDanAnh = c.DuongDanAnh,
                    DanhGia = model.DanhGia,
                    NoiDung = "",
                    ChamRieng = false
                }).ToList();
            }
            return View("DanhGia", model);
        }

        // Gửi batch đánh giá đơn hàng: Master (DANH_GIA) + Detail (CHI_TIET_DANH_GIA)
        var result = await phanHoiService.SubmitOrderReviewBatchAsync(
            customerId,
            model.MaDonHang!,
            model.DanhGia,
            model.NoiDung,
            model.ChiTietMonDanhGia
        );

        if (result.Success)
        {
            TempData["ThongBao"] = $"Cảm ơn bạn đã gửi đánh giá cho đơn hàng #{model.MaDonHang}!";
            return RedirectToAction(nameof(ChiTiet), new { maHoaDon = model.MaDonHang });
        }
        else if (TaiKhoanBiKhoaHelper.LaTaiKhoanBiKhoa(result.Message))
        {
            return this.DangXuatVaVeDangNhap();
        }
        else
        {
            TempData["Loi"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> ChiTiet(string maHoaDon)
    {
        var token = HttpContext.Session.GetString("Token");
        var customerId = HttpContext.Session.GetString("MaKhachHang");
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(customerId))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập để xem chi tiết đánh giá.";
            return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = Request.Path + Request.QueryString });
        }

        if (string.IsNullOrEmpty(maHoaDon))
        {
            return RedirectToAction(nameof(Index));
        }

        var detail = await phanHoiService.GetOrderReviewDetailAsync(maHoaDon);
        if (detail == null)
        {
            TempData["Loi"] = $"Chưa tìm thấy đánh giá cho đơn hàng #{maHoaDon}.";
            return RedirectToAction(nameof(Index));
        }

        return View(detail);
    }

    [HttpGet]
    public async Task<IActionResult> ChinhSua(string maHoaDon)
    {
        var token = HttpContext.Session.GetString("Token");
        var customerId = HttpContext.Session.GetString("MaKhachHang");
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(customerId))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập để chỉnh sửa đánh giá.";
            return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = Request.Path + Request.QueryString });
        }

        if (string.IsNullOrEmpty(maHoaDon))
        {
            return RedirectToAction(nameof(Index));
        }

        var detail = await phanHoiService.GetOrderReviewDetailAsync(maHoaDon);
        if (detail == null)
        {
            TempData["Loi"] = $"Chưa có đánh giá cho đơn hàng #{maHoaDon} để chỉnh sửa.";
            return RedirectToAction(nameof(Index));
        }

        var orderDetail = await donHangService.GetOrderDetailAsync(maHoaDon);

        var model = new PhanHoiViewModel
        {
            MaDonHang = maHoaDon,
            DanhGia = detail.SoSaoChung,
            NoiDung = detail.NoiDungChung ?? "",
            DonHangHienTai = orderDetail,
            // Món có số sao → khách đã chấm riêng; món NULL → hiển thị theo sao chung
            ChiTietMonDanhGia = detail.ChiTietMon.Select(m => new DishReviewInputItem
            {
                MaMon = m.MaMon,
                TenMon = m.TenMon,
                DuongDanAnh = m.DuongDanAnh,
                DanhGia = m.SoSao ?? detail.SoSaoChung,
                NoiDung = m.NoiDung ?? "",
                ChamRieng = m.SoSao.HasValue
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChinhSua(PhanHoiViewModel model)
    {
        var token = HttpContext.Session.GetString("Token");
        var customerId = HttpContext.Session.GetString("MaKhachHang");
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(customerId))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập để cập nhật đánh giá.";
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        if (string.IsNullOrWhiteSpace(model.MaDonHang))
        {
            return RedirectToAction(nameof(Index));
        }

        var result = await phanHoiService.UpdateOrderReviewBatchAsync(
            customerId,
            model.MaDonHang,
            model.DanhGia,
            model.NoiDung,
            model.ChiTietMonDanhGia
        );

        if (result.Success)
        {
            TempData["ThongBao"] = $"Cập nhật đánh giá cho đơn hàng #{model.MaDonHang} thành công!";
            return RedirectToAction(nameof(ChiTiet), new { maHoaDon = model.MaDonHang });
        }
        if (TaiKhoanBiKhoaHelper.LaTaiKhoanBiKhoa(result.Message))
        {
            return this.DangXuatVaVeDangNhap();
        }

        TempData["Loi"] = result.Message;
        model.DonHangHienTai = await donHangService.GetOrderDetailAsync(model.MaDonHang);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PhanHoiLai(string maDanhGia, string noiDung, string? returnUrl = null)
    {
        var token = HttpContext.Session.GetString("Token");
        var customerId = HttpContext.Session.GetString("MaKhachHang");
        var customerName = HttpContext.Session.GetString("HoTen") ?? "Khách hàng";

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(customerId))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập để gửi phản hồi.";
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        if (string.IsNullOrWhiteSpace(maDanhGia) || string.IsNullOrWhiteSpace(noiDung))
        {
            TempData["Loi"] = "Nội dung phản hồi không được để trống.";
            return Redirect(returnUrl ?? Url.Action(nameof(Index))!);
        }

        bool success = await phanHoiService.AddReplyAsync(maDanhGia, noiDung, customerName, customerId);
        if (success)
        {
            TempData["ThongBao"] = "Phản hồi của bạn đã được gửi thành công!";
        }
        else
        {
            TempData["Loi"] = "Không thể gửi phản hồi lúc này. Vui lòng thử lại sau.";
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }
}

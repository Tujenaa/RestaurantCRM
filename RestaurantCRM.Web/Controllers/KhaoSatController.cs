using Microsoft.AspNetCore.Mvc;
using RestaurantCRM.Web.Models;
using RestaurantCRM.Web.Services;

namespace RestaurantCRM.Web.Controllers;

public class KhaoSatController(KhaoSatService api) : Controller
{
    private string? MaKhachHang => HttpContext.Session.GetString("MaKhachHang");

    private bool DaTraLoi(string maKhaoSat) =>
        HttpContext.Session.GetString("KS_DONE_" + maKhaoSat) == "1";

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var ks = await api.GetHienTaiAsync();
            ViewBag.DaTraLoi = ks is not null && DaTraLoi(ks.MaKhaoSat);
            return View(ks);
        }
        catch (HttpRequestException)
        {
            TempData["ThongBao"] = "Không kết nối được API. Hãy chạy RestaurantCRM.API rồi thử lại.";
            return View((KhaoSatViewModel?)null);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(MaKhachHang))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập để tham gia khảo sát.";
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        try
        {
            var ks = await api.GetHienTaiAsync();
            if (ks is null || ks.MaKhaoSat != id)
            {
                TempData["ThongBao"] = "Khảo sát đã đóng hoặc không còn tồn tại.";
                return RedirectToAction(nameof(Index));
            }
            if (DaTraLoi(ks.MaKhaoSat))
            {
                TempData["ThongBao"] = "Bạn đã hoàn thành khảo sát này rồi.";
                return RedirectToAction(nameof(Index));
            }
            return View(ks);
        }
        catch (HttpRequestException)
        {
            TempData["ThongBao"] = "Không kết nối được API.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Gui(GuiPhieuTraLoiViewModel model)
    {
        var maKh = MaKhachHang;
        if (string.IsNullOrEmpty(maKh))
        {
            TempData["ThongBao"] = "Vui lòng đăng nhập để tham gia khảo sát.";
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        KhaoSatViewModel? ks;
        try
        {
            ks = await api.GetHienTaiAsync();
        }
        catch (HttpRequestException)
        {
            TempData["ThongBao"] = "Không kết nối được API.";
            return RedirectToAction(nameof(Index));
        }

        if (ks is null || ks.MaKhaoSat != model.MaKhaoSat)
        {
            TempData["ThongBao"] = "Khảo sát đã đóng, không thể gửi câu trả lời.";
            return RedirectToAction(nameof(Index));
        }
        if (DaTraLoi(ks.MaKhaoSat))
        {
            TempData["ThongBao"] = "Bạn đã hoàn thành khảo sát này rồi.";
            return RedirectToAction(nameof(Index));
        }

        var sach = new GuiPhieuTraLoiViewModel { MaKhaoSat = ks.MaKhaoSat };
        foreach (var q in ks.CauHoi)
        {
            var a = model.Answers.FirstOrDefault(x => x.MaCauHoi == q.MaCauHoi);

            if (q.IsTuDien)
            {
                var text = a?.NoiDungTuDien?.Trim();
                if (text is not null && text.Length > 1000)
                    ModelState.AddModelError("", $"Câu \"{q.NoiDungCauHoi}\" tối đa 1000 ký tự.");
                sach.Answers.Add(new CauTraLoiInput { MaCauHoi = q.MaCauHoi, NoiDungTuDien = text });
                continue;
            }

            var chon = (a?.MaTuyChons ?? new List<string>())
                .Where(t => q.TuyChon.Any(o => o.MaTuyChon == t))
                .Distinct()
                .ToList();

            if (chon.Count == 0)
                ModelState.AddModelError("", $"Vui lòng trả lời câu: {q.NoiDungCauHoi}");
            else if (!q.IsNhieu && chon.Count > 1)
                ModelState.AddModelError("", $"Câu \"{q.NoiDungCauHoi}\" chỉ được chọn một đáp án.");

            sach.Answers.Add(new CauTraLoiInput { MaCauHoi = q.MaCauHoi, MaTuyChons = chon });
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Posted = model;
            return View("Details", ks);
        }

        try
        {
            var (ok, message) = await api.GuiAsync(maKh, sach);
            if (ok)
            {
                HttpContext.Session.SetString("KS_DONE_" + ks.MaKhaoSat, "1");
                return RedirectToAction(nameof(Submit));
            }
            ModelState.AddModelError("", message);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Không kết nối được API, vui lòng thử lại.");
        }

        ViewBag.Posted = model;
        return View("Details", ks);
    }

    [HttpGet]
    public IActionResult Submit() => View();
}
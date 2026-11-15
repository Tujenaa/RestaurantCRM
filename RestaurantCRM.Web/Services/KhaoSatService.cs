using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RestaurantCRM.Web.DTOs;
using RestaurantCRM.Web.Models;

namespace RestaurantCRM.Web.Services;

public class KhaoSatService(IHttpClientFactory factory)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<(bool Ok, string Message)> GuiAsync(string maKhachHang, GuiPhieuTraLoiViewModel model)
    {
        var body = new SurveyAnswerRequest
        {
            MaKhachHang = maKhachHang,
            MaKhaoSat = model.MaKhaoSat
        };

        foreach (var a in model.Answers)
        {
            if (a.MaTuyChons.Count > 0)
            {
                // Mỗi câu 1 phần tử; câu chọn nhiều gửi kèm danh sách đáp án
                body.Answers.Add(new AnswerItem { MaCauHoi = a.MaCauHoi, MaTuyChons = a.MaTuyChons.ToList() });
            }
            else if (!string.IsNullOrWhiteSpace(a.NoiDungTuDien))
            {
                body.Answers.Add(new AnswerItem { MaCauHoi = a.MaCauHoi, NoiDungTuDien = a.NoiDungTuDien!.Trim() });
            }
        }

        var client = factory.CreateClient("ApiClient");
        var res = await client.PostAsJsonAsync("/api/KhaoSat/TraLoi", body, Json);
        if (res.IsSuccessStatusCode)
            return (true, "Cảm ơn bạn đã hoàn thành khảo sát!");

        var err = (await res.Content.ReadAsStringAsync()).Trim().Trim('"');
        if (string.IsNullOrWhiteSpace(err) || err.StartsWith("{") || err.Length > 200)
            err = "Gửi khảo sát thất bại, vui lòng thử lại.";
        return (false, err);
    }
    public async Task<List<KhaoSatItemViewModel>> GetDanhSachAsync(string? maKhachHang)
    {
        var client = factory.CreateClient("ApiClient");
        var url = "/api/KhaoSat/DanhSach" + (string.IsNullOrEmpty(maKhachHang) ? "" : $"?maKhachHang={Uri.EscapeDataString(maKhachHang)}");
        var res = await client.GetAsync(url);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<KhaoSatItemViewModel>>(Json) ?? new();
    }

    public async Task<List<LichSuKhaoSatItem>> GetLichSuAsync(string maKhachHang)
    {
        var client = factory.CreateClient("ApiClient");
        var res = await client.GetAsync($"/api/KhaoSat/LichSu?maKhachHang={Uri.EscapeDataString(maKhachHang)}");
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<LichSuKhaoSatItem>>(Json) ?? new();
    }

    // ── Hồ sơ sở thích ───────────────────────────────────────────────────

    public async Task<SoThichViewModel> GetSoThichAsync(string maKhachHang)
    {
        var client = factory.CreateClient("ApiClient");
        var res = await client.GetAsync($"/api/KhaoSat/SoThich?maKhachHang={Uri.EscapeDataString(maKhachHang)}");
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<SoThichViewModel>(Json) ?? new SoThichViewModel();
    }

    public async Task<(bool Ok, string Message)> LuuSoThichAsync(string maKhachHang, SoThichFormInput input)
    {
        var body = new
        {
            MaKhachHang = maKhachHang,
            CauTraLoi = input.CauTraLoi.Select(c => new
            {
                c.MaCauHoi,
                MaTuyChons = c.MaTuyChons.Where(m => !string.IsNullOrWhiteSpace(m)).Distinct().ToList(),
                NoiDung = string.IsNullOrWhiteSpace(c.NoiDung) ? null : c.NoiDung.Trim()
            }).ToList()
        };
        var client = factory.CreateClient("ApiClient");
        var res = await client.PutAsJsonAsync("/api/KhaoSat/SoThich", body, Json);
        if (res.IsSuccessStatusCode) return (true, "Đã lưu sở thích của bạn.");
        return (false, await DocLoiAsync(res, "Lưu sở thích thất bại, vui lòng thử lại."));
    }

    public async Task<MonYeuThichViewModel> GetMonYeuThichAsync(string? maKhachHang)
    {
        var client = factory.CreateClient("ApiClient");
        var url = "/api/KhaoSat/SoThich/MonYeuThich" + (string.IsNullOrEmpty(maKhachHang) ? "" : $"?maKhachHang={Uri.EscapeDataString(maKhachHang)}");
        var res = await client.GetAsync(url);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<MonYeuThichViewModel>(Json) ?? new MonYeuThichViewModel();
    }

    public async Task<(bool Ok, string Message)> DatMonYeuThichAsync(string maKhachHang, string maMon, bool yeuThich)
    {
        var client = factory.CreateClient("ApiClient");
        var res = await client.PutAsJsonAsync($"/api/KhaoSat/SoThich/MonYeuThich/{Uri.EscapeDataString(maMon)}",
            new { MaKhachHang = maKhachHang, YeuThich = yeuThich }, Json);
        if (res.IsSuccessStatusCode)
            return (true, yeuThich ? "Đã thêm vào món yêu thích." : "Đã bỏ khỏi món yêu thích.");
        return (false, await DocLoiAsync(res, "Không cập nhật được món yêu thích."));
    }

    private static async Task<string> DocLoiAsync(HttpResponseMessage res, string macDinh)
    {
        var err = (await res.Content.ReadAsStringAsync()).Trim().Trim('"');
        return string.IsNullOrWhiteSpace(err) || err.StartsWith("{") || err.Length > 200 ? macDinh : err;
    }

    public async Task<KhaoSatViewModel?> GetByIdAsync(string id, string? maKhachHang = null)
    {
        var client = factory.CreateClient("ApiClient");
        var url = $"/api/KhaoSat/{Uri.EscapeDataString(id)}"
                  + (string.IsNullOrEmpty(maKhachHang) ? "" : $"?maKhachHang={Uri.EscapeDataString(maKhachHang)}");
        var res = await client.GetAsync(url);
        if (res.StatusCode == HttpStatusCode.NotFound) return null;
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<KhaoSatViewModel>(Json);
    }
}

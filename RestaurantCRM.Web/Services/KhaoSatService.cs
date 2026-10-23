using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RestaurantCRM.Web.DTOs;
using RestaurantCRM.Web.Models;

namespace RestaurantCRM.Web.Services;

public class KhaoSatService(IHttpClientFactory factory)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<KhaoSatViewModel?> GetHienTaiAsync()
    {
        var client = factory.CreateClient("ApiClient");
        var res = await client.GetAsync("/api/KhaoSat/HienTai");
        if (res.StatusCode == HttpStatusCode.NotFound) return null;
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<KhaoSatViewModel>(Json);
    }

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
                foreach (var t in a.MaTuyChons)
                    body.Answers.Add(new AnswerItem { MaCauHoi = a.MaCauHoi, MaTuyChon = t });
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

    public async Task<KhaoSatViewModel?> GetByIdAsync(string id)
    {
        var client = factory.CreateClient("ApiClient");
        var res = await client.GetAsync($"/api/KhaoSat/{Uri.EscapeDataString(id)}");
        if (res.StatusCode == HttpStatusCode.NotFound) return null;
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<KhaoSatViewModel>(Json);
    }
}

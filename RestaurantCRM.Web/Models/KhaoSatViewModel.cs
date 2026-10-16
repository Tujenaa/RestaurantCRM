

namespace RestaurantCRM.Web.Models;

public class KhaoSatViewModel
{
    public string MaKhaoSat { get; set; } = "";
    public string TieuDe { get; set; } = "";
    public List<CauHoiViewModel> CauHoi { get; set; } = new();
}

public class CauHoiViewModel
{
    public string MaCauHoi { get; set; } = "";
    public string NoiDungCauHoi { get; set; } = "";
    public string LoaiCauHoi { get; set; } = "";   // DB hiện chỉ có: Option | Text
    public List<TuyChonViewModel> TuyChon { get; set; } = new();

    public bool IsTuDien => LoaiCauHoi.Equals("Text", StringComparison.OrdinalIgnoreCase);
    public bool IsNhieu => false;   // schema chưa có loại chọn nhiều
}

public class TuyChonViewModel
{
    public string MaTuyChon { get; set; } = "";
    public string NoiDungTuyChon { get; set; } = "";
}

// Dữ liệu form khách gửi lên
public class GuiPhieuTraLoiViewModel
{
    public string MaKhaoSat { get; set; } = "";
    public List<CauTraLoiInput> Answers { get; set; } = new();
}

public class CauTraLoiInput
{
    public string MaCauHoi { get; set; } = "";
    public List<string> MaTuyChons { get; set; } = new();
    public string? NoiDungTuDien { get; set; }
}
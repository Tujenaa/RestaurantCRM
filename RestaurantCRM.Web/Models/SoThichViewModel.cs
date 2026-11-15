namespace RestaurantCRM.Web.Models;

/// <summary>Hồ sơ sở thích của khách (GET api/KhaoSat/SoThich).</summary>
public class SoThichViewModel
{
    public bool CoHoSo { get; set; }
    public string MaKhaoSat { get; set; } = "";
    public bool CoMonYeuThich { get; set; }
    public bool DaKhaiBao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public List<SoThichCauHoiViewModel> CauHoi { get; set; } = new();

    /// <summary>Vừa đăng ký xong: hiện nút "Để sau".</summary>
    public bool KhachMoi { get; set; }
}

public class SoThichCauHoiViewModel
{
    public string MaCauHoi { get; set; } = "";
    public string NoiDungCauHoi { get; set; } = "";
    public string LoaiCauHoi { get; set; } = "";
    public bool ChonNhieu { get; set; }
    public bool LaMonYeuThich { get; set; }
    public bool LaDiUng { get; set; }
    public List<SoThichTuyChonViewModel> TuyChon { get; set; } = new();
    public string? NoiDung { get; set; }

    public bool IsTuDien => LoaiCauHoi.Equals("Text", StringComparison.OrdinalIgnoreCase);
}

public class SoThichTuyChonViewModel
{
    public string MaTuyChon { get; set; } = "";
    public string NoiDungTuyChon { get; set; } = "";
    public string? MaMon { get; set; }
    public string? TenMon { get; set; }
    public string? DuongDanAnh { get; set; }
    public bool NgungBan { get; set; }
    public bool DaChon { get; set; }
}

/// <summary>Dữ liệu form "Sở thích của tôi" gửi lên.</summary>
public class SoThichFormInput
{
    public List<SoThichCauTraLoiInput> CauTraLoi { get; set; } = new();
}

public class SoThichCauTraLoiInput
{
    public string MaCauHoi { get; set; } = "";
    public List<string> MaTuyChons { get; set; } = new();
    public string? NoiDung { get; set; }
}

/// <summary>Các món khách đang ♥ (GET api/KhaoSat/SoThich/MonYeuThich).</summary>
public class MonYeuThichViewModel
{
    public bool CoMonYeuThich { get; set; }
    public List<string> MaMons { get; set; } = new();
}

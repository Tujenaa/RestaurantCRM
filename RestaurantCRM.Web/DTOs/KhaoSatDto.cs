namespace RestaurantCRM.Web.DTOs;

public class SurveyAnswerRequest
{
    public string MaKhachHang { get; set; } = "";
    public string MaKhaoSat { get; set; } = "";
    public List<AnswerItem> Answers { get; set; } = new();
}

public class AnswerItem
{
    public string MaCauHoi { get; set; } = "";
    public string? MaTuyChon { get; set; }
    public string? NoiDungTuDien { get; set; }
}
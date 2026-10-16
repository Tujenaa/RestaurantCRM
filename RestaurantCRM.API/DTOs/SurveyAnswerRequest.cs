namespace RestaurantCRM.API.DTOs
{
    public class SurveyAnswerRequest
    {
        public string MaKhachHang { get; set; } = null!;
        public string MaKhaoSat { get; set; } = null!;
        public List<AnswerItem> Answers { get; set; } = new List<AnswerItem>();
    }

    public class AnswerItem
    {
        public string MaCauHoi { get; set; } = null!;
        public string? MaTuyChon { get; set; }
        public string? NoiDungTuDien { get; set; }
    }
}

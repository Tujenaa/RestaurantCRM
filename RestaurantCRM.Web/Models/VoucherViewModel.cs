namespace RestaurantCRM.Web.Models;

public class VoucherViewModel
{
    public string MaKmvoucher { get; set; } = "";
    public string? MaVoucher { get; set; }
    public string? TenChuongTrinh { get; set; }
    public double? PhanTramGiam { get; set; }
    public double? TienGiam { get; set; }
    public double? GiaTriDonToiThieu { get; set; }
    public double? GiamToiDa { get; set; }
    public string? PhamViApDung { get; set; }
    public string? MaLoaiMon { get; set; }
    public string? TenLoaiMon { get; set; }
    public string? MaMon { get; set; }
    public string? TenMon { get; set; }
    public int? SoLuongToiThieu { get; set; }
    public bool? ChoPhepCongDon { get; set; }
    public DateOnly? NgayBatDau { get; set; }
    public DateOnly? NgayKetThuc { get; set; }

    public string DisplayCode => !string.IsNullOrWhiteSpace(MaVoucher) ? MaVoucher : MaKmvoucher;

    public string DiscountLabel
    {
        get
        {
            if (PhanTramGiam.HasValue && PhanTramGiam.Value > 0)
            {
                return $"-{PhanTramGiam.Value:0.#}%";
            }
            if (TienGiam.HasValue && TienGiam.Value > 0)
            {
                if (TienGiam.Value >= 1000)
                {
                    var k = TienGiam.Value / 1000.0;
                    return k % 1 == 0 ? $"-{k:0}k" : $"-{k:0.#}k";
                }
                return $"-{TienGiam.Value:N0}đ";
            }
            return "Ưu đãi";
        }
    }

    public string ConditionLabel
    {
        get
        {
            var parts = new List<string>();
            if (GiaTriDonToiThieu.HasValue && GiaTriDonToiThieu.Value > 0)
            {
                parts.Add($"Đơn từ {GiaTriDonToiThieu.Value:N0}đ");
            }
            if (GiamToiDa.HasValue && GiamToiDa.Value > 0)
            {
                parts.Add($"Tối đa {GiamToiDa.Value:N0}đ");
            }
            if (!string.IsNullOrWhiteSpace(TenLoaiMon))
            {
                parts.Add($"Nhóm {TenLoaiMon}");
            }
            else if (!string.IsNullOrWhiteSpace(TenMon))
            {
                parts.Add($"Món {TenMon}");
            }
            return parts.Count > 0 ? string.Join(" • ", parts) : "Áp dụng toàn đơn";
        }
    }

    public bool IsEligible(decimal subtotal)
    {
        if (GiaTriDonToiThieu.HasValue && GiaTriDonToiThieu.Value > 0)
        {
            return subtotal >= (decimal)GiaTriDonToiThieu.Value;
        }
        return true;
    }
}

public class KiemTraVoucherApiResponse
{
    public bool Valid { get; set; }
    public string? Message { get; set; }
    public string? MaVoucher { get; set; }
    public string? MaKmvoucher { get; set; }
    public string? TenChuongTrinh { get; set; }
    public double? PhanTramGiam { get; set; }
    public double? TienGiam { get; set; }
    public double? GiaTriDonToiThieu { get; set; }
    public double? GiamToiDa { get; set; }
    public string? PhamViApDung { get; set; }
    public string? MaLoaiMon { get; set; }
    public string? TenLoaiMon { get; set; }
    public string? MaMon { get; set; }
    public string? TenMon { get; set; }
    public double GiaTriGiamThucTe { get; set; }
    public double TongTienSauGiam { get; set; }
}

public class AppliedVoucherSessionData
{
    public string Code { get; set; } = "";
    public string? MaKmvoucher { get; set; }
    public string? TenChuongTrinh { get; set; }
    public double? PhanTramGiam { get; set; }
    public double? TienGiam { get; set; }
    public double? GiaTriDonToiThieu { get; set; }
    public double? GiamToiDa { get; set; }
    public string? PhamViApDung { get; set; }
    public string? MaLoaiMon { get; set; }
    public string? MaMon { get; set; }
    public decimal LastCalculatedDiscount { get; set; }
}

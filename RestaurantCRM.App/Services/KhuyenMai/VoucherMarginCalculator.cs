using System;

namespace RestaurantCRM.AdminApp.Services.KhuyenMai
{
    public enum MarginHealth
    {
        Empty,
        NoRecipe,
        NegativeMargin,
        ThinMargin,
        SafeMargin
    }

    public class VoucherMarginResult
    {
        public double DonGiaGoc { get; set; }
        public double GiaVon { get; set; }
        public double GiaSauGiam { get; set; }
        public double TietKiem { get; set; }
        public double LaiGop { get; set; }
        public double TyLeLai { get; set; }
        public double TyLeFoodCost { get; set; }
        public MarginHealth Health { get; set; }
        public string WarningText { get; set; }
        public string LaiGopDisplayText { get; set; }

        public static VoucherMarginResult CreateEmpty()
        {
            return new VoucherMarginResult
            {
                Health = MarginHealth.Empty,
                WarningText = "Vui lòng chọn món ăn để xem dự tính giá sau giảm.",
                LaiGopDisplayText = "0 đ"
            };
        }
    }

    /// <summary>
    /// Bộ tính toán biên lợi nhuận, chi phí nguyên liệu (Food Cost) và đánh giá độ an toàn tài chính của Voucher ưu đãi theo món.
    /// Tính toán chuẩn xác có xét đến cả Mức giảm tối đa (Trần giảm giá).
    /// </summary>
    public static class VoucherMarginCalculator
    {
        public static VoucherMarginResult Calculate(double goc, double giaVon, int loaiGiamIndex, double giam, double giamToiDa = 0)
        {
            double tienGiam = 0;
            if (loaiGiamIndex == 0) // % (Phần trăm)
            {
                tienGiam = goc * (giam / 100.0);
                if (giamToiDa > 0 && tienGiam > giamToiDa)
                {
                    tienGiam = giamToiDa;
                }
            }
            else // VND (Tiền mặt / Giảm tối đa)
            {
                double discountAmt = giamToiDa > 0 ? giamToiDa : giam;
                tienGiam = Math.Min(goc, discountAmt);
            }

            double sauGiam = Math.Max(0, goc - tienGiam);
            double tietKiem = goc - sauGiam;
            double laiGop = sauGiam - giaVon;
            double tyLeLai = sauGiam > 0 ? (laiGop / sauGiam) * 100.0 : 0;
            double tyLeFoodCost = goc > 0 ? (giaVon / goc) * 100.0 : 0;

            var result = new VoucherMarginResult
            {
                DonGiaGoc = goc,
                GiaVon = giaVon,
                GiaSauGiam = sauGiam,
                TietKiem = tietKiem,
                LaiGop = laiGop,
                TyLeLai = tyLeLai,
                TyLeFoodCost = tyLeFoodCost
            };

            if (giaVon <= 0)
            {
                result.Health = MarginHealth.NoRecipe;
                result.LaiGopDisplayText = "Chưa xác định";
                result.WarningText = "[CHƯA THIẾT LẬP ĐỊNH LƯỢNG]: Món này chưa có công thức định lượng nguyên liệu.\nHãy cập nhật định lượng trong Thực đơn để kiểm soát giá vốn chính xác.";
            }
            else if (sauGiam < giaVon)
            {
                double tienLo = giaVon - sauGiam;
                result.Health = MarginHealth.NegativeMargin;
                result.LaiGopDisplayText = $"-{tienLo:N0} đ (ÂM VỐN {tyLeLai:N1}%)";
                result.WarningText = $"[BÁN LỖ TRỰC TIẾP]: Giá sau ưu đãi ({sauGiam:N0} đ) THẤP HƠN Giá vốn ({giaVon:N0} đ)!\nNhà hàng sẽ chịu lỗ {tienLo:N0} đ trên mỗi đĩa bán ra. Hệ thống sẽ chặn lưu ưu đãi này.";
            }
            else if (tyLeLai < 25.0)
            {
                result.Health = MarginHealth.ThinMargin;
                result.LaiGopDisplayText = $"+{laiGop:N0} đ (Lãi mỏng {tyLeLai:N1}%)";
                result.WarningText = $"[CẢNH BÁO BIÊN LÃI MỎNG]: Tỷ suất lãi gộp chỉ đạt {tyLeLai:N1}% (+{laiGop:N0} đ/đĩa).\nKhoản lời này khó bù đủ chi phí vận hành. Nên giới hạn số lượng hoặc giờ áp dụng.";
            }
            else
            {
                result.Health = MarginHealth.SafeMargin;
                result.LaiGopDisplayText = $"+{laiGop:N0} đ (Tỷ suất lãi {tyLeLai:N1}%)";
                result.WarningText = $"[AN TOÀN DOANH THU]: Biên lợi nhuận gộp đạt {tyLeLai:N1}% (+{laiGop:N0} đ/đĩa).\nĐảm bảo bù đắp chi phí vận hành và giữ mức doanh thu có lời ổn định.";
            }

            return result;
        }
    }
}

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace RestaurantCRM.AdminApp.Controls.BaoCao
{
    /// <summary>
    /// Thẻ KPI tối giản cho trang Báo cáo doanh thu: nhãn nhỏ viết hoa, số lớn, dòng mô tả.
    /// Không icon, không mũi tên. Cỡ chữ số tự thu nhỏ để không bao giờ tràn khung, hỗ trợ tooltip đầy đủ.
    /// </summary>
    public class BaoCaoKpiTile : RoundedPanel
    {
        private static readonly CultureInfo Vi = new CultureInfo("vi-VN");
        private static readonly ToolTip s_tip = new ToolTip { InitialDelay = 120, ReshowDelay = 80 };
        private readonly string _title;
        private string _value;
        private string _sub;
        private Color _valueColor = BcTheme.Text;
        private Color _subColor;

        /// <param name="icon">Giữ lại để tương thích chữ ký cũ – không còn được vẽ.</param>
        public BaoCaoKpiTile(string title, string value, string sub, string icon, Color accent)
        {
            _title = title ?? "";
            _value = value ?? "";
            _sub = sub ?? "";
            _subColor = accent;
            Radius = 16;
            Shadow = 0;
            FillColor = BcTheme.Card;
            BorderColor = BcTheme.Line;
            Margin = new Padding(0);
            UpdateTooltip();
        }

        private void UpdateTooltip()
        {
            s_tip.SetToolTip(this, _title + ": " + _value + (string.IsNullOrEmpty(_sub) ? "" : " (" + _sub + ")"));
        }

        public void SetValue(string val, string sub = null, Color? valueColor = null, Color? subColor = null)
        {
            _value = val ?? "";
            if (sub != null) _sub = sub;
            if (valueColor.HasValue) _valueColor = valueColor.Value;
            if (subColor.HasValue) _subColor = subColor.Value;
            UpdateTooltip();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            int w = Width - 36;
            if (w <= 10) return;

            BcTheme.DrawText(g, _title.ToUpper(Vi), BcTheme.F(8.25F, FontStyle.Bold), new Rectangle(18, 14, w, 18), BcTheme.Muted, TextFormatFlags.EndEllipsis);

            // Tự thu nhỏ cỡ chữ cho số tiền lớn
            float size = 17.5F;
            while (size > 10F && BcTheme.Measure(_value, BcTheme.F(size, FontStyle.Bold)) > w) size -= 0.75F;
            BcTheme.DrawText(g, _value, BcTheme.F(size, FontStyle.Bold), new Rectangle(18, 35, w, 34), _valueColor, TextFormatFlags.EndEllipsis);

            BcTheme.DrawText(g, _sub, BcTheme.F(8.75F), new Rectangle(18, 72, w, 20), _subColor, TextFormatFlags.EndEllipsis);
        }
    }
}

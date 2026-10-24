using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls.BaoCao
{
    // ================= LỚP THẺ KPI TỰ CO GIÃN THÔNG MINH (CHỐNG TRÀN SỐ TIỀN LỚN) =================
    public class BaoCaoKpiTile : RoundedPanel
    {
        private readonly Label _lblTitle;
        private readonly Label _lblValue;
        private readonly Label _lblSub;
        private readonly string _icon;
        private readonly Color _accent;

        public BaoCaoKpiTile(string title, string value, string sub, string icon, Color accent)
        {
            _icon = icon;
            _accent = accent;
            Radius = 14;
            FillColor = ThemeManager.Paper;
            BorderColor = Color.FromArgb(226, 232, 240);
            Margin = new Padding(0, 0, 10, 0);
            Dock = DockStyle.Fill;

            _lblTitle = new Label
            {
                Text = title.ToUpper(),
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Muted,
                Location = new Point(14, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(135, 18),
                AutoEllipsis = true,
                BackColor = Color.Transparent,
                UseMnemonic = false
            };
            Controls.Add(_lblTitle);

            _lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 15.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                Location = new Point(12, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(150, 42),
                AutoEllipsis = true,
                BackColor = Color.Transparent,
                UseMnemonic = false
            };
            Controls.Add(_lblValue);

            _lblSub = new Label
            {
                Text = "↑ " + sub,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = accent,
                Location = new Point(14, 82),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(150, 24),
                AutoEllipsis = true,
                BackColor = Color.Transparent,
                UseMnemonic = false
            };
            Controls.Add(_lblSub);

            Resize += (s, e) => UpdateLabelBounds();
            UpdateLabelBounds();
        }

        private void UpdateLabelBounds()
        {
            int titleWidth = Math.Max(40, Width - 46); // Chừa chỗ cho badge icon bên phải
            int contentWidth = Math.Max(40, Width - 24);
            if (_lblTitle != null)
            {
                _lblTitle.Width = titleWidth;
                _lblTitle.Height = 18;
            }
            if (_lblValue != null)
            {
                _lblValue.Width = contentWidth;
                _lblValue.Height = 42; // Chiều cao 42px hiển thị đầy đủ 100% font lớn không bị mất nửa số
            }
            if (_lblSub != null)
            {
                _lblSub.Width = contentWidth;
                _lblSub.Height = 24;
            }
        }

        public void SetValue(string val, string sub = null)
        {
            // Tự động điều chỉnh kích cỡ font thông minh theo độ dài số tiền để không bao giờ bị tràn khung
            if (_lblValue != null)
            {
                float fontSize = 15.5F;
                int len = val.Length;
                if (len > 17) fontSize = 10.5F;       // ví dụ: 120,500,000,000 đ
                else if (len > 14) fontSize = 12F;     // ví dụ: 1,500,000,000 đ
                else if (len > 11) fontSize = 13.5F;   // ví dụ: 350,000,000 đ

                _lblValue.Font = new Font("Segoe UI", fontSize, FontStyle.Bold);
                _lblValue.Height = 42;
                _lblValue.Text = val;
            }

            if (sub != null && _lblSub != null)
            {
                _lblSub.Text = "↑ " + sub;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Badge icon gọn gàng ở góc trên bên phải (28x28px)
            var badge = new Rectangle(Width - 38, 12, 28, 28);
            using (var path = ThemeManager.RoundedRect(badge, 8))
            using (var brush = new SolidBrush(ThemeManager.SoftFor(_accent)))
                g.FillPath(brush, path);

            using (var pen = new Pen(Color.FromArgb(40, _accent), 1f))
            using (var path = ThemeManager.RoundedRect(badge, 8))
                g.DrawPath(pen, path);

            using (var f = new Font(ThemeManager.EmojiFont, 10F))
                TextRenderer.DrawText(g, _icon, f, badge, _accent,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}

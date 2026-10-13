using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls {
    public enum HighlightCardType {
        TeamStats,
        Ideation,
        Training
    }

    /// <summary>
    /// Thẻ nổi bật phong cách SaaS hiện đại với hình minh họa vector, tiêu đề, mô tả và nút DETAILS.
    /// Thiết kế khớp 100% với giao diện tham khảo.
    /// </summary>
    public class UcHighlightCard : RoundedPanel {
        public event EventHandler DetailsClicked;

        private readonly HighlightCardType _type;
        private readonly string _title;
        private readonly string _sub;
        private readonly Color _accent;

        public UcHighlightCard(HighlightCardType type, string title, string sub, Color accent) {
            _type = type;
            _title = title;
            _sub = sub;
            _accent = accent;

            Size = new Size(220, 240);
            Radius = 14;
            FillColor = ThemeManager.Paper;
            BorderColor = ColorTranslator.FromHtml("#E2E8F0");
            Cursor = Cursors.Hand;

            var lblTitle = new Label {
                Text = _title,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                Location = new Point(16, 142),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            Controls.Add(lblTitle);

            var lblSub = new Label {
                Text = _sub,
                Font = new Font("Segoe UI", 8F),
                ForeColor = ThemeManager.Muted,
                Location = new Point(17, 166),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            Controls.Add(lblSub);

            var lblDetails = new Label {
                Text = "DETAILS    🔒",
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#8B5CF6"),
                Location = new Point(17, 204),
                AutoSize = true,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            Controls.Add(lblDetails);

            EventHandler onEnter = (s, e) => {
                BorderColor = Color.FromArgb(180, _accent);
                lblDetails.ForeColor = _accent;
                Invalidate();
            };
            EventHandler onLeave = (s, e) => {
                BorderColor = ColorTranslator.FromHtml("#E2E8F0");
                lblDetails.ForeColor = ColorTranslator.FromHtml("#8B5CF6");
                Invalidate();
            };
            EventHandler onClick = (s, e) => DetailsClicked?.Invoke(this, EventArgs.Empty);

            MouseEnter += onEnter;
            MouseLeave += onLeave;
            Click += onClick;
            lblTitle.Click += onClick;
            lblSub.Click += onClick;
            lblDetails.Click += onClick;
            lblTitle.MouseEnter += onEnter;
            lblSub.MouseEnter += onEnter;
            lblDetails.MouseEnter += onEnter;
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Vùng banner minh họa (Upper illustration area)
            var bannerRect = new Rectangle(14, 12, Width - 28, 118);
            using (var path = ThemeManager.RoundedRect(bannerRect, 10)) {
                // Nền gradient pastel nhẹ
                Color bgStart = ColorTranslator.FromHtml("#FAF5FF");
                Color bgEnd = ColorTranslator.FromHtml("#F3E8FF");
                if (_type == HighlightCardType.Ideation) {
                    bgStart = ColorTranslator.FromHtml("#FFFBEB");
                    bgEnd = ColorTranslator.FromHtml("#FEF3C7");
                } else if (_type == HighlightCardType.Training) {
                    bgStart = ColorTranslator.FromHtml("#EFF6FF");
                    bgEnd = ColorTranslator.FromHtml("#DBEAFE");
                }
                using (var brush = new LinearGradientBrush(bannerRect, bgStart, bgEnd, 45f))
                    g.FillPath(brush, path);
            }

            // Vẽ hình minh họa tùy theo loại thẻ
            int cx = bannerRect.Left + bannerRect.Width / 2;
            int cy = bannerRect.Top + bannerRect.Height / 2;

            if (_type == HighlightCardType.TeamStats) {
                DrawTeamStatsArt(g, cx, cy);
            } else if (_type == HighlightCardType.Ideation) {
                DrawIdeationArt(g, cx, cy);
            } else {
                DrawTrainingArt(g, cx, cy);
            }
        }

        private void DrawTeamStatsArt(Graphics g, int cx, int cy) {
            // Túi mua sắm / Briefcase màu tím hồng
            int bw = 46, bh = 42;
            var bagRect = new Rectangle(cx - bw / 2, cy - bh / 2 + 6, bw, bh);
            using (var brush = new LinearGradientBrush(bagRect, ColorTranslator.FromHtml("#7C3AED"), ColorTranslator.FromHtml("#C026D3"), 135f))
            using (var path = ThemeManager.RoundedRect(bagRect, 6))
                g.FillPath(brush, path);

            // Quai túi
            using (var pen = new Pen(ColorTranslator.FromHtml("#6D28D9"), 3))
                g.DrawArc(pen, cx - 12, cy - bh / 2 - 4, 24, 20, 180, 180);

            // Mũi tên tăng trưởng màu cam / vàng
            using (var pen = new Pen(ColorTranslator.FromHtml("#F59E0B"), 3)) {
                g.DrawLine(pen, cx - 10, cy + 18, cx + 8, cy - 2);
                g.DrawLine(pen, cx + 8, cy - 2, cx + 2, cy - 2);
                g.DrawLine(pen, cx + 8, cy - 2, cx + 8, cy + 4);
            }

            // Hình nhân vật nhỏ trừu tượng (Teal + Coral)
            using (var brush = new SolidBrush(ColorTranslator.FromHtml("#14B8A6")))
                g.FillEllipse(brush, cx - 28, cy + 4, 12, 12);
            using (var brush = new SolidBrush(ColorTranslator.FromHtml("#F43F5E")))
                g.FillEllipse(brush, cx + 18, cy - 8, 10, 10);
            using (var brush = new SolidBrush(ColorTranslator.FromHtml("#FBBF24")))
                g.FillEllipse(brush, cx - 22, cy - 14, 8, 8);
        }

        private void DrawIdeationArt(Graphics g, int cx, int cy) {
            // Bóng đèn vàng sáng tạo tỏa sáng
            int lw = 36, lh = 36;
            using (var brush = new LinearGradientBrush(new Rectangle(cx - 18, cy - 20, lw, lh), ColorTranslator.FromHtml("#FBBF24"), ColorTranslator.FromHtml("#F59E0B"), 90f))
                g.FillEllipse(brush, cx - 16, cy - 20, 32, 32);

            // Đuôi bóng đèn
            using (var brush = new SolidBrush(ColorTranslator.FromHtml("#9CA3AF"))) {
                g.FillRectangle(brush, cx - 8, cy + 10, 16, 6);
                g.FillRectangle(brush, cx - 5, cy + 16, 10, 4);
            }

            // Tia sáng & ý tưởng
            using (var pen = new Pen(ColorTranslator.FromHtml("#F59E0B"), 2)) {
                g.DrawLine(pen, cx, cy - 26, cx, cy - 32);
                g.DrawLine(pen, cx - 20, cy - 16, cx - 26, cy - 20);
                g.DrawLine(pen, cx + 20, cy - 16, cx + 26, cy - 20);
            }

            // Cuốn sách / tài liệu bên dưới
            using (var brush = new SolidBrush(ColorTranslator.FromHtml("#0D9488")))
                g.FillPolygon(brush, new[] {
                    new Point(cx - 24, cy + 24),
                    new Point(cx - 4, cy + 20),
                    new Point(cx - 4, cy + 28),
                    new Point(cx - 24, cy + 32)
                });
            using (var brush = new SolidBrush(ColorTranslator.FromHtml("#4F46E5")))
                g.FillPolygon(brush, new[] {
                    new Point(cx + 24, cy + 24),
                    new Point(cx + 4, cy + 20),
                    new Point(cx + 4, cy + 28),
                    new Point(cx + 24, cy + 32)
                });
        }

        private void DrawTrainingArt(Graphics g, int cx, int cy) {
            // Màn hình Laptop màu xanh navy
            int mw = 52, mh = 34;
            var monRect = new Rectangle(cx - mw / 2, cy - mh / 2 - 4, mw, mh);
            using (var path = ThemeManager.RoundedRect(monRect, 4))
            using (var brush = new SolidBrush(ColorTranslator.FromHtml("#1E293B")))
                g.FillPath(brush, path);

            // Màn hình bên trong
            var screenRect = new Rectangle(monRect.Left + 3, monRect.Top + 3, mw - 6, mh - 6);
            using (var brush = new SolidBrush(ColorTranslator.FromHtml("#E0F2FE")))
                g.FillRectangle(brush, screenRect);

            // Đồ thị bên trong màn hình
            using (var pen = new Pen(ColorTranslator.FromHtml("#0284C7"), 2)) {
                g.DrawLine(pen, screenRect.Left + 4, screenRect.Bottom - 6, screenRect.Left + 14, screenRect.Top + 8);
                g.DrawLine(pen, screenRect.Left + 14, screenRect.Top + 8, screenRect.Left + 26, screenRect.Bottom - 10);
                g.DrawLine(pen, screenRect.Left + 26, screenRect.Bottom - 10, screenRect.Right - 4, screenRect.Top + 4);
            }

            // Chân đế laptop
            using (var brush = new SolidBrush(ColorTranslator.FromHtml("#64748B")))
                g.FillPolygon(brush, new[] {
                    new Point(cx - 32, cy + mh / 2),
                    new Point(cx + 32, cy + mh / 2),
                    new Point(cx + 28, cy + mh / 2 + 5),
                    new Point(cx - 28, cy + mh / 2 + 5)
                });

            // Người đang học / Thao tác (chấm tròn đỏ/coral + thân áo)
            using (var brush = new SolidBrush(ColorTranslator.FromHtml("#EC4899")))
                g.FillEllipse(brush, cx + 22, cy - 8, 12, 12);
            using (var brush = new SolidBrush(ColorTranslator.FromHtml("#8B5CF6")))
                g.FillEllipse(brush, cx - 28, cy + 6, 10, 10);
        }
    }
}

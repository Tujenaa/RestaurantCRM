using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls {
    /// <summary>
    /// Biểu đồ tăng trưởng dạng Area Chart với các đỉnh núi mượt mà nhiều lớp màu (Tím, Hồng san hô, Hổ phách)
    /// Tái hiện chuẩn xác biểu đồ "Team Growth" trong giao diện tham khảo.
    /// </summary>
    public class UcGrowthChart : RoundedPanel {
        public UcGrowthChart() {
            Radius = 14;
            FillColor = ThemeManager.Paper;
            BorderColor = ColorTranslator.FromHtml("#E2E8F0");
            Size = new Size(360, 260);

            var lblTitle = new Label {
                Text = "Team Growth",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                Location = new Point(16, 14),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            Controls.Add(lblTitle);

            var lblSub = new Label {
                Text = "Tăng trưởng doanh số & khách hàng 6 tháng qua",
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = ThemeManager.Muted,
                Location = new Point(17, 36),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            Controls.Add(lblSub);
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int left = 44;
            int right = Width - 24;
            int top = 68;
            int bottom = Height - 36;
            int chartW = right - left;
            int chartH = bottom - top;

            if (chartW <= 20 || chartH <= 20) return;

            // 1. Vẽ trục Y và đường kẻ ngang
            using (var font = new Font("Segoe UI", 7.5F))
            using (var textBrush = new SolidBrush(ColorTranslator.FromHtml("#94A3B8")))
            using (var gridPen = new Pen(ColorTranslator.FromHtml("#F1F5F9"), 1)) {
                for (int i = 0; i <= 7; i++) {
                    int y = bottom - (int)((float)i / 7f * chartH);
                    g.DrawString(i.ToString(), font, textBrush, 18, y - 7);
                    if (i > 0) {
                        g.DrawLine(gridPen, left, y, right, y);
                    }
                }
            }

            // 2. Tọa độ X cho 6 tháng: Jan, Feb, Mar, Apr, May, Jun
            string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
            float[] xCoords = new float[6];
            for (int i = 0; i < 6; i++) {
                xCoords[i] = left + (float)i / 5f * chartW;
            }

            // 3. Dữ liệu các lớp sóng (Scale 0 -> 7)
            // Lớp 1 (Vàng / Hổ phách)
            float[] waveAmber = { 0.8f, 1.4f, 2.2f, 4.2f, 3.1f, 1.2f };
            // Lớp 2 (Hồng san hô)
            float[] wavePink = { 2.2f, 1.6f, 4.6f, 2.8f, 5.0f, 2.6f };
            // Lớp 3 (Tím Violet - Lớp chính)
            float[] wavePurple = { 1.2f, 3.8f, 2.5f, 6.0f, 4.4f, 6.8f };

            // Vẽ Lớp Hổ phách (Amber)
            DrawAreaWave(g, xCoords, waveAmber, bottom, chartH,
                Color.FromArgb(170, 245, 158, 11),
                Color.FromArgb(20, 245, 158, 11),
                ColorTranslator.FromHtml("#D97706"));

            // Vẽ Lớp Hồng san hô (Pink)
            DrawAreaWave(g, xCoords, wavePink, bottom, chartH,
                Color.FromArgb(170, 244, 63, 94),
                Color.FromArgb(25, 244, 63, 94),
                ColorTranslator.FromHtml("#E11D48"));

            // Vẽ Lớp Tím (Purple)
            DrawAreaWave(g, xCoords, wavePurple, bottom, chartH,
                Color.FromArgb(190, 124, 58, 237),
                Color.FromArgb(30, 124, 58, 237),
                ColorTranslator.FromHtml("#7C3AED"));

            // 4. Vẽ trục hoành & nhãn tháng
            using (var axisPen = new Pen(ColorTranslator.FromHtml("#CBD5E1"), 1.2f))
                g.DrawLine(axisPen, left, bottom, right, bottom);

            using (var font = new Font("Segoe UI", 7.5F))
            using (var textBrush = new SolidBrush(ColorTranslator.FromHtml("#64748B"))) {
                for (int i = 0; i < 6; i++) {
                    var size = g.MeasureString(months[i], font);
                    g.DrawString(months[i], font, textBrush, xCoords[i] - size.Width / 2, bottom + 6);
                }
            }
        }

        private void DrawAreaWave(Graphics g, float[] xCoords, float[] vals, int bottom, int chartH, Color fillStart, Color fillEnd, Color strokeColor) {
            PointF[] points = new PointF[xCoords.Length];
            for (int i = 0; i < xCoords.Length; i++) {
                float y = bottom - (vals[i] / 7f * chartH);
                points[i] = new PointF(xCoords[i], y);
            }

            // Tạo đường Path khép kín cho Area Fill
            using (var path = new GraphicsPath()) {
                path.AddLine(points[0].X, bottom, points[0].X, points[0].Y);
                // Nối các đỉnh thành đường cong mượt
                path.AddCurve(points, 0.45f);
                path.AddLine(points[points.Length - 1].X, points[points.Length - 1].Y, points[points.Length - 1].X, bottom);
                path.CloseFigure();

                using (var brush = new LinearGradientBrush(
                    new Rectangle(0, (int)points[0].Y, Width, bottom),
                    fillStart, fillEnd, LinearGradientMode.Vertical)) {
                    g.FillPath(brush, path);
                }
            }

            // Vẽ viền nét đứt/liền cho đỉnh sóng
            using (var strokePath = new GraphicsPath()) {
                strokePath.AddCurve(points, 0.45f);
                using (var pen = new Pen(strokeColor, 2f))
                    g.DrawPath(pen, strokePath);
            }

            // Vẽ các điểm nút (chấm tròn)
            using (var dotBrush = new SolidBrush(strokeColor))
            using (var whiteBrush = new SolidBrush(Color.White)) {
                for (int i = 0; i < points.Length; i++) {
                    g.FillEllipse(dotBrush, points[i].X - 4.5f, points[i].Y - 4.5f, 9f, 9f);
                    g.FillEllipse(whiteBrush, points[i].X - 2f, points[i].Y - 2f, 4f, 4f);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Controls.BaoCao
{
    /// <summary>
    /// Vẽ biểu đồ cột nhóm Quý (Thu vs Chi vs Lợi nhuận) tự co giãn thông minh,
    /// hỗ trợ cả lợi nhuận âm/dương và luôn nằm gọn trong khung hình.
    /// </summary>
    public class QuarterComparisonChartRenderer
    {
        private readonly Panel _chartArea;
        private readonly ToolTip _toolTip;
        private List<DongTaiChinhQuyDto> _data;
        private int _hoveredQuarter = -1;

        public QuarterComparisonChartRenderer(Panel chartArea, ToolTip toolTip)
        {
            _chartArea = chartArea ?? throw new ArgumentNullException(nameof(chartArea));
            _toolTip = toolTip;

            _chartArea.Paint += OnPaint;
            _chartArea.MouseMove += OnMouseMove;
            _chartArea.MouseLeave += OnMouseLeave;
            _chartArea.Resize += (s, e) => _chartArea.Invalidate();
        }

        public void SetData(List<DongTaiChinhQuyDto> data)
        {
            _data = data;
            _hoveredQuarter = -1;
            _chartArea.Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_data == null || _data.Count < 3)
            {
                using (var f = new Font("Segoe UI", 9.5F))
                {
                    TextRenderer.DrawText(g, "Chưa có dữ liệu tài chính cho năm này.", f,
                        _chartArea.ClientRectangle, ThemeManager.Muted,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                return;
            }

            var dongThu = _data[0];
            var dongChi = _data[1];
            var dongLn = _data[2];

            double[] thu = { dongThu.Quy1, dongThu.Quy2, dongThu.Quy3, dongThu.Quy4 };
            double[] chi = { dongChi.Quy1, dongChi.Quy2, dongChi.Quy3, dongChi.Quy4 };
            double[] ln  = { dongLn.Quy1, dongLn.Quy2, dongLn.Quy3, dongLn.Quy4 };

            int w = _chartArea.ClientSize.Width;
            int h = _chartArea.ClientSize.Height;

            int topMargin = 28;
            int bottomMargin = 30;
            int leftMargin = 58;
            int rightMargin = 20;

            int chartW = w - leftMargin - rightMargin;
            int chartH = h - topMargin - bottomMargin;
            if (chartW <= 60 || chartH <= 40) return;

            // 1. Chú thích gọn ở góc trên
            DrawLegend(g, leftMargin, 4);

            // 2. Xác định thang đo (Max & Min để hỗ trợ cả Lợi nhuận âm)
            double maxVal = Math.Max(thu.Max(), chi.Max());
            if (ln.Max() > maxVal) maxVal = ln.Max();
            if (maxVal <= 0) maxVal = 100000;
            maxVal *= 1.15; // chừa khoảng trên cho nhãn số tiền

            double minVal = Math.Min(0, ln.Min());
            if (minVal < 0) minVal *= 1.15;

            double range = maxVal - minVal;
            if (range <= 0) range = 100000;

            // Tọa độ Y trục 0 (Baseline)
            int yZero = topMargin + (int)((maxVal / range) * chartH);

            // 3. Vẽ các đường gióng ngang & nhãn mốc tiền
            using (var penGrid = new Pen(Color.FromArgb(238, 242, 246), 1f))
            using (var penZero = new Pen(Color.FromArgb(200, 208, 220), 1.2f))
            using (var fGrid = new Font("Segoe UI", 7.5F))
            {
                penGrid.DashStyle = DashStyle.Dash;

                // 3 mốc dương
                for (int level = 1; level <= 3; level++)
                {
                    double v = maxVal * level / 3.0;
                    int gy = topMargin + (int)(((maxVal - v) / range) * chartH);
                    g.DrawLine(penGrid, leftMargin, gy, w - rightMargin, gy);

                    var rectLbl = new Rectangle(0, gy - 8, leftMargin - 6, 16);
                    TextRenderer.DrawText(g, FormatCompact(v), fGrid, rectLbl, ThemeManager.Muted,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                }

                // Đường trục 0
                g.DrawLine(penZero, leftMargin, yZero, w - rightMargin, yZero);
                var rectZero = new Rectangle(0, yZero - 8, leftMargin - 6, 16);
                TextRenderer.DrawText(g, "0", fGrid, rectZero, ThemeManager.Muted,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter);

                // Nếu có số âm, vẽ 1 mốc âm
                if (minVal < 0)
                {
                    int gyMin = topMargin + (int)(((maxVal - minVal) / range) * chartH);
                    g.DrawLine(penGrid, leftMargin, gyMin, w - rightMargin, gyMin);
                    var rectMin = new Rectangle(0, gyMin - 8, leftMargin - 6, 16);
                    TextRenderer.DrawText(g, FormatCompact(minVal), fGrid, rectMin, ThemeManager.Red,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                }
            }

            // 4. Vẽ 4 cụm cột (Quý 1 .. Quý 4)
            int groupWidth = chartW / 4;
            int barWidth = Math.Max(12, Math.Min(28, (groupWidth - 24) / 2));
            var pointsLn = new Point[4];

            using (var fBarVal = new Font("Segoe UI", 7F, FontStyle.Bold))
            using (var fQ = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            {
                for (int i = 0; i < 4; i++)
                {
                    int groupCenter = leftMargin + i * groupWidth + (groupWidth / 2);
                    int xThu = groupCenter - barWidth - 2;
                    int xChi = groupCenter + 2;

                    // Chiều cao cột Thu
                    int hThu = (int)(Math.Max(0, thu[i]) / range * chartH);
                    int yThu = yZero - hThu;

                    // Chiều cao cột Chi
                    int hChi = (int)(Math.Max(0, chi[i]) / range * chartH);
                    int yChi = yZero - hChi;

                    // Vẽ cột Thu
                    if (hThu > 0)
                    {
                        var rectThu = new Rectangle(xThu, yThu, barWidth, hThu);
                        Color cThu = (_hoveredQuarter == i + 1) ? ThemeManager.PrimaryDark : ThemeManager.Primary;
                        using (var brush = new SolidBrush(cThu))
                        using (var path = ThemeManager.RoundedRect(rectThu, 3))
                            g.FillPath(brush, path);

                        // Nhãn số tiền trên đầu cột Thu
                        if (barWidth >= 16)
                        {
                            var rTxt = new Rectangle(xThu - 15, Math.Max(topMargin, yThu - 15), barWidth + 30, 14);
                            TextRenderer.DrawText(g, FormatCompact(thu[i]), fBarVal, rTxt, ThemeManager.Primary,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }

                    // Vẽ cột Chi
                    if (hChi > 0)
                    {
                        var rectChi = new Rectangle(xChi, yChi, barWidth, hChi);
                        Color cChi = (_hoveredQuarter == i + 1) ? Color.FromArgb(215, 115, 15) : ThemeManager.Amber;
                        using (var brush = new SolidBrush(cChi))
                        using (var path = ThemeManager.RoundedRect(rectChi, 3))
                            g.FillPath(brush, path);

                        // Nhãn số tiền trên đầu cột Chi
                        if (barWidth >= 16)
                        {
                            var rTxt = new Rectangle(xChi - 15, Math.Max(topMargin, yChi - 15), barWidth + 30, 14);
                            TextRenderer.DrawText(g, FormatCompact(chi[i]), fBarVal, rTxt, ThemeManager.Amber,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }

                    // Nhãn Quý bên dưới trục
                    var rectQ = new Rectangle(groupCenter - 40, h - bottomMargin + 6, 80, 18);
                    TextRenderer.DrawText(g, $"Quý {i + 1}", fQ, rectQ, ThemeManager.Ink,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);

                    // Điểm tọa độ lợi nhuận
                    int yPointLn = topMargin + (int)(((maxVal - ln[i]) / range) * chartH);
                    yPointLn = Math.Max(topMargin + 6, Math.Min(h - bottomMargin - 4, yPointLn));
                    pointsLn[i] = new Point(groupCenter, yPointLn);
                }
            }

            // 5. Đường nối Lợi nhuận
            using (var penLine = new Pen(ThemeManager.Green, 2.2f))
            {
                for (int i = 0; i < 3; i++)
                    g.DrawLine(penLine, pointsLn[i], pointsLn[i + 1]);
            }

            // Điểm tròn & nhãn Lợi nhuận
            using (var fLn = new Font("Segoe UI", 7.5F, FontStyle.Bold))
            {
                for (int i = 0; i < 4; i++)
                {
                    var pt = pointsLn[i];
                    Color dotColor = ln[i] >= 0 ? ThemeManager.Green : ThemeManager.Red;

                    using (var brush = new SolidBrush(ThemeManager.Paper))
                    using (var pen = new Pen(dotColor, 2f))
                    {
                        g.FillEllipse(brush, pt.X - 4, pt.Y - 4, 8, 8);
                        g.DrawEllipse(pen, pt.X - 4, pt.Y - 4, 8, 8);
                    }

                    string txtLn = (ln[i] > 0 ? "+" : "") + FormatCompact(ln[i]);
                    int lblY = ln[i] >= 0 ? pt.Y - 17 : pt.Y + 6;
                    var rectTxt = new Rectangle(pt.X - 35, lblY, 70, 14);
                    TextRenderer.DrawText(g, txtLn, fLn, rectTxt, dotColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }
        }

        private void DrawLegend(Graphics g, int x, int y)
        {
            using (var f = new Font("Segoe UI", 8F, FontStyle.Regular))
            {
                // Thu
                using (var b = new SolidBrush(ThemeManager.Primary))
                    g.FillRectangle(b, x, y + 2, 10, 10);
                TextRenderer.DrawText(g, "Tổng thu", f, new Point(x + 14, y), ThemeManager.Ink2);

                // Chi
                int xChi = x + 85;
                using (var b = new SolidBrush(ThemeManager.Amber))
                    g.FillRectangle(b, xChi, y + 2, 10, 10);
                TextRenderer.DrawText(g, "Tổng chi", f, new Point(xChi + 14, y), ThemeManager.Ink2);

                // Lợi nhuận
                int xLn = xChi + 85;
                using (var p = new Pen(ThemeManager.Green, 2f))
                    g.DrawLine(p, xLn, y + 6, xLn + 12, y + 6);
                using (var b = new SolidBrush(ThemeManager.Green))
                    g.FillEllipse(b, xLn + 4, y + 4, 5, 5);
                TextRenderer.DrawText(g, "Lợi nhuận", f, new Point(xLn + 16, y), ThemeManager.Green);
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_data == null || _data.Count < 3) return;

            int leftMargin = 58;
            int rightMargin = 20;
            int chartW = _chartArea.ClientSize.Width - leftMargin - rightMargin;
            if (chartW <= 0) return;

            int groupW = chartW / 4;
            int q = ((e.X - leftMargin) / groupW) + 1;

            if (q >= 1 && q <= 4)
            {
                if (_hoveredQuarter != q)
                {
                    _hoveredQuarter = q;
                    var dongThu = _data[0];
                    var dongChi = _data[1];
                    var dongLn = _data[2];

                    double thu = q == 1 ? dongThu.Quy1 : q == 2 ? dongThu.Quy2 : q == 3 ? dongThu.Quy3 : dongThu.Quy4;
                    double chi = q == 1 ? dongChi.Quy1 : q == 2 ? dongChi.Quy2 : q == 3 ? dongChi.Quy3 : dongChi.Quy4;
                    double ln  = q == 1 ? dongLn.Quy1  : q == 2 ? dongLn.Quy2  : q == 3 ? dongLn.Quy3  : dongLn.Quy4;

                    string tip = $"[ Quý {q} ]\n" +
                                 $"• Tổng thu: {thu:N0} đ\n" +
                                 $"• Tổng chi: {chi:N0} đ\n" +
                                 $"• Lợi nhuận: {ln:N0} đ";
                    _toolTip?.SetToolTip(_chartArea, tip);
                    _chartArea.Invalidate();
                }
            }
            else if (_hoveredQuarter != -1)
            {
                _hoveredQuarter = -1;
                _toolTip?.SetToolTip(_chartArea, null);
                _chartArea.Invalidate();
            }
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            _hoveredQuarter = -1;
            _toolTip?.SetToolTip(_chartArea, null);
            _chartArea.Invalidate();
        }

        private string FormatCompact(double val)
        {
            double abs = Math.Abs(val);
            string sign = val < 0 ? "-" : "";

            if (abs >= 1_000_000_000)
                return sign + (abs / 1_000_000_000).ToString("0.#") + " tỷ";
            if (abs >= 1_000_000)
                return sign + (abs / 1_000_000).ToString("0.#") + " tr";
            if (abs >= 1_000)
                return sign + (abs / 1_000).ToString("0.#") + " k";
            return val.ToString("N0");
        }
    }
}

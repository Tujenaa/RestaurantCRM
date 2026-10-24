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
    /// GDI+ renderer cho biểu đồ doanh thu theo ngày trong BaoCaoBanHangForm.
    /// Quản lý vẽ cột doanh thu, đường gióng ngang mờ, nhãn ngày tháng thông minh và tooltip tương tác.
    /// </summary>
    public class SalesLineChartRenderer
    {
        private readonly Panel _chartArea;
        private readonly ToolTip _toolTip;
        private IList<DoanhThuNgayItemDto> _data;
        private int _hoveredIndex = -1;

        public SalesLineChartRenderer(Panel chartArea, ToolTip toolTip)
        {
            _chartArea = chartArea ?? throw new ArgumentNullException(nameof(chartArea));
            _toolTip = toolTip;

            _chartArea.Paint += OnPaint;
            _chartArea.MouseMove += OnMouseMove;
            _chartArea.MouseLeave += OnMouseLeave;
        }

        public void SetData(IList<DoanhThuNgayItemDto> data)
        {
            _data = data;
            _hoveredIndex = -1;
            _chartArea.Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_data == null || _data.Count == 0)
            {
                using (var f = new Font("Segoe UI", 10F, FontStyle.Regular))
                {
                    TextRenderer.DrawText(g, "Chưa có dữ liệu đơn hàng trong khoảng thời gian này.", f,
                        _chartArea.ClientRectangle, ThemeManager.Muted,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                return;
            }

            int count = _data.Count;
            int w = _chartArea.Width;
            int h = _chartArea.Height;

            int bottomMargin = 52;
            int topMargin = 26;
            int leftMargin = 58;
            int rightMargin = 20;

            int chartWidth = w - leftMargin - rightMargin;
            int chartHeight = h - bottomMargin - topMargin;

            double maxVal = _data.Max(x => x.DoanhThu);
            if (maxVal <= 0) maxVal = 100000;

            // 1. Vẽ các đường gióng ngang mờ (Gridlines) & nhãn mốc tiền
            using (var gridPen = new Pen(Color.FromArgb(235, 240, 246), 1f))
            using (var fGrid = new Font("Segoe UI", 7.5F, FontStyle.Regular))
            {
                gridPen.DashStyle = DashStyle.Dash;
                for (int level = 0; level <= 3; level++)
                {
                    int gy = h - bottomMargin - (level * chartHeight / 3);
                    g.DrawLine(gridPen, leftMargin, gy, w - rightMargin, gy);

                    double valLevel = maxVal * level / 3.0;
                    string labelLevel = FormatCompactMoney(valLevel);
                    var rectLabel = new Rectangle(0, gy - 8, leftMargin - 6, 16);
                    TextRenderer.DrawText(g, labelLevel, fGrid, rectLabel, ThemeManager.Muted,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                }
            }

            // Vẽ đường trục ngang cơ sở
            using (var baseLinePen = new Pen(Color.FromArgb(203, 213, 225), 1.2f))
            {
                g.DrawLine(baseLinePen, leftMargin, h - bottomMargin, w - rightMargin, h - bottomMargin);
            }

            // 2. Tính bề rộng từng slot cột
            int slotWidth = Math.Max(18, chartWidth / count);
            int barWidth = Math.Min(38, Math.Max(8, slotWidth - (count > 20 ? 6 : 10)));

            // Tự động tính bước nhảy (step) cho nhãn ngày tháng để KHÔNG BAO GIỜ bị dính chữ khi xem theo tháng (30 ngày)
            int step = 1;
            if (slotWidth < 44)
            {
                double ratio = 44.0 / slotWidth;
                if (ratio <= 2.2) step = 2;       // Cách 1 ngày (1, 3, 5, 7, 9...)
                else if (ratio <= 3.2) step = 3;  // Cách 2 ngày (1, 4, 7, 10...)
                else step = 5;                    // Cách 4 ngày (1, 6, 11, 16...)
            }

            int lastDrawnDateSlotX = -999;
            int lastDrawnMoneyRightX = -999;

            using (var fontDateThu = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var fontDateNgay = new Font("Segoe UI", 8F, FontStyle.Regular))
            using (var fontValue = new Font("Segoe UI", count > 18 ? 7.5F : 8F, FontStyle.Bold))
            using (var tickPen = new Pen(Color.FromArgb(148, 163, 184), 1f))
            using (var hoverBrush = new SolidBrush(Color.FromArgb(24, 13, 148, 136)))
            {
                for (int i = 0; i < count; i++)
                {
                    var item = _data[i];
                    int slotX = leftMargin + i * slotWidth;
                    int barX = slotX + (slotWidth - barWidth) / 2;

                    // Vẽ vùng highlight dịu nhẹ khi rê chuột vào ngày đó
                    if (i == _hoveredIndex)
                    {
                        g.FillRectangle(hoverBrush, slotX, topMargin, slotWidth, chartHeight);
                    }

                    int barHeight = (int)((item.DoanhThu / maxVal) * chartHeight);
                    int barY = h - bottomMargin - barHeight;

                    // Vẽ vạch đế mờ nếu doanh thu = 0 để biểu đồ không trơ trọi
                    if (item.DoanhThu <= 0)
                    {
                        var zeroRect = new Rectangle(barX, h - bottomMargin - 3, barWidth, 3);
                        using (var br = new SolidBrush(Color.FromArgb(226, 232, 240)))
                        {
                            g.FillRectangle(br, zeroRect);
                        }
                    }
                    else
                    {
                        if (barHeight < 4) barHeight = 4;
                        var barRect = new Rectangle(barX, barY, barWidth, barHeight);
                        var barColor = (i == _hoveredIndex) ? Color.FromArgb(15, 118, 110) : Color.FromArgb(13, 148, 136);
                        using (var brush = new SolidBrush(barColor))
                        {
                            g.FillRectangle(brush, barRect);
                        }

                        // Nhãn số tiền trên đầu cột (chỉ vẽ nếu không va chạm với nhãn cột trước)
                        string valStr = FormatCompactMoney(item.DoanhThu);
                        int valWidth = Math.Max(slotWidth + 12, 38);
                        int valLeft = slotX + (slotWidth - valWidth) / 2;

                        if (valLeft > lastDrawnMoneyRightX + 2 || i == _hoveredIndex)
                        {
                            var valRect = new Rectangle(valLeft, Math.Max(2, barY - 18), valWidth, 16);
                            TextRenderer.DrawText(g, valStr, fontValue, valRect, ThemeManager.Ink,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
                            lastDrawnMoneyRightX = valLeft + valWidth;
                        }
                    }

                    // Quyết định vẽ nhãn ngày: tại các mốc chia hết cho step hoặc ngày cuối cùng
                    bool shouldDrawDate = (i % step == 0);
                    if (i == count - 1 && (slotX - lastDrawnDateSlotX) >= 36)
                    {
                        shouldDrawDate = true;
                    }

                    if (shouldDrawDate)
                    {
                        lastDrawnDateSlotX = slotX;
                        int centerX = slotX + slotWidth / 2;

                        // Vẽ vạch khấc (tick mark) nhỏ trên trục hoành để định vị cột rõ ràng
                        g.DrawLine(tickPen, centerX, h - bottomMargin, centerX, h - bottomMargin + 4);

                        int dateBoxWidth = Math.Max(slotWidth, 44);
                        var rectThu = new Rectangle(centerX - dateBoxWidth / 2, h - bottomMargin + 6, dateBoxWidth, 16);
                        var rectNgay = new Rectangle(centerX - dateBoxWidth / 2, h - bottomMargin + (count <= 14 ? 22 : 8), dateBoxWidth, 16);

                        if (count <= 14)
                        {
                            // Khoảng thời gian ngắn (<= 14 ngày): hiển thị cả Thứ và Ngày rõ nét
                            TextRenderer.DrawText(g, item.Thu, fontDateThu, rectThu, ThemeManager.Ink2,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
                            TextRenderer.DrawText(g, item.NgayHienThi, fontDateNgay, rectNgay, ThemeManager.Muted,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
                        }
                        else
                        {
                            // Khoảng thời gian dài (theo tháng: 30-31 ngày): hiển thị Ngày dd/MM tại các mốc thoáng đãng
                            TextRenderer.DrawText(g, item.NgayHienThi, fontDateThu, rectNgay, ThemeManager.Ink,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
                        }
                    }
                }
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_data == null || _data.Count == 0) return;
            int count = _data.Count;

            int leftMargin = 55;
            int rightMargin = 20;
            int chartWidth = _chartArea.Width - leftMargin - rightMargin;
            int slotWidth = Math.Max(18, chartWidth / count);

            int idx = (e.X - leftMargin) / slotWidth;
            if (idx >= 0 && idx < count && e.X >= leftMargin && e.X <= _chartArea.Width - rightMargin)
            {
                if (idx != _hoveredIndex)
                {
                    _hoveredIndex = idx;
                    var item = _data[idx];
                    string tip = $"📅 {item.Thu}, ngày {item.NgayHienThi} ({item.Ngay:dd/MM/yyyy})\n💰 Doanh thu: {item.DoanhThu:N0} đ\n🧾 Đơn hoàn tất: {item.SoDon:N0} đơn";
                    _toolTip?.Show(tip, _chartArea, e.X + 12, Math.Max(10, e.Y - 50), 3500);
                    _chartArea.Invalidate();
                }
            }
            else
            {
                if (_hoveredIndex != -1)
                {
                    _hoveredIndex = -1;
                    _toolTip?.Hide(_chartArea);
                    _chartArea.Invalidate();
                }
            }
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            if (_hoveredIndex != -1)
            {
                _hoveredIndex = -1;
                _toolTip?.Hide(_chartArea);
                _chartArea.Invalidate();
            }
        }

        public static string FormatCompactMoney(double amount)
        {
            if (amount >= 1000000000)
            {
                return (amount / 1000000000.0).ToString("0.#") + "B";
            }
            if (amount >= 1000000)
            {
                return (amount / 1000000.0).ToString("0.#") + "M";
            }
            if (amount >= 1000)
            {
                return (amount / 1000.0).ToString("0.#") + "k";
            }
            return amount.ToString("N0");
        }
    }
}

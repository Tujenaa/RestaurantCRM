using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Forms.PhanTich
{
    /// <summary>Phân tích khách hàng: thẻ số liệu (từ API khi có) + xếp hạng khách hàng tiêu biểu (minh họa).</summary>
    public class PhanTichForm : Form
    {
        private readonly UcThongKe _kKhach, _kDiem, _kLuot, _kKhaoSat;

        public PhanTichForm()
        {
            Text = "Phân tích khách hàng | RestaurantCRM";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1100, 720);
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);
            Padding = new Padding(14, 6, 14, 10);

            var header = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = ThemeManager.Bg };
            var lblTitle = new Label { Text = "Phân tích khách hàng", Location = new Point(10, 8), AutoSize = true, Font = new Font("Segoe UI", 17F, FontStyle.Bold), ForeColor = ThemeManager.Ink };
            var lblSub = new Label { Text = "Tổng hợp chỉ số khách hàng, đánh giá và mức độ chi tiêu.", Location = new Point(12, 44), AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = ThemeManager.Muted };
            header.Controls.Add(lblTitle); header.Controls.Add(lblSub);
            Action place = () => {
                bool embedded = !TopLevel;
                lblTitle.Visible = !embedded;
                lblSub.Location = new Point(12, embedded ? 12 : 44);
                header.Height = embedded ? 40 : 76;
            };
            ParentChanged += (s, e) => place();

            var kpis = new TableLayoutPanel { Dock = DockStyle.Top, Height = 140, ColumnCount = 4, RowCount = 1, BackColor = ThemeManager.Bg };
            kpis.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            _kKhach = new UcThongKe("Khách hàng", "...", "Tổng hồ sơ", "👥", ThemeManager.Primary);
            _kDiem = new UcThongKe("Điểm đánh giá", "...", "Trung bình", "⭐", ThemeManager.Amber);
            _kLuot = new UcThongKe("Lượt đánh giá", "...", "Đang hiển thị", "💬", ThemeManager.Violet);
            _kKhaoSat = new UcThongKe("Khảo sát mở", "...", "Đang diễn ra", "📋", ThemeManager.Teal);
            var all = new[] { _kKhach, _kDiem, _kLuot, _kKhaoSat };
            for (int i = 0; i < 4; i++) { all[i].Dock = DockStyle.Fill; kpis.Controls.Add(all[i], i, 0); }

            // Xếp hạng khách hàng tiêu biểu (số liệu minh họa)
            var top = new UcTopList { Dock = DockStyle.Fill, Title = "Khách hàng tiêu biểu theo chi tiêu", Subtitle = "SỐ LIỆU MINH HỌA — CHƯA LẤY TỪ API", Unit = "%" };
            top.SetItems(new List<KeyValuePair<string, double>> {
                new KeyValuePair<string, double>("Minh Anh", 88),
                new KeyValuePair<string, double>("Quốc Bảo", 71),
                new KeyValuePair<string, double>("Thu Hà", 57),
                new KeyValuePair<string, double>("Hoàng Nam", 42),
                new KeyValuePair<string, double>("Lan Phương", 28)
            });

            Controls.Add(top);
            Controls.Add(kpis);
            Controls.Add(header);
            top.BringToFront();
            place();

            Load += async (s, e) => {
                try
                {
                    using (var http = new System.Net.Http.HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl.TrimEnd('/') + "/"), Timeout = TimeSpan.FromSeconds(15) })
                    {
                        var m = await System.Net.Http.Json.HttpClientJsonExtensions.GetFromJsonAsync<DashboardMetricsDto>(http, "api/ThongKe/DashboardMetrics");
                        if (m == null || IsDisposed) return;
                        _kKhach.SetValue(m.TongKhachHang.ToString("N0"));
                        _kDiem.SetValue(m.DiemDanhGiaTb.ToString("0.0") + " / 5");
                        _kLuot.SetValue(m.TongDanhGia.ToString("N0"));
                        _kKhaoSat.SetValue(m.KhaoSatDangMo.ToString("N0"));
                    }
                }
                catch
                {
                    foreach (var k in all) if (!k.IsDisposed) k.SetValue("—", "Chưa kết nối API");
                }
            };
        }
    }
}
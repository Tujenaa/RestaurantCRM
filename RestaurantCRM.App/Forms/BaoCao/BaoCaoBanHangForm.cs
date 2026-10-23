using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Forms.BaoCao
{
    /// <summary>Báo cáo doanh thu: 4 thẻ số liệu, biểu đồ doanh thu theo tháng, món bán chạy (lấy từ API ThongKe).</summary>
    public class BaoCaoBanHangForm : Form
    {
        private readonly UcThongKe _kDoanhThu, _kHoanTat, _kChoXuLy, _kDangGiao;
        private readonly UcLineChart _chart;
        private readonly UcTopList _top;

        public BaoCaoBanHangForm()
        {
            Text = "Báo cáo bán hàng | RestaurantCRM";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1100, 720);
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);
            Padding = new Padding(14, 6, 14, 10);

            // Hàng đầu
            var header = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = ThemeManager.Bg };
            var lblTitle = new Label { Text = "Báo cáo bán hàng", Location = new Point(10, 8), AutoSize = true, Font = new Font("Segoe UI", 17F, FontStyle.Bold), ForeColor = ThemeManager.Ink };
            var lblSub = new Label { Text = "Theo dõi doanh thu, tình trạng đơn hàng và món bán chạy.", Location = new Point(12, 44), AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = ThemeManager.Muted };
            var btnRefresh = new Button { Text = "Làm mới" };
            ButtonSkin.SetKind(btnRefresh, ButtonSkin.Kind.Secondary);
            PageKit.FitButton(btnRefresh);
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Click += (s, e) => LoadData();
            header.Controls.Add(lblTitle); header.Controls.Add(lblSub); header.Controls.Add(btnRefresh);
            Action place = () => {
                bool embedded = !TopLevel;
                lblTitle.Visible = !embedded;
                lblSub.Location = new Point(12, embedded ? 16 : 44);
                header.Height = embedded ? 54 : 76;
                btnRefresh.Location = new Point(header.Width - btnRefresh.Width - 8, embedded ? 4 : 16);
            };
            header.Resize += (s, e) => place();
            btnRefresh.SizeChanged += (s, e) => place();
            ParentChanged += (s, e) => place();

            // Thẻ số liệu
            var kpis = new TableLayoutPanel { Dock = DockStyle.Top, Height = 140, ColumnCount = 4, RowCount = 1, BackColor = ThemeManager.Bg };
            kpis.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            _kDoanhThu = new UcThongKe("Doanh thu hôm nay", "...", "Đơn đã hoàn thành", "💰", ThemeManager.Green);
            _kHoanTat = new UcThongKe("Đơn hoàn tất", "...", "Trong hôm nay", "✅", ThemeManager.Primary);
            _kChoXuLy = new UcThongKe("Chờ xử lý", "...", "Cần xác nhận", "⏳", ThemeManager.Amber);
            _kDangGiao = new UcThongKe("Đang thực hiện", "...", "Chuẩn bị / giao", "🛵", ThemeManager.Sky);
            var all = new[] { _kDoanhThu, _kHoanTat, _kChoXuLy, _kDangGiao };
            for (int i = 0; i < 4; i++) { all[i].Dock = DockStyle.Fill; kpis.Controls.Add(all[i], i, 0); }

            // Biểu đồ + món bán chạy
            var bottom = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.Bg };
            _top = new UcTopList { Dock = DockStyle.Right, Width = 340, Title = "Món bán chạy", Subtitle = "TOP 5 THEO SỐ LƯỢNG ĐÃ BÁN", Unit = "phần" };
            _chart = new UcLineChart { Dock = DockStyle.Fill, Title = "Doanh thu theo tháng", Caption = "Năm " + DateTime.Now.Year, Unit = "đ" };
            bottom.Controls.Add(_chart);
            bottom.Controls.Add(_top);

            Controls.Add(bottom);
            Controls.Add(kpis);
            Controls.Add(header);
            bottom.BringToFront();
            place();

            Load += (s, e) => LoadData();
        }

        private class DoanhThuThangDto { public int Thang { get; set; } public double DoanhThu { get; set; } }
        private class MonBanChayDto { public string TenMon { get; set; } public double TongSoLuongBan { get; set; } }

        private async void LoadData()
        {
            var http = new System.Net.Http.HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl.TrimEnd('/') + "/"), Timeout = TimeSpan.FromSeconds(15) };
            try
            {
                var m = await System.Net.Http.Json.HttpClientJsonExtensions.GetFromJsonAsync<DashboardMetricsDto>(http, "api/ThongKe/DashboardMetrics");
                if (m != null && !IsDisposed)
                {
                    _kDoanhThu.SetValue(m.DoanhThuHomNay.ToString("N0") + " đ");
                    _kHoanTat.SetValue(m.DonHoanThanhHomNay.ToString("N0"));
                    _kChoXuLy.SetValue(m.DonChoXuLy.ToString("N0"));
                    _kDangGiao.SetValue(m.DonDangGiao.ToString("N0"));
                }
            }
            catch
            {
                foreach (var k in new[] { _kDoanhThu, _kHoanTat, _kChoXuLy, _kDangGiao }) if (!k.IsDisposed) k.SetValue("—", "Chưa kết nối API");
            }

            var labels = new string[12]; var values = new double[12];
            for (int i = 0; i < 12; i++) labels[i] = "T" + (i + 1);
            try
            {
                var data = await System.Net.Http.Json.HttpClientJsonExtensions.GetFromJsonAsync<List<DoanhThuThangDto>>(http, "api/ThongKe/DoanhThuTheoThang?nam=" + DateTime.Now.Year);
                if (data != null) foreach (var d in data) if (d.Thang >= 1 && d.Thang <= 12) values[d.Thang - 1] = d.DoanhThu;
            }
            catch { }
            if (!_chart.IsDisposed) _chart.SetData(labels, values);

            var items = new List<KeyValuePair<string, double>>();
            try
            {
                var data = await System.Net.Http.Json.HttpClientJsonExtensions.GetFromJsonAsync<List<MonBanChayDto>>(http, "api/ThongKe/MonBanChay?top=5");
                if (data != null) foreach (var d in data) items.Add(new KeyValuePair<string, double>(d.TenMon, d.TongSoLuongBan));
            }
            catch { }
            if (!_top.IsDisposed) _top.SetItems(items);
            http.Dispose();
        }
    }
}
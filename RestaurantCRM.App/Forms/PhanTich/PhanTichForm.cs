using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.PhanTich
{
    /// <summary>
    /// Màn hình phân tích khách hàng đa tab:
    /// - 4 thẻ KPI tổng hợp (Khách hàng, Điểm đánh giá, Lượt đánh giá, Khảo sát mở).
    /// - Tab 1: Cơ cấu độ tuổi & Giới tính (Biểu đồ phân bố GDI+ & Bảng chi tiết từng nhóm tuổi).
    /// - Tab 2: Sở thích ẩm thực (Món ưa chuộng) (Top món trực quan & Bảng chi tiết lượt gọi, doanh thu).
    /// - Tab 3: Xếp hạng & Chi tiêu (Top chi tiêu & Bảng danh sách khách hàng tiêu biểu).
    /// </summary>
    public class PhanTichForm : Form
    {
        private readonly UcThongKe _kKhach, _kDiem, _kLuot, _kKhaoSat;
        private TabControl _tabMain;

        // Tab 1: Độ tuổi & Giới tính
        private UcCustomerDemographics _demographics;
        private UcDataGridView _dgvDoTuoi;
        private Label _lblDoTuoiSummary;

        // Tab 2: Sở thích gọi món
        private UcTopList _topFood;
        private UcDataGridView _dgvFood;

        // Tab 3: Chi tiêu khách hàng
        private UcTopList _topSpending;
        private UcDataGridView _dgvSpending;

        public PhanTichForm()
        {
            Text = "Phân tích khách hàng | RestaurantCRM";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1220, 800);
            MinimumSize = new Size(1000, 680);
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);
            Padding = new Padding(14, 6, 14, 10);

            // 1. Header
            var header = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = ThemeManager.Bg };
            var lblTitle = new Label
            {
                Text = "Phân tích khách hàng",
                Location = new Point(10, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            };
            var lblSub = new Label
            {
                Text = "Báo cáo phân tích chuyên sâu về cơ cấu độ tuổi, giới tính, sở thích ẩm thực và chi tiêu của khách hàng.",
                Location = new Point(12, 44),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeManager.Muted
            };

            var btnReload = new Button
            {
                Text = "🔄 Làm mới",
                Size = new Size(104, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.Paper,
                ForeColor = ThemeManager.Ink2,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnReload.FlatAppearance.BorderColor = ThemeManager.Line;
            btnReload.Location = new Point(header.Width - 114, 20);
            header.Resize += (s, e) => { btnReload.Location = new Point(header.Width - 114, 20); };
            btnReload.Click += async (s, e) => await LoadDataAsync();

            header.Controls.Add(lblTitle);
            header.Controls.Add(lblSub);
            header.Controls.Add(btnReload);

            Action place = () =>
            {
                bool embedded = !TopLevel;
                lblTitle.Visible = !embedded;
                lblSub.Location = new Point(12, embedded ? 12 : 44);
                header.Height = embedded ? 44 : 76;
                btnReload.Location = new Point(header.Width - 114, embedded ? 6 : 20);
            };
            ParentChanged += (s, e) => place();

            // 2. Bốn thẻ KPI đầu trang
            var kpis = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 136,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = ThemeManager.Bg,
                Margin = new Padding(0)
            };
            kpis.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            _kKhach = new UcThongKe("Khách hàng", "...", "Tổng hồ sơ", "👥", ThemeManager.Primary);
            _kDiem = new UcThongKe("Điểm đánh giá", "...", "Trung bình", "⭐", ThemeManager.Amber);
            _kLuot = new UcThongKe("Lượt đánh giá", "...", "Đang hiển thị", "💬", ThemeManager.Violet);
            _kKhaoSat = new UcThongKe("Khảo sát mở", "...", "Đang diễn ra", "📋", ThemeManager.Teal);

            var allKpi = new[] { _kKhach, _kDiem, _kLuot, _kKhaoSat };
            for (int i = 0; i < 4; i++)
            {
                allKpi[i].Dock = DockStyle.Fill;
                kpis.Controls.Add(allKpi[i], i, 0);
            }

            // 3. Khung TabControl dạng Pill hiện đại
            var tabHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(0, 8, 0, 0)
            };

            _tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular)
            };

            // Dựng 3 Tab phân tích
            BuildTabDoTuoi();
            BuildTabSoThich();
            BuildTabChiTieu();

            tabHost.Controls.Add(_tabMain);
            TabSkin.Attach(_tabMain);

            Controls.Add(tabHost);
            Controls.Add(kpis);
            Controls.Add(header);
            tabHost.BringToFront();
            place();

            Load += async (s, e) => await LoadDataAsync();
        }

        // ================= TAB 1: CƠ CẤU ĐỘ TUỔI & GIỚI TÍNH =================
        private void BuildTabDoTuoi()
        {
            var tab = new TabPage("Cơ cấu độ tuổi & Giới tính") { BackColor = ThemeManager.Bg };

            var split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(0, 8, 0, 0)
            };
            split.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 420F)); // Biểu đồ bên trái
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));    // Bảng chi tiết bên phải

            _demographics = new UcCustomerDemographics
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 8, 0)
            };

            var rightCard = PageKit.NewCard();
            rightCard.Dock = DockStyle.Fill;
            rightCard.Margin = new Padding(4, 0, 0, 0);
            rightCard.Padding = new Padding(16, 14, 16, 14);

            var lblGridTitle = new Label
            {
                Text = "BẢNG CHI TIẾT CƠ CẤU NHÓM TUỔI",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                Dock = DockStyle.Top,
                Height = 28
            };

            _lblDoTuoiSummary = new Label
            {
                Text = "Đang tải dữ liệu...",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = ThemeManager.Muted,
                Dock = DockStyle.Bottom,
                Height = 24
            };

            _dgvDoTuoi = new UcDataGridView { Dock = DockStyle.Fill };
            _dgvDoTuoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nhóm tuổi", Width = 95 });
            _dgvDoTuoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phân khúc đối tượng", FillWeight = 160 });
            var cCount = new DataGridViewTextBoxColumn { HeaderText = "Số lượng khách", Width = 120 };
            cCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            cCount.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvDoTuoi.Columns.Add(cCount);

            var cPct = new DataGridViewTextBoxColumn { HeaderText = "Tỷ lệ (%)", Width = 110 };
            cPct.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            cPct.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            cPct.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _dgvDoTuoi.Columns.Add(cPct);

            rightCard.Controls.Add(_dgvDoTuoi);
            rightCard.Controls.Add(_lblDoTuoiSummary);
            rightCard.Controls.Add(lblGridTitle);

            split.Controls.Add(_demographics, 0, 0);
            split.Controls.Add(rightCard, 1, 0);

            tab.Controls.Add(split);
            _tabMain.TabPages.Add(tab);
        }

        // ================= TAB 2: SỞ THÍCH ẨM THỰC (MÓN ƯA CHUỘNG) =================
        private void BuildTabSoThich()
        {
            var tab = new TabPage("Sở thích ẩm thực (Món ưa chuộng)") { BackColor = ThemeManager.Bg };

            var split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(0, 8, 0, 0)
            };
            split.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400F)); // Top list bên trái
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));    // DataGridView bên phải

            _topFood = new UcTopList
            {
                Dock = DockStyle.Fill,
                Title = "Top món ưa thích nhất",
                Subtitle = "XẾP HẠNG THEO LƯỢT ĐẶT MÓN",
                Unit = "phần",
                Margin = new Padding(0, 0, 8, 0)
            };

            var rightCard = PageKit.NewCard();
            rightCard.Dock = DockStyle.Fill;
            rightCard.Margin = new Padding(4, 0, 0, 0);
            rightCard.Padding = new Padding(16, 14, 16, 14);

            var lblGridTitle = new Label
            {
                Text = "DANH SÁCH MÓN ĂN ĐƯỢC ƯA CHUỘNG (DỰA TRÊN ĐƠN HOÀN THÀNH)",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                Dock = DockStyle.Top,
                Height = 28
            };

            _dgvFood = new UcDataGridView { Dock = DockStyle.Fill };
            _dgvFood.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã món", Width = 80 });
            _dgvFood.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tên món ăn", FillWeight = 160 });
            _dgvFood.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Loại món", Width = 110 });

            var cGoi = new DataGridViewTextBoxColumn { HeaderText = "Lượt gọi", Width = 100 };
            cGoi.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            cGoi.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvFood.Columns.Add(cGoi);

            var cDoanhThu = new DataGridViewTextBoxColumn { HeaderText = "Doanh thu (đ)", Width = 130 };
            cDoanhThu.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            cDoanhThu.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            cDoanhThu.DefaultCellStyle.Format = "N0";
            _dgvFood.Columns.Add(cDoanhThu);

            var cTyLe = new DataGridViewTextBoxColumn { HeaderText = "Tỷ lệ ưa thích", Width = 110 };
            cTyLe.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            cTyLe.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            cTyLe.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _dgvFood.Columns.Add(cTyLe);

            rightCard.Controls.Add(_dgvFood);
            rightCard.Controls.Add(lblGridTitle);

            split.Controls.Add(_topFood, 0, 0);
            split.Controls.Add(rightCard, 1, 0);

            tab.Controls.Add(split);
            _tabMain.TabPages.Add(tab);
        }

        // ================= TAB 3: XẾP HẠNG & CHI TIÊU KHÁCH HÀNG =================
        private void BuildTabChiTieu()
        {
            var tab = new TabPage("Xếp hạng & Chi tiêu") { BackColor = ThemeManager.Bg };

            var split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(0, 8, 0, 0)
            };
            split.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400F)); // Top list bên trái
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));    // DataGridView bên phải

            _topSpending = new UcTopList
            {
                Dock = DockStyle.Fill,
                Title = "Khách hàng tiêu biểu",
                Subtitle = "TỔNG TIỀN CÁC ĐƠN ĐÃ HOÀN THÀNH",
                Unit = "đ",
                Margin = new Padding(0, 0, 8, 0)
            };

            var rightCard = PageKit.NewCard();
            rightCard.Dock = DockStyle.Fill;
            rightCard.Margin = new Padding(4, 0, 0, 0);
            rightCard.Padding = new Padding(16, 14, 16, 14);

            var lblGridTitle = new Label
            {
                Text = "BẢNG XẾP HẠNG KHÁCH HÀNG THEO MỨC ĐỘ CHI TIÊU",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                Dock = DockStyle.Top,
                Height = 28
            };

            _dgvSpending = new UcDataGridView { Dock = DockStyle.Fill };
            _dgvSpending.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã KH", Width = 80 });
            _dgvSpending.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Họ và tên", FillWeight = 160 });
            _dgvSpending.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Số điện thoại", Width = 115 });

            var cDon = new DataGridViewTextBoxColumn { HeaderText = "Số đơn", Width = 80 };
            cDon.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            cDon.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvSpending.Columns.Add(cDon);

            var cChiTieu = new DataGridViewTextBoxColumn { HeaderText = "Tổng chi tiêu (đ)", Width = 140 };
            cChiTieu.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            cChiTieu.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            cChiTieu.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            cChiTieu.DefaultCellStyle.Format = "N0";
            _dgvSpending.Columns.Add(cChiTieu);

            _dgvSpending.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", Width = 95 });

            rightCard.Controls.Add(_dgvSpending);
            rightCard.Controls.Add(lblGridTitle);

            split.Controls.Add(_topSpending, 0, 0);
            split.Controls.Add(rightCard, 1, 0);

            tab.Controls.Add(split);
            _tabMain.TabPages.Add(tab);
        }

        private async Task LoadDataAsync()
        {
            var kpiTask = LoadMetricsAsync();
            var demographicTask = LoadDemographicsAndPreferencesAsync();
            var customerSpendTask = LoadTopCustomersAsync();
            await Task.WhenAll(kpiTask, demographicTask, customerSpendTask);
        }

        private async Task LoadMetricsAsync()
        {
            try
            {
                using (var http = new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl.TrimEnd('/') + "/"), Timeout = TimeSpan.FromSeconds(15) })
                {
                    var m = await http.GetFromJsonAsync<DashboardMetricsDto>("api/ThongKe/DashboardMetrics");
                    if (m == null || IsDisposed) return;
                    _kKhach.SetValue(m.TongKhachHang.ToString("N0"));
                    _kDiem.SetValue(m.DiemDanhGiaTb.ToString("0.0") + " / 5");
                    _kLuot.SetValue(m.TongDanhGia.ToString("N0"));
                    _kKhaoSat.SetValue(m.KhaoSatDangMo.ToString("N0"));
                }
            }
            catch
            {
                var all = new[] { _kKhach, _kDiem, _kLuot, _kKhaoSat };
                foreach (var k in all) if (!k.IsDisposed) k.SetValue("—", "Chưa kết nối API");
            }
        }

        private async Task LoadDemographicsAndPreferencesAsync()
        {
            try
            {
                using (var http = new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl.TrimEnd('/') + "/"), Timeout = TimeSpan.FromSeconds(15) })
                {
                    var data = await http.GetFromJsonAsync<PhanTichKhachHangDto>("api/ThongKe/PhanTichKhachHang");
                    if (data == null || IsDisposed) return;

                    // 1. Tab Độ tuổi: Cập nhật thẻ biểu đồ & DataGridView
                    _demographics.SetData(data);

                    _dgvDoTuoi.Rows.Clear();
                    if (data.DoTuoi != null)
                    {
                        foreach (var it in data.DoTuoi)
                        {
                            _dgvDoTuoi.Rows.Add(it.TenNhom, it.MoTa, it.SoLuong.ToString("N0"), $"{it.TyLe:0.0}%");
                        }
                    }

                    _lblDoTuoiSummary.Text = $"Độ tuổi trung bình: {data.DoTuoiTrungBinh:0.0} tuổi  ·  Đã cập nhật ngày sinh: {data.KhachHangCoNgaySinh:N0} / {data.TongKhachHang:N0} khách ({((double)data.KhachHangCoNgaySinh / Math.Max(1, data.TongKhachHang) * 100):0.0}%)";

                    // 2. Tab Sở thích gọi món: Cập nhật Top list & DataGridView
                    var foodItems = new List<KeyValuePair<string, double>>();
                    _dgvFood.Rows.Clear();

                    if (data.TopMonUaThich != null)
                    {
                        foreach (var m in data.TopMonUaThich)
                        {
                            string label = $"{m.TenMon}  ·  {m.TyLeUaThich:0.0}%";
                            foodItems.Add(new KeyValuePair<string, double>(label, m.SoLuongGoi));
                            _dgvFood.Rows.Add(m.MaMon, m.TenMon, m.TenLoaiMon, m.SoLuongGoi.ToString("N0"), m.DoanhThu, $"{m.TyLeUaThich:0.0}%");
                        }
                    }

                    if (foodItems.Count == 0)
                        _topFood.Subtitle = "CHƯA CÓ DỮ LIỆU ĐẶT MÓN HOÀN THÀNH";
                    else
                        _topFood.Subtitle = "XẾP HẠNG THEO SỐ LƯỢT ĐẶT MÓN";

                    _topFood.SetItems(foodItems);
                }
            }
            catch (Exception)
            {
                if (IsDisposed) return;
                _demographics.SetData(null);
                _dgvDoTuoi.Rows.Clear();
                _lblDoTuoiSummary.Text = "Không tải được dữ liệu độ tuổi từ API";
                _topFood.Subtitle = "KHÔNG TẢI ĐƯỢC DỮ LIỆU API";
                _topFood.SetItems(new List<KeyValuePair<string, double>>());
                _dgvFood.Rows.Clear();
            }
        }

        private async Task LoadTopCustomersAsync()
        {
            try
            {
                var ds = await new KhachHangService().GetAllAsync();
                if (IsDisposed) return;
                var items = new List<KeyValuePair<string, double>>();
                _dgvSpending.Rows.Clear();

                if (ds != null)
                {
                    var topList = new List<RestaurantCRM.AdminApp.DTOs.KhachHang.KhachHangDto>();
                    foreach (var x in ds) if (x.TongChiTieu > 0) topList.Add(x);
                    topList.Sort((a, b) => b.TongChiTieu.CompareTo(a.TongChiTieu));

                    // Top 8 vào thẻ tóm tắt
                    int take = Math.Min(8, topList.Count);
                    for (int i = 0; i < take; i++)
                    {
                        var k = topList[i];
                        items.Add(new KeyValuePair<string, double>(k.HoTen + "  ·  " + k.SoDonHang + " đơn", k.TongChiTieu));
                    }

                    // Toàn bộ danh sách vào DataGridView xếp hạng
                    foreach (var k in topList)
                    {
                        _dgvSpending.Rows.Add(k.MaKhachHang, k.HoTen, k.SoDienThoai, k.SoDonHang.ToString("N0"), k.TongChiTieu, k.TrangThaiHienThi);
                    }
                }

                if (items.Count == 0) _topSpending.Subtitle = "CHƯA CÓ ĐƠN HÀNG HOÀN THÀNH NÀO";
                else _topSpending.Subtitle = "TỔNG TIỀN CÁC ĐƠN ĐÃ HOÀN THÀNH";

                _topSpending.SetItems(items);
            }
            catch (ApiException)
            {
                if (IsDisposed) return;
                _topSpending.Subtitle = "KHÔNG TẢI ĐƯỢC DỮ LIỆU — KIỂM TRA API";
                _topSpending.SetItems(new List<KeyValuePair<string, double>>());
                _dgvSpending.Rows.Clear();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls.BaoCao;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.BaoCao
{
    /// <summary>
    /// Trang "Báo cáo doanh thu" – thiết kế theo mockup Lẩu Phố:
    /// Chỉ dùng chữ (không icon), bảng màu ấm (#C2410C / #FAF6F2),
    /// 3 tab: Tài chính theo quý (chia đôi bảng + biểu đồ đường cong),
    /// Doanh thu theo nhân viên (bảng phân trang + biểu đồ Top 10),
    /// Doanh thu theo món ăn (bảng phân trang + biểu đồ Top 10, 4 quý / 3 năm).
    /// </summary>
    public class BaoCaoBanHangForm : Form
    {
        private readonly BaoCaoService _service;

        // Cuộn trang chính
        private Panel _scroll;

        // 1. Thanh lọc năm
        private BcCard _cardFilter;
        private Label _lblNam;
        private BcSelect _selNam;
        private BcButton _btnRefresh;
        private Label _lblStatus;

        // 2. 4 thẻ KPI
        private BaoCaoKpiTile _kpiThu;
        private BaoCaoKpiTile _kpiChi;
        private BaoCaoKpiTile _kpiLoiNhuan;
        private BaoCaoKpiTile _kpiTop;

        // 3. Thanh tab
        private BcTabBar _tabs;

        // 4. Tab 1 – Tài chính theo quý (chia đôi)
        private Panel _viewQuarter;
        private BcCard _cardQLeft;
        private BcTable _tblQuarter;
        private BcMiniStat _statBest;
        private BcMiniStat _statLoss;
        private BcMiniStat _statGrowth;
        private BcCard _cardQRight;
        private BcLegend _legQuarter;
        private BcQuarterChart _chartQuarter;

        // 5. Tab 2 – Doanh thu theo nhân viên
        private BcCard _cardEmp;
        private BcSearchBox _searchEmp;
        private BcSelect _selRoleEmp;
        private BcSegmented _segEmpMode;
        private BcTable _tblEmp;
        private BcPager _pagerEmp;
        private BcChips _chipsEmpPeriod;
        private BcLegend _legEmp;
        private BcStackedBarChart _chartEmp;
        private Label _lblEmpHint;
        private bool _updatingFilterNV;

        // 6. Tab 3 – Doanh thu theo món ăn
        private BcCard _cardDish;
        private BcSegmented _segDishSpan;
        private BcSearchBox _searchDish;
        private BcSelect _selCatDish;
        private BcSegmented _segDishMode;
        private BcTable _tblDish;
        private BcPager _pagerDish;
        private BcChips _chipsDishMetric;
        private BcLegend _legDish;
        private BcStackedBarChart _chartDish;
        private Label _lblDishHint;
        private bool _updatingFilterMon;

        // Dữ liệu hiện tại
        private BaoCaoDoanhThuTheoQuyDto _data;
        private bool _loaded;

        public BaoCaoBanHangForm()
        {
            _service = new BaoCaoService();

            Text = "Báo cáo doanh thu | Lẩu Phố";
            Tag = UiStyler.NoStyleTag;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1240, 860);
            MinimumSize = new Size(980, 680);
            BackColor = BcTheme.Bg;
            Font = BcTheme.F(9.5F);

            BuildUI();

            Shown += async (s, e) =>
            {
                InitYears();
                Relayout();
                await LoadDataAsync();
            };
        }

        private void InitYears()
        {
            int y = DateTime.Now.Year;
            _selNam.SetItems(new[] { y.ToString(), (y - 1).ToString(), (y - 2).ToString() }, 0);
        }

        private int SelectedYear
        {
            get
            {
                int y;
                return _selNam?.SelectedItem != null && int.TryParse(_selNam.SelectedItem, out y) ? y : DateTime.Now.Year;
            }
        }

        // ═════════════════════════════════════════════════════════════════
        //  DỰNG GIAO DIỆN
        // ═════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            _scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = BcTheme.Bg
            };
            _scroll.HorizontalScroll.Enabled = false;
            _scroll.HorizontalScroll.Visible = false;
            Controls.Add(_scroll);

            // ── 1. Thanh lọc năm ────────────────────────────────────────
            _cardFilter = new BcCard { Height = 64 };
            _lblNam = new Label
            {
                Text = "Năm tài chính",
                AutoSize = true,
                Font = BcTheme.F(10F, FontStyle.Bold),
                ForeColor = BcTheme.Text,
                BackColor = Color.White
            };
            _selNam = new BcSelect { BoldValue = true };
            _selNam.SelectedIndexChanged += async (s, e) =>
            {
                if (_loaded) await LoadDataAsync();
            };
            _btnRefresh = new BcButton("Làm mới");
            _btnRefresh.Click += async (s, e) => await LoadDataAsync();
            _lblStatus = new Label
            {
                Text = "",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,
                Font = BcTheme.F(9.25F),
                ForeColor = BcTheme.Muted,
                BackColor = Color.White
            };
            _cardFilter.Controls.AddRange(new Control[] { _lblNam, _selNam, _btnRefresh, _lblStatus });
            _scroll.Controls.Add(_cardFilter);

            // ── 2. 4 thẻ KPI ────────────────────────────────────────────
            _kpiThu = new BaoCaoKpiTile("Tổng doanh thu", "0 đ", "0 đơn hoàn tất", "", BcTheme.Brand);
            _kpiChi = new BaoCaoKpiTile("Tổng chi phí", "0 đ", "Nhập kho & giá vốn", "", BcTheme.AmberText);
            _kpiLoiNhuan = new BaoCaoKpiTile("Lợi nhuận ròng", "0 đ", "Biên lãi 0.0%", "", BcTheme.Green);
            _kpiTop = new BaoCaoKpiTile("Nhân viên xuất sắc", "Chưa có", "Món bán chạy: Chưa có", "", BcTheme.Text2);
            _scroll.Controls.AddRange(new Control[] { _kpiThu, _kpiChi, _kpiLoiNhuan, _kpiTop });

            // ── 3. Thanh tab ────────────────────────────────────────────
            _tabs = new BcTabBar
            {
                Items = new[] { "Doanh thu theo món ăn", "Tài chính theo quý", "Doanh thu theo nhân viên" }
            };
            _tabs.SelectedIndexChanged += (s, e) => SwitchTab(_tabs.SelectedIndex);
            _scroll.Controls.Add(_tabs);

            // ── 4. Ba vùng nội dung tab ─────────────────────────────────
            BuildTabDishes();
            BuildTabQuarter();
            BuildTabEmployees();

            _scroll.Resize += (s, e) => Relayout();
            SwitchTab(0);
        }

        // ─────────────── Tab 1: Tài chính theo quý ───────────────
        private void BuildTabQuarter()
        {
            _viewQuarter = new Panel { BackColor = BcTheme.Bg };

            _cardQLeft = new BcCard
            {
                Title = "Bảng tài chính theo quý",
                Subtitle = "Tổng thu, tổng chi và lợi nhuận năm " + DateTime.Now.Year + " · Đơn vị: VNĐ"
            };
            _tblQuarter = new BcTable { RowHeight = 38, HeaderHeight = 36 };
            _statBest = new BcMiniStat();
            _statLoss = new BcMiniStat();
            _statGrowth = new BcMiniStat();
            _cardQLeft.Controls.AddRange(new Control[] { _tblQuarter, _statBest, _statLoss, _statGrowth });

            _cardQRight = new BcCard
            {
                Title = "Thu – Chi – Lợi nhuận theo quý",
                Subtitle = "Rê chuột vào từng quý để xem chi tiết"
            };
            _legQuarter = new BcLegend();
            _legQuarter.SetItems(
                new[] { "Tổng thu", "Tổng chi", "Lợi nhuận" },
                new[] { BcTheme.Brand, BcTheme.Amber, BcTheme.Green });
            _chartQuarter = new BcQuarterChart();
            _cardQRight.Controls.AddRange(new Control[] { _legQuarter, _chartQuarter });

            _viewQuarter.Controls.AddRange(new Control[] { _cardQLeft, _cardQRight });
            _scroll.Controls.Add(_viewQuarter);
        }

        // ─────────────── Tab 2: Doanh thu theo nhân viên ───────────────
        private void BuildTabEmployees()
        {
            _cardEmp = new BcCard
            {
                Title = "Doanh thu theo nhân viên phục vụ",
                Subtitle = "Xếp hạng theo tổng doanh số năm " + DateTime.Now.Year + " · Đơn vị: VNĐ",
                Visible = false
            };

            _searchEmp = new BcSearchBox("Tìm mã, họ tên nhân viên…") { Width = 300 };
            _searchEmp.SearchChanged += (s, e) => { _pagerEmp.SetState(_pagerEmp.Total, 1); RenderEmployees(); };

            _selRoleEmp = new BcSelect("Chức vụ");
            _selRoleEmp.SetItems(new[] { "Tất cả" }, 0);
            _selRoleEmp.SelectedIndexChanged += (s, e) =>
            {
                if (_updatingFilterNV) return;
                _pagerEmp.SetState(_pagerEmp.Total, 1);
                RenderEmployees();
            };

            _segEmpMode = new BcSegmented("Xem bảng", "Xem biểu đồ");
            _segEmpMode.SelectedIndexChanged += (s, e) => { RenderEmployees(); Relayout(); };

            _tblEmp = new BcTable { RowHeight = 38, HeaderHeight = 36, EmptyText = "Không có nhân viên phù hợp" };
            _pagerEmp = new BcPager { Unit = "nhân viên", PageSize = 10 };
            _pagerEmp.PageChanged += (s, e) => RenderEmployees();

            _chipsEmpPeriod = new BcChips("Cả năm", "Quý 1", "Quý 2", "Quý 3", "Quý 4");
            _chipsEmpPeriod.SelectedIndexChanged += (s, e) => { RenderEmployees(); Relayout(); };

            _legEmp = new BcLegend();
            _legEmp.SetItems(BcTheme.QuarterNames, BcTheme.QuarterColors);

            _chartEmp = new BcStackedBarChart
            {
                Colors = BcTheme.QuarterColors,
                SeriesNames = BcTheme.QuarterNames,
                FormatValue = BcTheme.Money,
                FormatAxis = BcTheme.MoneyAxis,
                EmptyText = "Chưa phát sinh doanh thu",
                NoDataText = "Không có nhân viên phù hợp"
            };
            _chartEmp.ItemClick += (s, it) =>
            {
                var nv = it?.Tag as DongNhanVienQuyDto;
                if (nv == null) return;
                _searchEmp.Value = string.Equals(_searchEmp.Value.Trim(), nv.MaNhanVien, StringComparison.OrdinalIgnoreCase)
                    ? ""
                    : nv.MaNhanVien;
                _segEmpMode.SelectedIndex = 0;
            };

            _lblEmpHint = new Label
            {
                Text = "",
                AutoSize = false,
                Height = 20,
                Font = BcTheme.F(9F),
                ForeColor = BcTheme.Muted,
                BackColor = Color.White
            };

            _cardEmp.Controls.AddRange(new Control[]
            {
                _searchEmp, _selRoleEmp, _segEmpMode,
                _tblEmp, _pagerEmp,
                _chipsEmpPeriod, _legEmp, _chartEmp, _lblEmpHint
            });
            _scroll.Controls.Add(_cardEmp);
        }

        // ─────────────── Tab 3: Doanh thu theo món ăn ───────────────
        private void BuildTabDishes()
        {
            _cardDish = new BcCard
            {
                Title = "Doanh thu theo món ăn",
                Subtitle = "Xếp hạng theo tổng doanh thu năm " + DateTime.Now.Year + " · Đơn vị: VNĐ",
                Visible = false
            };

            _segDishSpan = new BcSegmented("4 quý của năm", "So sánh 3 năm liên tiếp");
            _segDishSpan.SelectedIndexChanged += (s, e) => { _pagerDish.SetState(_pagerDish.Total, 1); RenderDishes(); Relayout(); };

            _searchDish = new BcSearchBox("Tìm tên món, mã món…") { Width = 300 };
            _searchDish.SearchChanged += (s, e) => { _pagerDish.SetState(_pagerDish.Total, 1); RenderDishes(); };

            _selCatDish = new BcSelect("Danh mục");
            _selCatDish.SetItems(new[] { "Tất cả" }, 0);
            _selCatDish.SelectedIndexChanged += (s, e) =>
            {
                if (_updatingFilterMon) return;
                _pagerDish.SetState(_pagerDish.Total, 1);
                RenderDishes();
            };

            _segDishMode = new BcSegmented("Xem bảng", "Xem biểu đồ");
            _segDishMode.SelectedIndexChanged += (s, e) => { RenderDishes(); Relayout(); };

            _tblDish = new BcTable { RowHeight = 38, HeaderHeight = 36, EmptyText = "Không có món ăn phù hợp" };
            _pagerDish = new BcPager { Unit = "món", PageSize = 10 };
            _pagerDish.PageChanged += (s, e) => RenderDishes();

            _chipsDishMetric = new BcChips("Theo doanh thu", "Theo số lượng bán");
            _chipsDishMetric.SelectedIndexChanged += (s, e) => { RenderDishes(); Relayout(); };

            _legDish = new BcLegend();
            _legDish.SetItems(BcTheme.QuarterNames, BcTheme.QuarterColors);

            _chartDish = new BcStackedBarChart
            {
                Colors = BcTheme.QuarterColors,
                SeriesNames = BcTheme.QuarterNames,
                FormatValue = BcTheme.Money,
                FormatAxis = BcTheme.MoneyAxis,
                EmptyText = "Chưa phát sinh doanh thu",
                NoDataText = "Không có món ăn phù hợp"
            };
            _chartDish.ItemClick += (s, it) =>
            {
                var d = it?.Tag as DongMonAnDoanhThuDto;
                if (d == null) return;
                _searchDish.Value = d.TenMon ?? d.MaMon ?? "";
                _segDishMode.SelectedIndex = 0;
            };

            _lblDishHint = new Label
            {
                Text = "",
                AutoSize = false,
                Height = 20,
                Font = BcTheme.F(9F),
                ForeColor = BcTheme.Muted,
                BackColor = Color.White
            };

            _cardDish.Controls.AddRange(new Control[]
            {
                _segDishSpan, _searchDish, _selCatDish, _segDishMode,
                _tblDish, _pagerDish,
                _chipsDishMetric, _legDish, _chartDish, _lblDishHint
            });
            _scroll.Controls.Add(_cardDish);
        }

        private void SwitchTab(int index)
        {
            _cardDish.Visible = index == 0;
            _viewQuarter.Visible = index == 1;
            _cardEmp.Visible = index == 2;
            Relayout();
        }

        // ═════════════════════════════════════════════════════════════════
        //  BỐ CỤC RESPONSIVE (THEO KÍCH THƯỚC KHUNG NHÌN)
        // ═════════════════════════════════════════════════════════════════
        private void Relayout()
        {
            if (_scroll == null || _cardFilter == null || _viewQuarter == null || _cardEmp == null || _cardDish == null || _scroll.ClientSize.Width <= 0) return;

            const int padX = 18;
            const int gap = 14;
            int sx = _scroll.HorizontalScroll.Value;
            int sy = _scroll.VerticalScroll.Value;
            int x0 = padX - sx;
            int y = 8 - sy;
            int w = Math.Max(680, _scroll.ClientSize.Width - padX * 2);

            _scroll.SuspendLayout();

            // 1. Thanh lọc
            _cardFilter.SetBounds(x0, y, w, 58);
            _lblNam.Location = new Point(18, (58 - _lblNam.Height) / 2);
            _selNam.Location = new Point(_lblNam.Right + 10, (58 - _selNam.Height) / 2);
            _btnRefresh.Location = new Point(_selNam.Right + 10, (58 - _btnRefresh.Height) / 2);
            int statusX = _btnRefresh.Right + 14;
            _lblStatus.SetBounds(statusX, 15, Math.Max(80, w - statusX - 18), 28);
            y += 58 + gap;

            // 2. Hàng 4 KPI (4 cột bằng nhau)
            int kw = (w - gap * 3) / 4;
            var kpis = new[] { _kpiThu, _kpiChi, _kpiLoiNhuan, _kpiTop };
            for (int i = 0; i < 4; i++)
            {
                int cx = x0 + i * (kw + gap);
                int cw = i == 3 ? (x0 + w - cx) : kw;
                kpis[i].SetBounds(cx, y, cw, 104);
            }
            y += 104 + gap;

            // 3. Thanh tab
            _tabs.SetBounds(x0, y, w, 50);
            y += 50 + gap;

            // 4. Nội dung từng tab
            if (_viewQuarter.Visible)
                LayoutTabQuarter(x0, y, w, gap);
            else if (_cardEmp.Visible)
                LayoutTabEmployees(x0, y, w);
            else if (_cardDish.Visible)
                LayoutTabDishes(x0, y, w);

            _scroll.ResumeLayout(true);
        }

        private void LayoutTabQuarter(int x0, int y, int w, int gap)
        {
            int half = (w - gap) / 2;
            int rightW = w - half - gap;
            const int p = 20;

            _tblQuarter.Location = new Point(p, 70);
            _tblQuarter.Width = half - p * 2;

            int miniY = _tblQuarter.Bottom + 14;
            int mw = (_tblQuarter.Width - 20) / 3;
            _statBest.SetBounds(p, miniY, mw, 60);
            _statLoss.SetBounds(p + mw + 10, miniY, mw, 60);
            _statGrowth.SetBounds(p + (mw + 10) * 2, miniY, _tblQuarter.Width - (mw + 10) * 2, 60);

            int cardH = Math.Max(430, _statBest.Bottom + 18);
            _cardQLeft.SetBounds(0, 0, half, cardH);
            _cardQRight.SetBounds(half + gap, 0, rightW, cardH);

            int titleW = BcTheme.Measure(_cardQRight.Title, BcTheme.F(12F, FontStyle.Bold));
            bool legendOnTop = (rightW - p * 2 - _legQuarter.Width - 14) >= titleW;
            if (legendOnTop)
            {
                _cardQRight.HeaderRightReserve = _legQuarter.Width + 12;
                _legQuarter.Location = new Point(rightW - p - _legQuarter.Width, 18);
                _chartQuarter.SetBounds(p, 70, rightW - p * 2, cardH - 70 - 16);
            }
            else
            {
                _cardQRight.HeaderRightReserve = 0;
                _legQuarter.Location = new Point(p, 64);
                _chartQuarter.SetBounds(p, 88, rightW - p * 2, cardH - 88 - 16);
            }

            _viewQuarter.SetBounds(x0, y, w, cardH + 20);
        }

        private void LayoutTabEmployees(int x0, int y, int w)
        {
            const int p = 20;
            int iw = w - p * 2;
            bool isChart = _segEmpMode.SelectedIndex == 1;

            _searchEmp.Location = new Point(p, 72);
            _selRoleEmp.Location = new Point(_searchEmp.Right + 10, 72);
            _segEmpMode.Location = new Point(w - p - _segEmpMode.Width, 72);

            _tblEmp.Visible = !isChart;
            _pagerEmp.Visible = !isChart;
            _chipsEmpPeriod.Visible = isChart;
            _legEmp.Visible = isChart;
            _chartEmp.Visible = isChart;
            _lblEmpHint.Visible = isChart;

            int bottom;
            if (!isChart)
            {
                _tblEmp.Location = new Point(p, 118);
                _tblEmp.Width = iw;
                _pagerEmp.SetBounds(p, _tblEmp.Bottom, iw, 46);
                bottom = _pagerEmp.Bottom + 16;
            }
            else
            {
                _chipsEmpPeriod.Location = new Point(p, 118);
                _legEmp.Location = new Point(w - p - _legEmp.Width, 123);
                _chartEmp.Location = new Point(p, 158);
                _chartEmp.Width = iw;
                _lblEmpHint.SetBounds(p, _chartEmp.Bottom + 8, iw, 20);
                bottom = _lblEmpHint.Bottom + 16;
            }
            _cardEmp.SetBounds(x0, y, w, bottom);
        }

        private void LayoutTabDishes(int x0, int y, int w)
        {
            const int p = 20;
            int iw = w - p * 2;
            bool isChart = _segDishMode.SelectedIndex == 1;

            _cardDish.HeaderRightReserve = _segDishSpan.Width + 12;
            _segDishSpan.Location = new Point(w - p - _segDishSpan.Width, 18);
            _searchDish.Location = new Point(p, 72);
            _selCatDish.Location = new Point(_searchDish.Right + 10, 72);
            _segDishMode.Location = new Point(w - p - _segDishMode.Width, 72);

            _tblDish.Visible = !isChart;
            _pagerDish.Visible = !isChart;
            _chipsDishMetric.Visible = isChart;
            _legDish.Visible = isChart;
            _chartDish.Visible = isChart;
            _lblDishHint.Visible = isChart;

            int bottom;
            if (!isChart)
            {
                _tblDish.Location = new Point(p, 118);
                _tblDish.Width = iw;
                _pagerDish.SetBounds(p, _tblDish.Bottom, iw, 46);
                bottom = _pagerDish.Bottom + 16;
            }
            else
            {
                _chipsDishMetric.Location = new Point(p, 118);
                _legDish.Location = new Point(w - p - _legDish.Width, 123);
                _chartDish.Location = new Point(p, 158);
                _chartDish.Width = iw;
                _lblDishHint.SetBounds(p, _chartDish.Bottom + 8, iw, 20);
                bottom = _lblDishHint.Bottom + 16;
            }
            _cardDish.SetBounds(x0, y, w, bottom);
        }

        // ═════════════════════════════════════════════════════════════════
        //  TẢI DỮ LIỆU TỪ API
        // ═════════════════════════════════════════════════════════════════
        private async Task LoadDataAsync()
        {
            int year = SelectedYear;
            _lblStatus.Text = "Đang tổng hợp dữ liệu năm " + year + "...";
            _btnRefresh.Enabled = false;

            try
            {
                var data = await _service.GetBaoCaoDoanhThuTheoQuyAsync(year);
                if (data != null)
                {
                    _data = data;
                    _loaded = true;

                    // Đồng bộ danh sách năm từ CSDL
                    if (data.DanhSachNam != null && data.DanhSachNam.Count > 0)
                    {
                        var years = _selNam.Items
                            .Concat(data.DanhSachNam.Select(n => n.ToString()))
                            .Distinct()
                            .OrderByDescending(s => s)
                            .ToList();
                        int idx = Math.Max(0, years.IndexOf(year.ToString()));
                        _selNam.SetItems(years, idx);
                    }

                    PopulateFilters();
                    RenderAll();
                    _lblStatus.Text = "Dữ liệu năm " + year + " cập nhật lúc " + DateTime.Now.ToString("HH:mm:ss");
                }
                else
                {
                    _lblStatus.Text = "Không thể kết nối hoặc chưa có dữ liệu năm " + year;
                }
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "Lỗi tải dữ liệu: " + ex.Message;
            }
            finally
            {
                _btnRefresh.Enabled = true;
                Relayout();
            }
        }

        private void PopulateFilters()
        {
            if (_data == null) return;

            if (_data.BangNhanVien != null)
            {
                _updatingFilterNV = true;
                try
                {
                    string prev = _selRoleEmp.SelectedItem;
                    var roles = new List<string> { "Tất cả" };
                    roles.AddRange(_data.BangNhanVien
                        .Select(x => BcTheme.Cap(x.ChucVu))
                        .Where(r => !string.IsNullOrWhiteSpace(r))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(r => r));
                    int idx = !string.IsNullOrEmpty(prev) ? roles.FindIndex(r => string.Equals(r, prev, StringComparison.OrdinalIgnoreCase)) : 0;
                    _selRoleEmp.SetItems(roles, Math.Max(0, idx));
                }
                finally { _updatingFilterNV = false; }
            }

            if (_data.BangMonAn != null)
            {
                _updatingFilterMon = true;
                try
                {
                    string prev = _selCatDish.SelectedItem;
                    var cats = new List<string> { "Tất cả" };
                    cats.AddRange(_data.BangMonAn
                        .Select(x => BcTheme.Cap(x.DanhMuc))
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(c => c));
                    int idx = !string.IsNullOrEmpty(prev) ? cats.FindIndex(c => string.Equals(c, prev, StringComparison.OrdinalIgnoreCase)) : 0;
                    _selCatDish.SetItems(cats, Math.Max(0, idx));
                }
                finally { _updatingFilterMon = false; }
            }
        }

        private void RenderAll()
        {
            if (_data == null) return;

            // 1. 4 thẻ KPI
            _kpiThu.SetValue(BcTheme.Money(_data.TongThuNam), _data.TongDonNam + " đơn hoàn tất", BcTheme.Text, BcTheme.Brand);
            _kpiChi.SetValue(BcTheme.Money(_data.TongChiNam), "Nhập kho & giá vốn", BcTheme.Text, BcTheme.AmberText);

            Color lnColor = _data.LoiNhuanNam >= 0 ? BcTheme.Green : BcTheme.Red;
            string bienLai = _data.TyLeLoiNhuanNam >= 0
                ? "Biên lãi " + BcTheme.Pct(_data.TyLeLoiNhuanNam)
                : "Biên lãi " + BcTheme.Minus + BcTheme.Pct(Math.Abs(_data.TyLeLoiNhuanNam));
            _kpiLoiNhuan.SetValue(BcTheme.Money(_data.LoiNhuanNam), bienLai, lnColor, lnColor);

            string topNv = string.IsNullOrWhiteSpace(_data.NhanVienXuatSac) ? "Chưa có" : BcTheme.Clean(_data.NhanVienXuatSac);
            string topMon = string.IsNullOrWhiteSpace(_data.MonBanChayNhat) ? "Chưa có" : BcTheme.Clean(_data.MonBanChayNhat);
            _kpiTop.SetValue(topNv, "Món bán chạy: " + topMon, BcTheme.Text, BcTheme.Text2);

            // 2. Render 3 tab
            RenderQuarter();
            RenderEmployees();
            RenderDishes();
        }

        // ═════════════════════════════════════════════════════════════════
        //  RENDER TAB 1 – TÀI CHÍNH THEO QUÝ
        // ═════════════════════════════════════════════════════════════════
        private void RenderQuarter()
        {
            if (_data == null) return;
            int year = _data.Nam > 0 ? _data.Nam : SelectedYear;
            _cardQLeft.Subtitle = "Tổng thu, tổng chi và lợi nhuận năm " + year + " · Đơn vị: VNĐ";

            var thuRow = _data.BangTaiChinhQuy?.FirstOrDefault(r => r.ChiTieu != null && r.ChiTieu.IndexOf("thu", StringComparison.OrdinalIgnoreCase) >= 0)
                         ?? (_data.BangTaiChinhQuy != null && _data.BangTaiChinhQuy.Count > 0 ? _data.BangTaiChinhQuy[0] : new DongTaiChinhQuyDto());
            var chiRow = _data.BangTaiChinhQuy?.FirstOrDefault(r => r.ChiTieu != null && r.ChiTieu.IndexOf("chi", StringComparison.OrdinalIgnoreCase) >= 0)
                         ?? (_data.BangTaiChinhQuy != null && _data.BangTaiChinhQuy.Count > 1 ? _data.BangTaiChinhQuy[1] : new DongTaiChinhQuyDto());

            double[] thu = { thuRow.Quy1, thuRow.Quy2, thuRow.Quy3, thuRow.Quy4 };
            double[] chi = { chiRow.Quy1, chiRow.Quy2, chiRow.Quy3, chiRow.Quy4 };
            double[] ln = new double[4];
            for (int i = 0; i < 4; i++) ln[i] = thu[i] - chi[i];

            var cols = new List<BcColumn>
            {
                new BcColumn("Kỳ", false, 1.0f),
                new BcColumn("Tổng thu", true, 1.3f),
                new BcColumn("Tổng chi", true, 1.3f),
                new BcColumn("Lợi nhuận", true, 1.35f),
                new BcColumn("Biên LN", true, 0.9f)
            };

            var rows = new List<BcRow>();
            for (int i = 0; i < 4; i++)
            {
                bool pos = ln[i] >= 0;
                Color c = pos ? BcTheme.Green : BcTheme.Red;
                string marginStr = thu[i] > 0
                    ? (pos ? "" : BcTheme.Minus) + BcTheme.Pct(Math.Abs(ln[i] / thu[i] * 100))
                    : (ln[i] < 0 ? BcTheme.Minus + "100.0%" : "0.0%");

                rows.Add(new BcRow
                {
                    Cells = new[]
                    {
                        BcCell.T(BcTheme.QuarterNames[i], null, true),
                        BcCell.M(thu[i]),
                        BcCell.M(chi[i]),
                        new BcCell { Text = BcTheme.SignedMoney(ln[i]), Fore = c, Bold = true, IsMoney = true },
                        BcCell.T(marginStr, c, true)
                    }
                });
            }

            double totThu = _data.TongThuNam > 0 ? _data.TongThuNam : thu.Sum();
            double totChi = _data.TongChiNam > 0 ? _data.TongChiNam : chi.Sum();
            double totLn = totThu - totChi;
            bool totPos = totLn >= 0;
            Color totColor = totPos ? BcTheme.Green : BcTheme.Red;
            string totMargin = totThu > 0
                ? (totPos ? "" : BcTheme.Minus) + BcTheme.Pct(Math.Abs(totLn / totThu * 100))
                : "0.0%";

            rows.Add(new BcRow
            {
                Total = true,
                Cells = new[]
                {
                    BcCell.T("Cả năm", null, true),
                    BcCell.M(totThu, true),
                    BcCell.M(totChi, true),
                    new BcCell { Text = BcTheme.SignedMoney(totLn), Fore = totColor, Bold = true, IsMoney = true },
                    BcCell.T(totMargin, totColor, true)
                }
            });

            _tblQuarter.SetData(cols, rows);
            _chartQuarter.SetData(thu, chi, ln, year);

            // 3 ô tóm tắt dưới bảng
            double maxThu = thu.Max();
            int bestIdx = maxThu > 0 ? Array.IndexOf(thu, maxThu) : -1;
            _statBest.SetValue("Quý cao nhất", bestIdx >= 0 ? BcTheme.QuarterNames[bestIdx] : "Chưa có");

            var lossQuarters = Enumerable.Range(0, 4).Where(i => ln[i] < 0).Select(i => BcTheme.QuarterNames[i]).ToList();
            if (lossQuarters.Count > 0)
                _statLoss.SetValue("Quý lỗ", string.Join(", ", lossQuarters), BcTheme.Red);
            else
                _statLoss.SetValue("Quý lỗ", "Không có", BcTheme.Green);

            // Tăng trưởng: ưu tiên quý gần nhất có dữ liệu hoàn chỉnh so với quý liền trước
            int curQ = 2; // mặc định Q3 so Q2
            for (int i = 3; i >= 1; i--)
            {
                if (thu[i] > 0 && thu[i - 1] > 0)
                {
                    // Nếu đang trong năm hiện tại và quý cuối có doanh thu quá nhỏ so với quý trước (chưa kết thúc quý), ưu tiên quý đỉnh liền trước
                    if (i == 3 && year == DateTime.Now.Year && thu[3] < thu[2] * 0.5 && thu[1] > 0)
                        curQ = 2;
                    else
                        curQ = i;
                    break;
                }
            }
            string growthLabel = "Tăng trưởng Q" + (curQ + 1) + " so Q" + curQ;
            if (thu[curQ - 1] > 0)
            {
                double g = (thu[curQ] - thu[curQ - 1]) / thu[curQ - 1] * 100;
                _statGrowth.SetValue(growthLabel, BcTheme.SignedPct(g), g >= 0 ? BcTheme.Green : BcTheme.Red);
            }
            else
            {
                _statGrowth.SetValue(growthLabel, "0.0%", BcTheme.Muted);
            }
        }

        // ═════════════════════════════════════════════════════════════════
        //  RENDER TAB 2 – DOANH THU THEO NHÂN VIÊN
        // ═════════════════════════════════════════════════════════════════
        private static void RoleBadgeColors(string role, out Color back, out Color fore)
        {
            if (string.Equals(role, "Bán hàng", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(role, "Vận hành", StringComparison.OrdinalIgnoreCase))
            {
                back = BcTheme.BrandSoft;
                fore = BcTheme.Brand;
            }
            else if (string.Equals(role, "Sale", StringComparison.OrdinalIgnoreCase))
            {
                back = BcTheme.AmberSoft;
                fore = BcTheme.AmberText;
            }
            else
            {
                back = BcTheme.TagBg;
                fore = BcTheme.Text2;
            }
        }

        private static double EmpPeriodValue(DongNhanVienQuyDto nv, int periodIdx)
        {
            switch (periodIdx)
            {
                case 1: return nv.Quy1;
                case 2: return nv.Quy2;
                case 3: return nv.Quy3;
                case 4: return nv.Quy4;
                default: return nv.TongNam;
            }
        }

        private void RenderEmployees()
        {
            if (_data?.BangNhanVien == null) return;
            int year = _data.Nam > 0 ? _data.Nam : SelectedYear;
            bool isChart = _segEmpMode.SelectedIndex == 1;

            string q = (_searchEmp.Value ?? "").Trim();
            string roleFilter = _selRoleEmp.SelectedIndex > 0 ? _selRoleEmp.SelectedItem : null;

            // Sắp xếp toàn bộ danh sách theo doanh thu giảm dần để gán hạng chuẩn từ 1
            var orderedAll = _data.BangNhanVien
                .OrderByDescending(x => x.TongNam)
                .ThenBy(x => x.MaNhanVien)
                .ToList();

            var filtered = orderedAll
                .Where(nv =>
                {
                    string r = BcTheme.Cap(nv.ChucVu);
                    if (!string.IsNullOrEmpty(roleFilter) && !string.Equals(r, roleFilter, StringComparison.OrdinalIgnoreCase))
                        return false;
                    return UiStyler.MatchesSearch(q, nv.MaNhanVien, nv.TenNhanVien, r);
                })
                .ToList();

            double sysTotal = _data.TongThuNam > 0 ? _data.TongThuNam : orderedAll.Sum(x => x.TongNam);

            if (!isChart)
            {
                _cardEmp.Title = "Doanh thu theo nhân viên phục vụ";
                _cardEmp.Subtitle = "Xếp hạng theo tổng doanh số năm " + year + " · Đơn vị: VNĐ";

                _pagerEmp.SetState(filtered.Count, _pagerEmp.Page);
                var pageItems = filtered
                    .Skip((_pagerEmp.Page - 1) * _pagerEmp.PageSize)
                    .Take(_pagerEmp.PageSize)
                    .ToList();

                double maxShare = filtered.Count > 0 && sysTotal > 0
                    ? filtered.Max(x => x.TongNam / sysTotal * 100)
                    : 0;

                var cols = new List<BcColumn>
                {
                    new BcColumn("Hạng", false, 0.45f),
                    new BcColumn("Mã NV", false, 0.65f),
                    new BcColumn("Họ tên nhân viên", false, 1.7f, true),
                    new BcColumn("Chức vụ", false, 0.95f, true),
                    new BcColumn("Quý 1", true, 1.15f),
                    new BcColumn("Quý 2", true, 1.15f),
                    new BcColumn("Quý 3", true, 1.15f),
                    new BcColumn("Quý 4", true, 1.15f),
                    new BcColumn("Tổng năm", true, 1.25f),
                    new BcColumn("Tỷ trọng", true, 1.1f)
                };

                var rows = new List<BcRow>();
                for (int i = 0; i < pageItems.Count; i++)
                {
                    var nv = pageItems[i];
                    int rank = (_pagerEmp.Page - 1) * _pagerEmp.PageSize + i + 1;
                    string role = BcTheme.Cap(nv.ChucVu);
                    Color tagBack, tagFore;
                    RoleBadgeColors(role, out tagBack, out tagFore);

                    double pct = sysTotal > 0 ? nv.TongNam / sysTotal * 100 : 0;
                    double ratio = maxShare > 0 ? pct / maxShare : 0;

                    rows.Add(new BcRow
                    {
                        Tag = nv,
                        Cells = new[]
                        {
                            BcCell.Rank(rank),
                            BcCell.T(nv.MaNhanVien),
                            BcCell.T(BcTheme.Clean(nv.TenNhanVien), null, true),
                            BcCell.Tag(role, tagBack, tagFore),
                            BcCell.M(nv.Quy1),
                            BcCell.M(nv.Quy2),
                            BcCell.M(nv.Quy3),
                            BcCell.M(nv.Quy4),
                            BcCell.M(nv.TongNam, true),
                            BcCell.Share(pct, ratio)
                        }
                    });
                }

                _tblEmp.SetData(cols, rows);
            }
            else
            {
                int pIdx = _chipsEmpPeriod.SelectedIndex; // 0: Cả năm, 1..4: Quý 1..4
                string pName = pIdx == 0 ? "cả năm" : BcTheme.QuarterNames[pIdx - 1];
                _cardEmp.Title = "Top 10 nhân viên doanh thu cao nhất";
                _cardEmp.Subtitle = pIdx == 0
                    ? "Năm " + year + " · Thanh xếp chồng theo quý · Đơn vị: VNĐ"
                    : "Năm " + year + " · Xếp hạng theo " + BcTheme.QuarterNames[pIdx - 1] + " · Đơn vị: VNĐ";

                var top10 = filtered
                    .OrderByDescending(nv => EmpPeriodValue(nv, pIdx))
                    .ThenByDescending(nv => nv.TongNam)
                    .ThenBy(nv => nv.MaNhanVien)
                    .Take(10)
                    .ToList();

                double periodSysTotal = orderedAll.Sum(nv => EmpPeriodValue(nv, pIdx));
                if (pIdx == 0 && _data.TongThuNam > 0) periodSysTotal = _data.TongThuNam;

                var barItems = top10.Select(nv =>
                {
                    double tot = EmpPeriodValue(nv, pIdx);
                    double[] vals = pIdx == 0
                        ? new[] { nv.Quy1, nv.Quy2, nv.Quy3, nv.Quy4 }
                        : Enumerable.Range(1, 4).Select(k => k == pIdx ? tot : 0).ToArray();
                    return new BcBarItem
                    {
                        Name = BcTheme.Clean(nv.TenNhanVien),
                        Sub = nv.MaNhanVien + " · " + BcTheme.Cap(nv.ChucVu),
                        Values = vals,
                        Total = tot,
                        Share = periodSysTotal > 0 ? tot / periodSysTotal * 100 : 0,
                        Tag = nv
                    };
                }).ToList();

                _chartEmp.SetItems(barItems);

                var active = barItems.Where(b => b.Total > 0).ToList();
                double activeShare = active.Sum(b => b.Share);
                _lblEmpHint.Text = active.Count > 0
                    ? "Top " + active.Count + " nhân viên chiếm " + BcTheme.Pct(activeShare) + " doanh thu " + pName + ". Nhấn vào một thanh để lọc chi tiết nhân viên."
                    : "Chưa phát sinh doanh thu nhân viên trong " + pName + ".";
            }
        }

        // ═════════════════════════════════════════════════════════════════
        //  RENDER TAB 3 – DOANH THU THEO MÓN ĂN
        // ═════════════════════════════════════════════════════════════════
        private void RenderDishes()
        {
            if (_data?.BangMonAn == null) return;
            int year = _data.Nam > 0 ? _data.Nam : SelectedYear;
            bool is3Years = _segDishSpan.SelectedIndex == 1;
            bool isChart = _segDishMode.SelectedIndex == 1;
            bool byQty = isChart && _chipsDishMetric.SelectedIndex == 1;

            int n1 = _data.CacNamSoSanhMonAn != null && _data.CacNamSoSanhMonAn.Count > 0 ? _data.CacNamSoSanhMonAn[0] : year - 2;
            int n2 = _data.CacNamSoSanhMonAn != null && _data.CacNamSoSanhMonAn.Count > 1 ? _data.CacNamSoSanhMonAn[1] : year - 1;
            int n3 = _data.CacNamSoSanhMonAn != null && _data.CacNamSoSanhMonAn.Count > 2 ? _data.CacNamSoSanhMonAn[2] : year;

            string q = (_searchDish.Value ?? "").Trim();
            string catFilter = _selCatDish.SelectedIndex > 0 ? _selCatDish.SelectedItem : null;

            Func<DongMonAnDoanhThuDto, double> revOf = m =>
                is3Years ? (m.DoanhThuNam1 + m.DoanhThuNam2 + m.DoanhThuNam3) : m.TongDoanhThu;

            // Mặc định sắp xếp doanh thu giảm dần, bắt đầu từ hạng 1
            var orderedAll = _data.BangMonAn
                .OrderByDescending(m => byQty ? m.TongSoLuongBan : revOf(m))
                .ThenByDescending(m => m.TongSoLuongBan)
                .ThenBy(m => m.MaMon)
                .ToList();

            var filtered = orderedAll
                .Where(m =>
                {
                    string cat = BcTheme.Cap(m.DanhMuc);
                    if (!string.IsNullOrEmpty(catFilter) && !string.Equals(cat, catFilter, StringComparison.OrdinalIgnoreCase))
                        return false;
                    return UiStyler.MatchesSearch(q, m.TenMon, cat, m.MaMon);
                })
                .ToList();

            double totalRevAll = !is3Years && _data.TongThuNam > 0
                ? _data.TongThuNam
                : _data.BangMonAn.Sum(revOf);
            double totalQtyAll = _data.BangMonAn.Sum(m => (double)m.TongSoLuongBan);

            if (!isChart)
            {
                _cardDish.Title = "Doanh thu theo món ăn";
                _cardDish.Subtitle = !is3Years
                    ? "Xếp hạng theo tổng doanh thu năm " + year + " · Đơn vị: VNĐ"
                    : "So sánh doanh thu 3 năm (" + n1 + " " + BcTheme.EnDash + " " + n3 + ") · Đơn vị: VNĐ";

                _pagerDish.SetState(filtered.Count, _pagerDish.Page);
                var pageItems = filtered
                    .Skip((_pagerDish.Page - 1) * _pagerDish.PageSize)
                    .Take(_pagerDish.PageSize)
                    .ToList();

                double maxShare = filtered.Count > 0 && totalRevAll > 0
                    ? filtered.Max(m => revOf(m) / totalRevAll * 100)
                    : 0;

                List<BcColumn> cols;
                if (!is3Years)
                {
                    cols = new List<BcColumn>
                    {
                        new BcColumn("Hạng", false, 0.45f),
                        new BcColumn("Mã món", false, 0.65f),
                        new BcColumn("Tên món ăn", false, 1.6f, true),
                        new BcColumn("Danh mục", false, 1.05f, true),
                        new BcColumn("SL", true, 0.5f),
                        new BcColumn("Quý 1", true, 1.1f),
                        new BcColumn("Quý 2", true, 1.1f),
                        new BcColumn("Quý 3", true, 1.1f),
                        new BcColumn("Quý 4", true, 1.1f),
                        new BcColumn("Tổng năm", true, 1.25f),
                        new BcColumn("Tỷ trọng", true, 1.05f)
                    };
                }
                else
                {
                    cols = new List<BcColumn>
                    {
                        new BcColumn("Hạng", false, 0.45f),
                        new BcColumn("Mã món", false, 0.7f),
                        new BcColumn("Tên món ăn", false, 1.7f, true),
                        new BcColumn("Danh mục", false, 1.1f, true),
                        new BcColumn("SL", true, 0.55f),
                        new BcColumn("Năm " + n1, true, 1.2f),
                        new BcColumn("Năm " + n2, true, 1.2f),
                        new BcColumn("Năm " + n3, true, 1.2f),
                        new BcColumn("Tổng 3 năm", true, 1.3f),
                        new BcColumn("Tỷ trọng", true, 1.1f)
                    };
                }

                var rows = new List<BcRow>();
                for (int i = 0; i < pageItems.Count; i++)
                {
                    var m = pageItems[i];
                    int rank = (_pagerDish.Page - 1) * _pagerDish.PageSize + i + 1;
                    string cat = BcTheme.Cap(m.DanhMuc);
                    double tot = revOf(m);
                    double pct = totalRevAll > 0 ? tot / totalRevAll * 100 : 0;
                    double ratio = maxShare > 0 ? pct / maxShare : 0;

                    BcCell[] cells = !is3Years
                        ? new[]
                        {
                            BcCell.Rank(rank),
                            BcCell.T(m.MaMon),
                            BcCell.T(BcTheme.Clean(m.TenMon), null, true),
                            BcCell.Tag(cat, BcTheme.TagBg, BcTheme.Text2),
                            BcCell.T(m.TongSoLuongBan.ToString(BcTheme.Num), m.TongSoLuongBan == 0 ? BcTheme.Zero : (Color?)null),
                            BcCell.M(m.Quy1),
                            BcCell.M(m.Quy2),
                            BcCell.M(m.Quy3),
                            BcCell.M(m.Quy4),
                            BcCell.M(tot, true),
                            BcCell.Share(pct, ratio)
                        }
                        : new[]
                        {
                            BcCell.Rank(rank),
                            BcCell.T(m.MaMon),
                            BcCell.T(BcTheme.Clean(m.TenMon), null, true),
                            BcCell.Tag(cat, BcTheme.TagBg, BcTheme.Text2),
                            BcCell.T(m.TongSoLuongBan.ToString(BcTheme.Num), m.TongSoLuongBan == 0 ? BcTheme.Zero : (Color?)null),
                            BcCell.M(m.DoanhThuNam1),
                            BcCell.M(m.DoanhThuNam2),
                            BcCell.M(m.DoanhThuNam3),
                            BcCell.M(tot, true),
                            BcCell.Share(pct, ratio)
                        };

                    rows.Add(new BcRow { Tag = m, Cells = cells });
                }

                _tblDish.SetData(cols, rows);
            }
            else
            {
                _cardDish.Title = byQty ? "Top 10 món ăn bán chạy nhất" : "Top 10 món ăn doanh thu cao nhất";
                string spanDesc = !is3Years
                    ? "Năm " + year + " · Thanh xếp chồng theo quý"
                    : "Giai đoạn " + n1 + " " + BcTheme.EnDash + " " + n3 + " · Thanh xếp chồng theo 3 năm";
                _cardDish.Subtitle = spanDesc + " · Đơn vị: " + (byQty ? "phần" : "VNĐ");

                string[] seriesNames = !is3Years
                    ? BcTheme.QuarterNames
                    : new[] { "Năm " + n1, "Năm " + n2, "Năm " + n3 };
                Color[] seriesColors = !is3Years ? BcTheme.QuarterColors : BcTheme.YearColors;

                _legDish.SetItems(seriesNames, seriesColors);
                _chartDish.SeriesNames = seriesNames;
                _chartDish.Colors = seriesColors;
                _chartDish.FormatValue = byQty ? (Func<double, string>)(v => BcTheme.Qty(v) + " phần") : BcTheme.Money;
                _chartDish.FormatAxis = byQty ? (Func<double, string>)BcTheme.Qty : BcTheme.MoneyAxis;

                var top10 = filtered.Take(10).ToList();
                double denom = byQty ? totalQtyAll : totalRevAll;

                var barItems = top10.Select(m =>
                {
                    double[] raw = !is3Years
                        ? new[] { m.Quy1, m.Quy2, m.Quy3, m.Quy4 }
                        : new[] { m.DoanhThuNam1, m.DoanhThuNam2, m.DoanhThuNam3 };
                    double rawSum = raw.Sum();
                    double tot = byQty ? m.TongSoLuongBan : revOf(m);

                    double[] vals;
                    if (!byQty)
                    {
                        vals = raw;
                    }
                    else if (rawSum > 0)
                    {
                        // Phân bổ số lượng bán theo tỷ trọng doanh thu từng kỳ
                        vals = raw.Select(v => Math.Round(tot * v / rawSum)).ToArray();
                        double diff = tot - vals.Sum();
                        int maxIdx = Array.IndexOf(raw, raw.Max());
                        if (maxIdx >= 0) vals[maxIdx] = Math.Max(0, vals[maxIdx] + diff);
                    }
                    else
                    {
                        vals = new double[raw.Length];
                        if (vals.Length > 0) vals[vals.Length - 1] = tot;
                    }

                    return new BcBarItem
                    {
                        Name = BcTheme.Clean(m.TenMon),
                        Sub = m.MaMon + " · " + BcTheme.Cap(m.DanhMuc) + " · " + m.TongSoLuongBan + " phần",
                        Values = vals,
                        Total = tot,
                        Share = denom > 0 ? tot / denom * 100 : 0,
                        Tag = m
                    };
                }).ToList();

                _chartDish.SetItems(barItems);

                double topShare = barItems.Sum(b => b.Share);
                string metricText = byQty ? "số lượng bán" : "doanh thu";
                _lblDishHint.Text = barItems.Count > 0
                    ? "Top " + barItems.Count + " món chiếm " + BcTheme.Pct(topShare) + " tổng " + metricText + ". Nhấn vào thanh để lọc bảng theo món đó."
                    : "Không có món ăn phù hợp với bộ lọc hiện tại.";
            }
        }
    }
}

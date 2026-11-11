using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.NhaCungCap
{
    public partial class PhieuNhapForm : Form
    {
        private readonly PhieuNhapService _pnService;
        private List<PhieuNhapDto> _allPhieuNhap = new List<PhieuNhapDto>();
        private readonly ComboBox _cboNhaCungCap = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _cboThoiGian = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _cboMucTien = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label _lblCount = new Label { Text = "" };
        private bool _updatingFilterList;

        // "Tìm theo" (mặc định: tất cả)
        private readonly SearchScopes<PhieuNhapDto> _scopes = new SearchScopes<PhieuNhapDto>("PhieuNhap")
            .AddAll("Tìm mã phiếu, NCC, nhân viên, ghi chú, ngày, số tiền...",
                p => new[] { p.NgayNhap.HasValue ? p.NgayNhap.Value.ToString("dd/MM/yyyy HH:mm") : null,
                             p.TongTien.HasValue ? p.TongTien.Value.ToString("#,##0") : null })
            .Add("Mã phiếu", "Mã", "Tìm theo mã phiếu...", p => new[] { p.MaPhieuNhap })
            .Add("Nhà cung cấp", "NCC", "Tìm theo tên hoặc mã NCC...", p => new[] { p.TenNhaCungCap, p.MaNhaCungCap })
            .Add("Nhân viên", "NV", "Tìm theo nhân viên lập...", p => new[] { p.TenNhanVien, p.MaNhanVien })
            .Add("Ghi chú", "Ghi chú", "Tìm trong ghi chú...", p => new[] { p.GhiChu });

        // 2 ô số liệu ở trên
        private readonly Label _kTongPhieu = new Label { Text = "Tổng phiếu: —" };
        private readonly Label _kTongGiaTri = new Label { Text = "Tổng giá trị: —" };

        // Khung chi tiết bên phải (click 1 lần vào dòng để xem)
        private readonly DetailSidePanel _side = new DetailSidePanel(
            "Chọn một phiếu nhập trong danh sách\nđể xem nguyên liệu và thông tin phiếu.", "NGUYÊN LIỆU", "SỐ LƯỢNG", "THÀNH TIỀN");
        private readonly Dictionary<string, PhieuNhapDto> _detailCache = new Dictionary<string, PhieuNhapDto>();
        private string _sideMa;

        public PhieuNhapForm()
        {
            InitializeComponent();
            _scopes.Changed += (s, e) => FilterData();
            dgvPhieuNhap.AutoGenerateColumns = false;
            dgvPhieuNhap.AllowUserToResizeColumns = true;
            SetupGridColumns();

            var baseUrl = (ApiConfig.BaseUrl ?? "http://localhost:5275").TrimEnd('/') + "/";
            var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _pnService = new PhieuNhapService(httpClient);

            _cboNhaCungCap.Items.Add("Tất cả NCC");
            _cboNhaCungCap.SelectedIndex = 0;

            _cboThoiGian.Items.AddRange(new object[] {
                "Tất cả thời gian",
                "Hôm nay",
                "7 ngày qua",
                "30 ngày qua",
                "Tháng này",
                "Năm nay"
            });
            _cboThoiGian.SelectedIndex = 0;

            _cboMucTien.Items.AddRange(new object[] {
                "Tất cả mức tiền",
                "Dưới 1.000.000 đ",
                "1 - 5 triệu đ",
                "Trên 5.000.000 đ"
            });
            _cboMucTien.SelectedIndex = 0;

            _cboNhaCungCap.SelectedIndexChanged += (s, e) => { if (!_updatingFilterList) FilterData(); };
            _cboThoiGian.SelectedIndexChanged += (s, e) => FilterData();
            _cboMucTien.SelectedIndexChanged += (s, e) => FilterData();

            // Bố cục giống trang Phản hồi: danh sách bên trái, khung chi tiết bên phải, 2 ô số liệu ở trên
            PageKit.BuildListPage(this, new ListPageSpec
            {
                Title = lblTieuDe,
                Subtitle = lblSub,
                HeaderActions = new Control[] { btnLamMoi },
                PrimaryActions = new[] { btnTaoPhieuNhap },
                KpiLabels = new[] { _kTongPhieu, _kTongGiaTri },
                Search = txtTimKiem,
                SearchIcon = lblTimKiem,
                SearchPlaceholder = "Tìm mã phiếu, NCC, nhân viên, ghi chú...",
                SearchScopes = _scopes,
                Filters = new[]
                {
                    new FilterSpec(_cboNhaCungCap, new Label { Text = "Nhà cung cấp" }),
                    new FilterSpec(_cboThoiGian, new Label { Text = "Thời gian" }),
                    new FilterSpec(_cboMucTien, new Label { Text = "Tổng tiền" })
                },
                CountLabel = _lblCount,
                Grid = dgvPhieuNhap,
                SidePanel = _side,
                SideWidth = 400,
                SideFullHeight = true,
                KpiSlots = 5
            });

            // Nút "Chi tiết" nằm trong khung bên phải; double-click dòng vẫn mở hộp thoại chi tiết
            _side.AttachButton(btnXemChiTiet, "Chi tiết ↗");
            dgvPhieuNhap.SelectionChanged += (s, e) => ShowSide(GetSelected());
            dgvPhieuNhap.DataBindingComplete += (s, e) => ApplyPendingSelection();
            // Người dùng đã thao tác trên lưới: không tự bỏ chọn nữa
            dgvPhieuNhap.CellMouseDown += (s, e) => _bindingPending = false;
            dgvPhieuNhap.KeyDown += (s, e) => _bindingPending = false;
        }

        /// <summary>
        /// Bảng hẹp vì có khung bên phải: Mã phiếu – Ngày nhập – Tổng tiền – Nhà cung cấp – Nhân viên.
        /// Ghi chú xem trong hộp thoại "Chi tiết ↗".
        /// </summary>
        private void SetupGridColumns()
        {
            dgvPhieuNhap.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPhieuNhap.ScrollBars = ScrollBars.Vertical;

            colMaNhanVien.Visible = false;
            colGhiChu.Visible = false;

            colMaPhieu.DisplayIndex = 0;
            colNgayNhap.DisplayIndex = 1;
            colTongTien.DisplayIndex = 2;
            colNhaCungCap.DisplayIndex = 3;
            colNhanVien.DisplayIndex = 4;

            colMaPhieu.FillWeight = 16; colMaPhieu.MinimumWidth = 80;
            colNgayNhap.FillWeight = 20; colNgayNhap.MinimumWidth = 110;
            colTongTien.FillWeight = 18; colTongTien.MinimumWidth = 100;
            colNhaCungCap.FillWeight = 26; colNhaCungCap.MinimumWidth = 110;
            colNhanVien.FillWeight = 20; colNhanVien.MinimumWidth = 90;

            colMaPhieu.DefaultCellStyle.Font = new System.Drawing.Font(ThemeManager.FontFamily, 9F, System.Drawing.FontStyle.Bold);
            colNhanVien.HeaderText = "Nhân Viên";
            colNhanVien.DefaultCellStyle.NullValue = "—";
            colTongTien.HeaderText = "Tổng Tiền";
            colTongTien.DefaultCellStyle.Format = "#,##0 đ";
            colTongTien.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colTongTien.DefaultCellStyle.Font = new System.Drawing.Font(ThemeManager.FontFamily, 9F, System.Drawing.FontStyle.Bold);
            colTongTien.DefaultCellStyle.ForeColor = ThemeManager.Primary;
            colTongTien.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        private async void PhieuNhapForm_Load(object sender, EventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                string keep = GetSelected()?.MaPhieuNhap;
                _allPhieuNhap = await _pnService.GetAllAsync() ?? new List<PhieuNhapDto>();
                _detailCache.Clear();
                _sideMa = null;
                _kTongPhieu.Text = "Tổng phiếu: " + _allPhieuNhap.Count.ToString("N0") + " phiếu";
                _kTongGiaTri.Text = "Tổng giá trị: " + DetailSidePanel.Money(_allPhieuNhap.Sum(p => p.TongTien ?? 0m));
                PopulateSupplierFilter();
                FilterData(keep);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách phiếu nhập: " + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateSupplierFilter()
        {
            _updatingFilterList = true;
            try
            {
                string prev = _cboNhaCungCap.SelectedItem as string;
                var suppliers = _allPhieuNhap
                    .Select(p => p.TenNhaCungCap)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(s => s)
                    .ToList();

                _cboNhaCungCap.Items.Clear();
                _cboNhaCungCap.Items.Add("Tất cả NCC");
                foreach (var s in suppliers) _cboNhaCungCap.Items.Add(s);

                int idx = !string.IsNullOrEmpty(prev) ? _cboNhaCungCap.Items.IndexOf(prev) : 0;
                _cboNhaCungCap.SelectedIndex = idx >= 0 ? idx : 0;
            }
            finally
            {
                _updatingFilterList = false;
            }
        }

        private void FilterData() { FilterData(null); }

        private void FilterData(string selectMa)
        {
            // Giữ phiếu đang xem nếu nó vẫn còn sau khi lọc
            if (selectMa == null) selectMa = GetSelected()?.MaPhieuNhap;
            var keyword = (txtTimKiem.Text ?? "").Trim();
            string selectedNcc = _cboNhaCungCap.SelectedIndex > 0 ? (_cboNhaCungCap.SelectedItem as string) : null;
            int timeIndex = _cboThoiGian.SelectedIndex;
            int amountIndex = _cboMucTien.SelectedIndex;
            DateTime today = DateTime.Today;

            var filtered = _allPhieuNhap.Where(p =>
            {
                if (!_scopes.Matches(keyword, p))
                    return false;

                if (!string.IsNullOrEmpty(selectedNcc) && !string.Equals(p.TenNhaCungCap, selectedNcc, StringComparison.OrdinalIgnoreCase))
                    return false;

                if (timeIndex > 0)
                {
                    if (!p.NgayNhap.HasValue) return false;
                    DateTime d = p.NgayNhap.Value.Date;
                    if (timeIndex == 1 && d != today) return false;
                    if (timeIndex == 2 && d < today.AddDays(-6)) return false;
                    if (timeIndex == 3 && d < today.AddDays(-29)) return false;
                    if (timeIndex == 4 && (d.Month != today.Month || d.Year != today.Year)) return false;
                    if (timeIndex == 5 && d.Year != today.Year) return false;
                }

                if (amountIndex > 0)
                {
                    decimal total = p.TongTien ?? 0m;
                    if (amountIndex == 1 && total >= 1000000m) return false;
                    if (amountIndex == 2 && (total < 1000000m || total > 5000000m)) return false;
                    if (amountIndex == 3 && total <= 5000000m) return false;
                }

                return true;
            }).OrderByDescending(p => p.NgayNhap ?? DateTime.MinValue).ToList();

            _pendingSelect = selectMa ?? "";
            _bindingPending = true;
            dgvPhieuNhap.DataSource = null;
            dgvPhieuNhap.DataSource = filtered;
            _lblCount.Text = filtered.Count == _allPhieuNhap.Count
                ? $"Hiển thị {filtered.Count} phiếu"
                : $"Hiển thị {filtered.Count} / {_allPhieuNhap.Count} phiếu";
            ApplyPendingSelection();
        }

        // Lưới tự chọn dòng đầu khi gắn dữ liệu (kể cả lần gắn hoãn lúc lưới mới hiện lên).
        // Bỏ chọn đó: chưa chọn phiếu nào thì khung bên phải để trống; có phiếu đang xem thì chọn lại phiếu đó.
        private string _pendingSelect = "";
        private bool _bindingPending;

        private void ApplyPendingSelection()
        {
            if (!_bindingPending) return;
            dgvPhieuNhap.ClearSelection();
            dgvPhieuNhap.CurrentCell = null;
            if (!string.IsNullOrEmpty(_pendingSelect))
            {
                foreach (DataGridViewRow row in dgvPhieuNhap.Rows)
                {
                    if ((row.DataBoundItem as PhieuNhapDto)?.MaPhieuNhap != _pendingSelect) continue;
                    dgvPhieuNhap.CurrentCell = row.Cells[colMaPhieu.Index];
                    row.Selected = true;
                    break;
                }
            }
            ShowSide(GetSelected());
            if (dgvPhieuNhap.IsHandleCreated && dgvPhieuNhap.Visible) _bindingPending = false;
        }

        // ── Khung chi tiết bên phải ──────────────────────────────────
        private async void ShowSide(PhieuNhapDto item)
        {
            if (item == null || string.IsNullOrEmpty(item.MaPhieuNhap))
            {
                _sideMa = null;
                _side.ShowEmpty();
                return;
            }
            string ma = item.MaPhieuNhap;
            if (ma == _sideMa) return;   // đang hiển thị (hoặc đang tải) đúng phiếu này rồi
            _sideMa = ma;

            PhieuNhapDto detail;
            if (_detailCache.TryGetValue(ma, out detail)) { RenderSide(detail); return; }

            _side.ShowLoading(ma);
            try
            {
                detail = await _pnService.GetByIdAsync(ma);
                if (IsDisposed) return;
                if (detail == null) detail = item;   // không có chi tiết: hiện tối thiểu từ dòng danh sách
                _detailCache[ma] = detail;
                if (_sideMa == ma) RenderSide(detail);   // người dùng có thể đã chọn phiếu khác
            }
            catch (Exception ex)
            {
                if (!IsDisposed && _sideMa == ma)
                {
                    RenderSide(item);
                    _side.SetItems("Không tải được chi tiết phiếu", new List<DetailItem>(), ex.Message);
                }
            }
        }

        private void RenderSide(PhieuNhapDto p)
        {
            _side.SetHeader(p.MaPhieuNhap, "Đã nhập kho", ThemeManager.Green, ThemeManager.GreenSoft);

            // Nguyên liệu – ưu tiên hiển thị đầu tiên
            var chiTiet = p.ChiTietPhieuNhaps ?? new List<ChiTietPhieuNhapDto>();
            var items = chiTiet.Select(c => new DetailItem
            {
                Name = string.IsNullOrWhiteSpace(c.TenNguyenLieu) ? c.MaNguyenLieu : c.TenNguyenLieu,
                Code = c.MaNguyenLieu + "  ·  " + DetailSidePanel.Money(c.DonGiaNhap) + (string.IsNullOrWhiteSpace(c.DonViTinh) ? "" : "/" + c.DonViTinh),
                Qty = c.SoLuong.ToString("#,##0.##") + (string.IsNullOrWhiteSpace(c.DonViTinh) ? "" : " " + c.DonViTinh),
                Amount = DetailSidePanel.Money(c.ThanhTien)
            }).ToList();
            _side.SetItems("Nguyên liệu (" + chiTiet.Count + " mặt hàng)", items, "Phiếu nhập không có nguyên liệu nào.");

            _side.SetTotals(
                new[] { DetailSidePanel.Field("Số mặt hàng", chiTiet.Count.ToString("N0")) },
                "Tổng giá trị", DetailSidePanel.Money(p.TongTien ?? chiTiet.Sum(c => c.ThanhTien)));
            // Ghi chú / thông tin khác: xem trong hộp thoại "Chi tiết ↗"
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            FilterData();
        }

        private async void btnLamMoi_Click(object sender, EventArgs e)
        {
            txtTimKiem.Clear();
            await LoadData();
        }

        private async void btnTaoPhieuNhap_Click(object sender, EventArgs e)
        {
            using (var dialog = new PhieuNhapDialog())
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadData();
                }
            }
        }

        private async void btnXemChiTiet_Click(object sender, EventArgs e)
        {
            var selected = GetSelected();
            if (selected == null)
            {
                MessageBox.Show("Vui lòng chọn một phiếu nhập để xem chi tiết.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var detail = await _pnService.GetByIdAsync(selected.MaPhieuNhap);
                if (detail != null)
                {
                    using (var dialog = new ChiTietPhieuNhapDialog(detail))
                    {
                        dialog.ShowDialog(this);
                    }
                }
                else
                {
                    MessageBox.Show("Không tìm thấy thông tin chi tiết của phiếu nhập này.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải chi tiết phiếu nhập: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvPhieuNhap_DoubleClick(object sender, EventArgs e)
        {
            btnXemChiTiet_Click(sender, e);
        }

        private PhieuNhapDto GetSelected()
        {
            if (dgvPhieuNhap.SelectedRows.Count > 0)
            {
                return dgvPhieuNhap.SelectedRows[0].DataBoundItem as PhieuNhapDto;
            }
            return null;
        }
    }
}
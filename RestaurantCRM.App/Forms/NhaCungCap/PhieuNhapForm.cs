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

        public PhieuNhapForm()
        {
            InitializeComponent();
            dgvPhieuNhap.AutoGenerateColumns = false;
            dgvPhieuNhap.AllowUserToResizeColumns = true;
            dgvPhieuNhap.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvPhieuNhap.ScrollBars = ScrollBars.Both;

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

            // Bố cục hiện đại thống nhất (giữ nguyên toàn bộ control & sự kiện của Designer)
            PageKit.BuildListPage(this, new ListPageSpec
            {
                Title = lblTieuDe,
                Subtitle = lblSub,
                HeaderActions = new Control[] { btnLamMoi },
                PrimaryActions = new[] { btnTaoPhieuNhap },
                Search = txtTimKiem,
                SearchIcon = lblTimKiem,
                SearchPlaceholder = "Tìm mã phiếu, NCC, nhân viên, ghi chú...",
                Filters = new[]
                {
                    new FilterSpec(_cboNhaCungCap, new Label { Text = "Nhà cung cấp" }),
                    new FilterSpec(_cboThoiGian, new Label { Text = "Thời gian" }),
                    new FilterSpec(_cboMucTien, new Label { Text = "Tổng tiền" })
                },
                CountLabel = _lblCount,
                Grid = dgvPhieuNhap,
                SecondaryActions = new[] { btnXemChiTiet }
            });
        }

        private async void PhieuNhapForm_Load(object sender, EventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                _allPhieuNhap = await _pnService.GetAllAsync() ?? new List<PhieuNhapDto>();
                PopulateSupplierFilter();
                FilterData();
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

        private void FilterData()
        {
            var keyword = (txtTimKiem.Text ?? "").Trim();
            string selectedNcc = _cboNhaCungCap.SelectedIndex > 0 ? (_cboNhaCungCap.SelectedItem as string) : null;
            int timeIndex = _cboThoiGian.SelectedIndex;
            int amountIndex = _cboMucTien.SelectedIndex;
            DateTime today = DateTime.Today;

            var filtered = _allPhieuNhap.Where(p =>
            {
                if (!UiStyler.MatchesSearch(keyword, p.MaPhieuNhap, p.TenNhaCungCap, p.MaNhaCungCap, p.MaNhanVien, p.TenNhanVien, p.GhiChu))
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

            dgvPhieuNhap.DataSource = null;
            dgvPhieuNhap.DataSource = filtered;
            _lblCount.Text = filtered.Count == _allPhieuNhap.Count
                ? $"Hiển thị {filtered.Count} phiếu"
                : $"Hiển thị {filtered.Count} / {_allPhieuNhap.Count} phiếu";
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
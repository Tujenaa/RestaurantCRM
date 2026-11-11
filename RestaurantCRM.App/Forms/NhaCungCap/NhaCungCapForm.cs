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
    public partial class NhaCungCapForm : Form
    {
        private readonly NhaCungCapService _nccService;
        private List<NhaCungCapDto> _allNcc = new List<NhaCungCapDto>();

        // "Tìm theo" (mặc định: tất cả)
        private readonly SearchScopes<NhaCungCapDto> _scopes = new SearchScopes<NhaCungCapDto>("NhaCungCap")
            .AddAll("Tìm mã, tên NCC, SĐT, email, địa chỉ...", n => new[] { n.TrangThaiHienThi })
            .Add("Tên nhà cung cấp", "Tên", "Tìm theo tên nhà cung cấp...", n => new[] { n.TenNhaCungCap })
            .Add("Mã nhà cung cấp", "Mã", "Tìm theo mã nhà cung cấp...", n => new[] { n.MaNhaCungCap })
            .Add("Số điện thoại", "SĐT", "Tìm theo số điện thoại...", n => new[] { n.SoDienThoai })
            .Add("Email", "Email", "Tìm theo email...", n => new[] { n.Email })
            .Add("Địa chỉ", "Địa chỉ", "Tìm theo địa chỉ...", n => new[] { n.DiaChi });
        private readonly Button btnAnHien = new Button { Text = "Ẩn/Hiện NCC", AutoSize = true };
        private readonly ComboBox _cboTrangThai = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label _lblCount = new Label { Text = "" };

        public NhaCungCapForm()
        {
            InitializeComponent();
            _scopes.Changed += (s, e) => FilterData();
            dgvNhaCungCap.AutoGenerateColumns = false;
            dgvNhaCungCap.AllowUserToResizeColumns = true;
            dgvNhaCungCap.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvNhaCungCap.ScrollBars = ScrollBars.Both;
            dgvNhaCungCap.CellFormatting += dgvNhaCungCap_CellFormatting;
            dgvNhaCungCap.SelectionChanged += dgvNhaCungCap_SelectionChanged;

            var baseUrl = (ApiConfig.BaseUrl ?? "http://localhost:5275").TrimEnd('/') + "/";
            var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _nccService = new NhaCungCapService(httpClient);

            btnAnHien.Click += btnAnHien_Click;

            _cboTrangThai.Items.AddRange(new object[] { "Tất cả", "Đang hoạt động", "Đã ẩn" });
            _cboTrangThai.SelectedIndex = 0;
            _cboTrangThai.SelectedIndexChanged += (s, e) => FilterData();

            // Bố cục hiện đại thống nhất (giữ nguyên toàn bộ control & sự kiện của Designer)
            PageKit.BuildListPage(this, new ListPageSpec
            {
                Title = lblTieuDe,
                HeaderActions = new Control[] { btnLamMoi },
                PrimaryActions = new[] { btnThem },
                Search = txtTimKiem,
                SearchIcon = lblTimKiem,
                SearchPlaceholder = "Tìm mã, tên NCC, SĐT, email, địa chỉ...",
                SearchScopes = _scopes,
                Filters = new[] { new FilterSpec(_cboTrangThai, new Label { Text = "Trạng thái" }) },
                CountLabel = _lblCount,
                Grid = dgvNhaCungCap,
                SecondaryActions = new[] { btnSua, btnAnHien },
                DangerActions = new[] { btnXoa }
            });
        }

        private async void NhaCungCapForm_Load(object sender, EventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                _allNcc = await _nccService.GetAllAsync();
                FilterData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu Nhà Cung Cấp từ API Server.\nChi tiết: " + ex.Message,
                    "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterData()
        {
            var keyword = (txtTimKiem.Text ?? "").Trim();
            int statusIdx = _cboTrangThai.SelectedIndex;

            var filtered = _allNcc.Where(n =>
            {
                if (statusIdx == 1 && !n.IsActive) return false;
                if (statusIdx == 2 && n.IsActive) return false;
                return _scopes.Matches(keyword, n);
            }).OrderByDescending(n => n.MaNhaCungCap).ToList();

            dgvNhaCungCap.DataSource = null;
            dgvNhaCungCap.DataSource = filtered;
            _lblCount.Text = filtered.Count == _allNcc.Count
                ? $"Hiển thị {filtered.Count} NCC"
                : $"Hiển thị {filtered.Count} / {_allNcc.Count} NCC";
            dgvNhaCungCap_SelectionChanged(null, null);
        }

        private void dgvNhaCungCap_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvNhaCungCap.Rows.Count) return;

            var row = dgvNhaCungCap.Rows[e.RowIndex];
            if (row.DataBoundItem is NhaCungCapDto item)
            {
                if (!item.IsActive)
                {
                    // Chuyển thành màu xám trên danh sách khi đã ẩn
                    e.CellStyle.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
                    e.CellStyle.ForeColor = System.Drawing.Color.FromArgb(140, 140, 140);
                    e.CellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(220, 224, 230);
                    e.CellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(90, 90, 90);
                }
            }
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

        private async void btnThem_Click(object sender, EventArgs e)
        {
            using (var dialog = new NhaCungCapDialog())
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        var result = await _nccService.CreateAsync(dialog.NhaCungCap);
                        if (result.Success)
                        {
                            MessageBox.Show(result.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadData();
                        }
                        else
                        {
                            MessageBox.Show(result.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi thêm nhà cung cấp: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void btnSua_Click(object sender, EventArgs e)
        {
            var selected = GetSelected();
            if (selected == null)
            {
                MessageBox.Show("Vui lòng chọn một nhà cung cấp để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dialog = new NhaCungCapDialog(selected))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        var result = await _nccService.UpdateAsync(selected.MaNhaCungCap, dialog.NhaCungCap);
                        if (result.Success)
                        {
                            MessageBox.Show(result.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadData();
                        }
                        else
                        {
                            MessageBox.Show(result.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi cập nhật nhà cung cấp: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void btnXoa_Click(object sender, EventArgs e)
        {
            var selected = GetSelected();
            if (selected == null)
            {
                MessageBox.Show("Vui lòng chọn một nhà cung cấp để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Xác nhận bước 1
            var confirm1 = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa nhà cung cấp \"{selected.TenNhaCungCap}\" ({selected.MaNhaCungCap})?\n\nLưu ý: Nếu nhà cung cấp đã có lịch sử nhập hàng, hệ thống sẽ tự động chuyển sang trạng thái 'Ẩn' để bảo toàn dữ liệu nhập kho.",
                "Xác nhận xóa nhà cung cấp (Bước 1/2)", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm1 != DialogResult.Yes) return;

            // Xác nhận bước 2 (Double confirmation)
            string msgHard2 = $"⚠️ XÁC NHẬN LẦN CUỐI:\n"
                + $"Nếu nhà cung cấp \"{selected.TenNhaCungCap}\" chưa từng phát sinh phiếu nhập hàng, hệ thống sẽ XÓA VĨNH VIỄN khỏi cơ sở dữ liệu và KHÔNG THỂ HOÀN TÁC!\n\n"
                + "Bạn có CHẮC CHẮN muốn xóa vĩnh viễn không?";
            if (MessageBox.Show(msgHard2, "Cảnh báo xóa vĩnh viễn (Bước 2/2)", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                var result = await _nccService.DeleteAsync(selected.MaNhaCungCap);
                if (result.Success)
                {
                    MessageBox.Show(result.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadData();
                }
                else
                {
                    MessageBox.Show(result.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private NhaCungCapDto GetSelected()
        {
            if (dgvNhaCungCap.SelectedRows.Count > 0)
            {
                return dgvNhaCungCap.SelectedRows[0].DataBoundItem as NhaCungCapDto;
            }
            return null;
        }

        private void dgvNhaCungCap_DoubleClick(object sender, EventArgs e)
        {
            btnSua_Click(sender, e);
        }

        private void dgvNhaCungCap_SelectionChanged(object sender, EventArgs e)
        {
            var selected = GetSelected();
            bool has = selected != null;
            btnSua.Enabled = has;
            btnXoa.Enabled = has;
            btnAnHien.Enabled = has;
            if (has)
            {
                btnAnHien.Text = selected.IsActive ? "👁 Ẩn NCC" : "👁 Hiện NCC";
            }
            else
            {
                btnAnHien.Text = "Ẩn/Hiện NCC";
            }
        }

        private async void btnAnHien_Click(object sender, EventArgs e)
        {
            var selected = GetSelected();
            if (selected == null)
            {
                MessageBox.Show("Vui lòng chọn một nhà cung cấp để ẩn hoặc hiện.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string actionText = selected.IsActive ? "ẩn" : "hiển thị lại";
            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn {actionText} nhà cung cấp \"{selected.TenNhaCungCap}\" ({selected.MaNhaCungCap})?",
                $"Xác nhận {actionText} nhà cung cấp", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                btnAnHien.Enabled = false;
                try
                {
                    var result = await _nccService.ToggleStatusAsync(selected.MaNhaCungCap);
                    if (result.Success)
                    {
                        MessageBox.Show(result.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadData();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi cập nhật trạng thái: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnAnHien.Enabled = true;
                }
            }
        }
    }
}
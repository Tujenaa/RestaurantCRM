using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.HoaDon {
    public partial class HoaDonForm : Form {
        private readonly HoaDonService _hoaDonService;
        private List<HoaDonDto> _allHoaDon = new List<HoaDonDto>();

        public HoaDonForm() {
            InitializeComponent();
            _hoaDonService = new HoaDonService();
            SetupGridColumns();
        }

        private void SetupGridColumns() {
            dgvHoaDon.AutoGenerateColumns = false;
            dgvHoaDon.Columns.Clear();

            dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn {
                DataPropertyName = "MaHoaDon",
                HeaderText = "Mã Đơn",
                Width = 120
            });

            dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn {
                DataPropertyName = "NgayDatHienThi",
                HeaderText = "Thời Gian Đặt",
                Width = 140
            });

            dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn {
                DataPropertyName = "TenKhachHang",
                HeaderText = "Khách Hàng",
                Width = 180
            });

            dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn {
                DataPropertyName = "TenNhanVien",
                HeaderText = "Nhân Viên Phụ Trách",
                Width = 150
            });

            var colTongTien = new DataGridViewTextBoxColumn {
                DataPropertyName = "TongTienHienThi",
                HeaderText = "Tổng Thanh Toán",
                Width = 140,
                DefaultCellStyle = new DataGridViewCellStyle {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(29, 78, 216)
                }
            };
            dgvHoaDon.Columns.Add(colTongTien);

            var colTrangThai = new DataGridViewTextBoxColumn {
                DataPropertyName = "TrangThai",
                HeaderText = "Trạng Thái",
                Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                }
            };
            dgvHoaDon.Columns.Add(colTrangThai);
        }

        private async void HoaDonForm_Load(object sender, EventArgs e) {
            cboTrangThai.Items.Clear();
            cboTrangThai.Items.AddRange(new object[] {
                "Tất cả trạng thái",
                "Chờ xử lý (Pending)",
                "Đang chuẩn bị (Preparing)",
                "Đang giao (Delivering)",
                "Hoàn thành (Completed)",
                "Đã hủy (Cancelled)"
            });
            cboTrangThai.SelectedIndex = 0;

            await LoadData();
        }

        private async Task LoadData() {
            try {
                _allHoaDon = await _hoaDonService.GetAllAsync();
                UpdateThongKe();
                FilterData();
            } catch (Exception ex) {
                MessageBox.Show("Lỗi khi tải dữ liệu đơn hàng: " + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateThongKe() {
            int tongDon = _allHoaDon.Count;
            int choXuLy = _allHoaDon.Count(x => IsPending(x.TrangThai));
            int dangGiao = _allHoaDon.Count(x => IsDelivering(x.TrangThai));
            int hoanThanh = _allHoaDon.Count(x => IsCompleted(x.TrangThai));
            int daHuy = _allHoaDon.Count(x => IsCancelled(x.TrangThai));

            lblTongDon.Text = $"Tổng số: {tongDon} đơn";
            lblChoXuLy.Text = $"Chờ xử lý: {choXuLy} đơn";
            lblDangGiao.Text = $"Đang giao: {dangGiao} đơn";
            lblHoanThanh.Text = $"Hoàn thành: {hoanThanh} đơn";
            lblDaHuy.Text = $"Đã hủy: {daHuy} đơn";
        }

        private bool IsPending(string st) {
            if (string.IsNullOrEmpty(st)) return false;
            var s = st.ToLower();
            return s.Contains("pending") || s.Contains("chờ") || s.Contains("preparing") || s.Contains("chuẩn bị") || s.Contains("processing");
        }

        private bool IsDelivering(string st) {
            if (string.IsNullOrEmpty(st)) return false;
            var s = st.ToLower();
            return s.Contains("delivering") || s.Contains("shipping") || s.Contains("đang giao");
        }

        private bool IsCompleted(string st) {
            if (string.IsNullOrEmpty(st)) return false;
            var s = st.ToLower();
            return s.Contains("completed") || s.Contains("hoàn thành");
        }

        private bool IsCancelled(string st) {
            if (string.IsNullOrEmpty(st)) return false;
            var s = st.ToLower();
            return s.Contains("cancelled") || s.Contains("hủy");
        }

        private void FilterData() {
            var keyword = (txtTimKiem.Text ?? "").Trim().ToLower();
            int filterIndex = cboTrangThai.SelectedIndex;

            var filtered = _allHoaDon.Where(item => {
                bool matchKeyword = string.IsNullOrEmpty(keyword) ||
                    (!string.IsNullOrEmpty(item.MaHoaDon) && item.MaHoaDon.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(item.TenKhachHang) && item.TenKhachHang.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(item.TenNhanVien) && item.TenNhanVien.ToLower().Contains(keyword));

                if (!matchKeyword) return false;

                switch (filterIndex) {
                    case 1: // Chờ xử lý / Đang chuẩn bị
                        return IsPending(item.TrangThai);
                    case 2: // Đang chuẩn bị
                        return !string.IsNullOrEmpty(item.TrangThai) && (item.TrangThai.ToLower().Contains("preparing") || item.TrangThai.ToLower().Contains("chuẩn bị"));
                    case 3: // Đang giao
                        return IsDelivering(item.TrangThai);
                    case 4: // Hoàn thành
                        return IsCompleted(item.TrangThai);
                    case 5: // Đã hủy
                        return IsCancelled(item.TrangThai);
                    default:
                        return true;
                }
            }).ToList();

            dgvHoaDon.DataSource = filtered;
            ColorGridRows();
        }

        private void ColorGridRows() {
            foreach (DataGridViewRow row in dgvHoaDon.Rows) {
                var item = row.DataBoundItem as HoaDonDto;
                if (item != null) {
                    if (IsCompleted(item.TrangThai)) {
                        row.Cells[5].Style.ForeColor = Color.FromArgb(22, 163, 74);
                    } else if (IsCancelled(item.TrangThai)) {
                        row.Cells[5].Style.ForeColor = Color.FromArgb(225, 29, 72);
                    } else if (IsDelivering(item.TrangThai)) {
                        row.Cells[5].Style.ForeColor = Color.FromArgb(2, 132, 199);
                    } else {
                        row.Cells[5].Style.ForeColor = Color.FromArgb(217, 119, 6);
                    }
                }
            }
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e) {
            FilterData();
        }

        private void cboTrangThai_SelectedIndexChanged(object sender, EventArgs e) {
            FilterData();
        }

        private async void btnLamMoi_Click(object sender, EventArgs e) {
            await LoadData();
        }

        private void btnChiTiet_Click(object sender, EventArgs e) {
            OpenChiTietDialog();
        }

        private void dgvHoaDon_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            if (e.RowIndex >= 0) {
                OpenChiTietDialog();
            }
        }

        private async void OpenChiTietDialog() {
            if (dgvHoaDon.SelectedRows.Count == 0) {
                MessageBox.Show("Vui lòng chọn một đơn hàng từ danh sách để xem chi tiết.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selected = dgvHoaDon.SelectedRows[0].DataBoundItem as HoaDonDto;
            if (selected == null) return;

            using (var dialog = new HoaDonDialog(selected.MaHoaDon)) {
                if (dialog.ShowDialog(this) == DialogResult.OK) {
                    await LoadData();
                }
            }
        }
    }
}

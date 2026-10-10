using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.HoaDon {
    public partial class HoaDonDialog : Form {
        private readonly HoaDonService _hoaDonService;
        private readonly string _maHoaDon;
        private HoaDonDetailDto _detail;

        public HoaDonDialog(string maHoaDon) {
            InitializeComponent();
            _hoaDonService = new HoaDonService();
            _maHoaDon = maHoaDon;

            SetupGridColumns();
        }

        private void SetupGridColumns() {
            // Cột cho dgvChiTiet
            dgvChiTiet.AutoGenerateColumns = false;
            dgvChiTiet.Columns.Clear();

            dgvChiTiet.Columns.Add(new DataGridViewTextBoxColumn {
                DataPropertyName = "MaMon",
                HeaderText = "Mã Món",
                Width = 110
            });

            dgvChiTiet.Columns.Add(new DataGridViewTextBoxColumn {
                DataPropertyName = "TenMon",
                HeaderText = "Tên Món Ăn",
                Width = 260
            });

            dgvChiTiet.Columns.Add(new DataGridViewTextBoxColumn {
                DataPropertyName = "SoLuong",
                HeaderText = "Số Lượng",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            dgvChiTiet.Columns.Add(new DataGridViewTextBoxColumn {
                DataPropertyName = "DonGiaHienThi",
                HeaderText = "Đơn Giá",
                Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvChiTiet.Columns.Add(new DataGridViewTextBoxColumn {
                DataPropertyName = "ThanhTienHienThi",
                HeaderText = "Thành Tiền",
                Width = 140,
                DefaultCellStyle = new DataGridViewCellStyle {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(29, 78, 216)
                }
            });

            // Cột cho dgvLichSu
            dgvLichSu.AutoGenerateColumns = false;
            dgvLichSu.Columns.Clear();

            dgvLichSu.Columns.Add(new DataGridViewTextBoxColumn {
                DataPropertyName = "ThoiGianHienThi",
                HeaderText = "Thời Gian Ghi Nhận",
                Width = 160
            });

            dgvLichSu.Columns.Add(new DataGridViewTextBoxColumn {
                DataPropertyName = "TrangThai",
                HeaderText = "Trạng Thái",
                Width = 160,
                DefaultCellStyle = new DataGridViewCellStyle {
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                }
            });

            dgvLichSu.Columns.Add(new DataGridViewTextBoxColumn {
                DataPropertyName = "GhiChu",
                HeaderText = "Ghi Chú & Chi Tiết Xử Lý",
                Width = 450
            });
        }

        private async void HoaDonDialog_Load(object sender, EventArgs e) {
            cboCapNhatTrangThai.Items.Clear();
            cboCapNhatTrangThai.Items.AddRange(new object[] {
                "Pending",
                "Preparing",
                "Delivering",
                "Completed",
                "Cancelled"
            });

            await LoadData();
        }

        private async Task LoadData() {
            try {
                _detail = await _hoaDonService.GetByIdAsync(_maHoaDon);
                if (_detail == null) {
                    MessageBox.Show("Không tìm thấy thông tin đơn hàng này trên máy chủ.", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                    return;
                }

                // Header
                lblTitle.Text = $"Chi Tiết Đơn Hàng #{_detail.MaHoaDon}";
                lblStatusBadge.Text = _detail.TrangThai ?? "N/A";
                UpdateBadgeColor(_detail.TrangThai);

                // Khách hàng & địa chỉ
                var khTen = _detail.KhachHang?.HoTen ?? "Khách vãng lai";
                var khSdt = _detail.KhachHang?.SoDienThoai;
                lblKhachHang.Text = string.IsNullOrEmpty(khSdt) 
                    ? $"Khách hàng: {khTen}" 
                    : $"Khách hàng: {khTen} ({khSdt})";

                lblDiaChi.Text = string.IsNullOrWhiteSpace(_detail.DiaChiGiao)
                    ? "Địa chỉ: Dùng tại nhà hàng"
                    : $"Địa chỉ giao: {_detail.DiaChiGiao}";

                lblNgayDat.Text = $"Thời gian đặt: {_detail.NgayDat?.ToString("dd/MM/yyyy HH:mm") ?? "-"}";

                lblNhanVien.Text = string.IsNullOrEmpty(_detail.NhanVienPhuTrach)
                    ? "Nhân viên: Chưa phân công"
                    : $"Nhân viên: {_detail.NhanVienPhuTrach}";

                lblKhuyenMai.Text = string.IsNullOrEmpty(_detail.KhuyenMai)
                    ? "Khuyến mãi: Không áp dụng"
                    : $"Khuyến mãi: {_detail.KhuyenMai}";

                // Tiền
                lblTienHang.Text = $"Tiền món ăn: {(_detail.TongTienHang ?? 0):N0} đ";
                lblTienGiam.Text = $"Giảm voucher: -{(_detail.TienGiamVoucher ?? 0):N0} đ";
                lblTongThanhToan.Text = $"Tổng thanh toán: {(_detail.TongThanhToan ?? 0):N0} đ";

                // Tab Món ăn
                dgvChiTiet.DataSource = _detail.ChiTiet;

                // Tab Lịch sử trạng thái
                var lichSuList = await _hoaDonService.GetLichSuTrangThaiAsync(_maHoaDon);
                dgvLichSu.DataSource = lichSuList;

                // Chọn trạng thái tương ứng trong combo box
                SelectComboStatus(_detail.TrangThai);

            } catch (Exception ex) {
                MessageBox.Show("Lỗi khi tải chi tiết đơn hàng: " + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectComboStatus(string currentStatus) {
            if (string.IsNullOrEmpty(currentStatus)) return;
            for (int i = 0; i < cboCapNhatTrangThai.Items.Count; i++) {
                if (cboCapNhatTrangThai.Items[i].ToString().Equals(currentStatus, StringComparison.OrdinalIgnoreCase)) {
                    cboCapNhatTrangThai.SelectedIndex = i;
                    return;
                }
            }
            // Nếu trạng thái dạng tiếng Việt, ánh xạ sang mã tương ứng
            var s = currentStatus.ToLower();
            if (s.Contains("chờ") || s.Contains("pending")) cboCapNhatTrangThai.SelectedItem = "Pending";
            else if (s.Contains("chuẩn bị") || s.Contains("preparing")) cboCapNhatTrangThai.SelectedItem = "Preparing";
            else if (s.Contains("giao") || s.Contains("delivering")) cboCapNhatTrangThai.SelectedItem = "Delivering";
            else if (s.Contains("hoàn thành") || s.Contains("completed")) cboCapNhatTrangThai.SelectedItem = "Completed";
            else if (s.Contains("hủy") || s.Contains("cancelled")) cboCapNhatTrangThai.SelectedItem = "Cancelled";
        }

        private void UpdateBadgeColor(string status) {
            if (string.IsNullOrEmpty(status)) return;
            var s = status.ToLower();
            if (s.Contains("completed") || s.Contains("hoàn thành")) {
                lblStatusBadge.BackColor = Color.FromArgb(220, 252, 231);
                lblStatusBadge.ForeColor = Color.FromArgb(22, 163, 74);
            } else if (s.Contains("cancelled") || s.Contains("hủy")) {
                lblStatusBadge.BackColor = Color.FromArgb(255, 228, 230);
                lblStatusBadge.ForeColor = Color.FromArgb(225, 29, 72);
            } else if (s.Contains("delivering") || s.Contains("giao")) {
                lblStatusBadge.BackColor = Color.FromArgb(224, 242, 254);
                lblStatusBadge.ForeColor = Color.FromArgb(2, 132, 199);
            } else {
                lblStatusBadge.BackColor = Color.FromArgb(254, 243, 199);
                lblStatusBadge.ForeColor = Color.FromArgb(217, 119, 6);
            }
        }

        private async void btnCapNhat_Click(object sender, EventArgs e) {
            if (cboCapNhatTrangThai.SelectedItem == null) {
                MessageBox.Show("Vui lòng chọn trạng thái mới cần cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var newStatus = cboCapNhatTrangThai.SelectedItem.ToString();
            var note = txtGhiChu.Text.Trim();

            if (_detail != null && _detail.TrangThai != null && _detail.TrangThai.Equals(newStatus, StringComparison.OrdinalIgnoreCase)) {
                var confirm = MessageBox.Show($"Trạng thái hiện tại đã là '{newStatus}'. Bạn có muốn cập nhật lại và ghi thêm log ghi chú không?",
                    "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;
            } else {
                var confirm = MessageBox.Show($"Bạn có chắc chắn muốn chuyển trạng thái đơn hàng #{_maHoaDon} sang '{newStatus}' không?",
                    "Xác nhận thay đổi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;
            }

            btnCapNhat.Enabled = false;
            try {
                var (success, message) = await _hoaDonService.UpdateTrangThaiAsync(_maHoaDon, newStatus, note);
                if (success) {
                    MessageBox.Show("Cập nhật trạng thái đơn hàng thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                } else {
                    MessageBox.Show(message, "Lỗi cập nhật", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } finally {
                btnCapNhat.Enabled = true;
            }
        }

        private void btnDong_Click(object sender, EventArgs e) {
            this.Close();
        }
    }
}

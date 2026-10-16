using System;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Forms.NhaCungCap {
    public partial class NhaCungCapDialog : Form {
        public NhaCungCapDto NhaCungCap { get; private set; }
        private readonly bool _isEdit;

        public NhaCungCapDialog(NhaCungCapDto ncc = null) {
            InitializeComponent();
            _isEdit = ncc != null;
            NhaCungCap = ncc != null 
                ? new NhaCungCapDto {
                    MaNhaCungCap = ncc.MaNhaCungCap,
                    TenNhaCungCap = ncc.TenNhaCungCap,
                    SoDienThoai = ncc.SoDienThoai,
                    Email = ncc.Email,
                    DiaChi = ncc.DiaChi
                } 
                : new NhaCungCapDto();

            if (_isEdit) {
                this.Text = "Sửa Nhà Cung Cấp";
                lblTieuDe.Text = "Cập Nhật Nhà Cung Cấp";
                txtMaNCC.Text = NhaCungCap.MaNhaCungCap;
                txtTenNCC.Text = NhaCungCap.TenNhaCungCap;
                txtSoDienThoai.Text = NhaCungCap.SoDienThoai;
                txtEmail.Text = NhaCungCap.Email;
                txtDiaChi.Text = NhaCungCap.DiaChi;
            } else {
                this.Text = "Thêm Nhà Cung Cấp";
                lblTieuDe.Text = "Thêm Nhà Cung Cấp Mới";
                lblMaNCC.Visible = false;
                txtMaNCC.Visible = false;
                // Shift other fields up
                lblTenNCC.Top -= 35;
                txtTenNCC.Top -= 35;
                lblSoDienThoai.Top -= 35;
                txtSoDienThoai.Top -= 35;
                lblEmail.Top -= 35;
                txtEmail.Top -= 35;
                lblDiaChi.Top -= 35;
                txtDiaChi.Top -= 35;
                btnLuu.Top -= 35;
                btnHuy.Top -= 35;
                this.ClientSize = new System.Drawing.Size(this.ClientSize.Width, this.ClientSize.Height - 35);
            }
        }

        private void btnLuu_Click(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(txtTenNCC.Text)) {
                MessageBox.Show("Vui lòng nhập tên nhà cung cấp.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenNCC.Focus();
                return;
            }

            NhaCungCap.TenNhaCungCap = txtTenNCC.Text.Trim();
            NhaCungCap.SoDienThoai = txtSoDienThoai.Text.Trim();
            NhaCungCap.Email = txtEmail.Text.Trim();
            NhaCungCap.DiaChi = txtDiaChi.Text.Trim();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnHuy_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

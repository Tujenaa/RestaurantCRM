using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.Kho {
    public partial class NguyenLieuDialog : Form {
        private readonly NguyenLieuService _service;
        private readonly NguyenLieuDto _editingItem;
        private readonly bool _isEditMode;

        public NguyenLieuDto CreatedOrUpdatedNguyenLieu { get; private set; }

        public NguyenLieuDialog(NguyenLieuService service, string prefilledName = "") {
            InitializeComponent();
            _service = service;
            _isEditMode = false;
            if (!string.IsNullOrWhiteSpace(prefilledName)) {
                txtTenNL.Text = prefilledName.Trim();
            }
        }

        public NguyenLieuDialog(NguyenLieuService service, NguyenLieuDto item) {
            InitializeComponent();
            _service = service;
            _editingItem = item;
            _isEditMode = item != null;
        }

        private void NguyenLieuDialog_Load(object sender, EventArgs e) {
            if (_isEditMode && _editingItem != null) {
                lblTieuDe.Text = "Cập Nhật Nguyên Vật Liệu";
                this.Text = "Sửa Thông Tin Nguyên Vật Liệu";
                txtMaNL.Text = _editingItem.MaNguyenLieu ?? "";
                txtTenNL.Text = _editingItem.TenNguyenLieu ?? "";
                cboDVT.Text = _editingItem.DonViTinh ?? "";
                numGiaNhap.Value = Math.Min(numGiaNhap.Maximum, Math.Max(0, _editingItem.GiaNhapGanNhat));
                numTonKho.Value = Math.Min(numTonKho.Maximum, Math.Max(0, _editingItem.SoLuongTon));
                numTonKho.Enabled = false; // Tồn kho chỉ nên điều chỉnh qua phiếu nhập hoặc kiểm kê

                if (string.Equals(_editingItem.TrangThai, "Inactive", StringComparison.OrdinalIgnoreCase)) {
                    cboTrangThai.SelectedIndex = 1;
                } else {
                    cboTrangThai.SelectedIndex = 0;
                }
            } else {
                lblTieuDe.Text = "Thêm Nguyên Vật Liệu Mới";
                this.Text = "Thêm Nguyên Liệu Mới";
                txtMaNL.Text = "(Tự động sinh: NLxx)";
                cboTrangThai.SelectedIndex = 0;
                if (cboDVT.Items.Count > 0 && string.IsNullOrEmpty(cboDVT.Text)) {
                    cboDVT.SelectedIndex = 0; // "kg"
                }
            }

            txtTenNL.Focus();
        }

        private async void btnLuu_Click(object sender, EventArgs e) {
            string tenNL = txtTenNL.Text.Trim();
            string dvt = cboDVT.Text.Trim();

            if (string.IsNullOrWhiteSpace(tenNL)) {
                MessageBox.Show("Vui lòng nhập tên nguyên vật liệu.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenNL.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(dvt)) {
                MessageBox.Show("Vui lòng chọn hoặc nhập đơn vị tính (ví dụ: kg, g, lít, quả, hộp...).", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboDVT.Focus();
                return;
            }

            btnLuu.Enabled = false;
            btnLuu.Text = "Đang lưu...";

            try {
                string trangThai = (cboTrangThai.SelectedIndex == 1) ? "Inactive" : "Active";
                decimal giaNhap = numGiaNhap.Value;
                decimal tonKho = numTonKho.Value;

                if (_isEditMode && _editingItem != null) {
                    var updateDto = new NguyenLieuDto {
                        MaNguyenLieu = _editingItem.MaNguyenLieu,
                        TenNguyenLieu = tenNL,
                        DonViTinh = dvt,
                        GiaNhapGanNhat = giaNhap,
                        SoLuongTon = _editingItem.SoLuongTon,
                        TrangThai = trangThai
                    };

                    var result = await _service.UpdateAsync(_editingItem.MaNguyenLieu, updateDto);
                    if (result.Success) {
                        CreatedOrUpdatedNguyenLieu = updateDto;
                        MessageBox.Show(result.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DialogResult = DialogResult.OK;
                        Close();
                    } else {
                        MessageBox.Show(result.Message, "Lỗi cập nhật", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                } else {
                    var newDto = new NguyenLieuDto {
                        TenNguyenLieu = tenNL,
                        DonViTinh = dvt,
                        GiaNhapGanNhat = giaNhap,
                        SoLuongTon = tonKho,
                        TrangThai = trangThai
                    };

                    var result = await _service.CreateAsync(newDto);
                    if (result.Success) {
                        CreatedOrUpdatedNguyenLieu = result.CreatedItem ?? newDto;
                        MessageBox.Show($"Thêm nguyên vật liệu '{tenNL}' thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DialogResult = DialogResult.OK;
                        Close();
                    } else {
                        MessageBox.Show(result.Message, "Lỗi thêm mới", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } finally {
                btnLuu.Enabled = true;
                btnLuu.Text = "Lưu lại";
            }
        }

        private void btnHuy_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

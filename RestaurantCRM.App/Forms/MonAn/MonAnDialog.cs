using System;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.MonAn {
    public partial class MonAnDialog : Form {
        private readonly MonAnService _monAnService;
        private readonly LoaiMonService _loaiMonService;
        private readonly MonAnDto _monAnCurrent;
        private bool _isEdit = false;

        public MonAnDialog(MonAnDto monAn = null) {
            InitializeComponent();
            var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5275/") };
            _monAnService = new MonAnService(httpClient);
            _loaiMonService = new LoaiMonService(httpClient);
            
            _monAnCurrent = monAn;
            _isEdit = monAn != null;
        }

        private async void MonAnDialog_Load(object sender, EventArgs e) {
            var loaiMonList = await _loaiMonService.GetAllAsync();
            cboLoaiMon.DataSource = loaiMonList;
            cboLoaiMon.DisplayMember = "TenLoaiMon";
            cboLoaiMon.ValueMember = "MaLoaiMon";

            if (_isEdit) {
                this.Text = "Cập nhật Món Ăn";
                txtTenMon.Text = _monAnCurrent.TenMonAn;
                numGia.Value = _monAnCurrent.Gia;
                cboLoaiMon.SelectedValue = _monAnCurrent.MaLoaiMon;
                chkTrangThai.Checked = _monAnCurrent.TrangThai;
                txtHinhAnhUrl.Text = _monAnCurrent.HinhAnhUrl;
                try {
                    if (!string.IsNullOrEmpty(_monAnCurrent.HinhAnhUrl)) {
                        picHinhAnh.ImageLocation = _monAnCurrent.HinhAnhUrl;
                    }
                } catch { }
            } else {
                this.Text = "Thêm Món Ăn Mới";
            }
        }

        private void btnChonHinh_Click(object sender, EventArgs e) {
            using (var ofd = new OpenFileDialog()) {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK) {
                    txtHinhAnhUrl.Text = ofd.FileName;
                    picHinhAnh.Image = Image.FromFile(ofd.FileName);
                }
            }
        }

        private async void btnLuu_Click(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(txtTenMon.Text)) {
                MessageBox.Show("Vui lòng nhập tên món ăn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (cboLoaiMon.SelectedValue == null) {
                MessageBox.Show("Vui lòng chọn loại món.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var monAnSave = new MonAnDto {
                TenMonAn = txtTenMon.Text.Trim(),
                Gia = numGia.Value,
                MaLoaiMon = cboLoaiMon.SelectedValue?.ToString(),
                TrangThai = chkTrangThai.Checked,
                HinhAnhUrl = txtHinhAnhUrl.Text.Trim() // Th?c t? backend s? x? l� upload file, form luu string (du?ng d?n ho?c base64). ? d�y ch? demo bind.
            };

            (bool Success, string Message) result;
            if (_isEdit) {
                monAnSave.MaMonAn = _monAnCurrent.MaMonAn;
                result = await _monAnService.UpdateAsync(monAnSave.MaMonAn, monAnSave);
            } else {
                result = await _monAnService.CreateAsync(monAnSave);
            }

            if (result.Success) {
                MessageBox.Show(result.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            } else {
                MessageBox.Show(result.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnHuy_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}




using System;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.Kho {
    public partial class DieuChinhTonKhoDialog : Form {
        private readonly KhoService _khoService;
        private readonly string _maMon;
        private readonly string _tenMon;
        private readonly int _soLuongHienTai;

        public DieuChinhTonKhoDialog(TonKhoDto item, KhoService khoService) {
            InitializeComponent();
            _khoService = khoService;
            _maMon = item.MaMon;
            _tenMon = item.TenMon;
            _soLuongHienTai = item.SoLuongTon;
        }

        public DieuChinhTonKhoDialog(MonAnDto monAn) {
            InitializeComponent();
            _maMon = monAn.MaMonAn;
            _tenMon = monAn.TenMonAn;
            _soLuongHienTai = monAn.SoLuongTon;
        }

        private void DieuChinhTonKhoDialog_Load(object sender, EventArgs e) {
            txtTenMon.Text = _tenMon ?? "";
            txtSoLuongHienTai.Text = _soLuongHienTai.ToString("N0");
            numSoLuongMoi.Value = Math.Max(0, _soLuongHienTai);
            numSoLuongMoi.Focus();
        }

        private async void btnLuu_Click(object sender, EventArgs e) {
            int soLuongMoi = (int)numSoLuongMoi.Value;
            string ghiChu = txtGhiChu.Text.Trim();

            if (_khoService != null) {
                var result = await _khoService.DieuChinhTonKhoAsync(_maMon, soLuongMoi, ghiChu);
                if (result.Success) {
                    MessageBox.Show(result.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                } else {
                    MessageBox.Show(result.Message, "Lỗi cập nhật", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } else {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnHuy_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

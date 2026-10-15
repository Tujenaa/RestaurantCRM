using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.NhaCungCap {
    public partial class PhieuNhapDialog : Form {
        private readonly PhieuNhapService _pnService;
        private readonly NhaCungCapService _nccService;
        private readonly NguyenLieuService _nguyenLieuService;

        private List<NguyenLieuDto> _allNguyenLieu = new List<NguyenLieuDto>();
        private List<ChiTietPhieuNhapDto> _items = new List<ChiTietPhieuNhapDto>();

        public PhieuNhapDialog() {
            InitializeComponent();
            var baseUrl = (ApiConfig.BaseUrl ?? "http://localhost:5275").TrimEnd('/') + "/";
            var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _pnService = new PhieuNhapService(httpClient);
            _nccService = new NhaCungCapService(httpClient);
            _nguyenLieuService = new NguyenLieuService(httpClient);
        }

        private async void PhieuNhapDialog_Load(object sender, EventArgs e) {
            await LoadDropdowns();
            UpdateGrid();
        }

        private async Task LoadDropdowns() {
            try {
                // 1. Tải nhà cung cấp
                var dsNCC = await _nccService.GetAllAsync();
                cboNhaCungCap.DataSource = dsNCC;
                cboNhaCungCap.DisplayMember = "TenNhaCungCap";
                cboNhaCungCap.ValueMember = "MaNhaCungCap";

                // 2. Tải danh mục nguyên vật liệu
                _allNguyenLieu = await _nguyenLieuService.GetAllAsync();
                cboMonAn.DisplayMember = "DisplayText";
                cboMonAn.ValueMember = "MaNguyenLieu";
                cboMonAn.DataSource = _allNguyenLieu;

                cboMonAn.SelectedIndexChanged += (s, e) => {
                    var selected = cboMonAn.SelectedItem as NguyenLieuDto;
                    if (selected != null && selected.GiaNhapGanNhat > 0) {
                        numDonGiaNhap.Value = Math.Min(numDonGiaNhap.Maximum, selected.GiaNhapGanNhat);
                    }
                };

                if (_allNguyenLieu.Any()) {
                    numDonGiaNhap.Value = Math.Min(numDonGiaNhap.Maximum, _allNguyenLieu[0].GiaNhapGanNhat);
                }
            } catch (Exception ex) {
                MessageBox.Show("Lỗi khi tải dữ liệu ban đầu: " + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnThemChiTiet_Click(object sender, EventArgs e) {
            var nl = cboMonAn.SelectedItem as NguyenLieuDto;
            if (nl == null) {
                MessageBox.Show("Vui lòng chọn nguyên vật liệu cần nhập.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal soLuong = numSoLuong.Value;
            decimal donGia = numDonGiaNhap.Value;

            if (soLuong <= 0) {
                MessageBox.Show("Số lượng nhập phải lớn hơn 0.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var existing = _items.FirstOrDefault(x => x.MaNguyenLieu == nl.MaNguyenLieu);
            if (existing != null) {
                existing.SoLuong += soLuong;
                existing.DonGiaNhap = donGia;
            } else {
                _items.Add(new ChiTietPhieuNhapDto {
                    MaNguyenLieu = nl.MaNguyenLieu,
                    TenNguyenLieu = nl.TenNguyenLieu,
                    DonViTinh = nl.DonViTinh,
                    SoLuong = soLuong,
                    DonGiaNhap = donGia
                });
            }

            UpdateGrid();
        }

        private void btnXoaChiTiet_Click(object sender, EventArgs e) {
            if (dgvChiTiet.SelectedRows.Count > 0) {
                var selected = dgvChiTiet.SelectedRows[0].DataBoundItem as ChiTietPhieuNhapDto;
                if (selected != null) {
                    _items.Remove(selected);
                    UpdateGrid();
                }
            } else {
                MessageBox.Show("Vui lòng chọn một mặt hàng trong danh sách bên dưới để bỏ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateGrid() {
            dgvChiTiet.DataSource = null;
            dgvChiTiet.DataSource = _items.ToList();

            decimal tongTien = _items.Sum(x => x.ThanhTien);
            lblTongTien.Text = $"TỔNG TIỀN PHIẾU NHẬP: {tongTien:N0} VNĐ";
        }

        private async void btnLuuPhieu_Click(object sender, EventArgs e) {
            if (cboNhaCungCap.SelectedValue == null) {
                MessageBox.Show("Vui lòng chọn nhà cung cấp.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboNhaCungCap.Focus();
                return;
            }

            if (_items.Count == 0) {
                MessageBox.Show("Phiếu nhập phải có ít nhất một nguyên vật liệu. Vui lòng thêm mặt hàng vào phiếu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dto = new PhieuNhapCreateDto {
                MaNhaCungCap = cboNhaCungCap.SelectedValue.ToString(),
                GhiChu = txtGhiChu.Text.Trim(),
                ChiTiet = _items.Select(x => new ChiTietPhieuNhapCreateDto {
                    MaNguyenLieu = x.MaNguyenLieu,
                    SoLuong = x.SoLuong,
                    DonGiaNhap = (double)x.DonGiaNhap
                }).ToList()
            };

            btnLuuPhieu.Enabled = false;
            btnLuuPhieu.Text = "⏳ Đang lưu & nhập kho...";

            try {
                var result = await _pnService.CreateAsync(dto);
                if (result.Success) {
                    MessageBox.Show(result.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                } else {
                    MessageBox.Show(result.Message, "Lỗi lập phiếu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } finally {
                btnLuuPhieu.Enabled = true;
                btnLuuPhieu.Text = "💾 Hoàn Tất & Nhập Kho";
            }
        }

        private void btnHuy_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

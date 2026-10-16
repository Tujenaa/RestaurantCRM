using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Config;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.MonAn {
    public partial class MonAnDialog : Form {
        private readonly MonAnService _monAnService;
        private readonly LoaiMonService _loaiMonService;
        private readonly NguyenLieuService _nguyenLieuService;
        private readonly MonAnDto _monAnCurrent;
        private readonly bool _isEdit = false;

        private readonly BindingList<DinhLuongItemDto> _dinhLuongList = new BindingList<DinhLuongItemDto>();
        private List<NguyenLieuDto> _allNguyenLieu = new List<NguyenLieuDto>();

        public MonAnDialog(MonAnDto monAn = null) {
            InitializeComponent();

            var baseUrl = (ApiConfig.BaseUrl ?? "http://localhost:5275").TrimEnd('/') + "/";
            var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _monAnService = new MonAnService(httpClient);
            _loaiMonService = new LoaiMonService(httpClient);
            _nguyenLieuService = new NguyenLieuService(httpClient);

            _monAnCurrent = monAn;
            _isEdit = monAn != null;

            this.DoubleBuffered = true;
            picHinhAnh.Paint += picHinhAnh_Paint;

            SetupDinhLuongGrid();
            numGia.ValueChanged += (s, e) => RecalculateCogs();
        }

        private void SetupDinhLuongGrid() {
            // Kích hoạt DoubleBuffering cho DataGridView định lượng
            typeof(Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.SetProperty,
                null, dgvDinhLuong, new object[] { true });

            dgvDinhLuong.AutoGenerateColumns = false;
            dgvDinhLuong.Columns.Clear();
            dgvDinhLuong.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDinhLuong.BackgroundColor = Color.White;
            dgvDinhLuong.BorderStyle = BorderStyle.None;
            dgvDinhLuong.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvDinhLuong.GridColor = Color.FromArgb(226, 232, 240);
            dgvDinhLuong.RowHeadersVisible = false;
            dgvDinhLuong.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDinhLuong.MultiSelect = false;

            // Header styling hiện đại
            dgvDinhLuong.EnableHeadersVisualStyles = false;
            dgvDinhLuong.ColumnHeadersHeight = 36;
            dgvDinhLuong.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvDinhLuong.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvDinhLuong.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvDinhLuong.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            dgvDinhLuong.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            dgvDinhLuong.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);

            // Row styling
            dgvDinhLuong.RowTemplate.Height = 34;
            dgvDinhLuong.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvDinhLuong.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            dgvDinhLuong.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 231, 255);
            dgvDinhLuong.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);

            var colTenNL = new DataGridViewTextBoxColumn {
                DataPropertyName = "TenNguyenLieu",
                HeaderText = "Nguyên Liệu",
                FillWeight = 145,
                MinimumWidth = 110,
                ReadOnly = true
            };
            dgvDinhLuong.Columns.Add(colTenNL);

            var colSoLuong = new DataGridViewTextBoxColumn {
                DataPropertyName = "SoLuongCan",
                HeaderText = "Định Lượng",
                FillWeight = 75,
                MinimumWidth = 70,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Format = "0.##",
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                }
            };
            dgvDinhLuong.Columns.Add(colSoLuong);

            var colDvt = new DataGridViewTextBoxColumn {
                DataPropertyName = "DonViTinh",
                HeaderText = "ĐVT",
                FillWeight = 45,
                MinimumWidth = 45,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            };
            dgvDinhLuong.Columns.Add(colDvt);

            var colGiaNhap = new DataGridViewTextBoxColumn {
                DataPropertyName = "GiaNhapGanNhat",
                HeaderText = "Giá Nhập",
                FillWeight = 85,
                MinimumWidth = 75,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Format = "N0"
                }
            };
            dgvDinhLuong.Columns.Add(colGiaNhap);

            var colThanhTien = new DataGridViewTextBoxColumn {
                DataPropertyName = "ThanhTienGiaVon",
                HeaderText = "Thành Tiền",
                FillWeight = 95,
                MinimumWidth = 85,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Format = "N0",
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(2, 132, 199)
                }
            };
            dgvDinhLuong.Columns.Add(colThanhTien);

            var colXoa = new DataGridViewButtonColumn {
                Name = "colXoa",
                HeaderText = "Xóa",
                Text = "✕",
                UseColumnTextForButtonValue = true,
                FillWeight = 40,
                MinimumWidth = 40,
                FlatStyle = FlatStyle.Flat
            };
            dgvDinhLuong.Columns.Add(colXoa);

            dgvDinhLuong.DataSource = _dinhLuongList;
            dgvDinhLuong.CellContentClick += dgvDinhLuong_CellContentClick;
            _dinhLuongList.ListChanged += (s, e) => RecalculateCogs();
        }

        private async void MonAnDialog_Load(object sender, EventArgs e) {
            try {
                // 1. Tải danh mục loại món
                var loaiMonList = await _loaiMonService.GetAllAsync() ?? new List<LoaiMonDto>();
                cboLoaiMon.DataSource = loaiMonList;
                cboLoaiMon.DisplayMember = "TenLoaiMon";
                cboLoaiMon.ValueMember = "MaLoaiMon";

                // 2. Tải danh sách nguyên vật liệu kho
                _allNguyenLieu = await _nguyenLieuService.GetAllAsync() ?? new List<NguyenLieuDto>();
                cboNguyenLieu.DataSource = _allNguyenLieu;
                cboNguyenLieu.DisplayMember = "DisplayText";
                cboNguyenLieu.ValueMember = "MaNguyenLieu";
                cboNguyenLieu.SelectedIndexChanged += cboNguyenLieu_SelectedIndexChanged;

                if (_allNguyenLieu.Any()) {
                    UpdateNguyenLieuInfo(_allNguyenLieu[0]);
                }

                // 3. Nếu là chế độ chỉnh sửa: Tải chi tiết món ăn và danh sách định lượng (BOM)
                if (_isEdit) {
                    this.Text = $"Cập nhật Món Ăn: {_monAnCurrent.TenMonAn} ({_monAnCurrent.MaMonAn})";
                    lblFormTitle.Text = $"🍲 Cập Nhật Món Ăn: {_monAnCurrent.TenMonAn}";
                    lblFormSub.Text = $"Mã món: {_monAnCurrent.MaMonAn} - Điều chỉnh thông tin chi tiết và định mức nguyên vật liệu tiêu hao";
                    txtTenMon.Text = _monAnCurrent.TenMonAn;
                    numGia.Value = _monAnCurrent.Gia;
                    cboLoaiMon.SelectedValue = _monAnCurrent.MaLoaiMon;
                    chkTrangThai.Checked = _monAnCurrent.TrangThai;
                    txtMoTa.Text = _monAnCurrent.MoTa ?? "";
                    txtHinhAnhUrl.Text = _monAnCurrent.HinhAnhUrl ?? "";

                    if (!string.IsNullOrEmpty(_monAnCurrent.HinhAnhUrl)) {
                        try {
                            if (File.Exists(_monAnCurrent.HinhAnhUrl)) {
                                picHinhAnh.Image = Image.FromFile(_monAnCurrent.HinhAnhUrl);
                            } else {
                                picHinhAnh.ImageLocation = _monAnCurrent.HinhAnhUrl;
                            }
                        } catch { }
                    }

                    // Tải chi tiết định lượng từ server
                    var detail = await _monAnService.GetByIdAsync(_monAnCurrent.MaMonAn);
                    if (detail != null && detail.DinhLuong != null && detail.DinhLuong.Any()) {
                        _dinhLuongList.Clear();
                        foreach (var dl in detail.DinhLuong) {
                            dl.ThanhTienGiaVon = dl.SoLuongCan * dl.GiaNhapGanNhat;
                            _dinhLuongList.Add(dl);
                        }
                    }
                } else {
                    this.Text = "Thêm Món Ăn Mới & Thiết Lập Công Thức Định Lượng (BOM)";
                    lblFormTitle.Text = "🍲 Thiết Lập Món Ăn Mới & Công Thức Định Lượng (BOM)";
                    lblFormSub.Text = "Khai báo thông tin chi tiết món ăn, giá bán và định mức nguyên vật liệu tiêu hao từ kho khi bán hàng.";
                }

                RecalculateCogs();
            } catch (Exception ex) {
                MessageBox.Show("Lỗi khởi tạo form: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cboNguyenLieu_SelectedIndexChanged(object sender, EventArgs e) {
            if (cboNguyenLieu.SelectedItem is NguyenLieuDto selectedNl) {
                UpdateNguyenLieuInfo(selectedNl);
            }
        }

        private void UpdateNguyenLieuInfo(NguyenLieuDto nl) {
            if (nl == null) return;
            lblDvtInfo.Text = nl.DonViTinh ?? "đơn vị";
            lblGiaNhapInfo.Text = $"Giá nhập: {nl.GiaNhapGanNhat:N0} đ/{nl.DonViTinh}";
        }

        private async void btnThemNguyenLieuMoi_Click(object sender, EventArgs e) {
            using (var dialog = new RestaurantCRM.AdminApp.Forms.Kho.NguyenLieuDialog(_nguyenLieuService)) {
                if (dialog.ShowDialog(this) == DialogResult.OK && dialog.CreatedOrUpdatedNguyenLieu != null) {
                    var newNl = dialog.CreatedOrUpdatedNguyenLieu;
                    _allNguyenLieu = await _nguyenLieuService.GetAllAsync() ?? new List<NguyenLieuDto>();
                    cboNguyenLieu.DataSource = null;
                    cboNguyenLieu.DisplayMember = "DisplayText";
                    cboNguyenLieu.ValueMember = "MaNguyenLieu";
                    cboNguyenLieu.DataSource = _allNguyenLieu;

                    var found = _allNguyenLieu.FirstOrDefault(x => x.MaNguyenLieu == newNl.MaNguyenLieu);
                    if (found != null) {
                        cboNguyenLieu.SelectedItem = found;
                        UpdateNguyenLieuInfo(found);
                    }
                    numSoLuongCan.Focus();
                }
            }
        }

        private void btnThemNL_Click(object sender, EventArgs e) {
            var selectedNl = cboNguyenLieu.SelectedItem as NguyenLieuDto;
            if (selectedNl == null) {
                MessageBox.Show("Vui lòng chọn một nguyên liệu hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (numSoLuongCan.Value <= 0) {
                MessageBox.Show("Số lượng định lượng tiêu hao phải lớn hơn 0.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var existing = _dinhLuongList.FirstOrDefault(x => x.MaNguyenLieu == selectedNl.MaNguyenLieu);
            if (existing != null) {
                existing.SoLuongCan += numSoLuongCan.Value;
                existing.ThanhTienGiaVon = existing.SoLuongCan * existing.GiaNhapGanNhat;
                _dinhLuongList.ResetBindings();
            } else {
                var newItem = new DinhLuongItemDto {
                    MaNguyenLieu = selectedNl.MaNguyenLieu,
                    TenNguyenLieu = selectedNl.TenNguyenLieu,
                    DonViTinh = selectedNl.DonViTinh,
                    SoLuongCan = numSoLuongCan.Value,
                    GiaNhapGanNhat = selectedNl.GiaNhapGanNhat,
                    ThanhTienGiaVon = numSoLuongCan.Value * selectedNl.GiaNhapGanNhat
                };
                _dinhLuongList.Add(newItem);
            }

            numSoLuongCan.Value = 1;
            RecalculateCogs();
        }

        private void dgvDinhLuong_CellContentClick(object sender, DataGridViewCellEventArgs e) {
            if (e.RowIndex < 0) return;
            if (dgvDinhLuong.Columns[e.ColumnIndex].Name == "colXoa") {
                if (e.RowIndex < _dinhLuongList.Count) {
                    var item = _dinhLuongList[e.RowIndex];
                    if (MessageBox.Show($"Bạn có muốn xóa nguyên liệu '{item.TenNguyenLieu}' khỏi công thức định lượng?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                        _dinhLuongList.RemoveAt(e.RowIndex);
                        RecalculateCogs();
                    }
                }
            }
        }

        private void RecalculateCogs() {
            decimal tongGiaVon = _dinhLuongList.Sum(x => x.ThanhTienGiaVon);
            lblGiaVonValue.Text = $"{tongGiaVon:N0} đ";

            decimal giaBan = numGia.Value;
            decimal loiNhuanGop = giaBan - tongGiaVon;
            decimal tiLeLoiNhuan = giaBan > 0 ? (loiNhuanGop / giaBan) * 100 : 0;

            lblLoiNhuanGopValue.Text = $"{loiNhuanGop:N0} đ ({tiLeLoiNhuan:F1}%)";

            if (giaBan > 0 && giaBan < tongGiaVon) {
                lblCanhBaoGia.Visible = true;
                lblCanhBaoGia.Text = $"⚠️ Cảnh báo: Giá bán ({giaBan:N0} đ) đang thấp hơn giá vốn ({tongGiaVon:N0} đ)!";
                lblLoiNhuanGopValue.ForeColor = Color.FromArgb(220, 38, 38);
            } else {
                lblCanhBaoGia.Visible = false;
                lblLoiNhuanGopValue.ForeColor = Color.FromArgb(5, 150, 105);
            }
        }

        private void picHinhAnh_Paint(object sender, PaintEventArgs e) {
            if (picHinhAnh.Image == null) {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(148, 163, 184))) {
                    var text = "🖼️ Chưa có hình ảnh món ăn\nNhấn 'Chọn ảnh' phía trên để tải lên";
                    using (var font = new Font("Segoe UI", 9.5F, FontStyle.Regular)) {
                        var sf = new StringFormat {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString(text, font, brush, picHinhAnh.ClientRectangle, sf);
                    }
                }
            }
        }

        private void btnChonHinh_Click(object sender, EventArgs e) {
            using (var ofd = new OpenFileDialog()) {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK) {
                    txtHinhAnhUrl.Text = ofd.FileName;
                    try {
                        picHinhAnh.Image = Image.FromFile(ofd.FileName);
                    } catch { }
                    picHinhAnh.Invalidate();
                }
            }
        }

        private async void btnLuu_Click(object sender, EventArgs e) {
            // 1. Kiểm tra thông tin cơ bản
            if (string.IsNullOrWhiteSpace(txtTenMon.Text)) {
                MessageBox.Show("Vui lòng nhập tên món ăn.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenMon.Focus();
                return;
            }

            if (cboLoaiMon.SelectedValue == null) {
                MessageBox.Show("Vui lòng chọn loại món ăn.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboLoaiMon.Focus();
                return;
            }

            if (numGia.Value <= 0) {
                MessageBox.Show("Đơn giá món ăn phải lớn hơn 0.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numGia.Focus();
                return;
            }

            // 2. RÀNG BUỘC NGHIỆP VỤ BẮT BUỘC: Món ăn phải có ít nhất 1 nguyên liệu định lượng
            if (_dinhLuongList.Count == 0) {
                MessageBox.Show("Món ăn bắt buộc phải có ít nhất 1 nguyên vật liệu trong công thức định lượng (BOM)!\nVui lòng chọn nguyên liệu và nhấn '+ Thêm vào công thức'.", "Thiếu định lượng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboNguyenLieu.Focus();
                return;
            }

            var monAnSave = new MonAnDto {
                TenMonAn = txtTenMon.Text.Trim(),
                Gia = numGia.Value,
                MaLoaiMon = cboLoaiMon.SelectedValue?.ToString(),
                MoTa = txtMoTa.Text.Trim(),
                TrangThaiRaw = chkTrangThai.Checked ? "InStock" : "Discontinued",
                HinhAnhUrl = txtHinhAnhUrl.Text.Trim(),
                DinhLuong = _dinhLuongList.ToList()
            };

            btnLuu.Enabled = false;
            btnLuu.Text = "⏳ Đang lưu...";

            try {
                (bool Success, string Message) result;
                if (_isEdit) {
                    monAnSave.MaMonAn = _monAnCurrent.MaMonAn;
                    result = await _monAnService.UpdateAsync(monAnSave.MaMonAn, monAnSave);
                } else {
                    result = await _monAnService.CreateAsync(monAnSave);
                }

                if (result.Success) {
                    MessageBox.Show(result.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                } else {
                    MessageBox.Show(result.Message, "Lỗi từ máy chủ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } finally {
                btnLuu.Enabled = true;
                btnLuu.Text = "💾 Lưu Món Ăn";
            }
        }

        private void btnHuy_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}

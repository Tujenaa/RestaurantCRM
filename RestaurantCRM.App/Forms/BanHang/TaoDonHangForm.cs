using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Forms.HoaDon;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.BanHang {
    public class TaoDonHangForm : Form {
        private sealed class CartItem {
            public string MaMon;
            public string Name;
            public decimal Price;
            public int Quantity;
        }

        private readonly MonAnService _monAnService;
        private readonly LoaiMonService _loaiMonService;
        private readonly HoaDonService _hoaDonService;

        private List<MonAnDto> _allMonAn = new List<MonAnDto>();
        private List<LoaiMonDto> _allLoaiMon = new List<LoaiMonDto>();
        private readonly List<CartItem> _cart = new List<CartItem>();

        private readonly FlowLayoutPanel _productsPanel;
        private readonly FlowLayoutPanel _cartLines;
        private readonly Label _total;
        private readonly TextBox _txtTenKhach;
        private readonly TextBox _txtSdt;
        private readonly TextBox _txtGhiChu;
        private readonly TextBox _txtSearch;
        private readonly ComboBox _cboCategory;
        private readonly Button _btnPay;
        private readonly Label _lblNhanVien;

        private readonly Color _accent = Color.FromArgb(14, 124, 134);

        public TaoDonHangForm() {
            _monAnService = new MonAnService();
            _loaiMonService = new LoaiMonService();
            _hoaDonService = new HoaDonService();

            Text = "Tạo đơn hàng mang đi tại quầy | RestaurantCRM";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1050, 700);
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);

            var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24), BackColor = ThemeManager.Bg };
            Controls.Add(root);

            root.Controls.Add(new Label {
                Text = "Tạo Đơn Hàng Bán Mang Đi / Tại Quầy",
                Location = new Point(24, 14),
                AutoSize = true,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            });

            string currentStaff = UserSession.CurrentDisplayName ?? UserSession.CurrentUsername ?? "Nhân viên";
            _lblNhanVien = new Label {
                Text = $"Nhân viên lập đơn: {currentStaff}",
                Location = new Point(26, 52),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(29, 78, 216)
            };
            root.Controls.Add(_lblNhanVien);

            var split = new SplitContainer {
                Location = new Point(24, 85),
                Size = new Size(ClientSize.Width - 48, ClientSize.Height - 110),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                SplitterDistance = (int)((ClientSize.Width - 48) * 0.62),
                BackColor = ThemeManager.Bg,
                FixedPanel = FixedPanel.Panel2
            };
            root.Controls.Add(split);

            // ================== PANEL TRÁI: THỰC ĐƠN ==================
            var menuPanel = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.Paper, Padding = new Padding(16) };
            split.Panel1.Controls.Add(menuPanel);

            var headerPanel = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = ThemeManager.Paper };
            menuPanel.Controls.Add(headerPanel);
            headerPanel.Controls.Add(new Label {
                Text = "Thực đơn món ăn",
                Location = new Point(0, 0),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            });

            _txtSearch = new TextBox { Location = new Point(0, 30), Width = 230, Font = Font };
            _txtSearch.Text = "Tìm món...";
            _txtSearch.ForeColor = ThemeManager.Muted;
            _txtSearch.GotFocus += (s, e) => { if (_txtSearch.Text == "Tìm món...") { _txtSearch.Text = ""; _txtSearch.ForeColor = ThemeManager.Ink; } };
            _txtSearch.LostFocus += (s, e) => { if (_txtSearch.Text.Length == 0) { _txtSearch.Text = "Tìm món..."; _txtSearch.ForeColor = ThemeManager.Muted; } };
            _txtSearch.TextChanged += (s, e) => RenderProducts();
            headerPanel.Controls.Add(_txtSearch);

            _cboCategory = new ComboBox { Location = new Point(245, 30), Width = 180, Font = Font, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboCategory.SelectedIndexChanged += (s, e) => RenderProducts();
            headerPanel.Controls.Add(_cboCategory);

            _productsPanel = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ThemeManager.Paper,
                Padding = new Padding(0, 10, 0, 0)
            };
            menuPanel.Controls.Add(_productsPanel);
            _productsPanel.BringToFront();

            // ================== PANEL PHẢI: GIỎ HÀNG & THÔNG TIN ==================
            var cartPanel = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.Paper, Padding = new Padding(16) };
            split.Panel2.Controls.Add(cartPanel);

            cartPanel.Controls.Add(new Label {
                Text = "Thông tin đơn mang đi",
                Location = new Point(14, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            });

            var lblTen = new Label { Text = "Tên khách hàng:", Location = new Point(14, 38), AutoSize = true, ForeColor = ThemeManager.Muted, Font = new Font("Segoe UI", 8.5F) };
            cartPanel.Controls.Add(lblTen);
            _txtTenKhach = new TextBox { Text = "Khách mua mang đi", Location = new Point(14, 56), Width = 160, Font = Font };
            cartPanel.Controls.Add(_txtTenKhach);

            var lblSdt = new Label { Text = "Số điện thoại:", Location = new Point(185, 38), AutoSize = true, ForeColor = ThemeManager.Muted, Font = new Font("Segoe UI", 8.5F) };
            cartPanel.Controls.Add(lblSdt);
            _txtSdt = new TextBox { Location = new Point(185, 56), Width = 150, Font = Font };
            cartPanel.Controls.Add(_txtSdt);

            var lblGhichu = new Label { Text = "Ghi chú đơn hàng:", Location = new Point(14, 85), AutoSize = true, ForeColor = ThemeManager.Muted, Font = new Font("Segoe UI", 8.5F) };
            cartPanel.Controls.Add(lblGhichu);
            _txtGhiChu = new TextBox { Text = "Bán mang đi tại quầy", Location = new Point(14, 103), Width = 320, Font = Font };
            cartPanel.Controls.Add(_txtGhiChu);

            var lblChiTietMon = new Label {
                Text = "Chi tiết món chọn:",
                Location = new Point(14, 135),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            };
            cartPanel.Controls.Add(lblChiTietMon);

            _cartLines = new FlowLayoutPanel {
                Location = new Point(12, 158),
                Size = new Size(330, 260),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = ThemeManager.Paper
            };
            cartPanel.Controls.Add(_cartLines);

            _total = new Label {
                Text = "Tổng cộng: 0 đ",
                Location = new Point(14, 435),
                AutoSize = true,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 38, 38),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            cartPanel.Controls.Add(_total);

            _btnPay = new Button {
                Text = "✔ Xác nhận & Tạo hóa đơn mang đi",
                Location = new Point(14, 470),
                Size = new Size(328, 42),
                BackColor = _accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            _btnPay.FlatAppearance.BorderSize = 0;
            _btnPay.Click += async (s, e) => await Checkout();
            cartPanel.Controls.Add(_btnPay);

            var clear = new Button {
                Text = "Xóa giỏ hàng",
                Location = new Point(14, 518),
                Size = new Size(328, 32),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = ThemeManager.Muted
            };
            clear.Click += (s, e) => { _cart.Clear(); RenderCart(); };
            cartPanel.Controls.Add(clear);

            this.Load += async (s, e) => await InitData();
        }

        private async Task InitData() {
            try {
                // Tải loại món
                _allLoaiMon = await _loaiMonService.GetAllAsync();
                _cboCategory.Items.Clear();
                _cboCategory.Items.Add("Tất cả loại món");
                foreach (var lm in _allLoaiMon) {
                    _cboCategory.Items.Add(lm.TenLoaiMon);
                }
                _cboCategory.SelectedIndex = 0;

                // Tải danh sách món ăn thực tế từ API
                var list = await _monAnService.GetAllAsync();
                // Lọc bỏ món đã ngừng kinh doanh
                _allMonAn = list.Where(m => m.TrangThaiRaw != "Discontinued" && m.TrangThaiRaw != "Inactive").ToList();
                RenderProducts();
            } catch (Exception ex) {
                MessageBox.Show(this, "Không thể tải dữ liệu món ăn: " + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RenderProducts() {
            _productsPanel.Controls.Clear();
            string query = (_txtSearch.Text == "Tìm món..." ? "" : _txtSearch.Text).Trim().ToLowerInvariant();
            string selectedCatName = _cboCategory.SelectedItem?.ToString() ?? "Tất cả loại món";

            string selectedCatId = "";
            if (selectedCatName != "Tất cả loại món") {
                var foundCat = _allLoaiMon.FirstOrDefault(c => c.TenLoaiMon == selectedCatName);
                if (foundCat != null) selectedCatId = foundCat.MaLoaiMon;
            }

            var filtered = _allMonAn.Where(m => {
                bool matchName = string.IsNullOrEmpty(query) || (m.TenMonAn != null && m.TenMonAn.ToLowerInvariant().Contains(query));
                bool matchCat = string.IsNullOrEmpty(selectedCatId) || m.MaLoaiMon == selectedCatId;
                return matchName && matchCat;
            }).ToList();

            if (!filtered.Any()) {
                _productsPanel.Controls.Add(new Label {
                    Text = "Không có món ăn nào phù hợp.",
                    ForeColor = ThemeManager.Muted,
                    AutoSize = true,
                    Margin = new Padding(12)
                });
                return;
            }

            foreach (var mon in filtered) {
                var btn = new Button {
                    Text = $"{mon.TenMonAn}\r\n{mon.Gia:N0} đ\r\n[+ Thêm]",
                    Size = new Size(170, 95),
                    Margin = new Padding(6),
                    BackColor = Color.White,
                    ForeColor = ThemeManager.Ink,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Tag = mon
                };
                btn.FlatAppearance.BorderColor = ThemeManager.Line;
                btn.Click += (s, e) => {
                    var m = (MonAnDto)((Button)s).Tag;
                    AddItem(m);
                };
                _productsPanel.Controls.Add(btn);
            }
        }

        private void AddItem(MonAnDto mon) {
            var item = _cart.FirstOrDefault(x => x.MaMon == mon.MaMonAn);
            if (item == null) {
                _cart.Add(new CartItem {
                    MaMon = mon.MaMonAn,
                    Name = mon.TenMonAn,
                    Price = mon.Gia,
                    Quantity = 1
                });
            } else {
                item.Quantity++;
            }
            RenderCart();
        }

        private void RenderCart() {
            _cartLines.Controls.Clear();
            foreach (var item in _cart) {
                var row = new Panel {
                    Size = new Size(310, 52),
                    BackColor = Color.FromArgb(248, 250, 252),
                    Margin = new Padding(2)
                };

                row.Controls.Add(new Label {
                    Text = item.Name,
                    Location = new Point(6, 4),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = ThemeManager.Ink
                });

                row.Controls.Add(new Label {
                    Text = $"{item.Quantity} × {item.Price:N0} đ = {(item.Quantity * item.Price):N0} đ",
                    Location = new Point(6, 26),
                    AutoSize = true,
                    ForeColor = ThemeManager.Muted
                });

                var btnMinus = new Button {
                    Text = "−",
                    Location = new Point(230, 10),
                    Size = new Size(32, 30),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = ThemeManager.Red
                };
                btnMinus.Click += (s, e) => {
                    if (item.Quantity > 1) item.Quantity--;
                    else _cart.Remove(item);
                    RenderCart();
                };
                row.Controls.Add(btnMinus);

                var btnPlus = new Button {
                    Text = "+",
                    Location = new Point(268, 10),
                    Size = new Size(32, 30),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.FromArgb(22, 163, 74)
                };
                btnPlus.Click += (s, e) => {
                    item.Quantity++;
                    RenderCart();
                };
                row.Controls.Add(btnPlus);

                _cartLines.Controls.Add(row);
            }

            decimal totalAmount = _cart.Sum(x => x.Price * x.Quantity);
            _total.Text = $"Tổng cộng: {totalAmount:N0} đ";
        }

        private async Task Checkout() {
            if (_cart.Count == 0) {
                MessageBox.Show(this, "Vui lòng chọn ít nhất một món ăn vào giỏ hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string staffId = UserSession.CurrentUserId ?? UserSession.CurrentUsername;
            string staffName = UserSession.CurrentDisplayName ?? UserSession.CurrentUsername ?? "Nhân viên";

            var req = new CreateOrderRequestDto {
                TenKhachHang = string.IsNullOrWhiteSpace(_txtTenKhach.Text) ? "Khách mua mang đi" : _txtTenKhach.Text.Trim(),
                SoDienThoai = _txtSdt.Text.Trim(),
                DiaChiGiao = string.IsNullOrWhiteSpace(_txtGhiChu.Text) ? "Bán mang đi tại quầy" : _txtGhiChu.Text.Trim(),
                PhuongThucThanhToan = "Tiền mặt (Tại quầy)",
                MaNhanVien = staffId,
                Items = _cart.Select(x => new CreateOrderItemDto {
                    MaMon = x.MaMon,
                    SoLuong = x.Quantity
                }).ToList()
            };

            _btnPay.Enabled = false;
            try {
                var (success, message, maHoaDon) = await _hoaDonService.CreateOrderAsync(req);
                if (success) {
                    var confirm = MessageBox.Show(this,
                        $"🎉 Tạo hóa đơn mang đi thành công!\n\n" +
                        $"Mã hóa đơn: #{maHoaDon}\n" +
                        $"Nhân viên lập đơn: {staffName}\n" +
                        $"Tổng tiền: {_cart.Sum(x => x.Price * x.Quantity):N0} đ\n\n" +
                        $"Bạn có muốn mở xem chi tiết hóa đơn vừa tạo không?",
                        "Tạo đơn thành công", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    _cart.Clear();
                    RenderCart();

                    if (confirm == DialogResult.Yes && !string.IsNullOrEmpty(maHoaDon)) {
                        using (var dialog = new HoaDonDialog(maHoaDon)) {
                            dialog.ShowDialog(this);
                        }
                    }
                    this.DialogResult = DialogResult.OK;
                } else {
                    MessageBox.Show(this, message, "Lỗi tạo hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } finally {
                _btnPay.Enabled = true;
            }
        }
    }
}

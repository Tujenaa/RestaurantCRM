using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.BanHang {
    public class TaoDonHangForm : Form {
        private sealed class CartItem { public string Name; public decimal Price; public int Quantity; }
        private readonly List<CartItem> _cart = new List<CartItem>();
        private readonly FlowLayoutPanel _cartLines;
        private readonly Label _total;
        private readonly Color _accent = Color.FromArgb(14, 124, 134);
        private readonly string[] _names = { "Phở bò", "Gỏi cuốn", "Cơm gà", "Trà đào", "Bún chả", "Nước suối" };
        private readonly decimal[] _prices = { 65000, 45000, 72000, 35000, 68000, 12000 };

        public TaoDonHangForm() {
            Text = "Tạo đơn hàng | RestaurantCRM"; StartPosition = FormStartPosition.CenterParent; Size = new Size(1120, 720); MinimumSize = new Size(920, 600); BackColor = ThemeManager.Bg; Font = new Font("Segoe UI", 9.5F);
            var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24), BackColor = ThemeManager.Bg }; Controls.Add(root);
            root.Controls.Add(new Label { Text = "Tạo đơn hàng", Location = new Point(24, 17), AutoSize = true, Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = ThemeManager.Ink });
            root.Controls.Add(new Label { Text = "Chọn món để thêm vào giỏ hàng.", Location = new Point(26, 55), AutoSize = true, ForeColor = ThemeManager.Muted });
            var split = new SplitContainer { Location = new Point(24, 91), Size = new Size(1015, 540), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, SplitterDistance = 590, BackColor = ThemeManager.Bg, FixedPanel = FixedPanel.Panel2 };
            root.Controls.Add(split);
            var menuPanel = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.Paper, Padding = new Padding(16) }; split.Panel1.Controls.Add(menuPanel);
            menuPanel.Controls.Add(new Label { Text = "Thực đơn", Location = new Point(16, 12), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = ThemeManager.Ink });
            var products = new FlowLayoutPanel { Location = new Point(12, 46), Dock = DockStyle.Bottom, Height = 450, AutoScroll = true, BackColor = ThemeManager.Paper, Padding = new Padding(4) };
            menuPanel.Controls.Add(products);
            for (int i = 0; i < _names.Length; i++) {
                int index = i;
                var item = new Button { Text = _names[i] + "\r\n" + _prices[i].ToString("N0") + " đ\r\n+ Thêm", Size = new Size(168, 104), Margin = new Padding(6), BackColor = Color.White, ForeColor = ThemeManager.Ink, FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
                item.FlatAppearance.BorderColor = ThemeManager.Line; item.Click += (s, e) => AddItem(index); products.Controls.Add(item);
            }
            var cartPanel = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.Paper, Padding = new Padding(16) }; split.Panel2.Controls.Add(cartPanel);
            cartPanel.Controls.Add(new Label { Text = "Đơn hiện tại", Location = new Point(16, 12), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = ThemeManager.Ink });
            _cartLines = new FlowLayoutPanel { Location = new Point(12, 48), Size = new Size(348, 330), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = ThemeManager.Paper }; cartPanel.Controls.Add(_cartLines);
            _total = new Label { Text = "Tổng cộng: 0 đ", Location = new Point(16, 397), AutoSize = true, Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = ThemeManager.Ink, Anchor = AnchorStyles.Bottom | AnchorStyles.Left }; cartPanel.Controls.Add(_total);
            var pay = new Button { Text = "Xác nhận đơn hàng", Location = new Point(16, 438), Size = new Size(330, 42), BackColor = _accent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right }; pay.FlatAppearance.BorderSize = 0; pay.Click += (s, e) => Checkout(); cartPanel.Controls.Add(pay);
            var clear = new Button { Text = "Xóa giỏ hàng", Location = new Point(16, 486), Size = new Size(330, 34), Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = ThemeManager.Muted }; clear.Click += (s, e) => { _cart.Clear(); RenderCart(); }; cartPanel.Controls.Add(clear);
        }

        private void AddItem(int index) {
            CartItem item = _cart.FirstOrDefault(x => x.Name == _names[index]);
            if (item == null) _cart.Add(new CartItem { Name = _names[index], Price = _prices[index], Quantity = 1 }); else item.Quantity++;
            RenderCart();
        }

        private void RenderCart() {
            _cartLines.Controls.Clear();
            foreach (CartItem item in _cart) {
                var row = new Panel { Size = new Size(325, 58), BackColor = Color.FromArgb(250, 251, 254), Margin = new Padding(2) };
                row.Controls.Add(new Label { Text = item.Name, Location = new Point(8, 5), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeManager.Ink });
                row.Controls.Add(new Label { Text = item.Quantity + " × " + item.Price.ToString("N0") + " đ", Location = new Point(8, 29), AutoSize = true, ForeColor = ThemeManager.Muted });
                var remove = new Button { Text = "−", Location = new Point(278, 10), Size = new Size(35, 34), FlatStyle = FlatStyle.Flat, ForeColor = ThemeManager.Red }; remove.Click += (s, e) => { if (item.Quantity > 1) item.Quantity--; else _cart.Remove(item); RenderCart(); }; row.Controls.Add(remove);
                _cartLines.Controls.Add(row);
            }
            _total.Text = "Tổng cộng: " + _cart.Sum(x => x.Price * x.Quantity).ToString("N0") + " đ";
        }

        private void Checkout() {
            if (_cart.Count == 0) { MessageBox.Show(this, "Vui lòng chọn ít nhất một món.", "Đơn hàng", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            MessageBox.Show(this, "Đơn hàng demo đã được tạo. Dữ liệu chưa được gửi tới API.", "Đã xác nhận", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _cart.Clear(); RenderCart();
        }
    }
}

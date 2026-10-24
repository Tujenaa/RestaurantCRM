using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.BanHang
{
    public partial class TaoDonHangForm
    {
        // ════════════════════════════════════════════════════════════════════
        //  CỘT PHẢI – DỰNG GIAO DIỆN
        //  Bố cục 4 hàng (TableLayoutPanel):
        //   [0] Thông tin khách hàng        (cố định)
        //   [1] "Món đã chọn (n)" + "Xóa tất cả"  (cố định)
        //   [2] Danh sách món                (co giãn + cuộn dọc)  ← chỉ vùng này cuộn
        //   [3] Voucher + Tổng kết + Nút thanh toán (cố định ở đáy)
        // ════════════════════════════════════════════════════════════════════
        private Control BuildCartColumn()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = 1,
                RowCount = 4,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 118F));   // thông tin khách (tối ưu chiều cao)
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));    // header giỏ hàng
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));    // danh sách món (cuộn)
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));         // footer cố định

            // Vùng cuộn: chiều cao = phần còn lại của cột → tự có thanh cuộn dọc khi nhiều món
            _cartLines = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.White,
                Margin = Padding.Empty,
                Padding = new Padding(0, 2, 0, 2)
            };
            // Khi thanh cuộn xuất hiện/biến mất, ClientSize đổi → co các thẻ món cho vừa chiều ngang (không bị cuộn ngang)
            _cartLines.ClientSizeChanged += (s, e) => FitCartRowWidths();

            layout.Controls.Add(BuildCustomerSection(), 0, 0);
            layout.Controls.Add(BuildCartHeader(), 0, 1);
            layout.Controls.Add(_cartLines, 0, 2);
            layout.Controls.Add(BuildCartFooter(), 0, 3);
            return layout;
        }

        // ── [0] Thông tin khách hàng ──
        private Control BuildCustomerSection()
        {
            var t = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = 2,
                RowCount = 5,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));  // tiêu đề
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));  // caption
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));  // ô nhập
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));  // caption ghi chú
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));  // ô nhập ghi chú

            var title = new Label
            {
                Text = "Thông tin đơn mang đi",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                Margin = Padding.Empty
            };
            t.Controls.Add(title, 0, 0);
            t.SetColumnSpan(title, 2);

            t.Controls.Add(MakeCaption("Tên khách hàng", new Padding(0, 0, 6, 1)), 0, 1);
            t.Controls.Add(MakeCaption("Số điện thoại", new Padding(6, 0, 0, 1)), 1, 1);

            _txtTenKhach = new TextBox
            {
                Text = "Khách mua mang đi",
                Font = Font,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(0, 0, 6, 0)
            };
            _txtSdt = new TextBox
            {
                Font = Font,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(6, 0, 0, 0)
            };
            t.Controls.Add(_txtTenKhach, 0, 2);
            t.Controls.Add(_txtSdt, 1, 2);

            var lblGhiChu = MakeCaption("Ghi chú đơn hàng", new Padding(0, 0, 0, 1));
            t.Controls.Add(lblGhiChu, 0, 3);
            t.SetColumnSpan(lblGhiChu, 2);

            _txtGhiChu = new TextBox
            {
                Text = "Bán mang đi tại quầy",
                Font = Font,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = Padding.Empty
            };
            t.Controls.Add(_txtGhiChu, 0, 4);
            t.SetColumnSpan(_txtGhiChu, 2);
            return t;
        }

        private static Label MakeCaption(string text, Padding margin)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = ThemeManager.Muted,
                Margin = margin
            };
        }

        // ── [1] Tiêu đề "Món đã chọn (n)" + nút "Xóa tất cả" ngang hàng, góc phải ──
        private Control BuildCartHeader()
        {
            var t = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            _lblCartCount = new Label
            {
                Text = "Món đã chọn (0)",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                Margin = Padding.Empty
            };

            // Text-button nhỏ gọn: "🗑 Xóa tất cả" – chữ đỏ nhạt (rose-500), hover đậm hơn (rose-600).
            _btnClearCart = new Button
            {
                Text = "🗑  Xóa tất cả",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.Right,
                Margin = Padding.Empty,
                Padding = new Padding(4, 0, 4, 0),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Rose500,
                BackColor = Color.White,
                UseVisualStyleBackColor = false,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TabStop = false,
                UseMnemonic = false,
                Visible = false,
                AccessibleName = "Xóa tất cả món trong giỏ"
            };
            _btnClearCart.FlatAppearance.BorderSize = 0;
            _btnClearCart.FlatAppearance.MouseOverBackColor = Color.White;
            _btnClearCart.FlatAppearance.MouseDownBackColor = Rose50;
            _btnClearCart.MouseEnter += (s, e) => _btnClearCart.ForeColor = Rose600;
            _btnClearCart.MouseLeave += (s, e) => _btnClearCart.ForeColor = Rose500;
            _btnClearCart.Click += (s, e) => ConfirmClearCart();

            t.Controls.Add(_lblCartCount, 0, 0);
            t.Controls.Add(_btnClearCart, 1, 0);
            return t;
        }

        // ── [3] Footer cố định: Voucher → Tổng kết → Nút thanh toán ──
        private Control BuildCartFooter()
        {
            var t = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.White,
                ColumnCount = 2,
                RowCount = 6,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));   // 0: box voucher thu gọn
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));        // 1: Tiền món
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));        // 2: Giảm giá (ẩn nếu không có → cao 0)
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 13F));   // 3: đường kẻ
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));        // 4: Tổng thanh toán
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));   // 5: nút thanh toán

            // 0) Box gợi ý voucher / khuyến mãi
            var promo = BuildPromoBox();
            t.Controls.Add(promo, 0, 0);
            t.SetColumnSpan(promo, 2);

            // 1) Tiền món
            Label subCaption = MakeSummaryLabel("Tiền món", 9.5F, false, ThemeManager.Muted, ContentAlignment.MiddleLeft);
            _lblSubTotalValue = MakeSummaryLabel("0 đ", 9.5F, true, ThemeManager.Ink, ContentAlignment.MiddleRight);
            t.Controls.Add(subCaption, 0, 1);
            t.Controls.Add(_lblSubTotalValue, 1, 1);

            // 2) Giảm giá (chỉ hiện khi có voucher)
            _lblDiscountCaption = MakeSummaryLabel("Giảm giá", 9.5F, false, ThemeManager.Green, ContentAlignment.MiddleLeft);
            _lblDiscountValue = MakeSummaryLabel("-0 đ", 9.5F, true, ThemeManager.Green, ContentAlignment.MiddleRight);
            _lblDiscountCaption.Visible = false;
            _lblDiscountValue.Visible = false;
            t.Controls.Add(_lblDiscountCaption, 0, 2);
            t.Controls.Add(_lblDiscountValue, 1, 2);

            // 3) Đường kẻ phân cách
            var divider = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.Line, Margin = new Padding(0, 6, 0, 6) };
            t.Controls.Add(divider, 0, 3);
            t.SetColumnSpan(divider, 2);

            // 4) Tổng thanh toán – to, đậm
            Label totalCaption = MakeSummaryLabel("Tổng thanh toán", 11F, true, ThemeManager.Ink, ContentAlignment.MiddleLeft);
            _lblTotalValue = MakeSummaryLabel("0 đ", 15F, true, ThemeManager.Ink, ContentAlignment.MiddleRight);
            totalCaption.Margin = new Padding(0, 2, 0, 2);
            _lblTotalValue.Margin = new Padding(0, 2, 0, 2);
            t.Controls.Add(totalCaption, 0, 4);
            t.Controls.Add(_lblTotalValue, 1, 4);

            // 5) Nút chính – full chiều ngang
            _btnPay = new Button
            {
                Text = "✓  Xác nhận đơn hàng",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 10, 0, 0),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                UseMnemonic = false,
                Cursor = Cursors.Hand
            };
            ButtonSkin.SetKind(_btnPay, ButtonSkin.Kind.Primary);
            _btnPay.Click += async (s, e) => await Checkout();
            t.Controls.Add(_btnPay, 0, 5);
            t.SetColumnSpan(_btnPay, 2);

            return t;
        }

        private static Label MakeSummaryLabel(string text, float size, bool bold, Color color, ContentAlignment align)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Anchor = align == ContentAlignment.MiddleRight ? AnchorStyles.Right : AnchorStyles.Left,
                Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = color,
                Margin = new Padding(0, 3, 0, 3),
                UseMnemonic = false
            };
        }

        // Box voucher thu gọn: cao 38-40px, gọn gàng, hỗ trợ cả nhập mã nhanh lẫn chọn voucher
        private Control BuildPromoBox()
        {
            _pnlPromoBox = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 4, 0, 4),
                Radius = 8,
                BorderWidth = 1,
                BorderColor = ThemeManager.Line,
                FillColor = ThemeManager.Surface2
            };

            _lblPromoIcon = new Label
            {
                Text = "🎟️",
                Location = new Point(8, 8),
                Size = new Size(22, 22),
                Font = new Font("Segoe UI", 10.5F),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };

            _lblPromoTitle = new Label
            {
                Text = "Voucher / Mã giảm giá",
                Location = new Point(34, 4),
                Size = new Size(180, 16),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                BackColor = Color.Transparent,
                AutoEllipsis = true,
                UseMnemonic = false
            };

            _lblPromoSub = new Label
            {
                Text = "Nhập mã hoặc chọn ưu đãi",
                Location = new Point(34, 20),
                Size = new Size(180, 15),
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = ThemeManager.Muted,
                BackColor = Color.Transparent,
                AutoEllipsis = true,
                UseMnemonic = false
            };

            // Ô nhập mã nhanh
            _txtQuickVoucher = new TextBox
            {
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = ThemeManager.Muted,
                Text = "Mã voucher...",
                Height = 24
            };
            _txtQuickVoucher.GotFocus += (s, e) => {
                if (_txtQuickVoucher.Text == "Mã voucher...")
                {
                    _txtQuickVoucher.Text = "";
                    _txtQuickVoucher.ForeColor = ThemeManager.Ink;
                }
            };
            _txtQuickVoucher.LostFocus += (s, e) => {
                if (string.IsNullOrWhiteSpace(_txtQuickVoucher.Text))
                {
                    _txtQuickVoucher.Text = "Mã voucher...";
                    _txtQuickVoucher.ForeColor = ThemeManager.Muted;
                }
            };
            _txtQuickVoucher.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ApplyQuickVoucher();
                }
            };

            _btnQuickApply = new Button
            {
                Text = "Dùng",
                Size = new Size(54, 26),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            ButtonSkin.SetKind(_btnQuickApply, ButtonSkin.Kind.Primary);
            _btnQuickApply.Click += (s, e) => ApplyQuickVoucher();

            _btnPromoAction = new Button
            {
                Text = "Bỏ mã",
                Size = new Size(64, 26),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            ButtonSkin.SetKind(_btnPromoAction, ButtonSkin.Kind.Soft);
            _btnPromoAction.Click += (s, e) => HandlePromoActionClick();

            _lnkChangePromo = new Label
            {
                Text = "Chọn mã ▾",
                Size = new Size(70, 26),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = ThemeManager.Primary,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            _lnkChangePromo.Click += (s, e) => OpenSelectVoucherDialog();

            _pnlPromoBox.Controls.Add(_lblPromoIcon);
            _pnlPromoBox.Controls.Add(_lblPromoTitle);
            _pnlPromoBox.Controls.Add(_lblPromoSub);
            _pnlPromoBox.Controls.Add(_txtQuickVoucher);
            _pnlPromoBox.Controls.Add(_btnQuickApply);
            _pnlPromoBox.Controls.Add(_btnPromoAction);
            _pnlPromoBox.Controls.Add(_lnkChangePromo);

            _pnlPromoBox.Resize += (s, e) => LayoutPromoBox();
            LayoutPromoBox();
            return _pnlPromoBox;
        }

        private void LayoutPromoBox()
        {
            if (_pnlPromoBox == null) return;
            int w = _pnlPromoBox.Width;

            if (_appliedVoucher != null)
            {
                _txtQuickVoucher.Visible = false;
                _btnQuickApply.Visible = false;
                _btnPromoAction.Visible = true;
                _lnkChangePromo.Visible = true;

                int rightMargin = 8;
                _lnkChangePromo.Location = new Point(w - _lnkChangePromo.Width - rightMargin, 6);
                _btnPromoAction.Location = new Point(_lnkChangePromo.Left - _btnPromoAction.Width - 6, 6);

                int textW = Math.Max(60, _btnPromoAction.Left - 38);
                _lblPromoTitle.Width = textW;
                _lblPromoSub.Width = textW;
            }
            else
            {
                _btnPromoAction.Visible = false;
                _lnkChangePromo.Visible = true;
                _txtQuickVoucher.Visible = true;
                _btnQuickApply.Visible = true;

                int rightMargin = 8;
                _lnkChangePromo.Location = new Point(w - _lnkChangePromo.Width - rightMargin, 6);
                _btnQuickApply.Location = new Point(_lnkChangePromo.Left - _btnQuickApply.Width - 4, 6);

                int txtW = Math.Max(75, Math.Min(110, _btnQuickApply.Left - 180));
                _txtQuickVoucher.Size = new Size(txtW, 24);
                _txtQuickVoucher.Location = new Point(_btnQuickApply.Left - txtW - 4, 7);

                int textW = Math.Max(50, _txtQuickVoucher.Left - 38);
                _lblPromoTitle.Width = textW;
                _lblPromoSub.Width = textW;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  CỘT PHẢI – THẺ MÓN TRONG GIỎ (Cart Item Card)
        //   Hàng 1: Tên món (đậm, tự ngắt dòng)               [🗑 xóa món]
        //   Hàng 2: 140,000 đ / phần (chữ nhỏ, xám)
        //   Hàng 3: [SL: 2 ✎] (click để sửa)                  Thành tiền (đậm, xanh)
        // ════════════════════════════════════════════════════════════════════
        private Control CreateCartItemCard(CartItem item, int width)
        {
            var card = new RoundedPanel
            {
                Size = new Size(width, 66),
                Margin = new Padding(0, 0, 0, 5),
                Radius = 8,
                BorderWidth = 1,
                BorderColor = CartCardBorder,
                FillColor = CartCardBg
            };

            // Nút xóa món (thùng rác) ở góc phải
            var btnDelete = new Button
            {
                Text = ThemeManager.Glyph.Delete,
                Location = new Point(width - 8 - 24, 6),
                Size = new Size(24, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                Font = ThemeManager.Icon(9.5F),
                ForeColor = ThemeManager.Muted,
                BackColor = CartCardBg,
                UseVisualStyleBackColor = false,
                Cursor = Cursors.Hand,
                TabStop = false,
                AccessibleName = "Xóa món " + item.Name
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.FlatAppearance.MouseOverBackColor = ThemeManager.RedSoft;
            btnDelete.MouseEnter += (s, e) => btnDelete.ForeColor = ThemeManager.Red;
            btnDelete.MouseLeave += (s, e) => btnDelete.ForeColor = ThemeManager.Muted;
            btnDelete.Click += (s, e) =>
            {
                var confirm = MessageBox.Show(this,
                    $"Xác nhận xóa món '{item.Name}' khỏi giỏ hàng?",
                    "Xác nhận xóa món",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    _cart.Remove(item);
                    OnCartChanged();
                }
            };
            card.Controls.Add(btnDelete);

            // Ô chỉnh số lượng dạng pill [SL: 2 ✎] đặt KẾ BÊN NÚT THÙNG RÁC
            var qtyEditor = CreateInlineQuantityEditor(item);
            qtyEditor.Location = new Point(btnDelete.Left - qtyEditor.Width - 6, 6);
            qtyEditor.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            card.Controls.Add(qtyEditor);

            // Hàng 1: Tên món ăn
            int nameAvailableWidth = Math.Max(100, qtyEditor.Left - 10 - 6);
            var lblName = new Label
            {
                Text = item.Name,
                Location = new Point(10, 8),
                Size = new Size(nameAvailableWidth, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                UseMnemonic = false,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                BackColor = CartCardBg
            };
            card.Controls.Add(lblName);

            // Hàng 2 (trái): Đơn giá nhỏ gọn
            string unitPriceText = $"{item.Price:N0} đ / phần";
            if (item.Price < item.BasePrice)
                unitPriceText += $" (Gốc {item.BasePrice:N0} đ)";
            var lblPrice = new Label
            {
                Text = unitPriceText,
                Location = new Point(10, 36),
                Size = new Size(160, 18),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                AutoEllipsis = true,
                UseMnemonic = false,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = ThemeManager.Muted,
                BackColor = CartCardBg
            };
            card.Controls.Add(lblPrice);

            // Hàng 2 (phải): Tổng tiền món
            var lblTotal = new Label
            {
                Text = $"Thành tiền: {item.Price * item.Quantity:N0} đ",
                Location = new Point(width - 8 - 180, 36),
                Size = new Size(180, 20),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleRight,
                AutoEllipsis = true,
                UseMnemonic = false,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Primary,
                BackColor = CartCardBg
            };
            card.Controls.Add(lblTotal);

            return card;
        }

        // Tạo control hiển thị số lượng dạng pill [ SL: 2 ✎ ] và cho phép click sửa trực tiếp tại dòng
        private Control CreateInlineQuantityEditor(CartItem item)
        {
            var host = new Panel
            {
                Size = new Size(110, 24),
                BackColor = Color.Transparent
            };

            // Chế độ 1: Hiển thị nhãn [ SL: {qty} ✎ ]
            var pnlView = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Radius = 6,
                BorderWidth = 1,
                BorderColor = StepperBorder,
                FillColor = Color.White,
                Cursor = Cursors.Hand
            };

            var lblView = new Label
            {
                Text = $"SL: {item.Quantity}  ✎",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink2,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            pnlView.Controls.Add(lblView);

            // Chế độ 2: Panel chỉnh sửa trực tiếp với ô nhập + nút lưu + nút hủy
            var pnlEdit = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Visible = false
            };

            var numInput = new NumericUpDown
            {
                Location = new Point(0, 0),
                Size = new Size(52, 24),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Minimum = 0,
                Maximum = Math.Max(100, item.MaxStock),
                Value = Math.Min(Math.Max(1, item.Quantity), Math.Max(100, item.MaxStock)),
                TextAlign = HorizontalAlignment.Center
            };

            var btnOk = new Button
            {
                Text = "✓",
                Location = new Point(54, 0),
                Size = new Size(26, 24),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                BackColor = _accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnOk.FlatAppearance.BorderSize = 0;

            var btnCancel = new Button
            {
                Text = "✕",
                Location = new Point(82, 0),
                Size = new Size(26, 24),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = ThemeManager.Muted,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            pnlEdit.Controls.Add(numInput);
            pnlEdit.Controls.Add(btnOk);
            pnlEdit.Controls.Add(btnCancel);

            Action switchToEdit = () =>
            {
                pnlView.Visible = false;
                pnlEdit.Visible = true;
                numInput.Value = item.Quantity;
                numInput.Focus();
                if (numInput.Controls.Count > 1 && numInput.Controls[1] is TextBox tb)
                {
                    tb.SelectAll();
                }
            };

            Action switchToView = () =>
            {
                pnlEdit.Visible = false;
                pnlView.Visible = true;
            };

            Action saveQuantity = () =>
            {
                int newQty = (int)numInput.Value;
                if (newQty == item.Quantity)
                {
                    switchToView();
                    return;
                }

                if (newQty <= 0)
                {
                    var confirm = MessageBox.Show(this,
                        $"Bạn đã nhập số lượng là 0.\nXác nhận xóa món '{item.Name}' khỏi giỏ hàng?",
                        "Xác nhận xóa món",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    if (confirm == DialogResult.Yes)
                    {
                        _cart.Remove(item);
                        OnCartChanged();
                    }
                    else
                    {
                        numInput.Value = item.Quantity;
                        switchToView();
                    }
                    return;
                }

                if (newQty > item.MaxStock)
                {
                    MessageBox.Show(this,
                        $"Món '{item.Name}' trong kho chỉ còn {item.MaxStock} suất.\nSố lượng không được vượt quá tồn kho khả dụng.",
                        "Vượt quá tồn kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    numInput.Value = Math.Min(item.Quantity, item.MaxStock);
                    return;
                }

                item.Quantity = newQty;
                switchToView();
                OnCartChanged();
            };

            pnlView.Click += (s, e) => switchToEdit();
            lblView.Click += (s, e) => switchToEdit();

            pnlView.MouseEnter += (s, e) => {
                pnlView.BorderColor = _accent;
                lblView.ForeColor = _accent;
            };
            pnlView.MouseLeave += (s, e) => {
                pnlView.BorderColor = StepperBorder;
                lblView.ForeColor = ThemeManager.Ink2;
            };

            btnOk.Click += (s, e) => saveQuantity();
            btnCancel.Click += (s, e) => switchToView();

            numInput.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    saveQuantity();
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    e.SuppressKeyPress = true;
                    switchToView();
                }
            };

            host.Controls.Add(pnlView);
            host.Controls.Add(pnlEdit);
            return host;
        }

        // Co các thẻ món theo bề ngang khả dụng của vùng cuộn (trừ thanh cuộn dọc nếu có)
        private void FitCartRowWidths()
        {
            if (_cartLines == null) return;
            int avail = _cartLines.ClientSize.Width - _cartLines.Padding.Horizontal;
            foreach (Control c in _cartLines.Controls)
            {
                int target = avail - c.Margin.Horizontal;
                if (target > 0 && c.Width != target) c.Width = target;
            }
        }
    }
}

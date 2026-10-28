using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services.BanHang;

namespace RestaurantCRM.AdminApp.Controls.BanHang
{
    /// <summary>
    /// Chuyên trách dựng giao diện và cập nhật hiển thị cho cột Giỏ hàng bên phải:
    /// - Thông tin khách hàng (Tên, SĐT, Ghi chú)
    /// - Tiêu đề số lượng món và nút xóa giỏ hàng
    /// - Danh sách thẻ món cuộn dọc và sửa số lượng inline
    /// - Box voucher và bảng tổng kết tiền thanh toán
    /// </summary>
    public class CartPanelBuilder
    {
        private static readonly Color CartCardBg = Color.FromArgb(248, 250, 252);
        private static readonly Color CartCardBorder = Color.FromArgb(226, 232, 240);
        private static readonly Color StepperBorder = Color.FromArgb(203, 213, 225);
        private static readonly Color Rose500 = Color.FromArgb(244, 63, 94);
        private static readonly Color Rose600 = Color.FromArgb(225, 29, 72);
        private static readonly Color Rose50 = Color.FromArgb(255, 241, 242);

        private readonly Font _baseFont;
        private readonly Color _accent = ThemeManager.Primary;

        // UI Controls
        private FlowLayoutPanel _cartLines;
        private int _lastRenderedLineCount;

        private RoundedPanel _pnlPromoBox;
        private Label _lblPromoIcon;
        private Label _lblPromoTitle;
        private Label _lblPromoSub;
        private Button _btnPromoAction;
        private Label _lnkChangePromo;
        private TextBox _txtQuickVoucher;
        private Button _btnQuickApply;

        private Label _lblCartCount;
        private Button _btnClearCart;

        private Label _lblSubTotalValue;
        private Label _lblDiscountCaption;
        private Label _lblDiscountValue;
        private Label _lblTotalValue;

        private TextBox _txtTenKhach;
        private TextBox _txtSdt;
        private ComboBox _cboPaymentMethod;
        private TextBox _txtGhiChu;
        private Button _btnPay;

        // Callbacks
        public event Action<CartItem, int> QuantityChanged;
        public event Action<CartItem> ItemRemoved;
        public event Action ClearCartRequested;
        public event Action<string> QuickVoucherApplied;
        public event Action SelectVoucherClicked;
        public event Action PromoActionClicked;
        public event Action CheckoutClicked;

        public string CustomerName => _txtTenKhach?.Text?.Trim() ?? "";
        public string CustomerPhone => _txtSdt?.Text?.Trim() ?? "";
        public string PaymentMethod => _cboPaymentMethod?.SelectedItem?.ToString() ?? "Tiền mặt (Tại quầy)";
        public string Note => _txtGhiChu?.Text?.Trim() ?? "";

        public CartPanelBuilder(Font baseFont)
        {
            _baseFont = baseFont ?? new Font("Segoe UI", 9.5F);
        }

        public Control BuildCartColumn()
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
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 136F));   // thông tin đơn hàng
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));    // header giỏ hàng
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));    // danh sách món cuộn
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));         // footer cố định

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
            _cartLines.ClientSizeChanged += (s, e) => FitCartRowWidths();

            layout.Controls.Add(BuildCustomerSection(), 0, 0);
            layout.Controls.Add(BuildCartHeader(), 0, 1);
            layout.Controls.Add(_cartLines, 0, 2);
            layout.Controls.Add(BuildCartFooter(), 0, 3);
            return layout;
        }

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
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));

            var title = new Label
            {
                Text = "Thông tin đơn hàng",
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
                Font = _baseFont,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(0, 0, 6, 0)
            };
            _txtSdt = new TextBox
            {
                Font = _baseFont,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(6, 0, 0, 0)
            };
            t.Controls.Add(_txtTenKhach, 0, 2);
            t.Controls.Add(_txtSdt, 1, 2);

            t.Controls.Add(MakeCaption("Phương thức thanh toán", new Padding(0, 0, 6, 1)), 0, 3);
            t.Controls.Add(MakeCaption("Ghi chú đơn hàng", new Padding(6, 0, 0, 1)), 1, 3);

            _cboPaymentMethod = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = _baseFont,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(0, 0, 6, 0)
            };
            _cboPaymentMethod.Items.AddRange(new object[]
            {
                "Tiền mặt (Tại quầy)",
                "Chuyển khoản (QR / Ngân hàng)",
                "Thẻ ngân hàng (POS)",
                "Ví điện tử (Momo / VNPay)"
            });
            _cboPaymentMethod.SelectedIndex = 0;

            _txtGhiChu = new TextBox
            {
                Text = "Bán mang đi tại quầy",
                Font = _baseFont,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(6, 0, 0, 0)
            };
            t.Controls.Add(_cboPaymentMethod, 0, 4);
            t.Controls.Add(_txtGhiChu, 1, 4);
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
            _btnClearCart.Click += (s, e) => _cartLines.BeginInvoke((Action)(() => ClearCartRequested?.Invoke()));

            t.Controls.Add(_lblCartCount, 0, 0);
            t.Controls.Add(_btnClearCart, 1, 0);
            return t;
        }

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
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 13F));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));

            var promo = BuildPromoBox();
            t.Controls.Add(promo, 0, 0);
            t.SetColumnSpan(promo, 2);

            Label subCaption = MakeSummaryLabel("Tiền món", 9.5F, false, ThemeManager.Muted, ContentAlignment.MiddleLeft);
            _lblSubTotalValue = MakeSummaryLabel("0 đ", 9.5F, true, ThemeManager.Ink, ContentAlignment.MiddleRight);
            t.Controls.Add(subCaption, 0, 1);
            t.Controls.Add(_lblSubTotalValue, 1, 1);

            _lblDiscountCaption = MakeSummaryLabel("Giảm giá", 9.5F, false, ThemeManager.Green, ContentAlignment.MiddleLeft);
            _lblDiscountValue = MakeSummaryLabel("-0 đ", 9.5F, true, ThemeManager.Green, ContentAlignment.MiddleRight);
            _lblDiscountCaption.Visible = false;
            _lblDiscountValue.Visible = false;
            t.Controls.Add(_lblDiscountCaption, 0, 2);
            t.Controls.Add(_lblDiscountValue, 1, 2);

            var divider = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.Line, Margin = new Padding(0, 6, 0, 6) };
            t.Controls.Add(divider, 0, 3);
            t.SetColumnSpan(divider, 2);

            Label totalCaption = MakeSummaryLabel("Tổng thanh toán", 11F, true, ThemeManager.Ink, ContentAlignment.MiddleLeft);
            _lblTotalValue = MakeSummaryLabel("0 đ", 15F, true, ThemeManager.Ink, ContentAlignment.MiddleRight);
            totalCaption.Margin = new Padding(0, 2, 0, 2);
            _lblTotalValue.Margin = new Padding(0, 2, 0, 2);
            t.Controls.Add(totalCaption, 0, 4);
            t.Controls.Add(_lblTotalValue, 1, 4);

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
            _btnPay.Click += (s, e) => CheckoutClicked?.Invoke();
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

            _txtQuickVoucher = new TextBox
            {
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = ThemeManager.Ink,
                Height = 24
            };
            UiStyler.SetPlaceholder(_txtQuickVoucher, "Mã voucher...");
            _txtQuickVoucher.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    QuickVoucherApplied?.Invoke(_txtQuickVoucher.Text);
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
            _btnQuickApply.Click += (s, e) => QuickVoucherApplied?.Invoke(_txtQuickVoucher.Text);

            _btnPromoAction = new Button
            {
                Text = "Bỏ mã",
                Size = new Size(64, 26),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            ButtonSkin.SetKind(_btnPromoAction, ButtonSkin.Kind.Soft);
            _btnPromoAction.Click += (s, e) => PromoActionClicked?.Invoke();

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
            _lnkChangePromo.Click += (s, e) => SelectVoucherClicked?.Invoke();

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

            if (_btnPromoAction.Visible)
            {
                _txtQuickVoucher.Visible = false;
                _btnQuickApply.Visible = false;
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

        public void SetPayButtonEnabled(bool enabled)
        {
            if (_btnPay != null) _btnPay.Enabled = enabled;
        }

        public void ResetQuickVoucherText()
        {
            if (_txtQuickVoucher != null)
            {
                _txtQuickVoucher.Text = "Mã voucher...";
                _txtQuickVoucher.ForeColor = ThemeManager.Muted;
            }
        }

        public void Render(CartService cartService, OrderCalculationService calcService, VoucherDto appliedVoucher, IEnumerable<VoucherDto> allVouchers)
        {
            int distinctCount = cartService.DistinctCount;
            int totalQty = cartService.TotalQuantity;
            if (distinctCount <= 0)
            {
                _lblCartCount.Text = "Món đã chọn (0)";
            }
            else if (distinctCount == totalQty)
            {
                _lblCartCount.Text = $"Món đã chọn ({distinctCount} món)";
            }
            else
            {
                _lblCartCount.Text = $"Món đã chọn ({distinctCount} món · {totalQty} phần)";
            }
            _btnClearCart.Visible = distinctCount > 0;

            RenderCartLines(cartService);

            decimal subTotal = cartService.SubTotal;
            var (bestVoucher, bestDisc, nearVoucher, missingForNear) = calcService.FindBestVoucher(subTotal, allVouchers, cartService.Items);
            var (_, voucherDiscount, finalTotal) = calcService.CalculateTotals(cartService.Items, appliedVoucher);

            UpdatePromoBox(subTotal, appliedVoucher, bestVoucher, bestDisc, nearVoucher, missingForNear, calcService);

            _lblSubTotalValue.Text = $"{subTotal:N0} đ";

            bool hasDiscount = appliedVoucher != null && voucherDiscount > 0;
            _lblDiscountCaption.Visible = hasDiscount;
            _lblDiscountValue.Visible = hasDiscount;
            if (hasDiscount)
            {
                _lblDiscountCaption.Text = $"Giảm giá ({appliedVoucher.MaVoucher})";
                _lblDiscountValue.Text = $"-{voucherDiscount:N0} đ";
            }

            _lblTotalValue.Text = $"{finalTotal:N0} đ";
        }

        private void RenderCartLines(CartService cartService)
        {
            int scrollY = Math.Abs(_cartLines.AutoScrollPosition.Y);
            bool itemAdded = cartService.DistinctCount > _lastRenderedLineCount;

            _cartLines.SuspendLayout();

            var old = _cartLines.Controls.Cast<Control>().ToList();
            _cartLines.Controls.Clear();
            foreach (var c in old) c.Dispose();

            int rowWidth = Math.Max(240, _cartLines.ClientSize.Width - _cartLines.Padding.Horizontal);

            if (cartService.DistinctCount == 0)
            {
                _cartLines.Controls.Add(new Label
                {
                    Text = "Giỏ hàng đang trống.\nChọn món từ thực đơn để bắt đầu.",
                    Size = new Size(rowWidth, 64),
                    Margin = new Padding(0, 12, 0, 0),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = ThemeManager.Muted,
                    Font = new Font("Segoe UI", 9F)
                });
            }
            else
            {
                foreach (var item in cartService.Items)
                {
                    _cartLines.Controls.Add(CreateCartItemCard(item, rowWidth));
                }
            }

            _cartLines.ResumeLayout(true);
            FitCartRowWidths();

            if (itemAdded && _cartLines.Controls.Count > 0)
            {
                var lastControl = _cartLines.Controls[_cartLines.Controls.Count - 1];
                if (lastControl.Bottom > _cartLines.ClientSize.Height)
                {
                    _cartLines.ScrollControlIntoView(lastControl);
                }
            }
            else
            {
                try { _cartLines.AutoScrollPosition = new Point(0, scrollY); } catch { }
            }
            _lastRenderedLineCount = cartService.DistinctCount;
        }

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
            btnDelete.Click += (s, e) => {
                btnDelete.Enabled = false;
                _cartLines.BeginInvoke((Action)(() => ItemRemoved?.Invoke(item)));
            };
            card.Controls.Add(btnDelete);

            var qtyEditor = CreateInlineQuantityEditor(item);
            qtyEditor.Location = new Point(btnDelete.Left - qtyEditor.Width - 6, 6);
            qtyEditor.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            card.Controls.Add(qtyEditor);

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

        private Control CreateInlineQuantityEditor(CartItem item)
        {
            var host = new Panel
            {
                Size = new Size(110, 24),
                BackColor = Color.Transparent
            };

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
                switchToView();
                QuantityChanged?.Invoke(item, newQty);
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

            btnOk.Click += (s, e) => {
                btnOk.Enabled = false;
                _cartLines.BeginInvoke((Action)(() => saveQuantity()));
            };
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

        private void UpdatePromoBox(decimal subTotal, VoucherDto appliedVoucher, VoucherDto best, decimal bestDisc, VoucherDto near, decimal missing, OrderCalculationService calcService)
        {
            if (subTotal <= 0)
            {
                _lblPromoIcon.Text = "🎟️";
                _lblPromoTitle.Text = "Voucher / Giảm giá";
                _lblPromoTitle.ForeColor = ThemeManager.Ink;
                _lblPromoSub.Text = "Chưa có món";
                _btnPromoAction.Visible = false;
                _lnkChangePromo.Text = "Chọn mã ▾";
                _pnlPromoBox.FillColor = Color.FromArgb(248, 250, 252);
                _pnlPromoBox.BorderColor = Color.FromArgb(226, 232, 240);
                LayoutPromoBox();
                return;
            }

            if (appliedVoucher != null)
            {
                var (_, disc, _) = calcService.EvaluateVoucher(appliedVoucher, subTotal);
                _lblPromoIcon.Text = "✔";
                _lblPromoTitle.Text = $"Mã [{appliedVoucher.MaVoucher}]";
                _lblPromoTitle.ForeColor = Color.FromArgb(22, 163, 74);
                _lblPromoSub.Text = $"Giảm -{disc:N0} đ";
                _btnPromoAction.Visible = true;
                _btnPromoAction.Text = "Bỏ mã";
                _btnPromoAction.BackColor = Color.FromArgb(254, 242, 242);
                _btnPromoAction.ForeColor = ThemeManager.Red;
                _lnkChangePromo.Text = "Đổi mã ▾";
                _pnlPromoBox.FillColor = Color.FromArgb(240, 253, 244);
                _pnlPromoBox.BorderColor = Color.FromArgb(187, 247, 208);
                LayoutPromoBox();
                return;
            }

            if (best != null)
            {
                _lblPromoIcon.Text = "✨";
                _lblPromoTitle.Text = $"Gợi ý: [{best.MaVoucher}]";
                _lblPromoTitle.ForeColor = Color.FromArgb(79, 70, 229);
                _lblPromoSub.Text = $"Giảm {bestDisc:N0} đ";
                _btnPromoAction.Visible = false;
                _lnkChangePromo.Text = "Chọn mã ▾";
                _pnlPromoBox.FillColor = Color.FromArgb(238, 242, 255);
                _pnlPromoBox.BorderColor = Color.FromArgb(199, 210, 254);
            }
            else if (near != null)
            {
                _lblPromoIcon.Text = "💡";
                _lblPromoTitle.Text = $"+{missing:N0} đ";
                _lblPromoTitle.ForeColor = ThemeManager.Amber;
                _lblPromoSub.Text = $"Nhận mã [{near.MaVoucher}]";
                _btnPromoAction.Visible = false;
                _lnkChangePromo.Text = "Xem mã ▾";
                _pnlPromoBox.FillColor = Color.FromArgb(255, 251, 235);
                _pnlPromoBox.BorderColor = Color.FromArgb(254, 243, 199);
            }
            else
            {
                _lblPromoIcon.Text = "🎟️";
                _lblPromoTitle.Text = "Voucher / Giảm giá";
                _lblPromoTitle.ForeColor = ThemeManager.Muted;
                _lblPromoSub.Text = "Nhập mã hoặc chọn";
                _btnPromoAction.Visible = false;
                _lnkChangePromo.Text = "Chọn mã ▾";
                _pnlPromoBox.FillColor = Color.FromArgb(248, 250, 252);
                _pnlPromoBox.BorderColor = Color.FromArgb(226, 232, 240);
            }
            LayoutPromoBox();
        }

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

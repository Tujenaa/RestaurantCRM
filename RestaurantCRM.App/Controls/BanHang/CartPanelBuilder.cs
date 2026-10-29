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

        // Tỉ lệ DPI màn hình (100% = 1, 125% = 1.25...). Mọi kích thước pixel đều nhân qua S()
        // để bố cục không bị chật / cắt chữ khi Windows phóng to chữ.
        public static readonly float DpiScale = GetDpiScale();
        public static int S(int px) { return (int)Math.Round(px * DpiScale); }
        private static float GetDpiScale()
        {
            try { using (var g = Graphics.FromHwnd(IntPtr.Zero)) return Math.Max(1f, g.DpiX / 96f); }
            catch { return 1f; }
        }

        private InputBox _voucherBox;
        private bool _voucherApplied;

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
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, S(156)));  // thông tin đơn hàng
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, S(40)));   // header giỏ hàng
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
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, S(30)));   // tiêu đề
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, S(22)));   // nhãn
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, S(40)));   // ô nhập
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, S(24)));   // nhãn
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, S(40)));   // ô nhập

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

            var left = new Padding(0, 0, S(6), 0);
            var right = new Padding(S(6), 0, 0, 0);

            t.Controls.Add(MakeCaption("Tên khách hàng", left), 0, 1);
            t.Controls.Add(MakeCaption("Số điện thoại", right), 1, 1);

            _txtTenKhach = new TextBox { Text = "Khách mua mang đi", Font = _baseFont };
            _txtSdt = new TextBox { Font = _baseFont };
            t.Controls.Add(new InputBox(_txtTenKhach) { Dock = DockStyle.Fill, Margin = left }, 0, 2);
            t.Controls.Add(new InputBox(_txtSdt) { Dock = DockStyle.Fill, Margin = right }, 1, 2);

            t.Controls.Add(MakeCaption("Phương thức thanh toán", left), 0, 3);
            t.Controls.Add(MakeCaption("Ghi chú đơn hàng", right), 1, 3);

            _cboPaymentMethod = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = _baseFont
            };
            _cboPaymentMethod.Items.AddRange(new object[]
            {
                "Tiền mặt (Tại quầy)",
                "Chuyển khoản (QR / Ngân hàng)",
                "Thẻ ngân hàng (POS)",
                "Ví điện tử (Momo / VNPay)"
            });
            _cboPaymentMethod.SelectedIndex = 0;

            _txtGhiChu = new TextBox { Text = "Bán mang đi tại quầy", Font = _baseFont };
            t.Controls.Add(new InputBox(_cboPaymentMethod) { Dock = DockStyle.Fill, Margin = left }, 0, 4);
            t.Controls.Add(new InputBox(_txtGhiChu) { Dock = DockStyle.Fill, Margin = right }, 1, 4);
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
                Margin = new Padding(margin.Left, 0, margin.Right, S(3)),
                UseMnemonic = false
            };
        }

        /// <summary>
        /// Ô nhập bo tròn tự vẽ viền, chứa TextBox/ComboBox bên trong.
        /// Tự căn giữa control con nên không bị lệch khi nằm trong TableLayoutPanel.
        /// </summary>
        private class InputBox : Panel
        {
            private readonly Control _inner;
            private bool _focus;

            public InputBox(Control inner)
            {
                _inner = inner;
                inner.Tag = UiStyler.NoStyleTag;            // không cho FieldSkin chung can thiệp
                SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
                BackColor = Color.Transparent;   // góc bo lấy nền của khung cha (trắng hoặc nền ô voucher)

                var tb = inner as TextBox;
                if (tb != null) { tb.BorderStyle = BorderStyle.None; tb.BackColor = Color.White; }
                var cb = inner as ComboBox;
                if (cb != null) { cb.FlatStyle = FlatStyle.Flat; cb.BackColor = Color.White; }

                Controls.Add(inner);
                if (cb != null) ComboFrame.Attach(cb);   // che viền vuông + vẽ mũi tên chevron

                inner.Enter += (s, e) => { _focus = true; Invalidate(); };
                inner.Leave += (s, e) => { _focus = false; Invalidate(); };
                inner.EnabledChanged += (s, e) => Invalidate();
                Click += (s, e) => _inner.Focus();
                Cursor = Cursors.IBeam;
            }

            protected override void OnLayout(LayoutEventArgs e)
            {
                base.OnLayout(e);
                bool combo = _inner is ComboBox;
                int padL = combo ? S(5) : S(12);
                int padR = combo ? S(4) : S(12);
                int w = Math.Max(10, Width - padL - padR);
                int h = _inner.Height;
                _inner.SetBounds(padL, Math.Max(0, (Height - h) / 2), w, h);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
                using (var path = SkinUtil.Round(r, Math.Min(S(9), r.Height / 2f)))
                {
                    using (var br = new SolidBrush(_inner.Enabled ? Color.White : ThemeManager.Surface2)) g.FillPath(br, path);
                    using (var pen = new Pen(_focus ? ThemeManager.Primary : Color.FromArgb(222, 225, 234), _focus ? 1.6f : 1f))
                        g.DrawPath(pen, path);
                }
            }
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

            var promo = BuildPromoBox();
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, promo.Height + promo.Margin.Vertical));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, S(13)));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, S(60)));

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

            var divider = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.Line, Margin = new Padding(0, S(6), 0, S(6)) };
            t.Controls.Add(divider, 0, 3);
            t.SetColumnSpan(divider, 2);

            Label totalCaption = MakeSummaryLabel("Tổng thanh toán", 11F, true, ThemeManager.Ink, ContentAlignment.MiddleLeft);
            _lblTotalValue = MakeSummaryLabel("0 đ", 15F, true, ThemeManager.Primary, ContentAlignment.MiddleRight);
            totalCaption.Margin = new Padding(0, S(2), 0, S(2));
            _lblTotalValue.Margin = new Padding(0, S(2), 0, S(2));
            t.Controls.Add(totalCaption, 0, 4);
            t.Controls.Add(_lblTotalValue, 1, 4);

            _btnPay = new Button
            {
                Text = "✓  Xác nhận đơn hàng",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, S(10), 0, 0),
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
                Margin = new Padding(0, S(3), 0, S(3)),
                UseMnemonic = false
            };
        }

        /// <summary>
        /// Ô voucher 2 tầng:
        ///   [icon] Tiêu đề / dòng phụ ............ Chọn mã ▾
        ///   [ ô nhập mã voucher ................ ] [Dùng]   (hoặc [Bỏ mã] khi đã áp mã)
        /// </summary>
        private Control BuildPromoBox()
        {
            var fTitle = new Font("Segoe UI", 9F, FontStyle.Bold);
            var fSub = new Font("Segoe UI", 8F);
            int pad = S(10);
            int inputRowH = S(36);
            int textH = TextRenderer.MeasureText("Ag", fTitle).Height + TextRenderer.MeasureText("Ag", fSub).Height;

            _pnlPromoBox = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Height = pad + textH + S(8) + inputRowH + pad,
                Margin = new Padding(0, S(4), 0, S(6)),
                Padding = new Padding(pad, pad - S(2), pad, pad),
                Radius = S(10),
                BorderWidth = 1,
                BorderColor = ThemeManager.Line,
                FillColor = ThemeManager.Surface2
            };

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 3,
                RowCount = 3,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            _lblPromoIcon = new Label
            {
                Text = "🎟️",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Font = new Font("Segoe UI Emoji", 12F),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, S(6), 0)
            };
            grid.Controls.Add(_lblPromoIcon, 0, 0);
            grid.SetRowSpan(_lblPromoIcon, 2);

            _lblPromoTitle = new Label
            {
                Text = "Voucher / Mã giảm giá",
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = fTitle,
                ForeColor = ThemeManager.Ink,
                BackColor = Color.Transparent,
                AutoEllipsis = true,
                UseMnemonic = false,
                Margin = Padding.Empty
            };
            grid.Controls.Add(_lblPromoTitle, 1, 0);

            _lblPromoSub = new Label
            {
                Text = "Nhập mã hoặc chọn ưu đãi",
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = fSub,
                ForeColor = ThemeManager.Muted,
                BackColor = Color.Transparent,
                AutoEllipsis = true,
                UseMnemonic = false,
                Margin = Padding.Empty
            };
            grid.Controls.Add(_lblPromoSub, 1, 1);

            _lnkChangePromo = new Label
            {
                Text = "Chọn mã ▾",
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Primary,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent,
                Padding = new Padding(S(4)),
                Margin = Padding.Empty
            };
            _lnkChangePromo.Click += (s, e) => SelectVoucherClicked?.Invoke();
            _lnkChangePromo.MouseEnter += (s, e) => _lnkChangePromo.ForeColor = ThemeManager.PrimaryDark;
            _lnkChangePromo.MouseLeave += (s, e) => _lnkChangePromo.ForeColor = ThemeManager.Primary;
            grid.Controls.Add(_lnkChangePromo, 2, 0);
            grid.SetRowSpan(_lnkChangePromo, 2);

            // Hàng nhập mã
            var inputRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, S(8), 0, 0),
                Padding = Padding.Empty
            };
            inputRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            inputRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            inputRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            _txtQuickVoucher = new TextBox
            {
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeManager.Ink
            };
            UiStyler.SetPlaceholder(_txtQuickVoucher, "Mã voucher...");
            _txtQuickVoucher.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    QuickVoucherApplied?.Invoke(_txtQuickVoucher.Text);
                }
            };
            _voucherBox = new InputBox(_txtQuickVoucher) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, S(8), 0) };
            inputRow.Controls.Add(_voucherBox, 0, 0);

            var actions = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            _btnQuickApply = new Button
            {
                Text = "Dùng",
                Size = new Size(S(72), inputRowH),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = Padding.Empty
            };
            ButtonSkin.SetKind(_btnQuickApply, ButtonSkin.Kind.Primary);
            _btnQuickApply.Click += (s, e) => QuickVoucherApplied?.Invoke(_txtQuickVoucher.Text);

            _btnPromoAction = new Button
            {
                Text = "Bỏ mã",
                Size = new Size(S(84), inputRowH),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = Padding.Empty,
                Visible = false
            };
            ButtonSkin.SetKind(_btnPromoAction, ButtonSkin.Kind.Soft);
            _btnPromoAction.Click += (s, e) => PromoActionClicked?.Invoke();

            actions.Controls.Add(_btnQuickApply);
            actions.Controls.Add(_btnPromoAction);
            inputRow.Controls.Add(actions, 1, 0);

            grid.Controls.Add(inputRow, 0, 2);
            grid.SetColumnSpan(inputRow, 3);

            _pnlPromoBox.Controls.Add(grid);
            LayoutPromoBox();
            return _pnlPromoBox;
        }

        /// <summary>Đã áp mã: ẩn ô nhập + nút Dùng, hiện nút Bỏ mã (và ngược lại).</summary>
        private void SetVoucherApplied(bool applied)
        {
            _voucherApplied = applied;
        }

        private void LayoutPromoBox()
        {
            if (_pnlPromoBox == null) return;
            _voucherBox.Visible = !_voucherApplied;
            _btnQuickApply.Visible = !_voucherApplied;
            _btnPromoAction.Visible = _voucherApplied;
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
                    Size = new Size(rowWidth, S(64)),
                    Margin = new Padding(0, S(12), 0, 0),
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
                Size = new Size(width, S(70)),
                Margin = new Padding(0, 0, 0, S(6)),
                Radius = S(8),
                BorderWidth = 1,
                BorderColor = CartCardBorder,
                FillColor = CartCardBg
            };

            int pad = S(10);
            int btn = S(26);
            var btnDelete = new Button
            {
                Text = ThemeManager.Glyph.Delete,
                Location = new Point(width - pad - btn, S(7)),
                Size = new Size(btn, btn),
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
            qtyEditor.Location = new Point(btnDelete.Left - qtyEditor.Width - S(6), S(7));
            qtyEditor.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            card.Controls.Add(qtyEditor);

            var fName = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            int nameH = TextRenderer.MeasureText("Ag", fName).Height + S(2);
            var lblName = new Label
            {
                Text = item.Name,
                Location = new Point(pad, S(9)),
                Size = new Size(Math.Max(S(80), qtyEditor.Left - pad - S(6)), nameH),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                UseMnemonic = false,
                Font = fName,
                ForeColor = ThemeManager.Ink,
                BackColor = CartCardBg
            };
            card.Controls.Add(lblName);

            var fPrice = new Font("Segoe UI", 8.5F);
            var fTotal = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            string totalText = $"{item.Price * item.Quantity:N0} đ";
            int totalW = TextRenderer.MeasureText(totalText, fTotal).Width + S(4);
            int lineH = TextRenderer.MeasureText("Ag", fTotal).Height + S(2);
            int lineY = card.Height - pad - lineH + S(2);

            string unitPriceText = $"{item.Price:N0} đ × {item.Quantity}";
            if (item.Price < item.BasePrice)
                unitPriceText += $"  (gốc {item.BasePrice:N0} đ)";
            var lblPrice = new Label
            {
                Text = unitPriceText,
                Location = new Point(pad, lineY),
                Size = new Size(Math.Max(S(60), width - pad * 2 - totalW - S(8)), lineH),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                UseMnemonic = false,
                Font = fPrice,
                ForeColor = ThemeManager.Muted,
                BackColor = CartCardBg
            };
            card.Controls.Add(lblPrice);

            var lblTotal = new Label
            {
                Text = totalText,
                Location = new Point(width - pad - totalW, lineY),
                Size = new Size(totalW, lineH),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleRight,
                UseMnemonic = false,
                Font = fTotal,
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
                Size = new Size(S(112), S(26)),
                BackColor = Color.Transparent
            };

            var pnlView = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Radius = S(6),
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
                Size = new Size(S(54), S(26)),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Minimum = 0,
                Maximum = Math.Max(100, item.MaxStock),
                Value = Math.Min(Math.Max(1, item.Quantity), Math.Max(100, item.MaxStock)),
                TextAlign = HorizontalAlignment.Center
            };

            var btnOk = new Button
            {
                Text = "✓",
                Location = new Point(S(56), 0),
                Size = new Size(S(27), S(26)),
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
                Location = new Point(S(85), 0),
                Size = new Size(S(27), S(26)),
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
                SetVoucherApplied(false);
                _lnkChangePromo.Text = "Chọn mã ▾";
                _pnlPromoBox.FillColor = ThemeManager.Surface2;
                _pnlPromoBox.BorderColor = ThemeManager.Line;
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
                SetVoucherApplied(true);
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
                _lblPromoTitle.ForeColor = ThemeManager.Primary;
                _lblPromoSub.Text = $"Giảm {bestDisc:N0} đ";
                SetVoucherApplied(false);
                _lnkChangePromo.Text = "Chọn mã ▾";
                _pnlPromoBox.FillColor = ThemeManager.PrimarySoft;
                _pnlPromoBox.BorderColor = Color.FromArgb(247, 213, 192);
            }
            else if (near != null)
            {
                _lblPromoIcon.Text = "💡";
                _lblPromoTitle.Text = $"+{missing:N0} đ";
                _lblPromoTitle.ForeColor = ThemeManager.Amber;
                _lblPromoSub.Text = $"Nhận mã [{near.MaVoucher}]";
                SetVoucherApplied(false);
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
                SetVoucherApplied(false);
                _lnkChangePromo.Text = "Chọn mã ▾";
                _pnlPromoBox.FillColor = ThemeManager.Surface2;
                _pnlPromoBox.BorderColor = ThemeManager.Line;
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

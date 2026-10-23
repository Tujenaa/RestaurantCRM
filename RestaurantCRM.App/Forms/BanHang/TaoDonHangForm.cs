using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Forms.HoaDon;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.BanHang
{
    public class TaoDonHangForm : Form
    {
        private sealed class CartItem
        {
            public string MaMon;
            public string Name;
            public decimal BasePrice;       // Đơn giá gốc
            public decimal Price;           // Đơn giá bán (đã trừ KM món nếu có)
            public int Quantity;
            public int MaxStock;
            public string PromoInfo;        // Tên/mức giảm món nếu có
        }

        private readonly MonAnService _monAnService;
        private readonly LoaiMonService _loaiMonService;
        private readonly HoaDonService _hoaDonService;
        private readonly KhuyenMaiService _khuyenMaiService;

        private List<MonAnDto> _allMonAn = new List<MonAnDto>();
        private List<LoaiMonDto> _allLoaiMon = new List<LoaiMonDto>();
        private List<VoucherDto> _allVouchers = new List<VoucherDto>();
        private List<KmMonDto> _allKmMon = new List<KmMonDto>();
        private readonly List<CartItem> _cart = new List<CartItem>();

        private VoucherDto _appliedVoucher;
        private bool _autoApplyBestDeal = true;

        private readonly Timer _autoRefreshTimer;
        private bool _isRefreshing = false;

        private readonly FlowLayoutPanel _productsPanel;
        // ══════════════ CỘT PHẢI: GIỎ HÀNG ══════════════
        private FlowLayoutPanel _cartLines;             // Vùng cuộn danh sách món (chỉ vùng này cuộn)
        private int _lastRenderedLineCount;             // Nhận biết khi có món mới để tự cuộn tới món đó

        // Box khuyến mãi (giữ nguyên chức năng)
        private RoundedPanel _pnlPromoBox;
        private Label _lblPromoIcon;
        private Label _lblPromoTitle;
        private Label _lblPromoSub;
        private Button _btnPromoAction;
        private Label _lnkChangePromo;

        // Bảng tổng kết tiền (footer cố định)
        private Label _lblSubTotalValue;                // "Tiền món"      → giá trị bên phải
        private Label _lblDiscountCaption;              // "Giảm giá (…)"  → chỉ hiện khi có voucher
        private Label _lblDiscountValue;
        private Label _lblTotalValue;                   // "Tổng thanh toán" → giá trị lớn bên phải

        // Header giỏ hàng
        private Label _lblCartCount;
        private Button _btnClearCart;

        // Thông tin khách + nút thanh toán
        private TextBox _txtTenKhach;
        private TextBox _txtSdt;
        private TextBox _txtGhiChu;
        private Button _btnPay;

        private readonly TextBox _txtSearch;
        private readonly ComboBox _cboCategory;
        private readonly CheckBox _chkOnlyInStock;

        // Bảng màu giỏ hàng (Tailwind-like)
        private static readonly Color CartCardBg = Color.FromArgb(248, 250, 252); // #F8FAFC
        private static readonly Color CartCardBorder = Color.FromArgb(226, 232, 240); // #E2E8F0
        private static readonly Color StepperBorder = Color.FromArgb(203, 213, 225); // slate-300
        private static readonly Color StepperHover = Color.FromArgb(241, 245, 249); // slate-100
        private static readonly Color Rose500 = Color.FromArgb(244, 63, 94);   // text-rose-500
        private static readonly Color Rose600 = Color.FromArgb(225, 29, 72);   // hover: text-rose-600
        private static readonly Color Rose50 = Color.FromArgb(255, 241, 242);
        private readonly Label _lblNhanVien;

        private readonly Color _accent = ThemeManager.Primary;

        public TaoDonHangForm()
        {
            _monAnService = new MonAnService();
            _loaiMonService = new LoaiMonService();
            _hoaDonService = new HoaDonService();
            _khuyenMaiService = new KhuyenMaiService();

            Text = "Tạo đơn hàng mang đi tại quầy | RestaurantCRM";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1060, 710);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);

            _autoRefreshTimer = new Timer { Interval = 15000 };
            _autoRefreshTimer.Tick += async (s, e) => await RefreshStockContinuouslyAsync();
            this.Activated += async (s, e) => await RefreshStockContinuouslyAsync();
            this.FormClosing += (s, e) => {
                _autoRefreshTimer.Stop();
                _autoRefreshTimer.Dispose();
            };

            Padding = new Padding(14, 6, 14, 10);

            // ── Hàng đầu: tiêu đề (ẩn khi mở trong cửa sổ chính) + nhân viên lập đơn ──
            var header = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = ThemeManager.Bg };
            var lblPageTitle = new Label
            {
                Text = "Tạo đơn hàng bán mang đi / tại quầy",
                Location = new Point(10, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            };
            header.Controls.Add(lblPageTitle);

            string currentStaff = UserSession.CurrentDisplayName ?? UserSession.CurrentUsername ?? "Nhân viên";
            _lblNhanVien = new Label
            {
                Text = $"Nhân viên lập đơn: {currentStaff}",
                Location = new Point(12, 44),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeManager.Muted
            };
            header.Controls.Add(_lblNhanVien);
            Action placeTitle = () => {
                bool embedded = !TopLevel;
                lblPageTitle.Visible = !embedded;
                _lblNhanVien.Location = new Point(12, embedded ? 12 : 44);
                header.Height = embedded ? 40 : 76;
            };
            placeTitle();
            ParentChanged += (s, e) => placeTitle();

            // ================== THẺ PHẢI: GIỎ HÀNG & THÔNG TIN ==================
            var cartCard = PageKit.NewCard();
            cartCard.Dock = DockStyle.Right;
            cartCard.Width = 412;
            cartCard.Padding = new Padding(8 + 20, 4 + 16, 8 + 20, 4 + 8 + 16);
            // Kích thước ban đầu để các control neo đáy (Anchor Bottom) tính đúng khoảng cách
            var cartPanel = new Panel { Size = new Size(356, 570), BackColor = Color.White };

            // ================== THẺ TRÁI: THỰC ĐƠN ==================
            var menuCard = PageKit.NewCard();
            menuCard.Dock = DockStyle.Fill;
            menuCard.Padding = new Padding(8 + 20, 4 + 16, 8 + 14, 4 + 8 + 12);
            var menuPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            menuCard.Controls.Add(menuPanel);

            var headerPanel = new Panel { Dock = DockStyle.Top, Height = 92, BackColor = Color.White };
            menuPanel.Controls.Add(headerPanel);
            headerPanel.Controls.Add(new Label
            {
                Text = "Thực đơn món ăn",
                Location = new Point(0, 0),
                AutoSize = true,
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            });

            var filterFlow = new FlowLayoutPanel
            {
                Location = new Point(0, 36),
                Height = 44,
                Width = 900,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.White
            };
            headerPanel.Controls.Add(filterFlow);

            _txtSearch = new TextBox { Font = Font };
            _txtSearch.Text = "Tìm món...";
            _txtSearch.ForeColor = ThemeManager.Muted;
            _txtSearch.GotFocus += (s, e) => { if (_txtSearch.Text == "Tìm món...") { _txtSearch.Text = ""; _txtSearch.ForeColor = ThemeManager.Ink; } };
            _txtSearch.LostFocus += (s, e) => { if (_txtSearch.Text.Length == 0) { _txtSearch.Text = "Tìm món..."; _txtSearch.ForeColor = ThemeManager.Muted; } };
            _txtSearch.TextChanged += (s, e) => RenderProducts();
            var searchPill = new PillSearch(_txtSearch, null) { Width = 230, Margin = new Padding(0, 0, 10, 0) };
            _txtSearch.ForeColor = ThemeManager.Muted;
            filterFlow.Controls.Add(searchPill);

            _cboCategory = new ComboBox { Font = Font, DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
            _cboCategory.SelectedIndexChanged += (s, e) => RenderProducts();
            headerPanel.Controls.Add(_cboCategory);   // giữ trong form để DataSource hoạt động
            var catChip = new ComboChip(_cboCategory, "Loại") { Margin = new Padding(0, 0, 10, 0) };
            filterFlow.Controls.Add(catChip);

            _chkOnlyInStock = new CheckBox
            {
                Text = "Chỉ món còn hàng",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeManager.Ink2,
                Cursor = Cursors.Hand,
                Margin = new Padding(4, 10, 12, 0)
            };
            _chkOnlyInStock.CheckedChanged += (s, e) => RenderProducts();
            filterFlow.Controls.Add(_chkOnlyInStock);

            var btnRefresh = new Button { Text = "Làm mới" };
            ButtonSkin.SetKind(btnRefresh, ButtonSkin.Kind.Secondary);
            PageKit.FitButton(btnRefresh);
            btnRefresh.Margin = new Padding(0);
            btnRefresh.Click += async (s, e) => await InitData();
            filterFlow.Controls.Add(btnRefresh);

            _productsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White,
                Padding = new Padding(0, 4, 0, 0)
            };
            try
            {
                typeof(FlowLayoutPanel).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                    null, _productsPanel, new object[] { true });
            }
            catch { }
            menuPanel.Controls.Add(_productsPanel);
            _productsPanel.BringToFront();

            // ================== CỘT PHẢI: THÔNG TIN ĐƠN + GIỎ HÀNG ==================
            // Toàn bộ giao diện được dựng trong BuildCartColumn() (xem bên dưới constructor).
            cartPanel.Dock = DockStyle.Fill;
            cartPanel.Controls.Add(BuildCartColumn());
            cartCard.Controls.Add(cartPanel);

            Controls.Add(menuCard);
            Controls.Add(cartCard);
            Controls.Add(header);
            menuCard.BringToFront();

            this.Load += async (s, e) => {
                await InitData();
                _autoRefreshTimer.Start();
            };
        }

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
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 148F));   // thông tin khách
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
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));  // tiêu đề
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));  // caption
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));  // ô nhập
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));  // caption ghi chú
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));  // ô nhập ghi chú

            var title = new Label
            {
                Text = "Thông tin đơn mang đi",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                Margin = Padding.Empty
            };
            t.Controls.Add(title, 0, 0);
            t.SetColumnSpan(title, 2);

            t.Controls.Add(MakeCaption("Tên khách hàng", new Padding(0, 0, 6, 2)), 0, 1);
            t.Controls.Add(MakeCaption("Số điện thoại", new Padding(6, 0, 0, 2)), 1, 1);

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

            var lblGhiChu = MakeCaption("Ghi chú đơn hàng", new Padding(0, 0, 0, 2));
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
            // Chỉ hiện khi giỏ có ≥ 1 món (xem RenderCart). Có hộp thoại xác nhận trong ConfirmClearCart().
            _btnClearCart = new Button
            {
                Text = "🗑  Xóa tất cả",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.Right,               // canh giữa theo chiều dọc, dính mép phải
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
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 74F));   // 0: box voucher
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
                Text = "✓  Xác nhận & Tạo hóa đơn",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 10, 0, 0),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                UseMnemonic = false,                         // để dấu "&" hiển thị đúng
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

        // Box voucher: giữ nguyên các control & logic cũ (UpdatePromoBox / HandlePromoActionClick),
        // chỉ đổi cách bố trí: vị trí các control con tính theo bề rộng thực tế của box (co giãn được).
        private Control BuildPromoBox()
        {
            _pnlPromoBox = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 6, 0, 6),            // 74 - 12 = cao 62px
                Radius = 10,
                BorderWidth = 1,
                BorderColor = ThemeManager.Line,
                FillColor = ThemeManager.Surface2
            };

            _lblPromoIcon = new Label { Text = "✨", Location = new Point(10, 10), Size = new Size(24, 24), Font = new Font("Segoe UI", 12F), BackColor = Color.Transparent };
            _lblPromoTitle = new Label
            {
                Text = "Khuyến mãi & Voucher",
                Location = new Point(38, 8),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                BackColor = Color.Transparent,
                AutoEllipsis = true,
                UseMnemonic = false
            };
            _lblPromoSub = new Label
            {
                Text = "Chọn món để gợi ý ưu đãi tốt nhất",
                Location = new Point(38, 29),
                Size = new Size(200, 18),
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = ThemeManager.Muted,
                BackColor = Color.Transparent,
                AutoEllipsis = true,
                UseMnemonic = false
            };
            _btnPromoAction = new Button
            {
                Text = "Áp dụng",
                Size = new Size(96, 28),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            };
            ButtonSkin.SetKind(_btnPromoAction, ButtonSkin.Kind.Soft);
            _btnPromoAction.Click += (s, e) => HandlePromoActionClick();
            _lnkChangePromo = new Label
            {
                Text = "Đổi mã khác ▾",
                Size = new Size(96, 18),
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Primary,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.TopCenter,
                BackColor = Color.Transparent
            };
            _lnkChangePromo.Click += (s, e) => OpenSelectVoucherDialog();

            _pnlPromoBox.Controls.Add(_lblPromoIcon);
            _pnlPromoBox.Controls.Add(_lblPromoTitle);
            _pnlPromoBox.Controls.Add(_lblPromoSub);
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
            int rightX = w - 96 - 8;
            _btnPromoAction.Location = new Point(rightX, 8);
            _lnkChangePromo.Location = new Point(rightX, 38);
            int textW = Math.Max(60, rightX - 38 - 6);
            _lblPromoTitle.Width = textW;
            _lblPromoSub.Width = textW;
        }

        // ════════════════════════════════════════════════════════════════════
        //  CỘT PHẢI – THẺ MÓN TRONG GIỎ (Cart Item Card)
        //   Hàng 1: Tên món (đậm)                              [🗑 xóa món]
        //   Hàng 2: 140,000 đ / phần  (chữ nhỏ, xám)
        //   Hàng 3: [ − | 2 | + ]                         Thành tiền (đậm, xanh)
        // ════════════════════════════════════════════════════════════════════
        private Control CreateCartItemCard(CartItem item, int width)
        {
            bool atMax = item.Quantity >= item.MaxStock;

            var card = new RoundedPanel
            {
                Size = new Size(width, 96),
                Margin = new Padding(0, 0, 0, 8),
                Radius = 8,
                BorderWidth = 1,
                BorderColor = CartCardBorder,
                FillColor = CartCardBg
            };

            // ── Hàng 1: tên món + nút xóa món ──
            card.Controls.Add(new Label
            {
                Text = item.Name,
                Location = new Point(12, 8),
                Size = new Size(width - 12 - 44, 22),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                UseMnemonic = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                BackColor = CartCardBg
            });

            var btnDelete = new Button
            {
                Text = ThemeManager.Glyph.Delete,
                Location = new Point(width - 12 - 26, 6),
                Size = new Size(26, 26),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                Font = ThemeManager.Icon(10F),
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
            btnDelete.Click += (s, e) => { _cart.Remove(item); OnCartChanged(); };
            card.Controls.Add(btnDelete);

            // ── Hàng 2: đơn giá ──
            string unitPriceText = $"{item.Price:N0} đ / phần";
            if (item.Price < item.BasePrice)
                unitPriceText += $"  ·  Giá gốc {item.BasePrice:N0} đ";
            card.Controls.Add(new Label
            {
                Text = unitPriceText,
                Location = new Point(12, 32),
                Size = new Size(width - 24, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                UseMnemonic = false,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = ThemeManager.Muted,
                BackColor = CartCardBg
            });

            // ── Hàng 3: stepper (trái) + thành tiền (phải) ──
            var stepper = CreateStepper(item, atMax);
            stepper.Location = new Point(12, 56);
            stepper.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            card.Controls.Add(stepper);

            card.Controls.Add(new Label
            {
                Text = $"{item.Price * item.Quantity:N0} đ",
                Location = new Point(12 + stepper.Width + 8, 56),
                Size = new Size(Math.Max(60, width - (12 + stepper.Width + 8) - 12), 32),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleRight,
                AutoEllipsis = true,
                UseMnemonic = false,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = ThemeManager.Primary,
                BackColor = CartCardBg
            });

            return card;
        }

        // Stepper liền khối  [ − | 2 | + ]  – nền trắng, viền mảnh, mỗi nút 30x30px.
        // Viền ngoài + 2 vạch ngăn được vẽ bằng Paint của panel chứa → trông như 1 khối duy nhất.
        private Control CreateStepper(CartItem item, bool atMax)
        {
            var host = new Panel { Size = new Size(96, 32), BackColor = Color.White };
            host.Paint += (s, e) =>
            {
                using (var pen = new Pen(StepperBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, host.Width - 1, host.Height - 1);
                    e.Graphics.DrawLine(pen, 31, 1, 31, host.Height - 2);   // vạch giữa [−] và số lượng
                    e.Graphics.DrawLine(pen, 64, 1, 64, host.Height - 2);   // vạch giữa số lượng và [+]
                }
            };

            var btnMinus = new Button
            {
                Text = "−",
                Bounds = new Rectangle(1, 1, 30, 30),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink2,
                BackColor = Color.White,
                UseVisualStyleBackColor = false,
                Cursor = Cursors.Hand,
                TabStop = false,
                AccessibleName = "Giảm số lượng " + item.Name
            };
            btnMinus.FlatAppearance.BorderSize = 0;
            btnMinus.FlatAppearance.MouseOverBackColor = StepperHover;
            btnMinus.Click += (s, e) =>
            {
                if (item.Quantity > 1) item.Quantity--;
                else _cart.Remove(item);            // về 0 → gỡ món khỏi giỏ
                OnCartChanged();
            };

            var lblQty = new Label
            {
                Text = item.Quantity.ToString(),
                Bounds = new Rectangle(32, 1, 32, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                BackColor = Color.White,
                UseMnemonic = false
            };

            var btnPlus = new Button
            {
                Text = "+",
                Bounds = new Rectangle(65, 1, 30, 30),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = ThemeManager.Primary,
                BackColor = Color.White,
                UseVisualStyleBackColor = false,
                Cursor = atMax ? Cursors.No : Cursors.Hand,
                Enabled = !atMax,                   // đạt tồn kho tối đa → khóa nút +
                TabStop = false,
                AccessibleName = "Tăng số lượng " + item.Name
            };
            btnPlus.FlatAppearance.BorderSize = 0;
            btnPlus.FlatAppearance.MouseOverBackColor = StepperHover;
            btnPlus.Click += (s, e) =>
            {
                if (item.Quantity >= item.MaxStock)
                {
                    MessageBox.Show(this,
                        $"Món '{item.Name}' chỉ còn {item.MaxStock} suất trong kho.",
                        "Đã đạt số lượng tối đa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                item.Quantity++;
                OnCartChanged();
            };

            host.Controls.Add(btnMinus);
            host.Controls.Add(lblQty);
            host.Controls.Add(btnPlus);
            return host;
        }

        private void OnCartChanged()
        {
            RenderCart();
            UpdateProductsInPlace();
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

        private async Task RefreshStockContinuouslyAsync()
        {
            if (_isRefreshing || IsDisposed) return;
            // Nếu người dùng đang tập trung gõ tìm kiếm, không làm mới để tránh ngắt quãng thao tác
            if (_txtSearch.Focused) return;

            _isRefreshing = true;
            try
            {
                var list = await _monAnService.GetAllAsync();
                if (list == null || !list.Any() || IsDisposed) return;

                var available = list.Where(m => m.TrangThaiRaw != "Discontinued" && m.TrangThaiRaw != "Inactive").ToList();

                // Smart Refresh: Chỉ cập nhật khi tồn kho, giá hoặc trạng thái món thực sự thay đổi
                if (!HasMonAnStockChanged(_allMonAn, available))
                {
                    return; // Dữ liệu không đổi -> giữ nguyên 100% giao diện và thanh cuộn
                }

                _allMonAn = available;

                try
                {
                    _allVouchers = await _khuyenMaiService.GetVoucherListAsync();
                    _allKmMon = await _khuyenMaiService.GetKmMonListAsync();
                }
                catch { }

                bool cartAltered = false;
                string warningMsg = "";

                // Đồng bộ và kiểm tra ràng buộc kho với giỏ hàng
                for (int i = _cart.Count - 1; i >= 0; i--)
                {
                    var item = _cart[i];
                    var found = _allMonAn.FirstOrDefault(m => m.MaMonAn == item.MaMon);
                    int newStock = found != null ? found.SoLuongTon : 0;
                    item.MaxStock = newStock;

                    if (newStock <= 0)
                    {
                        _cart.RemoveAt(i);
                        cartAltered = true;
                        warningMsg += $"\n• Món '{item.Name}' vừa hết hàng trên hệ thống (đã gỡ khỏi giỏ).";
                    }
                    else if (item.Quantity > newStock)
                    {
                        item.Quantity = newStock;
                        cartAltered = true;
                        warningMsg += $"\n• Món '{item.Name}' chỉ còn {newStock} suất trong kho (đã điều chỉnh số lượng).";
                    }
                }

                // Cập nhật lại giao diện thực đơn tại chỗ (KHÔNG xóa controls, giữ nguyên 100% vị trí scroll)
                UpdateProductsInPlace();

                // Cập nhật lại giỏ hàng nếu số lượng bị thay đổi
                if (cartAltered)
                {
                    RenderCart();
                    if (!string.IsNullOrEmpty(warningMsg))
                    {
                        MessageBox.Show(this, "Thông báo đồng bộ tồn kho trong lúc bán:" + warningMsg, "Cập nhật tồn kho", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch
            {
                // Bỏ qua lỗi ngầm
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private bool HasMonAnStockChanged(List<MonAnDto> oldList, List<MonAnDto> newList)
        {
            if (oldList == null || newList == null) return true;
            if (oldList.Count != newList.Count) return true;
            for (int i = 0; i < oldList.Count; i++)
            {
                var a = oldList[i];
                var b = newList[i];
                if (a.MaMonAn != b.MaMonAn ||
                    a.SoLuongTon != b.SoLuongTon ||
                    a.Gia != b.Gia ||
                    a.TrangThaiRaw != b.TrangThaiRaw ||
                    a.TenMonAn != b.TenMonAn)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task InitData()
        {
            try
            {
                // Tải loại món
                _allLoaiMon = await _loaiMonService.GetAllAsync();
                _cboCategory.Items.Clear();
                _cboCategory.Items.Add("Tất cả loại món");
                foreach (var lm in _allLoaiMon)
                {
                    _cboCategory.Items.Add(lm.TenLoaiMon);
                }
                _cboCategory.SelectedIndex = 0;

                // Tải danh sách món ăn thực tế từ API (bao gồm số lượng tồn)
                var list = await _monAnService.GetAllAsync();
                // Lọc bỏ món đã ngừng kinh doanh
                _allMonAn = list.Where(m => m.TrangThaiRaw != "Discontinued" && m.TrangThaiRaw != "Inactive").ToList();

                // Tải danh sách khuyến mãi món và voucher hóa đơn
                _allVouchers = await _khuyenMaiService.GetVoucherListAsync();
                _allKmMon = await _khuyenMaiService.GetKmMonListAsync();

                // Cập nhật lại MaxStock trong giỏ hàng nếu món ăn đã thay đổi số lượng tồn
                foreach (var item in _cart)
                {
                    var found = _allMonAn.FirstOrDefault(m => m.MaMonAn == item.MaMon);
                    if (found != null)
                    {
                        item.MaxStock = found.SoLuongTon;
                    }
                }

                RenderProducts();
                RenderCart();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Không thể tải dữ liệu: " + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Lấy thông tin giá sau giảm của món nếu đang trong chương trình KM sản phẩm
        private (decimal effectivePrice, string promoText) GetDishPriceInfo(MonAnDto mon)
        {
            var promo = _allKmMon.FirstOrDefault(k => k.MaMon == mon.MaMonAn && k.TrangThai == "Đang áp dụng");
            if (promo != null)
            {
                decimal price = (decimal)promo.DonGiaSauGiam;
                return (price, promo.HinhThuc);
            }
            return (mon.Gia, null);
        }

        private void UpdateProductsInPlace()
        {
            var currentCards = _productsPanel.Controls.OfType<RoundedPanel>().ToList();
            if (!currentCards.Any())
            {
                RenderProducts();
                return;
            }

            string query = (_txtSearch.Text == "Tìm món..." ? "" : _txtSearch.Text).Trim().ToLowerInvariant();
            string selectedCatName = _cboCategory.SelectedItem?.ToString() ?? "Tất cả loại món";

            string selectedCatId = "";
            if (selectedCatName != "Tất cả loại món")
            {
                var foundCat = _allLoaiMon.FirstOrDefault(c => c.TenLoaiMon == selectedCatName);
                if (foundCat != null) selectedCatId = foundCat.MaLoaiMon;
            }

            var filtered = _allMonAn.Where(m => {
                bool matchName = string.IsNullOrEmpty(query) || (m.TenMonAn != null && m.TenMonAn.ToLowerInvariant().Contains(query));
                bool matchCat = string.IsNullOrEmpty(selectedCatId) || m.MaLoaiMon == selectedCatId;
                bool matchStock = !_chkOnlyInStock.Checked || m.SoLuongTon > 0;
                return matchName && matchCat && matchStock;
            }).ToList();

            var currentCardIds = new HashSet<string>(currentCards.Select(c => c.Tag as string).Where(id => !string.IsNullOrEmpty(id)));
            var filteredIds = new HashSet<string>(filtered.Select(m => m.MaMonAn));

            // Nếu tập hợp món ăn bị thay đổi (có món mới thêm vào hoặc bị lọc mất) => Gọi RenderProducts()
            if (!currentCardIds.SetEquals(filteredIds))
            {
                RenderProducts();
                return;
            }

            // Cập nhật tại chỗ: Giữ nguyên 100% controls và vị trí thanh cuộn
            _productsPanel.SuspendLayout();
            foreach (var card in currentCards)
            {
                string maMon = card.Tag as string;
                var mon = filtered.FirstOrDefault(m => m.MaMonAn == maMon);
                if (mon != null)
                {
                    UpdateDishCardState(card, mon);
                }
            }
            _productsPanel.ResumeLayout();
        }

        private void UpdateDishCardState(RoundedPanel card, MonAnDto mon)
        {
            int inCartQty = _cart.FirstOrDefault(x => x.MaMon == mon.MaMonAn)?.Quantity ?? 0;
            int virtualRemaining = Math.Max(0, mon.SoLuongTon - inCartQty);
            bool canAddMore = virtualRemaining > 0;
            bool rawInStock = mon.SoLuongTon > 0;

            var (effPrice, promoText) = GetDishPriceInfo(mon);
            bool hasPromo = promoText != null;

            card.BorderColor = rawInStock ? (hasPromo ? Color.FromArgb(254, 202, 202) : ThemeManager.Line) : Color.FromArgb(241, 245, 249);
            card.FillColor = rawInStock ? Color.White : Color.FromArgb(248, 250, 252);
            card.Cursor = canAddMore ? Cursors.Hand : Cursors.Default;

            var lblStock = card.Controls["lblStock"] as Label;
            if (lblStock != null)
            {
                if (!rawInStock)
                {
                    lblStock.Text = "● Hết món";
                    lblStock.ForeColor = ThemeManager.Red;
                    lblStock.BackColor = ThemeManager.RedSoft;
                }
                else if (virtualRemaining == 0)
                {
                    lblStock.Text = inCartQty > 0 ? $"● Hết suất (Đã lấy {inCartQty})" : "● Hết món";
                    lblStock.ForeColor = ThemeManager.Red;
                    lblStock.BackColor = ThemeManager.RedSoft;
                }
                else if (virtualRemaining <= 5)
                {
                    lblStock.Text = inCartQty > 0 ? $"● Còn: {virtualRemaining} (giỏ: {inCartQty})" : $"● Còn: {virtualRemaining}";
                    lblStock.ForeColor = ThemeManager.Amber;
                    lblStock.BackColor = ThemeManager.AmberSoft;
                }
                else
                {
                    lblStock.Text = inCartQty > 0 ? $"● Còn: {virtualRemaining} (giỏ: {inCartQty})" : $"● Còn: {virtualRemaining}";
                    lblStock.ForeColor = ThemeManager.Green;
                    lblStock.BackColor = ThemeManager.GreenSoft;
                }
            }

            var btnAdd = card.Controls["btnAdd"] as Button;
            if (btnAdd != null)
            {
                if (canAddMore)
                {
                    btnAdd.Text = "+ Thêm";
                    btnAdd.BackColor = _accent;
                    btnAdd.ForeColor = Color.White;
                    btnAdd.Enabled = true;
                    btnAdd.Cursor = Cursors.Hand;
                }
                else
                {
                    btnAdd.Text = inCartQty > 0 ? "Đầy" : "Hết";
                    btnAdd.BackColor = Color.FromArgb(226, 232, 240);
                    btnAdd.ForeColor = ThemeManager.Muted;
                    btnAdd.Enabled = false;
                    btnAdd.Cursor = Cursors.No;
                }
            }
        }

        private void RenderProducts()
        {
            Point scrollPos = new Point(Math.Abs(_productsPanel.AutoScrollPosition.X), Math.Abs(_productsPanel.AutoScrollPosition.Y));
            _productsPanel.SuspendLayout();
            _productsPanel.Controls.Clear();
            string query = (_txtSearch.Text == "Tìm món..." ? "" : _txtSearch.Text).Trim().ToLowerInvariant();
            string selectedCatName = _cboCategory.SelectedItem?.ToString() ?? "Tất cả loại món";

            string selectedCatId = "";
            if (selectedCatName != "Tất cả loại món")
            {
                var foundCat = _allLoaiMon.FirstOrDefault(c => c.TenLoaiMon == selectedCatName);
                if (foundCat != null) selectedCatId = foundCat.MaLoaiMon;
            }

            var filtered = _allMonAn.Where(m => {
                bool matchName = string.IsNullOrEmpty(query) || (m.TenMonAn != null && m.TenMonAn.ToLowerInvariant().Contains(query));
                bool matchCat = string.IsNullOrEmpty(selectedCatId) || m.MaLoaiMon == selectedCatId;
                bool matchStock = !_chkOnlyInStock.Checked || m.SoLuongTon > 0;
                return matchName && matchCat && matchStock;
            }).ToList();

            if (!filtered.Any())
            {
                _productsPanel.Controls.Add(new Label
                {
                    Text = "Không có món ăn nào phù hợp.",
                    ForeColor = ThemeManager.Muted,
                    AutoSize = true,
                    Margin = new Padding(12)
                });
                _productsPanel.ResumeLayout();
                return;
            }

            foreach (var mon in filtered)
            {
                var card = CreateDishCard(mon);
                _productsPanel.Controls.Add(card);
            }
            _productsPanel.ResumeLayout(true);
            _productsPanel.PerformLayout();
            try { _productsPanel.AutoScrollPosition = scrollPos; } catch { }
        }

        private Control CreateDishCard(MonAnDto mon)
        {
            int inCartQty = _cart.FirstOrDefault(x => x.MaMon == mon.MaMonAn)?.Quantity ?? 0;
            int virtualRemaining = Math.Max(0, mon.SoLuongTon - inCartQty);
            bool canAddMore = virtualRemaining > 0;
            bool rawInStock = mon.SoLuongTon > 0;

            var (effPrice, promoText) = GetDishPriceInfo(mon);
            bool hasPromo = promoText != null;

            var card = new RoundedPanel
            {
                Name = "card_" + mon.MaMonAn,
                Tag = mon.MaMonAn,
                Size = new Size(182, 114),
                Margin = new Padding(6),
                Radius = 10,
                BorderWidth = 1,
                BorderColor = rawInStock ? (hasPromo ? Color.FromArgb(254, 202, 202) : ThemeManager.Line) : Color.FromArgb(241, 245, 249),
                FillColor = rawInStock ? Color.White : Color.FromArgb(248, 250, 252),
                Cursor = canAddMore ? Cursors.Hand : Cursors.Default
            };

            var lblTen = new Label
            {
                Name = "lblTen",
                Text = mon.TenMonAn,
                Location = new Point(8, 6),
                Size = new Size(hasPromo ? 110 : 162, 34),
                AutoEllipsis = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = rawInStock ? ThemeManager.Ink : ThemeManager.Muted,
                BackColor = Color.Transparent,
                Cursor = canAddMore ? Cursors.Hand : Cursors.Default
            };
            card.Controls.Add(lblTen);

            // Badge sale nếu món có KM
            if (hasPromo)
            {
                var lblSaleBadge = new Label
                {
                    Text = $"🔥 {promoText}",
                    Location = new Point(116, 6),
                    Size = new Size(58, 20),
                    Font = new Font("Segoe UI", 7F, FontStyle.Bold),
                    ForeColor = ThemeManager.Red,
                    BackColor = ThemeManager.RedSoft,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                card.Controls.Add(lblSaleBadge);
            }

            var lblGia = new Label
            {
                Name = "lblGia",
                Text = $"{effPrice:N0} đ",
                Location = new Point(8, 44),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = rawInStock ? (hasPromo ? ThemeManager.Red : _accent) : ThemeManager.Muted,
                BackColor = Color.Transparent,
                Cursor = canAddMore ? Cursors.Hand : Cursors.Default
            };
            card.Controls.Add(lblGia);

            if (hasPromo && effPrice < mon.Gia)
            {
                var lblGiaGoc = new Label
                {
                    Text = $"{mon.Gia:N0} đ",
                    Location = new Point(lblGia.PreferredWidth + 12, 46),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 8F, FontStyle.Strikeout),
                    ForeColor = ThemeManager.Muted,
                    BackColor = Color.Transparent
                };
                card.Controls.Add(lblGiaGoc);
            }

            // Badge hiển thị số lượng còn (ảo: trừ theo số lượng đang có trong giỏ hàng)
            var lblStock = new Label
            {
                Name = "lblStock",
                Location = new Point(8, 72),
                AutoSize = true,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Padding = new Padding(3, 2, 3, 2)
            };

            if (!rawInStock)
            {
                lblStock.Text = "● Hết món";
                lblStock.ForeColor = ThemeManager.Red;
                lblStock.BackColor = ThemeManager.RedSoft;
            }
            else if (virtualRemaining == 0)
            {
                lblStock.Text = inCartQty > 0 ? $"● Hết suất (Đã lấy {inCartQty})" : "● Hết món";
                lblStock.ForeColor = ThemeManager.Red;
                lblStock.BackColor = ThemeManager.RedSoft;
            }
            else if (virtualRemaining <= 5)
            {
                lblStock.Text = inCartQty > 0 ? $"● Còn: {virtualRemaining} (giỏ: {inCartQty})" : $"● Còn: {virtualRemaining}";
                lblStock.ForeColor = ThemeManager.Amber;
                lblStock.BackColor = ThemeManager.AmberSoft;
            }
            else
            {
                lblStock.Text = inCartQty > 0 ? $"● Còn: {virtualRemaining} (giỏ: {inCartQty})" : $"● Còn: {virtualRemaining}";
                lblStock.ForeColor = ThemeManager.Green;
                lblStock.BackColor = ThemeManager.GreenSoft;
            }
            card.Controls.Add(lblStock);

            // Nút Thêm vào đơn
            var btnAdd = new Button
            {
                Name = "btnAdd",
                Location = new Point(112, 68),
                Size = new Size(60, 30),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            btnAdd.FlatAppearance.BorderSize = 0;

            if (canAddMore)
            {
                btnAdd.Text = "+ Thêm";
                btnAdd.BackColor = _accent;
                btnAdd.ForeColor = Color.White;
                btnAdd.Cursor = Cursors.Hand;
                btnAdd.Click += (s, e) => AddItem(mon);
            }
            else
            {
                btnAdd.Text = inCartQty > 0 ? "Đầy" : "Hết";
                btnAdd.BackColor = Color.FromArgb(226, 232, 240);
                btnAdd.ForeColor = ThemeManager.Muted;
                btnAdd.Enabled = false;
                btnAdd.Cursor = Cursors.No;
            }
            card.Controls.Add(btnAdd);

            Action onCardClick = () => {
                var currentMon = _allMonAn.FirstOrDefault(m => m.MaMonAn == mon.MaMonAn) ?? mon;
                int currentInCartQty = _cart.FirstOrDefault(x => x.MaMon == currentMon.MaMonAn)?.Quantity ?? 0;
                int currentVirtualRemaining = Math.Max(0, currentMon.SoLuongTon - currentInCartQty);
                bool currentCanAddMore = currentVirtualRemaining > 0;
                bool currentRawInStock = currentMon.SoLuongTon > 0;

                if (currentCanAddMore)
                {
                    AddItem(currentMon);
                }
                else if (!currentRawInStock)
                {
                    MessageBox.Show(this,
                        $"Món '{currentMon.TenMonAn}' hiện đã hết hàng trong kho (số lượng còn: 0).",
                        "Hết hàng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show(this,
                        $"Món '{currentMon.TenMonAn}' chỉ còn {currentMon.SoLuongTon} suất trong kho và bạn đã thêm tối đa ({currentInCartQty} suất) vào giỏ hàng.",
                        "Đã đạt số lượng tối đa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            card.Click += (s, e) => onCardClick();
            lblTen.Click += (s, e) => onCardClick();
            lblGia.Click += (s, e) => onCardClick();

            if (canAddMore)
            {
                card.MouseEnter += (s, e) => {
                    card.BorderColor = _accent;
                };
                card.MouseLeave += (s, e) => {
                    card.BorderColor = hasPromo ? Color.FromArgb(254, 202, 202) : ThemeManager.Line;
                };
            }

            return card;
        }

        private void AddItem(MonAnDto mon)
        {
            if (mon.SoLuongTon <= 0)
            {
                MessageBox.Show(this,
                    $"Món '{mon.TenMonAn}' hiện đã hết hàng trong kho.",
                    "Hết hàng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var (effPrice, promoText) = GetDishPriceInfo(mon);

            var item = _cart.FirstOrDefault(x => x.MaMon == mon.MaMonAn);
            if (item == null)
            {
                _cart.Add(new CartItem
                {
                    MaMon = mon.MaMonAn,
                    Name = mon.TenMonAn,
                    BasePrice = mon.Gia,
                    Price = effPrice,
                    Quantity = 1,
                    MaxStock = mon.SoLuongTon,
                    PromoInfo = promoText
                });
            }
            else
            {
                if (item.Quantity >= mon.SoLuongTon)
                {
                    MessageBox.Show(this,
                        $"Món '{mon.TenMonAn}' trong kho chỉ còn {mon.SoLuongTon} suất.\nKhông thể đặt thêm vượt quá số lượng còn lại.",
                        "Đã đạt giới hạn số lượng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                item.Quantity++;
            }
            RenderCart();
            UpdateProductsInPlace();
        }

        // ================== LOGIC TÍNH TOÁN SMART BEST DEAL VOUCHER ==================
        private (bool isEligible, decimal discount, decimal missingAmount) EvaluateVoucher(VoucherDto v, decimal subTotal)
        {
            if (v == null || subTotal <= 0) return (false, 0m, 0m);

            decimal minOrder = (decimal)(v.GiaTriDonToiThieu ?? 0);
            if (subTotal < minOrder)
            {
                return (false, 0m, minOrder - subTotal);
            }

            decimal discount = 0m;
            if (v.PhanTramGiam.HasValue && v.PhanTramGiam.Value > 0)
            {
                discount = subTotal * (decimal)v.PhanTramGiam.Value / 100m;
                if (v.GiamToiDa.HasValue && v.GiamToiDa.Value > 0 && discount > (decimal)v.GiamToiDa.Value)
                {
                    discount = (decimal)v.GiamToiDa.Value;
                }
            }
            else if (v.TienGiam.HasValue && v.TienGiam.Value > 0)
            {
                discount = (decimal)v.TienGiam.Value;
            }

            if (discount > subTotal) discount = subTotal;
            return (true, discount, 0m);
        }

        private (VoucherDto bestVoucher, decimal bestDiscount, VoucherDto nearVoucher, decimal missingForNear) FindBestVoucher(decimal subTotal)
        {
            var activeVouchers = _allVouchers.Where(v => v.TrangThai == "Đang áp dụng").ToList();
            VoucherDto best = null;
            decimal maxDiscount = 0m;

            VoucherDto near = null;
            decimal minMissing = decimal.MaxValue;

            foreach (var v in activeVouchers)
            {
                var (isEligible, discount, missing) = EvaluateVoucher(v, subTotal);
                if (isEligible)
                {
                    if (discount > maxDiscount)
                    {
                        maxDiscount = discount;
                        best = v;
                    }
                }
                else if (missing > 0 && missing < minMissing)
                {
                    minMissing = missing;
                    near = v;
                }
            }

            return (best, maxDiscount, near, minMissing);
        }

        private void ConfirmClearCart()
        {
            if (_cart.Count == 0) return;

            // Hộp thoại xác nhận – mặc định chọn "Không" để tránh lỡ tay
            var confirm = MessageBox.Show(
                this,
                $"Xóa toàn bộ {_cart.Count} món đã chọn khỏi giỏ hàng?",
                "Xác nhận xóa giỏ hàng",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (confirm != DialogResult.Yes) return;

            _cart.Clear();
            _appliedVoucher = null;
            RenderCart();
            UpdateProductsInPlace();
        }

        // Vẽ lại danh sách món trong vùng cuộn (giữ nguyên vị trí cuộn; có món mới thì cuộn tới món đó)
        private void RenderCartLines()
        {
            int scrollY = Math.Abs(_cartLines.AutoScrollPosition.Y);
            bool itemAdded = _cart.Count > _lastRenderedLineCount;

            _cartLines.SuspendLayout();

            // Gỡ control cũ ngay, nhưng Dispose trễ một nhịp (tránh hủy nút đang xử lý sự kiện Click của chính nó)
            var old = _cartLines.Controls.Cast<Control>().ToList();
            _cartLines.Controls.Clear();
            if (IsHandleCreated) BeginInvoke(new Action(() => { foreach (var c in old) c.Dispose(); }));
            else foreach (var c in old) c.Dispose();

            int rowWidth = Math.Max(240, _cartLines.ClientSize.Width - _cartLines.Padding.Horizontal);

            if (_cart.Count == 0)
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
                foreach (var item in _cart)
                    _cartLines.Controls.Add(CreateCartItemCard(item, rowWidth));
            }

            _cartLines.ResumeLayout(true);
            FitCartRowWidths();

            if (itemAdded && _cartLines.Controls.Count > 0)
                _cartLines.ScrollControlIntoView(_cartLines.Controls[_cartLines.Controls.Count - 1]);
            else
            {
                try { _cartLines.AutoScrollPosition = new Point(0, scrollY); } catch { }
            }
            _lastRenderedLineCount = _cart.Count;
        }

        private void RenderCart()
        {
            // ── Header: "Món đã chọn (n)" + nút Xóa tất cả (chỉ hiện khi có món) ──
            // n = tổng số phần. Muốn đếm số dòng món khác nhau thì đổi thành: _cart.Count
            int itemCount = _cart.Sum(x => x.Quantity);
            _lblCartCount.Text = $"Món đã chọn ({itemCount})";
            _btnClearCart.Visible = _cart.Count > 0;

            // ── Danh sách món ──
            RenderCartLines();

            decimal subTotal = _cart.Sum(x => x.Price * x.Quantity);

            // ================== ÁP DỤNG SMART BEST DEAL ==================
            if (_autoApplyBestDeal)
            {
                var (bestVoucher, _, _, _) = FindBestVoucher(subTotal);
                _appliedVoucher = bestVoucher;   // null nếu không có mã phù hợp
            }
            else if (_appliedVoucher != null)
            {
                // Voucher chọn thủ công: kiểm tra còn hợp lệ không
                var (isEligible, _, _) = EvaluateVoucher(_appliedVoucher, subTotal);
                if (!isEligible) _appliedVoucher = null;
            }

            // Tiền giảm của voucher
            decimal voucherDiscount = 0m;
            if (_appliedVoucher != null)
            {
                var (isEligible, disc, _) = EvaluateVoucher(_appliedVoucher, subTotal);
                if (isEligible) voucherDiscount = disc;
            }

            decimal finalTotal = Math.Max(0, subTotal - voucherDiscount);

            // Box gợi ý voucher
            UpdatePromoBox(subTotal);

            // ── Bảng tổng kết ──
            _lblSubTotalValue.Text = $"{subTotal:N0} đ";

            bool hasDiscount = _appliedVoucher != null && voucherDiscount > 0;
            _lblDiscountCaption.Visible = hasDiscount;
            _lblDiscountValue.Visible = hasDiscount;
            if (hasDiscount)
            {
                _lblDiscountCaption.Text = $"Giảm giá ({_appliedVoucher.MaVoucher})";
                _lblDiscountValue.Text = $"-{voucherDiscount:N0} đ";
            }

            _lblTotalValue.Text = $"{finalTotal:N0} đ";
        }

        private void UpdatePromoBox(decimal subTotal)
        {
            if (subTotal <= 0)
            {
                _lblPromoIcon.Text = "🎟️";
                _lblPromoTitle.Text = "Khuyến mãi & Voucher";
                _lblPromoTitle.ForeColor = ThemeManager.Ink;
                _lblPromoSub.Text = "Chọn món để gợi ý ưu đãi tốt nhất";
                _btnPromoAction.Visible = false;
                _lnkChangePromo.Text = "Xem tất cả mã ▾";
                _pnlPromoBox.FillColor = Color.FromArgb(248, 250, 252);
                _pnlPromoBox.BorderColor = Color.FromArgb(226, 232, 240);
                return;
            }

            if (_appliedVoucher != null)
            {
                var (_, disc, _) = EvaluateVoucher(_appliedVoucher, subTotal);
                _lblPromoIcon.Text = "✔";
                _lblPromoTitle.Text = $"Áp dụng: [{_appliedVoucher.MaVoucher}]";
                _lblPromoTitle.ForeColor = Color.FromArgb(22, 163, 74);
                _lblPromoSub.Text = $"Tiết kiệm: {disc:N0} đ ({_appliedVoucher.TenChuongTrinh})";
                _btnPromoAction.Visible = true;
                _btnPromoAction.Text = "Bỏ mã";
                _btnPromoAction.BackColor = Color.FromArgb(254, 242, 242);
                _btnPromoAction.ForeColor = ThemeManager.Red;
                _lnkChangePromo.Text = "Đổi mã khác ▾";
                _pnlPromoBox.FillColor = Color.FromArgb(240, 253, 244);
                _pnlPromoBox.BorderColor = Color.FromArgb(187, 247, 208);
                return;
            }

            var (best, bestDisc, near, missing) = FindBestVoucher(subTotal);
            if (best != null)
            {
                _lblPromoIcon.Text = "✨";
                _lblPromoTitle.Text = $"Gợi ý: [{best.MaVoucher}]";
                _lblPromoTitle.ForeColor = Color.FromArgb(79, 70, 229);
                _lblPromoSub.Text = $"Tiết kiệm: {bestDisc:N0} đ ({best.HinhThuc})";
                _btnPromoAction.Visible = true;
                _btnPromoAction.Text = "Áp dụng";
                _btnPromoAction.BackColor = _accent;
                _btnPromoAction.ForeColor = Color.White;
                _lnkChangePromo.Text = "Đổi mã khác ▾";
                _pnlPromoBox.FillColor = Color.FromArgb(238, 242, 255);
                _pnlPromoBox.BorderColor = Color.FromArgb(199, 210, 254);
            }
            else if (near != null)
            {
                _lblPromoIcon.Text = "💡";
                _lblPromoTitle.Text = $"Mua thêm {missing:N0} đ";
                _lblPromoTitle.ForeColor = ThemeManager.Amber;
                _lblPromoSub.Text = $"Để nhận mã [{near.MaVoucher}] ({near.HinhThuc})";
                _btnPromoAction.Visible = false;
                _lnkChangePromo.Text = "Xem tất cả mã ▾";
                _pnlPromoBox.FillColor = Color.FromArgb(255, 251, 235);
                _pnlPromoBox.BorderColor = Color.FromArgb(254, 243, 199);
            }
            else
            {
                _lblPromoIcon.Text = "🎟️";
                _lblPromoTitle.Text = "Chưa có mã phù hợp";
                _lblPromoTitle.ForeColor = ThemeManager.Muted;
                _lblPromoSub.Text = "Bấm để chọn hoặc nhập mã";
                _btnPromoAction.Visible = false;
                _lnkChangePromo.Text = "Chọn mã ▾";
                _pnlPromoBox.FillColor = Color.FromArgb(248, 250, 252);
                _pnlPromoBox.BorderColor = Color.FromArgb(226, 232, 240);
            }
        }

        private void HandlePromoActionClick()
        {
            decimal subTotal = _cart.Sum(x => x.Price * x.Quantity);
            if (_appliedVoucher != null)
            {
                // Bỏ áp dụng voucher
                _appliedVoucher = null;
                _autoApplyBestDeal = false;
                RenderCart();
            }
            else
            {
                // Áp dụng Best Deal
                var (best, _, _, _) = FindBestVoucher(subTotal);
                if (best != null)
                {
                    _appliedVoucher = best;
                    _autoApplyBestDeal = true;
                    RenderCart();
                }
            }
        }

        private void OpenSelectVoucherDialog()
        {
            decimal subTotal = _cart.Sum(x => x.Price * x.Quantity);
            using (var dlg = new ChonVoucherDialog(subTotal, _allVouchers, _appliedVoucher))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _appliedVoucher = dlg.SelectedVoucher;
                    _autoApplyBestDeal = (_appliedVoucher == null); // Nếu người dùng chủ động chọn, không auto ghi đè
                    RenderCart();
                }
            }
        }

        private async Task Checkout()
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show(this, "Vui lòng chọn ít nhất một món ăn vào giỏ hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra ràng buộc tồn kho trước khi gửi lên API
            foreach (var item in _cart)
            {
                if (item.Quantity > item.MaxStock)
                {
                    MessageBox.Show(this,
                        $"Món '{item.Name}' đang đặt {item.Quantity} suất nhưng trong kho chỉ còn {item.MaxStock} suất.\nVui lòng điều chỉnh lại số lượng trước khi xác nhận.",
                        "Vượt quá số lượng còn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            string staffId = UserSession.CurrentUserId ?? UserSession.CurrentUsername;
            string staffName = UserSession.CurrentDisplayName ?? UserSession.CurrentUsername ?? "Nhân viên";

            decimal subTotal = _cart.Sum(x => x.Price * x.Quantity);
            decimal voucherDiscount = 0m;
            if (_appliedVoucher != null)
            {
                var (isEligible, disc, _) = EvaluateVoucher(_appliedVoucher, subTotal);
                if (isEligible) voucherDiscount = disc;
            }
            decimal finalTotal = Math.Max(0, subTotal - voucherDiscount);

            var req = new CreateOrderRequestDto
            {
                TenKhachHang = string.IsNullOrWhiteSpace(_txtTenKhach.Text) ? "Khách mua mang đi" : _txtTenKhach.Text.Trim(),
                SoDienThoai = _txtSdt.Text.Trim(),
                DiaChiGiao = string.IsNullOrWhiteSpace(_txtGhiChu.Text) ? "Bán mang đi tại quầy" : _txtGhiChu.Text.Trim(),
                PhuongThucThanhToan = "Tiền mặt (Tại quầy)",
                MaNhanVien = staffId,
                MaKmvoucher = _appliedVoucher?.MaVoucher ?? _appliedVoucher?.MaKmvoucher,
                Items = _cart.Select(x => new CreateOrderItemDto
                {
                    MaMon = x.MaMon,
                    SoLuong = x.Quantity
                }).ToList()
            };

            _btnPay.Enabled = false;
            try
            {
                var (success, message, maHoaDon) = await _hoaDonService.CreateOrderAsync(req);
                if (success)
                {
                    string voucherMsg = _appliedVoucher != null ? $"Ưu đãi voucher ({_appliedVoucher.MaVoucher}): -{voucherDiscount:N0} đ\n" : "";
                    var confirm = MessageBox.Show(this,
                        $"🎉 Tạo hóa đơn mang đi thành công!\n\n" +
                        $"Mã hóa đơn: #{maHoaDon}\n" +
                        $"Nhân viên lập đơn: {staffName}\n" +
                        $"Tiền món: {subTotal:N0} đ\n" +
                        voucherMsg +
                        $"Tổng thanh toán: {finalTotal:N0} đ\n\n" +
                        $"Bạn có muốn mở xem chi tiết hóa đơn vừa tạo không?",
                        "Tạo đơn thành công", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    _cart.Clear();
                    _appliedVoucher = null;
                    _autoApplyBestDeal = true;
                    RenderCart();

                    // Cập nhật lại số lượng còn thực tế sau khi CSDL đã trừ tồn kho
                    await InitData();

                    if (confirm == DialogResult.Yes && !string.IsNullOrEmpty(maHoaDon))
                    {
                        using (var dialog = new HoaDonDialog(maHoaDon))
                        {
                            dialog.ShowDialog(this);
                        }
                    }
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show(this, message, "Lỗi tạo hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                _btnPay.Enabled = true;
            }
        }
    }

    // ==============================================================
    // DIALOG CHỌN HOẶC NHẬP MÃ VOUCHER THỦ CÔNG
    // ==============================================================
    public class ChonVoucherDialog : Form
    {
        public VoucherDto SelectedVoucher { get; private set; }

        public ChonVoucherDialog(decimal currentSubTotal, List<VoucherDto> vouchers, VoucherDto currentApplied)
        {
            Text = "Chọn Voucher & Khuyến Mãi Hóa Đơn";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(480, 560);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);

            SelectedVoucher = currentApplied;

            var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), BackColor = ThemeManager.Bg };
            Controls.Add(root);

            var lblHeader = new Label
            {
                Text = "🎟️ Danh Sách Ưu Đãi & Voucher",
                Location = new Point(0, 0),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            };
            root.Controls.Add(lblHeader);

            var lblSub = new Label
            {
                Text = $"Tổng tiền món hiện tại: {currentSubTotal:N0} đ",
                Location = new Point(2, 28),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(14, 124, 134)
            };
            root.Controls.Add(lblSub);

            // Khung nhập mã thủ công
            var pnlInput = new Panel
            {
                Location = new Point(0, 56),
                Size = new Size(424, 38),
                BackColor = Color.White
            };
            root.Controls.Add(pnlInput);

            var txtCode = new TextBox
            {
                Location = new Point(8, 7),
                Width = 280,
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeManager.Muted,
                Text = "Nhập mã voucher (VD: GIAM20K)..."
            };
            txtCode.GotFocus += (s, e) => {
                if (txtCode.Text == "Nhập mã voucher (VD: GIAM20K)...")
                {
                    txtCode.Text = "";
                    txtCode.ForeColor = ThemeManager.Ink;
                }
            };
            txtCode.LostFocus += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtCode.Text))
                {
                    txtCode.Text = "Nhập mã voucher (VD: GIAM20K)...";
                    txtCode.ForeColor = ThemeManager.Muted;
                }
            };
            pnlInput.Controls.Add(txtCode);

            var btnApplyCode = new Button
            {
                Text = "Áp dụng",
                Location = new Point(296, 4),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(14, 124, 134),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnApplyCode.FlatAppearance.BorderSize = 0;
            btnApplyCode.Click += (s, e) => {
                string input = (txtCode.Text == "Nhập mã voucher (VD: GIAM20K)..." ? "" : txtCode.Text).Trim();
                if (string.IsNullOrEmpty(input))
                {
                    MessageBox.Show(this, "Vui lòng nhập mã voucher.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var found = vouchers.FirstOrDefault(v => string.Equals(v.MaVoucher, input, StringComparison.OrdinalIgnoreCase)
                                                      || string.Equals(v.MaKmvoucher, input, StringComparison.OrdinalIgnoreCase));
                if (found == null)
                {
                    MessageBox.Show(this, $"Mã voucher '{input}' không tồn tại hoặc đã hết hạn.", "Mã không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (found.TrangThai != "Đang áp dụng")
                {
                    MessageBox.Show(this, $"Mã voucher '{found.MaVoucher}' hiện ở trạng thái: {found.TrangThai}.", "Chưa thể áp dụng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                decimal minOrder = (decimal)(found.GiaTriDonToiThieu ?? 0);
                if (currentSubTotal < minOrder)
                {
                    MessageBox.Show(this, $"Mã '{found.MaVoucher}' yêu cầu đơn tối thiểu {minOrder:N0} đ. Hiện tại giỏ hàng mới đạt {currentSubTotal:N0} đ.", "Chưa đủ điều kiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                SelectedVoucher = found;
                DialogResult = DialogResult.OK;
                Close();
            };
            pnlInput.Controls.Add(btnApplyCode);

            // Danh sách các voucher khả dụng
            var flow = new FlowLayoutPanel
            {
                Location = new Point(0, 104),
                Size = new Size(424, 320),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = ThemeManager.Bg
            };
            root.Controls.Add(flow);

            var activeList = vouchers.Where(v => v.TrangThai == "Đang áp dụng").ToList();
            if (!activeList.Any())
            {
                flow.Controls.Add(new Label
                {
                    Text = "Hiện không có voucher nào đang diễn ra.",
                    ForeColor = ThemeManager.Muted,
                    Margin = new Padding(10),
                    AutoSize = true
                });
            }
            else
            {
                foreach (var v in activeList)
                {
                    decimal minOrder = (decimal)(v.GiaTriDonToiThieu ?? 0);
                    bool isEligible = currentSubTotal >= minOrder;
                    bool isCurrent = currentApplied != null && (currentApplied.MaVoucher == v.MaVoucher || currentApplied.MaKmvoucher == v.MaKmvoucher);

                    var card = new RoundedPanel
                    {
                        Size = new Size(398, 76),
                        Margin = new Padding(0, 0, 0, 8),
                        Radius = 8,
                        BorderWidth = 1,
                        BorderColor = isCurrent ? Color.FromArgb(14, 124, 134) : isEligible ? Color.FromArgb(203, 213, 225) : Color.FromArgb(226, 232, 240),
                        FillColor = isCurrent ? Color.FromArgb(240, 253, 250) : Color.White
                    };

                    var lblCode = new Label
                    {
                        Text = $"🎟️ {v.MaVoucher}",
                        Location = new Point(10, 8),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(29, 78, 216)
                    };
                    card.Controls.Add(lblCode);

                    var lblDiscountDesc = new Label
                    {
                        Text = v.HinhThuc + (v.GiamToiDa.HasValue && v.GiamToiDa > 0 ? $" (Tối đa {v.GiamToiDa:N0} đ)" : ""),
                        Location = new Point(10, 30),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(22, 163, 74)
                    };
                    card.Controls.Add(lblDiscountDesc);

                    var lblCondition = new Label
                    {
                        Text = minOrder > 0 ? $"Đơn tối thiểu: {minOrder:N0} đ" : "Áp dụng cho mọi giá trị đơn",
                        Location = new Point(10, 50),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 8F),
                        ForeColor = ThemeManager.Muted
                    };
                    card.Controls.Add(lblCondition);

                    if (isEligible)
                    {
                        var btnSelect = new Button
                        {
                            Text = isCurrent ? "Đang chọn" : "Chọn mã",
                            Location = new Point(292, 22),
                            Size = new Size(94, 32),
                            BackColor = isCurrent ? Color.FromArgb(22, 163, 74) : Color.FromArgb(14, 124, 134),
                            ForeColor = Color.White,
                            FlatStyle = FlatStyle.Flat,
                            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                            Cursor = Cursors.Hand
                        };
                        btnSelect.FlatAppearance.BorderSize = 0;
                        btnSelect.Click += (s, e) => {
                            SelectedVoucher = v;
                            DialogResult = DialogResult.OK;
                            Close();
                        };
                        card.Controls.Add(btnSelect);
                    }
                    else
                    {
                        var lblMissing = new Label
                        {
                            Text = $"Thiếu {(minOrder - currentSubTotal):N0} đ",
                            Location = new Point(280, 28),
                            Size = new Size(110, 24),
                            TextAlign = ContentAlignment.MiddleRight,
                            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                            ForeColor = ThemeManager.Amber
                        };
                        card.Controls.Add(lblMissing);
                    }

                    flow.Controls.Add(card);
                }
            }

            // Thanh nút bấm dưới cùng
            var btnClearVoucher = new Button
            {
                Text = "Bỏ dùng voucher",
                Location = new Point(0, 436),
                Size = new Size(160, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = ThemeManager.Red,
                Font = new Font("Segoe UI", 9F)
            };
            btnClearVoucher.FlatAppearance.BorderColor = Color.FromArgb(254, 202, 202);
            btnClearVoucher.Click += (s, e) => {
                SelectedVoucher = null;
                DialogResult = DialogResult.OK;
                Close();
            };
            root.Controls.Add(btnClearVoucher);

            var btnClose = new Button
            {
                Text = "Đóng",
                Location = new Point(304, 436),
                Size = new Size(120, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = ThemeManager.Ink,
                Font = new Font("Segoe UI", 9F)
            };
            btnClose.FlatAppearance.BorderColor = ThemeManager.Line;
            btnClose.Click += (s, e) => {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            root.Controls.Add(btnClose);
        }
    }
}
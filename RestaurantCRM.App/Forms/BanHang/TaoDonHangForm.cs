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
    public partial class TaoDonHangForm : Form
    {
        public sealed class CartItem
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
        private TextBox _txtQuickVoucher;
        private Button _btnQuickApply;

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
            // Toàn bộ giao diện được dựng trong BuildCartColumn() (xem trong TaoDonHangForm.Cart.cs).
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
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    /// <summary>
    /// Thanh điều hướng Sidebar phong cách hiện đại khớp 100% với giao diện tham khảo:
    /// Nền sáng tinh tế, menu tối giản, và cụm chân trang có sóng lượn nghệ thuật tím/cam/vàng cùng avatar.
    /// </summary>
    public class UcSidebar : UserControl
    {
        public event Action<string> MenuSelected;
        public event Action<string> WorkspaceSelected;

        private readonly string _role;
        private readonly IList<string> _roles;
        private readonly string _displayName;
        private readonly Color _sidebarBg = Color.White;
        private readonly Color _itemHover = ColorTranslator.FromHtml("#F8FAFC");
        private readonly Color _textNormal = ColorTranslator.FromHtml("#475569");
        private readonly Color _textHeader = ColorTranslator.FromHtml("#94A3B8");
        private readonly Color _accent;
        private Button _activeBtn;

        public UcSidebar(string role, IList<string> roles, string displayName)
        {
            _role = role;
            _roles = roles;
            _displayName = displayName;
            _accent = ThemeManager.AccentFor(role);

            Width = 230;
            Dock = DockStyle.Left;
            BackColor = _sidebarBg;
            Font = new Font("Segoe UI", 9.5F);

            BuildLayout();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Kẻ viền phải thanh mảnh phân tách với vùng nội dung chính
            using (var pen = new Pen(ColorTranslator.FromHtml("#E2E8F0"), 1))
                e.Graphics.DrawLine(pen, Width - 1, 0, Width - 1, Height);
        }

        private void BuildLayout()
        {
            Controls.Clear();

            // 1. Top Header: Logo "LK CORP" / "RestaurantCRM"
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 82,
                BackColor = Color.White,
                Padding = new Padding(20, 18, 20, 10)
            };

            var logoBadge = new Label
            {
                Text = "🍽️",
                Font = new Font("Segoe UI Emoji", 13F),
                Size = new Size(34, 34),
                Location = new Point(18, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            logoBadge.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = ThemeManager.RoundedRect(new Rectangle(0, 0, 33, 33), 8))
                using (var brush = new LinearGradientBrush(logoBadge.ClientRectangle, ColorTranslator.FromHtml("#F5F3FF"), ColorTranslator.FromHtml("#EDE9FE"), 135f))
                using (var pen = new Pen(ColorTranslator.FromHtml("#C4B5FD"), 1))
                {
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }
            };
            topPanel.Controls.Add(logoBadge);

            var lblTitle = new Label
            {
                Text = "LK CORP",
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#1E1B4B"),
                AutoSize = true,
                Location = new Point(60, 18),
                BackColor = Color.Transparent
            };
            topPanel.Controls.Add(lblTitle);

            var lblRoleBadge = new Label
            {
                Text = "Restaurant CRM",
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = _accent,
                AutoSize = true,
                Location = new Point(60, 40),
                BackColor = Color.Transparent
            };
            topPanel.Controls.Add(lblRoleBadge);

            Controls.Add(topPanel);

            // 2. Bottom Footer: Thông tin người dùng tối giản & nút Đăng xuất (không màu mè)
            var footPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 72,
                BackColor = ColorTranslator.FromHtml("#F8FAFC"),
                Padding = new Padding(12, 10, 12, 10)
            };
            footPanel.Paint += (s, e) => {
                using (var pen = new Pen(ColorTranslator.FromHtml("#E2E8F0"), 1))
                    e.Graphics.DrawLine(pen, 0, 0, footPanel.Width, 0);
            };

            // Avatar tròn chữ cái đầu tối giản
            var avatar = new Label
            {
                Text = ThemeManager.Initials(_displayName),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(36, 36),
                Location = new Point(14, 18),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            avatar.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(0, 0, 35, 35);
                    using (var brush = new SolidBrush(ThemeManager.Primary))
                        e.Graphics.FillPath(brush, path);
                }
                TextRenderer.DrawText(e.Graphics, avatar.Text, avatar.Font,
                    avatar.ClientRectangle, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            footPanel.Controls.Add(avatar);

            var lblUser = new Label
            {
                Text = _displayName,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(56, 17),
                BackColor = Color.Transparent
            };
            footPanel.Controls.Add(lblUser);

            var lblSub = new Label
            {
                Text = _role,
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = ThemeManager.Muted,
                AutoSize = true,
                Location = new Point(57, 36),
                BackColor = Color.Transparent
            };
            footPanel.Controls.Add(lblSub);

            // Nút Đăng xuất gọn gàng
            var btnLogout = new Button
            {
                Text = "🚪",
                Font = new Font("Segoe UI Emoji", 11F),
                ForeColor = ColorTranslator.FromHtml("#EF4444"),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(32, 32),
                Location = new Point(footPanel.Width - 44, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatAppearance.MouseOverBackColor = ColorTranslator.FromHtml("#FEE2E2");
            btnLogout.Click += (s, e) => MenuSelected?.Invoke("Đăng xuất");
            footPanel.Controls.Add(btnLogout);

            Controls.Add(footPanel);

            // 3. Scrollable Center Menu Panel
            var menuPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(12, 4, 12, 12)
            };
            Controls.Add(menuPanel);
            menuPanel.BringToFront();

            int y = 6;

            // Workspace Switcher (nếu tài khoản có nhiều quyền)
            if (_roles != null && _roles.Count > 1)
            {
                AddSectionHeader(menuPanel, "Chuyển phân hệ", ref y);
                foreach (string r in _roles)
                {
                    bool isActive = (r == _role);
                    var btnSwitch = new Button
                    {
                        Text = "  🔄  " + r,
                        Location = new Point(8, y),
                        Size = new Size(200, 32),
                        FlatStyle = FlatStyle.Flat,
                        TextAlign = ContentAlignment.MiddleLeft,
                        ForeColor = isActive ? _accent : _textNormal,
                        BackColor = isActive ? ColorTranslator.FromHtml("#EEF2FF") : Color.Transparent,
                        Font = new Font("Segoe UI", 8.5F, isActive ? FontStyle.Bold : FontStyle.Regular),
                        Cursor = Cursors.Hand
                    };
                    btnSwitch.FlatAppearance.BorderSize = 0;
                    btnSwitch.FlatAppearance.MouseOverBackColor = _itemHover;
                    string target = r;
                    btnSwitch.Click += (s, e) => WorkspaceSelected?.Invoke(target);
                    menuPanel.Controls.Add(btnSwitch);
                    y += 34;
                }
                y += 8;
            }

            // Menu Items by Role
            AddSectionHeader(menuPanel, "Tổng quan", ref y);
            AddMenuItem(menuPanel, "Dashboard", "📊  Dashboard", ref y);
            y += 8;

            if (_role == "Admin")
            {
                AddSectionHeader(menuPanel, "QUẢN TRỊ HỆ THỐNG", ref y);
                if (UserSession.HasPermission("ADMIN_ACCOUNT"))
                    AddMenuItem(menuPanel, "Tài khoản", "👤  TÀI KHOẢN", ref y);
                if (UserSession.HasPermission("ADMIN_ROLE"))
                    AddMenuItem(menuPanel, "Vai trò & phân quyền", "🛡️  VAI TRÒ & PHÂN QUYỀN", ref y);
                if (UserSession.HasPermission("ADMIN_BACKUP"))
                    AddMenuItem(menuPanel, "Sao lưu dữ liệu", "💾  SAO LƯU DỮ LIỆU", ref y);
            }
            else if (_role == "Bán hàng")
            {
                AddSectionHeader(menuPanel, "VẬN HÀNH BÁN HÀNG", ref y);
                if (UserSession.HasPermission("ORDER_CREATE"))
                    AddMenuItem(menuPanel, "Tạo đơn hàng", "📝  TẠO ĐƠN HÀNG", ref y);
                if (UserSession.HasPermission("ORDER_VIEW"))
                    AddMenuItem(menuPanel, "Đơn hàng", "🧾  DANH SÁCH ĐƠN HÀNG", ref y);
                if (UserSession.HasPermission("MENU_VIEW"))
                    AddMenuItem(menuPanel, "Thực đơn", "🍲  THỰC ĐƠN MÓN ĂN", ref y);
                if (UserSession.HasPermission("PROMO_VIEW"))
                    AddMenuItem(menuPanel, "Khuyến mãi", "🎟️  KHUYẾN MÃI", ref y);
                if (UserSession.HasPermission("INVENTORY_VIEW"))
                    AddMenuItem(menuPanel, "Kho hàng", "📦  KIỂM TRA KHO", ref y);
                if (UserSession.HasPermission("IMPORT_VIEW"))
                    AddMenuItem(menuPanel, "Phiếu nhập hàng", "📥  QUẢN LÝ PHIẾU NHẬP", ref y);
                if (UserSession.HasPermission("INGREDIENT_VIEW"))
                    AddMenuItem(menuPanel, "Nguyên vật liệu", "🥕  NGUYÊN VẬT LIỆU", ref y);
                if (UserSession.HasPermission("REPORT_VIEW"))
                    AddMenuItem(menuPanel, "Báo cáo bán hàng", "📈  BÁO CÁO DOANH THU", ref y);
            }
            else
            {
                AddSectionHeader(menuPanel, "CHĂM SÓC KHÁCH HÀNG", ref y);
                if (UserSession.HasPermission("CRM_CUSTOMER"))
                    AddMenuItem(menuPanel, "Khách hàng", "👥  KHÁCH HÀNG", ref y);
                if (UserSession.HasPermission("CRM_REVIEW"))
                    AddMenuItem(menuPanel, "Đánh giá", "⭐  ĐÁNH GIÁ DỊCH VỤ", ref y);
                if (UserSession.HasPermission("CRM_SURVEY"))
                    AddMenuItem(menuPanel, "Khảo sát", "📋  KHẢO SÁT Ý KIẾN", ref y);
                if (UserSession.HasPermission("CRM_ANALYTICS"))
                    AddMenuItem(menuPanel, "Phân tích khách hàng", "📊  PHÂN TÍCH KHÁCH", ref y);
            }
        }

        private void AddSectionHeader(Panel parent, string title, ref int y)
        {
            var lbl = new Label
            {
                Text = title.ToUpper(),
                ForeColor = _textHeader,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(14, y)
            };
            parent.Controls.Add(lbl);
            y += 22;
        }

        private void AddMenuItem(Panel parent, string commandKey, string displayTitle, ref int y)
        {
            bool isDefault = commandKey == "Dashboard";
            var btn = new Button
            {
                Text = "  " + displayTitle,
                Tag = commandKey,
                Location = new Point(6, y),
                Size = new Size(204, 36),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = isDefault ? _accent : _textNormal,
                BackColor = isDefault ? ColorTranslator.FromHtml("#F5F3FF") : Color.Transparent,
                Font = new Font("Segoe UI", 8.5F, isDefault ? FontStyle.Bold : FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = _itemHover;

            if (isDefault) _activeBtn = btn;

            // Bo góc nhẹ và vạch nhấn bên trái khi active
            btn.Paint += (s, e) => {
                if (btn == _activeBtn)
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var path = ThemeManager.RoundedRect(new Rectangle(0, 0, btn.Width - 1, btn.Height - 1), 8))
                    using (var brush = new SolidBrush(ColorTranslator.FromHtml("#F5F3FF")))
                        e.Graphics.FillPath(brush, path);

                    using (var brush = new SolidBrush(_accent))
                    using (var barPath = ThemeManager.RoundedRect(new Rectangle(2, 6, 3, btn.Height - 12), 2))
                        e.Graphics.FillPath(brush, barPath);

                    TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font,
                        new Rectangle(6, 0, btn.Width - 10, btn.Height), _accent,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                }
            };

            btn.Click += (s, e) => {
                var clicked = (Button)s;
                if (_activeBtn != null && _activeBtn != clicked)
                {
                    _activeBtn.ForeColor = _textNormal;
                    _activeBtn.BackColor = Color.Transparent;
                    _activeBtn.Font = new Font("Segoe UI", 8.5F);
                    _activeBtn.Invalidate();
                }
                _activeBtn = clicked;
                _activeBtn.ForeColor = _accent;
                _activeBtn.BackColor = ColorTranslator.FromHtml("#F5F3FF");
                _activeBtn.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                _activeBtn.Invalidate();

                MenuSelected?.Invoke((string)clicked.Tag);
            };

            parent.Controls.Add(btn);
            y += 38;
        }
    }
}

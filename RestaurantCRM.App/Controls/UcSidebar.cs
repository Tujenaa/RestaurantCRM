using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls {
    public class UcSidebar : UserControl {
        public event Action<string> MenuSelected;
        public event Action<string> WorkspaceSelected;

        private readonly string _role;
        private readonly IList<string> _roles;
        private readonly string _displayName;
        private readonly Color _sidebarBg = ColorTranslator.FromHtml("#0B132B");
        private readonly Color _itemHover = ColorTranslator.FromHtml("#1C2541");
        private readonly Color _textNormal = ColorTranslator.FromHtml("#94A3B8");
        private readonly Color _textHeader = ColorTranslator.FromHtml("#64748B");
        private readonly Color _accent;
        private Button _activeBtn;

        public UcSidebar(string role, IList<string> roles, string displayName) {
            _role = role;
            _roles = roles;
            _displayName = displayName;
            _accent = ThemeManager.AccentFor(role);

            Width = 252;
            Dock = DockStyle.Left;
            BackColor = _sidebarBg;
            Font = new Font("Segoe UI", 9.5F);

            BuildLayout();
        }

        private void BuildLayout() {
            Controls.Clear();

            // 1. Top Header (Brand / Logo) with gradient
            var topPanel = new Panel {
                Dock = DockStyle.Top,
                Height = 88,
                BackColor = Color.Transparent,
                Padding = new Padding(20, 18, 20, 10)
            };
            topPanel.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new LinearGradientBrush(
                    topPanel.ClientRectangle,
                    Color.FromArgb(25, _accent),
                    Color.Transparent,
                    LinearGradientMode.Vertical))
                    e.Graphics.FillRectangle(brush, topPanel.ClientRectangle);
                using (var pen = new Pen(Color.FromArgb(20, _accent), 1))
                    e.Graphics.DrawLine(pen, 0, topPanel.Height - 1, topPanel.Width, topPanel.Height - 1);
            };

            var logoBadge = new Label {
                Text = "🍽️",
                Font = new Font("Segoe UI Emoji", 14F),
                Size = new Size(38, 38),
                Location = new Point(18, 18),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            logoBadge.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = ThemeManager.RoundedRect(new Rectangle(0, 0, logoBadge.Width - 1, logoBadge.Height - 1), 10))
                using (var brush = new LinearGradientBrush(logoBadge.ClientRectangle, _accent, ThemeManager.Violet, 135f))
                using (var pen = new Pen(Color.FromArgb(80, _accent), 1)) {
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }
            };
            topPanel.Controls.Add(logoBadge);

            var lblTitle = new Label {
                Text = "RestaurantCRM",
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(64, 16),
                BackColor = Color.Transparent
            };
            topPanel.Controls.Add(lblTitle);

            var lblRoleBadge = new Label {
                Text = _role.ToUpper() + " WORKSPACE",
                Font = new Font("Segoe UI", 7F, FontStyle.Bold),
                ForeColor = _accent,
                AutoSize = true,
                Location = new Point(64, 39),
                BackColor = Color.Transparent
            };
            topPanel.Controls.Add(lblRoleBadge);

            Controls.Add(topPanel);

            // 2. Bottom Footer (User info & Logout)
            var footPanel = new Panel {
                Dock = DockStyle.Bottom,
                Height = 82,
                BackColor = Color.FromArgb(10, 15, 30),
                Padding = new Padding(16, 12, 16, 12)
            };
            footPanel.Paint += (s, e) => {
                using (var pen = new Pen(ColorTranslator.FromHtml("#1E293B"), 1)) {
                    e.Graphics.DrawLine(pen, 0, 0, footPanel.Width, 0);
                }
            };

            // Avatar circle with initials
            var avatar = new Label {
                Text = ThemeManager.Initials(_displayName),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(36, 36),
                Location = new Point(14, 14),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            avatar.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = ThemeManager.RoundedRect(new Rectangle(0, 0, 35, 35), 18))
                using (var brush = new LinearGradientBrush(avatar.ClientRectangle, _accent, ThemeManager.Violet, 45f)) {
                    e.Graphics.FillPath(brush, path);
                }
            };
            footPanel.Controls.Add(avatar);

            var lblUser = new Label {
                Text = _displayName,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(56, 13)
            };
            footPanel.Controls.Add(lblUser);

            var lblSub = new Label {
                Text = _role,
                Font = new Font("Segoe UI", 8F),
                ForeColor = _textHeader,
                AutoSize = true,
                Location = new Point(56, 32)
            };
            footPanel.Controls.Add(lblSub);

            var btnLogout = new Button {
                Text = "🚪",
                Font = new Font("Segoe UI Emoji", 11F),
                ForeColor = ColorTranslator.FromHtml("#EF4444"),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(32, 32),
                Location = new Point(footPanel.Width - 46, 16),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 239, 68, 68);
            btnLogout.Click += (s, e) => MenuSelected?.Invoke("Đăng xuất");
            footPanel.Controls.Add(btnLogout);

            Controls.Add(footPanel);

            // 3. Scrollable Center Menu Panel
            var menuPanel = new Panel {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(14, 4, 14, 12)
            };
            Controls.Add(menuPanel);
            menuPanel.BringToFront();

            int y = 6;

            // Workspace Switcher (if user has multiple roles)
            if (_roles != null && _roles.Count > 1) {
                AddSectionHeader(menuPanel, "CHUYỂN PHÂN HỆ", ref y);
                foreach (string r in _roles) {
                    bool isActive = (r == _role);
                    var btnSwitch = new Button {
                        Text = "  🔄  " + r,
                        Location = new Point(14, y),
                        Size = new Size(216, 32),
                        FlatStyle = FlatStyle.Flat,
                        TextAlign = ContentAlignment.MiddleLeft,
                        ForeColor = isActive ? Color.White : _textNormal,
                        BackColor = isActive ? Color.FromArgb(45, _accent) : Color.Transparent,
                        Font = new Font("Segoe UI", 8.5F, isActive ? FontStyle.Bold : FontStyle.Regular),
                        Cursor = Cursors.Hand
                    };
                    btnSwitch.FlatAppearance.BorderSize = 0;
                    btnSwitch.FlatAppearance.MouseOverBackColor = _itemHover;
                    string target = r;
                    btnSwitch.Click += (s, e) => WorkspaceSelected?.Invoke(target);
                    menuPanel.Controls.Add(btnSwitch);
                    y += 36;
                }
                y += 10;
            }

            // Menu Items by Role
            AddSectionHeader(menuPanel, "TỔNG QUAN", ref y);
            AddMenuItem(menuPanel, "Dashboard", "📊  Dashboard", ref y);
            y += 8;

            if (_role == "Admin") {
                AddSectionHeader(menuPanel, "BÁN HÀNG & KHO", ref y);
                AddMenuItem(menuPanel, "Tạo đơn hàng", "📝  Tạo đơn hàng", ref y);
                AddMenuItem(menuPanel, "Đơn hàng", "🧾  Quản lý đơn hàng", ref y);
                AddMenuItem(menuPanel, "Món ăn", "🍲  Danh mục món ăn", ref y);
                AddMenuItem(menuPanel, "Loại món", "🏷️  Loại món ăn", ref y);
                AddMenuItem(menuPanel, "Kho hàng", "📦  Quản lý tồn kho", ref y);
                AddMenuItem(menuPanel, "Nhà cung cấp", "🏢  Nhà cung cấp", ref y);
                AddMenuItem(menuPanel, "Phiếu nhập hàng", "📥  Phiếu nhập hàng", ref y);
                AddMenuItem(menuPanel, "Khuyến mãi", "🎟️  Chương trình voucher", ref y);
                y += 8;

                AddSectionHeader(menuPanel, "HỆ THỐNG", ref y);
                AddMenuItem(menuPanel, "Tài khoản", "👤  Tài khoản người dùng", ref y);
                AddMenuItem(menuPanel, "Vai trò & phân quyền", "🛡️  Vai trò & quyền hạn", ref y);
            } else if (_role == "Bán hàng") {
                AddSectionHeader(menuPanel, "VẬN HÀNH BÁN HÀNG", ref y);
                AddMenuItem(menuPanel, "Tạo đơn hàng", "📝  Tạo đơn hàng mới", ref y);
                AddMenuItem(menuPanel, "Đơn hàng", "🧾  Danh sách đơn hàng", ref y);
                AddMenuItem(menuPanel, "Thực đơn", "🍲  Thực đơn món ăn", ref y);
                AddMenuItem(menuPanel, "Món ăn", "🍲  Quản lý món ăn", ref y);
                AddMenuItem(menuPanel, "Loại món", "🏷️  Phân loại món", ref y);
                AddMenuItem(menuPanel, "Kho hàng", "📦  Kiểm tra kho", ref y);
                AddMenuItem(menuPanel, "Nhà cung cấp", "🏢  Nhà cung cấp", ref y);
                AddMenuItem(menuPanel, "Phiếu nhập hàng", "📥  Phiếu nhập kho", ref y);
                AddMenuItem(menuPanel, "Khuyến mãi", "🎟️  Áp mã voucher", ref y);
                AddMenuItem(menuPanel, "Báo cáo bán hàng", "📈  Báo cáo doanh thu", ref y);
            } else {
                AddSectionHeader(menuPanel, "CHĂM SÓC KHÁCH HÀNG", ref y);
                AddMenuItem(menuPanel, "Khách hàng", "👥  Hồ sơ khách hàng", ref y);
                AddMenuItem(menuPanel, "Đánh giá", "⭐  Đánh giá chất lượng", ref y);
                AddMenuItem(menuPanel, "Phản hồi", "💬  Góp ý & phản hồi", ref y);
                y += 8;

                AddSectionHeader(menuPanel, "KHẢO SÁT & BÁO CÁO", ref y);
                AddMenuItem(menuPanel, "Khảo sát", "📋  Khảo sát khách hàng", ref y);
                AddMenuItem(menuPanel, "Phân tích khách hàng", "📊  Phân tích hành vi", ref y);
            }
        }

        private void AddSectionHeader(Panel parent, string title, ref int y) {
            var lbl = new Label {
                Text = title,
                ForeColor = _textHeader,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(18, y)
            };
            parent.Controls.Add(lbl);
            y += 22;
        }

        private void AddMenuItem(Panel parent, string commandKey, string displayTitle, ref int y) {
            bool isDefault = commandKey == "Dashboard";
            var btn = new Button {
                Text = "    " + displayTitle,
                Tag = commandKey,
                Location = new Point(8, y),
                Size = new Size(228, 36),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = isDefault ? Color.White : _textNormal,
                BackColor = isDefault ? Color.FromArgb(50, _accent) : Color.Transparent,
                Font = new Font("Segoe UI", 9F, isDefault ? FontStyle.Bold : FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = _itemHover;

            if (isDefault) _activeBtn = btn;

            // Paint left accent indicator for active item
            btn.Paint += (s, e) => {
                if (btn == _activeBtn) {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                        new Rectangle(0, 4, 3, btn.Height - 8),
                        _accent, ThemeManager.Violet,
                        System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                    using (var path = ThemeManager.RoundedRect(new Rectangle(0, 6, 3, btn.Height - 12), 2))
                        e.Graphics.FillPath(brush, path);
                }
            };

            btn.Click += (s, e) => {
                var clicked = (Button)s;
                if (_activeBtn != null && _activeBtn != clicked) {
                    _activeBtn.ForeColor = _textNormal;
                    _activeBtn.BackColor = Color.Transparent;
                    _activeBtn.Font = new Font("Segoe UI", 9F);
                    _activeBtn.Invalidate();
                }
                _activeBtn = clicked;
                _activeBtn.ForeColor = Color.White;
                _activeBtn.BackColor = Color.FromArgb(50, _accent);
                _activeBtn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                _activeBtn.Invalidate();

                MenuSelected?.Invoke((string)clicked.Tag);
            };

            parent.Controls.Add(btn);
            y += 38;
        }
    }
}

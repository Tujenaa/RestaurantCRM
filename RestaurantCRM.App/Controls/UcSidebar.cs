using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    /// <summary>
    /// Sidebar tối giản: logo trên cùng, menu chữ xám nhạt + icon nét mảnh,
    /// mục đang chọn tô xanh; chân trang có ô chuyển phân hệ và lời chào người dùng.
    /// </summary>
    public class UcSidebar : UserControl
    {
        public event Action<string> MenuSelected;
        public event Action<string> WorkspaceSelected;

        private string _role;
        private readonly IList<string> _roles;
        private readonly string _displayName;
        private readonly Color _textNormal = ColorTranslator.FromHtml("#9097AE");
        private readonly Color _textHeader = ColorTranslator.FromHtml("#B8BDCC");
        private readonly Color _hoverBg = ColorTranslator.FromHtml("#F7F8FC");
        private MenuItemControl _active;

        // Thu gọn / mở rộng sidebar (nhớ trạng thái trong suốt phiên làm việc)
        private const int ExpandedWidth = 224;
        private const int CollapsedWidth = 76;
        private static bool s_collapsed;
        private readonly ToolTip _tip = new ToolTip { InitialDelay = 200 };

        public bool IsCollapsed => s_collapsed;

        public void UpdateRole(string role)
        {
            if (_role == role) return;
            _role = role;
            _active = null;
            RebuildLayout();
        }

        /// <summary>Chuyển giữa chế độ đầy đủ và chế độ chỉ hiện icon.</summary>
        public void ToggleCollapsed()
        {
            string activeKey = _active?.Tag as string;
            s_collapsed = !s_collapsed;
            var parent = Parent;
            parent?.SuspendLayout();
            Width = s_collapsed ? CollapsedWidth : ExpandedWidth;
            RebuildLayout();
            if (!string.IsNullOrEmpty(activeKey)) SetActive(activeKey);
            parent?.ResumeLayout(true);
        }

        private void RebuildLayout()
        {
            SuspendLayout();
            _tip.RemoveAll();
            var oldControls = new Control[Controls.Count];
            Controls.CopyTo(oldControls, 0);
            Controls.Clear();
            foreach (var c in oldControls)
            {
                try { c.Dispose(); } catch { }
            }
            BuildLayout();
            ResumeLayout(true);
            Invalidate();
        }

        public UcSidebar(string role, IList<string> roles, string displayName)
        {
            _role = role;
            _roles = roles;
            _displayName = displayName;

            Width = s_collapsed ? CollapsedWidth : ExpandedWidth;
            Dock = DockStyle.Left;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9.5F);
            DoubleBuffered = true;

            BuildLayout();
        }

        private void BuildLayout()
        {
            Controls.Clear();

            // ── 1. Logo ──────────────────────────────────────────────
            bool collapsed = s_collapsed;
            var topPanel = new Panel { Dock = DockStyle.Top, Height = collapsed ? 108 : 92, BackColor = Color.White };
            topPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                // biểu tượng logo: ô vuông bo góc xanh
                var badge = collapsed ? new Rectangle((Width - 26) / 2, 24, 26, 26) : new Rectangle(28, 34, 26, 26);
                using (var path = ThemeManager.RoundedRect(badge, 7))
                using (var br = new SolidBrush(ThemeManager.Primary))
                    g.FillPath(br, path);
                using (var f = ThemeManager.Icon(10F))
                    TextRenderer.DrawText(g, ThemeManager.Glyph.Home, f, badge, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                if (collapsed) return;
                using (var f1 = new Font("Segoe UI", 13F, FontStyle.Bold))
                using (var f2 = new Font("Segoe UI", 13F, FontStyle.Regular))
                {
                    TextRenderer.DrawText(g, "Lẩu", f1, new Point(60, 33), ThemeManager.Ink, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    var w = TextRenderer.MeasureText(g, "Lẩu", f1, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width;
                    TextRenderer.DrawText(g, "Phố", f2, new Point(62 + w, 33), ThemeManager.Primary, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                }
            };
            Controls.Add(topPanel);

            // Nút thu gọn / mở rộng sidebar
            // Logo (expanded): badge y=34, cao 26 → tâm y=47; nút 32px đặt y=31 để cùng tâm
            var btnToggle = new IconButton(ThemeManager.Glyph.Hamburger, _hoverBg)
            {
                Size = new Size(32, 32),
                Location = collapsed ? new Point((Width - 32) / 2, 62) : new Point(Width - 48, 31)
            };
            _tip.SetToolTip(btnToggle, collapsed ? "Mở rộng menu" : "Thu gọn menu");
            btnToggle.Click += (s, e) => BeginInvoke((Action)ToggleCollapsed);
            topPanel.Controls.Add(btnToggle);

            // ── 2. Chân trang: ô phân hệ + người dùng ──────────────────
            var footPanel = new Panel { Dock = DockStyle.Bottom, Height = 132, BackColor = Color.White };
            Controls.Add(footPanel);
            BuildFooter(footPanel);

            // ── 3. Menu ──────────────────────────────────────────────
            var menuPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
            Controls.Add(menuPanel);
            menuPanel.BringToFront();

            int y = 4;
            AddMenuItem(menuPanel, "Dashboard", "Tổng quan", ThemeManager.Glyph.Home, ref y);
            y += 10;

            if (_role == "Hệ thống" || _role == "Admin")
            {
                AddSectionHeader(menuPanel, "Quản trị hệ thống", ref y);
                if (UserSession.HasPermission("ADMIN_ACCOUNT"))
                    AddMenuItem(menuPanel, "Tài khoản", "Tài khoản", ThemeManager.Glyph.Contact, ref y);
                if (UserSession.HasPermission("ADMIN_ROLE"))
                    AddMenuItem(menuPanel, "Vai trò & phân quyền", "Vai trò & phân quyền", ThemeManager.Glyph.Shield, ref y);
                if (UserSession.HasPermission("ADMIN_BACKUP"))
                    AddMenuItem(menuPanel, "Sao lưu dữ liệu", "Sao lưu dữ liệu", ThemeManager.Glyph.Save, ref y);
            }
            else if (_role == "Vận Hành" || _role == "Bán hàng")
            {
                bool hasSaleGroup = UserSession.HasAnyPermission("ORDER_CREATE", "ORDER_VIEW", "MENU_VIEW", "PROMO_VIEW");
                if (hasSaleGroup)
                {
                    AddSectionHeader(menuPanel, "Vận hành bán hàng", ref y);
                    if (UserSession.HasPermission("ORDER_CREATE"))
                        AddMenuItem(menuPanel, "Tạo đơn hàng", "Tạo đơn hàng", ThemeManager.Glyph.Cart, ref y);
                    if (UserSession.HasPermission("ORDER_VIEW"))
                        AddMenuItem(menuPanel, "Đơn hàng", "Đơn hàng", ThemeManager.Glyph.List, ref y);
                    if (UserSession.HasPermission("MENU_VIEW"))
                        AddMenuItem(menuPanel, "Thực đơn", "Thực đơn", ThemeManager.Glyph.Book, ref y);
                    if (UserSession.HasPermission("PROMO_VIEW"))
                        AddMenuItem(menuPanel, "Khuyến mãi", "Khuyến mãi", ThemeManager.Glyph.Tag, ref y);
                }

                bool hasKhoGroup = UserSession.HasAnyPermission("INVENTORY_VIEW", "IMPORT_VIEW", "INGREDIENT_VIEW");
                if (hasKhoGroup)
                {
                    if (hasSaleGroup) y += 10;
                    AddSectionHeader(menuPanel, "Kho & nhập hàng", ref y);
                    if (UserSession.HasPermission("INVENTORY_VIEW"))
                        AddMenuItem(menuPanel, "Kho hàng", "Kiểm tra kho", ThemeManager.Glyph.Package, ref y);
                    if (UserSession.HasPermission("IMPORT_VIEW"))
                        AddMenuItem(menuPanel, "Phiếu nhập hàng", "Phiếu nhập", ThemeManager.Glyph.Download, ref y);
                    if (UserSession.HasPermission("INGREDIENT_VIEW"))
                        AddMenuItem(menuPanel, "Nguyên vật liệu", "Nguyên vật liệu", ThemeManager.Glyph.Folder, ref y);
                    if (UserSession.HasPermission("INGREDIENT_VIEW"))
                        AddMenuItem(menuPanel, "Nhà cung cấp", "Nhà cung cấp", ThemeManager.Glyph.Contact, ref y);
                }

                if (UserSession.HasPermission("REPORT_VIEW"))
                {
                    if (hasSaleGroup || hasKhoGroup) y += 10;
                    AddSectionHeader(menuPanel, "Báo cáo", ref y);
                    AddMenuItem(menuPanel, "Báo cáo bán hàng", "Doanh thu", ThemeManager.Glyph.Chart, ref y);
                }
            }
            else
            {
                AddSectionHeader(menuPanel, "Chăm sóc khách hàng", ref y);
                if (UserSession.HasPermission("CRM_CUSTOMER"))
                    AddMenuItem(menuPanel, "Khách hàng", "Khách hàng", ThemeManager.Glyph.People, ref y);
                if (UserSession.HasPermission("CRM_REVIEW"))
                    AddMenuItem(menuPanel, "Đánh giá", "Đánh giá dịch vụ", ThemeManager.Glyph.Star, ref y);
                if (UserSession.HasPermission("CRM_SURVEY"))
                    AddMenuItem(menuPanel, "Khảo sát", "Khảo sát ý kiến", ThemeManager.Glyph.Document, ref y);
                if (UserSession.HasPermission("CRM_ANALYTICS"))
                    AddMenuItem(menuPanel, "Phân tích khách hàng", "Phân tích khách", ThemeManager.Glyph.Diagnostic, ref y);
            }
        }

        // ─────────────────────────────────────────────────────────────
        private void BuildFooter(Panel foot)
        {
            // Ô chuyển phân hệ (giống "Company 1 ⌄")
            bool collapsed = s_collapsed;
            var workspace = new Panel
            {
                Location = collapsed ? new Point((Width - 44) / 2, 10) : new Point(20, 14),
                Size = collapsed ? new Size(44, 44) : new Size(184, 44),
                BackColor = Color.White,
                Cursor = (_roles != null && _roles.Count > 1) ? Cursors.Hand : Cursors.Default
            };
            bool hover = false;
            workspace.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rc = new Rectangle(0, 0, workspace.Width - 1, workspace.Height - 1);
                using (var path = ThemeManager.RoundedRect(rc, 10))
                {
                    using (var br = new SolidBrush(hover ? _hoverBg : Color.White)) g.FillPath(br, path);
                    using (var pen = new Pen(ThemeManager.Line)) g.DrawPath(pen, path);
                }
                var dot = collapsed ? new Rectangle((workspace.Width - 22) / 2, 11, 22, 22) : new Rectangle(10, 11, 22, 22);
                using (var br = new SolidBrush(ThemeManager.Primary)) g.FillEllipse(br, dot);
                string displayRole = ThemeManager.DisplayWorkspaceName(_role);
                using (var f = new Font("Segoe UI", 8F, FontStyle.Bold))
                    TextRenderer.DrawText(g, string.IsNullOrEmpty(displayRole) ? "?" : displayRole.Substring(0, 1).ToUpper(), f, dot, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                if (collapsed) return;
                using (var f = new Font("Segoe UI", 9F, FontStyle.Bold))
                    TextRenderer.DrawText(g, "Phân hệ " + displayRole, f, new Rectangle(40, 0, workspace.Width - 64, workspace.Height),
                        ThemeManager.Primary, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                if (_roles != null && _roles.Count > 1)
                    using (var f = ThemeManager.Icon(7.5F))
                        TextRenderer.DrawText(g, ThemeManager.Glyph.ChevronDown, f, new Rectangle(workspace.Width - 26, 0, 18, workspace.Height),
                            ThemeManager.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            if (_roles != null && _roles.Count > 1)
            {
                var menu = new ContextMenuStrip { ShowImageMargin = false, Font = new Font("Segoe UI", 9.5F), BackColor = Color.White };
                string currentDisplay = ThemeManager.DisplayWorkspaceName(_role);
                foreach (string r in _roles)
                {
                    string target = r;
                    string rDisplay = ThemeManager.DisplayWorkspaceName(r);
                    bool isCurrent = string.Equals(r, _role, StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(rDisplay, currentDisplay, StringComparison.OrdinalIgnoreCase);
                    var item = new ToolStripMenuItem((isCurrent ? "✓  " : "     ") + "Phân hệ " + rDisplay) { ForeColor = isCurrent ? ThemeManager.Primary : ThemeManager.Ink2 };
                    item.Click += (s, e) =>
                    {
                        string selected = target;
                        BeginInvoke((Action)(() => WorkspaceSelected?.Invoke(selected)));
                    };
                    menu.Items.Add(item);
                }
                workspace.MouseEnter += (s, e) => { hover = true; workspace.Invalidate(); };
                workspace.MouseLeave += (s, e) => { hover = false; workspace.Invalidate(); };
                workspace.Click += (s, e) => menu.Show(workspace, new Point(0, -menu.PreferredSize.Height - 4));
            }
            _tip.SetToolTip(workspace, "Phân hệ " + ThemeManager.DisplayWorkspaceName(_role));
            foot.Controls.Add(workspace);

            // Người dùng
            var user = collapsed
                ? new Panel { Location = new Point((Width - 44) / 2, 58), Size = new Size(44, 72), BackColor = Color.White }
                : new Panel { Location = new Point(20, 70), Size = new Size(184, 44), BackColor = Color.White };
            if (collapsed) _tip.SetToolTip(user, _displayName ?? "");
            user.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var av = collapsed ? new Rectangle(6, 2, 32, 32) : new Rectangle(0, 6, 32, 32);
                using (var br = new SolidBrush(ThemeManager.AvatarColor(_displayName))) g.FillEllipse(br, av);
                using (var f = new Font("Segoe UI", 8.5F, FontStyle.Bold))
                    TextRenderer.DrawText(g, ThemeManager.Initials(_displayName), f, av, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                if (collapsed) return;
                string first = string.IsNullOrWhiteSpace(_displayName) ? "bạn" : _displayName.Trim().Split(' ')[_displayName.Trim().Split(' ').Length - 1];
                using (var f = new Font("Segoe UI", 9F))
                    TextRenderer.DrawText(g, "Chào, " + first + "!", f, new Rectangle(42, 4, user.Width - 80, 20),
                        ThemeManager.Ink, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                using (var f = new Font("Segoe UI", 7.5F))
                    TextRenderer.DrawText(g, _displayName ?? "", f, new Rectangle(42, 23, user.Width - 80, 16),
                        ThemeManager.Muted, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            };
            foot.Controls.Add(user);

            var btnLogout = new Button
            {
                Text = ThemeManager.Glyph.Power,
                Font = ThemeManager.Icon(10F),
                ForeColor = ThemeManager.Muted,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(32, 32),
                Location = collapsed ? new Point(6, 38) : new Point(user.Width - 32, 6),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatAppearance.MouseOverBackColor = ThemeManager.RedSoft;
            btnLogout.MouseEnter += (s, e) => btnLogout.ForeColor = ThemeManager.Red;
            btnLogout.MouseLeave += (s, e) => btnLogout.ForeColor = ThemeManager.Muted;
            _tip.SetToolTip(btnLogout, "Đăng xuất");
            btnLogout.Click += (s, e) => {
                btnLogout.Enabled = false;
                BeginInvoke((Action)(() => MenuSelected?.Invoke("Đăng xuất")));
            };
            user.Controls.Add(btnLogout);
        }

        /// <summary>Tô sáng mục menu theo khóa (dùng khi mở chức năng từ nơi khác, vd Thao tác nhanh).</summary>
        public void SetActive(string commandKey)
        {
            MenuItemControl found = null;
            FindItem(this, commandKey, ref found);
            if (found == null || found == _active) return;
            var prev = _active;
            _active = found;
            prev?.Invalidate();
            found.Invalidate();
        }

        private static void FindItem(Control root, string key, ref MenuItemControl found)
        {
            foreach (Control c in root.Controls)
            {
                var m = c as MenuItemControl;
                if (m != null && (m.Tag as string) == key) { found = m; return; }
                FindItem(c, key, ref found);
                if (found != null) return;
            }
        }

        private void AddSectionHeader(Panel parent, string title, ref int y)
        {
            if (s_collapsed)
            {
                // Chế độ thu gọn: thay tiêu đề nhóm bằng một vạch ngăn mảnh
                parent.Controls.Add(new Panel
                {
                    Location = new Point(22, y + 10),
                    Size = new Size(Width - 44, 1),
                    BackColor = ThemeManager.Line
                });
                y += 22;
                return;
            }
            var lbl = new Label
            {
                Text = title.ToUpper(),
                ForeColor = _textHeader,
                Font = new Font("Segoe UI", 7F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(28, y + 4),
                UseMnemonic = false,
                BackColor = Color.Transparent
            };
            parent.Controls.Add(lbl);
            y += 28;
        }

        private void AddMenuItem(Panel parent, string commandKey, string title, string glyph, ref int y)
        {
            var item = new MenuItemControl(this, commandKey, title, glyph)
            {
                Location = new Point(12, y),
                Size = new Size(Width - 24, 38)
            };
            if (commandKey == "Dashboard") _active = item;
            if (s_collapsed) _tip.SetToolTip(item, title);
            item.Click += (s, e) =>
            {
                var prev = _active;
                _active = item;
                prev?.Invalidate();
                item.Invalidate();
                MenuSelected?.Invoke(commandKey);
            };
            parent.Controls.Add(item);
            y += 40;
        }

        /// <summary>Nút icon tự vẽ: icon luôn nằm chính giữa (Button chuẩn có padding/viền làm lệch glyph).</summary>
        private class IconButton : Control
        {
            private readonly string _glyph;
            private readonly Color _hoverBg;
            private bool _hover, _down;

            public IconButton(string glyph, Color hoverBg)
            {
                _glyph = glyph; _hoverBg = hoverBg;
                Cursor = Cursors.Hand;
                TabStop = false;
                BackColor = Color.White;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            }

            protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
            protected override void OnMouseLeave(EventArgs e) { _hover = false; _down = false; Invalidate(); base.OnMouseLeave(e); }
            protected override void OnMouseDown(MouseEventArgs e) { _down = true; Invalidate(); base.OnMouseDown(e); }
            protected override void OnMouseUp(MouseEventArgs e) { _down = false; Invalidate(); base.OnMouseUp(e); }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(BackColor);
                if (_hover || _down)
                {
                    using (var path = ThemeManager.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 8))
                    using (var br = new SolidBrush(_down ? ThemeManager.PrimarySoft : _hoverBg))
                        g.FillPath(br, path);
                }
                using (var f = ThemeManager.Icon(11F))
                    TextRenderer.DrawText(g, _glyph, f, new Rectangle(0, 0, Width, Height),
                        _hover ? ThemeManager.Primary : ThemeManager.Muted,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
            }
        }

        /// <summary>Một mục menu tự vẽ (icon + chữ), trạng thái hover / active.</summary>
        private class MenuItemControl : Control
        {
            private readonly UcSidebar _owner;
            private readonly string _title;
            private readonly string _glyph;
            private bool _hover;

            public MenuItemControl(UcSidebar owner, string key, string title, string glyph)
            {
                _owner = owner; _title = title; _glyph = glyph;
                Tag = key;
                Cursor = Cursors.Hand;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                BackColor = Color.White;
            }

            protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
            protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                bool active = _owner._active == this;
                g.Clear(Color.White);
                if (active || _hover)
                {
                    using (var path = ThemeManager.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 9))
                    using (var br = new SolidBrush(active ? ThemeManager.PrimarySoft : _owner._hoverBg))
                        g.FillPath(br, path);
                }
                Color c = active ? ThemeManager.Primary : (_hover ? ThemeManager.Ink2 : _owner._textNormal);
                if (s_collapsed)
                {
                    using (var f = ThemeManager.Icon(11.5F))
                        TextRenderer.DrawText(g, _glyph, f, new Rectangle(0, 0, Width, Height), c,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    return;
                }
                using (var f = ThemeManager.Icon(10.5F))
                    TextRenderer.DrawText(g, _glyph, f, new Rectangle(14, 0, 22, Height), c,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                using (var f = new Font("Segoe UI", 9.5F, active ? FontStyle.Bold : FontStyle.Regular))
                    TextRenderer.DrawText(g, _title, f, new Rectangle(46, 0, Width - 50, Height), c,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
        }
    }
}
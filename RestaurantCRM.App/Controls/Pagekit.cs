using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    /// <summary>Một bộ lọc dạng ComboBox kèm nhãn mô tả (nhãn sẽ thành tiền tố của chip).</summary>
    public class FilterSpec
    {
        public ComboBox Combo;
        public Label Caption;
        public FilterSpec(ComboBox combo, Label caption = null) { Combo = combo; Caption = caption; }
    }

    /// <summary>Khai báo các phần của một màn danh sách để PageKit sắp xếp lại theo bố cục chuẩn.</summary>
    public class ListPageSpec
    {
        public Label Title;
        public Label Subtitle;
        public Control[] HeaderActions;      // nút phụ góc phải trên (Làm mới, Loại món...)
        public Button[] PrimaryActions;      // nút chính (Thêm...) — xanh đặc, góc phải trên
        public Label[] KpiLabels;            // nhãn dạng "Tiêu đề: giá trị" -> thẻ KPI
        public Control KpiHost;              // hoặc khối KPI có sẵn (TableLayoutPanel...)
        public TextBox Search;
        public Label SearchIcon;
        public string SearchPlaceholder = "Tìm kiếm...";
        public FilterSpec[] Filters;
        public Control[] FilterExtras;       // control khác trên hàng lọc
        public Label CountLabel;
        public DataGridView Grid;
        public Button[] SecondaryActions;    // nút dưới bảng (Sửa, Chi tiết...)
        public Button[] DangerActions;       // nút dưới bảng màu đỏ nhạt (Xóa, Ngừng...)
        public Label FooterInfo;
        public Control SidePanel;            // khối chi tiết bên phải (nếu có)
        public int SideWidth = 340;
        public bool SideFullHeight;          // khối bên phải cao từ dưới tiêu đề tới đáy (hàng KPI chỉ nằm bên trái)
        public int KpiSlots;                 // > số ô KPI: mỗi ô rộng = bề ngang trang / KpiSlots, xếp sát trái
    }

    /// <summary>
    /// Dựng lại một Form danh sách (tạo bằng Designer) theo bố cục thống nhất:
    ///   [Mô tả ................................. nút phụ | nút chính]
    ///   [KPI] [KPI] [KPI] [KPI]
    ///   ┌ thẻ trắng ──────────────────────────────────────────────┐
    ///   │ (Tìm kiếm...) [Lọc: ...] [Lọc: ...]          Hiển thị x │
    ///   │ BẢNG                                                    │
    ///   │ [Sửa] [Xóa]                         Thông tin dòng chọn │
    ///   └─────────────────────────────────────────────────────────┘
    /// Chỉ di chuyển control có sẵn — mọi sự kiện, tên biến trong code giữ nguyên.
    /// </summary>
    public static class PageKit
    {
        public const int ButtonHeight = 34;

        public static void BuildListPage(Form form, ListPageSpec spec)
        {
            form.SuspendLayout();

            // 1. Tháo các control cần giữ khỏi container cũ
            var keep = new List<Control>();
            Action<Control> take = c => { if (c != null) { c.Parent?.Controls.Remove(c); keep.Add(c); } };
            take(spec.Title); take(spec.Subtitle); take(spec.Search); take(spec.CountLabel); take(spec.Grid);
            take(spec.FooterInfo); take(spec.KpiHost); take(spec.SidePanel);
            if (spec.HeaderActions != null) foreach (var c in spec.HeaderActions) take(c);
            if (spec.PrimaryActions != null) foreach (var c in spec.PrimaryActions) take(c);
            if (spec.SecondaryActions != null) foreach (var c in spec.SecondaryActions) take(c);
            if (spec.DangerActions != null) foreach (var c in spec.DangerActions) take(c);
            if (spec.FilterExtras != null) foreach (var c in spec.FilterExtras) take(c);
            if (spec.Filters != null) foreach (var f in spec.Filters) take(f.Combo);
            if (spec.KpiLabels != null) foreach (var l in spec.KpiLabels) take(l);

            // 2. Bỏ toàn bộ khung cũ (không Dispose — code cũ có thể vẫn gán Text cho nhãn phụ)
            form.Controls.Clear();

            form.BackColor = ThemeManager.Bg;
            form.Padding = new Padding(12, 4, 12, 8);
            form.AutoScroll = false;

            // 3. Hàng đầu: tiêu đề + mô tả + nút
            var header = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = ThemeManager.Bg };
            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(0, 4, 4, 0)
            };
            if (spec.PrimaryActions != null)
                foreach (var b in spec.PrimaryActions) { ButtonSkin.SetKind(b, ButtonSkin.Kind.Primary); FitButton(b); b.Margin = new Padding(8, 0, 0, 0); actions.Controls.Add(b); }
            if (spec.HeaderActions != null)
                foreach (var c in spec.HeaderActions)
                {
                    var b = c as Button;
                    if (b != null) { ButtonSkin.SetKind(b, ButtonSkin.Kind.Secondary); FitButton(b); }
                    c.Margin = new Padding(8, 0, 0, 0);
                    actions.Controls.Add(c);
                }
            header.Controls.Add(actions);

            Label title = spec.Title, sub = spec.Subtitle;
            if (title != null)
            {
                title.Text = SkinUtil.CleanText(title.Text);
                title.Font = new Font(ThemeManager.FontFamily, 15F, FontStyle.Bold);
                title.ForeColor = ThemeManager.Ink; title.BackColor = Color.Transparent;
                title.AutoSize = true; title.UseMnemonic = false; title.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                header.Controls.Add(title);
            }
            if (sub != null)
            {
                sub.Font = new Font(ThemeManager.FontFamily, 8.5F);
                sub.ForeColor = ThemeManager.Muted; sub.BackColor = Color.Transparent;
                sub.AutoSize = true; sub.UseMnemonic = false; sub.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                header.Controls.Add(sub);
            }
            Action placeTitle = () =>
            {
                bool embedded = !form.TopLevel;
                if (title != null) { title.Visible = !embedded; title.Location = new Point(8, 4); }
                if (sub != null) sub.Location = new Point(8, embedded ? 10 : (title == null ? 14 : 32));
                header.Height = embedded ? 38 : (title == null ? 46 : 60);
                actions.Padding = new Padding(0, embedded ? 2 : 6, 4, 0);
            };
            // Mô tả dài thì cắt "..." thay vì đè lên các nút bên phải
            Action fitSub = () =>
            {
                if (sub == null) return;
                sub.AutoSize = false;
                sub.AutoEllipsis = true;
                sub.Height = 18;
                sub.Width = Math.Max(80, actions.Left - sub.Left - 16);
            };
            placeTitle();
            fitSub();
            form.ParentChanged += (s, e) => { placeTitle(); fitSub(); };
            header.Resize += (s, e) => fitSub();
            actions.SizeChanged += (s, e) => fitSub();

            // 4. Thẻ KPI
            Control kpiRow = null;
            if (spec.KpiLabels != null && spec.KpiLabels.Length > 0)
            {
                int n = spec.KpiLabels.Length;
                bool fixedSlots = spec.KpiSlots > n;
                var tl = new TableLayoutPanel { Dock = DockStyle.Top, Height = 84, ColumnCount = fixedSlots ? n + 1 : n, RowCount = 1, BackColor = ThemeManager.Bg, Margin = new Padding(0), Padding = new Padding(0) };
                tl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                for (int i = 0; i < n; i++)
                {
                    tl.ColumnStyles.Add(fixedSlots ? new ColumnStyle(SizeType.Absolute, 200) : new ColumnStyle(SizeType.Percent, 100f / n));
                    var tile = new KpiTile(spec.KpiLabels[i]) { Dock = DockStyle.Fill };
                    tl.Controls.Add(tile, i, 0);
                }
                if (fixedSlots)
                {
                    // Ô KPI giữ cỡ như khi trang có KpiSlots ô (chia theo bề ngang cả trang), phần còn lại để trống
                    tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                    int slots = spec.KpiSlots;
                    Action fitKpi = () =>
                    {
                        int w = Math.Max(150, (form.ClientSize.Width - form.Padding.Horizontal) / slots);
                        for (int i = 0; i < n; i++) tl.ColumnStyles[i].Width = w;
                    };
                    fitKpi();
                    form.Resize += (s, e) => fitKpi();
                }
                kpiRow = tl;
            }
            else if (spec.KpiHost != null)
            {
                var host = new Panel { Dock = DockStyle.Top, Height = 84, BackColor = ThemeManager.Bg };
                spec.KpiHost.Dock = DockStyle.Fill;
                spec.KpiHost.Margin = new Padding(0);
                spec.KpiHost.Padding = new Padding(0);
                spec.KpiHost.BackColor = ThemeManager.Bg;
                host.Controls.Add(spec.KpiHost);
                kpiRow = host;
            }

            // 5. Thẻ trắng chứa bảng
            var card = NewCard();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(16, 6, 16, 10);

            var filterRow = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.White };
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.White,
                Padding = new Padding(0, 4, 0, 2)
            };
            PillSearch searchPill = null;
            if (spec.Search != null)
            {
                searchPill = new PillSearch(spec.Search, spec.SearchPlaceholder) { Width = 250, Margin = new Padding(0, 0, 8, 4) };
                flow.Controls.Add(searchPill);
            }
            var filterCombos = new List<ComboBox>();
            var filterChips = new List<ComboChip>();
            int filterCount = 0;
            if (spec.Filters != null) foreach (var f in spec.Filters) if (f.Combo != null) filterCount++;

            // ≥ 2 bộ lọc: gom vào nút "Bộ lọc" (menu gọn) + chip nhỏ chỉ hiện cho bộ lọc đang bật.
            // 1 bộ lọc: giữ chip đầy đủ như cũ (một chip không tốn chỗ hơn nút).
            bool filterMenuMode = filterCount >= 2;
            if (filterMenuMode) flow.WrapContents = false;   // luôn 1 hàng (FitFilterRowSingleLine co giãn cho vừa)
            Button btnFilter = null;
            if (filterMenuMode)
            {
                btnFilter = new Button { Text = "Bộ lọc ▾", Margin = new Padding(0, 2, 8, 4) };
                ButtonSkin.SetKind(btnFilter, ButtonSkin.Kind.Secondary);
                FitButton(btnFilter, 84);
                btnFilter.Click += (s, e) => ShowFilterMenu(btnFilter, filterChips);
                flow.Controls.Add(btnFilter);
            }
            if (spec.Filters != null)
                foreach (var f in spec.Filters)
                {
                    if (f.Combo == null) continue;
                    var chip = new ComboChip(f.Combo, f.Caption != null ? f.Caption.Text : null) { Margin = new Padding(0, 0, 8, 4) };
                    if (filterMenuMode)
                    {
                        chip.Compact = true;
                        chip.AutoHideWhenInactive = true;
                        chip.Margin = new Padding(0, 4, 6, 4);   // căn giữa theo ô tìm kiếm cao 38px
                    }
                    flow.Controls.Add(chip);
                    filterChips.Add(chip);
                    f.Combo.Visible = false;            // vẫn giữ trong form để DataSource hoạt động
                    filterRow.Controls.Add(f.Combo);
                    filterCombos.Add(f.Combo);
                }
            if (spec.FilterExtras != null)
                foreach (var c in spec.FilterExtras)
                {
                    var b = c as Button;
                    if (b != null) { ButtonSkin.SetKind(b, ButtonSkin.Kind.Secondary); FitButton(b); }
                    c.Margin = new Padding(0, 0, 8, 4);
                    flow.Controls.Add(c);
                }

            // Nút "Đặt lại" tự động hiện khi đang có từ khóa tìm kiếm hoặc bộ lọc nâng cao đang bật
            // (chế độ nút "Bộ lọc" không cần: đã có "Đặt lại" trong menu, ✕ trên chip và trong ô tìm kiếm)
            Button btnResetFilters = null;
            if (!filterMenuMode && (spec.Search != null || filterCombos.Count > 0))
            {
                btnResetFilters = new Button
                {
                    Text = "Đặt lại lọc",
                    Visible = false,
                    Margin = new Padding(0, 2, 8, 4)
                };
                ButtonSkin.SetKind(btnResetFilters, ButtonSkin.Kind.Ghost);
                FitButton(btnResetFilters, 76);
                btnResetFilters.Click += (s, e) =>
                {
                    if (spec.Search != null && spec.Search.TextLength > 0)
                        spec.Search.Clear();
                    foreach (var cb in filterCombos)
                    {
                        if (cb.Items.Count > 0 && cb.SelectedIndex != 0)
                            cb.SelectedIndex = 0;
                    }
                };
                flow.Controls.Add(btnResetFilters);
            }

            filterRow.Controls.Add(flow);

            if (spec.CountLabel != null)
            {
                var cl = spec.CountLabel;
                cl.AutoSize = false; cl.Dock = DockStyle.Right; cl.Width = 155;
                cl.TextAlign = ContentAlignment.TopRight; cl.Padding = new Padding(0, 12, 2, 0);
                cl.Font = new Font(ThemeManager.FontFamily, 8.5F); cl.ForeColor = ThemeManager.Muted; cl.BackColor = Color.White;
                filterRow.Controls.Add(cl);
                cl.BringToFront();
                flow.BringToFront();
            }

            // Tự động cân đối chiều rộng ô tìm kiếm và chiều cao hàng lọc khi có nhiều chip lọc (3-5 bộ lọc)
            bool adjustingFilterLayout = false;
            Action syncFilterLayout = () =>
            {
                if (adjustingFilterLayout || filterRow.IsDisposed || flow.ClientSize.Width <= 40) return;
                adjustingFilterLayout = true;
                try
                {
                    if (btnResetFilters != null)
                    {
                        bool anyActive = (spec.Search != null && spec.Search.TextLength > 0);
                        if (!anyActive)
                        {
                            foreach (var cb in filterCombos)
                                if (cb.SelectedIndex > 0) { anyActive = true; break; }
                        }
                        if (btnResetFilters.Visible != anyActive)
                            btnResetFilters.Visible = anyActive;
                    }

                    if (btnFilter != null)
                    {
                        int active = 0;
                        foreach (var cb in filterCombos) if (cb.SelectedIndex > 0) active++;
                        string text = active > 0 ? "Bộ lọc (" + active + ") ▾" : "Bộ lọc ▾";
                        if (btnFilter.Text != text)
                        {
                            btnFilter.Text = text;
                            ButtonSkin.SetKind(btnFilter, active > 0 ? ButtonSkin.Kind.Soft : ButtonSkin.Kind.Secondary);
                        }
                    }

                    if (filterMenuMode)
                    {
                        FitFilterRowSingleLine(flow, searchPill, filterChips);
                        if (filterRow.Height != 46) filterRow.Height = 46;
                        return;
                    }

                    if (searchPill != null)
                    {
                        int others = 0;
                        foreach (Control c in flow.Controls)
                            if (c != searchPill && c.Visible) others += c.Width + c.Margin.Horizontal;

                        int availSingleRow = flow.ClientSize.Width - others - searchPill.Margin.Horizontal - 6;
                        int targetWidth;
                        if (availSingleRow >= 175)
                        {
                            // Đủ chỗ nằm gọn trên 1 hàng
                            targetWidth = Math.Min(270, availSingleRow);
                        }
                        else
                        {
                            // Nhiều bộ lọc -> cho phép xuống hàng thứ 2 cân đối, giữ ô tìm kiếm rộng rãi dễ gõ
                            targetWidth = Math.Min(250, Math.Max(185, (int)(flow.ClientSize.Width * 0.32)));
                        }
                        if (Math.Abs(searchPill.Width - targetWidth) > 2)
                            searchPill.Width = targetWidth;
                    }

                    int maxBottom = 0;
                    foreach (Control c in flow.Controls)
                    {
                        if (c.Visible && c.Bottom + c.Margin.Bottom > maxBottom)
                            maxBottom = c.Bottom + c.Margin.Bottom;
                    }
                    int desiredHeight = Math.Max(46, maxBottom + 4);
                    if (filterRow.Height != desiredHeight)
                        filterRow.Height = desiredHeight;
                }
                finally
                {
                    adjustingFilterLayout = false;
                }
            };

            flow.Layout += (s, e) => syncFilterLayout();
            flow.Resize += (s, e) => syncFilterLayout();
            if (spec.Search != null) spec.Search.TextChanged += (s, e) => syncFilterLayout();
            foreach (var cb in filterCombos) cb.SelectedIndexChanged += (s, e) => syncFilterLayout();

            Panel footer = null;
            bool hasFooter = (spec.SecondaryActions != null && spec.SecondaryActions.Length > 0)
                             || (spec.DangerActions != null && spec.DangerActions.Length > 0) || spec.FooterInfo != null;
            if (hasFooter)
            {
                footer = new Panel { Dock = DockStyle.Bottom, Height = 46, BackColor = Color.White };
                footer.Paint += (s, e) => { using (var pen = new Pen(ThemeManager.Line)) e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0); };
                var fl = new FlowLayoutPanel { Dock = DockStyle.Left, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, BackColor = Color.White, Padding = new Padding(0, 6, 0, 0) };
                if (spec.SecondaryActions != null)
                    foreach (var b in spec.SecondaryActions) { ButtonSkin.SetKind(b, ButtonSkin.Kind.Secondary); FitButton(b); b.Margin = new Padding(0, 0, 8, 0); fl.Controls.Add(b); }
                if (spec.DangerActions != null)
                    foreach (var b in spec.DangerActions) { ButtonSkin.SetKind(b, ButtonSkin.Kind.Danger); FitButton(b); b.Margin = new Padding(0, 0, 8, 0); fl.Controls.Add(b); }
                footer.Controls.Add(fl);
                if (spec.FooterInfo != null)
                {
                    var fi = spec.FooterInfo;
                    fi.AutoSize = false; fi.Dock = DockStyle.Fill; fi.TextAlign = ContentAlignment.MiddleRight;
                    fi.Padding = new Padding(12, 4, 2, 0); fi.AutoEllipsis = true; fi.UseMnemonic = false;
                    fi.Font = new Font(ThemeManager.FontFamily, 8.5F); fi.ForeColor = ThemeManager.Muted; fi.BackColor = Color.White;
                    footer.Controls.Add(fi);
                    fi.BringToFront();
                }
            }

            if (spec.Grid != null)
            {
                var g = spec.Grid;
                g.Dock = DockStyle.Fill;
                g.BackgroundColor = Color.White;
                g.BorderStyle = BorderStyle.None;
                UiStyler.StyleGrid(g);   // áp kiểu bảng ngay (dòng gọn, header chữ hoa xám, pill trạng thái)
                FitColumns(g);
                card.Controls.Add(g);
            }
            if (footer != null) card.Controls.Add(footer);
            card.Controls.Add(filterRow);
            if (spec.Grid != null) spec.Grid.BringToFront();

            // 6. Khối bên phải (nếu có)
            RoundedPanel side = null;
            if (spec.SidePanel != null)
            {
                side = NewCard();
                side.Dock = DockStyle.Right;
                side.Width = Math.Max(spec.SideWidth, spec.SidePanel.Width + 8 * 2 + 40 + 4);
                side.Padding = new Padding(16, 12, 16, 12);
                spec.SidePanel.Dock = DockStyle.Fill;
                spec.SidePanel.BackColor = Color.White;
                side.Controls.Add(spec.SidePanel);
            }

            // 7. Lắp vào form theo thứ tự trên -> dưới
            // (Dock xử lý từ control thêm sau cùng ngược về trước)
            form.Controls.Add(card);
            if (spec.SideFullHeight)
            {
                // Khối phải dock ngay dưới tiêu đề, trước hàng KPI -> cao tới đáy; KPI chỉ trải phần bên trái
                if (kpiRow != null) form.Controls.Add(kpiRow);
                if (side != null) form.Controls.Add(side);
            }
            else
            {
                if (side != null) form.Controls.Add(side);
                if (kpiRow != null) form.Controls.Add(kpiRow);
            }
            form.Controls.Add(header);
            card.BringToFront();

            form.ResumeLayout(true);
        }

        /// <summary>
        /// Cho cột tự giãn theo khung (tránh thanh cuộn ngang), giữ tỉ lệ độ rộng mà Designer đã đặt.
        /// </summary>
        public static void FitColumns(DataGridView g)
        {
            Action fit = () =>
            {
                if (g.IsDisposed || g.Columns.Count == 0 || g.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.Fill) return;
                foreach (DataGridViewColumn c in g.Columns)
                {
                    if (!c.Visible) continue;
                    if (c.AutoSizeMode != DataGridViewAutoSizeColumnMode.NotSet && c.AutoSizeMode != DataGridViewAutoSizeColumnMode.None) continue;
                    int w = Math.Max(40, c.Width);
                    if (c.MinimumWidth <= 5) c.MinimumWidth = Math.Max(50, (int)(w * 0.55));
                    c.FillWeight = Math.Min(1000, w);
                }
                g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            };
            g.DataBindingComplete += (s, e) => fit();
            g.ColumnAdded += (s, e) => { if (g.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.Fill) e.Column.FillWeight = Math.Min(1000, Math.Max(40, e.Column.Width)); };
            fit();
        }

        public static RoundedPanel NewCard()
        {
            return new RoundedPanel
            {
                Radius = 14,
                Shadow = 8,
                FillColor = Color.White,
                BorderColor = Color.FromArgb(120, ThemeManager.Line)
            };
        }

        /// <summary>Đặt kích thước nút theo chữ (chiều cao chuẩn 34).</summary>
        /// <summary>
        /// Giữ hàng lọc trên 1 hàng. Thiếu chỗ thì lần lượt: (1) thu hẹp ô tìm kiếm (270 → 150px),
        /// (2) rút ngắn chip đang bật (tối thiểu 90px, chữ cắt "…", tooltip hiện đủ),
        /// (3) tạm ẩn bớt chip từ cuối (nút "Bộ lọc (n)" và menu vẫn đủ thông tin).
        /// </summary>
        private static void FitFilterRowSingleLine(FlowLayoutPanel flow, PillSearch search, List<ComboChip> chips)
        {
            const int SearchMin = 150, SearchMax = 270, ChipMin = 90;
            int room = flow.ClientSize.Width - flow.Padding.Horizontal - 6;
            foreach (Control c in flow.Controls)
                if (c != search && !(c is ComboChip) && c.Visible) room -= c.Width + c.Margin.Horizontal;
            if (search != null) room -= search.Margin.Horizontal;

            var active = new List<ComboChip>();
            foreach (var chip in chips) if (chip.Combo.SelectedIndex > 0) active.Add(chip);
            int natural = 0;
            foreach (var chip in active) natural += chip.NaturalWidth + chip.Margin.Horizontal;

            int searchW = search != null ? Math.Min(SearchMax, Math.Max(SearchMin, room - natural)) : 0;
            int chipRoom = room - searchW;

            // Mặc định: chip hiện đủ, không giới hạn
            foreach (var chip in chips) { chip.MaxWidth = 0; chip.Suppressed = false; }

            if (natural > chipRoom && active.Count > 0)
            {
                int margin = active[0].Margin.Horizontal;
                // Số chip hiện được khi mỗi chip tối thiểu ChipMin
                int show = Math.Max(0, Math.Min(active.Count, chipRoom / (ChipMin + margin)));
                for (int i = 0; i < active.Count; i++)
                {
                    if (i >= show) { active[i].Suppressed = true; continue; }
                    active[i].MaxWidth = Math.Max(ChipMin, chipRoom / show - margin);
                }
            }

            if (search != null && Math.Abs(search.Width - searchW) > 2) search.Width = searchW;
        }

        /// <summary>
        /// Menu "Bộ lọc" gọn: mỗi dòng "Nhãn ......... giá trị ▸", submenu là các lựa chọn,
        /// "↺ Đặt lại" chỉ hiện khi có bộ lọc đang bật.
        /// </summary>
        private static void ShowFilterMenu(Button owner, List<ComboChip> chips)
        {
            var font = new Font(ThemeManager.FontFamily, 8.5F);
            var itemPad = new Padding(4, 1, 4, 1);
            Func<ToolStripDropDownMenu, ToolStripDropDownMenu> compact = m =>
            {
                m.ShowImageMargin = false;
                m.ShowCheckMargin = false;
                m.Padding = new Padding(2);
                m.Font = font;
                m.BackColor = Color.White;
                return m;
            };

            var menu = new ContextMenuStrip();
            compact(menu);
            bool anyActive = false;
            foreach (var chip in chips)
            {
                var cb = chip.Combo;
                bool active = cb.SelectedIndex > 0;
                anyActive |= active;
                string value = active ? cb.GetItemText(cb.SelectedItem) : "Tất cả";
                var parent = new ToolStripMenuItem(chip.Caption.Length > 0 ? chip.Caption : "Lọc")
                {
                    ShortcutKeyDisplayString = value,   // giá trị hiện tại căn phải
                    Padding = itemPad,
                    ForeColor = active ? ThemeManager.Primary : ThemeManager.Ink2
                };
                if (active) parent.Font = new Font(font, FontStyle.Bold);
                compact((ToolStripDropDownMenu)parent.DropDown);
                for (int i = 0; i < cb.Items.Count; i++)
                {
                    int idx = i;
                    bool sel = idx == cb.SelectedIndex;
                    var it = new ToolStripMenuItem((sel ? "✓  " : "     ") + cb.GetItemText(cb.Items[i]))
                    {
                        Padding = itemPad,
                        ForeColor = sel ? ThemeManager.Primary : ThemeManager.Ink2
                    };
                    it.Click += (s, e) => { cb.SelectedIndex = idx; };
                    parent.DropDownItems.Add(it);
                }
                menu.Items.Add(parent);
            }
            if (anyActive)
            {
                var reset = new ToolStripMenuItem("↺  Đặt lại") { Padding = itemPad, ForeColor = ThemeManager.Muted };
                reset.Click += (s, e) =>
                {
                    foreach (var chip in chips)
                        if (chip.Combo.Items.Count > 0 && chip.Combo.SelectedIndex != 0) chip.Combo.SelectedIndex = 0;
                };
                menu.Items.Add(reset);
            }
            menu.Closed += (s, e) => owner.BeginInvoke((Action)(() => { menu.Dispose(); font.Dispose(); }));
            menu.Show(owner, new Point(0, owner.Height + 2));
        }

        public static void FitButton(Button b, int min = 84)
        {
            b.Font = new Font(ThemeManager.FontFamily, 8.5F, FontStyle.Bold);
            b.AutoSize = false;
            b.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            b.Height = ButtonHeight;
            b.TextAlign = ContentAlignment.MiddleCenter;
            // Độ rộng tính theo chữ thật và tự cập nhật khi font / chữ / DPI thay đổi
            ButtonSkin.EnableAutoFit(b, min);
        }

        /// <summary>Kiểu chung cho một form nhập liệu / hộp thoại.</summary>
        public static void StyleDialog(Form dlg, Label title = null, Label subtitle = null, Button primary = null, Button cancel = null)
        {
            dlg.BackColor = Color.White;
            if (title != null)
            {
                title.Text = SkinUtil.CleanText(title.Text);
                title.Font = new Font(ThemeManager.FontFamily, 14F, FontStyle.Bold);
                title.ForeColor = ThemeManager.Ink; title.BackColor = Color.Transparent; title.UseMnemonic = false;
            }
            if (subtitle != null)
            {
                subtitle.Font = new Font(ThemeManager.FontFamily, 8.5F);
                subtitle.ForeColor = ThemeManager.Muted; subtitle.BackColor = Color.Transparent;
            }
            if (primary != null) ButtonSkin.SetKind(primary, ButtonSkin.Kind.Primary);
            if (cancel != null) ButtonSkin.SetKind(cancel, ButtonSkin.Kind.Secondary);
        }
    }
}

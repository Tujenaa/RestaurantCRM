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
        public const int ButtonHeight = 38;

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
            form.Padding = new Padding(14, 6, 14, 10);
            form.AutoScroll = false;

            // 3. Hàng đầu: tiêu đề + mô tả + nút
            var header = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = ThemeManager.Bg };
            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(0, 14, 8, 0)
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
                title.Font = new Font(ThemeManager.FontFamily, 17F, FontStyle.Bold);
                title.ForeColor = ThemeManager.Ink; title.BackColor = Color.Transparent;
                title.AutoSize = true; title.UseMnemonic = false; title.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                header.Controls.Add(title);
            }
            if (sub != null)
            {
                sub.Font = new Font(ThemeManager.FontFamily, 9F);
                sub.ForeColor = ThemeManager.Muted; sub.BackColor = Color.Transparent;
                sub.AutoSize = true; sub.UseMnemonic = false; sub.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                header.Controls.Add(sub);
            }
            Action placeTitle = () =>
            {
                bool embedded = !form.TopLevel;
                if (title != null) { title.Visible = !embedded; title.Location = new Point(10, 8); }
                if (sub != null) sub.Location = new Point(12, embedded ? 16 : (title == null ? 22 : 44));
                header.Height = embedded ? 54 : (title == null ? 64 : 76);
                actions.Padding = new Padding(0, embedded ? 4 : 14, 8, 0);
            };
            // Mô tả dài thì cắt "..." thay vì đè lên các nút bên phải
            Action fitSub = () =>
            {
                if (sub == null) return;
                sub.AutoSize = false;
                sub.AutoEllipsis = true;
                sub.Height = 20;
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
                var tl = new TableLayoutPanel { Dock = DockStyle.Top, Height = 104, ColumnCount = spec.KpiLabels.Length, RowCount = 1, BackColor = ThemeManager.Bg, Margin = new Padding(0), Padding = new Padding(0) };
                tl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                for (int i = 0; i < spec.KpiLabels.Length; i++)
                {
                    tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / spec.KpiLabels.Length));
                    var tile = new KpiTile(spec.KpiLabels[i]) { Dock = DockStyle.Fill };
                    tl.Controls.Add(tile, i, 0);
                }
                kpiRow = tl;
            }
            else if (spec.KpiHost != null)
            {
                var host = new Panel { Dock = DockStyle.Top, Height = 104, BackColor = ThemeManager.Bg };
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
            card.Padding = new Padding(8 + 20, 4 + 8, 8 + 20, 4 + 8 + 10);

            var filterRow = new Panel { Dock = DockStyle.Top, Height = 62, BackColor = Color.White };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.White, Padding = new Padding(0, 10, 0, 0) };
            if (spec.Search != null)
            {
                var pill = new PillSearch(spec.Search, spec.SearchPlaceholder) { Width = 300, Margin = new Padding(0, 0, 10, 0) };
                flow.Controls.Add(pill);
            }
            if (spec.Filters != null)
                foreach (var f in spec.Filters)
                {
                    if (f.Combo == null) continue;
                    var chip = new ComboChip(f.Combo, f.Caption != null ? f.Caption.Text : null) { Margin = new Padding(0, 0, 10, 0) };
                    flow.Controls.Add(chip);
                    f.Combo.Visible = false;            // vẫn giữ trong form để DataSource hoạt động
                    filterRow.Controls.Add(f.Combo);
                }
            if (spec.FilterExtras != null)
                foreach (var c in spec.FilterExtras)
                {
                    var b = c as Button;
                    if (b != null) { ButtonSkin.SetKind(b, ButtonSkin.Kind.Secondary); FitButton(b); }
                    c.Margin = new Padding(0, 0, 10, 0);
                    flow.Controls.Add(c);
                }
            filterRow.Controls.Add(flow);
            // Ô tìm kiếm co lại (300 → 180px) khi thẻ hẹp để các chip lọc không bị cắt
            PillSearch searchPill = null;
            foreach (Control c in flow.Controls) if (c is PillSearch) searchPill = (PillSearch)c;
            if (searchPill != null)
            {
                flow.Resize += (s, e) =>
                {
                    int others = 0;
                    foreach (Control c in flow.Controls) if (c != searchPill && c.Visible) others += c.Width + c.Margin.Horizontal;
                    int w = Math.Max(180, Math.Min(300, flow.ClientSize.Width - others - searchPill.Margin.Horizontal - 4));
                    if (searchPill.Width != w) searchPill.Width = w;
                };
            }
            if (spec.CountLabel != null)
            {
                var cl = spec.CountLabel;
                cl.AutoSize = false; cl.Dock = DockStyle.Right; cl.Width = 170;
                cl.TextAlign = ContentAlignment.MiddleRight; cl.Padding = new Padding(0, 8, 2, 0);
                cl.Font = new Font(ThemeManager.FontFamily, 8.5F); cl.ForeColor = ThemeManager.Muted; cl.BackColor = Color.White;
                filterRow.Controls.Add(cl);
                cl.BringToFront();
                flow.BringToFront();
            }

            Panel footer = null;
            bool hasFooter = (spec.SecondaryActions != null && spec.SecondaryActions.Length > 0)
                             || (spec.DangerActions != null && spec.DangerActions.Length > 0) || spec.FooterInfo != null;
            if (hasFooter)
            {
                footer = new Panel { Dock = DockStyle.Bottom, Height = 58, BackColor = Color.White };
                footer.Paint += (s, e) => { using (var pen = new Pen(ThemeManager.Line)) e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0); };
                var fl = new FlowLayoutPanel { Dock = DockStyle.Left, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, BackColor = Color.White, Padding = new Padding(0, 12, 0, 0) };
                if (spec.SecondaryActions != null)
                    foreach (var b in spec.SecondaryActions) { ButtonSkin.SetKind(b, ButtonSkin.Kind.Secondary); FitButton(b); b.Margin = new Padding(0, 0, 8, 0); fl.Controls.Add(b); }
                if (spec.DangerActions != null)
                    foreach (var b in spec.DangerActions) { ButtonSkin.SetKind(b, ButtonSkin.Kind.Danger); FitButton(b); b.Margin = new Padding(0, 0, 8, 0); fl.Controls.Add(b); }
                footer.Controls.Add(fl);
                if (spec.FooterInfo != null)
                {
                    var fi = spec.FooterInfo;
                    fi.AutoSize = false; fi.Dock = DockStyle.Fill; fi.TextAlign = ContentAlignment.MiddleRight;
                    fi.Padding = new Padding(12, 6, 2, 0); fi.AutoEllipsis = true; fi.UseMnemonic = false;
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
                UiStyler.StyleGrid(g);   // áp kiểu bảng ngay (dòng cao, header chữ hoa xám, pill trạng thái)
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
                side.Padding = new Padding(8 + 20, 4 + 18, 8 + 20, 4 + 8 + 16);
                spec.SidePanel.Dock = DockStyle.Fill;
                spec.SidePanel.BackColor = Color.White;
                side.Controls.Add(spec.SidePanel);
            }

            // 7. Lắp vào form theo thứ tự trên -> dưới
            form.Controls.Add(card);
            if (side != null) form.Controls.Add(side);
            if (kpiRow != null) form.Controls.Add(kpiRow);
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

        /// <summary>Đặt kích thước nút theo chữ (chiều cao chuẩn 38).</summary>
        public static void FitButton(Button b, int min = 96)
        {
            b.Font = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold);
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
                title.Font = new Font(ThemeManager.FontFamily, 15F, FontStyle.Bold);
                title.ForeColor = ThemeManager.Ink; title.BackColor = Color.Transparent; title.UseMnemonic = false;
            }
            if (subtitle != null)
            {
                subtitle.Font = new Font(ThemeManager.FontFamily, 9F);
                subtitle.ForeColor = ThemeManager.Muted; subtitle.BackColor = Color.Transparent;
            }
            if (primary != null) ButtonSkin.SetKind(primary, ButtonSkin.Kind.Primary);
            if (cancel != null) ButtonSkin.SetKind(cancel, ButtonSkin.Kind.Secondary);
        }
    }
}
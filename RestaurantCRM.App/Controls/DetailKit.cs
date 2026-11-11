using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    /// <summary>Một dòng mặt hàng trong khung chi tiết (món ăn / nguyên liệu).</summary>
    public class DetailItem
    {
        public string Name;
        public string Code;
        public string Qty;
        public string Amount;
    }

    /// <summary>
    /// Khung chi tiết đặt bên phải danh sách (dùng với ListPageSpec.SidePanel), cùng kiểu trang Phản hồi:
    ///   ┌ #MÃ   [pill]        [Chi tiết ↗] ┐  tiêu đề mảnh
    ///   │ Món ăn (n)                       │
    ///   │ lưới mặt hàng (vùng chính)       │  ưu tiên
    ///   │ Tiền món · Voucher · TỔNG        │
    ///   └──────────────────────────────────┘
    /// </summary>
    public class DetailSidePanel : Panel
    {
        private const TextFormatFlags F = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis;

        private readonly Panel _title = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.White };
        private readonly Label _itemsCaption = new Label { Dock = DockStyle.Top, Height = 30, UseMnemonic = false, TextAlign = ContentAlignment.MiddleLeft };
        private readonly DataGridView _grid = new DataGridView();
        private readonly Panel _totals = new Panel { Dock = DockStyle.Bottom, BackColor = Color.White };
        private readonly Label _empty = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, UseMnemonic = false };
        private readonly Label _gridEmpty = new Label { TextAlign = ContentAlignment.MiddleCenter, UseMnemonic = false, Visible = false };
        private Button _button;

        // Dữ liệu đang hiển thị
        private string _code, _pillText;
        private Color _pillFore, _pillBack;
        private bool _autoPill = true;
        private List<KeyValuePair<string, string>> _totalLines = new List<KeyValuePair<string, string>>();
        private string _totalCaption, _totalValue;

        public string EmptyText { get => _empty.Text; set => _empty.Text = value; }

        public DetailSidePanel(string emptyText, string nameHeader, string qtyHeader, string amountHeader)
        {
            Width = 340;
            BackColor = Color.White;
            SkinUtil.EnableDoubleBuffer(_title);
            SkinUtil.EnableDoubleBuffer(_totals);
            _title.Paint += PaintTitle;
            _title.Resize += (s, e) => LayoutButton();
            _totals.Paint += PaintTotals;

            _itemsCaption.Font = new Font(ThemeManager.FontFamily, 10F, FontStyle.Bold);
            _itemsCaption.ForeColor = ThemeManager.Ink;
            _itemsCaption.BackColor = Color.White;

            var g = _grid;
            g.Dock = DockStyle.Fill;
            g.AllowUserToAddRows = false; g.AllowUserToDeleteRows = false; g.AllowUserToResizeRows = false; g.AllowUserToResizeColumns = false;
            g.ReadOnly = true; g.MultiSelect = false; g.RowHeadersVisible = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.ScrollBars = ScrollBars.Vertical;
            UiStyler.StyleGrid(g);
            g.RowTemplate.Height = 46;
            g.Columns.Add(Col("Name", nameHeader, 58, DataGridViewContentAlignment.MiddleLeft));
            g.Columns.Add(Col("Qty", qtyHeader, 16, DataGridViewContentAlignment.MiddleCenter));
            g.Columns.Add(Col("Amount", amountHeader, 30, DataGridViewContentAlignment.MiddleRight));
            g.Columns["Qty"].DefaultCellStyle.Font = new Font(ThemeManager.FontFamily, 10F, FontStyle.Bold);
            g.Columns["Amount"].DefaultCellStyle.Font = new Font(ThemeManager.FontFamily, 9.5F, FontStyle.Bold);
            g.Columns["Amount"].DefaultCellStyle.ForeColor = ThemeManager.Ink;
            g.CellPainting += PaintNameWithCode;
            // Khung chỉ để xem: không giữ dòng chọn tô màu
            g.SelectionChanged += (s, e) => { if (g.SelectedRows.Count > 0) g.ClearSelection(); };

            _gridEmpty.ForeColor = ThemeManager.Muted;
            _gridEmpty.BackColor = Color.White;
            _gridEmpty.Font = new Font(ThemeManager.FontFamily, 9F);
            g.Controls.Add(_gridEmpty);
            g.Resize += (s, e) => _gridEmpty.SetBounds(0, g.ColumnHeadersHeight, g.Width, Math.Max(40, g.Height - g.ColumnHeadersHeight));

            _empty.Text = emptyText;
            _empty.Font = new Font(ThemeManager.FontFamily, 9.5F);
            _empty.ForeColor = ThemeManager.Muted;
            _empty.BackColor = Color.White;

            // Thứ tự Dock: Fill (lưới) thêm trước, các khối Top/Bottom thêm sau
            Controls.Add(_grid);
            Controls.Add(_itemsCaption);
            Controls.Add(_title);
            Controls.Add(_totals);
            Controls.Add(_empty);
            _grid.BringToFront();

            ShowEmpty();
        }

        private static DataGridViewTextBoxColumn Col(string name, string header, float weight, DataGridViewContentAlignment align)
        {
            var c = new DataGridViewTextBoxColumn { Name = name, HeaderText = header, FillWeight = weight, SortMode = DataGridViewColumnSortMode.NotSortable, MinimumWidth = 40 };
            c.DefaultCellStyle.Alignment = align;
            c.HeaderCell.Style.Alignment = align;
            return c;
        }

        /// <summary>Gắn nút "Chi tiết" (nút có sẵn của form, giữ nguyên sự kiện Click) vào góc phải dòng tiêu đề.</summary>
        public void AttachButton(Button button, string text)
        {
            _button = button;
            button.Parent?.Controls.Remove(button);
            button.Text = text;
            button.Anchor = AnchorStyles.None;
            button.Margin = new Padding(0);
            ButtonSkin.SetKind(button, ButtonSkin.Kind.Secondary);
            PageKit.FitButton(button, 96);
            _title.Controls.Add(button);
            button.SizeChanged += (s, e) => LayoutButton();
            LayoutButton();
            button.Visible = _code != null;
        }

        private void LayoutButton()
        {
            if (_button == null) return;
            _button.Location = new Point(_title.ClientSize.Width - _button.Width, Math.Max(0, (_title.Height - 8 - _button.Height) / 2));
        }

        // ── Trạng thái hiển thị ─────────────────────────────────────────

        public void ShowEmpty()
        {
            _code = null;
            SetContentVisible(false);
            if (_button != null) _button.Visible = false;
        }

        /// <summary>Trống: chỉ hiện dòng hướng dẫn. Có dữ liệu: hiện các khối nội dung.</summary>
        private void SetContentVisible(bool visible)
        {
            SuspendLayout();
            _title.Visible = _itemsCaption.Visible = _grid.Visible = _totals.Visible = visible;
            _empty.Visible = !visible;
            ResumeLayout(true);
        }

        public void ShowLoading(string code)
        {
            SetHeader(code, null);
            _itemsCaption.Text = "Đang tải...";
            _grid.Rows.Clear();
            _gridEmpty.Visible = false;
            _totalLines = new List<KeyValuePair<string, string>>();
            _totalCaption = _totalValue = null;
            Refresh2();
        }

        /// <summary>Mã chứng từ + pill trạng thái (màu tự nhận diện theo chữ).</summary>
        public void SetHeader(string code, string status)
        {
            _code = code ?? "—";
            _pillText = status;
            _autoPill = true;
            if (_empty.Visible) SetContentVisible(true);
            if (_button != null) _button.Visible = true;
            _title.Invalidate();
        }

        /// <summary>Mã chứng từ + pill màu cố định.</summary>
        public void SetHeader(string code, string pill, Color fore, Color back)
        {
            SetHeader(code, pill);
            _autoPill = false;
            _pillFore = fore; _pillBack = back;
        }

        public void SetItems(string caption, IList<DetailItem> items, string emptyText)
        {
            _itemsCaption.Text = caption;
            _grid.Rows.Clear();
            foreach (var it in items)
            {
                int i = _grid.Rows.Add(it.Name, it.Qty, it.Amount);
                _grid.Rows[i].Cells[0].Tag = it.Code;
            }
            _grid.ClearSelection();
            _gridEmpty.Text = emptyText;
            _gridEmpty.Visible = items.Count == 0;
        }

        public void SetTotals(IList<KeyValuePair<string, string>> lines, string totalCaption, string totalValue)
        {
            _totalLines = new List<KeyValuePair<string, string>>(lines);
            _totalCaption = totalCaption;
            _totalValue = totalValue;
            _totals.Height = 14 + _totalLines.Count * 22 + 38;
            _totals.Invalidate();
        }

        private void Refresh2()
        {
            _title.Invalidate();
            _totals.Invalidate();
        }

        // ── Vẽ ──────────────────────────────────────────────────────────

        private void PaintTitle(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.White);
            if (_code == null) return;
            int right = _button != null && _button.Visible ? _button.Left - 10 : _title.Width;
            int x = 0;
            using (var f = new Font(ThemeManager.FontFamily, 12.5F, FontStyle.Bold))
            {
                var sz = TextRenderer.MeasureText(g, _code, f, Size.Empty, TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
                int w = Math.Min(sz.Width, Math.Max(40, right - 110));
                TextRenderer.DrawText(g, _code, f, new Rectangle(0, 8, w, sz.Height), ThemeManager.Ink, F);
                x = w + 10;
            }
            if (!string.IsNullOrEmpty(_pillText))
            {
                if (_autoPill) CrmPaint.StatusPill(g, _pillText, x, 11);
                else CrmPaint.Pill(g, _pillText, x, 11, _pillFore, _pillBack);
            }
            using (var p = new Pen(ThemeManager.Line)) g.DrawLine(p, 0, _title.Height - 1, _title.Width, _title.Height - 1);
        }

        private void PaintTotals(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.White);
            if (_code == null) return;
            int w = _totals.Width;
            using (var p = new Pen(ThemeManager.Line)) g.DrawLine(p, 0, 0, w, 0);
            int y = 10;
            using (var f = new Font(ThemeManager.FontFamily, 9F))
            {
                foreach (var kv in _totalLines)
                {
                    TextRenderer.DrawText(g, kv.Key, f, new Rectangle(0, y, w / 2, 20), ThemeManager.Muted, F | TextFormatFlags.VerticalCenter);
                    TextRenderer.DrawText(g, kv.Value, f, new Rectangle(w / 2, y, w - w / 2, 20), ThemeManager.Ink2, F | TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                    y += 22;
                }
            }
            if (_totalValue == null) return;
            using (var fCap = new Font(ThemeManager.FontFamily, 9.5F, FontStyle.Bold))
            using (var fVal = new Font(ThemeManager.FontFamily, 14F, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, _totalCaption, fCap, new Rectangle(0, y, w / 2, 32), ThemeManager.Ink, F | TextFormatFlags.VerticalCenter);
                TextRenderer.DrawText(g, _totalValue, fVal, new Rectangle(w / 3, y, w - w / 3, 32), ThemeManager.Primary, F | TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            }
        }

        private static void PaintNameWithCode(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 0) return;
            var grid = (DataGridView)sender;
            string code = grid.Rows[e.RowIndex].Cells[0].Tag as string;
            e.PaintBackground(e.CellBounds, false);
            var b = e.CellBounds;
            int x = b.X + 12, w = b.Width - 16;
            bool hasCode = !string.IsNullOrWhiteSpace(code);
            using (var fName = new Font(ThemeManager.FontFamily, 9.5F, FontStyle.Bold))
            using (var fCode = new Font(ThemeManager.FontFamily, 7.5F))
            {
                int nameH = fName.Height, codeH = hasCode ? fCode.Height : 0;
                int top = b.Y + (b.Height - nameH - codeH) / 2;
                TextRenderer.DrawText(e.Graphics, Convert.ToString(e.Value), fName, new Rectangle(x, top, w, nameH), ThemeManager.Ink, F);
                if (hasCode)
                    TextRenderer.DrawText(e.Graphics, code, fCode, new Rectangle(x, top + nameH, w, codeH), ThemeManager.Muted, F);
            }
            e.Handled = true;
        }

        // ── Tiện ích ────────────────────────────────────────────────────
        public static KeyValuePair<string, string> Field(string caption, string value) { return new KeyValuePair<string, string>(caption, value); }
        public static string Money(double v) { return v.ToString("#,##0") + " đ"; }
        public static string Money(decimal v) { return v.ToString("#,##0") + " đ"; }
        public static string Or(string s) { return string.IsNullOrWhiteSpace(s) ? "—" : s; }
    }
}

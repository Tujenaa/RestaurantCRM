using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms
{
    public sealed class ModuleField
    {
        public string Name;
        public string Label;
        public bool Required;
        public string[] Options;
        public bool IsImage;
        public ModuleField(string name, string label, bool required, string[] options = null, bool isImage = false)
        {
            Name = name; Label = label; Required = required; Options = options; IsImage = isImage;
        }
    }

    /// <summary>Reusable, self-contained management screen. Data stays in memory until connected to the API.</summary>
    public class ModuleFormBase : Form
    {
        private readonly DataTable _table = new DataTable();
        private readonly UcDataGridView _grid = new UcDataGridView();
        private readonly TextBox _filterValue = new TextBox();
        private int _filterColIndex = 0;   // 0 = tất cả cột, i = cột thứ i-1
        private readonly string[] _columns;
        private int _sortColumn = -1;
        private bool _sortAscending = true;
        private readonly ModuleField[] _fields;
        private readonly Color _accent;
        private readonly string _moduleTitle;
        private Label _countLabel;

        public ModuleFormBase(string title, string description, string[] columns, ModuleField[] fields, string[][] rows, Color accent)
        {
            _moduleTitle = title; _fields = fields; _accent = ThemeManager.Primary; _columns = columns;
            Text = title + " | RestaurantCRM";
            StartPosition = FormStartPosition.CenterParent;

            Size = new Size(1060, 680);
            MinimumSize = new Size(820, 520);
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);

            for (int i = 0; i < columns.Length; i++) _table.Columns.Add("C" + i, typeof(string));
            foreach (string[] row in rows) _table.Rows.Add((object[])row);

            var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(22, 20, 22, 18), BackColor = ThemeManager.Bg };
            Controls.Add(content);

            // ── Tiêu đề trang + nút tròn "+" ─────────────────────────────
            var intro = new Panel { Dock = DockStyle.Top, Height = 74, BackColor = ThemeManager.Bg };
            var titleLabel = new Label { Text = title, Font = new Font("Segoe UI", 17F, FontStyle.Bold), ForeColor = ThemeManager.Ink, AutoSize = true, Location = new Point(6, 2), BackColor = Color.Transparent, UseMnemonic = false };
            var descLabel = new Label { Text = description, Font = new Font("Segoe UI", 9F), ForeColor = ThemeManager.Muted, AutoSize = true, Location = new Point(9, 40), BackColor = Color.Transparent, UseMnemonic = false };
            intro.Controls.Add(titleLabel); intro.Controls.Add(descLabel);

            // Nút chính "+ Thêm mới" (cùng kiểu với các màn danh sách khác)
            var btnAdd = new Button { Text = "+ Thêm mới", Anchor = AnchorStyles.Top | AnchorStyles.Right };
            ButtonSkin.SetKind(btnAdd, ButtonSkin.Kind.Primary);
            PageKit.FitButton(btnAdd, 120);
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Action placeIntro = () =>
            {
                bool embedded = !TopLevel;   // khi mở trong cửa sổ chính, tiêu đề đã nằm trên header
                titleLabel.Visible = !embedded;
                content.Padding = new Padding(22, embedded ? 6 : 20, 22, 18);
                descLabel.Location = new Point(9, embedded ? 16 : 40);
                intro.Height = embedded ? 54 : 74;
                btnAdd.Location = new Point(intro.Width - btnAdd.Width - 10, embedded ? 4 : 16);
            };
            intro.Resize += (s, e) => placeIntro();
            btnAdd.SizeChanged += (s, e) => placeIntro();
            ParentChanged += (s, e) => placeIntro();
            placeIntro();
            btnAdd.Click += (s, e) => OnAdd();
            intro.Controls.Add(btnAdd);

            // ── Thẻ trắng chứa bảng ──────────────────────────────────────
            var card = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Radius = 14,
                Shadow = 8,
                FillColor = ThemeManager.Paper,
                BorderColor = Color.FromArgb(120, ThemeManager.Line),
                Padding = new Padding(8 + 18, 4 + 6, 8 + 18, 16 + 8)
            };
            content.Controls.Add(card);
            content.Controls.Add(intro);

            // Thanh công cụ
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = ThemeManager.Paper };
            card.Controls.Add(toolbar);

            const int ctrlH = 36;
            int ctrlY = (toolbar.Height - ctrlH) / 2;

            // Ô tìm kiếm dạng pill
            var search = new Panel { Location = new Point(0, ctrlY), Size = new Size(300, ctrlH), BackColor = ThemeManager.Paper };
            bool focused = false;
            search.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var rc = new Rectangle(0, 0, search.Width - 1, search.Height - 1);
                using (var path = ThemeManager.RoundedRect(rc, ctrlH / 2))
                {
                    using (var br = new SolidBrush(ThemeManager.Surface2)) g.FillPath(br, path);
                    using (var pen = new Pen(focused ? ThemeManager.Primary : ThemeManager.Line, focused ? 1.5f : 1f)) g.DrawPath(pen, path);
                }
                using (var f = ThemeManager.Icon(9F))
                    TextRenderer.DrawText(g, ThemeManager.Glyph.Search, f, new Rectangle(12, 0, 20, search.Height),
                        focused ? ThemeManager.Primary : ThemeManager.Muted, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            };
            _filterValue.BorderStyle = BorderStyle.None;
            _filterValue.BackColor = ThemeManager.Surface2;
            _filterValue.ForeColor = ThemeManager.Ink;
            _filterValue.Font = new Font("Segoe UI", 9.5F);
            _filterValue.Location = new Point(40, (ctrlH - _filterValue.PreferredHeight) / 2 + 1);
            _filterValue.Width = search.Width - 40 - 34;
            UiStyler.SetPlaceholder(_filterValue, "Tìm kiếm...");
            _filterValue.Enter += (s, e) => { focused = true; search.Invalidate(); };
            _filterValue.Leave += (s, e) => { focused = false; search.Invalidate(); };
            search.Click += (s, e) => _filterValue.Focus();

            var clearBtn = new Label
            {
                Text = ThemeManager.Glyph.Clear,
                Font = ThemeManager.Icon(7.5F),
                ForeColor = ThemeManager.Muted,
                BackColor = ThemeManager.Surface2,
                Size = new Size(20, 20),
                Location = new Point(search.Width - 30, (ctrlH - 20) / 2),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Visible = false
            };
            clearBtn.Click += (s, e) => { _filterValue.Clear(); _filterValue.Focus(); };
            _filterValue.TextChanged += (s, e) => { clearBtn.Visible = _filterValue.TextLength > 0; ApplyAdvancedFilter(); };
            _filterValue.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ApplyAdvancedFilter(); } };
            search.Controls.Add(_filterValue);
            search.Controls.Add(clearBtn);
            toolbar.Controls.Add(search);

            // Chip chọn cột lọc
            var chip = new ChipButton { Location = new Point(search.Right + 10, ctrlY), Height = ctrlH };
            chip.SetLabel("Tất cả cột", false);
            var colMenu = new ContextMenuStrip { ShowImageMargin = false, Font = new Font("Segoe UI", 9.5F), BackColor = Color.White };
            Action rebuildMenu = null;
            rebuildMenu = () =>
            {
                colMenu.Items.Clear();
                for (int i = 0; i <= _columns.Length; i++)
                {
                    int idx = i;
                    string name = i == 0 ? "Tất cả cột" : _columns[i - 1];
                    var it = new ToolStripMenuItem((idx == _filterColIndex ? "✓  " : "     ") + name)
                    { ForeColor = idx == _filterColIndex ? ThemeManager.Primary : ThemeManager.Ink2 };
                    it.Click += (s, e) =>
                    {
                        _filterColIndex = idx;
                        chip.SetLabel(name, idx != 0);
                        rebuildMenu();
                        ApplyAdvancedFilter();
                    };
                    colMenu.Items.Add(it);
                }
            };
            rebuildMenu();
            chip.Click += (s, e) =>
            {
                if (chip.InClearZone)
                {
                    _filterColIndex = 0; chip.SetLabel("Tất cả cột", false); rebuildMenu(); ApplyAdvancedFilter();
                    return;
                }
                colMenu.Show(chip, new Point(0, chip.Height + 4));
            };
            toolbar.Controls.Add(chip);

            // Nút Sửa / Xóa dạng "ghost" bên phải
            var btnDelete = new GhostButton("Xóa", ThemeManager.Glyph.Delete, true) { Size = new Size(84, ctrlH) };
            var btnEdit = new GhostButton("Sửa", ThemeManager.Glyph.Edit, false) { Size = new Size(84, ctrlH) };
            btnEdit.Click += (s, e) => OnEdit();
            btnDelete.Click += (s, e) => OnDelete();
            Action placeRight = () =>
            {
                btnDelete.Location = new Point(toolbar.Width - btnDelete.Width, ctrlY);
                btnEdit.Location = new Point(btnDelete.Left - 8 - btnEdit.Width, ctrlY);
            };
            toolbar.Resize += (s, e) => placeRight();
            toolbar.Controls.Add(btnEdit);
            toolbar.Controls.Add(btnDelete);
            placeRight();

            // Chân thẻ: số bản ghi
            var footer = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = ThemeManager.Paper };
            _countLabel = new Label { AutoSize = true, Location = new Point(2, 16), Font = new Font("Segoe UI", 8.5F), ForeColor = ThemeManager.Muted, BackColor = Color.Transparent };
            footer.Controls.Add(_countLabel);
            footer.Paint += (s, e) => { using (var pen = new Pen(ThemeManager.Line)) e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0); };
            card.Controls.Add(footer);

            // Bảng
            _grid.Dock = DockStyle.Fill;
            _grid.AutoGenerateColumns = false;
            for (int i = 0; i < columns.Length; i++)
            {
                if (i < _fields.Length && _fields[i].IsImage) _grid.Columns.Add(new DataGridViewImageColumn { Name = "col" + i, HeaderText = columns[i], ImageLayout = DataGridViewImageCellLayout.Zoom });
                else _grid.Columns.Add("col" + i, columns[i]);
            }
            _grid.DataSource = _table;
            for (int i = 0; i < columns.Length; i++) _grid.Columns[i].DataPropertyName = "C" + i;
            if (_grid.Columns.Count > 0)
            {
                // Cột mã hiển thị như liên kết xanh (giống "#02313" trong mẫu)
                var idStyle = _grid.Columns[0].DefaultCellStyle;
                idStyle.ForeColor = ThemeManager.Primary;
                idStyle.SelectionForeColor = ThemeManager.Primary;
                idStyle.Font = new Font("Segoe UI", 9F);
                _grid.Columns[0].FillWeight = 70;
            }
            _grid.DataBindingComplete += (s, e) => { foreach (DataGridViewColumn c in _grid.Columns) c.SortMode = DataGridViewColumnSortMode.Programmatic; UpdateCount(); };
            _grid.ColumnHeaderMouseClick += (s, e) => SortByColumn(e.ColumnIndex);
            _grid.ScrollBars = ScrollBars.Vertical;
            _grid.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == 0 && e.RowIndex >= 0)
                {
                    // ghi đè màu dòng xen kẽ để cột mã luôn xanh
                    e.CellStyle.ForeColor = ThemeManager.Primary;
                    e.CellStyle.SelectionForeColor = ThemeManager.Primary;
                }
                if (e.ColumnIndex >= 0 && e.ColumnIndex < _fields.Length && _fields[e.ColumnIndex].IsImage && e.Value != null)
                {
                    string path = e.Value.ToString();
                    if (!string.IsNullOrEmpty(path)) { try { e.Value = Image.FromFile(path); } catch { e.Value = null; } } else e.Value = null;
                }
            };
            _grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    string id = GetSelectedId();
                    DataRow row = SelectedRow();
                    OnRowDoubleClick(id, row);
                }
            };
            _table.DefaultView.ListChanged += (s, e) => UpdateCount();
            UiStyler.StyleGrid(_grid);
            card.Controls.Add(_grid);
            _grid.BringToFront();
            UpdateCount();
        }

        private void UpdateCount()
        {
            if (_countLabel == null) return;
            int shown = _table.DefaultView.Count, total = _table.Rows.Count;
            _countLabel.Text = shown == total
                ? "Hiển thị " + total.ToString("N0") + " bản ghi"
                : "Hiển thị " + shown.ToString("N0") + " / " + total.ToString("N0") + " bản ghi";
        }

        // ─────────────── Các control tự vẽ của thanh công cụ ───────────────

        /// <summary>Nút tròn xanh "+" (giống nút nổi trong mẫu).</summary>
        private class RoundAddButton : Control
        {
            private bool _hover;
            public RoundAddButton()
            {
                Cursor = Cursors.Hand;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
            }
            protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
            protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                int d = Math.Min(Width, Height) - 6;
                var rc = new Rectangle((Width - d) / 2, 1, d, d);
                using (var br = new SolidBrush(Color.FromArgb(40, ThemeManager.Primary))) g.FillEllipse(br, rc.X - 1, rc.Y + 3, rc.Width + 2, rc.Height + 2);
                using (var br = new SolidBrush(_hover ? ThemeManager.PrimaryDark : ThemeManager.Primary)) g.FillEllipse(br, rc);
                using (var f = ThemeManager.Icon(11F))
                    TextRenderer.DrawText(g, ThemeManager.Glyph.Add, f, rc, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
        }

        /// <summary>Chip lọc theo cột: trắng viền mảnh; khi đang lọc thì nền xanh nhạt + dấu ✕.</summary>
        private class ChipButton : Control
        {
            private string _label = "";
            private bool _active, _hover;
            private Point _mouse;
            public bool InClearZone { get { return _active && _mouse.X > Width - 30; } }
            public ChipButton()
            {
                Cursor = Cursors.Hand;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
                BackColor = ThemeManager.Paper;
            }
            public void SetLabel(string label, bool active)
            {
                _label = label; _active = active;
                using (var f = new Font("Segoe UI", 8.5F, FontStyle.Bold))
                    Width = TextRenderer.MeasureText(_label, f).Width + 70;
                Invalidate();
            }
            protected override void OnMouseMove(MouseEventArgs e) { _mouse = e.Location; base.OnMouseMove(e); }
            protected override void OnMouseDown(MouseEventArgs e) { _mouse = e.Location; base.OnMouseDown(e); }
            protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
            protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(ThemeManager.Paper);
                var rc = new Rectangle(0, 0, Width - 1, Height - 1);
                using (var path = ThemeManager.RoundedRect(rc, Height / 2))
                {
                    using (var br = new SolidBrush(_active ? ThemeManager.PrimarySoft : (_hover ? ThemeManager.Surface2 : ThemeManager.Paper))) g.FillPath(br, path);
                    using (var pen = new Pen(_active ? Color.FromArgb(90, ThemeManager.Primary) : ThemeManager.Line)) g.DrawPath(pen, path);
                }
                Color c = _active ? ThemeManager.Primary : ThemeManager.Ink2;
                using (var f = ThemeManager.Icon(8.5F))
                    TextRenderer.DrawText(g, ThemeManager.Glyph.Filter, f, new Rectangle(12, 0, 18, Height), c,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                using (var f = new Font("Segoe UI", 8.5F, FontStyle.Bold))
                    TextRenderer.DrawText(g, _label, f, new Rectangle(36, 0, Width - 66, Height), c,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                using (var f = ThemeManager.Icon(7F))
                    TextRenderer.DrawText(g, _active ? ThemeManager.Glyph.Clear : ThemeManager.Glyph.ChevronDown, f,
                        new Rectangle(Width - 28, 0, 16, Height), _active ? ThemeManager.Primary : ThemeManager.Muted,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
        }

        /// <summary>Nút phụ không viền: icon + chữ, hover nền nhạt (đỏ nhạt nếu là nút xóa).</summary>
        private class GhostButton : Control
        {
            private readonly string _glyph;
            private readonly bool _danger;
            private bool _hover;
            public GhostButton(string text, string glyph, bool danger)
            {
                Text = text; _glyph = glyph; _danger = danger;
                Cursor = Cursors.Hand;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
                BackColor = ThemeManager.Paper;
            }
            protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
            protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(ThemeManager.Paper);
                if (_hover)
                    using (var path = ThemeManager.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 9))
                    using (var br = new SolidBrush(_danger ? ThemeManager.RedSoft : ThemeManager.PrimarySoft))
                        g.FillPath(br, path);
                Color c = _hover ? (_danger ? ThemeManager.Red : ThemeManager.Primary) : ThemeManager.Ink2;
                using (var fi = ThemeManager.Icon(9F))
                using (var f = new Font("Segoe UI", 9F, FontStyle.Bold))
                {
                    var ts = TextRenderer.MeasureText(g, Text, f, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    int total = 16 + 8 + ts.Width;
                    int x = (Width - total) / 2;
                    TextRenderer.DrawText(g, _glyph, fi, new Rectangle(x, 0, 16, Height), c,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    TextRenderer.DrawText(g, Text, f, new Rectangle(x + 24, 0, ts.Width + 2, Height), c,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                }
            }
        }

        protected virtual void OnAdd()
        {
            EditRow(null);
        }

        protected virtual void OnEdit()
        {
            EditSelected();
        }

        protected virtual void OnDelete()
        {
            DeleteSelected();
        }

        protected virtual void OnRowDoubleClick(string id, DataRow row)
        {
            if (row != null) EditRow(row);
        }

        public void SetTableData(System.Collections.Generic.IEnumerable<object[]> rows)
        {
            if (_grid.InvokeRequired)
            {
                _grid.Invoke(new Action(() => SetTableData(rows)));
                return;
            }
            _table.Rows.Clear();
            if (rows != null)
            {
                foreach (var r in rows) _table.Rows.Add(r);
            }
            // Mặc định sắp xếp tăng dần theo cột Mã (C0) để hiển thị KS01, KS02... hoặc KH01, KH02...
            if (_table.Columns.Count > 0)
            {
                _table.DefaultView.Sort = "[C0] ASC";
            }
        }

        public string GetSelectedId()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.IsNewRow) return null;
            return _grid.CurrentRow.Cells[0].Value?.ToString();
        }

        private Button MakeButton(string text, bool primary)
        {
            var b = new Button
            {
                Text = text,
                Size = new Size(82, 34),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                BackColor = primary ? _accent : ThemeManager.Paper,
                ForeColor = primary ? Color.White : ThemeManager.Ink2
            };
            b.FlatAppearance.BorderColor = primary ? _accent : ThemeManager.Line;
            b.FlatAppearance.BorderSize = 1;
            if (primary)
            {
                b.MouseEnter += (s, e) => b.BackColor = ThemeManager.PrimaryDark;
                b.MouseLeave += (s, e) => b.BackColor = _accent;
            }
            else
            {
                b.MouseEnter += (s, e) => b.BackColor = ThemeManager.Surface2;
                b.MouseLeave += (s, e) => b.BackColor = ThemeManager.Paper;
            }
            return b;
        }

        private void ApplySearch()
        {
            ApplyAdvancedFilter();
        }

        private void ApplyAdvancedFilter()
        {
            string value = _filterValue.Text.Trim().Replace("'", "''");
            if (string.IsNullOrWhiteSpace(value))
            {
                _table.DefaultView.RowFilter = "";
                return;
            }

            string safeValue = value.Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]");

            if (_filterColIndex <= 0)
            {
                // Tìm tất cả cột
                string filter = "";
                foreach (DataColumn column in _table.Columns)
                {
                    filter += (filter.Length == 0 ? "" : " OR ") + "Convert([" + column.ColumnName + "], 'System.String') LIKE '%" + safeValue + "%'";
                }
                _table.DefaultView.RowFilter = filter;
            }
            else
            {
                // Tìm trong cột được chọn
                int colIdx = _filterColIndex - 1;
                string column = "C" + colIdx;
                _table.DefaultView.RowFilter = "Convert([" + column + "], 'System.String') LIKE '%" + safeValue + "%'";
            }
        }

        private void SortByColumn(int index)
        {
            if (index < 0 || index >= _table.Columns.Count) return;
            _sortAscending = _sortColumn == index ? !_sortAscending : true;
            _sortColumn = index;
            _table.DefaultView.Sort = "[C" + index + "] " + (_sortAscending ? "ASC" : "DESC");
            for (int i = 0; i < _grid.Columns.Count; i++) _grid.Columns[i].HeaderCell.SortGlyphDirection = i == index ? (_sortAscending ? SortOrder.Ascending : SortOrder.Descending) : SortOrder.None;
        }

        public DataRow SelectedRow()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.IsNewRow) return null;
            var view = _grid.CurrentRow.DataBoundItem as DataRowView;
            return view == null ? null : view.Row;
        }

        private void EditSelected()
        {
            DataRow row = SelectedRow();
            if (row == null) { MessageBox.Show(this, "Hãy chọn một dòng để sửa.", _moduleTitle, MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            EditRow(row);
        }

        private void DeleteSelected()
        {
            DataRow row = SelectedRow();
            if (row == null) { MessageBox.Show(this, "Hãy chọn một dòng để xóa.", _moduleTitle, MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (MessageBox.Show(this, "Xóa bản ghi đã chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) row.Delete();
        }

        private void EditRow(DataRow row)
        {
            using (var dialog = new Form())
            {
                dialog.Text = row == null ? "Thêm " + _moduleTitle : "Sửa " + _moduleTitle;
                dialog.StartPosition = FormStartPosition.CenterParent; dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false; dialog.MinimizeBox = false; dialog.ShowInTaskbar = false;
                dialog.BackColor = ThemeManager.Paper; dialog.Font = Font;
                var heading = new Label { Text = dialog.Text, Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = ThemeManager.Ink, Location = new Point(22, 18), AutoSize = true };
                dialog.Controls.Add(heading);
                var editors = new Control[_fields.Length];
                int currentY = 62;
                for (int i = 0; i < _fields.Length; i++)
                {
                    var f = _fields[i];
                    dialog.Controls.Add(new Label { Text = f.Label + (f.Required ? " *" : ""), Location = new Point(24, currentY), ForeColor = ThemeManager.Muted, AutoSize = true });
                    Control editor;
                    if (f.IsImage)
                    {
                        var panel = new Panel { Location = new Point(24, currentY + 21), Width = 470, Height = 95 };
                        var thumbsPanel = new FlowLayoutPanel { Location = new Point(0, 0), Size = new Size(350, 95), WrapContents = false, AutoScroll = true };
                        var btn = new Button { Text = "Chọn ảnh", Location = new Point(360, 30), Size = new Size(100, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.White, Cursor = Cursors.Hand };
                        var txtPath = new TextBox { Visible = false };
                        Action<string> loadThumbs = (paths) => {
                            thumbsPanel.Controls.Clear();
                            if (string.IsNullOrEmpty(paths)) return;
                            foreach (string p in paths.Split('|'))
                            {
                                if (string.IsNullOrEmpty(p)) continue;
                                var pic = new PictureBox { Size = new Size(75, 75), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.White, Margin = new Padding(0, 0, 10, 0) };
                                try { pic.ImageLocation = p; } catch { }
                                thumbsPanel.Controls.Add(pic);
                            }
                        };
                        btn.Click += (s, e) => {
                            using (var ofd = new OpenFileDialog { Filter = "Ảnh|*.jpg;*.png;*.jpeg", Multiselect = true })
                            {
                                if (ofd.ShowDialog() == DialogResult.OK) { txtPath.Text = string.Join("|", ofd.FileNames); loadThumbs(txtPath.Text); }
                            }
                        };
                        if (row != null && i < _table.Columns.Count)
                        {
                            string paths = Convert.ToString(row[i]);
                            txtPath.Text = paths; loadThumbs(paths);
                        }
                        panel.Controls.Add(thumbsPanel); panel.Controls.Add(btn); panel.Controls.Add(txtPath);
                        editor = txtPath; dialog.Controls.Add(panel);
                        currentY += 125;
                    }
                    else if (f.Options != null && f.Options.Length > 0)
                    {
                        var cbo = new ComboBox { Location = new Point(24, currentY + 21), Width = 470, Font = Font, DropDownStyle = ComboBoxStyle.DropDownList };
                        cbo.Items.AddRange(f.Options);
                        if (row != null && i < _table.Columns.Count)
                        {
                            string val = Convert.ToString(row[i]);
                            if (cbo.Items.Contains(val)) cbo.SelectedItem = val;
                        }
                        if (cbo.SelectedIndex < 0 && cbo.Items.Count > 0) cbo.SelectedIndex = 0;
                        editor = cbo; currentY += 62;
                    }
                    else
                    {
                        var txt = new TextBox { Location = new Point(24, currentY + 21), Width = 470, Font = Font };
                        if (row != null && i < _table.Columns.Count) txt.Text = Convert.ToString(row[i]);
                        editor = txt; currentY += 62;
                    }
                    dialog.Controls.Add(editor); editors[i] = editor;
                }
                dialog.ClientSize = new Size(520, Math.Max(240, currentY + 70));
                var save = new Button { Text = "Lưu thay đổi", DialogResult = DialogResult.None, BackColor = _accent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(130, 36), Location = new Point(364, dialog.ClientSize.Height - 52) };
                save.Click += (s, e) => {
                    for (int i = 0; i < _fields.Length; i++)
                    {
                        if (_fields[i].Required && String.IsNullOrWhiteSpace(editors[i].Text))
                        {
                            MessageBox.Show(dialog, "Vui lòng nhập " + _fields[i].Label + ".", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            editors[i].Focus(); return;
                        }
                    }
                    dialog.DialogResult = DialogResult.OK;
                };
                var cancel = new Button { Text = "Hủy", DialogResult = DialogResult.Cancel, Size = new Size(85, 36), Location = new Point(270, dialog.ClientSize.Height - 52), FlatStyle = FlatStyle.Flat };
                dialog.Controls.Add(save); dialog.Controls.Add(cancel); dialog.AcceptButton = save; dialog.CancelButton = cancel;
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                if (row == null) { object[] values = new object[_table.Columns.Count]; for (int i = 0; i < values.Length; i++) values[i] = i < editors.Length ? editors[i].Text.Trim() : ""; _table.Rows.Add(values); }
                else for (int i = 0; i < editors.Length && i < _table.Columns.Count; i++) row[i] = editors[i].Text.Trim();
            }
        }
    }
}
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms {
    public sealed class ModuleField {
        public string Name;
        public string Label;
        public bool Required;
        public string[] Options;
        public bool IsImage;
        public ModuleField(string name, string label, bool required, string[] options = null, bool isImage = false) {
            Name = name; Label = label; Required = required; Options = options; IsImage = isImage;
        }
    }

    /// <summary>Reusable, self-contained management screen. Data stays in memory until connected to the API.</summary>
    public class ModuleFormBase : Form {
        private readonly DataTable _table = new DataTable();
        private readonly UcDataGridView _grid = new UcDataGridView();
        private readonly ComboBox _filterColumn = new ComboBox();
        private readonly ComboBox _filterMode = new ComboBox();
        private readonly TextBox _filterValue = new TextBox();
        private int _sortColumn = -1;
        private bool _sortAscending = true;
        private readonly ModuleField[] _fields;
        private readonly Color _accent;
        private readonly string _moduleTitle;

        public ModuleFormBase(string title, string description, string[] columns, ModuleField[] fields, string[][] rows, Color accent) {
            _moduleTitle = title; _fields = fields; _accent = accent;
            Text = title + " | RestaurantCRM";
            StartPosition = FormStartPosition.CenterParent;

            Size = new Size(960, 640);
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);

            for (int i = 0; i < columns.Length; i++) _table.Columns.Add("C" + i, typeof(string));
            foreach (string[] row in rows) _table.Rows.Add((object[])row);

            var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24), BackColor = ThemeManager.Bg };
            Controls.Add(content);
            var intro = new Panel { Dock = DockStyle.Top, Height = 78, BackColor = ThemeManager.Bg };
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.Paper, Padding = new Padding(0) };
            content.Controls.Add(panel);
            content.Controls.Add(intro);
            var titleLabel = new Label { Text = title, Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = ThemeManager.Ink, AutoSize = true, Location = new Point(0, 0) };
            var descLabel = new Label { Text = description, Font = new Font("Segoe UI", 9.5F), ForeColor = ThemeManager.Muted, AutoSize = true, Location = new Point(2, 39) };
            intro.Controls.Add(titleLabel); intro.Controls.Add(descLabel);

            panel.Paint += (s, e) => { using (var pen = new Pen(ThemeManager.Line)) e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1); };

            // Toolbar - premium tonal design
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = ThemeManager.Paper
            };
            toolbar.Paint += (s, e) => {
                using (var pen = new Pen(ThemeManager.Line, 1))
                    e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
                // top accent gradient strip
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Rectangle(0, 0, toolbar.Width, 3),
                    _accent, ThemeManager.Violet,
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(brush, 0, 0, toolbar.Width, 3);
            };
            panel.Controls.Add(toolbar);

            // Bên phải: Các nút hành động (Thêm mới, Sửa, Xóa) neo sát cạnh phải
            var btnAdd = MakeButton("＋ Thêm mới", true);
            btnAdd.Size = new Size(114, 34);
            btnAdd.Margin = new Padding(6, 0, 0, 0);
            btnAdd.Click += (s, e) => OnAdd();

            var btnEdit = MakeButton("✏  Sửa", false);
            btnEdit.Size = new Size(80, 34);
            btnEdit.Margin = new Padding(6, 0, 0, 0);
            btnEdit.Click += (s, e) => OnEdit();

            var btnDelete = MakeButton("🗑  Xóa", false);
            btnDelete.Size = new Size(80, 34);
            btnDelete.Margin = new Padding(6, 0, 0, 0);
            btnDelete.ForeColor = ThemeManager.Red;
            btnDelete.FlatAppearance.BorderColor = Color.FromArgb(220, ThemeManager.Red);
            btnDelete.Click += (s, e) => OnDelete();

            var pnlRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(0, 11, 14, 0),
                BackColor = Color.Transparent
            };
            pnlRight.Controls.Add(btnAdd);
            pnlRight.Controls.Add(btnEdit);
            pnlRight.Controls.Add(btnDelete);
            toolbar.Controls.Add(pnlRight);

            // Bên trái: Bộ lọc tìm kiếm neo sát cạnh trái
            _filterColumn.DropDownStyle = ComboBoxStyle.DropDownList;
            _filterColumn.Width = 130;
            _filterColumn.Font = Font;
            _filterColumn.Margin = new Padding(0, 0, 6, 0);
            _filterColumn.Items.Add("Tất cả cột");
            foreach (string column in columns) _filterColumn.Items.Add(column);
            _filterColumn.SelectedIndex = 0;

            _filterMode.DropDownStyle = ComboBoxStyle.DropDownList;
            _filterMode.Width = 72;
            _filterMode.Font = Font;
            _filterMode.Margin = new Padding(0, 0, 6, 0);
            _filterMode.Items.AddRange(new object[] { "Chứa", "Bằng" });
            _filterMode.SelectedIndex = 0;

            _filterValue.Width = 170;
            _filterValue.Font = Font;
            _filterValue.Margin = new Padding(0, 0, 6, 0);
            _filterValue.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ApplyAdvancedFilter(); } };

            var applyFilter = MakeButton("🔍  Lọc", false);
            applyFilter.Size = new Size(75, 34);
            applyFilter.Margin = new Padding(0, 0, 6, 0);
            applyFilter.Click += (s, e) => ApplyAdvancedFilter();

            var clearFilter = MakeButton("✕ Xóa lọc", false);
            clearFilter.Size = new Size(80, 34);
            clearFilter.Margin = new Padding(0, 0, 0, 0);
            clearFilter.Click += (s, e) => { _filterValue.Clear(); _filterColumn.SelectedIndex = 0; ApplyAdvancedFilter(); };

            var pnlLeft = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(14, 11, 0, 0),
                BackColor = Color.Transparent
            };
            pnlLeft.Controls.Add(_filterColumn);
            pnlLeft.Controls.Add(_filterMode);
            pnlLeft.Controls.Add(_filterValue);
            pnlLeft.Controls.Add(applyFilter);
            pnlLeft.Controls.Add(clearFilter);
            toolbar.Controls.Add(pnlLeft);

            _grid.Dock = DockStyle.Fill;
            _grid.AutoGenerateColumns = false;
            for (int i = 0; i < columns.Length; i++) {
                if (i < _fields.Length && _fields[i].IsImage) _grid.Columns.Add(new DataGridViewImageColumn { Name = "col" + i, HeaderText = columns[i], ImageLayout = DataGridViewImageCellLayout.Zoom });
                else _grid.Columns.Add("col" + i, columns[i]);
            }
            _grid.DataSource = _table;
            for (int i = 0; i < columns.Length; i++) _grid.Columns[i].DataPropertyName = "C" + i;
            _grid.DataBindingComplete += (s, e) => { foreach (DataGridViewColumn c in _grid.Columns) c.SortMode = DataGridViewColumnSortMode.Programmatic; };
            _grid.ColumnHeaderMouseClick += (s, e) => SortByColumn(e.ColumnIndex);
            _grid.CellFormatting += (s, e) => {
                if (e.ColumnIndex >= 0 && e.ColumnIndex < _fields.Length && _fields[e.ColumnIndex].IsImage && e.Value != null) {
                    string path = e.Value.ToString();
                    if (!string.IsNullOrEmpty(path)) { try { e.Value = Image.FromFile(path); } catch { e.Value = null; } } else e.Value = null;
                }
            };
            _grid.CellDoubleClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    string id = GetSelectedId();
                    DataRow row = SelectedRow();
                    OnRowDoubleClick(id, row);
                }
            };
            panel.Controls.Add(_grid);
            _grid.BringToFront();
        }

        protected virtual void OnAdd() {
            EditRow(null);
        }

        protected virtual void OnEdit() {
            EditSelected();
        }

        protected virtual void OnDelete() {
            DeleteSelected();
        }

        protected virtual void OnRowDoubleClick(string id, DataRow row) {
            if (row != null) EditRow(row);
        }

        public void SetTableData(System.Collections.Generic.IEnumerable<object[]> rows) {
            if (_grid.InvokeRequired) {
                _grid.Invoke(new Action(() => SetTableData(rows)));
                return;
            }
            _table.Rows.Clear();
            if (rows != null) {
                foreach (var r in rows) _table.Rows.Add(r);
            }
            // Mặc định sắp xếp tăng dần theo cột Mã (C0) để hiển thị KS01, KS02... hoặc KH01, KH02...
            if (_table.Columns.Count > 0) {
                _table.DefaultView.Sort = "[C0] ASC";
            }
        }

        public string GetSelectedId() {
            if (_grid.CurrentRow == null || _grid.CurrentRow.IsNewRow) return null;
            return _grid.CurrentRow.Cells[0].Value?.ToString();
        }

        private Button MakeButton(string text, bool primary) {
            var b = new Button {
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
            if (primary) {
                b.MouseEnter += (s, e) => b.BackColor = ThemeManager.PrimaryDark;
                b.MouseLeave += (s, e) => b.BackColor = _accent;
            } else {
                b.MouseEnter += (s, e) => b.BackColor = ThemeManager.Surface2;
                b.MouseLeave += (s, e) => b.BackColor = ThemeManager.Paper;
            }
            return b;
        }

        private void ApplySearch() {
            ApplyAdvancedFilter();
        }

        private void ApplyAdvancedFilter() {
            string value = _filterValue.Text.Trim().Replace("'", "''");
            if (string.IsNullOrWhiteSpace(value)) {
                _table.DefaultView.RowFilter = "";
                return;
            }

            if (_filterColumn.SelectedIndex <= 0) {
                // Tất cả cột
                string filter = "";
                foreach (DataColumn column in _table.Columns) {
                    filter += (filter.Length == 0 ? "" : " OR ") + "Convert([" + column.ColumnName + "], 'System.String') LIKE '%" + value + "%'";
                }
                _table.DefaultView.RowFilter = filter;
            } else {
                // Cột cụ thể
                int colIdx = _filterColumn.SelectedIndex - 1;
                string column = "C" + colIdx;
                string extra = _filterMode.SelectedIndex == 1
                    ? "Convert([" + column + "], 'System.String') = '" + value + "'"
                    : "Convert([" + column + "], 'System.String') LIKE '%" + value.Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]") + "%'";
                _table.DefaultView.RowFilter = extra;
            }
        }

        private void SortByColumn(int index) {
            if (index < 0 || index >= _table.Columns.Count) return;
            _sortAscending = _sortColumn == index ? !_sortAscending : true;
            _sortColumn = index;
            _table.DefaultView.Sort = "[C" + index + "] " + (_sortAscending ? "ASC" : "DESC");
            for (int i = 0; i < _grid.Columns.Count; i++) _grid.Columns[i].HeaderCell.SortGlyphDirection = i == index ? (_sortAscending ? SortOrder.Ascending : SortOrder.Descending) : SortOrder.None;
        }

        public DataRow SelectedRow() {
            if (_grid.CurrentRow == null || _grid.CurrentRow.IsNewRow) return null;
            var view = _grid.CurrentRow.DataBoundItem as DataRowView;
            return view == null ? null : view.Row;
        }

        private void EditSelected() {
            DataRow row = SelectedRow();
            if (row == null) { MessageBox.Show(this, "Hãy chọn một dòng để sửa.", _moduleTitle, MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            EditRow(row);
        }

        private void DeleteSelected() {
            DataRow row = SelectedRow();
            if (row == null) { MessageBox.Show(this, "Hãy chọn một dòng để xóa.", _moduleTitle, MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (MessageBox.Show(this, "Xóa bản ghi đã chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) row.Delete();
        }

        private void EditRow(DataRow row) {
            using (var dialog = new Form()) {
                dialog.Text = row == null ? "Thêm " + _moduleTitle : "Sửa " + _moduleTitle;
                dialog.StartPosition = FormStartPosition.CenterParent; dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false; dialog.MinimizeBox = false; dialog.ShowInTaskbar = false;
                dialog.BackColor = ThemeManager.Paper; dialog.Font = Font;
                var heading = new Label { Text = dialog.Text, Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = ThemeManager.Ink, Location = new Point(22, 18), AutoSize = true };
                dialog.Controls.Add(heading);
                var editors = new Control[_fields.Length];
                int currentY = 62;
                for (int i = 0; i < _fields.Length; i++) {
                    var f = _fields[i];
                    dialog.Controls.Add(new Label { Text = f.Label + (f.Required ? " *" : ""), Location = new Point(24, currentY), ForeColor = ThemeManager.Muted, AutoSize = true });
                    Control editor;
                    if (f.IsImage) {
                        var panel = new Panel { Location = new Point(24, currentY + 21), Width = 470, Height = 95 };
                        var thumbsPanel = new FlowLayoutPanel { Location = new Point(0, 0), Size = new Size(350, 95), WrapContents = false, AutoScroll = true };
                        var btn = new Button { Text = "Chọn ảnh", Location = new Point(360, 30), Size = new Size(100, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.White, Cursor = Cursors.Hand };
                        var txtPath = new TextBox { Visible = false };
                        Action<string> loadThumbs = (paths) => {
                            thumbsPanel.Controls.Clear();
                            if (string.IsNullOrEmpty(paths)) return;
                            foreach(string p in paths.Split('|')) {
                                if(string.IsNullOrEmpty(p)) continue;
                                var pic = new PictureBox { Size = new Size(75, 75), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.White, Margin = new Padding(0, 0, 10, 0) };
                                try { pic.ImageLocation = p; } catch {}
                                thumbsPanel.Controls.Add(pic);
                            }
                        };
                        btn.Click += (s, e) => {
                            using (var ofd = new OpenFileDialog { Filter = "Ảnh|*.jpg;*.png;*.jpeg", Multiselect = true }) {
                                if (ofd.ShowDialog() == DialogResult.OK) { txtPath.Text = string.Join("|", ofd.FileNames); loadThumbs(txtPath.Text); }
                            }
                        };
                        if (row != null && i < _table.Columns.Count) {
                            string paths = Convert.ToString(row[i]);
                            txtPath.Text = paths; loadThumbs(paths);
                        }
                        panel.Controls.Add(thumbsPanel); panel.Controls.Add(btn); panel.Controls.Add(txtPath);
                        editor = txtPath; dialog.Controls.Add(panel);
                        currentY += 125;
                    } else if (f.Options != null && f.Options.Length > 0) {
                        var cbo = new ComboBox { Location = new Point(24, currentY + 21), Width = 470, Font = Font, DropDownStyle = ComboBoxStyle.DropDownList };
                        cbo.Items.AddRange(f.Options);
                        if (row != null && i < _table.Columns.Count) {
                            string val = Convert.ToString(row[i]);
                            if (cbo.Items.Contains(val)) cbo.SelectedItem = val;
                        }
                        if (cbo.SelectedIndex < 0 && cbo.Items.Count > 0) cbo.SelectedIndex = 0;
                        editor = cbo; currentY += 62;
                    } else {
                        var txt = new TextBox { Location = new Point(24, currentY + 21), Width = 470, Font = Font };
                        if (row != null && i < _table.Columns.Count) txt.Text = Convert.ToString(row[i]);
                        editor = txt; currentY += 62;
                    }
                    dialog.Controls.Add(editor); editors[i] = editor;
                }
                dialog.ClientSize = new Size(520, Math.Max(240, currentY + 70));
                var save = new Button { Text = "Lưu thay đổi", DialogResult = DialogResult.None, BackColor = _accent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(130, 36), Location = new Point(364, dialog.ClientSize.Height - 52) };
                save.Click += (s, e) => {
                    for (int i = 0; i < _fields.Length; i++) {
                        if (_fields[i].Required && String.IsNullOrWhiteSpace(editors[i].Text)) {
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


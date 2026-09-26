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
        public ModuleField(string name, string label, bool required) {
            Name = name; Label = label; Required = required;
        }
    }

    /// <summary>Reusable, self-contained management screen. Data stays in memory until connected to the API.</summary>
    public class ModuleFormBase : Form {
        private readonly DataTable _table = new DataTable();
        private readonly UcDataGridView _grid = new UcDataGridView();
        private readonly TextBox _search = new TextBox();
        private readonly ModuleField[] _fields;
        private readonly Color _accent;
        private readonly string _moduleTitle;

        public ModuleFormBase(string title, string description, string[] columns, ModuleField[] fields, string[][] rows, Color accent) {
            _moduleTitle = title; _fields = fields; _accent = accent;
            Text = title + " | RestaurantCRM";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(900, 580);
            Size = new Size(1120, 700);
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
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Color.FromArgb(250, 251, 254), Padding = new Padding(14, 10, 14, 8) };
            panel.Controls.Add(toolbar);
            _search.Width = 270; _search.Location = new Point(14, 13); _search.Font = Font;
            _search.ForeColor = ThemeManager.Muted; _search.Text = "Tìm kiếm...";
            _search.GotFocus += (s, e) => { if (_search.Text == "Tìm kiếm...") { _search.Text = ""; _search.ForeColor = ThemeManager.Ink; } };
            _search.LostFocus += (s, e) => { if (_search.Text.Length == 0) { _search.Text = "Tìm kiếm..."; _search.ForeColor = ThemeManager.Muted; } };
            _search.TextChanged += (s, e) => ApplySearch();
            toolbar.Controls.Add(_search);
            var btnDelete = MakeButton("Xóa", false); btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right; btnDelete.Location = new Point(toolbar.Width - 230, 11); btnDelete.Click += (s, e) => DeleteSelected();
            var btnEdit = MakeButton("Sửa", false); btnEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right; btnEdit.Location = new Point(toolbar.Width - 145, 11); btnEdit.Click += (s, e) => EditSelected();
            var btnAdd = MakeButton("＋ Thêm mới", true); btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right; btnAdd.Location = new Point(toolbar.Width - 60, 11); btnAdd.Width = 130; btnAdd.Click += (s, e) => EditRow(null);
            toolbar.Controls.Add(btnDelete); toolbar.Controls.Add(btnEdit); toolbar.Controls.Add(btnAdd);
            toolbar.Resize += (s, e) => { btnDelete.Left = toolbar.ClientSize.Width - 315; btnEdit.Left = toolbar.ClientSize.Width - 230; btnAdd.Left = toolbar.ClientSize.Width - 145; };

            _grid.Dock = DockStyle.Fill;
            _grid.AutoGenerateColumns = false;
            for (int i = 0; i < columns.Length; i++) _grid.Columns.Add("col" + i, columns[i]);
            _grid.DataSource = _table;
            for (int i = 0; i < columns.Length; i++) _grid.Columns[i].DataPropertyName = "C" + i;
            _grid.DataBindingComplete += (s, e) => { foreach (DataGridViewColumn c in _grid.Columns) c.SortMode = DataGridViewColumnSortMode.NotSortable; };
            panel.Controls.Add(_grid); _grid.SendToBack();
        }

        private Button MakeButton(string text, bool primary) {
            var b = new Button { Text = text, Size = new Size(82, 35), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand, BackColor = primary ? _accent : Color.White, ForeColor = primary ? Color.White : ThemeManager.Ink };
            b.FlatAppearance.BorderColor = primary ? _accent : ThemeManager.Line;
            return b;
        }

        private void ApplySearch() {
            if (_search.Text == "Tìm kiếm...") { _table.DefaultView.RowFilter = ""; return; }
            string query = _search.Text.Replace("'", "''");
            string filter = "";
            foreach (DataColumn column in _table.Columns) filter += (filter.Length == 0 ? "" : " OR ") + "Convert([" + column.ColumnName + "], 'System.String') LIKE '%" + query + "%'";
            _table.DefaultView.RowFilter = filter;
        }

        private DataRow SelectedRow() {
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
                dialog.BackColor = ThemeManager.Paper; dialog.Font = Font; dialog.ClientSize = new Size(520, Math.Max(240, 100 + _fields.Length * 66));
                var heading = new Label { Text = dialog.Text, Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = ThemeManager.Ink, Location = new Point(22, 18), AutoSize = true };
                dialog.Controls.Add(heading);
                var editors = new TextBox[_fields.Length];
                for (int i = 0; i < _fields.Length; i++) {
                    int y = 62 + i * 62;
                    dialog.Controls.Add(new Label { Text = _fields[i].Label + (_fields[i].Required ? " *" : ""), Location = new Point(24, y), ForeColor = ThemeManager.Muted, AutoSize = true });
                    editors[i] = new TextBox { Location = new Point(24, y + 21), Width = 470, Font = Font };
                    if (row != null && i < _table.Columns.Count) editors[i].Text = Convert.ToString(row[i]);
                    dialog.Controls.Add(editors[i]);
                }
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

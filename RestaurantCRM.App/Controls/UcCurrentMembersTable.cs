using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls {
    /// <summary>
    /// Thẻ danh sách "Current Team Members" với bảng dữ liệu tối giản, sạch sẽ và hiện đại.
    /// Tái hiện bảng bên dưới góc trái trong giao diện tham khảo.
    /// </summary>
    public class UcCurrentMembersTable : RoundedPanel {
        private readonly DataGridView _grid;

        public UcCurrentMembersTable() {
            Radius = 14;
            FillColor = ThemeManager.Paper;
            BorderColor = ColorTranslator.FromHtml("#E2E8F0");
            Size = new Size(360, 260);

            var lblTitle = new Label {
                Text = "Current Team Members",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                Location = new Point(16, 14),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            Controls.Add(lblTitle);

            var lblSub = new Label {
                Text = "Danh sách nhân sự & khách hàng thân thiết",
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = ThemeManager.Muted,
                Location = new Point(17, 36),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            Controls.Add(lblSub);

            // Bảng DataGridView tối giản
            _grid = new DataGridView {
                Location = new Point(14, 60),
                Size = new Size(Width - 28, Height - 74),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackgroundColor = ThemeManager.Paper,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = ColorTranslator.FromHtml("#F1F5F9"),
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 32 },
                Font = new Font("Segoe UI", 8.5F)
            };

            // Header style
            _grid.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Paper;
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#64748B");
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.Paper;
            _grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = ColorTranslator.FromHtml("#64748B");
            _grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            _grid.ColumnHeadersHeight = 28;

            // Row style
            _grid.DefaultCellStyle.BackColor = ThemeManager.Paper;
            _grid.DefaultCellStyle.ForeColor = ThemeManager.Ink2;
            _grid.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#F5F3FF");
            _grid.DefaultCellStyle.SelectionForeColor = ThemeManager.Primary;

            // Columns
            _grid.Columns.Add("FirstName", "First name");
            _grid.Columns.Add("LastName", "Last name");
            _grid.Columns.Add("Hours", "Hours");

            _grid.Columns[0].FillWeight = 38;
            _grid.Columns[1].FillWeight = 38;
            _grid.Columns[2].FillWeight = 24;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Thêm dữ liệu mẫu theo đúng ảnh minh họa
            _grid.Rows.Add("Kate", "Loren", "44 h");
            _grid.Rows.Add("Anthony", "Murray", "38 h");
            _grid.Rows.Add("Tatiana", "Rivera", "42 h");
            _grid.Rows.Add("Thomas", "Scott", "36 h");
            _grid.Rows.Add("Leif", "Morse", "40 h");

            Controls.Add(_grid);
        }
    }
}

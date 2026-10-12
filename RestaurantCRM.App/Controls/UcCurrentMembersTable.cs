using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls {
    /// <summary>
    /// Bảng thống kê Top món ăn bán chạy & Tình trạng thực đơn trong ngày.
    /// Thay thế bảng nhân sự bằng dữ liệu nhà hàng thực tế, chuyên nghiệp và hữu ích.
    /// </summary>
    public class UcTopMonAnTable : RoundedPanel {
        private readonly DataGridView _grid;

        public UcTopMonAnTable() {
            Radius = 12;
            FillColor = ThemeManager.Paper;
            BorderColor = ThemeManager.Line;
            Size = new Size(360, 260);

            var lblTitle = new Label {
                Text = "🍲 Top món ăn bán chạy hôm nay",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                Location = new Point(16, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            Controls.Add(lblTitle);

            var lblSub = new Label {
                Text = "Thực đơn được khách gọi nhiều nhất trong ngày",
                Font = new Font("Segoe UI", 8F),
                ForeColor = ThemeManager.Muted,
                Location = new Point(18, 34),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            Controls.Add(lblSub);

            // Bảng DataGridView tối giản, chuyên nghiệp
            _grid = new DataGridView {
                Location = new Point(14, 58),
                Size = new Size(Width - 28, Height - 72),
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
                RowTemplate = { Height = 34 },
                Font = new Font("Segoe UI", 8.5F)
            };

            // Header style
            _grid.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Surface2;
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#475569");
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.Surface2;
            _grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = ColorTranslator.FromHtml("#475569");
            _grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            _grid.ColumnHeadersHeight = 30;

            // Row style
            _grid.DefaultCellStyle.BackColor = ThemeManager.Paper;
            _grid.DefaultCellStyle.ForeColor = ThemeManager.Ink2;
            _grid.DefaultCellStyle.SelectionBackColor = ThemeManager.PrimarySoft;
            _grid.DefaultCellStyle.SelectionForeColor = ThemeManager.Primary;

            // Columns
            _grid.Columns.Add("TenMon", "Món ăn");
            _grid.Columns.Add("Gia", "Đơn giá");
            _grid.Columns.Add("DaGoi", "Đã gọi");
            _grid.Columns.Add("TrangThai", "Trạng thái");

            _grid.Columns[0].FillWeight = 42;
            _grid.Columns[1].FillWeight = 22;
            _grid.Columns[2].FillWeight = 16;
            _grid.Columns[3].FillWeight = 20;

            _grid.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _grid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _grid.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Dữ liệu món ăn nhà hàng thực tế
            _grid.Rows.Add("Lẩu thái hải sản", "280.000 đ", "18 phần", "Bán chạy 🔥");
            _grid.Rows.Add("Bò lúc lắc khoai tây", "145.000 đ", "24 phần", "Bán chạy 🔥");
            _grid.Rows.Add("Mực trứng chiên mắm", "120.000 đ", "19 phần", "Còn món");
            _grid.Rows.Add("Gà ta nướng muối ớt", "160.000 đ", "14 phần", "Còn món");
            _grid.Rows.Add("Cơm chiên hải sản", "85.000 đ", "26 phần", "Bán chạy 🔥");
            _grid.Rows.Add("Trà đào cam sả", "45.000 đ", "38 ly", "Bán chạy 🔥");

            Controls.Add(_grid);
        }
    }

    /// <summary>Alias tương thích ngược cho Dashboard</summary>
    public class UcCurrentMembersTable : UcTopMonAnTable { }
}

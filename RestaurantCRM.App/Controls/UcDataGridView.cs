using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls {
    public partial class UcDataGridView : DataGridView {
        public UcDataGridView() {
            Dock = DockStyle.Fill;
            BackgroundColor = ThemeManager.Paper;
            BorderStyle = BorderStyle.None;
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            GridColor = ThemeManager.Line;
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            RowHeadersVisible = false;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            MultiSelect = false;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ReadOnly = true;
            EnableHeadersVisualStyles = false;

            // Header
            ColumnHeadersHeight = 44;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Surface2;
            ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.Ink2;
            ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.Surface2;
            ColumnHeadersDefaultCellStyle.SelectionForeColor = ThemeManager.Ink2;
            ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 8.5F);
            ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // Row
            RowTemplate.Height = 42;
            Font = new Font("Segoe UI", 9.5F);
            DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            DefaultCellStyle.SelectionBackColor = ThemeManager.PrimarySoft;
            DefaultCellStyle.SelectionForeColor = ThemeManager.PrimaryDark;
            DefaultCellStyle.BackColor = ThemeManager.Paper;
            DefaultCellStyle.ForeColor = ThemeManager.Ink2;

            // Alternating row color
            AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.Surface2;
            AlternatingRowsDefaultCellStyle.ForeColor = ThemeManager.Ink2;
            AlternatingRowsDefaultCellStyle.SelectionBackColor = ThemeManager.PrimarySoft;
            AlternatingRowsDefaultCellStyle.SelectionForeColor = ThemeManager.PrimaryDark;
        }
    }
}

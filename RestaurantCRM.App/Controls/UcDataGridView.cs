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
            GridColor = Color.FromArgb(237, 240, 245);
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            RowHeadersVisible = false;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            MultiSelect = false;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ReadOnly = true;
            EnableHeadersVisualStyles = false;
            ColumnHeadersHeight = 42;
            RowTemplate.Height = 40;
            Font = new Font("Segoe UI", 9.5F);
            ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 254);
            ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.Muted;
            ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            DefaultCellStyle.SelectionBackColor = Color.FromArgb(234, 241, 255);
            DefaultCellStyle.SelectionForeColor = ThemeManager.Ink;
            DefaultCellStyle.Padding = new Padding(5, 0, 5, 0);
        }
    }
}

using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    public partial class UcDataGridView : DataGridView
    {
        public UcDataGridView()
        {
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
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.Paper;
            ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.Muted;
            ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.Paper;
            ColumnHeadersDefaultCellStyle.SelectionForeColor = ThemeManager.Muted;
            ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 0, 6, 0);
            ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // Row
            RowTemplate.Height = 48;
            AllowUserToResizeRows = false;
            Font = new Font("Segoe UI", 9F);
            DefaultCellStyle.Padding = new Padding(12, 0, 8, 0);
            DefaultCellStyle.SelectionBackColor = ThemeManager.PrimarySoft;
            DefaultCellStyle.SelectionForeColor = ThemeManager.Ink;
            DefaultCellStyle.BackColor = ThemeManager.Paper;
            DefaultCellStyle.ForeColor = ThemeManager.Ink2;

            // Không sọc — nền trắng sạch
            AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.Paper;
            AlternatingRowsDefaultCellStyle.SelectionBackColor = ThemeManager.PrimarySoft;
            AlternatingRowsDefaultCellStyle.SelectionForeColor = ThemeManager.Ink;
        }
    }
}
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.Dashboard {
    public partial class DashboardForm : Form {
        public DashboardForm() {
            this.Text = "Tổng quan hệ thống";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ThemeManager.Bg;
            this.Font = new Font("Segoe UI", 10F);

            UcSidebar sidebar = new UcSidebar();
            this.Controls.Add(sidebar);

            Panel mainPanel = new Panel { Dock = DockStyle.Fill };
            this.Controls.Add(mainPanel);
            mainPanel.BringToFront();

            UcHeader header = new UcHeader();
            mainPanel.Controls.Add(header);

            FlowLayoutPanel content = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            content.Controls.Add(new UcThongKe("Tài khoản", "4", "Tài khoản hệ thống"));
            content.Controls.Add(new UcThongKe("Món ăn", "3", "Danh mục món ăn"));
            content.Controls.Add(new UcThongKe("Nhà cung cấp", "2", "Đối tác"));
            content.Controls.Add(new UcThongKe("Khách hàng CRM", "3", "Hồ sơ khách hàng"));

            mainPanel.Controls.Add(content);
            content.BringToFront();
        }
    }
}

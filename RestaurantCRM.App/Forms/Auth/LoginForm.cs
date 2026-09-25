using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.Auth
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Hệ thống Quản lý Chuỗi Nhà hàng - Khởi động";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ThemeManager.Bg;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Main container to center content
            Panel mainContainer = new Panel
            {
                Size = new Size(860, 480),
                BackColor = Color.Transparent
            };
            mainContainer.Location = new Point((this.ClientSize.Width - mainContainer.Width) / 2, (this.ClientSize.Height - mainContainer.Height) / 2);
            this.Controls.Add(mainContainer);

            // Hero section
            Panel heroPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.FromArgb(23, 61, 114) // Simulate gradient color
            };
            
            Label lblTitle = new Label
            {
                Text = "HỆ THỐNG QUẢN LÝ CHUỖI NHÀ HÀNG",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(30, 25)
            };
            
            Label lblSubtitle = new Label
            {
                Text = "Prototype giao diện desktop · Chọn phân hệ để đăng nhập và làm việc độc lập.",
                ForeColor = Color.FromArgb(220, 233, 255), // #dce9ff
                Font = new Font("Segoe UI", 11F),
                AutoSize = true,
                Location = new Point(35, 75)
            };

            heroPanel.Controls.Add(lblTitle);
            heroPanel.Controls.Add(lblSubtitle);
            mainContainer.Controls.Add(heroPanel);

            // Cards container
            Panel cardsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 20, 0, 0)
            };
            mainContainer.Controls.Add(cardsPanel);
            cardsPanel.BringToFront();

            // Admin Card
            Panel adminCard = CreateCard(
                "🛡️", 
                "Admin — Quản trị hệ thống", 
                "Quản lý tài khoản, vai trò, quyền truy cập, tra cứu dữ liệu toàn hệ thống và sao lưu dữ liệu.", 
                "Mở Admin →", 
                ThemeManager.Blue
            );
            adminCard.Location = new Point(0, 140);
            
            // CRM Card
            Panel crmCard = CreateCard(
                "💬", 
                "CRM — Quan hệ khách hàng", 
                "Quản lý hồ sơ khách hàng, phản hồi, khảo sát, thống kê và phân tích khách hàng.", 
                "Mở CRM →", 
                ThemeManager.Purple
            );
            crmCard.Location = new Point(440, 140);

            mainContainer.Controls.Add(adminCard);
            mainContainer.Controls.Add(crmCard);
            
            // Subtle footer
            Label lblFooter = new Label
            {
                Text = "Dữ liệu demo. Đây là giao diện WinForms được xây dựng từ HTML prototype.",
                ForeColor = ThemeManager.Muted,
                Font = new Font("Segoe UI", 9F),
                AutoSize = true,
                Location = new Point(0, 430)
            };
            mainContainer.Controls.Add(lblFooter);
        }

        private Panel CreateCard(string icon, string title, string description, string btnText, Color btnColor)
        {
            Panel card = new Panel
            {
                Size = new Size(420, 250),
                BackColor = ThemeManager.Paper,
                BorderStyle = BorderStyle.FixedSingle,
            };

            Label lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 24F),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(20, 70)
            };

            Label lblDesc = new Label
            {
                Text = description,
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeManager.Muted,
                Size = new Size(380, 50),
                Location = new Point(25, 110)
            };

            Button btnOpen = new Button
            {
                Text = btnText,
                BackColor = btnColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Size = new Size(150, 40),
                Location = new Point(25, 180),
                Cursor = Cursors.Hand
            };
            btnOpen.FlatAppearance.BorderSize = 0;
            
            btnOpen.Click += (s, e) => {
                MessageBox.Show($"Bạn đã chọn: {title}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Here we will eventually open the MainForm based on the selected module
            };

            card.Controls.Add(lblIcon);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblDesc);
            card.Controls.Add(btnOpen);

            return card;
        }
    }
}

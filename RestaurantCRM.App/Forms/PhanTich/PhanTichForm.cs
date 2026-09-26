using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.PhanTich {
    public class PhanTichForm : Form {
        public PhanTichForm() {
            Text = "Phân tích khách hàng | RestaurantCRM"; StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1080, 680); MinimumSize = new Size(850, 560); BackColor = ThemeManager.Bg; Font = new Font("Segoe UI", 9.5F);
            var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24), BackColor = ThemeManager.Bg }; Controls.Add(content);
            content.Controls.Add(new Label { Text = "Phân tích khách hàng", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = ThemeManager.Ink, AutoSize = true, Location = new Point(24, 18) });
            content.Controls.Add(new Label { Text = "Tổng hợp chỉ số từ dữ liệu minh họa hiện có.", ForeColor = ThemeManager.Muted, AutoSize = true, Location = new Point(26, 56) });
            var cards = new FlowLayoutPanel { Location = new Point(24, 94), Size = new Size(980, 130), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, WrapContents = false, BackColor = ThemeManager.Bg };
            cards.Controls.Add(new UcThongKe("Khách hàng", "128", "Tổng hồ sơ"));
            cards.Controls.Add(new UcThongKe("Chi tiêu", "24.6 tr", "Tổng ghi nhận"));
            cards.Controls.Add(new UcThongKe("Giá trị đơn TB", "192.000", "Theo đơn hàng"));
            cards.Controls.Add(new UcThongKe("Đánh giá", "4.6 / 5", "42 lượt đánh giá")); content.Controls.Add(cards);
            var panel = new Panel { Location = new Point(24, 244), Size = new Size(980, 310), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, BackColor = ThemeManager.Paper, Padding = new Padding(20) };
            panel.Paint += (s, e) => { using (var pen = new Pen(ThemeManager.Line)) e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1); };
            panel.Controls.Add(new Label { Text = "Khách hàng tiêu biểu theo chi tiêu", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = ThemeManager.Ink, AutoSize = true, Location = new Point(20, 16) });
            string[] names = { "Minh Anh", "Quốc Bảo", "Thu Hà", "Hoàng Nam", "Lan Phương" };
            int[] values = { 88, 71, 57, 42, 28 };
            for (int i = 0; i < names.Length; i++) {
                int y = 62 + i * 44;
                panel.Controls.Add(new Label { Text = names[i], Location = new Point(22, y), Width = 110, ForeColor = ThemeManager.Muted });
                var barBack = new Panel { Location = new Point(140, y + 2), Size = new Size(590, 13), BackColor = Color.FromArgb(237, 240, 245), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                var bar = new Panel { Dock = DockStyle.Left, Width = values[i] * 5, BackColor = ThemeManager.Purple }; barBack.Controls.Add(bar); panel.Controls.Add(barBack);
                panel.Controls.Add(new Label { Text = values[i] + "%", Location = new Point(750, y - 2), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeManager.Ink });
            }
            content.Controls.Add(panel);
            content.Controls.Add(new Label { Text = "Số liệu minh họa — chưa tải từ API hoặc cơ sở dữ liệu.", Location = new Point(26, 570), AutoSize = true, ForeColor = ThemeManager.Muted, Anchor = AnchorStyles.Bottom | AnchorStyles.Left });
        }
    }
}

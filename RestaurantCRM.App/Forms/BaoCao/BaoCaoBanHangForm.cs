using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.BaoCao {
    public class BaoCaoBanHangForm : Form {
        public BaoCaoBanHangForm() {
            Text = "Báo cáo bán hàng | RestaurantCRM"; StartPosition = FormStartPosition.CenterParent; Size = new Size(1080, 680); MinimumSize = new Size(850, 560); BackColor = ThemeManager.Bg; Font = new Font("Segoe UI", 9.5F);
            var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24), BackColor = ThemeManager.Bg }; Controls.Add(content);
            content.Controls.Add(new Label { Text = "Báo cáo bán hàng", Location = new Point(24, 18), AutoSize = true, Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = ThemeManager.Ink });
            content.Controls.Add(new Label { Text = "Theo dõi doanh thu và trạng thái đơn hàng.", Location = new Point(26, 56), AutoSize = true, ForeColor = ThemeManager.Muted });
            var cards = new FlowLayoutPanel { Location = new Point(24, 96), Size = new Size(980, 125), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, WrapContents = false, BackColor = ThemeManager.Bg };
            cards.Controls.Add(new UcThongKe("Doanh thu", "8.420.000 đ", "Hôm nay")); cards.Controls.Add(new UcThongKe("Đơn hàng", "36", "Hôm nay")); cards.Controls.Add(new UcThongKe("Đơn TB", "234.000 đ", "Giá trị trung bình")); cards.Controls.Add(new UcThongKe("Đã hoàn tất", "28", "78% tổng đơn")); content.Controls.Add(cards);
            var panel = new Panel { Location = new Point(24, 242), Size = new Size(980, 300), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, BackColor = ThemeManager.Paper, Padding = new Padding(20) };
            panel.Paint += (s, e) => { using (var pen = new Pen(ThemeManager.Line)) e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1); };
            panel.Controls.Add(new Label { Text = "Doanh thu theo ngày", Location = new Point(20, 16), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = ThemeManager.Ink });
            string[] days = { "T2", "T3", "T4", "T5", "T6", "T7", "CN" }; int[] amounts = { 45, 62, 38, 78, 54, 92, 70 };
            for (int i = 0; i < days.Length; i++) {
                int x = 60 + i * 115, height = amounts[i] * 2;
                var bar = new Panel { Location = new Point(x, 232 - height), Size = new Size(52, height), BackColor = Color.FromArgb(14, 124, 134), Anchor = AnchorStyles.Bottom };
                panel.Controls.Add(bar); panel.Controls.Add(new Label { Text = days[i], Location = new Point(x + 14, 240), AutoSize = true, ForeColor = ThemeManager.Muted });
            }
            content.Controls.Add(panel); content.Controls.Add(new Label { Text = "Số liệu minh họa — chưa lấy từ API hoặc cơ sở dữ liệu.", Location = new Point(26, 558), AutoSize = true, ForeColor = ThemeManager.Muted, Anchor = AnchorStyles.Bottom | AnchorStyles.Left });
        }
    }
}

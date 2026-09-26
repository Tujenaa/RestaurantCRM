using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Forms.BanHang;
using RestaurantCRM.AdminApp.Forms.BaoCao;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Forms.DanhGia;
using RestaurantCRM.AdminApp.Forms.DonHang;
using RestaurantCRM.AdminApp.Forms.KhachHang;
using RestaurantCRM.AdminApp.Forms.KhuyenMai;
using RestaurantCRM.AdminApp.Forms.KhaoSat;
using RestaurantCRM.AdminApp.Forms.MonAn;
using RestaurantCRM.AdminApp.Forms.NhaCungCap;
using RestaurantCRM.AdminApp.Forms.PhanHoi;
using RestaurantCRM.AdminApp.Forms.PhanTich;
using RestaurantCRM.AdminApp.Forms.PhieuNhap;
using RestaurantCRM.AdminApp.Forms.TaiKhoan;
using RestaurantCRM.AdminApp.Forms.VaiTro;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.Dashboard {
    public partial class DashboardForm : Form {
        private readonly string _role;
        private readonly UcHeader _header;
        private readonly Panel _body;
        private readonly Color _accent;

        public DashboardForm() : this("Admin") { }
        public DashboardForm(string role) {
            _role = role; _accent = role == "CRM" ? ThemeManager.Purple : role == "Bán hàng" ? Color.FromArgb(14, 124, 134) : ThemeManager.Blue;
            Text = role + " | RestaurantCRM"; Size = new Size(1280, 820); MinimumSize = new Size(1024, 680);
            StartPosition = FormStartPosition.CenterScreen; BackColor = ThemeManager.Bg; Font = new Font("Segoe UI", 9.5F);

            var main = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.Bg };
            Controls.Add(main);
            var sidebar = new UcSidebar(role); sidebar.MenuSelected += OnMenuSelected; Controls.Add(sidebar); sidebar.BringToFront();
            _body = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = ThemeManager.Bg, Padding = new Padding(24) }; main.Controls.Add(_body);
            _header = new UcHeader(); _header.Dock = DockStyle.Top; _header.Height = 76;
            _header.TitleLabel.Text = "Dashboard"; main.Controls.Add(_header);
            var user = new Label { Text = (role == "CRM" ? "CR  Nhân viên CRM" : role == "Bán hàng" ? "NV  Nhân viên bán hàng" : "AD  Quản trị viên"), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeManager.Ink, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(950, 28) };
            _header.Controls.Add(user); _header.Resize += (s, e) => user.Left = _header.ClientSize.Width - user.Width - 24;
            RenderDashboard();
        }

        private void OnMenuSelected(string item) {
            if (item == "Đăng xuất") { Close(); return; }
            if (item == "Dashboard") { _header.TitleLabel.Text = "Dashboard"; RenderDashboard(); return; }
            Form form = CreateModule(item);
            if (form == null) return;
            form.ShowDialog(this);
        }

        private Form CreateModule(string item) {
            switch (item) {
                case "Món ăn": case "Thực đơn": return new MonAnForm();
                case "Nhà cung cấp": return new NhaCungCapForm();
                case "Phiếu nhập hàng": return new PhieuNhapForm();
                case "Khuyến mãi": return new KhuyenMaiForm();
                case "Tài khoản": return new TaiKhoanForm();
                case "Vai trò & phân quyền": return new VaiTroForm();
                case "Tạo đơn hàng": return new TaoDonHangForm();
                case "Đơn hàng": return new DonHangForm();
                case "Khách hàng": return new KhachHangForm();
                case "Đánh giá": return new DanhGiaForm();
                case "Phản hồi": return new PhanHoiForm();
                case "Khảo sát": return new KhaoSatForm();
                case "Phân tích khách hàng": return new PhanTichForm();
                case "Báo cáo bán hàng": return new BaoCaoBanHangForm();
                default: return null;
            }
        }

        private void RenderDashboard() {
            _body.Controls.Clear();
            string title = _role == "Admin" ? "Tổng quan hệ thống" : _role == "Bán hàng" ? "Tổng quan bán hàng" : "Tổng quan CRM";
            AddLabel(title, 25, 18, 20F, FontStyle.Bold, ThemeManager.Ink);
            AddLabel("Số liệu minh họa cho giao diện; chưa lấy từ API hoặc cơ sở dữ liệu.", 27, 56, 9.5F, FontStyle.Regular, ThemeManager.Muted);
            var cards = new FlowLayoutPanel { Location = new Point(24, 95), Size = new Size(950, 128), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, WrapContents = false, BackColor = ThemeManager.Bg };
            if (_role == "Admin") {
                cards.Controls.Add(new UcThongKe("Món ăn", "24", "Trong thực đơn")); cards.Controls.Add(new UcThongKe("Nhà cung cấp", "8", "Đối tác")); cards.Controls.Add(new UcThongKe("Phiếu nhập", "12", "Tháng này")); cards.Controls.Add(new UcThongKe("Tài khoản", "6", "Nhân viên"));
            } else if (_role == "Bán hàng") {
                cards.Controls.Add(new UcThongKe("Doanh thu", "8.42 tr", "Hôm nay")); cards.Controls.Add(new UcThongKe("Chờ xử lý", "5", "Đơn hàng")); cards.Controls.Add(new UcThongKe("Đang giao", "3", "Đơn hàng")); cards.Controls.Add(new UcThongKe("Hoàn tất", "28", "Hôm nay"));
            } else {
                cards.Controls.Add(new UcThongKe("Khách hàng", "128", "Tổng hồ sơ")); cards.Controls.Add(new UcThongKe("Đánh giá TB", "4.6 / 5", "42 lượt đánh giá")); cards.Controls.Add(new UcThongKe("Phản hồi mới", "4", "Cần xử lý")); cards.Controls.Add(new UcThongKe("Khảo sát mở", "2", "Đang hoạt động"));
            }
            _body.Controls.Add(cards);
            var panel = new Panel { Location = new Point(24, 247), Size = new Size(950, 210), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BackColor = ThemeManager.Paper, Padding = new Padding(20) };
            panel.Paint += (s, e) => { using (var pen = new Pen(ThemeManager.Line)) e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1); };
            panel.Controls.Add(new Label { Text = "Truy cập nhanh", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = ThemeManager.Ink, AutoSize = true, Location = new Point(18, 15) });
            string[] actions = _role == "Admin" ? new[] { "Món ăn", "Nhà cung cấp", "Phiếu nhập hàng", "Khuyến mãi", "Tài khoản", "Vai trò & phân quyền" } : _role == "Bán hàng" ? new[] { "Tạo đơn hàng", "Đơn hàng", "Thực đơn", "Khuyến mãi", "Báo cáo bán hàng" } : new[] { "Khách hàng", "Đánh giá", "Phản hồi", "Khảo sát", "Phân tích khách hàng" };
            int x = 20, y = 62;
            foreach (string action in actions) {
                var button = new Button { Text = action, Size = new Size(190, 42), Location = new Point(x, y), BackColor = Color.White, ForeColor = _accent, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
                button.FlatAppearance.BorderColor = ThemeManager.Line; button.Click += (s, e) => OnMenuSelected(action); panel.Controls.Add(button);
                x += 205; if (x + 190 > panel.Width) { x = 20; y += 54; }
            }
            _body.Controls.Add(panel);
            var note = new Label { Text = "Dữ liệu hiện tại là mẫu giao diện. Các thao tác trên bảng chỉ thay đổi dữ liệu trong phiên làm việc; cần kết nối dịch vụ API để lưu bền vững.", Location = new Point(26, 482), Size = new Size(930, 50), ForeColor = ThemeManager.Muted, BackColor = Color.FromArgb(234, 241, 255), Padding = new Padding(10), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            _body.Controls.Add(note);
        }

        private void AddLabel(string text, int x, int y, float size, FontStyle style, Color color) {
            _body.Controls.Add(new Label { Text = text, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", size, style), ForeColor = color });
        }
    }
}

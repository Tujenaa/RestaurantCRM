using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls {
    /// <summary>
    /// Bảng điều khiển bên phải (Right Sidebar): Calendar Timeline & New Team Members
    /// Tái hiện 100% cột bên phải trong giao diện tham khảo.
    /// </summary>
    public class UcRightPanel : UserControl {
        public UcRightPanel() {
            Width = 270;
            Dock = DockStyle.Right;
            BackColor = ThemeManager.Paper;
            Font = new Font("Segoe UI", 9F);
            Padding = new Padding(18, 16, 18, 16);
            AutoScroll = true;

            BuildLayout();
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            // Đường kẻ viền trái phân cách nhẹ
            using (var pen = new Pen(ColorTranslator.FromHtml("#E2E8F0"), 1))
                e.Graphics.DrawLine(pen, 0, 0, 0, Height);
        }

        private void BuildLayout() {
            Controls.Clear();
            int y = 14;

            // 1. Calendar Header
            var lblCalendar = new Label {
                Text = "Calendar",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                Location = new Point(16, y),
                AutoSize = true
            };
            Controls.Add(lblCalendar);

            // Nút tròn tím cạnh chữ Calendar
            var btnCalDot = new Label {
                Text = "▶",
                Font = new Font("Segoe UI", 7F),
                ForeColor = Color.White,
                BackColor = ColorTranslator.FromHtml("#7C3AED"),
                Size = new Size(24, 24),
                Location = new Point(Width - 52, y + 2),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnCalDot.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = new GraphicsPath()) {
                    path.AddEllipse(0, 0, 23, 23);
                    using (var brush = new SolidBrush(ColorTranslator.FromHtml("#7C3AED")))
                        e.Graphics.FillPath(brush, path);
                }
                TextRenderer.DrawText(e.Graphics, "▶", new Font("Segoe UI", 7F),
                    btnCalDot.ClientRectangle, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            Controls.Add(btnCalDot);

            y += 34;

            // Month subhead
            var lblMonth = new Label {
                Text = "JULY 2024",
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#94A3B8"),
                Location = new Point(17, y),
                AutoSize = true
            };
            Controls.Add(lblMonth);

            y += 20;

            // 2. Timeline items
            var timelineItems = new[] {
                ("10 AM", "Intro to Scrum", "Họp giao ban đầu ca", ColorTranslator.FromHtml("#F97316")),
                ("12 PM", "Office Hours", "Phục vụ cao điểm trưa", ColorTranslator.FromHtml("#06B6D4")),
                ("10 AM", "Company Meeting", "Kiểm tra kho & NCC", ColorTranslator.FromHtml("#8B5CF6")),
                ("12 PM", "Managing People", "Chăm sóc & Phản hồi", ColorTranslator.FromHtml("#10B981"))
            };

            foreach (var item in timelineItems) {
                var pnlItem = new Panel {
                    Location = new Point(16, y),
                    Size = new Size(Width - 36, 42),
                    BackColor = Color.Transparent
                };

                // Giờ (10 AM / 12 PM)
                var lblTime = new Label {
                    Text = item.Item1,
                    Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                    ForeColor = ColorTranslator.FromHtml("#64748B"),
                    Location = new Point(0, 4),
                    Size = new Size(44, 18),
                    TextAlign = ContentAlignment.MiddleRight
                };
                pnlItem.Controls.Add(lblTime);

                // Thanh sọc màu dọc
                var stripe = new Panel {
                    Location = new Point(50, 4),
                    Size = new Size(3, 30),
                    BackColor = item.Item4
                };
                pnlItem.Controls.Add(stripe);

                // Tiêu đề sự kiện
                var lblEventTitle = new Label {
                    Text = item.Item2,
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = ThemeManager.Ink,
                    Location = new Point(58, 2),
                    AutoSize = true
                };
                pnlItem.Controls.Add(lblEventTitle);

                var lblEventSub = new Label {
                    Text = item.Item3,
                    Font = new Font("Segoe UI", 7F),
                    ForeColor = ThemeManager.Muted,
                    Location = new Point(58, 18),
                    AutoSize = true
                };
                pnlItem.Controls.Add(lblEventSub);

                Controls.Add(pnlItem);
                y += 46;
            }

            y += 10;

            // 3. New Team Members Header
            var lblMembersHeader = new Label {
                Text = "New Team Members",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                Location = new Point(16, y),
                AutoSize = true
            };
            Controls.Add(lblMembersHeader);

            y += 30;

            // Members list
            var members = new[] {
                ("Kate Loren", "Quản lý nhà hàng", ColorTranslator.FromHtml("#7C3AED")),
                ("Anthony Murray", "Trưởng ca bán hàng", ColorTranslator.FromHtml("#F97316")),
                ("Tatiana Rivera", "Chuyên viên CRM & CSKH", ColorTranslator.FromHtml("#0D9488")),
                ("Thomas Scott", "Bếp trưởng & Thực đơn", ColorTranslator.FromHtml("#DC2626"))
            };

            foreach (var m in members) {
                var pnlMember = new Panel {
                    Location = new Point(16, y),
                    Size = new Size(Width - 36, 44),
                    BackColor = Color.Transparent
                };

                // Avatar tròn màu sắc với Initials
                var avatar = new Label {
                    Text = ThemeManager.Initials(m.Item1),
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Size = new Size(32, 32),
                    Location = new Point(4, 4),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                Color avColor = m.Item3;
                avatar.Paint += (s, e) => {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var path = new GraphicsPath()) {
                        path.AddEllipse(0, 0, 31, 31);
                        using (var brush = new SolidBrush(avColor))
                            e.Graphics.FillPath(brush, path);
                    }
                    TextRenderer.DrawText(e.Graphics, avatar.Text, avatar.Font,
                        avatar.ClientRectangle, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                };
                pnlMember.Controls.Add(avatar);

                var lblName = new Label {
                    Text = m.Item1,
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                    ForeColor = ThemeManager.Ink,
                    Location = new Point(44, 4),
                    AutoSize = true
                };
                pnlMember.Controls.Add(lblName);

                var lblRole = new Label {
                    Text = m.Item2,
                    Font = new Font("Segoe UI", 7F),
                    ForeColor = ThemeManager.Muted,
                    Location = new Point(44, 22),
                    AutoSize = true
                };
                pnlMember.Controls.Add(lblRole);

                Controls.Add(pnlMember);
                y += 46;
            }
        }
    }
}

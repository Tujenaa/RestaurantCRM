using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    /// <summary>
    /// Hộp thoại chuẩn của ứng dụng: tiêu đề + mô tả ở trên, vùng nội dung ở giữa, hàng nút ở dưới.
    ///   ┌──────────────────────────────────────┐
    ///   │ Tiêu đề                               │
    ///   │ mô tả ngắn                            │
    ///   │ ───────────────────────────────────── │
    ///   │ Body (tự đặt control)                 │
    ///   │ ───────────────────────────────────── │
    ///   │ [nút trái]            [Hủy] [Lưu]     │
    ///   └──────────────────────────────────────┘
    /// </summary>
    public class CrmDialog : Form
    {
        public const int Gutter = 24;
        public readonly Panel Body;
        public readonly Panel Footer;
        public readonly Button PrimaryButton;
        public readonly Button CancelBtn;
        public readonly Label TitleLabel;
        public readonly Label SubtitleLabel;
        private readonly FlowLayoutPanel _left;
        private readonly FlowLayoutPanel _right;
        private string _primaryText;

        public CrmDialog(string title, string subtitle, int width, int bodyHeight, string primaryText = "Lưu", string cancelText = "Hủy")
        {
            Text = title;
            Font = new Font(ThemeManager.FontFamily, 9.5F);
            BackColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false; ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Font;

            var header = new Panel { Dock = DockStyle.Top, Height = subtitle == null ? 62 : 82, BackColor = Color.White };
            TitleLabel = new Label
            {
                Text = title,
                AutoSize = true,
                Location = new Point(Gutter - 2, 18),
                UseMnemonic = false,
                Font = new Font(ThemeManager.FontFamily, 14F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            };
            header.Controls.Add(TitleLabel);
            if (subtitle != null)
            {
                SubtitleLabel = new Label
                {
                    Text = subtitle,
                    AutoSize = false,
                    AutoEllipsis = true,
                    UseMnemonic = false,
                    Location = new Point(Gutter, 50),
                    Size = new Size(width - Gutter * 2, 20),
                    Font = new Font(ThemeManager.FontFamily, 9F),
                    ForeColor = ThemeManager.Muted
                };
                header.Controls.Add(SubtitleLabel);
            }
            header.Paint += (s, e) => { using (var p = new Pen(ThemeManager.Line)) e.Graphics.DrawLine(p, Gutter, header.Height - 1, header.Width - Gutter, header.Height - 1); };

            Footer = new Panel { Dock = DockStyle.Bottom, Height = 70, BackColor = Color.White, Padding = new Padding(0, 1, 0, 0) };   // chừa 1px cho đường kẻ
            Footer.Paint += (s, e) => { using (var p = new Pen(ThemeManager.Line)) e.Graphics.DrawLine(p, Gutter, 0, Footer.Width - Gutter, 0); };
            _right = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 16, Gutter - 8, 0),
                BackColor = Color.White
            };
            _left = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                Padding = new Padding(Gutter - 4, 16, 0, 0),
                BackColor = Color.White
            };
            Footer.Controls.Add(_right);
            Footer.Controls.Add(_left);

            if (primaryText != null)
            {
                PrimaryButton = MakeButton(primaryText, ButtonSkin.Kind.Primary, 104);
                _primaryText = primaryText;
                _right.Controls.Add(PrimaryButton);
            }
            if (cancelText != null)
            {
                CancelBtn = MakeButton(cancelText, ButtonSkin.Kind.Secondary, 88);
                CancelBtn.DialogResult = DialogResult.Cancel;
                CancelBtn.Click += (s, e) => Close();
                _right.Controls.Add(CancelBtn);
                CancelButton = CancelBtn;
            }

            Body = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(Gutter, 14, Gutter, 10) };
            Controls.Add(Body);
            Controls.Add(Footer);
            Controls.Add(header);
            Body.BringToFront();

            ClientSize = new Size(width, header.Height + bodyHeight + Footer.Height);
        }

        /// <summary>
        /// Màn hình nhỏ / phóng to chữ 125–150%: hộp thoại có thể cao hơn màn hình làm mất hàng nút.
        /// Thu hộp thoại vừa vùng làm việc và cho phần nội dung cuộn được.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var wa = Screen.FromControl(Owner ?? (Control)this).WorkingArea;
            int maxH = wa.Height - 16, maxW = wa.Width - 16;
            if (Height > maxH || Width > maxW)
            {
                Body.AutoScroll = true;
                Size = new Size(Math.Min(Width, maxW), Math.Min(Height, maxH));
                Location = new Point(wa.Left + (wa.Width - Width) / 2, wa.Top + (wa.Height - Height) / 2);
            }
        }

        public static Button MakeButton(string text, ButtonSkin.Kind kind, int minWidth = 96)
        {
            var b = new Button { Text = text, Margin = new Padding(8, 0, 0, 0), Cursor = Cursors.Hand };
            ButtonSkin.SetKind(b, kind);
            PageKit.FitButton(b, minWidth);
            return b;
        }

        /// <summary>Thêm một nút ở góc trái hàng nút (ví dụ "Xóa", "Xem trước").</summary>
        public Button AddLeftButton(string text, ButtonSkin.Kind kind)
        {
            var b = MakeButton(text, kind, 88);
            b.Margin = new Padding(0, 0, 8, 0);
            _left.Controls.Add(b);
            return b;
        }

        /// <summary>Đổi chữ nút chính (SetBusy sẽ khôi phục về chữ này).</summary>
        public void SetPrimaryText(string text)
        {
            _primaryText = text;
            if (PrimaryButton != null) PrimaryButton.Text = text;
        }

        /// <summary>Khóa các nút khi đang gọi API để tránh bấm hai lần.</summary>
        public void SetBusy(bool busy, string busyText = "Đang lưu...")
        {
            UseWaitCursor = busy;
            if (PrimaryButton != null)
            {
                PrimaryButton.Enabled = !busy;
                PrimaryButton.Text = busy ? busyText : _primaryText;
            }
            if (CancelBtn != null) CancelBtn.Enabled = !busy;
            foreach (Control c in _left.Controls) c.Enabled = !busy;
        }

        // ── Đặt control trong Body ──────────────────────────────────────────

        /// <summary>Nhãn ô nhập; required = thêm dấu * đỏ.</summary>
        public static Label FieldLabel(Control parent, string text, int x, int y, bool required = false)
        {
            var l = new Label
            {
                Text = text + (required ? " *" : ""),
                AutoSize = true,
                Location = new Point(x, y),
                UseMnemonic = false,
                Font = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink2,
                BackColor = Color.Transparent
            };
            parent.Controls.Add(l);
            return l;
        }

        /// <summary>Đặt ô nhập dưới nhãn. Khoảng cách nhãn → ô là 24px, mỗi hàng ô cao ~64px.</summary>
        public static T Field<T>(Control parent, string label, T input, int x, int y, int width, bool required = false) where T : Control
        {
            FieldLabel(parent, label, x, y, required);
            input.Location = new Point(x, y + 26);
            input.Width = width;
            parent.Controls.Add(input);
            return input;
        }

        /// <summary>Dòng chữ nhỏ màu xám (gợi ý / chú thích).</summary>
        public static Label Hint(Control parent, string text, int x, int y, int width)
        {
            var l = new Label
            {
                Text = text,
                AutoSize = false,
                Location = new Point(x, y),
                Size = new Size(width, 18),
                UseMnemonic = false,
                AutoEllipsis = true,
                Font = new Font(ThemeManager.FontFamily, 8.5F),
                ForeColor = ThemeManager.Muted,
                BackColor = Color.Transparent
            };
            parent.Controls.Add(l);
            return l;
        }
    }

    /// <summary>Hộp thông báo dùng chung (giữ tiêu đề và biểu tượng nhất quán).</summary>
    public static class Notify
    {
        /// <summary>Cửa sổ cấp cao nhất chứa control (màn hình nhúng trong Dashboard không phải top-level).</summary>
        public static IWin32Window OwnerOf(Control c)
        {
            if (c == null) return null;
            var top = c.TopLevelControl;
            return top ?? c;
        }

        public static void Info(Control owner, string message, string title = "Thông báo")
        {
            MessageBox.Show(OwnerOf(owner), message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Success(Control owner, string message)
        {
            MessageBox.Show(OwnerOf(owner), message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Warn(Control owner, string message, string title = "Kiểm tra lại")
        {
            MessageBox.Show(OwnerOf(owner), message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void Error(Control owner, string message, string title = "Không thực hiện được")
        {
            MessageBox.Show(OwnerOf(owner), message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool Confirm(Control owner, string message, string title = "Xác nhận", bool danger = false)
        {
            return MessageBox.Show(OwnerOf(owner), message, title, MessageBoxButtons.YesNo,
                danger ? MessageBoxIcon.Warning : MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        /// <summary>Mở hộp thoại modal, căn giữa cửa sổ chính kể cả khi gọi từ màn hình đang nhúng.</summary>
        public static DialogResult ShowDialog(Form dialog, Control owner)
        {
            return dialog.ShowDialog(OwnerOf(owner));
        }
    }

    /// <summary>Hàm vẽ dùng chung cho các khối tự vẽ (pill trạng thái, avatar chữ cái, thanh tỉ lệ).</summary>
    public static class CrmPaint
    {
        /// <summary>Vẽ pill trạng thái; trả về độ rộng đã vẽ.</summary>
        public static int Pill(Graphics g, string text, int x, int y, Color fore, Color back, float size = 8.5F)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            using (var f = new Font(ThemeManager.FontFamily, size, FontStyle.Bold))
            {
                var sz = TextRenderer.MeasureText(g, text, f, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                var r = new Rectangle(x, y, sz.Width + 18, sz.Height + 8);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = SkinUtil.Round(r, r.Height / 2f))
                using (var br = new SolidBrush(back)) g.FillPath(br, path);
                TextRenderer.DrawText(g, text, f, r, fore, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
                return r.Width;
            }
        }

        /// <summary>Pill theo màu trạng thái tự nhận diện (xanh / đỏ / vàng / xanh dương).</summary>
        public static int StatusPill(Graphics g, string text, int x, int y, float size = 8.5F)
        {
            Color fore, back;
            if (!ThemeManager.TryStatusColors(text, out fore, out back)) { fore = ThemeManager.Ink2; back = ThemeManager.Surface2; }
            return Pill(g, ThemeManager.CleanStatus(text), x, y, fore, back, size);
        }

        /// <summary>Avatar tròn với chữ cái đầu của tên cuối (Nguyễn Văn An -> "A").</summary>
        public static void Avatar(Graphics g, string name, Rectangle r)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var c = ThemeManager.AvatarColor(name ?? "?");
            using (var br = new SolidBrush(Color.FromArgb(36, c))) g.FillEllipse(br, r);
            string ch = "?";
            if (!string.IsNullOrWhiteSpace(name))
            {
                var parts = name.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                ch = parts[parts.Length - 1].Substring(0, 1).ToUpperInvariant();
            }
            using (var f = new Font(ThemeManager.FontFamily, Math.Max(8f, r.Height * 0.36f), FontStyle.Bold, GraphicsUnit.Pixel))
                TextRenderer.DrawText(g, ch, f, r, c, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }

        /// <summary>Thanh tỉ lệ bo tròn (0..1).</summary>
        public static void Bar(Graphics g, Rectangle r, double ratio, Color fill, Color track)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var path = SkinUtil.Round(r, r.Height / 2f))
            using (var br = new SolidBrush(track)) g.FillPath(br, path);
            ratio = Math.Max(0, Math.Min(1, ratio));
            if (ratio <= 0) return;
            int w = Math.Max(r.Height, (int)Math.Round(r.Width * ratio));
            var fr = new Rectangle(r.X, r.Y, w, r.Height);
            using (var path = SkinUtil.Round(fr, r.Height / 2f))
            using (var br = new SolidBrush(fill)) g.FillPath(br, path);
        }

        /// <summary>Vẽ sao đánh giá (★ đặc + ☆ rỗng); trả về độ rộng.</summary>
        public static int Stars(Graphics g, int soSao, int x, int y, float size = 10F)
        {
            soSao = Math.Max(0, Math.Min(5, soSao));
            string full = new string('\u2605', soSao), empty = new string('\u2605', 5 - soSao);
            using (var f = new Font(ThemeManager.FontFamily, size))
            {
                var flags = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
                int w1 = soSao > 0 ? TextRenderer.MeasureText(g, full, f, Size.Empty, flags).Width : 0;
                if (soSao > 0) TextRenderer.DrawText(g, full, f, new Point(x, y), ThemeManager.Amber, flags);
                int w2 = soSao < 5 ? TextRenderer.MeasureText(g, empty, f, Size.Empty, flags).Width : 0;
                if (soSao < 5) TextRenderer.DrawText(g, empty, f, new Point(x + w1, y), Color.FromArgb(222, 225, 234), flags);
                return w1 + w2;
            }
        }
    }
}
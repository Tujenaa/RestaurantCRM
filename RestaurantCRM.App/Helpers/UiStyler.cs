using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RestaurantCRM.AdminApp.Helpers
{
    /// <summary>
    /// Tự động áp phong cách hiện đại cho mọi Form / Control trong ứng dụng:
    /// nút bấm phẳng có hiệu ứng hover, bảng dữ liệu thoáng, ô nhập viền mảnh...
    /// Gọi <see cref="EnableGlobal"/> một lần ở Program.Main là đủ.
    /// Đặt Tag = "nostyle" cho control nào muốn giữ nguyên giao diện gốc.
    /// </summary>
    public static class UiStyler
    {
        public const string NoStyleTag = "nostyle";

        private static readonly ConditionalWeakTable<Control, object> _styled = new ConditionalWeakTable<Control, object>();
        private static bool _enabled;

        // Màu "cũ" được code cứng ở nhiều Form -> ánh xạ sang bảng màu mới
        private static readonly Dictionary<int, Color> _legacyMap = new Dictionary<int, Color> {
            { Color.FromArgb(36, 87, 167).ToArgb(), ThemeManager.Primary },   // #2457a7 (Blue cũ)
            { Color.FromArgb(29, 78, 216).ToArgb(), ThemeManager.Primary },   // xanh dương HoaDon
            { Color.FromArgb(37, 99, 235).ToArgb(), ThemeManager.Primary },
            { Color.FromArgb(23, 61, 114).ToArgb(), ThemeManager.PrimaryDark },
            { Color.FromArgb(14, 124, 134).ToArgb(), ThemeManager.Teal },     // teal Bán hàng cũ
            { Color.FromArgb(11, 77, 71).ToArgb(), ThemeManager.Teal },
            { Color.FromArgb(105, 65, 165).ToArgb(), ThemeManager.Violet },   // #6941a5 (Purple cũ)
            { Color.FromArgb(56, 35, 77).ToArgb(), ThemeManager.Violet },
            { Color.FromArgb(22, 121, 75).ToArgb(), ThemeManager.Green },     // #16794b
            { Color.FromArgb(22, 163, 74).ToArgb(), ThemeManager.Green },
            { Color.FromArgb(180, 35, 24).ToArgb(), ThemeManager.Red },       // #b42318
            { Color.FromArgb(220, 38, 38).ToArgb(), ThemeManager.Red },
            { Color.FromArgb(161, 92, 7).ToArgb(), ThemeManager.Amber },      // #a15c07
            { Color.FromArgb(234, 241, 255).ToArgb(), ThemeManager.PrimarySoft },
            { Color.FromArgb(235, 243, 255).ToArgb(), ThemeManager.PrimarySoft },
            { Color.FromArgb(243, 246, 251).ToArgb(), ThemeManager.Bg },
            // Bảng màu Tailwind cũ dùng nhiều trong các form Designer
            { Color.FromArgb(79, 70, 229).ToArgb(), ThemeManager.Primary },
            { Color.FromArgb(67, 56, 202).ToArgb(), ThemeManager.PrimaryDark },
            { Color.FromArgb(99, 102, 241).ToArgb(), ThemeManager.Primary },
            { Color.FromArgb(59, 130, 246).ToArgb(), ThemeManager.Primary },
            { Color.FromArgb(238, 242, 255).ToArgb(), ThemeManager.PrimarySoft },
            { Color.FromArgb(224, 231, 255).ToArgb(), ThemeManager.PrimarySoft },
            { Color.FromArgb(124, 58, 237).ToArgb(), ThemeManager.Violet },
            { Color.FromArgb(13, 148, 136).ToArgb(), ThemeManager.Teal },
            { Color.FromArgb(5, 150, 105).ToArgb(), ThemeManager.Green },
            { Color.FromArgb(16, 185, 129).ToArgb(), ThemeManager.Green },
            { Color.FromArgb(217, 119, 6).ToArgb(), ThemeManager.Amber },
            { Color.FromArgb(245, 158, 11).ToArgb(), ThemeManager.Amber },
            { Color.FromArgb(239, 68, 68).ToArgb(), ThemeManager.Red },
            { Color.FromArgb(254, 242, 242).ToArgb(), ThemeManager.RedSoft },
            { Color.FromArgb(240, 253, 244).ToArgb(), ThemeManager.GreenSoft },
            { Color.FromArgb(17, 24, 39).ToArgb(), ThemeManager.Ink },
            { Color.FromArgb(15, 23, 42).ToArgb(), ThemeManager.Ink },
            { Color.FromArgb(30, 41, 59).ToArgb(), ThemeManager.Ink },
            { Color.FromArgb(51, 65, 85).ToArgb(), ThemeManager.Ink2 },
            { Color.FromArgb(71, 85, 105).ToArgb(), ThemeManager.Ink2 },
            { Color.FromArgb(100, 116, 139).ToArgb(), ThemeManager.Muted },
            { Color.FromArgb(148, 163, 184).ToArgb(), ThemeManager.Muted },
            { Color.FromArgb(226, 232, 240).ToArgb(), ThemeManager.Line },
            { Color.FromArgb(203, 213, 225).ToArgb(), ThemeManager.Line },
            { Color.FromArgb(248, 250, 252).ToArgb(), ThemeManager.Surface2 },
            { Color.FromArgb(241, 245, 249).ToArgb(), ThemeManager.Surface2 },
            { Color.FromArgb(244, 246, 251).ToArgb(), ThemeManager.Bg },
        };

        /// <summary>Bật tự động style cho mọi Form được mở trong ứng dụng.</summary>
        public static void EnableGlobal()
        {
            if (_enabled) return;
            _enabled = true;
            Application.Idle += OnIdle;
        }

        private static void OnIdle(object sender, EventArgs e)
        {
            var forms = new List<Form>();
            foreach (Form f in Application.OpenForms) forms.Add(f);
            foreach (var f in forms)
            {
                object dummy;
                if (!_styled.TryGetValue(f, out dummy)) Apply(f);
            }
        }

        /// <summary>Áp style cho control và toàn bộ control con (kể cả control thêm sau này).</summary>
        public static void Apply(Control root)
        {
            if (root == null) return;
            if (IsNoStyle(root)) return;

            object dummy;
            if (!_styled.TryGetValue(root, out dummy))
            {
                _styled.Add(root, null);
                try { StyleControl(root); } catch { /* không để lỗi style làm hỏng nghiệp vụ */ }
                root.ControlAdded += (s, e) => Apply(e.Control);
            }
            foreach (Control child in root.Controls) Apply(child);
        }

        private static bool IsNoStyle(Control c)
        {
            return c.Tag is string && (string)c.Tag == NoStyleTag;
        }

        public static Color Remap(Color c)
        {
            Color mapped;
            if (c.A == 255 && _legacyMap.TryGetValue(c.ToArgb(), out mapped)) return mapped;
            return c;
        }

        private static void StyleControl(Control c)
        {
            if (c is Form) { StyleForm((Form)c); return; }
            if (c is DataGridView) { StyleGrid((DataGridView)c); return; }
            if (c is Button)
            {
                StyleButton((Button)c);
                if (Controls.ButtonSkin.CanSkin((Button)c)) Controls.ButtonSkin.Attach((Button)c);   // nút bo tròn
                return;
            }
            if (c is TextBox) { StyleTextBox((TextBox)c); return; }
            if (c is ComboBox) { ((ComboBox)c).FlatStyle = FlatStyle.Flat; return; }
            if (c is NumericUpDown) { ((NumericUpDown)c).BorderStyle = BorderStyle.FixedSingle; return; }
            if (c is CheckBox || c is RadioButton)
            {
                if (c.ForeColor == SystemColors.ControlText) c.ForeColor = ThemeManager.Ink2;
                else c.ForeColor = Remap(c.ForeColor);
                c.Cursor = Cursors.Hand;
                return;
            }
            if (c is Label)
            {
                var lbl = (Label)c;
                if (lbl.UseMnemonic && lbl.Text != null && lbl.Text.Contains("&&")) lbl.Text = lbl.Text.Replace("&&", "&");
                lbl.UseMnemonic = false;   // để dấu "&" hiển thị đúng (vd "Đơn hàng & Hóa đơn")
                if (c.ForeColor == SystemColors.ControlText) c.ForeColor = ThemeManager.Ink;
                else c.ForeColor = Remap(c.ForeColor);
                if (c.BackColor != Color.Transparent) c.BackColor = Remap(c.BackColor);
                return;
            }
            if (c is GroupBox)
            {
                if (c.ForeColor == SystemColors.ControlText) c.ForeColor = ThemeManager.Ink2;
                Controls.GroupSkin.Attach((GroupBox)c);
                return;
            }
            if (c is TabControl)
            {
                var tc = (TabControl)c;
                Action skin = () => { try { Controls.TabSkin.Attach(tc); } catch { } };
                if (tc.IsHandleCreated) tc.BeginInvoke(skin);
                else tc.HandleCreated += (s, e) => tc.BeginInvoke(skin);
                return;
            }
            if (c is Panel || c is FlowLayoutPanel || c is TableLayoutPanel)
            {
                if (c.BackColor != Color.Transparent) c.BackColor = Remap(c.BackColor);
                if (c is Panel) Controls.BorderSkin.Attach(c);   // viền đen FixedSingle -> viền mảnh bo góc
                return;
            }
            if (c is PictureBox) { Controls.BorderSkin.Attach(c); return; }
            if (c is TabPage)
            {
                if (c.BackColor == SystemColors.Control || ((TabPage)c).UseVisualStyleBackColor)
                {
                    ((TabPage)c).UseVisualStyleBackColor = false;
                    c.BackColor = ThemeManager.Bg;
                }
                return;
            }
        }

        private static void StyleForm(Form f)
        {
            bool dialog = f.TopLevel && (f.FormBorderStyle == FormBorderStyle.FixedDialog || f.FormBorderStyle == FormBorderStyle.FixedToolWindow);
            if (dialog && (f.BackColor == SystemColors.Control || IsVeryLight(f.BackColor))) f.BackColor = Color.White;
            else if (f.BackColor == SystemColors.Control) f.BackColor = ThemeManager.Bg;
            else f.BackColor = Remap(f.BackColor);

            // Ô nhập bo tròn cho mọi form (trừ màn đăng nhập & khung chính tự vẽ riêng)
            if (f is Forms.Auth.LoginForm || f is Forms.Dashboard.DashboardForm) return;
            Action fields = () => {
                try { Controls.FieldSkin.ApplyAll(f); } catch { }
                if (dialog) { try { StyleDialogTitles(f, f); } catch { } }
            };
            if (f.IsHandleCreated) f.BeginInvoke(fields);
            else f.Load += (s, e) => f.BeginInvoke(fields);
        }

        /// <summary>Tiêu đề hộp thoại: chữ đậm màu mực, bỏ emoji, đổi CHỮ HOA thành dạng câu.</summary>
        private static void StyleDialogTitles(Form f, Control root)
        {
            foreach (Control c in root.Controls)
            {
                var l = c as Label;
                if (l != null && l.Font.SizeInPoints >= 11.5f && l.Font.Bold)
                {
                    var pt = f.PointToClient(l.PointToScreen(Point.Empty));
                    if (pt.Y < 90)
                    {
                        string t = Controls.SkinUtil.CleanText(l.Text);
                        if (t.Length > 3 && t == t.ToUpper() && t != t.ToLower())
                            t = t.Substring(0, 1) + t.Substring(1).ToLower();
                        l.Text = t;
                        l.ForeColor = ThemeManager.Ink;
                        if (l.Font.SizeInPoints < 14f) l.Font = new Font(ThemeManager.FontFamily, 14F, FontStyle.Bold);
                    }
                }
                if (c.HasChildren && !(c is DataGridView)) StyleDialogTitles(f, c);
            }
        }

        // ================= BUTTON =================
        private static void StyleButton(Button b)
        {
            if (b.BackColor == Color.Transparent) return; // nút trong suốt (custom) -> giữ nguyên

            b.FlatStyle = FlatStyle.Flat;
            b.Cursor = Cursors.Hand;
            if (b.Font.Name == "Microsoft Sans Serif") b.Font = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold);

            bool isNeutral = (b.UseVisualStyleBackColor && (b.BackColor == SystemColors.Control || b.BackColor == SystemColors.ButtonFace || b.BackColor.IsSystemColor))
                             || b.BackColor == SystemColors.Control
                             || b.BackColor == SystemColors.ButtonFace
                             || b.BackColor == Color.White
                             || b.BackColor.ToArgb() == ThemeManager.Paper.ToArgb()
                             || IsVeryLight(b.BackColor);

            if (isNeutral)
            {
                // Nút phụ: nền trắng, viền mảnh, chữ đậm
                bool wasTinted = IsVeryLight(b.BackColor) && b.BackColor != Color.White && !b.UseVisualStyleBackColor
                                 && b.BackColor != SystemColors.Control && b.BackColor != SystemColors.ButtonFace;
                b.UseVisualStyleBackColor = false;
                if (!wasTinted) b.BackColor = ThemeManager.Paper;
                if (b.ForeColor == SystemColors.ControlText || b.ForeColor == Color.Black) b.ForeColor = ThemeManager.Ink2;
                else b.ForeColor = Remap(b.ForeColor);
                b.FlatAppearance.BorderSize = wasTinted ? 0 : 1;
                b.FlatAppearance.BorderColor = ThemeManager.Line;
                b.FlatAppearance.MouseOverBackColor = wasTinted ? ThemeManager.Darken(b.BackColor, 0.05f) : ThemeManager.PrimarySoft;
                b.FlatAppearance.MouseDownBackColor = ThemeManager.Darken(wasTinted ? b.BackColor : ThemeManager.PrimarySoft, 0.08f);
            }
            else
            {
                // Nút chính: màu đặc, không viền, hover đậm hơn
                b.UseVisualStyleBackColor = false;
                b.BackColor = Remap(b.BackColor);
                if (b.ForeColor == SystemColors.ControlText) b.ForeColor = Color.White;
                b.FlatAppearance.BorderSize = 0;
                b.FlatAppearance.MouseOverBackColor = ThemeManager.Darken(b.BackColor, 0.12f);
                b.FlatAppearance.MouseDownBackColor = ThemeManager.Darken(b.BackColor, 0.22f);
            }
        }

        private static bool IsVeryLight(Color c)
        {
            return c.A == 255 && c.GetBrightness() > 0.92f;
        }

        // ================= TEXTBOX =================
        private static void StyleTextBox(TextBox t)
        {
            if (t.BorderStyle == BorderStyle.Fixed3D) t.BorderStyle = BorderStyle.FixedSingle;
            if (t.ForeColor == SystemColors.WindowText) t.ForeColor = ThemeManager.Ink;
        }

        // ================= DATAGRIDVIEW =================
        private static readonly ConditionalWeakTable<DataGridView, object> _gridStyled = new ConditionalWeakTable<DataGridView, object>();

        /// <summary>Áp kiểu bảng hiện đại (gọi nhiều lần vẫn an toàn — chỉ áp một lần).</summary>
        public static void StyleGrid(DataGridView g)
        {
            object done;
            if (g == null || _gridStyled.TryGetValue(g, out done)) return;
            _gridStyled.Add(g, null);
            g.BackgroundColor = ThemeManager.Paper;
            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.GridColor = ThemeManager.Line;
            g.RowHeadersVisible = false;
            g.EnableHeadersVisualStyles = false;

            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            if (g.ColumnHeadersHeight < 44) g.ColumnHeadersHeight = 44;

            var header = g.ColumnHeadersDefaultCellStyle;
            header.BackColor = ThemeManager.Paper;
            header.ForeColor = ThemeManager.Muted;
            header.SelectionBackColor = ThemeManager.Paper;
            header.SelectionForeColor = ThemeManager.Muted;
            header.Font = new Font(ThemeManager.FontFamily, 7.5F, FontStyle.Bold);
            header.Padding = new Padding(12, 0, 6, 0);
            header.Alignment = DataGridViewContentAlignment.MiddleLeft;
            header.WrapMode = DataGridViewTriState.False;

            var cell = g.DefaultCellStyle;
            cell.BackColor = ThemeManager.Paper;
            cell.ForeColor = ThemeManager.Ink2;
            cell.SelectionBackColor = ThemeManager.PrimarySoft;
            cell.SelectionForeColor = ThemeManager.Ink;
            cell.Font = new Font(ThemeManager.FontFamily, 9F);
            cell.Padding = new Padding(12, 0, 6, 0);

            // Không dùng sọc ngựa vằn — nền trắng sạch như mẫu
            g.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.Paper;
            g.AlternatingRowsDefaultCellStyle.SelectionBackColor = ThemeManager.PrimarySoft;
            g.AlternatingRowsDefaultCellStyle.SelectionForeColor = ThemeManager.Ink;

            if (g.RowTemplate.Height < 46) g.RowTemplate.Height = 46;
            foreach (DataGridViewRow row in g.Rows)
            {
                if (row.Height < 46) row.Height = 46;
            }
            g.RowsAdded += (s, e) => {
                for (int i = e.RowIndex; i < e.RowIndex + e.RowCount && i < g.Rows.Count; i++)
                    if (g.Rows[i].Height < 46) g.Rows[i].Height = 46;
            };

            // Header: chữ hoa nhỏ màu xám + đường kẻ mảnh; Ô trạng thái: pill màu
            g.CellPainting += (s, e) => {
                if (e.Handled) return;
                try
                {
                    if (e.RowIndex == -1 && e.ColumnIndex >= 0) PaintHeaderCell(g, e);
                    else if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                    {
                        // Nội dung bảng dùng chữ thường (không in đậm), kể cả khi code của form đặt font đậm
                        if (e.CellStyle != null && e.CellStyle.Font != null && e.CellStyle.Font.Bold)
                            e.CellStyle.Font = RegularOf(e.CellStyle.Font);
                        PaintStatusCell(g, e);
                    }
                }
                catch { /* không để lỗi vẽ làm hỏng bảng */ }
            };

            // Hiệu ứng hover theo dòng
            int hoverRow = -1;
            g.CellMouseEnter += (s, e) => {
                if (e.RowIndex < 0 || e.RowIndex == hoverRow) return;
                int old = hoverRow; hoverRow = e.RowIndex;
                if (old >= 0 && old < g.Rows.Count) g.InvalidateRow(old);
                g.InvalidateRow(hoverRow);
            };
            g.MouseLeave += (s, e) => {
                int old = hoverRow; hoverRow = -1;
                if (old >= 0 && old < g.Rows.Count) g.InvalidateRow(old);
            };
            g.RowPrePaint += (s, e) => {
                if (e.RowIndex == hoverRow && !g.Rows[e.RowIndex].Selected)
                {
                    using (var brush = new SolidBrush(Color.FromArgb(248, 249, 253)))
                        e.Graphics.FillRectangle(brush, e.RowBounds);
                    e.PaintParts &= ~DataGridViewPaintParts.Background;
                }
            };
        }

        private static readonly Dictionary<string, Font> _regularFonts = new Dictionary<string, Font>();
        private static Font RegularOf(Font f)
        {
            string key = f.FontFamily.Name + "|" + f.Size + "|" + (int)(f.Style & ~FontStyle.Bold);
            Font r;
            if (!_regularFonts.TryGetValue(key, out r))
            {
                r = new Font(f.FontFamily, f.Size, f.Style & ~FontStyle.Bold, f.Unit);
                _regularFonts[key] = r;
            }
            return r;
        }

        private static bool IsStatusColumn(DataGridViewColumn col)
        {
            if (col == null) return false;
            string h = ((col.HeaderText ?? "") + " " + (col.Name ?? "") + " " + (col.DataPropertyName ?? "")).ToLowerInvariant();
            return h.Contains("trạng thái") || h.Contains("tình trạng") || h.Contains("status") || h.Contains("trangthai") || h.Contains("tinhtrang");
        }

        private static void PaintHeaderCell(DataGridView g, DataGridViewCellPaintingEventArgs e)
        {
            var r = e.CellBounds;
            using (var br = new SolidBrush(ThemeManager.Paper)) e.Graphics.FillRectangle(br, r);
            using (var pen = new Pen(ThemeManager.Line)) e.Graphics.DrawLine(pen, r.Left, r.Bottom - 1, r.Right, r.Bottom - 1);

            var col = g.Columns[e.ColumnIndex];
            string text = Convert.ToString(e.FormattedValue ?? col.HeaderText ?? "").ToUpper();
            var style = e.CellStyle;
            var pad = style.Padding;
            var tr = new Rectangle(r.X + pad.Left, r.Y, Math.Max(0, r.Width - pad.Left - pad.Right - 14), r.Height);
            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
            var al = style.Alignment;
            if (al == DataGridViewContentAlignment.MiddleRight || al == DataGridViewContentAlignment.TopRight || al == DataGridViewContentAlignment.BottomRight) flags |= TextFormatFlags.Right;
            else if (al == DataGridViewContentAlignment.MiddleCenter || al == DataGridViewContentAlignment.TopCenter || al == DataGridViewContentAlignment.BottomCenter) flags |= TextFormatFlags.HorizontalCenter;

            var sort = col.HeaderCell.SortGlyphDirection;
            Color fc = sort != SortOrder.None ? ThemeManager.Ink : ThemeManager.Muted;
            using (var f = new Font(ThemeManager.FontFamily, 7.5F, FontStyle.Bold))
                TextRenderer.DrawText(e.Graphics, text, f, tr, fc, flags);

            if (sort != SortOrder.None)
            {
                using (var f = ThemeManager.Icon(6.5F))
                    TextRenderer.DrawText(e.Graphics, sort == SortOrder.Ascending ? ThemeManager.Glyph.Up : ThemeManager.Glyph.Down, f,
                        new Rectangle(r.Right - 18, r.Y, 14, r.Height), ThemeManager.Primary,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
            e.Handled = true;
        }

        private static void PaintStatusCell(DataGridView g, DataGridViewCellPaintingEventArgs e)
        {
            if (!IsStatusColumn(g.Columns[e.ColumnIndex])) return;
            string raw = Convert.ToString(e.FormattedValue);
            Color fore, back;
            if (!ThemeManager.TryStatusColors(raw, out fore, out back)) return;
            string text = ThemeManager.CleanStatus(raw);

            e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
            var r = e.CellBounds;
            using (var f = new Font(ThemeManager.FontFamily, 7.5F, FontStyle.Bold))
            {
                var ts = TextRenderer.MeasureText(e.Graphics, text, f, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                int w = Math.Min(r.Width - 16, ts.Width + 22), h = 24;
                int x = r.X + e.CellStyle.Padding.Left;
                var al = e.CellStyle.Alignment;
                if (al == DataGridViewContentAlignment.MiddleCenter || al == DataGridViewContentAlignment.TopCenter || al == DataGridViewContentAlignment.BottomCenter)
                    x = r.X + (r.Width - w) / 2;
                else if (al == DataGridViewContentAlignment.MiddleRight) x = r.Right - w - e.CellStyle.Padding.Right;
                var pill = new Rectangle(x, r.Y + (r.Height - h) / 2, Math.Max(10, w), h);
                var old = e.Graphics.SmoothingMode;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = ThemeManager.RoundedRect(pill, 6))
                using (var br = new SolidBrush(back))
                    e.Graphics.FillPath(br, path);
                e.Graphics.SmoothingMode = old;
                TextRenderer.DrawText(e.Graphics, text, f, pill, fore,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
            e.Handled = true;
        }

        // ================= PLACEHOLDER (Cue banner) =================
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        public static void SetPlaceholder(TextBox box, string text)
        {
            if (box == null) return;
            Action set = () => { try { SendMessage(box.Handle, EM_SETCUEBANNER, (IntPtr)1, text); } catch { /* không hỗ trợ placeholder */ } };
            if (box.IsHandleCreated) set();
            else box.HandleCreated += (s, e) => set();
        }
    }
}
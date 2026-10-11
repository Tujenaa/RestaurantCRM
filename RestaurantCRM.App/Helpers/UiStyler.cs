using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RestaurantCRM.AdminApp.Helpers {
    /// <summary>
    /// Tự động áp phong cách hiện đại cho mọi Form / Control trong ứng dụng:
    /// nút bấm phẳng có hiệu ứng hover, bảng dữ liệu thoáng, ô nhập viền mảnh...
    /// Gọi <see cref="EnableGlobal"/> một lần ở Program.Main là đủ.
    /// Đặt Tag = "nostyle" cho control nào muốn giữ nguyên giao diện gốc.
    /// </summary>
    public static class UiStyler {
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
        };

        /// <summary>Bật tự động style cho mọi Form được mở trong ứng dụng.</summary>
        public static void EnableGlobal() {
            if (_enabled) return;
            _enabled = true;
            Application.Idle += OnIdle;
        }

        private static void OnIdle(object sender, EventArgs e) {
            var forms = new List<Form>();
            foreach (Form f in Application.OpenForms) forms.Add(f);
            foreach (var f in forms) {
                object dummy;
                if (!_styled.TryGetValue(f, out dummy)) Apply(f);
            }
        }

        /// <summary>Áp style cho control và toàn bộ control con (kể cả control thêm sau này).</summary>
        public static void Apply(Control root) {
            if (root == null) return;
            if (IsNoStyle(root)) return;

            object dummy;
            if (!_styled.TryGetValue(root, out dummy)) {
                _styled.Add(root, null);
                try { StyleControl(root); } catch { /* không để lỗi style làm hỏng nghiệp vụ */ }
                root.ControlAdded += (s, e) => Apply(e.Control);
            }
            foreach (Control child in root.Controls) Apply(child);
        }

        private static bool IsNoStyle(Control c) {
            return c.Tag is string && (string)c.Tag == NoStyleTag;
        }

        public static Color Remap(Color c) {
            Color mapped;
            if (c.A == 255 && _legacyMap.TryGetValue(c.ToArgb(), out mapped)) return mapped;
            return c;
        }

        private static void StyleControl(Control c) {
            if (c is Form) { StyleForm((Form)c); return; }
            if (c is DataGridView) { StyleGrid((DataGridView)c); return; }
            if (c is Button) { StyleButton((Button)c); return; }
            if (c is TextBox) { StyleTextBox((TextBox)c); return; }
            if (c is ComboBox) { ((ComboBox)c).FlatStyle = FlatStyle.Flat; return; }
            if (c is NumericUpDown) { ((NumericUpDown)c).BorderStyle = BorderStyle.FixedSingle; return; }
            if (c is CheckBox || c is RadioButton) {
                if (c.ForeColor == SystemColors.ControlText) c.ForeColor = ThemeManager.Ink2;
                c.Cursor = Cursors.Hand;
                return;
            }
            if (c is Label) {
                if (c.ForeColor == SystemColors.ControlText) c.ForeColor = ThemeManager.Ink;
                else c.ForeColor = Remap(c.ForeColor);
                if (c.BackColor != Color.Transparent) c.BackColor = Remap(c.BackColor);
                return;
            }
            if (c is GroupBox) {
                if (c.ForeColor == SystemColors.ControlText) c.ForeColor = ThemeManager.Ink2;
                return;
            }
            if (c is Panel || c is FlowLayoutPanel || c is TableLayoutPanel) {
                if (c.BackColor != Color.Transparent) c.BackColor = Remap(c.BackColor);
                return;
            }
            if (c is TabPage) {
                if (c.BackColor == SystemColors.Control || ((TabPage)c).UseVisualStyleBackColor) {
                    ((TabPage)c).UseVisualStyleBackColor = false;
                    c.BackColor = ThemeManager.Bg;
                }
                return;
            }
        }

        private static void StyleForm(Form f) {
            if (f.BackColor == SystemColors.Control) f.BackColor = ThemeManager.Bg;
            else f.BackColor = Remap(f.BackColor);
        }

        // ================= BUTTON =================
        private static void StyleButton(Button b) {
            if (b.BackColor == Color.Transparent) return; // nút trong suốt (custom) -> giữ nguyên

            b.FlatStyle = FlatStyle.Flat;
            b.Cursor = Cursors.Hand;
            if (b.Font.Name == "Microsoft Sans Serif") b.Font = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold);

            bool isNeutral = b.UseVisualStyleBackColor
                             || b.BackColor == SystemColors.Control
                             || b.BackColor == SystemColors.ButtonFace
                             || b.BackColor == Color.White
                             || b.BackColor.ToArgb() == ThemeManager.Paper.ToArgb()
                             || IsVeryLight(b.BackColor);

            if (isNeutral) {
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
            } else {
                // Nút chính: màu đặc, không viền, hover đậm hơn
                b.UseVisualStyleBackColor = false;
                b.BackColor = Remap(b.BackColor);
                if (b.ForeColor == SystemColors.ControlText) b.ForeColor = Color.White;
                b.FlatAppearance.BorderSize = 0;
                b.FlatAppearance.MouseOverBackColor = ThemeManager.Darken(b.BackColor, 0.12f);
                b.FlatAppearance.MouseDownBackColor = ThemeManager.Darken(b.BackColor, 0.22f);
            }
        }

        private static bool IsVeryLight(Color c) {
            return c.A == 255 && c.GetBrightness() > 0.92f;
        }

        // ================= TEXTBOX =================
        private static void StyleTextBox(TextBox t) {
            if (t.BorderStyle == BorderStyle.Fixed3D) t.BorderStyle = BorderStyle.FixedSingle;
            if (t.ForeColor == SystemColors.WindowText) t.ForeColor = ThemeManager.Ink;
        }

        // ================= DATAGRIDVIEW =================
        public static void StyleGrid(DataGridView g) {
            g.BackgroundColor = ThemeManager.Paper;
            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.GridColor = ThemeManager.Line;
            g.RowHeadersVisible = false;
            g.EnableHeadersVisualStyles = false;

            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            if (g.ColumnHeadersHeight < 42) g.ColumnHeadersHeight = 42;

            var header = g.ColumnHeadersDefaultCellStyle;
            header.BackColor = ThemeManager.Surface2;
            header.ForeColor = ThemeManager.Muted;
            header.SelectionBackColor = ThemeManager.Surface2;
            header.SelectionForeColor = ThemeManager.Muted;
            header.Font = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold);
            header.Padding = new Padding(10, 0, 6, 0);
            header.Alignment = DataGridViewContentAlignment.MiddleLeft;
            header.WrapMode = DataGridViewTriState.False;

            var cell = g.DefaultCellStyle;
            cell.BackColor = ThemeManager.Paper;
            cell.ForeColor = ThemeManager.Ink2;
            cell.SelectionBackColor = ThemeManager.PrimarySoft;
            cell.SelectionForeColor = ThemeManager.Ink;
            cell.Font = new Font(ThemeManager.FontFamily, 9.5F);
            cell.Padding = new Padding(10, 0, 6, 0);

            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 254);
            g.AlternatingRowsDefaultCellStyle.SelectionBackColor = ThemeManager.PrimarySoft;
            g.AlternatingRowsDefaultCellStyle.SelectionForeColor = ThemeManager.Ink;

            if (g.RowTemplate.Height < 40) g.RowTemplate.Height = 40;
            foreach (DataGridViewRow row in g.Rows) {
                if (row.Height < 40) row.Height = 40;
            }

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
                if (e.RowIndex == hoverRow && !g.Rows[e.RowIndex].Selected) {
                    using (var brush = new SolidBrush(Color.FromArgb(246, 247, 255)))
                        e.Graphics.FillRectangle(brush, e.RowBounds);
                    e.PaintParts &= ~DataGridViewPaintParts.Background;
                }
            };
        }

        // ================= PLACEHOLDER (Cue banner) =================
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        public static void SetPlaceholder(TextBox box, string text) {
            if (box == null) return;
            Action set = () => SendMessage(box.Handle, EM_SETCUEBANNER, (IntPtr)1, text);
            if (box.IsHandleCreated) set();
            else box.HandleCreated += (s, e) => set();
        }
    }
}

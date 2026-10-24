using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    internal static class SkinUtil
    {
        /// <summary>Màu nền thực tế phía sau control (xuyên qua các control trong suốt).</summary>
        public static Color Backdrop(Control c)
        {
            var p = c.Parent;
            while (p != null)
            {
                var rp = p as RoundedPanel;
                if (rp != null) return rp.FillColor;
                if (p.BackColor.A == 255) return p.BackColor;
                p = p.Parent;
            }
            return ThemeManager.Bg;
        }

        public static void EnableDoubleBuffer(Control c)
        {
            try
            {
                typeof(Control).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                    null, c, new object[] { true });
            }
            catch { }
        }

        /// <summary>Bỏ emoji / ký hiệu ở đầu chữ (vd "🔄 Làm Mới" -> "Làm Mới").</summary>
        public static string CleanText(string s)
        {
            if (string.IsNullOrEmpty(s)) return s ?? "";
            int i = 0;
            while (i < s.Length)
            {
                char ch = s[i];
                if (char.IsLetterOrDigit(ch)) break;
                i++;
            }
            string r = s.Substring(i).Trim();
            // bỏ emoji cuối chữ (vd "Bán chạy 🔥")
            int j = r.Length;
            while (j > 0 && (char.IsSurrogate(r[j - 1]) || r[j - 1] == '️' || r[j - 1] == ' ')) j--;
            return r.Substring(0, j);
        }

        public static string GlyphFor(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            string t = text.ToLowerInvariant();
            string raw = text.TrimStart();
            if (raw.StartsWith("+") || raw.StartsWith("＋") || t.Contains("thêm") || t.StartsWith("tạo") || t.Contains("lập phiếu")) return ThemeManager.Glyph.Add;
            if (t.Contains("sửa") || t.Contains("chỉnh")) return ThemeManager.Glyph.Edit;
            if (t.Contains("xóa") || t.Contains("xoá")) return ThemeManager.Glyph.Delete;
            if (t.Contains("làm mới") || t.Contains("tải lại") || t.Contains("nhập lại") || t.Contains("kinh doanh lại") || t.Contains("khôi phục")) return "\uE72C";
            if (t.Contains("ngừng") || t.Contains("khóa")) return "\uE711";
            if (t.Contains("chi tiết") || t.StartsWith("xem")) return "\uE890";
            if (t.Contains("lưu") || t.Contains("sao lưu")) return ThemeManager.Glyph.Save;
            if (t.Contains("xác nhận") || t.Contains("áp dụng") || t.Contains("cập nhật")) return "\uE8FB";
            if (t.Contains("loại món") || t.Contains("danh mục")) return ThemeManager.Glyph.List;
            if (t.Contains("tìm")) return ThemeManager.Glyph.Search;
            if (t.Contains("xuất") || t.Contains("tải xuống")) return ThemeManager.Glyph.Download;
            if (t.Contains("thống kê") || t.Contains("báo cáo")) return ThemeManager.Glyph.Chart;
            return null;
        }

        public static GraphicsPath Round(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            float d = Math.Max(1f, radius * 2);
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RestaurantCRM.AdminApp.Helpers {
    /// <summary>
    /// Bảng màu & font dùng chung cho toàn bộ ứng dụng (phong cách SaaS hiện đại - Indigo/Violet).
    /// Các tên màu cũ (Bg, Paper, Blue, Purple...) được giữ nguyên để mọi Form cũ tự động nhận màu mới.
    /// </summary>
    public static class ThemeManager {
        // ===== Nền & bề mặt =====
        public static readonly Color Bg = ColorTranslator.FromHtml("#F4F6FB");
        public static readonly Color Paper = ColorTranslator.FromHtml("#FFFFFF");
        public static readonly Color Surface2 = ColorTranslator.FromHtml("#F8FAFC");
        public static readonly Color Line = ColorTranslator.FromHtml("#E6E9F2");

        // ===== Chữ =====
        public static readonly Color Ink = ColorTranslator.FromHtml("#111827");
        public static readonly Color Ink2 = ColorTranslator.FromHtml("#374151");
        public static readonly Color Muted = ColorTranslator.FromHtml("#6B7280");

        // ===== Màu chủ đạo =====
        public static readonly Color Primary = ColorTranslator.FromHtml("#4F46E5");      // Indigo 600
        public static readonly Color PrimaryDark = ColorTranslator.FromHtml("#4338CA");  // Indigo 700
        public static readonly Color PrimarySoft = ColorTranslator.FromHtml("#EEF2FF");  // Indigo 50
        public static readonly Color Violet = ColorTranslator.FromHtml("#7C3AED");
        public static readonly Color VioletSoft = ColorTranslator.FromHtml("#F5F3FF");
        public static readonly Color Teal = ColorTranslator.FromHtml("#0D9488");
        public static readonly Color TealSoft = ColorTranslator.FromHtml("#F0FDFA");
        public static readonly Color Sky = ColorTranslator.FromHtml("#0284C7");
        public static readonly Color SkySoft = ColorTranslator.FromHtml("#F0F9FF");

        // ===== Trạng thái =====
        public static readonly Color Green = ColorTranslator.FromHtml("#059669");
        public static readonly Color GreenSoft = ColorTranslator.FromHtml("#ECFDF5");
        public static readonly Color Red = ColorTranslator.FromHtml("#DC2626");
        public static readonly Color RedSoft = ColorTranslator.FromHtml("#FEF2F2");
        public static readonly Color Amber = ColorTranslator.FromHtml("#D97706");
        public static readonly Color AmberSoft = ColorTranslator.FromHtml("#FFFBEB");

        // ===== Sidebar (tối) =====
        public static readonly Color SidebarBg = ColorTranslator.FromHtml("#0F172A");
        public static readonly Color SidebarBg2 = ColorTranslator.FromHtml("#1E1B4B");
        public static readonly Color SidebarText = ColorTranslator.FromHtml("#CBD5E1");
        public static readonly Color SidebarMuted = ColorTranslator.FromHtml("#64748B");
        public static readonly Color SidebarHover = ColorTranslator.FromHtml("#1E293B");
        public static readonly Color SidebarActive = ColorTranslator.FromHtml("#312E81");

        // ===== Tên cũ (tương thích ngược) =====
        public static readonly Color Blue = Primary;
        public static readonly Color Blue2 = PrimarySoft;
        public static readonly Color Purple = Violet;
        public static readonly Color Purple2 = VioletSoft;

        // ===== Font =====
        public const string FontFamily = "Segoe UI";
        public const string EmojiFont = "Segoe UI Emoji";
        public static Font Body(float size = 9.5F, FontStyle style = FontStyle.Regular) { return new Font(FontFamily, size, style); }
        public static Font Semibold(float size = 9.5F) { return new Font("Segoe UI Semibold", size, FontStyle.Regular); }

        /// <summary>Màu nhấn theo phân hệ.</summary>
        public static Color AccentFor(string role) {
            if (role == "CRM") return Violet;
            if (role == "Bán hàng") return Teal;
            return Primary;
        }
        public static Color SoftFor(Color accent) { return Blend(accent, Color.White, 0.90f); }

        // ===== Tiện ích màu =====
        public static Color Blend(Color a, Color b, float t) {
            t = Math.Max(0f, Math.Min(1f, t));
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }
        public static Color Darken(Color c, float amount) { return Blend(c, Color.Black, amount); }
        public static Color Lighten(Color c, float amount) { return Blend(c, Color.White, amount); }

        public static GraphicsPath RoundedRect(Rectangle r, int radius) {
            var path = new GraphicsPath();
            int d = Math.Max(1, radius * 2);
            if (radius <= 0) { path.AddRectangle(r); return path; }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static string Initials(string name) {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }
    }
}

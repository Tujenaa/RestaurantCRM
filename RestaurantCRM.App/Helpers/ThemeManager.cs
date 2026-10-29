using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RestaurantCRM.AdminApp.Helpers
{
    /// <summary>
    /// Bảng màu & font dùng chung cho toàn bộ ứng dụng (phong cách SaaS hiện đại - Indigo/Violet).
    /// Các tên màu cũ (Bg, Paper, Blue, Purple...) được giữ nguyên để mọi Form cũ tự động nhận màu mới.
    /// </summary>
    public static class ThemeManager
    {
        // ===== Nền & bề mặt (phong cách SaaS sáng, tối giản) =====
        public static readonly Color Bg = ColorTranslator.FromHtml("#FBF6F0");
        public static readonly Color Paper = ColorTranslator.FromHtml("#FFFFFF");
        public static readonly Color Surface2 = ColorTranslator.FromHtml("#FDF9F4");
        public static readonly Color Line = ColorTranslator.FromHtml("#F0E4D6");

        // ===== Chữ =====
        public static readonly Color Ink = ColorTranslator.FromHtml("#2A1A12");
        public static readonly Color Ink2 = ColorTranslator.FromHtml("#5A463A");
        public static readonly Color Muted = ColorTranslator.FromHtml("#9A8778");

        // ===== Màu chủ đạo Lẩu Phố (cam đất, đồng bộ với website) =====
        public static readonly Color Primary = ColorTranslator.FromHtml("#B8461B");
        public static readonly Color PrimaryDark = ColorTranslator.FromHtml("#933412");
        public static readonly Color PrimarySoft = ColorTranslator.FromHtml("#FCEDE3");
        public static readonly Color Violet = ColorTranslator.FromHtml("#7B6CF6");
        public static readonly Color VioletSoft = ColorTranslator.FromHtml("#F2F0FE");
        public static readonly Color Teal = ColorTranslator.FromHtml("#14A99A");
        public static readonly Color TealSoft = ColorTranslator.FromHtml("#E8F8F6");
        public static readonly Color Sky = ColorTranslator.FromHtml("#3BA4F5");
        public static readonly Color SkySoft = ColorTranslator.FromHtml("#EAF5FE");

        // ===== Trạng thái (dùng cho pill) =====
        public static readonly Color Green = ColorTranslator.FromHtml("#2DB37A");
        public static readonly Color GreenSoft = ColorTranslator.FromHtml("#E3F7EE");
        public static readonly Color Red = ColorTranslator.FromHtml("#EF5466");
        public static readonly Color RedSoft = ColorTranslator.FromHtml("#FDECEE");
        public static readonly Color Amber = ColorTranslator.FromHtml("#E99A2C");
        public static readonly Color AmberSoft = ColorTranslator.FromHtml("#FEF4E4");

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

        /// <summary>Màu nhấn theo phân hệ — thống nhất một màu chủ đạo cho mọi phân hệ.</summary>
        public static Color AccentFor(string role)
        {
            return Primary;
        }

        /// <summary>Tên hiển thị chuẩn cho các phân hệ.</summary>
        public static string DisplayWorkspaceName(string role)
        {
            if (string.IsNullOrEmpty(role)) return "";
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(role, "Hệ thống", StringComparison.OrdinalIgnoreCase))
                return "Hệ thống";
            if (string.Equals(role, "Bán hàng", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(role, "Vận Hành", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(role, "Vận hành", StringComparison.OrdinalIgnoreCase))
                return "Vận Hành";
            if (string.Equals(role, "CRM", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(role, "Khách hàng", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(role, "khách hàng", StringComparison.OrdinalIgnoreCase))
                return "Khách hàng";
            return role;
        }

        // ===== Icon font (Segoe Fluent Icons trên Win11, Segoe MDL2 Assets trên Win10) =====
        private static string _iconFamily;
        public static string IconFamily
        {
            get
            {
                if (_iconFamily != null) return _iconFamily;
                _iconFamily = "Segoe MDL2 Assets";
                try
                {
                    using (var installed = new System.Drawing.Text.InstalledFontCollection())
                    {
                        foreach (var ff in installed.Families)
                        {
                            if (ff.Name == "Segoe Fluent Icons") { _iconFamily = ff.Name; break; }
                        }
                    }
                }
                catch { }
                return _iconFamily;
            }
        }
        public static Font Icon(float size = 11F) { return new Font(IconFamily, size, FontStyle.Regular, GraphicsUnit.Point); }

        /// <summary>Mã glyph icon dùng chung (Segoe MDL2 / Fluent).</summary>
        public static class Glyph
        {
            public const string Home = "\uE80F";
            public const string People = "\uE716";
            public const string Contact = "\uE77B";
            public const string Star = "\uE734";
            public const string Document = "\uE8A5";
            public const string Chart = "\uE9D2";
            public const string Diagnostic = "\uE9D9";
            public const string Cart = "\uE7BF";
            public const string List = "\uE8FD";
            public const string Book = "\uE8F1";
            public const string Tag = "\uE8EC";
            public const string Package = "\uE7B8";
            public const string Download = "\uE896";
            public const string Folder = "\uE8B7";
            public const string Shield = "\uEA18";
            public const string Save = "\uE74E";
            public const string Search = "\uE721";
            public const string Add = "\uE710";
            public const string Edit = "\uE70F";
            public const string Delete = "\uE74D";
            public const string Filter = "\uE71C";
            public const string ChevronDown = "\uE70D";
            public const string Calendar = "\uE787";
            public const string Power = "\uE7E8";
            public const string Clear = "\uE711";
            public const string Up = "\uE74A";
            public const string Down = "\uE74B";
            public const string More = "\uE712";
            public const string Hamburger = "\uE700";
        }

        /// <summary>Nhận diện giá trị trạng thái để vẽ dạng pill màu (xanh / đỏ / vàng / xanh dương).</summary>
        public static bool TryStatusColors(string value, out Color fore, out Color back)
        {
            fore = Ink2; back = Surface2;
            string v = CleanStatus(value).ToLowerInvariant();
            if (v.Length == 0 || v.Length > 40) return false;

            // Thứ tự kiểm tra quan trọng: "sắp hết hàng" phải ra vàng trước khi khớp "hết hàng" (đỏ)
            string[] amber = { "sắp", "chờ", "pending", "chuẩn bị", "preparing", "nháp", "draft", "chưa", "lowstock", "low stock", "tạm dừng" };
            string[] red = { "hủy", "huỷ", "cancel", "hết hàng", "đã hết", "hết nl", "outofstock", "out of stock", "hethang", "khóa", "ngừng", "ngưng",
                             "đóng", "closed", "inactive", "ẩn", "hết hạn", "expired", "từ chối", "kết thúc", "discontinued", "thất bại", "lỗi" };
            string[] blue = { "đang giao", "delivering", "đang phục vụ", "xác nhận", "confirmed", "đang xử lý", "processing", "mới" };
            string[] green = { "hoàn thành", "completed", "active", "hoạt động", "đang diễn ra", "còn hàng", "instock", "in stock", "đã thanh toán",
                               "đã duyệt", "hiển thị", "đang bán", "còn món", "mở", "đã giao", "đủ hàng", "còn đủ", "kích hoạt", "sẵn sàng",
                               "đã xử lý", "đã trả lời", "đang kinh doanh", "đang nhập", "đang áp dụng", "thành công", "bán chạy" };
            if (ContainsAny(v, amber)) { fore = Amber; back = AmberSoft; return true; }
            if (ContainsAny(v, red)) { fore = Red; back = RedSoft; return true; }
            if (ContainsAny(v, blue)) { fore = Primary; back = PrimarySoft; return true; }
            if (ContainsAny(v, green)) { fore = Green; back = GreenSoft; return true; }
            return false;
        }

        private static bool ContainsAny(string v, string[] keys)
        {
            foreach (var k in keys) if (v.Contains(k)) return true;
            return false;
        }

        /// <summary>Làm sạch chữ trạng thái để hiển thị: bỏ emoji/ký hiệu đầu và phần "(English)" cuối.</summary>
        public static string CleanStatus(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            string s = value.Trim();
            int i = 0;
            while (i < s.Length && !char.IsLetterOrDigit(s[i])) i++;
            s = s.Substring(i);
            int p = s.IndexOf(" (", StringComparison.Ordinal);
            if (p > 0 && s.EndsWith(")")) s = s.Substring(0, p);
            s = s.Trim();
            // Dịch trạng thái thô tiếng Anh từ API sang tiếng Việt
            switch (s.ToLowerInvariant())
            {
                case "completed": return "Hoàn thành";
                case "pending": return "Chờ xử lý";
                case "confirmed": return "Đã xác nhận";
                case "preparing": return "Đang chuẩn bị";
                case "delivering": return "Đang giao";
                case "cancelled": case "canceled": return "Đã hủy";
                case "instock": return "Còn hàng";
                case "outofstock": return "Hết hàng";
                case "lowstock": return "Sắp hết hàng";
                case "discontinued": return "Ngừng kinh doanh";
                case "active": return "Hoạt động";
                case "inactive": return "Ngừng hoạt động";
                case "closed": return "Đã đóng";
                case "open": return "Đang mở";
                case "draft": return "Nháp";
                case "expired": return "Hết hạn";
            }
            return s;
        }

        /// <summary>Vẽ bóng đổ mềm quanh một khung bo góc (vẽ trên nền cha hoặc trong vùng lề của control).</summary>
        public static void DrawSoftShadow(Graphics g, Rectangle card, int radius, int spread = 8, int maxAlpha = 18)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            for (int i = spread; i >= 1; i--)
            {
                int alpha = (int)(maxAlpha * (1f - (float)i / (spread + 1)) * 0.6f);
                if (alpha <= 0) continue;
                var r = new Rectangle(card.X - i, card.Y - i / 2 + 2, card.Width + i * 2, card.Height + i * 2 - 1);
                using (var path = RoundedRect(r, radius + i))
                using (var pen = new Pen(Color.FromArgb(alpha, 40, 48, 90), 1.6f))
                    g.DrawPath(pen, path);
            }
        }

        /// <summary>Màu avatar ổn định theo tên.</summary>
        public static Color AvatarColor(string seed)
        {
            Color[] palette = { Primary, Violet, Teal, Sky, Amber, Green, Red };
            if (string.IsNullOrEmpty(seed)) return Primary;
            int h = 0; foreach (char ch in seed) h = (h * 31 + ch) & 0x7FFFFFFF;
            return palette[h % palette.Length];
        }
        public static Color SoftFor(Color accent) { return Blend(accent, Color.White, 0.90f); }

        // ===== Tiện ích màu =====
        public static Color Blend(Color a, Color b, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }
        public static Color Darken(Color c, float amount) { return Blend(c, Color.Black, amount); }
        public static Color Lighten(Color c, float amount) { return Blend(c, Color.White, amount); }

        public static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
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

        public static string Initials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }
    }
}
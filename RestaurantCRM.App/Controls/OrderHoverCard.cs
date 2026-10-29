using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Controls
{
    /// <summary>
    /// Thẻ xem nhanh đơn hàng: hiện khi rê chuột lên một dòng đơn hàng,
    /// không lấy focus và không cần click.
    /// </summary>
    public class OrderHoverCard : Form
    {
        private const int CardWidth = 380;
        private const int Pad = 20;
        private const int MaxItems = 8;

        private HoaDonDto _summary;
        private HoaDonDetailDto _detail;
        private bool _loading;

        private readonly Font _fTitle = new Font("Segoe UI", 12F, FontStyle.Bold);
        private readonly Font _fLabel = new Font("Segoe UI", 8.5F);
        private readonly Font _fValue = new Font("Segoe UI", 9F);
        private readonly Font _fValueBold = new Font("Segoe UI", 9F, FontStyle.Bold);
        private readonly Font _fSection = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        private readonly Font _fTotal = new Font("Segoe UI", 11F, FontStyle.Bold);
        private readonly Font _fPill = new Font("Segoe UI", 8F, FontStyle.Bold);

        private const TextFormatFlags Flags = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;

        public string CurrentMaHoaDon => _summary?.MaHoaDon;

        public OrderHoverCard()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            BackColor = Color.White;
            Width = CardWidth;
            Height = 200;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        // Không cướp focus khỏi form chính khi hiện
        protected override bool ShowWithoutActivation => true;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOACTIVATE = 0x0010, SWP_SHOWWINDOW = 0x0040, SWP_NOOWNERZORDER = 0x0200;
        private const int SW_HIDE = 0;

        private bool _shown;

        /// <summary>Thẻ có đang hiện trên màn hình không.</summary>
        public bool IsShown => _shown;

        /// <summary>Đặt vị trí + kích thước + hiện thẻ trong MỘT lệnh, tuyệt đối không kích hoạt cửa sổ (tránh chớp tắt).</summary>
        private void ApplyBounds(Rectangle b)
        {
            var h = Handle; // bảo đảm đã tạo handle
            bool sizeChanged = b.Size != Size;
            SetWindowPos(h, HWND_TOPMOST, b.X, b.Y, b.Width, b.Height, SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_SHOWWINDOW);
            _shown = true;
            if (!sizeChanged) Invalidate();
        }

        public void HideCard()
        {
            if (!_shown) return;
            _shown = false;
            if (IsHandleCreated) ShowWindow(Handle, SW_HIDE);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_NOACTIVATE = 0x08000000;
                const int WS_EX_TOOLWINDOW = 0x00000080;
                const int CS_DROPSHADOW = 0x00020000;
                var cp = base.CreateParams;
                const int WS_EX_TOPMOST = 0x00000008;
                cp.ExStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        // Chuột lọt vào thẻ thì cho "xuyên qua" xuống lưới bên dưới để thẻ không chặn thao tác
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HTTRANSPARENT = -1;
            if (m.Msg == WM_NCHITTEST) { m.Result = (IntPtr)HTTRANSPARENT; return; }
            base.WndProc(ref m);
        }

        /// <summary>Hiện thẻ cho một đơn. detail = null nghĩa là đang tải chi tiết.</summary>
        public void ShowFor(HoaDonDto summary, HoaDonDetailDto detail, Rectangle rowScreenBounds, Point cursorScreen)
        {
            _summary = summary;
            _detail = detail;
            _loading = detail == null;
            ApplyBounds(PlaceNear(rowScreenBounds, cursorScreen, MeasureHeight()));
        }

        /// <summary>Cập nhật chi tiết khi tải xong (chỉ khi thẻ vẫn đang hiện đúng đơn đó).</summary>
        public void UpdateDetail(HoaDonDetailDto detail)
        {
            if (detail == null || _summary == null || detail.MaHoaDon != _summary.MaHoaDon) return;
            _detail = detail;
            _loading = false;
            if (!_shown) return;
            int h = MeasureHeight();
            // Giữ thẻ trong màn hình khi chiều cao tăng
            var wa = Screen.FromPoint(Location).WorkingArea;
            int top = Top;
            if (top + h > wa.Bottom - 8) top = Math.Max(wa.Top + 8, wa.Bottom - 8 - h);
            ApplyBounds(new Rectangle(Left, top, CardWidth, h));
        }

        private Rectangle PlaceNear(Rectangle row, Point cursor, int height)
        {
            var wa = Screen.FromPoint(cursor).WorkingArea;
            int x = cursor.X + 28;                              // mặc định: bên phải con trỏ
            if (x + CardWidth > wa.Right - 8) x = cursor.X - 28 - CardWidth; // hết chỗ thì lật sang trái
            x = Math.Max(wa.Left + 8, x);
            int y = row.Top - 12;
            if (y + height > wa.Bottom - 8) y = wa.Bottom - 8 - height;
            y = Math.Max(wa.Top + 8, y);
            return new Rectangle(x, y, CardWidth, height);
        }

        // ── Bố cục ───────────────────────────────────────────────────
        private List<KeyValuePair<string, string>> InfoRows()
        {
            var rows = new List<KeyValuePair<string, string>>();
            if (_summary == null) return rows;

            string kh = _detail?.KhachHang?.HoTen ?? _summary.TenKhachHang ?? "Khách vãng lai";
            string sdt = _detail?.KhachHang?.SoDienThoai;
            if (!string.IsNullOrWhiteSpace(sdt)) kh += "  ·  " + sdt;

            rows.Add(new KeyValuePair<string, string>("Thời gian đặt", _summary.NgayDatHienThi));
            rows.Add(new KeyValuePair<string, string>("Khách hàng", kh));
            if (_detail != null)
                rows.Add(new KeyValuePair<string, string>("Địa chỉ",
                    string.IsNullOrWhiteSpace(_detail.DiaChiGiao) ? "Dùng tại nhà hàng" : _detail.DiaChiGiao));

            string nv = !string.IsNullOrWhiteSpace(_detail?.NhanVienPhuTrach) ? _detail.NhanVienPhuTrach
                : (!string.IsNullOrWhiteSpace(_summary.TenNhanVien)
                    ? (_summary.TenNhanVien + (string.IsNullOrWhiteSpace(_summary.MaNhanVien) ? "" : " (" + _summary.MaNhanVien + ")"))
                    : "Chưa phân công");
            rows.Add(new KeyValuePair<string, string>("Nhân viên", nv));

            string pttt = _detail?.ThanhToan?.FirstOrDefault()?.PhuongThuc;
            if (string.IsNullOrWhiteSpace(pttt)) pttt = _summary.PhuongThucThanhToan;
            rows.Add(new KeyValuePair<string, string>("Thanh toán", pttt ?? "-"));

            if (_detail != null)
                rows.Add(new KeyValuePair<string, string>("Khuyến mãi",
                    string.IsNullOrWhiteSpace(_detail.KhuyenMai) ? "Không áp dụng" : _detail.KhuyenMai));
            return rows;
        }

        private int ValueHeight(string text, Font f, int width)
        {
            var sz = TextRenderer.MeasureText(text ?? "", f, new Size(width, int.MaxValue), Flags | TextFormatFlags.WordBreak);
            return Math.Max(18, sz.Height);
        }

        private const int LabelWidth = 96;

        private int MeasureHeight()
        {
            int contentW = CardWidth - Pad * 2;
            int y = Pad + 30 + 14;                                   // tiêu đề + khoảng cách
            foreach (var r in InfoRows())
                y += ValueHeight(r.Value, _fValue, contentW - LabelWidth) + 8;

            y += 8 + 1 + 14;                                         // vạch ngăn
            y += 18;                                                 // tiêu đề "MÓN ĂN"
            if (_loading) y += 24;
            else
            {
                int n = _detail?.ChiTiet?.Count ?? 0;
                y += Math.Max(1, Math.Min(n, MaxItems)) * 24;
                if (n > MaxItems) y += 22;
            }

            y += 8 + 1 + 14;                                         // vạch ngăn
            if (_detail != null) y += 22 + 22;                       // tiền món + giảm giá
            y += 30;                                                 // tổng thanh toán
            return y + Pad - 4;
        }

        // ── Vẽ ──────────────────────────────────────────────────────
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(Color.White);
            using (var pen = new Pen(ThemeManager.Line))
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            using (var br = new SolidBrush(ThemeManager.Primary))
                g.FillRectangle(br, 0, 0, Width, 4);                 // dải màu nhấn trên cùng

            if (_summary == null) return;
            int contentW = Width - Pad * 2;
            int y = Pad;

            // Tiêu đề + trạng thái
            TextRenderer.DrawText(g, "Đơn hàng #" + _summary.MaHoaDon, _fTitle, new Point(Pad, y + 2), ThemeManager.Ink, Flags);
            string status = _detail?.TrangThai ?? _summary.TrangThai ?? "";
            if (!string.IsNullOrWhiteSpace(status))
            {
                Color fore, back;
                if (!ThemeManager.TryStatusColors(status, out fore, out back)) { fore = ThemeManager.Ink2; back = ThemeManager.Surface2; }
                var sz = TextRenderer.MeasureText(g, status, _fPill, Size.Empty, Flags);
                var pill = new Rectangle(Width - Pad - sz.Width - 20, y + 2, sz.Width + 20, 24);
                using (var path = ThemeManager.RoundedRect(pill, 12))
                using (var br = new SolidBrush(back))
                    g.FillPath(br, path);
                TextRenderer.DrawText(g, status, _fPill, pill, fore, Flags | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
            y += 30 + 14;

            // Thông tin chung
            foreach (var r in InfoRows())
            {
                int h = ValueHeight(r.Value, _fValue, contentW - LabelWidth);
                TextRenderer.DrawText(g, r.Key, _fLabel, new Rectangle(Pad, y + 1, LabelWidth, 18), ThemeManager.Muted, Flags);
                TextRenderer.DrawText(g, r.Value, _fValue, new Rectangle(Pad + LabelWidth, y, contentW - LabelWidth, h), ThemeManager.Ink, Flags | TextFormatFlags.WordBreak);
                y += h + 8;
            }

            y = Divider(g, y);

            // Món ăn
            TextRenderer.DrawText(g, "MÓN ĂN", _fSection, new Point(Pad, y), ThemeManager.Muted, Flags);
            y += 18;
            if (_loading)
            {
                TextRenderer.DrawText(g, "Đang tải chi tiết...", _fValue, new Point(Pad, y + 3), ThemeManager.Muted, Flags);
                y += 24;
            }
            else
            {
                var items = _detail?.ChiTiet ?? new List<ChiTietHoaDonDto>();
                if (items.Count == 0)
                {
                    TextRenderer.DrawText(g, "Không có món", _fValue, new Point(Pad, y + 3), ThemeManager.Muted, Flags);
                    y += 24;
                }
                foreach (var it in items.Take(MaxItems))
                {
                    string qty = it.SoLuong + "×";
                    string money = it.ThanhTienHienThi;
                    var moneyW = TextRenderer.MeasureText(g, money, _fValue, Size.Empty, Flags).Width;
                    TextRenderer.DrawText(g, qty, _fValueBold, new Rectangle(Pad, y + 3, 32, 18), ThemeManager.Primary, Flags);
                    TextRenderer.DrawText(g, it.TenMon ?? it.MaMon ?? "", _fValue, new Rectangle(Pad + 32, y + 3, contentW - 32 - moneyW - 10, 18),
                        ThemeManager.Ink, Flags | TextFormatFlags.EndEllipsis);
                    TextRenderer.DrawText(g, money, _fValue, new Rectangle(Pad, y + 3, contentW, 18), ThemeManager.Ink2, Flags | TextFormatFlags.Right);
                    y += 24;
                }
                if (items.Count > MaxItems)
                {
                    TextRenderer.DrawText(g, "+ " + (items.Count - MaxItems) + " món khác", _fLabel, new Point(Pad + 32, y + 3), ThemeManager.Muted, Flags);
                    y += 22;
                }
            }

            y = Divider(g, y);

            // Tiền
            if (_detail != null)
            {
                MoneyRow(g, ref y, "Tiền món ăn", (_detail.TongTienHang ?? 0).ToString("N0") + " đ", ThemeManager.Ink2);
                MoneyRow(g, ref y, "Giảm voucher", "-" + (_detail.TienGiamVoucher ?? 0).ToString("N0") + " đ", ThemeManager.Green);
            }
            double tong = _detail?.TongThanhToan ?? _summary.TongThanhToan ?? 0;
            TextRenderer.DrawText(g, "Tổng thanh toán", _fValueBold, new Point(Pad, y + 6), ThemeManager.Ink, Flags);
            TextRenderer.DrawText(g, tong.ToString("N0") + " đ", _fTotal, new Rectangle(Pad, y + 3, contentW, 26), ThemeManager.Primary, Flags | TextFormatFlags.Right);
        }

        private int Divider(Graphics g, int y)
        {
            y += 8;
            using (var pen = new Pen(ThemeManager.Line))
                g.DrawLine(pen, Pad, y, Width - Pad, y);
            return y + 1 + 14;
        }

        private void MoneyRow(Graphics g, ref int y, string label, string value, Color valueColor)
        {
            int contentW = Width - Pad * 2;
            TextRenderer.DrawText(g, label, _fLabel, new Point(Pad, y + 2), ThemeManager.Muted, Flags);
            TextRenderer.DrawText(g, value, _fValue, new Rectangle(Pad, y, contentW, 20), valueColor, Flags | TextFormatFlags.Right);
            y += 22;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fTitle.Dispose(); _fLabel.Dispose(); _fValue.Dispose(); _fValueBold.Dispose();
                _fSection.Dispose(); _fTotal.Dispose(); _fPill.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

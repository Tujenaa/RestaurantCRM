using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.DTOs.PhanHoi;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls.PhanHoi
{
    /// <summary>Thẻ xem nhanh đánh giá khi rê chuột lên một dòng trong danh sách.</summary>
    public class ReviewHoverCard : Form
    {
        private const int CardWidth = 420;
        private const int CardHeight = 268;
        private readonly DataGridView _grid;
        private readonly Label _title = new Label();
        private readonly Label _state = new Label();
        private readonly Label _meta = new Label();
        private readonly Label _stars = new Label();
        private readonly RichTextBox _comment = new RichTextBox();
        // Theo dõi chuột khi thẻ đang hiện: rời dòng đánh giá là ẩn ngay (giống thẻ xem nhanh đơn hàng)
        private readonly Timer _watch = new Timer { Interval = 60 };
        private bool _shown;
        private int _shownRow = -1;
        private bool _gridDisposed;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static readonly IntPtr HwndTopmost = new IntPtr(-1);
        private const uint SwpNoActivate = 0x0010, SwpShowWindow = 0x0040, SwpNoOwnerZOrder = 0x0200;
        private const int SwHide = 0;

        public ReviewHoverCard(DataGridView grid)
        {
            _grid = grid;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            BackColor = Color.White;
            Size = new Size(CardWidth, CardHeight);
            Tag = UiStyler.NoStyleTag;
            Padding = new Padding(16);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            _title.SetBounds(16, 14, 286, 26);
            _title.Font = new Font(ThemeManager.FontFamily, 11F, FontStyle.Bold);
            _title.ForeColor = ThemeManager.Ink;
            _title.AutoEllipsis = true;
            _title.UseMnemonic = false;
            Controls.Add(_title);

            _state.SetBounds(306, 14, 98, 25);
            _state.TextAlign = ContentAlignment.MiddleCenter;
            _state.Font = new Font(ThemeManager.FontFamily, 8F, FontStyle.Bold);
            _state.UseMnemonic = false;
            Controls.Add(_state);

            _meta.SetBounds(16, 45, 388, 20);
            _meta.Font = new Font(ThemeManager.FontFamily, 8.5F);
            _meta.ForeColor = ThemeManager.Muted;
            _meta.UseMnemonic = false;
            Controls.Add(_meta);

            _stars.SetBounds(16, 70, 388, 24);
            _stars.Font = new Font(ThemeManager.FontFamily, 12F, FontStyle.Bold);
            _stars.ForeColor = ThemeManager.Amber;
            _stars.UseMnemonic = false;
            Controls.Add(_stars);

            var commentTitle = new Label
            {
                Text = "NỘI DUNG ĐÁNH GIÁ",
                Bounds = new Rectangle(16, 101, 388, 18),
                Font = new Font(ThemeManager.FontFamily, 8F, FontStyle.Bold),
                ForeColor = ThemeManager.Muted,
                UseMnemonic = false
            };
            Controls.Add(commentTitle);

            _comment.SetBounds(16, 124, 388, 126);
            _comment.ReadOnly = true;
            _comment.BorderStyle = BorderStyle.None;
            _comment.BackColor = ThemeManager.Surface2;
            _comment.ForeColor = ThemeManager.Ink;
            _comment.Font = new Font(ThemeManager.FontFamily, 9F);
            _comment.ScrollBars = RichTextBoxScrollBars.Vertical;
            _comment.DetectUrls = false;
            Controls.Add(_comment);

            _watch.Tick += (s, e) => SyncWithCursor();

            _grid.CellMouseEnter += (s, e) => { if (e.RowIndex >= 0) ShowRow(e.RowIndex); else HideCard(); };
            _grid.MouseMove += (s, e) => SyncWithCursor();
            _grid.MouseLeave += (s, e) => SyncWithCursor();
            _grid.Scroll += (s, e) => HideCard();
            _grid.Disposed += (s, e) => { _gridDisposed = true; HideCard(); Dispose(); };
        }

        protected override bool ShowWithoutActivation { get { return true; } }

        protected override CreateParams CreateParams
        {
            get
            {
                const int WsExNoActivate = 0x08000000;
                const int WsExToolWindow = 0x00000080;
                const int CsDropShadow = 0x00020000;
                var cp = base.CreateParams;
                cp.ExStyle |= WsExNoActivate | WsExToolWindow | 0x00000008;
                cp.ClassStyle |= CsDropShadow;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(ThemeManager.Line)) e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            using (var brush = new SolidBrush(ThemeManager.Primary)) e.Graphics.FillRectangle(brush, 0, 0, Width, 4);
        }

        /// <summary>Chuột đang ở dòng nào thì hiện đúng dòng đó; không ở dòng nào (hoặc đè lên thẻ) thì ẩn ngay.</summary>
        private void SyncWithCursor()
        {
            if (_gridDisposed || IsDisposed) return;
            var cursor = Cursor.Position;
            var form = _grid.FindForm();
            bool active = form != null && Form.ActiveForm == form;
            var pt = _grid.PointToClient(cursor);
            int row = _grid.ClientRectangle.Contains(pt) ? _grid.HitTest(pt.X, pt.Y).RowIndex : -1;

            if (!_grid.Visible || !active || row < 0 || (_shown && Bounds.Contains(cursor)))
            {
                HideCard();
                return;
            }
            if (!_shown || row != _shownRow) ShowRow(row);
        }

        /// <summary>" · 2 chấm riêng" hoặc " · không đánh giá riêng món nào" (món SoSao NULL = không đánh giá riêng).</summary>
        private static string MoTaChamRieng(System.Collections.Generic.List<DanhGiaMonDto> mons)
        {
            int chamRieng = 0;
            foreach (var m in mons) if (m != null && m.SoSao.HasValue) chamRieng++;
            return chamRieng == 0 ? " · không đánh giá riêng món nào" : " · " + chamRieng + " chấm riêng";
        }

        private void ShowRow(int rowIndex)
        {
            if (_gridDisposed || rowIndex < 0 || rowIndex >= _grid.Rows.Count) return;
            var form = _grid.FindForm();
            if (form == null || Form.ActiveForm != form) return;
            var review = _grid.Rows[rowIndex].Tag as PhanHoiDto;
            if (review == null) { HideCard(); return; }
            if (_shown && rowIndex == _shownRow) return;
            _title.Text = review.DoiTuong;
            _meta.Text = "Ngày gửi: " + (review.NgayDanhGia.HasValue ? review.NgayDanhGia.Value.ToString("dd/MM/yyyy HH:mm") : "—")
                + (review.ChiTietMon != null && review.ChiTietMon.Count > 0
                    ? "  ·  " + review.ChiTietMon.Count + " món"
                      + MoTaChamRieng(review.ChiTietMon)
                      + (review.SoMonBiChe > 0 ? " · " + review.SoMonBiChe + " món bị chê" : "")
                    : "");
            _meta.ForeColor = review.CanChuY ? ThemeManager.Red : ThemeManager.Muted;
            int stars = Math.Max(0, Math.Min(5, review.SoSao ?? 0));
            _stars.Text = new string('★', stars) + new string('☆', 5 - stars)
                + (review.SoSao.HasValue ? "  " + review.SoSao.Value + " / 5" : "  Chưa có số sao");
            _comment.Text = string.IsNullOrWhiteSpace(review.NoiDung) ? "(Khách chỉ chấm sao, không để lại nội dung)" : review.NoiDung;
            _state.Text = review.HienThi ? "ĐANG HIỂN THỊ" : "ĐÃ ẨN";
            _state.ForeColor = review.HienThi ? ThemeManager.Green : ThemeManager.Muted;
            _state.BackColor = review.HienThi ? ThemeManager.GreenSoft : ThemeManager.Surface2;

            var row = _grid.GetRowDisplayRectangle(rowIndex, true);
            var rowScreen = _grid.RectangleToScreen(row);
            var cursor = Cursor.Position;
            var working = Screen.FromPoint(cursor).WorkingArea;
            int x = cursor.X + 24;
            if (x + CardWidth > working.Right - 8) x = cursor.X - CardWidth - 24;
            x = Math.Max(working.Left + 8, x);
            int y = rowScreen.Top - 10;
            if (y + CardHeight > working.Bottom - 8) y = working.Bottom - CardHeight - 8;
            y = Math.Max(working.Top + 8, y);

            if (!IsHandleCreated) CreateControl();
            if (!Visible) Show();
            SetWindowPos(Handle, HwndTopmost, x, y, CardWidth, CardHeight,
                SwpNoActivate | SwpNoOwnerZOrder | SwpShowWindow);
            _shown = true;
            _shownRow = rowIndex;
            _watch.Start();
            Invalidate();
        }

        public void HideCard()
        {
            _watch.Stop();
            _shownRow = -1;
            if (!_shown) return;
            _shown = false;
            if (IsHandleCreated) ShowWindow(Handle, SwHide);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _watch.Dispose();
            base.Dispose(disposing);
        }
    }
}

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
        private readonly Timer _hideTimer = new Timer { Interval = 260 };
        private bool _shown;
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

            _hideTimer.Tick += (s, e) =>
            {
                _hideTimer.Stop();
                if (!_gridDisposed && (_grid.RectangleToScreen(_grid.ClientRectangle).Contains(Cursor.Position)
                    || Bounds.Contains(Cursor.Position))) return;
                HideCard();
            };

            MouseEnter += (s, e) => CancelHide();
            MouseLeave += (s, e) => ScheduleHide();
            foreach (Control c in Controls)
            {
                c.MouseEnter += (s, e) => CancelHide();
                c.MouseLeave += (s, e) => ScheduleHide();
            }

            _grid.CellMouseEnter += GridCellMouseEnter;
            _grid.MouseLeave += (s, e) => ScheduleHide();
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

        private void GridCellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _grid.Rows.Count) return;
            var review = _grid.Rows[e.RowIndex].Tag as PhanHoiDto;
            if (review == null) return;

            CancelHide();
            _title.Text = review.DoiTuong;
            _meta.Text = "Ngày gửi: " + (review.NgayDanhGia.HasValue ? review.NgayDanhGia.Value.ToString("dd/MM/yyyy HH:mm") : "—");
            int stars = Math.Max(0, Math.Min(5, review.SoSao ?? 0));
            _stars.Text = new string('★', stars) + new string('☆', 5 - stars)
                + (review.SoSao.HasValue ? "  " + review.SoSao.Value + " / 5" : "  Chưa có số sao");
            _comment.Text = string.IsNullOrWhiteSpace(review.NoiDung) ? "(Khách chỉ chấm sao, không để lại nội dung)" : review.NoiDung;
            _state.Text = review.HienThi ? "ĐANG HIỂN THỊ" : "ĐÃ ẨN";
            _state.ForeColor = review.HienThi ? ThemeManager.Green : ThemeManager.Muted;
            _state.BackColor = review.HienThi ? ThemeManager.GreenSoft : ThemeManager.Surface2;

            var row = _grid.GetRowDisplayRectangle(e.RowIndex, true);
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
            Invalidate();
        }

        private void ScheduleHide()
        {
            if (_shown) _hideTimer.Start();
        }

        private void CancelHide()
        {
            _hideTimer.Stop();
        }

        public void HideCard()
        {
            _hideTimer.Stop();
            if (!_shown) return;
            _shown = false;
            if (IsHandleCreated) ShowWindow(Handle, SwHide);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _hideTimer.Dispose();
            base.Dispose(disposing);
        }
    }
}

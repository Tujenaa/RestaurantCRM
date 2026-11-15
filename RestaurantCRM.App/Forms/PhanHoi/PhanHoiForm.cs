using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Controls.PhanHoi;
using RestaurantCRM.AdminApp.DTOs.PhanHoi;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.PhanHoi
{
    /// <summary>
    /// Quản lý phản hồi / góp ý của khách hàng (dữ liệu DANH_GIA + TRA_LOI_DANH_GIA):
    /// danh sách, tìm kiếm, lọc theo trạng thái xử lý và số sao, xem nội dung + hội thoại,
    /// gửi trả lời cho khách, xóa câu trả lời gửi nhầm, ẩn / hiện phản hồi trên website, xóa phản hồi.
    /// </summary>
    public partial class PhanHoiForm : Form
    {
        private static readonly string[] MauTraLoi =
        {
            "Cảm ơn anh/chị đã dành thời gian góp ý. Nhà hàng rất vui khi anh/chị hài lòng và mong được phục vụ anh/chị lần sau!",
            "Nhà hàng thành thật xin lỗi vì trải nghiệm chưa tốt. Chúng tôi đã ghi nhận và sẽ cải thiện ngay. Mong anh/chị cho nhà hàng cơ hội phục vụ tốt hơn.",
            "Cảm ơn góp ý của anh/chị. Bộ phận chăm sóc khách hàng sẽ liên hệ qua số điện thoại đã đăng ký để hỗ trợ thêm.",
        };

        private readonly PhanHoiService _service = new PhanHoiService();
        private List<PhanHoiDto> _all = new List<PhanHoiDto>();

        // "Tìm theo" (mặc định: tất cả)
        private readonly SearchScopes<PhanHoiDto> _scopes = new SearchScopes<PhanHoiDto>("PhanHoi")
            .AddAll("Tìm mã ĐG, mã đơn, KH, SĐT, món ăn, nội dung...", d => new[] { d.TrangThaiHienThi })
            .Add("Nội dung đánh giá", "Nội dung", "Tìm trong nội dung đánh giá...",
                d => new[] { d.NoiDung, d.ChiTietMon != null ? string.Join(" ", d.ChiTietMon.Select(m => m.NoiDung ?? "")) : null })
            .Add("Khách hàng (tên, SĐT, mã)", "Khách", "Tìm tên, SĐT hoặc mã khách...", d => new[] { d.TenKhachHang, d.SoDienThoai, d.MaKhachHang })
            .Add("Món ăn", "Món", "Tìm theo món được đánh giá...",
                d => new[] { d.TenMon, d.ChiTietMon != null ? string.Join(" ", d.ChiTietMon.Select(m => m.TenMon ?? "")) : null })
            .Add("Mã đánh giá / mã đơn", "Mã", "Tìm mã đánh giá hoặc mã đơn...", d => new[] { d.MaDanhGia, d.MaHoaDon });
        private PhanHoiDto _current;
        private bool _loading;

        // Danh sách
        private readonly DataGridView _grid = new DataGridView();
        private readonly TextBox _txtSearch = new TextBox();
        private readonly ComboBox _cboTrangThai = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _cboSoSao = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _cboThoiGian = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _cboNoiDung = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private const string SaoTatCa = "Tất cả", SaoTieuCuc = "Từ 3 sao trở xuống";
        private readonly Label _lblCount = new Label();
        private readonly Label _lblFooter = new Label();
        private readonly Button _btnPhanTich = new Button { Text = "Phân tích món" };
        private readonly Label _kTong = new Label { Text = "Tổng phản hồi: —" };
        private readonly Label _kCho = new Label { Text = "Chưa xử lý: —" };
        private readonly Label _kDiem = new Label { Text = "Điểm trung bình: —" };
        private readonly Label _kAn = new Label { Text = "Cần chú ý: —" };
        private readonly Button _btnLamMoi = new Button { Text = "Làm mới" };
        // Đổi trạng thái xử lý 2 chiều: "✓ Đã xử lý" <-> "↺ Chưa xử lý"
        private readonly Button _btnDaXem = new Button { Text = "✓ Đánh dấu đã xử lý" };
        private readonly Button _btnAnHien = new Button { Text = "Ẩn phản hồi" };

        // Khung chi tiết bên phải
        private readonly Panel _detail = new Panel { Width = 340 };
        private readonly Panel _head = new Panel { Dock = DockStyle.Top, Height = 104 };
        private readonly Panel _thread = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        private readonly Panel _composer = new Panel { Dock = DockStyle.Bottom, Height = 156 };
        // Hiện thay khung soạn khi phản hồi đã được trả lời (mỗi phản hồi chỉ trả lời 1 lần)
        private readonly Label _closedNote = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 36,
            TextAlign = ContentAlignment.MiddleCenter,
            UseMnemonic = false,
            Text = "✓  Đã xử lý"
        };
        private const int DoDaiTraLoiToiDa = 1000;   // khớp giới hạn của API
        private readonly TextBox _txtReply = new TextBox { Multiline = true, BorderStyle = BorderStyle.None, MaxLength = DoDaiTraLoiToiDa, ScrollBars = ScrollBars.None };
        private readonly Label _replyHint = new Label { Text = "Nhập nội dung trả lời khách hàng...", AutoSize = true, UseMnemonic = false, Cursor = Cursors.IBeam };
        private readonly Panel _replyBox = new Panel();
        private readonly Button _btnGui = new Button { Text = "Gửi trả lời" };
        private readonly Button _btnMau = new Button { Text = "Mẫu câu" };
        private readonly Label _lblDem = new Label { Text = "Ctrl+Enter để gửi · 0/1000", AutoSize = false, TextAlign = ContentAlignment.MiddleRight };
        private readonly Label _empty = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            UseMnemonic = false,
            Text = "Chọn một phản hồi trong danh sách\nđể xem nội dung và trả lời khách hàng."
        };
        private ReviewHoverCard _hoverCard;
        // Khung soạn đang ở chế độ sửa câu trả lời (khách sửa đánh giá sau phản hồi)
        private bool _dangSuaTraLoi;

        public PhanHoiForm()
        {
            InitializeComponent();
            Text = "Phản hồi khách hàng";
            Size = new Size(1240, 740);
            Font = new Font(ThemeManager.FontFamily, 9.5F);
            StartPosition = FormStartPosition.CenterParent;

            BuildGrid();
            BuildDetail();
            _hoverCard = new ReviewHoverCard(_grid);
            _btnPhanTich.Font = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold);
            ButtonSkin.SetKind(_btnPhanTich, ButtonSkin.Kind.Secondary);
            PageKit.FitButton(_btnPhanTich, 116);

            _cboTrangThai.Items.AddRange(new object[] { "Tất cả", "Cần chú ý", LocCanXuLy, LocSuaSauPhanHoi, "Đã xử lý", "Đã ẩn" });
            _cboTrangThai.SelectedIndex = 0;
            _cboSoSao.Items.AddRange(new object[] { SaoTatCa, "5 sao", "4 sao", "3 sao", "2 sao", "1 sao", SaoTieuCuc });
            _cboSoSao.SelectedIndex = 0;
            _cboThoiGian.Items.AddRange(new object[] { "Tất cả", "Hôm nay", "7 ngày qua", "30 ngày qua", "Tháng này" });
            _cboThoiGian.SelectedIndex = 0;
            _cboNoiDung.Items.AddRange(new object[] { "Tất cả", "Có nhận xét chữ", "Đã trả lời", "Chưa trả lời" });
            _cboNoiDung.SelectedIndex = 0;

            PageKit.BuildListPage(this, new ListPageSpec
            {
                Title = new Label { Text = "Phản hồi khách hàng" },
                Subtitle = new Label { Text = "Đọc góp ý, trả lời khách và theo dõi trạng thái xử lý." },
                HeaderActions = new Control[] { _btnLamMoi, _btnPhanTich },
                KpiLabels = new[] { _kCho, _kDiem },   // 2 ô, cỡ như khi có 5 ô (đồng bộ trang Đơn hàng / Phiếu nhập)
                Search = _txtSearch,
                SearchPlaceholder = "Tìm mã ĐG, mã đơn, KH, SĐT, món ăn, nội dung...",
                SearchScopes = _scopes,
                Filters = new[]
                {
                    new FilterSpec(_cboTrangThai, new Label { Text = "Trạng thái" }),
                    new FilterSpec(_cboSoSao, new Label { Text = "Số sao" }),
                    new FilterSpec(_cboThoiGian, new Label { Text = "Thời gian" }),
                    new FilterSpec(_cboNoiDung, new Label { Text = "Loại" })
                },
                CountLabel = _lblCount,
                Grid = _grid,
                SecondaryActions = new[] { _btnDaXem, _btnAnHien },
                FooterInfo = _lblFooter,
                SidePanel = _detail,
                SideWidth = 400,
                SideFullHeight = true,
                KpiSlots = 5
            });

            // Bảng hẹp vì có khung chi tiết bên phải: không xuống dòng, cột co được nhỏ
            _grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            foreach (DataGridViewColumn c in _grid.Columns)
                c.MinimumWidth = c.Name == "Ngay" ? 96 : c.Name == "SoSao" ? 92
                    : c.Name == "TrangThai" ? 104 : c.Name == "HienThi" ? 100 : c.Name == "ChuY" || c.Name == "Chon" ? 34 : c.Name == "MaDG" ? 64 : 56;
            foreach (var hep in new[] { "ChuY", "Chon" })
            {
                if (_grid.Columns[hep] == null) continue;
                _grid.Columns[hep].Width = 34;
                _grid.Columns[hep].FillWeight = 12;
            }

            _txtSearch.TextChanged += (s, e) => ApplyFilter();
            _scopes.Changed += (s, e) => ApplyFilter();
            _cboTrangThai.SelectedIndexChanged += (s, e) => ApplyFilter();
            _cboSoSao.SelectedIndexChanged += (s, e) => ApplyFilter();
            _cboThoiGian.SelectedIndexChanged += (s, e) => ApplyFilter();
            // Ô tick: lưu ngay khi bấm (không chờ rời ô); bấm tiêu đề cột tick để chọn/bỏ tất cả
            _grid.CurrentCellDirtyStateChanged += (s, e) => { if (_grid.IsCurrentCellDirty) _grid.CommitEdit(DataGridViewDataErrorContexts.Commit); };
            _grid.CellValueChanged += Grid_ChonThayDoi;
            _grid.ColumnHeaderMouseClick += (s, e) => { if (e.ColumnIndex >= 0 && _grid.Columns[e.ColumnIndex].Name == "Chon") ChonTatCaDangHien(); };
            new ToolTip(components).SetToolTip(_cboThoiGian, "Tính theo ngày gửi hoặc ngày khách sửa gần nhất");
            _cboNoiDung.SelectedIndexChanged += (s, e) => ApplyFilter();
            _grid.SelectionChanged += (s, e) => ShowDetail(SelectedDto());
            // Sao luôn màu vàng (UiStyler đặt lại màu chữ chung của bảng nên phải gán khi định dạng ô)
            _grid.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex >= 0 && _grid.Columns[e.ColumnIndex].Name == "ChuY")
                {
                    e.CellStyle.ForeColor = e.CellStyle.SelectionForeColor = ThemeManager.Red;
                    e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                if (e.ColumnIndex >= 0 && _grid.Columns[e.ColumnIndex].Name == "SoSao")
                {
                    var d = e.RowIndex >= 0 ? _grid.Rows[e.RowIndex].Tag as PhanHoiDto : null;
                    e.CellStyle.ForeColor = e.CellStyle.SelectionForeColor = d != null && !d.HienThi ? ThemeManager.Muted : ThemeManager.Amber;
                }
                if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && _grid.Columns[e.ColumnIndex].Name == "Ngay")
                {
                    var d = _grid.Rows[e.RowIndex].Tag as PhanHoiDto;
                    if (d != null)
                    {
                        e.Value = NgayGuiHienThi(d);
                        e.FormattingApplied = true;
                        // Khách sửa sau khi nhà hàng trả lời: tô cam để nhân viên chú ý xem lại
                        if (d.SuaSauPhanHoi) e.CellStyle.ForeColor = e.CellStyle.SelectionForeColor = ThemeManager.Amber;
                    }
                }
            };

            _btnLamMoi.Click += async (s, e) => await LoadDataAsync();
            _btnPhanTich.Click += (s, e) =>
            {
                _hoverCard.HideCard();
                using (var dlg = new PhanTichDanhGiaDialog(_service)) dlg.ShowDialog(this);
            };
            _btnDaXem.Click += (s, e) => DanhDauDaXem();
            _btnAnHien.Click += (s, e) => DoiAnHien();
            _btnGui.Click += async (s, e) => await GuiTraLoiAsync();

            ShowDetail(null);
            Load += async (s, e) => await LoadDataAsync();
        }

        // ── Dựng giao diện ───────────────────────────────────────────────────

        private void BuildGrid()
        {
            var g = _grid;
            g.AllowUserToAddRows = false; g.AllowUserToDeleteRows = false; g.AllowUserToResizeRows = false;
            // Chỉ cột tick được sửa; các cột chữ đặt ReadOnly trong AddCol
            g.ReadOnly = false; g.MultiSelect = false; g.RowHeadersVisible = false; g.AutoGenerateColumns = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Tick chọn nhiều đánh giá để đánh dấu "Đã xử lý" một lần; bấm tiêu đề để chọn/bỏ tất cả dòng đang hiện
            g.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "Chon",
                HeaderText = "☐",
                Width = 34,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                ToolTipText = "Tick để đánh dấu đã xử lý nhiều đánh giá; bấm tiêu đề để chọn/bỏ tất cả"
            });
            var chuY = AddCol("ChuY", "⚠", 34);
            chuY.ToolTipText = "Cần chú ý: chấm 1–2 sao hoặc có món bị chấm thấp";
            chuY.SortMode = DataGridViewColumnSortMode.NotSortable;
            AddCol("MaDG", "Mã ĐG", 70);
            var sao = AddCol("SoSao", "Đánh giá", 96);
            sao.ToolTipText = "Bấm tiêu đề cột để sắp xếp theo số sao";
            sao.DefaultCellStyle.ForeColor = ThemeManager.Amber;
            sao.DefaultCellStyle.SelectionForeColor = ThemeManager.Amber;
            // Một ngày: ngày khách sửa gần nhất (có ✎) nếu đã sửa, không thì ngày gửi.
            // Giá trị ô là NgayHoatDong để bấm tiêu đề sắp xếp đúng theo ngày đang hiển thị.
            var ngay = AddCol("Ngay", "Ngày", 104);
            ngay.ToolTipText = "Ngày gửi; ✎ = ngày khách sửa gần nhất";
            AddCol("HienThi", "Hiển thị", 100).Tag = "status";   // tô pill màu như cột trạng thái
            AddCol("TrangThai", "Xử lý", 108);
        }

        private DataGridViewColumn AddCol(string name, string header, int width)
        {
            var c = new DataGridViewTextBoxColumn { Name = name, HeaderText = header, Width = width, SortMode = DataGridViewColumnSortMode.Automatic, ReadOnly = true };
            _grid.Columns.Add(c);
            return c;
        }

        private void BuildDetail()
        {
            _detail.BackColor = Color.White;
            _head.BackColor = Color.White;
            _thread.BackColor = Color.White;
            _composer.BackColor = Color.White;
            SkinUtil.EnableDoubleBuffer(_head);
            _head.Paint += PaintHead;

            _empty.Font = new Font(ThemeManager.FontFamily, 9.5F);
            _empty.ForeColor = ThemeManager.Muted;
            _empty.BackColor = Color.White;

            // Ô soạn trả lời: textbox không viền đặt trong khung bo tròn tự vẽ
            _replyBox.BackColor = Color.White;
            _replyBox.Padding = new Padding(12, 10, 8, 8);
            _replyBox.Paint += (s, e) =>
            {
                var r = new Rectangle(0, 0, _replyBox.Width - 1, _replyBox.Height - 1);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = SkinUtil.Round(r, 10))
                using (var pen = new Pen(_txtReply.Focused ? ThemeManager.Primary : ThemeManager.Line, _txtReply.Focused ? 1.5f : 1f))
                    e.Graphics.DrawPath(pen, path);
            };
            _txtReply.Dock = DockStyle.Fill;
            _txtReply.Font = new Font(ThemeManager.FontFamily, 9.5F);
            _txtReply.ForeColor = ThemeManager.Ink;
            _txtReply.GotFocus += (s, e) => _replyBox.Invalidate();
            _txtReply.LostFocus += (s, e) => _replyBox.Invalidate();
            _txtReply.TextChanged += (s, e) =>
            {
                _lblDem.Text = "Ctrl+Enter để gửi · " + _txtReply.TextLength + "/" + DoDaiTraLoiToiDa;
                _btnGui.Enabled = _current != null && _txtReply.Text.Trim().Length > 0;
            };
            _txtReply.KeyDown += async (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await GuiTraLoiAsync(); }
            };
            _replyBox.Controls.Add(_txtReply);
            // Gợi ý tự vẽ: EM_SETCUEBANNER không hoạt động với TextBox nhiều dòng
            _replyHint.Font = new Font(ThemeManager.FontFamily, 9.5F);
            _replyHint.ForeColor = ThemeManager.Muted;
            _replyHint.BackColor = Color.White;
            _replyHint.Location = new Point(1, 0);
            _replyHint.Click += (s, e) => _txtReply.Focus();
            _txtReply.Controls.Add(_replyHint);
            _txtReply.TextChanged += (s, e) => _replyHint.Visible = _txtReply.TextLength == 0;

            ButtonSkin.SetKind(_btnGui, ButtonSkin.Kind.Primary);
            PageKit.FitButton(_btnGui, 112);
            ButtonSkin.SetKind(_btnMau, ButtonSkin.Kind.Ghost);
            PageKit.FitButton(_btnMau, 88);
            _lblDem.Font = new Font(ThemeManager.FontFamily, 8.5F);
            _lblDem.ForeColor = ThemeManager.Muted;

            var menu = new ContextMenuStrip { ShowImageMargin = false, Font = new Font(ThemeManager.FontFamily, 9F) };
            foreach (var m in MauTraLoi)
            {
                string text = m;
                var item = menu.Items.Add(text.Length > 70 ? text.Substring(0, 67) + "..." : text);
                item.ToolTipText = text;
                item.Click += (s, e) => { _txtReply.Text = text; _txtReply.Focus(); _txtReply.SelectionStart = _txtReply.TextLength; };
            }
            _btnMau.Click += (s, e) => menu.Show(_btnMau, new Point(0, _btnMau.Height + 2));

            _composer.Controls.Add(_replyBox);
            _composer.Controls.Add(_btnGui);
            _composer.Controls.Add(_btnMau);
            _composer.Controls.Add(_lblDem);
            _composer.Paint += (s, e) => { using (var p = new Pen(ThemeManager.Line)) e.Graphics.DrawLine(p, 0, 0, _composer.Width, 0); };
            _composer.Resize += (s, e) => LayoutComposer();
            _btnGui.SizeChanged += (s, e) => LayoutComposer();

            _thread.Resize += (s, e) => LayoutThread();

            _closedNote.Font = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold);
            _closedNote.ForeColor = ThemeManager.Green;
            _closedNote.BackColor = ThemeManager.GreenSoft;

            _detail.Controls.Add(_thread);
            _detail.Controls.Add(_composer);
            _detail.Controls.Add(_closedNote);
            _detail.Controls.Add(_head);
            _detail.Controls.Add(_empty);
            _thread.BringToFront();
        }

        private void LayoutComposer()
        {
            int w = _composer.ClientSize.Width;
            _replyBox.SetBounds(0, 12, w, 88);
            int y = 12 + 88 + 10;
            _btnGui.Location = new Point(w - _btnGui.Width, y);
            _btnMau.Location = new Point(0, y);
            _lblDem.SetBounds(_btnMau.Right + 6, y, Math.Max(10, _btnGui.Left - _btnMau.Right - 14), _btnGui.Height);
        }

        private void PaintHead(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            var d = _current;
            if (d == null) return;
            const TextFormatFlags F = TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding;
            int w = _head.Width;

            CrmPaint.Avatar(g, d.TenKhachHang, new Rectangle(0, 2, 44, 44));
            using (var fName = new Font(ThemeManager.FontFamily, 11F, FontStyle.Bold))
            using (var fSmall = new Font(ThemeManager.FontFamily, 8.5F))
            {
                string name = string.IsNullOrWhiteSpace(d.TenKhachHang) ? "Khách ẩn danh" : d.TenKhachHang;
                // Pill trạng thái ở góc phải
                Color fore, back;
                if (!ThemeManager.TryStatusColors(d.TrangThaiHienThi, out fore, out back)) { fore = ThemeManager.Ink2; back = ThemeManager.Surface2; }
                int pillW;
                using (var fPill = new Font(ThemeManager.FontFamily, 8.5F, FontStyle.Bold))
                    pillW = TextRenderer.MeasureText(g, d.TrangThaiHienThi, fPill, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width + 18;
                CrmPaint.Pill(g, d.TrangThaiHienThi, w - pillW, 4, fore, back);

                TextRenderer.DrawText(g, name, fName, new Rectangle(56, 2, w - 56 - pillW - 8, 24), ThemeManager.Ink, F);
                string sub = (string.IsNullOrWhiteSpace(d.SoDienThoai) ? (d.MaKhachHang ?? "Không có tài khoản") : d.SoDienThoai)
                             + "  ·  " + d.MaDanhGia;
                TextRenderer.DrawText(g, sub, fSmall, new Rectangle(56, 27, w - 56, 18), ThemeManager.Muted, F);

                int x = CrmPaint.Stars(g, d.SoSao ?? 0, 0, 60, 11F);
                string meta = "  Gửi lúc " + (d.NgayDanhGia.HasValue ? d.NgayDanhGia.Value.ToString("HH:mm dd/MM/yyyy") : "—") + "  ·  Về: " + d.DoiTuong;
                TextRenderer.DrawText(g, meta, fSmall, new Rectangle(x + 4, 62, w - x - 4, 18), ThemeManager.Ink2, F);
                if (d.NgayCapNhat.HasValue)
                {
                    string sua = "✎ Sửa lần cuối lúc " + d.NgayCapNhat.Value.ToString("HH:mm dd/MM/yyyy")
                                 + (d.SuaSauPhanHoi ? "  ·  sau khi nhà hàng trả lời" : "");
                    TextRenderer.DrawText(g, sua, fSmall, new Rectangle(0, 82, w, 18),
                        d.SuaSauPhanHoi ? ThemeManager.Amber : ThemeManager.Muted, F);
                }
            }
            using (var p = new Pen(ThemeManager.Line)) g.DrawLine(p, 0, _head.Height - 1, w, _head.Height - 1);
        }

        // ── Dữ liệu ──────────────────────────────────────────────────────────

        public async Task LoadDataAsync(string chonMa = null)
        {
            if (_loading) return;
            _hoverCard.HideCard();
            _loading = true;
            _btnLamMoi.Enabled = false;
            _lblCount.Text = "Đang tải...";
            if (chonMa == null) chonMa = SelectedDto()?.MaDanhGia;
            try
            {
                _all = await _service.GetAllAsync(null, PhanHoiService.LocTatCa);
                // Bỏ tick những đánh giá không còn hoặc đã được xử lý (ở nơi khác)
                var conCho = new HashSet<string>(_all.Where(x => !x.LaDaXuLy).Select(x => x.MaDanhGia));
                _daChon.RemoveWhere(ma => !conCho.Contains(ma));
                UpdateKpis();
                ApplyFilter(chonMa);
            }
            catch (ApiException ex)
            {
                _lblCount.Text = "Không tải được dữ liệu";
                if (!IsDisposed) Notify.Error(this, ex.Message, "Lỗi tải danh sách phản hồi");
            }
            finally
            {
                _loading = false;
                if (!IsDisposed) _btnLamMoi.Enabled = true;
            }
        }

        private void UpdateKpis()
        {
            var coSao = _all.Where(d => d.SoSao.HasValue && d.SoSao > 0).ToList();
            _kTong.Text = "Tổng phản hồi: " + _all.Count.ToString("N0");
            _kCho.Text = "Cần xử lý: " + _all.Count(d => !d.LaDaXuLy).ToString("N0");
            _kDiem.Text = "Điểm trung bình: " + (coSao.Count == 0 ? "—" : coSao.Average(d => d.SoSao.Value).ToString("0.0") + " / 5");
            _kAn.Text = "Cần chú ý: " + _all.Count(d => d.CanChuY && !d.LaDaXuLy).ToString("N0");
        }

        private void ApplyFilter(string chonMa = null)
        {
            _hoverCard.HideCard();
            if (chonMa == null) chonMa = SelectedDto()?.MaDanhGia;
            string q = (_txtSearch.Text ?? "").Trim();
            string tt = _cboTrangThai.SelectedItem as string ?? "Tất cả";
            string locSao = _cboSoSao.SelectedItem as string ?? SaoTatCa;
            int timeIdx = _cboThoiGian.SelectedIndex;
            int ndIdx = _cboNoiDung.SelectedIndex;
            DateTime today = DateTime.Today;
            int saoChinhXac = 0;
            if (locSao != SaoTatCa && locSao != SaoTieuCuc) int.TryParse(locSao.Substring(0, 1), out saoChinhXac);

            var list = _all.Where(d =>
            {
                if (tt == "Đã ẩn") { if (d.HienThi) return false; }
                else if (tt == "Cần chú ý") { if (!d.CanChuY) return false; }
                // Hàng chờ: mọi đánh giá DaXuLy = 0 (khách sửa đánh giá cũng đưa DaXuLy về 0)
                else if (tt == LocCanXuLy) { if (d.LaDaXuLy) return false; }
                // Theo dữ liệu: khách sửa sau lần trả lời gần nhất, kể cả khi nhân viên đã giữ câu trả lời cũ
                else if (tt == LocSuaSauPhanHoi) { if (!d.SuaSauPhanHoi) return false; }
                else if (tt == "Đã xử lý") { if (!d.LaDaXuLy) return false; }

                if (locSao == SaoTieuCuc) { if (!d.SoSao.HasValue || d.SoSao.Value > 3) return false; }
                else if (saoChinhXac > 0 && (d.SoSao ?? 0) != saoChinhXac) return false;

                // Khớp nếu ngày gửi HOẶC ngày khách sửa gần nhất nằm trong khoảng thời gian đã chọn
                if (timeIdx > 0 && !TrongKhoang(d.NgayDanhGia, timeIdx, today) && !TrongKhoang(d.NgayCapNhat, timeIdx, today))
                    return false;

                if (ndIdx == 1 && string.IsNullOrWhiteSpace(d.NoiDung) && (d.ChiTietMon == null || !d.ChiTietMon.Any(m => !string.IsNullOrWhiteSpace(m.NoiDung))))
                    return false;
                if (ndIdx == 2 && !d.DaTraLoi) return false;
                if (ndIdx == 3 && d.DaTraLoi) return false;

                if (q.Length == 0) return true;
                return _scopes.Matches(q, d);
            })
            // Ưu tiên: chưa xử lý + cần chú ý → chưa xử lý → đã xử lý; trong mỗi nhóm hoạt động mới nhất
            // của khách (lần sửa, nếu chưa sửa thì lần gửi) lên trước
            .OrderBy(d => d.LaDaXuLy ? 2 : (d.CanChuY ? 0 : 1))
            .ThenByDescending(d => d.NgayHoatDong ?? DateTime.MinValue)
            .ToList();

            _dangVeLuoi = true;
            _grid.SuspendLayout();
            _grid.Rows.Clear();
            int chon = -1;
            foreach (var d in list)
            {
                int i = _grid.Rows.Add(_daChon.Contains(d.MaDanhGia), d.CanChuY ? "⚠" : "", d.MaDanhGia, Sao(d.SoSao ?? 0), (object)d.NgayHoatDong,
                    d.HienThi ? "Hiển thị" : "Đã ẩn", d.TrangThaiHienThi);
                _grid.Rows[i].Tag = d;
                _grid.Rows[i].Cells["Ngay"].ToolTipText = NgayTooltip(d);
                if (!d.HienThi) _grid.Rows[i].DefaultCellStyle.ForeColor = ThemeManager.Muted;
                if (chonMa != null && d.MaDanhGia == chonMa) chon = i;
            }
            _grid.ResumeLayout();
            _dangVeLuoi = false;
            _grid.ClearSelection();
            if (chon >= 0)
            {
                _grid.Rows[chon].Selected = true;
                _grid.CurrentCell = _grid.Rows[chon].Cells["MaDG"];
            }
            else
            {
                // Chưa chọn phản hồi nào: không để lưới tự chọn dòng đầu, khung bên phải để trống
                _grid.CurrentCell = null;
            }
            _soDongHienThi = list.Count;
            CapNhatTieuDeChon();
            CapNhatDem();
            ShowDetail(SelectedDto());
        }

        // ── Tick chọn nhiều để đánh dấu "Đã xử lý" ───────────────────────────

        /// <summary>Mã các đánh giá đang được tick; giữ qua lọc / sắp xếp / tải lại.</summary>
        private readonly HashSet<string> _daChon = new HashSet<string>();
        private bool _dangVeLuoi;
        private int _soDongHienThi;

        private void CapNhatDem()
        {
            string dem = _soDongHienThi == _all.Count ? "Hiển thị " + _soDongHienThi + " phản hồi" : "Hiển thị " + _soDongHienThi + " / " + _all.Count + " phản hồi";
            _lblCount.Text = _daChon.Count > 0 ? dem + " · Đã chọn " + _daChon.Count : dem;
        }

        /// <summary>Tiêu đề cột tick: ☑ khi mọi dòng đang hiện đều được tick.</summary>
        private void CapNhatTieuDeChon()
        {
            bool tatCa = _grid.Rows.Count > 0 && _grid.Rows.Cast<DataGridViewRow>().All(r => r.Tag is PhanHoiDto d && _daChon.Contains(d.MaDanhGia));
            _grid.Columns["Chon"].HeaderText = tatCa ? "☑" : "☐";
        }

        /// <summary>Số đánh giá đang tick mà chưa xử lý (sẽ được chuyển sang Đã xử lý).</summary>
        private List<PhanHoiDto> DaChonChuaXuLy()
        {
            return _all.Where(d => _daChon.Contains(d.MaDanhGia) && !d.LaDaXuLy).ToList();
        }

        private void Grid_ChonThayDoi(object sender, DataGridViewCellEventArgs e)
        {
            if (_dangVeLuoi || e.RowIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "Chon") return;
            var d = _grid.Rows[e.RowIndex].Tag as PhanHoiDto;
            if (d == null) return;
            if (Equals(_grid.Rows[e.RowIndex].Cells["Chon"].Value, true)) _daChon.Add(d.MaDanhGia);
            else _daChon.Remove(d.MaDanhGia);
            CapNhatTieuDeChon();
            CapNhatDem();
            CapNhatNutXuLy();
        }

        /// <summary>Bấm tiêu đề cột tick: chọn hết các dòng đang hiện, hoặc bỏ hết nếu đã chọn hết.</summary>
        private void ChonTatCaDangHien()
        {
            var dongs = _grid.Rows.Cast<DataGridViewRow>().Select(r => r.Tag as PhanHoiDto).Where(d => d != null).ToList();
            bool chonHet = !dongs.All(d => _daChon.Contains(d.MaDanhGia));
            foreach (var d in dongs)
            {
                if (chonHet) _daChon.Add(d.MaDanhGia); else _daChon.Remove(d.MaDanhGia);
            }
            _dangVeLuoi = true;
            foreach (DataGridViewRow r in _grid.Rows) r.Cells["Chon"].Value = chonHet;
            _dangVeLuoi = false;
            CapNhatTieuDeChon();
            CapNhatDem();
            CapNhatNutXuLy();
        }

        /// <summary>
        /// Có tick: nút đánh dấu hàng loạt "✓ Đánh dấu đã xử lý (N)". Không tick: đổi 2 chiều cho dòng đang chọn như cũ.
        /// </summary>
        private void CapNhatNutXuLy()
        {
            var d = _current;
            if (_daChon.Count > 0)
            {
                int n = DaChonChuaXuLy().Count;
                _btnDaXem.Text = "✓ Đánh dấu đã xử lý (" + n + ")";
                _btnDaXem.Enabled = n > 0;
            }
            else
            {
                _btnDaXem.Text = d != null && d.LaDaXuLy ? "↺ Chuyển về chưa xử lý" : "✓ Đánh dấu đã xử lý";
                _btnDaXem.Enabled = d != null;
            }
            PageKit.FitButton(_btnDaXem, 120);
        }

        private async Task DanhDauDaXuLyHangLoatAsync()
        {
            var ds = DaChonChuaXuLy();
            if (ds.Count == 0) return;
            if (!Notify.Confirm(this, "Đánh dấu " + ds.Count + " đánh giá đã chọn là đã xử lý?", "Đánh dấu đã xử lý")) return;

            _btnDaXem.Enabled = false;
            _btnDaXem.Text = "Đang xử lý...";
            int thanhCong = 0;
            var loi = new List<string>();
            foreach (var d in ds)
            {
                var r = await _service.SetProcessedAsync(d.MaDanhGia, true);
                if (IsDisposed) return;
                if (r.Success) thanhCong++; else loi.Add(d.MaDanhGia + ": " + r.Message);
            }

            _daChon.Clear();
            await LoadDataAsync(_current?.MaDanhGia);
            if (loi.Count == 0) Notify.Info(this, "Đã chuyển " + thanhCong + " đánh giá sang Đã xử lý.");
            else Notify.Error(this, "Đã chuyển " + thanhCong + " đánh giá sang Đã xử lý; " + loi.Count + " đánh giá bị lỗi.\n" + loi[0]);
        }

        private const string LocCanXuLy = "Cần xử lý";
        private const string LocSuaSauPhanHoi = "Khách sửa sau phản hồi";

        private static string Sao(int n) { n = Math.Max(0, Math.Min(5, n)); return new string('★', n) + new string('☆', 5 - n); }

        /// <summary>Ngày có nằm trong khoảng của bộ lọc Thời gian không (1 Hôm nay, 2 7 ngày qua, 3 30 ngày qua, 4 Tháng này).</summary>
        private static bool TrongKhoang(DateTime? ngay, int timeIdx, DateTime today)
        {
            if (!ngay.HasValue) return false;
            DateTime dt = ngay.Value.Date;
            switch (timeIdx)
            {
                case 1: return dt == today;
                case 2: return dt >= today.AddDays(-6);
                case 3: return dt >= today.AddDays(-29);
                case 4: return dt.Month == today.Month && dt.Year == today.Year;
                default: return true;
            }
        }

        /// <summary>Đã sửa: "✎ 22/10/2026" (ngày sửa gần nhất); chưa sửa: "20/10/2026" (ngày gửi).</summary>
        private static string NgayGuiHienThi(PhanHoiDto d)
        {
            if (d.NgayCapNhat.HasValue) return "✎ " + d.NgayCapNhat.Value.ToString("dd/MM/yyyy");
            return d.NgayDanhGia.HasValue ? d.NgayDanhGia.Value.ToString("dd/MM/yyyy") : "—";
        }

        /// <summary>Gợi ý khi di chuột vào ô ngày: đủ ngày gửi và ngày sửa.</summary>
        private static string NgayTooltip(PhanHoiDto d)
        {
            string gui = "Gửi lúc " + (d.NgayDanhGia.HasValue ? d.NgayDanhGia.Value.ToString("HH:mm dd/MM/yyyy") : "—");
            return d.NgayCapNhat.HasValue ? gui + "\nSửa lần cuối lúc " + d.NgayCapNhat.Value.ToString("HH:mm dd/MM/yyyy") : gui;
        }

        /// <summary>Câu trả lời gần nhất của nhà hàng (theo ngày gửi/sửa).</summary>
        private static TraLoiPhanHoiDto TraLoiMoiNhat(PhanHoiDto d)
        {
            return d?.HoiThoai == null ? null : d.HoiThoai.OrderByDescending(t => t.NgayGui ?? DateTime.MinValue).FirstOrDefault();
        }

        /// <summary>Khách sửa sau phản hồi và chưa xử lý → nhân viên được sửa câu trả lời hiện có.</summary>
        private static bool LaCheDoSuaTraLoi(PhanHoiDto d)
        {
            return d != null && d.SuaSauPhanHoi && !d.LaDaXuLy && TraLoiMoiNhat(d) != null;
        }

        private PhanHoiDto SelectedDto()
        {
            if (_grid.SelectedRows.Count == 0) return null;
            return _grid.SelectedRows[0].Tag as PhanHoiDto;
        }

        // ── Khung chi tiết ───────────────────────────────────────────────────

        private void ShowDetail(PhanHoiDto d)
        {
            bool doiDong = _current == null || d == null || _current.MaDanhGia != d.MaDanhGia;
            _current = d;
            bool has = d != null;
            _empty.Visible = !has;
            // Đã xử lý (hoặc đã trả lời 1 lần) => đóng khung soạn trả lời.
            // Ngoại lệ: khách sửa đánh giá sau khi nhà hàng trả lời và chưa xử lý → mở khung để sửa câu trả lời.
            bool suaTraLoi = LaCheDoSuaTraLoi(d);
            bool cheDoDoi = suaTraLoi != _dangSuaTraLoi;
            _dangSuaTraLoi = suaTraLoi;
            bool dong = has && (d.LaDaXuLy || d.DaTraLoi) && !suaTraLoi;
            _head.Visible = has; _thread.Visible = has;
            _composer.Visible = has && !dong;
            _closedNote.Visible = dong;
            if (dong)
                _closedNote.Text = d.DaTraLoi
                    ? "✓  Đã trả lời"
                    : "✓  Đã xử lý";
            if (has) _empty.SendToBack(); else _empty.BringToFront();

            _btnAnHien.Enabled = has;
            CapNhatNutXuLy();
            _btnAnHien.Text = has && !d.HienThi ? "Hiện lại phản hồi" : "Ẩn phản hồi";
            _lblFooter.Text = has
                ? (d.TenKhachHang ?? "Khách ẩn danh") + " · " + d.TrangThaiHienThi + (d.DaTraLoi ? " · đã trả lời" : "")
                : "Chọn một phản hồi để xem và trả lời";

            if (doiDong || cheDoDoi)
                _txtReply.Text = suaTraLoi ? (TraLoiMoiNhat(d).NoiDung ?? "") : "";
            _btnGui.Text = suaTraLoi ? "Cập nhật trả lời" : "Gửi trả lời";
            PageKit.FitButton(_btnGui, 112);
            _replyHint.Text = suaTraLoi ? "Sửa lại câu trả lời cho phù hợp với đánh giá mới..." : "Nhập nội dung trả lời khách hàng...";
            _btnGui.Enabled = has && _txtReply.Text.Trim().Length > 0;
            _txtReply.Enabled = has;
            _head.Invalidate();
            RenderThread();
        }

        private void RenderThread()
        {
            _thread.SuspendLayout();
            var old = _thread.Controls.Cast<Control>().ToArray();
            _thread.Controls.Clear();
            foreach (var c in old) c.Dispose();

            var d = _current;
            if (d != null)
            {
                _thread.Controls.Add(new ChatBubble
                {
                    // Thời gian là lần gửi gốc; nội dung đã sửa thì ghi rõ để nhân viên biết
                    Author = (string.IsNullOrWhiteSpace(d.TenKhachHang) ? "Khách hàng" : d.TenKhachHang)
                             + (d.NgayCapNhat.HasValue ? " (đã sửa " + d.NgayCapNhat.Value.ToString("dd/MM") + ")" : ""),
                    Time = d.NgayDanhGia,
                    Body = string.IsNullOrWhiteSpace(d.NoiDung) ? "(Khách chỉ chấm sao, không để lại nội dung)" : d.NoiDung,
                    Mine = false
                });
                if (d.ChiTietMon != null && d.ChiTietMon.Count > 0)
                    _thread.Controls.Add(new DanhGiaMonList(d.ChiTietMon));
                if (d.SuaSauPhanHoi)
                {
                    _thread.Controls.Add(new Label
                    {
                        Text = "⚠ Khách đã chỉnh sửa sau khi nhà hàng trả lời. Hãy xem lại câu trả lời.",
                        AutoSize = false,
                        Height = 34,
                        UseMnemonic = false,
                        ForeColor = ThemeManager.Amber,
                        BackColor = ThemeManager.AmberSoft,
                        Font = new Font(ThemeManager.FontFamily, 8.5F, FontStyle.Bold),
                        TextAlign = ContentAlignment.MiddleCenter
                    });
                }
                foreach (var t in d.HoiThoai ?? new List<TraLoiPhanHoiDto>())
                {
                    var traLoi = t;
                    var b = new ChatBubble
                    {
                        Author = string.IsNullOrWhiteSpace(t.NguoiTraLoi) ? "Nhân viên" : t.NguoiTraLoi,
                        Time = t.NgayGui,
                        Body = t.NoiDung ?? "",
                        Mine = true,
                        CanDelete = true
                    };
                    b.DeleteClicked += async (s, e) => await XoaTraLoiAsync(traLoi);
                    _thread.Controls.Add(b);
                }
                if (!d.HienThi)
                {
                    _thread.Controls.Add(new Label
                    {
                        Text = "Phản hồi đang bị ẩn trên website. Khách vẫn xem được câu trả lời trong tài khoản của mình.",
                        AutoSize = false,
                        Height = 40,
                        UseMnemonic = false,
                        ForeColor = ThemeManager.Muted,
                        Font = new Font(ThemeManager.FontFamily, 8.5F, FontStyle.Italic),
                        TextAlign = ContentAlignment.MiddleCenter
                    });
                }
            }
            LayoutThread();
            _thread.ResumeLayout();
            if (_thread.Controls.Count > 0) _thread.ScrollControlIntoView(_thread.Controls[_thread.Controls.Count - 1]);
        }

        private void LayoutThread()
        {
            int w = _thread.Width - SystemInformation.VerticalScrollBarWidth - 2;
            int y = 14 + _thread.AutoScrollPosition.Y;
            foreach (Control c in _thread.Controls)
            {
                var b = c as ChatBubble;
                var mon = c as DanhGiaMonList;
                if (b != null) b.LayoutFor(w);
                else if (mon != null) mon.LayoutFor(w);
                else c.Width = w;
                c.Location = new Point(0, y);
                y += c.Height + 10;
            }
        }

        // ── Thao tác ─────────────────────────────────────────────────────────

        private async Task GuiTraLoiAsync()
        {
            var d = _current;
            string nd = _txtReply.Text.Trim();
            if (d == null || nd.Length == 0) return;
            if (string.IsNullOrWhiteSpace(UserSession.CurrentUserId))
            {
                Notify.Warn(this, "Không xác định được nhân viên đang đăng nhập. Vui lòng đăng nhập lại.");
                return;
            }

            // Khách sửa sau phản hồi → cập nhật câu trả lời hiện có thay vì gửi câu mới
            var traLoiCu = LaCheDoSuaTraLoi(d) ? TraLoiMoiNhat(d) : null;
            string chuNut = _btnGui.Text;
            _btnGui.Enabled = false; _txtReply.Enabled = false;
            _btnGui.Text = "Đang gửi...";
            var r = traLoiCu != null
                ? await _service.UpdateReplyAsync(d.MaDanhGia, traLoiCu.MaTraLoi, nd)
                : await _service.ReplyAsync(d.MaDanhGia, nd);
            if (IsDisposed) return;
            _btnGui.Text = chuNut;
            _txtReply.Enabled = true;
            if (!r.Success) { _btnGui.Enabled = true; Notify.Error(this, r.Message); return; }

            _txtReply.Text = "";
            await LoadDataAsync(d.MaDanhGia);
        }

        private async Task XoaTraLoiAsync(TraLoiPhanHoiDto t)
        {
            var d = _current;
            if (d == null || t == null) return;
            if (!Notify.Confirm(this, "Xóa câu trả lời này của " + (t.NguoiTraLoi ?? "nhân viên") + "?\nKhách hàng sẽ không còn thấy câu trả lời.", "Xóa câu trả lời", true)) return;
            var r = await _service.DeleteReplyAsync(d.MaDanhGia, t.MaTraLoi);
            if (IsDisposed) return;
            if (!r.Success) { Notify.Error(this, r.Message); return; }
            await LoadDataAsync(d.MaDanhGia);
        }

        private async void DanhDauDaXem()
        {
            // Có đánh giá đang tick → đánh dấu đã xử lý hàng loạt
            if (_daChon.Count > 0) { await DanhDauDaXuLyHangLoatAsync(); return; }
            var d = _current;
            if (d == null) return;
            _btnDaXem.Enabled = false;
            var r = await _service.SetProcessedAsync(d.MaDanhGia, !d.LaDaXuLy);
            if (IsDisposed) return;
            if (!r.Success) { _btnDaXem.Enabled = true; Notify.Error(this, r.Message); return; }
            await LoadDataAsync(d.MaDanhGia);
        }

        private async void DoiAnHien()
        {
            var d = _current;
            if (d == null) return;
            bool hien = !d.HienThi;
            if (!hien && !Notify.Confirm(this, "Ẩn phản hồi " + d.MaDanhGia + " khỏi website?\nBạn có thể hiện lại bất cứ lúc nào.", "Ẩn phản hồi")) return;
            _btnAnHien.Enabled = false;
            var r = await _service.SetVisibleAsync(d.MaDanhGia, hien);
            if (IsDisposed) return;
            if (!r.Success) { _btnAnHien.Enabled = true; Notify.Error(this, r.Message); return; }
            await LoadDataAsync(d.MaDanhGia);
        }
    }
}

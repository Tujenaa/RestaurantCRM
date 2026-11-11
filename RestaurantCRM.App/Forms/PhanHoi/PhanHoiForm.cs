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
        private readonly Button _btnXoa = new Button { Text = "Xóa phản hồi" };

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
        private readonly TextBox _txtReply = new TextBox { Multiline = true, BorderStyle = BorderStyle.None, MaxLength = 2000, ScrollBars = ScrollBars.None };
        private readonly Label _replyHint = new Label { Text = "Nhập nội dung trả lời khách hàng...", AutoSize = true, UseMnemonic = false, Cursor = Cursors.IBeam };
        private readonly Panel _replyBox = new Panel();
        private readonly Button _btnGui = new Button { Text = "Gửi trả lời" };
        private readonly Button _btnMau = new Button { Text = "Mẫu câu" };
        private readonly Label _lblDem = new Label { Text = "Ctrl+Enter để gửi · 0/2000", AutoSize = false, TextAlign = ContentAlignment.MiddleRight };
        private readonly Label _empty = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            UseMnemonic = false,
            Text = "Chọn một phản hồi trong danh sách\nđể xem nội dung và trả lời khách hàng."
        };
        private ReviewHoverCard _hoverCard;

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

            _cboTrangThai.Items.AddRange(new object[] { "Tất cả", "Cần chú ý", "Chưa xử lý", "Đã xử lý", "Đã ẩn" });
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
                DangerActions = new[] { _btnXoa },
                FooterInfo = _lblFooter,
                SidePanel = _detail,
                SideWidth = 400,
                SideFullHeight = true,
                KpiSlots = 5
            });

            // Bảng hẹp vì có khung chi tiết bên phải: không xuống dòng, cột co được nhỏ
            _grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            foreach (DataGridViewColumn c in _grid.Columns)
                c.MinimumWidth = c.Name == "NoiDung" ? 120 : c.Name == "SoSao" ? 92
                    : c.Name == "TrangThai" ? 104 : c.Name == "HienThi" ? 100 : c.Name == "ChuY" ? 34 : 56;
            if (_grid.Columns["ChuY"] != null)
            {
                _grid.Columns["ChuY"].Width = 34;
                _grid.Columns["ChuY"].FillWeight = 12;
            }

            _txtSearch.TextChanged += (s, e) => ApplyFilter();
            _cboTrangThai.SelectedIndexChanged += (s, e) => ApplyFilter();
            _cboSoSao.SelectedIndexChanged += (s, e) => ApplyFilter();
            _cboThoiGian.SelectedIndexChanged += (s, e) => ApplyFilter();
            _cboNoiDung.SelectedIndexChanged += (s, e) => ApplyFilter();
            _grid.SelectionChanged += (s, e) => ShowDetail(SelectedDto());
            _grid.KeyDown += (s, e) => { if (e.KeyCode == Keys.Delete) { e.Handled = true; XoaPhanHoi(); } };
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
            };

            _btnLamMoi.Click += async (s, e) => await LoadDataAsync();
            _btnPhanTich.Click += (s, e) =>
            {
                _hoverCard.HideCard();
                using (var dlg = new PhanTichDanhGiaDialog(_service)) dlg.ShowDialog(this);
            };
            _btnDaXem.Click += (s, e) => DanhDauDaXem();
            _btnAnHien.Click += (s, e) => DoiAnHien();
            _btnXoa.Click += (s, e) => XoaPhanHoi();
            _btnGui.Click += async (s, e) => await GuiTraLoiAsync();

            ShowDetail(null);
            Load += async (s, e) => await LoadDataAsync();
        }

        // ── Dựng giao diện ───────────────────────────────────────────────────

        private void BuildGrid()
        {
            var g = _grid;
            g.AllowUserToAddRows = false; g.AllowUserToDeleteRows = false; g.AllowUserToResizeRows = false;
            g.ReadOnly = true; g.MultiSelect = false; g.RowHeadersVisible = false; g.AutoGenerateColumns = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            var chuY = AddCol("ChuY", "⚠", 34);
            chuY.ToolTipText = "Cần chú ý: chấm 1–2 sao hoặc có món bị chấm thấp";
            chuY.SortMode = DataGridViewColumnSortMode.NotSortable;
            var sao = AddCol("SoSao", "Đánh giá", 96);
            sao.ToolTipText = "Bấm tiêu đề cột để sắp xếp theo số sao";
            sao.DefaultCellStyle.ForeColor = ThemeManager.Amber;
            sao.DefaultCellStyle.SelectionForeColor = ThemeManager.Amber;
            AddCol("NoiDung", "Nội dung", 260);
            AddCol("Ngay", "Ngày gửi", 88).DefaultCellStyle.Format = "dd/MM/yyyy";
            AddCol("HienThi", "Trạng thái hiển thị", 100);
            AddCol("TrangThai", "Xử lý", 108);
        }

        private DataGridViewColumn AddCol(string name, string header, int width)
        {
            var c = new DataGridViewTextBoxColumn { Name = name, HeaderText = header, Width = width, SortMode = DataGridViewColumnSortMode.Automatic };
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
                _lblDem.Text = "Ctrl+Enter để gửi · " + _txtReply.TextLength + "/2000";
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
                string meta = "  " + (d.NgayDanhGia.HasValue ? d.NgayDanhGia.Value.ToString("HH:mm · dd/MM/yyyy") : "") + "  ·  Về: " + d.DoiTuong;
                TextRenderer.DrawText(g, meta, fSmall, new Rectangle(x + 4, 62, w - x - 4, 18), ThemeManager.Ink2, F);
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
            _kCho.Text = "Chưa xử lý: " + _all.Count(d => !d.LaDaXuLy).ToString("N0");
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
                else if (tt != "Tất cả" && d.TrangThaiHienThi != tt) return false;

                if (locSao == SaoTieuCuc) { if (!d.SoSao.HasValue || d.SoSao.Value > 3) return false; }
                else if (saoChinhXac > 0 && (d.SoSao ?? 0) != saoChinhXac) return false;

                if (timeIdx > 0)
                {
                    if (!d.NgayDanhGia.HasValue) return false;
                    DateTime dt = d.NgayDanhGia.Value.Date;
                    if (timeIdx == 1 && dt != today) return false;
                    if (timeIdx == 2 && dt < today.AddDays(-6)) return false;
                    if (timeIdx == 3 && dt < today.AddDays(-29)) return false;
                    if (timeIdx == 4 && (dt.Month != today.Month || dt.Year != today.Year)) return false;
                }

                if (ndIdx == 1 && string.IsNullOrWhiteSpace(d.NoiDung) && (d.ChiTietMon == null || !d.ChiTietMon.Any(m => !string.IsNullOrWhiteSpace(m.NoiDung))))
                    return false;
                if (ndIdx == 2 && !d.DaTraLoi) return false;
                if (ndIdx == 3 && d.DaTraLoi) return false;

                if (q.Length == 0) return true;
                string chiTietText = d.ChiTietMon != null && d.ChiTietMon.Count > 0
                    ? string.Join(" ", d.ChiTietMon.Select(m => (m.TenMon ?? "") + " " + (m.NoiDung ?? "")))
                    : null;
                return UiStyler.MatchesSearch(q, d.MaDanhGia, d.MaHoaDon, d.TenKhachHang, d.MaKhachHang, d.SoDienThoai, d.TenMon, d.NoiDung, chiTietText);
            })
            // Ưu tiên: chưa xử lý + cần chú ý → chưa xử lý → đã xử lý; trong mỗi nhóm mới nhất lên trước
            .OrderBy(d => d.LaDaXuLy ? 2 : (d.CanChuY ? 0 : 1))
            .ThenByDescending(d => d.NgayDanhGia ?? DateTime.MinValue)
            .ToList();

            _grid.SuspendLayout();
            _grid.Rows.Clear();
            int chon = -1;
            foreach (var d in list)
            {
                int i = _grid.Rows.Add(d.CanChuY ? "⚠" : "", Sao(d.SoSao ?? 0), MotDong(d.NoiDung), (object)d.NgayDanhGia,
                    d.HienThi ? "Đang hiển thị" : "Đã ẩn", d.TrangThaiHienThi);
                _grid.Rows[i].Tag = d;
                if (!d.HienThi) _grid.Rows[i].DefaultCellStyle.ForeColor = ThemeManager.Muted;
                if (chonMa != null && d.MaDanhGia == chonMa) chon = i;
            }
            _grid.ResumeLayout();
            _grid.ClearSelection();
            if (chon >= 0)
            {
                _grid.Rows[chon].Selected = true;
                _grid.CurrentCell = _grid.Rows[chon].Cells[0];
            }
            else
            {
                // Chưa chọn phản hồi nào: không để lưới tự chọn dòng đầu, khung bên phải để trống
                _grid.CurrentCell = null;
            }
            _lblCount.Text = list.Count == _all.Count ? "Hiển thị " + list.Count + " phản hồi" : "Hiển thị " + list.Count + " / " + _all.Count + " phản hồi";
            ShowDetail(SelectedDto());
        }

        private static bool Has(string s, string q) { return s != null && s.ToLowerInvariant().Contains(q); }
        private static string Sao(int n) { n = Math.Max(0, Math.Min(5, n)); return new string('★', n) + new string('☆', 5 - n); }
        private static string MotDong(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "(không có nội dung)";
            s = s.Replace("\r", " ").Replace("\n", " ").Trim();
            return s.Length > 120 ? s.Substring(0, 117) + "..." : s;
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
            // Đã xử lý (hoặc đã trả lời 1 lần) => đóng khung soạn trả lời
            bool dong = has && (d.LaDaXuLy || d.DaTraLoi);
            _head.Visible = has; _thread.Visible = has;
            _composer.Visible = has && !dong;
            _closedNote.Visible = dong;
            if (dong)
                _closedNote.Text = d.DaTraLoi
                    ? "✓  Đã trả lời"
                    : "✓  Đã xử lý";
            if (has) _empty.SendToBack(); else _empty.BringToFront();

            _btnAnHien.Enabled = has;
            _btnDaXem.Enabled = has;
            _btnDaXem.Text = has && d.LaDaXuLy ? "↺ Chuyển về chưa xử lý" : "✓ Đánh dấu đã xử lý";
            PageKit.FitButton(_btnDaXem, 120);
            _btnXoa.Enabled = has;
            _btnAnHien.Text = has && !d.HienThi ? "Hiện lại phản hồi" : "Ẩn phản hồi";
            _lblFooter.Text = has
                ? (d.TenKhachHang ?? "Khách ẩn danh") + " · " + d.TrangThaiHienThi + (d.DaTraLoi ? " · đã trả lời" : "")
                : "Chọn một phản hồi để xem và trả lời";

            if (doiDong) _txtReply.Text = "";
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
                    Author = string.IsNullOrWhiteSpace(d.TenKhachHang) ? "Khách hàng" : d.TenKhachHang,
                    Time = d.NgayDanhGia,
                    Body = string.IsNullOrWhiteSpace(d.NoiDung) ? "(Khách chỉ chấm sao, không để lại nội dung)" : d.NoiDung,
                    Mine = false
                });
                if (d.ChiTietMon != null && d.ChiTietMon.Count > 0)
                    _thread.Controls.Add(new DanhGiaMonList(d.ChiTietMon));
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

            _btnGui.Enabled = false; _txtReply.Enabled = false;
            _btnGui.Text = "Đang gửi...";
            var r = await _service.ReplyAsync(d.MaDanhGia, nd);
            if (IsDisposed) return;
            _btnGui.Text = "Gửi trả lời";
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

        private async void XoaPhanHoi()
        {
            var d = _current;
            if (d == null) return;
            string hoi = "Bạn có chắc chắn muốn gỡ bỏ phản hồi " + d.MaDanhGia + " của " + (d.TenKhachHang ?? "khách hàng") + " khỏi website?\n\n"
                         + "Lưu ý: Phản hồi sẽ được ẩn khỏi website công khai, nhưng toàn bộ nội dung và lịch sử trả lời vẫn được bảo lưu an toàn trong hệ thống.";
            if (!Notify.Confirm(this, hoi, "Ẩn phản hồi khỏi website", true)) return;
            _btnXoa.Enabled = false;
            var r = await _service.DeleteAsync(d.MaDanhGia);
            if (IsDisposed) return;
            if (!r.Success) { _btnXoa.Enabled = true; Notify.Error(this, r.Message); return; }
            Notify.Info(this, r.Message ?? "Đã ẩn phản hồi khỏi website thành công.");
            _current = null;
            await LoadDataAsync();
        }
    }
}

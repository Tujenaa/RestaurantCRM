using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
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
        private readonly Label _lblCount = new Label();
        private readonly Label _lblFooter = new Label();
        private readonly Label _kTong = new Label { Text = "Tổng phản hồi: —" };
        private readonly Label _kCho = new Label { Text = "Chưa trả lời: —" };
        private readonly Label _kDiem = new Label { Text = "Điểm trung bình: —" };
        private readonly Label _kAn = new Label { Text = "Đã ẩn: —" };
        private readonly Button _btnLamMoi = new Button { Text = "Làm mới" };
        private readonly Button _btnAnHien = new Button { Text = "Ẩn phản hồi" };
        private readonly Button _btnXoa = new Button { Text = "Xóa phản hồi" };

        // Khung chi tiết bên phải
        private readonly Panel _detail = new Panel { Width = 320 };
        private readonly Panel _head = new Panel { Dock = DockStyle.Top, Height = 104 };
        private readonly Panel _thread = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        private readonly Panel _composer = new Panel { Dock = DockStyle.Bottom, Height = 150 };
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

        public PhanHoiForm()
        {
            InitializeComponent();
            Text = "Phản hồi khách hàng";
            Size = new Size(1240, 740);
            Font = new Font(ThemeManager.FontFamily, 9.5F);
            StartPosition = FormStartPosition.CenterParent;

            BuildGrid();
            BuildDetail();

            _cboTrangThai.Items.AddRange(new object[] { "Tất cả", "Chưa trả lời", "Đã trả lời", "Đã ẩn" });
            _cboTrangThai.SelectedIndex = 0;

            PageKit.BuildListPage(this, new ListPageSpec
            {
                Title = new Label { Text = "Phản hồi khách hàng" },
                Subtitle = new Label { Text = "Đọc góp ý, trả lời khách và theo dõi trạng thái xử lý." },
                HeaderActions = new Control[] { _btnLamMoi },
                KpiLabels = new[] { _kTong, _kCho, _kDiem, _kAn },
                Search = _txtSearch,
                SearchPlaceholder = "Tìm theo mã, khách hàng, món ăn, nội dung...",
                Filters = new[] { new FilterSpec(_cboTrangThai, new Label { Text = "Trạng thái" }) },
                CountLabel = _lblCount,
                Grid = _grid,
                SecondaryActions = new[] { _btnAnHien },
                DangerActions = new[] { _btnXoa },
                FooterInfo = _lblFooter,
                SidePanel = _detail,
                SideWidth = 390
            });

            // Bảng hẹp vì có khung chi tiết bên phải: không xuống dòng, cột co được nhỏ
            _grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            foreach (DataGridViewColumn c in _grid.Columns)
                c.MinimumWidth = c.Name == "NoiDung" ? 120 : c.Name == "SoSao" ? 92 : c.Name == "TrangThai" ? 104 : 56;

            _txtSearch.TextChanged += (s, e) => ApplyFilter();
            _cboTrangThai.SelectedIndexChanged += (s, e) => ApplyFilter();
            _grid.SelectionChanged += (s, e) => ShowDetail(SelectedDto());
            _grid.KeyDown += (s, e) => { if (e.KeyCode == Keys.Delete) { e.Handled = true; XoaPhanHoi(); } };
            // Sao luôn màu vàng (UiStyler đặt lại màu chữ chung của bảng nên phải gán khi định dạng ô)
            _grid.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex >= 0 && _grid.Columns[e.ColumnIndex].Name == "SoSao")
                {
                    var d = e.RowIndex >= 0 ? _grid.Rows[e.RowIndex].Tag as PhanHoiDto : null;
                    e.CellStyle.ForeColor = e.CellStyle.SelectionForeColor = d != null && !d.HienThi ? ThemeManager.Muted : ThemeManager.Amber;
                }
            };

            _btnLamMoi.Click += async (s, e) => await LoadDataAsync();
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

            AddCol("Ma", "Mã", 58);
            AddCol("KhachHang", "Khách hàng", 130);
            var sao = AddCol("SoSao", "Đánh giá", 96);
            sao.ToolTipText = "Bấm tiêu đề cột để sắp xếp theo số sao";
            sao.DefaultCellStyle.ForeColor = ThemeManager.Amber;
            sao.DefaultCellStyle.SelectionForeColor = ThemeManager.Amber;
            AddCol("NoiDung", "Nội dung", 260);
            AddCol("Ngay", "Ngày gửi", 88).DefaultCellStyle.Format = "dd/MM/yyyy";
            AddCol("TrangThai", "Trạng thái", 112);
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

            _detail.Controls.Add(_thread);
            _detail.Controls.Add(_composer);
            _detail.Controls.Add(_head);
            _detail.Controls.Add(_empty);
            _thread.BringToFront();
        }

        private void LayoutComposer()
        {
            int w = _composer.ClientSize.Width;
            _replyBox.SetBounds(0, 14, w, 86);
            int y = 14 + 86 + 10;
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
            _kCho.Text = "Chưa trả lời: " + _all.Count(d => d.TrangThaiHienThi == "Chưa trả lời").ToString("N0");
            _kDiem.Text = "Điểm trung bình: " + (coSao.Count == 0 ? "—" : coSao.Average(d => d.SoSao.Value).ToString("0.0") + " / 5");
            _kAn.Text = "Đã ẩn: " + _all.Count(d => !d.HienThi).ToString("N0");
        }

        private void ApplyFilter(string chonMa = null)
        {
            if (chonMa == null) chonMa = SelectedDto()?.MaDanhGia;
            string q = (_txtSearch.Text ?? "").Trim().ToLowerInvariant();
            string tt = _cboTrangThai.SelectedItem as string ?? "Tất cả";

            var list = _all.Where(d =>
            {
                if (tt != "Tất cả" && d.TrangThaiHienThi != tt) return false;
                if (q.Length == 0) return true;
                return Has(d.MaDanhGia, q) || Has(d.TenKhachHang, q) || Has(d.TenMon, q) || Has(d.NoiDung, q) || Has(d.SoDienThoai, q);
            }).ToList();

            _grid.SuspendLayout();
            _grid.Rows.Clear();
            int chon = -1;
            foreach (var d in list)
            {
                int i = _grid.Rows.Add(d.MaDanhGia, string.IsNullOrWhiteSpace(d.TenKhachHang) ? "Khách ẩn danh" : d.TenKhachHang,
                    Sao(d.SoSao ?? 0), MotDong(d.NoiDung), (object)d.NgayDanhGia, d.TrangThaiHienThi);
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
            _head.Visible = has; _thread.Visible = has; _composer.Visible = has;
            if (has) _empty.SendToBack(); else _empty.BringToFront();

            _btnAnHien.Enabled = has;
            _btnXoa.Enabled = has;
            _btnAnHien.Text = has && !d.HienThi ? "Hiện lại phản hồi" : "Ẩn phản hồi";
            _lblFooter.Text = has
                ? (d.TenKhachHang ?? "Khách ẩn danh") + " · " + (d.TongSoTraLoi > 0 ? d.TongSoTraLoi + " câu trả lời" : "chưa trả lời")
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
                if (b != null) b.LayoutFor(w);
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
            string hoi = "Xóa vĩnh viễn phản hồi " + d.MaDanhGia + " của " + (d.TenKhachHang ?? "khách hàng") + "?"
                         + (d.TongSoTraLoi > 0 ? "\n" + d.TongSoTraLoi + " câu trả lời đi kèm cũng bị xóa." : "")
                         + "\nThao tác này không hoàn tác được. Nếu chỉ muốn gỡ khỏi website, hãy chọn Ẩn phản hồi.";
            if (!Notify.Confirm(this, hoi, "Xóa phản hồi", true)) return;
            _btnXoa.Enabled = false;
            var r = await _service.DeleteAsync(d.MaDanhGia);
            if (IsDisposed) return;
            if (!r.Success) { _btnXoa.Enabled = true; Notify.Error(this, r.Message); return; }
            _current = null;
            await LoadDataAsync();
        }
    }
}
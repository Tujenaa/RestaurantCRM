using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.DTOs.KhaoSat;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.KhaoSat
{
    /// <summary>
    /// Tạo / sửa khảo sát: tiêu đề + danh sách câu hỏi (thêm nhiều câu, sửa, xóa) + xem trước.
    /// Khảo sát mới được tạo ở trạng thái Nháp; sau khi tạo, hộp thoại chuyển sang chế độ sửa để thêm câu hỏi ngay.
    /// </summary>
    public class KhaoSatDialog : CrmDialog
    {
        private const int DlgWidth = 860;
        private readonly KhaoSatService _service = new KhaoSatService();
        private KhaoSatChiTietDto _ks;
        private string _tieuDeDaLuu = "";

        private readonly TextBox _txtTieuDe = new TextBox { MaxLength = 200 };
        private readonly Panel _note = new Panel();
        private readonly Label _lblCauHoi = new Label();
        private readonly DataGridView _grid = new DataGridView();
        private readonly Button _btnThemCH;
        private readonly Button _btnSuaCH;
        private readonly Button _btnXoaCH;
        private readonly Button _btnXemTruoc;
        private readonly Label _lblError = new Label();
        private string _noteText = "";
        private bool _noteWarn;

        /// <summary>Mã khảo sát (đã có hoặc vừa tạo).</summary>
        public string MaKhaoSat { get; private set; }
        /// <summary>true nếu đã tạo / sửa gì đó (danh sách cần tải lại).</summary>
        public bool DataChanged { get; private set; }

        public KhaoSatDialog(string maKhaoSat)
            : base(maKhaoSat == null ? "Tạo khảo sát" : "Sửa khảo sát",
                   maKhaoSat == null ? "Đặt tiêu đề rồi thêm các câu hỏi. Khảo sát mới ở trạng thái Nháp cho tới khi bạn mở." : "Mã khảo sát: " + maKhaoSat,
                   DlgWidth, 540, maKhaoSat == null ? "Tạo khảo sát" : "Lưu & đóng", "Đóng")
        {
            MaKhaoSat = maKhaoSat;
            const int L = Gutter, W = DlgWidth - Gutter * 2;

            Field(Body, "Tiêu đề khảo sát", _txtTieuDe, L, 6, W, true);
            UiStyler.SetPlaceholder(_txtTieuDe, "Ví dụ: Khảo sát mức độ hài lòng tháng 10");

            _note.SetBounds(L, 76, W, 36);
            _note.BackColor = Color.White;
            _note.Paint += PaintNote;
            Body.Controls.Add(_note);

            _lblCauHoi.SetBounds(L, 132, 300, 24);
            _lblCauHoi.Font = new Font(ThemeManager.FontFamily, 10.5F, FontStyle.Bold);
            _lblCauHoi.ForeColor = ThemeManager.Ink;
            _lblCauHoi.UseMnemonic = false;
            _lblCauHoi.Text = "Câu hỏi";
            Body.Controls.Add(_lblCauHoi);

            _btnXoaCH = MakeButton("Xóa", ButtonSkin.Kind.Danger, 72);
            _btnSuaCH = MakeButton("Sửa câu hỏi", ButtonSkin.Kind.Secondary, 104);
            _btnThemCH = MakeButton("+ Thêm câu hỏi", ButtonSkin.Kind.Soft, 128);
            foreach (var b in new[] { _btnXoaCH, _btnSuaCH, _btnThemCH })
            {
                b.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                Body.Controls.Add(b);
                b.SizeChanged += (s, e) => PlaceToolbar();
            }
            PlaceToolbar();

            BuildGrid();
            _grid.SetBounds(L, 172, W, 330);
            Body.Controls.Add(_grid);

            _lblError.SetBounds(L, 508, W, 26);
            _lblError.ForeColor = ThemeManager.Red;
            _lblError.Font = new Font(ThemeManager.FontFamily, 9F);
            _lblError.UseMnemonic = false;
            Body.Controls.Add(_lblError);

            _btnXemTruoc = AddLeftButton("Xem trước", ButtonSkin.Kind.Ghost);
            _btnXemTruoc.Click += async (s, e) => await XemTruocAsync();

            _txtTieuDe.TextChanged += (s, e) => _lblError.Text = "";
            _btnThemCH.Click += async (s, e) => await ThemCauHoiAsync();
            _btnSuaCH.Click += async (s, e) => await SuaCauHoiAsync();
            _btnXoaCH.Click += async (s, e) => await XoaCauHoiAsync();
            _grid.CellDoubleClick += async (s, e) => { if (e.RowIndex >= 0) await SuaCauHoiAsync(); };
            _grid.SelectionChanged += (s, e) => UpdateButtons();
            _grid.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Delete) { e.Handled = true; await XoaCauHoiAsync(); } };
            PrimaryButton.Click += async (s, e) => await LuuAsync();
            AcceptButton = PrimaryButton;

            Shown += async (s, e) =>
            {
                if (MaKhaoSat != null) await TaiLaiAsync();
                else { Render(); _txtTieuDe.Focus(); }
            };
        }

        private void PlaceToolbar()
        {
            int right = DlgWidth - Gutter;
            _btnXoaCH.Location = new Point(right - _btnXoaCH.Width, 126);
            _btnSuaCH.Location = new Point(_btnXoaCH.Left - 8 - _btnSuaCH.Width, 126);
            _btnThemCH.Location = new Point(_btnSuaCH.Left - 8 - _btnThemCH.Width, 126);
        }

        private void BuildGrid()
        {
            var g = _grid;
            g.AllowUserToAddRows = false; g.AllowUserToDeleteRows = false; g.AllowUserToResizeRows = false;
            g.ReadOnly = true; g.MultiSelect = false; g.RowHeadersVisible = false; g.AutoGenerateColumns = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.BackgroundColor = Color.White; g.BorderStyle = BorderStyle.None;
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "STT", HeaderText = "#", FillWeight = 6 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "NoiDung", HeaderText = "Câu hỏi", FillWeight = 46 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Loai", HeaderText = "Loại", FillWeight = 14 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TuyChon", HeaderText = "Tùy chọn", FillWeight = 34 });
            foreach (DataGridViewColumn c in g.Columns) c.SortMode = DataGridViewColumnSortMode.NotSortable;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            UiStyler.StyleGrid(g);
        }

        // ── Hiển thị ─────────────────────────────────────────────────────────

        private async Task TaiLaiAsync(string chonCauHoi = null)
        {
            try
            {
                if (chonCauHoi == null && _grid.SelectedRows.Count > 0) chonCauHoi = (_grid.SelectedRows[0].Tag as CauHoiDto)?.MaCauHoi;
                _ks = await _service.GetByIdAsync(MaKhaoSat);
                if (IsDisposed) return;
                if (_tieuDeDaLuu == "" || _txtTieuDe.Text.Trim() == _tieuDeDaLuu) _txtTieuDe.Text = _ks.TieuDe ?? "";
                _tieuDeDaLuu = (_ks.TieuDe ?? "").Trim();
                Render(chonCauHoi);
            }
            catch (ApiException ex)
            {
                if (!IsDisposed) _lblError.Text = ex.Message;
            }
        }

        private void Render(string chonCauHoi = null)
        {
            bool coKS = _ks != null;
            int n = coKS ? _ks.CauHoi.Count : 0;
            _lblCauHoi.Text = coKS ? "Câu hỏi (" + n + ")" : "Câu hỏi";

            if (!coKS)
                SetNote("Nhấn \"Tạo khảo sát\" để lưu tiêu đề, sau đó thêm câu hỏi ngay trong hộp thoại này.", false);
            else if (_ks.TrangThaiHienThi == "Đã đóng")
                SetNote("Khảo sát đã đóng: không thêm được câu hỏi mới. Mở lại khảo sát ở danh sách nếu cần thu thêm câu trả lời.", true);
            else if (_ks.TrangThaiHienThi == "Đang mở")
                SetNote("Khảo sát đang mở (" + _ks.SoNguoiTraLoi + " phiếu). Thay đổi câu hỏi sẽ hiển thị ngay với khách đang trả lời.", true);
            else
                SetNote("Bản nháp · khách hàng chưa thấy khảo sát này. Cần ít nhất 1 câu hỏi (câu trắc nghiệm có ≥ 2 tùy chọn) để mở.", false);

            _grid.Rows.Clear();
            int chon = -1, stt = 1;
            if (coKS)
            {
                foreach (var c in _ks.CauHoi)
                {
                    string tc = c.LaTracNghiem
                        ? (c.TuyChon.Count == 0 ? "(chưa có tùy chọn)" : string.Join("  ·  ", c.TuyChon.Select(t => t.NoiDungTuyChon)))
                        : "Khách tự nhập câu trả lời";
                    int i = _grid.Rows.Add(stt++, c.NoiDungCauHoi + (c.DaCoTraLoi ? "   (đã có trả lời)" : ""), KhaoSatConst.TenLoai(c.LoaiCauHoi), tc);
                    _grid.Rows[i].Tag = c;
                    if (c.LaTracNghiem && c.TuyChon.Count < 2) _grid.Rows[i].Cells["TuyChon"].Style.ForeColor = ThemeManager.Red;
                    if (c.MaCauHoi == chonCauHoi) chon = i;
                }
            }
            _grid.ClearSelection();
            if (chon >= 0) { _grid.Rows[chon].Selected = true; _grid.CurrentCell = _grid.Rows[chon].Cells[0]; }
            UpdateButtons();
        }

        private void SetNote(string text, bool warn)
        {
            _noteText = text; _noteWarn = warn;
            _note.Invalidate();
        }

        private void PaintNote(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, _note.Width - 1, _note.Height - 1);
            using (var path = SkinUtil.Round(r, 8))
            using (var br = new SolidBrush(_noteWarn ? ThemeManager.AmberSoft : ThemeManager.PrimarySoft))
                g.FillPath(br, path);
            int x = 12;
            if (_ks != null)
                x += CrmPaint.StatusPill(g, _ks.TrangThaiHienThi, 8, 7, 8F) + 6;
            using (var f = new Font(ThemeManager.FontFamily, 9F))
                TextRenderer.DrawText(g, _noteText, f, new Rectangle(x, 0, _note.Width - x - 10, _note.Height), ThemeManager.Ink2,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
        }

        private CauHoiDto SelectedCauHoi()
        {
            return _grid.SelectedRows.Count == 0 ? null : _grid.SelectedRows[0].Tag as CauHoiDto;
        }

        private void UpdateButtons()
        {
            bool coKS = _ks != null;
            bool dong = coKS && _ks.TrangThaiHienThi == "Đã đóng";
            var c = SelectedCauHoi();
            _btnThemCH.Enabled = coKS && !dong;
            _btnSuaCH.Enabled = c != null;
            _btnXoaCH.Enabled = c != null;
            _btnXemTruoc.Enabled = coKS;
            _grid.Enabled = coKS;
        }

        // ── Lưu khảo sát ─────────────────────────────────────────────────────

        private bool KiemTraTieuDe(out string tieuDe)
        {
            tieuDe = _txtTieuDe.Text.Trim();
            if (tieuDe.Length == 0) { _lblError.Text = "Vui lòng nhập tiêu đề khảo sát."; _txtTieuDe.Focus(); return false; }
            if (tieuDe.Length < 5) { _lblError.Text = "Tiêu đề quá ngắn (tối thiểu 5 ký tự)."; _txtTieuDe.Focus(); return false; }
            return true;
        }

        private async Task LuuAsync()
        {
            string tieuDe;
            if (!KiemTraTieuDe(out tieuDe)) return;

            if (_ks == null)
            {
                SetBusy(true, "Đang tạo...");
                var r = await _service.CreateAsync(tieuDe, UserSession.CurrentUserId);
                if (IsDisposed) return;
                SetBusy(false);
                if (!r.Success) { _lblError.Text = r.Message; return; }
                MaKhaoSat = r.Field("maKhaoSat");
                DataChanged = true;
                if (string.IsNullOrWhiteSpace(MaKhaoSat)) { DialogResult = DialogResult.OK; Close(); return; }

                // Chuyển sang chế độ sửa để thêm câu hỏi ngay
                Text = TitleLabel.Text = "Sửa khảo sát";
                if (SubtitleLabel != null) SubtitleLabel.Text = "Mã khảo sát: " + MaKhaoSat + " · đã lưu tiêu đề, giờ hãy thêm câu hỏi.";
                SetPrimaryText("Lưu & đóng");
                _tieuDeDaLuu = tieuDe;
                await TaiLaiAsync();
                await ThemCauHoiAsync();
                return;
            }

            if (tieuDe != _tieuDeDaLuu)
            {
                SetBusy(true);
                var r = await _service.UpdateAsync(MaKhaoSat, tieuDe);
                if (IsDisposed) return;
                SetBusy(false);
                if (!r.Success) { _lblError.Text = r.Message; return; }
                DataChanged = true;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        // ── Câu hỏi ──────────────────────────────────────────────────────────

        private async Task ThemCauHoiAsync()
        {
            if (_ks == null) return;
            using (var dlg = new CauHoiKhaoSatDialog(MaKhaoSat, null))
            {
                Notify.ShowDialog(dlg, this);
                if (dlg.SoCauDaLuu > 0)
                {
                    DataChanged = true;
                    await TaiLaiAsync(dlg.MaCauHoiCuoi);
                }
            }
        }

        private async Task SuaCauHoiAsync()
        {
            var c = SelectedCauHoi();
            if (c == null) return;
            using (var dlg = new CauHoiKhaoSatDialog(MaKhaoSat, c))
            {
                Notify.ShowDialog(dlg, this);
                if (dlg.SoCauDaLuu > 0)
                {
                    DataChanged = true;
                    await TaiLaiAsync(c.MaCauHoi);
                }
            }
        }

        private async Task XoaCauHoiAsync()
        {
            var c = SelectedCauHoi();
            if (c == null) return;
            if (c.DaCoTraLoi)
            {
                Notify.Warn(this, "Câu hỏi này đã có khách trả lời nên không thể xóa (cần giữ dữ liệu thống kê).", "Không thể xóa");
                return;
            }
            if (!Notify.Confirm(this, "Xóa câu hỏi \"" + c.NoiDungCauHoi + "\"" + (c.TuyChon.Count > 0 ? " cùng " + c.TuyChon.Count + " tùy chọn" : "") + "?", "Xóa câu hỏi", true)) return;
            var r = await _service.DeleteCauHoiAsync(MaKhaoSat, c.MaCauHoi);
            if (IsDisposed) return;
            if (!r.Success) { Notify.Error(this, r.Message); return; }
            DataChanged = true;
            await TaiLaiAsync();
        }

        private async Task XemTruocAsync()
        {
            if (_ks == null) return;
            await TaiLaiAsync();
            if (IsDisposed || _ks == null) return;
            var ban = _ks;
            // Xem trước theo tiêu đề đang gõ (kể cả chưa lưu)
            if (_txtTieuDe.Text.Trim().Length > 0) ban.TieuDe = _txtTieuDe.Text.Trim();
            using (var dlg = new KhaoSatXemTruocDialog(ban)) Notify.ShowDialog(dlg, this);
        }
    }
}
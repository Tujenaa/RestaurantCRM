using System;
using System.Collections.Generic;
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
    /// Thêm / sửa câu hỏi khảo sát và các tùy chọn trả lời của nó.
    ///  - Trắc nghiệm: cần ≥ 2 tùy chọn, không trùng nhau; thêm / sửa / xóa nhiều tùy chọn ngay trong hộp thoại.
    ///  - Tự luận: không cần tùy chọn.
    ///  - Chế độ thêm có nút "Lưu & thêm câu khác" để nhập liên tiếp nhiều câu hỏi.
    ///  - Khi sửa: tùy chọn đổi chữ được cập nhật giữ nguyên mã (không mất lượt chọn cũ); câu đã có người trả lời không đổi loại được.
    /// </summary>
    public class CauHoiKhaoSatDialog : CrmDialog
    {
        private const int DlgWidth = 640;

        private class OptItem
        {
            public string Ma;      // null = tùy chọn mới chưa lưu
            public string Text;
            public string Goc;     // nội dung lúc tải (để biết có đổi chữ không)
            public override string ToString() { return Text; }
        }

        private readonly KhaoSatService _service = new KhaoSatService();
        private readonly string _maKhaoSat;
        private readonly CauHoiDto _edit;

        private readonly TextBox _txtNoiDung = new TextBox { Multiline = true, MaxLength = 500, ScrollBars = ScrollBars.None };
        private readonly ComboBox _cboLoai = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Panel _pnlTuyChon = new Panel();
        private readonly Label _lblTuLuan = new Label();
        private readonly TextBox _txtTuyChonMoi = new TextBox { MaxLength = 255 };
        private readonly Button _btnThemTC;
        private readonly ListBox _lst = new ListBox();
        private readonly Button _btnSuaTC;
        private readonly Button _btnXoaTC;
        private readonly Label _lblSoTC = new Label();
        private readonly Label _lblError = new Label();
        private readonly Button _btnLuuTiep;

        /// <summary>Số câu hỏi đã lưu thành công trong lần mở hộp thoại này.</summary>
        public int SoCauDaLuu { get; private set; }
        /// <summary>Mã câu hỏi lưu gần nhất (để danh sách chọn lại).</summary>
        public string MaCauHoiCuoi { get; private set; }

        public CauHoiKhaoSatDialog(string maKhaoSat, CauHoiDto edit)
            : base(edit == null ? "Thêm câu hỏi" : "Sửa câu hỏi",
                   edit == null ? "Nhập nội dung, chọn loại câu hỏi và các tùy chọn trả lời." : "Mã câu hỏi: " + edit.MaCauHoi + (edit.DaCoTraLoi ? " · đã có khách trả lời" : ""),
                   DlgWidth, 466, edit == null ? "Lưu câu hỏi" : "Lưu thay đổi")
        {
            _maKhaoSat = maKhaoSat;
            _edit = edit;
            const int L = Gutter, W = DlgWidth - Gutter * 2;

            Field(Body, "Nội dung câu hỏi", _txtNoiDung, L, 4, W, true);
            _txtNoiDung.Height = 58;
            UiStyler.SetPlaceholder(_txtNoiDung, "Ví dụ: Anh/chị đánh giá thế nào về chất lượng món ăn?");

            Field(Body, "Loại câu hỏi", _cboLoai, L, 98, 300, true);
            _cboLoai.Items.AddRange(new object[] { "Trắc nghiệm — khách chọn một đáp án", "Tự luận — khách tự nhập câu trả lời" });
            if (edit != null && edit.DaCoTraLoi)
            {
                _cboLoai.Enabled = false;
                Hint(Body, "Đã có người trả lời nên không đổi loại được.", L + 316, 132, W - 316);
            }

            // Khối tùy chọn (chỉ hiện với trắc nghiệm)
            _pnlTuyChon.SetBounds(L, 166, W, 262);
            _pnlTuyChon.BackColor = Color.White;
            Body.Controls.Add(_pnlTuyChon);
            FieldLabel(_pnlTuyChon, "Tùy chọn trả lời", 0, 0, true);
            _lblSoTC.SetBounds(W - 220, 0, 220, 20);
            _lblSoTC.TextAlign = ContentAlignment.MiddleRight;
            _lblSoTC.Font = new Font(ThemeManager.FontFamily, 8.5F);
            _lblSoTC.ForeColor = ThemeManager.Muted;
            _lblSoTC.UseMnemonic = false;
            _pnlTuyChon.Controls.Add(_lblSoTC);

            _btnThemTC = MakeButton("Thêm", ButtonSkin.Kind.Soft, 92);
            _btnSuaTC = MakeButton("Sửa", ButtonSkin.Kind.Secondary, 92);
            _btnXoaTC = MakeButton("Xóa", ButtonSkin.Kind.Danger, 92);
            int colX = W - 92;
            _txtTuyChonMoi.SetBounds(0, 28, colX - 10, 30);
            _pnlTuyChon.Controls.Add(_txtTuyChonMoi);
            UiStyler.SetPlaceholder(_txtTuyChonMoi, "Nhập tùy chọn rồi nhấn Enter (ví dụ: Rất hài lòng)");
            _btnThemTC.Location = new Point(colX, 24);
            _pnlTuyChon.Controls.Add(_btnThemTC);

            // Khung viền mảnh bo góc (UiStyler đổi Panel FixedSingle thành viền mảnh)
            var lstFrame = new Panel { BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, Padding = new Padding(2) };
            lstFrame.SetBounds(0, 72, colX - 10, 188);
            _pnlTuyChon.Controls.Add(lstFrame);
            _lst.Dock = DockStyle.Fill;
            _lst.BorderStyle = BorderStyle.None;
            _lst.DrawMode = DrawMode.OwnerDrawFixed;
            _lst.ItemHeight = 34;
            _lst.IntegralHeight = false;
            _lst.Font = new Font(ThemeManager.FontFamily, 9.5F);
            _lst.DrawItem += DrawOption;
            lstFrame.Controls.Add(_lst);
            _btnSuaTC.Location = new Point(colX, 72);
            _btnXoaTC.Location = new Point(colX, 72 + 46);
            _pnlTuyChon.Controls.Add(_btnSuaTC);
            _pnlTuyChon.Controls.Add(_btnXoaTC);

            _lblTuLuan.SetBounds(L, 166, W, 56);
            _lblTuLuan.Text = "Câu hỏi tự luận không cần tùy chọn.\nKhách hàng sẽ nhập câu trả lời vào một ô văn bản; bạn xem các câu trả lời trong phần Thống kê.";
            _lblTuLuan.ForeColor = ThemeManager.Ink2;
            _lblTuLuan.BackColor = ThemeManager.Surface2;
            _lblTuLuan.Padding = new Padding(12, 8, 12, 8);
            _lblTuLuan.Font = new Font(ThemeManager.FontFamily, 9F);
            _lblTuLuan.UseMnemonic = false;
            Body.Controls.Add(_lblTuLuan);

            _lblError.SetBounds(L, 432, W, 34);
            _lblError.ForeColor = ThemeManager.Red;
            _lblError.Font = new Font(ThemeManager.FontFamily, 9F);
            _lblError.UseMnemonic = false;
            Body.Controls.Add(_lblError);

            if (edit == null)
            {
                _btnLuuTiep = AddLeftButton("Lưu & thêm câu khác", ButtonSkin.Kind.Secondary);
                _btnLuuTiep.Click += async (s, e) => await LuuAsync(true);
            }

            // Dữ liệu ban đầu
            if (edit != null)
            {
                _txtNoiDung.Text = edit.NoiDungCauHoi ?? "";
                _cboLoai.SelectedIndex = edit.LaTracNghiem ? 0 : 1;
                foreach (var t in edit.TuyChon)
                    _lst.Items.Add(new OptItem { Ma = t.MaTuyChon, Text = t.NoiDungTuyChon ?? "", Goc = t.NoiDungTuyChon ?? "" });
            }
            else _cboLoai.SelectedIndex = 0;

            _cboLoai.SelectedIndexChanged += (s, e) => { _lblError.Text = ""; UpdateUi(); };
            _txtNoiDung.TextChanged += (s, e) => _lblError.Text = "";
            _txtTuyChonMoi.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ThemTuyChon(); } };
            _txtTuyChonMoi.Enter += (s, e) => AcceptButton = null;
            _txtTuyChonMoi.Leave += (s, e) => AcceptButton = PrimaryButton;
            _btnThemTC.Click += (s, e) => ThemTuyChon();
            _btnSuaTC.Click += (s, e) => SuaTuyChon();
            _btnXoaTC.Click += (s, e) => XoaTuyChon();
            _lst.DoubleClick += (s, e) => SuaTuyChon();
            _lst.SelectedIndexChanged += (s, e) => UpdateUi();
            _lst.KeyDown += (s, e) => { if (e.KeyCode == Keys.Delete) { e.Handled = true; XoaTuyChon(); } };

            PrimaryButton.Click += async (s, e) => await LuuAsync(false);
            AcceptButton = PrimaryButton;
            Shown += (s, e) => _txtNoiDung.Focus();
            UpdateUi();
        }

        private bool LaTracNghiem { get { return _cboLoai.SelectedIndex != 1; } }

        private List<OptItem> Items { get { return _lst.Items.Cast<OptItem>().ToList(); } }

        private void UpdateUi()
        {
            _pnlTuyChon.Visible = LaTracNghiem;
            _lblTuLuan.Visible = !LaTracNghiem;
            bool sel = _lst.SelectedItem != null;
            _btnSuaTC.Enabled = sel;
            _btnXoaTC.Enabled = sel;
            int n = _lst.Items.Count;
            _lblSoTC.Text = n + " tùy chọn" + (n < 2 ? " · cần ít nhất 2" : "");
            _lblSoTC.ForeColor = n < 2 ? ThemeManager.Amber : ThemeManager.Muted;
        }

        private void DrawOption(object sender, DrawItemEventArgs e)
        {
            var g = e.Graphics;
            bool sel = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            using (var br = new SolidBrush(sel ? ThemeManager.PrimarySoft : Color.White)) g.FillRectangle(br, e.Bounds);
            if (e.Index < 0 || e.Index >= _lst.Items.Count) return;
            var it = (OptItem)_lst.Items[e.Index];

            // Ô tròn chữ cái A, B, C... giống giao diện khách
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var circle = new Rectangle(e.Bounds.X + 10, e.Bounds.Y + (e.Bounds.Height - 20) / 2, 20, 20);
            using (var pen = new Pen(sel ? ThemeManager.Primary : ThemeManager.Line, 1.5f)) g.DrawEllipse(pen, circle);
            string chu = e.Index < 26 ? ((char)('A' + e.Index)).ToString() : (e.Index + 1).ToString();
            using (var f = new Font(ThemeManager.FontFamily, 7.5F, FontStyle.Bold))
                TextRenderer.DrawText(g, chu, f, circle, sel ? ThemeManager.Primary : ThemeManager.Muted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

            string tag = it.Ma == null ? "mới" : (it.Text != it.Goc ? "đã sửa" : null);
            int tagW = 0;
            if (tag != null)
            {
                using (var f = new Font(ThemeManager.FontFamily, 7.5F, FontStyle.Bold))
                {
                    tagW = TextRenderer.MeasureText(g, tag, f).Width + 10;
                    var tr = new Rectangle(e.Bounds.Right - tagW - 10, e.Bounds.Y + (e.Bounds.Height - 18) / 2, tagW, 18);
                    using (var path = SkinUtil.Round(tr, 9))
                    using (var br = new SolidBrush(it.Ma == null ? ThemeManager.GreenSoft : ThemeManager.AmberSoft)) g.FillPath(br, path);
                    TextRenderer.DrawText(g, tag, f, tr, it.Ma == null ? ThemeManager.Green : ThemeManager.Amber,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                }
            }
            var textRect = new Rectangle(circle.Right + 10, e.Bounds.Y, e.Bounds.Width - circle.Right - 20 - tagW - 10, e.Bounds.Height);
            TextRenderer.DrawText(g, it.Text, _lst.Font, textRect, ThemeManager.Ink,
                TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
            using (var pen = new Pen(ThemeManager.Line)) g.DrawLine(pen, e.Bounds.Left + 8, e.Bounds.Bottom - 1, e.Bounds.Right - 8, e.Bounds.Bottom - 1);
        }

        // ── Tùy chọn ─────────────────────────────────────────────────────────

        private bool TrungTuyChon(string text, OptItem tru)
        {
            return Items.Any(i => i != tru && string.Equals(i.Text.Trim(), text.Trim(), StringComparison.CurrentCultureIgnoreCase));
        }

        private void ThemTuyChon()
        {
            string t = _txtTuyChonMoi.Text.Trim();
            if (t.Length == 0) { _txtTuyChonMoi.Focus(); return; }
            if (TrungTuyChon(t, null)) { _lblError.Text = "Tùy chọn \"" + t + "\" đã có trong danh sách."; _txtTuyChonMoi.SelectAll(); _txtTuyChonMoi.Focus(); return; }
            _lst.Items.Add(new OptItem { Text = t });
            _lst.SelectedIndex = _lst.Items.Count - 1;
            _txtTuyChonMoi.Text = "";
            _txtTuyChonMoi.Focus();
            _lblError.Text = "";
            UpdateUi();
        }

        private void SuaTuyChon()
        {
            var it = _lst.SelectedItem as OptItem;
            if (it == null) return;
            var others = Items.Where(i => i != it).Select(i => i.Text).ToList();
            using (var dlg = new TuyChonCauHoiDialog(it.Text, others, _lst.SelectedIndex))
            {
                if (Notify.ShowDialog(dlg, this) != DialogResult.OK) return;
                it.Text = dlg.Value;
                int idx = _lst.SelectedIndex;
                _lst.Items[idx] = it;   // vẽ lại dòng
                _lst.SelectedIndex = idx;
            }
            UpdateUi();
        }

        private void XoaTuyChon()
        {
            var it = _lst.SelectedItem as OptItem;
            if (it == null) return;
            if (_edit != null && _edit.DaCoTraLoi && it.Ma != null &&
                !Notify.Confirm(this, "Câu hỏi đã có người trả lời. Nếu tùy chọn \"" + it.Goc + "\" đã có người chọn, máy chủ sẽ từ chối xóa khi lưu.\nVẫn bỏ tùy chọn này khỏi danh sách?", "Xóa tùy chọn"))
                return;
            int idx = _lst.SelectedIndex;
            _lst.Items.RemoveAt(idx);
            if (_lst.Items.Count > 0) _lst.SelectedIndex = Math.Min(idx, _lst.Items.Count - 1);
            UpdateUi();
        }

        // ── Lưu ──────────────────────────────────────────────────────────────

        private bool KiemTra()
        {
            string nd = _txtNoiDung.Text.Trim();
            if (nd.Length == 0) return Fail(_txtNoiDung, "Vui lòng nhập nội dung câu hỏi.");
            if (nd.Length < 5) return Fail(_txtNoiDung, "Nội dung câu hỏi quá ngắn.");
            if (LaTracNghiem)
            {
                if (_txtTuyChonMoi.Text.Trim().Length > 0)
                    return Fail(_txtTuyChonMoi, "Bạn đang nhập dở một tùy chọn — nhấn Thêm (hoặc xóa chữ trong ô) trước khi lưu.");
                if (_lst.Items.Count < 2) return Fail(_txtTuyChonMoi, "Câu hỏi trắc nghiệm cần ít nhất 2 tùy chọn.");
                if (Items.Any(i => i.Text.Trim().Length == 0)) return Fail(_lst, "Có tùy chọn đang để trống.");
            }
            return true;
        }

        private bool Fail(Control c, string msg)
        {
            _lblError.Text = msg;
            c.Focus();
            return false;
        }

        private CauHoiUpsertRequest TaoRequest()
        {
            return new CauHoiUpsertRequest
            {
                NoiDung = _txtNoiDung.Text.Trim(),
                LoaiCauHoi = LaTracNghiem ? KhaoSatConst.TracNghiem : KhaoSatConst.TuLuan,
                TuyChons = LaTracNghiem ? Items.Select(i => i.Text.Trim()).ToList() : new List<string>()
            };
        }

        private async Task LuuAsync(bool themTiep)
        {
            if (!KiemTra()) return;
            var req = TaoRequest();
            SetBusy(true);
            ApiResult r;

            if (_edit == null)
            {
                r = await _service.AddCauHoiAsync(_maKhaoSat, req);
                if (IsDisposed) return;
                SetBusy(false);
                if (!r.Success) { _lblError.Text = r.Message; return; }
                SoCauDaLuu++;
                MaCauHoiCuoi = r.Field("maCauHoi") ?? MaCauHoiCuoi;
                if (themTiep)
                {
                    if (SubtitleLabel != null) SubtitleLabel.Text = "Đã thêm " + SoCauDaLuu + " câu hỏi. Nhập câu tiếp theo hoặc nhấn Đóng.";
                    _txtNoiDung.Text = "";
                    _lst.Items.Clear();
                    _lblError.Text = "";
                    UpdateUi();
                    _txtNoiDung.Focus();
                    return;
                }
            }
            else
            {
                // 1. Tùy chọn đã có mà đổi chữ: cập nhật theo mã để giữ lượt chọn cũ
                if (LaTracNghiem)
                {
                    foreach (var it in Items.Where(i => i.Ma != null && i.Text.Trim() != (i.Goc ?? "").Trim()))
                    {
                        r = await _service.UpdateTuyChonAsync(_maKhaoSat, _edit.MaCauHoi, it.Ma, it.Text.Trim());
                        if (IsDisposed) return;
                        if (!r.Success) { SetBusy(false); _lblError.Text = "Tùy chọn \"" + it.Goc + "\": " + r.Message; return; }
                        it.Goc = it.Text.Trim();
                        SoCauDaLuu = Math.Max(SoCauDaLuu, 1);
                    }
                }
                // 2. Nội dung, loại và danh sách tùy chọn cuối cùng (API tự thêm tùy chọn mới, xóa tùy chọn bị bỏ)
                r = await _service.UpdateCauHoiAsync(_maKhaoSat, _edit.MaCauHoi, req);
                if (IsDisposed) return;
                SetBusy(false);
                if (!r.Success) { _lblError.Text = r.Message; return; }
                SoCauDaLuu++;
                MaCauHoiCuoi = _edit.MaCauHoi;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
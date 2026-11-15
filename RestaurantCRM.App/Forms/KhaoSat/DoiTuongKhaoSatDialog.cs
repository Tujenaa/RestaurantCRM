using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.DTOs.KhachHang;
using RestaurantCRM.AdminApp.DTOs.KhaoSat;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.KhaoSat
{
    /// <summary>
    /// Chọn đối tượng khảo sát:
    ///  - Tất cả khách hàng (khảo sát công khai, không lưu dòng DOI_TUONG_KHAO_SAT nào);
    ///  - Theo tiêu chí (giới tính, độ tuổi, sở thích khách tự khai) — áp dụng tại thời điểm lưu thành danh sách khách cụ thể.
    ///    Các tiêu chí kết hợp AND; tiêu chí sở thích: trong cùng câu khớp 1 trong các đáp án được tick (OR),
    ///    giữa các câu có tick thì phải khớp tất cả (AND);
    ///  - Chọn bất kỳ khách hàng nào.
    /// Khách đã trả lời khảo sát luôn nằm trong danh sách và không bỏ chọn được.
    /// </summary>
    public class DoiTuongKhaoSatDialog : CrmDialog
    {
        private const int DlgWidth = 900;
        private const string GtTatCa = "Tất cả", GtNam = "Nam", GtNu = "Nữ";

        private readonly KhaoSatService _service = new KhaoSatService();
        private readonly KhachHangService _khService = new KhachHangService();
        private readonly string _maKhaoSat;
        private readonly bool _chiXem;

        private List<KhachHangDto> _khach = new List<KhachHangDto>();
        private readonly Dictionary<string, bool> _chon = new Dictionary<string, bool>();
        private readonly HashSet<string> _daTraLoi = new HashSet<string>();
        private bool _tieuChiDaApDung;
        private bool _rendering;

        private readonly RadioButton _rdTatCa = new RadioButton { Text = "Tất cả khách hàng", AutoSize = true };
        private readonly RadioButton _rdTieuChi = new RadioButton { Text = "Theo tiêu chí (giới tính, tuổi, sở thích)", AutoSize = true };
        private readonly RadioButton _rdCuThe = new RadioButton { Text = "Chọn khách hàng cụ thể", AutoSize = true };

        private readonly Panel _pnlTieuChi = new Panel();
        private readonly ComboBox _cboGioiTinh = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly CheckBox _chkTuoi = new CheckBox { Text = "Giới hạn độ tuổi:", AutoSize = true };
        private readonly NumericUpDown _nudTuTuoi = new NumericUpDown { Minimum = 0, Maximum = 120, Value = 18 };
        private readonly NumericUpDown _nudDenTuoi = new NumericUpDown { Minimum = 0, Maximum = 120, Value = 35 };
        private readonly Button _btnApDung;

        // Tiêu chí "Theo sở thích" (hồ sơ sở thích)
        private readonly CheckBox _chkSoThich = new CheckBox { Text = "Theo sở thích:", AutoSize = true };
        private readonly Button _btnChonSoThich;
        private readonly Label _lblSoThich = new Label { AutoSize = false, AutoEllipsis = true, UseMnemonic = false };
        private readonly List<DapAnSoThich> _dapAnSoThich = new List<DapAnSoThich>();
        private readonly HashSet<string> _tcSoThichDaChon = new HashSet<string>();
        private readonly Dictionary<string, HashSet<string>> _soThichKhach = new Dictionary<string, HashSet<string>>();

        private class DapAnSoThich
        {
            public string MaTuyChon;
            public string MaCauHoi;
            public string CauHoi;  // nội dung câu hỏi
            public string DapAn;   // nội dung đáp án
            public string Nhan;    // "Câu hỏi: Đáp án"
            public override string ToString() { return Nhan; }
        }

        private readonly TextBox _txtSearch = new TextBox();
        private readonly Button _btnChonTatCa;
        private readonly Button _btnBoChon;
        private readonly Label _lblCount = new Label();
        private readonly DataGridView _grid = new DataGridView();
        private readonly Label _lblError = new Label();

        /// <summary>true nếu đã lưu thay đổi.</summary>
        public bool DataChanged { get; private set; }
        /// <summary>Số khách được mời sau khi lưu (0 = tất cả khách hàng).</summary>
        public int SoDoiTuong { get; private set; }

        public DoiTuongKhaoSatDialog(string maKhaoSat, string tieuDe, bool daDong)
            : base("Đối tượng khảo sát",
                   (tieuDe ?? maKhaoSat) + (daDong ? " · khảo sát đã đóng (chỉ xem)" : ""),
                   DlgWidth, 600, daDong ? null : "Lưu đối tượng", daDong ? "Đóng" : "Hủy")
        {
            _maKhaoSat = maKhaoSat;
            _chiXem = daDong;
            const int L = Gutter, W = DlgWidth - Gutter * 2;

            // ── Chế độ ──
            _rdTatCa.Location = new Point(L, 8);
            _rdTieuChi.Location = new Point(L + 190, 8);
            _rdCuThe.Location = new Point(L + 470, 8);
            foreach (var rd in new[] { _rdTatCa, _rdTieuChi, _rdCuThe })
            {
                rd.Font = new Font(ThemeManager.FontFamily, 9.5F, FontStyle.Bold);
                rd.ForeColor = ThemeManager.Ink;
                rd.CheckedChanged += (s, e) => { if (((RadioButton)s).Checked) CapNhatCheDo(); };
                Body.Controls.Add(rd);
            }

            // ── Tiêu chí ──
            _pnlTieuChi.SetBounds(L, 42, W, 76);
            _pnlTieuChi.BackColor = ThemeManager.Surface2;
            Body.Controls.Add(_pnlTieuChi);

            var lblGt = new Label { Text = "Giới tính:", AutoSize = true, Location = new Point(12, 14), ForeColor = ThemeManager.Ink2, BackColor = Color.Transparent };
            _cboGioiTinh.Items.AddRange(new object[] { GtTatCa, GtNam, GtNu });
            _cboGioiTinh.SelectedIndex = 0;
            _cboGioiTinh.SetBounds(82, 10, 100, 24);
            _chkTuoi.Location = new Point(206, 12);
            _chkTuoi.BackColor = Color.Transparent;
            _nudTuTuoi.SetBounds(332, 10, 60, 24);
            var lblDen = new Label { Text = "đến", AutoSize = true, Location = new Point(400, 14), ForeColor = ThemeManager.Ink2, BackColor = Color.Transparent };
            _nudDenTuoi.SetBounds(434, 10, 60, 24);
            var lblTuoi = new Label { Text = "tuổi", AutoSize = true, Location = new Point(500, 14), ForeColor = ThemeManager.Ink2, BackColor = Color.Transparent };
            _btnApDung = MakeButton("Áp dụng tiêu chí", ButtonSkin.Kind.Soft, 120);
            _btnApDung.Margin = Padding.Empty;
            _chkSoThich.Location = new Point(12, 48);
            _chkSoThich.BackColor = Color.Transparent;
            _btnChonSoThich = MakeButton("Chọn đáp án...", ButtonSkin.Kind.Secondary, 110);
            _btnChonSoThich.Margin = Padding.Empty;
            _btnChonSoThich.Location = new Point(236, 42);
            _lblSoThich.SetBounds(360, 48, 340, 20);
            _lblSoThich.ForeColor = ThemeManager.Ink2;
            _lblSoThich.BackColor = Color.Transparent;
            _pnlTieuChi.Controls.AddRange(new Control[] { lblGt, _cboGioiTinh, _chkTuoi, _nudTuTuoi, lblDen, _nudDenTuoi, lblTuoi, _btnApDung,
                _chkSoThich, _btnChonSoThich, _lblSoThich });
            _pnlTieuChi.Resize += (s, e) => _btnApDung.Location = new Point(_pnlTieuChi.Width - _btnApDung.Width - 8, (_pnlTieuChi.Height - _btnApDung.Height) / 2);
            _btnApDung.Location = new Point(_pnlTieuChi.Width - _btnApDung.Width - 8, (_pnlTieuChi.Height - _btnApDung.Height) / 2);
            CapNhatNhanSoThich();

            Hint(Body, "Tiêu chí được áp dụng lúc lưu: khách thỏa điều kiện sẽ được thêm vào danh sách. Khách đăng ký sau sẽ không tự được thêm — mở lại và áp dụng lại nếu cần.", L, 124, W);

            // ── Tìm + chọn nhanh ──
            _txtSearch.SetBounds(L, 152, 300, 26);
            UiStyler.SetPlaceholder(_txtSearch, "Tìm theo mã, họ tên, số điện thoại...");
            Body.Controls.Add(_txtSearch);

            _lblCount.SetBounds(L + 312, 156, 260, 20);
            _lblCount.ForeColor = ThemeManager.Muted;
            _lblCount.UseMnemonic = false;
            Body.Controls.Add(_lblCount);

            _btnBoChon = MakeButton("Bỏ chọn tất cả", ButtonSkin.Kind.Secondary, 110);
            _btnChonTatCa = MakeButton("Chọn tất cả", ButtonSkin.Kind.Secondary, 96);
            int right = DlgWidth - Gutter;
            _btnBoChon.Location = new Point(right - _btnBoChon.Width, 150);
            _btnChonTatCa.Location = new Point(_btnBoChon.Left - 8 - _btnChonTatCa.Width, 150);
            Body.Controls.Add(_btnBoChon);
            Body.Controls.Add(_btnChonTatCa);

            BuildGrid();
            _grid.SetBounds(L, 190, W, 368);
            Body.Controls.Add(_grid);

            _lblError.SetBounds(L, 564, W, 26);
            _lblError.ForeColor = ThemeManager.Red;
            _lblError.Font = new Font(ThemeManager.FontFamily, 9F);
            _lblError.UseMnemonic = false;
            Body.Controls.Add(_lblError);

            // ── Sự kiện ──
            _cboGioiTinh.SelectedIndexChanged += (s, e) => _tieuChiDaApDung = false;
            _chkTuoi.CheckedChanged += (s, e) => { _tieuChiDaApDung = false; _nudTuTuoi.Enabled = _nudDenTuoi.Enabled = _chkTuoi.Checked && _rdTieuChi.Checked && !_chiXem; };
            _nudTuTuoi.ValueChanged += (s, e) => _tieuChiDaApDung = false;
            _nudDenTuoi.ValueChanged += (s, e) => _tieuChiDaApDung = false;
            _btnApDung.Click += (s, e) => ApDungTieuChi();
            _chkSoThich.CheckedChanged += (s, e) => { _tieuChiDaApDung = false; CapNhatNhanSoThich(); };
            _btnChonSoThich.Click += (s, e) => ChonDapAnSoThich();
            _txtSearch.TextChanged += (s, e) => Render();
            _btnChonTatCa.Click += (s, e) => DatChonDangHienThi(true);
            _btnBoChon.Click += (s, e) => DatChonDangHienThi(false);
            _grid.CurrentCellDirtyStateChanged += (s, e) => { if (_grid.IsCurrentCellDirty) _grid.CommitEdit(DataGridViewDataErrorContexts.Commit); };
            _grid.CellValueChanged += Grid_CellValueChanged;
            if (PrimaryButton != null)
            {
                PrimaryButton.Click += async (s, e) => await LuuAsync();
                AcceptButton = PrimaryButton;
            }

            _rdTatCa.Checked = true;
            Shown += async (s, e) => await TaiDuLieuAsync();
        }

        private void BuildGrid()
        {
            var g = _grid;
            g.AllowUserToAddRows = false; g.AllowUserToDeleteRows = false; g.AllowUserToResizeRows = false;
            g.MultiSelect = false; g.RowHeadersVisible = false; g.AutoGenerateColumns = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Chon", HeaderText = "", FillWeight = 6 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ma", HeaderText = "Mã KH", FillWeight = 11, ReadOnly = true });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "HoTen", HeaderText = "Họ tên", FillWeight = 28, ReadOnly = true });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sdt", HeaderText = "Số điện thoại", FillWeight = 17, ReadOnly = true });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "GioiTinh", HeaderText = "Giới tính", FillWeight = 11, ReadOnly = true });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tuoi", HeaderText = "Tuổi", FillWeight = 8, ReadOnly = true });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "TrangThai", HeaderText = "Tình trạng", FillWeight = 19, ReadOnly = true });
            foreach (DataGridViewColumn c in g.Columns) c.SortMode = DataGridViewColumnSortMode.NotSortable;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            UiStyler.StyleGrid(g);
            g.ReadOnly = false;
        }

        // ── Dữ liệu ──────────────────────────────────────────────────────────

        private async Task TaiDuLieuAsync()
        {
            UseWaitCursor = true;
            _lblCount.Text = "Đang tải...";
            try
            {
                var tKhach = _khService.GetAllAsync();
                var tDoiTuong = _service.GetDoiTuongAsync(_maKhaoSat);
                var tDaTraLoi = _service.GetNguoiDaTraLoiAsync(_maKhaoSat);
                await Task.WhenAll(tKhach, tDoiTuong, tDaTraLoi);
                if (IsDisposed) return;
                await TaiSoThichAsync();
                if (IsDisposed) return;

                var doiTuong = tDoiTuong.Result;
                _khach = tKhach.Result.OrderBy(k => k.MaKhachHang).ToList();
                _chon.Clear();
                _daTraLoi.Clear();
                foreach (var k in _khach) _chon[k.MaKhachHang] = false;
                foreach (var d in doiTuong)
                {
                    if (string.IsNullOrEmpty(d.MaKhachHang)) continue;
                    _chon[d.MaKhachHang] = true;
                }
                // Người đã nộp phiếu lấy từ PHIEU_TRA_LOI (kể cả khi khảo sát đang công khai):
                // luôn được chọn sẵn và khóa ô, không thể bỏ khỏi danh sách đối tượng
                foreach (var ma in tDaTraLoi.Result)
                {
                    if (string.IsNullOrEmpty(ma) || !_chon.ContainsKey(ma)) continue;
                    _daTraLoi.Add(ma);
                    _chon[ma] = true;
                }

                _rendering = true;
                if (doiTuong.Count > 0) _rdCuThe.Checked = true; else _rdTatCa.Checked = true;
                _rendering = false;
                CapNhatCheDo();
            }
            catch (ApiException ex)
            {
                if (!IsDisposed) { BaoLoi(ex.Message); _lblCount.Text = ""; }
            }
            finally
            {
                if (!IsDisposed) UseWaitCursor = false;
            }
        }

        /// <summary>Tải danh mục đáp án của hồ sơ sở thích và đáp án từng khách đã khai báo.</summary>
        private async Task TaiSoThichAsync()
        {
            _dapAnSoThich.Clear();
            _soThichKhach.Clear();
            try
            {
                var ma = await _service.GetMaBoSoThichAsync();
                if (ma != null)
                {
                    var tBo = _service.GetByIdAsync(ma);
                    var tKhach = _service.GetSoThichKhachHangAsync();
                    await Task.WhenAll(tBo, tKhach);
                    foreach (var c in tBo.Result?.CauHoi ?? new List<CauHoiDto>())
                    {
                        if (!c.LaTracNghiem) continue;
                        foreach (var t in c.TuyChon)
                            _dapAnSoThich.Add(new DapAnSoThich
                            {
                                MaTuyChon = t.MaTuyChon,
                                MaCauHoi = c.MaCauHoi,
                                CauHoi = c.NoiDungCauHoi,
                                DapAn = t.NoiDungTuyChon,
                                Nhan = c.NoiDungCauHoi + ": " + t.NoiDungTuyChon
                            });
                    }
                    foreach (var k in tKhach.Result)
                        if (!string.IsNullOrEmpty(k.MaKhachHang))
                            _soThichKhach[k.MaKhachHang] = new HashSet<string>(k.MaTuyChons ?? new List<string>());
                }
            }
            catch (ApiException)
            {
                // Không tải được sở thích: chỉ tắt tiêu chí này, các tiêu chí khác vẫn dùng được
                _dapAnSoThich.Clear();
            }
            if (IsDisposed) return;
            bool co = _dapAnSoThich.Count > 0;
            if (!co) _chkSoThich.Checked = false;
            _chkSoThich.Text = co ? "Theo sở thích:" : "Theo sở thích (chưa có hồ sơ sở thích)";
            CapNhatNhanSoThich();
        }

        private void CapNhatNhanSoThich()
        {
            bool co = _dapAnSoThich.Count > 0;
            _chkSoThich.Enabled = co && !_chiXem;
            _btnChonSoThich.Enabled = co && !_chiXem && _chkSoThich.Checked;
            // Nhóm theo câu (giữ thứ tự trong bộ câu hỏi): "Khẩu vị: Thích cay, Đậm đà · Nguyên liệu: Hải sản"
            var nhan = _dapAnSoThich.Where(d => _tcSoThichDaChon.Contains(d.MaTuyChon))
                .GroupBy(d => d.MaCauHoi)
                .Select(g => g.First().CauHoi + ": " + string.Join(", ", g.Select(d => d.DapAn)))
                .ToList();
            _lblSoThich.Text = nhan.Count == 0 ? (_chkSoThich.Checked ? "Chưa chọn đáp án nào" : "") : string.Join(" · ", nhan);
            _lblSoThich.ForeColor = _chkSoThich.Checked && nhan.Count == 0 ? ThemeManager.Amber : ThemeManager.Ink2;
        }

        private void ChonDapAnSoThich()
        {
            using (var dlg = new ChonSoThichDialog(_dapAnSoThich, _tcSoThichDaChon))
            {
                if (Notify.ShowDialog(dlg, this) != DialogResult.OK) return;
                _tcSoThichDaChon.Clear();
                foreach (var m in dlg.DaChon) _tcSoThichDaChon.Add(m);
                _tieuChiDaApDung = false;
                CapNhatNhanSoThich();
            }
        }

        /// <summary>Hộp chọn các đáp án của hồ sơ sở thích dùng làm tiêu chí.</summary>
        private class ChonSoThichDialog : CrmDialog
        {
            private readonly CheckedListBox _lst = new CheckedListBox { CheckOnClick = true, IntegralHeight = false, BorderStyle = BorderStyle.FixedSingle };
            public List<string> DaChon { get; private set; } = new List<string>();

            public ChonSoThichDialog(List<DapAnSoThich> dapAns, HashSet<string> daChon)
                : base("Chọn đáp án sở thích", "Cùng câu: khớp 1 trong các đáp án · Khác câu: phải khớp tất cả", 560, 360, "Chọn")
            {
                _lst.SetBounds(Gutter, 4, 560 - Gutter * 2, 340);
                _lst.Font = new Font(ThemeManager.FontFamily, 9.5F);
                foreach (var d in dapAns) _lst.Items.Add(d, daChon.Contains(d.MaTuyChon));
                Body.Controls.Add(_lst);
                PrimaryButton.Click += (s, e) =>
                {
                    DaChon = _lst.CheckedItems.Cast<DapAnSoThich>().Select(d => d.MaTuyChon).ToList();
                    DialogResult = DialogResult.OK;
                    Close();
                };
            }
        }

        private void CapNhatCheDo()
        {
            if (_rendering) return;
            bool tieuChi = _rdTieuChi.Checked, tatCa = _rdTatCa.Checked;
            bool sua = !_chiXem;

            foreach (var rd in new[] { _rdTatCa, _rdTieuChi, _rdCuThe }) rd.Enabled = sua;
            _pnlTieuChi.Enabled = tieuChi && sua;
            _nudTuTuoi.Enabled = _nudDenTuoi.Enabled = tieuChi && sua && _chkTuoi.Checked;
            CapNhatNhanSoThich();
            _grid.Enabled = !tatCa;
            _txtSearch.Enabled = !tatCa;
            _btnChonTatCa.Enabled = _btnBoChon.Enabled = !tatCa && sua;
            _grid.Columns["Chon"].ReadOnly = !sua;
            _lblError.Text = "";
            Render();
        }

        private static int? TinhTuoi(KhachHangDto k)
        {
            var ns = k.NgaySinhDate;
            if (!ns.HasValue) return null;
            var homNay = DateTime.Today;
            int tuoi = homNay.Year - ns.Value.Year;
            if (ns.Value.Date > homNay.AddYears(-tuoi)) tuoi--;
            return tuoi;
        }

        private static string ChuanHoa(string s)
        {
            return (s ?? "").Trim().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }

        private bool KhopTieuChi(KhachHangDto k)
        {
            if (k.BiKhoa) return false;
            string gt = _cboGioiTinh.SelectedItem as string ?? GtTatCa;
            if (gt != GtTatCa && ChuanHoa(k.GioiTinh) != ChuanHoa(gt)) return false;
            if (_chkTuoi.Checked)
            {
                int tu = (int)Math.Min(_nudTuTuoi.Value, _nudDenTuoi.Value);
                int den = (int)Math.Max(_nudTuTuoi.Value, _nudDenTuoi.Value);
                var tuoi = TinhTuoi(k);
                if (!tuoi.HasValue || tuoi.Value < tu || tuoi.Value > den) return false;
            }
            // Theo sở thích (AND với các tiêu chí trên): gom đáp án đã tick theo câu;
            // cùng câu khớp 1 trong các đáp án (OR), mọi câu có tick đều phải khớp (AND)
            if (_chkSoThich.Checked)
            {
                HashSet<string> cuaKhach;
                if (!_soThichKhach.TryGetValue(k.MaKhachHang, out cuaKhach)) return false;
                var theoCau = _dapAnSoThich.Where(d => _tcSoThichDaChon.Contains(d.MaTuyChon)).GroupBy(d => d.MaCauHoi);
                foreach (var cau in theoCau)
                    if (!cau.Any(d => cuaKhach.Contains(d.MaTuyChon))) return false;
            }
            return true;
        }

        private void ApDungTieuChi()
        {
            if (_chkSoThich.Checked && _tcSoThichDaChon.Count == 0)
            {
                BaoLoi("Bạn đã bật \"Theo sở thích\" nhưng chưa chọn đáp án nào.");
                return;
            }
            int n = 0, giuThem = 0;
            foreach (var k in _khach)
            {
                bool khop = KhopTieuChi(k);
                bool daTraLoi = _daTraLoi.Contains(k.MaKhachHang);
                bool chon = khop || daTraLoi;
                _chon[k.MaKhachHang] = chon;
                if (chon) n++;
                if (!khop && daTraLoi) giuThem++;
            }
            _tieuChiDaApDung = true;
            _txtSearch.Text = "";
            Render();
            if (n == 0) BaoLoi("Không có khách hàng nào thỏa tiêu chí.");
            else if (giuThem > 0) BaoTin($"Đã giữ thêm {giuThem} khách đã trả lời khảo sát dù không khớp tiêu chí.");
            else _lblError.Text = "";
        }

        /// <summary>Hiện lỗi (chữ đỏ) trên nhãn thông báo.</summary>
        private void BaoLoi(string text)
        {
            _lblError.ForeColor = ThemeManager.Red;
            _lblError.Text = text;
        }

        /// <summary>Hiện thông tin (không phải lỗi) trên nhãn thông báo.</summary>
        private void BaoTin(string text)
        {
            _lblError.ForeColor = ThemeManager.Primary;
            _lblError.Text = text;
        }

        /// <summary>Khách hiển thị: theo ô tìm (hỗ trợ không dấu); ẩn khách bị khóa trừ khi đã được chọn.</summary>
        private List<KhachHangDto> DangHienThi()
        {
            string q = (_txtSearch.Text ?? "").Trim();
            return _khach.Where(k =>
                (!k.BiKhoa || _chon[k.MaKhachHang]) &&
                UiStyler.MatchesSearch(q, k.MaKhachHang, k.HoTen, k.SoDienThoai, k.Email, k.DiaChi)).ToList();
        }

        private void Render()
        {
            _rendering = true;
            _grid.SuspendLayout();
            _grid.Rows.Clear();
            bool tatCa = _rdTatCa.Checked;
            foreach (var k in DangHienThi())
            {
                var tuoi = TinhTuoi(k);
                bool traLoi = _daTraLoi.Contains(k.MaKhachHang);
                string tinhTrang = traLoi ? "Đã trả lời" : (k.BiKhoa ? "Tài khoản bị khóa" : "");
                int i = _grid.Rows.Add(!tatCa && _chon[k.MaKhachHang], k.MaKhachHang, k.HoTen, k.SoDienThoai,
                    k.GioiTinh, tuoi.HasValue ? tuoi.Value.ToString() : "—", tinhTrang);
                var row = _grid.Rows[i];
                row.Tag = k;
                if (traLoi)
                {
                    row.Cells["Chon"].ReadOnly = true;
                    row.Cells["TrangThai"].Style.ForeColor = ThemeManager.Primary;
                }
                else if (k.BiKhoa) row.Cells["TrangThai"].Style.ForeColor = ThemeManager.Red;
            }
            _grid.ResumeLayout();
            _grid.ClearSelection();
            _rendering = false;
            CapNhatDem();
        }

        private void CapNhatDem()
        {
            if (_rdTatCa.Checked)
            {
                _lblCount.Text = "Áp dụng cho tất cả " + _khach.Count(k => !k.BiKhoa).ToString("N0") + " khách hàng";
                return;
            }
            int chon = _chon.Count(p => p.Value);
            _lblCount.Text = "Đã chọn " + chon.ToString("N0") + " / " + _khach.Count.ToString("N0") + " khách";
        }

        private void Grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_rendering || e.RowIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "Chon") return;
            var k = _grid.Rows[e.RowIndex].Tag as KhachHangDto;
            if (k == null) return;
            bool v = Convert.ToBoolean(_grid.Rows[e.RowIndex].Cells["Chon"].Value ?? false);
            _chon[k.MaKhachHang] = v || _daTraLoi.Contains(k.MaKhachHang);
            _lblError.Text = "";
            CapNhatDem();
        }

        private void DatChonDangHienThi(bool chon)
        {
            foreach (var k in DangHienThi())
                _chon[k.MaKhachHang] = chon || _daTraLoi.Contains(k.MaKhachHang);
            Render();
        }

        // ── Lưu ──────────────────────────────────────────────────────────────

        private async Task LuuAsync()
        {
            _lblError.Text = "";
            List<string> ds;
            if (_rdTatCa.Checked)
            {
                ds = new List<string>();
            }
            else
            {
                if (_rdTieuChi.Checked && !_tieuChiDaApDung)
                {
                    ApDungTieuChi();
                    if (!_tieuChiDaApDung) return;   // tiêu chí chưa hợp lệ (vd. bật sở thích mà chưa chọn đáp án)
                }
                ds =_chon.Where(p => p.Value).Select(p => p.Key).ToList();
                if (ds.Count == 0)
                {
                    BaoLoi(_rdTieuChi.Checked
                        ? "Không có khách hàng nào thỏa tiêu chí. Hãy đổi tiêu chí hoặc chọn \"Tất cả khách hàng\"."
                        : "Hãy chọn ít nhất một khách hàng, hoặc chọn \"Tất cả khách hàng\".");
                    return;
                }
            }

            SetBusy(true);
            var r = await _service.SetDoiTuongAsync(_maKhaoSat, ds);
            if (IsDisposed) return;
            SetBusy(false);
            if (!r.Success) { BaoLoi(r.Message); return; }

            DataChanged = true;
            SoDoiTuong = ds.Count;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

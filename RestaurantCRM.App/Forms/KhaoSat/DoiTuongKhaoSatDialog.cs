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
    ///  - Theo tiêu chí (giới tính, độ tuổi) — áp dụng tại thời điểm lưu thành danh sách khách cụ thể;
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
        private readonly RadioButton _rdTieuChi = new RadioButton { Text = "Theo tiêu chí (giới tính, độ tuổi)", AutoSize = true };
        private readonly RadioButton _rdCuThe = new RadioButton { Text = "Chọn khách hàng cụ thể", AutoSize = true };

        private readonly Panel _pnlTieuChi = new Panel();
        private readonly ComboBox _cboGioiTinh = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly CheckBox _chkTuoi = new CheckBox { Text = "Giới hạn độ tuổi:", AutoSize = true };
        private readonly NumericUpDown _nudTuTuoi = new NumericUpDown { Minimum = 0, Maximum = 120, Value = 18 };
        private readonly NumericUpDown _nudDenTuoi = new NumericUpDown { Minimum = 0, Maximum = 120, Value = 35 };
        private readonly Button _btnApDung;

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
            _pnlTieuChi.SetBounds(L, 42, W, 44);
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
            _pnlTieuChi.Controls.AddRange(new Control[] { lblGt, _cboGioiTinh, _chkTuoi, _nudTuTuoi, lblDen, _nudDenTuoi, lblTuoi, _btnApDung });
            _pnlTieuChi.Resize += (s, e) => _btnApDung.Location = new Point(_pnlTieuChi.Width - _btnApDung.Width - 8, (_pnlTieuChi.Height - _btnApDung.Height) / 2);
            _btnApDung.Location = new Point(_pnlTieuChi.Width - _btnApDung.Width - 8, (_pnlTieuChi.Height - _btnApDung.Height) / 2);

            Hint(Body, "Tiêu chí được áp dụng lúc lưu: khách thỏa điều kiện sẽ được thêm vào danh sách. Khách đăng ký sau sẽ không tự được thêm — mở lại và áp dụng lại nếu cần.", L, 92, W);

            // ── Tìm + chọn nhanh ──
            _txtSearch.SetBounds(L, 120, 300, 26);
            UiStyler.SetPlaceholder(_txtSearch, "Tìm theo mã, họ tên, số điện thoại...");
            Body.Controls.Add(_txtSearch);

            _lblCount.SetBounds(L + 312, 124, 260, 20);
            _lblCount.ForeColor = ThemeManager.Muted;
            _lblCount.UseMnemonic = false;
            Body.Controls.Add(_lblCount);

            _btnBoChon = MakeButton("Bỏ chọn tất cả", ButtonSkin.Kind.Secondary, 110);
            _btnChonTatCa = MakeButton("Chọn tất cả", ButtonSkin.Kind.Secondary, 96);
            int right = DlgWidth - Gutter;
            _btnBoChon.Location = new Point(right - _btnBoChon.Width, 118);
            _btnChonTatCa.Location = new Point(_btnBoChon.Left - 8 - _btnChonTatCa.Width, 118);
            Body.Controls.Add(_btnBoChon);
            Body.Controls.Add(_btnChonTatCa);

            BuildGrid();
            _grid.SetBounds(L, 158, W, 400);
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
                await Task.WhenAll(tKhach, tDoiTuong);
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
                    if (d.DaHoanThanh) _daTraLoi.Add(d.MaKhachHang);
                }

                _rendering = true;
                if (doiTuong.Count > 0) _rdCuThe.Checked = true; else _rdTatCa.Checked = true;
                _rendering = false;
                CapNhatCheDo();
            }
            catch (ApiException ex)
            {
                if (!IsDisposed) { _lblError.Text = ex.Message; _lblCount.Text = ""; }
            }
            finally
            {
                if (!IsDisposed) UseWaitCursor = false;
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
            return true;
        }

        private void ApDungTieuChi()
        {
            int n = 0;
            foreach (var k in _khach)
            {
                bool chon = KhopTieuChi(k) || _daTraLoi.Contains(k.MaKhachHang);
                _chon[k.MaKhachHang] = chon;
                if (chon) n++;
            }
            _tieuChiDaApDung = true;
            _txtSearch.Text = "";
            Render();
            _lblError.Text = n == 0 ? "Không có khách hàng nào thỏa tiêu chí." : "";
        }

        /// <summary>Khách hiển thị: theo ô tìm; ẩn khách bị khóa trừ khi đã được chọn.</summary>
        private List<KhachHangDto> DangHienThi()
        {
            string q = ChuanHoa(_txtSearch.Text);
            return _khach.Where(k =>
                (!k.BiKhoa || _chon[k.MaKhachHang]) &&
                (q.Length == 0
                 || ChuanHoa(k.MaKhachHang).Contains(q)
                 || ChuanHoa(k.HoTen).Contains(q)
                 || (k.SoDienThoai ?? "").Contains(q))).ToList();
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
                if (_rdTieuChi.Checked && !_tieuChiDaApDung) ApDungTieuChi();
                ds = _chon.Where(p => p.Value).Select(p => p.Key).ToList();
                if (ds.Count == 0)
                {
                    _lblError.Text = _rdTieuChi.Checked
                        ? "Không có khách hàng nào thỏa tiêu chí. Hãy đổi tiêu chí hoặc chọn \"Tất cả khách hàng\"."
                        : "Hãy chọn ít nhất một khách hàng, hoặc chọn \"Tất cả khách hàng\".";
                    return;
                }
            }

            SetBusy(true);
            var r = await _service.SetDoiTuongAsync(_maKhaoSat, ds);
            if (IsDisposed) return;
            SetBusy(false);
            if (!r.Success) { _lblError.Text = r.Message; return; }

            DataChanged = true;
            SoDoiTuong = ds.Count;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

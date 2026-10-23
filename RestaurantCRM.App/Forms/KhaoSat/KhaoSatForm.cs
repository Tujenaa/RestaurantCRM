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
    /// Quản trị khảo sát: danh sách, tìm kiếm, lọc trạng thái; tạo / sửa khảo sát và câu hỏi;
    /// xem trước như khách hàng thấy; mở / đóng khảo sát; xem thống kê kết quả; xóa khảo sát chưa có phiếu.
    /// </summary>
    public partial class KhaoSatForm : Form
    {
        private readonly KhaoSatService _service = new KhaoSatService();
        private List<KhaoSatDto> _all = new List<KhaoSatDto>();
        private bool _loading;

        private readonly DataGridView _grid = new DataGridView();
        private readonly TextBox _txtSearch = new TextBox();
        private readonly ComboBox _cboTrangThai = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label _lblCount = new Label();
        private readonly Label _lblFooter = new Label();
        private readonly Label _kTong = new Label { Text = "Tổng khảo sát: —" };
        private readonly Label _kMo = new Label { Text = "Đang mở: —" };
        private readonly Label _kNhap = new Label { Text = "Bản nháp: —" };
        private readonly Label _kPhieu = new Label { Text = "Phiếu trả lời: —" };
        private readonly Button _btnThem = new Button { Text = "Tạo khảo sát" };
        private readonly Button _btnLamMoi = new Button { Text = "Làm mới" };
        private readonly Button _btnSua = new Button { Text = "Sửa & câu hỏi" };
        private readonly Button _btnXemTruoc = new Button { Text = "Xem trước" };
        private readonly Button _btnMoDong = new Button { Text = "Mở khảo sát" };
        private readonly Button _btnThongKe = new Button { Text = "Thống kê" };
        private readonly Button _btnXoa = new Button { Text = "Xóa" };

        public KhaoSatForm()
        {
            InitializeComponent();
            Text = "Khảo sát";
            Size = new Size(1180, 720);
            Font = new Font(ThemeManager.FontFamily, 9.5F);
            StartPosition = FormStartPosition.CenterParent;

            BuildGrid();
            _cboTrangThai.Items.AddRange(new object[] { "Tất cả", "Nháp", "Đang mở", "Đã đóng" });
            _cboTrangThai.SelectedIndex = 0;

            PageKit.BuildListPage(this, new ListPageSpec
            {
                Title = new Label { Text = "Khảo sát" },
                Subtitle = new Label { Text = "Tạo bộ câu hỏi, mở cho khách trả lời và theo dõi kết quả." },
                PrimaryActions = new[] { _btnThem },
                HeaderActions = new Control[] { _btnLamMoi },
                KpiLabels = new[] { _kTong, _kMo, _kNhap, _kPhieu },
                Search = _txtSearch,
                SearchPlaceholder = "Tìm theo mã hoặc tiêu đề khảo sát...",
                Filters = new[] { new FilterSpec(_cboTrangThai, new Label { Text = "Trạng thái" }) },
                CountLabel = _lblCount,
                Grid = _grid,
                SecondaryActions = new[] { _btnSua, _btnXemTruoc, _btnMoDong, _btnThongKe },
                DangerActions = new[] { _btnXoa },
                FooterInfo = _lblFooter
            });

            _txtSearch.TextChanged += (s, e) => ApplyFilter();
            _cboTrangThai.SelectedIndexChanged += (s, e) => ApplyFilter();
            _grid.SelectionChanged += (s, e) => UpdateSelectionUi();
            _grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) Sua(); };
            _grid.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.Handled = true; Sua(); }
                else if (e.KeyCode == Keys.Delete) { e.Handled = true; Xoa(); }
            };

            _btnThem.Click += (s, e) => TaoMoi();
            _btnLamMoi.Click += async (s, e) => await LoadDataAsync();
            _btnSua.Click += (s, e) => Sua();
            _btnXemTruoc.Click += (s, e) => XemTruoc();
            _btnMoDong.Click += (s, e) => MoDong();
            _btnThongKe.Click += (s, e) => ThongKe();
            _btnXoa.Click += (s, e) => Xoa();

            UpdateSelectionUi();
            Load += async (s, e) => await LoadDataAsync();
        }

        private void BuildGrid()
        {
            var g = _grid;
            g.AllowUserToAddRows = false; g.AllowUserToDeleteRows = false; g.AllowUserToResizeRows = false;
            g.ReadOnly = true; g.MultiSelect = false; g.RowHeadersVisible = false; g.AutoGenerateColumns = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            AddCol("Ma", "Mã KS", 80);
            AddCol("TieuDe", "Tiêu đề", 320);
            AddCol("SoCauHoi", "Số câu hỏi", 90, DataGridViewContentAlignment.MiddleRight);
            AddCol("SoPhieu", "Phiếu trả lời", 100, DataGridViewContentAlignment.MiddleRight);
            AddCol("NguoiTao", "Người tạo", 150);
            AddCol("NgayTao", "Ngày tạo", 100).DefaultCellStyle.Format = "dd/MM/yyyy";
            AddCol("TrangThai", "Trạng thái", 110);
        }

        private DataGridViewColumn AddCol(string name, string header, int width, DataGridViewContentAlignment align = DataGridViewContentAlignment.MiddleLeft)
        {
            var c = new DataGridViewTextBoxColumn { Name = name, HeaderText = header, Width = width, SortMode = DataGridViewColumnSortMode.Automatic };
            c.DefaultCellStyle.Alignment = align;
            _grid.Columns.Add(c);
            return c;
        }

        // ── Dữ liệu ──────────────────────────────────────────────────────────

        public async Task LoadDataAsync(string chonMa = null)
        {
            if (_loading) return;
            _loading = true;
            _btnLamMoi.Enabled = false;
            _lblCount.Text = "Đang tải...";
            if (chonMa == null) chonMa = SelectedDto()?.MaKhaoSat;
            try
            {
                _all = await _service.GetAllAsync();
                _kTong.Text = "Tổng khảo sát: " + _all.Count.ToString("N0");
                _kMo.Text = "Đang mở: " + _all.Count(k => k.DangMo).ToString("N0");
                _kNhap.Text = "Bản nháp: " + _all.Count(k => k.LaNhap).ToString("N0");
                _kPhieu.Text = "Phiếu trả lời: " + _all.Sum(k => k.SoNguoiTraLoi).ToString("N0");
                ApplyFilter(chonMa);
            }
            catch (ApiException ex)
            {
                _lblCount.Text = "Không tải được dữ liệu";
                if (!IsDisposed) Notify.Error(this, ex.Message, "Lỗi tải danh sách khảo sát");
            }
            finally
            {
                _loading = false;
                if (!IsDisposed) _btnLamMoi.Enabled = true;
            }
        }

        private void ApplyFilter(string chonMa = null)
        {
            if (chonMa == null) chonMa = SelectedDto()?.MaKhaoSat;
            string q = (_txtSearch.Text ?? "").Trim().ToLowerInvariant();
            string tt = _cboTrangThai.SelectedItem as string ?? "Tất cả";

            var list = _all.Where(k =>
                (tt == "Tất cả" || k.TrangThaiHienThi == tt) &&
                (q.Length == 0 || (k.MaKhaoSat ?? "").ToLowerInvariant().Contains(q) || (k.TieuDe ?? "").ToLowerInvariant().Contains(q))).ToList();

            _grid.SuspendLayout();
            _grid.Rows.Clear();
            int chon = -1;
            foreach (var k in list)
            {
                int i = _grid.Rows.Add(k.MaKhaoSat, k.TieuDe, k.SoCauHoi, k.SoNguoiTraLoi,
                    string.IsNullOrWhiteSpace(k.NguoiTao) ? (k.MaNhanVien ?? "—") : k.NguoiTao, (object)k.NgayTao, k.TrangThaiHienThi);
                _grid.Rows[i].Tag = k;
                if (k.MaKhaoSat == chonMa) chon = i;
            }
            _grid.ResumeLayout();
            _grid.ClearSelection();
            if (chon >= 0)
            {
                _grid.Rows[chon].Selected = true;
                _grid.CurrentCell = _grid.Rows[chon].Cells[0];
            }
            _lblCount.Text = list.Count == _all.Count ? "Hiển thị " + list.Count + " khảo sát" : "Hiển thị " + list.Count + " / " + _all.Count + " khảo sát";
            UpdateSelectionUi();
        }

        private KhaoSatDto SelectedDto()
        {
            if (_grid.SelectedRows.Count == 0) return null;
            return _grid.SelectedRows[0].Tag as KhaoSatDto;
        }

        private void UpdateSelectionUi()
        {
            var k = SelectedDto();
            bool has = k != null;
            foreach (var b in new[] { _btnSua, _btnXemTruoc, _btnMoDong, _btnThongKe, _btnXoa }) b.Enabled = has;
            _btnMoDong.Text = has && k.DangMo ? "Đóng khảo sát" : "Mở khảo sát";
            _lblFooter.Text = has
                ? k.TieuDe + " · " + k.SoCauHoi + " câu hỏi · " + k.SoNguoiTraLoi + " phiếu"
                : "Chọn một khảo sát để thao tác";
        }

        // ── Thao tác ─────────────────────────────────────────────────────────

        private async void TaoMoi()
        {
            using (var dlg = new KhaoSatDialog(null))
            {
                Notify.ShowDialog(dlg, this);
                if (dlg.DataChanged) await LoadDataAsync(dlg.MaKhaoSat);
            }
        }

        private async void Sua()
        {
            var k = SelectedDto();
            if (k == null) return;
            using (var dlg = new KhaoSatDialog(k.MaKhaoSat))
            {
                Notify.ShowDialog(dlg, this);
                if (dlg.DataChanged) await LoadDataAsync(k.MaKhaoSat);
            }
        }

        private async void XemTruoc()
        {
            var k = SelectedDto();
            if (k == null) return;
            try
            {
                var ct = await _service.GetByIdAsync(k.MaKhaoSat);
                if (IsDisposed || ct == null) return;
                using (var dlg = new KhaoSatXemTruocDialog(ct)) Notify.ShowDialog(dlg, this);
            }
            catch (ApiException ex) { Notify.Error(this, ex.Message); }
        }

        private async void MoDong()
        {
            var k = SelectedDto();
            if (k == null) return;
            bool mo = !k.DangMo;
            string hoi = mo
                ? "Mở khảo sát \"" + k.TieuDe + "\"?\nKhách hàng sẽ thấy và trả lời được khảo sát trên website."
                : "Đóng khảo sát \"" + k.TieuDe + "\"?\nKhách hàng sẽ không gửi thêm câu trả lời được. Kết quả đã thu vẫn được giữ.";
            if (!Notify.Confirm(this, hoi, mo ? "Mở khảo sát" : "Đóng khảo sát")) return;

            _btnMoDong.Enabled = false;
            var r = await _service.SetStatusAsync(k.MaKhaoSat, mo ? KhaoSatConst.DangMo : KhaoSatConst.DaDong);
            if (IsDisposed) return;
            if (!r.Success) { _btnMoDong.Enabled = true; Notify.Error(this, r.Message, mo ? "Chưa mở được khảo sát" : "Không đóng được khảo sát"); return; }
            await LoadDataAsync(k.MaKhaoSat);
        }

        private void ThongKe()
        {
            var k = SelectedDto();
            if (k == null) return;
            using (var dlg = new ThongKeKhaoSatForm(k.MaKhaoSat, k.TieuDe)) Notify.ShowDialog(dlg, this);
        }

        private async void Xoa()
        {
            var k = SelectedDto();
            if (k == null) return;
            if (k.SoNguoiTraLoi > 0)
            {
                Notify.Warn(this, "Khảo sát \"" + k.TieuDe + "\" đã có " + k.SoNguoiTraLoi + " phiếu trả lời nên không thể xóa (cần giữ dữ liệu thống kê).\nBạn có thể đóng khảo sát thay thế.", "Không thể xóa");
                return;
            }
            if (!Notify.Confirm(this, "Xóa khảo sát \"" + k.TieuDe + "\" cùng toàn bộ câu hỏi và tùy chọn?\nThao tác này không hoàn tác được.", "Xóa khảo sát", true)) return;
            _btnXoa.Enabled = false;
            var r = await _service.DeleteAsync(k.MaKhaoSat);
            if (IsDisposed) return;
            if (!r.Success) { _btnXoa.Enabled = true; Notify.Error(this, r.Message); return; }
            await LoadDataAsync();
        }
    }
}
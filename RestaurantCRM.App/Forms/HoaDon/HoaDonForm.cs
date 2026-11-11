using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.HoaDon
{
    public partial class HoaDonForm : Form
    {
        private readonly HoaDonService _hoaDonService;
        private List<HoaDonDto> _allHoaDon = new List<HoaDonDto>();
        private readonly ComboBox _cboThoiGian = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _cboPhuongThuc = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _cboMucTien = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label _lblCount = new Label { Text = "" };

        // Chi tiết đơn tải sẵn ở nền: dùng cho khung bên phải và tìm kiếm theo tên món
        private readonly Dictionary<string, HoaDonDetailDto> _detailCache = new Dictionary<string, HoaDonDetailDto>();

        // Khung chi tiết bên phải (click 1 lần vào dòng để xem)
        private readonly DetailSidePanel _side = new DetailSidePanel(
            "Chọn một đơn hàng trong danh sách\nđể xem món ăn và thông tin đơn.", "MÓN ĂN", "SL", "THÀNH TIỀN");
        private string _sideMa;

        public HoaDonForm()
        {
            InitializeComponent();
            _hoaDonService = new HoaDonService();
            SetupGridColumns();

            _cboThoiGian.Items.AddRange(new object[] {
                "Tất cả thời gian",
                "Hôm nay",
                "Hôm qua",
                "7 ngày qua",
                "30 ngày qua",
                "Tháng này",
                "Năm nay"
            });
            _cboThoiGian.SelectedIndex = 0;

            _cboPhuongThuc.Items.AddRange(new object[] {
                "Tất cả PTTT",
                "Tiền mặt",
                "Chuyển khoản",
                "Thẻ / Ví điện tử"
            });
            _cboPhuongThuc.SelectedIndex = 0;

            _cboMucTien.Items.AddRange(new object[] {
                "Tất cả mức tiền",
                "Dưới 200.000 đ",
                "200.000 - 500.000 đ",
                "Trên 500.000 đ"
            });
            _cboMucTien.SelectedIndex = 0;

            _cboThoiGian.SelectedIndexChanged += (s, e) => FilterData();
            _cboPhuongThuc.SelectedIndexChanged += (s, e) => FilterData();
            _cboMucTien.SelectedIndexChanged += (s, e) => FilterData();

            // Bố cục giống trang Phản hồi: danh sách bên trái, khung chi tiết bên phải, 2 ô số liệu ở trên
            PageKit.BuildListPage(this, new ListPageSpec
            {
                Title = lblTieuDe,
                Subtitle = lblSub,
                HeaderActions = new Control[] { btnLamMoi },
                KpiLabels = new[] { lblTongDon, lblChoXuLy },
                Search = txtTimKiem,
                SearchIcon = lblTimKiem,
                SearchPlaceholder = "Tìm mã đơn, tên/SĐT khách, NV, địa chỉ...",
                Filters = new[]
                {
                    new FilterSpec(cboTrangThai, lblTrangThai),
                    new FilterSpec(_cboThoiGian, new Label { Text = "Thời gian" }),
                    new FilterSpec(_cboPhuongThuc, new Label { Text = "Thanh toán" }),
                    new FilterSpec(_cboMucTien, new Label { Text = "Mức tiền" })
                },
                CountLabel = _lblCount,
                Grid = dgvHoaDon,
                SidePanel = _side,
                SideWidth = 400,
                SideFullHeight = true,
                KpiSlots = 5
            });

            // Nút "Chi tiết" nằm trong khung bên phải (mở hộp thoại xử lý đơn như cũ)
            _side.AttachButton(btnChiTiet, "Chi tiết ↗");

            // Click 1 lần: xem ở khung bên phải; double-click: mở hộp thoại chi tiết
            dgvHoaDon.CellClick -= dgvHoaDon_CellClick;
            dgvHoaDon.SelectionChanged += (s, e) => ShowSide(SelectedHoaDon());
            dgvHoaDon.CellDoubleClick += dgvHoaDon_CellDoubleClick;
            dgvHoaDon.DataBindingComplete += (s, e) => { ApplyPendingSelection(); ColorGridRows(); };
            // Người dùng đã thao tác trên lưới: không tự bỏ chọn nữa
            dgvHoaDon.CellMouseDown += (s, e) => _bindingPending = false;
            dgvHoaDon.KeyDown += (s, e) => _bindingPending = false;
        }

        private void SetupGridColumns()
        {
            // Bảng hẹp vì có khung bên phải: chỉ giữ trường quan trọng, khách hàng xem ở khung bên phải
            dgvHoaDon.AutoGenerateColumns = false;
            dgvHoaDon.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHoaDon.ScrollBars = ScrollBars.Vertical;
            dgvHoaDon.AllowUserToResizeColumns = true;
            dgvHoaDon.Columns.Clear();

            dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MaHoaDon",
                DataPropertyName = "MaHoaDon",
                HeaderText = "Mã Đơn",
                FillWeight = 14,
                MinimumWidth = 70,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold)
                }
            });

            dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NgayDat",
                DataPropertyName = "NgayDat",
                HeaderText = "Ngày Đặt",
                FillWeight = 19,
                MinimumWidth = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yy HH:mm", NullValue = "-" }
            });

            dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TongTienHienThi",
                DataPropertyName = "TongTienHienThi",
                HeaderText = "Tổng Tiền",
                FillWeight = 17,
                MinimumWidth = 90,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Font = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold),
                    ForeColor = ThemeManager.Primary
                }
            });

            dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TrangThai",
                DataPropertyName = "TrangThai",
                HeaderText = "Trạng Thái",
                FillWeight = 16,
                MinimumWidth = 90,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold)
                }
            });

            dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PhuongThucThanhToan",
                DataPropertyName = "PhuongThucThanhToan",
                HeaderText = "Thanh Toán",
                FillWeight = 17,
                MinimumWidth = 80
            });

            dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TenNhanVien",
                DataPropertyName = "TenNhanVien",
                HeaderText = "Nhân Viên",
                FillWeight = 17,
                MinimumWidth = 80,
                DefaultCellStyle = new DataGridViewCellStyle { NullValue = "—" }
            });

            // Trạng thái hiển thị tiếng Việt (Pending -> Chờ xử lý ...)
            dgvHoaDon.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0 || dgvHoaDon.Columns[e.ColumnIndex].Name != "TrangThai") return;
                var st = e.Value as string;
                if (st == null) return;
                e.Value = ThemeManager.CleanStatus(st);
                e.FormattingApplied = true;
            };
        }

        private async void HoaDonForm_Load(object sender, EventArgs e)
        {
            cboTrangThai.Items.Clear();
            cboTrangThai.Items.AddRange(new object[] {
                "Tất cả trạng thái",
                "Chờ xử lý (Pending)",
                "Đã xác nhận (Confirmed)",
                "Đang chuẩn bị (Preparing)",
                "Đang giao (Delivering)",
                "Hoàn thành (Completed)",
                "Đã hủy (Cancelled)"
            });
            cboTrangThai.SelectedIndex = 0;

            await LoadData();
        }

        private async Task LoadData(string selectMa = null)
        {
            try
            {
                _allHoaDon = await _hoaDonService.GetAllAsync();
                _detailCache.Clear();
                _sideMa = null;
                PrefetchDetails();
                UpdateThongKe();
                FilterData(selectMa);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu đơn hàng: " + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateThongKe()
        {
            int tongDon = _allHoaDon.Count;
            int choXuLy = _allHoaDon.Count(x => IsPending(x.TrangThai));
            int dangGiao = _allHoaDon.Count(x => IsDelivering(x.TrangThai));
            int hoanThanh = _allHoaDon.Count(x => IsCompleted(x.TrangThai));
            int daHuy = _allHoaDon.Count(x => IsCancelled(x.TrangThai));

            lblTongDon.Text = $"Tổng số: {tongDon} đơn";
            lblChoXuLy.Text = $"Chờ xử lý: {choXuLy} đơn";
            lblDangGiao.Text = $"Đang giao: {dangGiao} đơn";
            lblHoanThanh.Text = $"Hoàn thành: {hoanThanh} đơn";
            lblDaHuy.Text = $"Đã hủy: {daHuy} đơn";
        }

        private bool IsPending(string st)
        {
            if (string.IsNullOrEmpty(st)) return false;
            var s = st.ToLower();
            return s.Contains("pending") || s.Contains("chờ") || s.Contains("preparing") || s.Contains("chuẩn bị") || s.Contains("processing");
        }

        private bool IsDelivering(string st)
        {
            if (string.IsNullOrEmpty(st)) return false;
            var s = st.ToLower();
            return s.Contains("delivering") || s.Contains("shipping") || s.Contains("đang giao");
        }

        private bool IsCompleted(string st)
        {
            if (string.IsNullOrEmpty(st)) return false;
            var s = st.ToLower();
            return s.Contains("completed") || s.Contains("hoàn thành");
        }

        private bool IsCancelled(string st)
        {
            if (string.IsNullOrEmpty(st)) return false;
            var s = st.ToLower();
            return s.Contains("cancelled") || s.Contains("hủy");
        }

        private void FilterData() { FilterData(null); }

        private void FilterData(string selectMa)
        {
            // Giữ dòng đang xem nếu nó vẫn còn sau khi lọc
            if (selectMa == null) selectMa = SelectedHoaDon()?.MaHoaDon;

            var keyword = (txtTimKiem.Text ?? "").Trim();
            int filterIndex = cboTrangThai.SelectedIndex;
            int timeIndex = _cboThoiGian.SelectedIndex;
            int payIndex = _cboPhuongThuc.SelectedIndex;
            int amountIndex = _cboMucTien.SelectedIndex;
            DateTime today = DateTime.Today;

            var filtered = _allHoaDon.Where(item => {
                if (!string.IsNullOrEmpty(keyword))
                {
                    string dishNames = null;
                    HoaDonDetailDto det;
                    if (!string.IsNullOrEmpty(item.MaHoaDon) && _detailCache.TryGetValue(item.MaHoaDon, out det) && det?.ChiTiet != null)
                    {
                        dishNames = string.Join(" ", det.ChiTiet.Select(c => c.TenMon));
                    }

                    bool matchKeyword = UiStyler.MatchesSearch(keyword,
                        item.MaHoaDon,
                        item.TenKhachHang,
                        item.SoDienThoai,
                        item.MaKhachHang,
                        item.MaNhanVien,
                        item.TenNhanVien,
                        item.DiaChiGiao,
                        item.PhuongThucThanhToan,
                        dishNames);

                    if (!matchKeyword) return false;
                }

                // Lọc theo thời gian
                if (timeIndex > 0)
                {
                    if (!item.NgayDat.HasValue) return false;
                    DateTime d = item.NgayDat.Value.Date;
                    if (timeIndex == 1 && d != today) return false; // Hôm nay
                    if (timeIndex == 2 && d != today.AddDays(-1)) return false; // Hôm qua
                    if (timeIndex == 3 && d < today.AddDays(-6)) return false; // 7 ngày qua
                    if (timeIndex == 4 && d < today.AddDays(-29)) return false; // 30 ngày qua
                    if (timeIndex == 5 && (d.Month != today.Month || d.Year != today.Year)) return false; // Tháng này
                    if (timeIndex == 6 && d.Year != today.Year) return false; // Năm nay
                }

                // Lọc theo phương thức thanh toán
                if (payIndex > 0)
                {
                    string pt = UiStyler.RemoveDiacritics(item.PhuongThucThanhToan ?? "");
                    if (payIndex == 1 && !pt.Contains("tien mat") && !pt.Contains("cash") && !pt.Contains("cod")) return false;
                    if (payIndex == 2 && !pt.Contains("chuyen khoan") && !pt.Contains("bank") && !pt.Contains("qr")) return false;
                    if (payIndex == 3 && !pt.Contains("the") && !pt.Contains("vi") && !pt.Contains("momo") && !pt.Contains("zalo") && !pt.Contains("vnpay") && !pt.Contains("card")) return false;
                }

                // Lọc theo mức tiền
                if (amountIndex > 0)
                {
                    double total = item.TongThanhToan ?? 0;
                    if (amountIndex == 1 && total >= 200000) return false;
                    if (amountIndex == 2 && (total < 200000 || total > 500000)) return false;
                    if (amountIndex == 3 && total <= 500000) return false;
                }

                switch (filterIndex)
                {
                    case 1: // Chờ xử lý (Pending)
                        return !string.IsNullOrEmpty(item.TrangThai) && (item.TrangThai.ToLower().Contains("pending") || item.TrangThai.ToLower().Contains("chờ"));
                    case 2: // Đã xác nhận (Confirmed)
                        return !string.IsNullOrEmpty(item.TrangThai) && (item.TrangThai.ToLower().Contains("confirmed") || item.TrangThai.ToLower().Contains("xác nhận"));
                    case 3: // Đang chuẩn bị (Preparing)
                        return !string.IsNullOrEmpty(item.TrangThai) && (item.TrangThai.ToLower().Contains("preparing") || item.TrangThai.ToLower().Contains("chuẩn bị"));
                    case 4: // Đang giao (Delivering)
                        return IsDelivering(item.TrangThai);
                    case 5: // Hoàn thành (Completed)
                        return IsCompleted(item.TrangThai);
                    case 6: // Đã hủy (Cancelled)
                        return IsCancelled(item.TrangThai);
                    default:
                        return true;
                }
            })
            // Sắp xếp đơn hàng mới nhất lên đầu (từ mới đến cũ)
            .OrderByDescending(x => x.NgayDat ?? DateTime.MinValue)
            .ThenByDescending(x => CodeNumber(x.MaHoaDon))
            .ThenByDescending(x => x.MaHoaDon)
            .ToList();

            _pendingSelect = selectMa ?? "";
            _bindingPending = true;
            dgvHoaDon.DataSource = filtered;
            _lblCount.Text = filtered.Count == _allHoaDon.Count
                ? $"Hiển thị {filtered.Count} đơn"
                : $"Hiển thị {filtered.Count} / {_allHoaDon.Count} đơn";
            ColorGridRows();
            ApplyPendingSelection();
        }

        // Lưới tự chọn dòng đầu khi gắn dữ liệu (kể cả lần gắn hoãn lúc lưới mới hiện lên).
        // Bỏ chọn đó: chưa chọn đơn nào thì khung bên phải để trống; có đơn đang xem thì chọn lại đơn đó.
        private string _pendingSelect = "";
        private bool _bindingPending;

        private void ApplyPendingSelection()
        {
            if (!_bindingPending) return;
            dgvHoaDon.ClearSelection();
            dgvHoaDon.CurrentCell = null;
            SelectRow(_pendingSelect);
            ShowSide(SelectedHoaDon());
            if (dgvHoaDon.IsHandleCreated && dgvHoaDon.Visible) _bindingPending = false;
        }

        private void SelectRow(string maHoaDon)
        {
            if (string.IsNullOrEmpty(maHoaDon)) return;
            foreach (DataGridViewRow row in dgvHoaDon.Rows)
            {
                if ((row.DataBoundItem as HoaDonDto)?.MaHoaDon != maHoaDon) continue;
                dgvHoaDon.CurrentCell = row.Cells[0];
                row.Selected = true;
                return;
            }
        }

        private HoaDonDto SelectedHoaDon()
        {
            return dgvHoaDon.SelectedRows.Count > 0 ? dgvHoaDon.SelectedRows[0].DataBoundItem as HoaDonDto : null;
        }

        private void ColorGridRows()
        {
            int trangThaiColIndex = dgvHoaDon.Columns["TrangThai"]?.Index ?? -1;
            foreach (DataGridViewRow row in dgvHoaDon.Rows)
            {
                var item = row.DataBoundItem as HoaDonDto;
                if (item != null && trangThaiColIndex >= 0 && trangThaiColIndex < row.Cells.Count)
                {
                    if (IsCompleted(item.TrangThai))
                    {
                        row.Cells[trangThaiColIndex].Style.ForeColor = Color.FromArgb(22, 163, 74);
                    }
                    else if (IsCancelled(item.TrangThai))
                    {
                        row.Cells[trangThaiColIndex].Style.ForeColor = Color.FromArgb(225, 29, 72);
                    }
                    else if (IsDelivering(item.TrangThai))
                    {
                        row.Cells[trangThaiColIndex].Style.ForeColor = Color.FromArgb(2, 132, 199);
                    }
                    else
                    {
                        row.Cells[trangThaiColIndex].Style.ForeColor = Color.FromArgb(217, 119, 6);
                    }
                }
            }
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            FilterData();
        }

        private void cboTrangThai_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterData();
        }

        private async void btnLamMoi_Click(object sender, EventArgs e)
        {
            await LoadData(SelectedHoaDon()?.MaHoaDon);
        }

        private void btnChiTiet_Click(object sender, EventArgs e)
        {
            OpenChiTietDialog();
        }

        // Designer vẫn gắn sự kiện này; click 1 lần giờ chỉ xem ở khung bên phải (gỡ trong constructor)
        private void dgvHoaDon_CellClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        // Double-click vào dòng: mở màn hình xử lý đơn
        private void dgvHoaDon_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvHoaDon.Rows.Count) return;
            var item = dgvHoaDon.Rows[e.RowIndex].DataBoundItem as HoaDonDto;
            if (item == null) return;
            OpenChiTietDialog(item);
        }

        private async void OpenChiTietDialog(HoaDonDto selected = null)
        {
            if (selected == null) selected = SelectedHoaDon();
            if (selected == null)
            {
                MessageBox.Show("Vui lòng chọn một đơn hàng từ danh sách để xem chi tiết.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new HoaDonDialog(selected.MaHoaDon))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadData(selected.MaHoaDon);
                }
            }
        }

        // ── Khung chi tiết bên phải ──────────────────────────────────
        private void ShowSide(HoaDonDto item)
        {
            if (item == null || string.IsNullOrEmpty(item.MaHoaDon))
            {
                _sideMa = null;
                _side.ShowEmpty();
                return;
            }

            if (item.MaHoaDon == _sideMa) return;   // đang hiển thị (hoặc đang tải) đúng đơn này rồi
            _sideMa = item.MaHoaDon;

            HoaDonDetailDto detail;
            if (_detailCache.TryGetValue(item.MaHoaDon, out detail)) RenderSide(item, detail);
            else
            {
                _side.ShowLoading("#" + item.MaHoaDon);
                LoadSideDetail(item);
            }
        }

        private async void LoadSideDetail(HoaDonDto item)
        {
            string ma = item.MaHoaDon;
            try
            {
                var detail = await _hoaDonService.GetByIdAsync(ma);
                if (IsDisposed || detail == null) return;
                _detailCache[ma] = detail;
                if (_sideMa == ma) RenderSide(item, detail);   // người dùng có thể đã chọn đơn khác
            }
            catch (Exception ex)
            {
                if (!IsDisposed && _sideMa == ma) _side.SetItems("Không tải được chi tiết đơn", new List<DetailItem>(), ex.Message);
            }
        }

        private void RenderSide(HoaDonDto item, HoaDonDetailDto d)
        {
            _side.SetHeader("#" + item.MaHoaDon, d.TrangThai ?? item.TrangThai);

            // Món ăn – ưu tiên hiển thị đầu tiên
            var chiTiet = d.ChiTiet ?? new List<ChiTietHoaDonDto>();
            var items = chiTiet.Select(c => new DetailItem
            {
                Name = string.IsNullOrWhiteSpace(c.TenMon) ? c.MaMon : c.TenMon,
                Code = c.ChietKhauTinhToan > 0 ? c.MaMon + "  ·  " + c.DonGiaHienThi + "  ·  CK " + c.ChietKhauHienThi : c.MaMon + "  ·  " + c.DonGiaHienThi,
                Qty = "x" + c.SoLuong,
                Amount = c.ThanhTienHienThi
            }).ToList();
            _side.SetItems("Món ăn (" + chiTiet.Count + " món, " + chiTiet.Sum(c => c.SoLuong) + " phần)", items, "Đơn hàng không có món nào.");

            // Tổng tiền
            double ck = chiTiet.Sum(c => c.ChietKhauTinhToan);
            double goc = chiTiet.Count > 0
                ? chiTiet.Sum(c => ((c.DonGiaGoc.HasValue && c.DonGiaGoc.Value > 0) ? c.DonGiaGoc.Value : (c.DonGiaSauGiam ?? 0)) * c.SoLuong)
                : (d.TongTienHang ?? 0);
            var lines = new List<KeyValuePair<string, string>> { DetailSidePanel.Field("Tiền món", DetailSidePanel.Money(goc)) };
            if (ck > 0) lines.Add(DetailSidePanel.Field("Chiết khấu món", "-" + DetailSidePanel.Money(ck)));
            if ((d.TienGiamVoucher ?? 0) > 0) lines.Add(DetailSidePanel.Field("Giảm voucher", "-" + DetailSidePanel.Money(d.TienGiamVoucher.Value)));
            _side.SetTotals(lines, "Tổng thanh toán", DetailSidePanel.Money(d.TongThanhToan ?? item.TongThanhToan ?? 0));
            // Thông tin khách / địa chỉ: xem trong hộp thoại "Chi tiết ↗"
        }

        // ── Sắp xếp mã đơn theo số ───────────────────────────────────
        private static long CodeNumber(string code)
        {
            if (string.IsNullOrEmpty(code)) return 0;
            var digits = new string(code.Where(char.IsDigit).ToArray());
            long n;
            return long.TryParse(digits, out n) ? n : long.MaxValue;
        }

        // Tải sẵn chi tiết tất cả đơn ở chế độ nền: khung bên phải hiện ngay và tìm kiếm khớp được theo tên món.
        private int _prefetchVersion;
        private async void PrefetchDetails()
        {
            int version = ++_prefetchVersion;
            var codes = _allHoaDon.Where(x => !string.IsNullOrEmpty(x.MaHoaDon))
                                  .Select(x => x.MaHoaDon).Distinct().ToList();
            using (var gate = new System.Threading.SemaphoreSlim(4))
            {
                var tasks = codes.Select(async ma =>
                {
                    await gate.WaitAsync();
                    try
                    {
                        if (version != _prefetchVersion || IsDisposed || _detailCache.ContainsKey(ma)) return;
                        var d = await _hoaDonService.GetByIdAsync(ma);
                        if (d == null || version != _prefetchVersion || IsDisposed || _detailCache.ContainsKey(ma)) return;
                        _detailCache[ma] = d;
                    }
                    catch { /* bỏ qua lỗi từng đơn */ }
                    finally { gate.Release(); }
                }).ToList();
                try { await Task.WhenAll(tasks); } catch { }
            }
        }
    }
}

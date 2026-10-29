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

        // Xem nhanh khi rê chuột (không cần click)
        private readonly OrderHoverCard _hoverCard = new OrderHoverCard();
        private readonly Dictionary<string, HoaDonDetailDto> _detailCache = new Dictionary<string, HoaDonDetailDto>();
        private readonly HashSet<string> _loadingDetail = new HashSet<string>();
        private readonly Timer _hoverWatch = new Timer { Interval = 60 };
        private int _hoverRow = -1;

        public HoaDonForm()
        {
            InitializeComponent();
            _hoaDonService = new HoaDonService();
            SetupGridColumns();
            SetupHoverPreview();

            // Bố cục hiện đại thống nhất (giữ nguyên toàn bộ control & sự kiện của Designer)
            PageKit.BuildListPage(this, new ListPageSpec
            {
                Title = lblTieuDe,
                Subtitle = lblSub,
                HeaderActions = new Control[] { btnLamMoi },
                KpiLabels = new[] { lblTongDon, lblChoXuLy, lblDangGiao, lblHoanThanh, lblDaHuy },
                Search = txtTimKiem,
                SearchIcon = lblTimKiem,
                SearchPlaceholder = "Tìm theo mã đơn, khách hàng...",
                Filters = new[] { new FilterSpec(cboTrangThai, lblTrangThai) },
                Grid = dgvHoaDon,
                SecondaryActions = new[] { btnChiTiet }
            });
        }

        private void SetupGridColumns()
        {
            dgvHoaDon.AutoGenerateColumns = false;
            dgvHoaDon.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvHoaDon.ScrollBars = ScrollBars.Both;
            dgvHoaDon.AllowUserToResizeColumns = true;
            dgvHoaDon.Columns.Clear();

            dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MaHoaDon",
                DataPropertyName = "MaHoaDon",
                HeaderText = "Mã Đơn",
                Width = 130,
                MinimumWidth = 100
            });

            dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NgayDatHienThi",
                DataPropertyName = "NgayDatHienThi",
                HeaderText = "Thời Gian Đặt",
                Width = 160,
                MinimumWidth = 130
            });

            dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TenKhachHang",
                DataPropertyName = "TenKhachHang",
                HeaderText = "Khách Hàng",
                Width = 220,
                MinimumWidth = 160
            });

            dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MaNhanVien",
                DataPropertyName = "MaNhanVien",
                HeaderText = "Mã NV",
                Width = 100,
                MinimumWidth = 80,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TenNhanVien",
                DataPropertyName = "TenNhanVien",
                HeaderText = "Nhân Viên Phụ Trách",
                Width = 180,
                MinimumWidth = 140
            });

            dgvHoaDon.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PhuongThucThanhToan",
                DataPropertyName = "PhuongThucThanhToan",
                HeaderText = "Phương Thức TT",
                Width = 160,
                MinimumWidth = 130
            });

            var colTongTien = new DataGridViewTextBoxColumn
            {
                Name = "TongTienHienThi",
                DataPropertyName = "TongTienHienThi",
                HeaderText = "Tổng Thanh Toán",
                Width = 160,
                MinimumWidth = 130,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(29, 78, 216)
                }
            };
            dgvHoaDon.Columns.Add(colTongTien);

            var colTrangThai = new DataGridViewTextBoxColumn
            {
                Name = "TrangThai",
                DataPropertyName = "TrangThai",
                HeaderText = "Trạng Thái",
                Width = 160,
                MinimumWidth = 130,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                }
            };
            dgvHoaDon.Columns.Add(colTrangThai);
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

        private async Task LoadData()
        {
            try
            {
                _allHoaDon = await _hoaDonService.GetAllAsync();
                _detailCache.Clear();
                PrefetchDetails();
                UpdateThongKe();
                FilterData();
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

        private void FilterData()
        {
            var keyword = (txtTimKiem.Text ?? "").Trim().ToLower();
            int filterIndex = cboTrangThai.SelectedIndex;

            var filtered = _allHoaDon.Where(item => {
                bool matchKeyword = string.IsNullOrEmpty(keyword) ||
                    (!string.IsNullOrEmpty(item.MaHoaDon) && item.MaHoaDon.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(item.TenKhachHang) && item.TenKhachHang.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(item.TenNhanVien) && item.TenNhanVien.ToLower().Contains(keyword));

                if (!matchKeyword) return false;

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
            // Sắp xếp mã đơn tăng dần theo số (DH1, DH2, ..., DH10)
            .OrderBy(x => CodePrefix(x.MaHoaDon))
            .ThenBy(x => CodeNumber(x.MaHoaDon))
            .ThenBy(x => x.MaHoaDon)
            .ToList();

            HideHover();
            dgvHoaDon.DataSource = filtered;
            ColorGridRows();
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
            await LoadData();
        }

        private void btnChiTiet_Click(object sender, EventArgs e)
        {
            OpenChiTietDialog();
        }

        // Click 1 lần vào dòng là mở luôn màn hình xử lý đơn
        private void dgvHoaDon_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvHoaDon.Rows.Count) return;
            var item = dgvHoaDon.Rows[e.RowIndex].DataBoundItem as HoaDonDto;
            if (item == null) return;
            HideHover();
            OpenChiTietDialog(item);
        }

        private async void OpenChiTietDialog(HoaDonDto selected = null)
        {
            HideHover();
            if (selected == null)
            {
                if (dgvHoaDon.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn một đơn hàng từ danh sách để xem chi tiết.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                selected = dgvHoaDon.SelectedRows[0].DataBoundItem as HoaDonDto;
            }
            if (selected == null) return;

            using (var dialog = new HoaDonDialog(selected.MaHoaDon))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadData();
                }
            }
        }

        // ── Sắp xếp mã đơn theo số ───────────────────────────────────
        private static string CodePrefix(string code)
        {
            if (string.IsNullOrEmpty(code)) return "";
            int i = 0;
            while (i < code.Length && !char.IsDigit(code[i])) i++;
            return code.Substring(0, i).ToUpperInvariant();
        }

        private static long CodeNumber(string code)
        {
            if (string.IsNullOrEmpty(code)) return 0;
            var digits = new string(code.Where(char.IsDigit).ToArray());
            long n;
            return long.TryParse(digits, out n) ? n : long.MaxValue;
        }

        // ── Xem nhanh đơn hàng khi rê chuột ──────────────────────────
        private void SetupHoverPreview()
        {
            dgvHoaDon.ShowCellToolTips = false; // tooltip mặc định của lưới chen vào làm thẻ nháy
            dgvHoaDon.CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex < 0) { HideHover(); return; }
                if (e.RowIndex == _hoverRow) return;
                _hoverRow = e.RowIndex;
                ShowHover(_hoverRow);   // hiện ngay, không chờ
            };
            // Rời một ô: nếu chuột không còn nằm trên dòng đơn nào thì ẩn ngay
            dgvHoaDon.CellMouseLeave += (s, e) => BeginInvoke((Action)HideIfOffRow);
            dgvHoaDon.MouseMove += (s, e) =>
            {
                if (_hoverCard.IsShown && dgvHoaDon.HitTest(e.X, e.Y).RowIndex < 0) HideHover();
            };
            dgvHoaDon.Scroll += (s, e) => HideHover();
            dgvHoaDon.MouseLeave += (s, e) =>
            {
                // Chuột đi qua thẻ (trong suốt) vẫn tính là trên lưới; chỉ ẩn khi thật sự rời lưới
                var pt = dgvHoaDon.PointToClient(Cursor.Position);
                if (!dgvHoaDon.ClientRectangle.Contains(pt)) HideHover();
            };

            // Lưới an toàn khi thẻ đang hiện: rời lưới / đổi cửa sổ / mở dialog thì ẩn
            _hoverWatch.Tick += (s, e) =>
            {
                if (!_hoverCard.IsShown) { _hoverWatch.Stop(); return; }
                HideIfOffRow();
                if (!dgvHoaDon.Visible || !IsOwnerWindowActive()) HideHover();
            };

            VisibleChanged += (s, e) => { if (!Visible) HideHover(); };
            Disposed += (s, e) =>
            {
                _hoverWatch.Dispose();
                _hoverCard.Dispose();
            };
        }

        private void HideIfOffRow()
        {
            if (IsDisposed || !_hoverCard.IsShown) return;
            var pt = dgvHoaDon.PointToClient(Cursor.Position);
            if (!dgvHoaDon.ClientRectangle.Contains(pt) || dgvHoaDon.HitTest(pt.X, pt.Y).RowIndex < 0)
                HideHover();
        }

        private bool IsOwnerWindowActive()
        {
            var active = Form.ActiveForm;
            return active != null && (active == (TopLevelControl as Form) || active == _hoverCard);
        }

        private void ShowHover(int rowIndex)
        {
            if (IsDisposed || rowIndex < 0 || rowIndex >= dgvHoaDon.Rows.Count) return;
            if (!IsOwnerWindowActive()) return;
            var item = dgvHoaDon.Rows[rowIndex].DataBoundItem as HoaDonDto;
            if (item == null || string.IsNullOrEmpty(item.MaHoaDon)) return;

            var rowRect = dgvHoaDon.RectangleToScreen(dgvHoaDon.GetRowDisplayRectangle(rowIndex, false));
            HoaDonDetailDto detail;
            _detailCache.TryGetValue(item.MaHoaDon, out detail);
            _hoverCard.ShowFor(item, detail, rowRect, Cursor.Position);
            _hoverWatch.Start();

            if (detail == null) LoadHoverDetail(item.MaHoaDon);
        }

        private async void LoadHoverDetail(string maHoaDon)
        {
            if (!_loadingDetail.Add(maHoaDon)) return;
            try
            {
                var detail = await _hoaDonService.GetByIdAsync(maHoaDon);
                if (detail == null || IsDisposed) return;
                _detailCache[maHoaDon] = detail;
                if (_hoverCard.IsShown && _hoverCard.CurrentMaHoaDon == maHoaDon)
                    _hoverCard.UpdateDetail(detail);
            }
            catch
            {
                // Lỗi mạng khi xem nhanh: bỏ qua, thẻ vẫn hiện thông tin tóm tắt
            }
            finally
            {
                _loadingDetail.Remove(maHoaDon);
            }
        }

        private void HideHover()
        {
            _hoverWatch.Stop();
            _hoverRow = -1;
            _hoverCard.HideCard();
        }

        // Tải sẵn chi tiết tất cả đơn ở chế độ nền để rê chuột là có đủ thông tin ngay,
        // không bị thẻ "Đang tải..." rồi giãn ra (gây cảm giác nháy).
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
                        if (d == null || version != _prefetchVersion || IsDisposed) return;
                        _detailCache[ma] = d;
                        if (_hoverCard.IsShown && _hoverCard.CurrentMaHoaDon == ma) _hoverCard.UpdateDetail(d);
                    }
                    catch { /* bỏ qua lỗi từng đơn */ }
                    finally { gate.Release(); }
                }).ToList();
                try { await Task.WhenAll(tasks); } catch { }
            }
        }
    }
}
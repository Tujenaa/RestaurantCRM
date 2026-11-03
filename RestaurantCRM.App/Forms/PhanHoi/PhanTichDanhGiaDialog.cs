using System;
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
    /// <summary>Phân tích số sao và nhận xét theo từng món ăn.</summary>
    public class PhanTichDanhGiaDialog : CrmDialog
    {
        private readonly PhanHoiService _service;
        private readonly DataGridView _dishes = new DataGridView();
        private readonly DataGridView _comments = new DataGridView();
        private readonly Label _summary = new Label();
        private readonly Label _commentsTitle = new Label();
        private readonly Label _emptyComments = new Label();
        private System.Collections.Generic.List<PhanTichDanhGiaMonDto> _data = new System.Collections.Generic.List<PhanTichDanhGiaMonDto>();

        public PhanTichDanhGiaDialog(PhanHoiService service)
            : base("Phân tích đánh giá món ăn", "Điểm trung bình, phân bố số sao và nhận xét theo từng món.", 1080, 610, null, "Đóng")
        {
            _service = service;
            var root = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            Body.Controls.Add(root);

            _summary.Font = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold);
            _summary.ForeColor = ThemeManager.Ink2;
            _summary.TextAlign = ContentAlignment.MiddleLeft;
            root.Controls.Add(_summary);

            _dishes.AllowUserToAddRows = false;
            _dishes.AllowUserToDeleteRows = false;
            _dishes.AllowUserToResizeRows = false;
            _dishes.ReadOnly = true;
            _dishes.MultiSelect = false;
            _dishes.RowHeadersVisible = false;
            _dishes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dishes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dishes.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenMon", HeaderText = "Món ăn", FillWeight = 180 });
            _dishes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Luot", HeaderText = "Lượt", FillWeight = 45, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            _dishes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Diem", HeaderText = "Điểm TB", FillWeight = 55, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            _dishes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sao1", HeaderText = "1 sao", FillWeight = 45, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            _dishes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sao2", HeaderText = "2 sao", FillWeight = 45, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            _dishes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sao3", HeaderText = "3 sao", FillWeight = 45, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            _dishes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sao4", HeaderText = "4 sao", FillWeight = 45, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            _dishes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sao5", HeaderText = "5 sao", FillWeight = 45, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            UiStyler.StyleGrid(_dishes);
            root.Controls.Add(_dishes);

            _commentsTitle.Font = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold);
            _commentsTitle.ForeColor = ThemeManager.Ink;
            _commentsTitle.Text = "Nhận xét của khách hàng";
            _commentsTitle.TextAlign = ContentAlignment.MiddleLeft;
            root.Controls.Add(_commentsTitle);

            _comments.AllowUserToAddRows = false;
            _comments.AllowUserToDeleteRows = false;
            _comments.AllowUserToResizeRows = false;
            _comments.ReadOnly = true;
            _comments.MultiSelect = false;
            _comments.RowHeadersVisible = false;
            _comments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _comments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _comments.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ngay", HeaderText = "Ngày gửi", FillWeight = 24 });
            _comments.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sao", HeaderText = "Số sao", FillWeight = 16 });
            _comments.Columns.Add(new DataGridViewTextBoxColumn { Name = "NoiDung", HeaderText = "Nội dung nhận xét", FillWeight = 160 });
            _comments.Columns["NoiDung"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            _comments.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            UiStyler.StyleGrid(_comments);
            root.Controls.Add(_comments);

            _emptyComments.Text = "Chọn một món để xem nhận xét.";
            _emptyComments.Dock = DockStyle.None;
            _emptyComments.TextAlign = ContentAlignment.MiddleCenter;
            _emptyComments.ForeColor = ThemeManager.Muted;
            _emptyComments.BackColor = Color.White;
            _emptyComments.Visible = false;
            root.Controls.Add(_emptyComments);

            root.Resize += (s, e) => LayoutContent(root);
            LayoutContent(root);
            _dishes.SelectionChanged += (s, e) => ShowComments(SelectedDish());
            Shown += async (s, e) => await LoadAsync();
        }

        private void LayoutContent(Control root)
        {
            _summary.SetBounds(0, 0, root.ClientSize.Width, 24);
            _dishes.SetBounds(0, 28, root.ClientSize.Width, 226);
            _commentsTitle.SetBounds(0, 262, root.ClientSize.Width, 26);
            int commentsTop = 292;
            _comments.SetBounds(0, commentsTop, root.ClientSize.Width, Math.Max(90, root.ClientSize.Height - commentsTop));
            _emptyComments.SetBounds(0, commentsTop, root.ClientSize.Width, Math.Max(90, root.ClientSize.Height - commentsTop));
        }

        private async Task LoadAsync()
        {
            if (CancelBtn != null) CancelBtn.Enabled = false;
            _summary.Text = "Đang tải dữ liệu phân tích...";
            try
            {
                _data = await _service.GetPhanTichMonAsync();
                _dishes.Rows.Clear();
                foreach (var mon in _data)
                {
                    int row = _dishes.Rows.Add(
                        mon.TenMon ?? mon.MaMon ?? "Món ăn",
                        mon.SoLuotDanhGia,
                        mon.DiemTrungBinh.HasValue ? mon.DiemTrungBinh.Value.ToString("0.00") + " / 5" : "—",
                        mon.SoSao1, mon.SoSao2, mon.SoSao3, mon.SoSao4, mon.SoSao5);
                    _dishes.Rows[row].Tag = mon;
                }

                int tong = _data.Sum(x => x.SoLuotDanhGia);
                _summary.Text = _data.Count == 0
                    ? "Chưa có đánh giá nào gắn với món ăn."
                    : "" + _data.Count.ToString("N0") + " món · " + tong.ToString("N0") + " lượt đánh giá";

                if (_dishes.Rows.Count > 0)
                {
                    _dishes.Rows[0].Selected = true;
                    _dishes.CurrentCell = _dishes.Rows[0].Cells[0];
                    ShowComments(SelectedDish());
                }
                else
                {
                    _commentsTitle.Text = "Nhận xét của khách hàng";
                    _comments.Rows.Clear();
                    _emptyComments.Text = "Chưa có món nào được đánh giá.";
                    _emptyComments.Visible = true;
                    _emptyComments.BringToFront();
                }
            }
            catch (ApiException ex)
            {
                _summary.Text = "Không tải được dữ liệu phân tích.";
                _emptyComments.Text = ex.Message;
                _emptyComments.Visible = true;
                _emptyComments.BringToFront();
            }
            finally
            {
                if (!IsDisposed && CancelBtn != null) CancelBtn.Enabled = true;
            }
        }

        private PhanTichDanhGiaMonDto SelectedDish()
        {
            return _dishes.SelectedRows.Count == 0 ? null : _dishes.SelectedRows[0].Tag as PhanTichDanhGiaMonDto;
        }

        private void ShowComments(PhanTichDanhGiaMonDto mon)
        {
            if (mon == null) return;
            _commentsTitle.Text = "Nhận xét: " + (mon.TenMon ?? mon.MaMon ?? "Món ăn");
            _comments.Rows.Clear();
            foreach (var item in mon.NhanXet ?? new System.Collections.Generic.List<PhanTichNhanXetDto>())
            {
                _comments.Rows.Add(
                    item.NgayDanhGia.HasValue ? item.NgayDanhGia.Value.ToString("dd/MM/yyyy HH:mm") : "—",
                    item.SoSao.HasValue ? item.SoSao.Value + " / 5" : "—",
                    item.NoiDung);
            }

            bool none = _comments.Rows.Count == 0;
            _emptyComments.Text = "Món này chưa có nhận xét bằng chữ.";
            _emptyComments.Visible = none;
            if (none) _emptyComments.BringToFront(); else _comments.BringToFront();
        }
    }
}

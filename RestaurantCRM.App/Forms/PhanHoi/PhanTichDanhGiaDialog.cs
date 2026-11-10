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
        private readonly TextBox _txtSearchDish = new TextBox();
        private readonly ComboBox _cboFilterDish = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _cboFilterCommentSao = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly PillSearch _pillDish;
        private readonly ComboChip _chipDish;
        private readonly ComboChip _chipCommentSao;
        private System.Collections.Generic.List<PhanTichDanhGiaMonDto> _data = new System.Collections.Generic.List<PhanTichDanhGiaMonDto>();

        public PhanTichDanhGiaDialog(PhanHoiService service)
            : base("Phân tích đánh giá món ăn", "Điểm trung bình, phân bố số sao và nhận xét theo từng món.", 1080, 650, null, "Đóng")
        {
            _service = service;
            var root = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            Body.Controls.Add(root);

            _summary.Font = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold);
            _summary.ForeColor = ThemeManager.Ink2;
            _summary.TextAlign = ContentAlignment.MiddleRight;
            root.Controls.Add(_summary);

            _cboFilterDish.Items.AddRange(new object[]
            {
                "Tất cả món",
                "Điểm cao (≥ 4.0 sao)",
                "Cần cải thiện (< 3.5 sao)",
                "Có đánh giá 1-2 sao"
            });
            _cboFilterDish.SelectedIndex = 0;
            _cboFilterDish.Visible = false;
            root.Controls.Add(_cboFilterDish);

            _pillDish = new PillSearch(_txtSearchDish, "Tìm tên hoặc mã món ăn...") { Width = 260 };
            _chipDish = new ComboChip(_cboFilterDish, "Mức điểm");
            root.Controls.Add(_pillDish);
            root.Controls.Add(_chipDish);

            _cboFilterCommentSao.Items.AddRange(new object[]
            {
                "Tất cả nhận xét",
                "5 sao", "4 sao", "3 sao", "2 sao", "1 sao",
                "Từ 3 sao trở xuống"
            });
            _cboFilterCommentSao.SelectedIndex = 0;
            _cboFilterCommentSao.Visible = false;
            root.Controls.Add(_cboFilterCommentSao);
            _chipCommentSao = new ComboChip(_cboFilterCommentSao, "Lọc sao");
            root.Controls.Add(_chipCommentSao);

            _txtSearchDish.TextChanged += (s, e) => RenderDishes();
            _cboFilterDish.SelectedIndexChanged += (s, e) => RenderDishes();
            _cboFilterCommentSao.SelectedIndexChanged += (s, e) => ShowComments(SelectedDish());

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
            int w = root.ClientSize.Width;
            _pillDish.Location = new Point(0, 0);
            _chipDish.Location = new Point(_pillDish.Right + 8, 0);
            _summary.SetBounds(_chipDish.Right + 12, 6, Math.Max(120, w - _chipDish.Right - 12), 26);

            _dishes.SetBounds(0, 44, w, 220);

            int commentBarY = 272;
            _commentsTitle.SetBounds(0, commentBarY + 6, Math.Max(150, w - _chipCommentSao.Width - 12), 26);
            _chipCommentSao.Location = new Point(w - _chipCommentSao.Width, commentBarY);

            int commentsTop = commentBarY + 42;
            _comments.SetBounds(0, commentsTop, w, Math.Max(90, root.ClientSize.Height - commentsTop));
            _emptyComments.SetBounds(0, commentsTop, w, Math.Max(90, root.ClientSize.Height - commentsTop));
        }

        private async Task LoadAsync()
        {
            if (CancelBtn != null) CancelBtn.Enabled = false;
            _summary.Text = "Đang tải dữ liệu phân tích...";
            try
            {
                _data = await _service.GetPhanTichMonAsync();
                RenderDishes();
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

        private void RenderDishes()
        {
            string q = (_txtSearchDish.Text ?? "").Trim();
            int filterIdx = _cboFilterDish.SelectedIndex;

            var filtered = _data.Where(mon =>
            {
                if (!UiStyler.MatchesSearch(q, mon.TenMon, mon.MaMon)) return false;
                double avg = mon.DiemTrungBinh ?? 0;
                if (filterIdx == 1 && avg < 4.0) return false;
                if (filterIdx == 2 && (avg <= 0 || avg >= 3.5)) return false;
                if (filterIdx == 3 && (mon.SoSao1 + mon.SoSao2) <= 0) return false;
                return true;
            }).ToList();

            _dishes.Rows.Clear();
            foreach (var mon in filtered)
            {
                int row = _dishes.Rows.Add(
                    mon.TenMon ?? mon.MaMon ?? "Món ăn",
                    mon.SoLuotDanhGia,
                    mon.DiemTrungBinh.HasValue ? mon.DiemTrungBinh.Value.ToString("0.00") + " / 5" : "—",
                    mon.SoSao1, mon.SoSao2, mon.SoSao3, mon.SoSao4, mon.SoSao5);
                _dishes.Rows[row].Tag = mon;
            }

            int tong = filtered.Sum(x => x.SoLuotDanhGia);
            _summary.Text = _data.Count == 0
                ? "Chưa có đánh giá nào gắn với món ăn."
                : $"Hiển thị {filtered.Count}/{_data.Count} món · {tong:N0} lượt đánh giá";

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
                _emptyComments.Text = "Không có món ăn nào khớp bộ lọc.";
                _emptyComments.Visible = true;
                _emptyComments.BringToFront();
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
            int saoFilter = _cboFilterCommentSao.SelectedIndex;
            var comments = (mon.NhanXet ?? new System.Collections.Generic.List<PhanTichNhanXetDto>()).Where(c =>
            {
                int s = c.SoSao ?? 0;
                if (saoFilter >= 1 && saoFilter <= 5) return s == (6 - saoFilter);
                if (saoFilter == 6) return s > 0 && s <= 3;
                return true;
            }).ToList();

            foreach (var item in comments)
            {
                _comments.Rows.Add(
                    item.NgayDanhGia.HasValue ? item.NgayDanhGia.Value.ToString("dd/MM/yyyy HH:mm") : "—",
                    item.SoSao.HasValue ? item.SoSao.Value + " / 5" : "—",
                    item.NoiDung);
            }

            bool none = _comments.Rows.Count == 0;
            _emptyComments.Text = saoFilter > 0 ? "Không có nhận xét nào ở mức sao này." : "Món này chưa có nhận xét bằng chữ.";
            _emptyComments.Visible = none;
            if (none) _emptyComments.BringToFront(); else _comments.BringToFront();
            LayoutContent(_dishes.Parent);
        }
    }
}

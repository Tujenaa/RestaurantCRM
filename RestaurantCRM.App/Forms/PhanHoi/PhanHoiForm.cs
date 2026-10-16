using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RestaurantCRM.AdminApp.Forms.PhanHoi
{
    public partial class PhanHoiForm : ModuleFormBase
    {
        private static readonly HttpClient _http = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5275")
        };

        private List<PhanHoiDTO> _danhSach = new List<PhanHoiDTO>();

        public PhanHoiForm() : base(
            title: "Quản lý Phản Hồi",
            description: "Danh sách phản hồi / góp ý từ khách hàng",
            columns: new[] { "Mã PH", "Khách Hàng", "Nội Dung", "Trạng Thái", "Ngày Gửi" },
            fields: new ModuleField[] { },
            rows: new string[][] { },
            accent: Color.FromArgb(14, 165, 233)
        )
        {
            this.Load += async (s, e) => await LoadData();
        }

        private async Task LoadData(string trangThai = "")
        {
            try
            {
                string url = "/api/AdminPhanHoi";
                if (!string.IsNullOrEmpty(trangThai))
                    url += $"?trangThai={trangThai}";

                _danhSach = await _http.GetFromJsonAsync<List<PhanHoiDTO>>(url)
                    ?? new List<PhanHoiDTO>();

                var rows = new List<object[]>();
                foreach (var ph in _danhSach)
                {
                    rows.Add(new object[]
                    {
                        ph.MaPhanHoi ?? "",
                        ph.TenKhachHang ?? "",
                        ph.NoiDung != null && ph.NoiDung.Length > 60
                            ? ph.NoiDung.Substring(0, 60) + "..."
                            : ph.NoiDung ?? "",
                        ph.TrangThai ?? "",
                        ph.NgayGui.HasValue
                            ? ph.NgayGui.Value.ToString("dd/MM/yyyy HH:mm")
                            : ""
                    });
                }

                SetTableData(rows);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối API: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnAdd()
        {
            MessageBox.Show("Phản hồi được gửi trực tiếp từ ứng dụng khách hàng.",
                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override async void OnEdit()
        {
            string maPH = GetSelectedId();
            if (string.IsNullOrEmpty(maPH))
            {
                MessageBox.Show("Vui lòng chọn phản hồi để xem chi tiết / trả lời!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var dialog = new PhanHoiChiTietDialog(_http, maPH))
            {
                dialog.ShowDialog(this);
            }
            await LoadData();
        }

        protected override async void OnDelete()
        {
            string maPH = GetSelectedId();
            if (string.IsNullOrEmpty(maPH))
            {
                MessageBox.Show("Vui lòng chọn phản hồi cần đóng/xử lý!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var cf = MessageBox.Show($"Đánh dấu phản hồi #{maPH} là đã đóng (Closed)?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (cf != DialogResult.Yes) return;

            try
            {
                var res = await _http.PutAsJsonAsync($"/api/AdminPhanHoi/{maPH}/TrangThai", new { TrangThai = "Closed" });
                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Đã đóng phản hồi!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadData();
                }
                else
                {
                    MessageBox.Show("Thao tác thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override async void OnRowDoubleClick(string id, System.Data.DataRow row)
        {
            if (!string.IsNullOrEmpty(id))
            {
                using (var dialog = new PhanHoiChiTietDialog(_http, id))
                {
                    dialog.ShowDialog(this);
                }
                await LoadData();
            }
        }
    }

    public class PhanHoiDTO
    {
        public string MaPhanHoi { get; set; }
        public string TenKhachHang { get; set; }
        public string NoiDung { get; set; }
        public string TrangThai { get; set; }
        public DateTime? NgayGui { get; set; }
    }
}
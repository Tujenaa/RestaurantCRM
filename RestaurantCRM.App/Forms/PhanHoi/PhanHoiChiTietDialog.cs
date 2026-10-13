using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RestaurantCRM.AdminApp.Forms.PhanHoi
{
    public class PhanHoiChiTietDialog : Form
    {
        private readonly HttpClient _http;
        private readonly string _maPH;

        private Label _lblInfo;
        private RichTextBox _txtNoiDung;
        private RichTextBox _txtHoiThoai;
        private RichTextBox _txtTraLoi;

        public PhanHoiChiTietDialog(HttpClient http, string maPH)
        {
            _http = http;
            _maPH = maPH;

            this.Text = "Chi tiết phản hồi #" + maPH;
            this.Size = new Size(560, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            BuildUI();
            this.Load += async (s, e) => await LoadChiTiet();
        }

        private void BuildUI()
        {
            _lblInfo = new Label
            {
                Location = new Point(16, 16),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            _txtNoiDung = new RichTextBox
            {
                Location = new Point(16, 40),
                Size = new Size(520, 80),
                ReadOnly = true,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            var lblHoiThoai = new Label
            {
                Text = "Lịch sử hội thoại:",
                Location = new Point(16, 132),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            _txtHoiThoai = new RichTextBox
            {
                Location = new Point(16, 152),
                Size = new Size(520, 130),
                ReadOnly = true,
                BackColor = Color.White
            };

            var lblTraLoi = new Label
            {
                Text = "Trả lời:",
                Location = new Point(16, 295),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            _txtTraLoi = new RichTextBox
            {
                Location = new Point(16, 315),
                Size = new Size(520, 80)
            };

            var btnGui = new Button
            {
                Text = "Gửi trả lời",
                Location = new Point(400, 410),
                Size = new Size(136, 36),
                BackColor = Color.FromArgb(14, 165, 233),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGui.Click += async (s, e) => await GuiTraLoi();

            var btnDong = new Button
            {
                Text = "Đóng",
                Location = new Point(256, 410),
                Size = new Size(136, 36),
                FlatStyle = FlatStyle.Flat
            };
            btnDong.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[]
            {
                _lblInfo, _txtNoiDung,
                lblHoiThoai, _txtHoiThoai,
                lblTraLoi, _txtTraLoi,
                btnGui, btnDong
            });
        }

        private async Task LoadChiTiet()
        {
            try
            {
                var chiTiet = await _http.GetFromJsonAsync<PhanHoiChiTietDTO>
                    ($"/api/AdminPhanHoi/{_maPH}");

                _lblInfo.Text = "Khách hàng: " + chiTiet?.TenKhachHang +
                    "  |  Ngày gửi: " +
                    (chiTiet?.NgayGui.HasValue == true
                        ? chiTiet.NgayGui.Value.ToString("dd/MM/yyyy HH:mm")
                        : "");

                _txtNoiDung.Text = chiTiet?.NoiDung ?? "";

                _txtHoiThoai.Clear();
                if (chiTiet?.HoiThoai != null)
                    foreach (var ht in chiTiet.HoiThoai)
                        _txtHoiThoai.AppendText(
                            "[" + (ht.NgayGui.HasValue
                                ? ht.NgayGui.Value.ToString("dd/MM/yyyy HH:mm")
                                : "") +
                            "] " + ht.NguoiGui + ": " + ht.NoiDung + "\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task GuiTraLoi()
        {
            if (string.IsNullOrWhiteSpace(_txtTraLoi.Text))
            {
                MessageBox.Show("Vui lòng nhập nội dung trả lời!",
                    "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var resReply = await _http.PostAsJsonAsync(
                $"/api/AdminPhanHoi/{_maPH}/Reply",
                new { NoiDung = _txtTraLoi.Text.Trim() });

            if (!resReply.IsSuccessStatusCode)
            {
                MessageBox.Show("Gửi trả lời thất bại!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            await _http.PutAsJsonAsync(
                $"/api/AdminPhanHoi/{_maPH}/TrangThai",
                new { TrangThai = "Đã xử lý" });

            MessageBox.Show("Gửi trả lời thành công!", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }

    public class PhanHoiChiTietDTO : PhanHoiDTO
    {
        public List<HoiThoaiDTO> HoiThoai { get; set; }
    }

    public class HoiThoaiDTO
    {
        public string NguoiGui { get; set; }
        public string NoiDung { get; set; }
        public DateTime? NgayGui { get; set; }
    }
}
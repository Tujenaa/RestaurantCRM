using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.KhachHang
{
    public partial class KhachHangForm : ModuleFormBase
    {
        private static readonly HttpClient _http = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5275")
        };

        private List<KhachHangDTO> _danhSach = new List<KhachHangDTO>();

        public KhachHangForm() : base(
            title: "Quản lý Khách Hàng",
            description: "Danh sách khách hàng của nhà hàng",
            columns: new[] { "Mã KH", "Họ Tên", "Số Điện Thoại", "Email", "Giới Tính", "Địa Chỉ", "Trạng Thái" },
            fields: new ModuleField[]
            {
                new ModuleField("MaKhachHang", "Mã Khách Hàng", false),
                new ModuleField("HoTen",       "Họ Tên",        true),
                new ModuleField("SoDienThoai", "Số Điện Thoại", true),
                new ModuleField("Email",       "Email",         false),
                new ModuleField("GioiTinh",    "Giới Tính",     false, options: new[] { "Nam", "Nữ", "Khác" }),
                new ModuleField("DiaChi",      "Địa Chỉ",       false),
                new ModuleField("TrangThai",   "Trạng Thái",    false, options: new[] { "Active", "Locked" }),
            },
            rows: new string[][] { },
            accent: Color.FromArgb(37, 99, 235)
        )
        {
            this.Load += async (s, e) => await LoadData();
        }

        // ==================== LOAD DATA TỪ API ====================
        public async Task LoadData()
        {
            try
            {
                _danhSach = await _http.GetFromJsonAsync<List<KhachHangDTO>>("/api/AdminKhachHang")
                    ?? new List<KhachHangDTO>();

                var rows = new List<object[]>();
                foreach (var kh in _danhSach)
                {
                    rows.Add(new object[]
                    {
                        kh.MaKhachHang ?? "",
                        kh.HoTen ?? "",
                        kh.SoDienThoai ?? "",
                        kh.Email ?? "",
                        kh.GioiTinh ?? "",
                        kh.DiaChi ?? "",
                        kh.TrangThai ?? "Active"
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

        // ==================== HOOKS CHO TOOLBAR BUTTONS ====================
        protected override async void OnAdd()
        {
            await MoFormChiTiet(null, true);
        }

        protected override async void OnEdit()
        {
            string maKH = GetSelectedId();
            if (string.IsNullOrEmpty(maKH))
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            await MoFormChiTiet(maKH, false);
        }

        protected override async void OnDelete()
        {
            string maKH = GetSelectedId();
            if (string.IsNullOrEmpty(maKH))
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            await XoaKhachHang(maKH);
        }

        protected override async void OnRowDoubleClick(string id, System.Data.DataRow row)
        {
            if (!string.IsNullOrEmpty(id))
            {
                await MoFormChiTiet(id, false);
            }
        }

        // ==================== FORM THÊM / SỬA KHÁCH HÀNG (NGAY HÀNG THẲNG LỐI) ====================
        private async Task MoFormChiTiet(string maKH, bool isNew = false)
        {
            KhachHangDTO kh = null;

            if (!isNew && !string.IsNullOrEmpty(maKH))
            {
                try
                {
                    kh = await _http.GetFromJsonAsync<KhachHangDTO>($"/api/AdminKhachHang/{maKH}");
                }
                catch
                {
                    MessageBox.Show("Không lấy được thông tin chi tiết khách hàng!",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            using (var form = new Form())
            {
                form.Text = isNew ? "Thêm Khách Hàng Mới" : $"Chỉnh Sửa Khách Hàng #{maKH}";
                form.ClientSize = new Size(520, 440);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.BackColor = Color.White;
                form.Font = new Font("Segoe UI", 9.5F);

                // --- Header ---
                var lblHeader = new Label
                {
                    Text = isNew ? "＋ Thêm Khách Hàng Mới" : "✏️ Cập Nhật Thông Tin Khách Hàng",
                    Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(17, 24, 39),
                    Location = new Point(24, 16),
                    AutoSize = true
                };
                var lblSubHeader = new Label
                {
                    Text = isNew ? "Vui lòng nhập các thông tin bắt buộc có dấu (*)" : $"Mã khách hàng: {maKH}",
                    Font = new Font("Segoe UI", 8.5F),
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Location = new Point(25, 43),
                    AutoSize = true
                };

                // --- CỘT 1 (X = 24, Width = 226) & CỘT 2 (X = 270, Width = 226) ---
                // Hàng 1 (Y = 76)
                var lblHoTen = new Label { Text = "Họ Tên *", Location = new Point(24, 76), AutoSize = true, ForeColor = Color.FromArgb(55, 65, 81) };
                var txtHoTen = new TextBox { Location = new Point(24, 97), Width = 226, Text = kh?.HoTen ?? "" };

                var lblSDT = new Label { Text = "Số Điện Thoại *", Location = new Point(270, 76), AutoSize = true, ForeColor = Color.FromArgb(55, 65, 81) };
                var txtSDT = new TextBox { Location = new Point(270, 97), Width = 226, Text = kh?.SoDienThoai ?? "" };

                // Hàng 2 (Y = 138)
                var lblEmail = new Label { Text = "Email", Location = new Point(24, 138), AutoSize = true, ForeColor = Color.FromArgb(55, 65, 81) };
                var txtEmail = new TextBox { Location = new Point(24, 159), Width = 226, Text = kh?.Email ?? "" };

                var lblGioiTinh = new Label { Text = "Giới Tính", Location = new Point(270, 138), AutoSize = true, ForeColor = Color.FromArgb(55, 65, 81) };
                var cboGioiTinh = new ComboBox
                {
                    Location = new Point(270, 159),
                    Width = 226,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cboGioiTinh.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
                cboGioiTinh.SelectedItem = !string.IsNullOrEmpty(kh?.GioiTinh) ? kh.GioiTinh : "Nam";

                // Hàng 3 (Y = 200)
                var lblNgaySinh = new Label { Text = "Ngày Sinh (yyyy-MM-dd)", Location = new Point(24, 200), AutoSize = true, ForeColor = Color.FromArgb(55, 65, 81) };
                var txtNgaySinh = new TextBox { Location = new Point(24, 221), Width = 226, Text = kh?.NgaySinh ?? "" };

                var lblTrangThai = new Label { Text = "Trạng Thái", Location = new Point(270, 200), AutoSize = true, ForeColor = Color.FromArgb(55, 65, 81) };
                var cboTrangThai = new ComboBox
                {
                    Location = new Point(270, 221),
                    Width = 226,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cboTrangThai.Items.AddRange(new object[] { "Active", "Locked" });
                cboTrangThai.SelectedItem = !string.IsNullOrEmpty(kh?.TrangThai) ? kh.TrangThai : "Active";

                // Hàng 4 (Full Width: X = 24, Width = 472)
                var lblDiaChi = new Label { Text = "Địa Chỉ", Location = new Point(24, 262), AutoSize = true, ForeColor = Color.FromArgb(55, 65, 81) };
                var txtDiaChi = new TextBox { Location = new Point(24, 283), Width = 472, Text = kh?.DiaChi ?? "" };

                // Divider line
                var pnlDivider = new Panel
                {
                    Location = new Point(24, 345),
                    Size = new Size(472, 1),
                    BackColor = Color.FromArgb(229, 231, 235)
                };

                // Buttons
                var btnHuy = new Button
                {
                    Text = "Hủy",
                    Location = new Point(286, 368),
                    Size = new Size(95, 36),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(75, 85, 99),
                    Cursor = Cursors.Hand
                };
                btnHuy.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
                btnHuy.Click += (s, e) => form.Close();

                var btnLuu = new Button
                {
                    Text = isNew ? "＋ Lưu Mới" : "💾 Cập Nhật",
                    Location = new Point(391, 368),
                    Size = new Size(105, 36),
                    BackColor = Color.FromArgb(37, 99, 235),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnLuu.FlatAppearance.BorderColor = Color.FromArgb(37, 99, 235);

                btnLuu.Click += async (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(txtHoTen.Text))
                    {
                        MessageBox.Show("Vui lòng nhập họ tên khách hàng!", "Thiếu thông tin",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtHoTen.Focus();
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txtSDT.Text))
                    {
                        MessageBox.Show("Vui lòng nhập số điện thoại!", "Thiếu thông tin",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtSDT.Focus();
                        return;
                    }

                    btnLuu.Enabled = false;
                    btnLuu.Text = "Đang lưu...";

                    try
                    {
                        var req = new
                        {
                            HoTen = txtHoTen.Text.Trim(),
                            SoDienThoai = string.IsNullOrWhiteSpace(txtSDT.Text) ? null : txtSDT.Text.Trim(),
                            Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                            GioiTinh = cboGioiTinh.SelectedItem?.ToString(),
                            NgaySinh = string.IsNullOrWhiteSpace(txtNgaySinh.Text) ? null : txtNgaySinh.Text.Trim(),
                            DiaChi = string.IsNullOrWhiteSpace(txtDiaChi.Text) ? null : txtDiaChi.Text.Trim(),
                            TrangThai = cboTrangThai.SelectedItem?.ToString() ?? "Active"
                        };

                        HttpResponseMessage res;
                        if (isNew)
                            res = await _http.PostAsJsonAsync("/api/AdminKhachHang", req);
                        else
                            res = await _http.PutAsJsonAsync($"/api/AdminKhachHang/{maKH}", req);

                        if (res.IsSuccessStatusCode)
                        {
                            MessageBox.Show(isNew ? "Thêm khách hàng thành công!" : "Cập nhật khách hàng thành công!",
                                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            form.Close();
                            await LoadData();
                        }
                        else
                        {
                            string err = await res.Content.ReadAsStringAsync();
                            MessageBox.Show($"Thao tác thất bại: {err}", "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi gọi API: {ex.Message}", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        btnLuu.Enabled = true;
                        btnLuu.Text = isNew ? "＋ Lưu Mới" : "💾 Cập Nhật";
                    }
                };

                form.Controls.AddRange(new Control[]
                {
                    lblHeader, lblSubHeader,
                    lblHoTen, txtHoTen,
                    lblSDT, txtSDT,
                    lblEmail, txtEmail,
                    lblGioiTinh, cboGioiTinh,
                    lblNgaySinh, txtNgaySinh,
                    lblTrangThai, cboTrangThai,
                    lblDiaChi, txtDiaChi,
                    pnlDivider,
                    btnHuy, btnLuu
                });

                form.ShowDialog(this);
            }
        }

        // ==================== XÓA KHÁCH HÀNG QUA API ====================
        public async Task XoaKhachHang(string maKH)
        {
            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa khách hàng #{maKH}?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                var res = await _http.DeleteAsync($"/api/AdminKhachHang/{maKH}");

                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Xóa khách hàng thành công!",
                        "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadData();
                }
                else
                {
                    string err = await res.Content.ReadAsStringAsync();
                    MessageBox.Show(string.IsNullOrEmpty(err) ? "Xóa thất bại!" : err,
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối API: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class KhachHangDTO
    {
        public string MaKhachHang { get; set; }
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string DiaChi { get; set; }
        public string TrangThai { get; set; }
    }
}
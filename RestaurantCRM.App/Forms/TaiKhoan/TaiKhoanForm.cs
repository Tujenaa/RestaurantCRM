using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Forms;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.TaiKhoan
{
    public class TaiKhoanForm : ModuleFormBase
    {
        private readonly TaiKhoanService _taiKhoanService = new TaiKhoanService();
        private readonly VaiTroService _vaiTroService = new VaiTroService();
        private List<NhanVienModel> _allEmployees = new List<NhanVienModel>();
        private List<VaiTroModel> _allRoles = new List<VaiTroModel>();

        public TaiKhoanForm() : base(
            title: "Tài khoản nhân viên",
            description: "Quản lý danh sách tài khoản, phân quyền vai trò và trạng thái hoạt động.",
            columns: new[] { "Mã NV", "Họ tên", "Tên đăng nhập", "Vai trò", "Trạng thái", "Quyền hạn chi tiết" },
            fields: new[]
            {
                new ModuleField("id", "Mã nhân viên", true),
                new ModuleField("name", "Họ tên", true),
                new ModuleField("username", "Tên đăng nhập", true),
                new ModuleField("role", "Vai trò", true),
                new ModuleField("status", "Trạng thái", true, new[] { "Active", "Inactive" }),
                new ModuleField("permissions", "Quyền", false)
            },
            rows: new string[][] { },
            accent: ThemeManager.Primary)
        {
            this.Load += async (s, e) =>
            {
                await LoadRoles();
                await LoadData();
            };
        }

        private async Task LoadRoles()
        {
            _allRoles = await _vaiTroService.GetAllAsync();
        }

        public async Task LoadData()
        {
            try
            {
                _allEmployees = await _taiKhoanService.GetAllAsync();
                var rows = new List<object[]>();
                foreach (var nv in _allEmployees)
                {
                    string statusText = nv.TrangThai == "Active" ? "Hoạt động" : "Tạm khóa";
                    string permSummary = nv.Permissions != null && nv.Permissions.Count > 0
                        ? $"{nv.Permissions.Count} quyền ({string.Join(", ", nv.Permissions.Take(3))}{(nv.Permissions.Count > 3 ? "..." : "")})"
                        : "Chưa cấp quyền";

                    rows.Add(new object[]
                    {
                        nv.MaNhanVien ?? "",
                        nv.HoTen ?? "",
                        nv.TenDangNhap ?? "",
                        nv.TenVaiTro ?? nv.MaVaiTro ?? "",
                        statusText,
                        permSummary
                    });
                }
                SetTableData(rows);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu tài khoản: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override async void OnAdd()
        {
            await ShowEmployeeDialog(null, true);
        }

        protected override async void OnEdit()
        {
            var id = GetSelectedId();
            if (string.IsNullOrEmpty(id))
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần chỉnh sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var nv = _allEmployees.FirstOrDefault(x => x.MaNhanVien == id);
            await ShowEmployeeDialog(nv, false);
        }

        protected override async void OnRowDoubleClick(string id, System.Data.DataRow row)
        {
            if (string.IsNullOrEmpty(id)) return;
            var nv = _allEmployees.FirstOrDefault(x => x.MaNhanVien == id);
            await ShowEmployeeDialog(nv, false);
        }

        protected override async void OnDelete()
        {
            var id = GetSelectedId();
            if (string.IsNullOrEmpty(id))
            {
                MessageBox.Show("Vui lòng chọn tài khoản cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var nv = _allEmployees.FirstOrDefault(x => x.MaNhanVien == id);
            if (nv != null && nv.TenDangNhap?.ToLower() == "admin")
            {
                MessageBox.Show("Không thể xóa tài khoản Quản trị viên tối cao (admin)!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa/vô hiệu hóa tài khoản '{id}' ({nv?.HoTen})?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var result = await _taiKhoanService.DeleteAsync(id);
                if (result.Success)
                {
                    MessageBox.Show(result.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadData();
                }
                else
                {
                    MessageBox.Show(result.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task ShowEmployeeDialog(NhanVienModel model, bool isNew)
        {
            if (_allRoles == null || _allRoles.Count == 0)
            {
                await LoadRoles();
            }

            using (var dlg = new Form())
            {
                dlg.Text = isNew ? "Thêm Tài Khoản Nhân Viên Mới" : $"Chỉnh Sửa Tài Khoản - {model?.HoTen} ({model?.MaNhanVien})";
                dlg.Size = new Size(540, 520);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = Color.White;
                dlg.Font = new Font("Segoe UI", 9.5F);

                // Header
                var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(20, 10, 20, 10) };
                var lblH1 = new Label { Text = isNew ? "THÊM TÀI KHOẢN MỚI" : "CẬP NHẬT TÀI KHOẢN", Font = new Font("Segoe UI", 11.5F, FontStyle.Bold), ForeColor = ThemeManager.Ink, AutoSize = true, Location = new Point(18, 10) };
                var lblH2 = new Label { Text = "Gán vai trò và thiết lập quyền truy cập cho nhân viên.", Font = new Font("Segoe UI", 8.5F), ForeColor = ThemeManager.Muted, AutoSize = true, Location = new Point(20, 32) };
                pnlHeader.Controls.Add(lblH1);
                pnlHeader.Controls.Add(lblH2);
                dlg.Controls.Add(pnlHeader);

                // Body Panel
                var pnlBody = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(24, 16, 24, 16) };
                dlg.Controls.Add(pnlBody);
                pnlBody.BringToFront();

                int y = 14;

                // Mã NV
                var lblMa = new Label { Text = "Mã nhân viên (để trống để tự sinh):", Location = new Point(24, y), AutoSize = true, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };
                y += 20;
                var txtMa = new TextBox { Location = new Point(24, y), Width = 470, Font = new Font("Segoe UI", 9.5F), Text = model?.MaNhanVien ?? "", ReadOnly = !isNew, BackColor = !isNew ? Color.FromArgb(241, 245, 249) : Color.White };
                pnlBody.Controls.Add(lblMa);
                pnlBody.Controls.Add(txtMa);
                y += 36;

                // Họ tên
                var lblHoTen = new Label { Text = "Họ và tên nhân viên (*):", Location = new Point(24, y), AutoSize = true, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };
                y += 20;
                var txtHoTen = new TextBox { Location = new Point(24, y), Width = 470, Font = new Font("Segoe UI", 9.5F), Text = model?.HoTen ?? "" };
                pnlBody.Controls.Add(lblHoTen);
                pnlBody.Controls.Add(txtHoTen);
                y += 36;

                // Tên đăng nhập
                var lblUser = new Label { Text = "Tên đăng nhập (*):", Location = new Point(24, y), AutoSize = true, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };
                y += 20;
                var txtUser = new TextBox { Location = new Point(24, y), Width = 470, Font = new Font("Segoe UI", 9.5F), Text = model?.TenDangNhap ?? "", ReadOnly = !isNew, BackColor = !isNew ? Color.FromArgb(241, 245, 249) : Color.White };
                pnlBody.Controls.Add(lblUser);
                pnlBody.Controls.Add(txtUser);
                y += 36;

                // Mật khẩu (nếu là thêm mới) hoặc Đổi mật khẩu
                TextBox txtPass = null;
                if (isNew)
                {
                    var lblPass = new Label { Text = "Mật khẩu ban đầu (*):", Location = new Point(24, y), AutoSize = true, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };
                    y += 20;
                    txtPass = new TextBox { Location = new Point(24, y), Width = 470, Font = new Font("Segoe UI", 9.5F), UseSystemPasswordChar = true, Text = "123456" };
                    pnlBody.Controls.Add(lblPass);
                    pnlBody.Controls.Add(txtPass);
                    y += 36;
                }
                else
                {
                    var btnResetPass = new Button { Text = "🔑 Đặt lại / Đổi mật khẩu cho tài khoản này", Location = new Point(24, y), Size = new Size(320, 30), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = ThemeManager.Primary, Cursor = Cursors.Hand };
                    btnResetPass.Click += async (s, e) =>
                    {
                        await ShowChangePasswordDialog(model?.MaNhanVien, model?.HoTen);
                    };
                    pnlBody.Controls.Add(btnResetPass);
                    y += 38;
                }

                // Vai trò (ComboBox nạp từ API VaiTro)
                var lblRole = new Label { Text = "Vai trò & Phân quyền (*):", Location = new Point(24, y), AutoSize = true, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };
                y += 20;
                var cboRole = new ComboBox { Location = new Point(24, y), Width = 470, Font = new Font("Segoe UI", 9.5F), DropDownStyle = ComboBoxStyle.DropDownList };
                foreach (var r in _allRoles)
                {
                    cboRole.Items.Add(new KeyValuePair<string, string>(r.MaVaiTro, $"{r.TenVaiTro} ({r.MaVaiTro}) - {r.Permissions.Count} quyền"));
                }
                cboRole.DisplayMember = "Value";
                cboRole.ValueMember = "Key";
                if (model != null && !string.IsNullOrEmpty(model.MaVaiTro))
                {
                    for (int i = 0; i < cboRole.Items.Count; i++)
                    {
                        var item = (KeyValuePair<string, string>)cboRole.Items[i];
                        if (item.Key == model.MaVaiTro) { cboRole.SelectedIndex = i; break; }
                    }
                }
                if (cboRole.SelectedIndex < 0 && cboRole.Items.Count > 0) cboRole.SelectedIndex = 0;
                pnlBody.Controls.Add(lblRole);
                pnlBody.Controls.Add(cboRole);
                y += 36;

                // Trạng thái
                var lblStatus = new Label { Text = "Trạng thái hoạt động:", Location = new Point(24, y), AutoSize = true, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };
                y += 20;
                var cboStatus = new ComboBox { Location = new Point(24, y), Width = 470, Font = new Font("Segoe UI", 9.5F), DropDownStyle = ComboBoxStyle.DropDownList };
                cboStatus.Items.Add("Active - Hoạt động bình thường");
                cboStatus.Items.Add("Inactive - Tạm khóa tài khoản");
                cboStatus.SelectedIndex = (model != null && model.TrangThai == "Inactive") ? 1 : 0;
                pnlBody.Controls.Add(lblStatus);
                pnlBody.Controls.Add(cboStatus);
                y += 44;

                // Footer
                var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(20, 10, 20, 10) };
                var btnSave = new Button { Text = "💾 Lưu tài khoản", DialogResult = DialogResult.None, BackColor = ThemeManager.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Size = new Size(150, 36), Location = new Point(260, 10), Cursor = Cursors.Hand };
                var btnCancel = new Button { Text = "Hủy bỏ", DialogResult = DialogResult.Cancel, Size = new Size(90, 36), Location = new Point(420, 10), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
                pnlFooter.Controls.Add(btnSave);
                pnlFooter.Controls.Add(btnCancel);
                dlg.Controls.Add(pnlFooter);

                btnSave.Click += async (s, e) =>
                {
                    string hoTen = txtHoTen.Text.Trim();
                    string username = txtUser.Text.Trim();
                    if (string.IsNullOrEmpty(hoTen) || string.IsNullOrEmpty(username))
                    {
                        MessageBox.Show("Vui lòng nhập đầy đủ họ tên và tên đăng nhập.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (cboRole.SelectedItem == null)
                    {
                        MessageBox.Show("Vui lòng chọn vai trò cho nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    string maVaiTro = ((KeyValuePair<string, string>)cboRole.SelectedItem).Key;
                    string trangThai = cboStatus.SelectedIndex == 1 ? "Inactive" : "Active";

                    btnSave.Enabled = false;
                    try
                    {
                        if (isNew)
                        {
                            string pass = txtPass?.Text?.Trim() ?? "123456";
                            string maNV = txtMa.Text.Trim();
                            var res = await _taiKhoanService.CreateAsync(maNV, hoTen, username, pass, maVaiTro, trangThai);
                            if (res.Success)
                            {
                                MessageBox.Show("Thêm mới tài khoản nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                dlg.DialogResult = DialogResult.OK;
                                dlg.Close();
                            }
                            else
                            {
                                MessageBox.Show(res.Message, "Lỗi thêm mới", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            var res = await _taiKhoanService.UpdateAsync(model.MaNhanVien, hoTen, maVaiTro, trangThai);
                            if (res.Success)
                            {
                                MessageBox.Show("Cập nhật thông tin tài khoản thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                dlg.DialogResult = DialogResult.OK;
                                dlg.Close();
                            }
                            else
                            {
                                MessageBox.Show(res.Message, "Lỗi cập nhật", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    finally
                    {
                        btnSave.Enabled = true;
                    }
                };

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadData();
                }
            }
        }

        private async Task ShowChangePasswordDialog(string maNV, string hoTen)
        {
            using (var dlg = new Form())
            {
                dlg.Text = "Đổi Mật Khẩu";
                dlg.Size = new Size(380, 220);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = Color.White;

                var lblInfo = new Label { Text = $"Đổi mật khẩu cho: {hoTen} ({maNV})", Location = new Point(20, 16), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
                var lblPass = new Label { Text = "Mật khẩu mới:", Location = new Point(20, 50), AutoSize = true };
                var txtPass = new TextBox { Location = new Point(20, 72), Width = 320, UseSystemPasswordChar = true };

                var btnOk = new Button { Text = "Cập nhật", Location = new Point(140, 120), Size = new Size(100, 32), BackColor = ThemeManager.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                var btnCancel = new Button { Text = "Hủy", Location = new Point(250, 120), Size = new Size(90, 32), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel };

                dlg.Controls.Add(lblInfo);
                dlg.Controls.Add(lblPass);
                dlg.Controls.Add(txtPass);
                dlg.Controls.Add(btnOk);
                dlg.Controls.Add(btnCancel);

                btnOk.Click += async (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(txtPass.Text))
                    {
                        MessageBox.Show("Mật khẩu mới không được để trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    btnOk.Enabled = false;
                    try
                    {
                        var res = await _taiKhoanService.ChangePasswordAsync(maNV, txtPass.Text.Trim());
                        if (res.Success)
                        {
                            MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            dlg.DialogResult = DialogResult.OK;
                            dlg.Close();
                        }
                        else
                        {
                            MessageBox.Show(res.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    finally
                    {
                        btnOk.Enabled = true;
                    }
                };

                dlg.ShowDialog(this);
            }
        }
    }
}

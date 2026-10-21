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

namespace RestaurantCRM.AdminApp.Forms.VaiTro
{
    public class VaiTroForm : ModuleFormBase
    {
        private readonly VaiTroService _service = new VaiTroService();
        private List<VaiTroModel> _allRoles = new List<VaiTroModel>();
        private List<PermissionItemModel> _availablePermissions = new List<PermissionItemModel>();

        public VaiTroForm() : base(
            title: "Vai trò & Phân quyền",
            description: "Xem và quản lý các vai trò, phạm vi quyền chi tiết của từng chức năng.",
            columns: new[] { "Mã vai trò", "Tên vai trò", "Số nhân viên", "Quyền hạn được cấp" },
            fields: new[]
            {
                new ModuleField("id", "Mã vai trò", true),
                new ModuleField("name", "Tên vai trò", true),
                new ModuleField("count", "Số nhân viên", false),
                new ModuleField("permissions", "Quyền hạn", false)
            },
            rows: new string[][] { },
            accent: ThemeManager.Violet)
        {
            this.Load += async (s, e) =>
            {
                await LoadPermissionsCatalog();
                await LoadData();
            };
        }

        private async Task LoadPermissionsCatalog()
        {
            _availablePermissions = await _service.GetAvailablePermissionsAsync();
        }

        public async Task LoadData()
        {
            try
            {
                _allRoles = await _service.GetAllAsync();
                var rows = new List<object[]>();
                foreach (var r in _allRoles)
                {
                    string permSummary = "";
                    if (r.Permissions == null || r.Permissions.Count == 0)
                    {
                        permSummary = "Chưa cấu hình quyền";
                    }
                    else if (r.Permissions.Count >= _availablePermissions.Count && _availablePermissions.Count > 0)
                    {
                        permSummary = "⭐ Toàn quyền hệ thống (" + r.Permissions.Count + " quyền)";
                    }
                    else
                    {
                        permSummary = $"{r.Permissions.Count} quyền: " + string.Join(", ", r.Permissions);
                    }

                    rows.Add(new object[]
                    {
                        r.MaVaiTro ?? "",
                        r.TenVaiTro ?? "",
                        r.SoLuongNhanVien + " tài khoản",
                        permSummary
                    });
                }
                SetTableData(rows);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách vai trò: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override async void OnAdd()
        {
            await ShowRoleDialog(null, true);
        }

        protected override async void OnEdit()
        {
            var id = GetSelectedId();
            if (string.IsNullOrEmpty(id))
            {
                MessageBox.Show("Vui lòng chọn vai trò cần chỉnh sửa phân quyền.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var role = _allRoles.FirstOrDefault(r => r.MaVaiTro == id);
            await ShowRoleDialog(role, false);
        }

        protected override async void OnRowDoubleClick(string id, System.Data.DataRow row)
        {
            if (string.IsNullOrEmpty(id)) return;
            var role = _allRoles.FirstOrDefault(r => r.MaVaiTro == id);
            await ShowRoleDialog(role, false);
        }

        protected override async void OnDelete()
        {
            var id = GetSelectedId();
            if (string.IsNullOrEmpty(id))
            {
                MessageBox.Show("Vui lòng chọn vai trò cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa vai trò '{id}'?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var result = await _service.DeleteAsync(id);
                if (result.Success)
                {
                    MessageBox.Show("Đã xóa vai trò thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadData();
                }
                else
                {
                    MessageBox.Show(result.Message, "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private async Task ShowRoleDialog(VaiTroModel model, bool isNew)
        {
            if (_availablePermissions == null || _availablePermissions.Count == 0)
            {
                await LoadPermissionsCatalog();
            }

            using (var dlg = new Form())
            {
                dlg.Text = isNew ? "Thêm Vai Trò & Phân Quyền Mới" : $"Cấu Hình Phân Quyền - {model?.TenVaiTro} ({model?.MaVaiTro})";
                dlg.Size = new Size(680, 620);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = Color.White;
                dlg.Font = new Font("Segoe UI", 9.5F);

                // Header
                var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(20, 12, 20, 12) };
                var lblH1 = new Label { Text = isNew ? "TẠO VAI TRÒ MỚI" : "CHỈNH SỬA VAI TRÒ & PHÂN QUYỀN", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = ThemeManager.Ink, AutoSize = true, Location = new Point(18, 12) };
                var lblH2 = new Label { Text = "Tích chọn các chức năng cho phép vai trò này truy cập trên thanh điều hướng.", Font = new Font("Segoe UI", 8.5F), ForeColor = ThemeManager.Muted, AutoSize = true, Location = new Point(20, 36) };
                pnlHeader.Controls.Add(lblH1);
                pnlHeader.Controls.Add(lblH2);
                dlg.Controls.Add(pnlHeader);

                // Body
                var pnlBody = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(24, 16, 24, 16) };
                dlg.Controls.Add(pnlBody);
                pnlBody.BringToFront();

                // Mã vai trò
                var lblMa = new Label { Text = "Mã vai trò (VD: VT_KHO, VT_THUNGAN):", Location = new Point(24, 16), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
                var txtMa = new TextBox { Location = new Point(24, 38), Width = 280, Font = new Font("Segoe UI", 10F), Text = model?.MaVaiTro ?? "", ReadOnly = !isNew, BackColor = !isNew ? Color.FromArgb(241, 245, 249) : Color.White };
                pnlBody.Controls.Add(lblMa);
                pnlBody.Controls.Add(txtMa);

                // Tên vai trò
                var lblTen = new Label { Text = "Tên vai trò hiển thị:", Location = new Point(324, 16), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
                var txtTen = new TextBox { Location = new Point(324, 38), Width = 300, Font = new Font("Segoe UI", 10F), Text = model?.TenVaiTro ?? "" };
                pnlBody.Controls.Add(lblTen);
                pnlBody.Controls.Add(txtTen);

                // Quick buttons: Chọn tất cả / Bỏ chọn tất cả
                var lblPermTitle = new Label { Text = "Danh mục quyền theo phân hệ chức năng:", Location = new Point(24, 82), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = ThemeManager.Primary };
                pnlBody.Controls.Add(lblPermTitle);

                var btnSelectAll = new Button { Text = "✓ Chọn tất cả", Location = new Point(380, 78), Size = new Size(110, 26), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5F), Cursor = Cursors.Hand };
                var btnDeselectAll = new Button { Text = "✕ Bỏ chọn hết", Location = new Point(500, 78), Size = new Size(110, 26), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5F), Cursor = Cursors.Hand };
                pnlBody.Controls.Add(btnSelectAll);
                pnlBody.Controls.Add(btnDeselectAll);

                // Danh sách Checkbox theo từng nhóm
                var checkBoxes = new Dictionary<string, CheckBox>();
                var curPerms = new HashSet<string>(model?.Permissions ?? new List<string>(), StringComparer.OrdinalIgnoreCase);

                var groups = _availablePermissions.GroupBy(p => p.Group).ToList();
                int currentY = 114;

                foreach (var grp in groups)
                {
                    var pnlGrp = new Panel
                    {
                        Location = new Point(24, currentY),
                        Width = 600,
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.FromArgb(250, 250, 252)
                    };

                    var lblGrp = new Label
                    {
                        Text = "📁 " + grp.Key.ToUpper(),
                        Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                        ForeColor = ThemeManager.Ink,
                        Location = new Point(12, 10),
                        AutoSize = true
                    };
                    pnlGrp.Controls.Add(lblGrp);

                    int chkY = 36;
                    foreach (var p in grp)
                    {
                        var chk = new CheckBox
                        {
                            Text = $"{p.Name} ({p.Code}) - {p.Description}",
                            Tag = p.Code,
                            Font = new Font("Segoe UI", 9F),
                            Location = new Point(24, chkY),
                            Size = new Size(560, 24),
                            Checked = curPerms.Contains(p.Code) || curPerms.Contains("*") || curPerms.Contains("ALL"),
                            Cursor = Cursors.Hand
                        };
                        checkBoxes[p.Code] = chk;
                        pnlGrp.Controls.Add(chk);
                        chkY += 28;
                    }

                    pnlGrp.Height = chkY + 10;
                    pnlBody.Controls.Add(pnlGrp);
                    currentY += pnlGrp.Height + 14;
                }

                btnSelectAll.Click += (s, e) => { foreach (var cb in checkBoxes.Values) cb.Checked = true; };
                btnDeselectAll.Click += (s, e) => { foreach (var cb in checkBoxes.Values) cb.Checked = false; };

                // Footer
                var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(20, 10, 20, 10) };
                var btnSave = new Button { Text = "💾 Lưu vai trò & quyền", DialogResult = DialogResult.None, BackColor = ThemeManager.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Size = new Size(180, 36), Location = new Point(340, 10), Cursor = Cursors.Hand };
                var btnCancel = new Button { Text = "Hủy bỏ", DialogResult = DialogResult.Cancel, Size = new Size(90, 36), Location = new Point(530, 10), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
                pnlFooter.Controls.Add(btnSave);
                pnlFooter.Controls.Add(btnCancel);
                dlg.Controls.Add(pnlFooter);

                btnSave.Click += async (s, e) =>
                {
                    string ma = txtMa.Text.Trim();
                    string ten = txtTen.Text.Trim();
                    if (string.IsNullOrEmpty(ma) || string.IsNullOrEmpty(ten))
                    {
                        MessageBox.Show("Vui lòng điền đầy đủ mã và tên vai trò.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var selectedCodes = checkBoxes.Where(kv => kv.Value.Checked).Select(kv => kv.Key).ToList();
                    btnSave.Enabled = false;
                    try
                    {
                        var res = await _service.SaveAsync(ma, ten, selectedCodes, isNew);
                        if (res.Success)
                        {
                            MessageBox.Show("Lưu vai trò và phân quyền thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        btnSave.Enabled = true;
                    }
                };

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadData();
                }
            }
        }
    }
}

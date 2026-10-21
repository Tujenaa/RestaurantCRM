using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Forms;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.Backup
{
    public class BackupRestoreForm : Form
    {
        private readonly BackupService _backupService = new BackupService();
        private List<BackupItemModel> _backups = new List<BackupItemModel>();

        private Label _lblTotalBackups;
        private Label _lblTotalSize;
        private Label _lblLatestBackup;
        private DataGridView _dgvBackups;
        private Button _btnCreateBackup;
        private Button _btnRefresh;
        private Button _btnRestore;
        private Button _btnDownload;
        private Button _btnDelete;
        private Button _btnOpenFolder;
        private Panel _pnlLoading;

        public BackupRestoreForm()
        {
            Text = "Sao Lưu & Phục Hồi Dữ Liệu | RestaurantCRM";
            Size = new Size(1020, 680);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);

            BuildLayout();
            this.Load += async (s, e) => await LoadData();
        }

        private void BuildLayout()
        {
            // 1. Header Banner
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 74, BackColor = Color.White, Padding = new Padding(24, 12, 24, 12) };
            pnlTop.Paint += (s, e) =>
            {
                using (var pen = new Pen(ThemeManager.Line))
                    e.Graphics.DrawLine(pen, 0, pnlTop.Height - 1, pnlTop.Width, pnlTop.Height - 1);
            };

            var lblTitle = new Label
            {
                Text = "💾 SAO LƯU & PHỤC HỒI CƠ SỞ DỮ LIỆU",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(20, 12)
            };
            var lblSub = new Label
            {
                Text = "Tạo các bản sao lưu an toàn cho hệ thống và phục hồi dữ liệu khi có sự cố.",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = ThemeManager.Muted,
                AutoSize = true,
                Location = new Point(22, 42)
            };
            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(lblSub);
            Controls.Add(pnlTop);

            // 2. Main Content
            var pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 16, 24, 20), BackColor = ThemeManager.Bg };
            Controls.Add(pnlContent);
            pnlContent.BringToFront();

            // 2.1 KPI Cards (3 Cards)
            var pnlKpis = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 84,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 14)
            };
            pnlKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            pnlKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            pnlKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));

            pnlKpis.Controls.Add(CreateKpiCard("🕒", "BẢN SAO LƯU GẦN NHẤT", "Chưa có", out _lblLatestBackup, ThemeManager.Primary), 0, 0);
            pnlKpis.Controls.Add(CreateKpiCard("📁", "TỔNG SỐ BẢN SAO LƯU", "0 bản", out _lblTotalBackups, ThemeManager.Ink), 1, 0);
            pnlKpis.Controls.Add(CreateKpiCard("💾", "TỔNG DUNG LƯỢNG LƯU TRỮ", "0 MB", out _lblTotalSize, Color.FromArgb(5, 150, 105)), 2, 0);
            pnlContent.Controls.Add(pnlKpis);

            // 2.2 Table Card
            var pnlGridCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0)
            };
            pnlGridCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(ThemeManager.Line))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlGridCard.Width - 1, pnlGridCard.Height - 1);
            };
            pnlContent.Controls.Add(pnlGridCard);
            pnlGridCard.BringToFront();

            // Toolbar
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White, Padding = new Padding(14, 10, 14, 10) };
            toolbar.Paint += (s, e) =>
            {
                using (var pen = new Pen(ThemeManager.Line))
                    e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
            };

            _btnCreateBackup = new Button
            {
                Text = "＋ Tạo bản sao lưu ngay",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = ThemeManager.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(180, 34),
                Location = new Point(14, 11),
                Cursor = Cursors.Hand
            };
            _btnCreateBackup.FlatAppearance.BorderSize = 0;
            _btnCreateBackup.Click += async (s, e) => await OnCreateBackup();
            toolbar.Controls.Add(_btnCreateBackup);

            _btnRefresh = new Button
            {
                Text = "🔄 Làm mới",
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(95, 34),
                Location = new Point(202, 11),
                Cursor = Cursors.Hand
            };
            _btnRefresh.Click += async (s, e) => await LoadData();
            toolbar.Controls.Add(_btnRefresh);

            _btnOpenFolder = new Button
            {
                Text = "📂 Mở thư mục",
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(115, 34),
                Location = new Point(305, 11),
                Cursor = Cursors.Hand
            };
            _btnOpenFolder.Click += (s, e) => OpenBackupFolder();
            toolbar.Controls.Add(_btnOpenFolder);

            // Action buttons on the right
            _btnDelete = new Button
            {
                Text = "🗑 Xóa",
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeManager.Red,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(toolbar.Width - 94, 11),
                Cursor = Cursors.Hand
            };
            _btnDelete.FlatAppearance.BorderColor = Color.FromArgb(254, 202, 202);
            _btnDelete.Click += async (s, e) => await OnDeleteBackup();
            toolbar.Controls.Add(_btnDelete);

            _btnDownload = new Button
            {
                Text = "⬇ Tải file .bak",
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(115, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(_btnDelete.Left - 123, 11),
                Cursor = Cursors.Hand
            };
            _btnDownload.Click += async (s, e) => await OnDownloadBackup();
            toolbar.Controls.Add(_btnDownload);

            _btnRestore = new Button
            {
                Text = "♻ Phục hồi CSDL (Restore)",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(254, 242, 242),
                ForeColor = Color.FromArgb(220, 38, 38),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(190, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(_btnDownload.Left - 198, 11),
                Cursor = Cursors.Hand
            };
            _btnRestore.FlatAppearance.BorderColor = Color.FromArgb(254, 202, 202);
            _btnRestore.Click += async (s, e) => await OnRestoreBackup();
            toolbar.Controls.Add(_btnRestore);

            pnlGridCard.Controls.Add(toolbar);

            // DataGridView
            _dgvBackups = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(241, 245, 249),
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 42 },
                Font = new Font("Segoe UI", 9.5F)
            };
            _dgvBackups.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            _dgvBackups.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.Ink;
            _dgvBackups.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _dgvBackups.ColumnHeadersHeight = 44;
            _dgvBackups.EnableHeadersVisualStyles = false;

            _dgvBackups.Columns.Add("FileName", "Tên File Bản Sao Lưu");
            _dgvBackups.Columns["FileName"].FillWeight = 40;
            _dgvBackups.Columns.Add("CreatedAt", "Ngày Giờ Tạo");
            _dgvBackups.Columns["CreatedAt"].FillWeight = 22;
            _dgvBackups.Columns.Add("FileSize", "Dung Lượng");
            _dgvBackups.Columns["FileSize"].FillWeight = 16;
            _dgvBackups.Columns.Add("Note", "Ghi Chú");
            _dgvBackups.Columns["Note"].FillWeight = 22;

            pnlGridCard.Controls.Add(_dgvBackups);
            _dgvBackups.BringToFront();

            // Loading overlay
            _pnlLoading = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(160, 255, 255, 255),
                Visible = false
            };
            var lblLoading = new Label
            {
                Text = "⏳ Đang thực hiện tác vụ CSDL, vui lòng đợi...",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeManager.Primary,
                AutoSize = true
            };
            _pnlLoading.Controls.Add(lblLoading);
            _pnlLoading.Resize += (s, e) =>
            {
                lblLoading.Location = new Point((_pnlLoading.Width - lblLoading.Width) / 2, (_pnlLoading.Height - lblLoading.Height) / 2);
            };
            pnlGridCard.Controls.Add(_pnlLoading);
            _pnlLoading.BringToFront();
        }

        private Panel CreateKpiCard(string icon, string title, string val, out Label lblVal, Color valColor)
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 12),
                Margin = new Padding(4, 0, 4, 0)
            };
            pnl.Paint += (s, e) =>
            {
                using (var pen = new Pen(ThemeManager.Line))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
            };

            var lblIco = new Label { Text = icon, Font = new Font("Segoe UI Emoji", 14F), Location = new Point(14, 14), AutoSize = true };
            var lblT = new Label { Text = title, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), ForeColor = ThemeManager.Muted, Location = new Point(50, 14), AutoSize = true };
            var lblV = new Label { Text = val, Font = new Font("Segoe UI", 12.5F, FontStyle.Bold), ForeColor = valColor, Location = new Point(50, 32), AutoSize = true };

            pnl.Controls.Add(lblIco);
            pnl.Controls.Add(lblT);
            pnl.Controls.Add(lblV);
            lblVal = lblV;
            return pnl;
        }

        public async Task LoadData()
        {
            try
            {
                _backups = await _backupService.GetAllAsync();
                _dgvBackups.Rows.Clear();

                long totalBytes = 0;
                foreach (var b in _backups)
                {
                    totalBytes += b.FileSizeBytes;
                    _dgvBackups.Rows.Add(b.FileName, b.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"), b.FileSizeFormatted, b.Note);
                }

                _lblTotalBackups.Text = _backups.Count + " bản sao lưu";
                _lblTotalSize.Text = FormatBytes(totalBytes);
                if (_backups.Count > 0)
                {
                    _lblLatestBackup.Text = _backups[0].CreatedAt.ToString("dd/MM/yyyy HH:mm");
                }
                else
                {
                    _lblLatestBackup.Text = "Chưa có bản sao lưu";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi nạp dữ liệu sao lưu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task OnCreateBackup()
        {
            using (var prompt = new Form())
            {
                prompt.Text = "Tạo Bản Sao Lưu CSDL";
                prompt.Size = new Size(420, 220);
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.MaximizeBox = false;
                prompt.MinimizeBox = false;
                prompt.BackColor = Color.White;

                var lbl = new Label { Text = "Nhập ghi chú sao lưu (tùy chọn):", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
                var txt = new TextBox { Location = new Point(20, 50), Width = 360, Font = new Font("Segoe UI", 9.5F), Text = "Sao lưu thủ công định kỳ" };

                var btnOk = new Button { Text = "Bắt đầu sao lưu", Location = new Point(180, 110), Size = new Size(120, 34), BackColor = ThemeManager.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK };
                var btnCancel = new Button { Text = "Hủy", Location = new Point(310, 110), Size = new Size(70, 34), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel };

                prompt.Controls.Add(lbl);
                prompt.Controls.Add(txt);
                prompt.Controls.Add(btnOk);
                prompt.Controls.Add(btnCancel);

                if (prompt.ShowDialog(this) == DialogResult.OK)
                {
                    ShowLoading(true);
                    try
                    {
                        var res = await _backupService.CreateBackupAsync(txt.Text.Trim());
                        if (res.Success)
                        {
                            MessageBox.Show(res.Message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadData();
                        }
                        else
                        {
                            MessageBox.Show(res.Message, "Thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    finally
                    {
                        ShowLoading(false);
                    }
                }
            }
        }

        private async Task OnRestoreBackup()
        {
            var fileName = GetSelectedFileName();
            if (string.IsNullOrEmpty(fileName))
            {
                MessageBox.Show("Vui lòng chọn một bản sao lưu từ danh sách để phục hồi.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmMsg = $"⚠️ CẢNH BÁO QUAN TRỌNG:\n\nBạn đang chuẩn bị phục hồi cơ sở dữ liệu về bản sao lưu:\n'{fileName}'\n\nToàn bộ dữ liệu sau thời điểm sao lưu này sẽ bị ghi đè! Hành động này không thể hoàn tác.\n\nBạn có chắc chắn muốn thực hiện phục hồi ngay bây giờ?";
            if (MessageBox.Show(confirmMsg, "Xác nhận phục hồi CSDL", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                ShowLoading(true);
                try
                {
                    var res = await _backupService.RestoreBackupAsync(fileName);
                    if (res.Success)
                    {
                        MessageBox.Show(res.Message + "\n\nỨng dụng sẽ tự động tải lại dữ liệu phiên làm việc.", "Phục hồi thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(res.Message, "Lỗi phục hồi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                finally
                {
                    ShowLoading(false);
                }
            }
        }

        private async Task OnDownloadBackup()
        {
            var fileName = GetSelectedFileName();
            if (string.IsNullOrEmpty(fileName))
            {
                MessageBox.Show("Vui lòng chọn bản sao lưu cần tải về.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.FileName = fileName;
                sfd.Filter = "SQL Server Backup (*.bak)|*.bak|All Files (*.*)|*.*";
                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    ShowLoading(true);
                    try
                    {
                        var res = await _backupService.DownloadBackupAsync(fileName, sfd.FileName);
                        if (res.Success)
                        {
                            MessageBox.Show($"Đã lưu bản sao lưu vào:\n{sfd.FileName}", "Tải về hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(res.Message, "Lỗi tải về", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    finally
                    {
                        ShowLoading(false);
                    }
                }
            }
        }

        private async Task OnDeleteBackup()
        {
            var fileName = GetSelectedFileName();
            if (string.IsNullOrEmpty(fileName))
            {
                MessageBox.Show("Vui lòng chọn bản sao lưu cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa bản sao lưu '{fileName}' khỏi máy chủ?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var res = await _backupService.DeleteBackupAsync(fileName);
                if (res.Success)
                {
                    MessageBox.Show(res.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadData();
                }
                else
                {
                    MessageBox.Show(res.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenBackupFolder()
        {
            try
            {
                if (_backups.Count > 0 && !string.IsNullOrEmpty(_backups[0].FilePath))
                {
                    var dir = Path.GetDirectoryName(_backups[0].FilePath);
                    if (Directory.Exists(dir))
                    {
                        Process.Start("explorer.exe", dir);
                        return;
                    }
                }
                MessageBox.Show("Thư mục sao lưu sẽ được mở khi có ít nhất một bản backup được tạo.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở thư mục: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetSelectedFileName()
        {
            if (_dgvBackups.SelectedRows.Count > 0)
            {
                return _dgvBackups.SelectedRows[0].Cells["FileName"].Value?.ToString();
            }
            return null;
        }

        private void ShowLoading(bool show)
        {
            _pnlLoading.Visible = show;
            _btnCreateBackup.Enabled = !show;
            _btnRestore.Enabled = !show;
            _btnRefresh.Enabled = !show;
            _btnDownload.Enabled = !show;
            _btnDelete.Enabled = !show;
            Cursor = show ? Cursors.WaitCursor : Cursors.Default;
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("F1") + " KB";
            if (bytes < 1024 * 1024 * 1024) return (bytes / (1024.0 * 1024)).ToString("F1") + " MB";
            return (bytes / (1024.0 * 1024 * 1024)).ToString("F2") + " GB";
        }
    }
}

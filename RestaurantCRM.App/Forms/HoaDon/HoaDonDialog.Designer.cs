namespace RestaurantCRM.AdminApp.Forms.HoaDon {
    partial class HoaDonDialog {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblStatusBadge;
        private System.Windows.Forms.Panel pnlThongTin;
        private System.Windows.Forms.Label lblKhachHang;
        private System.Windows.Forms.Label lblDiaChi;
        private System.Windows.Forms.Label lblNgayDat;
        private System.Windows.Forms.Label lblNhanVien;
        private System.Windows.Forms.Label lblKhuyenMai;
        private System.Windows.Forms.Label lblTienHang;
        private System.Windows.Forms.Label lblTienGiam;
        private System.Windows.Forms.Label lblTongThanhToan;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabMonAn;
        private System.Windows.Forms.TabPage tabLichSu;
        private System.Windows.Forms.DataGridView dgvChiTiet;
        private System.Windows.Forms.DataGridView dgvLichSu;
        private System.Windows.Forms.Panel pnlCapNhat;
        private System.Windows.Forms.Label lblChonTrangThai;
        private System.Windows.Forms.ComboBox cboCapNhatTrangThai;
        private System.Windows.Forms.Label lblGhiChu;
        private System.Windows.Forms.TextBox txtGhiChu;
        private System.Windows.Forms.Button btnCapNhat;
        private System.Windows.Forms.Button btnDong;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            System.Windows.Forms.DataGridViewCellStyle dgvHeaderStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dgvHeaderStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblStatusBadge = new System.Windows.Forms.Label();
            this.pnlThongTin = new System.Windows.Forms.Panel();
            this.lblTongThanhToan = new System.Windows.Forms.Label();
            this.lblTienGiam = new System.Windows.Forms.Label();
            this.lblTienHang = new System.Windows.Forms.Label();
            this.lblKhuyenMai = new System.Windows.Forms.Label();
            this.lblNhanVien = new System.Windows.Forms.Label();
            this.lblNgayDat = new System.Windows.Forms.Label();
            this.lblDiaChi = new System.Windows.Forms.Label();
            this.lblKhachHang = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabMonAn = new System.Windows.Forms.TabPage();
            this.dgvChiTiet = new System.Windows.Forms.DataGridView();
            this.tabLichSu = new System.Windows.Forms.TabPage();
            this.dgvLichSu = new System.Windows.Forms.DataGridView();
            this.pnlCapNhat = new System.Windows.Forms.Panel();
            this.btnDong = new System.Windows.Forms.Button();
            this.btnCapNhat = new System.Windows.Forms.Button();
            this.txtGhiChu = new System.Windows.Forms.TextBox();
            this.lblGhiChu = new System.Windows.Forms.Label();
            this.cboCapNhatTrangThai = new System.Windows.Forms.ComboBox();
            this.lblChonTrangThai = new System.Windows.Forms.Label();
            this.pnlThongTin.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabMonAn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChiTiet)).BeginInit();
            this.tabLichSu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLichSu)).BeginInit();
            this.pnlCapNhat.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(36)))), ((int)(((byte)(43)))));
            this.lblTitle.Location = new System.Drawing.Point(16, 14);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(182, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Chi Tiết Đơn Hàng #";
            // 
            // lblStatusBadge
            // 
            this.lblStatusBadge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatusBadge.AutoSize = true;
            this.lblStatusBadge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(231)))), ((int)(((byte)(255)))));
            this.lblStatusBadge.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblStatusBadge.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.lblStatusBadge.Location = new System.Drawing.Point(740, 16);
            this.lblStatusBadge.Name = "lblStatusBadge";
            this.lblStatusBadge.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.lblStatusBadge.Size = new System.Drawing.Size(116, 27);
            this.lblStatusBadge.TabIndex = 1;
            this.lblStatusBadge.Text = "Đang chuẩn bị";
            this.lblStatusBadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlThongTin
            // 
            this.pnlThongTin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlThongTin.BackColor = System.Drawing.Color.White;
            this.pnlThongTin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlThongTin.Controls.Add(this.lblTongThanhToan);
            this.pnlThongTin.Controls.Add(this.lblTienGiam);
            this.pnlThongTin.Controls.Add(this.lblTienHang);
            this.pnlThongTin.Controls.Add(this.lblKhuyenMai);
            this.pnlThongTin.Controls.Add(this.lblNhanVien);
            this.pnlThongTin.Controls.Add(this.lblNgayDat);
            this.pnlThongTin.Controls.Add(this.lblDiaChi);
            this.pnlThongTin.Controls.Add(this.lblKhachHang);
            this.pnlThongTin.Location = new System.Drawing.Point(18, 52);
            this.pnlThongTin.Name = "pnlThongTin";
            this.pnlThongTin.Size = new System.Drawing.Size(890, 120);
            this.pnlThongTin.TabIndex = 2;
            // 
            // lblTongThanhToan
            // 
            this.lblTongThanhToan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTongThanhToan.AutoSize = true;
            this.lblTongThanhToan.Font = new System.Drawing.Font("Segoe UI", 11.5F, System.Drawing.FontStyle.Bold);
            this.lblTongThanhToan.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.lblTongThanhToan.Location = new System.Drawing.Point(620, 85);
            this.lblTongThanhToan.Name = "lblTongThanhToan";
            this.lblTongThanhToan.Size = new System.Drawing.Size(161, 21);
            this.lblTongThanhToan.TabIndex = 7;
            this.lblTongThanhToan.Text = "Tổng thanh toán: 0 đ";
            // 
            // lblTienGiam
            // 
            this.lblTienGiam.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTienGiam.AutoSize = true;
            this.lblTienGiam.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblTienGiam.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(29)))), ((int)(((byte)(72)))));
            this.lblTienGiam.Location = new System.Drawing.Point(620, 50);
            this.lblTienGiam.Name = "lblTienGiam";
            this.lblTienGiam.Size = new System.Drawing.Size(126, 17);
            this.lblTienGiam.TabIndex = 6;
            this.lblTienGiam.Text = "Giảm voucher: -0 đ";
            // 
            // lblTienHang
            // 
            this.lblTienHang.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTienHang.AutoSize = true;
            this.lblTienHang.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblTienHang.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblTienHang.Location = new System.Drawing.Point(620, 16);
            this.lblTienHang.Name = "lblTienHang";
            this.lblTienHang.Size = new System.Drawing.Size(117, 17);
            this.lblTienHang.TabIndex = 5;
            this.lblTienHang.Text = "Tiền món ăn: 0 đ";
            // 
            // lblKhuyenMai
            // 
            this.lblKhuyenMai.AutoSize = true;
            this.lblKhuyenMai.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblKhuyenMai.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblKhuyenMai.Location = new System.Drawing.Point(340, 85);
            this.lblKhuyenMai.Name = "lblKhuyenMai";
            this.lblKhuyenMai.Size = new System.Drawing.Size(125, 17);
            this.lblKhuyenMai.TabIndex = 4;
            this.lblKhuyenMai.Text = "Khuyến mãi: Không";
            // 
            // lblNhanVien
            // 
            this.lblNhanVien.AutoSize = true;
            this.lblNhanVien.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblNhanVien.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblNhanVien.Location = new System.Drawing.Point(340, 16);
            this.lblNhanVien.Name = "lblNhanVien";
            this.lblNhanVien.Size = new System.Drawing.Size(149, 17);
            this.lblNhanVien.TabIndex = 3;
            this.lblNhanVien.Text = "Nhân viên: Chưa phân";
            // 
            // lblNgayDat
            // 
            this.lblNgayDat.AutoSize = true;
            this.lblNgayDat.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblNgayDat.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblNgayDat.Location = new System.Drawing.Point(14, 85);
            this.lblNgayDat.Name = "lblNgayDat";
            this.lblNgayDat.Size = new System.Drawing.Size(130, 17);
            this.lblNgayDat.TabIndex = 2;
            this.lblNgayDat.Text = "Thời gian đặt: -";
            // 
            // lblDiaChi
            // 
            this.lblDiaChi.AutoSize = true;
            this.lblDiaChi.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblDiaChi.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblDiaChi.Location = new System.Drawing.Point(14, 50);
            this.lblDiaChi.Name = "lblDiaChi";
            this.lblDiaChi.Size = new System.Drawing.Size(144, 17);
            this.lblDiaChi.TabIndex = 1;
            this.lblDiaChi.Text = "Địa chỉ: Tại nhà hàng";
            // 
            // lblKhachHang
            // 
            this.lblKhachHang.AutoSize = true;
            this.lblKhachHang.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblKhachHang.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblKhachHang.Location = new System.Drawing.Point(14, 16);
            this.lblKhachHang.Name = "lblKhachHang";
            this.lblKhachHang.Size = new System.Drawing.Size(183, 17);
            this.lblKhachHang.TabIndex = 0;
            this.lblKhachHang.Text = "Khách hàng: Khách vãng lai";
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabMonAn);
            this.tabControl.Controls.Add(this.tabLichSu);
            this.tabControl.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.tabControl.Location = new System.Drawing.Point(18, 185);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(890, 275);
            this.tabControl.TabIndex = 3;
            // 
            // tabMonAn
            // 
            this.tabMonAn.Controls.Add(this.dgvChiTiet);
            this.tabMonAn.Location = new System.Drawing.Point(4, 25);
            this.tabMonAn.Name = "tabMonAn";
            this.tabMonAn.Padding = new System.Windows.Forms.Padding(3);
            this.tabMonAn.Size = new System.Drawing.Size(882, 246);
            this.tabMonAn.TabIndex = 0;
            this.tabMonAn.Text = "Món ăn trong đơn";
            this.tabMonAn.UseVisualStyleBackColor = true;
            // 
            // dgvChiTiet
            // 
            this.dgvChiTiet.AllowUserToAddRows = false;
            this.dgvChiTiet.AllowUserToDeleteRows = false;
            this.dgvChiTiet.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvChiTiet.BackgroundColor = System.Drawing.Color.White;
            this.dgvChiTiet.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dgvHeaderStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dgvHeaderStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            dgvHeaderStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dgvHeaderStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.dgvChiTiet.ColumnHeadersDefaultCellStyle = dgvHeaderStyle1;
            this.dgvChiTiet.ColumnHeadersHeight = 32;
            this.dgvChiTiet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvChiTiet.EnableHeadersVisualStyles = false;
            this.dgvChiTiet.Location = new System.Drawing.Point(3, 3);
            this.dgvChiTiet.MultiSelect = false;
            this.dgvChiTiet.Name = "dgvChiTiet";
            this.dgvChiTiet.ReadOnly = true;
            this.dgvChiTiet.RowHeadersVisible = false;
            this.dgvChiTiet.RowTemplate.Height = 30;
            this.dgvChiTiet.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvChiTiet.Size = new System.Drawing.Size(876, 240);
            this.dgvChiTiet.TabIndex = 0;
            // 
            // tabLichSu
            // 
            this.tabLichSu.Controls.Add(this.dgvLichSu);
            this.tabLichSu.Location = new System.Drawing.Point(4, 25);
            this.tabLichSu.Name = "tabLichSu";
            this.tabLichSu.Padding = new System.Windows.Forms.Padding(3);
            this.tabLichSu.Size = new System.Drawing.Size(882, 246);
            this.tabLichSu.TabIndex = 1;
            this.tabLichSu.Text = "Lịch sử chuyển trạng thái";
            this.tabLichSu.UseVisualStyleBackColor = true;
            // 
            // dgvLichSu
            // 
            this.dgvLichSu.AllowUserToAddRows = false;
            this.dgvLichSu.AllowUserToDeleteRows = false;
            this.dgvLichSu.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvLichSu.BackgroundColor = System.Drawing.Color.White;
            this.dgvLichSu.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dgvHeaderStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dgvHeaderStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            dgvHeaderStyle2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dgvHeaderStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.dgvLichSu.ColumnHeadersDefaultCellStyle = dgvHeaderStyle2;
            this.dgvLichSu.ColumnHeadersHeight = 32;
            this.dgvLichSu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvLichSu.EnableHeadersVisualStyles = false;
            this.dgvLichSu.Location = new System.Drawing.Point(3, 3);
            this.dgvLichSu.MultiSelect = false;
            this.dgvLichSu.Name = "dgvLichSu";
            this.dgvLichSu.ReadOnly = true;
            this.dgvLichSu.RowHeadersVisible = false;
            this.dgvLichSu.RowTemplate.Height = 30;
            this.dgvLichSu.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLichSu.Size = new System.Drawing.Size(876, 240);
            this.dgvLichSu.TabIndex = 0;
            // 
            // pnlCapNhat
            // 
            this.pnlCapNhat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlCapNhat.BackColor = System.Drawing.Color.White;
            this.pnlCapNhat.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCapNhat.Controls.Add(this.btnDong);
            this.pnlCapNhat.Controls.Add(this.btnCapNhat);
            this.pnlCapNhat.Controls.Add(this.txtGhiChu);
            this.pnlCapNhat.Controls.Add(this.lblGhiChu);
            this.pnlCapNhat.Controls.Add(this.cboCapNhatTrangThai);
            this.pnlCapNhat.Controls.Add(this.lblChonTrangThai);
            this.pnlCapNhat.Location = new System.Drawing.Point(18, 470);
            this.pnlCapNhat.Name = "pnlCapNhat";
            this.pnlCapNhat.Size = new System.Drawing.Size(890, 70);
            this.pnlCapNhat.TabIndex = 4;
            // 
            // btnDong
            // 
            this.btnDong.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDong.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnDong.Location = new System.Drawing.Point(795, 18);
            this.btnDong.Name = "btnDong";
            this.btnDong.Size = new System.Drawing.Size(80, 32);
            this.btnDong.TabIndex = 5;
            this.btnDong.Text = "Đóng";
            this.btnDong.UseVisualStyleBackColor = true;
            this.btnDong.Click += new System.EventHandler(this.btnDong_Click);
            // 
            // btnCapNhat
            // 
            this.btnCapNhat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCapNhat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnCapNhat.FlatAppearance.BorderSize = 0;
            this.btnCapNhat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCapNhat.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnCapNhat.ForeColor = System.Drawing.Color.White;
            this.btnCapNhat.Location = new System.Drawing.Point(645, 18);
            this.btnCapNhat.Name = "btnCapNhat";
            this.btnCapNhat.Size = new System.Drawing.Size(140, 32);
            this.btnCapNhat.TabIndex = 4;
            this.btnCapNhat.Text = "Lưu Cập Nhật";
            this.btnCapNhat.UseVisualStyleBackColor = false;
            this.btnCapNhat.Click += new System.EventHandler(this.btnCapNhat_Click);
            // 
            // txtGhiChu
            // 
            this.txtGhiChu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGhiChu.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtGhiChu.Location = new System.Drawing.Point(340, 22);
            this.txtGhiChu.Name = "txtGhiChu";
            this.txtGhiChu.Size = new System.Drawing.Size(285, 24);
            this.txtGhiChu.TabIndex = 3;
            // 
            // lblGhiChu
            // 
            this.lblGhiChu.AutoSize = true;
            this.lblGhiChu.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblGhiChu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblGhiChu.Location = new System.Drawing.Point(285, 26);
            this.lblGhiChu.Name = "lblGhiChu";
            this.lblGhiChu.Size = new System.Drawing.Size(51, 15);
            this.lblGhiChu.TabIndex = 2;
            this.lblGhiChu.Text = "Ghi chú:";
            // 
            // cboCapNhatTrangThai
            // 
            this.cboCapNhatTrangThai.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCapNhatTrangThai.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboCapNhatTrangThai.FormattingEnabled = true;
            this.cboCapNhatTrangThai.Location = new System.Drawing.Point(105, 22);
            this.cboCapNhatTrangThai.Name = "cboCapNhatTrangThai";
            this.cboCapNhatTrangThai.Size = new System.Drawing.Size(165, 25);
            this.cboCapNhatTrangThai.TabIndex = 1;
            // 
            // lblChonTrangThai
            // 
            this.lblChonTrangThai.AutoSize = true;
            this.lblChonTrangThai.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblChonTrangThai.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblChonTrangThai.Location = new System.Drawing.Point(14, 26);
            this.lblChonTrangThai.Name = "lblChonTrangThai";
            this.lblChonTrangThai.Size = new System.Drawing.Size(89, 15);
            this.lblChonTrangThai.TabIndex = 0;
            this.lblChonTrangThai.Text = "Trạng thái mới:";
            // 
            // HoaDonDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(926, 555);
            this.Controls.Add(this.pnlCapNhat);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.pnlThongTin);
            this.Controls.Add(this.lblStatusBadge);
            this.Controls.Add(this.lblTitle);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HoaDonDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chi Tiết Đơn Hàng & Lịch Sử Trạng Thái";
            this.Load += new System.EventHandler(this.HoaDonDialog_Load);
            this.pnlThongTin.ResumeLayout(false);
            this.pnlThongTin.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabMonAn.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvChiTiet)).EndInit();
            this.tabLichSu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLichSu)).EndInit();
            this.pnlCapNhat.ResumeLayout(false);
            this.pnlCapNhat.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}

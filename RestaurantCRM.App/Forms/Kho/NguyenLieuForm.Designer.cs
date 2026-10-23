namespace RestaurantCRM.AdminApp.Forms.Kho {
    partial class NguyenLieuForm {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTieuDe;
        private System.Windows.Forms.Label lblPhuDe;
        private System.Windows.Forms.Panel pnlThongKe;
        private System.Windows.Forms.Label lblTongSo;
        private System.Windows.Forms.Label lblDangNhap;
        private System.Windows.Forms.Label lblNgungNhap;
        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.Label lblTimKiem;
        private System.Windows.Forms.TextBox txtTimKiem;
        private System.Windows.Forms.Label lblBoLoc;
        private System.Windows.Forms.ComboBox cboBoLoc;
        private System.Windows.Forms.Button btnLamMoi;
        private System.Windows.Forms.Button btnThem;
        private System.Windows.Forms.Button btnSua;
        private System.Windows.Forms.Button btnNgungNhap;
        private System.Windows.Forms.DataGridView dgvNguyenLieu;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMaNL;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTenNL;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDVT;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGiaNhap;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSoLuongTon;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTrangThai;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTieuDe = new System.Windows.Forms.Label();
            this.lblPhuDe = new System.Windows.Forms.Label();
            this.pnlThongKe = new System.Windows.Forms.Panel();
            this.lblTongSo = new System.Windows.Forms.Label();
            this.lblDangNhap = new System.Windows.Forms.Label();
            this.lblNgungNhap = new System.Windows.Forms.Label();
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.lblTimKiem = new System.Windows.Forms.Label();
            this.txtTimKiem = new System.Windows.Forms.TextBox();
            this.lblBoLoc = new System.Windows.Forms.Label();
            this.cboBoLoc = new System.Windows.Forms.ComboBox();
            this.btnLamMoi = new System.Windows.Forms.Button();
            this.btnThem = new System.Windows.Forms.Button();
            this.btnSua = new System.Windows.Forms.Button();
            this.btnNgungNhap = new System.Windows.Forms.Button();
            this.dgvNguyenLieu = new System.Windows.Forms.DataGridView();
            this.colMaNL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTenNL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDVT = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGiaNhap = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSoLuongTon = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTrangThai = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlHeader.SuspendLayout();
            this.pnlThongKe.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNguyenLieu)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.White;
            this.pnlHeader.Controls.Add(this.lblTieuDe);
            this.pnlHeader.Controls.Add(this.lblPhuDe);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(20, 14, 20, 10);
            this.pnlHeader.Size = new System.Drawing.Size(1008, 64);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblTieuDe
            // 
            this.lblTieuDe.AutoSize = true;
            this.lblTieuDe.Font = new System.Drawing.Font("Segoe UI", 13.5F, System.Drawing.FontStyle.Bold);
            this.lblTieuDe.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblTieuDe.Location = new System.Drawing.Point(16, 10);
            this.lblTieuDe.Name = "lblTieuDe";
            this.lblTieuDe.Size = new System.Drawing.Size(342, 25);
            this.lblTieuDe.TabIndex = 0;
            this.lblTieuDe.Text = "QUẢN LÝ DANH MỤC NGUYÊN VẬT LIỆU";
            // 
            // lblPhuDe
            // 
            this.lblPhuDe.AutoSize = true;
            this.lblPhuDe.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblPhuDe.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblPhuDe.Location = new System.Drawing.Point(18, 37);
            this.lblPhuDe.Name = "lblPhuDe";
            this.lblPhuDe.Size = new System.Drawing.Size(534, 15);
            this.lblPhuDe.TabIndex = 1;
            this.lblPhuDe.Text = "Quản lý thông tin nguyên vật liệu, giá nhập tham khảo và kiểm soát trạng thái nhập hàng an toàn";
            // 
            // pnlThongKe
            // 
            this.pnlThongKe.BackColor = System.Drawing.Color.White;
            this.pnlThongKe.Controls.Add(this.lblTongSo);
            this.pnlThongKe.Controls.Add(this.lblDangNhap);
            this.pnlThongKe.Controls.Add(this.lblNgungNhap);
            this.pnlThongKe.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlThongKe.Location = new System.Drawing.Point(0, 64);
            this.pnlThongKe.Name = "pnlThongKe";
            this.pnlThongKe.Padding = new System.Windows.Forms.Padding(20, 0, 20, 8);
            this.pnlThongKe.Size = new System.Drawing.Size(1008, 36);
            this.pnlThongKe.TabIndex = 1;
            // 
            // lblTongSo
            // 
            this.lblTongSo.AutoSize = true;
            this.lblTongSo.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblTongSo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblTongSo.Location = new System.Drawing.Point(18, 6);
            this.lblTongSo.Name = "lblTongSo";
            this.lblTongSo.Size = new System.Drawing.Size(146, 17);
            this.lblTongSo.TabIndex = 0;
            this.lblTongSo.Text = "Tổng số: 0 nguyên liệu";
            // 
            // lblDangNhap
            // 
            this.lblDangNhap.AutoSize = true;
            this.lblDangNhap.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblDangNhap.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(163)))), ((int)(((byte)(74)))));
            this.lblDangNhap.Location = new System.Drawing.Point(210, 6);
            this.lblDangNhap.Name = "lblDangNhap";
            this.lblDangNhap.Size = new System.Drawing.Size(147, 17);
            this.lblDangNhap.TabIndex = 1;
            this.lblDangNhap.Text = "🟢 Đang nhập: 0 loại";
            // 
            // lblNgungNhap
            // 
            this.lblNgungNhap.AutoSize = true;
            this.lblNgungNhap.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblNgungNhap.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.lblNgungNhap.Location = new System.Drawing.Point(395, 6);
            this.lblNgungNhap.Name = "lblNgungNhap";
            this.lblNgungNhap.Size = new System.Drawing.Size(155, 17);
            this.lblNgungNhap.TabIndex = 2;
            this.lblNgungNhap.Text = "⚪ Ngừng nhập: 0 loại";
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.pnlToolbar.Controls.Add(this.lblTimKiem);
            this.pnlToolbar.Controls.Add(this.txtTimKiem);
            this.pnlToolbar.Controls.Add(this.lblBoLoc);
            this.pnlToolbar.Controls.Add(this.cboBoLoc);
            this.pnlToolbar.Controls.Add(this.btnLamMoi);
            this.pnlToolbar.Controls.Add(this.btnThem);
            this.pnlToolbar.Controls.Add(this.btnSua);
            this.pnlToolbar.Controls.Add(this.btnNgungNhap);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 100);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Size = new System.Drawing.Size(1008, 48);
            this.pnlToolbar.TabIndex = 2;
            // 
            // lblTimKiem
            // 
            this.lblTimKiem.AutoSize = true;
            this.lblTimKiem.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTimKiem.Location = new System.Drawing.Point(18, 16);
            this.lblTimKiem.Name = "lblTimKiem";
            this.lblTimKiem.Size = new System.Drawing.Size(59, 15);
            this.lblTimKiem.TabIndex = 0;
            this.lblTimKiem.Text = "Tìm kiếm:";
            // 
            // txtTimKiem
            // 
            this.txtTimKiem.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtTimKiem.Location = new System.Drawing.Point(82, 12);
            this.txtTimKiem.Name = "txtTimKiem";
            this.txtTimKiem.Size = new System.Drawing.Size(200, 23);
            this.txtTimKiem.TabIndex = 1;
            this.txtTimKiem.TextChanged += new System.EventHandler(this.txtTimKiem_TextChanged);
            // 
            // lblBoLoc
            // 
            this.lblBoLoc.AutoSize = true;
            this.lblBoLoc.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblBoLoc.Location = new System.Drawing.Point(298, 16);
            this.lblBoLoc.Name = "lblBoLoc";
            this.lblBoLoc.Size = new System.Drawing.Size(64, 15);
            this.lblBoLoc.TabIndex = 2;
            this.lblBoLoc.Text = "Trạng thái:";
            // 
            // cboBoLoc
            // 
            this.cboBoLoc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBoLoc.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboBoLoc.FormattingEnabled = true;
            this.cboBoLoc.Items.AddRange(new object[] {
            "Tất cả trạng thái",
            "🟢 Đang nhập (Active)",
            "⚪ Đã ngừng nhập (Inactive)"});
            this.cboBoLoc.Location = new System.Drawing.Point(368, 12);
            this.cboBoLoc.Name = "cboBoLoc";
            this.cboBoLoc.Size = new System.Drawing.Size(170, 23);
            this.cboBoLoc.TabIndex = 3;
            this.cboBoLoc.SelectedIndexChanged += new System.EventHandler(this.cboBoLoc_SelectedIndexChanged);
            // 
            // btnLamMoi
            // 
            this.btnLamMoi.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnLamMoi.Location = new System.Drawing.Point(546, 10);
            this.btnLamMoi.Name = "btnLamMoi";
            this.btnLamMoi.Size = new System.Drawing.Size(75, 27);
            this.btnLamMoi.TabIndex = 4;
            this.btnLamMoi.Text = "Làm mới";
            this.btnLamMoi.UseVisualStyleBackColor = true;
            this.btnLamMoi.Click += new System.EventHandler(this.btnLamMoi_Click);
            // 
            // btnThem
            // 
            this.btnThem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnThem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnThem.FlatAppearance.BorderSize = 0;
            this.btnThem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnThem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnThem.ForeColor = System.Drawing.Color.White;
            this.btnThem.Location = new System.Drawing.Point(628, 10);
            this.btnThem.Name = "btnThem";
            this.btnThem.Size = new System.Drawing.Size(120, 27);
            this.btnThem.TabIndex = 5;
            this.btnThem.Text = "＋ Thêm mới";
            this.btnThem.UseVisualStyleBackColor = false;
            this.btnThem.Click += new System.EventHandler(this.btnThem_Click);
            // 
            // btnSua
            // 
            this.btnSua.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSua.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(132)))), ((int)(((byte)(199)))));
            this.btnSua.FlatAppearance.BorderSize = 0;
            this.btnSua.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSua.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSua.ForeColor = System.Drawing.Color.White;
            this.btnSua.Location = new System.Drawing.Point(756, 10);
            this.btnSua.Name = "btnSua";
            this.btnSua.Size = new System.Drawing.Size(95, 27);
            this.btnSua.TabIndex = 6;
            this.btnSua.Text = "✎ Sửa";
            this.btnSua.UseVisualStyleBackColor = false;
            this.btnSua.Click += new System.EventHandler(this.btnSua_Click);
            // 
            // btnNgungNhap
            // 
            this.btnNgungNhap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNgungNhap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(119)))), ((int)(((byte)(6)))));
            this.btnNgungNhap.FlatAppearance.BorderSize = 0;
            this.btnNgungNhap.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNgungNhap.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnNgungNhap.ForeColor = System.Drawing.Color.White;
            this.btnNgungNhap.Location = new System.Drawing.Point(859, 10);
            this.btnNgungNhap.Name = "btnNgungNhap";
            this.btnNgungNhap.Size = new System.Drawing.Size(135, 27);
            this.btnNgungNhap.TabIndex = 7;
            this.btnNgungNhap.Text = "🚫 Ngừng nhập";
            this.btnNgungNhap.UseVisualStyleBackColor = false;
            this.btnNgungNhap.Click += new System.EventHandler(this.btnNgungNhap_Click);
            // 
            // dgvNguyenLieu
            // 
            this.dgvNguyenLieu.AllowUserToAddRows = false;
            this.dgvNguyenLieu.AllowUserToDeleteRows = false;
            this.dgvNguyenLieu.AllowUserToResizeRows = false;
            this.dgvNguyenLieu.BackgroundColor = System.Drawing.Color.White;
            this.dgvNguyenLieu.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvNguyenLieu.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvNguyenLieu.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            dataGridViewCellStyle1.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvNguyenLieu.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvNguyenLieu.ColumnHeadersHeight = 38;
            this.dgvNguyenLieu.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvNguyenLieu.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colMaNL,
            this.colTenNL,
            this.colDVT,
            this.colGiaNhap,
            this.colSoLuongTon,
            this.colTrangThai});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvNguyenLieu.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvNguyenLieu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvNguyenLieu.EnableHeadersVisualStyles = false;
            this.dgvNguyenLieu.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.dgvNguyenLieu.Location = new System.Drawing.Point(0, 148);
            this.dgvNguyenLieu.MultiSelect = false;
            this.dgvNguyenLieu.Name = "dgvNguyenLieu";
            this.dgvNguyenLieu.ReadOnly = true;
            this.dgvNguyenLieu.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvNguyenLieu.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvNguyenLieu.RowHeadersVisible = false;
            this.dgvNguyenLieu.RowTemplate.Height = 36;
            this.dgvNguyenLieu.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvNguyenLieu.Size = new System.Drawing.Size(1008, 492);
            this.dgvNguyenLieu.TabIndex = 3;
            this.dgvNguyenLieu.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvNguyenLieu_CellFormatting);
            this.dgvNguyenLieu.SelectionChanged += new System.EventHandler(this.dgvNguyenLieu_SelectionChanged);
            this.dgvNguyenLieu.DoubleClick += new System.EventHandler(this.dgvNguyenLieu_DoubleClick);
            // 
            // colMaNL
            // 
            this.colMaNL.DataPropertyName = "MaNguyenLieu";
            this.colMaNL.HeaderText = "Mã NL";
            this.colMaNL.MinimumWidth = 90;
            this.colMaNL.Name = "colMaNL";
            this.colMaNL.ReadOnly = true;
            this.colMaNL.Width = 100;
            // 
            // colTenNL
            // 
            this.colTenNL.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colTenNL.DataPropertyName = "TenNguyenLieu";
            this.colTenNL.HeaderText = "Tên Nguyên Vật Liệu";
            this.colTenNL.MinimumWidth = 180;
            this.colTenNL.Name = "colTenNL";
            this.colTenNL.ReadOnly = true;
            // 
            // colDVT
            // 
            this.colDVT.DataPropertyName = "DonViTinh";
            this.colDVT.HeaderText = "Đơn Vị Tính";
            this.colDVT.MinimumWidth = 90;
            this.colDVT.Name = "colDVT";
            this.colDVT.ReadOnly = true;
            this.colDVT.Width = 100;
            // 
            // colGiaNhap
            // 
            this.colGiaNhap.DataPropertyName = "GiaNhapGanNhat";
            this.colGiaNhap.HeaderText = "Giá Nhập Gần Nhất";
            this.colGiaNhap.MinimumWidth = 130;
            this.colGiaNhap.Name = "colGiaNhap";
            this.colGiaNhap.ReadOnly = true;
            this.colGiaNhap.Width = 140;
            // 
            // colSoLuongTon
            // 
            this.colSoLuongTon.DataPropertyName = "SoLuongTon";
            this.colSoLuongTon.HeaderText = "Tồn Kho Hiện Tại";
            this.colSoLuongTon.MinimumWidth = 120;
            this.colSoLuongTon.Name = "colSoLuongTon";
            this.colSoLuongTon.ReadOnly = true;
            this.colSoLuongTon.Width = 130;
            // 
            // colTrangThai
            // 
            this.colTrangThai.DataPropertyName = "TrangThaiHienThi";
            this.colTrangThai.HeaderText = "Trạng Thái Nhập";
            this.colTrangThai.MinimumWidth = 140;
            this.colTrangThai.Name = "colTrangThai";
            this.colTrangThai.ReadOnly = true;
            this.colTrangThai.Width = 150;
            // 
            // NguyenLieuForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(1008, 640);
            this.Controls.Add(this.dgvNguyenLieu);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlThongKe);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(920, 500);
            this.Name = "NguyenLieuForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Quản Lý Danh Mục Nguyên Vật Liệu";
            this.Load += new System.EventHandler(this.NguyenLieuForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlThongKe.ResumeLayout(false);
            this.pnlThongKe.PerformLayout();
            this.pnlToolbar.ResumeLayout(false);
            this.pnlToolbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNguyenLieu)).EndInit();
            this.ResumeLayout(false);

        }
    }
}

namespace RestaurantCRM.AdminApp.Forms.MonAn {
    partial class LoaiMonForm {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTieuDe;
        private System.Windows.Forms.Label lblSub;
        private System.Windows.Forms.Label lblTimKiem;
        private System.Windows.Forms.TextBox txtTimKiem;
        private System.Windows.Forms.Button btnLamMoi;
        private System.Windows.Forms.DataGridView dgvLoaiMon;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMaLoaiMon;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTenLoaiMon;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSoLuongMon;
        private System.Windows.Forms.GroupBox grpChiTiet;
        private System.Windows.Forms.Label lblMaLoai;
        private System.Windows.Forms.TextBox txtMaLoaiMon;
        private System.Windows.Forms.Label lblTenLoai;
        private System.Windows.Forms.TextBox txtTenLoaiMon;
        private System.Windows.Forms.Label lblThongTinRangBuoc;
        private System.Windows.Forms.Button btnThem;
        private System.Windows.Forms.Button btnSua;
        private System.Windows.Forms.Button btnXoa;
        private System.Windows.Forms.Button btnNhapLai;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            this.lblTieuDe = new System.Windows.Forms.Label();
            this.lblSub = new System.Windows.Forms.Label();
            this.lblTimKiem = new System.Windows.Forms.Label();
            this.txtTimKiem = new System.Windows.Forms.TextBox();
            this.btnLamMoi = new System.Windows.Forms.Button();
            this.dgvLoaiMon = new System.Windows.Forms.DataGridView();
            this.colMaLoaiMon = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTenLoaiMon = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSoLuongMon = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grpChiTiet = new System.Windows.Forms.GroupBox();
            this.lblMaLoai = new System.Windows.Forms.Label();
            this.txtMaLoaiMon = new System.Windows.Forms.TextBox();
            this.lblTenLoai = new System.Windows.Forms.Label();
            this.txtTenLoaiMon = new System.Windows.Forms.TextBox();
            this.lblThongTinRangBuoc = new System.Windows.Forms.Label();
            this.btnThem = new System.Windows.Forms.Button();
            this.btnSua = new System.Windows.Forms.Button();
            this.btnXoa = new System.Windows.Forms.Button();
            this.btnNhapLai = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLoaiMon)).BeginInit();
            this.grpChiTiet.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTieuDe
            // 
            this.lblTieuDe.AutoSize = true;
            this.lblTieuDe.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTieuDe.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(36)))), ((int)(((byte)(43)))));
            this.lblTieuDe.Location = new System.Drawing.Point(14, 14);
            this.lblTieuDe.Name = "lblTieuDe";
            this.lblTieuDe.Size = new System.Drawing.Size(206, 25);
            this.lblTieuDe.TabIndex = 0;
            this.lblTieuDe.Text = "Quản Lý Loại Món Ăn";
            // 
            // lblSub
            // 
            this.lblSub.AutoSize = true;
            this.lblSub.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSub.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblSub.Location = new System.Drawing.Point(16, 42);
            this.lblSub.Name = "lblSub";
            this.lblSub.Size = new System.Drawing.Size(347, 15);
            this.lblSub.TabIndex = 1;
            this.lblSub.Text = "Quản lý danh mục các loại món và kiểm tra ràng buộc khi xóa.";
            // 
            // lblTimKiem
            // 
            this.lblTimKiem.AutoSize = true;
            this.lblTimKiem.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblTimKiem.Location = new System.Drawing.Point(16, 75);
            this.lblTimKiem.Name = "lblTimKiem";
            this.lblTimKiem.Size = new System.Drawing.Size(63, 17);
            this.lblTimKiem.TabIndex = 2;
            this.lblTimKiem.Text = "Tìm kiếm:";
            // 
            // txtTimKiem
            // 
            this.txtTimKiem.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtTimKiem.Location = new System.Drawing.Point(83, 72);
            this.txtTimKiem.Name = "txtTimKiem";
            this.txtTimKiem.Size = new System.Drawing.Size(240, 24);
            this.txtTimKiem.TabIndex = 3;
            this.txtTimKiem.TextChanged += new System.EventHandler(this.txtTimKiem_TextChanged);
            // 
            // btnLamMoi
            // 
            this.btnLamMoi.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnLamMoi.Location = new System.Drawing.Point(330, 70);
            this.btnLamMoi.Name = "btnLamMoi";
            this.btnLamMoi.Size = new System.Drawing.Size(80, 28);
            this.btnLamMoi.TabIndex = 4;
            this.btnLamMoi.Text = "Làm mới";
            this.btnLamMoi.UseVisualStyleBackColor = true;
            this.btnLamMoi.Click += new System.EventHandler(this.btnLamMoi_Click);
            // 
            // dgvLoaiMon
            // 
            this.dgvLoaiMon.AllowUserToAddRows = false;
            this.dgvLoaiMon.AllowUserToDeleteRows = false;
            this.dgvLoaiMon.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvLoaiMon.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvLoaiMon.BackgroundColor = System.Drawing.Color.White;
            this.dgvLoaiMon.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLoaiMon.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colMaLoaiMon,
            this.colTenLoaiMon,
            this.colSoLuongMon});
            this.dgvLoaiMon.Location = new System.Drawing.Point(18, 107);
            this.dgvLoaiMon.MultiSelect = false;
            this.dgvLoaiMon.Name = "dgvLoaiMon";
            this.dgvLoaiMon.ReadOnly = true;
            this.dgvLoaiMon.RowHeadersVisible = false;
            this.dgvLoaiMon.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLoaiMon.Size = new System.Drawing.Size(462, 380);
            this.dgvLoaiMon.TabIndex = 5;
            this.dgvLoaiMon.SelectionChanged += new System.EventHandler(this.dgvLoaiMon_SelectionChanged);
            // 
            // colMaLoaiMon
            // 
            this.colMaLoaiMon.DataPropertyName = "MaLoaiMon";
            this.colMaLoaiMon.FillWeight = 60F;
            this.colMaLoaiMon.HeaderText = "Mã Loại";
            this.colMaLoaiMon.MinimumWidth = 80;
            this.colMaLoaiMon.Name = "colMaLoaiMon";
            this.colMaLoaiMon.ReadOnly = true;
            // 
            // colTenLoaiMon
            // 
            this.colTenLoaiMon.DataPropertyName = "TenLoaiMon";
            this.colTenLoaiMon.FillWeight = 110F;
            this.colTenLoaiMon.HeaderText = "Tên Loại Món";
            this.colTenLoaiMon.MinimumWidth = 130;
            this.colTenLoaiMon.Name = "colTenLoaiMon";
            this.colTenLoaiMon.ReadOnly = true;
            // 
            // colSoLuongMon
            // 
            this.colSoLuongMon.DataPropertyName = "SoLuongMon";
            this.colSoLuongMon.FillWeight = 70F;
            this.colSoLuongMon.HeaderText = "Số Món Đang Có";
            this.colSoLuongMon.MinimumWidth = 90;
            this.colSoLuongMon.Name = "colSoLuongMon";
            this.colSoLuongMon.ReadOnly = true;
            // 
            // grpChiTiet
            // 
            this.grpChiTiet.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpChiTiet.Controls.Add(this.btnNhapLai);
            this.grpChiTiet.Controls.Add(this.btnXoa);
            this.grpChiTiet.Controls.Add(this.btnSua);
            this.grpChiTiet.Controls.Add(this.btnThem);
            this.grpChiTiet.Controls.Add(this.lblThongTinRangBuoc);
            this.grpChiTiet.Controls.Add(this.txtTenLoaiMon);
            this.grpChiTiet.Controls.Add(this.lblTenLoai);
            this.grpChiTiet.Controls.Add(this.txtMaLoaiMon);
            this.grpChiTiet.Controls.Add(this.lblMaLoai);
            this.grpChiTiet.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.grpChiTiet.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.grpChiTiet.Location = new System.Drawing.Point(495, 72);
            this.grpChiTiet.Name = "grpChiTiet";
            this.grpChiTiet.Size = new System.Drawing.Size(325, 415);
            this.grpChiTiet.TabIndex = 6;
            this.grpChiTiet.TabStop = false;
            this.grpChiTiet.Text = "Thông Tin Loại Món";
            // 
            // lblMaLoai
            // 
            this.lblMaLoai.AutoSize = true;
            this.lblMaLoai.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMaLoai.Location = new System.Drawing.Point(18, 38);
            this.lblMaLoai.Name = "lblMaLoai";
            this.lblMaLoai.Size = new System.Drawing.Size(78, 15);
            this.lblMaLoai.TabIndex = 0;
            this.lblMaLoai.Text = "Mã loại món:";
            // 
            // txtMaLoaiMon
            // 
            this.txtMaLoaiMon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.txtMaLoaiMon.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtMaLoaiMon.Location = new System.Drawing.Point(21, 58);
            this.txtMaLoaiMon.Name = "txtMaLoaiMon";
            this.txtMaLoaiMon.ReadOnly = true;
            this.txtMaLoaiMon.Size = new System.Drawing.Size(286, 24);
            this.txtMaLoaiMon.TabIndex = 1;
            // 
            // lblTenLoai
            // 
            this.lblTenLoai.AutoSize = true;
            this.lblTenLoai.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTenLoai.Location = new System.Drawing.Point(18, 97);
            this.lblTenLoai.Name = "lblTenLoai";
            this.lblTenLoai.Size = new System.Drawing.Size(97, 15);
            this.lblTenLoai.TabIndex = 2;
            this.lblTenLoai.Text = "Tên loại món (*):";
            // 
            // txtTenLoaiMon
            // 
            this.txtTenLoaiMon.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtTenLoaiMon.Location = new System.Drawing.Point(21, 117);
            this.txtTenLoaiMon.Name = "txtTenLoaiMon";
            this.txtTenLoaiMon.Size = new System.Drawing.Size(286, 24);
            this.txtTenLoaiMon.TabIndex = 3;
            // 
            // lblThongTinRangBuoc
            // 
            this.lblThongTinRangBuoc.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic);
            this.lblThongTinRangBuoc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(83)))), ((int)(((byte)(9)))));
            this.lblThongTinRangBuoc.Location = new System.Drawing.Point(18, 153);
            this.lblThongTinRangBuoc.Name = "lblThongTinRangBuoc";
            this.lblThongTinRangBuoc.Size = new System.Drawing.Size(289, 48);
            this.lblThongTinRangBuoc.TabIndex = 4;
            this.lblThongTinRangBuoc.Text = "⚠️ Ràng buộc dữ liệu: Không thể xóa loại món nếu đang có món ăn thuộc loại này.";
            // 
            // btnThem
            // 
            this.btnThem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnThem.FlatAppearance.BorderSize = 0;
            this.btnThem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnThem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnThem.ForeColor = System.Drawing.Color.White;
            this.btnThem.Location = new System.Drawing.Point(21, 215);
            this.btnThem.Name = "btnThem";
            this.btnThem.Size = new System.Drawing.Size(135, 34);
            this.btnThem.TabIndex = 5;
            this.btnThem.Text = "＋ Thêm mới";
            this.btnThem.UseVisualStyleBackColor = false;
            this.btnThem.Click += new System.EventHandler(this.btnThem_Click);
            // 
            // btnSua
            // 
            this.btnSua.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSua.Location = new System.Drawing.Point(168, 215);
            this.btnSua.Name = "btnSua";
            this.btnSua.Size = new System.Drawing.Size(139, 34);
            this.btnSua.TabIndex = 6;
            this.btnSua.Text = "✎ Lưu Sửa";
            this.btnSua.UseVisualStyleBackColor = true;
            this.btnSua.Click += new System.EventHandler(this.btnSua_Click);
            // 
            // btnXoa
            // 
            this.btnXoa.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnXoa.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(18)))), ((int)(((byte)(60)))));
            this.btnXoa.Location = new System.Drawing.Point(21, 260);
            this.btnXoa.Name = "btnXoa";
            this.btnXoa.Size = new System.Drawing.Size(135, 34);
            this.btnXoa.TabIndex = 7;
            this.btnXoa.Text = "🗑 Xóa loại món";
            this.btnXoa.UseVisualStyleBackColor = true;
            this.btnXoa.Click += new System.EventHandler(this.btnXoa_Click);
            // 
            // btnNhapLai
            // 
            this.btnNhapLai.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnNhapLai.Location = new System.Drawing.Point(168, 260);
            this.btnNhapLai.Name = "btnNhapLai";
            this.btnNhapLai.Size = new System.Drawing.Size(139, 34);
            this.btnNhapLai.TabIndex = 8;
            this.btnNhapLai.Text = "🔄 Nhập lại";
            this.btnNhapLai.UseVisualStyleBackColor = true;
            this.btnNhapLai.Click += new System.EventHandler(this.btnNhapLai_Click);
            // 
            // LoaiMonForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(834, 501);
            this.Controls.Add(this.grpChiTiet);
            this.Controls.Add(this.dgvLoaiMon);
            this.Controls.Add(this.btnLamMoi);
            this.Controls.Add(this.txtTimKiem);
            this.Controls.Add(this.lblTimKiem);
            this.Controls.Add(this.lblSub);
            this.Controls.Add(this.lblTieuDe);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "LoaiMonForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Quản Lý Loại Món Ăn";
            this.Load += new System.EventHandler(this.LoaiMonForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLoaiMon)).EndInit();
            this.grpChiTiet.ResumeLayout(false);
            this.grpChiTiet.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}

namespace RestaurantCRM.AdminApp.Forms.Kho {
    partial class KhoForm {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTieuDe;
        private System.Windows.Forms.Label lblSub;
        private System.Windows.Forms.Panel pnlThongKe;
        private System.Windows.Forms.Label lblTongMon;
        private System.Windows.Forms.Label lblDuHang;
        private System.Windows.Forms.Label lblSapHet;
        private System.Windows.Forms.Label lblHetHang;
        private System.Windows.Forms.Label lblTimKiem;
        private System.Windows.Forms.TextBox txtTimKiem;
        private System.Windows.Forms.Label lblBoLoc;
        private System.Windows.Forms.ComboBox cboBoLoc;
        private System.Windows.Forms.Button btnLamMoi;
        private System.Windows.Forms.Button btnKiemKe;
        private System.Windows.Forms.Button btnLapPhieuNhap;
        private System.Windows.Forms.DataGridView dgvKho;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMaMon;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTenMon;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLoaiMon;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDonGia;
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
            this.lblTieuDe = new System.Windows.Forms.Label();
            this.lblSub = new System.Windows.Forms.Label();
            this.pnlThongKe = new System.Windows.Forms.Panel();
            this.lblHetHang = new System.Windows.Forms.Label();
            this.lblSapHet = new System.Windows.Forms.Label();
            this.lblDuHang = new System.Windows.Forms.Label();
            this.lblTongMon = new System.Windows.Forms.Label();
            this.lblTimKiem = new System.Windows.Forms.Label();
            this.txtTimKiem = new System.Windows.Forms.TextBox();
            this.lblBoLoc = new System.Windows.Forms.Label();
            this.cboBoLoc = new System.Windows.Forms.ComboBox();
            this.btnLamMoi = new System.Windows.Forms.Button();
            this.btnKiemKe = new System.Windows.Forms.Button();
            this.btnLapPhieuNhap = new System.Windows.Forms.Button();
            this.dgvKho = new System.Windows.Forms.DataGridView();
            this.colMaMon = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTenMon = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLoaiMon = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDonGia = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSoLuongTon = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTrangThai = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlThongKe.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvKho)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTieuDe
            // 
            this.lblTieuDe.AutoSize = true;
            this.lblTieuDe.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTieuDe.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(36)))), ((int)(((byte)(43)))));
            this.lblTieuDe.Location = new System.Drawing.Point(14, 12);
            this.lblTieuDe.Name = "lblTieuDe";
            this.lblTieuDe.Size = new System.Drawing.Size(262, 25);
            this.lblTieuDe.TabIndex = 0;
            this.lblTieuDe.Text = "Quản Lý Kho Hàng & Tồn Kho";
            // 
            // lblSub
            // 
            this.lblSub.AutoSize = true;
            this.lblSub.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSub.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblSub.Location = new System.Drawing.Point(16, 40);
            this.lblSub.Name = "lblSub";
            this.lblSub.Size = new System.Drawing.Size(395, 15);
            this.lblSub.TabIndex = 1;
            this.lblSub.Text = "Theo dõi số lượng tồn kho theo thời gian thực và cảnh báo sắp hết nguyên liệu.";
            // 
            // pnlThongKe
            // 
            this.pnlThongKe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlThongKe.BackColor = System.Drawing.Color.White;
            this.pnlThongKe.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlThongKe.Controls.Add(this.lblHetHang);
            this.pnlThongKe.Controls.Add(this.lblSapHet);
            this.pnlThongKe.Controls.Add(this.lblDuHang);
            this.pnlThongKe.Controls.Add(this.lblTongMon);
            this.pnlThongKe.Location = new System.Drawing.Point(18, 64);
            this.pnlThongKe.Name = "pnlThongKe";
            this.pnlThongKe.Size = new System.Drawing.Size(840, 52);
            this.pnlThongKe.TabIndex = 2;
            // 
            // lblHetHang
            // 
            this.lblHetHang.AutoSize = true;
            this.lblHetHang.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblHetHang.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(29)))), ((int)(((byte)(72)))));
            this.lblHetHang.Location = new System.Drawing.Point(580, 16);
            this.lblHetHang.Name = "lblHetHang";
            this.lblHetHang.Size = new System.Drawing.Size(107, 17);
            this.lblHetHang.TabIndex = 3;
            this.lblHetHang.Text = "Đã hết: 0 món";
            // 
            // lblSapHet
            // 
            this.lblSapHet.AutoSize = true;
            this.lblSapHet.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblSapHet.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(119)))), ((int)(((byte)(6)))));
            this.lblSapHet.Location = new System.Drawing.Point(370, 16);
            this.lblSapHet.Name = "lblSapHet";
            this.lblSapHet.Size = new System.Drawing.Size(139, 17);
            this.lblSapHet.TabIndex = 2;
            this.lblSapHet.Text = "Sắp hết (<= 10): 0 món";
            // 
            // lblDuHang
            // 
            this.lblDuHang.AutoSize = true;
            this.lblDuHang.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblDuHang.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(163)))), ((int)(((byte)(74)))));
            this.lblDuHang.Location = new System.Drawing.Point(180, 16);
            this.lblDuHang.Name = "lblDuHang";
            this.lblDuHang.Size = new System.Drawing.Size(126, 17);
            this.lblDuHang.TabIndex = 1;
            this.lblDuHang.Text = "Đủ hàng: 0 món";
            // 
            // lblTongMon
            // 
            this.lblTongMon.AutoSize = true;
            this.lblTongMon.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblTongMon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblTongMon.Location = new System.Drawing.Point(18, 16);
            this.lblTongMon.Name = "lblTongMon";
            this.lblTongMon.Size = new System.Drawing.Size(117, 17);
            this.lblTongMon.TabIndex = 0;
            this.lblTongMon.Text = "Tổng số: 0 món";
            // 
            // lblTimKiem
            // 
            this.lblTimKiem.AutoSize = true;
            this.lblTimKiem.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblTimKiem.Location = new System.Drawing.Point(16, 131);
            this.lblTimKiem.Name = "lblTimKiem";
            this.lblTimKiem.Size = new System.Drawing.Size(63, 17);
            this.lblTimKiem.TabIndex = 3;
            this.lblTimKiem.Text = "Tìm kiếm:";
            // 
            // txtTimKiem
            // 
            this.txtTimKiem.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtTimKiem.Location = new System.Drawing.Point(82, 128);
            this.txtTimKiem.Name = "txtTimKiem";
            this.txtTimKiem.Size = new System.Drawing.Size(190, 24);
            this.txtTimKiem.TabIndex = 4;
            this.txtTimKiem.TextChanged += new System.EventHandler(this.txtTimKiem_TextChanged);
            // 
            // lblBoLoc
            // 
            this.lblBoLoc.AutoSize = true;
            this.lblBoLoc.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblBoLoc.Location = new System.Drawing.Point(285, 131);
            this.lblBoLoc.Name = "lblBoLoc";
            this.lblBoLoc.Size = new System.Drawing.Size(68, 17);
            this.lblBoLoc.TabIndex = 5;
            this.lblBoLoc.Text = "Tình trạng:";
            // 
            // cboBoLoc
            // 
            this.cboBoLoc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBoLoc.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboBoLoc.FormattingEnabled = true;
            this.cboBoLoc.Items.AddRange(new object[] {
            "Tất cả mặt hàng",
            "Sắp hết hàng (<= 10)",
            "Đã hết hàng (= 0)",
            "Còn đủ hàng (> 10)"});
            this.cboBoLoc.Location = new System.Drawing.Point(356, 127);
            this.cboBoLoc.Name = "cboBoLoc";
            this.cboBoLoc.Size = new System.Drawing.Size(160, 25);
            this.cboBoLoc.TabIndex = 6;
            this.cboBoLoc.SelectedIndexChanged += new System.EventHandler(this.cboBoLoc_SelectedIndexChanged);
            // 
            // btnLamMoi
            // 
            this.btnLamMoi.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnLamMoi.Location = new System.Drawing.Point(525, 126);
            this.btnLamMoi.Name = "btnLamMoi";
            this.btnLamMoi.Size = new System.Drawing.Size(75, 27);
            this.btnLamMoi.TabIndex = 7;
            this.btnLamMoi.Text = "Làm mới";
            this.btnLamMoi.UseVisualStyleBackColor = true;
            this.btnLamMoi.Click += new System.EventHandler(this.btnLamMoi_Click);
            // 
            // btnKiemKe
            // 
            this.btnKiemKe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnKiemKe.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnKiemKe.FlatAppearance.BorderSize = 0;
            this.btnKiemKe.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnKiemKe.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnKiemKe.ForeColor = System.Drawing.Color.White;
            this.btnKiemKe.Location = new System.Drawing.Point(607, 126);
            this.btnKiemKe.Name = "btnKiemKe";
            this.btnKiemKe.Size = new System.Drawing.Size(130, 27);
            this.btnKiemKe.TabIndex = 8;
            this.btnKiemKe.Text = "✎ Điều chỉnh tồn";
            this.btnKiemKe.UseVisualStyleBackColor = false;
            this.btnKiemKe.Click += new System.EventHandler(this.btnKiemKe_Click);
            // 
            // btnLapPhieuNhap
            // 
            this.btnLapPhieuNhap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLapPhieuNhap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.btnLapPhieuNhap.FlatAppearance.BorderSize = 0;
            this.btnLapPhieuNhap.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLapPhieuNhap.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnLapPhieuNhap.ForeColor = System.Drawing.Color.White;
            this.btnLapPhieuNhap.Location = new System.Drawing.Point(743, 126);
            this.btnLapPhieuNhap.Name = "btnLapPhieuNhap";
            this.btnLapPhieuNhap.Size = new System.Drawing.Size(115, 27);
            this.btnLapPhieuNhap.TabIndex = 9;
            this.btnLapPhieuNhap.Text = "＋ Nhập hàng";
            this.btnLapPhieuNhap.UseVisualStyleBackColor = false;
            this.btnLapPhieuNhap.Click += new System.EventHandler(this.btnLapPhieuNhap_Click);
            // 
            // dgvKho
            // 
            this.dgvKho.AllowUserToAddRows = false;
            this.dgvKho.AllowUserToDeleteRows = false;
            this.dgvKho.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvKho.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvKho.BackgroundColor = System.Drawing.Color.White;
            this.dgvKho.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvKho.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colMaMon,
            this.colTenMon,
            this.colLoaiMon,
            this.colDonGia,
            this.colSoLuongTon,
            this.colTrangThai});
            this.dgvKho.Location = new System.Drawing.Point(18, 165);
            this.dgvKho.MultiSelect = false;
            this.dgvKho.Name = "dgvKho";
            this.dgvKho.ReadOnly = true;
            this.dgvKho.RowHeadersVisible = false;
            this.dgvKho.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvKho.Size = new System.Drawing.Size(840, 360);
            this.dgvKho.TabIndex = 10;
            this.dgvKho.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvKho_CellFormatting);
            this.dgvKho.DoubleClick += new System.EventHandler(this.dgvKho_DoubleClick);
            // 
            // colMaMon
            // 
            this.colMaMon.DataPropertyName = "MaMon";
            this.colMaMon.FillWeight = 50F;
            this.colMaMon.HeaderText = "Mã Món";
            this.colMaMon.MinimumWidth = 70;
            this.colMaMon.Name = "colMaMon";
            this.colMaMon.ReadOnly = true;
            // 
            // colTenMon
            // 
            this.colTenMon.DataPropertyName = "TenMon";
            this.colTenMon.FillWeight = 120F;
            this.colTenMon.HeaderText = "Tên Món Ăn";
            this.colTenMon.MinimumWidth = 140;
            this.colTenMon.Name = "colTenMon";
            this.colTenMon.ReadOnly = true;
            // 
            // colLoaiMon
            // 
            this.colLoaiMon.DataPropertyName = "TenLoaiMon";
            this.colLoaiMon.FillWeight = 80F;
            this.colLoaiMon.HeaderText = "Loại Món";
            this.colLoaiMon.MinimumWidth = 90;
            this.colLoaiMon.Name = "colLoaiMon";
            this.colLoaiMon.ReadOnly = true;
            // 
            // colDonGia
            // 
            this.colDonGia.DataPropertyName = "DonGia";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle1.Format = "N0";
            this.colDonGia.DefaultCellStyle = dataGridViewCellStyle1;
            this.colDonGia.FillWeight = 70F;
            this.colDonGia.HeaderText = "Đơn Giá (đ)";
            this.colDonGia.MinimumWidth = 80;
            this.colDonGia.Name = "colDonGia";
            this.colDonGia.ReadOnly = true;
            // 
            // colSoLuongTon
            // 
            this.colSoLuongTon.DataPropertyName = "SoLuongTon";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle2.Format = "N0";
            this.colSoLuongTon.DefaultCellStyle = dataGridViewCellStyle2;
            this.colSoLuongTon.FillWeight = 65F;
            this.colSoLuongTon.HeaderText = "Số Lượng Tồn";
            this.colSoLuongTon.MinimumWidth = 80;
            this.colSoLuongTon.Name = "colSoLuongTon";
            this.colSoLuongTon.ReadOnly = true;
            // 
            // colTrangThai
            // 
            this.colTrangThai.DataPropertyName = "TrangThaiHienThi";
            this.colTrangThai.FillWeight = 80F;
            this.colTrangThai.HeaderText = "Cảnh Báo Tồn";
            this.colTrangThai.MinimumWidth = 100;
            this.colTrangThai.Name = "colTrangThai";
            this.colTrangThai.ReadOnly = true;
            // 
            // KhoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(874, 541);
            this.Controls.Add(this.dgvKho);
            this.Controls.Add(this.btnLapPhieuNhap);
            this.Controls.Add(this.btnKiemKe);
            this.Controls.Add(this.btnLamMoi);
            this.Controls.Add(this.cboBoLoc);
            this.Controls.Add(this.lblBoLoc);
            this.Controls.Add(this.txtTimKiem);
            this.Controls.Add(this.lblTimKiem);
            this.Controls.Add(this.pnlThongKe);
            this.Controls.Add(this.lblSub);
            this.Controls.Add(this.lblTieuDe);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "KhoForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Quản Lý Kho Hàng & Tồn Kho";
            this.Load += new System.EventHandler(this.KhoForm_Load);
            this.pnlThongKe.ResumeLayout(false);
            this.pnlThongKe.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvKho)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}

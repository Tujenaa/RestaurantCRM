namespace RestaurantCRM.AdminApp.Forms.Kho {
    partial class NguyenLieuDialog {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTieuDe;
        private System.Windows.Forms.Label lblMaNL;
        private System.Windows.Forms.TextBox txtMaNL;
        private System.Windows.Forms.Label lblTenNL;
        private System.Windows.Forms.TextBox txtTenNL;
        private System.Windows.Forms.Label lblDVT;
        private System.Windows.Forms.ComboBox cboDVT;
        private System.Windows.Forms.Label lblGiaNhap;
        private System.Windows.Forms.NumericUpDown numGiaNhap;
        private System.Windows.Forms.Label lblTonKho;
        private System.Windows.Forms.NumericUpDown numTonKho;
        private System.Windows.Forms.Label lblTrangThai;
        private System.Windows.Forms.ComboBox cboTrangThai;
        private System.Windows.Forms.Button btnLuu;
        private System.Windows.Forms.Button btnHuy;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            this.lblTieuDe = new System.Windows.Forms.Label();
            this.lblMaNL = new System.Windows.Forms.Label();
            this.txtMaNL = new System.Windows.Forms.TextBox();
            this.lblTenNL = new System.Windows.Forms.Label();
            this.txtTenNL = new System.Windows.Forms.TextBox();
            this.lblDVT = new System.Windows.Forms.Label();
            this.cboDVT = new System.Windows.Forms.ComboBox();
            this.lblGiaNhap = new System.Windows.Forms.Label();
            this.numGiaNhap = new System.Windows.Forms.NumericUpDown();
            this.lblTonKho = new System.Windows.Forms.Label();
            this.numTonKho = new System.Windows.Forms.NumericUpDown();
            this.lblTrangThai = new System.Windows.Forms.Label();
            this.cboTrangThai = new System.Windows.Forms.ComboBox();
            this.btnLuu = new System.Windows.Forms.Button();
            this.btnHuy = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numGiaNhap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTonKho)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTieuDe
            // 
            this.lblTieuDe.AutoSize = true;
            this.lblTieuDe.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTieuDe.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblTieuDe.Location = new System.Drawing.Point(20, 16);
            this.lblTieuDe.Name = "lblTieuDe";
            this.lblTieuDe.Size = new System.Drawing.Size(225, 21);
            this.lblTieuDe.TabIndex = 0;
            this.lblTieuDe.Text = "Thêm Nguyên Vật Liệu Mới";
            // 
            // lblMaNL
            // 
            this.lblMaNL.AutoSize = true;
            this.lblMaNL.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMaNL.Location = new System.Drawing.Point(22, 55);
            this.lblMaNL.Name = "lblMaNL";
            this.lblMaNL.Size = new System.Drawing.Size(91, 15);
            this.lblMaNL.TabIndex = 1;
            this.lblMaNL.Text = "Mã nguyên liệu:";
            // 
            // txtMaNL
            // 
            this.txtMaNL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.txtMaNL.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtMaNL.Location = new System.Drawing.Point(145, 51);
            this.txtMaNL.Name = "txtMaNL";
            this.txtMaNL.ReadOnly = true;
            this.txtMaNL.Size = new System.Drawing.Size(220, 24);
            this.txtMaNL.TabIndex = 2;
            this.txtMaNL.Text = "(Tự động sinh: NLxx)";
            // 
            // lblTenNL
            // 
            this.lblTenNL.AutoSize = true;
            this.lblTenNL.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTenNL.Location = new System.Drawing.Point(22, 92);
            this.lblTenNL.Name = "lblTenNL";
            this.lblTenNL.Size = new System.Drawing.Size(111, 15);
            this.lblTenNL.TabIndex = 3;
            this.lblTenNL.Text = "Tên nguyên liệu (*):";
            // 
            // txtTenNL
            // 
            this.txtTenNL.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtTenNL.Location = new System.Drawing.Point(145, 88);
            this.txtTenNL.Name = "txtTenNL";
            this.txtTenNL.Size = new System.Drawing.Size(220, 24);
            this.txtTenNL.TabIndex = 4;
            // 
            // lblDVT
            // 
            this.lblDVT.AutoSize = true;
            this.lblDVT.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDVT.Location = new System.Drawing.Point(22, 129);
            this.lblDVT.Name = "lblDVT";
            this.lblDVT.Size = new System.Drawing.Size(89, 15);
            this.lblDVT.TabIndex = 5;
            this.lblDVT.Text = "Đơn vị tính (*):";
            // 
            // cboDVT
            // 
            this.cboDVT.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboDVT.FormattingEnabled = true;
            this.cboDVT.Items.AddRange(new object[] {
            "kg",
            "g",
            "lít",
            "ml",
            "quả",
            "hộp",
            "gói",
            "chai",
            "lon",
            "con",
            "bó",
            "cây",
            "cái",
            "khay",
            "túi",
            "thùng"});
            this.cboDVT.Location = new System.Drawing.Point(145, 125);
            this.cboDVT.Name = "cboDVT";
            this.cboDVT.Size = new System.Drawing.Size(220, 24);
            this.cboDVT.TabIndex = 6;
            // 
            // lblGiaNhap
            // 
            this.lblGiaNhap.AutoSize = true;
            this.lblGiaNhap.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblGiaNhap.Location = new System.Drawing.Point(22, 166);
            this.lblGiaNhap.Name = "lblGiaNhap";
            this.lblGiaNhap.Size = new System.Drawing.Size(107, 15);
            this.lblGiaNhap.TabIndex = 7;
            this.lblGiaNhap.Text = "Giá nhập gần nhất:";
            // 
            // numGiaNhap
            // 
            this.numGiaNhap.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.numGiaNhap.Increment = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numGiaNhap.Location = new System.Drawing.Point(145, 162);
            this.numGiaNhap.Maximum = new decimal(new int[] { 100000000, 0, 0, 0 });
            this.numGiaNhap.Name = "numGiaNhap";
            this.numGiaNhap.Size = new System.Drawing.Size(220, 24);
            this.numGiaNhap.TabIndex = 8;
            this.numGiaNhap.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numGiaNhap.ThousandsSeparator = true;
            // 
            // lblTonKho
            // 
            this.lblTonKho.AutoSize = true;
            this.lblTonKho.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTonKho.Location = new System.Drawing.Point(22, 203);
            this.lblTonKho.Name = "lblTonKho";
            this.lblTonKho.Size = new System.Drawing.Size(96, 15);
            this.lblTonKho.TabIndex = 9;
            this.lblTonKho.Text = "Tồn kho ban đầu:";
            // 
            // numTonKho
            // 
            this.numTonKho.DecimalPlaces = 2;
            this.numTonKho.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.numTonKho.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            this.numTonKho.Location = new System.Drawing.Point(145, 199);
            this.numTonKho.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            this.numTonKho.Name = "numTonKho";
            this.numTonKho.Size = new System.Drawing.Size(220, 24);
            this.numTonKho.TabIndex = 10;
            this.numTonKho.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblTrangThai
            // 
            this.lblTrangThai.AutoSize = true;
            this.lblTrangThai.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTrangThai.Location = new System.Drawing.Point(22, 240);
            this.lblTrangThai.Name = "lblTrangThai";
            this.lblTrangThai.Size = new System.Drawing.Size(62, 15);
            this.lblTrangThai.TabIndex = 11;
            this.lblTrangThai.Text = "Trạng thái:";
            // 
            // cboTrangThai
            // 
            this.cboTrangThai.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTrangThai.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboTrangThai.FormattingEnabled = true;
            this.cboTrangThai.Items.AddRange(new object[] {
            "Active (Đang sử dụng)",
            "Inactive (Tạm ngưng)"});
            this.cboTrangThai.Location = new System.Drawing.Point(145, 236);
            this.cboTrangThai.Name = "cboTrangThai";
            this.cboTrangThai.Size = new System.Drawing.Size(220, 24);
            this.cboTrangThai.TabIndex = 12;
            // 
            // btnLuu
            // 
            this.btnLuu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnLuu.FlatAppearance.BorderSize = 0;
            this.btnLuu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLuu.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnLuu.ForeColor = System.Drawing.Color.White;
            this.btnLuu.Location = new System.Drawing.Point(175, 280);
            this.btnLuu.Name = "btnLuu";
            this.btnLuu.Size = new System.Drawing.Size(100, 32);
            this.btnLuu.TabIndex = 13;
            this.btnLuu.Text = "Lưu lại";
            this.btnLuu.UseVisualStyleBackColor = false;
            this.btnLuu.Click += new System.EventHandler(this.btnLuu_Click);
            // 
            // btnHuy
            // 
            this.btnHuy.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnHuy.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnHuy.Location = new System.Drawing.Point(285, 280);
            this.btnHuy.Name = "btnHuy";
            this.btnHuy.Size = new System.Drawing.Size(80, 32);
            this.btnHuy.TabIndex = 14;
            this.btnHuy.Text = "Hủy";
            this.btnHuy.UseVisualStyleBackColor = true;
            this.btnHuy.Click += new System.EventHandler(this.btnHuy_Click);
            // 
            // NguyenLieuDialog
            // 
            this.AcceptButton = this.btnLuu;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.CancelButton = this.btnHuy;
            this.ClientSize = new System.Drawing.Size(395, 332);
            this.Controls.Add(this.btnHuy);
            this.Controls.Add(this.btnLuu);
            this.Controls.Add(this.cboTrangThai);
            this.Controls.Add(this.lblTrangThai);
            this.Controls.Add(this.numTonKho);
            this.Controls.Add(this.lblTonKho);
            this.Controls.Add(this.numGiaNhap);
            this.Controls.Add(this.lblGiaNhap);
            this.Controls.Add(this.cboDVT);
            this.Controls.Add(this.lblDVT);
            this.Controls.Add(this.txtTenNL);
            this.Controls.Add(this.lblTenNL);
            this.Controls.Add(this.txtMaNL);
            this.Controls.Add(this.lblMaNL);
            this.Controls.Add(this.lblTieuDe);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NguyenLieuDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Thông Tin Nguyên Vật Liệu";
            this.Load += new System.EventHandler(this.NguyenLieuDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numGiaNhap)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTonKho)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}

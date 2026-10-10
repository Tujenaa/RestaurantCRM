namespace RestaurantCRM.AdminApp.Forms.MonAn {
    partial class MonAnDialog {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTenMon;
        private System.Windows.Forms.TextBox txtTenMon;
        private System.Windows.Forms.Label lblGia;
        private System.Windows.Forms.NumericUpDown numGia;
        private System.Windows.Forms.Label lblLoaiMon;
        private System.Windows.Forms.ComboBox cboLoaiMon;
        private System.Windows.Forms.CheckBox chkTrangThai;
        private System.Windows.Forms.Label lblHinhAnh;
        private System.Windows.Forms.TextBox txtHinhAnhUrl;
        private System.Windows.Forms.Button btnChonHinh;
        private System.Windows.Forms.PictureBox picHinhAnh;
        private System.Windows.Forms.Button btnLuu;
        private System.Windows.Forms.Button btnHuy;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            this.lblTenMon = new System.Windows.Forms.Label();
            this.txtTenMon = new System.Windows.Forms.TextBox();
            this.lblGia = new System.Windows.Forms.Label();
            this.numGia = new System.Windows.Forms.NumericUpDown();
            this.lblLoaiMon = new System.Windows.Forms.Label();
            this.cboLoaiMon = new System.Windows.Forms.ComboBox();
            this.chkTrangThai = new System.Windows.Forms.CheckBox();
            this.lblHinhAnh = new System.Windows.Forms.Label();
            this.txtHinhAnhUrl = new System.Windows.Forms.TextBox();
            this.btnChonHinh = new System.Windows.Forms.Button();
            this.picHinhAnh = new System.Windows.Forms.PictureBox();
            this.btnLuu = new System.Windows.Forms.Button();
            this.btnHuy = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numGia)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHinhAnh)).BeginInit();
            this.SuspendLayout();
            
            // lblTenMon
            this.lblTenMon.AutoSize = true;
            this.lblTenMon.Location = new System.Drawing.Point(20, 20);
            this.lblTenMon.Name = "lblTenMon";
            this.lblTenMon.Size = new System.Drawing.Size(67, 13);
            this.lblTenMon.TabIndex = 0;
            this.lblTenMon.Text = "Tên Món ăn:";
            
            // txtTenMon
            this.txtTenMon.Location = new System.Drawing.Point(100, 17);
            this.txtTenMon.Name = "txtTenMon";
            this.txtTenMon.Size = new System.Drawing.Size(200, 20);
            this.txtTenMon.TabIndex = 1;
            
            // lblGia
            this.lblGia.AutoSize = true;
            this.lblGia.Location = new System.Drawing.Point(20, 60);
            this.lblGia.Name = "lblGia";
            this.lblGia.Size = new System.Drawing.Size(26, 13);
            this.lblGia.TabIndex = 2;
            this.lblGia.Text = "Giá:";
            
            // numGia
            this.numGia.Location = new System.Drawing.Point(100, 58);
            this.numGia.Maximum = new decimal(new int[] { 100000000, 0, 0, 0 });
            this.numGia.Name = "numGia";
            this.numGia.Size = new System.Drawing.Size(200, 20);
            this.numGia.TabIndex = 3;
            
            // lblLoaiMon
            this.lblLoaiMon.AutoSize = true;
            this.lblLoaiMon.Location = new System.Drawing.Point(20, 100);
            this.lblLoaiMon.Name = "lblLoaiMon";
            this.lblLoaiMon.Size = new System.Drawing.Size(53, 13);
            this.lblLoaiMon.TabIndex = 4;
            this.lblLoaiMon.Text = "Loại món:";
            
            // cboLoaiMon
            this.cboLoaiMon.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLoaiMon.FormattingEnabled = true;
            this.cboLoaiMon.Location = new System.Drawing.Point(100, 97);
            this.cboLoaiMon.Name = "cboLoaiMon";
            this.cboLoaiMon.Size = new System.Drawing.Size(200, 21);
            this.cboLoaiMon.TabIndex = 5;
            
            // chkTrangThai
            this.chkTrangThai.AutoSize = true;
            this.chkTrangThai.Checked = true;
            this.chkTrangThai.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTrangThai.Location = new System.Drawing.Point(100, 140);
            this.chkTrangThai.Name = "chkTrangThai";
            this.chkTrangThai.Size = new System.Drawing.Size(107, 17);
            this.chkTrangThai.TabIndex = 6;
            this.chkTrangThai.Text = "Đang kinh doanh";
            this.chkTrangThai.UseVisualStyleBackColor = true;
            
            // lblHinhAnh
            this.lblHinhAnh.AutoSize = true;
            this.lblHinhAnh.Location = new System.Drawing.Point(20, 180);
            this.lblHinhAnh.Name = "lblHinhAnh";
            this.lblHinhAnh.Size = new System.Drawing.Size(53, 13);
            this.lblHinhAnh.TabIndex = 7;
            this.lblHinhAnh.Text = "Hình ảnh:";
            
            // txtHinhAnhUrl
            this.txtHinhAnhUrl.Location = new System.Drawing.Point(100, 177);
            this.txtHinhAnhUrl.Name = "txtHinhAnhUrl";
            this.txtHinhAnhUrl.ReadOnly = true;
            this.txtHinhAnhUrl.Size = new System.Drawing.Size(120, 20);
            this.txtHinhAnhUrl.TabIndex = 8;
            
            // btnChonHinh
            this.btnChonHinh.Location = new System.Drawing.Point(225, 175);
            this.btnChonHinh.Name = "btnChonHinh";
            this.btnChonHinh.Size = new System.Drawing.Size(75, 23);
            this.btnChonHinh.TabIndex = 9;
            this.btnChonHinh.Text = "Chọn...";
            this.btnChonHinh.UseVisualStyleBackColor = true;
            this.btnChonHinh.Click += new System.EventHandler(this.btnChonHinh_Click);
            
            // picHinhAnh
            this.picHinhAnh.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picHinhAnh.Location = new System.Drawing.Point(330, 17);
            this.picHinhAnh.Name = "picHinhAnh";
            this.picHinhAnh.Size = new System.Drawing.Size(180, 180);
            this.picHinhAnh.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picHinhAnh.TabIndex = 10;
            this.picHinhAnh.TabStop = false;
            
            // btnLuu
            this.btnLuu.Location = new System.Drawing.Point(164, 230);
            this.btnLuu.Name = "btnLuu";
            this.btnLuu.Size = new System.Drawing.Size(75, 23);
            this.btnLuu.TabIndex = 11;
            this.btnLuu.Text = "Lưu";
            this.btnLuu.UseVisualStyleBackColor = true;
            this.btnLuu.Click += new System.EventHandler(this.btnLuu_Click);
            
            // btnHuy
            this.btnHuy.Location = new System.Drawing.Point(245, 230);
            this.btnHuy.Name = "btnHuy";
            this.btnHuy.Size = new System.Drawing.Size(75, 23);
            this.btnHuy.TabIndex = 12;
            this.btnHuy.Text = "Hủy";
            this.btnHuy.UseVisualStyleBackColor = true;
            this.btnHuy.Click += new System.EventHandler(this.btnHuy_Click);
            
            // MonAnDialog
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(530, 270);
            this.Controls.Add(this.btnHuy);
            this.Controls.Add(this.btnLuu);
            this.Controls.Add(this.picHinhAnh);
            this.Controls.Add(this.btnChonHinh);
            this.Controls.Add(this.txtHinhAnhUrl);
            this.Controls.Add(this.lblHinhAnh);
            this.Controls.Add(this.chkTrangThai);
            this.Controls.Add(this.cboLoaiMon);
            this.Controls.Add(this.lblLoaiMon);
            this.Controls.Add(this.numGia);
            this.Controls.Add(this.lblGia);
            this.Controls.Add(this.txtTenMon);
            this.Controls.Add(this.lblTenMon);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MonAnDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Thông Tin Món ăn";
            this.Load += new System.EventHandler(this.MonAnDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numGia)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHinhAnh)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}


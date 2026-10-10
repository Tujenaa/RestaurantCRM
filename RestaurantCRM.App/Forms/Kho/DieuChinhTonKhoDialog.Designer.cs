namespace RestaurantCRM.AdminApp.Forms.Kho {
    partial class DieuChinhTonKhoDialog {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTenMon;
        private System.Windows.Forms.TextBox txtTenMon;
        private System.Windows.Forms.Label lblSoLuongHienTai;
        private System.Windows.Forms.TextBox txtSoLuongHienTai;
        private System.Windows.Forms.Label lblSoLuongMoi;
        private System.Windows.Forms.NumericUpDown numSoLuongMoi;
        private System.Windows.Forms.Label lblGhiChu;
        private System.Windows.Forms.TextBox txtGhiChu;
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
            this.lblSoLuongHienTai = new System.Windows.Forms.Label();
            this.txtSoLuongHienTai = new System.Windows.Forms.TextBox();
            this.lblSoLuongMoi = new System.Windows.Forms.Label();
            this.numSoLuongMoi = new System.Windows.Forms.NumericUpDown();
            this.lblGhiChu = new System.Windows.Forms.Label();
            this.txtGhiChu = new System.Windows.Forms.TextBox();
            this.btnLuu = new System.Windows.Forms.Button();
            this.btnHuy = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numSoLuongMoi)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTenMon
            // 
            this.lblTenMon.AutoSize = true;
            this.lblTenMon.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTenMon.Location = new System.Drawing.Point(20, 20);
            this.lblTenMon.Name = "lblTenMon";
            this.lblTenMon.Size = new System.Drawing.Size(55, 15);
            this.lblTenMon.TabIndex = 0;
            this.lblTenMon.Text = "Tên món:";
            // 
            // txtTenMon
            // 
            this.txtTenMon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.txtTenMon.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtTenMon.Location = new System.Drawing.Point(125, 16);
            this.txtTenMon.Name = "txtTenMon";
            this.txtTenMon.ReadOnly = true;
            this.txtTenMon.Size = new System.Drawing.Size(185, 24);
            this.txtTenMon.TabIndex = 1;
            // 
            // lblSoLuongHienTai
            // 
            this.lblSoLuongHienTai.AutoSize = true;
            this.lblSoLuongHienTai.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSoLuongHienTai.Location = new System.Drawing.Point(20, 58);
            this.lblSoLuongHienTai.Name = "lblSoLuongHienTai";
            this.lblSoLuongHienTai.Size = new System.Drawing.Size(99, 15);
            this.lblSoLuongHienTai.TabIndex = 2;
            this.lblSoLuongHienTai.Text = "Số lượng hiện tại:";
            // 
            // txtSoLuongHienTai
            // 
            this.txtSoLuongHienTai.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.txtSoLuongHienTai.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtSoLuongHienTai.Location = new System.Drawing.Point(125, 54);
            this.txtSoLuongHienTai.Name = "txtSoLuongHienTai";
            this.txtSoLuongHienTai.ReadOnly = true;
            this.txtSoLuongHienTai.Size = new System.Drawing.Size(185, 24);
            this.txtSoLuongHienTai.TabIndex = 3;
            // 
            // lblSoLuongMoi
            // 
            this.lblSoLuongMoi.AutoSize = true;
            this.lblSoLuongMoi.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSoLuongMoi.Location = new System.Drawing.Point(20, 96);
            this.lblSoLuongMoi.Name = "lblSoLuongMoi";
            this.lblSoLuongMoi.Size = new System.Drawing.Size(99, 15);
            this.lblSoLuongMoi.TabIndex = 4;
            this.lblSoLuongMoi.Text = "Số lượng thực tế:";
            // 
            // numSoLuongMoi
            // 
            this.numSoLuongMoi.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.numSoLuongMoi.Location = new System.Drawing.Point(125, 92);
            this.numSoLuongMoi.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            this.numSoLuongMoi.Name = "numSoLuongMoi";
            this.numSoLuongMoi.Size = new System.Drawing.Size(185, 24);
            this.numSoLuongMoi.TabIndex = 5;
            // 
            // lblGhiChu
            // 
            this.lblGhiChu.AutoSize = true;
            this.lblGhiChu.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblGhiChu.Location = new System.Drawing.Point(20, 134);
            this.lblGhiChu.Name = "lblGhiChu";
            this.lblGhiChu.Size = new System.Drawing.Size(81, 15);
            this.lblGhiChu.TabIndex = 6;
            this.lblGhiChu.Text = "Lý do / Ghi chú:";
            // 
            // txtGhiChu
            // 
            this.txtGhiChu.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtGhiChu.Location = new System.Drawing.Point(125, 130);
            this.txtGhiChu.Name = "txtGhiChu";
            this.txtGhiChu.Size = new System.Drawing.Size(185, 24);
            this.txtGhiChu.TabIndex = 7;
            // 
            // btnLuu
            // 
            this.btnLuu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnLuu.FlatAppearance.BorderSize = 0;
            this.btnLuu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLuu.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnLuu.ForeColor = System.Drawing.Color.White;
            this.btnLuu.Location = new System.Drawing.Point(135, 175);
            this.btnLuu.Name = "btnLuu";
            this.btnLuu.Size = new System.Drawing.Size(85, 30);
            this.btnLuu.TabIndex = 8;
            this.btnLuu.Text = "Xác nhận";
            this.btnLuu.UseVisualStyleBackColor = false;
            this.btnLuu.Click += new System.EventHandler(this.btnLuu_Click);
            // 
            // btnHuy
            // 
            this.btnHuy.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnHuy.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnHuy.Location = new System.Drawing.Point(230, 175);
            this.btnHuy.Name = "btnHuy";
            this.btnHuy.Size = new System.Drawing.Size(80, 30);
            this.btnHuy.TabIndex = 9;
            this.btnHuy.Text = "Hủy";
            this.btnHuy.UseVisualStyleBackColor = true;
            this.btnHuy.Click += new System.EventHandler(this.btnHuy_Click);
            // 
            // DieuChinhTonKhoDialog
            // 
            this.AcceptButton = this.btnLuu;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.CancelButton = this.btnHuy;
            this.ClientSize = new System.Drawing.Size(334, 222);
            this.Controls.Add(this.btnHuy);
            this.Controls.Add(this.btnLuu);
            this.Controls.Add(this.txtGhiChu);
            this.Controls.Add(this.lblGhiChu);
            this.Controls.Add(this.numSoLuongMoi);
            this.Controls.Add(this.lblSoLuongMoi);
            this.Controls.Add(this.txtSoLuongHienTai);
            this.Controls.Add(this.lblSoLuongHienTai);
            this.Controls.Add(this.txtTenMon);
            this.Controls.Add(this.lblTenMon);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DieuChinhTonKhoDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Kiểm Kê & Điều Chỉnh Tồn Kho";
            this.Load += new System.EventHandler(this.DieuChinhTonKhoDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numSoLuongMoi)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}

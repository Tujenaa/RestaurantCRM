namespace RestaurantCRM.AdminApp.Forms.MonAn {
    partial class MonAnForm {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvMonAn;
        private System.Windows.Forms.Button btnThem;
        private System.Windows.Forms.Button btnSua;
        private System.Windows.Forms.Button btnNgungKinhDoanh;
        private System.Windows.Forms.TextBox txtTimKiem;
        private System.Windows.Forms.ComboBox cboLoaiMon;
        private System.Windows.Forms.Button btnLoaiMon;
        private System.Windows.Forms.Label lblTimKiem;
        private System.Windows.Forms.Label lblLoaiMon;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            this.dgvMonAn = new System.Windows.Forms.DataGridView();
            this.btnThem = new System.Windows.Forms.Button();
            this.btnSua = new System.Windows.Forms.Button();
            this.btnNgungKinhDoanh = new System.Windows.Forms.Button();
            this.txtTimKiem = new System.Windows.Forms.TextBox();
            this.cboLoaiMon = new System.Windows.Forms.ComboBox();
            this.btnLoaiMon = new System.Windows.Forms.Button();
            this.lblTimKiem = new System.Windows.Forms.Label();
            this.lblLoaiMon = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonAn)).BeginInit();
            this.SuspendLayout();
            
            // dgvMonAn
            this.dgvMonAn.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMonAn.Location = new System.Drawing.Point(12, 50);
            this.dgvMonAn.Name = "dgvMonAn";
            this.dgvMonAn.Size = new System.Drawing.Size(776, 350);
            this.dgvMonAn.TabIndex = 0;
            this.dgvMonAn.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMonAn.MultiSelect = false;
            
            // btnThem
            this.btnThem.Location = new System.Drawing.Point(12, 415);
            this.btnThem.Name = "btnThem";
            this.btnThem.Size = new System.Drawing.Size(75, 23);
            this.btnThem.TabIndex = 1;
            this.btnThem.Text = "Thêm";
            this.btnThem.UseVisualStyleBackColor = true;
            this.btnThem.Click += new System.EventHandler(this.btnThem_Click);
            
            // btnSua
            this.btnSua.Location = new System.Drawing.Point(93, 415);
            this.btnSua.Name = "btnSua";
            this.btnSua.Size = new System.Drawing.Size(75, 23);
            this.btnSua.TabIndex = 2;
            this.btnSua.Text = "Sửa";
            this.btnSua.UseVisualStyleBackColor = true;
            this.btnSua.Click += new System.EventHandler(this.btnSua_Click);
            
            // btnNgungKinhDoanh
            this.btnNgungKinhDoanh.Location = new System.Drawing.Point(174, 415);
            this.btnNgungKinhDoanh.Name = "btnNgungKinhDoanh";
            this.btnNgungKinhDoanh.Size = new System.Drawing.Size(120, 23);
            this.btnNgungKinhDoanh.TabIndex = 3;
            this.btnNgungKinhDoanh.Text = "Ngừng Kinh Doanh";
            this.btnNgungKinhDoanh.UseVisualStyleBackColor = true;
            this.btnNgungKinhDoanh.Click += new System.EventHandler(this.btnNgungKinhDoanh_Click);
            
            // txtTimKiem
            this.txtTimKiem.Location = new System.Drawing.Point(75, 15);
            this.txtTimKiem.Name = "txtTimKiem";
            this.txtTimKiem.Size = new System.Drawing.Size(200, 20);
            this.txtTimKiem.TabIndex = 4;
            this.txtTimKiem.TextChanged += new System.EventHandler(this.txtTimKiem_TextChanged);
            
            // cboLoaiMon
            this.cboLoaiMon.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLoaiMon.FormattingEnabled = true;
            this.cboLoaiMon.Location = new System.Drawing.Point(350, 15);
            this.cboLoaiMon.Name = "cboLoaiMon";
            this.cboLoaiMon.Size = new System.Drawing.Size(150, 21);
            this.cboLoaiMon.TabIndex = 5;
            this.cboLoaiMon.SelectedIndexChanged += new System.EventHandler(this.cboLoaiMon_SelectedIndexChanged);
            
            // lblTimKiem
            this.lblTimKiem.AutoSize = true;
            this.lblTimKiem.Location = new System.Drawing.Point(15, 18);
            this.lblTimKiem.Name = "lblTimKiem";
            this.lblTimKiem.Size = new System.Drawing.Size(52, 13);
            this.lblTimKiem.TabIndex = 6;
            this.lblTimKiem.Text = "Tìm kiếm:";
            
            // lblLoaiMon
            this.lblLoaiMon.AutoSize = true;
            this.lblLoaiMon.Location = new System.Drawing.Point(290, 18);
            this.lblLoaiMon.Name = "lblLoaiMon";
            this.lblLoaiMon.Size = new System.Drawing.Size(53, 13);
            this.lblLoaiMon.TabIndex = 7;
            this.lblLoaiMon.Text = "Loại món:";
            // 
            // btnLoaiMon
            // 
            this.btnLoaiMon.Location = new System.Drawing.Point(510, 14);
            this.btnLoaiMon.Name = "btnLoaiMon";
            this.btnLoaiMon.Size = new System.Drawing.Size(120, 23);
            this.btnLoaiMon.TabIndex = 8;
            this.btnLoaiMon.Text = "⚙ Quản lý loại món";
            this.btnLoaiMon.UseVisualStyleBackColor = true;
            this.btnLoaiMon.Click += new System.EventHandler(this.btnLoaiMon_Click);
            
            // MonAnForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnLoaiMon);
            this.Controls.Add(this.lblLoaiMon);
            this.Controls.Add(this.lblTimKiem);
            this.Controls.Add(this.cboLoaiMon);
            this.Controls.Add(this.txtTimKiem);
            this.Controls.Add(this.btnNgungKinhDoanh);
            this.Controls.Add(this.btnSua);
            this.Controls.Add(this.btnThem);
            this.Controls.Add(this.dgvMonAn);
            this.Name = "MonAnForm";
            this.Text = "Quản Lý Món Ăn";
            this.Load += new System.EventHandler(this.MonAnForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonAn)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}


namespace RestaurantCRM.AdminApp.Forms.MonAn {
    partial class MonAnDialog {
        private System.ComponentModel.IContainer components = null;

        // Dải Header hiện đại phía trên
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblFormTitle;
        private System.Windows.Forms.Label lblFormSub;
        private System.Windows.Forms.Panel pnlHeaderDivider;

        // Cột trái: Thông tin cơ bản
        private System.Windows.Forms.GroupBox grpThongTinCoBan;
        private System.Windows.Forms.Label lblTenMon;
        private System.Windows.Forms.TextBox txtTenMon;
        private System.Windows.Forms.Label lblLoaiMon;
        private System.Windows.Forms.ComboBox cboLoaiMon;
        private System.Windows.Forms.Label lblGia;
        private System.Windows.Forms.NumericUpDown numGia;
        private System.Windows.Forms.CheckBox chkTrangThai;
        private System.Windows.Forms.Label lblMoTa;
        private System.Windows.Forms.TextBox txtMoTa;
        private System.Windows.Forms.Label lblHinhAnh;
        private System.Windows.Forms.TextBox txtHinhAnhUrl;
        private System.Windows.Forms.Button btnChonHinh;
        private System.Windows.Forms.PictureBox picHinhAnh;

        // Cột phải: Định lượng công thức (BOM)
        private System.Windows.Forms.GroupBox grpDinhLuong;
        private System.Windows.Forms.Panel pnlThemDinhLuong;
        private System.Windows.Forms.Label lblChonNL;
        private System.Windows.Forms.ComboBox cboNguyenLieu;
        private System.Windows.Forms.Button btnThemNguyenLieuMoi;
        private System.Windows.Forms.Label lblSoLuongCan;
        private System.Windows.Forms.NumericUpDown numSoLuongCan;
        private System.Windows.Forms.Label lblDvtInfo;
        private System.Windows.Forms.Label lblGiaNhapInfo;
        private System.Windows.Forms.Button btnThemNL;
        private System.Windows.Forms.DataGridView dgvDinhLuong;
        private System.Windows.Forms.Panel pnlCogsSummary;
        private System.Windows.Forms.Label lblGiaVonTitle;
        private System.Windows.Forms.Label lblGiaVonValue;
        private System.Windows.Forms.Label lblLoiNhuanTitle;
        private System.Windows.Forms.Label lblLoiNhuanGopValue;
        private System.Windows.Forms.Label lblCanhBaoGia;

        // Nút bấm chính
        private System.Windows.Forms.Button btnLuu;
        private System.Windows.Forms.Button btnHuy;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblFormTitle = new System.Windows.Forms.Label();
            this.lblFormSub = new System.Windows.Forms.Label();
            this.pnlHeaderDivider = new System.Windows.Forms.Panel();

            this.grpThongTinCoBan = new System.Windows.Forms.GroupBox();
            this.lblTenMon = new System.Windows.Forms.Label();
            this.txtTenMon = new System.Windows.Forms.TextBox();
            this.lblLoaiMon = new System.Windows.Forms.Label();
            this.cboLoaiMon = new System.Windows.Forms.ComboBox();
            this.lblGia = new System.Windows.Forms.Label();
            this.numGia = new System.Windows.Forms.NumericUpDown();
            this.chkTrangThai = new System.Windows.Forms.CheckBox();
            this.lblMoTa = new System.Windows.Forms.Label();
            this.txtMoTa = new System.Windows.Forms.TextBox();
            this.lblHinhAnh = new System.Windows.Forms.Label();
            this.txtHinhAnhUrl = new System.Windows.Forms.TextBox();
            this.btnChonHinh = new System.Windows.Forms.Button();
            this.picHinhAnh = new System.Windows.Forms.PictureBox();

            this.grpDinhLuong = new System.Windows.Forms.GroupBox();
            this.pnlThemDinhLuong = new System.Windows.Forms.Panel();
            this.lblChonNL = new System.Windows.Forms.Label();
            this.cboNguyenLieu = new System.Windows.Forms.ComboBox();
            this.btnThemNguyenLieuMoi = new System.Windows.Forms.Button();
            this.lblSoLuongCan = new System.Windows.Forms.Label();
            this.numSoLuongCan = new System.Windows.Forms.NumericUpDown();
            this.lblDvtInfo = new System.Windows.Forms.Label();
            this.lblGiaNhapInfo = new System.Windows.Forms.Label();
            this.btnThemNL = new System.Windows.Forms.Button();
            this.dgvDinhLuong = new System.Windows.Forms.DataGridView();
            this.pnlCogsSummary = new System.Windows.Forms.Panel();
            this.lblGiaVonTitle = new System.Windows.Forms.Label();
            this.lblGiaVonValue = new System.Windows.Forms.Label();
            this.lblLoiNhuanTitle = new System.Windows.Forms.Label();
            this.lblLoiNhuanGopValue = new System.Windows.Forms.Label();
            this.lblCanhBaoGia = new System.Windows.Forms.Label();

            this.btnLuu = new System.Windows.Forms.Button();
            this.btnHuy = new System.Windows.Forms.Button();

            this.pnlHeader.SuspendLayout();
            this.grpThongTinCoBan.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numGia)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHinhAnh)).BeginInit();
            this.grpDinhLuong.SuspendLayout();
            this.pnlThemDinhLuong.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSoLuongCan)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDinhLuong)).BeginInit();
            this.pnlCogsSummary.SuspendLayout();
            this.SuspendLayout();

            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.White;
            this.pnlHeader.Controls.Add(this.pnlHeaderDivider);
            this.pnlHeader.Controls.Add(this.lblFormSub);
            this.pnlHeader.Controls.Add(this.lblFormTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1024, 60);
            this.pnlHeader.TabIndex = 4;

            // 
            // lblFormTitle
            // 
            this.lblFormTitle.AutoSize = true;
            this.lblFormTitle.Font = new System.Drawing.Font("Segoe UI", 12.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblFormTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblFormTitle.Location = new System.Drawing.Point(18, 9);
            this.lblFormTitle.Name = "lblFormTitle";
            this.lblFormTitle.Size = new System.Drawing.Size(465, 23);
            this.lblFormTitle.TabIndex = 0;
            this.lblFormTitle.Text = "🍲 Thiết Lập Món Ăn Mới & Công Thức Định Lượng (BOM)";

            // 
            // lblFormSub
            // 
            this.lblFormSub.AutoSize = true;
            this.lblFormSub.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblFormSub.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblFormSub.Location = new System.Drawing.Point(21, 34);
            this.lblFormSub.Name = "lblFormSub";
            this.lblFormSub.Size = new System.Drawing.Size(532, 15);
            this.lblFormSub.TabIndex = 1;
            this.lblFormSub.Text = "Khai báo thông tin chi tiết món ăn, giá bán và định mức nguyên vật liệu tiêu hao từ kho khi bán hàng.";

            // 
            // pnlHeaderDivider
            // 
            this.pnlHeaderDivider.BackColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.pnlHeaderDivider.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlHeaderDivider.Location = new System.Drawing.Point(0, 59);
            this.pnlHeaderDivider.Name = "pnlHeaderDivider";
            this.pnlHeaderDivider.Size = new System.Drawing.Size(1024, 1);
            this.pnlHeaderDivider.TabIndex = 2;

            // 
            // grpThongTinCoBan
            // 
            this.grpThongTinCoBan.BackColor = System.Drawing.Color.White;
            this.grpThongTinCoBan.Controls.Add(this.picHinhAnh);
            this.grpThongTinCoBan.Controls.Add(this.btnChonHinh);
            this.grpThongTinCoBan.Controls.Add(this.txtHinhAnhUrl);
            this.grpThongTinCoBan.Controls.Add(this.lblHinhAnh);
            this.grpThongTinCoBan.Controls.Add(this.txtMoTa);
            this.grpThongTinCoBan.Controls.Add(this.lblMoTa);
            this.grpThongTinCoBan.Controls.Add(this.chkTrangThai);
            this.grpThongTinCoBan.Controls.Add(this.numGia);
            this.grpThongTinCoBan.Controls.Add(this.lblGia);
            this.grpThongTinCoBan.Controls.Add(this.cboLoaiMon);
            this.grpThongTinCoBan.Controls.Add(this.lblLoaiMon);
            this.grpThongTinCoBan.Controls.Add(this.txtTenMon);
            this.grpThongTinCoBan.Controls.Add(this.lblTenMon);
            this.grpThongTinCoBan.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.grpThongTinCoBan.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.grpThongTinCoBan.Location = new System.Drawing.Point(16, 72);
            this.grpThongTinCoBan.Name = "grpThongTinCoBan";
            this.grpThongTinCoBan.Size = new System.Drawing.Size(425, 498);
            this.grpThongTinCoBan.TabIndex = 0;
            this.grpThongTinCoBan.TabStop = false;
            this.grpThongTinCoBan.Text = "  1. Thông tin món ăn & Giá bán  ";

            // 
            // lblTenMon
            // 
            this.lblTenMon.AutoSize = true;
            this.lblTenMon.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTenMon.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblTenMon.Location = new System.Drawing.Point(16, 26);
            this.lblTenMon.Name = "lblTenMon";
            this.lblTenMon.Size = new System.Drawing.Size(76, 15);
            this.lblTenMon.TabIndex = 0;
            this.lblTenMon.Text = "Tên món (*):";

            // 
            // txtTenMon
            // 
            this.txtTenMon.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtTenMon.Location = new System.Drawing.Point(16, 45);
            this.txtTenMon.Name = "txtTenMon";
            this.txtTenMon.Size = new System.Drawing.Size(392, 24);
            this.txtTenMon.TabIndex = 1;

            // 
            // lblLoaiMon
            // 
            this.lblLoaiMon.AutoSize = true;
            this.lblLoaiMon.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblLoaiMon.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblLoaiMon.Location = new System.Drawing.Point(16, 78);
            this.lblLoaiMon.Name = "lblLoaiMon";
            this.lblLoaiMon.Size = new System.Drawing.Size(79, 15);
            this.lblLoaiMon.TabIndex = 2;
            this.lblLoaiMon.Text = "Loại món (*):";

            // 
            // cboLoaiMon
            // 
            this.cboLoaiMon.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLoaiMon.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cboLoaiMon.FormattingEnabled = true;
            this.cboLoaiMon.Location = new System.Drawing.Point(16, 97);
            this.cboLoaiMon.Name = "cboLoaiMon";
            this.cboLoaiMon.Size = new System.Drawing.Size(186, 23);
            this.cboLoaiMon.TabIndex = 3;

            // 
            // lblGia
            // 
            this.lblGia.AutoSize = true;
            this.lblGia.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblGia.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblGia.Location = new System.Drawing.Point(218, 78);
            this.lblGia.Name = "lblGia";
            this.lblGia.Size = new System.Drawing.Size(110, 15);
            this.lblGia.TabIndex = 4;
            this.lblGia.Text = "Giá bán (VNĐ) (*):";

            // 
            // numGia
            // 
            this.numGia.DecimalPlaces = 0;
            this.numGia.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.numGia.ForeColor = System.Drawing.Color.FromArgb(2, 132, 199);
            this.numGia.Increment = new decimal(new int[] { 5000, 0, 0, 0 });
            this.numGia.Location = new System.Drawing.Point(218, 96);
            this.numGia.Maximum = new decimal(new int[] { 100000000, 0, 0, 0 });
            this.numGia.Name = "numGia";
            this.numGia.Size = new System.Drawing.Size(190, 25);
            this.numGia.TabIndex = 5;
            this.numGia.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numGia.ThousandsSeparator = true;

            // 
            // chkTrangThai
            // 
            this.chkTrangThai.AutoSize = true;
            this.chkTrangThai.Checked = true;
            this.chkTrangThai.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTrangThai.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.chkTrangThai.ForeColor = System.Drawing.Color.FromArgb(5, 150, 105);
            this.chkTrangThai.Location = new System.Drawing.Point(16, 130);
            this.chkTrangThai.Name = "chkTrangThai";
            this.chkTrangThai.Size = new System.Drawing.Size(232, 19);
            this.chkTrangThai.TabIndex = 6;
            this.chkTrangThai.Text = "✓ Đang áp dụng kinh doanh trên menu";
            this.chkTrangThai.UseVisualStyleBackColor = true;

            // 
            // lblMoTa
            // 
            this.lblMoTa.AutoSize = true;
            this.lblMoTa.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblMoTa.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.lblMoTa.Location = new System.Drawing.Point(16, 157);
            this.lblMoTa.Name = "lblMoTa";
            this.lblMoTa.Size = new System.Drawing.Size(73, 15);
            this.lblMoTa.TabIndex = 7;
            this.lblMoTa.Text = "Mô tả ngắn:";

            // 
            // txtMoTa
            // 
            this.txtMoTa.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtMoTa.Location = new System.Drawing.Point(16, 175);
            this.txtMoTa.Multiline = true;
            this.txtMoTa.Name = "txtMoTa";
            this.txtMoTa.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMoTa.Size = new System.Drawing.Size(392, 46);
            this.txtMoTa.TabIndex = 8;

            // 
            // lblHinhAnh
            // 
            this.lblHinhAnh.AutoSize = true;
            this.lblHinhAnh.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblHinhAnh.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.lblHinhAnh.Location = new System.Drawing.Point(16, 230);
            this.lblHinhAnh.Name = "lblHinhAnh";
            this.lblHinhAnh.Size = new System.Drawing.Size(107, 15);
            this.lblHinhAnh.TabIndex = 9;
            this.lblHinhAnh.Text = "Hình ảnh đại diện:";

            // 
            // txtHinhAnhUrl
            // 
            this.txtHinhAnhUrl.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.txtHinhAnhUrl.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtHinhAnhUrl.Location = new System.Drawing.Point(16, 249);
            this.txtHinhAnhUrl.Name = "txtHinhAnhUrl";
            this.txtHinhAnhUrl.ReadOnly = true;
            this.txtHinhAnhUrl.Size = new System.Drawing.Size(300, 23);
            this.txtHinhAnhUrl.TabIndex = 10;

            // 
            // btnChonHinh
            // 
            this.btnChonHinh.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.btnChonHinh.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.btnChonHinh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnChonHinh.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnChonHinh.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.btnChonHinh.Location = new System.Drawing.Point(322, 248);
            this.btnChonHinh.Name = "btnChonHinh";
            this.btnChonHinh.Size = new System.Drawing.Size(86, 25);
            this.btnChonHinh.TabIndex = 11;
            this.btnChonHinh.Text = "📁 Chọn ảnh";
            this.btnChonHinh.UseVisualStyleBackColor = false;
            this.btnChonHinh.Click += new System.EventHandler(this.btnChonHinh_Click);

            // 
            // picHinhAnh
            // 
            this.picHinhAnh.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.picHinhAnh.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picHinhAnh.Location = new System.Drawing.Point(16, 280);
            this.picHinhAnh.Name = "picHinhAnh";
            this.picHinhAnh.Size = new System.Drawing.Size(392, 204);
            this.picHinhAnh.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picHinhAnh.TabIndex = 12;
            this.picHinhAnh.TabStop = false;

            // 
            // grpDinhLuong
            // 
            this.grpDinhLuong.BackColor = System.Drawing.Color.White;
            this.grpDinhLuong.Controls.Add(this.pnlCogsSummary);
            this.grpDinhLuong.Controls.Add(this.dgvDinhLuong);
            this.grpDinhLuong.Controls.Add(this.pnlThemDinhLuong);
            this.grpDinhLuong.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.grpDinhLuong.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.grpDinhLuong.Location = new System.Drawing.Point(452, 72);
            this.grpDinhLuong.Name = "grpDinhLuong";
            this.grpDinhLuong.Size = new System.Drawing.Size(556, 498);
            this.grpDinhLuong.TabIndex = 1;
            this.grpDinhLuong.TabStop = false;
            this.grpDinhLuong.Text = "  2. Công thức định lượng nguyên vật liệu (BOM)  ";

            // 
            // pnlThemDinhLuong
            // 
            this.pnlThemDinhLuong.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.pnlThemDinhLuong.Controls.Add(this.btnThemNL);
            this.pnlThemDinhLuong.Controls.Add(this.lblGiaNhapInfo);
            this.pnlThemDinhLuong.Controls.Add(this.lblDvtInfo);
            this.pnlThemDinhLuong.Controls.Add(this.numSoLuongCan);
            this.pnlThemDinhLuong.Controls.Add(this.lblSoLuongCan);
            this.pnlThemDinhLuong.Controls.Add(this.btnThemNguyenLieuMoi);
            this.pnlThemDinhLuong.Controls.Add(this.cboNguyenLieu);
            this.pnlThemDinhLuong.Controls.Add(this.lblChonNL);
            this.pnlThemDinhLuong.Location = new System.Drawing.Point(14, 25);
            this.pnlThemDinhLuong.Name = "pnlThemDinhLuong";
            this.pnlThemDinhLuong.Size = new System.Drawing.Size(528, 76);
            this.pnlThemDinhLuong.TabIndex = 0;

            // 
            // lblChonNL
            // 
            this.lblChonNL.AutoSize = true;
            this.lblChonNL.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblChonNL.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblChonNL.Location = new System.Drawing.Point(10, 12);
            this.lblChonNL.Name = "lblChonNL";
            this.lblChonNL.Size = new System.Drawing.Size(126, 15);
            this.lblChonNL.TabIndex = 0;
            this.lblChonNL.Text = "Chọn nguyên liệu (*):";

            // 
            // cboNguyenLieu
            // 
            this.cboNguyenLieu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboNguyenLieu.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cboNguyenLieu.FormattingEnabled = true;
            this.cboNguyenLieu.Location = new System.Drawing.Point(140, 9);
            this.cboNguyenLieu.Name = "cboNguyenLieu";
            this.cboNguyenLieu.Size = new System.Drawing.Size(195, 23);
            this.cboNguyenLieu.TabIndex = 1;

            // 
            // btnThemNguyenLieuMoi
            // 
            this.btnThemNguyenLieuMoi.BackColor = System.Drawing.Color.FromArgb(238, 242, 255);
            this.btnThemNguyenLieuMoi.FlatAppearance.BorderSize = 0;
            this.btnThemNguyenLieuMoi.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnThemNguyenLieuMoi.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnThemNguyenLieuMoi.ForeColor = System.Drawing.Color.FromArgb(79, 70, 229);
            this.btnThemNguyenLieuMoi.Location = new System.Drawing.Point(340, 8);
            this.btnThemNguyenLieuMoi.Name = "btnThemNguyenLieuMoi";
            this.btnThemNguyenLieuMoi.Size = new System.Drawing.Size(35, 25);
            this.btnThemNguyenLieuMoi.TabIndex = 2;
            this.btnThemNguyenLieuMoi.Text = "＋";
            this.btnThemNguyenLieuMoi.UseVisualStyleBackColor = false;
            this.btnThemNguyenLieuMoi.Click += new System.EventHandler(this.btnThemNguyenLieuMoi_Click);

            // 
            // lblSoLuongCan
            // 
            this.lblSoLuongCan.AutoSize = true;
            this.lblSoLuongCan.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblSoLuongCan.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblSoLuongCan.Location = new System.Drawing.Point(10, 44);
            this.lblSoLuongCan.Name = "lblSoLuongCan";
            this.lblSoLuongCan.Size = new System.Drawing.Size(95, 15);
            this.lblSoLuongCan.TabIndex = 2;
            this.lblSoLuongCan.Text = "Định lượng cần:";

            // 
            // numSoLuongCan
            // 
            this.numSoLuongCan.DecimalPlaces = 2;
            this.numSoLuongCan.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.numSoLuongCan.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            this.numSoLuongCan.Location = new System.Drawing.Point(140, 41);
            this.numSoLuongCan.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            this.numSoLuongCan.Name = "numSoLuongCan";
            this.numSoLuongCan.Size = new System.Drawing.Size(78, 24);
            this.numSoLuongCan.TabIndex = 3;
            this.numSoLuongCan.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numSoLuongCan.Value = new decimal(new int[] { 1, 0, 0, 0 });

            // 
            // lblDvtInfo
            // 
            this.lblDvtInfo.AutoSize = true;
            this.lblDvtInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblDvtInfo.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.lblDvtInfo.Location = new System.Drawing.Point(222, 44);
            this.lblDvtInfo.Name = "lblDvtInfo";
            this.lblDvtInfo.Size = new System.Drawing.Size(20, 15);
            this.lblDvtInfo.TabIndex = 4;
            this.lblDvtInfo.Text = "kg";

            // 
            // lblGiaNhapInfo
            // 
            this.lblGiaNhapInfo.AutoSize = true;
            this.lblGiaNhapInfo.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblGiaNhapInfo.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblGiaNhapInfo.Location = new System.Drawing.Point(252, 45);
            this.lblGiaNhapInfo.Name = "lblGiaNhapInfo";
            this.lblGiaNhapInfo.Size = new System.Drawing.Size(115, 13);
            this.lblGiaNhapInfo.TabIndex = 5;
            this.lblGiaNhapInfo.Text = "Giá nhập: 0 đ/đơn vị";

            // 
            // btnThemNL
            // 
            this.btnThemNL.BackColor = System.Drawing.Color.FromArgb(5, 150, 105);
            this.btnThemNL.FlatAppearance.BorderSize = 0;
            this.btnThemNL.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnThemNL.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnThemNL.ForeColor = System.Drawing.Color.White;
            this.btnThemNL.Location = new System.Drawing.Point(384, 8);
            this.btnThemNL.Name = "btnThemNL";
            this.btnThemNL.Size = new System.Drawing.Size(134, 58);
            this.btnThemNL.TabIndex = 6;
            this.btnThemNL.Text = "＋ Thêm vào\r\n    công thức";
            this.btnThemNL.UseVisualStyleBackColor = false;
            this.btnThemNL.Click += new System.EventHandler(this.btnThemNL_Click);

            // 
            // dgvDinhLuong
            // 
            this.dgvDinhLuong.AllowUserToAddRows = false;
            this.dgvDinhLuong.AllowUserToDeleteRows = false;
            this.dgvDinhLuong.BackgroundColor = System.Drawing.Color.White;
            this.dgvDinhLuong.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvDinhLuong.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDinhLuong.Location = new System.Drawing.Point(14, 109);
            this.dgvDinhLuong.Name = "dgvDinhLuong";
            this.dgvDinhLuong.ReadOnly = true;
            this.dgvDinhLuong.RowHeadersVisible = false;
            this.dgvDinhLuong.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDinhLuong.Size = new System.Drawing.Size(528, 260);
            this.dgvDinhLuong.TabIndex = 1;

            // 
            // pnlCogsSummary
            // 
            this.pnlCogsSummary.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.pnlCogsSummary.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCogsSummary.Controls.Add(this.lblCanhBaoGia);
            this.pnlCogsSummary.Controls.Add(this.lblLoiNhuanGopValue);
            this.pnlCogsSummary.Controls.Add(this.lblLoiNhuanTitle);
            this.pnlCogsSummary.Controls.Add(this.lblGiaVonValue);
            this.pnlCogsSummary.Controls.Add(this.lblGiaVonTitle);
            this.pnlCogsSummary.Location = new System.Drawing.Point(14, 377);
            this.pnlCogsSummary.Name = "pnlCogsSummary";
            this.pnlCogsSummary.Size = new System.Drawing.Size(528, 108);
            this.pnlCogsSummary.TabIndex = 2;

            // 
            // lblGiaVonTitle
            // 
            this.lblGiaVonTitle.AutoSize = true;
            this.lblGiaVonTitle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblGiaVonTitle.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblGiaVonTitle.Location = new System.Drawing.Point(14, 12);
            this.lblGiaVonTitle.Name = "lblGiaVonTitle";
            this.lblGiaVonTitle.Size = new System.Drawing.Size(189, 13);
            this.lblGiaVonTitle.TabIndex = 0;
            this.lblGiaVonTitle.Text = "TỔNG GIÁ VỐN NGUYÊN LIỆU (COGS)";

            // 
            // lblGiaVonValue
            // 
            this.lblGiaVonValue.AutoSize = true;
            this.lblGiaVonValue.Font = new System.Drawing.Font("Segoe UI", 13.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblGiaVonValue.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            this.lblGiaVonValue.Location = new System.Drawing.Point(13, 30);
            this.lblGiaVonValue.Name = "lblGiaVonValue";
            this.lblGiaVonValue.Size = new System.Drawing.Size(43, 25);
            this.lblGiaVonValue.TabIndex = 1;
            this.lblGiaVonValue.Text = "0 đ";

            // 
            // lblLoiNhuanTitle
            // 
            this.lblLoiNhuanTitle.AutoSize = true;
            this.lblLoiNhuanTitle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblLoiNhuanTitle.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblLoiNhuanTitle.Location = new System.Drawing.Point(265, 12);
            this.lblLoiNhuanTitle.Name = "lblLoiNhuanTitle";
            this.lblLoiNhuanTitle.Size = new System.Drawing.Size(193, 13);
            this.lblLoiNhuanTitle.TabIndex = 2;
            this.lblLoiNhuanTitle.Text = "LỢI NHUẬN GỘP DỰ KIẾN (TỶ SUẤT %)";

            // 
            // lblLoiNhuanGopValue
            // 
            this.lblLoiNhuanGopValue.AutoSize = true;
            this.lblLoiNhuanGopValue.Font = new System.Drawing.Font("Segoe UI", 13.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblLoiNhuanGopValue.ForeColor = System.Drawing.Color.FromArgb(5, 150, 105);
            this.lblLoiNhuanGopValue.Location = new System.Drawing.Point(264, 30);
            this.lblLoiNhuanGopValue.Name = "lblLoiNhuanGopValue";
            this.lblLoiNhuanGopValue.Size = new System.Drawing.Size(120, 25);
            this.lblLoiNhuanGopValue.TabIndex = 3;
            this.lblLoiNhuanGopValue.Text = "0 đ (0.0%)";

            // 
            // lblCanhBaoGia
            // 
            this.lblCanhBaoGia.AutoSize = true;
            this.lblCanhBaoGia.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblCanhBaoGia.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            this.lblCanhBaoGia.Location = new System.Drawing.Point(14, 68);
            this.lblCanhBaoGia.Name = "lblCanhBaoGia";
            this.lblCanhBaoGia.Size = new System.Drawing.Size(268, 15);
            this.lblCanhBaoGia.TabIndex = 4;
            this.lblCanhBaoGia.Text = "⚠️ Cảnh báo: Giá bán đang thấp hơn giá vốn!";
            this.lblCanhBaoGia.Visible = false;

            // 
            // btnLuu
            // 
            this.btnLuu.BackColor = System.Drawing.Color.FromArgb(79, 70, 229);
            this.btnLuu.FlatAppearance.BorderSize = 0;
            this.btnLuu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLuu.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnLuu.ForeColor = System.Drawing.Color.White;
            this.btnLuu.Location = new System.Drawing.Point(725, 582);
            this.btnLuu.Name = "btnLuu";
            this.btnLuu.Size = new System.Drawing.Size(155, 38);
            this.btnLuu.TabIndex = 2;
            this.btnLuu.Text = "💾 Lưu Món Ăn";
            this.btnLuu.UseVisualStyleBackColor = false;
            this.btnLuu.Click += new System.EventHandler(this.btnLuu_Click);

            // 
            // btnHuy
            // 
            this.btnHuy.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.btnHuy.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.btnHuy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHuy.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnHuy.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.btnHuy.Location = new System.Drawing.Point(892, 582);
            this.btnHuy.Name = "btnHuy";
            this.btnHuy.Size = new System.Drawing.Size(116, 38);
            this.btnHuy.TabIndex = 3;
            this.btnHuy.Text = "✕ Đóng";
            this.btnHuy.UseVisualStyleBackColor = false;
            this.btnHuy.Click += new System.EventHandler(this.btnHuy_Click);

            // 
            // MonAnDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.ClientSize = new System.Drawing.Size(1024, 632);
            this.Controls.Add(this.btnHuy);
            this.Controls.Add(this.btnLuu);
            this.Controls.Add(this.grpDinhLuong);
            this.Controls.Add(this.grpThongTinCoBan);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MonAnDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Thiết lập Món Ăn & Công thức Định Lượng (BOM)";
            this.Load += new System.EventHandler(this.MonAnDialog_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.grpThongTinCoBan.ResumeLayout(false);
            this.grpThongTinCoBan.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numGia)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHinhAnh)).EndInit();
            this.grpDinhLuong.ResumeLayout(false);
            this.pnlThemDinhLuong.ResumeLayout(false);
            this.pnlThemDinhLuong.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSoLuongCan)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDinhLuong)).EndInit();
            this.pnlCogsSummary.ResumeLayout(false);
            this.pnlCogsSummary.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}

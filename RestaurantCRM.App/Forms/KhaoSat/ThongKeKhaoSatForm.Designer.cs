namespace RestaurantCRM.AdminApp.Forms.KhaoSat
{
    partial class ThongKeKhaoSatForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Text        = "Thống kê kết quả khảo sát";
            this.Size        = new System.Drawing.Size(800, 620);
            this.MinimumSize = new System.Drawing.Size(720, 480);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Forms.BanHang
{
    // ==============================================================
    // DIALOG XÁC NHẬN CHI TIẾT ĐƠN HÀNG TRƯỚC KHI TẠO HÓA ĐƠN
    // ==============================================================
    public class XacNhanDonHangDialog : Form
    {
        public XacNhanDonHangDialog(
            string tenKhach,
            string sdt,
            string ghiChu,
            string staffName,
            List<TaoDonHangForm.CartItem> items,
            decimal subTotal,
            VoucherDto appliedVoucher,
            decimal voucherDiscount,
            decimal finalTotal,
            string phuongThuc = "Tiền mặt (Tại quầy)")
        {
            Text = "Xác nhận & Kiểm tra chi tiết đơn hàng";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(580, 680);
            MinimumSize = new Size(520, 600);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);

            var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), BackColor = ThemeManager.Bg };
            Controls.Add(root);

            // 1. Header Dialog
            var lblTitle = new Label
            {
                Text = "🧾 Chi Tiết Đơn Hàng Chuẩn Bị Tạo",
                Location = new Point(0, 0),
                AutoSize = true,
                Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            };
            root.Controls.Add(lblTitle);

            var lblSub = new Label
            {
                Text = "Vui lòng rà soát lại thông tin khách hàng và các món đã chọn trước khi thanh toán.",
                Location = new Point(2, 28),
                Size = new Size(520, 20),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = ThemeManager.Muted
            };
            root.Controls.Add(lblSub);

            // 2. Thông tin khách & nhân viên (Card nhỏ)
            var pnlInfo = new RoundedPanel
            {
                Location = new Point(0, 54),
                Size = new Size(524, 76),
                Radius = 8,
                BorderWidth = 1,
                BorderColor = ThemeManager.Line,
                FillColor = Color.White
            };
            root.Controls.Add(pnlInfo);

            var lblCustomer = new Label
            {
                Text = $"Khách hàng: {tenKhach} {(string.IsNullOrEmpty(sdt) ? "" : "• SĐT: " + sdt)}",
                Location = new Point(12, 8),
                Size = new Size(500, 18),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            };
            pnlInfo.Controls.Add(lblCustomer);

            var lblStaff = new Label
            {
                Text = $"Nhân viên lập: {staffName}  •  Phương thức: {phuongThuc}",
                Location = new Point(12, 30),
                Size = new Size(500, 18),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = ThemeManager.Muted
            };
            pnlInfo.Controls.Add(lblStaff);

            var lblNote = new Label
            {
                Text = $"Ghi chú: {ghiChu}",
                Location = new Point(12, 50),
                Size = new Size(500, 18),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = ThemeManager.Ink2
            };
            pnlInfo.Controls.Add(lblNote);

            // 3. Bảng danh sách món ăn (DataGridView)
            var dgv = new DataGridView
            {
                Location = new Point(0, 138),
                Size = new Size(524, 280),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(241, 245, 249)
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.Ink;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 32;
            dgv.RowTemplate.Height = 28;

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Món ăn",
                DataPropertyName = "Name",
                Width = 220,
                DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeManager.Ink }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Đơn giá",
                DataPropertyName = "PriceText",
                Width = 95,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "SL",
                DataPropertyName = "Quantity",
                Width = 55,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Thành tiền",
                DataPropertyName = "SubTotalText",
                Width = 135,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeManager.Primary }
            });

            var displayList = items.Select(x => new
            {
                Name = x.Name,
                PriceText = $"{x.Price:N0} đ",
                Quantity = x.Quantity,
                SubTotalText = $"{x.Price * x.Quantity:N0} đ"
            }).ToList();
            dgv.DataSource = displayList;
            root.Controls.Add(dgv);

            // 4. Khung tổng kết thanh toán (Card nhỏ dưới bảng)
            var pnlSummary = new RoundedPanel
            {
                Location = new Point(0, 426),
                Size = new Size(524, 134),
                Radius = 8,
                BorderWidth = 1,
                BorderColor = ThemeManager.Line,
                FillColor = Color.White,
                Padding = new Padding(14, 8, 14, 8)
            };
            root.Controls.Add(pnlSummary);

            int sumY = 8;
            Action<string, string, Color, bool> addSummaryRow = (label, val, color, isBold) =>
            {
                var lblL = new Label
                {
                    Text = label,
                    Location = new Point(14, sumY),
                    Size = new Size(240, 20),
                    Font = new Font("Segoe UI", isBold ? 10F : 9F, isBold ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = isBold ? ThemeManager.Ink : ThemeManager.Muted
                };
                var lblR = new Label
                {
                    Text = val,
                    Location = new Point(260, sumY),
                    Size = new Size(248, 20),
                    TextAlign = ContentAlignment.MiddleRight,
                    Font = new Font("Segoe UI", isBold ? 11F : 9F, isBold ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = color
                };
                pnlSummary.Controls.Add(lblL);
                pnlSummary.Controls.Add(lblR);
                sumY += 24;
            };

            int totalQty = items.Sum(x => x.Quantity);
            addSummaryRow($"Tiền món ({items.Count} món · {totalQty} phần):", $"{subTotal:N0} đ", ThemeManager.Ink, false);

            if (appliedVoucher != null && voucherDiscount > 0)
            {
                addSummaryRow($"Voucher ưu đãi [{appliedVoucher.MaVoucher}]:", $"-{voucherDiscount:N0} đ", ThemeManager.Green, false);
            }
            else
            {
                addSummaryRow("Khuyến mãi / Voucher:", "0 đ", ThemeManager.Muted, false);
            }

            var line = new Panel { Location = new Point(14, sumY + 2), Size = new Size(496, 1), BackColor = ThemeManager.Line };
            pnlSummary.Controls.Add(line);
            sumY += 8;

            addSummaryRow("TỔNG THANH TOÁN:", $"{finalTotal:N0} đ", ThemeManager.Primary, true);

            // 5. Thanh nút bấm
            var btnConfirm = new Button
            {
                Text = "✓  Tạo đơn hàng",
                Location = new Point(274, 576),
                Size = new Size(250, 44),
                BackColor = ThemeManager.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.Click += (s, e) => {
                DialogResult = DialogResult.OK;
                Close();
            };
            root.Controls.Add(btnConfirm);

            var btnCancel = new Button
            {
                Text = "Quay lại sửa đơn",
                Location = new Point(0, 576),
                Size = new Size(260, 44),
                BackColor = Color.White,
                ForeColor = ThemeManager.Ink,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = ThemeManager.Line;
            btnCancel.Click += (s, e) => {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            root.Controls.Add(btnCancel);
        }
    }
}

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
    // DIALOG CHỌN HOẶC NHẬP MÃ VOUCHER THỦ CÔNG
    // ==============================================================
    public class ChonVoucherDialog : Form
    {
        public VoucherDto SelectedVoucher { get; private set; }

        public ChonVoucherDialog(decimal currentSubTotal, List<VoucherDto> vouchers, VoucherDto currentApplied)
        {
            Text = "Chọn Voucher & Khuyến Mãi Hóa Đơn";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(480, 560);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);

            SelectedVoucher = currentApplied;

            var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), BackColor = ThemeManager.Bg };
            Controls.Add(root);

            var lblHeader = new Label
            {
                Text = "🎟️ Danh Sách Ưu Đãi & Voucher",
                Location = new Point(0, 0),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            };
            root.Controls.Add(lblHeader);

            var lblSub = new Label
            {
                Text = $"Tổng tiền món hiện tại: {currentSubTotal:N0} đ",
                Location = new Point(2, 28),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(14, 124, 134)
            };
            root.Controls.Add(lblSub);

            // Khung nhập mã thủ công
            var pnlInput = new Panel
            {
                Location = new Point(0, 56),
                Size = new Size(424, 38),
                BackColor = Color.White
            };
            root.Controls.Add(pnlInput);

            var txtCode = new TextBox
            {
                Location = new Point(8, 7),
                Width = 280,
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeManager.Muted,
                Text = "Nhập mã voucher (VD: GIAM20K)..."
            };
            txtCode.GotFocus += (s, e) => {
                if (txtCode.Text == "Nhập mã voucher (VD: GIAM20K)...")
                {
                    txtCode.Text = "";
                    txtCode.ForeColor = ThemeManager.Ink;
                }
            };
            txtCode.LostFocus += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtCode.Text))
                {
                    txtCode.Text = "Nhập mã voucher (VD: GIAM20K)...";
                    txtCode.ForeColor = ThemeManager.Muted;
                }
            };
            pnlInput.Controls.Add(txtCode);

            var btnApplyCode = new Button
            {
                Text = "Áp dụng",
                Location = new Point(296, 4),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(14, 124, 134),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnApplyCode.FlatAppearance.BorderSize = 0;
            btnApplyCode.Click += (s, e) => {
                string input = (txtCode.Text == "Nhập mã voucher (VD: GIAM20K)..." ? "" : txtCode.Text).Trim();
                if (string.IsNullOrEmpty(input))
                {
                    MessageBox.Show(this, "Vui lòng nhập mã voucher.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var found = vouchers.FirstOrDefault(v => string.Equals(v.MaVoucher, input, StringComparison.OrdinalIgnoreCase)
                                                      || string.Equals(v.MaKmvoucher, input, StringComparison.OrdinalIgnoreCase));
                if (found == null)
                {
                    MessageBox.Show(this, $"Mã voucher '{input}' không tồn tại hoặc đã hết hạn.", "Mã không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (found.TrangThai != "Đang áp dụng")
                {
                    MessageBox.Show(this, $"Mã voucher '{found.MaVoucher}' hiện ở trạng thái: {found.TrangThai}.", "Chưa thể áp dụng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                decimal minOrder = (decimal)(found.GiaTriDonToiThieu ?? 0);
                if (currentSubTotal < minOrder)
                {
                    MessageBox.Show(this, $"Mã '{found.MaVoucher}' yêu cầu đơn tối thiểu {minOrder:N0} đ. Hiện tại giỏ hàng mới đạt {currentSubTotal:N0} đ.", "Chưa đủ điều kiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                SelectedVoucher = found;
                DialogResult = DialogResult.OK;
                Close();
            };
            pnlInput.Controls.Add(btnApplyCode);

            // Danh sách các voucher khả dụng
            var flow = new FlowLayoutPanel
            {
                Location = new Point(0, 104),
                Size = new Size(424, 320),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = ThemeManager.Bg
            };
            root.Controls.Add(flow);

            var activeList = vouchers.Where(v => v.TrangThai == "Đang áp dụng").ToList();
            if (!activeList.Any())
            {
                flow.Controls.Add(new Label
                {
                    Text = "Hiện không có voucher nào đang diễn ra.",
                    ForeColor = ThemeManager.Muted,
                    Margin = new Padding(10),
                    AutoSize = true
                });
            }
            else
            {
                foreach (var v in activeList)
                {
                    decimal minOrder = (decimal)(v.GiaTriDonToiThieu ?? 0);
                    bool isEligible = currentSubTotal >= minOrder;
                    bool isCurrent = currentApplied != null && (currentApplied.MaVoucher == v.MaVoucher || currentApplied.MaKmvoucher == v.MaKmvoucher);

                    var card = new RoundedPanel
                    {
                        Size = new Size(398, 76),
                        Margin = new Padding(0, 0, 0, 8),
                        Radius = 8,
                        BorderWidth = 1,
                        BorderColor = isCurrent ? Color.FromArgb(14, 124, 134) : isEligible ? Color.FromArgb(203, 213, 225) : Color.FromArgb(226, 232, 240),
                        FillColor = isCurrent ? Color.FromArgb(240, 253, 250) : Color.White
                    };

                    var lblCode = new Label
                    {
                        Text = $"🎟️ {v.MaVoucher}",
                        Location = new Point(10, 8),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(29, 78, 216)
                    };
                    card.Controls.Add(lblCode);

                    var lblDiscountDesc = new Label
                    {
                        Text = v.HinhThuc + (v.GiamToiDa.HasValue && v.GiamToiDa > 0 ? $" (Tối đa {v.GiamToiDa:N0} đ)" : ""),
                        Location = new Point(10, 30),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(22, 163, 74)
                    };
                    card.Controls.Add(lblDiscountDesc);

                    var lblCondition = new Label
                    {
                        Text = minOrder > 0 ? $"Đơn tối thiểu: {minOrder:N0} đ" : "Áp dụng cho mọi giá trị đơn",
                        Location = new Point(10, 50),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 8F),
                        ForeColor = ThemeManager.Muted
                    };
                    card.Controls.Add(lblCondition);

                    if (isEligible)
                    {
                        var btnSelect = new Button
                        {
                            Text = isCurrent ? "Đang chọn" : "Chọn mã",
                            Location = new Point(292, 22),
                            Size = new Size(94, 32),
                            BackColor = isCurrent ? Color.FromArgb(22, 163, 74) : Color.FromArgb(14, 124, 134),
                            ForeColor = Color.White,
                            FlatStyle = FlatStyle.Flat,
                            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                            Cursor = Cursors.Hand
                        };
                        btnSelect.FlatAppearance.BorderSize = 0;
                        btnSelect.Click += (s, e) => {
                            SelectedVoucher = v;
                            DialogResult = DialogResult.OK;
                            Close();
                        };
                        card.Controls.Add(btnSelect);
                    }
                    else
                    {
                        var lblMissing = new Label
                        {
                            Text = $"Thiếu {(minOrder - currentSubTotal):N0} đ",
                            Location = new Point(280, 28),
                            Size = new Size(110, 24),
                            TextAlign = ContentAlignment.MiddleRight,
                            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                            ForeColor = ThemeManager.Amber
                        };
                        card.Controls.Add(lblMissing);
                    }

                    flow.Controls.Add(card);
                }
            }

            // Thanh nút bấm dưới cùng
            var btnClearVoucher = new Button
            {
                Text = "Bỏ dùng voucher",
                Location = new Point(0, 436),
                Size = new Size(160, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = ThemeManager.Red,
                Font = new Font("Segoe UI", 9F)
            };
            btnClearVoucher.FlatAppearance.BorderColor = Color.FromArgb(254, 202, 202);
            btnClearVoucher.Click += (s, e) => {
                SelectedVoucher = null;
                DialogResult = DialogResult.OK;
                Close();
            };
            root.Controls.Add(btnClearVoucher);

            var btnClose = new Button
            {
                Text = "Đóng",
                Location = new Point(304, 436),
                Size = new Size(120, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = ThemeManager.Ink,
                Font = new Font("Segoe UI", 9F)
            };
            btnClose.FlatAppearance.BorderColor = ThemeManager.Line;
            btnClose.Click += (s, e) => {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            root.Controls.Add(btnClose);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services.BanHang;

namespace RestaurantCRM.AdminApp.Forms.BanHang
{
    // ==============================================================
    // DIALOG CHỌN HOẶC NHẬP MÃ VOUCHER THỦ CÔNG & ĐỔI VOUCHER MÓN
    // ==============================================================
    public class ChonVoucherDialog : Form
    {
        public VoucherDto SelectedVoucher { get; private set; }

        public ChonVoucherDialog(
            decimal currentSubTotal, 
            List<VoucherDto> vouchers, 
            VoucherDto currentApplied, 
            IEnumerable<CartItem> cartItems = null)
        {
            Text = "Chọn Voucher & Khuyến Mãi Hóa Đơn";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(520, 600);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);

            SelectedVoucher = currentApplied;
            var calcService = new OrderCalculationService();
            var currentCartList = (cartItems ?? Enumerable.Empty<CartItem>()).ToList();

            var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), BackColor = ThemeManager.Bg };
            Controls.Add(root);

            var lblHeader = new Label
            {
                Text = "Danh Sách Ưu Đãi & Voucher",
                Location = new Point(0, 0),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            };
            root.Controls.Add(lblHeader);

            var lblSub = new Label
            {
                Text = $"Tổng tiền món: {currentSubTotal:N0} đ",
                Location = new Point(2, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(14, 124, 134)
            };
            root.Controls.Add(lblSub);

            var chkShowAll = new CheckBox
            {
                Text = "Hiện cả mã chưa đủ ĐK",
                Location = new Point(280, 28),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.75F),
                ForeColor = ThemeManager.Ink2,
                Checked = true,
                Cursor = Cursors.Hand
            };
            root.Controls.Add(chkShowAll);

            // Khung nhập mã thủ công & tìm kiếm nhanh
            var pnlInput = new Panel
            {
                Location = new Point(0, 56),
                Size = new Size(464, 38),
                BackColor = Color.White
            };
            root.Controls.Add(pnlInput);

            string placeholderText = "Tìm hoặc nhập mã voucher (VD: GIAM20K)...";
            var txtCode = new TextBox
            {
                Location = new Point(8, 7),
                Width = 320,
                Font = new Font("Segoe UI", 10F),
                ForeColor = ThemeManager.Muted,
                Text = placeholderText
            };
            txtCode.GotFocus += (s, e) => {
                if (txtCode.Text == placeholderText)
                {
                    txtCode.Text = "";
                    txtCode.ForeColor = ThemeManager.Ink;
                }
            };
            txtCode.LostFocus += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtCode.Text))
                {
                    txtCode.Text = placeholderText;
                    txtCode.ForeColor = ThemeManager.Muted;
                }
            };
            pnlInput.Controls.Add(txtCode);

            var btnApplyCode = new Button
            {
                Text = "Áp dụng",
                Location = new Point(336, 4),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(14, 124, 134),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnApplyCode.FlatAppearance.BorderSize = 0;
            btnApplyCode.Click += (s, e) => {
                string input = (txtCode.Text == placeholderText ? "" : txtCode.Text).Trim();
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

                // Voucher theo món hoặc loại món đã được hệ thống tự động chọn mức cao nhất áp thẳng vào món
                if (found.PhamViApDung == "THEO_MON" || found.PhamViApDung == "THEO_LOAI")
                {
                    MessageBox.Show(this,
                        $"Mã '{found.MaVoucher}' là ưu đãi theo {(found.PhamViApDung == "THEO_LOAI" ? "loại món" : "món ăn")}.\n" +
                        "Các ưu đãi theo món và loại món được hệ thống tự động chọn mức giảm cao nhất và trừ thẳng vào đơn giá món ăn. Tại đây chỉ chọn voucher giảm theo toàn đơn hàng.",
                        "Ưu đãi tự động theo món", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var (isEligible, discount, missing) = calcService.EvaluateVoucher(found, currentSubTotal, currentCartList);
                if (!isEligible)
                {
                    decimal minOrder = (decimal)(found.GiaTriDonToiThieu ?? 0);
                    MessageBox.Show(this, $"Mã '{found.MaVoucher}' yêu cầu đơn tối thiểu {minOrder:N0} đ. Hiện tại giỏ hàng mới đạt {currentSubTotal:N0} đ (Cần thêm {missing:N0} đ).", "Chưa đủ điều kiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                SelectedVoucher = found;
                DialogResult = DialogResult.OK;
                Close();
            };
            pnlInput.Controls.Add(btnApplyCode);

            // Danh sách các voucher toàn đơn
            var flow = new FlowLayoutPanel
            {
                Location = new Point(0, 104),
                Size = new Size(464, 350),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = ThemeManager.Bg
            };
            root.Controls.Add(flow);

            Action renderVoucherList = () =>
            {
                string q = (txtCode.Text == placeholderText ? "" : txtCode.Text).Trim();
                bool includeIneligible = chkShowAll.Checked;

                flow.SuspendLayout();
                var oldControls = flow.Controls.Cast<Control>().ToList();
                flow.Controls.Clear();
                foreach (var c in oldControls) c.Dispose();

                var list = (vouchers ?? new List<VoucherDto>())
                    .Where(v => v.TrangThai == "Đang áp dụng" && v.PhamViApDung == "TOAN_DON")
                    .Where(v => UiStyler.MatchesSearch(q, v.MaVoucher, v.MaKmvoucher, v.HinhThuc))
                    .Select(v => new
                    {
                        Voucher = v,
                        Eval = calcService.EvaluateVoucher(v, currentSubTotal, currentCartList)
                    })
                    .Where(x => includeIneligible || x.Eval.isEligible)
                    .OrderByDescending(x => x.Eval.isEligible)
                    .ThenByDescending(x => x.Eval.discount)
                    .ToList();

                if (!list.Any())
                {
                    flow.Controls.Add(new Label
                    {
                        Text = "Không có voucher toàn đơn nào phù hợp với điều kiện lọc.",
                        ForeColor = ThemeManager.Muted,
                        Margin = new Padding(10),
                        AutoSize = true
                    });
                    flow.ResumeLayout();
                    return;
                }

                foreach (var item in list)
                {
                    var v = item.Voucher;
                    bool isEligible = item.Eval.isEligible;
                    decimal missing = item.Eval.missingAmount;
                    decimal minOrder = (decimal)(v.GiaTriDonToiThieu ?? 0);
                    bool isCurrentOrderVoucher = currentApplied != null
                        && (currentApplied.MaVoucher == v.MaVoucher || currentApplied.MaKmvoucher == v.MaKmvoucher);

                    var card = new RoundedPanel
                    {
                        Size = new Size(438, 86),
                        Margin = new Padding(0, 0, 0, 8),
                        Radius = 8,
                        BorderWidth = 1,
                        BorderColor = isCurrentOrderVoucher
                            ? Color.FromArgb(14, 124, 134)
                            : (isEligible ? Color.FromArgb(203, 213, 225) : Color.FromArgb(226, 232, 240)),
                        FillColor = isCurrentOrderVoucher
                            ? Color.FromArgb(240, 253, 250)
                            : (isEligible ? Color.White : Color.FromArgb(248, 250, 252))
                    };

                    var lblCode = new Label
                    {
                        Text = $"{v.MaVoucher} [Toàn đơn]",
                        Location = new Point(10, 8),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                        ForeColor = isEligible ? Color.FromArgb(29, 78, 216) : ThemeManager.Muted
                    };
                    card.Controls.Add(lblCode);

                    var lblDiscountDesc = new Label
                    {
                        Text = v.HinhThuc + (v.GiamToiDa.HasValue && v.GiamToiDa > 0 ? $" (Tối đa {v.GiamToiDa:N0} đ)" : ""),
                        Location = new Point(10, 30),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                        ForeColor = isEligible ? Color.FromArgb(22, 163, 74) : ThemeManager.Muted
                    };
                    card.Controls.Add(lblDiscountDesc);

                    string conditionText = minOrder > 0
                        ? (isEligible ? $"Đơn tối thiểu: {minOrder:N0} đ (Đã đủ ĐK)" : $"Đơn tối thiểu: {minOrder:N0} đ • Mua thêm {missing:N0} đ")
                        : "Áp dụng cho mọi giá trị đơn";
                    var lblCondition = new Label
                    {
                        Text = conditionText,
                        Location = new Point(10, 52),
                        Size = new Size(295, 28),
                        Font = new Font("Segoe UI", 8F),
                        ForeColor = isEligible ? ThemeManager.Muted : ThemeManager.Amber
                    };
                    card.Controls.Add(lblCondition);

                    var btnSelect = new Button
                    {
                        Text = isCurrentOrderVoucher ? "Đang chọn" : (isEligible ? "Chọn mã" : "Chưa đủ ĐK"),
                        Location = new Point(316, 24),
                        Size = new Size(110, 32),
                        BackColor = isCurrentOrderVoucher
                            ? Color.FromArgb(22, 163, 74)
                            : (isEligible ? Color.FromArgb(14, 124, 134) : Color.FromArgb(226, 232, 240)),
                        ForeColor = isEligible ? Color.White : ThemeManager.Muted,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                        Cursor = isEligible ? Cursors.Hand : Cursors.Default,
                        Enabled = isEligible
                    };
                    btnSelect.FlatAppearance.BorderSize = 0;
                    if (isEligible)
                    {
                        btnSelect.Click += (s, e) => {
                            SelectedVoucher = v;
                            DialogResult = DialogResult.OK;
                            Close();
                        };
                    }
                    card.Controls.Add(btnSelect);

                    flow.Controls.Add(card);
                }

                flow.ResumeLayout(true);
            };

            txtCode.TextChanged += (s, e) => renderVoucherList();
            chkShowAll.CheckedChanged += (s, e) => renderVoucherList();
            renderVoucherList();

            // Thanh nút bấm dưới cùng
            var btnClearVoucher = new Button
            {
                Text = "Bỏ dùng voucher toàn đơn",
                Location = new Point(0, 470),
                Size = new Size(200, 36),
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
                Location = new Point(344, 470),
                Size = new Size(120, 36),
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

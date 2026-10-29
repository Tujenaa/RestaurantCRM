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
            IEnumerable<CartItem> cartItems = null,
            CartService cartService = null,
            Action onCartUpdated = null)
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
                Text = "🎟️ Danh Sách Ưu Đãi & Voucher",
                Location = new Point(0, 0),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            };
            root.Controls.Add(lblHeader);

            var lblSub = new Label
            {
                Text = $"Tổng tiền món tính voucher: {currentSubTotal:N0} đ",
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
                Size = new Size(464, 38),
                BackColor = Color.White
            };
            root.Controls.Add(pnlInput);

            var txtCode = new TextBox
            {
                Location = new Point(8, 7),
                Width = 320,
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

                // Nếu là voucher món hoặc loại món: kiểm tra và cho phép đổi trực tiếp
                if (found.PhamViApDung == "THEO_MON" || found.PhamViApDung == "THEO_LOAI")
                {
                    var matched = found.PhamViApDung == "THEO_MON"
                        ? currentCartList.Where(x => string.Equals(x.MaMon, found.MaMon, StringComparison.OrdinalIgnoreCase)).ToList()
                        : currentCartList.Where(x => string.Equals(x.MaLoaiMon, found.MaLoaiMon, StringComparison.OrdinalIgnoreCase)).ToList();

                    if (!matched.Any())
                    {
                        string target = found.PhamViApDung == "THEO_LOAI" ? $"nhóm '{found.TenLoaiMon ?? found.MaLoaiMon}'" : $"món '{found.TenMon ?? found.MaMon}'";
                        MessageBox.Show(this, $"Mã '{found.MaVoucher}' chỉ áp dụng cho {target}. Giỏ hàng hiện chưa có món phù hợp.", "Không áp dụng được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (matched.All(x => string.Equals(x.AppliedVoucherCode, found.MaVoucher, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show(this, $"Mã voucher '{found.MaVoucher}' đã được giảm trực tiếp vào đơn giá món ăn rồi (không áp trùng 2 lần).", "Đã áp dụng", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    cartService?.ChangeDishVoucher(matched, found, calcService);
                    onCartUpdated?.Invoke();
                    MessageBox.Show(this, $"Đã đổi món ăn sang áp dụng ưu đãi voucher [{found.MaVoucher}].", "Đổi voucher thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
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

            // Danh sách các voucher khả dụng
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

            var activeList = (vouchers ?? new List<VoucherDto>()).Where(v => v.TrangThai == "Đang áp dụng").ToList();
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
                    bool isDishScope = v.PhamViApDung == "THEO_MON" || v.PhamViApDung == "THEO_LOAI";
                    var matchedItems = isDishScope
                        ? (v.PhamViApDung == "THEO_MON"
                            ? currentCartList.Where(x => string.Equals(x.MaMon, v.MaMon, StringComparison.OrdinalIgnoreCase)).ToList()
                            : currentCartList.Where(x => string.Equals(x.MaLoaiMon, v.MaLoaiMon, StringComparison.OrdinalIgnoreCase)).ToList())
                        : new List<CartItem>();

                    bool isAppliedDirectlyToDish = isDishScope && matchedItems.Any() 
                        && matchedItems.All(x => string.Equals(x.AppliedVoucherCode, v.MaVoucher, StringComparison.OrdinalIgnoreCase));

                    bool canSwitchToThisDishVoucher = isDishScope && matchedItems.Any() && !isAppliedDirectlyToDish;

                    var (isEligible, discount, missing) = calcService.EvaluateVoucher(v, currentSubTotal, currentCartList);
                    bool isCurrentOrderVoucher = !isDishScope && currentApplied != null 
                        && (currentApplied.MaVoucher == v.MaVoucher || currentApplied.MaKmvoucher == v.MaKmvoucher);

                    bool cardActive = isAppliedDirectlyToDish || canSwitchToThisDishVoucher || isEligible;

                    var card = new RoundedPanel
                    {
                        Size = new Size(438, 86),
                        Margin = new Padding(0, 0, 0, 8),
                        Radius = 8,
                        BorderWidth = 1,
                        BorderColor = isCurrentOrderVoucher || isAppliedDirectlyToDish 
                            ? Color.FromArgb(14, 124, 134) 
                            : cardActive ? Color.FromArgb(203, 213, 225) : Color.FromArgb(226, 232, 240),
                        FillColor = isCurrentOrderVoucher || isAppliedDirectlyToDish 
                            ? Color.FromArgb(240, 253, 250) 
                            : cardActive ? Color.White : Color.FromArgb(241, 245, 249)
                    };
                    Color grayText = Color.FromArgb(148, 163, 184);

                    string phamViTag = v.PhamViApDung == "THEO_LOAI" ? $" [Nhóm: {v.TenLoaiMon ?? v.MaLoaiMon}]"
                                     : v.PhamViApDung == "THEO_MON" ? $" [Món: {v.TenMon ?? v.MaMon}]"
                                     : " [Toàn đơn]";

                    var lblCode = new Label
                    {
                        Text = $"🎟️ {v.MaVoucher}{phamViTag}",
                        Location = new Point(10, 8),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                        ForeColor = cardActive ? Color.FromArgb(29, 78, 216) : grayText
                    };
                    card.Controls.Add(lblCode);

                    var lblDiscountDesc = new Label
                    {
                        Text = v.HinhThuc + (v.GiamToiDa.HasValue && v.GiamToiDa > 0 ? $" (Tối đa {v.GiamToiDa:N0} đ)" : ""),
                        Location = new Point(10, 30),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                        ForeColor = cardActive ? Color.FromArgb(22, 163, 74) : grayText
                    };
                    card.Controls.Add(lblDiscountDesc);

                    decimal minOrder = (decimal)(v.GiaTriDonToiThieu ?? 0);
                    string conditionText = isAppliedDirectlyToDish
                        ? $"✓ Đang áp dụng trực tiếp cho: {string.Join(", ", matchedItems.Select(m => m.Name).Distinct())}"
                        : canSwitchToThisDishVoucher
                        ? $"Áp dụng cho: {string.Join(", ", matchedItems.Select(m => m.Name).Distinct())}"
                        : (minOrder > 0 ? $"Đơn tối thiểu: {minOrder:N0} đ" : "Áp dụng cho mọi giá trị đơn");

                    var lblCondition = new Label
                    {
                        Text = conditionText,
                        Location = new Point(10, 52),
                        Size = new Size(270, 28),
                        Font = new Font("Segoe UI", 8F),
                        ForeColor = isAppliedDirectlyToDish ? Color.FromArgb(22, 163, 74) : ThemeManager.Muted
                    };
                    card.Controls.Add(lblCondition);

                    // Xử lý các loại nút bấm cho thẻ
                    if (isAppliedDirectlyToDish)
                    {
                        // 1. Voucher món/loại món này ĐÃ ĐƯỢC GIẢM TRỰC TIẾP vào món rồi -> không cho áp trùng lần 2!
                        var btnApplied = new Button
                        {
                            Text = "Đang áp vào món",
                            Location = new Point(296, 20),
                            Size = new Size(130, 34),
                            BackColor = Color.FromArgb(22, 163, 74),
                            ForeColor = Color.White,
                            FlatStyle = FlatStyle.Flat,
                            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                            Cursor = Cursors.Default,
                            Enabled = false
                        };
                        btnApplied.FlatAppearance.BorderSize = 0;
                        card.Controls.Add(btnApplied);

                        var lblNote = new Label
                        {
                            Text = "Đã trừ vào giá món",
                            Location = new Point(296, 56),
                            Size = new Size(130, 20),
                            TextAlign = ContentAlignment.MiddleCenter,
                            Font = new Font("Segoe UI", 7.5F, FontStyle.Italic),
                            ForeColor = Color.FromArgb(22, 163, 74)
                        };
                        card.Controls.Add(lblNote);
                    }
                    else if (canSwitchToThisDishVoucher)
                    {
                        // 2. Voucher món/loại khác (ví dụ voucher giảm ít hơn, hoặc đổi qua lại) -> Cho phép bấm "Đổi sang mã này"
                        var btnSwitch = new Button
                        {
                            Text = "Đổi sang mã này",
                            Location = new Point(296, 20),
                            Size = new Size(130, 34),
                            BackColor = Color.FromArgb(249, 115, 22),
                            ForeColor = Color.White,
                            FlatStyle = FlatStyle.Flat,
                            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                            Cursor = Cursors.Hand
                        };
                        btnSwitch.FlatAppearance.BorderSize = 0;
                        btnSwitch.Click += (s, e) => {
                            cartService?.ChangeDishVoucher(matchedItems, v, calcService);
                            onCartUpdated?.Invoke();
                            MessageBox.Show(this, 
                                $"Đã chuyển món [{string.Join(", ", matchedItems.Select(m => m.Name).Distinct())}] sang áp dụng voucher [{v.MaVoucher}].", 
                                "Đổi voucher thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DialogResult = DialogResult.OK;
                            Close();
                        };
                        card.Controls.Add(btnSwitch);

                        var lblSwitchNote = new Label
                        {
                            Text = "Bấm để đổi ưu đãi món",
                            Location = new Point(296, 56),
                            Size = new Size(130, 20),
                            TextAlign = ContentAlignment.MiddleCenter,
                            Font = new Font("Segoe UI", 7.5F, FontStyle.Italic),
                            ForeColor = Color.FromArgb(249, 115, 22)
                        };
                        card.Controls.Add(lblSwitchNote);
                    }
                    else if (isEligible)
                    {
                        // 3. Voucher toàn đơn hợp lệ
                        var btnSelect = new Button
                        {
                            Text = isCurrentOrderVoucher ? "Đang chọn" : "Chọn mã",
                            Location = new Point(316, 24),
                            Size = new Size(110, 32),
                            BackColor = isCurrentOrderVoucher ? Color.FromArgb(22, 163, 74) : Color.FromArgb(14, 124, 134),
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
                        // 4. Voucher chưa đủ điều kiện: xám, không bấm chọn được
                        card.Cursor = Cursors.No;
                        var btnDisabled = new Button
                        {
                            Text = "Chưa đủ ĐK",
                            Location = new Point(316, 16),
                            Size = new Size(110, 28),
                            Enabled = false,
                            BackColor = Color.FromArgb(226, 232, 240),
                            ForeColor = grayText,
                            FlatStyle = FlatStyle.Flat,
                            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
                        };
                        btnDisabled.FlatAppearance.BorderSize = 0;
                        card.Controls.Add(btnDisabled);

                        var lblMissing = new Label
                        {
                            Text = missing > 0 ? $"Thiếu {missing:N0} đ" : "Chưa có món phù hợp",
                            Location = new Point(286, 48),
                            Size = new Size(140, 24),
                            TextAlign = ContentAlignment.MiddleRight,
                            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                            ForeColor = grayText
                        };
                        card.Controls.Add(lblMissing);
                    }

                    flow.Controls.Add(card);
                }
            }

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

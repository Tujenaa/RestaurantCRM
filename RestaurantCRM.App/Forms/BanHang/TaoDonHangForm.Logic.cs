using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Forms.HoaDon;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Forms.BanHang
{
    public partial class TaoDonHangForm
    {
        private void OnCartChanged()
        {
            RenderCart();
            UpdateProductsInPlace();
        }

        private void AddItem(MonAnDto mon)
        {
            if (mon.SoLuongTon <= 0)
            {
                MessageBox.Show(this,
                    $"Món '{mon.TenMonAn}' hiện đã hết hàng trong kho.",
                    "Hết hàng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var (effPrice, promoText) = GetDishPriceInfo(mon);

            var item = _cart.FirstOrDefault(x => x.MaMon == mon.MaMonAn);
            if (item == null)
            {
                _cart.Add(new CartItem
                {
                    MaMon = mon.MaMonAn,
                    Name = mon.TenMonAn,
                    BasePrice = mon.Gia,
                    Price = effPrice,
                    Quantity = 1,
                    MaxStock = mon.SoLuongTon,
                    PromoInfo = promoText
                });
            }
            else
            {
                if (item.Quantity >= mon.SoLuongTon)
                {
                    MessageBox.Show(this,
                        $"Món '{mon.TenMonAn}' trong kho chỉ còn {mon.SoLuongTon} suất.\nKhông thể đặt thêm vượt quá số lượng còn lại.",
                        "Đã đạt giới hạn số lượng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                item.Quantity++;
            }
            RenderCart();
            UpdateProductsInPlace();
        }

        // ================== LOGIC TÍNH TOÁN SMART BEST DEAL VOUCHER ==================
        private (bool isEligible, decimal discount, decimal missingAmount) EvaluateVoucher(VoucherDto v, decimal subTotal)
        {
            if (v == null || subTotal <= 0) return (false, 0m, 0m);

            decimal minOrder = (decimal)(v.GiaTriDonToiThieu ?? 0);
            if (subTotal < minOrder)
            {
                return (false, 0m, minOrder - subTotal);
            }

            decimal discount = 0m;
            if (v.PhanTramGiam.HasValue && v.PhanTramGiam.Value > 0)
            {
                discount = subTotal * (decimal)v.PhanTramGiam.Value / 100m;
                if (v.GiamToiDa.HasValue && v.GiamToiDa.Value > 0 && discount > (decimal)v.GiamToiDa.Value)
                {
                    discount = (decimal)v.GiamToiDa.Value;
                }
            }
            else if (v.TienGiam.HasValue && v.TienGiam.Value > 0)
            {
                discount = (decimal)v.TienGiam.Value;
            }

            if (discount > subTotal) discount = subTotal;
            return (true, discount, 0m);
        }

        private (VoucherDto bestVoucher, decimal bestDiscount, VoucherDto nearVoucher, decimal missingForNear) FindBestVoucher(decimal subTotal)
        {
            var activeVouchers = _allVouchers.Where(v => v.TrangThai == "Đang áp dụng").ToList();
            VoucherDto best = null;
            decimal maxDiscount = 0m;

            VoucherDto near = null;
            decimal minMissing = decimal.MaxValue;

            foreach (var v in activeVouchers)
            {
                var (isEligible, discount, missing) = EvaluateVoucher(v, subTotal);
                if (isEligible)
                {
                    if (discount > maxDiscount)
                    {
                        maxDiscount = discount;
                        best = v;
                    }
                }
                else if (missing > 0 && missing < minMissing)
                {
                    minMissing = missing;
                    near = v;
                }
            }

            return (best, maxDiscount, near, minMissing);
        }

        private void ApplyQuickVoucher()
        {
            string code = _txtQuickVoucher.Text.Trim();
            if (string.IsNullOrWhiteSpace(code) || code == "Mã voucher...")
            {
                MessageBox.Show(this, "Vui lòng nhập mã voucher cần áp dụng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var found = _allVouchers.FirstOrDefault(v => string.Equals(v.MaVoucher, code, StringComparison.OrdinalIgnoreCase)
                                                      || string.Equals(v.MaKmvoucher, code, StringComparison.OrdinalIgnoreCase));
            if (found == null)
            {
                MessageBox.Show(this, $"Mã voucher '{code}' không tồn tại hoặc đã hết hiệu lực trên hệ thống.", "Mã không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (found.TrangThai != "Đang áp dụng")
            {
                MessageBox.Show(this, $"Voucher '{code}' hiện có trạng thái: {found.TrangThai}.", "Không thể áp dụng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal subTotal = _cart.Sum(x => x.Price * x.Quantity);
            var (isEligible, discount, missing) = EvaluateVoucher(found, subTotal);
            if (!isEligible)
            {
                decimal minOrder = (decimal)(found.GiaTriDonToiThieu ?? 0);
                MessageBox.Show(this,
                    $"Đơn hàng chưa đạt điều kiện tối thiểu của voucher [{found.MaVoucher}].\n" +
                    $"• Yêu cầu tối thiểu: {minOrder:N0} đ\n" +
                    $"• Tiền món hiện tại: {subTotal:N0} đ\n" +
                    $"• Cần mua thêm: {missing:N0} đ",
                    "Chưa đủ điều kiện áp dụng", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _appliedVoucher = found;
            _autoApplyBestDeal = false; // Người dùng chủ động áp dụng mã
            _txtQuickVoucher.Text = "Mã voucher...";
            _txtQuickVoucher.ForeColor = ThemeManager.Muted;
            RenderCart();
            MessageBox.Show(this, $"Áp dụng thành công voucher [{found.MaVoucher}]!\nTiết kiệm: {discount:N0} đ.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ConfirmClearCart()
        {
            if (_cart.Count == 0) return;

            var confirm = MessageBox.Show(
                this,
                $"Xóa toàn bộ {_cart.Count} món đã chọn khỏi giỏ hàng?",
                "Xác nhận xóa giỏ hàng",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (confirm != DialogResult.Yes) return;

            _cart.Clear();
            _appliedVoucher = null;
            RenderCart();
            UpdateProductsInPlace();
        }

        // Vẽ lại danh sách món trong vùng cuộn
        private void RenderCartLines()
        {
            int scrollY = Math.Abs(_cartLines.AutoScrollPosition.Y);
            bool itemAdded = _cart.Count > _lastRenderedLineCount;

            _cartLines.SuspendLayout();

            var old = _cartLines.Controls.Cast<Control>().ToList();
            _cartLines.Controls.Clear();
            if (IsHandleCreated) BeginInvoke(new Action(() => { foreach (var c in old) c.Dispose(); }));
            else foreach (var c in old) c.Dispose();

            int rowWidth = Math.Max(240, _cartLines.ClientSize.Width - _cartLines.Padding.Horizontal);

            if (_cart.Count == 0)
            {
                _cartLines.Controls.Add(new Label
                {
                    Text = "Giỏ hàng đang trống.\nChọn món từ thực đơn để bắt đầu.",
                    Size = new System.Drawing.Size(rowWidth, 64),
                    Margin = new Padding(0, 12, 0, 0),
                    TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                    ForeColor = ThemeManager.Muted,
                    Font = new System.Drawing.Font("Segoe UI", 9F)
                });
            }
            else
            {
                foreach (var item in _cart)
                    _cartLines.Controls.Add(CreateCartItemCard(item, rowWidth));
            }

            _cartLines.ResumeLayout(true);
            FitCartRowWidths();

            if (itemAdded && _cartLines.Controls.Count > 0)
            {
                var lastControl = _cartLines.Controls[_cartLines.Controls.Count - 1];
                if (lastControl.Bottom > _cartLines.ClientSize.Height)
                {
                    _cartLines.ScrollControlIntoView(lastControl);
                }
            }
            else
            {
                try { _cartLines.AutoScrollPosition = new System.Drawing.Point(0, scrollY); } catch { }
            }
            _lastRenderedLineCount = _cart.Count;
        }

        private void RenderCart()
        {
            int distinctCount = _cart.Count;
            int totalQty = _cart.Sum(x => x.Quantity);
            if (distinctCount <= 0)
            {
                _lblCartCount.Text = "Món đã chọn (0)";
            }
            else if (distinctCount == totalQty)
            {
                _lblCartCount.Text = $"Món đã chọn ({distinctCount} món)";
            }
            else
            {
                _lblCartCount.Text = $"Món đã chọn ({distinctCount} món · {totalQty} phần)";
            }
            _btnClearCart.Visible = _cart.Count > 0;

            RenderCartLines();

            decimal subTotal = _cart.Sum(x => x.Price * x.Quantity);

            // ================== ÁP DỤNG SMART BEST DEAL ==================
            if (_autoApplyBestDeal)
            {
                var (bestVoucher, _, _, _) = FindBestVoucher(subTotal);
                _appliedVoucher = bestVoucher;
            }
            else if (_appliedVoucher != null)
            {
                var (isEligible, _, _) = EvaluateVoucher(_appliedVoucher, subTotal);
                if (!isEligible) _appliedVoucher = null;
            }

            decimal voucherDiscount = 0m;
            if (_appliedVoucher != null)
            {
                var (isEligible, disc, _) = EvaluateVoucher(_appliedVoucher, subTotal);
                if (isEligible) voucherDiscount = disc;
            }

            decimal finalTotal = Math.Max(0, subTotal - voucherDiscount);

            UpdatePromoBox(subTotal);

            _lblSubTotalValue.Text = $"{subTotal:N0} đ";

            bool hasDiscount = _appliedVoucher != null && voucherDiscount > 0;
            _lblDiscountCaption.Visible = hasDiscount;
            _lblDiscountValue.Visible = hasDiscount;
            if (hasDiscount)
            {
                _lblDiscountCaption.Text = $"Giảm giá ({_appliedVoucher.MaVoucher})";
                _lblDiscountValue.Text = $"-{voucherDiscount:N0} đ";
            }

            _lblTotalValue.Text = $"{finalTotal:N0} đ";
        }

        private void UpdatePromoBox(decimal subTotal)
        {
            if (subTotal <= 0)
            {
                _lblPromoIcon.Text = "🎟️";
                _lblPromoTitle.Text = "Voucher / Giảm giá";
                _lblPromoTitle.ForeColor = ThemeManager.Ink;
                _lblPromoSub.Text = "Chưa có món";
                _btnPromoAction.Visible = false;
                _lnkChangePromo.Text = "Chọn mã ▾";
                _pnlPromoBox.FillColor = System.Drawing.Color.FromArgb(248, 250, 252);
                _pnlPromoBox.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
                LayoutPromoBox();
                return;
            }

            if (_appliedVoucher != null)
            {
                var (_, disc, _) = EvaluateVoucher(_appliedVoucher, subTotal);
                _lblPromoIcon.Text = "✔";
                _lblPromoTitle.Text = $"Mã [{_appliedVoucher.MaVoucher}]";
                _lblPromoTitle.ForeColor = System.Drawing.Color.FromArgb(22, 163, 74);
                _lblPromoSub.Text = $"Giảm -{disc:N0} đ";
                _btnPromoAction.Visible = true;
                _btnPromoAction.Text = "Bỏ mã";
                _btnPromoAction.BackColor = System.Drawing.Color.FromArgb(254, 242, 242);
                _btnPromoAction.ForeColor = ThemeManager.Red;
                _lnkChangePromo.Text = "Đổi mã ▾";
                _pnlPromoBox.FillColor = System.Drawing.Color.FromArgb(240, 253, 244);
                _pnlPromoBox.BorderColor = System.Drawing.Color.FromArgb(187, 247, 208);
                LayoutPromoBox();
                return;
            }

            var (best, bestDisc, near, missing) = FindBestVoucher(subTotal);
            if (best != null)
            {
                _lblPromoIcon.Text = "✨";
                _lblPromoTitle.Text = $"Gợi ý: [{best.MaVoucher}]";
                _lblPromoTitle.ForeColor = System.Drawing.Color.FromArgb(79, 70, 229);
                _lblPromoSub.Text = $"Giảm {bestDisc:N0} đ";
                _btnPromoAction.Visible = false;
                _lnkChangePromo.Text = "Chọn mã ▾";
                _pnlPromoBox.FillColor = System.Drawing.Color.FromArgb(238, 242, 255);
                _pnlPromoBox.BorderColor = System.Drawing.Color.FromArgb(199, 210, 254);
            }
            else if (near != null)
            {
                _lblPromoIcon.Text = "💡";
                _lblPromoTitle.Text = $"+{missing:N0} đ";
                _lblPromoTitle.ForeColor = ThemeManager.Amber;
                _lblPromoSub.Text = $"Nhận mã [{near.MaVoucher}]";
                _btnPromoAction.Visible = false;
                _lnkChangePromo.Text = "Xem mã ▾";
                _pnlPromoBox.FillColor = System.Drawing.Color.FromArgb(255, 251, 235);
                _pnlPromoBox.BorderColor = System.Drawing.Color.FromArgb(254, 243, 199);
            }
            else
            {
                _lblPromoIcon.Text = "🎟️";
                _lblPromoTitle.Text = "Voucher / Giảm giá";
                _lblPromoTitle.ForeColor = ThemeManager.Muted;
                _lblPromoSub.Text = "Nhập mã hoặc chọn";
                _btnPromoAction.Visible = false;
                _lnkChangePromo.Text = "Chọn mã ▾";
                _pnlPromoBox.FillColor = System.Drawing.Color.FromArgb(248, 250, 252);
                _pnlPromoBox.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
            }
            LayoutPromoBox();
        }

        private void HandlePromoActionClick()
        {
            decimal subTotal = _cart.Sum(x => x.Price * x.Quantity);
            if (_appliedVoucher != null)
            {
                _appliedVoucher = null;
                _autoApplyBestDeal = false;
                RenderCart();
            }
            else
            {
                var (best, _, _, _) = FindBestVoucher(subTotal);
                if (best != null)
                {
                    _appliedVoucher = best;
                    _autoApplyBestDeal = true;
                    RenderCart();
                }
            }
        }

        private void OpenSelectVoucherDialog()
        {
            decimal subTotal = _cart.Sum(x => x.Price * x.Quantity);
            using (var dlg = new ChonVoucherDialog(subTotal, _allVouchers, _appliedVoucher))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _appliedVoucher = dlg.SelectedVoucher;
                    _autoApplyBestDeal = (_appliedVoucher == null);
                    RenderCart();
                }
            }
        }

        private async Task Checkout()
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show(this, "Vui lòng chọn ít nhất một món ăn vào giỏ hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var item in _cart)
            {
                if (item.Quantity > item.MaxStock)
                {
                    MessageBox.Show(this,
                        $"Món '{item.Name}' đang đặt {item.Quantity} suất nhưng trong kho chỉ còn {item.MaxStock} suất.\nVui lòng điều chỉnh lại số lượng trước khi xác nhận.",
                        "Vượt quá số lượng còn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            string staffId = UserSession.CurrentUserId ?? UserSession.CurrentUsername;
            string staffName = UserSession.CurrentDisplayName ?? UserSession.CurrentUsername ?? "Nhân viên";

            decimal subTotal = _cart.Sum(x => x.Price * x.Quantity);
            decimal voucherDiscount = 0m;
            if (_appliedVoucher != null)
            {
                var (isEligible, disc, _) = EvaluateVoucher(_appliedVoucher, subTotal);
                if (isEligible) voucherDiscount = disc;
            }
            decimal finalTotal = Math.Max(0, subTotal - voucherDiscount);

            using (var previewDlg = new XacNhanDonHangDialog(
                tenKhach: string.IsNullOrWhiteSpace(_txtTenKhach.Text) ? "Khách mua mang đi" : _txtTenKhach.Text.Trim(),
                sdt: _txtSdt.Text.Trim(),
                ghiChu: string.IsNullOrWhiteSpace(_txtGhiChu.Text) ? "Bán mang đi tại quầy" : _txtGhiChu.Text.Trim(),
                staffName: staffName,
                items: _cart,
                subTotal: subTotal,
                appliedVoucher: _appliedVoucher,
                voucherDiscount: voucherDiscount,
                finalTotal: finalTotal))
            {
                if (previewDlg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
            }

            var req = new CreateOrderRequestDto
            {
                TenKhachHang = string.IsNullOrWhiteSpace(_txtTenKhach.Text) ? "Khách mua mang đi" : _txtTenKhach.Text.Trim(),
                SoDienThoai = _txtSdt.Text.Trim(),
                DiaChiGiao = string.IsNullOrWhiteSpace(_txtGhiChu.Text) ? "Bán mang đi tại quầy" : _txtGhiChu.Text.Trim(),
                PhuongThucThanhToan = "Tiền mặt (Tại quầy)",
                MaNhanVien = staffId,
                MaKmvoucher = _appliedVoucher?.MaVoucher ?? _appliedVoucher?.MaKmvoucher,
                Items = _cart.Select(x => new CreateOrderItemDto
                {
                    MaMon = x.MaMon,
                    SoLuong = x.Quantity
                }).ToList()
            };

            _btnPay.Enabled = false;
            try
            {
                var (success, message, maHoaDon) = await _hoaDonService.CreateOrderAsync(req);
                if (success)
                {
                    string voucherMsg = _appliedVoucher != null ? $"Ưu đãi voucher ({_appliedVoucher.MaVoucher}): -{voucherDiscount:N0} đ\n" : "";
                    var confirm = MessageBox.Show(this,
                        $"🎉 Tạo hóa đơn mang đi thành công!\n\n" +
                        $"Mã hóa đơn: #{maHoaDon}\n" +
                        $"Nhân viên lập đơn: {staffName}\n" +
                        $"Tiền món: {subTotal:N0} đ\n" +
                        voucherMsg +
                        $"Tổng thanh toán: {finalTotal:N0} đ\n\n" +
                        $"Bạn có muốn mở xem chi tiết hóa đơn vừa tạo không?",
                        "Tạo đơn thành công", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    _cart.Clear();
                    _appliedVoucher = null;
                    _autoApplyBestDeal = true;
                    RenderCart();

                    await InitData();

                    if (confirm == DialogResult.Yes && !string.IsNullOrEmpty(maHoaDon))
                    {
                        using (var dialog = new HoaDonDialog(maHoaDon))
                        {
                            dialog.ShowDialog(this);
                        }
                    }
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show(this, message, "Lỗi tạo hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                _btnPay.Enabled = true;
            }
        }
    }
}

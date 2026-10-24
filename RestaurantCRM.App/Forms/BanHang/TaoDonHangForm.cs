using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls.BanHang;
using RestaurantCRM.AdminApp.Forms.HoaDon;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services;
using RestaurantCRM.AdminApp.Services.BanHang;

namespace RestaurantCRM.AdminApp.Forms.BanHang
{
    public class TaoDonHangForm : Form
    {
        // Kế thừa Model CartItem độc lập để bảo toàn 100% tương thích mã nguồn và Dialog
        public sealed class CartItem : RestaurantCRM.AdminApp.Models.CartItem { }

        // Services
        private readonly MonAnService _monAnService;
        private readonly LoaiMonService _loaiMonService;
        private readonly HoaDonService _hoaDonService;
        private readonly KhuyenMaiService _khuyenMaiService;
        private readonly OrderCalculationService _calcService;
        private readonly CartService _cartService;

        // UI Panel Builders
        private readonly CartPanelBuilder _cartBuilder;
        private readonly MenuPanelBuilder _menuBuilder;

        // Data caches
        private List<MonAnDto> _allMonAn = new List<MonAnDto>();
        private List<LoaiMonDto> _allLoaiMon = new List<LoaiMonDto>();
        private List<VoucherDto> _allVouchers = new List<VoucherDto>();
        private List<KmMonDto> _allKmMon = new List<KmMonDto>();

        private VoucherDto _appliedVoucher;
        private bool _autoApplyBestDeal = true;

        private readonly Timer _autoRefreshTimer;
        private bool _isRefreshing = false;
        private readonly Label _lblNhanVien;

        public TaoDonHangForm()
        {
            _monAnService = new MonAnService();
            _loaiMonService = new LoaiMonService();
            _hoaDonService = new HoaDonService();
            _khuyenMaiService = new KhuyenMaiService();
            _calcService = new OrderCalculationService();
            _cartService = new CartService();

            Text = "Tạo đơn hàng mang đi tại quầy | RestaurantCRM";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1300, 800);
            MinimumSize = new Size(1080, 680);
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);

            string staffName = UserSession.CurrentDisplayName ?? UserSession.CurrentUsername ?? "Nhân viên";
            _lblNhanVien = new Label
            {
                Text = $"Thu ngân: {staffName}",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = ThemeManager.Muted,
                AutoSize = true
            };

            _cartBuilder = new CartPanelBuilder(Font);
            _menuBuilder = new MenuPanelBuilder(Font);

            WireEvents();

            BuildMainLayout();

            _autoRefreshTimer = new Timer { Interval = 15000 };
            _autoRefreshTimer.Tick += async (s, e) => await RefreshStockContinuouslyAsync();

            this.Load += async (s, e) =>
            {
                await InitData();
                _autoRefreshTimer.Start();
            };

            this.FormClosing += (s, e) =>
            {
                _autoRefreshTimer.Stop();
                _autoRefreshTimer.Dispose();
            };
        }

        private void WireEvents()
        {
            // Sự kiện từ Cột Thực đơn
            _menuBuilder.DishSelected += OnDishSelected;
            _menuBuilder.FilterChanged += OnFilterChanged;

            // Sự kiện từ Cột Giỏ hàng
            _cartBuilder.QuantityChanged += OnCartQuantityChanged;
            _cartBuilder.ItemRemoved += OnCartItemRemoved;
            _cartBuilder.ClearCartRequested += OnClearCartRequested;
            _cartBuilder.QuickVoucherApplied += OnQuickVoucherApplied;
            _cartBuilder.SelectVoucherClicked += OnSelectVoucherClicked;
            _cartBuilder.PromoActionClicked += OnPromoActionClicked;
            _cartBuilder.CheckoutClicked += async () => await CheckoutAsync();
        }

        private void BuildMainLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ThemeManager.Bg,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64F));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            root.Controls.Add(_menuBuilder.BuildMenuColumn(_lblNhanVien), 0, 0);

            var cartWrapper = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(12, 12, 12, 10)
            };
            cartWrapper.Controls.Add(_cartBuilder.BuildCartColumn());
            root.Controls.Add(cartWrapper, 1, 0);

            Controls.Add(root);
        }

        private async Task InitData()
        {
            try
            {
                _allLoaiMon = await _loaiMonService.GetAllAsync() ?? new List<LoaiMonDto>();
                _menuBuilder.PopulateCategories(_allLoaiMon);

                var list = await _monAnService.GetAllAsync() ?? new List<MonAnDto>();
                _allMonAn = list.Where(m => m.TrangThaiRaw != "Discontinued" && m.TrangThaiRaw != "Inactive").ToList();

                try
                {
                    _allVouchers = await _khuyenMaiService.GetVoucherListAsync() ?? new List<VoucherDto>();
                    _allKmMon = await _khuyenMaiService.GetKmMonListAsync() ?? new List<KmMonDto>();
                }
                catch { }

                RefreshView();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Không thể tải dữ liệu thực đơn: " + ex.Message, "Lỗi tải dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshView()
        {
            _menuBuilder.RenderProducts(_allMonAn, _allKmMon, _cartService, _calcService);
            RenderCart();
        }

        private void RenderCart()
        {
            decimal subTotal = _cartService.SubTotal;

            if (_autoApplyBestDeal)
            {
                var (bestVoucher, _, _, _) = _calcService.FindBestVoucher(subTotal, _allVouchers);
                _appliedVoucher = bestVoucher;
            }
            else if (_appliedVoucher != null)
            {
                var (isEligible, _, _) = _calcService.EvaluateVoucher(_appliedVoucher, subTotal);
                if (!isEligible) _appliedVoucher = null;
            }

            _cartBuilder.Render(_cartService, _calcService, _appliedVoucher, _allVouchers);
        }

        private void OnDishSelected(MonAnDto mon)
        {
            var (effPrice, promoText) = _calcService.GetDishPriceInfo(mon, _allKmMon);
            var (success, msg, isWarning) = _cartService.AddItem(mon, effPrice, promoText);
            if (!success)
            {
                MessageBox.Show(this, msg, isWarning ? "Thông báo tồn kho" : "Lỗi", MessageBoxButtons.OK, isWarning ? MessageBoxIcon.Warning : MessageBoxIcon.Error);
                return;
            }

            RenderCart();
            _menuBuilder.UpdateProductsInPlace(_allMonAn, _allKmMon, _cartService, _calcService);
        }

        private void OnFilterChanged()
        {
            _menuBuilder.RenderProducts(_allMonAn, _allKmMon, _cartService, _calcService);
        }

        private void OnCartQuantityChanged(RestaurantCRM.AdminApp.Models.CartItem item, int newQty)
        {
            if (newQty <= 0)
            {
                var confirm = MessageBox.Show(this,
                    $"Bạn đã nhập số lượng là 0.\nXác nhận xóa món '{item.Name}' khỏi giỏ hàng?",
                    "Xác nhận xóa món", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    _cartService.RemoveItem(item);
                }
            }
            else
            {
                int delta = newQty - item.Quantity;
                var (success, msg) = _cartService.UpdateQuantity(item, delta);
                if (!success)
                {
                    MessageBox.Show(this, msg, "Vượt quá tồn kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            RenderCart();
            _menuBuilder.UpdateProductsInPlace(_allMonAn, _allKmMon, _cartService, _calcService);
        }

        private void OnCartItemRemoved(RestaurantCRM.AdminApp.Models.CartItem item)
        {
            var confirm = MessageBox.Show(this,
                $"Xác nhận xóa món '{item.Name}' khỏi giỏ hàng?",
                "Xác nhận xóa món", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                _cartService.RemoveItem(item);
                RenderCart();
                _menuBuilder.UpdateProductsInPlace(_allMonAn, _allKmMon, _cartService, _calcService);
            }
        }

        private void OnClearCartRequested()
        {
            if (_cartService.DistinctCount == 0) return;

            var confirm = MessageBox.Show(this,
                $"Xóa toàn bộ {_cartService.DistinctCount} món đã chọn khỏi giỏ hàng?",
                "Xác nhận xóa giỏ hàng", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (confirm == DialogResult.Yes)
            {
                _cartService.Clear();
                _appliedVoucher = null;
                RenderCart();
                _menuBuilder.UpdateProductsInPlace(_allMonAn, _allKmMon, _cartService, _calcService);
            }
        }

        private void OnQuickVoucherApplied(string code)
        {
            string trimmed = (code ?? "").Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed == "Mã voucher...")
            {
                MessageBox.Show(this, "Vui lòng nhập mã voucher cần áp dụng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var found = _allVouchers.FirstOrDefault(v => string.Equals(v.MaVoucher, trimmed, StringComparison.OrdinalIgnoreCase)
                                                      || string.Equals(v.MaKmvoucher, trimmed, StringComparison.OrdinalIgnoreCase));
            if (found == null)
            {
                MessageBox.Show(this, $"Mã voucher '{trimmed}' không tồn tại hoặc đã hết hiệu lực trên hệ thống.", "Mã không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (found.TrangThai != "Đang áp dụng")
            {
                MessageBox.Show(this, $"Voucher '{trimmed}' hiện có trạng thái: {found.TrangThai}.", "Không thể áp dụng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal subTotal = _cartService.SubTotal;
            var (isEligible, discount, missing) = _calcService.EvaluateVoucher(found, subTotal);
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
            _autoApplyBestDeal = false;
            _cartBuilder.ResetQuickVoucherText();
            RenderCart();
            MessageBox.Show(this, $"Áp dụng thành công voucher [{found.MaVoucher}]!\nTiết kiệm: {discount:N0} đ.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnSelectVoucherClicked()
        {
            decimal subTotal = _cartService.SubTotal;
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

        private void OnPromoActionClicked()
        {
            decimal subTotal = _cartService.SubTotal;
            if (_appliedVoucher != null)
            {
                _appliedVoucher = null;
                _autoApplyBestDeal = false;
                RenderCart();
            }
            else
            {
                var (best, _, _, _) = _calcService.FindBestVoucher(subTotal, _allVouchers);
                if (best != null)
                {
                    _appliedVoucher = best;
                    _autoApplyBestDeal = true;
                    RenderCart();
                }
            }
        }

        private async Task RefreshStockContinuouslyAsync()
        {
            if (_isRefreshing || IsDisposed) return;
            if (_menuBuilder.IsSearchFocused) return;

            _isRefreshing = true;
            try
            {
                var list = await _monAnService.GetAllAsync();
                if (list == null || !list.Any() || IsDisposed) return;

                var available = list.Where(m => m.TrangThaiRaw != "Discontinued" && m.TrangThaiRaw != "Inactive").ToList();
                if (!HasMonAnStockChanged(_allMonAn, available)) return;

                _allMonAn = available;
                try
                {
                    _allVouchers = await _khuyenMaiService.GetVoucherListAsync() ?? _allVouchers;
                    _allKmMon = await _khuyenMaiService.GetKmMonListAsync() ?? _allKmMon;
                }
                catch { }

                var (cartAltered, warningMsg) = _cartService.SyncStock(_allMonAn);

                _menuBuilder.UpdateProductsInPlace(_allMonAn, _allKmMon, _cartService, _calcService);

                if (cartAltered)
                {
                    RenderCart();
                    if (!string.IsNullOrEmpty(warningMsg))
                    {
                        MessageBox.Show(this, "Thông báo đồng bộ tồn kho trong lúc bán:" + warningMsg, "Cập nhật tồn kho", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch { }
            finally
            {
                _isRefreshing = false;
            }
        }

        private static bool HasMonAnStockChanged(List<MonAnDto> oldList, List<MonAnDto> newList)
        {
            if (oldList == null || newList == null) return true;
            if (oldList.Count != newList.Count) return true;
            for (int i = 0; i < oldList.Count; i++)
            {
                var a = oldList[i];
                var b = newList[i];
                if (a.MaMonAn != b.MaMonAn ||
                    a.SoLuongTon != b.SoLuongTon ||
                    a.Gia != b.Gia ||
                    a.TrangThaiRaw != b.TrangThaiRaw ||
                    a.TenMonAn != b.TenMonAn)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task CheckoutAsync()
        {
            if (_cartService.DistinctCount == 0)
            {
                MessageBox.Show(this, "Vui lòng chọn ít nhất một món ăn vào giỏ hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var item in _cartService.Items)
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

            var (subTotal, voucherDiscount, finalTotal) = _calcService.CalculateTotals(_cartService.Items, _appliedVoucher);

            string tenKhach = string.IsNullOrWhiteSpace(_cartBuilder.CustomerName) ? "Khách mua mang đi" : _cartBuilder.CustomerName;
            string sdt = _cartBuilder.CustomerPhone;
            string ghiChu = string.IsNullOrWhiteSpace(_cartBuilder.Note) ? "Bán mang đi tại quầy" : _cartBuilder.Note;
            string paymentMethod = _cartBuilder.PaymentMethod;

            var cartSnapshot = _cartService.Items.Select(x => new TaoDonHangForm.CartItem
            {
                MaMon = x.MaMon,
                Name = x.Name,
                BasePrice = x.BasePrice,
                Price = x.Price,
                Quantity = x.Quantity,
                MaxStock = x.MaxStock,
                PromoInfo = x.PromoInfo
            }).ToList();

            using (var previewDlg = new XacNhanDonHangDialog(
                tenKhach: tenKhach,
                sdt: sdt,
                ghiChu: ghiChu,
                staffName: staffName,
                items: cartSnapshot,
                subTotal: subTotal,
                appliedVoucher: _appliedVoucher,
                voucherDiscount: voucherDiscount,
                finalTotal: finalTotal,
                phuongThuc: paymentMethod))
            {
                if (previewDlg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
            }

            var req = new CreateOrderRequestDto
            {
                TenKhachHang = tenKhach,
                SoDienThoai = sdt,
                DiaChiGiao = ghiChu,
                PhuongThucThanhToan = paymentMethod,
                MaNhanVien = staffId,
                MaKmvoucher = _appliedVoucher?.MaVoucher ?? _appliedVoucher?.MaKmvoucher,
                Items = _cartService.Items.Select(x => new CreateOrderItemDto
                {
                    MaMon = x.MaMon,
                    SoLuong = x.Quantity
                }).ToList()
            };

            _cartBuilder.SetPayButtonEnabled(false);
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
                        $"Phương thức thanh toán: {paymentMethod}\n" +
                        $"Tiền món: {subTotal:N0} đ\n" +
                        voucherMsg +
                        $"Tổng thanh toán: {finalTotal:N0} đ\n\n" +
                        $"Bạn có muốn mở xem chi tiết hóa đơn vừa tạo không?",
                        "Tạo đơn thành công", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    _cartService.Clear();
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
                _cartBuilder.SetPayButtonEnabled(true);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Services.BanHang
{
    /// <summary>
    /// Chuyên trách quản lý dữ liệu giỏ hàng:
    /// - Thêm món, tăng giảm số lượng, xóa món
    /// - Kiểm tra giới hạn số lượng theo tồn kho thực tế
    /// - Đồng bộ tồn kho nền khi hệ thống có cập nhật mới
    /// </summary>
    public class CartService
    {
        private readonly List<CartItem> _cart = new List<CartItem>();

        public IReadOnlyList<CartItem> Items => _cart;
        public int DistinctCount => _cart.Count;
        public int TotalQuantity => _cart.Sum(x => x.Quantity);
        public decimal SubTotal => _cart.Sum(x => x.Price * x.Quantity);

        /// <summary>
        /// Thêm món vào giỏ hàng. Trả về thông báo lỗi nếu hết hàng hoặc vượt tồn kho.
        /// </summary>
        public (bool success, string message, bool isWarning) AddItem(MonAnDto mon, decimal effPrice, string promoText, string appliedVoucherCode = null, decimal dishDiscount = 0m)
        {
            if (mon == null) return (false, "Món ăn không hợp lệ.", true);

            if (mon.SoLuongTon <= 0)
            {
                return (false, $"Món '{mon.TenMonAn}' hiện đã hết hàng trong kho.", true);
            }

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
                    PromoInfo = promoText,
                    MaLoaiMon = mon.MaLoaiMon,
                    GiaVon = mon.GiaVonUocTinh,
                    AppliedVoucherCode = appliedVoucherCode,
                    DishDiscount = dishDiscount
                });
                return (true, null, false);
            }

            if (item.Quantity >= mon.SoLuongTon)
            {
                return (false, $"Món '{mon.TenMonAn}' trong kho chỉ còn {mon.SoLuongTon} suất.\nKhông thể đặt thêm vượt quá số lượng còn lại.", true);
            }

            item.Quantity++;
            item.MaxStock = mon.SoLuongTon;
            return (true, null, false);
        }

        /// <summary>
        /// Đổi voucher cho các món trong giỏ (hoặc gỡ voucher nếu newVoucher == null).
        /// Cho phép chọn qua lại giữa các mã voucher món / loại món mà không bị trừ trùng lặp 2 lần.
        /// </summary>
        public void ChangeDishVoucher(IEnumerable<CartItem> items, VoucherDto newVoucher, OrderCalculationService calcService)
        {
            if (items == null || calcService == null) return;
            foreach (var item in items)
            {
                if (newVoucher == null)
                {
                    item.AppliedVoucherCode = null;
                    item.Price = item.BasePrice;
                    item.PromoInfo = null;
                    item.DishDiscount = 0m;
                }
                else
                {
                    var (effPrice, promoText, disc) = calcService.CalculateDishPriceWithVoucher(item.BasePrice, item.GiaVon, newVoucher);
                    item.AppliedVoucherCode = newVoucher.MaVoucher;
                    item.Price = effPrice;
                    item.PromoInfo = promoText;
                    item.DishDiscount = disc;
                }
            }
        }

        /// <summary>
        /// Tăng hoặc giảm số lượng của một món trong giỏ.
        /// </summary>
        public (bool success, string message) UpdateQuantity(CartItem item, int delta)
        {
            if (item == null) return (false, null);

            int targetQty = item.Quantity + delta;
            if (targetQty <= 0)
            {
                _cart.Remove(item);
                return (true, null);
            }

            if (targetQty > item.MaxStock)
            {
                return (false, $"Món '{item.Name}' trong kho chỉ còn {item.MaxStock} suất.\nKhông thể tăng thêm.");
            }

            item.Quantity = targetQty;
            return (true, null);
        }

        /// <summary>
        /// Xóa một món khỏi giỏ hàng.
        /// </summary>
        public void RemoveItem(CartItem item)
        {
            if (item != null)
            {
                _cart.Remove(item);
            }
        }

        /// <summary>
        /// Xóa sạch giỏ hàng.
        /// </summary>
        public void Clear()
        {
            _cart.Clear();
        }

        /// <summary>
        /// Lấy số lượng của món đang có trong giỏ hàng.
        /// </summary>
        public int GetQuantityInCart(string maMon)
        {
            var item = _cart.FirstOrDefault(x => x.MaMon == maMon);
            return item?.Quantity ?? 0;
        }

        /// <summary>
        /// Đồng bộ giỏ hàng với dữ liệu tồn kho mới nhất từ hệ thống.
        /// Tự động gỡ món hết hàng hoặc hạ số lượng nếu vượt quá tồn kho.
        /// </summary>
        public (bool cartAltered, string warningMsg) SyncStock(IEnumerable<MonAnDto> latestMonAn)
        {
            if (latestMonAn == null) return (false, "");

            var monAnList = latestMonAn.ToList();
            bool cartAltered = false;
            string warningMsg = "";

            for (int i = _cart.Count - 1; i >= 0; i--)
            {
                var item = _cart[i];
                var found = monAnList.FirstOrDefault(m => m.MaMonAn == item.MaMon);
                int newStock = found != null ? found.SoLuongTon : 0;
                item.MaxStock = newStock;

                if (newStock <= 0)
                {
                    _cart.RemoveAt(i);
                    cartAltered = true;
                    warningMsg += $"\n• Món '{item.Name}' vừa hết hàng trên hệ thống (đã gỡ khỏi giỏ).";
                }
                else if (item.Quantity > newStock)
                {
                    item.Quantity = newStock;
                    cartAltered = true;
                    warningMsg += $"\n• Món '{item.Name}' chỉ còn {newStock} suất trong kho (đã điều chỉnh số lượng).";
                }
            }

            return (cartAltered, warningMsg);
        }
    }
}

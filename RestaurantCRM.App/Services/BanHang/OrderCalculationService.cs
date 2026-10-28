using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Services.BanHang
{
    /// <summary>
    /// Chuyên trách xử lý tính toán tài chính cho đơn hàng:
    /// - Đơn giá hiệu lực sau khuyến mãi món
    /// - Điều kiện áp dụng và mức chiết khấu của voucher
    /// - Thuật toán Smart Best Deal tìm voucher tối ưu nhất
    /// - Tổng tiền hàng, chiết khấu và tổng thanh toán cuối cùng
    /// </summary>
    public class OrderCalculationService
    {
        /// <summary>
        /// Điểm 5 & Điểm 2 & Điểm 3: Tính đơn giá bán thực tế của món ăn theo Tầng 1.
        /// Khi món vừa có voucher riêng vừa có voucher theo loại món:
        /// -> Tự động so sánh và lấy mức giảm cao nhất Max(Món, Loại), KHÔNG cộng dồn.
        /// -> Bảo vệ giá sàn (Floor margin): Đơn giá sau giảm không được thấp hơn giá vốn BOM.
        /// </summary>
        public (decimal effectivePrice, string promoText, decimal discountPerDish) GetDishPriceInfo(
            MonAnDto mon, 
            IEnumerable<VoucherDto> vouchers, 
            IEnumerable<KmMonDto> kmMonList = null)
        {
            if (mon == null) return (0m, null, 0m);
            decimal basePrice = mon.Gia;
            if (basePrice <= 0) return (0m, null, 0m);

            decimal maxDiscount = 0m;
            string bestPromoText = null;

            // 1. Kiểm tra KM từ danh sách KmMonDto (nếu có chương trình giảm món trực tiếp)
            if (kmMonList != null)
            {
                var promo = kmMonList.FirstOrDefault(k => k.MaMon == mon.MaMonAn && k.TrangThai == "Đang áp dụng");
                if (promo != null)
                {
                    decimal disc = basePrice - (decimal)promo.DonGiaSauGiam;
                    if (disc > maxDiscount)
                    {
                        maxDiscount = disc;
                        bestPromoText = promo.HinhThuc;
                    }
                }
            }

            // 2. Kiểm tra Voucher theo Món (THEO_MON)
            if (vouchers != null)
            {
                var dishVouchers = vouchers.Where(v => v.TrangThai == "Đang áp dụng" 
                    && v.PhamViApDung == "THEO_MON" 
                    && string.Equals(v.MaMon, mon.MaMonAn, StringComparison.OrdinalIgnoreCase));

                foreach (var v in dishVouchers)
                {
                    decimal disc = 0m;
                    if (v.PhanTramGiam.HasValue && v.PhanTramGiam.Value > 0)
                    {
                        disc = basePrice * (decimal)v.PhanTramGiam.Value / 100m;
                        if (v.GiamToiDa.HasValue && v.GiamToiDa.Value > 0 && disc > (decimal)v.GiamToiDa.Value)
                        {
                            disc = (decimal)v.GiamToiDa.Value;
                        }
                    }
                    else if (v.TienGiam.HasValue && v.TienGiam.Value > 0)
                    {
                        disc = (decimal)v.TienGiam.Value;
                    }

                    if (disc > maxDiscount)
                    {
                        maxDiscount = disc;
                        bestPromoText = $"{v.MaVoucher} (-{disc:N0}đ)";
                    }
                }

                // 3. Kiểm tra Voucher theo Loại Món (THEO_LOAI)
                var catVouchers = vouchers.Where(v => v.TrangThai == "Đang áp dụng" 
                    && v.PhamViApDung == "THEO_LOAI" 
                    && string.Equals(v.MaLoaiMon, mon.MaLoaiMon, StringComparison.OrdinalIgnoreCase));

                foreach (var v in catVouchers)
                {
                    // Điều kiện: Giá tối thiểu được giảm (Chỉ món có đơn giá >= GiaTriDonToiThieu mới được giảm)
                    decimal minItemPrice = (decimal)(v.GiaTriDonToiThieu ?? 0);
                    if (minItemPrice > 0 && basePrice < minItemPrice)
                    {
                        continue; // Đơn giá món thấp hơn giá tối thiểu được giảm, giữ nguyên giá gốc
                    }

                    decimal disc = 0m;
                    if (v.PhanTramGiam.HasValue && v.PhanTramGiam.Value > 0)
                    {
                        disc = basePrice * (decimal)v.PhanTramGiam.Value / 100m;
                        if (v.GiamToiDa.HasValue && v.GiamToiDa.Value > 0 && disc > (decimal)v.GiamToiDa.Value)
                        {
                            disc = (decimal)v.GiamToiDa.Value;
                        }
                    }
                    else if (v.TienGiam.HasValue && v.TienGiam.Value > 0)
                    {
                        disc = (decimal)v.TienGiam.Value;
                    }

                    // Ưu tiên mức giảm cao nhất
                    if (disc > maxDiscount)
                    {
                        maxDiscount = disc;
                        bestPromoText = $"{v.MaVoucher} (Nhóm -{disc:N0}đ)";
                    }
                }
            }

            // Chốt chặn giá vốn BOM chống bán lỗ trên đĩa món
            decimal giaVon = mon.GiaVonUocTinh;
            decimal effectivePrice = basePrice - maxDiscount;
            if (giaVon > 0 && effectivePrice < giaVon)
            {
                maxDiscount = Math.Max(0m, basePrice - giaVon);
                effectivePrice = basePrice - maxDiscount;
            }

            return (effectivePrice, bestPromoText, maxDiscount);
        }

        public (decimal effectivePrice, string promoText) GetDishPriceInfo(MonAnDto mon, IEnumerable<KmMonDto> kmMonList)
        {
            var (price, text, _) = GetDishPriceInfo(mon, null, kmMonList);
            return (price, text);
        }

        /// <summary>
        /// Đánh giá điều kiện áp dụng và số tiền giảm của một Voucher đối với tổng tiền món (subTotal).
        /// </summary>
        public (bool isEligible, decimal discount, decimal missingAmount) EvaluateVoucher(VoucherDto v, decimal subTotal)
        {
            return EvaluateVoucher(v, subTotal, null);
        }

        /// <summary>
        /// Điểm 4 & Điểm 1: Đánh giá Voucher toàn hóa đơn (TOAN_DON) dựa trên Subtotal sau khi đã giảm giá món/loại món.
        /// Chốt chặn an toàn giá vốn (Floor Margin): Không để tổng tiền sau giảm thấp hơn tổng giá vốn BOM.
        /// </summary>
        public (bool isEligible, decimal discount, decimal missingAmount) EvaluateVoucher(VoucherDto v, decimal subTotalAfterTier1, IEnumerable<CartItem> cart)
        {
            if (v == null || subTotalAfterTier1 <= 0) return (false, 0m, 0m);

            decimal minOrder = (decimal)(v.GiaTriDonToiThieu ?? 0);
            if (subTotalAfterTier1 < minOrder)
            {
                return (false, 0m, minOrder - subTotalAfterTier1);
            }

            decimal discount = 0m;
            if (v.PhanTramGiam.HasValue && v.PhanTramGiam.Value > 0)
            {
                discount = subTotalAfterTier1 * (decimal)v.PhanTramGiam.Value / 100m;
                if (v.GiamToiDa.HasValue && v.GiamToiDa.Value > 0 && discount > (decimal)v.GiamToiDa.Value)
                {
                    discount = (decimal)v.GiamToiDa.Value;
                }
            }
            else if (v.TienGiam.HasValue && v.TienGiam.Value > 0)
            {
                discount = (decimal)v.TienGiam.Value;
            }

            if (discount > subTotalAfterTier1) discount = subTotalAfterTier1;

            // Chốt chặn bảo vệ giá vốn (Floor Margin) cho nhà hàng
            if (cart != null)
            {
                decimal totalCogs = cart.Sum(x => x.GiaVon * x.Quantity);
                if (totalCogs > 0 && (subTotalAfterTier1 - discount) < totalCogs)
                {
                    decimal safeDiscount = Math.Max(0m, subTotalAfterTier1 - totalCogs);
                    if (discount > safeDiscount)
                    {
                        discount = safeDiscount;
                    }
                }
            }

            return (true, discount, 0m);
        }

        /// <summary>
        /// Điểm 1: Thuật toán Smart Best Deal: Tự động tìm voucher hóa đơn có mức chiết khấu cao nhất đã đủ điều kiện.
        /// </summary>
        public (VoucherDto bestVoucher, decimal bestDiscount, VoucherDto nearVoucher, decimal missingForNear) FindBestVoucher(decimal subTotalAfterTier1, IEnumerable<VoucherDto> vouchers, IEnumerable<CartItem> cart = null)
        {
            if (vouchers == null || subTotalAfterTier1 <= 0) return (null, 0m, null, 0m);
            var activeOrderVouchers = vouchers.Where(v => v.TrangThai == "Đang áp dụng" 
                && (string.IsNullOrEmpty(v.PhamViApDung) || v.PhamViApDung == "TOAN_DON")).ToList();

            VoucherDto best = null;
            decimal maxDiscount = 0m;

            VoucherDto near = null;
            decimal minMissing = decimal.MaxValue;

            foreach (var v in activeOrderVouchers)
            {
                var (isEligible, discount, missing) = EvaluateVoucher(v, subTotalAfterTier1, cart);
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

        /// <summary>
        /// Điểm 1: Lấy danh sách tất cả voucher hóa đơn để khách hàng/thu ngân có thể chọn voucher khác nếu muốn.
        /// </summary>
        public List<(VoucherDto voucher, bool isEligible, decimal discount, decimal missingAmount)> GetAvailableOrderVouchers(
            decimal subTotalAfterTier1, 
            IEnumerable<VoucherDto> vouchers, 
            IEnumerable<CartItem> cart = null)
        {
            var result = new List<(VoucherDto voucher, bool isEligible, decimal discount, decimal missingAmount)>();
            if (vouchers == null) return result;

            var activeOrderVouchers = vouchers.Where(v => v.TrangThai == "Đang áp dụng" 
                && (string.IsNullOrEmpty(v.PhamViApDung) || v.PhamViApDung == "TOAN_DON")).ToList();

            foreach (var v in activeOrderVouchers)
            {
                var (isEligible, discount, missing) = EvaluateVoucher(v, subTotalAfterTier1, cart);
                result.Add((v, isEligible, discount, missing));
            }

            return result.OrderByDescending(x => x.discount).ThenBy(x => x.missingAmount).ToList();
        }

        /// <summary>
        /// Tính toán tổng tiền hàng, số tiền giảm voucher hóa đơn và tổng thanh toán cuối cùng.
        /// </summary>
        public (decimal subTotal, decimal voucherDiscount, decimal finalTotal) CalculateTotals(IEnumerable<CartItem> cart, VoucherDto appliedVoucher)
        {
            if (cart == null) return (0m, 0m, 0m);
            decimal subTotal = cart.Sum(x => x.Price * x.Quantity);
            decimal voucherDiscount = 0m;

            if (appliedVoucher != null)
            {
                var (isEligible, disc, _) = EvaluateVoucher(appliedVoucher, subTotal, cart);
                if (isEligible) voucherDiscount = disc;
            }

            decimal finalTotal = Math.Max(0, subTotal - voucherDiscount);
            return (subTotal, voucherDiscount, finalTotal);
        }
    }
}

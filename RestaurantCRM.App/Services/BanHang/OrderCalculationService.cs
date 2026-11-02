using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Services.BanHang
{
    /// <summary>
    /// Chuyên trách xử lý tính toán tài chính cho đơn hàng:
    /// - Đơn giá hiệu lực sau khuyến mãi món / loại món (Tầng 1)
    /// - Điều kiện áp dụng và mức chiết khấu của voucher toàn đơn (Tầng 2)
    /// - Thuật toán Smart Best Deal tìm voucher toàn đơn tối ưu nhất
    /// - Tổng tiền hàng, chiết khấu và tổng thanh toán cuối cùng
    /// </summary>
    public class OrderCalculationService
    {
        /// <summary>
        /// Tính đơn giá bán thực tế của món ăn theo Tầng 1:
        /// Khi món vừa có voucher riêng (THEO_MON) vừa có voucher theo loại món (THEO_LOAI):
        /// -> Tự động so sánh và lấy mức giảm cao nhất Max(Món, Loại), KHÔNG cộng dồn.
        /// -> Bảo vệ giá sàn (Floor margin): Đơn giá sau giảm không được thấp hơn giá vốn BOM.
        /// </summary>
        public (decimal effectivePrice, string promoText, decimal discountPerDish, string appliedVoucherCode) GetDishPriceInfo(
            MonAnDto mon, 
            IEnumerable<VoucherDto> vouchers)
        {
            if (mon == null) return (0m, null, 0m, null);
            decimal basePrice = mon.Gia;
            if (basePrice <= 0) return (0m, null, 0m, null);

            decimal maxDiscount = 0m;
            string bestPromoText = null;
            string bestVoucherCode = null;

            if (vouchers != null)
            {
                // 1. Kiểm tra Voucher theo Món (THEO_MON)
                var dishVouchers = vouchers.Where(v => v.TrangThai == "Đang áp dụng" 
                    && v.PhamViApDung == "THEO_MON" 
                    && string.Equals(v.MaMon, mon.MaMonAn, StringComparison.OrdinalIgnoreCase));

                foreach (var v in dishVouchers)
                {
                    decimal minItemPrice = (decimal)(v.GiaTriDonToiThieu ?? 0);
                    if (minItemPrice > 0 && basePrice < minItemPrice)
                    {
                        continue;
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

                    if (disc > maxDiscount)
                    {
                        maxDiscount = disc;
                        bestPromoText = $"{v.MaVoucher} (-{disc:N0}đ)";
                        bestVoucherCode = v.MaVoucher;
                    }
                }

                // 2. Kiểm tra Voucher theo Loại Món (THEO_LOAI)
                var catVouchers = vouchers.Where(v => v.TrangThai == "Đang áp dụng" 
                    && v.PhamViApDung == "THEO_LOAI" 
                    && string.Equals(v.MaLoaiMon, mon.MaLoaiMon, StringComparison.OrdinalIgnoreCase));

                foreach (var v in catVouchers)
                {
                    decimal minItemPrice = (decimal)(v.GiaTriDonToiThieu ?? 0);
                    if (minItemPrice > 0 && basePrice < minItemPrice)
                    {
                        continue;
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

                    if (disc > maxDiscount)
                    {
                        maxDiscount = disc;
                        bestPromoText = $"{v.MaVoucher} (Nhóm -{disc:N0}đ)";
                        bestVoucherCode = v.MaVoucher;
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

            return (effectivePrice, bestPromoText, maxDiscount, bestVoucherCode);
        }

        /// <summary>
        /// Đánh giá điều kiện áp dụng và số tiền giảm của một Voucher toàn đơn đối với tổng tiền món (subTotal).
        /// </summary>
        public (bool isEligible, decimal discount, decimal missingAmount) EvaluateVoucher(VoucherDto v, decimal subTotal)
        {
            return EvaluateVoucher(v, subTotal, null);
        }

        /// <summary>
        /// Đánh giá Voucher toàn đơn (TOAN_DON) dựa trên Subtotal sau khi đã giảm giá món/loại món (Tầng 1).
        /// Chốt chặn an toàn giá vốn (Floor Margin): Không để tổng tiền sau giảm thấp hơn tổng giá vốn BOM.
        /// </summary>
        public (bool isEligible, decimal discount, decimal missingAmount) EvaluateVoucher(VoucherDto v, decimal subTotalAfterTier1, IEnumerable<CartItem> cart)
        {
            if (v == null || subTotalAfterTier1 <= 0) return (false, 0m, 0m);

            string scope = string.IsNullOrEmpty(v.PhamViApDung) ? "TOAN_DON" : v.PhamViApDung;
            if (scope != "TOAN_DON") return (false, 0m, 0m);

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
        /// Thuật toán Smart Best Deal: Tự động tìm voucher toàn đơn có mức chiết khấu cao nhất đã đủ điều kiện.
        /// </summary>
        public (VoucherDto bestVoucher, decimal bestDiscount) FindBestVoucher(decimal subTotalAfterTier1, IEnumerable<VoucherDto> vouchers, IEnumerable<CartItem> cart = null)
        {
            if (vouchers == null || subTotalAfterTier1 <= 0) return (null, 0m);
            var activeOrderVouchers = vouchers.Where(v => v.TrangThai == "Đang áp dụng" 
                && (string.IsNullOrEmpty(v.PhamViApDung) || v.PhamViApDung == "TOAN_DON")).ToList();

            VoucherDto best = null;
            decimal maxDiscount = 0m;

            foreach (var v in activeOrderVouchers)
            {
                var (isEligible, discount, _) = EvaluateVoucher(v, subTotalAfterTier1, cart);
                if (isEligible && discount > maxDiscount)
                {
                    maxDiscount = discount;
                    best = v;
                }
            }

            return (best, maxDiscount);
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

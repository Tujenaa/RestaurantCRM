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
        /// Tính đơn giá bán thực tế và thông tin khuyến mãi của món ăn (nếu có).
        /// </summary>
        public (decimal effectivePrice, string promoText) GetDishPriceInfo(MonAnDto mon, IEnumerable<KmMonDto> kmMonList)
        {
            if (mon == null) return (0m, null);
            var promo = kmMonList?.FirstOrDefault(k => k.MaMon == mon.MaMonAn && k.TrangThai == "Đang áp dụng");
            if (promo != null)
            {
                decimal price = (decimal)promo.DonGiaSauGiam;
                return (price, promo.HinhThuc);
            }
            return (mon.Gia, null);
        }

        /// <summary>
        /// Đánh giá điều kiện áp dụng và số tiền giảm của một Voucher đối với tổng tiền món (subTotal).
        /// </summary>
        public (bool isEligible, decimal discount, decimal missingAmount) EvaluateVoucher(VoucherDto v, decimal subTotal)
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

        /// <summary>
        /// Thuật toán Smart Best Deal: Tìm voucher có mức chiết khấu cao nhất đã đủ điều kiện,
        /// hoặc voucher cận điều kiện (gần đạt nhất) để gợi ý mua thêm.
        /// </summary>
        public (VoucherDto bestVoucher, decimal bestDiscount, VoucherDto nearVoucher, decimal missingForNear) FindBestVoucher(decimal subTotal, IEnumerable<VoucherDto> vouchers)
        {
            if (vouchers == null || subTotal <= 0) return (null, 0m, null, 0m);
            var activeVouchers = vouchers.Where(v => v.TrangThai == "Đang áp dụng").ToList();
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

        /// <summary>
        /// Tính toán tổng tiền hàng, số tiền giảm voucher và tổng thanh toán.
        /// </summary>
        public (decimal subTotal, decimal voucherDiscount, decimal finalTotal) CalculateTotals(IEnumerable<CartItem> cart, VoucherDto appliedVoucher)
        {
            if (cart == null) return (0m, 0m, 0m);
            decimal subTotal = cart.Sum(x => x.Price * x.Quantity);
            decimal voucherDiscount = 0m;

            if (appliedVoucher != null)
            {
                var (isEligible, disc, _) = EvaluateVoucher(appliedVoucher, subTotal);
                if (isEligible) voucherDiscount = disc;
            }

            decimal finalTotal = Math.Max(0, subTotal - voucherDiscount);
            return (subTotal, voucherDiscount, finalTotal);
        }
    }
}

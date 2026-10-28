using System;

namespace RestaurantCRM.AdminApp.Models
{
    /// <summary>
    /// Đại diện cho một món ăn đã được thêm vào giỏ hàng của đơn bán mang đi tại quầy.
    /// </summary>
    public class CartItem
    {
        public string MaMon;
        public string Name;
        public decimal BasePrice;       // Đơn giá gốc
        public decimal Price;           // Đơn giá bán (đã trừ KM món nếu có)
        public int Quantity;
        public int MaxStock;
        public string PromoInfo;        // Tên/mức giảm món nếu có
        public string MaLoaiMon;        // Mã loại món để áp dụng voucher danh mục
        public decimal GiaVon;          // Giá vốn nguyên liệu định lượng (BOM)
    }
}

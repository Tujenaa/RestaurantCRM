namespace RestaurantCRM.API.DTOs
{
    public class OrderRequest
    {
        public string? MaKhachHang { get; set; }
        public string? TenKhachHang { get; set; }
        public string? SoDienThoai { get; set; }
        public string? DiaChiGiao { get; set; }
        public string? MaKmvoucher { get; set; }
        public string? PhuongThucThanhToan { get; set; }
        public string? MaNhanVien { get; set; }
        public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
    }

    public class OrderItemRequest
    {
        public string MaMon { get; set; } = null!;
        public int SoLuong { get; set; }
    }

    public class CartCheckRequest
    {
        public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
    }

    public class CartCheckResult
    {
        public bool HopLe { get; set; }
        public string? ThongBao { get; set; }
        public List<CartItemErrorDetail> ChiTietLoi { get; set; } = new List<CartItemErrorDetail>();
    }

    public class CartItemErrorDetail
    {
        public string MaMon { get; set; } = null!;
        public string? TenMon { get; set; }
        public int SoLuongYeuCau { get; set; }
        public int SoLuongKhaDung { get; set; }
        public string? LyDo { get; set; }
    }
}

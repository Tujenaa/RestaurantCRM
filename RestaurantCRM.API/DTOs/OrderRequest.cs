namespace RestaurantCRM.API.DTOs
{
    public class OrderRequest
    {
        public string MaKhachHang { get; set; } = null!;
        public string? TenKhachHang { get; set; }
        public string? SoDienThoai { get; set; }
        public string? DiaChiGiao { get; set; }
        public string? MaKmvoucher { get; set; }
        public string? PhuongThucThanhToan { get; set; }
        public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
    }

    public class OrderItemRequest
    {
        public string MaMon { get; set; } = null!;
        public int SoLuong { get; set; }
    }
}

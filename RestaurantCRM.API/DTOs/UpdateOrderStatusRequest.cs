namespace RestaurantCRM.API.DTOs
{
    public class UpdateOrderStatusRequest
    {
        public string TrangThaiMoi { get; set; } = null!;
        public string? GhiChu { get; set; }
    }
}

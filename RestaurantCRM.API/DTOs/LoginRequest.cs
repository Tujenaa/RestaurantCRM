namespace RestaurantCRM.API.DTOs
{
    public class LoginRequest
    {
        public string EmailOrPhone { get; set; } = null!;
        public string MatKhau { get; set; } = null!;
    }
}

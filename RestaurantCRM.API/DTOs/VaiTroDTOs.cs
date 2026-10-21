using System.Collections.Generic;

namespace RestaurantCRM.API.DTOs
{
    public class VaiTroDto
    {
        public string MaVaiTro { get; set; } = null!;
        public string? TenVaiTro { get; set; }
        public string? MoTa { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
        public int SoLuongNhanVien { get; set; }
    }

    public class SaveVaiTroRequest
    {
        public string MaVaiTro { get; set; } = null!;
        public string TenVaiTro { get; set; } = null!;
        public string? MoTa { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
    }

    public class PermissionDefinitionDto
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Group { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}

using Microsoft.EntityFrameworkCore;
using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.BusinessLogic.Services;

namespace RestaurantCRM.API.Helpers
{
    public static class KhachHangMoiHelper
    {
        private const int SoLanThuToiDa = 3;

        /// <summary>
        /// Gán mã mới cho khách hàng rồi lưu. Nếu 2 yêu cầu cùng lúc sinh trùng mã thì sinh lại và thử lại.
        /// Trả về null khi lưu thành công, ngược lại trả về thông báo lỗi tiếng Việt.
        /// </summary>
        public static async Task<string?> LuuKhachHangMoiAsync(IRestaurantService service, KhachHang khachHang)
        {
            for (int lan = 1; lan <= SoLanThuToiDa; lan++)
            {
                khachHang.MaKhachHang = await CodeGenerator.GenerateMaKhachHangAsync(service);
                service.Add(khachHang);
                try
                {
                    await service.SaveChangesAsync();
                    return null;
                }
                catch (DbUpdateException ex) when (DbErrorHelper.IsUniqueViolation(ex))
                {
                    // Bỏ bản ghi đang chờ lưu khỏi context trước khi thử lại hoặc báo lỗi
                    service.Remove(khachHang);

                    bool trungMa = await service.AnyAsync<KhachHang>(k => k.MaKhachHang == khachHang.MaKhachHang);
                    if (!trungMa)
                    {
                        return "Email hoặc số điện thoại đã được sử dụng.";
                    }
                }
            }

            return "Hệ thống đang bận, không thể tạo mã khách hàng. Vui lòng thử lại.";
        }
    }
}

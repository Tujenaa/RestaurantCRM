using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.BusinessLogic.Services;

namespace RestaurantCRM.BusinessLogic.Services
{
    public static class CodeGenerator
    {
        public static async Task<string> GenerateMaLoaiMonAsync(IRestaurantService service)
        {
            var codes = await service.ListAsync<LoaiMon, string>(x => x.MaLoaiMon != null && x.MaLoaiMon.StartsWith("LM"), x => x.MaLoaiMon!);

            int maxNum = 0;
            foreach (var code in codes)
            {
                var numStr = code.Substring(2);
                if (int.TryParse(numStr, out int val) && val > maxNum)
                {
                    maxNum = val;
                }
            }

            int nextNum = maxNum + 1;
            while (await service.AnyAsync<LoaiMon>(x => x.MaLoaiMon == $"LM{nextNum:D2}"))
            {
                nextNum++;
            }
            return $"LM{nextNum:D2}";
        }

        public static async Task<string> GenerateMaNhaCungCapAsync(IRestaurantService service)
        {
            var codes = await service.ListAsync<NhaCungCap, string>(x => x.MaNhaCungCap != null && x.MaNhaCungCap.StartsWith("NCC"), x => x.MaNhaCungCap!);

            int maxNum = 0;
            foreach (var code in codes)
            {
                var numStr = code.Substring(3);
                if (int.TryParse(numStr, out int val) && val > maxNum)
                {
                    maxNum = val;
                }
            }

            int nextNum = maxNum + 1;
            while (await service.AnyAsync<NhaCungCap>(x => x.MaNhaCungCap == $"NCC{nextNum:D3}"))
            {
                nextNum++;
            }
            return $"NCC{nextNum:D3}";
        }

        public static async Task<string> GenerateMaMonAnAsync(IRestaurantService service)
        {
            var codes = await service.ListAsync<MonAn, string>(x => x.MaMon != null, x => x.MaMon!);

            int maxNum = 0;
            foreach (var code in codes)
            {
                string numStr = code;
                if (code.StartsWith("MA", StringComparison.OrdinalIgnoreCase))
                    numStr = code.Substring(2);
                else if (code.StartsWith("M", StringComparison.OrdinalIgnoreCase))
                    numStr = code.Substring(1);

                if (int.TryParse(numStr, out int val) && val > maxNum)
                {
                    maxNum = val;
                }
            }

            int nextNum = maxNum + 1;
            while (await service.AnyAsync<MonAn>(x => x.MaMon == $"M{nextNum:D2}"))
            {
                nextNum++;
            }
            return $"M{nextNum:D2}";
        }

        public static async Task<string> GenerateMaNguyenLieuAsync(IRestaurantService service)
        {
            var codes = await service.ListAsync<NguyenLieu, string>(x => x.MaNguyenLieu != null && x.MaNguyenLieu.StartsWith("NL"), x => x.MaNguyenLieu!);

            int maxNum = 0;
            foreach (var code in codes)
            {
                var numStr = code.Substring(2);
                if (int.TryParse(numStr, out int val) && val > maxNum)
                {
                    maxNum = val;
                }
            }

            int nextNum = maxNum + 1;
            while (await service.AnyAsync<NguyenLieu>(x => x.MaNguyenLieu == $"NL{nextNum:D2}"))
            {
                nextNum++;
            }
            return $"NL{nextNum:D2}";
        }

        public static async Task<string> GenerateMaDinhLuongAsync(IRestaurantService service)
        {
            var codes = await service.ListAsync<DinhLuong, string>(x => x.MaDinhLuong != null && x.MaDinhLuong.StartsWith("DL"), x => x.MaDinhLuong!);

            int maxNum = 0;
            foreach (var code in codes)
            {
                var numStr = code.Substring(2);
                if (int.TryParse(numStr, out int val) && val > maxNum)
                {
                    maxNum = val;
                }
            }

            int nextNum = maxNum + 1;
            while (await service.AnyAsync<DinhLuong>(x => x.MaDinhLuong == $"DL{nextNum:D2}"))
            {
                nextNum++;
            }
            return $"DL{nextNum:D2}";
        }

        public static async Task<string> GenerateMaPhieuNhapAsync(IRestaurantService service)
        {
            string prefix = $"PN{DateTime.Now:yyMMdd}-";
            var codes = await service.ListAsync<PhieuNhapHang, string>(x => x.MaPhieuNhap != null && x.MaPhieuNhap.StartsWith(prefix), x => x.MaPhieuNhap!);

            int maxNum = 0;
            foreach (var code in codes)
            {
                var numStr = code.Substring(prefix.Length);
                if (int.TryParse(numStr, out int val) && val > maxNum)
                {
                    maxNum = val;
                }
            }

            int nextNum = maxNum + 1;
            while (await service.AnyAsync<PhieuNhapHang>(x => x.MaPhieuNhap == $"{prefix}{nextNum:D3}"))
            {
                nextNum++;
            }
            return $"{prefix}{nextNum:D3}";
        }

        public static async Task<string> GenerateMaHoaDonAsync(IRestaurantService service)
        {
            string prefix = $"HD{DateTime.Now:yyMMdd}-";
            var codes = await service.ListAsync<HoaDon, string>(x => x.MaHoaDon != null && x.MaHoaDon.StartsWith(prefix), x => x.MaHoaDon!);

            int maxNum = 0;
            foreach (var code in codes)
            {
                var numStr = code.Substring(prefix.Length);
                if (int.TryParse(numStr, out int val) && val > maxNum)
                {
                    maxNum = val;
                }
            }

            int nextNum = maxNum + 1;
            while (await service.AnyAsync<HoaDon>(x => x.MaHoaDon == $"{prefix}{nextNum:D3}"))
            {
                nextNum++;
            }
            return $"{prefix}{nextNum:D3}";
        }
    }
}

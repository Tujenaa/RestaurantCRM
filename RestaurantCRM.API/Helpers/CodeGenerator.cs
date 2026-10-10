using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;

namespace RestaurantCRM.API.Helpers
{
    public static class CodeGenerator
    {
        public static async Task<string> GenerateMaLoaiMonAsync(RestaurantCrmContext context)
        {
            var codes = await context.LoaiMon
                .Where(x => x.MaLoaiMon != null && x.MaLoaiMon.StartsWith("LM"))
                .Select(x => x.MaLoaiMon!)
                .ToListAsync();

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
            while (await context.LoaiMon.AnyAsync(x => x.MaLoaiMon == $"LM{nextNum:D2}"))
            {
                nextNum++;
            }
            return $"LM{nextNum:D2}";
        }

        public static async Task<string> GenerateMaNhaCungCapAsync(RestaurantCrmContext context)
        {
            var codes = await context.NhaCungCap
                .Where(x => x.MaNhaCungCap != null && x.MaNhaCungCap.StartsWith("NCC"))
                .Select(x => x.MaNhaCungCap!)
                .ToListAsync();

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
            while (await context.NhaCungCap.AnyAsync(x => x.MaNhaCungCap == $"NCC{nextNum:D3}"))
            {
                nextNum++;
            }
            return $"NCC{nextNum:D3}";
        }

        public static async Task<string> GenerateMaMonAnAsync(RestaurantCrmContext context)
        {
            var codes = await context.MonAn
                .Where(x => x.MaMon != null && x.MaMon.StartsWith("MA"))
                .Select(x => x.MaMon!)
                .ToListAsync();

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
            while (await context.MonAn.AnyAsync(x => x.MaMon == $"MA{nextNum:D3}"))
            {
                nextNum++;
            }
            return $"MA{nextNum:D3}";
        }

        public static async Task<string> GenerateMaPhieuNhapAsync(RestaurantCrmContext context)
        {
            string prefix = $"PN{DateTime.Now:yyMMdd}-";
            var codes = await context.PhieuNhapHang
                .Where(x => x.MaPhieuNhap != null && x.MaPhieuNhap.StartsWith(prefix))
                .Select(x => x.MaPhieuNhap!)
                .ToListAsync();

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
            while (await context.PhieuNhapHang.AnyAsync(x => x.MaPhieuNhap == $"{prefix}{nextNum:D3}"))
            {
                nextNum++;
            }
            return $"{prefix}{nextNum:D3}";
        }

        public static async Task<string> GenerateMaHoaDonAsync(RestaurantCrmContext context)
        {
            string prefix = $"HD{DateTime.Now:yyMMdd}-";
            var codes = await context.HoaDon
                .Where(x => x.MaHoaDon != null && x.MaHoaDon.StartsWith(prefix))
                .Select(x => x.MaHoaDon!)
                .ToListAsync();

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
            while (await context.HoaDon.AnyAsync(x => x.MaHoaDon == $"{prefix}{nextNum:D3}"))
            {
                nextNum++;
            }
            return $"{prefix}{nextNum:D3}";
        }
    }
}

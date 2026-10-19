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
                .Where(x => x.MaMon != null)
                .Select(x => x.MaMon!)
                .ToListAsync();

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
            while (await context.MonAn.AnyAsync(x => x.MaMon == $"M{nextNum:D2}"))
            {
                nextNum++;
            }
            return $"M{nextNum:D2}";
        }

        public static async Task<string> GenerateMaNguyenLieuAsync(RestaurantCrmContext context)
        {
            var codes = await context.NguyenLieu
                .Where(x => x.MaNguyenLieu != null && x.MaNguyenLieu.StartsWith("NL"))
                .Select(x => x.MaNguyenLieu!)
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
            while (await context.NguyenLieu.AnyAsync(x => x.MaNguyenLieu == $"NL{nextNum:D2}"))
            {
                nextNum++;
            }
            return $"NL{nextNum:D2}";
        }

        public static async Task<string> GenerateMaDinhLuongAsync(RestaurantCrmContext context)
        {
            var codes = await context.DinhLuong
                .Where(x => x.MaDinhLuong != null && x.MaDinhLuong.StartsWith("DL"))
                .Select(x => x.MaDinhLuong!)
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
            while (await context.DinhLuong.AnyAsync(x => x.MaDinhLuong == $"DL{nextNum:D2}"))
            {
                nextNum++;
            }
            return $"DL{nextNum:D2}";
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

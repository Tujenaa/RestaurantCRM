using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.BusinessLogic.Services;

namespace RestaurantCRM.BusinessLogic.Services
{
    public static class CodeGenerator
    {
        public static string RemoveVietnameseAccents(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            string[] vietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };
            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                    text = text.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
            }
            return text;
        }

        public static string GeneratePrefixFromTenLoaiMon(string tenLoaiMon)
        {
            if (string.IsNullOrWhiteSpace(tenLoaiMon)) return "LM";

            string unaccented = RemoveVietnameseAccents(tenLoaiMon).Trim().ToUpper();
            var words = unaccented.Split(new[] { ' ', ',', '-', '_', '&', '+', '/', '\\', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return "LM";

            // Nếu 1 từ (ví dụ "Lẩu" -> "LAU", "Rau" -> "RAU", "Mì" -> "MI")
            if (words.Length == 1)
            {
                return words[0].Length > 6 ? words[0].Substring(0, 6) : words[0];
            }

            // Nếu có chứa từ "LAU" -> ưu tiên "LAU"
            if (words.Contains("LAU"))
            {
                return "LAU";
            }

            // Nếu 2 từ ngắn thông dụng: "DO UONG" -> "DU", "TRANG MIENG" -> "TM", "KHAI VI" -> "KV"
            if (words.Length == 2)
            {
                string combined = string.Concat(words);
                if (combined == "DOUONG") return "DU";
                if (combined == "TRANGMIENG") return "TM";
                if (combined == "KHAIVI") return "KV";
            }

            // Nếu từ đầu tiên có ý nghĩa và độ dài 2-5 ký tự (ví dụ: "THIT", "VIEN", "RAU", "MI")
            string first = words[0];
            if (first.Length >= 2 && first.Length <= 5)
            {
                return first;
            }

            // Rút gọn chữ cái đầu
            string acronym = string.Concat(words.Select(w => w[0]));
            if (acronym.Length >= 2 && acronym.Length <= 5)
            {
                return acronym;
            }

            return words[0].Length > 5 ? words[0].Substring(0, 5) : words[0];
        }

        public static async Task<string> GenerateMaLoaiMonAsync(IRestaurantService service, string? tenLoaiMon = null)
        {
            const string prefix = "LM";
            var codes = await service.ListAsync<LoaiMon, string>(
                x => x.MaLoaiMon != null && x.MaLoaiMon.StartsWith(prefix),
                x => x.MaLoaiMon!);

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
            while (await service.AnyAsync<LoaiMon>(x => x.MaLoaiMon == $"{prefix}{nextNum:D3}"))
            {
                nextNum++;
            }
            return $"{prefix}{nextNum:D3}";
        }

        public static Task<string> GenerateMaLoaiMonAsync(IRestaurantService service)
        {
            return GenerateMaLoaiMonAsync(service, null);
        }

        public static async Task<string> GenerateMaMonAnAsync(IRestaurantService service, string? maLoaiMon = null)
        {
            const string prefix = "MA";

            var codes = await service.ListAsync<MonAn, string>(
                x => x.MaMon != null && x.MaMon.StartsWith(prefix),
                x => x.MaMon!);

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
            while (await service.AnyAsync<MonAn>(x => x.MaMon == $"{prefix}{nextNum:D3}"))
            {
                nextNum++;
            }
            return $"{prefix}{nextNum:D3}";
        }

        public static Task<string> GenerateMaMonAnAsync(IRestaurantService service)
        {
            return GenerateMaMonAnAsync(service, null);
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

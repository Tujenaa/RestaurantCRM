using System.Globalization;
using System.Text;

namespace RestaurantCRM.Web.Helpers;

/// <summary>
/// Tìm kiếm tiếng Việt – cùng quy tắc với app WinForms (UiStyler.MatchesSearch).
/// Câu tìm được tách thành các từ; MỌI từ phải khớp ở ít nhất một trường:
///  • Từ có chữ số (mã, SĐT, số tiền, ngày): khớp ở bất kỳ vị trí ("012" ra "HD012").
///    Từ chỉ gồm số thì so thêm với phần chữ số của trường ("350000" ra "350.000 đ").
///  • Từ chỉ có chữ: khớp ở ĐẦU một chữ ("an" ra "An", "Anh"; không ra "Thanh").
///  • Gõ không dấu → so không dấu ("bo" ra Bò, Bơ); gõ có dấu → so đúng dấu ("bò" chỉ ra Bò).
///  • Không phân biệt hoa/thường; chuẩn hóa Unicode (NFC) cả hai phía.
/// </summary>
public static class SearchHelper
{
    public static bool MatchesSearch(string? query, params string?[] fields)
    {
        if (string.IsNullOrWhiteSpace(query)) return true;
        if (fields == null || fields.Length == 0) return false;

        var tokens = PrepareTokens(query);
        if (tokens.Count == 0) return true;

        var accented = new string?[fields.Length];
        var plain = new string?[fields.Length];
        var digits = new string?[fields.Length];

        foreach (var t in tokens)
        {
            bool found = false;
            for (int i = 0; i < fields.Length && !found; i++)
            {
                var f = fields[i];
                if (string.IsNullOrEmpty(f)) continue;
                string hay = t.Accented
                    ? accented[i] ??= Fold(f, true)
                    : plain[i] ??= Fold(f, false);

                if (t.AnyPosition)
                {
                    if (hay.Contains(t.Key, StringComparison.Ordinal)) found = true;
                    else if (t.Digits != null)
                    {
                        string d = digits[i] ??= DigitsOnly(f);
                        found = d.Length > 0 && d.Contains(t.Digits, StringComparison.Ordinal);
                    }
                }
                else found = StartsWordAt(hay, t.Key);
            }
            if (!found) return false;
        }
        return true;
    }

    /// <summary>Chuẩn hóa về chữ thường không dấu ("Đỗ" -> "do").</summary>
    public static string RemoveDiacritics(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        string normalized = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (char c in normalized)
        {
            if (c == 'đ' || c == 'Đ') { sb.Append('d'); continue; }
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private sealed class SearchToken
    {
        public string Key = "";       // từ khóa đã chuẩn hóa (giữ hoặc bỏ dấu tùy Accented)
        public bool Accented;         // gõ có dấu -> so đúng dấu
        public bool AnyPosition;      // có chữ số / bắt đầu bằng ký hiệu -> khớp ở bất kỳ vị trí
        public string? Digits;        // từ chỉ gồm số -> phần chữ số để so
    }

    private static List<SearchToken> PrepareTokens(string query)
    {
        var list = new List<SearchToken>();
        foreach (var raw in query.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
        {
            bool accented = HasDiacritics(raw);
            string key = Fold(raw, accented);
            if (key.Length == 0) continue;

            bool hasDigit = false, onlyNumeric = true;
            foreach (char c in key)
            {
                if (char.IsDigit(c)) hasDigit = true;
                else if (c != '.' && c != ',' && c != '-' && c != '/') onlyNumeric = false;
            }
            list.Add(new SearchToken
            {
                Key = key,
                Accented = accented,
                AnyPosition = hasDigit || !char.IsLetterOrDigit(key[0]),
                Digits = hasDigit && onlyNumeric ? DigitsOnly(key) : null
            });
        }
        return list;
    }

    private static bool HasDiacritics(string s)
        => s.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormC) != RemoveDiacritics(s);

    private static string Fold(string s, bool keepAccents)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return keepAccents ? s.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormC) : RemoveDiacritics(s);
    }

    private static string DigitsOnly(string s)
    {
        var sb = new StringBuilder(s.Length);
        foreach (char c in s) if (c >= '0' && c <= '9') sb.Append(c);
        return sb.ToString();
    }

    /// <summary>Từ khóa xuất hiện ở đầu một chữ (đầu chuỗi hoặc sau ký tự không phải chữ/số).</summary>
    private static bool StartsWordAt(string hay, string key)
    {
        int idx = hay.IndexOf(key, StringComparison.Ordinal);
        while (idx >= 0)
        {
            if (idx == 0 || !char.IsLetterOrDigit(hay[idx - 1])) return true;
            idx = hay.IndexOf(key, idx + 1, StringComparison.Ordinal);
        }
        return false;
    }
}

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantCRM.AdminApp.Config;

namespace RestaurantCRM.AdminApp.Services
{
    /// <summary>Kết quả một thao tác ghi (thêm / sửa / xóa) gọi API.</summary>
    public class ApiResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        /// <summary>Nội dung JSON máy chủ trả về khi thành công (để đọc mã vừa tạo...).</summary>
        public string Body { get; set; }

        /// <summary>Đọc một trường chuỗi trong Body (không phân biệt hoa thường), ví dụ Field("maKhaoSat").</summary>
        public string Field(string name)
        {
            if (string.IsNullOrWhiteSpace(Body)) return null;
            try
            {
                using (var doc = JsonDocument.Parse(Body))
                {
                    if (doc.RootElement.ValueKind != JsonValueKind.Object) return null;
                    foreach (var p in doc.RootElement.EnumerateObject())
                        if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                            return p.Value.ValueKind == JsonValueKind.String ? p.Value.GetString() : p.Value.ToString();
                }
            }
            catch { }
            return null;
        }

        public static ApiResult Ok(string message) { return new ApiResult { Success = true, Message = message }; }
        public static ApiResult Fail(string message) { return new ApiResult { Success = false, Message = message }; }
    }

    /// <summary>Lỗi khi đọc dữ liệu từ API (thông báo đã là tiếng Việt, hiển thị được cho người dùng).</summary>
    public class ApiException : Exception
    {
        public ApiException(string message) : base(message) { }
    }

    /// <summary>
    /// Lớp gọi API dùng chung: một HttpClient cho cả ứng dụng, đọc lỗi từ máy chủ thành câu tiếng Việt dễ hiểu
    /// (chuỗi thuần, { message }, hoặc ProblemDetails của ASP.NET).
    /// </summary>
    public class ApiService
    {
        private static readonly Lazy<HttpClient> _shared = new Lazy<HttpClient>(() =>
            new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl.TrimEnd('/') + "/"), Timeout = TimeSpan.FromSeconds(20) });

        protected readonly HttpClient Http;

        public ApiService(HttpClient http = null)
        {
            Http = http ?? _shared.Value;
        }

        protected async Task<T> GetAsync<T>(string url)
        {
            HttpResponseMessage res;
            try
            {
                res = await Http.GetAsync(url);
            }
            catch (Exception ex)
            {
                throw new ApiException(ConnectionError(ex));
            }
            if (!res.IsSuccessStatusCode)
            {
                throw new ApiException(await ReadErrorAsync(res, "Không tải được dữ liệu từ máy chủ."));
            }
            try
            {
                return await res.Content.ReadFromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                throw new ApiException("Dữ liệu trả về không đúng định dạng: " + ex.Message);
            }
        }

        protected Task<ApiResult> PostAsync(string url, object body, string okMessage) { return SendAsync(HttpMethod.Post, url, body, okMessage); }
        protected Task<ApiResult> PutAsync(string url, object body, string okMessage) { return SendAsync(HttpMethod.Put, url, body, okMessage); }
        protected Task<ApiResult> DeleteAsync(string url, string okMessage) { return SendAsync(HttpMethod.Delete, url, null, okMessage); }

        protected async Task<ApiResult> SendAsync(HttpMethod method, string url, object body, string okMessage)
        {
            try
            {
                var req = new HttpRequestMessage(method, url);
                if (body != null) req.Content = JsonContent.Create(body, body.GetType());
                var res = await Http.SendAsync(req);
                if (res.IsSuccessStatusCode)
                {
                    string resBody = null;
                    try { resBody = await res.Content.ReadAsStringAsync(); } catch { }
                    string serverMsg = MessageFrom(resBody);
                    var ok = ApiResult.Ok(string.IsNullOrWhiteSpace(serverMsg) ? okMessage : serverMsg);
                    ok.Body = resBody;
                    return ok;
                }
                return ApiResult.Fail(await ReadErrorAsync(res, "Máy chủ từ chối thao tác."));
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ConnectionError(ex));
            }
        }

        private static string ConnectionError(Exception ex)
        {
            if (ex is TaskCanceledException) return "Máy chủ phản hồi quá lâu. Vui lòng thử lại.";
            return "Không kết nối được API (" + ApiConfig.BaseUrl + "). Hãy kiểm tra RestaurantCRM.API đã chạy chưa.\n\nChi tiết: " + ex.Message;
        }

        /// <summary>Đọc trường message trong phản hồi thành công (nếu có).</summary>
        private static string MessageFrom(string text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text)) return null;
                text = text.Trim();
                if (text.StartsWith("\"")) return JsonSerializer.Deserialize<string>(text);
                if (!text.StartsWith("{")) return text.Length < 300 ? text : null;
                using (var doc = JsonDocument.Parse(text))
                {
                    JsonElement m;
                    if (TryGetCaseInsensitive(doc.RootElement, "message", out m) && m.ValueKind == JsonValueKind.String) return m.GetString();
                }
            }
            catch { }
            return null;
        }

        /// <summary>Đọc thông báo lỗi trả về từ API.</summary>
        public static async Task<string> ReadErrorAsync(HttpResponseMessage res, string fallback)
        {
            string text = null;
            try { text = await res.Content.ReadAsStringAsync(); } catch { }
            if (string.IsNullOrWhiteSpace(text))
            {
                switch ((int)res.StatusCode)
                {
                    case 404: return "Không tìm thấy dữ liệu (có thể đã bị xóa). Vui lòng tải lại danh sách.";
                    case 500: return "Máy chủ gặp lỗi khi xử lý. Vui lòng thử lại sau.";
                    default: return fallback + " (mã " + (int)res.StatusCode + ")";
                }
            }
            text = text.Trim();
            try
            {
                if (text.StartsWith("\"")) return JsonSerializer.Deserialize<string>(text);
                if (text.StartsWith("{"))
                {
                    using (var doc = JsonDocument.Parse(text))
                    {
                        var root = doc.RootElement;
                        JsonElement e;
                        if (TryGetCaseInsensitive(root, "message", out e) && e.ValueKind == JsonValueKind.String) return e.GetString();
                        // ProblemDetails với lỗi kiểm tra dữ liệu: { title, errors: { field: [msg] } }
                        if (TryGetCaseInsensitive(root, "errors", out e) && e.ValueKind == JsonValueKind.Object)
                        {
                            var sb = new StringBuilder();
                            foreach (var p in e.EnumerateObject())
                                if (p.Value.ValueKind == JsonValueKind.Array)
                                    foreach (var msg in p.Value.EnumerateArray()) sb.AppendLine("• " + msg.GetString());
                            if (sb.Length > 0) return "Dữ liệu chưa hợp lệ:\n" + sb.ToString().TrimEnd();
                        }
                        if (TryGetCaseInsensitive(root, "title", out e) && e.ValueKind == JsonValueKind.String) return e.GetString();
                    }
                }
            }
            catch { }
            if (text.StartsWith("<") || text.Length > 400) return fallback + " (mã " + (int)res.StatusCode + ")";
            return text;
        }

        private static bool TryGetCaseInsensitive(JsonElement obj, string name, out JsonElement value)
        {
            value = default(JsonElement);
            if (obj.ValueKind != JsonValueKind.Object) return false;
            foreach (var p in obj.EnumerateObject())
            {
                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) { value = p.Value; return true; }
            }
            return false;
        }
    }
}
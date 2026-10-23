using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.BusinessLogic.Services;
using System.ComponentModel.DataAnnotations;

namespace RestaurantCRM.API.Controllers
{
    public class CapNhatUrlHinhAnhRequest
    {
        [Required(ErrorMessage = "Đường dẫn ảnh không được để trống")]
        [Url(ErrorMessage = "Đường dẫn không đúng định dạng URL")]
        public string UrlHinhAnh { get; set; } = null!;
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AdminMonAnImageController : ControllerBase
    {
        private readonly IRestaurantService _service;
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminMonAnImageController(IRestaurantService service, IHttpClientFactory httpClientFactory)
        {
            _service = service;
            _httpClientFactory = httpClientFactory;
        }

        // GET: api/AdminMonAnImage/{maMon} - Lấy link ảnh hiện tại của món ăn
        [HttpGet("{maMon}")]
        public async Task<IActionResult> GetImageUrl(string maMon)
        {
            var monAn = await _service.Query<MonAn>()
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MaMon == maMon);

            if (monAn == null)
            {
                return NotFound(new { success = false, message = $"Không tìm thấy món ăn có mã '{maMon}'" });
            }

            return Ok(new
            {
                success = true,
                maMon = monAn.MaMon,
                tenMon = monAn.TenMon,
                duongDanAnh = monAn.DuongDanAnh
            });
        }

        // PATCH: api/AdminMonAnImage/{maMon}/url - Cập nhật link ảnh trực tuyến cho món ăn
        [HttpPatch("{maMon}/url")]
        public async Task<IActionResult> UpdateImageUrl(
            string maMon,
            [FromBody] CapNhatUrlHinhAnhRequest request,
            [FromQuery] bool skipValidation = false)
        {
            var monAn = await _service.Query<MonAn>().FirstOrDefaultAsync(m => m.MaMon == maMon);
            if (monAn == null)
            {
                return NotFound(new { success = false, message = $"Không tìm thấy món ăn có mã '{maMon}'" });
            }

            var url = request.UrlHinhAnh.Trim();

            // Kiểm tra tính hợp lệ & khả dụng của URL nếu không chọn bỏ qua kiểm tra
            if (!skipValidation)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    client.Timeout = TimeSpan.FromSeconds(6);

                    var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
                    headRequest.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) RestaurantCRM-ImageChecker/1.0");

                    using var response = await client.SendAsync(headRequest);

                    if (!response.IsSuccessStatusCode)
                    {
                        // Thử fallback GET nếu một số server chặn HTTP HEAD
                        var getRequest = new HttpRequestMessage(HttpMethod.Get, url);
                        getRequest.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) RestaurantCRM-ImageChecker/1.0");
                        getRequest.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(0, 0); // chỉ lấy 1 byte đầu
                        using var getResponse = await client.SendAsync(getRequest);

                        if (!getResponse.IsSuccessStatusCode)
                        {
                            return BadRequest(new
                            {
                                success = false,
                                message = $"Không thể truy cập hình ảnh từ đường dẫn này (Mã phản hồi HTTP: {(int)getResponse.StatusCode})."
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Đường dẫn URL không thể kết nối hoặc bị timeout: " + ex.Message
                    });
                }
            }

            // Cập nhật CSDL
            monAn.DuongDanAnh = url;
            await _service.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Cập nhật đường dẫn ảnh món ăn thành công!",
                maMon = monAn.MaMon,
                tenMon = monAn.TenMon,
                duongDanAnh = monAn.DuongDanAnh
            });
        }

        // DELETE: api/AdminMonAnImage/{maMon} - Xóa ảnh món ăn (đưa về rỗng/null)
        [HttpDelete("{maMon}")]
        public async Task<IActionResult> RemoveImageUrl(string maMon)
        {
            var monAn = await _service.Query<MonAn>().FirstOrDefaultAsync(m => m.MaMon == maMon);
            if (monAn == null)
            {
                return NotFound(new { success = false, message = $"Không tìm thấy món ăn có mã '{maMon}'" });
            }

            monAn.DuongDanAnh = null;
            await _service.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Đã xóa đường dẫn ảnh của món ăn thành công!",
                maMon = monAn.MaMon
            });
        }
    }
}

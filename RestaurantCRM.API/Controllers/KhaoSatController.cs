using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhaoSatController : ControllerBase
    {
        private readonly RestaurantCRMContext _context;

        public KhaoSatController(RestaurantCRMContext context)
        {
            _context = context;
        }

        // GET: api/KhaoSat/HienTai - Lấy bài khảo sát đang được mở
        [HttpGet("HienTai")]
        public async Task<IActionResult> GetKhaoSatHienTai()
        {
            var khaoSat = await _context.KhaoSats
                .Include(k => k.CauHoiKhaoSats)
                    .ThenInclude(c => c.TuyChonCauHois)
                .Where(k => k.TrangThai == "Active")
                .OrderByDescending(k => k.NgayTao)
                .FirstOrDefaultAsync();

            if (khaoSat == null)
            {
                return NotFound("Không có khảo sát nào đang hoạt động.");
            }

            var result = new
            {
                khaoSat.MaKhaoSat,
                khaoSat.TieuDe,
                CauHoi = khaoSat.CauHoiKhaoSats.Select(c => new
                {
                    c.MaCauHoi,
                    c.NoiDungCauHoi,
                    c.LoaiCauHoi,
                    TuyChon = c.TuyChonCauHois.Select(t => new
                    {
                        t.MaTuyChon,
                        t.NoiDungTuyChon
                    })
                })
            };

            return Ok(result);
        }

        // POST: api/KhaoSat/TraLoi - Nhận phiếu trả lời khảo sát từ khách hàng
        [HttpPost("TraLoi")]
        public async Task<IActionResult> SubmitTraLoi(SurveyAnswerRequest request)
        {
            var phieuTraLoi = new PhieuTraLoi
            {
                MaPhieuTraLoi = "PTL" + Guid.NewGuid().ToString().Substring(0, 7).ToUpper(),
                MaKhaoSat = request.MaKhaoSat,
                MaKhachHang = request.MaKhachHang,
                NgayTraLoi = DateTime.Now
            };

            _context.PhieuTraLois.Add(phieuTraLoi);

            if (request.Answers != null && request.Answers.Any())
            {
                foreach (var answer in request.Answers)
                {
                    var cauTraLoi = new CauTraLoi
                    {
                        MaCauTraLoi = "CTL" + Guid.NewGuid().ToString().Substring(0, 7).ToUpper(),
                        MaPhieuTraLoi = phieuTraLoi.MaPhieuTraLoi,
                        MaCauHoi = answer.MaCauHoi,
                        MaTuyChon = answer.MaTuyChon,
                        NoiDungTuDien = answer.NoiDungTuDien
                    };
                    _context.CauTraLois.Add(cauTraLoi);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Gửi phản hồi thành công" });
        }
    }
}

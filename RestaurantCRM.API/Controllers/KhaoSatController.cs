using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhaoSatController : ControllerBase
    {
        private readonly RestaurantCRM.BusinessLogic.Services.IRestaurantService _service;

        public KhaoSatController(RestaurantCRM.BusinessLogic.Services.IRestaurantService service)
        {
            _service = service;
        }

        // GET: api/KhaoSat/HienTai - Lấy bài khảo sát đang được mở
        [HttpGet("HienTai")]
        public async Task<IActionResult> GetKhaoSatHienTai()
        {
            var khaoSat = await _service.Query<KhaoSat>()
                .Include(k => k.CauHoiKhaoSat)
                    .ThenInclude(c => c.TuyChonCauHoi)
                .Where(k => k.TrangThai == "Open")
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
                CauHoi = khaoSat.CauHoiKhaoSat.Select(c => new
                {
                    c.MaCauHoi,
                    c.NoiDungCauHoi,
                    c.LoaiCauHoi,
                    TuyChon = c.TuyChonCauHoi.Select(t => new
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
            var daGui = await _service.Query<PhieuTraLoi>()
            .AnyAsync(p => p.MaKhaoSat == request.MaKhaoSat && p.MaKhachHang == request.MaKhachHang);
            if (daGui) return BadRequest("Bạn đã hoàn thành khảo sát này rồi.");
            var phieuTraLoi = new PhieuTraLoi
            {
                MaPhieuTraLoi = "PTL" + Guid.NewGuid().ToString().Substring(0, 7).ToUpper(),
                MaKhaoSat = request.MaKhaoSat,
                MaKhachHang = request.MaKhachHang,
                NgayTraLoi = DateTime.Now
            };

            _service.Add(phieuTraLoi);

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
                    _service.Add(cauTraLoi);
                }
            }

            await _service.SaveChangesAsync();

            return Ok(new { message = "Gửi phản hồi thành công" });
        }
        // GET: api/KhaoSat/DanhSach?maKhachHang=KH01
        [HttpGet("DanhSach")]
        public async Task<IActionResult> GetDanhSach([FromQuery] string? maKhachHang)
        {
            var dsKhaoSat = await _service.Query<KhaoSat>()
                .Include(k => k.CauHoiKhaoSat)
                .Where(k => k.TrangThai == "Open")
                .OrderByDescending(k => k.NgayTao)
                .ToListAsync();

            var daTraLoi = new HashSet<string>();
            if (!string.IsNullOrEmpty(maKhachHang))
            {
                var ds = await _service.Query<PhieuTraLoi>()
                    .Where(p => p.MaKhachHang == maKhachHang)
                    .Select(p => p.MaKhaoSat)
                    .ToListAsync();
                daTraLoi = ds.Where(x => x != null).Select(x => x!).ToHashSet();
            }

            return Ok(dsKhaoSat.Select(k => new
            {
                k.MaKhaoSat,
                k.TieuDe,
                SoCauHoi = k.CauHoiKhaoSat.Count,
                DaTraLoi = daTraLoi.Contains(k.MaKhaoSat)
            }));
        }
        // GET: api/KhaoSat/LichSu?maKhachHang=KH01
        [HttpGet("LichSu")]
        public async Task<IActionResult> GetLichSu([FromQuery] string maKhachHang)
        {
            var phieus = await _service.Query<PhieuTraLoi>()
                .Where(p => p.MaKhachHang == maKhachHang)
                .OrderByDescending(p => p.NgayTraLoi)
                .ToListAsync();

            var maKs = phieus.Select(p => p.MaKhaoSat).Distinct().ToList();
            var tieuDe = await _service.Query<KhaoSat>()
                .Where(k => maKs.Contains(k.MaKhaoSat))
                .ToDictionaryAsync(k => k.MaKhaoSat, k => k.TieuDe);

            return Ok(phieus.Select(p => new
            {
                p.MaKhaoSat,
                TieuDe = tieuDe.GetValueOrDefault(p.MaKhaoSat ?? "") ?? "(Khảo sát đã xóa)",
                NgayGui = p.NgayTraLoi
            }));
        }
        // GET: api/KhaoSat/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var khaoSat = await _service.Query<KhaoSat>()
                .Include(k => k.CauHoiKhaoSat).ThenInclude(c => c.TuyChonCauHoi)
                .FirstOrDefaultAsync(k => k.MaKhaoSat == id && k.TrangThai == "Open");

            if (khaoSat == null) return NotFound("Khảo sát không tồn tại hoặc đã đóng.");

            return Ok(new
            {
                khaoSat.MaKhaoSat,
                khaoSat.TieuDe,
                CauHoi = khaoSat.CauHoiKhaoSat.Select(c => new
                {
                    c.MaCauHoi,
                    c.NoiDungCauHoi,
                    c.LoaiCauHoi,
                    TuyChon = c.TuyChonCauHoi.Select(t => new { t.MaTuyChon, t.NoiDungTuyChon })
                })
            });
        }
    }
}
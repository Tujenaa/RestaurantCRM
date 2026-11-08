using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.API.DTOs;
using RestaurantCRM.API.Services;

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

        /// <summary>
        /// Khách được tham gia khi khảo sát không giới hạn đối tượng (công khai)
        /// hoặc khách có trong danh sách DOI_TUONG_KHAO_SAT của khảo sát.
        /// </summary>
        private static bool DuocThamGia(KhaoSat ks, string? maKhachHang) =>
            !ks.DoiTuongKhaoSat.Any() ||
            (!string.IsNullOrEmpty(maKhachHang) && ks.DoiTuongKhaoSat.Any(d => d.MaKhachHang == maKhachHang));

        // GET: api/KhaoSat/HienTai?maKhachHang= - Lấy bài khảo sát đang được mở (mà khách được tham gia)
        [HttpGet("HienTai")]
        public async Task<IActionResult> GetKhaoSatHienTai([FromQuery] string? maKhachHang)
        {
            var dsMo = await _service.Query<KhaoSat>()
                .Include(k => k.CauHoiKhaoSat)
                    .ThenInclude(c => c.TuyChonCauHoi)
                .Include(k => k.DoiTuongKhaoSat)
                .Where(k => k.TrangThai == "Open")
                .OrderByDescending(k => k.NgayTao)
                .ToListAsync();
            var khaoSat = dsMo.FirstOrDefault(k => DuocThamGia(k, maKhachHang));

            if (khaoSat == null)
            {
                return NotFound("Không có khảo sát nào đang hoạt động.");
            }

            var result = new
            {
                khaoSat.MaKhaoSat,
                khaoSat.TieuDe,
                CauHoi = AdminKhaoSatService.SapXep(khaoSat.CauHoiKhaoSat, c => c.MaCauHoi).Select(c => new
                {
                    c.MaCauHoi,
                    c.NoiDungCauHoi,
                    c.LoaiCauHoi,
                    TuyChon = AdminKhaoSatService.SapXep(c.TuyChonCauHoi, t => t.MaTuyChon).Select(t => new
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
            if (request == null || string.IsNullOrWhiteSpace(request.MaKhaoSat) || string.IsNullOrWhiteSpace(request.MaKhachHang))
            {
                return BadRequest("Thông tin khảo sát hoặc khách hàng không hợp lệ.");
            }

            var khaoSat = await _service.Query<KhaoSat>()
                .Include(k => k.DoiTuongKhaoSat)
                .FirstOrDefaultAsync(k => k.MaKhaoSat == request.MaKhaoSat && k.TrangThai == "Open");
            if (khaoSat == null)
            {
                return BadRequest("Khảo sát không tồn tại hoặc đã đóng.");
            }
            if (!DuocThamGia(khaoSat, request.MaKhachHang))
            {
                return BadRequest("Bạn không thuộc đối tượng của khảo sát này.");
            }

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

            // Đánh dấu hoàn thành đối tượng khảo sát nếu khách hàng có trong danh sách
            var doiTuong = khaoSat.DoiTuongKhaoSat.FirstOrDefault(d => d.MaKhachHang == request.MaKhachHang);
            if (doiTuong != null)
            {
                doiTuong.DaHoanThanh = true;
            }

            await _service.SaveChangesAsync();

            return Ok(new { message = "Gửi phản hồi thành công" });
        }
        // GET: api/KhaoSat/DanhSach?maKhachHang=KH01
        [HttpGet("DanhSach")]
        public async Task<IActionResult> GetDanhSach([FromQuery] string? maKhachHang)
        {
            var dsKhaoSat = (await _service.Query<KhaoSat>()
                .Include(k => k.CauHoiKhaoSat)
                .Include(k => k.DoiTuongKhaoSat)
                .Where(k => k.TrangThai == "Open")
                .OrderByDescending(k => k.NgayTao)
                .ToListAsync())
                .Where(k => DuocThamGia(k, maKhachHang))
                .ToList();

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
        // GET: api/KhaoSat/{id}?maKhachHang=
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, [FromQuery] string? maKhachHang)
        {
            var khaoSat = await _service.Query<KhaoSat>()
                .Include(k => k.CauHoiKhaoSat).ThenInclude(c => c.TuyChonCauHoi)
                .Include(k => k.DoiTuongKhaoSat)
                .FirstOrDefaultAsync(k => k.MaKhaoSat == id && k.TrangThai == "Open");

            if (khaoSat == null || !DuocThamGia(khaoSat, maKhachHang))
                return NotFound("Khảo sát không tồn tại, đã đóng hoặc bạn không thuộc đối tượng khảo sát.");

            return Ok(new
            {
                khaoSat.MaKhaoSat,
                khaoSat.TieuDe,
                CauHoi = AdminKhaoSatService.SapXep(khaoSat.CauHoiKhaoSat, c => c.MaCauHoi).Select(c => new
                {
                    c.MaCauHoi,
                    c.NoiDungCauHoi,
                    c.LoaiCauHoi,
                    TuyChon = AdminKhaoSatService.SapXep(c.TuyChonCauHoi, t => t.MaTuyChon).Select(t => new { t.MaTuyChon, t.NoiDungTuyChon })
                })
            });
        }
    }
}
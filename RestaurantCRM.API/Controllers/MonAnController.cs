using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MonAnController : ControllerBase
    {
        private readonly RestaurantCRM.BusinessLogic.Services.IRestaurantService _service;

        public MonAnController(RestaurantCRM.BusinessLogic.Services.IRestaurantService service)
        {
            _service = service;
        }

        // GET: api/MonAn - Lấy danh sách món ăn (hỗ trợ tìm kiếm theo tên và lọc theo loại món)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetMonAns([FromQuery] string? search = null, [FromQuery] string? maLoai = null, [FromQuery] string? sort = null)
        {
            var query = _service.Query<MonAn>()
                .Include(m => m.KmTheoSp)
                    .ThenInclude(k => k.MaChuongTrinhNavigation)
                .Include(m => m.DinhLuong)
                    .ThenInclude(dl => dl.MaNguyenLieuNavigation)
                .Where(m => m.TrangThai == "Active" || m.TrangThai == "InStock" || (m.TrangThai != "Discontinued" && m.TrangThai != "Inactive" && m.TrangThai != "OutOfStock"))
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(maLoai) && maLoai != "Tất cả")
            {
                query = query.Where(m => m.MaLoaiMon == maLoai);
            }

            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);
            var dbList = await query.ToListAsync();
            
            var result = dbList.Select(m => {
                var activeKm = m.KmTheoSp.FirstOrDefault(k => k.MaChuongTrinhNavigation != null && (k.MaChuongTrinhNavigation.NgayBatDau == null || k.MaChuongTrinhNavigation.NgayBatDau <= today) && (k.MaChuongTrinhNavigation.NgayKetThuc == null || k.MaChuongTrinhNavigation.NgayKetThuc >= today));
                double donGia = m.DonGia ?? 0;
                double giaSauGiam = donGia;
                if (activeKm != null) {
                    giaSauGiam = donGia - (activeKm.TienGiam ?? 0) - (donGia * (activeKm.PhanTramGiam ?? 0) / 100.0);
                }

                int soSuatKhaDung = 0;
                if (m.DinhLuong != null && m.DinhLuong.Any())
                {
                    soSuatKhaDung = (int)m.DinhLuong.Min(dl =>
                    {
                        var ton = dl.MaNguyenLieuNavigation?.SoLuongTon ?? 0;
                        if (dl.SoLuongCan <= 0) return 0;
                        return Math.Floor(ton / dl.SoLuongCan);
                    });
                    if (soSuatKhaDung < 0) soSuatKhaDung = 0;
                }

                return new {
                    m.MaMon, m.MaLoaiMon, m.TenMon, m.MoTa, m.DuongDanAnh, m.DonGia,
                    SoLuong = soSuatKhaDung,
                    m.TrangThai,
                    DonGiaSauGiam = Math.Max(0, giaSauGiam)
                };
            }).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = RemoveDiacritics(search);
                result = result.Where(m => 
                    RemoveDiacritics(m.TenMon ?? "").Contains(s)
                ).ToList();
            }

            if (sort == "asc")
                result = result.OrderBy(m => m.DonGiaSauGiam).ToList();
            else if (sort == "desc")
                result = result.OrderByDescending(m => m.DonGiaSauGiam).ToList();

            return Ok(result);
        }

        // GET: api/MonAn/TheoLoai/{maLoai} - Lấy danh sách món ăn theo loại
        [HttpGet("TheoLoai/{maLoai}")]
        public async Task<ActionResult<IEnumerable<MonAn>>> GetMonAnsByLoai(string maLoai)
        {
            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);
            var list = await _service.Query<MonAn>()
                .Include(m => m.KmTheoSp)
                    .ThenInclude(k => k.MaChuongTrinhNavigation)
                .Include(m => m.DinhLuong)
                    .ThenInclude(dl => dl.MaNguyenLieuNavigation)
                .Where(m => m.MaLoaiMon == maLoai && (m.TrangThai == "Active" || m.TrangThai == "InStock" || (m.TrangThai != "Discontinued" && m.TrangThai != "Inactive" && m.TrangThai != "OutOfStock")))
                .AsNoTracking()
                .ToListAsync();

            return Ok(list.Select(m => {
                var activeKm = m.KmTheoSp.FirstOrDefault(k => k.MaChuongTrinhNavigation != null && (k.MaChuongTrinhNavigation.NgayBatDau == null || k.MaChuongTrinhNavigation.NgayBatDau <= today) && (k.MaChuongTrinhNavigation.NgayKetThuc == null || k.MaChuongTrinhNavigation.NgayKetThuc >= today));
                double donGia = m.DonGia ?? 0;
                double giaSauGiam = donGia;
                if (activeKm != null) {
                    giaSauGiam = donGia - (activeKm.TienGiam ?? 0) - (donGia * (activeKm.PhanTramGiam ?? 0) / 100.0);
                }

                int soSuatKhaDung = 0;
                if (m.DinhLuong != null && m.DinhLuong.Any())
                {
                    soSuatKhaDung = (int)m.DinhLuong.Min(dl =>
                    {
                        var ton = dl.MaNguyenLieuNavigation?.SoLuongTon ?? 0;
                        if (dl.SoLuongCan <= 0) return 0;
                        return Math.Floor(ton / dl.SoLuongCan);
                    });
                    if (soSuatKhaDung < 0) soSuatKhaDung = 0;
                }

                return new {
                    m.MaMon, m.MaLoaiMon, m.TenMon, m.MoTa, m.DuongDanAnh, m.DonGia,
                    SoLuong = soSuatKhaDung,
                    m.TrangThai,
                    DonGiaSauGiam = Math.Max(0, giaSauGiam)
                };
            }));
        }

        // GET: api/MonAn/{id} - Lấy thông tin chi tiết một món ăn.
        [HttpGet("{id}")]
        public async Task<ActionResult<MonAn>> GetMonAn(string id)
        {
            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);
            var monAn = await _service.Query<MonAn>()
                .Include(m => m.KmTheoSp)
                    .ThenInclude(k => k.MaChuongTrinhNavigation)
                .Include(m => m.DinhLuong)
                    .ThenInclude(dl => dl.MaNguyenLieuNavigation)
                .FirstOrDefaultAsync(m => m.MaMon == id);

            if (monAn == null)
            {
                return NotFound();
            }

            var activeKm = monAn.KmTheoSp.FirstOrDefault(k => k.MaChuongTrinhNavigation != null && (k.MaChuongTrinhNavigation.NgayBatDau == null || k.MaChuongTrinhNavigation.NgayBatDau <= today) && (k.MaChuongTrinhNavigation.NgayKetThuc == null || k.MaChuongTrinhNavigation.NgayKetThuc >= today));
            double donGia = monAn.DonGia ?? 0;
            double giaSauGiam = donGia;
            if (activeKm != null) {
                giaSauGiam = donGia - (activeKm.TienGiam ?? 0) - (donGia * (activeKm.PhanTramGiam ?? 0) / 100.0);
            }

            int soSuatKhaDung = 0;
            if (monAn.DinhLuong != null && monAn.DinhLuong.Any())
            {
                soSuatKhaDung = (int)monAn.DinhLuong.Min(dl =>
                {
                    var ton = dl.MaNguyenLieuNavigation?.SoLuongTon ?? 0;
                    if (dl.SoLuongCan <= 0) return 0;
                    return Math.Floor(ton / dl.SoLuongCan);
                });
                if (soSuatKhaDung < 0) soSuatKhaDung = 0;
            }

            return Ok(new {
                monAn.MaMon, monAn.MaLoaiMon, monAn.TenMon, monAn.MoTa, monAn.DuongDanAnh, monAn.DonGia,
                SoLuong = soSuatKhaDung,
                monAn.TrangThai,
                DonGiaSauGiam = Math.Max(0, giaSauGiam)
            });
        }



        private bool MonAnExists(string id)
        {
            return _service.Query<MonAn>().Any(e => e.MaMon == id);
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            text = text.ToLower();
            string[] a = new string[] { "à", "á", "ạ", "ả", "ã", "â", "ầ", "ấ", "ậ", "ẩ", "ẫ", "ă", "ằ", "ắ", "ặ", "ẳ", "ẵ" };
            string[] e = new string[] { "è", "é", "ẹ", "ẻ", "ẽ", "ê", "ề", "ế", "ệ", "ể", "ễ" };
            string[] i = new string[] { "ì", "í", "ị", "ỉ", "ĩ" };
            string[] o = new string[] { "ò", "ó", "ọ", "ỏ", "õ", "ô", "ồ", "ố", "ộ", "ổ", "ỗ", "ơ", "ờ", "ớ", "ợ", "ở", "ỡ" };
            string[] u = new string[] { "ù", "ú", "ụ", "ủ", "ũ", "ư", "ừ", "ứ", "ự", "ử", "ữ" };
            string[] y = new string[] { "ỳ", "ý", "ỵ", "ỷ", "ỹ" };
            string[] d = new string[] { "đ" };

            foreach (var ch in a) text = text.Replace(ch, "a");
            foreach (var ch in e) text = text.Replace(ch, "e");
            foreach (var ch in i) text = text.Replace(ch, "i");
            foreach (var ch in o) text = text.Replace(ch, "o");
            foreach (var ch in u) text = text.Replace(ch, "u");
            foreach (var ch in y) text = text.Replace(ch, "y");
            foreach (var ch in d) text = text.Replace(ch, "d");
            
            return text;
        }
    }
}

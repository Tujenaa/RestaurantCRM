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
                .Include(m => m.KmTheoSp)
                    .ThenInclude(k => k.ChiTietHoaDon)
                        .ThenInclude(c => c.MaHoaDonNavigation)
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
            var activeLoaiPromos = await GetActiveLoaiPromosAsync(today);

            var result = dbList.Select(m => {
                double giaSauGiam = CalculateDonGiaSauGiam(m, activeLoaiPromos, today);

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
                    ConHang = (m.TrangThai == "Active" || m.TrangThai == "InStock") && soSuatKhaDung > 0,
                    m.TrangThai,
                    DonGiaSauGiam = Math.Max(0, giaSauGiam)
                };
            }).Where(m => m.SoLuong > 0).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = RemoveDiacritics(search);
                result = result.Where(m =>
                    RemoveDiacritics(m.TenMon ?? "").Contains(s)
                ).ToList();
            }

            // Sắp xếp món còn hàng lên đầu, món hết hàng đẩy xuống dưới cùng
            if (sort == "asc")
                result = result.OrderByDescending(m => m.ConHang).ThenBy(m => m.DonGiaSauGiam).ToList();
            else if (sort == "desc")
                result = result.OrderByDescending(m => m.ConHang).ThenByDescending(m => m.DonGiaSauGiam).ToList();
            else
                result = result.OrderByDescending(m => m.ConHang).ToList();

            return Ok(result);
        }

        // GET: api/MonAn/TheoLoai/{maLoai} - Lấy danh sách món ăn theo loại
        [HttpGet("TheoLoai/{maLoai}")]
        public async Task<ActionResult<IEnumerable<object>>> GetMonAnsByLoai(string maLoai)
        {
            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);
            var list = await _service.Query<MonAn>()
                .Include(m => m.KmTheoSp)
                    .ThenInclude(k => k.MaChuongTrinhNavigation)
                .Include(m => m.KmTheoSp)
                    .ThenInclude(k => k.ChiTietHoaDon)
                        .ThenInclude(c => c.MaHoaDonNavigation)
                .Include(m => m.DinhLuong)
                    .ThenInclude(dl => dl.MaNguyenLieuNavigation)
                .Where(m => m.MaLoaiMon == maLoai && (m.TrangThai == "Active" || m.TrangThai == "InStock" || (m.TrangThai != "Discontinued" && m.TrangThai != "Inactive" && m.TrangThai != "OutOfStock")))
                .AsNoTracking()
                .ToListAsync();
            var activeLoaiPromos = await GetActiveLoaiPromosAsync(today);

            var result = list.Select(m => {
                double giaSauGiam = CalculateDonGiaSauGiam(m, activeLoaiPromos, today);

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
                    ConHang = (m.TrangThai == "Active" || m.TrangThai == "InStock") && soSuatKhaDung > 0,
                    m.TrangThai,
                    DonGiaSauGiam = Math.Max(0, giaSauGiam)
                };
            }).Where(m => m.SoLuong > 0).ToList();

            return Ok(result);
        }

        // GET: api/MonAn/{id} - Lấy thông tin chi tiết một món ăn.
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetMonAn(string id)
        {
            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);
            var monAn = await _service.Query<MonAn>()
                .Include(m => m.KmTheoSp)
                    .ThenInclude(k => k.MaChuongTrinhNavigation)
                .Include(m => m.KmTheoSp)
                    .ThenInclude(k => k.ChiTietHoaDon)
                        .ThenInclude(c => c.MaHoaDonNavigation)
                .Include(m => m.DinhLuong)
                    .ThenInclude(dl => dl.MaNguyenLieuNavigation)
                .FirstOrDefaultAsync(m => m.MaMon == id);

            if (monAn == null || monAn.TrangThai == "Discontinued" || monAn.TrangThai == "Inactive" || monAn.TrangThai == "OutOfStock")
            {
                return NotFound();
            }

            var activeLoaiPromos = await GetActiveLoaiPromosAsync(today);
            double giaSauGiam = CalculateDonGiaSauGiam(monAn, activeLoaiPromos, today);

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

            if (soSuatKhaDung <= 0)
            {
                return NotFound();
            }

            return Ok(new {
                monAn.MaMon, monAn.MaLoaiMon, monAn.TenMon, monAn.MoTa, monAn.DuongDanAnh, monAn.DonGia,
                SoLuong = soSuatKhaDung,
                ConHang = (monAn.TrangThai == "Active" || monAn.TrangThai == "InStock") && soSuatKhaDung > 0,
                monAn.TrangThai,
                DonGiaSauGiam = Math.Max(0, giaSauGiam)
            });
        }

        private async Task<List<KmTheoLoai>> GetActiveLoaiPromosAsync(DateOnly today)
        {
            return await _service.Query<KmTheoLoai>()
                .Include(k => k.MaChuongTrinhNavigation)
                .Include(k => k.ChiTietHoaDon)
                    .ThenInclude(c => c.MaHoaDonNavigation)
                .Where(k => k.MaChuongTrinhNavigation != null
                         && (k.MaVoucher == null || !k.MaVoucher.StartsWith("[DA_HUY]_"))
                         && ((k.PhanTramGiam.HasValue && k.PhanTramGiam > 0) || (k.TienGiam.HasValue && k.TienGiam > 0))
                         && (!k.SoLuongToiThieu.HasValue || k.SoLuongToiThieu <= 0 || k.ChiTietHoaDon.Where(c => c.MaHoaDonNavigation != null && c.MaHoaDonNavigation.TrangThai != "Cancelled").Select(c => c.MaHoaDon).Distinct().Count() < k.SoLuongToiThieu.Value)
                         && (k.MaChuongTrinhNavigation.NgayBatDau == null || k.MaChuongTrinhNavigation.NgayBatDau <= today)
                         && (k.MaChuongTrinhNavigation.NgayKetThuc == null || k.MaChuongTrinhNavigation.NgayKetThuc >= today))
                .AsNoTracking()
                .ToListAsync();
        }

        private static double CalculateDonGiaSauGiam(MonAn m, List<KmTheoLoai> activeLoaiPromos, DateOnly today)
        {
            double donGia = m.DonGia ?? 0;
            if (donGia <= 0) return 0;

            double maxDiscount = 0;

            if (m.KmTheoSp != null)
            {
                var validSpPromos = m.KmTheoSp.Where(k =>
                    k.MaChuongTrinhNavigation != null &&
                    (k.MaVoucher == null || !k.MaVoucher.StartsWith("[DA_HUY]_")) &&
                    ((k.PhanTramGiam.HasValue && k.PhanTramGiam > 0) || (k.TienGiam.HasValue && k.TienGiam > 0)) &&
                    (!k.SoLuongToiThieu.HasValue || k.SoLuongToiThieu <= 0 || k.ChiTietHoaDon.Where(c => c.MaHoaDonNavigation != null && c.MaHoaDonNavigation.TrangThai != "Cancelled").Select(c => c.MaHoaDon).Distinct().Count() < k.SoLuongToiThieu.Value) &&
                    (k.MaChuongTrinhNavigation.NgayBatDau == null || k.MaChuongTrinhNavigation.NgayBatDau <= today) &&
                    (k.MaChuongTrinhNavigation.NgayKetThuc == null || k.MaChuongTrinhNavigation.NgayKetThuc >= today));

                foreach (var k in validSpPromos)
                {
                    double disc = 0;
                    if (k.PhanTramGiam.HasValue && k.PhanTramGiam.Value > 0)
                        disc = donGia * k.PhanTramGiam.Value / 100.0;
                    else if (k.TienGiam.HasValue && k.TienGiam.Value > 0)
                        disc = k.TienGiam.Value;

                    if (k.GiamToiDa.HasValue && k.GiamToiDa.Value > 0 && disc > k.GiamToiDa.Value)
                        disc = k.GiamToiDa.Value;

                    if (disc > maxDiscount) maxDiscount = disc;
                }
            }

            if (!string.IsNullOrEmpty(m.MaLoaiMon) && activeLoaiPromos != null)
            {
                foreach (var p in activeLoaiPromos.Where(x => x.MaLoaiMon == m.MaLoaiMon))
                {
                    if (p.GiaTriDonToiThieu.HasValue && donGia < p.GiaTriDonToiThieu.Value)
                        continue;

                    double disc = 0;
                    if (p.PhanTramGiam.HasValue && p.PhanTramGiam.Value > 0)
                        disc = donGia * p.PhanTramGiam.Value / 100.0;
                    else if (p.TienGiam.HasValue && p.TienGiam.Value > 0)
                        disc = p.TienGiam.Value;

                    if (p.GiamToiDa.HasValue && p.GiamToiDa.Value > 0 && disc > p.GiamToiDa.Value)
                        disc = p.GiamToiDa.Value;

                    if (disc > maxDiscount) maxDiscount = disc;
                }
            }

            double giaVon = m.DinhLuong != null
                ? Math.Round(m.DinhLuong.Sum(dl => (double)dl.SoLuongCan * (dl.MaNguyenLieuNavigation?.GiaNhapGanNhat ?? 0)), 0)
                : 0;

            double giaSauGiam = Math.Max(0, donGia - maxDiscount);
            if (giaVon > 0 && giaSauGiam < giaVon)
            {
                maxDiscount = Math.Max(0, donGia - giaVon);
                giaSauGiam = donGia - maxDiscount;
            }

            return giaSauGiam;
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

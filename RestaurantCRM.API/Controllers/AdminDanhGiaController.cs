using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    /// <summary>
    /// Quản lý phản hồi / góp ý của khách hàng.
    /// CSDL đã bỏ bảng PHAN_HOI: phản hồi của khách chính là DANH_GIA, nhân viên trả lời lưu ở TRA_LOI_DANH_GIA.
    /// Trạng thái xử lý: "Chưa xử lý" / "Đã xử lý" theo cột DANH_GIA.daXuLy.
    /// Gửi trả lời tự chuyển sang Đã xử lý; admin có thể đổi trạng thái bất cứ lúc nào (kể cả chưa trả lời).
    /// Ẩn / hiện trên website là thông tin riêng (DANH_GIA.trangThai). Mỗi phản hồi chỉ trả lời 1 lần.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AdminDanhGiaController : ControllerBase
    {
        private readonly RestaurantCRM.BusinessLogic.Services.IRestaurantService _service;

        public AdminDanhGiaController(RestaurantCRM.BusinessLogic.Services.IRestaurantService service)
        {
            _service = service;
        }

        // GET: api/AdminDanhGia/PhanTichMon - Tổng hợp đánh giá theo món ăn.
        [HttpGet("PhanTichMon")]
        public async Task<IActionResult> GetPhanTichMon()
        {
            var ctDanhGias = await _service.Query<ChiTietDanhGia>()
                .Select(ct => new
                {
                    ct.MaMon,
                    TenMon = ct.MaMonNavigation != null ? ct.MaMonNavigation.TenMon : ct.MaMon,
                    SoSao = (int?)ct.SoSao,
                    ct.NoiDung,
                    NgayDanhGia = ct.MaDanhGiaNavigation.NgayDanhGia
                })
                .ToListAsync();

            var legacyDanhGias = await _service.Query<DanhGia>()
                .Where(d => d.MaMon != null)
                .Select(d => new
                {
                    d.MaMon,
                    TenMon = d.MaMonNavigation != null ? d.MaMonNavigation.TenMon : (d.MaMon != null ? d.MaMon : "Đánh giá chung đơn hàng"),
                    d.SoSao,
                    d.NoiDung,
                    d.NgayDanhGia
                })
                .ToListAsync();

            var danhGias = ctDanhGias.Concat(legacyDanhGias).ToList();

            var ketQua = danhGias
                .GroupBy(d => d.MaMon)
                .Select(g =>
                {
                    var diem = g.Where(d => d.SoSao.HasValue && d.SoSao >= 1 && d.SoSao <= 5)
                        .Select(d => d.SoSao!.Value)
                        .ToList();
                    return new
                    {
                        MaMon = g.Key,
                        TenMon = g.Select(d => d.TenMon).FirstOrDefault(t => !string.IsNullOrWhiteSpace(t)) ?? g.Key,
                        SoLuotDanhGia = g.Count(),
                        DiemTrungBinh = diem.Count == 0 ? (double?)null : Math.Round(diem.Average(), 2),
                        SoSao1 = diem.Count(s => s == 1),
                        SoSao2 = diem.Count(s => s == 2),
                        SoSao3 = diem.Count(s => s == 3),
                        SoSao4 = diem.Count(s => s == 4),
                        SoSao5 = diem.Count(s => s == 5),
                        NhanXet = g.Where(d => !string.IsNullOrWhiteSpace(d.NoiDung))
                            .OrderByDescending(d => d.NgayDanhGia)
                            .Select(d => new
                            {
                                d.SoSao,
                                d.NoiDung,
                                d.NgayDanhGia
                            })
                            .ToList()
                    };
                })
                .OrderByDescending(mon => mon.SoLuotDanhGia)
                .ThenBy(mon => mon.TenMon)
                .ToList();

            return Ok(ketQua);
        }

        // GET: api/AdminDanhGia?search=&trangThai=&soSao=
        //   trangThai: (bỏ trống) = chỉ phản hồi đang hiển thị (giữ như cũ cho Dashboard)
        //              TatCa | ChuaXuLy | DaXuLy | DaAn  (ChuaTraLoi / DaTraLoi giữ để tương thích)
        //   search: tìm theo mã, tên khách hàng, tên món, nội dung
        [HttpGet]
        public async Task<IActionResult> GetAllDanhGia([FromQuery] string? search, [FromQuery] string? trangThai, [FromQuery] int? soSao)
        {
            var query = _service.Query<DanhGia>().AsQueryable();

            // Chỉ lấy các phản hồi có nội dung ý kiến đóng góp hoặc đánh giá cấp đơn hàng (MaMon == null)
            // Bỏ qua các dòng món ăn chỉ chấm sao không có nội dung chữ để danh sách Admin không bị rác/trùng lặp
            query = query.Where(d => !string.IsNullOrEmpty(d.NoiDung) || d.MaMon == null);

            switch ((trangThai ?? "").Trim())
            {
                case "TatCa": break;
                case "ChuaTraLoi": query = query.Where(d => d.TrangThai && !d.TraLoiDanhGia.Any()); break;
                case "DaTraLoi": query = query.Where(d => d.TrangThai && d.TraLoiDanhGia.Any()); break;
                case "ChuaXuLy": query = query.Where(d => !d.DaXuLy); break;
                case "DaXuLy": query = query.Where(d => d.DaXuLy); break;
                case "DaAn": query = query.Where(d => !d.TrangThai); break;
                case "": query = query.Where(d => d.TrangThai); break;
                default: return BadRequest("Trạng thái lọc không hợp lệ. Chấp nhận: TatCa, ChuaXuLy, DaXuLy, DaAn.");
            }

            if (soSao.HasValue)
            {
                if (soSao < 1 || soSao > 5) return BadRequest("Số sao phải từ 1 đến 5.");
                query = query.Where(d => d.SoSao == soSao);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(d =>
                    d.MaDanhGia.Contains(s) ||
                    (d.NoiDung != null && d.NoiDung.Contains(s)) ||
                    (d.MaKhachHangNavigation != null && d.MaKhachHangNavigation.HoTen != null && d.MaKhachHangNavigation.HoTen.Contains(s)) ||
                    (d.MaMonNavigation != null && d.MaMonNavigation.TenMon != null && d.MaMonNavigation.TenMon.Contains(s)));
            }

            var danhGias = await query
                .OrderByDescending(d => d.NgayDanhGia)
                .Select(d => new
                {
                    d.MaDanhGia,
                    d.MaKhachHang,
                    d.MaHoaDon,
                    d.MaMon,
                    TenMon = d.MaMonNavigation != null ? d.MaMonNavigation.TenMon : null,
                    d.SoSao,
                    d.NoiDung,
                    d.NgayDanhGia,
                    HienThi = d.TrangThai,
                    TenKhachHang = d.MaKhachHangNavigation != null ? d.MaKhachHangNavigation.HoTen : "Khách ẩn danh",
                    TongSoTraLoi = d.TraLoiDanhGia.Count,
                    d.DaXuLy,
                    TrangThaiXuLy = d.DaXuLy ? "Đã xử lý" : "Chưa xử lý",
                    HoiThoai = d.TraLoiDanhGia.OrderBy(t => t.NgayGui).Select(t => new
                    {
                        t.MaTraLoi,
                        t.NoiDung,
                        t.NgayGui,
                        t.MaNhanVien,
                        NguoiTraLoi = t.MaNhanVienNavigation.HoTen
                    }).ToList()
                })
                .ToListAsync();

            return Ok(danhGias);
        }

        // GET: api/AdminDanhGia/{id} - Chi tiết một phản hồi kèm hội thoại
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDanhGia(string id)
        {
            var d = await _service.Query<DanhGia>()
                .Where(x => x.MaDanhGia == id)
                .Select(x => new
                {
                    x.MaDanhGia,
                    x.MaKhachHang,
                    x.MaHoaDon,
                    x.MaMon,
                    TenMon = x.MaMonNavigation != null ? x.MaMonNavigation.TenMon : (x.MaMon != null ? x.MaMon : "Đánh giá chung đơn hàng"),
                    x.SoSao,
                    x.NoiDung,
                    x.NgayDanhGia,
                    HienThi = x.TrangThai,
                    TenKhachHang = x.MaKhachHangNavigation != null ? x.MaKhachHangNavigation.HoTen : "Khách ẩn danh",
                    SoDienThoai = x.MaKhachHangNavigation != null ? x.MaKhachHangNavigation.SoDienThoai : null,
                    TongSoTraLoi = x.TraLoiDanhGia.Count,
                    x.DaXuLy,
                    TrangThaiXuLy = x.DaXuLy ? "Đã xử lý" : "Chưa xử lý",
                    HoiThoai = x.TraLoiDanhGia.OrderBy(t => t.NgayGui).Select(t => new
                    {
                        t.MaTraLoi,
                        t.NoiDung,
                        t.NgayGui,
                        t.MaNhanVien,
                        NguoiTraLoi = t.MaNhanVienNavigation.HoTen
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (d == null) return NotFound("Không tìm thấy phản hồi.");
            return Ok(d);
        }

        // POST: api/AdminDanhGia/{id}/Reply - Nhân viên trả lời phản hồi
        [HttpPost("{id}/Reply")]
        public async Task<IActionResult> CreateReply(string id, ReplyRequest request)
        {
            var danhGia = await _service.FindAsync<DanhGia>(id);
            if (danhGia == null)
            {
                return NotFound("Không tìm thấy phản hồi.");
            }

            if (request == null || string.IsNullOrWhiteSpace(request.NoiDung))
                return BadRequest("Nội dung trả lời không được để trống.");
            if (await _service.Query<TraLoiDanhGia>().AnyAsync(t => t.MaDanhGia == id))
                return BadRequest("Phản hồi này đã được trả lời. Mỗi phản hồi chỉ trả lời 1 lần.");
            if (request.NoiDung.Trim().Length > 2000)
                return BadRequest("Nội dung trả lời tối đa 2000 ký tự.");
            if (string.IsNullOrWhiteSpace(request.NguoiGuiId) ||
                !await _service.Query<NhanVien>().AnyAsync(n => n.MaNhanVien == request.NguoiGuiId))
                return BadRequest("Không xác định được nhân viên trả lời. Vui lòng đăng nhập lại.");

            var traLoi = new TraLoiDanhGia
            {
                MaTraLoi = "TL" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                MaDanhGia = id,
                MaNhanVien = request.NguoiGuiId,
                NoiDung = request.NoiDung.Trim(),
                NgayGui = DateTime.Now
            };

            _service.Add(traLoi);
            danhGia.DaXuLy = true;
            await _service.SaveChangesAsync();

            return Ok(new { message = "Gửi trả lời thành công.", maTraLoi = traLoi.MaTraLoi });
        }

        // PUT: api/AdminDanhGia/{id}/XuLy - Admin đổi trạng thái xử lý (Chưa xử lý <-> Đã xử lý), không cần trả lời
        [HttpPut("{id}/XuLy")]
        public async Task<IActionResult> CapNhatXuLy(string id, CapNhatXuLyRequest request)
        {
            var danhGia = await _service.FindAsync<DanhGia>(id);
            if (danhGia == null) return NotFound("Không tìm thấy phản hồi.");
            if (request == null) return BadRequest("Thiếu dữ liệu trạng thái.");

            danhGia.DaXuLy = request.DaXuLy;
            await _service.SaveChangesAsync();
            return Ok(new { message = request.DaXuLy ? "Đã chuyển phản hồi sang Đã xử lý." : "Đã chuyển phản hồi về Chưa xử lý.", daXuLy = request.DaXuLy });
        }

        public class CapNhatXuLyRequest
        {
            public bool DaXuLy { get; set; }
        }

        // PUT: api/AdminDanhGia/{id}/DaXem - Đánh dấu đã xem => chuyển sang "Đã xử lý" mà không cần trả lời
        [HttpPut("{id}/DaXem")]
        public async Task<IActionResult> DanhDauDaXem(string id)
        {
            var danhGia = await _service.FindAsync<DanhGia>(id);
            if (danhGia == null) return NotFound("Không tìm thấy phản hồi.");

            danhGia.DaXuLy = true;
            await _service.SaveChangesAsync();
            return Ok(new { message = "Đã đánh dấu phản hồi là đã xử lý." });
        }

        // DELETE: api/AdminDanhGia/{id}/Reply/{maTraLoi} - Xóa một câu trả lời gửi nhầm
        [HttpDelete("{id}/Reply/{maTraLoi}")]
        public async Task<IActionResult> DeleteReply(string id, string maTraLoi)
        {
            var traLoi = await _service.Query<TraLoiDanhGia>()
                .FirstOrDefaultAsync(t => t.MaTraLoi == maTraLoi && t.MaDanhGia == id);
            if (traLoi == null) return NotFound("Không tìm thấy câu trả lời.");

            _service.Remove(traLoi);
            await _service.SaveChangesAsync();
            return Ok(new { message = "Đã xóa câu trả lời." });
        }

        // PUT: api/AdminDanhGia/{id}/TrangThai - Ẩn / hiện phản hồi trên website
        [HttpPut("{id}/TrangThai")]
        public async Task<IActionResult> UpdateTrangThai(string id, PhanHoiTrangThaiRequest request)
        {
            var danhGia = await _service.FindAsync<DanhGia>(id);
            if (danhGia == null) return NotFound("Không tìm thấy phản hồi.");
            if (request == null) return BadRequest("Thiếu dữ liệu trạng thái.");

            danhGia.TrangThai = request.HienThi;
            await _service.SaveChangesAsync();

            return Ok(new { message = request.HienThi ? "Đã hiển thị lại phản hồi." : "Đã ẩn phản hồi.", hienThi = request.HienThi });
        }

        // DELETE: api/AdminDanhGia/{id} - Xóa phản hồi (và các câu trả lời liên quan)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDanhGia(string id)
        {
            var danhGia = await _service.Query<DanhGia>()
                .Include(d => d.TraLoiDanhGia)
                .FirstOrDefaultAsync(d => d.MaDanhGia == id);

            if (danhGia == null)
            {
                return NotFound("Không tìm thấy phản hồi.");
            }

            if (danhGia.TraLoiDanhGia.Any())
            {
                _service.RemoveRange(danhGia.TraLoiDanhGia);
            }

            _service.Remove(danhGia);
            await _service.SaveChangesAsync();

            return NoContent();
        }
    }
}

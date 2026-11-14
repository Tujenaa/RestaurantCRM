using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.API.DTOs;
using RestaurantCRM.API.Helpers;

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
        // Tính trên mọi CHI_TIET_DANH_GIA, kể cả đánh giá đã ẩn (ẩn chỉ để không hiện trên website).
        // Điểm món chỉ lấy các lượt khách chấm riêng (SoSao != NULL); sao chung của đơn chỉ để tham khảo (DiemTheoDon).
        // NhanXet: các lượt chấm riêng (có sao) hoặc có nhận xét của món, kèm mã đánh giá, sao món và sao đơn.
        [HttpGet("PhanTichMon")]
        public async Task<IActionResult> GetPhanTichMon()
        {
            const int NguongDuDuLieu = 3;

            var ctDanhGias = await _service.Query<ChiTietDanhGia>()
                .Select(ct => new
                {
                    ct.MaDanhGia,
                    ct.MaMon,
                    TenMon = ct.MaMonNavigation != null ? ct.MaMonNavigation.TenMon : ct.MaMon,
                    ct.SoSao,
                    SoSaoDon = ct.MaDanhGiaNavigation.SoSao,
                    ct.NoiDung,
                    NgayDanhGia = ct.MaDanhGiaNavigation.NgayDanhGia,
                    HienThi = ct.MaDanhGiaNavigation.TrangThai
                })
                .ToListAsync();

            var ketQua = ctDanhGias
                .GroupBy(d => d.MaMon)
                .Select(g =>
                {
                    var diem = g.Where(d => d.SoSao is >= 1 and <= 5).Select(d => d.SoSao!.Value).ToList();
                    var diemDon = g.Where(d => d.SoSaoDon is >= 1 and <= 5).Select(d => d.SoSaoDon!.Value).ToList();
                    return new
                    {
                        MaMon = g.Key,
                        TenMon = g.Select(d => d.TenMon).FirstOrDefault(t => !string.IsNullOrWhiteSpace(t)) ?? g.Key,
                        SoLuotTrongDon = g.Count(),
                        SoLuotChamRieng = diem.Count,
                        SoLuotDanhGia = diem.Count, // giữ tên cũ cho App, = SoLuotChamRieng
                        DiemTrungBinh = diem.Count == 0 ? (double?)null : Math.Round(diem.Average(), 2),
                        SoSao1 = diem.Count(s => s == 1),
                        SoSao2 = diem.Count(s => s == 2),
                        SoSao3 = diem.Count(s => s == 3),
                        SoSao4 = diem.Count(s => s == 4),
                        SoSao5 = diem.Count(s => s == 5),
                        DiemTheoDon = diemDon.Count == 0 ? (double?)null : Math.Round(diemDon.Average(), 2),
                        DuDuLieu = diem.Count >= NguongDuDuLieu,
                        NhanXet = g.Where(d => d.SoSao != null || !string.IsNullOrWhiteSpace(d.NoiDung))
                            .OrderByDescending(d => d.NgayDanhGia)
                            .Select(d => new
                            {
                                d.MaDanhGia,
                                d.SoSao,
                                d.SoSaoDon,
                                ChamRieng = d.SoSao != null,
                                d.NoiDung,
                                d.NgayDanhGia,
                                d.HienThi
                            })
                            .ToList()
                    };
                })
                // Món đủ dữ liệu trước, điểm thấp lên đầu; món chưa đủ dữ liệu xếp sau
                .OrderByDescending(mon => mon.DuDuLieu)
                .ThenBy(mon => mon.DuDuLieu ? mon.DiemTrungBinh : null)
                .ThenBy(mon => mon.TenMon)
                .ToList();

            return Ok(ketQua);
        }

        // GET: api/AdminDanhGia?search=&trangThai=&soSao=
        //   trangThai: (bỏ trống) = chỉ phản hồi Hiển thị (giữ như cũ cho Dashboard)
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
                    d.NgayCapNhat,
                    HienThi = d.TrangThai,
                    TenKhachHang = d.MaKhachHangNavigation != null ? d.MaKhachHangNavigation.HoTen : "Khách ẩn danh",
                    TongSoTraLoi = d.TraLoiDanhGia.Count,
                    d.DaXuLy,
                    TrangThaiXuLy = d.DaXuLy ? "Đã xử lý" : "Chưa xử lý",
                    ChiTietMon = d.ChiTietDanhGia.OrderBy(ct => ct.SoSao == null).ThenBy(ct => ct.SoSao).Select(ct => new
                    {
                        ct.MaMon,
                        TenMon = ct.MaMonNavigation != null ? ct.MaMonNavigation.TenMon : ct.MaMon,
                        ct.SoSao,
                        ct.NoiDung
                    }).ToList(),
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
                    x.NgayCapNhat,
                    HienThi = x.TrangThai,
                    TenKhachHang = x.MaKhachHangNavigation != null ? x.MaKhachHangNavigation.HoTen : "Khách ẩn danh",
                    SoDienThoai = x.MaKhachHangNavigation != null ? x.MaKhachHangNavigation.SoDienThoai : null,
                    TongSoTraLoi = x.TraLoiDanhGia.Count,
                    x.DaXuLy,
                    TrangThaiXuLy = x.DaXuLy ? "Đã xử lý" : "Chưa xử lý",
                    ChiTietMon = x.ChiTietDanhGia.OrderBy(ct => ct.SoSao == null).ThenBy(ct => ct.SoSao).Select(ct => new
                    {
                        ct.MaMon,
                        TenMon = ct.MaMonNavigation != null ? ct.MaMonNavigation.TenMon : ct.MaMon,
                        ct.SoSao,
                        ct.NoiDung
                    }).ToList(),
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

            var loi = KiemTraNoiDungTraLoi(request?.NoiDung);
            if (loi != null) return BadRequest(loi);
            if (await _service.Query<TraLoiDanhGia>().AnyAsync(t => t.MaDanhGia == id))
                return BadRequest("Phản hồi này đã được trả lời. Mỗi phản hồi chỉ trả lời 1 lần.");
            loi = await KiemTraNhanVienTraLoiAsync(request!.NguoiGuiId);
            if (loi != null) return BadRequest(loi);

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
            try
            {
                await _service.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (DbErrorHelper.IsUniqueViolation(ex))
            {
                // 2 nhân viên trả lời cùng lúc: lần sau vướng UNIQUE(maDanhGia)
                return BadRequest("Phản hồi này đã được trả lời. Mỗi phản hồi chỉ trả lời 1 lần.");
            }

            return Ok(new { message = "Gửi trả lời thành công.", maTraLoi = traLoi.MaTraLoi });
        }

        // PUT: api/AdminDanhGia/{id}/Reply/{maTraLoi} - Sửa câu trả lời (dùng khi khách đã sửa đánh giá sau phản hồi).
        // ngayGui = lúc sửa, người trả lời = người sửa; đánh giá chuyển sang "Đã xử lý". Không đổi ngayCapNhat của đánh giá.
        [HttpPut("{id}/Reply/{maTraLoi}")]
        public async Task<IActionResult> UpdateReply(string id, string maTraLoi, ReplyRequest request)
        {
            var traLoi = await _service.Query<TraLoiDanhGia>()
                .FirstOrDefaultAsync(t => t.MaTraLoi == maTraLoi && t.MaDanhGia == id);
            if (traLoi == null) return NotFound("Không tìm thấy câu trả lời của phản hồi này.");

            var loi = KiemTraNoiDungTraLoi(request?.NoiDung)
                      ?? await KiemTraNhanVienTraLoiAsync(request!.NguoiGuiId);
            if (loi != null) return BadRequest(loi);

            var danhGia = await _service.FindAsync<DanhGia>(id);
            if (danhGia == null) return NotFound("Không tìm thấy phản hồi.");

            traLoi.NoiDung = request.NoiDung!.Trim();
            traLoi.NgayGui = DateTime.Now;
            traLoi.MaNhanVien = request.NguoiGuiId;
            danhGia.DaXuLy = true;
            await _service.SaveChangesAsync();

            return Ok(new { message = "Đã cập nhật câu trả lời.", maTraLoi });
        }

        private const int DoDaiTraLoiToiDa = 1000;
        private static readonly string[] VaiTroDuocTraLoi = { "VT01", "VT05" };

        /// <summary>Nội dung trả lời bắt buộc, tối đa 1000 ký tự. Trả về lỗi hoặc null.</summary>
        private static string? KiemTraNoiDungTraLoi(string? noiDung)
        {
            if (string.IsNullOrWhiteSpace(noiDung)) return "Nội dung trả lời không được để trống.";
            if (noiDung.Trim().Length > DoDaiTraLoiToiDa) return $"Nội dung trả lời tối đa {DoDaiTraLoiToiDa} ký tự.";
            return null;
        }

        /// <summary>Người trả lời phải là nhân viên đang hoạt động, vai trò Admin (VT01) hoặc CRM (VT05).</summary>
        private async Task<string?> KiemTraNhanVienTraLoiAsync(string? maNhanVien)
        {
            if (string.IsNullOrWhiteSpace(maNhanVien))
                return "Không xác định được nhân viên trả lời. Vui lòng đăng nhập lại.";

            var nv = await _service.Query<NhanVien>()
                .Where(n => n.MaNhanVien == maNhanVien)
                .Select(n => new { n.TrangThai, n.MaVaiTro })
                .FirstOrDefaultAsync();
            if (nv == null) return "Không xác định được nhân viên trả lời. Vui lòng đăng nhập lại.";
            if (!string.Equals(nv.TrangThai, "Active", StringComparison.OrdinalIgnoreCase))
                return "Tài khoản nhân viên không còn hoạt động, không thể trả lời đánh giá.";
            if (nv.MaVaiTro == null || !VaiTroDuocTraLoi.Contains(nv.MaVaiTro))
                return "Chỉ nhân viên quản trị hoặc CRM mới được trả lời đánh giá.";
            return null;
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

            // Không còn câu trả lời → phản hồi quay lại "Chưa xử lý" để nhân viên xử lý lại
            var danhGia = await _service.FindAsync<DanhGia>(id);
            if (danhGia != null) danhGia.DaXuLy = false;

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

        // DELETE: api/AdminDanhGia/{id} - Ẩn phản hồi khỏi website (Bảo lưu dữ liệu lịch sử phản hồi và câu trả lời)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDanhGia(string id)
        {
            var danhGia = await _service.FindAsync<DanhGia>(id);
            if (danhGia == null)
            {
                return NotFound("Không tìm thấy phản hồi.");
            }

            // Bảo toàn dữ liệu: chỉ chuyển sang ẩn, giống PUT {id}/TrangThai (ẩn không đổi trạng thái xử lý)
            danhGia.TrangThai = false;
            _service.Update(danhGia);
            await _service.SaveChangesAsync();

            return Ok(new { message = "Đã ẩn phản hồi khỏi website thành công (dữ liệu được lưu trữ an toàn trong hệ thống)." });
        }
    }
}

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.API.DTOs;
using RestaurantCRM.API.Helpers;
using RestaurantCRM.API.Services;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminKhachHangController : ControllerBase
    {
        private static readonly string[] TrangThaiHopLe = KhachHangValidator.TrangThaiHopLe;

        // Ngưỡng "Hay gọi" (tab Món hay gọi): gọi ít nhất 2 đơn VÀ chiếm ít nhất 30% số đơn hoàn tất của khách
        private const int SoLanHayGoiToiThieu = 2;
        private const double TyLeHayGoiToiThieu = 30;

        private readonly RestaurantCRM.BusinessLogic.Services.IRestaurantService _service;

        public AdminKhachHangController(RestaurantCRM.BusinessLogic.Services.IRestaurantService service)
        {
            _service = service;
        }

        // GET: api/AdminKhachHang?search=&trangThai=
        // Danh sách khách hàng; tìm theo mã, tên, số điện thoại, email; lọc theo trạng thái (Active / Locked).
        [HttpGet]
        public async Task<IActionResult> GetKhachHangs([FromQuery] string? search, [FromQuery] string? trangThai)
        {
            var query = _service.Query<KhachHang>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(k =>
                    (k.MaKhachHang != null && k.MaKhachHang.Contains(s)) ||
                    (k.HoTen != null && k.HoTen.Contains(s)) ||
                    (k.SoDienThoai != null && k.SoDienThoai.Contains(s)) ||
                    (k.Email != null && k.Email.Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                var tt = trangThai.Trim();
                query = query.Where(k => k.TrangThai == tt);
            }

            var khachHangs = await query
                .OrderByDescending(k => k.MaKhachHang)
                .Select(k => new
                {
                    k.MaKhachHang,
                    k.HoTen,
                    k.SoDienThoai,
                    k.Email,
                    k.NgaySinh,
                    k.GioiTinh,
                    k.DiaChi,
                    k.TrangThai,
                    SoDonHang = k.HoaDon.Count(),
                    TongChiTieu = k.HoaDon.Where(d => d.TrangThai == "Completed").Sum(d => d.TongThanhToan) ?? 0
                })
                .ToListAsync();

            return Ok(khachHangs);
        }

        // GET: api/AdminKhachHang/{id} - Chi tiết khách hàng kèm lịch sử tương tác và sở thích món.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetKhachHang(string id)
        {
            var khachHang = await _service.Query<KhachHang>()
                .AsSplitQuery()
                .Where(k => k.MaKhachHang == id)
                .Select(k => new
                {
                    k.MaKhachHang,
                    k.HoTen,
                    k.SoDienThoai,
                    k.Email,
                    k.NgaySinh,
                    k.GioiTinh,
                    k.DiaChi,
                    k.TrangThai,
                    SoDonHang = k.HoaDon.Count(),
                    TongChiTieu = k.HoaDon.Where(d => d.TrangThai == "Completed").Sum(d => d.TongThanhToan) ?? 0,
                    SoDanhGia = k.DanhGia.Count(),
                    // Không tính phiếu hồ sơ sở thích (không phải khảo sát)
                    SoKhaoSatDaTraLoi = k.PhieuTraLoi.Count(p => !(p.MaKhaoSatNavigation != null && p.MaKhaoSatNavigation.LaSoThich)),
                    LichSuDanhGia = k.DanhGia
                        .OrderByDescending(d => d.NgayDanhGia)
                        .Select(d => new
                        {
                            d.MaDanhGia,
                            d.MaMon,
                            TenMon = d.MaMonNavigation != null ? d.MaMonNavigation.TenMon : null,
                            d.SoSao,
                            d.NoiDung,
                            d.NgayDanhGia,
                            HienThi = d.TrangThai
                        })
                        .ToList(),
                    LichSuKhaoSat = k.PhieuTraLoi
                        .Where(p => !(p.MaKhaoSatNavigation != null && p.MaKhaoSatNavigation.LaSoThich))
                        .OrderByDescending(p => p.NgayTraLoi)
                        .Select(p => new
                        {
                            p.MaPhieuTraLoi,
                            p.MaKhaoSat,
                            TenKhaoSat = p.MaKhaoSatNavigation != null ? p.MaKhaoSatNavigation.TieuDe : null,
                            p.NgayTraLoi
                        })
                        .ToList(),
                    LichSuHoaDon = k.HoaDon
                        .OrderByDescending(d => d.NgayDat)
                        .Select(d => new
                        {
                            d.MaHoaDon,
                            d.NgayDat,
                            TongTien = d.TongThanhToan,
                            d.TrangThai
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (khachHang == null)
            {
                return NotFound("Không tìm thấy khách hàng.");
            }

            // "Món hay gọi": suy ra từ các đơn đã hoàn tất (không trộn với sở thích khách tự khai).
            // Gom theo mã món, tên và loại lấy từ MON_AN để món đổi tên không bị tách đôi.
            var chiTietDaMua = await _service.Query<ChiTietHoaDon>()
                .Where(ct => ct.MaHoaDonNavigation != null
                    && ct.MaHoaDonNavigation.MaKhachHang == id
                    && ct.MaHoaDonNavigation.TrangThai == "Completed"
                    && ct.SoLuong > 0
                    && ct.MaMon != null)
                .Select(ct => new
                {
                    ct.MaHoaDon,
                    MaMon = ct.MaMon!,
                    // Chỉ dùng tên lưu trên hóa đơn khi món không còn trong MON_AN
                    TenMon = ct.MaMonNavigation != null ? ct.MaMonNavigation.TenMon : ct.TenMon,
                    TenLoaiMon = ct.MaMonNavigation != null && ct.MaMonNavigation.MaLoaiMonNavigation != null
                        ? ct.MaMonNavigation.MaLoaiMonNavigation.TenLoaiMon : null,
                    SoLuong = ct.SoLuong ?? 0,
                    Ngay = ct.MaHoaDonNavigation!.NgayHoanTat ?? ct.MaHoaDonNavigation.NgayDat
                })
                .ToListAsync();

            // Mẫu số: tổng số đơn hoàn tất của khách
            int soDonHoanTat = await _service.Query<HoaDon>()
                .CountAsync(d => d.MaKhachHang == id && d.TrangThai == "Completed");

            // Các món khách đã ♥ trong hồ sơ sở thích
            var monYeuThich = (await _service.Query<CauTraLoi>()
                .Where(c => c.MaPhieuTraLoiNavigation != null
                    && c.MaPhieuTraLoiNavigation.MaKhachHang == id
                    && c.MaPhieuTraLoiNavigation.MaKhaoSatNavigation != null
                    && c.MaPhieuTraLoiNavigation.MaKhaoSatNavigation.LaSoThich
                    && c.MaTuyChonNavigation != null && c.MaTuyChonNavigation.MaMon != null)
                .Select(c => c.MaTuyChonNavigation!.MaMon!)
                .ToListAsync()).ToHashSet();

            var monUaThich = chiTietDaMua
                .GroupBy(ct => ct.MaMon)
                .Select(g =>
                {
                    // Số lần gọi = số đơn có món; gọi nhiều phần trong 1 đơn vẫn tính 1 lần
                    int soLanGoi = g.Select(ct => ct.MaHoaDon).Distinct().Count();
                    double tyLe = soDonHoanTat > 0 ? Math.Round(soLanGoi * 100.0 / soDonHoanTat, 1) : 0;
                    bool hayGoi = soLanGoi >= SoLanHayGoiToiThieu && tyLe >= TyLeHayGoiToiThieu;
                    return new
                    {
                        MaMon = g.Key,
                        TenMon = g.Select(ct => ct.TenMon).FirstOrDefault(ten => !string.IsNullOrWhiteSpace(ten)) ?? g.Key,
                        TenLoaiMon = g.Select(ct => ct.TenLoaiMon).FirstOrDefault(t => !string.IsNullOrWhiteSpace(t)),
                        SoLanGoi = soLanGoi,
                        TyLeDon = tyLe,
                        SoLuongDaMua = g.Sum(ct => ct.SoLuong),   // tổng số phần, chỉ để tham khảo
                        LanGanNhat = g.Max(ct => ct.Ngay),
                        MucDo = hayGoi ? "Hay gọi" : "Đã thử",
                        YeuThich = monYeuThich.Contains(g.Key)
                    };
                })
                .OrderByDescending(m => m.MucDo == "Hay gọi")
                .ThenByDescending(m => m.SoLanGoi)
                .ThenByDescending(m => m.LanGanNhat)
                .ThenBy(m => m.TenMon)
                .ToList();

            return Ok(new
            {
                khachHang.MaKhachHang,
                khachHang.HoTen,
                khachHang.SoDienThoai,
                khachHang.Email,
                khachHang.NgaySinh,
                khachHang.GioiTinh,
                khachHang.DiaChi,
                khachHang.TrangThai,
                khachHang.SoDonHang,
                khachHang.TongChiTieu,
                khachHang.SoDanhGia,
                khachHang.SoKhaoSatDaTraLoi,
                khachHang.LichSuHoaDon,
                khachHang.LichSuDanhGia,
                khachHang.LichSuKhaoSat,
                SoDonHoanTat = soDonHoanTat,
                MonUaThich = monUaThich
            });
        }

        // GET: api/AdminKhachHang/{id}/SoThich - Sở thích khách tự khai báo, nhóm theo câu hỏi (câu dị ứng đứng đầu).
        [HttpGet("{id}/SoThich")]
        public async Task<IActionResult> GetSoThich(string id)
        {
            if (!await _service.Query<KhachHang>().AnyAsync(k => k.MaKhachHang == id))
                return NotFound("Không tìm thấy khách hàng.");

            var phieu = await _service.Query<PhieuTraLoi>()
                .Include(p => p.CauTraLoi).ThenInclude(c => c.MaCauHoiNavigation)
                .Include(p => p.CauTraLoi).ThenInclude(c => c.MaTuyChonNavigation!).ThenInclude(t => t.MaMonNavigation)
                .FirstOrDefaultAsync(p => p.MaKhachHang == id && p.MaKhaoSatNavigation != null && p.MaKhaoSatNavigation.LaSoThich);

            if (phieu == null)
                return Ok(new { daKhaiBao = false, ngayCapNhat = (DateTime?)null, cauHoi = new List<object>() });

            var cauHoi = phieu.CauTraLoi
                .Where(c => c.MaCauHoiNavigation != null)
                .GroupBy(c => c.MaCauHoiNavigation!)
                .Select(g => new
                {
                    MaCauHoi = g.Key.MaCauHoi,
                    CauHoi = g.Key.NoiDungCauHoi,
                    Loai = AdminKhaoSatService.ChuanHoaLoai(g.Key.LoaiCauHoi) ?? g.Key.LoaiCauHoi,
                    LaDiUng = SoThichService.LaCauDiUng(g.Key.NoiDungCauHoi),
                    DapAn = AdminKhaoSatService.SapXep(g.Where(c => c.MaTuyChonNavigation != null), c => c.MaTuyChon!)
                        .Select(c => new
                        {
                            c.MaTuyChon,
                            NoiDung = c.MaTuyChonNavigation!.NoiDungTuyChon,
                            c.MaTuyChonNavigation.MaMon,
                            NgungBan = c.MaTuyChonNavigation.MaMonNavigation != null
                                       && SoThichService.NgungBan(c.MaTuyChonNavigation.MaMonNavigation.TrangThai)
                        }).ToList(),
                    NoiDung = g.Where(c => c.MaTuyChon == null).Select(c => c.NoiDungTuDien).FirstOrDefault()
                })
                .OrderByDescending(x => x.LaDiUng)
                .ThenBy(x => x.MaCauHoi.Length).ThenBy(x => x.MaCauHoi, StringComparer.Ordinal)
                .ToList();

            return Ok(new { daKhaiBao = true, ngayCapNhat = phieu.NgayTraLoi, cauHoi });
        }

        // POST: api/AdminKhachHang - Thêm khách hàng mới.
        [HttpPost]
        public async Task<IActionResult> CreateKhachHang(AdminCreateKhachHangRequest request)
        {
            var loi = KiemTraDuLieu(request.HoTen, request.SoDienThoai, request.Email, request.NgaySinh, request.GioiTinh, request.TrangThai, batBuocHoTen: true);
            if (loi != null) return BadRequest(loi);

            var email = ChuanHoa(request.Email);
            var sdt = ChuanHoa(request.SoDienThoai);

            if (email != null && await _service.Query<KhachHang>().AnyAsync(k => k.Email == email))
            {
                return BadRequest("Email đã được sử dụng.");
            }
            if (sdt != null && await _service.Query<KhachHang>().AnyAsync(k => k.SoDienThoai == sdt))
            {
                return BadRequest("Số điện thoại đã được sử dụng.");
            }

            var khachHang = new KhachHang
            {
                HoTen = request.HoTen.Trim(),
                SoDienThoai = sdt,
                Email = email,
                NgaySinh = request.NgaySinh,
                GioiTinh = ChuanHoa(request.GioiTinh),
                DiaChi = ChuanHoa(request.DiaChi),
                MatKhau = BCrypt.Net.BCrypt.HashPassword("123456"), // Mật khẩu mặc định, khách tự đổi sau khi đăng nhập
                TrangThai = ChuanHoa(request.TrangThai) ?? "Active"
            };

            var loiLuu = await KhachHangMoiHelper.LuuKhachHangMoiAsync(_service, khachHang);
            if (loiLuu != null) return BadRequest(loiLuu);

            return Ok(new { message = "Thêm khách hàng thành công.", maKhachHang = khachHang.MaKhachHang });
        }

        // PUT: api/AdminKhachHang/{id} - Cập nhật thông tin khách hàng.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateKhachHang(string id, AdminUpdateKhachHangRequest request)
        {
            var khachHang = await _service.FindAsync<KhachHang>(id);
            if (khachHang == null)
            {
                return NotFound("Không tìm thấy khách hàng.");
            }

            var loi = KiemTraDuLieu(request.HoTen, request.SoDienThoai, request.Email, request.NgaySinh, request.GioiTinh, request.TrangThai, batBuocHoTen: false);
            if (loi != null) return BadRequest(loi);

            var email = ChuanHoa(request.Email);
            var sdt = ChuanHoa(request.SoDienThoai);

            if (email != null &&
                await _service.Query<KhachHang>().AnyAsync(k => k.Email == email && k.MaKhachHang != id))
            {
                return BadRequest("Email đã được sử dụng bởi khách hàng khác.");
            }
            if (sdt != null &&
                await _service.Query<KhachHang>().AnyAsync(k => k.SoDienThoai == sdt && k.MaKhachHang != id))
            {
                return BadRequest("Số điện thoại đã được sử dụng bởi khách hàng khác.");
            }

            if (!string.IsNullOrWhiteSpace(request.HoTen))
                khachHang.HoTen = request.HoTen.Trim();
            khachHang.SoDienThoai = sdt;
            khachHang.Email = email;
            khachHang.NgaySinh = request.NgaySinh;
            khachHang.GioiTinh = ChuanHoa(request.GioiTinh);
            khachHang.DiaChi = ChuanHoa(request.DiaChi);
            if (!string.IsNullOrWhiteSpace(request.TrangThai))
                khachHang.TrangThai = request.TrangThai.Trim();

            await _service.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thông tin khách hàng thành công." });
        }

        // PUT: api/AdminKhachHang/{id}/Status - Khóa / mở khóa tài khoản khách hàng.
        [HttpPut("{id}/Status")]
        public async Task<IActionResult> UpdateStatus(string id, AdminLockKhachHangRequest request)
        {
            var khachHang = await _service.FindAsync<KhachHang>(id);
            if (khachHang == null)
            {
                return NotFound("Không tìm thấy khách hàng.");
            }

            if (request == null || !TrangThaiHopLe.Contains(request.TrangThai))
            {
                return BadRequest("Trạng thái không hợp lệ. Chỉ chấp nhận 'Active' hoặc 'Locked'.");
            }

            khachHang.TrangThai = request.TrangThai;
            await _service.SaveChangesAsync();

            return Ok(new
            {
                message = request.TrangThai == "Locked" ? "Đã khóa tài khoản khách hàng." : "Đã mở khóa tài khoản khách hàng.",
                trangThai = request.TrangThai
            });
        }

        // DELETE: api/AdminKhachHang/{id} - Khóa tài khoản nếu có dữ liệu liên quan, xóa vĩnh viễn nếu chưa có.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKhachHang(string id)
        {
            var khachHang = await _service.Query<KhachHang>()
                .Include(k => k.HoaDon)
                .Include(k => k.DanhGia)
                .Include(k => k.PhieuTraLoi)
                .Include(k => k.DoiTuongKhaoSat)
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (khachHang == null)
            {
                return NotFound("Không tìm thấy khách hàng.");
            }

            // Phiếu hồ sơ sở thích không tính là dữ liệu liên quan (R12)
            var maBoSoThich = await SoThichService.LayMaBoSoThichAsync(_service);
            bool hasRelatedData = khachHang.HoaDon.Any() || khachHang.DanhGia.Any()
                || khachHang.PhieuTraLoi.Any(p => p.MaKhaoSat != maBoSoThich)
                || khachHang.DoiTuongKhaoSat.Any();

            if (hasRelatedData)
            {
                khachHang.TrangThai = "Locked";
                await _service.SaveChangesAsync();
                return Ok(new
                {
                    message = "Khách hàng đã có dữ liệu giao dịch liên quan (hóa đơn, khảo sát hoặc đánh giá). Hệ thống đã tự động chuyển sang trạng thái tạm khóa tài khoản để bảo toàn dữ liệu lịch sử.",
                    isLocked = true
                });
            }

            // Khách chỉ có sở thích: xóa câu trả lời và phiếu sở thích trước, rồi xóa khách (cùng 1 lần lưu)
            await SoThichService.XoaPhieuSoThichAsync(_service, id);
            _service.Remove(khachHang);
            await _service.SaveChangesAsync();

            return Ok(new
            {
                message = "Đã xóa vĩnh viễn khách hàng khỏi hệ thống thành công.",
                isLocked = false
            });
        }

        // PUT: api/AdminKhachHang/{id}/DoiMatKhau - Đổi/đặt lại mật khẩu cho khách hàng từ quản trị
        [HttpPut("{id}/DoiMatKhau")]
        public async Task<IActionResult> DoiMatKhau(string id, [FromBody] AdminDoiMatKhauKhachHangRequest request)
        {
            var khachHang = await _service.FindAsync<KhachHang>(id);
            if (khachHang == null)
            {
                return NotFound("Không tìm thấy khách hàng.");
            }

            if (string.IsNullOrWhiteSpace(request?.MatKhauMoi) || request.MatKhauMoi.Trim().Length < 6)
            {
                return BadRequest("Mật khẩu mới phải có tối thiểu 6 ký tự.");
            }

            khachHang.MatKhau = BCrypt.Net.BCrypt.HashPassword(request.MatKhauMoi.Trim());
            await _service.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu cho khách hàng thành công." });
        }

        // ── Hàm hỗ trợ ───────────────────────────────────────────────────────

        private static string? ChuanHoa(string? s) => KhachHangValidator.ChuanHoa(s);

        private static string? KiemTraDuLieu(string? hoTen, string? soDienThoai, string? email, DateOnly? ngaySinh,
            string? gioiTinh, string? trangThai, bool batBuocHoTen)
            => KhachHangValidator.KiemTraDuLieu(hoTen, soDienThoai, email, ngaySinh, gioiTinh, trangThai, batBuocHoTen);    }
}

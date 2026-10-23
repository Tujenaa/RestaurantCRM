using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminKhachHangController : ControllerBase
    {
        private static readonly string[] TrangThaiHopLe = { "Active", "Locked" };
        private static readonly string[] GioiTinhHopLe = { "Nam", "Nữ", "Khác" };
        private static readonly Regex SoDienThoaiRegex = new Regex(@"^0\d{9,10}$");
        private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

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
                .OrderBy(k => k.MaKhachHang)
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

        // GET: api/AdminKhachHang/{id} - Chi tiết khách hàng kèm lịch sử đơn hàng và số liệu tổng hợp.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetKhachHang(string id)
        {
            var khachHang = await _service.Query<KhachHang>()
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
                    SoKhaoSatDaTraLoi = k.PhieuTraLoi.Count(),
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

            return Ok(khachHang);
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
                MaKhachHang = await TaoMaKhachHangAsync(),
                HoTen = request.HoTen.Trim(),
                SoDienThoai = sdt,
                Email = email,
                NgaySinh = request.NgaySinh,
                GioiTinh = ChuanHoa(request.GioiTinh),
                DiaChi = ChuanHoa(request.DiaChi),
                MatKhau = BCrypt.Net.BCrypt.HashPassword("123456"), // Mật khẩu mặc định, khách tự đổi sau khi đăng nhập
                TrangThai = ChuanHoa(request.TrangThai) ?? "Active"
            };

            _service.Add(khachHang);
            await _service.SaveChangesAsync();

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

        // DELETE: api/AdminKhachHang/{id} - Xóa khách hàng (chỉ khi chưa phát sinh dữ liệu liên quan).
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

            if (khachHang.HoaDon.Any() || khachHang.DanhGia.Any() || khachHang.PhieuTraLoi.Any())
            {
                return BadRequest("Khách hàng này đã phát sinh dữ liệu (đơn hàng, đánh giá hoặc phiếu khảo sát) nên không thể xóa. Vui lòng dùng chức năng 'Khóa tài khoản' thay thế.");
            }

            // Khách chỉ được mời khảo sát (chưa trả lời) -> xóa dòng đối tượng khảo sát để không vướng khóa ngoại
            if (khachHang.DoiTuongKhaoSat.Any())
            {
                _service.RemoveRange(khachHang.DoiTuongKhaoSat);
            }

            _service.Remove(khachHang);
            await _service.SaveChangesAsync();

            return NoContent();
        }

        // ── Hàm hỗ trợ ───────────────────────────────────────────────────────

        private static string? ChuanHoa(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private static string? KiemTraDuLieu(string? hoTen, string? soDienThoai, string? email, DateOnly? ngaySinh,
            string? gioiTinh, string? trangThai, bool batBuocHoTen)
        {
            if (batBuocHoTen && string.IsNullOrWhiteSpace(hoTen))
                return "Họ tên không được để trống.";
            if (!string.IsNullOrWhiteSpace(hoTen) && hoTen.Trim().Length > 100)
                return "Họ tên tối đa 100 ký tự.";

            var sdt = ChuanHoa(soDienThoai);
            if (sdt != null && !SoDienThoaiRegex.IsMatch(sdt))
                return "Số điện thoại không hợp lệ (bắt đầu bằng 0, gồm 10–11 chữ số).";

            var mail = ChuanHoa(email);
            if (mail != null && (!EmailRegex.IsMatch(mail) || mail.Length > 100))
                return "Email không hợp lệ.";

            if (ngaySinh.HasValue)
            {
                var homNay = DateOnly.FromDateTime(DateTime.Today);
                if (ngaySinh.Value > homNay) return "Ngày sinh không được lớn hơn ngày hiện tại.";
                if (ngaySinh.Value < homNay.AddYears(-120)) return "Ngày sinh không hợp lệ.";
            }

            var gt = ChuanHoa(gioiTinh);
            if (gt != null && !GioiTinhHopLe.Contains(gt))
                return "Giới tính chỉ nhận 'Nam', 'Nữ' hoặc 'Khác'.";

            var tt = ChuanHoa(trangThai);
            if (tt != null && !TrangThaiHopLe.Contains(tt))
                return "Trạng thái không hợp lệ. Chỉ chấp nhận 'Active' hoặc 'Locked'.";

            return null;
        }

        /// <summary>Sinh mã khách hàng tiếp theo dạng KH01, KH02, ... (bỏ qua các mã không theo số).</summary>
        private async Task<string> TaoMaKhachHangAsync()
        {
            var codes = await _service.ListAsync<KhachHang, string>(k => k.MaKhachHang.StartsWith("KH"), k => k.MaKhachHang);
            int max = 0;
            foreach (var code in codes)
            {
                if (code.Length > 2 && code.Length <= 8 && int.TryParse(code.Substring(2), out int n) && n > max)
                    max = n;
            }

            int next = max + 1;
            while (await _service.AnyAsync<KhachHang>(k => k.MaKhachHang == $"KH{next:D2}"))
                next++;
            return $"KH{next:D2}";
        }
    }
}
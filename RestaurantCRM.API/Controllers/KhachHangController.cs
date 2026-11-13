using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.API.DTOs;
using RestaurantCRM.API.Helpers;
using RestaurantCRM.API.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhachHangController : ControllerBase
    {
        private readonly RestaurantCRM.BusinessLogic.Services.IRestaurantService _service;

        public KhachHangController(RestaurantCRM.BusinessLogic.Services.IRestaurantService service)
        {
            _service = service;
        }

        // POST: api/KhachHang/DangKy - Đăng ký tài khoản khách hàng mới
        [HttpPost("DangKy")]
        public async Task<IActionResult> DangKy(RegisterRequest request)
        {
            var email = KhachHangValidator.ChuanHoa(request.Email);
            var sdt = KhachHangValidator.ChuanHoa(request.SoDienThoai);
            var gioiTinh = KhachHangValidator.ChuanHoa(request.GioiTinh);

            var loi = KhachHangValidator.KiemTraDuLieu(request.HoTen, sdt, email, request.NgaySinh, gioiTinh, null, batBuocHoTen: true)
                      ?? KhachHangValidator.KiemTraLienHe(sdt, email)
                      ?? KhachHangValidator.KiemTraMatKhau(request.MatKhau);
            if (loi != null) return BadRequest(loi);

            if (email != null && await _service.Query<KhachHang>().AnyAsync(k => k.Email == email))
            {
                return BadRequest("Email đã được sử dụng.");
            }
            if (sdt != null && await _service.Query<KhachHang>().AnyAsync(k => k.SoDienThoai == sdt))
            {
                return BadRequest("Số điện thoại đã được sử dụng.");
            }

            var newMaKhachHang = "KH" + DateTime.Now.Ticks.ToString().Substring(8, 6);

            var khachHang = new KhachHang
            {
                MaKhachHang = newMaKhachHang,
                HoTen = request.HoTen.Trim(),
                SoDienThoai = sdt,
                Email = email,
                NgaySinh = request.NgaySinh,
                GioiTinh = gioiTinh,
                MatKhau = BCrypt.Net.BCrypt.HashPassword(request.MatKhau),
                TrangThai = "Active"
            };

            _service.Add(khachHang);
            await _service.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công", maKhachHang = khachHang.MaKhachHang });
        }

        // POST: api/KhachHang/DangNhap - Xác thực đăng nhập khách hàng
        [HttpPost("DangNhap")]
        public async Task<IActionResult> DangNhap(LoginRequest request)
        {
            var khachHang = await _service.Query<KhachHang>().FirstOrDefaultAsync(k =>
                (k.Email == request.EmailOrPhone || k.SoDienThoai == request.EmailOrPhone));

            if (khachHang == null)
            {
                return NotFound("Tài khoản không tồn tại.");
            }

            if (!PasswordVerifier.Verify(request.MatKhau, khachHang.MatKhau))
            {
                return Unauthorized("Email/Số điện thoại hoặc mật khẩu không đúng.");
            }

            if (khachHang.TrangThai != "Active")
            {
                return BadRequest("Tài khoản đã bị khóa.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("Chuoi_Bao_Mat_Bi_Mat_Cua_Nha_Hang_CRM_123456");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, khachHang.MaKhachHang),
                    new Claim(ClaimTypes.Name, khachHang.HoTen ?? ""),
                    new Claim(ClaimTypes.Role, "KhachHang")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return Ok(new
            {
                message = "Đăng nhập thành công",
                token = jwtToken,
                khachHang = new
                {
                    khachHang.MaKhachHang,
                    khachHang.HoTen,
                    khachHang.Email,
                    khachHang.SoDienThoai,
                    khachHang.DiaChi
                }
            });
        }

        // GET: api/KhachHang/ThongTin/{id} - Lấy thông tin chi tiết của khách hàng
        // Lưu ý: Sau khi có JWT, ta sẽ lấy ID từ token thay vì truyền qua URL
        [HttpGet("ThongTin/{id}")]
        public async Task<IActionResult> ThongTin(string id)
        {
            var khachHang = await _service.Query<KhachHang>()
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (khachHang == null)
            {
                return NotFound("Không tìm thấy thông tin khách hàng.");
            }

            return Ok(new
            {
                khachHang.MaKhachHang,
                khachHang.HoTen,
                khachHang.Email,
                khachHang.SoDienThoai,
                khachHang.NgaySinh,
                khachHang.GioiTinh,
                khachHang.DiaChi
            });
        }

        // PUT: api/KhachHang/ThongTin/{id} - Cập nhật thông tin cá nhân khách hàng
        [HttpPut("ThongTin/{id}")]
        // Cập nhật từng phần: HoTen bắt buộc; SoDienThoai, Email, NgaySinh, GioiTinh, DiaChi để trống thì giữ giá trị cũ.
        // Sau khi gộp với dữ liệu cũ phải còn ít nhất email hoặc số điện thoại.
        public async Task<IActionResult> CapNhatThongTin(string id, UpdateProfileRequest request)
        {
            var loiTrangThai = await KhachHangValidator.KiemTraHoatDongAsync(_service, id);
            if (loiTrangThai != null) return BadRequest(loiTrangThai);

            var khachHang = await _service.FindAsync<KhachHang>(id);
            if (khachHang == null)
            {
                return NotFound("Không tìm thấy thông tin khách hàng.");
            }

            var hoTen = KhachHangValidator.ChuanHoa(request.HoTen);
            var soDienThoai = KhachHangValidator.ChuanHoa(request.SoDienThoai) ?? khachHang.SoDienThoai;
            var email = KhachHangValidator.ChuanHoa(request.Email) ?? khachHang.Email;
            var ngaySinh = request.NgaySinh ?? khachHang.NgaySinh;
            var gioiTinh = KhachHangValidator.ChuanHoa(request.GioiTinh) ?? khachHang.GioiTinh;
            var diaChi = KhachHangValidator.ChuanHoa(request.DiaChi) ?? khachHang.DiaChi;

            var loi = KhachHangValidator.KiemTraDuLieu(hoTen, soDienThoai, email, request.NgaySinh,
                          KhachHangValidator.ChuanHoa(request.GioiTinh), null, batBuocHoTen: true)
                      ?? KhachHangValidator.KiemTraLienHe(soDienThoai, email);
            if (loi != null) return BadRequest(loi);
            if (diaChi?.Length > 255) return BadRequest("Địa chỉ tối đa 255 ký tự.");

            if (email != null && await _service.Query<KhachHang>().AnyAsync(k => k.Email == email && k.MaKhachHang != id))
            {
                return BadRequest("Email đã được sử dụng bởi người khác.");
            }
            if (soDienThoai != null && await _service.Query<KhachHang>().AnyAsync(k => k.SoDienThoai == soDienThoai && k.MaKhachHang != id))
            {
                return BadRequest("Số điện thoại đã được sử dụng bởi người khác.");
            }

            khachHang.HoTen = hoTen;
            khachHang.SoDienThoai = soDienThoai;
            khachHang.Email = email;
            khachHang.NgaySinh = ngaySinh;
            khachHang.GioiTinh = gioiTinh;
            khachHang.DiaChi = diaChi;

            await _service.SaveChangesAsync();

            return Ok("Cập nhật thông tin thành công.");
        }

        // PUT: api/KhachHang/DiaChi/{id} - Chỉ cập nhật địa chỉ giao hàng
        [HttpPut("DiaChi/{id}")]
        public async Task<IActionResult> CapNhatDiaChi(string id, UpdateAddressRequest request)
        {
            var diaChi = request.DiaChi?.Trim();
            if (string.IsNullOrEmpty(diaChi)) return BadRequest("Vui lòng nhập địa chỉ.");
            if (diaChi.Length > 255) return BadRequest("Địa chỉ tối đa 255 ký tự.");

            var loiTrangThai = await KhachHangValidator.KiemTraHoatDongAsync(_service, id);
            if (loiTrangThai != null) return BadRequest(loiTrangThai);

            var khachHang = await _service.FindAsync<KhachHang>(id);
            if (khachHang == null)
            {
                return NotFound("Không tìm thấy thông tin khách hàng.");
            }

            khachHang.DiaChi = diaChi;
            await _service.SaveChangesAsync();

            return Ok("Cập nhật địa chỉ thành công.");
        }

        // PUT: api/KhachHang/DoiMatKhau/{id} - Đổi mật khẩu
        [HttpPut("DoiMatKhau/{id}")]
        public async Task<IActionResult> DoiMatKhau(string id, ChangePasswordRequest request)
        {
            var loiTrangThai = await KhachHangValidator.KiemTraHoatDongAsync(_service, id);
            if (loiTrangThai != null) return BadRequest(loiTrangThai);

            var loiMatKhau = KhachHangValidator.KiemTraMatKhau(request.MatKhauMoi);
            if (loiMatKhau != null) return BadRequest("Mật khẩu mới phải có tối thiểu 6 ký tự.");

            var khachHang = await _service.FindAsync<KhachHang>(id);
            if (khachHang == null)
            {
                return NotFound("Không tìm thấy thông tin khách hàng.");
            }

            if (!PasswordVerifier.Verify(request.MatKhauCu, khachHang.MatKhau))
            {
                return BadRequest("Mật khẩu cũ không chính xác.");
            }

            khachHang.MatKhau = BCrypt.Net.BCrypt.HashPassword(request.MatKhauMoi);
            await _service.SaveChangesAsync();

            return Ok("Đổi mật khẩu thành công.");
        }
    }
}

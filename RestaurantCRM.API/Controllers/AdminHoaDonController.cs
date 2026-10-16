using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminHoaDonController : ControllerBase
    {
        private readonly RestaurantCrmContext _context;

        public AdminHoaDonController(RestaurantCrmContext context)
        {
            _context = context;
        }

        // GET: api/AdminHoaDon - Lấy toàn bộ danh sách đơn hàng
        [HttpGet]
        public async Task<IActionResult> GetAllHoaDons()
        {
            var hoaDons = await _context.HoaDon
                .Include(d => d.MaKhachHangNavigation)
                .Include(d => d.MaNhanVienNavigation)
                .OrderByDescending(d => d.NgayDat)
                .AsNoTracking()
                .Select(d => new
                {
                    d.MaHoaDon,
                    d.NgayDat,
                    TenKhachHang = d.MaKhachHangNavigation != null ? d.MaKhachHangNavigation.HoTen : (!string.IsNullOrEmpty(d.TenKhachHang) ? d.TenKhachHang : "Khách vãng lai"),
                    d.TrangThai,
                    d.TongTienHang,
                    d.TongThanhToan,
                    d.MaNhanVien,
                    TenNhanVien = d.MaNhanVienNavigation != null ? d.MaNhanVienNavigation.HoTen : null
                })
                .ToListAsync();

            return Ok(hoaDons);
        }

        // GET: api/AdminHoaDon/{id} - Lấy chi tiết đơn hàng
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChiTietHoaDon(string id)
        {
            var hoaDon = await _context.HoaDon
                .Include(d => d.MaKhachHangNavigation)
                .Include(d => d.MaNhanVienNavigation)
                .Include(d => d.MaKmvoucherNavigation)
                    .ThenInclude(v => v.MaChuongTrinhNavigation)
                .Include(d => d.ThanhToan)
                .Include(d => d.ChiTietHoaDon)
                    .ThenInclude(c => c.MaMonNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaHoaDon == id);

            if (hoaDon == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            var result = new
            {
                hoaDon.MaHoaDon,
                hoaDon.NgayDat,
                hoaDon.TrangThai,
                hoaDon.DiaChiGiao,
                hoaDon.TongTienHang,
                hoaDon.TienGiamVoucher,
                hoaDon.TongThanhToan,
                KhachHang = hoaDon.MaKhachHangNavigation != null 
                    ? new { HoTen = hoaDon.MaKhachHangNavigation.HoTen, SoDienThoai = hoaDon.MaKhachHangNavigation.SoDienThoai } 
                    : new { HoTen = !string.IsNullOrEmpty(hoaDon.TenKhachHang) ? hoaDon.TenKhachHang : "Khách vãng lai", SoDienThoai = hoaDon.SoDienThoai ?? "" },
                NhanVienPhuTrach = hoaDon.MaNhanVienNavigation != null ? hoaDon.MaNhanVienNavigation.HoTen : null,
                KhuyenMai = hoaDon.MaKmvoucherNavigation?.MaChuongTrinhNavigation?.TenChuongTrinh,
                ThanhToan = hoaDon.ThanhToan.Select(t => new { t.PhuongThuc, t.NgayThanhToan, t.SoTien, t.TrangThai }),
                ChiTiet = hoaDon.ChiTietHoaDon.Select(c => new
                {
                    c.MaMon,
                    TenMon = c.MaMonNavigation?.TenMon,
                    c.SoLuong,
                    c.DonGiaSauGiam,
                    c.ThanhTien
                })
            };

            return Ok(result);
        }

        // GET: api/AdminHoaDon/{id}/LichSuTrangThai - Xem chi tiết lịch sử cập nhật trạng thái của đơn hàng
        [HttpGet("{id}/LichSuTrangThai")]
        public async Task<IActionResult> GetLichSuTrangThai(string id)
        {
            var lichSu = await _context.LichSuTrangThai
                .Where(l => l.MaHoaDon == id)
                .OrderByDescending(l => l.ThoiGian)
                .Select(l => new
                {
                    l.MaLichSu,
                    l.TrangThai,
                    l.ThoiGian,
                    l.GhiChu
                })
                .ToListAsync();

            return Ok(lichSu);
        }

        private static readonly Dictionary<string, int> StatusOrder = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Pending", 1 },
            { "Confirmed", 2 },
            { "Preparing", 3 },
            { "Delivering", 4 },
            { "Completed", 5 }
        };

        private static string NormalizeStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return "Pending";
            var s = status.Trim().ToLower();
            if (s.Contains("cancel") || s.Contains("hủy")) return "Cancelled";
            if (s.Contains("complete") || s.Contains("hoàn thành")) return "Completed";
            if (s.Contains("deliver") || s.Contains("giao") || s.Contains("shipping")) return "Delivering";
            if (s.Contains("prepar") || s.Contains("chuẩn bị") || s.Contains("processing")) return "Preparing";
            if (s.Contains("confirm") || s.Contains("xác nhận")) return "Confirmed";
            return "Pending";
        }

        // PUT: api/AdminHoaDon/{id}/TrangThai - Cập nhật trạng thái mới cho đơn hàng và lưu log
        [HttpPut("{id}/TrangThai")]
        public async Task<IActionResult> UpdateTrangThaiHoaDon(string id, UpdateOrderStatusRequest request)
        {
            var hoaDon = await _context.HoaDon
                .Include(d => d.ChiTietHoaDon)
                .FirstOrDefaultAsync(d => d.MaHoaDon == id);

            if (hoaDon == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            var trangThaiCu = hoaDon.TrangThai ?? "Pending";
            var curNorm = NormalizeStatus(trangThaiCu);
            var newNorm = NormalizeStatus(request.TrangThaiMoi);

            // Kiểm tra trạng thái kết thúc
            if (curNorm == "Completed")
            {
                return BadRequest("Đơn hàng đã ở trạng thái 'Completed' (Hoàn thành), không thể cập nhật thêm.");
            }
            if (curNorm == "Cancelled")
            {
                return BadRequest("Đơn hàng đã ở trạng thái 'Cancelled' (Đã hủy), không thể cập nhật thêm.");
            }

            // Kiểm tra cập nhật 1 chiều
            if (newNorm != "Cancelled")
            {
                int curStep = StatusOrder.GetValueOrDefault(curNorm, 1);
                int newStep = StatusOrder.GetValueOrDefault(newNorm, 1);

                if (newStep <= curStep)
                {
                    return BadRequest($"Trạng thái đơn hàng chỉ được phép cập nhật 1 chiều tiến tới: Pending -> Confirmed -> Preparing -> Delivering -> Completed. Không thể chuyển từ '{curNorm}' về '{newNorm}'.");
                }
            }

            // Logic hoàn kho: Hoàn lại số lượng nguyên liệu nếu đơn hủy
            if (newNorm == "Cancelled")
            {
                foreach (var chiTiet in hoaDon.ChiTietHoaDon)
                {
                    var dinhLuongs = await _context.DinhLuong.Where(dl => dl.MaMon == chiTiet.MaMon).ToListAsync();
                    foreach (var dl in dinhLuongs)
                    {
                        var nl = await _context.NguyenLieu.FindAsync(dl.MaNguyenLieu);
                        if (nl != null)
                        {
                            nl.SoLuongTon = (nl.SoLuongTon ?? 0) + (decimal)(chiTiet.SoLuong ?? 0) * dl.SoLuongCan;
                        }
                    }
                }
            }

            // Gán nhân viên tiếp nhận đơn (người đầu tiên xác nhận/xử lý đơn)
            if (!string.IsNullOrEmpty(request.MaNhanVien) && string.IsNullOrEmpty(hoaDon.MaNhanVien))
            {
                var nv = await _context.NhanVien.FirstOrDefaultAsync(n => n.MaNhanVien == request.MaNhanVien || n.TenDangNhap == request.MaNhanVien);
                hoaDon.MaNhanVien = nv != null ? nv.MaNhanVien : request.MaNhanVien;
            }

            // Nếu hoàn tất đơn hàng và chưa có ngày hoàn tất, lưu thời gian hiện tại
            if (newNorm == "Completed" && hoaDon.NgayHoanTat == null)
            {
                hoaDon.NgayHoanTat = DateTime.Now;
            }

            // Cập nhật trạng thái mới
            hoaDon.TrangThai = newNorm;

            // Load tên nhân viên để ghi log chi tiết
            if (hoaDon.MaNhanVienNavigation == null && !string.IsNullOrEmpty(hoaDon.MaNhanVien))
            {
                await _context.Entry(hoaDon).Reference(h => h.MaNhanVienNavigation).LoadAsync();
            }

            var tenNv = hoaDon.MaNhanVienNavigation?.HoTen;
            var defaultNote = !string.IsNullOrEmpty(tenNv)
                ? $"Nhân viên '{tenNv}' chuyển trạng thái: {trangThaiCu} -> {newNorm}"
                : $"Chuyển trạng thái: {trangThaiCu} -> {newNorm}";

            var note = !string.IsNullOrWhiteSpace(request.GhiChu) 
                ? request.GhiChu 
                : defaultNote;

            var lichSu = new LichSuTrangThai
            {
                MaLichSu = "LS" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                MaHoaDon = id,
                TrangThai = newNorm,
                ThoiGian = DateTime.Now,
                GhiChu = note
            };
            
            _context.LichSuTrangThai.Add(lichSu);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật trạng thái thành công", trangThai = newNorm });
        }
    }
}

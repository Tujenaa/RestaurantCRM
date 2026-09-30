using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonHangController : ControllerBase
    {
        private readonly RestaurantCRMContext _context;

        public DonHangController(RestaurantCRMContext context)
        {
            _context = context;
        }

        // POST: api/DonHang - Tạo đơn hàng mới
        [HttpPost]
        public async Task<IActionResult> CreateOrder(OrderRequest request)
        {
            if (request.Items == null || !request.Items.Any())
            {
                return BadRequest("Đơn hàng phải có ít nhất 1 món.");
            }

            // Tạo mã đơn hàng mới
            var maDonHang = "DH" + DateTime.Now.Ticks.ToString().Substring(8, 6);

            var donHang = new DonHang
            {
                MaDonHang = maDonHang,
                MaKhachHang = request.MaKhachHang,
                DiaChiGiao = request.DiaChiGiao,
                NgayDat = DateTime.Now,
                TrangThai = "Pending", // Đang chờ xác nhận
                TongTienHang = 0,
                TongThanhToan = 0
            };

            _context.DonHangs.Add(donHang);

            double tongTien = 0;

            foreach (var item in request.Items)
            {
                // Lấy thông tin món từ DB để có giá chính xác
                var monAn = await _context.MonAns.FindAsync(item.MaMon);
                if (monAn != null && monAn.DonGia.HasValue)
                {
                    // Kiểm tra tồn kho
                    if (monAn.SoLuong == null || monAn.SoLuong < item.SoLuong)
                    {
                        return BadRequest($"Món ăn '{monAn.TenMon}' không đủ số lượng phục vụ (Chỉ còn: {monAn.SoLuong ?? 0}).");
                    }

                    // Trừ tồn kho
                    monAn.SoLuong -= item.SoLuong;

                    var maChiTiet = "CT" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                    var chiTiet = new ChiTietDonHang
                    {
                        MaChiTiet = maChiTiet,
                        MaDonHang = maDonHang,
                        MaMon = item.MaMon,
                        SoLuong = item.SoLuong,
                        DonGiaGoc = monAn.DonGia.Value,
                        DonGiaSauGiam = monAn.DonGia.Value, // Tạm thời bằng giá gốc
                        ThanhTien = monAn.DonGia.Value * item.SoLuong
                    };

                    _context.ChiTietDonHangs.Add(chiTiet);
                    tongTien += chiTiet.ThanhTien ?? 0;
                }
            }

            donHang.TongTienHang = tongTien;
            donHang.TongThanhToan = tongTien; // Tạm thời bằng tổng tiền hàng (chưa trừ voucher)

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đặt hàng thành công", maDonHang = donHang.MaDonHang });
        }

        // GET: api/DonHang/LichSu/{maKhachHang} - Lấy lịch sử đơn hàng của khách
        [HttpGet("LichSu/{maKhachHang}")]
        public async Task<IActionResult> GetLichSuDonHang(string maKhachHang)
        {
            var donHangs = await _context.DonHangs
                .Where(d => d.MaKhachHang == maKhachHang)
                .OrderByDescending(d => d.NgayDat)
                .AsNoTracking()
                .Select(d => new 
                {
                    d.MaDonHang,
                    d.NgayDat,
                    d.TrangThai,
                    d.TongThanhToan,
                    SoMon = d.ChiTietDonHangs.Sum(c => c.SoLuong)
                })
                .ToListAsync();

            return Ok(donHangs);
        }

        // GET: api/DonHang/{id} - Lấy chi tiết một đơn hàng
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChiTietDonHang(string id)
        {
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.MaMonNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDonHang == id);

            if (donHang == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            var result = new
            {
                donHang.MaDonHang,
                donHang.NgayDat,
                donHang.TrangThai,
                donHang.DiaChiGiao,
                donHang.TongTienHang,
                donHang.TongThanhToan,
                ChiTiet = donHang.ChiTietDonHangs.Select(c => new
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
    }
}

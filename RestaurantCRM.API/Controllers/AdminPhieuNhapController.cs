using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminPhieuNhapController : ControllerBase
    {
        private readonly RestaurantCrmContext _context;

        public AdminPhieuNhapController(RestaurantCrmContext context)
        {
            _context = context;
        }

        // GET: api/AdminPhieuNhap - Lấy danh sách phiếu nhập
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var phieuNhaps = await _context.PhieuNhapHang
                .Include(p => p.MaNhaCungCapNavigation)
                .Include(p => p.MaNhanVienNavigation)
                .OrderByDescending(p => p.NgayNhap)
                .Select(p => new
                {
                    p.MaPhieuNhap,
                    p.MaNhaCungCap,
                    TenNhaCungCap = p.MaNhaCungCapNavigation != null ? p.MaNhaCungCapNavigation.TenNhaCungCap : null,
                    p.MaNhanVien,
                    TenNhanVien = p.MaNhanVienNavigation != null ? p.MaNhanVienNavigation.HoTen : null,
                    p.NgayNhap,
                    p.TongTien,
                    p.GhiChu
                })
                .ToListAsync();

            return Ok(phieuNhaps);
        }

        // GET: api/AdminPhieuNhap/{id} - Lấy chi tiết phiếu nhập
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var phieuNhap = await _context.PhieuNhapHang
                .Include(p => p.MaNhaCungCapNavigation)
                .Include(p => p.MaNhanVienNavigation)
                .Include(p => p.ChiTietPhieuNhap)
                    .ThenInclude(c => c.MaMonNavigation)
                .FirstOrDefaultAsync(p => p.MaPhieuNhap == id);

            if (phieuNhap == null)
            {
                return NotFound("Không tìm thấy phiếu nhập.");
            }

            var result = new
            {
                phieuNhap.MaPhieuNhap,
                phieuNhap.MaNhaCungCap,
                TenNhaCungCap = phieuNhap.MaNhaCungCapNavigation?.TenNhaCungCap,
                phieuNhap.MaNhanVien,
                TenNhanVien = phieuNhap.MaNhanVienNavigation?.HoTen,
                phieuNhap.NgayNhap,
                phieuNhap.TongTien,
                phieuNhap.GhiChu,
                ChiTiet = phieuNhap.ChiTietPhieuNhap.Select(c => new
                {
                    c.MaChiTiet,
                    c.MaMon,
                    TenMon = c.MaMonNavigation?.TenMon,
                    c.SoLuong,
                    c.DonGiaNhap,
                    c.ThanhTien
                })
            };

            return Ok(result);
        }

        // POST: api/AdminPhieuNhap - Lập phiếu nhập và cộng tồn kho
        [HttpPost]
        public async Task<IActionResult> Create(PhieuNhapCreateRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var phieuNhap = new PhieuNhapHang
                {
                    MaPhieuNhap = "PN" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    MaNhaCungCap = request.MaNhaCungCap,
                    MaNhanVien = request.MaNhanVien,
                    NgayNhap = DateTime.Now,
                    GhiChu = request.GhiChu,
                    TongTien = 0
                };

                _context.PhieuNhapHang.Add(phieuNhap);

                double tongTien = 0;

                foreach (var item in request.ChiTiet)
                {
                    var thanhTien = item.SoLuong * item.DonGiaNhap;
                    tongTien += thanhTien;

                    var chiTiet = new ChiTietPhieuNhap
                    {
                        MaChiTiet = "CTPN" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper(),
                        MaPhieuNhap = phieuNhap.MaPhieuNhap,
                        MaMon = item.MaMon,
                        SoLuong = item.SoLuong,
                        DonGiaNhap = item.DonGiaNhap,
                        ThanhTien = thanhTien
                    };
                    _context.ChiTietPhieuNhap.Add(chiTiet);

                    // Cập nhật tồn kho trong bảng MonAn
                    var monAn = await _context.MonAn.FindAsync(item.MaMon);
                    if (monAn != null)
                    {
                        monAn.SoLuong = (monAn.SoLuong ?? 0) + item.SoLuong;
                        _context.MonAn.Update(monAn);
                    }
                }

                phieuNhap.TongTien = tongTien;
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetById), new { id = phieuNhap.MaPhieuNhap }, new { message = "Lập phiếu nhập thành công", MaPhieuNhap = phieuNhap.MaPhieuNhap });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi khi lập phiếu nhập: " + ex.Message);
            }
        }
    }
}

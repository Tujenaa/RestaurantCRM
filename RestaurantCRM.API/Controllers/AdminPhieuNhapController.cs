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
            // Tự động gán mã nhân viên cho các phiếu nhập cũ bị thiếu MaNhanVien
            var missingStaffPns = await _context.PhieuNhapHang.Where(p => string.IsNullOrEmpty(p.MaNhanVien)).ToListAsync();
            if (missingStaffPns.Any())
            {
                var defaultStaff = await _context.NhanVien.OrderBy(n => n.MaNhanVien).FirstOrDefaultAsync();
                if (defaultStaff != null)
                {
                    foreach (var pn in missingStaffPns)
                    {
                        pn.MaNhanVien = defaultStaff.MaNhanVien;
                    }
                    await _context.SaveChangesAsync();
                }
            }

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
                    TenNhanVien = p.MaNhanVienNavigation != null ? p.MaNhanVienNavigation.HoTen : (p.MaNhanVien ?? "Quản trị viên"),
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
                    .ThenInclude(c => c.MaNguyenLieuNavigation)
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
                TenNhanVien = phieuNhap.MaNhanVienNavigation?.HoTen ?? phieuNhap.MaNhanVien ?? "Quản trị viên",
                phieuNhap.NgayNhap,
                phieuNhap.TongTien,
                phieuNhap.GhiChu,
                ChiTiet = phieuNhap.ChiTietPhieuNhap.Select(c => new
                {
                    c.MaChiTiet,
                    c.MaNguyenLieu,
                    TenNguyenLieu = c.MaNguyenLieuNavigation?.TenNguyenLieu,
                    DonViTinh = c.MaNguyenLieuNavigation?.DonViTinh,
                    c.SoLuong,
                    c.DonGiaNhap,
                    c.ThanhTien,
                    // Tương thích ngược với client cũ
                    MaMon = c.MaNguyenLieu,
                    TenMon = c.MaNguyenLieuNavigation?.TenNguyenLieu
                })
            };

            return Ok(result);
        }

        // POST: api/AdminPhieuNhap - Lập phiếu nhập và cộng tồn kho nguyên liệu
        [HttpPost]
        public async Task<IActionResult> Create(PhieuNhapCreateRequest request)
        {
            if (request == null || request.ChiTiet == null || !request.ChiTiet.Any())
            {
                return BadRequest("Phiếu nhập phải có ít nhất một mặt hàng nguyên liệu.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var maPN = await Helpers.CodeGenerator.GenerateMaPhieuNhapAsync(_context);

                // Xác định mã nhân viên hợp lệ
                string? maNhanVien = request.MaNhanVien;
                if (!string.IsNullOrWhiteSpace(maNhanVien))
                {
                    var nv = await _context.NhanVien.FirstOrDefaultAsync(n => n.MaNhanVien == maNhanVien || n.TenDangNhap == maNhanVien);
                    if (nv != null)
                    {
                        maNhanVien = nv.MaNhanVien;
                    }
                    else
                    {
                        var defaultNv = await _context.NhanVien.OrderBy(n => n.MaNhanVien).FirstOrDefaultAsync();
                        maNhanVien = defaultNv?.MaNhanVien;
                    }
                }
                else
                {
                    var defaultNv = await _context.NhanVien.OrderBy(n => n.MaNhanVien).FirstOrDefaultAsync();
                    maNhanVien = defaultNv?.MaNhanVien;
                }

                var phieuNhap = new PhieuNhapHang
                {
                    MaPhieuNhap = maPN,
                    MaNhaCungCap = request.MaNhaCungCap,
                    MaNhanVien = maNhanVien,
                    NgayNhap = DateTime.Now,
                    GhiChu = request.GhiChu,
                    TongTien = 0
                };

                _context.PhieuNhapHang.Add(phieuNhap);

                double tongTien = 0;

                foreach (var item in request.ChiTiet)
                {
                    string maNl = item.TargetMaNguyenLieu;
                    var thanhTien = (double)item.SoLuong * item.DonGiaNhap;
                    tongTien += thanhTien;

                    var chiTiet = new ChiTietPhieuNhap
                    {
                        MaChiTiet = "CTPN" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper(),
                        MaPhieuNhap = phieuNhap.MaPhieuNhap,
                        MaNguyenLieu = maNl,
                        SoLuong = item.SoLuong,
                        DonGiaNhap = item.DonGiaNhap,
                        ThanhTien = thanhTien
                    };
                    _context.ChiTietPhieuNhap.Add(chiTiet);

                    // Cập nhật tồn kho và giá nhập gần nhất trong bảng NGUYEN_LIEU
                    var nguyenLieu = await _context.NguyenLieu.FindAsync(maNl);
                    if (nguyenLieu != null)
                    {
                        nguyenLieu.SoLuongTon = (nguyenLieu.SoLuongTon ?? 0) + item.SoLuong;
                        nguyenLieu.GiaNhapGanNhat = item.DonGiaNhap;
                        _context.NguyenLieu.Update(nguyenLieu);
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

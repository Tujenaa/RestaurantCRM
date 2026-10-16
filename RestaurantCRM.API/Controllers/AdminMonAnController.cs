using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminMonAnController : ControllerBase
    {
        private readonly RestaurantCrmContext _context;

        public AdminMonAnController(RestaurantCrmContext context)
        {
            _context = context;
        }

        // GET: api/AdminMonAn - Lấy toàn bộ danh sách món ăn (kể cả món ngừng kinh doanh)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MonAn>>> GetMonAns()
        {
            // Admin cần xem toàn bộ các món (kể cả ngừng kinh doanh)
            return await _context.MonAn
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/AdminMonAn/5 - Xem chi tiết 1 món ăn
        [HttpGet("{id}")]
        public async Task<ActionResult<MonAn>> GetMonAn(string id)
        {
            var monAn = await _context.MonAn.FindAsync(id);

            if (monAn == null)
            {
                return NotFound();
            }

            return monAn;
        }

        // POST: api/AdminMonAn - Thêm món ăn mới
        [HttpPost]
        public async Task<ActionResult<MonAn>> PostMonAn(MonAn monAn)
        {
            if (string.IsNullOrWhiteSpace(monAn.MaMon))
            {
                monAn.MaMon = await Helpers.CodeGenerator.GenerateMaMonAnAsync(_context);
            }

            if (string.IsNullOrEmpty(monAn.TrangThai) || monAn.TrangThai == "Active")
            {
                monAn.TrangThai = "InStock";
            }

            // Tự động điều chỉnh trạng thái dựa trên số lượng (nếu không phải là hàng đã ngừng kinh doanh)
            if (monAn.TrangThai != "Discontinued")
            {
                if ((monAn.SoLuong ?? 0) <= 0 && monAn.TrangThai != "InStock")
                {
                    monAn.TrangThai = "OutOfStock";
                }
                else if ((monAn.SoLuong ?? 0) > 0 && monAn.TrangThai == "OutOfStock")
                {
                    monAn.TrangThai = "InStock";
                }
            }

            _context.MonAn.Add(monAn);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (MonAnExists(monAn.MaMon))
                {
                    return Conflict("Mã món ăn đã tồn tại.");
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction(nameof(GetMonAn), new { id = monAn.MaMon }, monAn);
        }

        // PUT: api/AdminMonAn/5 - Sửa thông tin món ăn
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMonAn(string id, MonAn monAn)
        {
            if (id != monAn.MaMon)
            {
                return BadRequest("Mã món ăn không khớp.");
            }

            var existing = await _context.MonAn.FindAsync(id);
            if (existing == null)
            {
                return NotFound("Không tìm thấy món ăn để cập nhật.");
            }

            // Cập nhật các trường thông tin cơ bản
            if (!string.IsNullOrWhiteSpace(monAn.TenMon))
            {
                existing.TenMon = monAn.TenMon;
            }
            if (monAn.DonGia.HasValue)
            {
                existing.DonGia = monAn.DonGia;
            }
            if (!string.IsNullOrWhiteSpace(monAn.MaLoaiMon))
            {
                existing.MaLoaiMon = monAn.MaLoaiMon;
            }
            if (monAn.DuongDanAnh != null)
            {
                existing.DuongDanAnh = monAn.DuongDanAnh;
            }
            if (monAn.MoTa != null)
            {
                existing.MoTa = monAn.MoTa;
            }

            // Cập nhật trạng thái nếu có
            if (!string.IsNullOrWhiteSpace(monAn.TrangThai))
            {
                if (monAn.TrangThai == "Inactive")
                {
                    existing.TrangThai = "Discontinued";
                }
                else
                {
                    existing.TrangThai = monAn.TrangThai;
                }
            }

            // Giữ nguyên số lượng hiện có khi sửa món, chỉ thay đổi nếu có gửi số lượng cụ thể > 0
            if (monAn.SoLuong.HasValue && monAn.SoLuong.Value > 0)
            {
                existing.SoLuong = monAn.SoLuong.Value;
            }

            // Tự động điều chỉnh trạng thái InStock / OutOfStock dựa trên số lượng (nếu không phải là hàng đã ngừng kinh doanh)
            if (existing.TrangThai != "Discontinued")
            {
                if ((existing.SoLuong ?? 0) <= 0)
                {
                    existing.TrangThai = "OutOfStock";
                }
                else
                {
                    existing.TrangThai = "InStock";
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/AdminMonAn/5 - Xóa cứng món ăn có kiểm tra điều kiện ràng buộc
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMonAn(string id)
        {
            var monAn = await _context.MonAn
                .Include(m => m.ChiTietHoaDon)
                .Include(m => m.ChiTietPhieuNhap)
                .Include(m => m.DanhGia) // Đã sửa DanhGias thành DanhGia theo đúng Model
                .FirstOrDefaultAsync(m => m.MaMon == id);

            if (monAn == null)
            {
                return NotFound("Không tìm thấy món ăn để xóa.");
            }

            // Kiểm tra ràng buộc trước khi xóa cứng
            if (monAn.ChiTietHoaDon.Any() || monAn.ChiTietPhieuNhap.Any())
            {
                return BadRequest("Không thể xóa món ăn này vì đã có dữ liệu liên quan trong Đơn hàng hoặc Phiếu nhập. Vui lòng chuyển trạng thái thành 'Ngừng kinh doanh' (Inactive) thay vì xóa.");
            }

            _context.MonAn.Remove(monAn);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MonAnExists(string id)
        {
            return _context.MonAn.Any(e => e.MaMon == id);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminNguyenLieuController : ControllerBase
    {
        private readonly RestaurantCrmContext _context;

        public AdminNguyenLieuController(RestaurantCrmContext context)
        {
            _context = context;
        }

        // GET: api/AdminNguyenLieu - Lấy danh sách tất cả nguyên vật liệu (có lọc theo trạng thái nếu có)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NguyenLieu>>> GetNguyenLieus([FromQuery] string? trangThai = null)
        {
            var query = _context.NguyenLieu.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(nl => nl.TrangThai == trangThai);
            }
            return await query
                .OrderBy(nl => nl.TenNguyenLieu)
                .ToListAsync();
        }

        // GET: api/AdminNguyenLieu/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<NguyenLieu>> GetNguyenLieu(string id)
        {
            var nl = await _context.NguyenLieu.FindAsync(id);
            if (nl == null)
            {
                return NotFound("Không tìm thấy nguyên vật liệu.");
            }
            return nl;
        }

        // POST: api/AdminNguyenLieu - Thêm nguyên vật liệu mới
        [HttpPost]
        public async Task<ActionResult<NguyenLieu>> PostNguyenLieu([FromBody] NguyenLieu nl)
        {
            if (string.IsNullOrWhiteSpace(nl.TenNguyenLieu))
            {
                return BadRequest("Tên nguyên vật liệu không được để trống.");
            }
            if (string.IsNullOrWhiteSpace(nl.DonViTinh))
            {
                return BadRequest("Đơn vị tính không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(nl.MaNguyenLieu))
            {
                nl.MaNguyenLieu = await Helpers.CodeGenerator.GenerateMaNguyenLieuAsync(_context);
            }

            if (string.IsNullOrWhiteSpace(nl.TrangThai))
            {
                nl.TrangThai = "Active";
            }

            if (!nl.SoLuongTon.HasValue)
            {
                nl.SoLuongTon = 0;
            }

            if (!nl.GiaNhapGanNhat.HasValue)
            {
                nl.GiaNhapGanNhat = 0;
            }

            _context.NguyenLieu.Add(nl);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (_context.NguyenLieu.Any(e => e.MaNguyenLieu == nl.MaNguyenLieu))
                {
                    return Conflict("Mã nguyên liệu đã tồn tại.");
                }
                throw;
            }

            return CreatedAtAction(nameof(GetNguyenLieu), new { id = nl.MaNguyenLieu }, nl);
        }

        // PUT: api/AdminNguyenLieu/{id} - Sửa thông tin nguyên liệu
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNguyenLieu(string id, [FromBody] NguyenLieu nl)
        {
            if (id != nl.MaNguyenLieu)
            {
                return BadRequest("Mã nguyên liệu không khớp.");
            }

            var existing = await _context.NguyenLieu.FindAsync(id);
            if (existing == null)
            {
                return NotFound("Không tìm thấy nguyên vật liệu.");
            }

            if (!string.IsNullOrWhiteSpace(nl.TenNguyenLieu))
            {
                existing.TenNguyenLieu = nl.TenNguyenLieu.Trim();
            }
            if (!string.IsNullOrWhiteSpace(nl.DonViTinh))
            {
                existing.DonViTinh = nl.DonViTinh.Trim();
            }
            if (!string.IsNullOrWhiteSpace(nl.TrangThai))
            {
                existing.TrangThai = nl.TrangThai;
            }
            if (nl.GiaNhapGanNhat.HasValue)
            {
                existing.GiaNhapGanNhat = nl.GiaNhapGanNhat.Value;
            }

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        // DELETE: api/AdminNguyenLieu/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNguyenLieu(string id)
        {
            var nl = await _context.NguyenLieu
                .Include(n => n.DinhLuong)
                .Include(n => n.ChiTietPhieuNhap)
                .FirstOrDefaultAsync(n => n.MaNguyenLieu == id);

            if (nl == null)
            {
                return NotFound("Không tìm thấy nguyên vật liệu.");
            }

            if (nl.DinhLuong.Any())
            {
                return BadRequest("Không thể xóa nguyên liệu này vì đang được dùng trong công thức món ăn.");
            }

            if (nl.ChiTietPhieuNhap.Any())
            {
                return BadRequest("Không thể xóa nguyên liệu này vì đã có lịch sử nhập kho.");
            }

            _context.NguyenLieu.Remove(nl);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Đã xóa nguyên liệu thành công." });
        }
    }
}

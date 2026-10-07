using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.Models;
using RestaurantCRM.API.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminKhuyenMaiController : ControllerBase
    {
        private readonly RestaurantCrmContext _context;

        public AdminKhuyenMaiController(RestaurantCrmContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var khuyenMais = await _context.ChuongTrinhKhuyenMai
                .OrderByDescending(k => k.NgayBatDau)
                .Select(k => new
                {
                    k.MaChuongTrinh,
                    k.TenChuongTrinh,
                    k.NgayBatDau,
                    k.NgayKetThuc,
                    TrangThai = "Active" // Fake status as DB removed it
                })
                .ToListAsync();

            return Ok(khuyenMais);
        }

        [HttpGet("LoaiKhuyenMai")]
        public IActionResult GetLoaiKhuyenMai()
        {
            return Ok(new[] { "Voucher", "GiamGiaMon" });
        }

        [HttpGet("LoaiGiam")]
        public IActionResult GetLoaiGiam()
        {
            return Ok(new[] { "%", "VND" });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var khuyenMai = await _context.ChuongTrinhKhuyenMai
                .Include(k => k.KmTheoSp)
                .Include(k => k.KmTheoVoucher)
                .FirstOrDefaultAsync(k => k.MaChuongTrinh == id);

            if (khuyenMai == null) return NotFound("Không tìm thấy chương trình khuyến mãi.");

            return Ok(new
            {
                khuyenMai.MaChuongTrinh,
                khuyenMai.TenChuongTrinh,
                khuyenMai.NgayBatDau,
                khuyenMai.NgayKetThuc,
                TrangThai = "Active",
                DanhSachMon = khuyenMai.KmTheoSp.Select(c => new
                {
                    c.MaKmsp,
                    c.MaMon,
                    c.PhanTramGiam,
                    c.TienGiam
                })
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(KhuyenMaiCreateRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var khuyenMai = new ChuongTrinhKhuyenMai
                {
                    MaChuongTrinh = "KM" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    TenChuongTrinh = request.TenChuongTrinh,
                    NgayBatDau = request.NgayBatDau,
                    NgayKetThuc = request.NgayKetThuc
                };

                _context.ChuongTrinhKhuyenMai.Add(khuyenMai);

                if (request.LoaiKhuyenMai == "Voucher")
                {
                    var voucher = new KmTheoVoucher
                    {
                        MaKmvoucher = "VCH" + Guid.NewGuid().ToString().Substring(0, 7).ToUpper(),
                        MaChuongTrinh = khuyenMai.MaChuongTrinh,
                        MaVoucher = khuyenMai.MaChuongTrinh,
                        GiaTriDonToiThieu = request.GiaTriDonToiThieu,
                        PhanTramGiam = request.LoaiGiam == "%" ? request.GiaTriGiam : null,
                        TienGiam = request.LoaiGiam == "VND" ? request.GiaTriGiam : null
                    };
                    _context.KmTheoVoucher.Add(voucher);
                }
                else if (request.LoaiKhuyenMai == "GiamGiaMon" && request.DanhSachMon != null)
                {
                    foreach (var item in request.DanhSachMon)
                    {
                        var chiTiet = new KmTheoSp
                        {
                            MaKmsp = "CTKM" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper(),
                            MaChuongTrinh = khuyenMai.MaChuongTrinh,
                            MaMon = item.MaMon,
                            PhanTramGiam = request.LoaiGiam == "%" ? request.GiaTriGiam : null,
                            TienGiam = request.LoaiGiam == "VND" ? request.GiaTriGiam : null
                        };
                        _context.KmTheoSp.Add(chiTiet);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetById), new { id = khuyenMai.MaChuongTrinh }, new { message = "Thêm khuyến mãi thành công.", MaChuongTrinh = khuyenMai.MaChuongTrinh });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi hệ thống: " + ex.Message);
            }
        }

        [HttpPut("{id}/Status")]
        public async Task<IActionResult> UpdateStatus(string id, KhuyenMaiStatusRequest request)
        {
            return Ok($"Đã cập nhật trạng thái khuyến mãi (Mock - Schema removed status).");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var khuyenMai = await _context.ChuongTrinhKhuyenMai
                .Include(k => k.KmTheoSp)
                .Include(k => k.KmTheoVoucher)
                    .ThenInclude(v => v.HoaDon)
                .FirstOrDefaultAsync(k => k.MaChuongTrinh == id);

            if (khuyenMai == null) return NotFound("Không tìm thấy chương trình khuyến mãi.");

            if (khuyenMai.KmTheoVoucher.Any(v => v.HoaDon.Any()))
            {
                return BadRequest("Chương trình khuyến mãi này đã được sử dụng trong Đơn hàng. Không thể xóa.");
            }

            if (khuyenMai.KmTheoSp.Any()) _context.KmTheoSp.RemoveRange(khuyenMai.KmTheoSp);
            if (khuyenMai.KmTheoVoucher.Any()) _context.KmTheoVoucher.RemoveRange(khuyenMai.KmTheoVoucher);

            _context.ChuongTrinhKhuyenMai.Remove(khuyenMai);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

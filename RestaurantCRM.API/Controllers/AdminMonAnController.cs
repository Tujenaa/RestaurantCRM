using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Data;
using RestaurantCRM.API.DTOs;
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

        // GET: api/AdminMonAn - Lấy danh sách món ăn kèm định lượng, giá vốn ước tính và số suất khả dụng
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetMonAns()
        {
            var dsMonAn = await _context.MonAn
                .Include(m => m.MaLoaiMonNavigation)
                .Include(m => m.DinhLuong)
                    .ThenInclude(dl => dl.MaNguyenLieuNavigation)
                .AsNoTracking()
                .ToListAsync();

            var result = dsMonAn.Select(m =>
            {
                var dinhLuongList = m.DinhLuong.Select(dl => new DinhLuongDetailDto
                {
                    MaDinhLuong = dl.MaDinhLuong,
                    MaNguyenLieu = dl.MaNguyenLieu,
                    TenNguyenLieu = dl.MaNguyenLieuNavigation?.TenNguyenLieu ?? "",
                    DonViTinh = dl.MaNguyenLieuNavigation?.DonViTinh ?? "",
                    SoLuongCan = dl.SoLuongCan,
                    GiaNhapGanNhat = dl.MaNguyenLieuNavigation?.GiaNhapGanNhat ?? 0
                }).ToList();

                // Tính giá vốn ước tính = SUM(soLuongCan * giaNhapGanNhat)
                double giaVonUocTinh = dinhLuongList.Sum(dl => dl.ThanhTienGiaVon);

                // Tính số suất có thể phục vụ từ tồn kho nguyên liệu hiện có: Min(Floor(TonKho / SoLuongCan))
                int soSuatKhaDung = 0;
                if (m.DinhLuong.Any())
                {
                    soSuatKhaDung = (int)m.DinhLuong.Min(dl =>
                    {
                        var ton = dl.MaNguyenLieuNavigation?.SoLuongTon ?? 0;
                        if (dl.SoLuongCan <= 0) return 0;
                        return Math.Floor(ton / dl.SoLuongCan);
                    });
                    if (soSuatKhaDung < 0) soSuatKhaDung = 0;
                }

                return new
                {
                    m.MaMon,
                    m.TenMon,
                    m.MaLoaiMon,
                    TenLoaiMon = m.MaLoaiMonNavigation?.TenLoaiMon,
                    m.DonGia,
                    m.MoTa,
                    m.DuongDanAnh,
                    m.TrangThai,
                    GiaVonUocTinh = giaVonUocTinh,
                    SoSuatKhaDung = soSuatKhaDung,
                    // Giữ trường SoLuong để tương thích ngược với WinForms/Web
                    SoLuong = soSuatKhaDung,
                    DinhLuong = dinhLuongList
                };
            }).ToList();

            return Ok(result);
        }

        // GET: api/AdminMonAn/{id} - Xem chi tiết 1 món ăn cùng công thức định lượng (BOM)
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetMonAn(string id)
        {
            var monAn = await _context.MonAn
                .Include(m => m.MaLoaiMonNavigation)
                .Include(m => m.DinhLuong)
                    .ThenInclude(dl => dl.MaNguyenLieuNavigation)
                .FirstOrDefaultAsync(m => m.MaMon == id);

            if (monAn == null)
            {
                return NotFound("Không tìm thấy món ăn.");
            }

            var dinhLuongList = monAn.DinhLuong.Select(dl => new DinhLuongDetailDto
            {
                MaDinhLuong = dl.MaDinhLuong,
                MaNguyenLieu = dl.MaNguyenLieu,
                TenNguyenLieu = dl.MaNguyenLieuNavigation?.TenNguyenLieu ?? "",
                DonViTinh = dl.MaNguyenLieuNavigation?.DonViTinh ?? "",
                SoLuongCan = dl.SoLuongCan,
                GiaNhapGanNhat = dl.MaNguyenLieuNavigation?.GiaNhapGanNhat ?? 0
            }).ToList();

            double giaVonUocTinh = dinhLuongList.Sum(dl => dl.ThanhTienGiaVon);

            int soSuatKhaDung = 0;
            if (monAn.DinhLuong.Any())
            {
                soSuatKhaDung = (int)monAn.DinhLuong.Min(dl =>
                {
                    var ton = dl.MaNguyenLieuNavigation?.SoLuongTon ?? 0;
                    if (dl.SoLuongCan <= 0) return 0;
                    return Math.Floor(ton / dl.SoLuongCan);
                });
                if (soSuatKhaDung < 0) soSuatKhaDung = 0;
            }

            return Ok(new
            {
                monAn.MaMon,
                monAn.TenMon,
                monAn.MaLoaiMon,
                TenLoaiMon = monAn.MaLoaiMonNavigation?.TenLoaiMon,
                monAn.DonGia,
                monAn.MoTa,
                monAn.DuongDanAnh,
                monAn.TrangThai,
                GiaVonUocTinh = giaVonUocTinh,
                SoSuatKhaDung = soSuatKhaDung,
                SoLuong = soSuatKhaDung,
                DinhLuong = dinhLuongList
            });
        }

        // POST: api/AdminMonAn - Thêm món ăn mới kèm bảng định lượng (Database Transaction)
        [HttpPost]
        public async Task<IActionResult> PostMonAn([FromBody] TaoMonAnRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TenMon))
            {
                return BadRequest("Tên món ăn không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(request.MaLoaiMon))
            {
                return BadRequest("Vui lòng chọn loại món ăn.");
            }

            if (request.DonGia < 0)
            {
                return BadRequest("Đơn giá món ăn không hợp lệ.");
            }

            // RÀNG BUỘC BẮT BUỘC: Món ăn phải có ít nhất 1 định lượng nguyên liệu
            if (request.DinhLuong == null || !request.DinhLuong.Any())
            {
                return BadRequest("Món ăn bắt buộc phải có ít nhất 1 nguyên vật liệu định lượng.");
            }

            // Kiểm tra tính hợp lệ của từng nguyên liệu
            var dsMaNl = request.DinhLuong.Select(d => d.MaNguyenLieu).Distinct().ToList();
            var countExistingNl = await _context.NguyenLieu.CountAsync(nl => dsMaNl.Contains(nl.MaNguyenLieu));
            if (countExistingNl != dsMaNl.Count)
            {
                return BadRequest("Danh sách định lượng chứa mã nguyên liệu không tồn tại trong hệ thống.");
            }

            foreach (var item in request.DinhLuong)
            {
                if (item.SoLuongCan <= 0)
                {
                    return BadRequest($"Số lượng tiêu hao cho nguyên liệu '{item.MaNguyenLieu}' phải lớn hơn 0.");
                }
            }

            // Mở Database Transaction để đảm bảo tính toàn vẹn
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tạo Món ăn mới
                var maMon = await Helpers.CodeGenerator.GenerateMaMonAnAsync(_context);

                string trangThai = "InStock";
                if (!string.IsNullOrWhiteSpace(request.TrangThai))
                {
                    trangThai = (request.TrangThai == "Discontinued" || request.TrangThai == "Inactive")
                        ? "Discontinued"
                        : (request.TrangThai == "OutOfStock" ? "OutOfStock" : "InStock");
                }

                var monAn = new MonAn
                {
                    MaMon = maMon,
                    TenMon = request.TenMon.Trim(),
                    MaLoaiMon = request.MaLoaiMon,
                    DonGia = request.DonGia,
                    MoTa = request.MoTa,
                    DuongDanAnh = request.DuongDanAnh,
                    TrangThai = trangThai
                };

                _context.MonAn.Add(monAn);
                await _context.SaveChangesAsync();

                // 2. Thêm các bản ghi Định lượng
                int stt = 1;
                foreach (var item in request.DinhLuong)
                {
                    var maDl = await Helpers.CodeGenerator.GenerateMaDinhLuongAsync(_context);
                    var dinhLuong = new DinhLuong
                    {
                        MaDinhLuong = maDl,
                        MaMon = maMon,
                        MaNguyenLieu = item.MaNguyenLieu,
                        SoLuongCan = item.SoLuongCan
                    };
                    _context.DinhLuong.Add(dinhLuong);
                    await _context.SaveChangesAsync(); // Lưu từng dòng để đảm bảo sinh mã duy nhất
                    stt++;
                }

                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetMonAn), new { id = maMon }, new
                {
                    Message = "Thêm món ăn và công thức định lượng thành công.",
                    MaMon = maMon,
                    TenMon = monAn.TenMon
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Lỗi hệ thống khi lưu món ăn: {ex.Message}");
            }
        }

        // PUT: api/AdminMonAn/{id} - Sửa món ăn & cập nhật công thức định lượng (Database Transaction)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMonAn(string id, [FromBody] CapNhatMonAnRequest request)
        {
            var monAn = await _context.MonAn
                .Include(m => m.DinhLuong)
                .FirstOrDefaultAsync(m => m.MaMon == id);

            if (monAn == null)
            {
                return NotFound("Không tìm thấy món ăn để cập nhật.");
            }

            if (string.IsNullOrWhiteSpace(request.TenMon))
            {
                return BadRequest("Tên món ăn không được để trống.");
            }

            if (request.DonGia < 0)
            {
                return BadRequest("Đơn giá không hợp lệ.");
            }

            // Nếu người dùng gửi kèm danh sách định lượng mới, bắt buộc phải có ít nhất 1 dòng
            if (request.DinhLuong != null && !request.DinhLuong.Any())
            {
                return BadRequest("Món ăn bắt buộc phải có ít nhất 1 nguyên vật liệu định lượng.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Cập nhật thông tin cơ bản món ăn
                monAn.TenMon = request.TenMon.Trim();
                monAn.DonGia = request.DonGia;
                if (!string.IsNullOrWhiteSpace(request.MaLoaiMon))
                {
                    monAn.MaLoaiMon = request.MaLoaiMon;
                }
                if (request.DuongDanAnh != null)
                {
                    monAn.DuongDanAnh = request.DuongDanAnh;
                }
                if (request.MoTa != null)
                {
                    monAn.MoTa = request.MoTa;
                }

                if (!string.IsNullOrWhiteSpace(request.TrangThai))
                {
                    monAn.TrangThai = (request.TrangThai == "Discontinued" || request.TrangThai == "Inactive")
                        ? "Discontinued"
                        : (request.TrangThai == "OutOfStock" ? "OutOfStock" : "InStock");
                }

                // 2. Cập nhật danh sách định lượng (nếu client gửi danh sách mới)
                if (request.DinhLuong != null && request.DinhLuong.Any())
                {
                    // Validate các mã nguyên liệu
                    var dsMaNl = request.DinhLuong.Select(d => d.MaNguyenLieu).Distinct().ToList();
                    var countExisting = await _context.NguyenLieu.CountAsync(nl => dsMaNl.Contains(nl.MaNguyenLieu));
                    if (countExisting != dsMaNl.Count)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest("Danh sách định lượng chứa nguyên liệu không hợp lệ.");
                    }

                    // Xóa các dòng định lượng cũ
                    _context.DinhLuong.RemoveRange(monAn.DinhLuong);
                    await _context.SaveChangesAsync();

                    // Thêm lại các dòng định lượng mới
                    foreach (var item in request.DinhLuong)
                    {
                        var maDl = await Helpers.CodeGenerator.GenerateMaDinhLuongAsync(_context);
                        _context.DinhLuong.Add(new DinhLuong
                        {
                            MaDinhLuong = maDl,
                            MaMon = id,
                            MaNguyenLieu = item.MaNguyenLieu,
                            SoLuongCan = item.SoLuongCan
                        });
                        await _context.SaveChangesAsync();
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Message = "Cập nhật món ăn và định lượng thành công." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Lỗi khi cập nhật món ăn: {ex.Message}");
            }
        }

        // DELETE: api/AdminMonAn/{id} - Xóa món ăn (kiểm tra ràng buộc đơn hàng)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMonAn(string id)
        {
            var monAn = await _context.MonAn
                .Include(m => m.ChiTietHoaDon)
                .Include(m => m.DinhLuong)
                .FirstOrDefaultAsync(m => m.MaMon == id);

            if (monAn == null)
            {
                return NotFound("Không tìm thấy món ăn để xóa.");
            }

            // Nếu món đã từng có trong hóa đơn bán, không cho xóa cứng để bảo vệ dữ liệu kế toán
            if (monAn.ChiTietHoaDon.Any())
            {
                return BadRequest("Không thể xóa món ăn này vì đã phát sinh hóa đơn bán hàng. Vui lòng chuyển trạng thái sang 'Ngừng kinh doanh' (Discontinued).");
            }

            // Khóa ngoại ON DELETE CASCADE sẽ tự động xóa các dòng DINH_LUONG liên quan
            _context.MonAn.Remove(monAn);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đã xóa món ăn thành công." });
        }
    }
}

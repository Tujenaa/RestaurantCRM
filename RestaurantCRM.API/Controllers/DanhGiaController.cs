using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DanhGiaController : ControllerBase
    {
        private readonly RestaurantCRM.BusinessLogic.Services.IRestaurantService _service;

        public DanhGiaController(RestaurantCRM.BusinessLogic.Services.IRestaurantService service)
        {
            _service = service;
        }

        // GET: api/DanhGia/MonAn/{maMon} - Lấy danh sách đánh giá của một món ăn
        [HttpGet("MonAn/{maMon}")]
        public async Task<IActionResult> GetDanhGiaByMonAn(string maMon)
        {
            // 1. Lấy từ bảng CHI_TIET_DANH_GIA (chuẩn Master-Detail)
            var ctReviews = await _service.Query<ChiTietDanhGia>()
                .Where(ct => ct.MaMon == maMon && ct.MaDanhGiaNavigation.TrangThai)
                .Include(ct => ct.MaDanhGiaNavigation)
                    .ThenInclude(d => d.MaKhachHangNavigation)
                .Include(ct => ct.MaDanhGiaNavigation)
                    .ThenInclude(d => d.TraLoiDanhGia)
                        .ThenInclude(t => t.MaNhanVienNavigation)
                .Select(ct => new
                {
                    MaDanhGia = ct.MaChiTietDanhGia,
                    SoSao = (int?)ct.SoSao,
                    NoiDung = ct.NoiDung,
                    NgayDanhGia = ct.MaDanhGiaNavigation.NgayDanhGia,
                    TenKhachHang = ct.MaDanhGiaNavigation.MaKhachHangNavigation != null 
                        ? ct.MaDanhGiaNavigation.MaKhachHangNavigation.HoTen 
                        : "Khách ẩn danh",
                    HoiThoai = ct.MaDanhGiaNavigation.TraLoiDanhGia.OrderBy(t => t.NgayGui).Select(t => new
                    {
                        t.MaTraLoi,
                        t.NoiDung,
                        t.NgayGui,
                        NguoiTraLoi = t.MaNhanVienNavigation.HoTen
                    })
                })
                .ToListAsync();

            // 2. Lấy thêm từ bảng DANH_GIA cũ (nếu có bản ghi legacy)
            var legacyReviews = await _service.Query<DanhGia>()
                .Where(d => d.MaMon == maMon && d.TrangThai)
                .Include(d => d.MaKhachHangNavigation)
                .Include(d => d.TraLoiDanhGia)
                    .ThenInclude(t => t.MaNhanVienNavigation)
                .Select(d => new
                {
                    MaDanhGia = d.MaDanhGia,
                    SoSao = d.SoSao,
                    NoiDung = d.NoiDung,
                    NgayDanhGia = d.NgayDanhGia,
                    TenKhachHang = d.MaKhachHangNavigation != null ? d.MaKhachHangNavigation.HoTen : "Khách ẩn danh",
                    HoiThoai = d.TraLoiDanhGia.OrderBy(t => t.NgayGui).Select(t => new
                    {
                        t.MaTraLoi,
                        t.NoiDung,
                        t.NgayGui,
                        NguoiTraLoi = t.MaNhanVienNavigation.HoTen
                    })
                })
                .ToListAsync();

            var combined = ctReviews.Concat(legacyReviews)
                .OrderByDescending(x => x.NgayDanhGia)
                .ToList();

            return Ok(combined);
        }

        // GET: api/DanhGia/KhachHang/{maKhachHang} - Lấy lịch sử đánh giá của riêng một khách hàng
        [HttpGet("KhachHang/{maKhachHang}")]
        public async Task<IActionResult> GetDanhGiaByKhachHang(string maKhachHang)
        {
            var danhGias = await _service.Query<DanhGia>()
                .Where(d => d.MaKhachHang == maKhachHang)
                .Where(d => d.TrangThai)
                .Include(d => d.MaKhachHangNavigation)
                .Include(d => d.MaMonNavigation)
                .Include(d => d.TraLoiDanhGia)
                .OrderByDescending(d => d.NgayDanhGia)
                .Select(d => new
                {
                    d.MaDanhGia,
                    d.MaMon,
                    TenMon = d.MaMon != null && d.MaMonNavigation != null 
                        ? d.MaMonNavigation.TenMon 
                        : (d.MaMon != null ? "Món ăn Lẩu Phố" : "Đánh giá chung đơn hàng"),
                    d.MaHoaDon,
                    d.SoSao,
                    d.NoiDung,
                    d.NgayDanhGia,
                    TenKhachHang = d.MaKhachHangNavigation != null ? d.MaKhachHangNavigation.HoTen : "Khách hàng",
                    HoiThoai = d.TraLoiDanhGia.OrderByDescending(t => t.NgayGui).Select(t => new
                    {
                        t.MaTraLoi,
                        t.NoiDung,
                        t.NgayGui,
                        NguoiTraLoi = t.MaNhanVienNavigation.HoTen
                    })
                })
                .ToListAsync();

            return Ok(danhGias);
        }

        // GET: api/DanhGia/DaDanhGia/{maKhachHang} - Danh sách mã đơn hàng mà khách đã đánh giá
        [HttpGet("DaDanhGia/{maKhachHang}")]
        public async Task<IActionResult> GetDonHangDaDanhGia(string maKhachHang)
        {
            var ordersReviewed = await _service.Query<DanhGia>()
                .Where(d => d.MaKhachHang == maKhachHang && !string.IsNullOrEmpty(d.MaHoaDon))
                .Select(d => d.MaHoaDon!)
                .Distinct()
                .ToListAsync();
            return Ok(ordersReviewed);
        }

        // GET: api/DanhGia/KiemTraDonHang/{maHoaDon} - Kiểm tra đơn hàng cụ thể đã có đánh giá chưa
        [HttpGet("KiemTraDonHang/{maHoaDon}")]
        public async Task<IActionResult> KiemTraDonHang(string maHoaDon)
        {
            bool daDanhGia = await _service.Query<DanhGia>().AnyAsync(d => d.MaHoaDon == maHoaDon);
            return Ok(new { maHoaDon, daDanhGia });
        }

        // GET: api/DanhGia/DonHang/{maHoaDon} - Lấy chi tiết toàn bộ đánh giá của 1 đơn hàng (Master & Detail)
        [HttpGet("DonHang/{maHoaDon}")]
        public async Task<IActionResult> GetOrderReviewDetail(string maHoaDon)
        {
            if (string.IsNullOrWhiteSpace(maHoaDon))
            {
                return BadRequest("Mã đơn hàng không hợp lệ.");
            }

            var dg = await _service.Query<DanhGia>()
                .Where(d => d.MaHoaDon == maHoaDon)
                .Include(d => d.MaKhachHangNavigation)
                .Include(d => d.ChiTietDanhGia)
                    .ThenInclude(ct => ct.MaMonNavigation)
                .Include(d => d.TraLoiDanhGia)
                    .ThenInclude(t => t.MaNhanVienNavigation)
                .FirstOrDefaultAsync();

            if (dg == null)
            {
                return NotFound("Chưa có đánh giá nào cho đơn hàng này.");
            }

            var dto = new OrderReviewDetailDto
            {
                MaDanhGia = dg.MaDanhGia,
                MaHoaDon = dg.MaHoaDon ?? maHoaDon,
                MaKhachHang = dg.MaKhachHang ?? "",
                TenKhachHang = dg.MaKhachHangNavigation?.HoTen ?? "Khách hàng",
                SoSaoChung = dg.SoSao ?? 5,
                NoiDungChung = dg.NoiDung,
                NgayDanhGia = dg.NgayDanhGia,
                ChiTietMon = dg.ChiTietDanhGia.Select(ct => new DishReviewDetailItemDto
                {
                    MaChiTietDanhGia = ct.MaChiTietDanhGia,
                    MaMon = ct.MaMon,
                    TenMon = ct.MaMonNavigation?.TenMon ?? ct.MaMon,
                    DuongDanAnh = ct.MaMonNavigation?.DuongDanAnh,
                    SoSao = ct.SoSao,
                    NoiDung = ct.NoiDung
                }).ToList(),
                HoiThoai = dg.TraLoiDanhGia.OrderBy(t => t.NgayGui).Select(t => new ReviewReplyDetailDto
                {
                    MaTraLoi = t.MaTraLoi,
                    NoiDung = t.NoiDung,
                    NgayGui = t.NgayGui,
                    NguoiTraLoi = t.MaNhanVienNavigation?.HoTen ?? "Nhân viên Lẩu Phố"
                }).ToList()
            };

            return Ok(dto);
        }

        // POST: api/DanhGia/DonHang - Gửi đánh giá toàn bộ đơn hàng (chuẩn Master-Detail)
        [HttpPost("DonHang")]
        public async Task<IActionResult> CreateOrderReviewBatch(OrderReviewBatchRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MaHoaDon))
            {
                return BadRequest("Vui lòng cung cấp mã đơn hàng.");
            }

            if (request.SoSaoChung < 1 || request.SoSaoChung > 5)
            {
                return BadRequest("Số sao đánh giá chung phải từ 1 đến 5.");
            }

            // Kiểm tra xem đơn hàng này đã từng được đánh giá chưa
            bool daDanhGia = await _service.Query<DanhGia>().AnyAsync(d => d.MaHoaDon == request.MaHoaDon);
            if (daDanhGia)
            {
                return BadRequest("Đơn hàng này đã được đánh giá trước đó.");
            }

            // 1. Bản ghi Master: Đánh giá chung toàn đơn hàng
            var dgChung = new DanhGia
            {
                MaDanhGia = "DG" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                MaKhachHang = request.MaKhachHang,
                MaHoaDon = request.MaHoaDon,
                MaMon = null,
                SoSao = request.SoSaoChung,
                NoiDung = string.IsNullOrWhiteSpace(request.NoiDungChung) ? null : request.NoiDungChung.Trim(),
                NgayDanhGia = DateTime.Now,
                TrangThai = true,
                DaXuLy = false
            };
            _service.Add(dgChung);

            // 2. Các bản ghi Detail: CHI_TIET_DANH_GIA cho từng món
            if (request.ChiTietMon != null && request.ChiTietMon.Count > 0)
            {
                var distinctDishes = request.ChiTietMon
                    .Where(m => !string.IsNullOrWhiteSpace(m.MaMon))
                    .GroupBy(m => m.MaMon)
                    .Select(g => g.First());

                foreach (var item in distinctDishes)
                {
                    int soSaoMon = item.SoSao >= 1 && item.SoSao <= 5 ? item.SoSao : request.SoSaoChung;
                    string? noiDungMon = !string.IsNullOrWhiteSpace(item.NoiDung) ? item.NoiDung.Trim() : null;

                    var ctMon = new ChiTietDanhGia
                    {
                        MaChiTietDanhGia = "CT" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                        MaDanhGia = dgChung.MaDanhGia,
                        MaMon = item.MaMon,
                        SoSao = soSaoMon,
                        NoiDung = noiDungMon
                    };
                    _service.Add(ctMon);
                }
            }

            await _service.SaveChangesAsync();

            return Ok(new 
            { 
                message = "Đánh giá đơn hàng thành công", 
                maDanhGia = dgChung.MaDanhGia
            });
        }

        // PUT: api/DanhGia/DonHang - Chỉnh sửa đánh giá của đơn hàng (Master & Detail)
        [HttpPut("DonHang")]
        public async Task<IActionResult> UpdateOrderReviewBatch(UpdateOrderReviewRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MaHoaDon))
            {
                return BadRequest("Vui lòng cung cấp mã đơn hàng.");
            }

            if (request.SoSaoChung < 1 || request.SoSaoChung > 5)
            {
                return BadRequest("Số sao đánh giá chung phải từ 1 đến 5.");
            }

            var dg = await _service.Query<DanhGia>()
                .Where(d => d.MaHoaDon == request.MaHoaDon)
                .Include(d => d.ChiTietDanhGia)
                .FirstOrDefaultAsync();

            if (dg == null)
            {
                return NotFound("Không tìm thấy đánh giá của đơn hàng này để cập nhật.");
            }

            // Kiểm tra quyền sở hữu
            if (!string.IsNullOrWhiteSpace(request.MaKhachHang) && !string.IsNullOrWhiteSpace(dg.MaKhachHang)
                && !string.Equals(dg.MaKhachHang, request.MaKhachHang, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid("Bạn không có quyền chỉnh sửa đánh giá này.");
            }

            // 1. Cập nhật bản ghi Master
            dg.SoSao = request.SoSaoChung;
            dg.NoiDung = string.IsNullOrWhiteSpace(request.NoiDungChung) ? null : request.NoiDungChung.Trim();
            dg.NgayDanhGia = DateTime.Now;

            // 2. Cập nhật các món Detail
            if (request.ChiTietMon != null)
            {
                foreach (var item in request.ChiTietMon)
                {
                    if (string.IsNullOrWhiteSpace(item.MaMon)) continue;

                    var existingCt = dg.ChiTietDanhGia.FirstOrDefault(ct => ct.MaMon == item.MaMon);
                    int soSaoMon = item.SoSao >= 1 && item.SoSao <= 5 ? item.SoSao : request.SoSaoChung;
                    string? noiDungMon = !string.IsNullOrWhiteSpace(item.NoiDung) ? item.NoiDung.Trim() : null;

                    if (existingCt != null)
                    {
                        existingCt.SoSao = soSaoMon;
                        existingCt.NoiDung = noiDungMon;
                    }
                    else
                    {
                        var newCt = new ChiTietDanhGia
                        {
                            MaChiTietDanhGia = "CT" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                            MaDanhGia = dg.MaDanhGia,
                            MaMon = item.MaMon,
                            SoSao = soSaoMon,
                            NoiDung = noiDungMon
                        };
                        _service.Add(newCt);
                    }
                }
            }

            await _service.SaveChangesAsync();

            return Ok(new { message = "Cập nhật đánh giá thành công" });
        }

        // POST: api/DanhGia - Gửi đánh giá món ăn lẻ
        [HttpPost]
        public async Task<IActionResult> CreateDanhGia(ReviewRequest request)
        {
            if (request.SoSao < 1 || request.SoSao > 5)
            {
                return BadRequest("Số sao phải từ 1 đến 5.");
            }

            if (!string.IsNullOrWhiteSpace(request.MaHoaDon))
            {
                bool daTonTai = await _service.Query<DanhGia>().AnyAsync(d => 
                    d.MaHoaDon == request.MaHoaDon && d.MaMon == request.MaMon);
                if (daTonTai)
                {
                    return BadRequest("Món ăn này trong đơn hàng đã được gửi đánh giá trước đó.");
                }
            }

            var danhGia = new DanhGia
            {
                MaDanhGia = "DG" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                MaKhachHang = request.MaKhachHang,
                MaMon = request.MaMon,
                MaHoaDon = request.MaHoaDon,
                SoSao = request.SoSao,
                NoiDung = request.NoiDung,
                NgayDanhGia = DateTime.Now,
                TrangThai = true,
                DaXuLy = false
            };

            _service.Add(danhGia);
            await _service.SaveChangesAsync();

            return Ok(new { message = "Đánh giá thành công", maDanhGia = danhGia.MaDanhGia });
        }
    }
}

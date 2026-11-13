using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.API.DTOs;
using RestaurantCRM.API.Services;

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
        // Chỉ lấy lượt khách chấm riêng món (có số sao) hoặc có nhận xét món, của đánh giá đang hiển thị.
        [HttpGet("MonAn/{maMon}")]
        public async Task<IActionResult> GetDanhGiaByMonAn(string maMon)
        {
            var ctReviews = await _service.Query<ChiTietDanhGia>()
                .Where(ct => ct.MaMon == maMon && ct.MaDanhGiaNavigation.TrangThai)
                .Where(ct => ct.SoSao != null || (ct.NoiDung != null && ct.NoiDung.Trim() != ""))
                .OrderByDescending(ct => ct.MaDanhGiaNavigation.NgayDanhGia)
                .Select(ct => new
                {
                    MaDanhGia = ct.MaChiTietDanhGia,
                    ct.SoSao,
                    ct.NoiDung,
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

            return Ok(ctReviews);
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
        // Sao chung bắt buộc; chấm từng món là tùy chọn. Mỗi món trong đơn có đúng 1 dòng CHI_TIET_DANH_GIA,
        // món khách không đánh giá riêng có SoSao = NULL (không lấy sao chung gán cho món).
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

            var hoaDon = await _service.Query<HoaDon>()
                .Where(h => h.MaHoaDon == request.MaHoaDon)
                .Select(h => new { h.MaKhachHang, h.TrangThai })
                .FirstOrDefaultAsync();
            if (hoaDon == null)
            {
                return BadRequest("Không tìm thấy đơn hàng.");
            }
            if (hoaDon.TrangThai != "Completed")
            {
                return BadRequest("Chỉ được đánh giá đơn hàng đã hoàn tất.");
            }
            if (string.IsNullOrWhiteSpace(request.MaKhachHang)
                || !string.Equals(hoaDon.MaKhachHang, request.MaKhachHang, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Đơn hàng này không thuộc tài khoản của bạn.");
            }

            var loiKhach = await KhachHangValidator.KiemTraHoatDongAsync(_service, request.MaKhachHang);
            if (loiKhach != null) return BadRequest(loiKhach);

            // Kiểm tra xem đơn hàng này đã từng được đánh giá chưa
            bool daDanhGia = await _service.Query<DanhGia>().AnyAsync(d => d.MaHoaDon == request.MaHoaDon);
            if (daDanhGia)
            {
                return BadRequest("Đơn hàng này đã được đánh giá trước đó.");
            }

            var monTrongDon = await LayMonTrongDonAsync(request.MaHoaDon);
            if (monTrongDon.Count == 0)
            {
                return BadRequest("Đơn hàng không có món để đánh giá.");
            }

            // 1. Bản ghi Master: Đánh giá chung toàn đơn hàng (không gắn món)
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

            // 2. Các bản ghi Detail: 1 dòng cho mỗi món trong đơn. Món gửi lên không thuộc đơn bị bỏ qua.
            var chamRieng = GomMonChamRieng(request.ChiTietMon);
            foreach (var maMon in monTrongDon)
            {
                var (soSao, noiDung) = chamRieng.TryGetValue(maMon, out var item)
                    ? TinhDiemMon(item, request.SoSaoChung)
                    : (null, null);

                _service.Add(new ChiTietDanhGia
                {
                    MaChiTietDanhGia = "CT" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    MaDanhGia = dgChung.MaDanhGia,
                    MaMon = maMon,
                    SoSao = soSao,
                    NoiDung = noiDung
                });
            }

            await _service.SaveChangesAsync();

            return Ok(new
            {
                message = "Đánh giá đơn hàng thành công",
                maDanhGia = dgChung.MaDanhGia
            });
        }

        // PUT: api/DanhGia/DonHang - Chỉnh sửa đánh giá của đơn hàng (Master & Detail)
        // Món có trong ChiTietMon → cập nhật theo lựa chọn của khách; món không gửi kèm → không đánh giá riêng (NULL).
        [HttpPut("DonHang")]
        public async Task<IActionResult> UpdateOrderReviewBatch(UpdateOrderReviewRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MaHoaDon))
            {
                return BadRequest("Vui lòng cung cấp mã đơn hàng.");
            }

            if (string.IsNullOrWhiteSpace(request.MaKhachHang))
            {
                return BadRequest("Thiếu mã khách hàng.");
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
            if (!string.Equals(dg.MaKhachHang, request.MaKhachHang, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Bạn không có quyền chỉnh sửa đánh giá này.");
            }

            var loiKhach = await KhachHangValidator.KiemTraHoatDongAsync(_service, request.MaKhachHang);
            if (loiKhach != null) return BadRequest(loiKhach);

            // 1. Cập nhật bản ghi Master (giữ nguyên ngày đánh giá; khách sửa thì nhân viên cần xem lại)
            dg.SoSao = request.SoSaoChung;
            dg.NoiDung = string.IsNullOrWhiteSpace(request.NoiDungChung) ? null : request.NoiDungChung.Trim();
            if (dg.DaXuLy) dg.DaXuLy = false;

            // 2. Từng dòng chi tiết: món gửi kèm → theo khách chọn; món không gửi kèm → không đánh giá riêng
            var chamRieng = GomMonChamRieng(request.ChiTietMon);
            foreach (var ct in dg.ChiTietDanhGia)
            {
                if (chamRieng.TryGetValue(ct.MaMon, out var item))
                {
                    (ct.SoSao, ct.NoiDung) = TinhDiemMon(item, request.SoSaoChung);
                }
                else
                {
                    ct.SoSao = null;
                    ct.NoiDung = null;
                }
            }

            // 3. Món trong đơn chưa có dòng chi tiết (đánh giá cũ) → bổ sung
            var monTrongDon = await LayMonTrongDonAsync(request.MaHoaDon);
            var daCo = dg.ChiTietDanhGia.Select(ct => ct.MaMon).ToHashSet();
            foreach (var maMon in monTrongDon.Where(m => !daCo.Contains(m)))
            {
                var (soSao, noiDung) = chamRieng.TryGetValue(maMon, out var item)
                    ? TinhDiemMon(item, request.SoSaoChung)
                    : (null, null);

                _service.Add(new ChiTietDanhGia
                {
                    MaChiTietDanhGia = "CT" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    MaDanhGia = dg.MaDanhGia,
                    MaMon = maMon,
                    SoSao = soSao,
                    NoiDung = noiDung
                });
            }

            await _service.SaveChangesAsync();

            return Ok(new { message = "Cập nhật đánh giá thành công" });
        }

        /// <summary>Gom các món khách gửi kèm (bỏ mã rỗng; trùng mã thì lấy dòng đầu).</summary>
        private static Dictionary<string, DishReviewItemDto> GomMonChamRieng(List<DishReviewItemDto>? ds)
        {
            return (ds ?? new List<DishReviewItemDto>())
                .Where(m => !string.IsNullOrWhiteSpace(m.MaMon))
                .GroupBy(m => m.MaMon.Trim())
                .ToDictionary(g => g.Key, g => g.First());
        }

        /// <summary>
        /// Quy tắc chấm món: có sao hợp lệ → dùng sao đó; chỉ có nhận xét → lấy sao chung (khách đã có ý kiến về món);
        /// không có gì → không đánh giá riêng (NULL).
        /// </summary>
        private static (int? SoSao, string? NoiDung) TinhDiemMon(DishReviewItemDto item, int soSaoChung)
        {
            string? noiDung = string.IsNullOrWhiteSpace(item.NoiDung) ? null : item.NoiDung.Trim();
            if (item.SoSao is >= 1 and <= 5) return (item.SoSao, noiDung);
            if (noiDung != null) return (soSaoChung, noiDung);
            return (null, null);
        }

        /// <summary>Danh sách mã món (không trùng) thực sự có trong đơn hàng.</summary>
        private async Task<List<string>> LayMonTrongDonAsync(string maHoaDon)
        {
            return await _service.Query<ChiTietHoaDon>()
                .Where(ct => ct.MaHoaDon == maHoaDon && ct.MaMon != null && ct.SoLuong > 0)
                .Select(ct => ct.MaMon!)
                .Distinct()
                .ToListAsync();
        }
    }
}

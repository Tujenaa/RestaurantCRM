using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;
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
        private readonly RestaurantCRM.BusinessLogic.Services.IRestaurantService _service;

        public AdminKhuyenMaiController(RestaurantCRM.BusinessLogic.Services.IRestaurantService service)
        {
            _service = service;
        }

        // ==============================================================
        // 1. QUẢN LÝ CHƯƠNG TRÌNH KHUYẾN MÃI (CAMPAIGN)
        // ==============================================================

        [HttpGet]
        [HttpGet("ChuongTrinh")]
        public async Task<IActionResult> GetAllChuongTrinh()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var list = await _service.Query<ChuongTrinhKhuyenMai>()
                .Include(k => k.KmTheoVoucher)
                .Include(k => k.KmTheoSp)
                .OrderByDescending(k => k.NgayBatDau)
                .Select(k => new
                {
                    k.MaChuongTrinh,
                    k.TenChuongTrinh,
                    k.NgayBatDau,
                    k.NgayKetThuc,
                    TrangThai = (k.NgayKetThuc != null && today > k.NgayKetThuc) ? "Đã kết thúc" :
                                (k.NgayBatDau != null && today < k.NgayBatDau) ? "Chưa diễn ra" : "Đang áp dụng",
                    SoLuongVoucher = k.KmTheoVoucher.Count,
                    SoLuongMonKm = k.KmTheoSp.Count
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("ChuongTrinh/{id}")]
        public async Task<IActionResult> GetChuongTrinhById(string id)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var k = await _service.Query<ChuongTrinhKhuyenMai>()
                .Include(x => x.KmTheoVoucher)
                .Include(x => x.KmTheoSp)
                    .ThenInclude(sp => sp.MaMonNavigation)
                .FirstOrDefaultAsync(x => x.MaChuongTrinh == id);

            if (k == null) return NotFound("Không tìm thấy chương trình khuyến mãi.");

            return Ok(new
            {
                k.MaChuongTrinh,
                k.TenChuongTrinh,
                k.NgayBatDau,
                k.NgayKetThuc,
                TrangThai = (k.NgayKetThuc != null && today > k.NgayKetThuc) ? "Đã kết thúc" :
                            (k.NgayBatDau != null && today < k.NgayBatDau) ? "Chưa diễn ra" : "Đang áp dụng",
                Vouchers = k.KmTheoVoucher.Select(v => new
                {
                    v.MaKmvoucher,
                    v.MaVoucher,
                    v.PhanTramGiam,
                    v.TienGiam,
                    v.GiaTriDonToiThieu,
                    v.GiamToiDa
                }),
                MonAns = k.KmTheoSp.Select(sp => new
                {
                    sp.MaKmsp,
                    sp.MaMon,
                    TenMon = sp.MaMonNavigation != null ? sp.MaMonNavigation.TenMon : "",
                    sp.PhanTramGiam,
                    sp.TienGiam
                })
            });
        }

        [HttpPost("ChuongTrinh")]
        public async Task<IActionResult> CreateChuongTrinh([FromBody] ChuongTrinhCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TenChuongTrinh))
            {
                return BadRequest("Tên chương trình không được để trống.");
            }

            if (request.NgayBatDau.HasValue && request.NgayKetThuc.HasValue && request.NgayKetThuc < request.NgayBatDau)
            {
                return BadRequest("Ngày kết thúc không được nhỏ hơn ngày bắt đầu.");
            }

            var maCT = "CTKM" + DateTime.Now.ToString("yyMMddHHmmss");
            var ct = new ChuongTrinhKhuyenMai
            {
                MaChuongTrinh = maCT,
                TenChuongTrinh = request.TenChuongTrinh.Trim(),
                NgayBatDau = request.NgayBatDau,
                NgayKetThuc = request.NgayKetThuc
            };

            _service.Add(ct);
            await _service.SaveChangesAsync();

            return Ok(new { message = "Tạo chương trình thành công.", maChuongTrinh = ct.MaChuongTrinh, data = ct });
        }

        [HttpPut("ChuongTrinh/{id}")]
        public async Task<IActionResult> UpdateChuongTrinh(string id, [FromBody] ChuongTrinhUpdateRequest request)
        {
            var ct = await _service.FindAsync<ChuongTrinhKhuyenMai>(id);
            if (ct == null) return NotFound("Không tìm thấy chương trình khuyến mãi.");

            if (string.IsNullOrWhiteSpace(request.TenChuongTrinh))
            {
                return BadRequest("Tên chương trình không được để trống.");
            }

            if (request.NgayBatDau.HasValue && request.NgayKetThuc.HasValue && request.NgayKetThuc < request.NgayBatDau)
            {
                return BadRequest("Ngày kết thúc không được nhỏ hơn ngày bắt đầu.");
            }

            ct.TenChuongTrinh = request.TenChuongTrinh.Trim();
            ct.NgayBatDau = request.NgayBatDau;
            ct.NgayKetThuc = request.NgayKetThuc;

            await _service.SaveChangesAsync();
            return Ok(new { message = "Cập nhật chương trình thành công.", data = ct });
        }

        [HttpDelete("ChuongTrinh/{id}")]
        public async Task<IActionResult> DeleteChuongTrinh(string id)
        {
            var ct = await _service.Query<ChuongTrinhKhuyenMai>()
                .Include(k => k.KmTheoVoucher).ThenInclude(v => v.HoaDon)
                .Include(k => k.KmTheoSp).ThenInclude(sp => sp.ChiTietHoaDon)
                .FirstOrDefaultAsync(k => k.MaChuongTrinh == id);

            if (ct == null) return NotFound("Không tìm thấy chương trình.");

            var today = DateOnly.FromDateTime(DateTime.Now);

            // Kiểm tra xem đã có hóa đơn nào từng áp dụng voucher hoặc món ưu đãi trong chương trình này chưa
            bool hasVoucherInvoice = ct.KmTheoVoucher.Any(v => v.HoaDon.Any());
            bool hasSpInvoice = ct.KmTheoSp.Any(sp => sp.ChiTietHoaDon.Any());

            if (hasVoucherInvoice || hasSpInvoice)
            {
                // Đã từng phát sinh giao dịch trên hóa đơn:
                // Không xóa cứng để bảo vệ dữ liệu kế toán và tránh vi phạm ràng buộc khóa ngoại (FK Exception).
                // Tự động kết thúc chương trình (Soft Close/Deactivate).
                ct.NgayKetThuc = today.AddDays(-1);
                if (!string.IsNullOrEmpty(ct.TenChuongTrinh) && !ct.TenChuongTrinh.StartsWith("[ĐÃ DỪNG]"))
                {
                    ct.TenChuongTrinh = "[ĐÃ DỪNG] " + ct.TenChuongTrinh;
                }

                await _service.SaveChangesAsync();
                return Ok(new
                {
                    message = "Chương trình đã từng phát sinh hóa đơn. Hệ thống đã tự động ĐÓNG (kết thúc hiệu lực) chương trình để bảo toàn lịch sử thanh toán.",
                    softDeleted = true
                });
            }

            // Chưa từng sử dụng: Xóa sạch an toàn khỏi CSDL
            if (ct.KmTheoSp.Any()) _service.RemoveRange(ct.KmTheoSp);
            if (ct.KmTheoVoucher.Any()) _service.RemoveRange(ct.KmTheoVoucher);

            _service.Remove(ct);
            await _service.SaveChangesAsync();

            return Ok(new { message = "Đã xóa chương trình thành công.", softDeleted = false });
        }

        // ==============================================================
        // 2. QUẢN LÝ VOUCHER TOÀN HÓA ĐƠN (ORDER VOUCHER)
        // ==============================================================

        [HttpGet("Voucher")]
        public async Task<IActionResult> GetAllVouchers()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var vouchers = await _service.Query<KmTheoVoucher>()
                .Include(v => v.MaChuongTrinhNavigation)
                .Include(v => v.MaLoaiMonNavigation)
                .Include(v => v.MaMonNavigation)
                .Include(v => v.HoaDon)
                .OrderByDescending(v => v.MaKmvoucher)
                .Select(v => new
                {
                    v.MaKmvoucher,
                    v.MaVoucher,
                    v.MaChuongTrinh,
                    TenChuongTrinh = v.MaChuongTrinhNavigation != null ? v.MaChuongTrinhNavigation.TenChuongTrinh : "",
                    NgayBatDau = v.MaChuongTrinhNavigation != null ? v.MaChuongTrinhNavigation.NgayBatDau : null,
                    NgayKetThuc = v.MaChuongTrinhNavigation != null ? v.MaChuongTrinhNavigation.NgayKetThuc : null,
                    SoLuongDaDung = v.HoaDon.Count(h => h.TrangThai != "Cancelled"),
                    SoLuongToiDa = v.SoLuongToiThieu,
                    TrangThai = (v.ChoPhepCongDon == false) ? "Đã kết thúc" :
                                (v.MaVoucher != null && v.MaVoucher.StartsWith("[DA_HUY]_")) ? "Đã kết thúc" :
                                (v.SoLuongToiThieu != null && v.SoLuongToiThieu > 0 && v.HoaDon.Count(h => h.TrangThai != "Cancelled") >= v.SoLuongToiThieu.Value) ? "Đã kết thúc" :
                                (v.MaChuongTrinhNavigation != null && v.MaChuongTrinhNavigation.NgayKetThuc != null && today > v.MaChuongTrinhNavigation.NgayKetThuc) ? "Đã kết thúc" :
                                (v.MaChuongTrinhNavigation != null && v.MaChuongTrinhNavigation.NgayBatDau != null && today < v.MaChuongTrinhNavigation.NgayBatDau) ? "Chưa diễn ra" : "Đang áp dụng",
                    v.PhanTramGiam,
                    v.TienGiam,
                    v.GiaTriDonToiThieu,
                    v.GiamToiDa,
                    v.PhamViApDung,
                    v.MaLoaiMon,
                    TenLoaiMon = v.MaLoaiMonNavigation != null ? v.MaLoaiMonNavigation.TenLoaiMon : null,
                    v.MaMon,
                    TenMon = v.MaMonNavigation != null ? v.MaMonNavigation.TenMon : null,
                    v.SoLuongToiThieu,
                    v.ChoPhepCongDon,
                    HinhThuc = v.PhamViApDung == "THEO_MON"
                        ? $"Giảm {(v.PhanTramGiam.HasValue ? $"{v.PhanTramGiam.Value}%" : $"{v.TienGiam:N0} đ")} món {v.MaMonNavigation!.TenMon ?? v.MaMon}"
                        : (v.PhamViApDung == "THEO_LOAI"
                            ? (v.GiaTriDonToiThieu.HasValue && v.GiaTriDonToiThieu.Value > 0
                                ? $"Giảm {(v.PhanTramGiam.HasValue ? $"{v.PhanTramGiam.Value}%" : $"{v.TienGiam:N0} đ")} nhóm {v.MaLoaiMonNavigation!.TenLoaiMon ?? v.MaLoaiMon} (món ≥ {v.GiaTriDonToiThieu:N0} đ)"
                                : $"Giảm {(v.PhanTramGiam.HasValue ? $"{v.PhanTramGiam.Value}%" : $"{v.TienGiam:N0} đ")} nhóm {v.MaLoaiMonNavigation!.TenLoaiMon ?? v.MaLoaiMon}")
                            : (v.PhanTramGiam.HasValue ? $"Giảm {v.PhanTramGiam.Value}%" : $"Giảm {v.TienGiam:N0} đ"))
                })
                .ToListAsync();

            return Ok(vouchers);
        }

        [HttpPost("Voucher")]
        public async Task<IActionResult> CreateVoucher([FromBody] VoucherCreateRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.MaChuongTrinh))
                return BadRequest("Vui lòng chọn chương trình khuyến mãi.");

            if (string.IsNullOrWhiteSpace(req.MaVoucher))
                return BadRequest("Vui lòng nhập mã Voucher.");

            var ct = await _service.FindAsync<ChuongTrinhKhuyenMai>(req.MaChuongTrinh);
            if (ct == null) return NotFound("Chương trình khuyến mãi không tồn tại.");

            var maVoucherUpper = req.MaVoucher.Trim().ToUpper();
            var exists = await _service.Query<KmTheoVoucher>().AnyAsync(v => v.MaVoucher == maVoucherUpper);
            if (exists)
                return BadRequest($"Mã voucher '{maVoucherUpper}' đã tồn tại trong hệ thống. Vui lòng đặt mã khác.");

            if (req.GiaTriGiam <= 0)
                return BadRequest("Giá trị giảm giá phải lớn hơn 0.");

            string phamVi = req.PhamViApDung?.ToUpper() ?? "TOAN_DON";
            if (phamVi == "THEO_LOAI" && string.IsNullOrWhiteSpace(req.MaLoaiMon))
                return BadRequest("Vui lòng chọn loại món áp dụng cho voucher nhóm.");

            if (phamVi == "THEO_MON" && string.IsNullOrWhiteSpace(req.MaMon))
                return BadRequest("Vui lòng chọn món ăn áp dụng cho voucher.");

            // Kiểm tra an toàn giá vốn nếu là voucher theo món
            if (phamVi == "THEO_MON")
            {
                var dish = await _service.Query<MonAn>()
                    .Include(m => m.DinhLuong)
                        .ThenInclude(dl => dl.MaNguyenLieuNavigation)
                    .FirstOrDefaultAsync(m => m.MaMon == req.MaMon);

                if (dish != null && dish.DonGia.HasValue)
                {
                    double goc = dish.DonGia.Value;
                    double giaVon = dish.DinhLuong.Sum(dl => (double)dl.SoLuongCan * (dl.MaNguyenLieuNavigation?.GiaNhapGanNhat ?? 0));
                    double sauGiam = req.LoaiGiam == "%" ? goc * (1 - req.GiaTriGiam / 100.0) : Math.Max(0, goc - req.GiaTriGiam);

                    if (giaVon > 0 && sauGiam < giaVon)
                    {
                        double tienLo = giaVon - sauGiam;
                        return BadRequest($"Mức giảm khiến giá sau ưu đãi ({sauGiam:N0} đ) thấp hơn giá vốn ({giaVon:N0} đ). Thua lỗ {tienLo:N0} đ/đĩa. Hệ thống chặn lưu voucher này.");
                    }
                }
            }

            var voucher = new KmTheoVoucher
            {
                MaKmvoucher = "VCH" + Guid.NewGuid().ToString().Substring(0, 7).ToUpper(),
                MaChuongTrinh = req.MaChuongTrinh,
                MaVoucher = maVoucherUpper,
                GiaTriDonToiThieu = req.GiaTriDonToiThieu > 0 ? req.GiaTriDonToiThieu : null,
                PhanTramGiam = req.LoaiGiam == "%" ? req.GiaTriGiam : null,
                TienGiam = req.LoaiGiam == "VND" ? req.GiaTriGiam : null,
                GiamToiDa = (req.LoaiGiam == "%" && req.GiamToiDa > 0) ? req.GiamToiDa : (req.GiamToiDa > 0 ? req.GiamToiDa : null),
                PhamViApDung = phamVi,
                MaLoaiMon = phamVi == "THEO_LOAI" ? req.MaLoaiMon : null,
                MaMon = phamVi == "THEO_MON" ? req.MaMon : null,
                SoLuongToiThieu = req.SoLuongToiThieu > 0 ? req.SoLuongToiThieu : 1,
                ChoPhepCongDon = req.ChoPhepCongDon ?? true
            };

            _service.Add(voucher);
            await _service.SaveChangesAsync();

            return Ok(new { message = "Tạo voucher thành công.", data = voucher });
        }

        [HttpPut("Voucher/{id}")]
        public async Task<IActionResult> UpdateVoucher(string id, [FromBody] VoucherCreateRequest req)
        {
            var voucher = await _service.Query<KmTheoVoucher>()
                .Include(v => v.HoaDon)
                .FirstOrDefaultAsync(v => v.MaKmvoucher == id || v.MaVoucher == id);

            if (voucher == null) return NotFound("Không tìm thấy voucher cần cập nhật.");

            if (req.GiaTriGiam <= 0)
                return BadRequest("Giá trị giảm giá phải lớn hơn 0.");

            if (!string.IsNullOrWhiteSpace(req.MaChuongTrinh))
            {
                var ct = await _service.FindAsync<ChuongTrinhKhuyenMai>(req.MaChuongTrinh);
                if (ct != null) voucher.MaChuongTrinh = req.MaChuongTrinh;
            }

            if (!string.IsNullOrWhiteSpace(req.MaVoucher))
            {
                var maUpper = req.MaVoucher.Trim().ToUpper();
                if (maUpper != voucher.MaVoucher)
                {
                    var exists = await _service.Query<KmTheoVoucher>().AnyAsync(v => v.MaVoucher == maUpper && v.MaKmvoucher != voucher.MaKmvoucher);
                    if (exists) return BadRequest($"Mã voucher '{maUpper}' đã được sử dụng. Vui lòng chọn mã khác.");
                    voucher.MaVoucher = maUpper;
                }
            }

            voucher.PhanTramGiam = req.LoaiGiam == "%" ? req.GiaTriGiam : null;
            voucher.TienGiam = req.LoaiGiam == "VND" ? req.GiaTriGiam : null;
            voucher.GiaTriDonToiThieu = req.GiaTriDonToiThieu > 0 ? req.GiaTriDonToiThieu : null;
            voucher.GiamToiDa = (req.LoaiGiam == "%" && req.GiamToiDa > 0) ? req.GiamToiDa : (req.GiamToiDa > 0 ? req.GiamToiDa : null);
            voucher.SoLuongToiThieu = req.SoLuongToiThieu > 0 ? req.SoLuongToiThieu : null;

            await _service.SaveChangesAsync();
            return Ok(new { message = "Cập nhật voucher thành công.", data = voucher });
        }

        [HttpDelete("Voucher/{id}")]
        public async Task<IActionResult> DeleteVoucher(string id)
        {
            var voucher = await _service.Query<KmTheoVoucher>()
                .Include(v => v.HoaDon)
                .FirstOrDefaultAsync(v => v.MaKmvoucher == id || v.MaVoucher == id);

            if (voucher == null) return NotFound("Không tìm thấy voucher.");

            if (voucher.HoaDon.Any())
            {
                // Đã từng phát sinh trên hóa đơn: Không thể xóa cứng vì ràng buộc FK với bảng HOA_DON.
                // Hệ thống thực hiện ẨN và vô hiệu hóa voucher:
                voucher.ChoPhepCongDon = false;

                // Cập nhật số lượng áp dụng bằng số lượng đã dùng để hệ thống ghi nhận hết lượt ngay lập tức
                int daDung = voucher.HoaDon.Count(h => h.TrangThai != "Cancelled");
                voucher.SoLuongToiThieu = Math.Max(1, daDung);

                // Giữ nguyên PhanTramGiam / TienGiam để KHÔNG vi phạm ràng buộc CK_KM_THEO_VOUCHER_giamHopLe
                // Đổi tiền tố mã voucher sang [DA_HUY]_ (cắt chuỗi tối đa 30 ký tự để không vượt quá giới hạn cột VARCHAR(30))
                if (!string.IsNullOrEmpty(voucher.MaVoucher) && !voucher.MaVoucher.StartsWith("[DA_HUY]_"))
                {
                    string prefix = "[DA_HUY]_";
                    int maxLen = 30 - prefix.Length;
                    string raw = voucher.MaVoucher;
                    voucher.MaVoucher = prefix + (raw.Length > maxLen ? raw.Substring(0, maxLen) : raw);
                }

                await _service.SaveChangesAsync();
                return Ok(new
                {
                    message = "Voucher này đã có đơn hàng sử dụng nên không thể xóa khỏi CSDL. Hệ thống đã ẨN voucher (chuyển sang trạng thái Đã kết thúc) để bảo toàn lịch sử hóa đơn.",
                    softDeleted = true
                });
            }

            _service.Remove(voucher);
            await _service.SaveChangesAsync();
            return Ok(new { message = "Đã xóa voucher thành công khỏi hệ thống.", softDeleted = false });
        }

        // ==============================================================
        // 3. QUẢN LÝ KHUYẾN MÃI THEO MÓN ĂN (TỰ ĐỘNG THEO SẢN PHẨM)
        // ==============================================================

        [HttpGet("MonAn")]
        public async Task<IActionResult> GetAllKmMonAn()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var list = await _service.Query<KmTheoSp>()
                .Include(k => k.MaChuongTrinhNavigation)
                .Include(k => k.MaMonNavigation)
                .OrderByDescending(k => k.MaKmsp)
                .Select(k => new
                {
                    k.MaKmsp,
                    k.MaChuongTrinh,
                    TenChuongTrinh = k.MaChuongTrinhNavigation != null ? k.MaChuongTrinhNavigation.TenChuongTrinh : "",
                    NgayBatDau = k.MaChuongTrinhNavigation != null ? k.MaChuongTrinhNavigation.NgayBatDau : null,
                    NgayKetThuc = k.MaChuongTrinhNavigation != null ? k.MaChuongTrinhNavigation.NgayKetThuc : null,
                    TrangThai = (k.MaChuongTrinhNavigation != null && k.MaChuongTrinhNavigation.NgayKetThuc != null && today > k.MaChuongTrinhNavigation.NgayKetThuc) ? "Đã kết thúc" :
                                (k.MaChuongTrinhNavigation != null && k.MaChuongTrinhNavigation.NgayBatDau != null && today < k.MaChuongTrinhNavigation.NgayBatDau) ? "Chưa diễn ra" : "Đang áp dụng",
                    k.MaMon,
                    TenMon = k.MaMonNavigation != null ? k.MaMonNavigation.TenMon : "",
                    DonGiaGoc = k.MaMonNavigation != null ? k.MaMonNavigation.DonGia : 0,
                    k.PhanTramGiam,
                    k.TienGiam,
                    HinhThuc = k.PhanTramGiam.HasValue ? $"Giảm {k.PhanTramGiam.Value}%" : $"Giảm {k.TienGiam:N0} đ",
                    DonGiaSauGiam = (k.MaMonNavigation != null && k.MaMonNavigation.DonGia.HasValue) ?
                        (k.PhanTramGiam.HasValue ? (k.MaMonNavigation.DonGia.Value * (1 - k.PhanTramGiam.Value / 100.0)) :
                         k.TienGiam.HasValue ? Math.Max(0, k.MaMonNavigation.DonGia.Value - k.TienGiam.Value) : k.MaMonNavigation.DonGia.Value) : 0
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpPost("MonAn")]
        public async Task<IActionResult> CreateKmMonAn([FromBody] KmMonCreateRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.MaChuongTrinh))
                return BadRequest("Vui lòng chọn chương trình khuyến mãi.");

            if (string.IsNullOrWhiteSpace(req.MaMon))
                return BadRequest("Vui lòng chọn món ăn cần áp dụng khuyến mãi.");

            var ct = await _service.FindAsync<ChuongTrinhKhuyenMai>(req.MaChuongTrinh);
            if (ct == null) return NotFound("Chương trình khuyến mãi không tồn tại.");

            var mon = await _service.FindAsync<MonAn>(req.MaMon);
            if (mon == null) return NotFound("Món ăn không tồn tại.");

            // Kiểm tra món đã được thêm vào chương trình này chưa
            var exists = await _service.Query<KmTheoSp>().AnyAsync(s => s.MaChuongTrinh == req.MaChuongTrinh && s.MaMon == req.MaMon);
            if (exists)
                return BadRequest($"Món '{mon.TenMon}' đã có trong chương trình này.");

            if (req.GiaTriGiam <= 0)
                return BadRequest("Giá trị giảm giá phải lớn hơn 0.");

            var kmsp = new KmTheoSp
            {
                MaKmsp = "CTKM" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper(),
                MaChuongTrinh = req.MaChuongTrinh,
                MaMon = req.MaMon,
                PhanTramGiam = req.LoaiGiam == "%" ? req.GiaTriGiam : null,
                TienGiam = req.LoaiGiam == "VND" ? req.GiaTriGiam : null
            };

            _service.Add(kmsp);
            await _service.SaveChangesAsync();

            return Ok(new { message = $"Đã áp dụng ưu đãi cho món '{mon.TenMon}'.", data = kmsp });
        }

        [HttpDelete("MonAn/{id}")]
        public async Task<IActionResult> DeleteKmMonAn(string id)
        {
            var kmsp = await _service.Query<KmTheoSp>()
                .Include(k => k.ChiTietHoaDon)
                .Include(k => k.MaMonNavigation)
                .FirstOrDefaultAsync(k => k.MaKmsp == id);

            if (kmsp == null) return NotFound("Không tìm thấy chi tiết khuyến mãi món.");

            if (kmsp.ChiTietHoaDon.Any())
            {
                // Đã từng phát sinh trong chi tiết đơn hàng: Không xóa cứng khỏi DB để bảo toàn lịch sử
                return Ok(new
                {
                    message = $"Ưu đãi món '{kmsp.MaMonNavigation?.TenMon ?? kmsp.MaMon}' đã có đơn hàng sử dụng. Hệ thống giữ thông tin để bảo toàn lịch sử hóa đơn.",
                    softDeleted = true
                });
            }

            _service.Remove(kmsp);
            await _service.SaveChangesAsync();

            return Ok(new { message = "Đã xóa khuyến mãi món thành công.", softDeleted = false });
        }

        // ==============================================================
        // 4. DANH SÁCH MÓN ĂN ĐỂ CHỌN ÁP DỤNG KHUYẾN MÃI
        // ==============================================================

        [HttpGet("AvailableDishes")]
        public async Task<IActionResult> GetAvailableDishes()
        {
            var dishes = await _service.Query<MonAn>()
                .Where(m => m.TrangThai != "Discontinued" && m.TrangThai != "Inactive")
                .Include(m => m.DinhLuong)
                    .ThenInclude(dl => dl.MaNguyenLieuNavigation)
                .OrderBy(m => m.TenMon)
                .ToListAsync();

            var result = dishes.Select(m =>
            {
                double giaVon = m.DinhLuong.Sum(dl => (double)dl.SoLuongCan * (dl.MaNguyenLieuNavigation?.GiaNhapGanNhat ?? 0));
                return new
                {
                    m.MaMon,
                    m.TenMon,
                    m.DonGia,
                    m.MaLoaiMon,
                    m.DuongDanAnh,
                    GiaVonUocTinh = Math.Round(giaVon, 0)
                };
            }).ToList();

            return Ok(result);
        }

        // ==============================================================
        // 5. LEGACY CONTROLLER COMPATIBILITY
        // ==============================================================

        [HttpGet("LoaiKhuyenMai")]
        public IActionResult GetLoaiKhuyenMai() => Ok(new[] { "ToanDon", "TheoSanPham" });

        [HttpGet("LoaiGiam")]
        public IActionResult GetLoaiGiam() => Ok(new[] { "%", "VND" });

        [HttpGet("LoaiMon")]
        public async Task<IActionResult> GetLoaiMon()
        {
            var list = await _service.Query<LoaiMon>()
                .OrderBy(l => l.MaLoaiMon)
                .Select(l => new { l.MaLoaiMon, l.TenLoaiMon })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id) => await GetChuongTrinhById(id);

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id) => await DeleteChuongTrinh(id);
    }
}

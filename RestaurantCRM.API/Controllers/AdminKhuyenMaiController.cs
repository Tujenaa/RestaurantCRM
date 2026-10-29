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
                .Include(k => k.KmTheoDonHang)
                .Include(k => k.KmTheoLoai)
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
                    SoLuongVoucher = k.KmTheoDonHang.Count 
                                   + k.KmTheoLoai.Count(l => !string.IsNullOrEmpty(l.MaVoucher)) 
                                   + k.KmTheoSp.Count(s => !string.IsNullOrEmpty(s.MaVoucher)),
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
                .Include(x => x.KmTheoDonHang)
                .Include(x => x.KmTheoLoai)
                    .ThenInclude(l => l.MaLoaiMonNavigation)
                .Include(x => x.KmTheoSp)
                    .ThenInclude(sp => sp.MaMonNavigation)
                .FirstOrDefaultAsync(x => x.MaChuongTrinh == id);

            if (k == null) return NotFound("Không tìm thấy chương trình khuyến mãi.");

            var vDon = k.KmTheoDonHang.Select(v => new
            {
                MaKmvoucher = v.MaKMDonHang,
                v.MaVoucher,
                v.PhanTramGiam,
                v.TienGiam,
                v.GiaTriDonToiThieu,
                v.GiamToiDa,
                PhamViApDung = "TOAN_DON",
                MaLoaiMon = (string?)null,
                TenLoaiMon = (string?)null,
                MaMon = (string?)null,
                TenMon = (string?)null
            });

            var vLoai = k.KmTheoLoai.Where(v => !string.IsNullOrEmpty(v.MaVoucher)).Select(v => new
            {
                MaKmvoucher = v.MaKMLoai,
                v.MaVoucher,
                v.PhanTramGiam,
                v.TienGiam,
                v.GiaTriDonToiThieu,
                v.GiamToiDa,
                PhamViApDung = "THEO_LOAI",
                v.MaLoaiMon,
                TenLoaiMon = v.MaLoaiMonNavigation != null ? v.MaLoaiMonNavigation.TenLoaiMon : null,
                MaMon = (string?)null,
                TenMon = (string?)null
            });

            var vMon = k.KmTheoSp.Where(v => !string.IsNullOrEmpty(v.MaVoucher)).Select(v => new
            {
                MaKmvoucher = v.MaKmsp,
                v.MaVoucher,
                v.PhanTramGiam,
                v.TienGiam,
                GiaTriDonToiThieu = (double?)null,
                v.GiamToiDa,
                PhamViApDung = "THEO_MON",
                MaLoaiMon = (string?)null,
                TenLoaiMon = (string?)null,
                v.MaMon,
                TenMon = v.MaMonNavigation != null ? v.MaMonNavigation.TenMon : null
            });

            var allVouchers = vDon.Concat(vLoai).Concat(vMon).ToList();

            return Ok(new
            {
                k.MaChuongTrinh,
                k.TenChuongTrinh,
                k.NgayBatDau,
                k.NgayKetThuc,
                TrangThai = (k.NgayKetThuc != null && today > k.NgayKetThuc) ? "Đã kết thúc" :
                            (k.NgayBatDau != null && today < k.NgayBatDau) ? "Chưa diễn ra" : "Đang áp dụng",
                Vouchers = allVouchers,
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

            if (request.NgayBatDau.HasValue && request.NgayKetThuc.HasValue && request.NgayBatDau > request.NgayKetThuc)
            {
                return BadRequest("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
            }

            var ct = new ChuongTrinhKhuyenMai
            {
                MaChuongTrinh = "KM" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper(),
                TenChuongTrinh = request.TenChuongTrinh.Trim(),
                NgayBatDau = request.NgayBatDau,
                NgayKetThuc = request.NgayKetThuc
            };

            _service.Add(ct);
            await _service.SaveChangesAsync();

            return Ok(new { message = "Tạo chương trình khuyến mãi thành công.", data = ct });
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

            if (request.NgayBatDau.HasValue && request.NgayKetThuc.HasValue && request.NgayBatDau > request.NgayKetThuc)
            {
                return BadRequest("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
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
                .Include(k => k.KmTheoDonHang).ThenInclude(v => v.HoaDon)
                .Include(k => k.KmTheoLoai).ThenInclude(l => l.ChiTietHoaDon)
                .Include(k => k.KmTheoSp).ThenInclude(sp => sp.ChiTietHoaDon)
                .FirstOrDefaultAsync(k => k.MaChuongTrinh == id);

            if (ct == null) return NotFound("Không tìm thấy chương trình.");

            var today = DateOnly.FromDateTime(DateTime.Now);

            bool hasDonInvoice = ct.KmTheoDonHang.Any(v => v.HoaDon.Any());
            bool hasLoaiInvoice = ct.KmTheoLoai.Any(l => l.ChiTietHoaDon.Any());
            bool hasSpInvoice = ct.KmTheoSp.Any(sp => sp.ChiTietHoaDon.Any());

            if (hasDonInvoice || hasLoaiInvoice || hasSpInvoice)
            {
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

            if (ct.KmTheoSp.Any()) _service.RemoveRange(ct.KmTheoSp);
            if (ct.KmTheoLoai.Any()) _service.RemoveRange(ct.KmTheoLoai);
            if (ct.KmTheoDonHang.Any()) _service.RemoveRange(ct.KmTheoDonHang);

            _service.Remove(ct);
            await _service.SaveChangesAsync();

            return Ok(new { message = "Đã xóa chương trình thành công.", softDeleted = false });
        }

        // ==============================================================
        // 2. QUẢN LÝ VOUCHER (ĐƠN HÀNG, THEO LOẠI, THEO MÓN)
        // ==============================================================

        [HttpGet("Voucher")]
        public async Task<IActionResult> GetAllVouchers()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            // 1. Voucher toàn đơn (KM_THEO_DON_HANG)
            var vDon = await _service.Query<KmTheoDonHang>()
                .Include(v => v.MaChuongTrinhNavigation)
                .Include(v => v.HoaDon)
                .OrderByDescending(v => v.MaKMDonHang)
                .Select(v => new
                {
                    MaKmvoucher = v.MaKMDonHang,
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
                    PhamViApDung = "TOAN_DON",
                    MaLoaiMon = (string?)null,
                    TenLoaiMon = (string?)null,
                    MaMon = (string?)null,
                    TenMon = (string?)null,
                    v.SoLuongToiThieu,
                    v.ChoPhepCongDon,
                    HinhThuc = v.PhanTramGiam.HasValue ? $"Giảm {v.PhanTramGiam.Value}%" : $"Giảm {v.TienGiam:N0} đ"
                })
                .ToListAsync();

            // 2. Voucher theo loại món (KM_THEO_LOAI)
            var vLoai = await _service.Query<KmTheoLoai>()
                .Include(v => v.MaChuongTrinhNavigation)
                .Include(v => v.MaLoaiMonNavigation)
                .Include(v => v.ChiTietHoaDon)
                .OrderByDescending(v => v.MaKMLoai)
                .Select(v => new
                {
                    MaKmvoucher = v.MaKMLoai,
                    v.MaVoucher,
                    v.MaChuongTrinh,
                    TenChuongTrinh = v.MaChuongTrinhNavigation != null ? v.MaChuongTrinhNavigation.TenChuongTrinh : "",
                    NgayBatDau = v.MaChuongTrinhNavigation != null ? v.MaChuongTrinhNavigation.NgayBatDau : null,
                    NgayKetThuc = v.MaChuongTrinhNavigation != null ? v.MaChuongTrinhNavigation.NgayKetThuc : null,
                    SoLuongDaDung = v.ChiTietHoaDon.Count(c => c.MaHoaDonNavigation != null && c.MaHoaDonNavigation.TrangThai != "Cancelled"),
                    SoLuongToiDa = v.SoLuongToiThieu,
                    TrangThai = (v.ChoPhepCongDon == false) ? "Đã kết thúc" :
                                (v.MaVoucher != null && v.MaVoucher.StartsWith("[DA_HUY]_")) ? "Đã kết thúc" :
                                (v.SoLuongToiThieu != null && v.SoLuongToiThieu > 0 && v.ChiTietHoaDon.Count(c => c.MaHoaDonNavigation != null && c.MaHoaDonNavigation.TrangThai != "Cancelled") >= v.SoLuongToiThieu.Value) ? "Đã kết thúc" :
                                (v.MaChuongTrinhNavigation != null && v.MaChuongTrinhNavigation.NgayKetThuc != null && today > v.MaChuongTrinhNavigation.NgayKetThuc) ? "Đã kết thúc" :
                                (v.MaChuongTrinhNavigation != null && v.MaChuongTrinhNavigation.NgayBatDau != null && today < v.MaChuongTrinhNavigation.NgayBatDau) ? "Chưa diễn ra" : "Đang áp dụng",
                    v.PhanTramGiam,
                    v.TienGiam,
                    v.GiaTriDonToiThieu,
                    v.GiamToiDa,
                    PhamViApDung = "THEO_LOAI",
                    v.MaLoaiMon,
                    TenLoaiMon = v.MaLoaiMonNavigation != null ? v.MaLoaiMonNavigation.TenLoaiMon : null,
                    MaMon = (string?)null,
                    TenMon = (string?)null,
                    v.SoLuongToiThieu,
                    v.ChoPhepCongDon,
                    HinhThuc = v.GiaTriDonToiThieu.HasValue && v.GiaTriDonToiThieu.Value > 0
                        ? $"Giảm {(v.PhanTramGiam.HasValue ? $"{v.PhanTramGiam.Value}%" : $"{v.TienGiam:N0} đ")} nhóm {v.MaLoaiMonNavigation!.TenLoaiMon ?? v.MaLoaiMon} (món ≥ {v.GiaTriDonToiThieu:N0} đ)"
                        : $"Giảm {(v.PhanTramGiam.HasValue ? $"{v.PhanTramGiam.Value}%" : $"{v.TienGiam:N0} đ")} nhóm {v.MaLoaiMonNavigation!.TenLoaiMon ?? v.MaLoaiMon}"
                })
                .ToListAsync();

            // 3. Voucher theo món ăn (KM_THEO_SP có MaVoucher)
            var vMon = await _service.Query<KmTheoSp>()
                .Include(v => v.MaChuongTrinhNavigation)
                .Include(v => v.MaMonNavigation)
                .Include(v => v.ChiTietHoaDon)
                .Where(v => !string.IsNullOrEmpty(v.MaVoucher))
                .OrderByDescending(v => v.MaKmsp)
                .Select(v => new
                {
                    MaKmvoucher = v.MaKmsp,
                    v.MaVoucher,
                    v.MaChuongTrinh,
                    TenChuongTrinh = v.MaChuongTrinhNavigation != null ? v.MaChuongTrinhNavigation.TenChuongTrinh : "",
                    NgayBatDau = v.MaChuongTrinhNavigation != null ? v.MaChuongTrinhNavigation.NgayBatDau : null,
                    NgayKetThuc = v.MaChuongTrinhNavigation != null ? v.MaChuongTrinhNavigation.NgayKetThuc : null,
                    SoLuongDaDung = v.ChiTietHoaDon.Count(c => c.MaHoaDonNavigation != null && c.MaHoaDonNavigation.TrangThai != "Cancelled"),
                    SoLuongToiDa = v.SoLuongToiThieu,
                    TrangThai = (v.ChoPhepCongDon == false) ? "Đã kết thúc" :
                                (v.MaVoucher != null && v.MaVoucher.StartsWith("[DA_HUY]_")) ? "Đã kết thúc" :
                                (v.SoLuongToiThieu != null && v.SoLuongToiThieu > 0 && v.ChiTietHoaDon.Count(c => c.MaHoaDonNavigation != null && c.MaHoaDonNavigation.TrangThai != "Cancelled") >= v.SoLuongToiThieu.Value) ? "Đã kết thúc" :
                                (v.MaChuongTrinhNavigation != null && v.MaChuongTrinhNavigation.NgayKetThuc != null && today > v.MaChuongTrinhNavigation.NgayKetThuc) ? "Đã kết thúc" :
                                (v.MaChuongTrinhNavigation != null && v.MaChuongTrinhNavigation.NgayBatDau != null && today < v.MaChuongTrinhNavigation.NgayBatDau) ? "Chưa diễn ra" : "Đang áp dụng",
                    v.PhanTramGiam,
                    v.TienGiam,
                    GiaTriDonToiThieu = (double?)null,
                    v.GiamToiDa,
                    PhamViApDung = "THEO_MON",
                    MaLoaiMon = (string?)null,
                    TenLoaiMon = (string?)null,
                    v.MaMon,
                    TenMon = v.MaMonNavigation != null ? v.MaMonNavigation.TenMon : null,
                    v.SoLuongToiThieu,
                    v.ChoPhepCongDon,
                    HinhThuc = $"Giảm {(v.PhanTramGiam.HasValue ? $"{v.PhanTramGiam.Value}%" : $"{v.TienGiam:N0} đ")} món {v.MaMonNavigation!.TenMon ?? v.MaMon}"
                })
                .ToListAsync();

            var all = vDon.Concat(vLoai).Concat(vMon).ToList();
            return Ok(all);
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
            var existsDon = await _service.Query<KmTheoDonHang>().AnyAsync(v => v.MaVoucher == maVoucherUpper);
            var existsLoai = await _service.Query<KmTheoLoai>().AnyAsync(v => v.MaVoucher == maVoucherUpper);
            var existsSp = await _service.Query<KmTheoSp>().AnyAsync(v => v.MaVoucher == maVoucherUpper);

            if (existsDon || existsLoai || existsSp)
                return BadRequest($"Mã voucher '{maVoucherUpper}' đã tồn tại trong hệ thống. Vui lòng đặt mã khác.");

            if (req.GiaTriGiam <= 0)
                return BadRequest("Giá trị giảm giá phải lớn hơn 0.");

            string phamVi = req.PhamViApDung?.ToUpper() ?? "TOAN_DON";
            if (phamVi == "THEO_LOAI" && string.IsNullOrWhiteSpace(req.MaLoaiMon))
                return BadRequest("Vui lòng chọn loại món áp dụng cho voucher nhóm.");

            if (phamVi == "THEO_MON" && string.IsNullOrWhiteSpace(req.MaMon))
                return BadRequest("Vui lòng chọn món ăn áp dụng cho voucher.");

            if (phamVi == "THEO_MON")
            {
                var voucherSp = new KmTheoSp
                {
                    MaKmsp = "VCH" + Guid.NewGuid().ToString().Substring(0, 7).ToUpper(),
                    MaChuongTrinh = req.MaChuongTrinh,
                    MaMon = req.MaMon!,
                    MaVoucher = maVoucherUpper,
                    PhanTramGiam = req.LoaiGiam == "%" ? req.GiaTriGiam : null,
                    TienGiam = req.LoaiGiam == "VND" ? (req.GiamToiDa > 0 ? req.GiamToiDa : req.GiaTriGiam) : null,
                    GiamToiDa = (req.LoaiGiam == "%" && req.GiamToiDa > 0) ? req.GiamToiDa : (req.GiamToiDa > 0 ? req.GiamToiDa : null),
                    SoLuongToiThieu = req.SoLuongToiThieu > 0 ? req.SoLuongToiThieu : 1,
                    ChoPhepCongDon = req.ChoPhepCongDon ?? true
                };

                _service.Add(voucherSp);
                await _service.SaveChangesAsync();
                return Ok(new { message = "Tạo voucher theo món ăn thành công.", data = voucherSp });
            }
            else if (phamVi == "THEO_LOAI")
            {
                var voucherLoai = new KmTheoLoai
                {
                    MaKMLoai = "VCH" + Guid.NewGuid().ToString().Substring(0, 7).ToUpper(),
                    MaChuongTrinh = req.MaChuongTrinh,
                    MaLoaiMon = req.MaLoaiMon!,
                    MaVoucher = maVoucherUpper,
                    GiaTriDonToiThieu = req.GiaTriDonToiThieu > 0 ? req.GiaTriDonToiThieu : null,
                    PhanTramGiam = req.LoaiGiam == "%" ? req.GiaTriGiam : null,
                    TienGiam = req.LoaiGiam == "VND" ? (req.GiamToiDa > 0 ? req.GiamToiDa : req.GiaTriGiam) : null,
                    GiamToiDa = (req.LoaiGiam == "%" && req.GiamToiDa > 0) ? req.GiamToiDa : (req.GiamToiDa > 0 ? req.GiamToiDa : null),
                    SoLuongToiThieu = req.SoLuongToiThieu > 0 ? req.SoLuongToiThieu : 1,
                    ChoPhepCongDon = req.ChoPhepCongDon ?? true
                };

                _service.Add(voucherLoai);
                await _service.SaveChangesAsync();
                return Ok(new { message = "Tạo voucher theo nhóm món thành công.", data = voucherLoai });
            }
            else
            {
                var voucherDon = new KmTheoDonHang
                {
                    MaKMDonHang = "VCH" + Guid.NewGuid().ToString().Substring(0, 7).ToUpper(),
                    MaChuongTrinh = req.MaChuongTrinh,
                    MaVoucher = maVoucherUpper,
                    GiaTriDonToiThieu = req.GiaTriDonToiThieu > 0 ? req.GiaTriDonToiThieu : null,
                    PhanTramGiam = req.LoaiGiam == "%" ? req.GiaTriGiam : null,
                    TienGiam = req.LoaiGiam == "VND" ? (req.GiamToiDa > 0 ? req.GiamToiDa : req.GiaTriGiam) : null,
                    GiamToiDa = (req.LoaiGiam == "%" && req.GiamToiDa > 0) ? req.GiamToiDa : (req.GiamToiDa > 0 ? req.GiamToiDa : null),
                    SoLuongToiThieu = req.SoLuongToiThieu > 0 ? req.SoLuongToiThieu : 1,
                    ChoPhepCongDon = req.ChoPhepCongDon ?? true
                };

                _service.Add(voucherDon);
                await _service.SaveChangesAsync();
                return Ok(new { message = "Tạo voucher toàn đơn thành công.", data = voucherDon });
            }
        }

        [HttpPut("Voucher/{id}")]
        public async Task<IActionResult> UpdateVoucher(string id, [FromBody] VoucherCreateRequest req)
        {
            if (req.GiaTriGiam <= 0)
                return BadRequest("Giá trị giảm giá phải lớn hơn 0.");

            var maUpper = !string.IsNullOrWhiteSpace(req.MaVoucher) ? req.MaVoucher.Trim().ToUpper() : null;

            // 1. Kiểm tra KM_THEO_DON_HANG
            var vDon = await _service.Query<KmTheoDonHang>().FirstOrDefaultAsync(v => v.MaKMDonHang == id || v.MaVoucher == id);
            if (vDon != null)
            {
                if (maUpper != null && maUpper != vDon.MaVoucher)
                {
                    var exists = await _service.Query<KmTheoDonHang>().AnyAsync(v => v.MaVoucher == maUpper && v.MaKMDonHang != vDon.MaKMDonHang)
                              || await _service.Query<KmTheoLoai>().AnyAsync(v => v.MaVoucher == maUpper)
                              || await _service.Query<KmTheoSp>().AnyAsync(v => v.MaVoucher == maUpper);
                    if (exists) return BadRequest($"Mã voucher '{maUpper}' đã được sử dụng. Vui lòng chọn mã khác.");
                    vDon.MaVoucher = maUpper;
                }

                if (!string.IsNullOrWhiteSpace(req.MaChuongTrinh))
                {
                    var ct = await _service.FindAsync<ChuongTrinhKhuyenMai>(req.MaChuongTrinh);
                    if (ct != null) vDon.MaChuongTrinh = req.MaChuongTrinh;
                }

                vDon.PhanTramGiam = req.LoaiGiam == "%" ? req.GiaTriGiam : null;
                vDon.TienGiam = req.LoaiGiam == "VND" ? (req.GiamToiDa > 0 ? req.GiamToiDa : req.GiaTriGiam) : null;
                vDon.GiaTriDonToiThieu = req.GiaTriDonToiThieu > 0 ? req.GiaTriDonToiThieu : null;
                vDon.GiamToiDa = (req.LoaiGiam == "%" && req.GiamToiDa > 0) ? req.GiamToiDa : (req.GiamToiDa > 0 ? req.GiamToiDa : null);
                vDon.SoLuongToiThieu = req.SoLuongToiThieu > 0 ? req.SoLuongToiThieu : null;

                await _service.SaveChangesAsync();
                return Ok(new { message = "Cập nhật voucher thành công.", data = vDon });
            }

            // 2. Kiểm tra KM_THEO_LOAI
            var vLoai = await _service.Query<KmTheoLoai>().FirstOrDefaultAsync(v => v.MaKMLoai == id || v.MaVoucher == id);
            if (vLoai != null)
            {
                if (maUpper != null && maUpper != vLoai.MaVoucher)
                {
                    var exists = await _service.Query<KmTheoDonHang>().AnyAsync(v => v.MaVoucher == maUpper)
                              || await _service.Query<KmTheoLoai>().AnyAsync(v => v.MaVoucher == maUpper && v.MaKMLoai != vLoai.MaKMLoai)
                              || await _service.Query<KmTheoSp>().AnyAsync(v => v.MaVoucher == maUpper);
                    if (exists) return BadRequest($"Mã voucher '{maUpper}' đã được sử dụng. Vui lòng chọn mã khác.");
                    vLoai.MaVoucher = maUpper;
                }

                if (!string.IsNullOrWhiteSpace(req.MaChuongTrinh))
                {
                    var ct = await _service.FindAsync<ChuongTrinhKhuyenMai>(req.MaChuongTrinh);
                    if (ct != null) vLoai.MaChuongTrinh = req.MaChuongTrinh;
                }

                if (!string.IsNullOrWhiteSpace(req.MaLoaiMon)) vLoai.MaLoaiMon = req.MaLoaiMon;

                vLoai.PhanTramGiam = req.LoaiGiam == "%" ? req.GiaTriGiam : null;
                vLoai.TienGiam = req.LoaiGiam == "VND" ? (req.GiamToiDa > 0 ? req.GiamToiDa : req.GiaTriGiam) : null;
                vLoai.GiaTriDonToiThieu = req.GiaTriDonToiThieu > 0 ? req.GiaTriDonToiThieu : null;
                vLoai.GiamToiDa = (req.LoaiGiam == "%" && req.GiamToiDa > 0) ? req.GiamToiDa : (req.GiamToiDa > 0 ? req.GiamToiDa : null);
                vLoai.SoLuongToiThieu = req.SoLuongToiThieu > 0 ? req.SoLuongToiThieu : null;

                await _service.SaveChangesAsync();
                return Ok(new { message = "Cập nhật voucher thành công.", data = vLoai });
            }

            // 3. Kiểm tra KM_THEO_SP
            var vSp = await _service.Query<KmTheoSp>().FirstOrDefaultAsync(v => v.MaKmsp == id || v.MaVoucher == id);
            if (vSp != null)
            {
                if (maUpper != null && maUpper != vSp.MaVoucher)
                {
                    var exists = await _service.Query<KmTheoDonHang>().AnyAsync(v => v.MaVoucher == maUpper)
                              || await _service.Query<KmTheoLoai>().AnyAsync(v => v.MaVoucher == maUpper)
                              || await _service.Query<KmTheoSp>().AnyAsync(v => v.MaVoucher == maUpper && v.MaKmsp != vSp.MaKmsp);
                    if (exists) return BadRequest($"Mã voucher '{maUpper}' đã được sử dụng. Vui lòng chọn mã khác.");
                    vSp.MaVoucher = maUpper;
                }

                if (!string.IsNullOrWhiteSpace(req.MaChuongTrinh))
                {
                    var ct = await _service.FindAsync<ChuongTrinhKhuyenMai>(req.MaChuongTrinh);
                    if (ct != null) vSp.MaChuongTrinh = req.MaChuongTrinh;
                }

                if (!string.IsNullOrWhiteSpace(req.MaMon)) vSp.MaMon = req.MaMon;

                vSp.PhanTramGiam = req.LoaiGiam == "%" ? req.GiaTriGiam : null;
                vSp.TienGiam = req.LoaiGiam == "VND" ? (req.GiamToiDa > 0 ? req.GiamToiDa : req.GiaTriGiam) : null;
                vSp.GiamToiDa = (req.LoaiGiam == "%" && req.GiamToiDa > 0) ? req.GiamToiDa : (req.GiamToiDa > 0 ? req.GiamToiDa : null);
                vSp.SoLuongToiThieu = req.SoLuongToiThieu > 0 ? req.SoLuongToiThieu : null;

                await _service.SaveChangesAsync();
                return Ok(new { message = "Cập nhật voucher thành công.", data = vSp });
            }

            return NotFound("Không tìm thấy voucher cần cập nhật.");
        }

        [HttpDelete("Voucher/{id}")]
        public async Task<IActionResult> DeleteVoucher(string id)
        {
            // 1. Kiểm tra KM_THEO_DON_HANG
            var vDon = await _service.Query<KmTheoDonHang>()
                .Include(v => v.HoaDon)
                .FirstOrDefaultAsync(v => v.MaKMDonHang == id || v.MaVoucher == id);

            if (vDon != null)
            {
                if (vDon.HoaDon.Any())
                {
                    vDon.ChoPhepCongDon = false;
                    int daDung = vDon.HoaDon.Count(h => h.TrangThai != "Cancelled");
                    vDon.SoLuongToiThieu = Math.Max(1, daDung);

                    if (!string.IsNullOrEmpty(vDon.MaVoucher) && !vDon.MaVoucher.StartsWith("[DA_HUY]_"))
                    {
                        string prefix = "[DA_HUY]_";
                        int maxLen = 30 - prefix.Length;
                        string raw = vDon.MaVoucher;
                        vDon.MaVoucher = prefix + (raw.Length > maxLen ? raw.Substring(0, maxLen) : raw);
                    }

                    await _service.SaveChangesAsync();
                    return Ok(new
                    {
                        message = "Voucher này đã có đơn hàng sử dụng nên không thể xóa khỏi CSDL. Hệ thống đã ẨN voucher (chuyển sang trạng thái Đã kết thúc) để bảo toàn lịch sử hóa đơn.",
                        softDeleted = true
                    });
                }

                _service.Remove(vDon);
                await _service.SaveChangesAsync();
                return Ok(new { message = "Đã xóa voucher thành công khỏi hệ thống.", softDeleted = false });
            }

            // 2. Kiểm tra KM_THEO_LOAI
            var vLoai = await _service.Query<KmTheoLoai>()
                .Include(v => v.ChiTietHoaDon)
                .FirstOrDefaultAsync(v => v.MaKMLoai == id || v.MaVoucher == id);

            if (vLoai != null)
            {
                if (vLoai.ChiTietHoaDon.Any())
                {
                    vLoai.ChoPhepCongDon = false;
                    int daDung = vLoai.ChiTietHoaDon.Count(c => c.MaHoaDonNavigation != null && c.MaHoaDonNavigation.TrangThai != "Cancelled");
                    vLoai.SoLuongToiThieu = Math.Max(1, daDung);

                    if (!string.IsNullOrEmpty(vLoai.MaVoucher) && !vLoai.MaVoucher.StartsWith("[DA_HUY]_"))
                    {
                        string prefix = "[DA_HUY]_";
                        int maxLen = 30 - prefix.Length;
                        string raw = vLoai.MaVoucher;
                        vLoai.MaVoucher = prefix + (raw.Length > maxLen ? raw.Substring(0, maxLen) : raw);
                    }

                    await _service.SaveChangesAsync();
                    return Ok(new
                    {
                        message = "Voucher này đã có đơn hàng sử dụng nên không thể xóa khỏi CSDL. Hệ thống đã ẨN voucher (chuyển sang trạng thái Đã kết thúc) để bảo toàn lịch sử hóa đơn.",
                        softDeleted = true
                    });
                }

                _service.Remove(vLoai);
                await _service.SaveChangesAsync();
                return Ok(new { message = "Đã xóa voucher thành công khỏi hệ thống.", softDeleted = false });
            }

            // 3. Kiểm tra KM_THEO_SP
            var vSp = await _service.Query<KmTheoSp>()
                .Include(v => v.ChiTietHoaDon)
                .FirstOrDefaultAsync(v => v.MaKmsp == id || v.MaVoucher == id);

            if (vSp != null)
            {
                if (vSp.ChiTietHoaDon.Any())
                {
                    vSp.ChoPhepCongDon = false;
                    int daDung = vSp.ChiTietHoaDon.Count(c => c.MaHoaDonNavigation != null && c.MaHoaDonNavigation.TrangThai != "Cancelled");
                    vSp.SoLuongToiThieu = Math.Max(1, daDung);

                    if (!string.IsNullOrEmpty(vSp.MaVoucher) && !vSp.MaVoucher.StartsWith("[DA_HUY]_"))
                    {
                        string prefix = "[DA_HUY]_";
                        int maxLen = 30 - prefix.Length;
                        string raw = vSp.MaVoucher;
                        vSp.MaVoucher = prefix + (raw.Length > maxLen ? raw.Substring(0, maxLen) : raw);
                    }

                    await _service.SaveChangesAsync();
                    return Ok(new
                    {
                        message = "Voucher này đã có đơn hàng sử dụng nên không thể xóa khỏi CSDL. Hệ thống đã ẨN voucher (chuyển sang trạng thái Đã kết thúc) để bảo toàn lịch sử hóa đơn.",
                        softDeleted = true
                    });
                }

                _service.Remove(vSp);
                await _service.SaveChangesAsync();
                return Ok(new { message = "Đã xóa voucher thành công khỏi hệ thống.", softDeleted = false });
            }

            return NotFound("Không tìm thấy voucher.");
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

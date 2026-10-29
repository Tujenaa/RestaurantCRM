using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RestaurantCRM.BusinessLogic.Models;
using RestaurantCRM.API.DTOs;

namespace RestaurantCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HoaDonController : ControllerBase
    {
        private readonly RestaurantCRM.BusinessLogic.Services.IRestaurantService _service;

        public HoaDonController(RestaurantCRM.BusinessLogic.Services.IRestaurantService service)
        {
            _service = service;
        }

        // POST: api/HoaDon/KiemTraGioHang - Kiểm tra tính khả dụng và số lượng món ăn trước khi vào thanh toán
        [HttpPost("KiemTraGioHang")]
        public async Task<IActionResult> KiemTraGioHang([FromBody] CartCheckRequest request)
        {
            if (request.Items == null || !request.Items.Any())
            {
                return Ok(new CartCheckResult { HopLe = true });
            }

            var result = new CartCheckResult { HopLe = true };
            var tongNguyenLieuCan = new Dictionary<string, decimal>();
            var monAnList = new List<(OrderItemRequest Item, MonAn MonAn)>();

            foreach (var item in request.Items)
            {
                var monAn = await _service.Query<MonAn>()
                    .Include(m => m.DinhLuong)
                        .ThenInclude(dl => dl.MaNguyenLieuNavigation)
                    .FirstOrDefaultAsync(m => m.MaMon == item.MaMon);

                if (monAn == null)
                {
                    result.HopLe = false;
                    result.ChiTietLoi.Add(new CartItemErrorDetail
                    {
                        MaMon = item.MaMon,
                        TenMon = "Món không xác định",
                        SoLuongYeuCau = item.SoLuong,
                        SoLuongKhaDung = 0,
                        LyDo = $"Không tìm thấy món ăn với mã '{item.MaMon}' trong hệ thống."
                    });
                    continue;
                }

                // 1. Kiểm tra trạng thái đang kinh doanh
                if (string.Equals(monAn.TrangThai, "Discontinued", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(monAn.TrangThai, "Inactive", StringComparison.OrdinalIgnoreCase))
                {
                    result.HopLe = false;
                    result.ChiTietLoi.Add(new CartItemErrorDetail
                    {
                        MaMon = item.MaMon,
                        TenMon = monAn.TenMon,
                        SoLuongYeuCau = item.SoLuong,
                        SoLuongKhaDung = 0,
                        LyDo = $"Món '{monAn.TenMon}' hiện đã ngừng kinh doanh."
                    });
                    continue;
                }

                // 2. Kiểm tra trạng thái hết hàng chủ động
                if (string.Equals(monAn.TrangThai, "OutOfStock", StringComparison.OrdinalIgnoreCase))
                {
                    result.HopLe = false;
                    result.ChiTietLoi.Add(new CartItemErrorDetail
                    {
                        MaMon = item.MaMon,
                        TenMon = monAn.TenMon,
                        SoLuongYeuCau = item.SoLuong,
                        SoLuongKhaDung = 0,
                        LyDo = $"Món '{monAn.TenMon}' hiện tạm hết hàng."
                    });
                    continue;
                }

                // 3. Kiểm tra công thức định lượng
                if (!monAn.DinhLuong.Any())
                {
                    result.HopLe = false;
                    result.ChiTietLoi.Add(new CartItemErrorDetail
                    {
                        MaMon = item.MaMon,
                        TenMon = monAn.TenMon,
                        SoLuongYeuCau = item.SoLuong,
                        SoLuongKhaDung = 0,
                        LyDo = $"Món '{monAn.TenMon}' chưa có công thức định lượng, tạm thời chưa thể phục vụ."
                    });
                    continue;
                }

                monAnList.Add((item, monAn));

                // Gom nguyên vật liệu cần tiêu thụ theo BOM
                foreach (var dl in monAn.DinhLuong)
                {
                    var can = (decimal)item.SoLuong * dl.SoLuongCan;
                    if (tongNguyenLieuCan.ContainsKey(dl.MaNguyenLieu))
                        tongNguyenLieuCan[dl.MaNguyenLieu] += can;
                    else
                        tongNguyenLieuCan[dl.MaNguyenLieu] = can;
                }
            }

            // 4. Kiểm tra tồn kho nguyên vật liệu
            if (tongNguyenLieuCan.Any())
            {
                var dsMaNl = tongNguyenLieuCan.Keys.ToList();
                var dbNguyenLieu = await _service.Query<NguyenLieu>()
                    .Where(nl => dsMaNl.Contains(nl.MaNguyenLieu))
                    .ToListAsync();

                var thieuNguyenLieu = false;
                foreach (var maNl in dsMaNl)
                {
                    var nl = dbNguyenLieu.FirstOrDefault(x => x.MaNguyenLieu == maNl);
                    var canDung = tongNguyenLieuCan[maNl];
                    var tonHienTai = nl?.SoLuongTon ?? 0;
                    if (nl == null || tonHienTai < canDung)
                    {
                        thieuNguyenLieu = true;
                        break;
                    }
                }

                if (thieuNguyenLieu)
                {
                    result.HopLe = false;
                    // Tính số suất tối đa khả dụng cho từng món ăn dựa trên nguyên liệu tồn
                    foreach (var (item, monAn) in monAnList)
                    {
                        int maxSuat = (int)monAn.DinhLuong.Min(dl =>
                        {
                            var nl = dbNguyenLieu.FirstOrDefault(x => x.MaNguyenLieu == dl.MaNguyenLieu);
                            var ton = nl?.SoLuongTon ?? 0;
                            if (dl.SoLuongCan <= 0) return 0;
                            return Math.Floor(ton / dl.SoLuongCan);
                        });
                        if (maxSuat < 0) maxSuat = 0;

                        if (item.SoLuong > maxSuat)
                        {
                            result.ChiTietLoi.Add(new CartItemErrorDetail
                            {
                                MaMon = item.MaMon,
                                TenMon = monAn.TenMon,
                                SoLuongYeuCau = item.SoLuong,
                                SoLuongKhaDung = maxSuat,
                                LyDo = maxSuat > 0 
                                    ? $"Món '{monAn.TenMon}' chỉ còn tối đa {maxSuat} phần (bạn đang chọn {item.SoLuong} phần)."
                                    : $"Món '{monAn.TenMon}' hiện đã hết nguyên liệu để phục vụ."
                            });
                        }
                    }
                }
            }

            if (!result.HopLe)
            {
                var messages = result.ChiTietLoi.Select(c => c.LyDo).Where(m => !string.IsNullOrEmpty(m)).Distinct();
                result.ThongBao = string.Join(" ", messages);
            }

            return Ok(result);
        }

        // POST: api/HoaDon - Tạo đơn hàng mới
        [HttpPost]
        public async Task<IActionResult> CreateOrder(OrderRequest request)
        {
            if (request.Items == null || !request.Items.Any())
            {
                return BadRequest("Đơn hàng phải có ít nhất 1 món.");
            }

            using var transaction = await _service.BeginTransactionAsync();
            try
            {
                var maHoaDon = await RestaurantCRM.BusinessLogic.Services.CodeGenerator.GenerateMaHoaDonAsync(_service);

                var isOnline = string.Equals(request.PhuongThucThanhToan, "online", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(request.PhuongThucThanhToan, "transfer", StringComparison.OrdinalIgnoreCase);

                string? resolvedMaNhanVien = null;
                if (!string.IsNullOrEmpty(request.MaNhanVien))
                {
                    var nv = await _service.Query<NhanVien>().FirstOrDefaultAsync(n => n.MaNhanVien == request.MaNhanVien || n.TenDangNhap == request.MaNhanVien);
                    resolvedMaNhanVien = nv != null ? nv.MaNhanVien : request.MaNhanVien;
                }

                var initialStatus = !string.IsNullOrEmpty(resolvedMaNhanVien) ? "Confirmed" : "Pending";

                var hoaDon = new HoaDon
                {
                    MaHoaDon = maHoaDon,
                    MaKhachHang = string.IsNullOrWhiteSpace(request.MaKhachHang) ? null : request.MaKhachHang,
                    MaNhanVien = resolvedMaNhanVien,
                    TenKhachHang = string.IsNullOrWhiteSpace(request.TenKhachHang) ? "Khách mua mang đi" : request.TenKhachHang,
                    SoDienThoai = request.SoDienThoai,
                    DiaChiGiao = string.IsNullOrWhiteSpace(request.DiaChiGiao) ? "Bán mang đi tại quầy" : request.DiaChiGiao,
                    NgayDat = DateTime.Now,
                    TrangThai = initialStatus,
                    TongTienHang = 0,
                    TongThanhToan = 0,
                    TienGiamVoucher = 0
                };

                _service.Add(hoaDon);

                double tongTienHang = 0;
                var today = DateOnly.FromDateTime(DateTime.Now);

                // Lấy tất cả các món đang được sale hợp lệ
                var activeDishPromos = await _service.Query<KmTheoSp>()
                    .Include(c => c.MaChuongTrinhNavigation)
                    .Where(c => c.MaChuongTrinhNavigation != null
                             && (c.MaChuongTrinhNavigation.NgayBatDau == null || c.MaChuongTrinhNavigation.NgayBatDau <= today)
                             && (c.MaChuongTrinhNavigation.NgayKetThuc == null || c.MaChuongTrinhNavigation.NgayKetThuc >= today))
                    .ToListAsync();

                // Bước 1: Tính toán tổng nguyên vật liệu cần tiêu thụ theo công thức BOM
                var tongNguyenLieuCan = new Dictionary<string, decimal>();
                var monAnCache = new Dictionary<string, MonAn>();

                foreach (var item in request.Items)
                {
                    var monAn = await _service.Query<MonAn>()
                        .Include(m => m.DinhLuong)
                            .ThenInclude(dl => dl.MaNguyenLieuNavigation)
                        .FirstOrDefaultAsync(m => m.MaMon == item.MaMon);

                    if (monAn == null)
                    {
                        return BadRequest($"Không tìm thấy món ăn với mã '{item.MaMon}'.");
                    }

                    // Kiểm tra trạng thái đang kinh doanh
                    if (string.Equals(monAn.TrangThai, "Discontinued", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(monAn.TrangThai, "Inactive", StringComparison.OrdinalIgnoreCase))
                    {
                        return BadRequest($"Món ăn '{monAn.TenMon}' hiện đã ngừng kinh doanh, không thể đặt hàng.");
                    }

                    // Kiểm tra trạng thái hết hàng chủ động
                    if (string.Equals(monAn.TrangThai, "OutOfStock", StringComparison.OrdinalIgnoreCase))
                    {
                        return BadRequest($"Món ăn '{monAn.TenMon}' hiện đã hết hàng, vui lòng chọn món khác.");
                    }

                    if (!monAn.DinhLuong.Any())
                    {
                        return BadRequest($"Món ăn '{monAn.TenMon}' chưa được thiết lập công thức định lượng, không thể đặt món.");
                    }

                    monAnCache[item.MaMon] = monAn;

                    foreach (var dl in monAn.DinhLuong)
                    {
                        var can = (decimal)item.SoLuong * dl.SoLuongCan;
                        if (tongNguyenLieuCan.ContainsKey(dl.MaNguyenLieu))
                            tongNguyenLieuCan[dl.MaNguyenLieu] += can;
                        else
                            tongNguyenLieuCan[dl.MaNguyenLieu] = can;
                    }
                }

                // Bước 2: Kiểm tra tồn kho nguyên liệu (sắp xếp key tăng dần để chống Deadlock)
                var dsMaNl = tongNguyenLieuCan.Keys.OrderBy(k => k).ToList();
                var dbNguyenLieu = await _service.Query<NguyenLieu>()
                    .Where(nl => dsMaNl.Contains(nl.MaNguyenLieu))
                    .ToListAsync();

                foreach (var maNl in dsMaNl)
                {
                    var nl = dbNguyenLieu.FirstOrDefault(x => x.MaNguyenLieu == maNl);
                    var canDung = tongNguyenLieuCan[maNl];
                    if (nl == null || (nl.SoLuongTon ?? 0) < canDung)
                    {
                        string tenNl = nl?.TenNguyenLieu ?? maNl;
                        string dvt = nl?.DonViTinh ?? "";
                        decimal tonHienTai = nl?.SoLuongTon ?? 0;
                        return BadRequest($"Không đủ nguyên liệu '{tenNl}' để thực hiện đơn hàng (Kho chỉ còn: {tonHienTai:N2} {dvt}, cần: {canDung:N2} {dvt}).");
                    }
                }

                // Bước 3: Trừ tồn kho nguyên liệu
                foreach (var maNl in dsMaNl)
                {
                    var nl = dbNguyenLieu.First(x => x.MaNguyenLieu == maNl);
                    nl.SoLuongTon -= tongNguyenLieuCan[maNl];
                }

                foreach (var item in request.Items)
                {
                    var monAn = monAnCache[item.MaMon];
                    if (monAn.DonGia.HasValue)
                    {
                        double donGiaGoc = monAn.DonGia.Value;
                        double donGiaSauGiam = donGiaGoc;
                        double tienGiamMon = 0;
                        string? maChuongTrinhKm = null;
                        int soLuongGiam = 0;

                        // Kiểm tra xem món này có đang Sale không
                        var promo = activeDishPromos.FirstOrDefault(p => p.MaMon == item.MaMon);
                        if (promo != null && promo.MaChuongTrinhNavigation != null)
                        {
                            maChuongTrinhKm = promo.MaKmsp;
                            soLuongGiam = item.SoLuong;

                            if (soLuongGiam > 0)
                            {
                                double discountPerItem = 0;
                                if (promo.PhanTramGiam.HasValue && promo.PhanTramGiam.Value > 0)
                                {
                                    discountPerItem = donGiaGoc * promo.PhanTramGiam.Value / 100.0;
                                }
                                else if (promo.TienGiam.HasValue && promo.TienGiam.Value > 0)
                                {
                                    discountPerItem = promo.TienGiam.Value;
                                }
                                
                                donGiaSauGiam = donGiaGoc - discountPerItem;
                                if (donGiaSauGiam < 0) donGiaSauGiam = 0;
                                
                                tienGiamMon = discountPerItem * soLuongGiam;
                            }
                        }

                        double thanhTien = (donGiaSauGiam * soLuongGiam) + (donGiaGoc * (item.SoLuong - soLuongGiam));

                        var maChiTiet = "CT" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                        var chiTiet = new ChiTietHoaDon
                        {
                            MaChiTiet = maChiTiet,
                            MaHoaDon = maHoaDon,
                            MaMon = item.MaMon,
                            MaKmsp = maChuongTrinhKm,
                            TenMon = monAn.TenMon,
                            SoLuong = item.SoLuong,
                            DonGiaGoc = donGiaGoc,
                            DonGiaSauGiam = soLuongGiam > 0 ? donGiaSauGiam : donGiaGoc,
                            TienGiamMon = tienGiamMon,
                            ThanhTien = thanhTien
                        };

                        _service.Add(chiTiet);
                        tongTienHang += thanhTien;
                    }
                }

                hoaDon.TongTienHang = tongTienHang;
                hoaDon.TongThanhToan = tongTienHang;

                // Áp dụng Voucher (nếu có)
                if (!string.IsNullOrEmpty(request.MaKmvoucher))
                {
                    var voucher = await _service.Query<KmTheoVoucher>()
                        .Include(v => v.MaChuongTrinhNavigation)
                        .FirstOrDefaultAsync(v => v.MaKmvoucher == request.MaKmvoucher || v.MaVoucher == request.MaKmvoucher);

                    if (voucher != null && voucher.MaChuongTrinhNavigation != null)
                    {
                        var ct = voucher.MaChuongTrinhNavigation;
                        if ((ct.NgayBatDau == null || ct.NgayBatDau <= today) &&
                            (ct.NgayKetThuc == null || ct.NgayKetThuc >= today) &&
                            (voucher.GiaTriDonToiThieu == null || hoaDon.TongTienHang >= voucher.GiaTriDonToiThieu))
                        {
                            hoaDon.MaKmvoucher = voucher.MaKmvoucher;
                            
                            double tienGiam = 0;
                            if (voucher.PhanTramGiam.HasValue && voucher.PhanTramGiam.Value > 0)
                            {
                                tienGiam = tongTienHang * voucher.PhanTramGiam.Value / 100.0;
                                if (voucher.GiamToiDa.HasValue && voucher.GiamToiDa.Value > 0 && tienGiam > voucher.GiamToiDa.Value)
                                {
                                    tienGiam = voucher.GiamToiDa.Value;
                                }
                            }
                            else if (voucher.TienGiam.HasValue && voucher.TienGiam.Value > 0)
                            {
                                tienGiam = voucher.TienGiam.Value;
                            }

                            hoaDon.TienGiamVoucher = tienGiam;
                            hoaDon.TongThanhToan = tongTienHang - tienGiam;
                            if (hoaDon.TongThanhToan < 0) hoaDon.TongThanhToan = 0;
                        }
                    }
                }

                // Luôn luôn lưu lại bản ghi phương thức thanh toán cho đơn hàng (COD hoặc Online/Tại quầy)
                string phuongThucTen;
                string trangThaiThanhToan;
                DateTime? ngayThanhToan;

                if (isOnline)
                {
                    phuongThucTen = "Chuyển khoản ngân hàng (VietQR / Online)";
                    trangThaiThanhToan = "Paid";
                    ngayThanhToan = DateTime.Now;
                }
                else if (!string.IsNullOrEmpty(resolvedMaNhanVien))
                {
                    phuongThucTen = !string.IsNullOrWhiteSpace(request.PhuongThucThanhToan)
                        ? request.PhuongThucThanhToan
                        : "Tiền mặt (Tại quầy)";
                    trangThaiThanhToan = "Paid";
                    ngayThanhToan = DateTime.Now;
                }
                else
                {
                    phuongThucTen = "Tiền mặt khi nhận hàng (COD)";
                    trangThaiThanhToan = "Pending";
                    ngayThanhToan = null;
                }

                var thanhToan = new ThanhToan
                {
                    MaThanhToan = "TT" + DateTime.Now.Ticks.ToString().Substring(8, 6),
                    MaHoaDon = maHoaDon,
                    PhuongThuc = phuongThucTen,
                    TrangThai = trangThaiThanhToan,
                    NgayThanhToan = ngayThanhToan,
                    SoTien = hoaDon.TongThanhToan
                };
                _service.Add(thanhToan);

                // Lưu lại lịch sử trạng thái
                var logNote = !string.IsNullOrEmpty(resolvedMaNhanVien)
                    ? $"Đơn hàng mang đi/tại quầy do nhân viên tạo."
                    : (isOnline 
                        ? "Đơn hàng mới được tạo (Khách đã chọn chuyển khoản, đang chờ nhân viên nhà hàng xác nhận)" 
                        : "Đơn hàng mới được tạo (Khách chọn thanh toán tiền mặt khi nhận hàng COD, đang chờ nhân viên nhà hàng xác nhận)");

                var lichSu = new LichSuTrangThai
                {
                    MaLichSu = "LS" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    MaHoaDon = maHoaDon,
                    TrangThai = initialStatus,
                    ThoiGian = DateTime.Now,
                    GhiChu = logNote
                };
                _service.Add(lichSu);

                await _service.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Đặt hàng thành công", maHoaDon = hoaDon.MaHoaDon });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi hệ thống: " + ex.Message + " | Inner: " + ex.InnerException?.Message);
            }
        }

        // PUT: api/HoaDon/{id}/HuyDon - Khách hàng hủy đơn (chỉ khi Pending)
        [HttpPut("{id}/HuyDon")]
        public async Task<IActionResult> CancelOrder(string id, [FromQuery] string maKhachHang)
        {
            var hoaDon = await _service.Query<HoaDon>()
                .Include(d => d.ChiTietHoaDon)
                .FirstOrDefaultAsync(d => d.MaHoaDon == id && d.MaKhachHang == maKhachHang);

            if (hoaDon == null)
                return NotFound("Không tìm thấy đơn hàng của bạn.");

            if (hoaDon.TrangThai != "Pending")
                return BadRequest("Đơn hàng đã được xác nhận hoặc đang xử lý, không thể tự hủy.");

            using var transaction = await _service.BeginTransactionAsync();
            try
            {
                // Hoàn lại kho nguyên vật liệu theo định lượng BOM
                foreach (var chiTiet in hoaDon.ChiTietHoaDon)
                {
                    var dinhLuongs = await _service.Query<DinhLuong>().Where(dl => dl.MaMon == chiTiet.MaMon).ToListAsync();
                    foreach (var dl in dinhLuongs)
                    {
                        var nl = await _service.FindAsync<NguyenLieu>(dl.MaNguyenLieu);
                        if (nl != null)
                        {
                            nl.SoLuongTon = (nl.SoLuongTon ?? 0) + (decimal)(chiTiet.SoLuong ?? 0) * dl.SoLuongCan;
                        }
                    }
                }
                hoaDon.TrangThai = "Cancelled";
                
                var lichSu = new LichSuTrangThai
                {
                    MaLichSu = "LS" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    MaHoaDon = id,
                    TrangThai = "Cancelled",
                    ThoiGian = DateTime.Now,
                    GhiChu = "Khách hàng tự hủy đơn"
                };
                _service.Add(lichSu);

                await _service.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok("Hủy đơn hàng thành công.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi hệ thống: " + ex.Message);
            }
        }

        // GET: api/HoaDon/LichSu/{maKhachHang} - Lấy lịch sử đơn hàng của khách
        [HttpGet("LichSu/{maKhachHang}")]
        public async Task<IActionResult> GetLichSuHoaDon(string maKhachHang)
        {
            var hoaDons = await _service.Query<HoaDon>()
                .Where(d => d.MaKhachHang == maKhachHang)
                .OrderByDescending(d => d.NgayDat)
                .AsNoTracking()
                .Select(d => new 
                {
                    d.MaHoaDon,
                    NgayDat = d.NgayDat,
                    d.TrangThai,
                    d.TongThanhToan,
                    SoMon = d.ChiTietHoaDon.Sum(c => c.SoLuong)
                })
                .ToListAsync();

            return Ok(hoaDons);
        }

        // GET: api/HoaDon/{id} - Lấy chi tiết một đơn hàng
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChiTietHoaDon(string id)
        {
            var hoaDon = await _service.Query<HoaDon>()
                .Include(d => d.ChiTietHoaDon)
                    .ThenInclude(c => c.MaMonNavigation)
                .Include(d => d.ThanhToan)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaHoaDon == id);

            if (hoaDon == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            var result = new
            {
                hoaDon.MaHoaDon,
                NgayDat = hoaDon.NgayDat,
                hoaDon.TrangThai,
                hoaDon.DiaChiGiao,
                hoaDon.TongTienHang,
                hoaDon.TienGiamVoucher,
                hoaDon.TongThanhToan,
                PhuongThucThanhToan = hoaDon.ThanhToan.FirstOrDefault()?.PhuongThuc ?? "COD",
                ChiTiet = hoaDon.ChiTietHoaDon.Select(c => new
                {
                    c.MaMon,
                    TenMon = c.MaMonNavigation?.TenMon ?? c.TenMon,
                    DuongDanAnh = c.MaMonNavigation?.DuongDanAnh,
                    c.SoLuong,
                    c.DonGiaGoc,
                    c.DonGiaSauGiam,
                    c.TienGiamMon,
                    c.ThanhTien
                })
            };

            return Ok(result);
        }
    }
}

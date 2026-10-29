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

                // Lấy tất cả các món đang được sale hợp lệ (KM_THEO_SP)
                // Chỉ lấy khuyến mãi thường (không có mã voucher). Voucher có mã chỉ áp dụng khi khách nhập đúng mã ở Bước 5.
                var activeDishPromos = await _service.Query<KmTheoSp>()
                    .Include(c => c.MaChuongTrinhNavigation)
                    .Where(c => c.MaChuongTrinhNavigation != null
                             && string.IsNullOrEmpty(c.MaVoucher)
                             && (c.ChoPhepCongDon == null || c.ChoPhepCongDon == true)
                             && (c.MaChuongTrinhNavigation.NgayBatDau == null || c.MaChuongTrinhNavigation.NgayBatDau <= today)
                             && (c.MaChuongTrinhNavigation.NgayKetThuc == null || c.MaChuongTrinhNavigation.NgayKetThuc >= today))
                    .ToListAsync();

                // Lấy tất cả ưu đãi theo loại món hợp lệ (KM_THEO_LOAI) - chỉ khuyến mãi thường, không phải voucher có mã
                var activeCategoryPromos = await _service.Query<KmTheoLoai>()
                    .Include(c => c.MaChuongTrinhNavigation)
                    .Where(c => c.MaChuongTrinhNavigation != null
                             && string.IsNullOrEmpty(c.MaVoucher)
                             && (c.ChoPhepCongDon == null || c.ChoPhepCongDon == true)
                             && (c.MaChuongTrinhNavigation.NgayBatDau == null || c.MaChuongTrinhNavigation.NgayBatDau <= today)
                             && (c.MaChuongTrinhNavigation.NgayKetThuc == null || c.MaChuongTrinhNavigation.NgayKetThuc >= today))
                    .ToListAsync();

                var createdLines = new List<(ChiTietHoaDon Line, MonAn Mon)>();

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

                // Bước 4: Áp dụng Tầng 1 - So sánh lấy khuyến mãi cao nhất giữa Món ăn và Loại món (không cộng dồn)
                foreach (var item in request.Items)
                {
                    var monAn = monAnCache[item.MaMon];
                    if (monAn.DonGia.HasValue)
                    {
                        double donGiaGoc = monAn.DonGia.Value;
                        double donGiaSauGiam = donGiaGoc;
                        double tienGiamMon = 0;
                        string? maKmsp = null;
                        string? maKmLoai = null;
                        int soLuong = item.SoLuong;

                        // 1. Kiểm tra KM theo Món (KM_THEO_SP)
                        double discountSP = 0;
                        KmTheoSp? bestDishPromo = null;
                        var dishPromos = activeDishPromos.Where(p => p.MaMon == item.MaMon).ToList();
                        foreach (var p in dishPromos)
                        {
                            double disc = 0;
                            if (p.PhanTramGiam.HasValue && p.PhanTramGiam.Value > 0)
                                disc = donGiaGoc * p.PhanTramGiam.Value / 100.0;
                            else if (p.TienGiam.HasValue && p.TienGiam.Value > 0)
                                disc = p.TienGiam.Value;
                            if (p.GiamToiDa.HasValue && p.GiamToiDa.Value > 0 && disc > p.GiamToiDa.Value)
                                disc = p.GiamToiDa.Value;

                            if (disc > discountSP)
                            {
                                discountSP = disc;
                                bestDishPromo = p;
                            }
                        }

                        // 2. Kiểm tra KM theo Loại Món (KM_THEO_LOAI)
                        double discountLoai = 0;
                        KmTheoLoai? bestCategoryPromo = null;
                        if (!string.IsNullOrEmpty(monAn.MaLoaiMon))
                        {
                            var catPromos = activeCategoryPromos.Where(p => p.MaLoaiMon == monAn.MaLoaiMon).ToList();
                            foreach (var p in catPromos)
                            {
                                if (p.GiaTriDonToiThieu.HasValue && donGiaGoc < p.GiaTriDonToiThieu.Value)
                                    continue; // Chỉ áp dụng cho món có đơn giá >= GiaTriDonToiThieu

                                double disc = 0;
                                if (p.PhanTramGiam.HasValue && p.PhanTramGiam.Value > 0)
                                    disc = donGiaGoc * p.PhanTramGiam.Value / 100.0;
                                else if (p.TienGiam.HasValue && p.TienGiam.Value > 0)
                                    disc = p.TienGiam.Value;
                                if (p.GiamToiDa.HasValue && p.GiamToiDa.Value > 0 && disc > p.GiamToiDa.Value)
                                    disc = p.GiamToiDa.Value;

                                if (disc > discountLoai)
                                {
                                    discountLoai = disc;
                                    bestCategoryPromo = p;
                                }
                            }
                        }

                        // 3. Quy tắc người dùng: Không cộng dồn, lấy mức giảm cao nhất Max(SP, Loai)
                        double chosenDiscount = 0;
                        if (discountSP >= discountLoai && discountSP > 0 && bestDishPromo != null)
                        {
                            chosenDiscount = discountSP;
                            maKmsp = bestDishPromo.MaKmsp;
                        }
                        else if (discountLoai > discountSP && discountLoai > 0 && bestCategoryPromo != null)
                        {
                            chosenDiscount = discountLoai;
                            maKmLoai = bestCategoryPromo.MaKMLoai;
                        }

                        donGiaSauGiam = Math.Max(0, donGiaGoc - chosenDiscount);
                        tienGiamMon = chosenDiscount * soLuong;
                        double thanhTien = donGiaSauGiam * soLuong;

                        var maChiTiet = "CT" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                        var chiTiet = new ChiTietHoaDon
                        {
                            MaChiTiet = maChiTiet,
                            MaHoaDon = maHoaDon,
                            MaMon = item.MaMon,
                            MaKmsp = maKmsp,
                            MaKmLoai = maKmLoai,
                            SoLuong = soLuong,
                            DonGiaGoc = donGiaGoc,
                            DonGiaSauGiam = donGiaSauGiam,
                            TienGiamMon = tienGiamMon,
                            ThanhTien = thanhTien
                        };

                        _service.Add(chiTiet);
                        createdLines.Add((chiTiet, monAn));
                        tongTienHang += thanhTien;
                    }
                }

                hoaDon.TongTienHang = tongTienHang;
                hoaDon.TongThanhToan = tongTienHang;

                // Bước 5: Áp dụng Tầng 2 - Voucher (toàn đơn / theo loại món / theo món). Server tự tính, không tin số tiền từ client.
                var voucherCode = request.MaKmvoucher?.Trim();
                if (!string.IsNullOrEmpty(voucherCode))
                {
                    var voucherError = await ApplyVoucherAsync(hoaDon, voucherCode, createdLines, tongTienHang, today);
                    if (voucherError != null)
                    {
                        return BadRequest(voucherError);
                    }
                }

                // Lưu lại thanh toán nếu online hoặc nhân viên bán mang đi tại quầy
                if (isOnline || !string.IsNullOrEmpty(resolvedMaNhanVien))
                {
                    string phuongThuc = !string.IsNullOrWhiteSpace(request.PhuongThucThanhToan)
                        ? request.PhuongThucThanhToan
                        : (isOnline ? "Online" : "Tiền mặt (Tại quầy)");

                    var thanhToan = new ThanhToan
                    {
                        MaThanhToan = "TT" + DateTime.Now.Ticks.ToString().Substring(8, 6),
                        MaHoaDon = maHoaDon,
                        PhuongThuc = phuongThuc,
                        TrangThai = "Paid",
                        NgayThanhToan = DateTime.Now,
                        SoTien = hoaDon.TongThanhToan
                    };
                    _service.Add(thanhToan);
                }

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

        /// <summary>
        /// Kiểm tra và áp dụng voucher cho hóa đơn. Trả về thông báo lỗi nếu mã không hợp lệ, ngược lại trả null.
        /// TOAN_DON: tính trên cả đơn. THEO_LOAI / THEO_MON: chỉ tính trên các dòng thuộc loại món / món đó.
        /// </summary>
        private async Task<string?> ApplyVoucherAsync(
            HoaDon hoaDon, string code,
            List<(ChiTietHoaDon Line, MonAn Mon)> lines, double tongTienHang, DateOnly today)
        {
            ChuongTrinhKhuyenMai? ct = null;
            string? maVoucher = null;
            bool? choPhep = null;
            int? gioiHanDung = null;
            int daDung = 0;
            double? pct = null, tien = null, giamToiDa = null, donToiThieu = null;
            string scope;
            KmTheoDonHang? vDon = null;
            KmTheoSp? vSp = null;
            KmTheoLoai? vLoai = null;

            vDon = await _service.Query<KmTheoDonHang>()
                .Include(v => v.MaChuongTrinhNavigation)
                .FirstOrDefaultAsync(v => v.MaVoucher == code || v.MaKMDonHang == code);
            if (vDon != null)
            {
                scope = "TOAN_DON";
                ct = vDon.MaChuongTrinhNavigation; maVoucher = vDon.MaVoucher; choPhep = vDon.ChoPhepCongDon;
                gioiHanDung = vDon.SoLuongToiThieu; pct = vDon.PhanTramGiam; tien = vDon.TienGiam;
                giamToiDa = vDon.GiamToiDa; donToiThieu = vDon.GiaTriDonToiThieu;
                var maKm = vDon.MaKMDonHang;
                daDung = await _service.Query<HoaDon>().CountAsync(h => h.MaKMDonHang == maKm && h.TrangThai != "Cancelled");
            }
            else
            {
                vLoai = await _service.Query<KmTheoLoai>()
                    .Include(v => v.MaChuongTrinhNavigation)
                    .FirstOrDefaultAsync(v => v.MaVoucher == code || (v.MaKMLoai == code && v.MaVoucher != null && v.MaVoucher != ""));
                if (vLoai != null)
                {
                    scope = "THEO_LOAI";
                    ct = vLoai.MaChuongTrinhNavigation; maVoucher = vLoai.MaVoucher; choPhep = vLoai.ChoPhepCongDon;
                    gioiHanDung = vLoai.SoLuongToiThieu; pct = vLoai.PhanTramGiam; tien = vLoai.TienGiam;
                    giamToiDa = vLoai.GiamToiDa; donToiThieu = vLoai.GiaTriDonToiThieu;
                    var maKm = vLoai.MaKMLoai;
                    daDung = await _service.Query<ChiTietHoaDon>().CountAsync(c => c.MaKmLoai == maKm
                        && c.MaHoaDonNavigation != null && c.MaHoaDonNavigation.TrangThai != "Cancelled");
                }
                else
                {
                    vSp = await _service.Query<KmTheoSp>()
                        .Include(v => v.MaChuongTrinhNavigation)
                        .FirstOrDefaultAsync(v => v.MaVoucher == code || (v.MaKmsp == code && v.MaVoucher != null && v.MaVoucher != ""));
                    if (vSp == null) return "Mã voucher không tồn tại.";

                    scope = "THEO_MON";
                    ct = vSp.MaChuongTrinhNavigation; maVoucher = vSp.MaVoucher; choPhep = vSp.ChoPhepCongDon;
                    gioiHanDung = vSp.SoLuongToiThieu; pct = vSp.PhanTramGiam; tien = vSp.TienGiam;
                    giamToiDa = vSp.GiamToiDa; donToiThieu = null;
                    var maKm = vSp.MaKmsp;
                    daDung = await _service.Query<ChiTietHoaDon>().CountAsync(c => c.MaKmsp == maKm
                        && c.MaHoaDonNavigation != null && c.MaHoaDonNavigation.TrangThai != "Cancelled");
                }
            }

            // Kiểm tra hiệu lực chung
            if (ct == null) return "Chương trình khuyến mãi không tồn tại.";
            if (choPhep == false || (maVoucher != null && maVoucher.StartsWith("[DA_HUY]_")))
                return "Mã voucher này đã bị ẩn hoặc hủy áp dụng.";
            if (ct.NgayBatDau.HasValue && today < ct.NgayBatDau.Value) return "Chương trình khuyến mãi chưa bắt đầu.";
            if (ct.NgayKetThuc.HasValue && today > ct.NgayKetThuc.Value) return "Mã voucher đã hết hạn sử dụng.";
            if (gioiHanDung.HasValue && gioiHanDung.Value > 0 && daDung >= gioiHanDung.Value)
                return "Mã voucher này đã hết lượt sử dụng.";
            if ((!pct.HasValue || pct <= 0) && (!tien.HasValue || tien <= 0))
                return "Voucher không còn hiệu lực giảm giá.";

            // Xác định phần tiền (sau giảm giá món) được áp dụng voucher theo phạm vi
            double baseAmount = tongTienHang;
            List<(ChiTietHoaDon Line, MonAn Mon)> scopedLines = lines;
            if (scope == "THEO_MON")
                scopedLines = lines.Where(l => l.Line.MaMon == vSp!.MaMon).ToList();
            else if (scope == "THEO_LOAI")
                scopedLines = lines.Where(l => l.Mon.MaLoaiMon == vLoai!.MaLoaiMon).ToList();

            if (scope != "TOAN_DON")
            {
                baseAmount = scopedLines.Sum(l => l.Line.ThanhTien ?? 0);
                if (baseAmount <= 0)
                    return scope == "THEO_MON"
                        ? "Mã này chỉ áp dụng cho một món nhất định mà đơn hàng không có."
                        : "Mã này chỉ áp dụng cho một nhóm món nhất định mà đơn hàng không có.";
            }

            if (donToiThieu.HasValue && donToiThieu.Value > 0 && baseAmount < donToiThieu.Value)
                return $"Mã này yêu cầu giá trị áp dụng tối thiểu {donToiThieu.Value:N0} đ.";

            double tienGiam = 0;
            if (pct.HasValue && pct.Value > 0)
            {
                tienGiam = baseAmount * pct.Value / 100.0;
                if (giamToiDa.HasValue && giamToiDa.Value > 0 && tienGiam > giamToiDa.Value) tienGiam = giamToiDa.Value;
            }
            else if (tien.HasValue && tien.Value > 0)
            {
                tienGiam = tien.Value;
            }
            tienGiam = Math.Min(tienGiam, baseAmount);

            hoaDon.TienGiamVoucher = tienGiam;
            hoaDon.TongThanhToan = Math.Max(0, tongTienHang - tienGiam);

            // Ghi nhận voucher đã dùng để đếm lượt sử dụng
            if (scope == "TOAN_DON")
            {
                hoaDon.MaKMDonHang = vDon!.MaKMDonHang;
            }
            else
            {
                // Gắn mã voucher vào dòng đầu tiên thuộc phạm vi chưa gắn khuyến mãi nào
                var tagLine = scopedLines.Select(l => l.Line).FirstOrDefault(l => l.MaKmsp == null && l.MaKmLoai == null);
                if (tagLine != null)
                {
                    if (scope == "THEO_MON") tagLine.MaKmsp = vSp!.MaKmsp;
                    else tagLine.MaKmLoai = vLoai!.MaKMLoai;
                }
            }

            return null;
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
                    d.NgayDat,
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
                hoaDon.NgayDat,
                hoaDon.TrangThai,
                hoaDon.DiaChiGiao,
                hoaDon.TongTienHang,
                hoaDon.TienGiamVoucher,
                hoaDon.TongThanhToan,
                PhuongThucThanhToan = hoaDon.ThanhToan.FirstOrDefault()?.PhuongThuc ?? "COD",
                ChiTiet = hoaDon.ChiTietHoaDon.Select(c => new
                {
                    c.MaMon,
                    TenMon = c.MaMonNavigation?.TenMon,
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

using System.Net.Http.Json;
using System.Text.Json;
using RestaurantCRM.Web.Models;

namespace RestaurantCRM.Web.Services;

public class DonHangService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DonHangService> _logger;

    // Fallback demo storage in case API is offline
    private readonly List<DonHangViewModel> _fallbackOrders = new()
    {
        new()
        {
            MaDonHang = "DH00231",
            MaKhachHang = "KH01",
            NgayDat = DateTime.Today.AddHours(-2),
            TongTienHang = 507000,
            TienGiamVoucher = 20000,
            TongThanhToan = 487000,
            TrangThai = "Pending",
            TrangThaiLoc = "active",
            DiaChiGiao = "12 Lê Lợi, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh",
            BuocHienTai = 1,
            TienTrinh = Steps(),
            ChiTiet = new()
            {
                new() { MaMon = "M01", TenMon = "Lẩu Thái chua cay", SoLuong = 1, DonGiaGoc = 180000, DonGiaSauGiam = 180000, ThanhTien = 180000 },
                new() { MaMon = "M05", TenMon = "Bò Mỹ thái lát", SoLuong = 2, DonGiaGoc = 120000, DonGiaSauGiam = 120000, ThanhTien = 240000 },
                new() { MaMon = "M09", TenMon = "Rau lẩu thập cẩm", SoLuong = 1, DonGiaGoc = 50000, DonGiaSauGiam = 50000, ThanhTien = 50000 }
            }
        },
        new()
        {
            MaDonHang = "DH00198",
            MaKhachHang = "KH01",
            NgayDat = DateTime.Today.AddDays(-6),
            TongTienHang = 310000,
            TienGiamVoucher = 0,
            TongThanhToan = 310000,
            TrangThai = "Completed",
            TrangThaiLoc = "done",
            DiaChiGiao = "45 Nguyễn Thị Minh Khai, Quận 1, TP. Hồ Chí Minh",
            BuocHienTai = 4,
            TienTrinh = Steps(),
            ChiTiet = new()
            {
                new() { MaMon = "M03", TenMon = "Lẩu nấm thanh đạm", SoLuong = 1, DonGiaGoc = 170000, DonGiaSauGiam = 170000, ThanhTien = 170000 },
                new() { MaMon = "M06", TenMon = "Tôm sú tươi", SoLuong = 1, DonGiaGoc = 140000, DonGiaSauGiam = 140000, ThanhTien = 140000 }
            }
        }
    };

    public DonHangService(IHttpClientFactory httpClientFactory, ILogger<DonHangService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<(bool Success, string? OrderId, string? ErrorMessage)> CreateOrderAsync(
        DatHangViewModel customer,
        string? maKhachHang,
        string? voucherCode,
        IReadOnlyList<GioHangItemViewModel> items)
    {
        if (items == null || items.Count == 0)
        {
            return (false, null, "Giỏ hàng đang trống, vui lòng chọn ít nhất 1 món ăn.");
        }

        string customerId = string.IsNullOrWhiteSpace(maKhachHang) ? "KH01" : maKhachHang;
        string address = customer.HinhThucNhan == "pickup"
            ? "Nhận tại nhà hàng Lẩu Phố (12 Lê Lợi, Q.1)"
            : $"{customer.DiaChiGiao}, {customer.TinhThanh}".Trim().Trim(',');

        var payload = new
        {
            MaKhachHang = customerId,
            TenKhachHang = customer.HoTen,
            SoDienThoai = customer.SoDienThoai,
            DiaChiGiao = address,
            MaKmvoucher = string.IsNullOrWhiteSpace(voucherCode) ? null : voucherCode.Trim(),
            PhuongThucThanhToan = customer.PhuongThucThanhToan,
            Items = items.Select(i => new
            {
                MaMon = i.MaMon,
                SoLuong = i.SoLuong
            }).ToList()
        };

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PostAsJsonAsync("/api/HoaDon", payload);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiOrderCreateResponse>();
                string orderId = result?.MaHoaDon ?? ("DH" + DateTime.Now.ToString("yyMMddHHmmss"));
                return (true, orderId, null);
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("API HoaDon trả về lỗi đặt hàng: {Error}", errorBody);
                return (false, null, string.IsNullOrWhiteSpace(errorBody) ? "Đặt hàng không thành công. Vui lòng thử lại." : errorBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi API HoaDon tạo đơn hàng.");

            // Fallback: save to memory
            var fallbackId = "DH" + DateTime.Now.ToString("yyMMddHHmmss");
            _fallbackOrders.Insert(0, new DonHangViewModel
            {
                MaDonHang = fallbackId,
                MaKhachHang = customerId,
                NgayDat = DateTime.Now,
                TongThanhToan = items.Sum(x => x.ThanhTien),
                TongTienHang = items.Sum(x => x.ThanhTien),
                TrangThai = "Pending",
                TrangThaiLoc = "active",
                DiaChiGiao = address,
                BuocHienTai = 1,
                TienTrinh = Steps(),
                ChiTiet = items.Select(x => new DonHangChiTietViewModel
                {
                    MaMon = x.MaMon,
                    TenMon = x.TenMon,
                    SoLuong = x.SoLuong,
                    DonGiaGoc = x.DonGia,
                    DonGiaSauGiam = x.DonGia,
                    ThanhTien = x.ThanhTien
                }).ToList()
            });

            return (true, fallbackId, null);
        }
    }

    public async Task<List<DonHangViewModel>> GetOrdersForCustomerAsync(string? maKhachHang, string filter = "all")
    {
        string customerId = string.IsNullOrWhiteSpace(maKhachHang) ? "KH01" : maKhachHang;

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/HoaDon/LichSu/{Uri.EscapeDataString(customerId)}");

            if (response.IsSuccessStatusCode)
            {
                var list = await response.Content.ReadFromJsonAsync<List<ApiOrderSummaryDto>>();
                if (list != null && list.Count > 0)
                {
                    var orders = new List<DonHangViewModel>();

                    foreach (var s in list)
                    {
                        var detail = await GetOrderDetailAsync(s.MaHoaDon);
                        if (detail != null)
                        {
                            orders.Add(detail);
                        }
                        else
                        {
                            var statusLoc = MapStatusLoc(s.TrangThai);
                            orders.Add(new DonHangViewModel
                            {
                                MaDonHang = s.MaHoaDon,
                                MaKhachHang = customerId,
                                NgayDat = s.NgayDat,
                                TongThanhToan = (decimal)s.TongThanhToan,
                                TrangThai = MapStatusText(s.TrangThai),
                                TrangThaiLoc = statusLoc,
                                BuocHienTai = MapStep(s.TrangThai),
                                TienTrinh = Steps()
                            });
                        }
                    }

                    return FilterOrders(orders, filter);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lỗi khi lấy lịch sử đơn hàng từ API, dùng dữ liệu fallback.");
        }

        return FilterOrders(_fallbackOrders, filter);
    }

    public async Task<DonHangViewModel?> GetOrderDetailAsync(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId)) return null;

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/HoaDon/{Uri.EscapeDataString(orderId)}");

            if (response.IsSuccessStatusCode)
            {
                var dto = await response.Content.ReadFromJsonAsync<ApiOrderDetailDto>();
                if (dto != null)
                {
                    var statusLoc = MapStatusLoc(dto.TrangThai);
                    return new DonHangViewModel
                    {
                        MaDonHang = dto.MaHoaDon,
                        NgayDat = dto.NgayDat,
                        DiaChiGiao = dto.DiaChiGiao ?? "",
                        TrangThai = MapStatusText(dto.TrangThai),
                        TrangThaiLoc = statusLoc,
                        TongTienHang = (decimal)dto.TongTienHang,
                        TienGiamVoucher = (decimal)dto.TienGiamVoucher,
                        TongThanhToan = (decimal)dto.TongThanhToan,
                        BuocHienTai = MapStep(dto.TrangThai),
                        TienTrinh = Steps(),
                        ChiTiet = (dto.ChiTiet ?? new()).Select(c => new DonHangChiTietViewModel
                        {
                            MaMon = c.MaMon,
                            TenMon = c.TenMon ?? "Món lẩu",
                            SoLuong = c.SoLuong,
                            DonGiaGoc = (decimal)c.DonGiaGoc,
                            DonGiaSauGiam = (decimal)c.DonGiaSauGiam,
                            TienGiamMon = (decimal)c.TienGiamMon,
                            ThanhTien = (decimal)c.ThanhTien
                        }).ToList()
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lỗi khi lấy chi tiết đơn hàng #{OrderId} từ API.", orderId);
        }

        return _fallbackOrders.FirstOrDefault(o => o.MaDonHang == orderId);
    }

    public async Task<(bool Success, string Message)> CancelOrderAsync(string orderId, string? maKhachHang)
    {
        string customerId = string.IsNullOrWhiteSpace(maKhachHang) ? "KH01" : maKhachHang;

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PutAsync($"/api/HoaDon/{Uri.EscapeDataString(orderId)}/HuyDon?maKhachHang={Uri.EscapeDataString(customerId)}", null);

            if (response.IsSuccessStatusCode)
            {
                return (true, "Hủy đơn hàng thành công. Các món ăn đã được hoàn trả kho phục vụ.");
            }
            else
            {
                var msg = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(msg) ? "Không thể hủy đơn hàng này." : msg);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi API hủy đơn hàng #{OrderId}", orderId);
            var local = _fallbackOrders.FirstOrDefault(o => o.MaDonHang == orderId);
            if (local != null)
            {
                local.TrangThai = "Cancelled";
                local.TrangThaiLoc = "cancel";
                local.BuocHienTai = 0;
                return (true, "Hủy đơn hàng thành công (chế độ demo).");
            }
            return (false, "Lỗi hệ thống khi hủy đơn.");
        }
    }

    // Synchronous helpers for compatibility
    public IReadOnlyList<DonHangViewModel> GetForDemoCustomer(string state = "all")
        => FilterOrders(_fallbackOrders, state);

    public string Create(DatHangViewModel customer, decimal total, IReadOnlyList<GioHangItemViewModel> items)
    {
        var id = "DH" + DateTime.Now.ToString("yyMMddHHmmss");
        _fallbackOrders.Insert(0, new DonHangViewModel
        {
            MaDonHang = id,
            MaKhachHang = "KH01",
            NgayDat = DateTime.Now,
            TongThanhToan = total,
            TongTienHang = total,
            TrangThai = "Pending",
            TrangThaiLoc = "active",
            BuocHienTai = 1,
            TienTrinh = Steps(),
            DiaChiGiao = customer.DiaChiGiao,
            ChiTiet = items.Select(x => new DonHangChiTietViewModel
            {
                MaMon = x.MaMon,
                TenMon = x.TenMon,
                SoLuong = x.SoLuong,
                DonGiaGoc = x.DonGia,
                DonGiaSauGiam = x.DonGia,
                ThanhTien = x.ThanhTien
            }).ToList()
        });
        return id;
    }

    public DonHangViewModel? Find(string id) => _fallbackOrders.FirstOrDefault(x => x.MaDonHang == id);

    private static List<DonHangViewModel> FilterOrders(IEnumerable<DonHangViewModel> orders, string filter)
    {
        return orders
            .Where(x => filter == "all" || x.TrangThaiLoc == filter)
            .OrderByDescending(x => x.NgayDat)
            .ToList();
    }

    private static string MapStatusLoc(string? status)
    {
        var s = (status ?? "").ToLowerInvariant();
        if (s == "completed" || s.Contains("hoàn tất") || s == "delivered") return "done";
        if (s == "cancelled" || s.Contains("hủy") || s == "rejected") return "cancel";
        return "active"; // Pending, Confirmed, Preparing, Shipping
    }

    private static string MapStatusText(string? status)
    {
        var s = (status ?? "").ToLowerInvariant();
        if (s == "pending") return "Đang chờ xác nhận";
        if (s == "confirmed") return "Đã xác nhận";
        if (s == "preparing") return "Đang chuẩn bị";
        if (s == "shipping" || s == "delivering") return "Đang giao hàng (Đã thanh toán online)";
        if (s == "completed") return "Đã hoàn tất";
        if (s == "cancelled") return "Đã hủy đơn";
        return status ?? "Đang xử lý";
    }

    private static int MapStep(string? status)
    {
        var s = (status ?? "").ToLowerInvariant();
        if (s == "pending") return 1;
        if (s == "confirmed") return 2;
        if (s == "preparing") return 3;
        if (s == "shipping" || s == "delivering") return 4;
        if (s == "completed") return 5;
        if (s == "cancelled") return 0;
        return 1;
    }

    private static List<string> Steps() => new() { "Đã đặt", "Đã xác nhận", "Đang chuẩn bị", "Đang giao", "Hoàn tất" };
}

// Internal DTOs
internal class ApiOrderCreateResponse
{
    public string? Message { get; set; }
    public string? MaHoaDon { get; set; }
}

internal class ApiOrderSummaryDto
{
    public string MaHoaDon { get; set; } = "";
    public DateTime NgayDat { get; set; }
    public string TrangThai { get; set; } = "";
    public double TongThanhToan { get; set; }
    public int SoMon { get; set; }
}

internal class ApiOrderDetailDto
{
    public string MaHoaDon { get; set; } = "";
    public DateTime NgayDat { get; set; }
    public string TrangThai { get; set; } = "";
    public string? DiaChiGiao { get; set; }
    public double TongTienHang { get; set; }
    public double TienGiamVoucher { get; set; }
    public double TongThanhToan { get; set; }
    public List<ApiOrderDetailItemDto>? ChiTiet { get; set; }
}

internal class ApiOrderDetailItemDto
{
    public string MaMon { get; set; } = "";
    public string? TenMon { get; set; }
    public int SoLuong { get; set; }
    public double DonGiaGoc { get; set; }
    public double DonGiaSauGiam { get; set; }
    public double TienGiamMon { get; set; }
    public double ThanhTien { get; set; }
}

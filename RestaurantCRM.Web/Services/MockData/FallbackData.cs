using System;
using System.Collections.Generic;
using RestaurantCRM.Web.Models;

namespace RestaurantCRM.Web.Services.MockData;

/// <summary>
/// Kho lưu trữ dữ liệu mẫu dự phòng (fallback demo data) cho website khách hàng khi API chưa sẵn sàng hoặc offline.
/// </summary>
public static class FallbackData
{
    public static List<string> DefaultSteps() => new() { "Đã đặt", "Đã xác nhận", "Đang chuẩn bị", "Đang giao", "Hoàn tất" };

    public static List<PhanHoiItemViewModel> CreateFallbackReviews()
    {
        return new List<PhanHoiItemViewModel>
        {
            new()
            {
                MaDanhGia = "DG001",
                MaMon = "LAU001",
                TenMon = "Lẩu Thái chua cay",
                DuongDanAnh = "https://images.unsplash.com/photo-1547928576-a4a33237cbc3?auto=format&fit=crop&w=800&q=80",
                MaDonHang = "DH00198",
                TenKhachHang = "Nguyễn Văn Hùng",
                DanhGia = 5,
                NoiDung = "Nước lẩu Thái rất đậm đà, vị chua cay hài hòa, hải sản kèm theo tươi giòn. Giao hàng đúng giờ!",
                NgayGui = DateTime.Now.AddDays(-1).AddHours(-3),
                TraLois = new List<TraLoiDanhGiaViewModel>
                {
                    new()
                    {
                        MaTraLoi = "TL002",
                        NguoiGui = "KhachHang",
                        NguoiTraLoi = "Nguyễn Văn Hùng",
                        NoiDung = "Dạ lần sau mình sẽ gọi thêm phần thịt bò Mỹ nữa nhé quán!",
                        NgayGui = DateTime.Now.AddHours(-12)
                    },
                    new()
                    {
                        MaTraLoi = "TL001",
                        NguoiGui = "NhanVien",
                        NguoiTraLoi = "Quản lý Lẩu Phố",
                        NoiDung = "Lẩu Phố cảm ơn anh Hùng rất nhiều ạ! Chúc anh có những bữa ăn ngon miệng cùng gia đình.",
                        NgayGui = DateTime.Now.AddDays(-1).AddHours(-1)
                    }
                }
            },
            new()
            {
                MaDanhGia = "DG002",
                MaMon = "THIT001",
                TenMon = "Bò Mỹ thái lát",
                DuongDanAnh = "https://images.unsplash.com/photo-1558030006-450675393462?auto=format&fit=crop&w=800&q=80",
                MaDonHang = "DH00231",
                TenKhachHang = "Trần Thị Mai",
                DanhGia = 4,
                NoiDung = "Thịt bò mềm tan, vân mỡ đều đẹp. Đóng khay giữ lạnh cẩn thận.",
                NgayGui = DateTime.Now.AddDays(-2).AddHours(-5),
                TraLois = new List<TraLoiDanhGiaViewModel>
                {
                    new()
                    {
                        MaTraLoi = "TL003",
                        NguoiGui = "NhanVien",
                        NguoiTraLoi = "Chăm sóc khách hàng Lẩu Phố",
                        NoiDung = "Lẩu Phố chân thành cảm ơn chị Mai. Nhà hàng luôn cam kết nhập thịt bò chuẩn chứng nhận mỗi ngày ạ.",
                        NgayGui = DateTime.Now.AddDays(-2).AddHours(-3)
                    }
                }
            },
            new()
            {
                MaDanhGia = "DG003",
                MaMon = "M03",
                TenMon = "Lẩu nấm thanh đạm",
                DuongDanAnh = "https://images.unsplash.com/photo-1504674900247-0877df9cc836?auto=format&fit=crop&w=800&q=80",
                MaDonHang = "DH00198",
                TenKhachHang = "Lê Hoàng Long",
                DanhGia = 5,
                NoiDung = "Nước dùng thanh ngọt tự nhiên không gắt bột ngọt, các loại nấm tươi sạch ăn rất nhẹ bụng.",
                NgayGui = DateTime.Now.AddDays(-4),
                TraLois = new List<TraLoiDanhGiaViewModel>()
            },
            new()
            {
                MaDanhGia = "DG004",
                MaMon = "M06",
                TenMon = "Tôm sú tươi",
                DuongDanAnh = "https://images.unsplash.com/photo-1565680018434-b513d5e5fd47?auto=format&fit=crop&w=800&q=80",
                MaDonHang = "DH00198",
                TenKhachHang = "Phạm Quỳnh Chi",
                DanhGia = 4,
                NoiDung = "Tôm sú tươi giòn, nhúng lẩu vị ngọt lịm. Shipper giao rất thân thiện.",
                NgayGui = DateTime.Now.AddDays(-6),
                TraLois = new List<TraLoiDanhGiaViewModel>()
            }
        };
    }

    public static List<DonHangViewModel> CreateFallbackOrders()
    {
        return new List<DonHangViewModel>
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
                TienTrinh = DefaultSteps(),
                ChiTiet = new()
                {
                    new() { MaMon = "LAU001", TenMon = "Lẩu Thái chua cay", DuongDanAnh = "https://images.unsplash.com/photo-1547928576-a4a33237cbc3?auto=format&fit=crop&w=800&q=80", SoLuong = 1, DonGiaGoc = 180000, DonGiaSauGiam = 180000, ThanhTien = 180000 },
                    new() { MaMon = "THIT001", TenMon = "Bò Mỹ thái lát", DuongDanAnh = "https://images.unsplash.com/photo-1558030006-450675393462?auto=format&fit=crop&w=800&q=80", SoLuong = 2, DonGiaGoc = 120000, DonGiaSauGiam = 120000, ThanhTien = 240000 },
                    new() { MaMon = "RAU001", TenMon = "Rau lẩu thập cẩm", DuongDanAnh = "https://images.unsplash.com/photo-1540420773420-3366772f4999?auto=format&fit=crop&w=800&q=80", SoLuong = 1, DonGiaGoc = 50000, DonGiaSauGiam = 50000, ThanhTien = 50000 }
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
                BuocHienTai = 5,
                TienTrinh = DefaultSteps(),
                ChiTiet = new()
                {
                    new() { MaMon = "LAU003", TenMon = "Lẩu nấm thanh đạm", DuongDanAnh = "https://images.unsplash.com/photo-1504674900247-0877df9cc836?auto=format&fit=crop&w=800&q=80", SoLuong = 1, DonGiaGoc = 170000, DonGiaSauGiam = 170000, ThanhTien = 170000 },
                    new() { MaMon = "THIT002", TenMon = "Tôm sú tươi", DuongDanAnh = "https://images.unsplash.com/photo-1565680018434-b513d5e5fd47?auto=format&fit=crop&w=800&q=80", SoLuong = 1, DonGiaGoc = 140000, DonGiaSauGiam = 140000, ThanhTien = 140000 }
                }
            }
        };
    }
}

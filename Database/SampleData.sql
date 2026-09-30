-- ============================================
-- DỮ LIỆU MẪU: Hệ thống quản lý đặt món & CRM khách hàng
-- SQL Server | Chạy sau khi đã chạy RestaurantCRM.sql
-- Lưu ý: 
--   * matKhau dùng chuỗi băm MD5 mẫu của "123456" (chỉ để test)
--   * Ngày giờ dùng định dạng ISO (yyyy-mm-ddThh:mm:ss) để không phụ thuộc cấu hình ngôn ngữ
--   * tienGiamMon trong CHI_TIET_DON_HANG là số tiền giảm trên MỖI đơn vị món
--   * tongTienHang = tổng thanhTien các dòng chi tiết
--   * tongThanhToan = tongTienHang - tienGiamVoucher
-- ============================================

USE RestaurantCRM;
GO

-- 1. Tài khoản & phân quyền
INSERT INTO TAI_KHOAN_ADMIN (maAdmin, tenDangNhap, matKhau, hoTen) VALUES
('AD01', N'admin', 'e10adc3949ba59abbe56e057f20f883e', N'Nguyễn Quốc Bảo'),
('AD02', N'admin2', 'e10adc3949ba59abbe56e057f20f883e', N'Trần Minh Khôi');

INSERT INTO VAI_TRO (maVaiTro, tenVaiTro) VALUES
('VT01', N'Quản lý'),
('VT02', N'Thu ngân'),
('VT03', N'Phục vụ'),
('VT04', N'Đầu bếp'),
('VT05', N'Thủ kho');

INSERT INTO NHAN_VIEN (maNhanVien, maVaiTro, tenDangNhap, matKhau, hoTen, trangThai) VALUES
('NV01', 'VT01', N'quanly01', 'e10adc3949ba59abbe56e057f20f883e', N'Lê Thị Hồng Nhung', N'Hoạt động'),
('NV02', 'VT02', N'thungan01', 'e10adc3949ba59abbe56e057f20f883e', N'Phạm Văn Đức', N'Hoạt động'),
('NV03', 'VT02', N'thungan02', 'e10adc3949ba59abbe56e057f20f883e', N'Võ Thị Mai', N'Hoạt động'),
('NV04', 'VT03', N'phucvu01', 'e10adc3949ba59abbe56e057f20f883e', N'Đặng Hoàng Long', N'Hoạt động'),
('NV05', 'VT04', N'bep01', 'e10adc3949ba59abbe56e057f20f883e', N'Bùi Quang Vinh', N'Hoạt động'),
('NV06', 'VT05', N'kho01', 'e10adc3949ba59abbe56e057f20f883e', N'Ngô Thị Thu Hà', N'Hoạt động'),
('NV07', 'VT03', N'phucvu02', 'e10adc3949ba59abbe56e057f20f883e', N'Huỳnh Minh Tuấn', N'Nghỉ việc');

INSERT INTO KHACH_HANG (maKhachHang, hoTen, soDienThoai, email, matKhau, ngaySinh, gioiTinh, soThich, trangThai) VALUES
('KH01', N'Nguyễn Thị Lan Anh', '0901234561', N'lananh.nguyen@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '1995-03-14', N'Nữ', N'Món Việt, đồ uống ít đường', N'Hoạt động'),
('KH02', N'Trần Văn Minh', '0902345672', N'minh.tran@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '1990-07-22', N'Nam', N'Cơm tấm, cà phê', N'Hoạt động'),
('KH03', N'Lê Hoàng Phúc', '0903456783', N'phuc.le@yahoo.com', 'e10adc3949ba59abbe56e057f20f883e', '1988-11-05', N'Nam', N'Bò, đồ nướng', N'Hoạt động'),
('KH04', N'Phạm Ngọc Diệp', '0904567894', N'diep.pham@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '1993-01-30', N'Nữ', N'Lẩu, hải sản', N'Hoạt động'),
('KH05', N'Võ Thanh Tùng', '0905678905', N'tung.vo@outlook.com', 'e10adc3949ba59abbe56e057f20f883e', '1999-09-18', N'Nam', N'Bún, phở', N'Hoạt động'),
('KH06', N'Đặng Thị Kim Ngân', '0906789016', N'ngan.dang@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '2000-05-02', N'Nữ', N'Tráng miệng, trà trái cây', N'Hoạt động'),
('KH07', N'Bùi Anh Khoa', '0907890127', N'khoa.bui@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '1985-12-25', N'Nam', N'Lẩu, món cay', N'Hoạt động'),
('KH08', N'Hồ Thị Thanh Thảo', '0908901238', N'thao.ho@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '1997-08-09', N'Nữ', N'Món ăn nhẹ, ăn chay', N'Hoạt động'),
('KH09', N'Ngô Đức Thịnh', '0909012349', N'thinh.ngo@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '1982-04-17', N'Nam', N'Đồ nướng, bia', N'Hoạt động'),
('KH10', N'Dương Mỹ Linh', '0910123450', N'linh.duong@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '1996-10-11', N'Nữ', N'Chả giò, bún chả', N'Hoạt động'),
('KH11', N'Trịnh Quốc Việt', '0911234561', N'viet.trinh@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '1992-02-28', N'Nam', N'Cơm, món xào', N'Hoạt động'),
('KH12', N'Lý Bảo Châu', '0912345672', N'chau.ly@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '2001-06-06', N'Nữ', N'Đồ ngọt, cà phê', N'Khóa');

-- 2. Thực đơn
INSERT INTO LOAI_MON (maLoaiMon, tenLoaiMon) VALUES
('LM01', N'Khai vị'),
('LM02', N'Món chính'),
('LM03', N'Lẩu & Nướng'),
('LM04', N'Tráng miệng'),
('LM05', N'Đồ uống');

INSERT INTO MON_AN (maMon, maLoaiMon, tenMon, moTa, donGia, soLuong, trangThai) VALUES
('M01', 'LM01', N'Gỏi cuốn tôm thịt', N'Bánh tráng cuốn tôm, thịt heo, bún, rau thơm; chấm tương đậu phộng', 45000, 80, N'Còn hàng'),
('M02', 'LM01', N'Chả giò hải sản', N'Chả giò chiên giòn nhân tôm, mực, cua', 50000, 70, N'Còn hàng'),
('M03', 'LM02', N'Phở bò tái', N'Phở nước dùng hầm xương 12 giờ, thịt bò tái mềm', 65000, 60, N'Còn hàng'),
('M04', 'LM02', N'Bún chả Hà Nội', N'Bún tươi, chả viên và chả miếng nướng than, nước mắm chua ngọt', 60000, 50, N'Còn hàng'),
('M05', 'LM02', N'Cơm tấm sườn bì chả', N'Cơm tấm với sườn nướng, bì, chả trứng, mỡ hành', 55000, 90, N'Còn hàng'),
('M06', 'LM02', N'Bò lúc lắc', N'Thăn bò Úc xào tỏi, ăn kèm khoai tây chiên và salad', 120000, 40, N'Còn hàng'),
('M07', 'LM03', N'Lẩu Thái hải sản', N'Nước lẩu chua cay, tôm, mực, nghêu, cá; dành cho 2-3 người', 350000, 20, N'Còn hàng'),
('M08', 'LM03', N'Sườn nướng mật ong', N'Sườn heo ướp mật ong, nướng than hoa', 150000, 35, N'Còn hàng'),
('M09', 'LM04', N'Chè khúc bạch', N'Khúc bạch phô mai, hạnh nhân, vải thiều', 35000, 45, N'Còn hàng'),
('M10', 'LM04', N'Bánh flan caramel', N'Bánh flan mềm mịn, caramel đắng nhẹ', 25000, 60, N'Còn hàng'),
('M11', 'LM05', N'Trà đào cam sả', N'Trà đào tươi, cam vàng, sả thơm', 40000, 100, N'Còn hàng'),
('M12', 'LM05', N'Cà phê sữa đá', N'Cà phê phin truyền thống pha sữa đặc', 30000, 120, N'Còn hàng'),
('M13', 'LM05', N'Nước ép cam', N'Cam sành vắt tươi, không đường', 40000, 0, N'Hết hàng');

INSERT INTO HINH_ANH_MON_AN (maHinhAnh, maMon, duongDanAnh, laAnhDaiDien, thuTuHienThi) VALUES
('HA01', 'M01', N'/images/mon/goi-cuon-tom-thit.jpg', 1, 1),
('HA02', 'M02', N'/images/mon/cha-gio-hai-san.jpg', 1, 1),
('HA03', 'M03', N'/images/mon/pho-bo-tai.jpg', 1, 1),
('HA04', 'M04', N'/images/mon/bun-cha-ha-noi.jpg', 1, 1),
('HA05', 'M05', N'/images/mon/com-tam-suon-bi-cha.jpg', 1, 1),
('HA06', 'M06', N'/images/mon/bo-luc-lac.jpg', 1, 1),
('HA07', 'M07', N'/images/mon/lau-thai-hai-san.jpg', 1, 1),
('HA08', 'M08', N'/images/mon/suon-nuong-mat-ong.jpg', 1, 1),
('HA09', 'M09', N'/images/mon/che-khuc-bach.jpg', 1, 1),
('HA10', 'M10', N'/images/mon/banh-flan.jpg', 1, 1),
('HA11', 'M11', N'/images/mon/tra-dao-cam-sa.jpg', 1, 1),
('HA12', 'M12', N'/images/mon/ca-phe-sua-da.jpg', 1, 1),
('HA13', 'M13', N'/images/mon/nuoc-ep-cam.jpg', 1, 1),
('HA14', 'M07', N'/images/mon/lau-thai-hai-san-2.jpg', 0, 2),
('HA15', 'M03', N'/images/mon/pho-bo-tai-2.jpg', 0, 2);

-- 3. Nhà cung cấp & nhập hàng
INSERT INTO NHA_CUNG_CAP (maNhaCungCap, tenNhaCungCap, soDienThoai, diaChi, email) VALUES
('NCC01', N'Công ty TNHH Thực phẩm Sạch Việt', '02838123456', N'12 Nguyễn Văn Linh, Quận 7, TP.HCM', N'contact@thucphamsachviet.vn'),
('NCC02', N'Cơ sở Thịt bò Ba Miền', '02838234567', N'85 Quốc lộ 22, Hóc Môn, TP.HCM', N'thitbobamien@gmail.com'),
('NCC03', N'Công ty CP Đồ uống Sài Gòn', '02839345678', N'245 Điện Biên Phủ, Quận Bình Thạnh, TP.HCM', N'sales@douongsaigon.vn'),
('NCC04', N'Cửa hàng Bánh & Nguyên liệu Ngọt', '02838456789', N'56 Lê Văn Sỹ, Quận 3, TP.HCM', N'nguyenlieungot@gmail.com'),
('NCC05', N'Hải sản Tươi Phú Quốc', '02839567890', N'Chợ đầu mối Bình Điền, Quận 8, TP.HCM', N'haisanphuquoc@gmail.com');

INSERT INTO PHIEU_NHAP_HANG (maPhieuNhap, maNhaCungCap, maNhanVien, ngayNhap, tongTien, ghiChu) VALUES
('PN01', 'NCC01', 'NV06', '2026-08-25T08:30:00', 3400000, N'Nhập phở và bún chả đợt đầu tháng'),
('PN02', 'NCC02', 'NV06', '2026-08-26T09:00:00', 5250000, N'Thịt bò và sườn tươi'),
('PN03', 'NCC03', 'NV06', '2026-08-28T14:15:00', 3700000, N'Nguyên liệu đồ uống'),
('PN04', 'NCC04', 'NV06', '2026-08-30T10:00:00', 2160000, N'Nguyên liệu tráng miệng'),
('PN05', 'NCC05', 'NV06', '2026-09-02T07:45:00', 5720000, N'Hải sản cho lẩu và nước ép');

INSERT INTO CHI_TIET_PHIEU_NHAP (maChiTiet, maPhieuNhap, maMon, soLuong, donGiaNhap, thanhTien) VALUES
('CTPN01', 'PN01', 'M03', 50, 40000, 2000000),
('CTPN02', 'PN01', 'M04', 40, 35000, 1400000),
('CTPN03', 'PN02', 'M06', 30, 80000, 2400000),
('CTPN04', 'PN02', 'M08', 30, 95000, 2850000),
('CTPN05', 'PN03', 'M11', 100, 22000, 2200000),
('CTPN06', 'PN03', 'M12', 100, 15000, 1500000),
('CTPN07', 'PN04', 'M09', 60, 20000, 1200000),
('CTPN08', 'PN04', 'M10', 80, 12000, 960000),
('CTPN09', 'PN05', 'M07', 20, 220000, 4400000),
('CTPN10', 'PN05', 'M13', 60, 22000, 1320000);

-- 4. Khuyến mãi
-- loaiKhuyenMai: VOUCHER (giảm trên đơn) | MON (giảm trên món)
-- loaiGiam: PHAN_TRAM | TIEN
INSERT INTO CHUONG_TRINH_KHUYEN_MAI (maChuongTrinh, tenChuongTrinh, loaiKhuyenMai, loaiGiam, giaTriGiam, giaTriDonToiThieu, ngayBatDau, ngayKetThuc, trangThai) VALUES
('KM01', N'Giảm 10% cho đơn từ 200.000đ', 'VOUCHER', 'PHAN_TRAM', 10, 200000, '2026-08-01', '2026-12-31', N'Đang áp dụng'),
('KM02', N'Giảm 30.000đ cho đơn từ 150.000đ', 'VOUCHER', 'TIEN', 30000, 150000, '2026-08-01', '2026-12-31', N'Đang áp dụng'),
('KM03', N'Đồ uống trái cây giảm 20%', 'MON', 'PHAN_TRAM', 20, 0, '2026-08-01', '2026-10-31', N'Đang áp dụng'),
('KM04', N'Tráng miệng giảm 15%', 'MON', 'PHAN_TRAM', 15, 0, '2026-08-01', '2026-12-31', N'Đang áp dụng'),
('KM05', N'Ưu đãi khách VIP giảm 15% đơn từ 500.000đ', 'VOUCHER', 'PHAN_TRAM', 15, 500000, '2026-08-01', '2026-12-31', N'Đang áp dụng'),
('KM06', N'Phở bò giảm 10.000đ', 'MON', 'TIEN', 10000, 0, '2026-08-01', '2026-11-30', N'Đang áp dụng');

INSERT INTO CHI_TIET_KHUYEN_MAI_MON (maChiTietKM, maChuongTrinh, maMon, soLuongApDung) VALUES
('CTKM01', 'KM03', 'M11', 5),
('CTKM02', 'KM03', 'M13', 5),
('CTKM03', 'KM04', 'M09', 5),
('CTKM04', 'KM04', 'M10', 5),
('CTKM05', 'KM06', 'M03', 5);

-- 5. Đơn hàng
-- maNhanVien = NULL: khách tự đặt online, chưa có nhân viên tiếp nhận
INSERT INTO DON_HANG (maDonHang, maKhachHang, maNhanVien, maChuongTrinhVoucher, diaChiGiao, ngayDat, trangThai, tongTienHang, tienGiamVoucher, tongThanhToan) VALUES
('DH01', 'KH01', 'NV02', NULL,   N'25 Lê Lợi, Quận 1, TP.HCM',               '2026-09-01T11:30:00', N'Hoàn thành',    174000,  0,      174000),
('DH02', 'KH02', 'NV02', 'KM02', N'102 Cách Mạng Tháng 8, Quận 3, TP.HCM',   '2026-09-03T18:45:00', N'Hoàn thành',    170000,  30000,  140000),
('DH03', 'KH03', 'NV03', 'KM01', N'45 Nguyễn Trãi, Quận 5, TP.HCM',          '2026-09-05T19:10:00', N'Hoàn thành',    269500,  26950,  242550),
('DH04', 'KH04', 'NV02', 'KM05', N'88 Phan Xích Long, Quận Phú Nhuận, TP.HCM','2026-09-08T12:00:00', N'Hoàn thành',    564000,  84600,  479400),
('DH05', 'KH05', 'NV03', NULL,   N'17 Lý Thường Kiệt, Quận 10, TP.HCM',      '2026-09-10T18:20:00', N'Hoàn thành',    170000,  0,      170000),
('DH06', 'KH06', NULL,   'KM02', N'9 Nguyễn Thị Minh Khai, Quận 3, TP.HCM',  '2026-09-29T19:00:00', N'Đang giao',     239500,  30000,  209500),
('DH07', 'KH07', 'NV03', 'KM01', N'63 Võ Văn Tần, Quận 3, TP.HCM',           '2026-09-15T20:00:00', N'Hoàn thành',    490000,  49000,  441000),
('DH08', 'KH08', NULL,   NULL,   N'31 Trần Hưng Đạo, Quận 1, TP.HCM',        '2026-09-18T10:15:00', N'Đã hủy',        85000,   0,      85000),
('DH09', 'KH09', 'NV04', 'KM05', N'120 Hoàng Văn Thụ, Quận Tân Bình, TP.HCM','2026-09-20T19:30:00', N'Hoàn thành',    804000,  120600, 683400),
('DH10', 'KH10', 'NV02', 'KM02', N'54 Điện Biên Phủ, Quận Bình Thạnh, TP.HCM','2026-09-22T12:40:00', N'Hoàn thành',   192000,  30000,  162000),
('DH11', 'KH01', 'NV03', NULL,   N'25 Lê Lợi, Quận 1, TP.HCM',               '2026-09-30T11:00:00', N'Đang chuẩn bị', 149750,  0,      149750),
('DH12', 'KH12', NULL,   'KM01', N'77 Lê Văn Sỹ, Quận Phú Nhuận, TP.HCM',    '2026-09-30T11:20:00', N'Chờ xác nhận',  415000,  41500,  373500);

INSERT INTO CHI_TIET_DON_HANG (maChiTiet, maDonHang, maMon, maChuongTrinhKM, soLuong, donGiaGoc, tienGiamMon, donGiaSauGiam, thanhTien) VALUES
-- DH01
('CT001', 'DH01', 'M03', 'KM06', 2, 65000,  10000, 55000,  110000),
('CT002', 'DH01', 'M11', 'KM03', 2, 40000,  8000,  32000,  64000),
-- DH02
('CT003', 'DH02', 'M05', NULL,   2, 55000,  0,     55000,  110000),
('CT004', 'DH02', 'M12', NULL,   2, 30000,  0,     30000,  60000),
-- DH03
('CT005', 'DH03', 'M06', NULL,   1, 120000, 0,     120000, 120000),
('CT006', 'DH03', 'M01', NULL,   2, 45000,  0,     45000,  90000),
('CT007', 'DH03', 'M09', 'KM04', 2, 35000,  5250,  29750,  59500),
-- DH04
('CT008', 'DH04', 'M07', NULL,   1, 350000, 0,     350000, 350000),
('CT009', 'DH04', 'M08', NULL,   1, 150000, 0,     150000, 150000),
('CT010', 'DH04', 'M11', 'KM03', 2, 40000,  8000,  32000,  64000),
-- DH05
('CT011', 'DH05', 'M04', NULL,   2, 60000,  0,     60000,  120000),
('CT012', 'DH05', 'M02', NULL,   1, 50000,  0,     50000,  50000),
-- DH06
('CT013', 'DH06', 'M05', NULL,   3, 55000,  0,     55000,  165000),
('CT014', 'DH06', 'M10', 'KM04', 2, 25000,  3750,  21250,  42500),
('CT015', 'DH06', 'M13', 'KM03', 1, 40000,  8000,  32000,  32000),
-- DH07
('CT016', 'DH07', 'M07', NULL,   1, 350000, 0,     350000, 350000),
('CT017', 'DH07', 'M01', NULL,   2, 45000,  0,     45000,  90000),
('CT018', 'DH07', 'M02', NULL,   1, 50000,  0,     50000,  50000),
-- DH08
('CT019', 'DH08', 'M03', 'KM06', 1, 65000,  10000, 55000,  55000),
('CT020', 'DH08', 'M12', NULL,   1, 30000,  0,     30000,  30000),
-- DH09
('CT021', 'DH09', 'M07', NULL,   1, 350000, 0,     350000, 350000),
('CT022', 'DH09', 'M06', NULL,   2, 120000, 0,     120000, 240000),
('CT023', 'DH09', 'M08', NULL,   1, 150000, 0,     150000, 150000),
('CT024', 'DH09', 'M13', 'KM03', 2, 40000,  8000,  32000,  64000),
-- DH10
('CT025', 'DH10', 'M04', NULL,   1, 60000,  0,     60000,  60000),
('CT026', 'DH10', 'M02', NULL,   2, 50000,  0,     50000,  100000),
('CT027', 'DH10', 'M11', 'KM03', 1, 40000,  8000,  32000,  32000),
-- DH11
('CT028', 'DH11', 'M06', NULL,   1, 120000, 0,     120000, 120000),
('CT029', 'DH11', 'M09', 'KM04', 1, 35000,  5250,  29750,  29750),
-- DH12
('CT030', 'DH12', 'M08', NULL,   2, 150000, 0,     150000, 300000),
('CT031', 'DH12', 'M05', NULL,   1, 55000,  0,     55000,  55000),
('CT032', 'DH12', 'M12', NULL,   2, 30000,  0,     30000,  60000);

INSERT INTO THANH_TOAN (maThanhToan, maDonHang, phuongThuc, trangThai, ngayThanhToan, soTien) VALUES
('TT01', 'DH01', N'Ví MoMo',        N'Đã thanh toán',  '2026-09-01T11:35:00', 174000),
('TT02', 'DH02', N'Tiền mặt',       N'Đã thanh toán',  '2026-09-03T19:30:00', 140000),
('TT03', 'DH03', N'Chuyển khoản',   N'Đã thanh toán',  '2026-09-05T19:15:00', 242550),
('TT04', 'DH04', N'Thẻ tín dụng',   N'Đã thanh toán',  '2026-09-08T12:05:00', 479400),
('TT05', 'DH05', N'Tiền mặt',       N'Đã thanh toán',  '2026-09-10T19:00:00', 170000),
('TT06', 'DH06', N'COD',            N'Chờ thanh toán', NULL,                  209500),
('TT07', 'DH07', N'Chuyển khoản',   N'Đã thanh toán',  '2026-09-15T20:05:00', 441000),
('TT08', 'DH08', N'Ví MoMo',        N'Đã hoàn tiền',   '2026-09-18T10:20:00', 85000),
('TT09', 'DH09', N'Thẻ tín dụng',   N'Đã thanh toán',  '2026-09-20T19:35:00', 683400),
('TT10', 'DH10', N'Ví ZaloPay',     N'Đã thanh toán',  '2026-09-22T12:45:00', 162000),
('TT11', 'DH11', N'Tiền mặt',       N'Chờ thanh toán', NULL,                  149750),
('TT12', 'DH12', N'Chuyển khoản',   N'Chờ thanh toán', NULL,                  373500);

INSERT INTO DANH_GIA (maDanhGia, maKhachHang, maDonHang, maMon, soSao, noiDung, ngayDanhGia) VALUES
('DG01', 'KH01', 'DH01', 'M03', 5, N'Nước phở đậm đà, thịt bò mềm, rất đáng thử.', '2026-09-01T13:00:00'),
('DG02', 'KH01', 'DH01', 'M11', 4, N'Trà đào thơm, hơi ngọt so với khẩu vị của mình.', '2026-09-01T13:02:00'),
('DG03', 'KH02', 'DH02', 'M05', 4, N'Sườn nướng thơm, cơm dẻo, phần ăn vừa đủ.', '2026-09-03T20:00:00'),
('DG04', 'KH03', 'DH03', 'M06', 5, N'Bò lúc lắc mềm, tỏi phi thơm, sẽ quay lại.', '2026-09-05T21:00:00'),
('DG05', 'KH03', 'DH03', 'M09', 5, N'Chè khúc bạch mát lạnh, topping đầy đủ.', '2026-09-05T21:05:00'),
('DG06', 'KH04', 'DH04', 'M07', 5, N'Lẩu Thái hải sản tươi, nước lẩu chua cay vừa miệng.', '2026-09-08T14:00:00'),
('DG07', 'KH05', 'DH05', 'M04', 4, N'Chả nướng thơm mùi than, nước chấm ngon.', '2026-09-10T19:30:00'),
('DG08', 'KH07', 'DH07', 'M07', 3, N'Hải sản tươi nhưng nước lẩu hơi nhạt, mong quán cải thiện.', '2026-09-15T21:30:00'),
('DG09', 'KH09', 'DH09', 'M08', 5, N'Sườn nướng mật ong tuyệt vời, thịt mềm và thơm.', '2026-09-20T21:00:00'),
('DG10', 'KH10', 'DH10', 'M02', 4, N'Chả giò giòn, nhân đầy đặn.', '2026-09-22T14:00:00');

INSERT INTO LICH_SU_TRANG_THAI (maLichSu, maDonHang, trangThai, thoiGian, ghiChu) VALUES
('LS01', 'DH01', N'Chờ xác nhận',   '2026-09-01T11:30:00', N'Khách đặt món online'),
('LS02', 'DH01', N'Đang chuẩn bị',  '2026-09-01T11:35:00', N'Bếp đã nhận đơn'),
('LS03', 'DH01', N'Hoàn thành',     '2026-09-01T12:15:00', N'Giao hàng thành công'),
('LS04', 'DH02', N'Chờ xác nhận',   '2026-09-03T18:45:00', N'Khách đặt món tại quầy'),
('LS05', 'DH02', N'Đang chuẩn bị',  '2026-09-03T18:50:00', N'Bếp đã nhận đơn'),
('LS06', 'DH02', N'Hoàn thành',     '2026-09-03T19:30:00', N'Khách đã dùng bữa và thanh toán'),
('LS07', 'DH03', N'Chờ xác nhận',   '2026-09-05T19:10:00', N'Khách đặt món online'),
('LS08', 'DH03', N'Đang chuẩn bị',  '2026-09-05T19:15:00', N'Bếp đã nhận đơn'),
('LS09', 'DH03', N'Hoàn thành',     '2026-09-05T20:05:00', N'Giao hàng thành công'),
('LS10', 'DH04', N'Chờ xác nhận',   '2026-09-08T12:00:00', N'Khách đặt món online'),
('LS11', 'DH04', N'Đang chuẩn bị',  '2026-09-08T12:10:00', N'Bếp đã nhận đơn'),
('LS12', 'DH04', N'Hoàn thành',     '2026-09-08T13:00:00', N'Giao hàng thành công'),
('LS13', 'DH05', N'Chờ xác nhận',   '2026-09-10T18:20:00', N'Khách đặt món tại quầy'),
('LS14', 'DH05', N'Đang chuẩn bị',  '2026-09-10T18:25:00', N'Bếp đã nhận đơn'),
('LS15', 'DH05', N'Hoàn thành',     '2026-09-10T19:00:00', N'Khách đã dùng bữa và thanh toán'),
('LS16', 'DH06', N'Chờ xác nhận',   '2026-09-29T19:00:00', N'Khách đặt món online'),
('LS17', 'DH06', N'Đang chuẩn bị',  '2026-09-29T19:10:00', N'Bếp đã nhận đơn'),
('LS18', 'DH06', N'Đang giao',      '2026-09-29T19:40:00', N'Shipper đang giao đến khách'),
('LS19', 'DH07', N'Chờ xác nhận',   '2026-09-15T20:00:00', N'Khách đặt món online'),
('LS20', 'DH07', N'Đang chuẩn bị',  '2026-09-15T20:10:00', N'Bếp đã nhận đơn'),
('LS21', 'DH07', N'Hoàn thành',     '2026-09-15T21:00:00', N'Giao hàng thành công'),
('LS22', 'DH08', N'Chờ xác nhận',   '2026-09-18T10:15:00', N'Khách đặt món online'),
('LS23', 'DH08', N'Đã hủy',         '2026-09-18T10:20:00', N'Khách yêu cầu hủy, đã hoàn tiền'),
('LS24', 'DH09', N'Chờ xác nhận',   '2026-09-20T19:30:00', N'Khách đặt món tại bàn'),
('LS25', 'DH09', N'Đang chuẩn bị',  '2026-09-20T19:35:00', N'Bếp đã nhận đơn'),
('LS26', 'DH09', N'Hoàn thành',     '2026-09-20T20:30:00', N'Khách đã dùng bữa và thanh toán'),
('LS27', 'DH10', N'Chờ xác nhận',   '2026-09-22T12:40:00', N'Khách đặt món online'),
('LS28', 'DH10', N'Đang chuẩn bị',  '2026-09-22T12:45:00', N'Bếp đã nhận đơn'),
('LS29', 'DH10', N'Hoàn thành',     '2026-09-22T13:20:00', N'Giao hàng thành công'),
('LS30', 'DH11', N'Chờ xác nhận',   '2026-09-30T11:00:00', N'Khách đặt món online'),
('LS31', 'DH11', N'Đang chuẩn bị',  '2026-09-30T11:10:00', N'Bếp đã nhận đơn'),
('LS32', 'DH12', N'Chờ xác nhận',   '2026-09-30T11:20:00', N'Đơn mới, chờ nhân viên xác nhận');

-- 6. Tương tác & khảo sát (CRM)
INSERT INTO PHAN_HOI (maPhanHoi, maKhachHang, maNhanVien, noiDung, danhGia, ngayPhanHoi, trangThai) VALUES
('PH01', 'KH01', 'NV01', N'Món ăn ngon, giao hàng nhanh. Cảm ơn quán!', 5, '2026-09-01T14:00:00', N'Đã xử lý'),
('PH02', 'KH02', 'NV01', N'Cơm tấm ngon nhưng đợi món hơi lâu vào giờ cao điểm.', 3, '2026-09-03T20:30:00', N'Đã xử lý'),
('PH03', 'KH03', 'NV01', N'Nhân viên phục vụ nhiệt tình, không gian sạch sẽ.', 5, '2026-09-05T21:30:00', N'Đã xử lý'),
('PH04', 'KH07', 'NV01', N'Nước lẩu hơi nhạt so với lần trước, mong quán kiểm tra lại.', 2, '2026-09-15T21:45:00', N'Đã xử lý'),
('PH05', 'KH08', 'NV01', N'Mình đặt nhầm món nên đã hủy đơn, mong quán hỗ trợ hoàn tiền nhanh.', 3, '2026-09-18T10:30:00', N'Đã xử lý'),
('PH06', 'KH09', 'NV01', N'Sườn nướng rất ngon, đề nghị quán có thêm combo nướng cho nhóm.', 5, '2026-09-20T21:15:00', N'Đã xử lý'),
('PH07', 'KH06', NULL,   N'Đơn giao hơi trễ so với dự kiến.', 3, '2026-09-29T20:30:00', N'Chưa xử lý'),
('PH08', 'KH12', NULL,   N'Tài khoản của tôi bị khóa, nhờ quán kiểm tra giúp.', 2, '2026-09-30T09:00:00', N'Chưa xử lý');

INSERT INTO KHAO_SAT (maKhaoSat, maNhanVien, tieuDe, trangThai, ngayTao) VALUES
('KS01', 'NV01', N'Khảo sát mức độ hài lòng về dịch vụ', N'Đang mở', '2026-09-01T09:00:00'),
('KS02', 'NV01', N'Khảo sát sở thích món ăn mới', N'Đang mở', '2026-09-10T09:00:00');

-- loaiCauHoi: CHON_MOT | TU_DIEN
INSERT INTO CAU_HOI_KHAO_SAT (maCauHoi, maKhaoSat, noiDungCauHoi, loaiCauHoi) VALUES
('CH01', 'KS01', N'Bạn đánh giá thế nào về trải nghiệm tổng thể tại quán?', 'CHON_MOT'),
('CH02', 'KS01', N'Bạn hài lòng nhất về điều gì?', 'CHON_MOT'),
('CH03', 'KS01', N'Bạn có góp ý gì để chúng tôi phục vụ tốt hơn?', 'TU_DIEN'),
('CH04', 'KS02', N'Bạn muốn quán bổ sung nhóm món nào?', 'CHON_MOT'),
('CH05', 'KS02', N'Bạn muốn thử món ăn nào nhất?', 'TU_DIEN');

INSERT INTO TUY_CHON_CAU_HOI (maTuyChon, maCauHoi, noiDungTuyChon) VALUES
('TC01', 'CH01', N'Rất hài lòng'),
('TC02', 'CH01', N'Hài lòng'),
('TC03', 'CH01', N'Bình thường'),
('TC04', 'CH01', N'Không hài lòng'),
('TC05', 'CH02', N'Chất lượng món ăn'),
('TC06', 'CH02', N'Tốc độ phục vụ / giao hàng'),
('TC07', 'CH02', N'Giá cả'),
('TC08', 'CH02', N'Thái độ nhân viên'),
('TC09', 'CH04', N'Hải sản'),
('TC10', 'CH04', N'Món chay'),
('TC11', 'CH04', N'Đồ nướng'),
('TC12', 'CH04', N'Tráng miệng');

INSERT INTO DOI_TUONG_KHAO_SAT (maDoiTuong, maKhaoSat, maKhachHang, daHoanThanh) VALUES
('DT01', 'KS01', 'KH01', 1),
('DT02', 'KS01', 'KH02', 1),
('DT03', 'KS01', 'KH03', 1),
('DT04', 'KS01', 'KH04', 1),
('DT05', 'KS01', 'KH05', 1),
('DT06', 'KS01', 'KH06', 0),
('DT07', 'KS01', 'KH07', 0),
('DT08', 'KS01', 'KH08', 0),
('DT09', 'KS01', 'KH09', 0),
('DT10', 'KS01', 'KH10', 0),
('DT11', 'KS02', 'KH01', 1),
('DT12', 'KS02', 'KH02', 1),
('DT13', 'KS02', 'KH03', 0);

INSERT INTO PHIEU_TRA_LOI (maPhieuTraLoi, maKhaoSat, maKhachHang, ngayTraLoi) VALUES
('PT01', 'KS01', 'KH01', '2026-09-02T10:00:00'),
('PT02', 'KS01', 'KH02', '2026-09-04T09:30:00'),
('PT03', 'KS01', 'KH03', '2026-09-06T11:00:00'),
('PT04', 'KS01', 'KH04', '2026-09-09T15:20:00'),
('PT05', 'KS01', 'KH05', '2026-09-11T08:45:00'),
('PT06', 'KS02', 'KH01', '2026-09-12T10:10:00'),
('PT07', 'KS02', 'KH02', '2026-09-13T16:00:00');

-- Câu chọn đáp án: maTuyChon có giá trị, noiDungTuDien = NULL
-- Câu tự luận: maTuyChon = NULL, noiDungTuDien có giá trị
INSERT INTO CAU_TRA_LOI (maCauTraLoi, maPhieuTraLoi, maCauHoi, maTuyChon, noiDungTuDien) VALUES
('CTL01', 'PT01', 'CH01', 'TC01', NULL),
('CTL02', 'PT01', 'CH02', 'TC05', NULL),
('CTL03', 'PT01', 'CH03', NULL,   N'Món ăn ngon, giao hàng đúng giờ.'),
('CTL04', 'PT02', 'CH01', 'TC02', NULL),
('CTL05', 'PT02', 'CH02', 'TC07', NULL),
('CTL06', 'PT02', 'CH03', NULL,   N'Nên có thêm nhiều khuyến mãi cho khách quen.'),
('CTL07', 'PT03', 'CH01', 'TC01', NULL),
('CTL08', 'PT03', 'CH02', 'TC05', NULL),
('CTL09', 'PT03', 'CH03', NULL,   N'Bò lúc lắc rất mềm, sẽ quay lại.'),
('CTL10', 'PT04', 'CH01', 'TC02', NULL),
('CTL11', 'PT04', 'CH02', 'TC08', NULL),
('CTL12', 'PT04', 'CH03', NULL,   N'Nhân viên nhiệt tình, không gian sạch sẽ.'),
('CTL13', 'PT05', 'CH01', 'TC03', NULL),
('CTL14', 'PT05', 'CH02', 'TC06', NULL),
('CTL15', 'PT05', 'CH03', NULL,   N'Thời gian chờ món hơi lâu vào giờ cao điểm.'),
('CTL16', 'PT06', 'CH04', 'TC11', NULL),
('CTL17', 'PT06', 'CH05', NULL,   N'Muốn có thêm bò nướng lá lốt.'),
('CTL18', 'PT07', 'CH04', 'TC12', NULL),
('CTL19', 'PT07', 'CH05', NULL,   N'Thêm các loại chè và bánh ngọt.');
GO
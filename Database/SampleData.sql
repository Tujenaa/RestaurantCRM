USE RestaurantCRM;
GO

-- 1. VAI_TRO (maVaiTro, tenVaiTro, moTa)
INSERT INTO VAI_TRO (maVaiTro, tenVaiTro, moTa) VALUES
('VT01', N'admin', N'Quản trị hệ thống'),
('VT02', N'sale',  N'Quản lý bán hàng và đơn hàng'),
('VT03', N'crm',   N'Quản lý và chăm sóc khách hàng');
GO

-- 2. NHAN_VIEN (maNhanVien, maVaiTro, tenDangNhap, matKhau, hoTen, trangThai)
INSERT INTO NHAN_VIEN (maNhanVien, maVaiTro, tenDangNhap, matKhau, hoTen, trangThai) VALUES
('NV01', 'VT01', 'quanly01', 'e10adc3949ba59abbe56e057f20f883e', N'Lê Thị Hồng Nhung', 'Active'),
('NV02', 'VT02', 'thungan01', 'e10adc3949ba59abbe56e057f20f883e', N'Phạm Văn Đức', 'Active'),
('NV03', 'VT02', 'thungan02', 'e10adc3949ba59abbe56e057f20f883e', N'Võ Thị Mai', 'Active'),
('NV04', 'VT03', 'phucvu01', 'e10adc3949ba59abbe56e057f20f883e', N'Đặng Hoàng Long', 'Active'),
('NV05', 'VT03', 'bep01', 'e10adc3949ba59abbe56e057f20f883e', N'Bùi Quang Vinh', 'Active'),
('NV06', 'VT03', 'kho01', 'e10adc3949ba59abbe56e057f20f883e', N'Ngô Thị Thu Hà', 'Active'),
('NV07', 'VT03', 'phucvu02', 'e10adc3949ba59abbe56e057f20f883e', N'Huỳnh Minh Tuấn', 'Inactive'),
('NV08', 'VT01', 'admin', 'e10adc3949ba59abbe56e057f20f883e', N'Nguyễn Quốc Bảo', 'Active'),
('NV09', 'VT01', 'admin2', 'e10adc3949ba59abbe56e057f20f883e', N'Trần Minh Khôi', 'Active');
GO

-- 3. KHACH_HANG (maKhachHang, hoTen, soDienThoai, email, matKhau, ngaySinh, gioiTinh, soThich, trangThai)
INSERT INTO KHACH_HANG (maKhachHang, hoTen, soDienThoai, email, matKhau, ngaySinh, gioiTinh, soThich, trangThai) VALUES
('KH01', N'Nguyễn Thị Lan Anh', '0901234561', 'lananh.nguyen@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '1995-03-14', N'Nữ', N'Lẩu Thái, đồ uống ít đường', 'Active'),
('KH02', N'Trần Văn Minh', '0902345672', 'minh.tran@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '1990-07-22', N'Nam', N'Lẩu bò nhúng dấm, trà đào', 'Active'),
('KH03', N'Lê Hoàng Phúc', '0903456783', 'phuc.le@yahoo.com', 'e10adc3949ba59abbe56e057f20f883e', '1988-11-05', N'Nam', N'Bò Mỹ, ba chỉ heo', 'Active'),
('KH04', N'Phạm Ngọc Diệp', '0904567894', 'diep.pham@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '1993-01-30', N'Nữ', N'Lẩu hải sản, tôm sú', 'Active'),
('KH05', N'Võ Thanh Tùng', '0905678905', 'tung.vo@outlook.com', 'e10adc3949ba59abbe56e057f20f883e', '1999-09-18', N'Nam', N'Lẩu nấm, rau xanh', 'Active'),
('KH06', N'Đặng Thị Kim Ngân', '0906789016', 'ngan.dang@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '2000-05-02', N'Nữ', N'Tráng miệng, trà trái cây', 'Active'),
('KH07', N'Bùi Anh Khoa', '0907890127', 'khoa.bui@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '1985-12-25', N'Nam', N'Lẩu cay, món cay', 'Active'),
('KH08', N'Hồ Thị Thanh Thảo', '0908901238', 'thao.ho@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '1997-08-09', N'Nữ', N'Lẩu chay, ăn thanh đạm', 'Active'),
('KH09', N'Ngô Đức Thịnh', '0909012349', 'thinh.ngo@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '1982-04-17', N'Nam', N'Lẩu cho nhóm đông, bia', 'Active'),
('KH10', N'Dương Mỹ Linh', '0910123450', 'linh.duong@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '1996-10-11', N'Nữ', N'Lẩu gà lá é, nấm', 'Active'),
('KH11', N'Trịnh Quốc Việt', '0911234561', 'viet.trinh@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '1992-02-28', N'Nam', N'Lẩu Thái, hải sản', 'Active'),
('KH12', N'Lý Bảo Châu', '0912345672', 'chau.ly@gmail.com', 'e10adc3949ba59abbe56e057f20f883e', '2001-06-06', N'Nữ', N'Đồ ngọt, trà trái cây', 'Locked');
GO

-- 4. LOAI_MON (maLoaiMon, tenLoaiMon)
INSERT INTO LOAI_MON (maLoaiMon, tenLoaiMon) VALUES
('LM01', N'Nước lẩu'),
('LM02', N'Thịt & Hải sản nhúng lẩu'),
('LM03', N'Rau & Nấm'),
('LM04', N'Mì, Bún ăn kèm'),
('LM05', N'Đồ uống & Tráng miệng');
GO

-- 5. MON_AN (maMon, maLoaiMon, tenMon, moTa, duongDanAnh, donGia, soLuong, trangThai)
INSERT INTO MON_AN (maMon, maLoaiMon, tenMon, moTa, duongDanAnh, donGia, soLuong, trangThai) VALUES
('M01', 'LM01', N'Lẩu Thái chua cay', N'Nước lẩu Thái chua cay, thơm sả và lá chanh.', '/images/Lau_thai.jpg', 180000, 37, 'InStock'),
('M02', 'LM01', N'Lẩu bò nhúng dấm', N'Nước lẩu bò nhúng dấm chua dịu, thơm tỏi.', '/images/Lau_bo_nhung_giam.png', 220000, 31, 'InStock'),
('M03', 'LM01', N'Lẩu nấm thanh đạm', N'Nước lẩu nấm vị thanh nhẹ, phù hợp ăn thanh đạm.', '/images/Lau_nam.jpg', 170000, 28, 'InStock'),
('M04', 'LM01', N'Lẩu gà lá é', N'Lẩu gà với lá é thơm đặc trưng.', '/images/Lau_ga_la_e.jpg', 200000, 27, 'InStock'),
('M05', 'LM02', N'Bò Mỹ thái lát', N'Thịt bò Mỹ thái lát mỏng dùng để nhúng lẩu.', '/images/Bo_my_thai_lat.jpg', 120000, 52, 'InStock'),
('M06', 'LM02', N'Tôm sú tươi', N'Tôm sú tươi, thịt chắc và ngọt.', '/images/Tom_su_tuoi.jpg', 140000, 46, 'InStock'),
('M07', 'LM02', N'Viên thả lẩu thập cẩm', N'Các loại viên thả lẩu tổng hợp.', '/images/Vien_tha_lau_thap_cam.jpg', 70000, 76, 'InStock'),
('M08', 'LM02', N'Ba chỉ heo thái mỏng', N'Ba chỉ heo thái mỏng dùng để nhúng lẩu.', '/images/Ba_chi_thai_mong.jpg', 90000, 56, 'InStock'),
('M09', 'LM03', N'Rau lẩu thập cẩm', N'Rau xanh tươi dùng kèm các món lẩu.', '/images/Rau.png', 50000, 95, 'InStock'),
('M10', 'LM03', N'Nấm thập cẩm', N'Nhiều loại nấm tươi dùng cho món lẩu.', '/images/Nam.jpg', 60000, 77, 'InStock'),
('M11', 'LM04', N'Mì Udon', N'Mì Udon ăn kèm lẩu.', '/images/Mi_udon.jpg', 25000, 117, 'InStock'),
('M12', 'LM04', N'Bún tươi', N'Bún tươi dùng ăn kèm lẩu.', '/images/bun_tuoi.jpg', 15000, 146, 'InStock'),
('M13', 'LM05', N'Trà đào cam sả', N'Trà đào kết hợp cam và sả.', '/images/Tra_dao_cam_sa.jpg', 40000, 93, 'InStock'),
('M14', 'LM05', N'Chè khúc bạch', N'Món tráng miệng mát lạnh.', '/images/Che_khuc_bach.jpg', 35000, 55, 'InStock'),
('M15', 'LM05', N'Nước ép cam', N'Nước ép cam tươi.', '/images/Nuoc_ep_cam.jpg', 40000, 0, 'OutOfStock');
GO

-- 6. NHA_CUNG_CAP (maNhaCungCap, tenNhaCungCap, soDienThoai, diaChi, email)
INSERT INTO NHA_CUNG_CAP (maNhaCungCap, tenNhaCungCap, soDienThoai, diaChi, email) VALUES
('NCC01', N'Công ty TNHH Thực phẩm Sạch Việt', '02838123456', N'12 Nguyễn Văn Linh, Quận 7, TP.HCM', 'contact@thucphamsachviet.vn'),
('NCC02', N'Cơ sở Thịt bò Ba Miền', '02838234567', N'85 Quốc lộ 22, Hóc Môn, TP.HCM', 'thitbobamien@gmail.com'),
('NCC03', N'Công ty CP Đồ uống & Tráng miệng Sài Gòn', '02839345678', N'245 Điện Biên Phủ, Quận Bình Thạnh, TP.HCM', 'sales@douongsaigon.vn'),
('NCC04', N'Cơ sở Nước cốt Lẩu & Gia vị An Phát', '02838456789', N'56 Lê Văn Sỹ, Quận 3, TP.HCM', 'nuoccotlauanphat@gmail.com'),
('NCC05', N'Hải sản Tươi Phú Quốc', '02839567890', N'Chợ đầu mối Bình Điền, Quận 8, TP.HCM', 'haisanphuquoc@gmail.com');
GO

-- 7. PHIEU_NHAP_HANG (maPhieuNhap, maNhaCungCap, maNhanVien, ngayNhap, tongTien, ghiChu)
INSERT INTO PHIEU_NHAP_HANG (maPhieuNhap, maNhaCungCap, maNhanVien, ngayNhap, tongTien, ghiChu) VALUES
('PN01', 'NCC01', 'NV06', '2026-08-25T08:30:00', 8530000, N'Rau, nấm, mì và bún cho đợt đầu tháng'),
('PN02', 'NCC02', 'NV06', '2026-08-26T09:00:00', 8100000, N'Bò Mỹ và ba chỉ heo tươi'),
('PN03', 'NCC03', 'NV06', '2026-08-28T14:15:00', 3400000, N'Nguyên liệu đồ uống và tráng miệng'),
('PN04', 'NCC04', 'NV06', '2026-08-30T10:00:00', 15900000, N'Nước cốt các loại lẩu'),
('PN05', 'NCC05', 'NV06', '2026-09-02T07:45:00', 7950000, N'Tôm sú và viên thả lẩu');
GO

-- 8. CHI_TIET_PHIEU_NHAP (maChiTiet, maPhieuNhap, maMon, soLuong, donGiaNhap, thanhTien)
INSERT INTO CHI_TIET_PHIEU_NHAP (maChiTiet, maPhieuNhap, maMon, soLuong, donGiaNhap, thanhTien) VALUES
('CTPN01', 'PN01', 'M09', 100, 30000, 3000000),
('CTPN02', 'PN01', 'M10', 80, 38000, 3040000),
('CTPN03', 'PN01', 'M11', 120, 12000, 1440000),
('CTPN04', 'PN01', 'M12', 150, 7000, 1050000),
('CTPN05', 'PN02', 'M05', 60, 80000, 4800000),
('CTPN06', 'PN02', 'M08', 60, 55000, 3300000),
('CTPN07', 'PN03', 'M13', 100, 22000, 2200000),
('CTPN08', 'PN03', 'M14', 60, 20000, 1200000),
('CTPN09', 'PN04', 'M01', 40, 110000, 4400000),
('CTPN10', 'PN04', 'M02', 35, 140000, 4900000),
('CTPN11', 'PN04', 'M03', 30, 100000, 3000000),
('CTPN12', 'PN04', 'M04', 30, 120000, 3600000),
('CTPN13', 'PN05', 'M06', 50, 95000, 4750000),
('CTPN14', 'PN05', 'M07', 80, 40000, 3200000);
GO

-- 9. CHUONG_TRINH_KHUYEN_MAI (maChuongTrinh, tenChuongTrinh, ngayBatDau, ngayKetThuc)
INSERT INTO CHUONG_TRINH_KHUYEN_MAI (maChuongTrinh, tenChuongTrinh, ngayBatDau, ngayKetThuc) VALUES
('KM01', N'Giảm 10% cho đơn từ 300.000đ', '2026-08-01', '2026-12-31'),
('KM02', N'Giảm 40.000đ cho đơn từ 250.000đ', '2026-08-01', '2026-12-31'),
('KM03', N'Trà & nước ép giảm 20%', '2026-08-01', '2026-10-31'),
('KM04', N'Tráng miệng giảm 15%', '2026-08-01', '2026-12-31'),
('KM05', N'Ưu đãi nhóm đông giảm 15% đơn từ 700.000đ', '2026-08-01', '2026-12-31'),
('KM06', N'Bò Mỹ thái lát giảm 20.000đ', '2026-08-01', '2026-11-30');
GO

-- 10. KM_THEO_SP (maKMSP, maChuongTrinh, maMon, phanTramGiam, tienGiam)
INSERT INTO KM_THEO_SP (maKMSP, maChuongTrinh, maMon, phanTramGiam, tienGiam) VALUES
('KMSP01', 'KM03', 'M13', 20, NULL),
('KMSP02', 'KM03', 'M15', 20, NULL),
('KMSP03', 'KM04', 'M14', 15, NULL),
('KMSP04', 'KM06', 'M05', NULL, 20000);
GO

-- 11. KM_THEO_VOUCHER (maKMVoucher, maChuongTrinh, maVoucher, giaTriDonToiThieu, phanTramGiam, tienGiam)
INSERT INTO KM_THEO_VOUCHER (maKMVoucher, maChuongTrinh, maVoucher, giaTriDonToiThieu, phanTramGiam, tienGiam) VALUES
('KMV01', 'KM01', 'VOUCHER10', 300000, 10, NULL),
('KMV02', 'KM02', 'VOUCHER40K', 250000, NULL, 40000),
('KMV03', 'KM05', 'NHOM15', 700000, 15, NULL);
GO

-- 12. HOA_DON (maHoaDon, maKhachHang, maNhanVien, maKMVoucher, tenKhachHang, soDienThoai, diaChiGiao, ngayDat, ngayHoanTat, trangThai, tongTienHang, tienGiamVoucher, tongThanhToan)
INSERT INTO HOA_DON (maHoaDon, maKhachHang, maNhanVien, maKMVoucher, tenKhachHang, soDienThoai, diaChiGiao, ngayDat, ngayHoanTat, trangThai, tongTienHang, tienGiamVoucher, tongThanhToan) VALUES
('DH01', 'KH01', 'NV02', 'KMV01', N'Nguyễn Thị Lan Anh', '0901234561', N'25 Lê Lợi, Quận 1, TP.HCM', '2026-09-01T11:30:00', '2026-09-01T12:15:00', 'Completed', 344000, 34400, 309600),
('DH02', 'KH02', 'NV02', 'KMV02', N'Trần Văn Minh', '0902345672', N'102 Cách Mạng Tháng 8, Quận 3, TP.HCM', '2026-09-03T18:45:00', '2026-09-03T19:30:00', 'Completed', 320000, 40000, 280000),
('DH03', 'KH03', 'NV03', 'KMV01', N'Lê Hoàng Phúc', '0903456783', N'45 Nguyễn Trãi, Quận 5, TP.HCM', '2026-09-05T19:10:00', '2026-09-05T20:05:00', 'Completed', 569500, 56950, 512550),
('DH04', 'KH04', 'NV02', 'KMV03', N'Phạm Ngọc Diệp', '0904567894', N'88 Phan Xích Long, Phú Nhuận, TP.HCM', '2026-09-08T12:00:00', '2026-09-08T13:00:00', 'Completed', 894000, 134100, 759900),
('DH05', 'KH05', 'NV03', NULL, N'Võ Thanh Tùng', '0905678905', N'17 Lý Thường Kiệt, Quận 10, TP.HCM', '2026-09-10T18:20:00', '2026-09-10T19:00:00', 'Completed', 245000, 0, 245000),
('DH06', 'KH06', 'NV06', 'KMV02', N'Đặng Thị Kim Ngân', '0906789016', N'9 Nguyễn Thị Minh Khai, Quận 3, TP.HCM', '2026-09-29T19:00:00', NULL, 'Delivering', 361500, 40000, 321500),
('DH07', 'KH07', 'NV03', 'KMV01', N'Bùi Anh Khoa', '0907890127', N'63 Võ Văn Tần, Quận 3, TP.HCM', '2026-09-15T20:00:00', '2026-09-15T21:00:00', 'Completed', 580000, 58000, 522000),
('DH08', 'KH08', 'NV01', NULL, N'Hồ Thị Thanh Thảo', '0908901238', N'31 Trần Hưng Đạo, Quận 1, TP.HCM', '2026-09-18T10:15:00', NULL, 'Cancelled', 202000, 0, 202000),
('DH09', 'KH09', 'NV02', 'KMV03', N'Ngô Đức Thịnh', '0909012349', N'120 Hoàng Văn Thụ, Tân Bình, TP.HCM', '2026-09-20T19:30:00', '2026-09-20T20:30:00', 'Completed', 984000, 147600, 836400),
('DH10', 'KH10', 'NV02', 'KMV02', N'Dương Mỹ Linh', '0910123450', N'54 Điện Biên Phủ, Bình Thạnh, TP.HCM', '2026-09-22T12:40:00', '2026-09-22T13:20:00', 'Completed', 290000, 40000, 250000),
('DH11', 'KH01', 'NV03', NULL, N'Nguyễn Thị Lan Anh', '0901234561', N'25 Lê Lợi, Quận 1, TP.HCM', '2026-09-30T11:00:00', NULL, 'Preparing', 249750, 0, 249750),
('DH12', 'KH11', 'NV01', 'KMV01', N'Trịnh Quốc Việt', '0911234561', N'14 Nguyễn Đình Chiểu, Quận 3, TP.HCM', '2026-09-30T11:20:00', NULL, 'Pending', 460000, 46000, 414000);
GO

-- 13. CHI_TIET_HOA_DON (maChiTiet, maHoaDon, maMon, maKMSP, tenMon, soLuong, donGiaGoc, tienGiamMon, donGiaSauGiam, thanhTien)
INSERT INTO CHI_TIET_HOA_DON (maChiTiet, maHoaDon, maMon, maKMSP, tenMon, soLuong, donGiaGoc, tienGiamMon, donGiaSauGiam, thanhTien) VALUES
('CT001', 'DH01', 'M01', NULL, N'Lẩu Thái chua cay', 1, 180000, 0, 180000, 180000),
('CT002', 'DH01', 'M05', 'KMSP04', N'Bò Mỹ thái lát', 1, 120000, 20000, 100000, 100000),
('CT003', 'DH01', 'M13', 'KMSP01', N'Trà đào cam sả', 2, 40000, 8000, 32000, 64000),
('CT004', 'DH02', 'M02', NULL, N'Lẩu bò nhúng dấm', 1, 220000, 0, 220000, 220000),
('CT005', 'DH02', 'M07', NULL, N'Viên thả lẩu thập cẩm', 1, 70000, 0, 70000, 70000),
('CT006', 'DH02', 'M12', NULL, N'Bún tươi', 2, 15000, 0, 15000, 30000),
('CT007', 'DH03', 'M02', NULL, N'Lẩu bò nhúng dấm', 1, 220000, 0, 220000, 220000),
('CT008', 'DH03', 'M05', 'KMSP04', N'Bò Mỹ thái lát', 2, 120000, 20000, 100000, 200000),
('CT009', 'DH03', 'M08', NULL, N'Ba chỉ heo thái mỏng', 1, 90000, 0, 90000, 90000),
('CT010', 'DH03', 'M14', 'KMSP03', N'Chè khúc bạch', 2, 35000, 5250, 29750, 59500),
('CT011', 'DH04', 'M01', NULL, N'Lẩu Thái chua cay', 1, 180000, 0, 180000, 180000),
('CT012', 'DH04', 'M06', NULL, N'Tôm sú tươi', 3, 140000, 0, 140000, 420000),
('CT013', 'DH04', 'M07', NULL, N'Viên thả lẩu thập cẩm', 1, 70000, 0, 70000, 70000),
('CT014', 'DH04', 'M09', NULL, N'Rau lẩu thập cẩm', 2, 50000, 0, 50000, 100000),
('CT015', 'DH04', 'M10', NULL, N'Nấm thập cẩm', 1, 60000, 0, 60000, 60000),
('CT016', 'DH04', 'M13', 'KMSP01', N'Trà đào cam sả', 2, 40000, 8000, 32000, 64000),
('CT017', 'DH05', 'M03', NULL, N'Lẩu nấm thanh đạm', 1, 170000, 0, 170000, 170000),
('CT018', 'DH05', 'M09', NULL, N'Rau lẩu thập cẩm', 1, 50000, 0, 50000, 50000),
('CT019', 'DH05', 'M11', NULL, N'Mì Udon', 1, 25000, 0, 25000, 25000),
('CT020', 'DH06', 'M04', NULL, N'Lẩu gà lá é', 1, 200000, 0, 200000, 200000),
('CT021', 'DH06', 'M07', NULL, N'Viên thả lẩu thập cẩm', 1, 70000, 0, 70000, 70000),
('CT022', 'DH06', 'M14', 'KMSP03', N'Chè khúc bạch', 2, 35000, 5250, 29750, 59500),
('CT023', 'DH06', 'M13', 'KMSP01', N'Trà đào cam sả', 1, 40000, 8000, 32000, 32000),
('CT024', 'DH07', 'M01', NULL, N'Lẩu Thái chua cay', 1, 180000, 0, 180000, 180000),
('CT025', 'DH07', 'M05', 'KMSP04', N'Bò Mỹ thái lát', 2, 120000, 20000, 100000, 200000),
('CT026', 'DH07', 'M08', NULL, N'Ba chỉ heo thái mỏng', 1, 90000, 0, 90000, 90000),
('CT027', 'DH07', 'M10', NULL, N'Nấm thập cẩm', 1, 60000, 0, 60000, 60000),
('CT028', 'DH07', 'M11', NULL, N'Mì Udon', 2, 25000, 0, 25000, 50000),
('CT029', 'DH08', 'M03', NULL, N'Lẩu nấm thanh đạm', 1, 170000, 0, 170000, 170000),
('CT030', 'DH08', 'M13', 'KMSP01', N'Trà đào cam sả', 1, 40000, 8000, 32000, 32000),
('CT031', 'DH09', 'M02', NULL, N'Lẩu bò nhúng dấm', 2, 220000, 0, 220000, 440000),
('CT032', 'DH09', 'M05', 'KMSP04', N'Bò Mỹ thái lát', 3, 120000, 20000, 100000, 300000),
('CT033', 'DH09', 'M08', NULL, N'Ba chỉ heo thái mỏng', 2, 90000, 0, 90000, 180000),
('CT034', 'DH09', 'M13', 'KMSP01', N'Trà đào cam sả', 2, 40000, 8000, 32000, 64000),
('CT035', 'DH10', 'M04', NULL, N'Lẩu gà lá é', 1, 200000, 0, 200000, 200000),
('CT036', 'DH10', 'M10', NULL, N'Nấm thập cẩm', 1, 60000, 0, 60000, 60000),
('CT037', 'DH10', 'M12', NULL, N'Bún tươi', 2, 15000, 0, 15000, 30000),
('CT038', 'DH11', 'M03', NULL, N'Lẩu nấm thanh đạm', 1, 170000, 0, 170000, 170000),
('CT039', 'DH11', 'M09', NULL, N'Rau lẩu thập cẩm', 1, 50000, 0, 50000, 50000),
('CT040', 'DH11', 'M14', 'KMSP03', N'Chè khúc bạch', 1, 35000, 5250, 29750, 29750),
('CT041', 'DH12', 'M04', NULL, N'Lẩu gà lá é', 1, 200000, 0, 200000, 200000),
('CT042', 'DH12', 'M06', NULL, N'Tôm sú tươi', 1, 140000, 0, 140000, 140000),
('CT043', 'DH12', 'M07', NULL, N'Viên thả lẩu thập cẩm', 1, 70000, 0, 70000, 70000),
('CT044', 'DH12', 'M09', NULL, N'Rau lẩu thập cẩm', 1, 50000, 0, 50000, 50000);
GO

-- 14. THANH_TOAN (maThanhToan, maHoaDon, phuongThuc, trangThai, ngayThanhToan, soTien)
INSERT INTO THANH_TOAN (maThanhToan, maHoaDon, phuongThuc, trangThai, ngayThanhToan, soTien) VALUES
('TT01', 'DH01', N'Ví MoMo', 'Paid', '2026-09-01T11:35:00', 309600),
('TT02', 'DH02', N'Tiền mặt', 'Paid', '2026-09-03T19:30:00', 280000),
('TT03', 'DH03', N'Chuyển khoản', 'Paid', '2026-09-05T19:15:00', 512550),
('TT04', 'DH04', N'Thẻ tín dụng', 'Paid', '2026-09-08T12:05:00', 759900),
('TT05', 'DH05', N'Tiền mặt', 'Paid', '2026-09-10T19:00:00', 245000),
('TT06', 'DH06', N'COD', 'Pending', NULL, 321500),
('TT07', 'DH07', N'Chuyển khoản', 'Paid', '2026-09-15T20:05:00', 522000),
('TT08', 'DH08', N'Ví MoMo', 'Refunded', '2026-09-18T10:20:00', 202000),
('TT09', 'DH09', N'Thẻ tín dụng', 'Paid', '2026-09-20T19:35:00', 836400),
('TT10', 'DH10', N'Ví ZaloPay', 'Paid', '2026-09-22T12:45:00', 250000),
('TT11', 'DH11', N'Tiền mặt', 'Pending', NULL, 249750),
('TT12', 'DH12', N'Chuyển khoản', 'Pending', NULL, 414000);
GO

-- 15. DANH_GIA (maDanhGia, maKhachHang, maHoaDon, maMon, soSao, noiDung, ngayDanhGia)
INSERT INTO DANH_GIA (maDanhGia, maKhachHang, maHoaDon, maMon, soSao, noiDung, ngayDanhGia) VALUES
('DG01', 'KH01', 'DH01', 'M01', 5, N'Nước lẩu Thái chua cay đúng vị, thơm sả và lá chanh.', '2026-09-01T13:00:00'),
('DG02', 'KH01', 'DH01', 'M13', 4, N'Trà đào thơm, hơi ngọt so với khẩu vị của mình.', '2026-09-01T13:02:00'),
('DG03', 'KH02', 'DH02', 'M02', 5, N'Lẩu bò nhúng dấm chua dịu, nước dùng thơm tỏi, rất hợp ăn tối.', '2026-09-03T20:00:00'),
('DG04', 'KH03', 'DH03', 'M05', 5, N'Bò Mỹ thái mỏng, nhúng vừa tới là mềm ngọt, sẽ quay lại.', '2026-09-05T21:00:00'),
('DG05', 'KH03', 'DH03', 'M14', 5, N'Chè khúc bạch mát lạnh, ăn xong bữa lẩu rất đã.', '2026-09-05T21:05:00'),
('DG06', 'KH04', 'DH04', 'M06', 5, N'Tôm sú tươi, thịt chắc ngọt, nước lẩu Thái chua cay vừa miệng.', '2026-09-08T14:00:00'),
('DG07', 'KH05', 'DH05', 'M03', 4, N'Nước lẩu nấm ngọt thanh, nấm tươi, ăn không bị ngán.', '2026-09-10T19:30:00'),
('DG08', 'KH07', 'DH07', 'M01', 3, N'Nguyên liệu tươi nhưng nước lẩu hơi nhạt, mong quán cải thiện.', '2026-09-15T21:30:00'),
('DG09', 'KH09', 'DH09', 'M08', 5, N'Ba chỉ heo thái mỏng, nhúng giòn ngọt, cả nhóm đều thích.', '2026-09-20T21:00:00'),
('DG10', 'KH10', 'DH10', 'M04', 4, N'Lẩu gà lá é thơm, thịt gà mềm, nước dùng đậm đà.', '2026-09-22T14:00:00');
GO

-- 16. LICH_SU_TRANG_THAI (maLichSu, maHoaDon, trangThai, thoiGian, ghiChu)
INSERT INTO LICH_SU_TRANG_THAI (maLichSu, maHoaDon, trangThai, thoiGian, ghiChu) VALUES
('LS01', 'DH01', 'Pending', '2026-09-01T11:30:00', N'Khách đặt món online'),
('LS02', 'DH01', 'Confirmed', '2026-09-01T11:33:00', N'Nhân viên đã xác nhận đơn'),
('LS03', 'DH01', 'Preparing', '2026-09-01T11:35:00', N'Bếp đã nhận đơn'),
('LS04', 'DH01', 'Completed', '2026-09-01T12:15:00', N'Giao hàng thành công'),
('LS05', 'DH02', 'Pending', '2026-09-03T18:45:00', N'Khách đặt món tại quầy'),
('LS06', 'DH02', 'Confirmed', '2026-09-03T18:48:00', N'Nhân viên đã xác nhận đơn'),
('LS07', 'DH02', 'Preparing', '2026-09-03T18:50:00', N'Bếp đã nhận đơn'),
('LS08', 'DH02', 'Completed', '2026-09-03T19:30:00', N'Khách đã dùng bữa và thanh toán'),
('LS09', 'DH03', 'Pending', '2026-09-05T19:10:00', N'Khách đặt món online'),
('LS10', 'DH03', 'Confirmed', '2026-09-05T19:13:00', N'Nhân viên đã xác nhận đơn'),
('LS11', 'DH03', 'Preparing', '2026-09-05T19:15:00', N'Bếp đã nhận đơn'),
('LS12', 'DH03', 'Completed', '2026-09-05T20:05:00', N'Giao hàng thành công'),
('LS13', 'DH04', 'Pending', '2026-09-08T12:00:00', N'Khách đặt món online'),
('LS14', 'DH04', 'Confirmed', '2026-09-08T12:08:00', N'Nhân viên đã xác nhận đơn'),
('LS15', 'DH04', 'Preparing', '2026-09-08T12:10:00', N'Bếp đã nhận đơn'),
('LS16', 'DH04', 'Completed', '2026-09-08T13:00:00', N'Giao hàng thành công'),
('LS17', 'DH05', 'Pending', '2026-09-10T18:20:00', N'Khách đặt món tại quầy'),
('LS18', 'DH05', 'Confirmed', '2026-09-10T18:23:00', N'Nhân viên đã xác nhận đơn'),
('LS19', 'DH05', 'Preparing', '2026-09-10T18:25:00', N'Bếp đã nhận đơn'),
('LS20', 'DH05', 'Completed', '2026-09-10T19:00:00', N'Khách đã dùng bữa và thanh toán'),
('LS21', 'DH06', 'Pending', '2026-09-29T19:00:00', N'Khách đặt món online'),
('LS22', 'DH06', 'Confirmed', '2026-09-29T19:08:00', N'Nhân viên đã xác nhận đơn'),
('LS23', 'DH06', 'Preparing', '2026-09-29T19:10:00', N'Bếp đã nhận đơn'),
('LS24', 'DH06', 'Delivering', '2026-09-29T19:40:00', N'Shipper đang giao đến khách'),
('LS25', 'DH07', 'Pending', '2026-09-15T20:00:00', N'Khách đặt món online'),
('LS26', 'DH07', 'Confirmed', '2026-09-15T20:08:00', N'Nhân viên đã xác nhận đơn'),
('LS27', 'DH07', 'Preparing', '2026-09-15T20:10:00', N'Bếp đã nhận đơn'),
('LS28', 'DH07', 'Completed', '2026-09-15T21:00:00', N'Giao hàng thành công'),
('LS29', 'DH08', 'Pending', '2026-09-18T10:15:00', N'Khách đặt món online'),
('LS30', 'DH08', 'Cancelled', '2026-09-18T10:20:00', N'Khách yêu cầu hủy, đã hoàn tiền'),
('LS31', 'DH09', 'Pending', '2026-09-20T19:30:00', N'Khách đặt món tại bàn'),
('LS32', 'DH09', 'Confirmed', '2026-09-20T19:33:00', N'Nhân viên đã xác nhận đơn'),
('LS33', 'DH09', 'Preparing', '2026-09-20T19:35:00', N'Bếp đã nhận đơn'),
('LS34', 'DH09', 'Completed', '2026-09-20T20:30:00', N'Khách đã dùng bữa và thanh toán'),
('LS35', 'DH10', 'Pending', '2026-09-22T12:40:00', N'Khách đặt món online'),
('LS36', 'DH10', 'Confirmed', '2026-09-22T12:43:00', N'Nhân viên đã xác nhận đơn'),
('LS37', 'DH10', 'Preparing', '2026-09-22T12:45:00', N'Bếp đã nhận đơn'),
('LS38', 'DH10', 'Completed', '2026-09-22T13:20:00', N'Giao hàng thành công'),
('LS39', 'DH11', 'Pending', '2026-09-30T11:00:00', N'Khách đặt món online'),
('LS40', 'DH11', 'Confirmed', '2026-09-30T11:08:00', N'Nhân viên đã xác nhận đơn'),
('LS41', 'DH11', 'Preparing', '2026-09-30T11:10:00', N'Bếp đã nhận đơn'),
('LS42', 'DH12', 'Pending', '2026-09-30T11:20:00', N'Đơn mới, chờ nhân viên xác nhận');
GO

-- 17. PHAN_HOI (maPhanHoi, maKhachHang, maNhanVien, noiDung, danhGia, ngayPhanHoi, trangThai)
INSERT INTO PHAN_HOI (maPhanHoi, maKhachHang, maNhanVien, noiDung, danhGia, ngayPhanHoi, trangThai) VALUES
('PH01', 'KH01', 'NV04', N'Nước lẩu đậm đà, đồ nhúng đầy đủ, giao hàng nhanh. Cảm ơn quán!', 5, '2026-09-01T14:00:00', 'Resolved'),
('PH02', 'KH02', 'NV04', N'Lẩu bò ngon nhưng đợi món hơi lâu vào giờ cao điểm.', 3, '2026-09-03T20:30:00', 'Resolved'),
('PH03', 'KH03', 'NV04', N'Nhân viên phục vụ nhiệt tình, bàn sạch sẽ, châm nước lẩu kịp thời.', 5, '2026-09-05T21:30:00', 'Resolved'),
('PH04', 'KH07', 'NV04', N'Nước lẩu Thái hơi nhạt so với lần trước, mong quán kiểm tra lại.', 2, '2026-09-15T21:45:00', 'Resolved'),
('PH05', 'KH08', 'NV04', N'Mình đặt nhầm nồi lẩu nên đã hủy đơn, mong quán hỗ trợ hoàn tiền nhanh.', 3, '2026-09-18T10:30:00', 'Resolved'),
('PH06', 'KH09', 'NV04', N'Bò Mỹ và ba chỉ rất ngon, đề nghị quán có thêm combo lẩu cho nhóm đông.', 5, '2026-09-20T21:15:00', 'Resolved'),
('PH07', 'KH06', NULL, N'Đơn giao hơi trễ so với dự kiến, nồi lẩu bị nguội.', 3, '2026-09-29T20:30:00', 'Unresolved'),
('PH08', 'KH12', NULL, N'Tài khoản của tôi bị khóa, nhờ quán kiểm tra giúp.', 2, '2026-09-30T09:00:00', 'Unresolved');
GO

-- 18. TRA_LOI_DANH_GIA (maTraLoi, maDanhGia, nguoiGui, maKhachHang, maNhanVien, noiDung, ngayGui)
INSERT INTO TRA_LOI_DANH_GIA (maTraLoi, maDanhGia, nguoiGui, maKhachHang, maNhanVien, noiDung, ngayGui) VALUES
('TLDG01', 'DG01', 'NhanVien', NULL, 'NV04', N'Cảm ơn chị đã ủng hộ quán! Hẹn gặp lại chị vào lần sau.', '2026-09-02T08:00:00'),
('TLDG02', 'DG02', 'NhanVien', NULL, 'NV04', N'Quán ghi nhận góp ý về độ ngọt. Lần sau chị có thể dặn bên mình giảm đường nhé.', '2026-09-02T08:05:00'),
('TLDG03', 'DG02', 'KhachHang', 'KH01', NULL, N'Ok quán, cảm ơn nhé!', '2026-09-02T09:10:00'),
('TLDG04', 'DG08', 'NhanVien', NULL, 'NV04', N'Cảm ơn anh đã góp ý. Quán sẽ kiểm tra lại công thức nước lẩu.', '2026-09-16T09:15:00'),
('TLDG05', 'DG08', 'KhachHang', 'KH07', NULL, N'Mong quán cải thiện để lần sau mình có trải nghiệm tốt hơn.', '2026-09-16T10:00:00'),
('TLDG06', 'DG08', 'NhanVien', NULL, 'NV04', N'Quán đã ghi nhận và sẽ điều chỉnh ngay.', '2026-09-16T10:30:00'),
('TLDG07', 'DG10', 'NhanVien', NULL, 'NV04', N'Cảm ơn chị đã đánh giá. Quán rất vui vì chị thích món lẩu gà lá é.', '2026-09-22T15:00:00'),
('TLDG08', 'DG10', 'KhachHang', 'KH10', NULL, N'Cảm ơn quán nhé!', '2026-09-22T15:30:00');
GO

-- 19. TRA_LOI_PHAN_HOI (maTraLoi, maPhanHoi, nguoiGui, maKhachHang, maNhanVien, noiDung, ngayGui)
INSERT INTO TRA_LOI_PHAN_HOI (maTraLoi, maPhanHoi, nguoiGui, maKhachHang, maNhanVien, noiDung, ngayGui) VALUES
('TLPH01', 'PH02', 'NhanVien', NULL, 'NV04', N'Quán xin lỗi vì thời gian phục vụ lâu. Quán sẽ tăng cường nhân viên vào giờ cao điểm.', '2026-09-04T08:30:00'),
('TLPH02', 'PH02', 'KhachHang', 'KH02', NULL, N'Không sao, mong lần sau quán phục vụ nhanh hơn.', '2026-09-04T09:10:00'),
('TLPH03', 'PH04', 'NhanVien', NULL, 'NV04', N'Quán đã kiểm tra lại công thức và sẽ điều chỉnh độ đậm đà của nước lẩu.', '2026-09-16T09:00:00'),
('TLPH04', 'PH04', 'KhachHang', 'KH07', NULL, N'Cảm ơn quán đã phản hồi nhanh.', '2026-09-16T10:30:00'),
('TLPH05', 'PH04', 'NhanVien', NULL, 'NV04', N'Cảm ơn anh đã góp ý để quán cải thiện chất lượng.', '2026-09-16T11:00:00'),
('TLPH06', 'PH05', 'NhanVien', NULL, 'NV04', N'Quán đã tiếp nhận yêu cầu và hoàn tiền cho đơn hàng.', '2026-09-18T11:00:00'),
('TLPH07', 'PH05', 'KhachHang', 'KH08', NULL, N'Cảm ơn quán đã hỗ trợ.', '2026-09-18T11:40:00'),
('TLPH08', 'PH07', 'KhachHang', 'KH06', NULL, N'Mình mong quán hỗ trợ kiểm tra tình trạng giao hàng.', '2026-09-29T21:00:00');
GO

-- 20. KHAO_SAT (maKhaoSat, maNhanVien, tieuDe, trangThai, ngayTao)
INSERT INTO KHAO_SAT (maKhaoSat, maNhanVien, tieuDe, trangThai, ngayTao) VALUES
('KS01', 'NV01', N'Khảo sát mức độ hài lòng về dịch vụ', 'Open', '2026-09-01T09:00:00'),
('KS02', 'NV01', N'Khảo sát sở thích nước lẩu & món nhúng mới', 'Open', '2026-09-10T09:00:00');
GO

-- 21. CAU_HOI_KHAO_SAT (maCauHoi, maKhaoSat, noiDungCauHoi, loaiCauHoi)
INSERT INTO CAU_HOI_KHAO_SAT (maCauHoi, maKhaoSat, noiDungCauHoi, loaiCauHoi) VALUES
('CH01', 'KS01', N'Bạn đánh giá thế nào về trải nghiệm tổng thể tại quán?', 'Option'),
('CH02', 'KS01', N'Bạn hài lòng nhất về điều gì?', 'Option'),
('CH03', 'KS01', N'Bạn có góp ý gì để chúng tôi phục vụ tốt hơn?', 'Text'),
('CH04', 'KS02', N'Bạn muốn quán bổ sung loại nước lẩu nào?', 'Option'),
('CH05', 'KS02', N'Bạn muốn thử món nhúng nào nhất?', 'Text');
GO

-- 22. TUY_CHON_CAU_HOI (maTuyChon, maCauHoi, noiDungTuyChon)
INSERT INTO TUY_CHON_CAU_HOI (maTuyChon, maCauHoi, noiDungTuyChon) VALUES
('TC01', 'CH01', N'Rất hài lòng'),
('TC02', 'CH01', N'Hài lòng'),
('TC03', 'CH01', N'Bình thường'),
('TC04', 'CH01', N'Không hài lòng'),
('TC05', 'CH02', N'Chất lượng món ăn'),
('TC06', 'CH02', N'Tốc độ phục vụ / giao hàng'),
('TC07', 'CH02', N'Giá cả'),
('TC08', 'CH02', N'Thái độ nhân viên'),
('TC09', 'CH04', N'Lẩu Tứ Xuyên cay tê'),
('TC10', 'CH04', N'Lẩu hải sản'),
('TC11', 'CH04', N'Lẩu chay thanh đạm'),
('TC12', 'CH04', N'Lẩu cá');
GO

-- 23. DOI_TUONG_KHAO_SAT (maDoiTuong, maKhaoSat, maKhachHang, daHoanThanh)
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
GO

-- 24. PHIEU_TRA_LOI (maPhieuTraLoi, maKhaoSat, maKhachHang, ngayTraLoi)
INSERT INTO PHIEU_TRA_LOI (maPhieuTraLoi, maKhaoSat, maKhachHang, ngayTraLoi) VALUES
('PT01', 'KS01', 'KH01', '2026-09-02T10:00:00'),
('PT02', 'KS01', 'KH02', '2026-09-04T09:30:00'),
('PT03', 'KS01', 'KH03', '2026-09-06T11:00:00'),
('PT04', 'KS01', 'KH04', '2026-09-09T15:20:00'),
('PT05', 'KS01', 'KH05', '2026-09-11T08:45:00'),
('PT06', 'KS02', 'KH01', '2026-09-12T10:10:00'),
('PT07', 'KS02', 'KH02', '2026-09-13T16:00:00');
GO

-- 25. CAU_TRA_LOI (maCauTraLoi, maPhieuTraLoi, maCauHoi, maTuyChon, noiDungTuDien)
INSERT INTO CAU_TRA_LOI (maCauTraLoi, maPhieuTraLoi, maCauHoi, maTuyChon, noiDungTuDien) VALUES
('CTL01', 'PT01', 'CH01', 'TC01', NULL),
('CTL02', 'PT01', 'CH02', 'TC05', NULL),
('CTL03', 'PT01', 'CH03', NULL, N'Nước lẩu đậm đà, giao hàng đúng giờ.'),
('CTL04', 'PT02', 'CH01', 'TC02', NULL),
('CTL05', 'PT02', 'CH02', 'TC07', NULL),
('CTL06', 'PT02', 'CH03', NULL, N'Nên có thêm nhiều khuyến mãi cho khách quen.'),
('CTL07', 'PT03', 'CH01', 'TC01', NULL),
('CTL08', 'PT03', 'CH02', 'TC05', NULL),
('CTL09', 'PT03', 'CH03', NULL, N'Bò Mỹ nhúng lẩu rất mềm, sẽ quay lại.'),
('CTL10', 'PT04', 'CH01', 'TC02', NULL),
('CTL11', 'PT04', 'CH02', 'TC08', NULL),
('CTL12', 'PT04', 'CH03', NULL, N'Nhân viên nhiệt tình, bàn ăn sạch sẽ.'),
('CTL13', 'PT05', 'CH01', 'TC03', NULL),
('CTL14', 'PT05', 'CH02', 'TC06', NULL),
('CTL15', 'PT05', 'CH03', NULL, N'Thời gian chờ món hơi lâu vào giờ cao điểm.'),
('CTL16', 'PT06', 'CH04', 'TC09', NULL),
('CTL17', 'PT06', 'CH05', NULL, N'Muốn có thêm bò cuộn nấm kim châm.'),
('CTL18', 'PT07', 'CH04', 'TC10', NULL),
('CTL19', 'PT07', 'CH05', NULL, N'Thêm mực, sò điệp và các loại nấm.');
GO
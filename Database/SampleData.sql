/*
    SAMPLE DATA - RESTAURANT CRM
    Khớp với RestaurantCRM(3).sql (schema gộp HOA_DON, không có bảng PHAN_HOI).
    Có thể chạy trên CSDL RestaurantCRM vừa tạo và đang rỗng.

    Tài khoản nhân viên mẫu: admin / 123456
    Mật khẩu lưu theo MD5 giống dữ liệu mẫu cũ.

    Bộ dữ liệu gồm 60 hóa đơn trong năm 2026, 24 món ăn, 20 khách hàng,
    15 nhân viên, 5 chương trình khuyến mãi, dữ liệu nhập kho, đánh giá,
    phản hồi đánh giá và khảo sát. Các hóa đơn và chi tiết được tạo bằng
    vòng lặp để dữ liệu doanh thu, trạng thái và thời gian có phân bố.
*/
USE RestaurantCRM;
GO

IF EXISTS (SELECT 1 FROM dbo.VAI_TRO)
BEGIN
    THROW 51000, N'CSDL RestaurantCRM đã có dữ liệu. Hãy dùng CSDL rỗng hoặc xóa dữ liệu hiện có trước khi chạy SampleData.', 1;
END;
GO

-- 1. Vai trò và quyền
INSERT INTO VAI_TRO (maVaiTro, tenVaiTro, moTa) VALUES
('VT01',N'admin',N'Quản trị toàn hệ thống'),
('VT02',N'sale',N'Quản lý bán hàng, đơn hàng và kho'),
('VT03',N'bán hàng',N'Tiếp nhận và xử lý đơn hàng'),
('VT04',N'kho',N'Quản lý nguyên liệu và nhập kho'),
('VT05',N'crm',N'Quản lý khách hàng, đánh giá và khảo sát');

INSERT INTO QUYEN (maQuyen, tenQuyen, nhomQuyen, moTa) VALUES
('ADMIN_ACCOUNT',N'Quản lý tài khoản',N'Hệ thống',N'Tạo và quản lý tài khoản nhân viên'),
('ADMIN_ROLE',N'Quản lý vai trò',N'Hệ thống',N'Gán quyền cho vai trò'),
('ADMIN_BACKUP',N'Sao lưu dữ liệu',N'Hệ thống',N'Sao lưu và phục hồi cơ sở dữ liệu'),
('MENU_VIEW',N'Quản lý thực đơn',N'Vận hành',N'Quản lý loại món và món ăn'),
('ORDER_CREATE',N'Tạo hóa đơn',N'Bán hàng',N'Tạo đơn hàng cho khách'),
('ORDER_VIEW',N'Quản lý hóa đơn',N'Bán hàng',N'Tra cứu và cập nhật trạng thái hóa đơn'),
('PROMO_VIEW',N'Quản lý khuyến mãi',N'Bán hàng',N'Quản lý chương trình và mã giảm giá'),
('REPORT_VIEW',N'Xem thống kê',N'Bán hàng',N'Xem báo cáo doanh thu và hoạt động'),
('INGREDIENT_VIEW',N'Quản lý nguyên liệu',N'Kho',N'Quản lý danh mục nguyên liệu'),
('INVENTORY_VIEW',N'Kiểm tra tồn kho',N'Kho',N'Theo dõi số lượng nguyên liệu'),
('IMPORT_VIEW',N'Quản lý phiếu nhập',N'Kho',N'Tạo và tra cứu phiếu nhập hàng'),
('CRM_CUSTOMER',N'Quản lý khách hàng',N'CRM',N'Quản lý hồ sơ khách hàng'),
('CRM_REVIEW',N'Quản lý đánh giá',N'CRM',N'Xem và phản hồi đánh giá'),
('CRM_SURVEY',N'Quản lý khảo sát',N'CRM',N'Tạo khảo sát và tổng hợp phản hồi'),
('CRM_ANALYTICS',N'Phân tích khách hàng',N'CRM',N'Xem báo cáo khách hàng');

INSERT INTO VAI_TRO_QUYEN (maVaiTro, maQuyen) VALUES
('VT01','ADMIN_ACCOUNT'),('VT01','ADMIN_ROLE'),('VT01','ADMIN_BACKUP'),('VT01','MENU_VIEW'),('VT01','ORDER_CREATE'),('VT01','ORDER_VIEW'),('VT01','PROMO_VIEW'),('VT01','REPORT_VIEW'),('VT01','INGREDIENT_VIEW'),('VT01','INVENTORY_VIEW'),('VT01','IMPORT_VIEW'),('VT01','CRM_CUSTOMER'),('VT01','CRM_REVIEW'),('VT01','CRM_SURVEY'),('VT01','CRM_ANALYTICS'),
('VT02','MENU_VIEW'),('VT02','ORDER_CREATE'),('VT02','ORDER_VIEW'),('VT02','PROMO_VIEW'),('VT02','REPORT_VIEW'),('VT02','INGREDIENT_VIEW'),('VT02','INVENTORY_VIEW'),('VT02','IMPORT_VIEW'),
('VT03','MENU_VIEW'),('VT03','ORDER_CREATE'),('VT03','ORDER_VIEW'),
('VT04','INGREDIENT_VIEW'),('VT04','INVENTORY_VIEW'),('VT04','IMPORT_VIEW'),
('VT05','CRM_CUSTOMER'),('VT05','CRM_REVIEW'),('VT05','CRM_SURVEY'),('VT05','CRM_ANALYTICS');

-- Mật khẩu MD5 của 123456: e10adc3949ba59abbe56e057f20f883e
INSERT INTO NHAN_VIEN (maNhanVien,maVaiTro,tenDangNhap,matKhau,hoTen,trangThai) VALUES
('NV01','VT01',N'admin',N'e10adc3949ba59abbe56e057f20f883e',N'Nguyễn Quốc Bảo',N'Active'),
('NV02','VT02',N'sale01',N'e10adc3949ba59abbe56e057f20f883e',N'Phạm Văn Đức',N'Active'),
('NV03','VT03',N'sale02',N'e10adc3949ba59abbe56e057f20f883e',N'Võ Thị Mai',N'Active'),
('NV04','VT03',N'sale03',N'e10adc3949ba59abbe56e057f20f883e',N'Trần Gia Hân',N'Active'),
('NV05','VT05',N'crm01',N'e10adc3949ba59abbe56e057f20f883e',N'Đặng Hoàng Long',N'Active'),
('NV06','VT05',N'crm02',N'e10adc3949ba59abbe56e057f20f883e',N'Lương Thảo Vy',N'Active'),
('NV07','VT04',N'kho01',N'e10adc3949ba59abbe56e057f20f883e',N'Ngô Thị Thu Hà',N'Active'),
('NV08','VT04',N'kho02',N'e10adc3949ba59abbe56e057f20f883e',N'Phùng Văn Sơn',N'Active'),
('NV09','VT03',N'thungan01',N'e10adc3949ba59abbe56e057f20f883e',N'Lê Thị Hồng Nhung',N'Active'),
('NV10','VT03',N'bep01',N'e10adc3949ba59abbe56e057f20f883e',N'Bùi Quang Vinh',N'Active'),
('NV11','VT03',N'phucvu01',N'e10adc3949ba59abbe56e057f20f883e',N'Huỳnh Minh Tuấn',N'Inactive'),
('NV12','VT03',N'giaohang01',N'e10adc3949ba59abbe56e057f20f883e',N'Tạ Đình Phong',N'Active'),
('NV13','VT02',N'ketoan01',N'e10adc3949ba59abbe56e057f20f883e',N'Châu Ngọc Trâm',N'Active'),
('NV14','VT02',N'marketing01',N'e10adc3949ba59abbe56e057f20f883e',N'Kiều Anh Thư',N'Active'),
('NV15','VT05',N'crm03',N'e10adc3949ba59abbe56e057f20f883e',N'Trần Minh Khôi',N'Active');

-- 2. Khách hàng
INSERT INTO KHACH_HANG (maKhachHang,hoTen,soDienThoai,email,matKhau,ngaySinh,gioiTinh,diaChi,trangThai) VALUES
('KH001',N'Phạm Ngọc Diệp','0904938364',N'diep.pham@gmail.com',N'e10adc3949ba59abbe56e057f20f883e','1993-01-30',N'Nữ',N'120 Pasteur, Quận 3, TP. Hồ Chí Minh',N'Active'),
('KH002',N'Đặng Thị Kim Ngân','0906407546',N'ngan.dang@gmail.com',N'e10adc3949ba59abbe56e057f20f883e','2000-05-02',N'Nữ',N'68 Nam Kỳ Khởi Nghĩa, Quận 1, TP. Hồ Chí Minh',N'Active'),
('KH003',N'Dương Mỹ Linh','0910345910',N'linh.duong@gmail.com',N'e10adc3949ba59abbe56e057f20f883e','1996-10-11',N'Nữ',N'77 Trần Hưng Đạo, Quận 1, TP. Hồ Chí Minh',N'Active'),
('KH004',N'Lý Bảo Châu','0912815092',N'chau.ly@gmail.com',N'e10adc3949ba59abbe56e057f20f883e','2001-06-06',N'Nữ',N'250 Lý Thường Kiệt, Quận 10, TP. Hồ Chí Minh',N'Locked'),
('KH005',N'Phan Thị Hồng Nhung','0913549683',N'hongnhung.phan@gmail.com',N'e10adc3949ba59abbe56e057f20f883e','1994-04-21',N'Nữ',N'27 Võ Thị Sáu, Quận 1, TP. Hồ Chí Minh',N'Active'),
('KH006',N'Nguyễn Hữu Tài','0914284274',N'tai.nguyen@gmail.com',N'e10adc3949ba59abbe56e057f20f883e','1989-09-03',N'Nam',N'190 Nguyễn Tri Phương, Quận 10, TP. Hồ Chí Minh',N'Active'),
('KH007',N'Lâm Thị Cẩm Tú','0915018865',N'camtu.lam@gmail.com',N'e10adc3949ba59abbe56e057f20f883e','1998-12-12',N'Nữ',N'36 Võ Văn Ngân, TP. Thủ Đức, TP. Hồ Chí Minh',N'Active'),
('KH008',N'Đỗ Quang Huy','0916753456',N'huy.do@outlook.com',N'e10adc3949ba59abbe56e057f20f883e','1991-07-30',N'Nam',N'412 Quang Trung, Gò Vấp, TP. Hồ Chí Minh',N'Active'),
('KH009',N'Huỳnh Gia Bảo','0918222638',N'bao.huynh@gmail.com',N'e10adc3949ba59abbe56e057f20f883e','1986-10-26',N'Nam',N'25 Lũy Bán Bích, Tân Phú, TP. Hồ Chí Minh',N'Active'),
('KH010',N'Mai Phương Thảo','0919957229',N'phuongthao.mai@gmail.com',N'e10adc3949ba59abbe56e057f20f883e','1995-06-17',N'Nữ',N'140 Nguyễn Trãi, Quận 5, TP. Hồ Chí Minh',N'Active'),
('KH011',N'Cao Minh Trí','0920691820',N'tri.cao@gmail.com',N'e10adc3949ba59abbe56e057f20f883e','1984-01-09',N'Nam',N'9 Phạm Văn Đồng, Gò Vấp, TP. Hồ Chí Minh',N'Locked'),
('KH012',N'Tô Ngọc Hân','0921426411',N'han.to@gmail.com',N'e10adc3949ba59abbe56e057f20f883e','1999-03-03',N'Nữ',N'61 Hoàng Văn Thụ, Phú Nhuận, TP. Hồ Chí Minh',N'Active'),
('KH013',N'Hà Nhật Nam','0924630184',N'nam.ha@outlook.com',N'e10adc3949ba59abbe56e057f20f883e','2000-11-08',N'Nam',N'5 Nguyễn Hữu Thọ, Quận 7, TP. Hồ Chí Minh',N'Active'),
('KH014',N'Kim Thùy Dung','0925364775',N'dung.kim@gmail.com',N'e10adc3949ba59abbe56e057f20f883e','1996-01-15',N'Nữ',N'99 Bạch Đằng, Tân Bình, TP. Hồ Chí Minh',N'Active'),
('KH015',N'Vũ Thành Đạt','0931123401',N'dat.vu@gmail.com',N'e10adc3949ba59abbe56e057f20f883e','1992-08-12',N'Nam',N'18 Cộng Hòa, Tân Bình, TP. Hồ Chí Minh',N'Active'),
('KH016',N'Nguyễn Thảo My','0932456702',N'my.nguyen@gmail.com',N'e10adc3949ba59abbe56e057f20f883e','2002-04-19',N'Nữ',N'45 Kinh Dương Vương, Bình Tân, TP. Hồ Chí Minh',N'Active'),
('KH017',N'Trần Minh Khang','0933678903',N'khang.tran@gmail.com',N'e10adc3949ba59abbe56e057f20f883e','1988-02-05',N'Nam',N'26 Nguyễn Oanh, Gò Vấp, TP. Hồ Chí Minh',N'Active'),
('KH018',N'Bùi Ngọc Anh','0934789014',N'anh.bui@gmail.com',N'e10adc3949ba59abbe56e057f20f883e','1997-07-21',N'Nữ',N'81 Điện Biên Phủ, Bình Thạnh, TP. Hồ Chí Minh',N'Active'),
('KH019',N'Phan Quốc Khánh','0935890125',N'khanh.phan@gmail.com',N'e10adc3949ba59abbe56e057f20f883e','1990-12-09',N'Nam',N'71 Nguyễn Văn Cừ, Quận 5, TP. Hồ Chí Minh',N'Active'),
('KH020',N'Hoàng Yến Nhi','0936901236',N'nhi.hoang@gmail.com',N'e10adc3949ba59abbe56e057f20f883e','2001-09-16',N'Nữ',N'16 Âu Cơ, Tân Phú, TP. Hồ Chí Minh',N'Active');

-- 3. Thực đơn (5 loại, 24 món)
INSERT INTO LOAI_MON (maLoaiMon,tenLoaiMon) VALUES
('LM001',N'Lẩu'),('LM002',N'Thịt và hải sản'),('LM003',N'Rau và món ăn kèm'),('LM004',N'Đồ uống và tráng miệng'),('LM005',N'Combo');

INSERT INTO MON_AN (maMon,maLoaiMon,tenMon,moTa,duongDanAnh,donGia,trangThai) VALUES
('MA001','LM001',N'Lẩu Thái chua cay',N'Nước lẩu Thái thơm sả và lá chanh.',N'https://images.unsplash.com/photo-1547592180-85f173990554?auto=format&fit=crop&w=800&q=80',189000,N'InStock'),
('MA002','LM001',N'Lẩu Tứ Xuyên',N'Nước lẩu cay tê đậm vị hoa tiêu.',N'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?auto=format&fit=crop&w=800&q=80',219000,N'InStock'),
('MA003','LM001',N'Lẩu nấm thanh ngọt',N'Nước dùng nấm, phù hợp ăn nhẹ.',N'https://images.unsplash.com/photo-1547592180-85f173990554?auto=format&fit=crop&w=800&q=80',179000,N'InStock'),
('MA004','LM001',N'Lẩu kim chi Hàn Quốc',N'Lẩu kim chi cay nhẹ, kèm đậu hũ.',N'https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=800&q=80',195000,N'InStock'),
('MA005','LM002',N'Ba chỉ bò Mỹ',N'Thịt bò thái mỏng, mềm ngọt.',N'https://images.unsplash.com/photo-1559847844-5315695dadae?auto=format&fit=crop&w=800&q=80',129000,N'InStock'),
('MA006','LM002',N'Bò Wagyu A4',N'Thịt bò Wagyu vân mỡ đẹp.',N'https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?auto=format&fit=crop&w=800&q=80',459000,N'InStock'),
('MA007','LM002',N'Nạc vai bò',N'Nạc vai bò ít mỡ, đậm vị.',N'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?auto=format&fit=crop&w=800&q=80',139000,N'InStock'),
('MA008','LM002',N'Tôm sú tươi',N'Tôm sú chắc thịt, vị ngọt tự nhiên.',N'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?auto=format&fit=crop&w=800&q=80',149000,N'InStock'),
('MA009','LM002',N'Sò điệp Nhật',N'Sò điệp Nhật thịt dày và ngọt.',N'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=800&q=80',189000,N'InStock'),
('MA010','LM002',N'Mực ống tươi',N'Mực ống làm sạch, thích hợp nhúng lẩu.',N'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?auto=format&fit=crop&w=800&q=80',119000,N'InStock'),
('MA011','LM003',N'Rau lẩu thập cẩm',N'Rau xanh theo mùa.',N'https://images.unsplash.com/photo-1547592180-85f173990554?auto=format&fit=crop&w=800&q=80',49000,N'InStock'),
('MA012','LM003',N'Nấm kim châm',N'Nấm kim châm tươi.',N'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?auto=format&fit=crop&w=800&q=80',35000,N'InStock'),
('MA013','LM003',N'Mì trứng',N'Mì trứng ăn kèm nước lẩu.',N'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?auto=format&fit=crop&w=800&q=80',25000,N'InStock'),
('MA014','LM003',N'Đậu hũ non',N'Đậu hũ non mềm.',N'https://images.unsplash.com/photo-1547592180-85f173990554?auto=format&fit=crop&w=800&q=80',29000,N'InStock'),
('MA015','LM003',N'Khoai tây chiên',N'Khoai tây chiên giòn.',N'https://images.unsplash.com/photo-1573080496219-bb080dd4f877?auto=format&fit=crop&w=800&q=80',45000,N'InStock'),
('MA016','LM003',N'Cơm chiên trứng',N'Cơm chiên trứng tơi hạt.',N'https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?auto=format&fit=crop&w=800&q=80',55000,N'InStock'),
('MA017','LM004',N'Trà đào cam sả',N'Trà đào thơm mát.',N'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?auto=format&fit=crop&w=800&q=80',39000,N'InStock'),
('MA018','LM004',N'Nước suối',N'Nước suối đóng chai.',N'https://images.unsplash.com/photo-1548839140-29a749e1cf4d?auto=format&fit=crop&w=800&q=80',15000,N'InStock'),
('MA019','LM004',N'Nước ngọt',N'Nước ngọt có ga.',N'https://images.unsplash.com/photo-1544145945-f90425340c7e?auto=format&fit=crop&w=800&q=80',25000,N'InStock'),
('MA020','LM004',N'Kem dừa Thái',N'Kem dừa béo mát.',N'https://images.unsplash.com/photo-1563805042-7684c019e1cb?auto=format&fit=crop&w=800&q=80',45000,N'InStock'),
('MA021','LM005',N'Combo sum vầy',N'Combo lẩu và thịt cho 3-4 người.',N'https://images.unsplash.com/photo-1547592180-85f173990554?auto=format&fit=crop&w=800&q=80',499000,N'InStock'),
('MA022','LM005',N'Combo gia đình',N'Combo lẩu và món ăn kèm cho 4 người.',N'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?auto=format&fit=crop&w=800&q=80',699000,N'InStock'),
('MA023','LM003',N'Gỏi cuốn tôm thịt',N'Gỏi cuốn chấm tương đậu.',N'https://images.unsplash.com/photo-1555939594-58d7cb561ad1?auto=format&fit=crop&w=800&q=80',59000,N'InStock'),
('MA024','LM003',N'Chả giò hải sản',N'Chả giò giòn nhân hải sản.',N'https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=800&q=80',69000,N'InStock');

-- 4. Nguyên liệu, định lượng, nhà cung cấp và nhập kho
INSERT INTO NGUYEN_LIEU (maNguyenLieu,tenNguyenLieu,donViTinh,soLuongTon,giaNhapGanNhat,trangThai) VALUES
('NL01',N'Ba chỉ bò',N'kg',42,210000,N'Active'),('NL02',N'Tôm sú',N'kg',28,185000,N'Active'),('NL03',N'Nấm kim châm',N'gói',55,12000,N'Active'),('NL04',N'Rau cải',N'kg',38,28000,N'Active'),('NL05',N'Mì trứng',N'gói',70,9000,N'Active'),('NL06',N'Đậu hũ',N'kg',25,24000,N'Active'),('NL07',N'Nước cốt lẩu Thái',N'lít',20,65000,N'Active'),('NL08',N'Ớt khô',N'kg',8,95000,N'Active'),('NL09',N'Sả',N'kg',18,30000,N'Active'),('NL10',N'Lá chanh',N'kg',6,75000,N'Active'),('NL11',N'Mực ống',N'kg',24,160000,N'Active'),('NL12',N'Sò điệp',N'kg',15,280000,N'Active'),('NL13',N'Gạo',N'kg',60,18000,N'Active'),('NL14',N'Trứng gà',N'quả',120,3200,N'Active'),('NL15',N'Khoai tây',N'kg',35,26000,N'Active'),('NL16',N'Trà đào',N'lít',18,52000,N'Active'),('NL17',N'Kem dừa',N'hộp',22,38000,N'Active'),('NL18',N'Bò Wagyu',N'kg',10,680000,N'Active');

INSERT INTO DINH_LUONG (maDinhLuong,maMon,maNguyenLieu,soLuongCan) VALUES
('DL001','MA001','NL07',0.5),('DL002','MA001','NL09',0.1),('DL003','MA002','NL08',0.15),('DL004','MA005','NL01',0.25),('DL005','MA008','NL02',0.3),('DL006','MA011','NL04',0.4),('DL007','MA012','NL03',1),('DL008','MA013','NL05',1),('DL009','MA016','NL13',0.25),('DL010','MA016','NL14',1),('DL011','MA017','NL16',0.3),('DL012','MA020','NL17',1),('DL013','MA010','NL11',0.25),('DL014','MA009','NL12',0.2),('DL015','MA006','NL18',0.2),('DL016','MA015','NL15',0.3);

INSERT INTO NHA_CUNG_CAP (maNhaCungCap,tenNhaCungCap,soDienThoai,diaChi,email,trangThai) VALUES
('NCC01',N'Thực phẩm An Toàn','02838112233',N'Quận 5, TP. Hồ Chí Minh',N'lienhe@antoan.vn',N'Active'),
('NCC02',N'Hải sản Biển Xanh','02838223344',N'Quận 4, TP. Hồ Chí Minh',N'kinhdoanh@bienxanh.vn',N'Active'),
('NCC03',N'Rau sạch Đà Lạt','02838334455',N'Tân Bình, TP. Hồ Chí Minh',N'cskh@rausach.vn',N'Active'),
('NCC04',N'Tổng kho thực phẩm Sài Gòn','02838445566',N'Bình Tân, TP. Hồ Chí Minh',N'kho@sgfood.vn',N'Active'),
('NCC05',N'Đồ uống Minh Phát','02838556677',N'Gò Vấp, TP. Hồ Chí Minh',N'hello@minhphat.vn',N'Active');

INSERT INTO PHIEU_NHAP_HANG (maPhieuNhap,maNhaCungCap,maNhanVien,ngayNhap,tongTien,ghiChu) VALUES
('PN001','NCC01','NV07','2026-01-05T09:00:00',4250000,N'Nhập thịt và nguyên liệu đầu tháng'),
('PN002','NCC02','NV08','2026-02-10T10:30:00',3859800,N'Nhập hải sản tươi'),
('PN003','NCC03','NV07','2026-03-12T08:45:00',2139975,N'Nhập rau và nấm'),
('PN004','NCC04','NV08','2026-04-18T11:00:00',4984000,N'Nhập nguyên liệu tổng hợp'),
('PN005','NCC05','NV07','2026-05-20T14:15:00',1760002,N'Nhập đồ uống và tráng miệng'),
('PN006','NCC01','NV08','2026-06-15T09:20:00',5319992,N'Nhập thịt và gạo'),
('PN007','NCC02','NV07','2026-07-22T10:10:00',4269996,N'Nhập hải sản'),
('PN008','NCC03','NV08','2026-08-17T08:30:00',2379996,N'Nhập rau củ'),
('PN009','NCC04','NV07','2026-09-21T13:00:00',3605000,N'Nhập nguyên liệu cho mùa cao điểm'),
('PN010','NCC05','NV08','2026-10-19T15:00:00',1920000,N'Nhập đồ uống');

INSERT INTO CHI_TIET_PHIEU_NHAP (maChiTiet,maPhieuNhap,maNguyenLieu,soLuong,donGiaNhap,thanhTien) VALUES
('CTN001','PN001','NL01',10,210000,2100000),('CTN002','PN001','NL07',10,65000,650000),('CTN003','PN001','NL09',20,30000,600000),('CTN004','PN001','NL13',50,18000,900000),
('CTN005','PN002','NL02',12,185000,2220000),('CTN006','PN002','NL11',8,160000,1280000),('CTN007','PN002','NL12',1.285,280000,359800),
('CTN008','PN003','NL03',50,12000,600000),('CTN009','PN003','NL04',35,28000,980000),('CTN010','PN003','NL15',20,26000,520000),('CTN011','PN003','NL10',0.533,75000,39975),
('CTN012','PN004','NL18',5,680000,3400000),('CTN013','PN004','NL05',80,9000,720000),('CTN014','PN004','NL14',120,3200,384000),('CTN015','PN004','NL06',20,24000,480000),
('CTN016','PN005','NL16',20,52000,1040000),('CTN017','PN005','NL17',10,38000,380000),('CTN018','PN005','NL13',18.889,18000,340002),
('CTN019','PN006','NL01',15,210000,3150000),('CTN020','PN006','NL13',70,18000,1260000),('CTN021','PN006','NL14',100,3200,320000),('CTN022','PN006','NL15',22.692,26000,589992),
('CTN023','PN007','NL02',14,185000,2590000),('CTN024','PN007','NL11',8,160000,1280000),('CTN025','PN007','NL03',33.333,12000,399996),
('CTN026','PN008','NL04',40,28000,1120000),('CTN027','PN008','NL03',45,12000,540000),('CTN028','PN008','NL09',12,30000,360000),('CTN029','PN008','NL15',13.846,26000,359996),
('CTN030','PN009','NL18',4,680000,2720000),('CTN031','PN009','NL12',2,280000,560000),('CTN032','PN009','NL07',5,65000,325000),
('CTN033','PN010','NL16',18,52000,936000),('CTN034','PN010','NL17',12,38000,456000),('CTN035','PN010','NL14',165,3200,528000);

-- 5. Chương trình khuyến mãi: 5 voucher đơn hàng, 5 theo loại món, 5 theo món
INSERT INTO CHUONG_TRINH_KHUYEN_MAI (maChuongTrinh,tenChuongTrinh,ngayBatDau,ngayKetThuc) VALUES
('CTKM01',N'Khai xuân 2026','2026-01-01','2026-02-28'),('CTKM02',N'Tháng lẩu yêu thương','2026-03-01','2026-04-30'),('CTKM03',N'Hè vui sum vầy','2026-05-01','2026-06-30'),('CTKM04',N'Ưu đãi mùa tựu trường','2026-07-01','2026-09-30'),('CTKM05',N'Tri ân khách hàng','2026-10-01','2026-12-31');

INSERT INTO KM_THEO_DON_HANG (maKMDonHang,maChuongTrinh,maVoucher,giaTriDonToiThieu,phanTramGiam,tienGiam,giamToiDa,soLuongToiThieu,choPhepCongDon) VALUES
('KMDH01','CTKM01','XUAN5',200000,5,NULL,30000,1,0),('KMDH02','CTKM02','LAU8',300000,8,NULL,40000,1,0),('KMDH03','CTKM03','HE10',400000,10,NULL,50000,1,0),('KMDH04','CTKM04','BACK12',500000,12,NULL,60000,1,0),('KMDH05','CTKM05','THANK15',600000,15,NULL,80000,1,0);

INSERT INTO KM_THEO_LOAI (maKMLoai,maChuongTrinh,maLoaiMon,maVoucher,giaTriDonToiThieu,phanTramGiam,tienGiam,giamToiDa,soLuongToiThieu,choPhepCongDon) VALUES
('KML001','CTKM01','LM001','LAU5',NULL,5,NULL,30000,1,0),('KML002','CTKM02','LM002','THIT10',NULL,10,NULL,50000,1,0),('KML003','CTKM03','LM003','RAU8',NULL,8,NULL,25000,1,0),('KML004','CTKM04','LM004','NUOC10',NULL,10,NULL,20000,1,0),('KML005','CTKM05','LM005','COMBO12',NULL,12,NULL,70000,1,0);

INSERT INTO KM_THEO_SP (maKMSP,maChuongTrinh,maMon,maVoucher,phanTramGiam,tienGiam,giamToiDa,soLuongToiThieu,choPhepCongDon) VALUES
('KMSP01','CTKM01','MA001','THAI5',5,NULL,20000,1,0),('KMSP02','CTKM02','MA005','BO10',10,NULL,25000,1,0),('KMSP03','CTKM03','MA008','TOM8',8,NULL,20000,1,0),('KMSP04','CTKM04','MA017','TRA10',10,NULL,10000,1,0),('KMSP05','CTKM05','MA021','COMBO15',15,NULL,70000,1,0);

-- 6. Tạo 60 hóa đơn và 120 dòng chi tiết trong năm 2026.
-- Mỗi hóa đơn gồm 2 món; tổng tiền được tính từ chi tiết và voucher đã áp dụng.
DECLARE @i INT = 1;
DECLARE @maHoaDon VARCHAR(20), @maKH VARCHAR(20), @maNV VARCHAR(20), @maVoucher VARCHAR(20), @maMon1 VARCHAR(20), @maMon2 VARCHAR(20);
DECLARE @tenKH NVARCHAR(100), @sdt VARCHAR(15), @diaChi NVARCHAR(255), @trangThai NVARCHAR(30), @ngayDat DATETIME, @ngayHoanTat DATETIME;
DECLARE @gia1 FLOAT, @gia2 FLOAT, @sl1 INT, @sl2 INT, @tienHang FLOAT, @tienGiam FLOAT, @tong FLOAT, @phanTram FLOAT, @giamToiDa FLOAT;
DECLARE @n1 INT, @n2 INT, @thang INT, @ngay INT;
WHILE @i <= 60
BEGIN
    SET @maHoaDon = 'HD' + RIGHT('000' + CAST(@i AS VARCHAR(3)), 3);
    SET @maKH = 'KH' + RIGHT('000' + CAST(((@i - 1) % 20) + 1 AS VARCHAR(3)), 3);
    SET @maNV = 'NV' + RIGHT('0' + CAST(((@i - 1) % 4) + 2 AS VARCHAR(2)), 2);
    SELECT @tenKH=hoTen, @sdt=soDienThoai, @diaChi=diaChi FROM KHACH_HANG WHERE maKhachHang=@maKH;
    SET @n1 = ((@i - 1) % 24) + 1;
    SET @n2 = (@i % 24) + 1;
    SET @maMon1 = 'MA' + RIGHT('000' + CAST(@n1 AS VARCHAR(3)), 3);
    SET @maMon2 = 'MA' + RIGHT('000' + CAST(@n2 AS VARCHAR(3)), 3);
    SELECT @gia1=donGia FROM MON_AN WHERE maMon=@maMon1;
    SELECT @gia2=donGia FROM MON_AN WHERE maMon=@maMon2;
    SET @sl1 = (@i % 3) + 1;
    SET @sl2 = ((@i + 1) % 2) + 1;
    SET @tienHang = @gia1 * @sl1 + @gia2 * @sl2;
    SET @maVoucher = CASE WHEN @i % 6 = 0 THEN NULL ELSE 'KMDH' + RIGHT('0' + CAST(((@i - 1) % 5) + 1 AS VARCHAR(2)), 2) END;
    SET @tienGiam = 0;
    IF @maVoucher IS NOT NULL
    BEGIN
        SELECT @phanTram=phanTramGiam, @giamToiDa=giamToiDa FROM KM_THEO_DON_HANG WHERE maKMDonHang=@maVoucher;
        IF @tienHang >= (SELECT giaTriDonToiThieu FROM KM_THEO_DON_HANG WHERE maKMDonHang=@maVoucher)
            SET @tienGiam = CASE WHEN @tienHang * @phanTram / 100.0 < @giamToiDa THEN @tienHang * @phanTram / 100.0 ELSE @giamToiDa END;
    END;
    SET @tong = @tienHang - @tienGiam;
    SET @thang = ((@i - 1) % 10) + 1;
    SET @ngay = ((@i - 1) / 10) * 5 + 1;
    SET @ngayDat = DATETIMEFROMPARTS(2026,@thang,@ngay,10 + (@i % 10),(@i * 7) % 60,0,0);
    SET @trangThai = CASE @i % 12 WHEN 0 THEN N'Cancelled' WHEN 1 THEN N'Pending' WHEN 2 THEN N'Confirmed' WHEN 3 THEN N'Preparing' WHEN 4 THEN N'Delivering' ELSE N'Completed' END;
    SET @ngayHoanTat = CASE WHEN @trangThai=N'Completed' THEN DATEADD(HOUR,2,@ngayDat) ELSE NULL END;

    INSERT INTO HOA_DON (maHoaDon,maKhachHang,maNhanVien,maKMDonHang,tenKhachHang,soDienThoai,diaChiGiao,ngayDat,ngayHoanTat,trangThai,tongTienHang,tienGiamVoucher,tongThanhToan)
    VALUES (@maHoaDon,@maKH,@maNV,@maVoucher,@tenKH,@sdt,@diaChi,@ngayDat,@ngayHoanTat,@trangThai,@tienHang,@tienGiam,@tong);

    INSERT INTO CHI_TIET_HOA_DON (maChiTiet,maHoaDon,maMon,maKMSP,maKMLoai,tenMon,soLuong,donGiaGoc,tienGiamMon,donGiaSauGiam,thanhTien)
    SELECT 'CT' + RIGHT('0000' + CAST(@i * 2 - 1 AS VARCHAR(4)),4),@maHoaDon,@maMon1,NULL,NULL,tenMon,@sl1,@gia1,0,@gia1,@gia1*@sl1 FROM MON_AN WHERE maMon=@maMon1;
    INSERT INTO CHI_TIET_HOA_DON (maChiTiet,maHoaDon,maMon,maKMSP,maKMLoai,tenMon,soLuong,donGiaGoc,tienGiamMon,donGiaSauGiam,thanhTien)
    SELECT 'CT' + RIGHT('0000' + CAST(@i * 2 AS VARCHAR(4)),4),@maHoaDon,@maMon2,NULL,NULL,tenMon,@sl2,@gia2,0,@gia2,@gia2*@sl2 FROM MON_AN WHERE maMon=@maMon2;

    INSERT INTO THANH_TOAN (maThanhToan,maHoaDon,phuongThuc,trangThai,ngayThanhToan,soTien)
    VALUES ('TT' + RIGHT('000' + CAST(@i AS VARCHAR(3)),3),@maHoaDon,
        CASE @i % 3 WHEN 0 THEN N'Tiền mặt' WHEN 1 THEN N'Chuyển khoản' ELSE N'Ví điện tử' END,
        CASE @trangThai WHEN N'Completed' THEN N'Paid' WHEN N'Cancelled' THEN N'Failed' ELSE N'Pending' END,
        CASE WHEN @trangThai IN (N'Completed',N'Cancelled') THEN DATEADD(MINUTE,30,@ngayDat) ELSE NULL END,
        @tong);

    INSERT INTO LICH_SU_TRANG_THAI (maLichSu,maHoaDon,trangThai,thoiGian,ghiChu)
    VALUES ('LS' + RIGHT('0000' + CAST(@i * 5 - 4 AS VARCHAR(4)),4),@maHoaDon,N'Pending',@ngayDat,N'Khách tạo hóa đơn');
    IF @trangThai IN (N'Confirmed',N'Preparing',N'Delivering',N'Completed')
        INSERT INTO LICH_SU_TRANG_THAI VALUES ('LS' + RIGHT('0000' + CAST(@i * 5 - 3 AS VARCHAR(4)),4),@maHoaDon,N'Confirmed',DATEADD(MINUTE,10,@ngayDat),N'Nhà hàng xác nhận');
    IF @trangThai IN (N'Preparing',N'Delivering',N'Completed')
        INSERT INTO LICH_SU_TRANG_THAI VALUES ('LS' + RIGHT('0000' + CAST(@i * 5 - 2 AS VARCHAR(4)),4),@maHoaDon,N'Preparing',DATEADD(MINUTE,20,@ngayDat),N'Đang chuẩn bị món');
    IF @trangThai IN (N'Delivering',N'Completed')
        INSERT INTO LICH_SU_TRANG_THAI VALUES ('LS' + RIGHT('0000' + CAST(@i * 5 - 1 AS VARCHAR(4)),4),@maHoaDon,N'Delivering',DATEADD(MINUTE,40,@ngayDat),N'Đơn đang được giao');
    IF @trangThai IN (N'Completed',N'Cancelled')
        INSERT INTO LICH_SU_TRANG_THAI VALUES ('LS' + RIGHT('0000' + CAST(@i * 5 AS VARCHAR(4)),4),@maHoaDon,@trangThai,DATEADD(HOUR,2,@ngayDat),CASE WHEN @trangThai=N'Completed' THEN N'Giao hàng hoàn tất' ELSE N'Đơn đã hủy' END);

    SET @i += 1;
END;
GO

-- 7. Đánh giá và nhân viên trả lời
INSERT INTO DANH_GIA (maDanhGia,maKhachHang,maHoaDon,maMon,soSao,noiDung,ngayDanhGia,trangThai,daXuLy) VALUES
('DG001','KH005','HD005','MA005',5,N'Thịt bò mềm, giao hàng nhanh.','2026-01-03T14:00:00',1,1),
('DG002','KH006','HD006','MA006',4,N'Thịt ngon, đóng gói cẩn thận.','2026-02-04T15:20:00',1,1),
('DG003','KH007','HD007','MA008',5,N'Tôm tươi và ngọt.','2026-03-07T18:10:00',1,1),
('DG004','KH008','HD008','MA008',3,N'Tôm ngon, mong nhà hàng giao nhanh hơn.','2026-04-09T12:30:00',1,1),
('DG005','KH009','HD009','MA009',5,N'Sò điệp tươi, khẩu phần phù hợp nhóm bạn.','2026-05-11T20:00:00',1,1),
('DG006','KH010','HD010','MA010',4,N'Mực tươi, nước lẩu thanh.','2026-06-13T19:15:00',1,0),
('DG007','KH011','HD011','MA011',2,N'Món đến muộn hơn dự kiến.','2026-07-15T21:10:00',1,0),
('DG008','KH003','HD023','MA023',5,N'Gỏi cuốn vừa miệng.','2026-03-13T13:00:00',1,1),
('DG009','KH017','HD017','MA017',4,N'Trà đào thơm, sẽ đặt lại.','2026-09-19T17:30:00',1,1),
('DG010','KH018','HD018','MA018',5,N'Nước suối giao đúng giờ.','2026-10-21T20:40:00',1,1),
('DG011','KH019','HD019','MA019',3,N'Nước ngọt hơi ngọt với khẩu vị của mình.','2026-10-24T19:30:00',0,1),
('DG012','KH020','HD020','MA020',4,N'Kem dừa béo và vừa miệng.','2026-10-26T12:10:00',1,0);

INSERT INTO CHI_TIET_DANH_GIA (maChiTietDanhGia,maDanhGia,maMon,soSao,noiDung) VALUES
('CTDG001','DG001','MA005',5,N'Thịt bò mềm'),('CTDG002','DG001','MA006',5,N'Chất lượng tốt'),('CTDG003','DG002','MA007',4,N'Thịt mềm'),('CTDG004','DG003','MA008',5,N'Tôm tươi'),('CTDG005','DG004','MA009',3,N'Sò điệp có vị ngọt'),('CTDG006','DG005','MA010',5,N'Mực tươi'),('CTDG007','DG006','MA011',4,N'Rau còn tươi'),('CTDG008','DG007','MA012',2,N'Món đến muộn'),('CTDG009','DG008','MA024',5,N'Chả giò giòn'),('CTDG010','DG009','MA018',4,N'Nước uống đóng gói tốt'),('CTDG011','DG010','MA019',5,N'Nước ngọt giao đủ'),('CTDG012','DG012','MA021',4,N'Combo có khẩu phần phù hợp');

INSERT INTO TRA_LOI_DANH_GIA (maTraLoi,maDanhGia,maNhanVien,noiDung,ngayGui) VALUES
('TL001','DG001','NV05',N'Cảm ơn anh/chị đã ủng hộ nhà hàng.','2026-01-04T09:00:00'),('TL002','DG002','NV06',N'Nhà hàng cảm ơn góp ý của anh/chị.','2026-02-05T09:30:00'),('TL003','DG003','NV05',N'Rất vui vì anh/chị hài lòng với hải sản.','2026-03-08T10:00:00'),('TL004','DG004','NV06',N'Nhà hàng đã ghi nhận góp ý về độ ngọt.','2026-04-10T10:15:00'),('TL005','DG005','NV15',N'Cảm ơn anh/chị đã chọn combo của nhà hàng.','2026-05-12T11:00:00'),('TL006','DG007','NV05',N'Nhà hàng xin lỗi vì đơn giao chậm và sẽ kiểm tra quy trình.','2026-07-16T09:20:00'),('TL007','DG009','NV06',N'Cảm ơn anh/chị, nhà hàng mong được phục vụ lần tới.','2026-09-20T09:00:00'),('TL008','DG010','NV15',N'Cảm ơn đánh giá tích cực của anh/chị.','2026-10-22T09:15:00');

-- 8. Khảo sát: hai phiếu, có câu hỏi lựa chọn và câu hỏi tự do
INSERT INTO KHAO_SAT (maKhaoSat,maNhanVien,tieuDe,trangThai,ngayTao) VALUES
('KS001','NV05',N'Khảo sát trải nghiệm đặt món','Closed','2026-05-01T09:00:00'),
('KS002','NV06',N'Khảo sát món ăn mùa hè','Open','2026-07-01T09:00:00');

INSERT INTO CAU_HOI_KHAO_SAT (maCauHoi,maKhaoSat,noiDungCauHoi,loaiCauHoi) VALUES
('CH001','KS001',N'Bạn đánh giá tốc độ giao hàng thế nào?',N'Option'),('CH002','KS001',N'Bạn biết nhà hàng qua kênh nào?',N'Option'),('CH003','KS001',N'Bạn muốn nhà hàng cải thiện điều gì?',N'Text'),
('CH004','KS002',N'Món bạn yêu thích nhất là gì?',N'Option'),('CH005','KS002',N'Mức độ hài lòng về chất lượng món ăn?',N'Option'),('CH006','KS002',N'Góp ý thêm cho nhà hàng',N'Text');

INSERT INTO TUY_CHON_CAU_HOI (maTuyChon,maCauHoi,noiDungTuyChon) VALUES
('TC001','CH001',N'Rất nhanh'),('TC002','CH001',N'Đúng giờ'),('TC003','CH001',N'Hơi chậm'),('TC004','CH001',N'Chậm'),
('TC005','CH002',N'Mạng xã hội'),('TC006','CH002',N'Bạn bè giới thiệu'),('TC007','CH002',N'Tìm kiếm trên mạng'),('TC008','CH002',N'Khác'),
('TC009','CH004',N'Lẩu'),('TC010','CH004',N'Thịt và hải sản'),('TC011','CH004',N'Đồ uống'),('TC012','CH004',N'Combo'),
('TC013','CH005',N'Rất hài lòng'),('TC014','CH005',N'Hài lòng'),('TC015','CH005',N'Bình thường'),('TC016','CH005',N'Chưa hài lòng');

INSERT INTO DOI_TUONG_KHAO_SAT (maDoiTuong,maKhaoSat,maKhachHang,daHoanThanh) VALUES
('DT001','KS001','KH001',1),('DT002','KS001','KH002',1),('DT003','KS001','KH003',1),('DT004','KS001','KH004',0),('DT005','KS001','KH005',1),('DT006','KS001','KH006',1),
('DT007','KS002','KH007',1),('DT008','KS002','KH008',1),('DT009','KS002','KH009',0),('DT010','KS002','KH010',1),('DT011','KS002','KH011',1),('DT012','KS002','KH012',0);

INSERT INTO PHIEU_TRA_LOI (maPhieuTraLoi,maKhaoSat,maKhachHang,ngayTraLoi) VALUES
('PTL001','KS001','KH001','2026-05-03T12:00:00'),('PTL002','KS001','KH002','2026-05-04T13:00:00'),('PTL003','KS001','KH003','2026-05-05T14:00:00'),('PTL004','KS001','KH005','2026-05-07T15:00:00'),('PTL005','KS001','KH006','2026-05-08T16:00:00'),
('PTL006','KS002','KH007','2026-07-03T12:30:00'),('PTL007','KS002','KH008','2026-07-04T13:30:00'),('PTL008','KS002','KH010','2026-07-06T14:30:00'),('PTL009','KS002','KH011','2026-07-08T15:30:00'),('PTL010','KS002','KH012','2026-07-10T16:30:00');

INSERT INTO CAU_TRA_LOI (maCauTraLoi,maPhieuTraLoi,maCauHoi,maTuyChon,noiDungTuDien) VALUES
('CTL001','PTL001','CH001','TC001',NULL),('CTL002','PTL001','CH002','TC005',NULL),('CTL003','PTL001','CH003',NULL,N'Thêm lựa chọn ít cay'),
('CTL004','PTL002','CH001','TC002',NULL),('CTL005','PTL002','CH002','TC006',NULL),('CTL006','PTL002','CH003',NULL,N'Đóng gói thêm muỗng đũa'),
('CTL007','PTL003','CH001','TC003',NULL),('CTL008','PTL003','CH002','TC007',NULL),('CTL009','PTL003','CH003',NULL,N'Thời gian giao hàng có thể nhanh hơn'),
('CTL010','PTL004','CH001','TC001',NULL),('CTL011','PTL004','CH002','TC005',NULL),('CTL012','PTL004','CH003',NULL,N'Tôi hài lòng với dịch vụ'),
('CTL013','PTL005','CH001','TC002',NULL),('CTL014','PTL005','CH002','TC006',NULL),('CTL015','PTL005','CH003',NULL,N'Mong có thêm món rau'),
('CTL016','PTL006','CH004','TC009',NULL),('CTL017','PTL006','CH005','TC013',NULL),('CTL018','PTL006','CH006',NULL,N'Lẩu ngon và giao nhanh'),
('CTL019','PTL007','CH004','TC010',NULL),('CTL020','PTL007','CH005','TC014',NULL),('CTL021','PTL007','CH006',NULL,N'Thêm combo cho nhóm đông'),
('CTL022','PTL008','CH004','TC011',NULL),('CTL023','PTL008','CH005','TC013',NULL),('CTL024','PTL008','CH006',NULL,N'Trà đào nên giảm ngọt'),
('CTL025','PTL009','CH004','TC012',NULL),('CTL026','PTL009','CH005','TC015',NULL),('CTL027','PTL009','CH006',NULL,N'Món ăn đa dạng'),
('CTL028','PTL010','CH004','TC009',NULL),('CTL029','PTL010','CH005','TC014',NULL),('CTL030','PTL010','CH006',NULL,N'Chất lượng ổn định');
GO

PRINT N'Đã thêm dữ liệu mẫu RestaurantCRM thành công.';
PRINT N'Quy mô: 60 hóa đơn, 120 chi tiết hóa đơn, 24 món ăn, 20 khách hàng; tất cả bảng trong schema đều có dữ liệu.';

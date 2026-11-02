-- =============================================================
-- BO DU LIEU MAU RUT GON - RESTAURANT CRM (khớp cấu trúc RestaurantCRM.sql)
-- Dữ liệu mẫu đa dạng; định dạng mã giữ như bộ cũ (VT01, NV01, KH01, LM001, MA001, NL01, ...).
-- Bản rút gọn có 12 đơn hàng, phân bố đều trong 4 quý năm 2026 (3 đơn mỗi quý).
-- Giữ dữ liệu mẫu liên kết cho bán hàng, kho, đánh giá, khảo sát và phân quyền.
-- Tài khoản đăng nhập: admin / 123456 (mọi nhân viên dùng mật khẩu 123456).
-- Chạy trên CSDL RỖNG (vừa tạo bằng RestaurantCRM.sql). Nếu CSDL đã có dữ liệu cũ,
-- bỏ comment khối XÓA DỮ LIỆU CŨ bên dưới trước khi chạy.
-- =============================================================
USE RestaurantCRM;
GO

/* ---- XÓA DỮ LIỆU CŨ (tùy chọn) ----
DELETE FROM CAU_TRA_LOI; DELETE FROM PHIEU_TRA_LOI; DELETE FROM DOI_TUONG_KHAO_SAT;
DELETE FROM TUY_CHON_CAU_HOI; DELETE FROM CAU_HOI_KHAO_SAT; DELETE FROM KHAO_SAT;
DELETE FROM TRA_LOI_DANH_GIA; DELETE FROM LICH_SU_TRANG_THAI; DELETE FROM DANH_GIA;
DELETE FROM THANH_TOAN; DELETE FROM CHI_TIET_HOA_DON; DELETE FROM HOA_DON;
DELETE FROM KM_THEO_SP; DELETE FROM KM_THEO_LOAI; DELETE FROM KM_THEO_DON_HANG; DELETE FROM CHUONG_TRINH_KHUYEN_MAI;
DELETE FROM CHI_TIET_PHIEU_NHAP; DELETE FROM PHIEU_NHAP_HANG; DELETE FROM NHA_CUNG_CAP;
DELETE FROM DINH_LUONG; DELETE FROM NGUYEN_LIEU; DELETE FROM MON_AN; DELETE FROM LOAI_MON;
DELETE FROM KHACH_HANG; DELETE FROM NHAN_VIEN; DELETE FROM VAI_TRO_QUYEN; DELETE FROM QUYEN; DELETE FROM VAI_TRO;
GO
*/

-- 1. VAI_TRO (5 vai trò)
-- Bản rút gọn: 12 đơn (3 đơn mỗi quý), dữ liệu liên quan được giữ theo khóa ngoại.
-- VAI_TRO: 5 dòng
INSERT INTO VAI_TRO (maVaiTro, tenVaiTro, moTa) VALUES
  ('VT01', N'admin', N'Quản trị toàn hệ thống'),
  ('VT02', N'sale', N'Quản lý toàn bộ hoạt động khối bán hàng, bao gồm bán hàng và kho'),
  ('VT03', N'bán hàng', N'Nhân viên xử lý các nghiệp vụ liên quan đến bán hàng'),
  ('VT04', N'kho', N'Quản lý kho, nguyên vật liệu và phiếu nhập'),
  ('VT05', N'crm', N'Quản lý và chăm sóc khách hàng');
GO
-- QUYEN: 16 dòng
INSERT INTO QUYEN (maQuyen, tenQuyen, nhomQuyen, moTa) VALUES
  ('ADMIN_ACCOUNT', N'Quản lý Tài khoản', N'Hệ thống', N'Quản trị danh sách tài khoản nhân viên'),
  ('ADMIN_BACKUP', N'Sao lưu & Phục hồi CSDL', N'Hệ thống', N'Toàn quyền sao lưu, tải về và phục hồi cơ sở dữ liệu'),
  ('ADMIN_ROLE', N'Vai trò & Phân quyền', N'Hệ thống', N'Cấu hình vai trò và gán quyền chi tiết'),
  ('CRM_ANALYTICS', N'Phân tích khách hàng', N'Khách hàng', N'Xem biểu đồ và báo cáo phân tích tệp khách hàng'),
  ('CRM_CUSTOMER', N'Quản lý khách hàng', N'Khách hàng', N'Xem và quản lý hồ sơ khách hàng'),
  ('CRM_FEEDBACK', N'Góp ý & Phản hồi', N'Khách hàng', N'Xử lý góp ý và khiếu nại của khách'),
  ('CRM_REVIEW', N'Đánh giá dịch vụ', N'Khách hàng', N'Xem và phản hồi đánh giá món ăn / dịch vụ'),
  ('CRM_SURVEY', N'Khảo sát ý kiến', N'Khách hàng', N'Thiết lập và quản lý các phiếu khảo sát'),
  ('IMPORT_VIEW', N'Quản lý phiếu nhập', N'Vận Hành', N'Xem và tạo phiếu nhập nguyên vật liệu'),
  ('INGREDIENT_VIEW', N'Nguyên vật liệu', N'Vận Hành', N'Quản lý danh mục nguyên vật liệu nhà hàng'),
  ('INVENTORY_VIEW', N'Kiểm tra kho', N'Vận Hành', N'Xem thông tin tồn kho thực phẩm'),
  ('MENU_VIEW', N'Thực đơn món ăn', N'Vận Hành', N'Xem và tra cứu thực đơn món ăn'),
  ('ORDER_CREATE', N'Tạo đơn hàng mới', N'Vận Hành', N'Cho phép mở màn hình Tạo đơn hàng và lập hóa đơn mới'),
  ('ORDER_VIEW', N'Quản lý đơn hàng', N'Vận Hành', N'Xem danh sách và trạng thái hóa đơn / đơn hàng'),
  ('PROMO_VIEW', N'Khuyến mãi', N'Vận Hành', N'Quản lý chương trình khuyến mãi và voucher'),
  ('REPORT_VIEW', N'Báo cáo doanh thu', N'Vận Hành', N'Xem báo cáo thống kê doanh thu bán hàng');
GO
-- VAI_TRO_QUYEN: 37 dòng
INSERT INTO VAI_TRO_QUYEN (maVaiTro, maQuyen, ngayTao) VALUES
  ('VT01', 'ADMIN_ACCOUNT', '2026-08-20T08:00:00'),
  ('VT01', 'ADMIN_BACKUP', '2026-08-20T08:00:00'),
  ('VT01', 'ADMIN_ROLE', '2026-08-20T08:00:00'),
  ('VT01', 'CRM_ANALYTICS', '2026-08-20T08:00:00'),
  ('VT01', 'CRM_CUSTOMER', '2026-08-20T08:00:00'),
  ('VT01', 'CRM_FEEDBACK', '2026-08-20T08:00:00'),
  ('VT01', 'CRM_REVIEW', '2026-08-20T08:00:00'),
  ('VT01', 'CRM_SURVEY', '2026-08-20T08:00:00'),
  ('VT01', 'IMPORT_VIEW', '2026-08-20T08:00:00'),
  ('VT01', 'INGREDIENT_VIEW', '2026-08-20T08:00:00'),
  ('VT01', 'INVENTORY_VIEW', '2026-08-20T08:00:00'),
  ('VT01', 'MENU_VIEW', '2026-08-20T08:00:00'),
  ('VT01', 'ORDER_CREATE', '2026-08-20T08:00:00'),
  ('VT01', 'ORDER_VIEW', '2026-08-20T08:00:00'),
  ('VT01', 'PROMO_VIEW', '2026-08-20T08:00:00'),
  ('VT01', 'REPORT_VIEW', '2026-08-20T08:00:00'),
  ('VT02', 'IMPORT_VIEW', '2026-08-20T08:00:00'),
  ('VT02', 'INGREDIENT_VIEW', '2026-08-20T08:00:00'),
  ('VT02', 'INVENTORY_VIEW', '2026-08-20T08:00:00'),
  ('VT02', 'MENU_VIEW', '2026-08-20T08:00:00'),
  ('VT02', 'ORDER_CREATE', '2026-08-20T08:00:00'),
  ('VT02', 'ORDER_VIEW', '2026-08-20T08:00:00'),
  ('VT02', 'PROMO_VIEW', '2026-08-20T08:00:00'),
  ('VT02', 'REPORT_VIEW', '2026-08-20T08:00:00'),
  ('VT03', 'MENU_VIEW', '2026-08-20T08:00:00'),
  ('VT03', 'ORDER_CREATE', '2026-08-20T08:00:00'),
  ('VT03', 'ORDER_VIEW', '2026-08-20T08:00:00'),
  ('VT03', 'PROMO_VIEW', '2026-08-20T08:00:00'),
  ('VT03', 'REPORT_VIEW', '2026-08-20T08:00:00'),
  ('VT04', 'IMPORT_VIEW', '2026-08-20T08:00:00'),
  ('VT04', 'INGREDIENT_VIEW', '2026-08-20T08:00:00'),
  ('VT04', 'INVENTORY_VIEW', '2026-08-20T08:00:00'),
  ('VT05', 'CRM_ANALYTICS', '2026-08-20T08:00:00'),
  ('VT05', 'CRM_CUSTOMER', '2026-08-20T08:00:00'),
  ('VT05', 'CRM_FEEDBACK', '2026-08-20T08:00:00'),
  ('VT05', 'CRM_REVIEW', '2026-08-20T08:00:00'),
  ('VT05', 'CRM_SURVEY', '2026-08-20T08:00:00');
GO
-- NHAN_VIEN: 15 dòng
INSERT INTO NHAN_VIEN (maNhanVien, maVaiTro, tenDangNhap, matKhau, hoTen, trangThai) VALUES
  ('NV01', 'VT01', N'admin', N'e10adc3949ba59abbe56e057f20f883e', N'Nguyễn Quốc Bảo', N'Active'),
  ('NV02', 'VT02', N'sale01', N'e10adc3949ba59abbe56e057f20f883e', N'Phạm Văn Đức', N'Active'),
  ('NV03', 'VT03', N'sale02', N'e10adc3949ba59abbe56e057f20f883e', N'Võ Thị Mai', N'Active'),
  ('NV04', 'VT03', N'sale03', N'e10adc3949ba59abbe56e057f20f883e', N'Trần Gia Hân', N'Active'),
  ('NV05', 'VT05', N'crm01', N'e10adc3949ba59abbe56e057f20f883e', N'Đặng Hoàng Long', N'Active'),
  ('NV06', 'VT05', N'crm02', N'e10adc3949ba59abbe56e057f20f883e', N'Lương Thảo Vy', N'Active'),
  ('NV07', 'VT04', N'kho01', N'e10adc3949ba59abbe56e057f20f883e', N'Ngô Thị Thu Hà', N'Active'),
  ('NV08', 'VT04', N'kho02', N'e10adc3949ba59abbe56e057f20f883e', N'Phùng Văn Sơn', N'Active'),
  ('NV09', 'VT03', N'thungan01', N'e10adc3949ba59abbe56e057f20f883e', N'Lê Thị Hồng Nhung', N'Active'),
  ('NV10', 'VT03', N'bep01', N'e10adc3949ba59abbe56e057f20f883e', N'Bùi Quang Vinh', N'Active'),
  ('NV11', 'VT03', N'phucvu01', N'e10adc3949ba59abbe56e057f20f883e', N'Huỳnh Minh Tuấn', N'Inactive'),
  ('NV12', 'VT03', N'giaohang01', N'e10adc3949ba59abbe56e057f20f883e', N'Tạ Đình Phong', N'Active'),
  ('NV13', 'VT02', N'ketoan01', N'e10adc3949ba59abbe56e057f20f883e', N'Châu Ngọc Trâm', N'Active'),
  ('NV14', 'VT02', N'marketing01', N'e10adc3949ba59abbe56e057f20f883e', N'Kiều Anh Thư', N'Active'),
  ('NV15', 'VT02', N'giamsat01', N'e10adc3949ba59abbe56e057f20f883e', N'Trần Minh Khôi', N'Active');
GO
-- KHACH_HANG: 14 dòng
INSERT INTO KHACH_HANG (maKhachHang, hoTen, soDienThoai, email, matKhau, ngaySinh, gioiTinh, diaChi, trangThai) VALUES
  ('KH04', N'Phạm Ngọc Diệp', '0904938364', N'diep.pham@gmail.com', N'e10adc3949ba59abbe56e057f20f883e', '1993-01-30', N'Nữ', N'120 Pasteur, Phường Võ Thị Sáu, Quận 3, TP. Hồ Chí Minh', N'Active'),
  ('KH06', N'Đặng Thị Kim Ngân', '0906407546', N'ngan.dang@gmail.com', N'e10adc3949ba59abbe56e057f20f883e', '2000-05-02', N'Nữ', N'68 Nam Kỳ Khởi Nghĩa, Phường Nguyễn Thái Bình, Quận 1, TP. Hồ Chí Minh', N'Active'),
  ('KH10', N'Dương Mỹ Linh', '0910345910', N'linh.duong@gmail.com', N'e10adc3949ba59abbe56e057f20f883e', '1996-10-11', N'Nữ', N'77 Trần Hưng Đạo, Phường Cầu Ông Lãnh, Quận 1, TP. Hồ Chí Minh', N'Active'),
  ('KH12', N'Lý Bảo Châu', '0912815092', N'chau.ly@gmail.com', N'e10adc3949ba59abbe56e057f20f883e', '2001-06-06', N'Nữ', N'250 Lý Thường Kiệt, Phường 14, Quận 10, TP. Hồ Chí Minh', N'Locked'),
  ('KH13', N'Phan Thị Hồng Nhung', '0913549683', N'hongnhung.phan@gmail.com', N'e10adc3949ba59abbe56e057f20f883e', '1994-04-21', N'Nữ', N'27 Võ Thị Sáu, Phường Đa Kao, Quận 1, TP. Hồ Chí Minh', N'Active'),
  ('KH14', N'Nguyễn Hữu Tài', '0914284274', N'tai.nguyen@gmail.com', N'e10adc3949ba59abbe56e057f20f883e', '1989-09-03', N'Nam', N'190 Nguyễn Tri Phương, Phường 9, Quận 10, TP. Hồ Chí Minh', N'Active'),
  ('KH15', N'Lâm Thị Cẩm Tú', '0915018865', N'camtu.lam@gmail.com', N'e10adc3949ba59abbe56e057f20f883e', '1998-12-12', N'Nữ', N'36 Võ Văn Ngân, Phường Linh Chiểu, TP. Thủ Đức, TP. Hồ Chí Minh', N'Active'),
  ('KH16', N'Đỗ Quang Huy', '0916753456', N'huy.do@outlook.com', N'e10adc3949ba59abbe56e057f20f883e', '1991-07-30', N'Nam', N'412 Quang Trung, Phường 10, Quận Gò Vấp, TP. Hồ Chí Minh', N'Active'),
  ('KH18', N'Huỳnh Gia Bảo', '0918222638', N'bao.huynh@gmail.com', N'e10adc3949ba59abbe56e057f20f883e', '1986-10-26', N'Nam', N'25 Lũy Bán Bích, Phường Tân Thới Hòa, Quận Tân Phú, TP. Hồ Chí Minh', N'Active'),
  ('KH19', N'Mai Phương Thảo', '0919957229', N'phuongthao.mai@gmail.com', N'e10adc3949ba59abbe56e057f20f883e', '1995-06-17', N'Nữ', N'140 Nguyễn Trãi, Phường 2, Quận 5, TP. Hồ Chí Minh', N'Active'),
  ('KH20', N'Cao Minh Trí', '0920691820', N'tri.cao@gmail.com', N'e10adc3949ba59abbe56e057f20f883e', '1984-01-09', N'Nam', N'9 Phạm Văn Đồng, Phường 3, Quận Gò Vấp, TP. Hồ Chí Minh', N'Locked'),
  ('KH21', N'Tô Ngọc Hân', '0921426411', N'han.to@gmail.com', N'e10adc3949ba59abbe56e057f20f883e', '1999-03-03', N'Nữ', N'61 Hoàng Văn Thụ, Phường 9, Quận Phú Nhuận, TP. Hồ Chí Minh', N'Active'),
  ('KH24', N'Hà Nhật Nam', '0924630184', N'nam.ha@outlook.com', N'e10adc3949ba59abbe56e057f20f883e', '2000-11-08', N'Nam', N'5 Nguyễn Hữu Thọ, Phường Tân Hưng, Quận 7, TP. Hồ Chí Minh', N'Active'),
  ('KH25', N'Kim Thùy Dung', '0925364775', N'dung.kim@gmail.com', N'e10adc3949ba59abbe56e057f20f883e', '1996-01-15', N'Nữ', N'99 Bạch Đằng, Phường 2, Quận Tân Bình, TP. Hồ Chí Minh', N'Active');
GO
-- LOAI_MON: 5 dòng
INSERT INTO LOAI_MON (maLoaiMon, tenLoaiMon) VALUES
  ('LM001', N'Lẩu'),
  ('LM002', N'Thịt, hải sản & viên nhúng'),
  ('LM003', N'Rau, mì & món ăn kèm'),
  ('LM004', N'Đồ uống & tráng miệng'),
  ('LM005', N'Món nướng & combo');
GO
-- MON_AN: 24 dòng
INSERT INTO MON_AN (maMon, maLoaiMon, tenMon, moTa, duongDanAnh, donGia, trangThai) VALUES
  ('MA001', 'LM001', N'Lẩu Thái chua cay', N'Nước lẩu Thái chua cay thơm sả, lá chanh, kèm tôm và rau tươi.', N'https://images.unsplash.com/photo-1547592180-85f173990554?auto=format&fit=crop&w=800&q=80', 189000, N'InStock'),
  ('MA002', 'LM001', N'Lẩu Tứ Xuyên cay tê', N'Nước lẩu Tứ Xuyên cay tê đậm vị hoa tiêu và ớt khô.', N'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?auto=format&fit=crop&w=800&q=80', 219000, N'InStock'),
  ('MA004', 'LM001', N'Lẩu kim chi Hàn Quốc', N'Lẩu kim chi cay nhẹ, ăn kèm ba chỉ heo và đậu hũ.', N'https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=800&q=80', 195000, N'InStock'),
  ('MA005', 'LM002', N'Ba chỉ bò Mỹ', N'Ba chỉ bò Mỹ thái mỏng, mềm ngọt, nhúng nhanh.', N'https://images.unsplash.com/photo-1559847844-5315695dadae?auto=format&fit=crop&w=800&q=80', 129000, N'InStock'),
  ('MA006', 'LM002', N'Bò Wagyu A4 thái lát', N'Wagyu A4 vân mỡ đẹp, tan trong miệng.', N'https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?auto=format&fit=crop&w=800&q=80', 459000, N'InStock'),
  ('MA007', 'LM002', N'Nạc vai bò thái mỏng', N'Nạc vai bò ít mỡ, đậm vị bò.', N'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?auto=format&fit=crop&w=800&q=80', 139000, N'InStock'),
  ('MA008', 'LM002', N'Tôm sú tươi', N'Tôm sú tươi chắc thịt, ngọt.', N'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?auto=format&fit=crop&w=800&q=80', 149000, N'InStock'),
  ('MA009', 'LM002', N'Sò điệp Nhật', N'Sò điệp Nhật size lớn, thịt dày béo.', N'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=800&q=80', 189000, N'InStock'),
  ('MA011', 'LM003', N'Rau lẩu thập cẩm', N'Rau xanh tươi theo mùa cho nồi lẩu.', N'https://images.unsplash.com/photo-1547592180-85f173990554?auto=format&fit=crop&w=800&q=80', 49000, N'InStock'),
  ('MA016', 'LM003', N'Cơm chiên trứng', N'Cơm chiên trứng tơi hạt.', N'https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?auto=format&fit=crop&w=800&q=80', 55000, N'InStock'),
  ('MA017', 'LM004', N'Trà đào cam sả', N'Trà đào thơm mát, có miếng đào.', N'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?auto=format&fit=crop&w=800&q=80', 39000, N'InStock'),
  ('MA018', 'LM004', N'Bia lon', N'Bia lon ướp lạnh.', N'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?auto=format&fit=crop&w=800&q=80', 29000, N'InStock'),
  ('MA020', 'LM004', N'Kem dừa Thái', N'Kem dừa béo mát, rắc đậu phộng.', N'https://images.unsplash.com/photo-1563805042-7684c019e1cb?auto=format&fit=crop&w=800&q=80', 45000, N'InStock'),
  ('MA023', 'LM003', N'Gỏi cuốn tôm thịt', N'Gỏi cuốn tôm thịt chấm tương đậu.', N'https://images.unsplash.com/photo-1555939594-58d7cb561ad1?auto=format&fit=crop&w=800&q=80', 59000, N'InStock'),
  ('MA024', 'LM003', N'Chả giò hải sản', N'Chả giò hải sản giòn rụm.', N'https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=800&q=80', 69000, N'InStock'),
  ('MA025', 'LM003', N'Khoai tây chiên', N'Khoai tây chiên giòn, rắc muối.', N'https://images.unsplash.com/photo-1559847844-5315695dadae?auto=format&fit=crop&w=800&q=80', 45000, N'InStock'),
  ('MA026', 'LM002', N'Chả cá thác lác', N'Chả cá thác lác dai ngọt, viên nhúng lẩu.', N'https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?auto=format&fit=crop&w=800&q=80', 79000, N'InStock'),
  ('MA027', 'LM002', N'Bò viên gân', N'Bò viên gân giòn sần sật.', N'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?auto=format&fit=crop&w=800&q=80', 69000, N'InStock'),
  ('MA028', 'LM002', N'Viên thả lẩu thập cẩm', N'Đĩa viên thập cẩm: cá, tôm, phô mai.', N'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?auto=format&fit=crop&w=800&q=80', 89000, N'InStock'),
  ('MA029', 'LM005', N'Ba chỉ heo nướng BBQ', N'Ba chỉ heo ướp sốt BBQ nướng thơm.', N'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=800&q=80', 119000, N'InStock'),
  ('MA030', 'LM005', N'Bò Mỹ nướng sốt', N'Ba chỉ bò nướng sốt mật ong.', N'https://images.unsplash.com/photo-1563805042-7684c019e1cb?auto=format&fit=crop&w=800&q=80', 139000, N'InStock'),
  ('MA032', 'LM005', N'Combo lẩu Thái 2 người', N'Lẩu Thái + bò Mỹ + rau + mì Udon.', N'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?auto=format&fit=crop&w=800&q=80', 369000, N'InStock'),
  ('MA034', 'LM005', N'Combo chay thanh đạm', N'Lẩu nấm + đậu hũ + rau + tàu hũ ky.', N'https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=800&q=80', 299000, N'InStock'),
  ('MA036', 'LM003', N'Nước chấm tương me', N'Tương me chua ngọt cho đồ nướng.', N'https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?auto=format&fit=crop&w=800&q=80', 10000, N'InStock');
GO
-- NGUYEN_LIEU: 18 dòng
INSERT INTO NGUYEN_LIEU (maNguyenLieu, tenNguyenLieu, donViTinh, soLuongTon, giaNhapGanNhat, trangThai) VALUES
  ('NL01', N'Ba chỉ bò Mỹ thái lát', N'kg', 32.15, 85000, N'Active'),
  ('NL02', N'Tôm sú tươi', N'kg', 15.25, 230000, N'Active'),
  ('NL04', N'Ba chỉ heo tươi', N'kg', 28.29, 135000, N'Active'),
  ('NL05', N'Rau lẩu thập cẩm', N'kg', 6.17, 30000, N'Active'),
  ('NL06', N'Nấm thập cẩm tươi', N'kg', 14.87, 70000, N'Active'),
  ('NL09', N'Nước cốt lẩu Thái', N'lít', 5.42, 110000, N'Active'),
  ('NL10', N'Nước cốt lẩu bò', N'lít', 36.39, 120000, N'Active'),
  ('NL11', N'Nước cốt lẩu nấm', N'lít', 13.88, 95000, N'Active'),
  ('NL13', N'Nước cốt lẩu Tứ Xuyên', N'lít', 26.04, 150000, N'Active'),
  ('NL14', N'Nước cốt lẩu kim chi', N'lít', 18.8, 115000, N'Active'),
  ('NL17', N'Nạc vai bò', N'kg', 17.96, 260000, N'Active'),
  ('NL21', N'Nấm kim châm', N'kg', 11.22, 45000, N'Active'),
  ('NL22', N'Đậu hũ non', N'kg', 21.01, 25000, N'Active'),
  ('NL25', N'Trà đào & sả', N'phần', 129, 18000, N'Active'),
  ('NL35', N'Bò viên gân', N'kg', 15.01, 110000, N'Active'),
  ('NL36', N'Sốt ướp nướng BBQ', N'lít', 15.13, 90000, N'Active'),
  ('NL39', N'Lá é & rau thơm', N'kg', 0, 60000, N'Active'),
  ('NL40', N'Đậu phụ chiên & tàu hũ ky', N'kg', 9.05, 70000, N'Active');
GO
-- DINH_LUONG: 8 dòng
INSERT INTO DINH_LUONG (maDinhLuong, maMon, maNguyenLieu, soLuongCan) VALUES
  ('DL01', 'MA001', 'NL09', 0.5),
  ('DL02', 'MA001', 'NL02', 0.15),
  ('DL03', 'MA001', 'NL05', 0.3),
  ('DL13', 'MA005', 'NL01', 0.3),
  ('DL19', 'MA011', 'NL05', 0.4),
  ('DL27', 'MA017', 'NL25', 1),
  ('DL39', 'MA029', 'NL04', 0.3),
  ('DL40', 'MA029', 'NL36', 0.05);
GO
-- NHA_CUNG_CAP: 3 dòng
INSERT INTO NHA_CUNG_CAP (maNhaCungCap, tenNhaCungCap, soDienThoai, diaChi, email, trangThai) VALUES
  ('NCC01', N'Công ty TNHH Thực phẩm Sạch Việt', '02838111111', N'12 Nguyễn Văn Linh, Quận 7, TP.HCM', N'contact@thucphamsachviet.vn', N'Active'),
  ('NCC02', N'Cơ sở Thịt bò Ba Miền', '02838122222', N'85 Quốc lộ 22, Hóc Môn, TP.HCM', N'thitbobamien@gmail.com', N'Active'),
  ('NCC04', N'Cơ sở Nước cốt Lẩu An Phát', '02838144444', N'56 Lê Văn Sỹ, Quận 3, TP.HCM', N'nuoccotlauanphat@gmail.com', N'Active');
GO
-- PHIEU_NHAP_HANG: 3 dòng
INSERT INTO PHIEU_NHAP_HANG (maPhieuNhap, maNhaCungCap, maNhanVien, ngayNhap, tongTien, ghiChu) VALUES
  ('PN2609001', 'NCC01', 'NV08', '2026-09-02T09:15:00', 2325000, N'Nhập rau, nấm đầu tháng 9'),
  ('PN2609002', 'NCC02', 'NV07', '2026-09-05T10:20:00', 3310000, N'Nhập thịt bò'),
  ('PN2609003', 'NCC04', 'NV08', '2026-09-09T08:45:00', 5720000, N'Nhập nước cốt lẩu');
GO
-- CHI_TIET_PHIEU_NHAP: 14 dòng
INSERT INTO CHI_TIET_PHIEU_NHAP (maChiTiet, maPhieuNhap, maNguyenLieu, soLuong, donGiaNhap, thanhTien) VALUES
  ('CTPN0001', 'PN2609001', 'NL05', 5, 30000, 150000),
  ('CTPN0002', 'PN2609001', 'NL06', 10, 70000, 700000),
  ('CTPN0003', 'PN2609001', 'NL21', 10, 45000, 450000),
  ('CTPN0004', 'PN2609001', 'NL22', 15, 25000, 375000),
  ('CTPN0005', 'PN2609001', 'NL39', 5, 60000, 300000),
  ('CTPN0006', 'PN2609001', 'NL40', 5, 70000, 350000),
  ('CTPN0007', 'PN2609002', 'NL01', 8, 85000, 680000),
  ('CTPN0008', 'PN2609002', 'NL17', 8, 260000, 2080000),
  ('CTPN0009', 'PN2609002', 'NL35', 5, 110000, 550000),
  ('CTPN0010', 'PN2609003', 'NL09', 12, 110000, 1320000),
  ('CTPN0011', 'PN2609003', 'NL10', 10, 120000, 1200000),
  ('CTPN0012', 'PN2609003', 'NL11', 15, 95000, 1425000),
  ('CTPN0013', 'PN2609003', 'NL13', 8, 150000, 1200000),
  ('CTPN0014', 'PN2609003', 'NL14', 5, 115000, 575000);
GO
-- CHUONG_TRINH_KHUYEN_MAI: 6 dòng
INSERT INTO CHUONG_TRINH_KHUYEN_MAI (maChuongTrinh, tenChuongTrinh, ngayBatDau, ngayKetThuc) VALUES
  ('CTKM260901', N'Chào tháng 9 rộn ràng', '2026-09-01', '2026-09-30'),
  ('CTKM260904', N'Ưu đãi khách thân thiết', '2026-09-01', '2026-12-31'),
  ('CTKM260905', N'Sinh nhật Lẩu Phố', '2026-08-01', '2026-12-31'),
  ('CTKM261001', N'Mùa mưa ấm bụng', '2026-10-01', '2026-11-30'),
  ('CTKM261002', N'Cuối tuần sum vầy', '2026-10-01', '2026-12-31'),
  ('CTKM261004', N'Halloween cay nồng', '2026-10-25', '2026-10-31');
GO
-- KM_THEO_DON_HANG: 1 dòng
INSERT INTO KM_THEO_DON_HANG (maKMDonHang, maChuongTrinh, maVoucher, giaTriDonToiThieu, phanTramGiam, tienGiam, giamToiDa, soLuongToiThieu, choPhepCongDon) VALUES
  ('KMHD005', 'CTKM260905', 'D05', 200000, 8, NULL, 100000, 1, 0);
GO
-- KM_THEO_LOAI: 5 dòng
INSERT INTO KM_THEO_LOAI (maKMLoai, maChuongTrinh, maLoaiMon, maVoucher, giaTriDonToiThieu, phanTramGiam, tienGiam, giamToiDa, soLuongToiThieu, choPhepCongDon) VALUES
  ('KML001', 'CTKM260901', 'LM001', 'L01', 200000, 10, NULL, 30000, 1, 0),
  ('KML004', 'CTKM260904', 'LM003', 'L04', 80000, NULL, 10000, NULL, 1, 1),
  ('KML005', 'CTKM260905', 'LM003', 'L05', 100000, 10, NULL, 15000, 1, 0),
  ('KML007', 'CTKM261002', 'LM002', 'L07', 200000, 10, NULL, 40000, 1, 0),
  ('KML009', 'CTKM261004', 'LM005', 'L09', 150000, 15, NULL, 40000, 1, 0);
GO
-- KM_THEO_SP: 3 dòng
INSERT INTO KM_THEO_SP (maKMSP, maChuongTrinh, maMon, maVoucher, phanTramGiam, tienGiam, giamToiDa, soLuongToiThieu, choPhepCongDon) VALUES
  ('KMSP001', 'CTKM260901', 'MA001', 'S01', 10, NULL, 25000, 1, 0),
  ('KMSP005', 'CTKM260905', 'MA006', 'S05', NULL, 50000, NULL, 1, 0),
  ('KMSP006', 'CTKM261001', 'MA004', 'S06', 10, NULL, 25000, 1, 1);
GO
-- HOA_DON: 9 dòng
INSERT INTO HOA_DON (maHoaDon, maKhachHang, maNhanVien, maKMDonHang, tenKhachHang, soDienThoai, diaChiGiao, ngayDat, ngayHoanTat, trangThai, tongTienHang, tienGiamVoucher, tongThanhToan) VALUES
  ('HD260903-005', 'KH24', 'NV03', NULL, N'Hà Nhật Nam', '0924630184', N'5 Nguyễn Hữu Thọ, Quận 7, TP. Hồ Chí Minh', '2026-02-13T19:52:00', '2026-02-13T20:59:00', N'Completed', 894000, 0, 894000),
  ('HD260905-008', 'KH20', 'NV02', NULL, N'Cao Minh Trí', '0920691820', N'9 Phạm Văn Đồng, Quận Gò Vấp, TP. Hồ Chí Minh', '2026-11-14T16:31:00', '2026-11-14T17:19:00', N'Completed', 494000, 0, 494000),
  ('HD260906-010', 'KH15', 'NV04', 'KMHD005', N'Lâm Thị Cẩm Tú', '0915018865', N'36 Võ Văn Ngân, TP. Thủ Đức, TP. Hồ Chí Minh', '2026-06-08T14:57:00', '2026-06-08T15:45:00', N'Completed', 386000, 30000, 356000),
  ('HD260908-012', 'KH13', 'NV03', NULL, N'Phan Thị Hồng Nhung', '0913549683', N'27 Võ Thị Sáu, Quận 1, TP. Hồ Chí Minh', '2026-12-22T12:23:00', NULL, N'Cancelled', 1231000, 0, 1231000),
  ('HD260909-015', 'KH14', 'NV02', NULL, N'Nguyễn Hữu Tài', '0914284274', N'190 Nguyễn Tri Phương, Quận 10, TP. Hồ Chí Minh', '2026-07-23T21:02:00', '2026-07-23T21:59:00', N'Completed', 672000, 0, 672000),
  ('HD260919-029', 'KH21', 'NV03', NULL, N'Tô Ngọc Hân', '0921426411', N'61 Hoàng Văn Thụ, Quận Phú Nhuận, TP. Hồ Chí Minh', '2026-02-21T19:04:00', NULL, N'Cancelled', 607000, 0, 607000),
  ('HD261006-055', 'KH19', 'NV04', NULL, N'Mai Phương Thảo', '0919957229', N'140 Nguyễn Trãi, Quận 5, TP. Hồ Chí Minh', '2026-08-23T17:42:00', NULL, N'Cancelled', 913000, 0, 913000),
  ('HD261015-070', 'KH15', 'NV03', NULL, N'Lâm Thị Cẩm Tú', '0915018865', N'36 Võ Văn Ngân, TP. Thủ Đức, TP. Hồ Chí Minh', '2026-06-08T14:57:00', NULL, N'Cancelled', 1818000, 0, 1818000),
  ('HD261026-092', 'KH25', 'NV02', NULL, N'Kim Thùy Dung', '0925364775', N'99 Bạch Đằng, Quận Tân Bình, TP. Hồ Chí Minh', '2026-11-22T16:43:00', NULL, N'Preparing', 890000, 0, 890000);
GO
-- CHI_TIET_HOA_DON: 39 dòng
INSERT INTO CHI_TIET_HOA_DON (maChiTiet, maHoaDon, maMon, maKMSP, maKMLoai, tenMon, soLuong, donGiaGoc, tienGiamMon, donGiaSauGiam, thanhTien) VALUES
  ('CT00501', 'HD260903-005', 'MA032', NULL, NULL, N'Combo lẩu Thái 2 người', 1, 369000, 0, 369000, 369000),
  ('CT00502', 'HD260903-005', 'MA008', NULL, NULL, N'Tôm sú tươi', 3, 149000, 0, 149000, 447000),
  ('CT00503', 'HD260903-005', 'MA011', NULL, 'KML004', N'Rau lẩu thập cẩm', 2, 49000, 10000, 39000, 78000),
  ('CT00801', 'HD260905-008', 'MA001', 'KMSP001', NULL, N'Lẩu Thái chua cay', 1, 189000, 18000, 171000, 171000),
  ('CT00802', 'HD260905-008', 'MA026', NULL, NULL, N'Chả cá thác lác', 2, 79000, 0, 79000, 158000),
  ('CT00803', 'HD260905-008', 'MA016', NULL, NULL, N'Cơm chiên trứng', 3, 55000, 0, 55000, 165000),
  ('CT01001', 'HD260906-010', 'MA002', NULL, 'KML001', N'Lẩu Tứ Xuyên cay tê', 1, 219000, 21000, 198000, 198000),
  ('CT01002', 'HD260906-010', 'MA030', NULL, NULL, N'Bò Mỹ nướng sốt', 1, 139000, 0, 139000, 139000),
  ('CT01003', 'HD260906-010', 'MA011', NULL, NULL, N'Rau lẩu thập cẩm', 1, 49000, 0, 49000, 49000),
  ('CT01201', 'HD260908-012', 'MA034', NULL, NULL, N'Combo chay thanh đạm', 1, 299000, 0, 299000, 299000),
  ('CT01202', 'HD260908-012', 'MA027', NULL, NULL, N'Bò viên gân', 3, 69000, 0, 69000, 207000),
  ('CT01203', 'HD260908-012', 'MA028', NULL, NULL, N'Viên thả lẩu thập cẩm', 3, 89000, 0, 89000, 267000),
  ('CT01204', 'HD260908-012', 'MA030', NULL, NULL, N'Bò Mỹ nướng sốt', 2, 139000, 0, 139000, 278000),
  ('CT01205', 'HD260908-012', 'MA020', NULL, NULL, N'Kem dừa Thái', 2, 45000, 0, 45000, 90000),
  ('CT01206', 'HD260908-012', 'MA025', NULL, NULL, N'Khoai tây chiên', 2, 45000, 0, 45000, 90000),
  ('CT01501', 'HD260909-015', 'MA004', NULL, NULL, N'Lẩu kim chi Hàn Quốc', 1, 195000, 0, 195000, 195000),
  ('CT01502', 'HD260909-015', 'MA005', NULL, NULL, N'Ba chỉ bò Mỹ', 3, 129000, 0, 129000, 387000),
  ('CT01503', 'HD260909-015', 'MA025', NULL, NULL, N'Khoai tây chiên', 2, 45000, 0, 45000, 90000),
  ('CT02901', 'HD260919-029', 'MA004', NULL, NULL, N'Lẩu kim chi Hàn Quốc', 1, 195000, 0, 195000, 195000),
  ('CT02902', 'HD260919-029', 'MA008', NULL, NULL, N'Tôm sú tươi', 2, 149000, 0, 149000, 298000),
  ('CT02903', 'HD260919-029', 'MA027', NULL, NULL, N'Bò viên gân', 1, 69000, 0, 69000, 69000),
  ('CT02904', 'HD260919-029', 'MA025', NULL, NULL, N'Khoai tây chiên', 1, 45000, 0, 45000, 45000),
  ('CT05501', 'HD261006-055', 'MA001', NULL, NULL, N'Lẩu Thái chua cay', 1, 189000, 0, 189000, 189000),
  ('CT05502', 'HD261006-055', 'MA008', NULL, 'KML007', N'Tôm sú tươi', 2, 149000, 14000, 135000, 270000),
  ('CT05503', 'HD261006-055', 'MA007', NULL, NULL, N'Nạc vai bò thái mỏng', 1, 139000, 0, 139000, 139000),
  ('CT05504', 'HD261006-055', 'MA028', NULL, NULL, N'Viên thả lẩu thập cẩm', 2, 89000, 0, 89000, 178000),
  ('CT05505', 'HD261006-055', 'MA017', NULL, NULL, N'Trà đào cam sả', 3, 39000, 0, 39000, 117000),
  ('CT05506', 'HD261006-055', 'MA036', NULL, NULL, N'Nước chấm tương me', 2, 10000, 0, 10000, 20000),
  ('CT07001', 'HD261015-070', 'MA034', NULL, NULL, N'Combo chay thanh đạm', 1, 299000, 0, 299000, 299000),
  ('CT07002', 'HD261015-070', 'MA026', NULL, NULL, N'Chả cá thác lác', 3, 79000, 0, 79000, 237000),
  ('CT07003', 'HD261015-070', 'MA006', 'KMSP005', NULL, N'Bò Wagyu A4 thái lát', 2, 459000, 50000, 409000, 818000),
  ('CT07004', 'HD261015-070', 'MA030', NULL, NULL, N'Bò Mỹ nướng sốt', 2, 139000, 0, 139000, 278000),
  ('CT07005', 'HD261015-070', 'MA023', NULL, 'KML005', N'Gỏi cuốn tôm thịt', 2, 59000, 5000, 54000, 108000),
  ('CT07006', 'HD261015-070', 'MA011', NULL, 'KML004', N'Rau lẩu thập cẩm', 2, 49000, 10000, 39000, 78000),
  ('CT09201', 'HD261026-092', 'MA004', 'KMSP006', NULL, N'Lẩu kim chi Hàn Quốc', 1, 195000, 19000, 176000, 176000),
  ('CT09202', 'HD261026-092', 'MA027', NULL, NULL, N'Bò viên gân', 2, 69000, 0, 69000, 138000),
  ('CT09203', 'HD261026-092', 'MA008', NULL, NULL, N'Tôm sú tươi', 1, 149000, 0, 149000, 149000),
  ('CT09204', 'HD261026-092', 'MA030', NULL, 'KML009', N'Bò Mỹ nướng sốt', 2, 139000, 20000, 119000, 238000),
  ('CT09205', 'HD261026-092', 'MA024', NULL, 'KML005', N'Chả giò hải sản', 3, 69000, 6000, 63000, 189000);
GO
-- THANH_TOAN: 9 dòng
INSERT INTO THANH_TOAN (maThanhToan, maHoaDon, phuongThuc, trangThai, ngayThanhToan, soTien) VALUES
  ('TT005', 'HD260903-005', N'Tiền mặt', N'Paid', '2026-02-13T20:59:00', 894000),
  ('TT008', 'HD260905-008', N'Ví điện tử', N'Paid', '2026-11-14T17:19:00', 494000),
  ('TT010', 'HD260906-010', N'Chuyển khoản', N'Paid', '2026-06-08T15:45:00', 356000),
  ('TT012', 'HD260908-012', N'Ví điện tử', N'Failed', '2026-12-22T12:23:00', 0),
  ('TT015', 'HD260909-015', N'Thẻ ngân hàng', N'Paid', '2026-07-23T21:59:00', 672000),
  ('TT029', 'HD260919-029', N'Tiền mặt', N'Failed', '2026-02-21T19:04:00', 0),
  ('TT055', 'HD261006-055', N'Thẻ ngân hàng', N'Failed', '2026-08-23T17:42:00', 0),
  ('TT070', 'HD261015-070', N'Chuyển khoản', N'Failed', '2026-06-08T14:57:00', 0),
  ('TT092', 'HD261026-092', N'Ví điện tử', N'Pending', '2026-11-22T16:43:00', 0);
GO
-- DANH_GIA: 4 dòng
INSERT INTO DANH_GIA (maDanhGia, maKhachHang, maHoaDon, maMon, soSao, noiDung, ngayDanhGia, trangThai) VALUES
  ('DG001', 'KH24', 'HD260903-005', 'MA011', 4, N'Món ngon, giá hợp lý.', '2026-02-14T12:59:00', 1),
  ('DG002', 'KH20', 'HD260905-008', 'MA016', 5, N'Nhân viên thân thiện, không gian sạch sẽ.', '2026-11-14T19:19:00', 1),
  ('DG003', 'KH15', 'HD260906-010', 'MA002', 4, N'Đóng gói cẩn thận, giao đúng giờ.', '2026-06-09T20:45:00', 1),
  ('DG004', 'KH14', 'HD260909-015', 'MA004', 5, N'Nguyên liệu tươi, phục vụ nhanh.', '2026-07-25T09:59:00', 1);
GO
-- LICH_SU_TRANG_THAI: 31 dòng
INSERT INTO LICH_SU_TRANG_THAI (maLichSu, maHoaDon, trangThai, thoiGian, ghiChu) VALUES
  ('LS00501', 'HD260903-005', N'Pending', '2026-02-13T19:52:00', N'Chuyển trạng thái Pending.'),
  ('LS00502', 'HD260903-005', N'Confirmed', '2026-02-13T20:00:00', N'Chuyển trạng thái Confirmed.'),
  ('LS00503', 'HD260903-005', N'Preparing', '2026-02-13T20:08:00', N'Chuyển trạng thái Preparing.'),
  ('LS00504', 'HD260903-005', N'Delivering', '2026-02-13T20:16:00', N'Chuyển trạng thái Delivering.'),
  ('LS00505', 'HD260903-005', N'Completed', '2026-02-13T20:59:00', N'Chuyển trạng thái Completed.'),
  ('LS00801', 'HD260905-008', N'Pending', '2026-11-14T16:31:00', N'Chuyển trạng thái Pending.'),
  ('LS00802', 'HD260905-008', N'Confirmed', '2026-11-14T16:39:00', N'Chuyển trạng thái Confirmed.'),
  ('LS00803', 'HD260905-008', N'Preparing', '2026-11-14T16:47:00', N'Chuyển trạng thái Preparing.'),
  ('LS00804', 'HD260905-008', N'Delivering', '2026-11-14T16:55:00', N'Chuyển trạng thái Delivering.'),
  ('LS00805', 'HD260905-008', N'Completed', '2026-11-14T17:19:00', N'Chuyển trạng thái Completed.'),
  ('LS01001', 'HD260906-010', N'Pending', '2026-06-08T14:57:00', N'Chuyển trạng thái Pending.'),
  ('LS01002', 'HD260906-010', N'Confirmed', '2026-06-08T15:05:00', N'Chuyển trạng thái Confirmed.'),
  ('LS01003', 'HD260906-010', N'Preparing', '2026-06-08T15:13:00', N'Chuyển trạng thái Preparing.'),
  ('LS01004', 'HD260906-010', N'Delivering', '2026-06-08T15:21:00', N'Chuyển trạng thái Delivering.'),
  ('LS01005', 'HD260906-010', N'Completed', '2026-06-08T15:45:00', N'Chuyển trạng thái Completed.'),
  ('LS01201', 'HD260908-012', N'Pending', '2026-12-22T12:23:00', N'Chuyển trạng thái Pending.'),
  ('LS01202', 'HD260908-012', N'Cancelled', '2026-12-22T12:31:00', N'Khách hủy đơn.'),
  ('LS01501', 'HD260909-015', N'Pending', '2026-07-23T21:02:00', N'Chuyển trạng thái Pending.'),
  ('LS01502', 'HD260909-015', N'Confirmed', '2026-07-23T21:10:00', N'Chuyển trạng thái Confirmed.'),
  ('LS01503', 'HD260909-015', N'Preparing', '2026-07-23T21:18:00', N'Chuyển trạng thái Preparing.'),
  ('LS01504', 'HD260909-015', N'Delivering', '2026-07-23T21:26:00', N'Chuyển trạng thái Delivering.'),
  ('LS01505', 'HD260909-015', N'Completed', '2026-07-23T21:59:00', N'Chuyển trạng thái Completed.'),
  ('LS02901', 'HD260919-029', N'Pending', '2026-02-21T19:04:00', N'Chuyển trạng thái Pending.'),
  ('LS02902', 'HD260919-029', N'Cancelled', '2026-02-21T19:12:00', N'Khách hủy đơn.'),
  ('LS05501', 'HD261006-055', N'Pending', '2026-08-23T17:42:00', N'Chuyển trạng thái Pending.'),
  ('LS05502', 'HD261006-055', N'Cancelled', '2026-08-23T17:50:00', N'Khách hủy đơn.'),
  ('LS07001', 'HD261015-070', N'Pending', '2026-06-08T14:57:00', N'Chuyển trạng thái Pending.'),
  ('LS07002', 'HD261015-070', N'Cancelled', '2026-06-08T15:05:00', N'Khách hủy đơn.'),
  ('LS09201', 'HD261026-092', N'Pending', '2026-11-22T16:43:00', N'Chuyển trạng thái Pending.'),
  ('LS09202', 'HD261026-092', N'Confirmed', '2026-11-22T16:51:00', N'Chuyển trạng thái Confirmed.'),
  ('LS09203', 'HD261026-092', N'Preparing', '2026-11-22T16:59:00', N'Chuyển trạng thái Preparing.');
GO
-- TRA_LOI_DANH_GIA: 4 dòng
INSERT INTO TRA_LOI_DANH_GIA (maTraLoi, maDanhGia, maNhanVien, noiDung, ngayGui) VALUES
  ('TL001', 'DG001', 'NV06', N'Cảm ơn bạn đã tin chọn Lẩu Phố! Hẹn gặp lại bạn lần sau.', '2026-02-15T01:59:00'),
  ('TL002', 'DG002', 'NV06', N'Cảm ơn bạn đã tin chọn Lẩu Phố! Hẹn gặp lại bạn lần sau.', '2026-11-14T23:19:00'),
  ('TL003', 'DG003', 'NV05', N'Cảm ơn bạn đã tin chọn Lẩu Phố! Hẹn gặp lại bạn lần sau.', '2026-06-10T00:45:00'),
  ('TL004', 'DG004', 'NV05', N'Cảm ơn bạn đã tin chọn Lẩu Phố! Hẹn gặp lại bạn lần sau.', '2026-07-25T13:59:00');
GO
-- KHAO_SAT: 2 dòng
INSERT INTO KHAO_SAT (maKhaoSat, maNhanVien, tieuDe, trangThai, ngayTao) VALUES
  ('KS01', 'NV05', N'Khảo sát mức độ hài lòng về dịch vụ', N'Open', '2026-08-28T09:00:00'),
  ('KS02', 'NV05', N'Khảo sát sở thích nước lẩu', N'Open', '2026-09-03T09:00:00');
GO
-- CAU_HOI_KHAO_SAT: 6 dòng
INSERT INTO CAU_HOI_KHAO_SAT (maCauHoi, maKhaoSat, noiDungCauHoi, loaiCauHoi) VALUES
  ('CH01', 'KS01', N'Bạn đánh giá trải nghiệm tổng thể thế nào?', 'Option'),
  ('CH02', 'KS01', N'Bạn hài lòng nhất về điều gì?', 'Option'),
  ('CH03', 'KS01', N'Bạn có góp ý gì thêm?', 'Text'),
  ('CH04', 'KS02', N'Bạn thích loại nước lẩu nào nhất?', 'Option'),
  ('CH05', 'KS02', N'Độ cay bạn mong muốn?', 'Option'),
  ('CH06', 'KS02', N'Bạn muốn quán thêm nước lẩu nào?', 'Text');
GO
-- TUY_CHON_CAU_HOI: 16 dòng
INSERT INTO TUY_CHON_CAU_HOI (maTuyChon, maCauHoi, noiDungTuyChon) VALUES
  ('TC0101', 'CH01', N'Rất hài lòng'),
  ('TC0102', 'CH01', N'Hài lòng'),
  ('TC0103', 'CH01', N'Bình thường'),
  ('TC0104', 'CH01', N'Không hài lòng'),
  ('TC0201', 'CH02', N'Chất lượng món ăn'),
  ('TC0202', 'CH02', N'Tốc độ phục vụ'),
  ('TC0203', 'CH02', N'Giá cả'),
  ('TC0204', 'CH02', N'Không gian'),
  ('TC0401', 'CH04', N'Lẩu Thái'),
  ('TC0402', 'CH04', N'Lẩu Tứ Xuyên'),
  ('TC0403', 'CH04', N'Lẩu nấm'),
  ('TC0404', 'CH04', N'Lẩu kim chi'),
  ('TC0501', 'CH05', N'Không cay'),
  ('TC0502', 'CH05', N'Cay nhẹ'),
  ('TC0503', 'CH05', N'Cay vừa'),
  ('TC0504', 'CH05', N'Rất cay');
GO
-- DOI_TUONG_KHAO_SAT: 8 dòng
INSERT INTO DOI_TUONG_KHAO_SAT (maDoiTuong, maKhaoSat, maKhachHang, daHoanThanh) VALUES
  ('DT02', 'KS01', 'KH14', 1),
  ('DT03', 'KS01', 'KH18', 1),
  ('DT04', 'KS01', 'KH13', 1),
  ('DT05', 'KS01', 'KH16', 0),
  ('DT08', 'KS02', 'KH10', 1),
  ('DT09', 'KS02', 'KH12', 0),
  ('DT12', 'KS02', 'KH04', 1),
  ('DT14', 'KS02', 'KH06', 1);
GO
-- PHIEU_TRA_LOI: 6 dòng
INSERT INTO PHIEU_TRA_LOI (maPhieuTraLoi, maKhaoSat, maKhachHang, ngayTraLoi) VALUES
  ('PT01', 'KS01', 'KH14', '2026-10-05T14:25:35'),
  ('PT02', 'KS01', 'KH18', '2026-09-21T09:34:00'),
  ('PT03', 'KS01', 'KH13', '2026-09-10T21:07:52'),
  ('PT05', 'KS02', 'KH10', '2026-10-12T18:34:19'),
  ('PT06', 'KS02', 'KH04', '2026-09-10T02:30:13'),
  ('PT07', 'KS02', 'KH06', '2026-10-16T08:32:10');
GO
-- CAU_TRA_LOI: 18 dòng
INSERT INTO CAU_TRA_LOI (maCauTraLoi, maPhieuTraLoi, maCauHoi, maTuyChon, noiDungTuDien) VALUES
  ('CTL001', 'PT01', 'CH01', 'TC0102', NULL),
  ('CTL002', 'PT01', 'CH02', 'TC0202', NULL),
  ('CTL003', 'PT01', 'CH03', NULL, N'Mở thêm chi nhánh ở quận khác.'),
  ('CTL004', 'PT02', 'CH01', 'TC0102', NULL),
  ('CTL005', 'PT02', 'CH02', 'TC0201', NULL),
  ('CTL006', 'PT02', 'CH03', NULL, N'Nên có thêm combo nhỏ cho 1 người.'),
  ('CTL007', 'PT03', 'CH01', 'TC0102', NULL),
  ('CTL008', 'PT03', 'CH02', 'TC0201', NULL),
  ('CTL009', 'PT03', 'CH03', NULL, N'Nên có thêm combo nhỏ cho 1 người.'),
  ('CTL013', 'PT05', 'CH04', 'TC0404', NULL),
  ('CTL014', 'PT05', 'CH05', 'TC0501', NULL),
  ('CTL015', 'PT05', 'CH06', NULL, N'Giao hàng nhanh hơn vào giờ cao điểm.'),
  ('CTL016', 'PT06', 'CH04', 'TC0404', NULL),
  ('CTL017', 'PT06', 'CH05', 'TC0504', NULL),
  ('CTL018', 'PT06', 'CH06', NULL, N'Nên có thêm combo nhỏ cho 1 người.'),
  ('CTL019', 'PT07', 'CH04', 'TC0401', NULL),
  ('CTL020', 'PT07', 'CH05', 'TC0501', NULL),
  ('CTL021', 'PT07', 'CH06', NULL, N'Có chỗ đậu xe rộng hơn.');
GO
-- KHACH_HANG: 3 dòng
INSERT INTO KHACH_HANG (maKhachHang, hoTen, soDienThoai, email, matKhau, ngaySinh, gioiTinh, diaChi, trangThai) VALUES
  (N'KHX013', N'Dương Quốc Bảo', N'0930102947', N'khachhang013@example.com', N'e10adc3949ba59abbe56e057f20f883e', N'1993-02-14', N'Nữ', N'23 Phan Văn Trị, Quận Gò Vấp, TP. Hồ Chí Minh', N'Active'),
  (N'KHX033', N'Bùi Quốc Bảo', N'0930261327', N'khachhang033@example.com', N'e10adc3949ba59abbe56e057f20f883e', N'1990-10-07', N'Nữ', N'43 Phan Văn Trị, Quận Gò Vấp, TP. Hồ Chí Minh', N'Active'),
  (N'KHX069', N'Phạm Yến Nhi', N'0930546411', N'khachhang069@example.com', N'e10adc3949ba59abbe56e057f20f883e', N'1980-10-16', N'Nữ', N'79 Phạm Văn Đồng, Quận Tân Phú, TP. Hồ Chí Minh', N'Active');
GO
-- MON_AN: 5 dòng
INSERT INTO MON_AN (maMon, maLoaiMon, tenMon, moTa, duongDanAnh, donGia, trangThai) VALUES
  (N'MA048', N'LM003', N'Bắp Mỹ', N'Bắp Mỹ ngọt, cắt khúc nhúng lẩu.', N'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?auto=format&fit=crop&w=800&q=80', 29000, N'InStock'),
  (N'MA049', N'LM003', N'Miến khoai lang', N'Miến dai, dễ ăn cùng nước lẩu.', N'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=800&q=80', 22000, N'InStock'),
  (N'MA055', N'LM003', N'Đậu hũ phô mai chiên', N'Đậu hũ phô mai chiên vàng giòn.', N'https://images.unsplash.com/photo-1559847844-5315695dadae?auto=format&fit=crop&w=800&q=80', 59000, N'InStock'),
  (N'MA057', N'LM002', N'Há cảo tôm', N'Há cảo nhân tôm dùng nhúng lẩu.', N'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?auto=format&fit=crop&w=800&q=80', 69000, N'InStock'),
  (N'MA059', N'LM005', N'Combo lẩu nấm 3 người', N'Lẩu nấm, bò, nấm tổng hợp, rau và mì.', N'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=800&q=80', 429000, N'InStock');
GO
-- HOA_DON: 3 dòng
INSERT INTO HOA_DON (maHoaDon, maKhachHang, maNhanVien, maKMDonHang, tenKhachHang, soDienThoai, diaChiGiao, ngayDat, ngayHoanTat, trangThai, tongTienHang, tienGiamVoucher, tongThanhToan) VALUES
  (N'HDX0019', N'KHX013', 'NV04', NULL, N'Dương Quốc Bảo', N'0930102947', N'23 Phan Văn Trị, Quận Gò Vấp, TP. Hồ Chí Minh', N'2026-03-17T15:08:00', NULL, N'Pending', 1112000, 0, 1112000),
  (N'HDX0069', N'KHX033', 'NV04', NULL, N'Bùi Quốc Bảo', N'0930261327', N'43 Phan Văn Trị, Quận Gò Vấp, TP. Hồ Chí Minh', N'2026-09-07T13:58:00', NULL, N'Confirmed', 274000, 0, 274000),
  (N'HDX0096', N'KHX069', 'NV03', NULL, N'Phạm Yến Nhi', N'0930546411', N'79 Phạm Văn Đồng, Quận Tân Phú, TP. Hồ Chí Minh', N'2026-04-16T22:49:00', NULL, N'Preparing', 454000, 0, 454000);
GO
-- CHI_TIET_HOA_DON: 10 dòng
INSERT INTO CHI_TIET_HOA_DON (maChiTiet, maHoaDon, maMon, maKMSP, maKMLoai, tenMon, soLuong, donGiaGoc, tienGiamMon, donGiaSauGiam, thanhTien) VALUES
  (N'CTHX001901', N'HDX0019', N'MA059', NULL, NULL, N'Combo lẩu nấm 3 người', 2, 429000, 0, 429000, 858000),
  (N'CTHX001902', N'HDX0019', N'MA016', NULL, NULL, N'Cơm chiên trứng', 3, 55000, 0, 55000, 165000),
  (N'CTHX001903', N'HDX0019', N'MA025', NULL, NULL, N'Khoai tây chiên', 1, 45000, 0, 45000, 45000),
  (N'CTHX001904', N'HDX0019', N'MA049', NULL, NULL, N'Miến khoai lang', 2, 22000, 0, 22000, 44000),
  (N'CTHX006901', N'HDX0069', N'MA057', NULL, NULL, N'Há cảo tôm', 1, 69000, 0, 69000, 69000),
  (N'CTHX006902', N'HDX0069', N'MA055', NULL, NULL, N'Đậu hũ phô mai chiên', 2, 59000, 0, 59000, 118000),
  (N'CTHX006903', N'HDX0069', N'MA018', NULL, NULL, N'Bia lon', 3, 29000, 0, 29000, 87000),
  (N'CTHX009601', N'HDX0096', N'MA009', NULL, NULL, N'Sò điệp Nhật', 1, 189000, 0, 189000, 189000),
  (N'CTHX009602', N'HDX0096', N'MA048', NULL, NULL, N'Bắp Mỹ', 2, 29000, 0, 29000, 58000),
  (N'CTHX009603', N'HDX0096', N'MA024', NULL, NULL, N'Chả giò hải sản', 3, 69000, 0, 69000, 207000);
GO
-- THANH_TOAN: 3 dòng
INSERT INTO THANH_TOAN (maThanhToan, maHoaDon, phuongThuc, trangThai, ngayThanhToan, soTien) VALUES
  (N'TTX0019', N'HDX0019', N'Tiền mặt', N'Pending', NULL, 0),
  (N'TTX0069', N'HDX0069', N'Thẻ ngân hàng', N'Pending', NULL, 0),
  (N'TTX0096', N'HDX0096', N'Chuyển khoản', N'Pending', NULL, 0);
GO
-- LICH_SU_TRANG_THAI: 6 dòng
INSERT INTO LICH_SU_TRANG_THAI (maLichSu, maHoaDon, trangThai, thoiGian, ghiChu) VALUES
  (N'LSX001901', N'HDX0019', N'Pending', N'2026-03-17T15:08:00', N'Cập nhật trạng thái đơn Pending (dữ liệu mẫu).'),
  (N'LSX006901', N'HDX0069', N'Pending', N'2026-09-07T13:58:00', N'Cập nhật trạng thái đơn Pending (dữ liệu mẫu).'),
  (N'LSX006902', N'HDX0069', N'Confirmed', N'2026-09-07T14:06:00', N'Cập nhật trạng thái đơn Confirmed (dữ liệu mẫu).'),
  (N'LSX009601', N'HDX0096', N'Pending', N'2026-04-16T22:49:00', N'Cập nhật trạng thái đơn Pending (dữ liệu mẫu).'),
  (N'LSX009602', N'HDX0096', N'Confirmed', N'2026-04-16T22:57:00', N'Cập nhật trạng thái đơn Confirmed (dữ liệu mẫu).'),
  (N'LSX009603', N'HDX0096', N'Preparing', N'2026-04-16T23:05:00', N'Cập nhật trạng thái đơn Preparing (dữ liệu mẫu).');
GO
-- DOI_TUONG_KHAO_SAT: 1 dòng
INSERT INTO DOI_TUONG_KHAO_SAT (maDoiTuong, maKhaoSat, maKhachHang, daHoanThanh) VALUES
  (N'DKX001', N'KS01', N'KH18', 1);
GO

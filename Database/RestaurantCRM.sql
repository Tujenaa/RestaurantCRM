-- ============================================
-- CSDL: Hệ thống quản lý đặt món & CRM khách hàng
-- SQL Server
-- ============================================

CREATE DATABASE RestaurantCRM;
GO

USE RestaurantCRM;
GO

-- 1. Tài khoản & phân quyền
CREATE TABLE TAI_KHOAN_ADMIN (
    maAdmin VARCHAR(20) PRIMARY KEY,
    tenDangNhap NVARCHAR(50),
    matKhau NVARCHAR(100),
    hoTen NVARCHAR(100)
);

CREATE TABLE VAI_TRO (
    maVaiTro VARCHAR(20) PRIMARY KEY,
    tenVaiTro NVARCHAR(100)
);

CREATE TABLE NHAN_VIEN (
    maNhanVien VARCHAR(20) PRIMARY KEY,
    maVaiTro VARCHAR(20) FOREIGN KEY REFERENCES VAI_TRO(maVaiTro),
    tenDangNhap NVARCHAR(50),
    matKhau NVARCHAR(100),
    hoTen NVARCHAR(100),
    trangThai NVARCHAR(20)
);

CREATE TABLE KHACH_HANG (
    maKhachHang VARCHAR(20) PRIMARY KEY,
    hoTen NVARCHAR(100),
    soDienThoai VARCHAR(15),
    email NVARCHAR(100),
    matKhau NVARCHAR(100),
    ngaySinh DATE,
    gioiTinh NVARCHAR(10),
    soThich NVARCHAR(255),
    trangThai NVARCHAR(20)
);

-- 2. Thực đơn
CREATE TABLE LOAI_MON (
    maLoaiMon VARCHAR(20) PRIMARY KEY,
    tenLoaiMon NVARCHAR(100)
);

CREATE TABLE MON_AN (
    maMon VARCHAR(20) PRIMARY KEY,
    maLoaiMon VARCHAR(20) FOREIGN KEY REFERENCES LOAI_MON(maLoaiMon),
    tenMon NVARCHAR(150),
    moTa NVARCHAR(MAX),
    donGia FLOAT,
    soLuong INT,
    trangThai NVARCHAR(20)
);

CREATE TABLE HINH_ANH_MON_AN (
    maHinhAnh VARCHAR(20) PRIMARY KEY,
    maMon VARCHAR(20) FOREIGN KEY REFERENCES MON_AN(maMon),
    duongDanAnh NVARCHAR(255),
    laAnhDaiDien BIT,
    thuTuHienThi INT
);

-- 3. Nhà cung cấp & nhập hàng
CREATE TABLE NHA_CUNG_CAP (
    maNhaCungCap VARCHAR(20) PRIMARY KEY,
    tenNhaCungCap NVARCHAR(150),
    soDienThoai VARCHAR(15),
    diaChi NVARCHAR(255),
    email NVARCHAR(100)
);

CREATE TABLE PHIEU_NHAP_HANG (
    maPhieuNhap VARCHAR(20) PRIMARY KEY,
    maNhaCungCap VARCHAR(20) FOREIGN KEY REFERENCES NHA_CUNG_CAP(maNhaCungCap),
    maNhanVien VARCHAR(20) FOREIGN KEY REFERENCES NHAN_VIEN(maNhanVien),
    ngayNhap DATETIME,
    tongTien FLOAT,
    ghiChu NVARCHAR(255)
);

CREATE TABLE CHI_TIET_PHIEU_NHAP (
    maChiTiet VARCHAR(20) PRIMARY KEY,
    maPhieuNhap VARCHAR(20) FOREIGN KEY REFERENCES PHIEU_NHAP_HANG(maPhieuNhap),
    maMon VARCHAR(20) FOREIGN KEY REFERENCES MON_AN(maMon),
    soLuong INT,
    donGiaNhap FLOAT,
    thanhTien FLOAT
);

-- 4. Khuyến mãi
CREATE TABLE CHUONG_TRINH_KHUYEN_MAI (
    maChuongTrinh VARCHAR(20) PRIMARY KEY,
    tenChuongTrinh NVARCHAR(150),
    loaiKhuyenMai VARCHAR(20),
    loaiGiam VARCHAR(20),
    giaTriGiam FLOAT,
    giaTriDonToiThieu FLOAT,
    ngayBatDau DATE,
    ngayKetThuc DATE,
    trangThai NVARCHAR(20)
);

CREATE TABLE CHI_TIET_KHUYEN_MAI_MON (
    maChiTietKM VARCHAR(20) PRIMARY KEY,
    maChuongTrinh VARCHAR(20) FOREIGN KEY REFERENCES CHUONG_TRINH_KHUYEN_MAI(maChuongTrinh),
    maMon VARCHAR(20) FOREIGN KEY REFERENCES MON_AN(maMon),
    soLuongApDung INT
);

-- 5. Đơn hàng
CREATE TABLE DON_HANG (
    maDonHang VARCHAR(20) PRIMARY KEY,
    maKhachHang VARCHAR(20) FOREIGN KEY REFERENCES KHACH_HANG(maKhachHang),
    maNhanVien VARCHAR(20) FOREIGN KEY REFERENCES NHAN_VIEN(maNhanVien),
    maChuongTrinhVoucher VARCHAR(20) FOREIGN KEY REFERENCES CHUONG_TRINH_KHUYEN_MAI(maChuongTrinh),
    diaChiGiao NVARCHAR(255),
    ngayDat DATETIME,
    trangThai NVARCHAR(30),
    tongTienHang FLOAT,
    tienGiamVoucher FLOAT,
    tongThanhToan FLOAT
);

CREATE TABLE CHI_TIET_DON_HANG (
    maChiTiet VARCHAR(20) PRIMARY KEY,
    maDonHang VARCHAR(20) FOREIGN KEY REFERENCES DON_HANG(maDonHang),
    maMon VARCHAR(20) FOREIGN KEY REFERENCES MON_AN(maMon),
    maChuongTrinhKM VARCHAR(20) FOREIGN KEY REFERENCES CHUONG_TRINH_KHUYEN_MAI(maChuongTrinh),
    soLuong INT,
    donGiaGoc FLOAT,
    tienGiamMon FLOAT,
    donGiaSauGiam FLOAT,
    thanhTien FLOAT
);

CREATE TABLE THANH_TOAN (
    maThanhToan VARCHAR(20) PRIMARY KEY,
    maDonHang VARCHAR(20) FOREIGN KEY REFERENCES DON_HANG(maDonHang),
    phuongThuc NVARCHAR(30),
    trangThai NVARCHAR(20),
    ngayThanhToan DATETIME,
    soTien FLOAT
);

CREATE TABLE DANH_GIA (
    maDanhGia VARCHAR(20) PRIMARY KEY,
    maKhachHang VARCHAR(20) FOREIGN KEY REFERENCES KHACH_HANG(maKhachHang),
    maDonHang VARCHAR(20) FOREIGN KEY REFERENCES DON_HANG(maDonHang),
    maMon VARCHAR(20) FOREIGN KEY REFERENCES MON_AN(maMon),
    soSao INT,
    noiDung NVARCHAR(MAX),
    ngayDanhGia DATETIME
);

CREATE TABLE LICH_SU_TRANG_THAI (
    maLichSu VARCHAR(20) PRIMARY KEY,
    maDonHang VARCHAR(20) FOREIGN KEY REFERENCES DON_HANG(maDonHang),
    trangThai NVARCHAR(30),
    thoiGian DATETIME,
    ghiChu NVARCHAR(255)
);

-- 6. Tương tác & khảo sát (CRM)
CREATE TABLE PHAN_HOI (
    maPhanHoi VARCHAR(20) PRIMARY KEY,
    maKhachHang VARCHAR(20) FOREIGN KEY REFERENCES KHACH_HANG(maKhachHang),
    maNhanVien VARCHAR(20) FOREIGN KEY REFERENCES NHAN_VIEN(maNhanVien),
    noiDung NVARCHAR(MAX),
    danhGia INT,
    ngayPhanHoi DATETIME,
    trangThai NVARCHAR(20)
);

CREATE TABLE KHAO_SAT (
    maKhaoSat VARCHAR(20) PRIMARY KEY,
    maNhanVien VARCHAR(20) FOREIGN KEY REFERENCES NHAN_VIEN(maNhanVien),
    tieuDe NVARCHAR(200),
    trangThai NVARCHAR(20),
    ngayTao DATETIME
);

CREATE TABLE CAU_HOI_KHAO_SAT (
    maCauHoi VARCHAR(20) PRIMARY KEY,
    maKhaoSat VARCHAR(20) FOREIGN KEY REFERENCES KHAO_SAT(maKhaoSat),
    noiDungCauHoi NVARCHAR(500),
    loaiCauHoi VARCHAR(20)
);

CREATE TABLE TUY_CHON_CAU_HOI (
    maTuyChon VARCHAR(20) PRIMARY KEY,
    maCauHoi VARCHAR(20) FOREIGN KEY REFERENCES CAU_HOI_KHAO_SAT(maCauHoi),
    noiDungTuyChon NVARCHAR(255)
);

CREATE TABLE DOI_TUONG_KHAO_SAT (
    maDoiTuong VARCHAR(20) PRIMARY KEY,
    maKhaoSat VARCHAR(20) FOREIGN KEY REFERENCES KHAO_SAT(maKhaoSat),
    maKhachHang VARCHAR(20) FOREIGN KEY REFERENCES KHACH_HANG(maKhachHang),
    daHoanThanh BIT
);

CREATE TABLE PHIEU_TRA_LOI (
    maPhieuTraLoi VARCHAR(20) PRIMARY KEY,
    maKhaoSat VARCHAR(20) FOREIGN KEY REFERENCES KHAO_SAT(maKhaoSat),
    maKhachHang VARCHAR(20) FOREIGN KEY REFERENCES KHACH_HANG(maKhachHang),
    ngayTraLoi DATETIME
);

CREATE TABLE CAU_TRA_LOI (
    maCauTraLoi VARCHAR(20) PRIMARY KEY,
    maPhieuTraLoi VARCHAR(20) FOREIGN KEY REFERENCES PHIEU_TRA_LOI(maPhieuTraLoi),
    maCauHoi VARCHAR(20) FOREIGN KEY REFERENCES CAU_HOI_KHAO_SAT(maCauHoi),
    maTuyChon VARCHAR(20) FOREIGN KEY REFERENCES TUY_CHON_CAU_HOI(maTuyChon),
    noiDungTuDien NVARCHAR(MAX)
);
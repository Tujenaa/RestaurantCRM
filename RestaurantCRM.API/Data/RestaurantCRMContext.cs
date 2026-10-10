using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Models;

namespace RestaurantCRM.API.Data;

public partial class RestaurantCrmContext : DbContext
{
    public RestaurantCrmContext()
    {
    }

    public RestaurantCrmContext(DbContextOptions<RestaurantCrmContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CauHoiKhaoSat> CauHoiKhaoSat { get; set; }

    public virtual DbSet<CauTraLoi> CauTraLoi { get; set; }

    public virtual DbSet<ChiTietHoaDon> ChiTietHoaDon { get; set; }

    public virtual DbSet<ChiTietPhieuNhap> ChiTietPhieuNhap { get; set; }

    public virtual DbSet<ChuongTrinhKhuyenMai> ChuongTrinhKhuyenMai { get; set; }

    public virtual DbSet<DanhGia> DanhGia { get; set; }

    public virtual DbSet<DoiTuongKhaoSat> DoiTuongKhaoSat { get; set; }

    public virtual DbSet<HoaDon> HoaDon { get; set; }

    public virtual DbSet<KhachHang> KhachHang { get; set; }

    public virtual DbSet<KhaoSat> KhaoSat { get; set; }

    public virtual DbSet<KmTheoSp> KmTheoSp { get; set; }

    public virtual DbSet<KmTheoVoucher> KmTheoVoucher { get; set; }

    public virtual DbSet<LichSuTrangThai> LichSuTrangThai { get; set; }

    public virtual DbSet<LoaiMon> LoaiMon { get; set; }

    public virtual DbSet<MonAn> MonAn { get; set; }

    public virtual DbSet<NhaCungCap> NhaCungCap { get; set; }

    public virtual DbSet<NhanVien> NhanVien { get; set; }

    public virtual DbSet<PhanHoi> PhanHoi { get; set; }

    public virtual DbSet<PhieuNhapHang> PhieuNhapHang { get; set; }

    public virtual DbSet<PhieuTraLoi> PhieuTraLoi { get; set; }

    public virtual DbSet<ThanhToan> ThanhToan { get; set; }

    public virtual DbSet<TraLoiDanhGia> TraLoiDanhGia { get; set; }

    public virtual DbSet<TraLoiPhanHoi> TraLoiPhanHoi { get; set; }

    public virtual DbSet<TuyChonCauHoi> TuyChonCauHoi { get; set; }

    public virtual DbSet<VaiTro> VaiTro { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CauHoiKhaoSat>(entity =>
        {
            entity.HasKey(e => e.MaCauHoi).HasName("PK__CAU_HOI___82BDFBDC5D533584");

            entity.ToTable("CAU_HOI_KHAO_SAT");

            entity.Property(e => e.MaCauHoi)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maCauHoi");
            entity.Property(e => e.LoaiCauHoi)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("loaiCauHoi");
            entity.Property(e => e.MaKhaoSat)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maKhaoSat");
            entity.Property(e => e.NoiDungCauHoi)
                .HasMaxLength(500)
                .HasColumnName("noiDungCauHoi");

            entity.HasOne(d => d.MaKhaoSatNavigation).WithMany(p => p.CauHoiKhaoSat)
                .HasForeignKey(d => d.MaKhaoSat)
                .HasConstraintName("FK__CAU_HOI_K__maKha__7B5B524B");
        });

        modelBuilder.Entity<CauTraLoi>(entity =>
        {
            entity.HasKey(e => e.MaCauTraLoi).HasName("PK__CAU_TRA___51956E3873953A29");

            entity.ToTable("CAU_TRA_LOI");

            entity.Property(e => e.MaCauTraLoi)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maCauTraLoi");
            entity.Property(e => e.MaCauHoi)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maCauHoi");
            entity.Property(e => e.MaPhieuTraLoi)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maPhieuTraLoi");
            entity.Property(e => e.MaTuyChon)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maTuyChon");
            entity.Property(e => e.NoiDungTuDien).HasColumnName("noiDungTuDien");

            entity.HasOne(d => d.MaCauHoiNavigation).WithMany(p => p.CauTraLoi)
                .HasForeignKey(d => d.MaCauHoi)
                .HasConstraintName("FK__CAU_TRA_L__maCau__0A9D95DB");

            entity.HasOne(d => d.MaPhieuTraLoiNavigation).WithMany(p => p.CauTraLoi)
                .HasForeignKey(d => d.MaPhieuTraLoi)
                .HasConstraintName("FK__CAU_TRA_L__maPhi__09A971A2");

            entity.HasOne(d => d.MaTuyChonNavigation).WithMany(p => p.CauTraLoi)
                .HasForeignKey(d => d.MaTuyChon)
                .HasConstraintName("FK__CAU_TRA_L__maTuy__0B91BA14");
        });

        modelBuilder.Entity<ChiTietHoaDon>(entity =>
        {
            entity.HasKey(e => e.MaChiTiet).HasName("PK__CHI_TIET__99964888A30BF10F");

            entity.ToTable("CHI_TIET_HOA_DON");

            entity.Property(e => e.MaChiTiet)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maChiTiet");
            entity.Property(e => e.DonGiaGoc).HasColumnName("donGiaGoc");
            entity.Property(e => e.DonGiaSauGiam).HasColumnName("donGiaSauGiam");
            entity.Property(e => e.MaHoaDon)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maHoaDon");
            entity.Property(e => e.MaKmsp)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maKMSP");
            entity.Property(e => e.MaMon)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maMon");
            entity.Property(e => e.SoLuong).HasColumnName("soLuong");
            entity.Property(e => e.TenMon)
                .HasMaxLength(150)
                .HasColumnName("tenMon");
            entity.Property(e => e.ThanhTien).HasColumnName("thanhTien");
            entity.Property(e => e.TienGiamMon).HasColumnName("tienGiamMon");

            entity.HasOne(d => d.MaHoaDonNavigation).WithMany(p => p.ChiTietHoaDon)
                .HasForeignKey(d => d.MaHoaDon)
                .HasConstraintName("FK__CHI_TIET___maHoa__619B8048");

            entity.HasOne(d => d.MaKmspNavigation).WithMany(p => p.ChiTietHoaDon)
                .HasForeignKey(d => d.MaKmsp)
                .HasConstraintName("FK__CHI_TIET___maKMS__6383C8BA");

            entity.HasOne(d => d.MaMonNavigation).WithMany(p => p.ChiTietHoaDon)
                .HasForeignKey(d => d.MaMon)
                .HasConstraintName("FK__CHI_TIET___maMon__628FA481");
        });

        modelBuilder.Entity<ChiTietPhieuNhap>(entity =>
        {
            entity.HasKey(e => e.MaChiTiet).HasName("PK__CHI_TIET__99964888CC649751");

            entity.ToTable("CHI_TIET_PHIEU_NHAP");

            entity.Property(e => e.MaChiTiet)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maChiTiet");
            entity.Property(e => e.DonGiaNhap).HasColumnName("donGiaNhap");
            entity.Property(e => e.MaMon)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maMon");
            entity.Property(e => e.MaPhieuNhap)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maPhieuNhap");
            entity.Property(e => e.SoLuong).HasColumnName("soLuong");
            entity.Property(e => e.ThanhTien).HasColumnName("thanhTien");

            entity.HasOne(d => d.MaMonNavigation).WithMany(p => p.ChiTietPhieuNhap)
                .HasForeignKey(d => d.MaMon)
                .HasConstraintName("FK__CHI_TIET___maMon__4CA06362");

            entity.HasOne(d => d.MaPhieuNhapNavigation).WithMany(p => p.ChiTietPhieuNhap)
                .HasForeignKey(d => d.MaPhieuNhap)
                .HasConstraintName("FK__CHI_TIET___maPhi__4BAC3F29");
        });

        modelBuilder.Entity<ChuongTrinhKhuyenMai>(entity =>
        {
            entity.HasKey(e => e.MaChuongTrinh).HasName("PK__CHUONG_T__4F35F615057E944F");

            entity.ToTable("CHUONG_TRINH_KHUYEN_MAI");

            entity.Property(e => e.MaChuongTrinh)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maChuongTrinh");
            entity.Property(e => e.NgayBatDau).HasColumnName("ngayBatDau");
            entity.Property(e => e.NgayKetThuc).HasColumnName("ngayKetThuc");
            entity.Property(e => e.TenChuongTrinh)
                .HasMaxLength(150)
                .HasColumnName("tenChuongTrinh");
        });

        modelBuilder.Entity<DanhGia>(entity =>
        {
            entity.HasKey(e => e.MaDanhGia).HasName("PK__DANH_GIA__6B15DD9AC8DC656C");

            entity.ToTable("DANH_GIA");

            entity.Property(e => e.MaDanhGia)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maDanhGia");
            entity.Property(e => e.MaHoaDon)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maHoaDon");
            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maKhachHang");
            entity.Property(e => e.MaMon)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maMon");
            entity.Property(e => e.NgayDanhGia)
                .HasColumnType("datetime")
                .HasColumnName("ngayDanhGia");
            entity.Property(e => e.NoiDung).HasColumnName("noiDung");
            entity.Property(e => e.SoSao).HasColumnName("soSao");

            entity.HasOne(d => d.MaHoaDonNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaHoaDon)
                .HasConstraintName("FK__DANH_GIA__maHoaD__6B24EA82");

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__DANH_GIA__maKhac__6A30C649");

            entity.HasOne(d => d.MaMonNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaMon)
                .HasConstraintName("FK__DANH_GIA__maMon__6C190EBB");
        });

        modelBuilder.Entity<DoiTuongKhaoSat>(entity =>
        {
            entity.HasKey(e => e.MaDoiTuong).HasName("PK__DOI_TUON__8B6358B47299E3F7");

            entity.ToTable("DOI_TUONG_KHAO_SAT");

            entity.Property(e => e.MaDoiTuong)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maDoiTuong");
            entity.Property(e => e.DaHoanThanh).HasColumnName("daHoanThanh");
            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maKhachHang");
            entity.Property(e => e.MaKhaoSat)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maKhaoSat");

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.DoiTuongKhaoSat)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__DOI_TUONG__maKha__02FC7413");

            entity.HasOne(d => d.MaKhaoSatNavigation).WithMany(p => p.DoiTuongKhaoSat)
                .HasForeignKey(d => d.MaKhaoSat)
                .HasConstraintName("FK__DOI_TUONG__maKha__02084FDA");
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.MaHoaDon).HasName("PK__HOA_DON__026B4D9A21D75BFF");

            entity.ToTable("HOA_DON");

            entity.Property(e => e.MaHoaDon)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maHoaDon");
            entity.Property(e => e.DiaChiGiao)
                .HasMaxLength(255)
                .HasColumnName("diaChiGiao");
            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maKhachHang");
            entity.Property(e => e.MaKmvoucher)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maKMVoucher");
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maNhanVien");
            entity.Property(e => e.NgayDat)
                .HasColumnType("datetime")
                .HasColumnName("ngayDat");
            entity.Property(e => e.NgayHoanTat)
                .HasColumnType("datetime")
                .HasColumnName("ngayHoanTat");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("soDienThoai");
            entity.Property(e => e.TenKhachHang)
                .HasMaxLength(100)
                .HasColumnName("tenKhachHang");
            entity.Property(e => e.TienGiamVoucher).HasColumnName("tienGiamVoucher");
            entity.Property(e => e.TongThanhToan).HasColumnName("tongThanhToan");
            entity.Property(e => e.TongTienHang).HasColumnName("tongTienHang");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasColumnName("trangThai");

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.HoaDon)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__HOA_DON__maKhach__5BE2A6F2");

            entity.HasOne(d => d.MaKmvoucherNavigation).WithMany(p => p.HoaDon)
                .HasForeignKey(d => d.MaKmvoucher)
                .HasConstraintName("FK__HOA_DON__maKMVou__5DCAEF64");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.HoaDon)
                .HasForeignKey(d => d.MaNhanVien)
                .HasConstraintName("FK__HOA_DON__maNhanV__5CD6CB2B");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKhachHang).HasName("PK__KHACH_HA__0CCB3D49AECB4455");

            entity.ToTable("KHACH_HANG");

            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maKhachHang");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.GioiTinh)
                .HasMaxLength(10)
                .HasColumnName("gioiTinh");
            entity.Property(e => e.HoTen)
                .HasMaxLength(100)
                .HasColumnName("hoTen");
            entity.Property(e => e.MatKhau)
                .HasMaxLength(100)
                .HasColumnName("matKhau");
            entity.Property(e => e.NgaySinh).HasColumnName("ngaySinh");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("soDienThoai");
            entity.Property(e => e.SoThich)
                .HasMaxLength(255)
                .HasColumnName("soThich");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasColumnName("trangThai");
        });

        modelBuilder.Entity<KhaoSat>(entity =>
        {
            entity.HasKey(e => e.MaKhaoSat).HasName("PK__KHAO_SAT__AFDCED6E921ABB66");

            entity.ToTable("KHAO_SAT");

            entity.Property(e => e.MaKhaoSat)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maKhaoSat");
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maNhanVien");
            entity.Property(e => e.NgayTao)
                .HasColumnType("datetime")
                .HasColumnName("ngayTao");
            entity.Property(e => e.TieuDe)
                .HasMaxLength(200)
                .HasColumnName("tieuDe");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasColumnName("trangThai");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.KhaoSat)
                .HasForeignKey(d => d.MaNhanVien)
                .HasConstraintName("FK__KHAO_SAT__maNhan__778AC167");
        });

        modelBuilder.Entity<KmTheoSp>(entity =>
        {
            entity.HasKey(e => e.MaKmsp).HasName("PK__KM_THEO___86D5D326F2348C92");

            entity.ToTable("KM_THEO_SP");

            entity.Property(e => e.MaKmsp)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maKMSP");
            entity.Property(e => e.MaChuongTrinh)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maChuongTrinh");
            entity.Property(e => e.MaMon)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maMon");
            entity.Property(e => e.PhanTramGiam).HasColumnName("phanTramGiam");
            entity.Property(e => e.TienGiam).HasColumnName("tienGiam");

            entity.HasOne(d => d.MaChuongTrinhNavigation).WithMany(p => p.KmTheoSp)
                .HasForeignKey(d => d.MaChuongTrinh)
                .HasConstraintName("FK__KM_THEO_S__maChu__52593CB8");

            entity.HasOne(d => d.MaMonNavigation).WithMany(p => p.KmTheoSp)
                .HasForeignKey(d => d.MaMon)
                .HasConstraintName("FK__KM_THEO_S__maMon__534D60F1");
        });

        modelBuilder.Entity<KmTheoVoucher>(entity =>
        {
            entity.HasKey(e => e.MaKmvoucher).HasName("PK__KM_THEO___C33F4D9EB5637068");

            entity.ToTable("KM_THEO_VOUCHER");

            entity.HasIndex(e => e.MaVoucher, "UQ_KM_THEO_VOUCHER_maVoucher").IsUnique();

            entity.Property(e => e.MaKmvoucher)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maKMVoucher");
            entity.Property(e => e.GiaTriDonToiThieu).HasColumnName("giaTriDonToiThieu");
            entity.Property(e => e.MaChuongTrinh)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maChuongTrinh");
            entity.Property(e => e.MaVoucher)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("maVoucher");
            entity.Property(e => e.PhanTramGiam).HasColumnName("phanTramGiam");
            entity.Property(e => e.TienGiam).HasColumnName("tienGiam");

            entity.HasOne(d => d.MaChuongTrinhNavigation).WithMany(p => p.KmTheoVoucher)
                .HasForeignKey(d => d.MaChuongTrinh)
                .HasConstraintName("FK__KM_THEO_V__maChu__5812160E");
        });

        modelBuilder.Entity<LichSuTrangThai>(entity =>
        {
            entity.HasKey(e => e.MaLichSu).HasName("PK__LICH_SU___1F9C95FD2D2B6CD9");

            entity.ToTable("LICH_SU_TRANG_THAI");

            entity.Property(e => e.MaLichSu)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maLichSu");
            entity.Property(e => e.GhiChu)
                .HasMaxLength(255)
                .HasColumnName("ghiChu");
            entity.Property(e => e.MaHoaDon)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maHoaDon");
            entity.Property(e => e.ThoiGian)
                .HasColumnType("datetime")
                .HasColumnName("thoiGian");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasColumnName("trangThai");

            entity.HasOne(d => d.MaHoaDonNavigation).WithMany(p => p.LichSuTrangThai)
                .HasForeignKey(d => d.MaHoaDon)
                .HasConstraintName("FK__LICH_SU_T__maHoa__6EF57B66");
        });

        modelBuilder.Entity<LoaiMon>(entity =>
        {
            entity.HasKey(e => e.MaLoaiMon).HasName("PK__LOAI_MON__3A9F145500A3F8E4");

            entity.ToTable("LOAI_MON");

            entity.Property(e => e.MaLoaiMon)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maLoaiMon");
            entity.Property(e => e.TenLoaiMon)
                .HasMaxLength(100)
                .HasColumnName("tenLoaiMon");
        });

        modelBuilder.Entity<MonAn>(entity =>
        {
            entity.HasKey(e => e.MaMon).HasName("PK__MON_AN__27547BFA7696FAA3");

            entity.ToTable("MON_AN");

            entity.Property(e => e.MaMon)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maMon");
            entity.Property(e => e.DonGia).HasColumnName("donGia");
            entity.Property(e => e.DuongDanAnh)
                .HasMaxLength(255)
                .HasColumnName("duongDanAnh");
            entity.Property(e => e.MaLoaiMon)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maLoaiMon");
            entity.Property(e => e.MoTa).HasColumnName("moTa");
            entity.Property(e => e.SoLuong).HasColumnName("soLuong");
            entity.Property(e => e.TenMon)
                .HasMaxLength(150)
                .HasColumnName("tenMon");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasColumnName("trangThai");

            entity.HasOne(d => d.MaLoaiMonNavigation).WithMany(p => p.MonAn)
                .HasForeignKey(d => d.MaLoaiMon)
                .HasConstraintName("FK__MON_AN__maLoaiMo__4222D4EF");
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.MaNhaCungCap).HasName("PK__NHA_CUNG__D0B4D6DE70C4FE4B");

            entity.ToTable("NHA_CUNG_CAP");

            entity.Property(e => e.MaNhaCungCap)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maNhaCungCap");
            entity.Property(e => e.DiaChi)
                .HasMaxLength(255)
                .HasColumnName("diaChi");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("soDienThoai");
            entity.Property(e => e.TenNhaCungCap)
                .HasMaxLength(150)
                .HasColumnName("tenNhaCungCap");
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.MaNhanVien).HasName("PK__NHAN_VIE__BDDEF20DB00CF3BE");

            entity.ToTable("NHAN_VIEN");

            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maNhanVien");
            entity.Property(e => e.HoTen)
                .HasMaxLength(100)
                .HasColumnName("hoTen");
            entity.Property(e => e.MaVaiTro)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maVaiTro");
            entity.Property(e => e.MatKhau)
                .HasMaxLength(100)
                .HasColumnName("matKhau");
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .HasColumnName("tenDangNhap");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasColumnName("trangThai");

            entity.HasOne(d => d.MaVaiTroNavigation).WithMany(p => p.NhanVien)
                .HasForeignKey(d => d.MaVaiTro)
                .HasConstraintName("FK__NHAN_VIEN__maVai__398D8EEE");
        });

        modelBuilder.Entity<PhanHoi>(entity =>
        {
            entity.HasKey(e => e.MaPhanHoi).HasName("PK__PHAN_HOI__4E079CEC6B52822A");

            entity.ToTable("PHAN_HOI");

            entity.Property(e => e.MaPhanHoi)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maPhanHoi");
            entity.Property(e => e.DanhGia).HasColumnName("danhGia");
            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maKhachHang");
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maNhanVien");
            entity.Property(e => e.NgayPhanHoi)
                .HasColumnType("datetime")
                .HasColumnName("ngayPhanHoi");
            entity.Property(e => e.NoiDung).HasColumnName("noiDung");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasColumnName("trangThai");

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.PhanHoi)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__PHAN_HOI__maKhac__72C60C4A");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.PhanHoi)
                .HasForeignKey(d => d.MaNhanVien)
                .HasConstraintName("FK__PHAN_HOI__maNhan__73BA3083");
        });

        modelBuilder.Entity<PhieuNhapHang>(entity =>
        {
            entity.HasKey(e => e.MaPhieuNhap).HasName("PK__PHIEU_NH__E276393475E92193");

            entity.ToTable("PHIEU_NHAP_HANG");

            entity.Property(e => e.MaPhieuNhap)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maPhieuNhap");
            entity.Property(e => e.GhiChu)
                .HasMaxLength(255)
                .HasColumnName("ghiChu");
            entity.Property(e => e.MaNhaCungCap)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maNhaCungCap");
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maNhanVien");
            entity.Property(e => e.NgayNhap)
                .HasColumnType("datetime")
                .HasColumnName("ngayNhap");
            entity.Property(e => e.TongTien).HasColumnName("tongTien");

            entity.HasOne(d => d.MaNhaCungCapNavigation).WithMany(p => p.PhieuNhapHang)
                .HasForeignKey(d => d.MaNhaCungCap)
                .HasConstraintName("FK__PHIEU_NHA__maNha__47DBAE45");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.PhieuNhapHang)
                .HasForeignKey(d => d.MaNhanVien)
                .HasConstraintName("FK__PHIEU_NHA__maNha__48CFD27E");
        });

        modelBuilder.Entity<PhieuTraLoi>(entity =>
        {
            entity.HasKey(e => e.MaPhieuTraLoi).HasName("PK__PHIEU_TR__528C69450F64FAC8");

            entity.ToTable("PHIEU_TRA_LOI");

            entity.Property(e => e.MaPhieuTraLoi)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maPhieuTraLoi");
            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maKhachHang");
            entity.Property(e => e.MaKhaoSat)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maKhaoSat");
            entity.Property(e => e.NgayTraLoi)
                .HasColumnType("datetime")
                .HasColumnName("ngayTraLoi");

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.PhieuTraLoi)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__PHIEU_TRA__maKha__06CD04F7");

            entity.HasOne(d => d.MaKhaoSatNavigation).WithMany(p => p.PhieuTraLoi)
                .HasForeignKey(d => d.MaKhaoSat)
                .HasConstraintName("FK__PHIEU_TRA__maKha__05D8E0BE");
        });

        modelBuilder.Entity<ThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaThanhToan).HasName("PK__THANH_TO__7675FE60286E11C5");

            entity.ToTable("THANH_TOAN");

            entity.Property(e => e.MaThanhToan)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maThanhToan");
            entity.Property(e => e.MaHoaDon)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maHoaDon");
            entity.Property(e => e.NgayThanhToan)
                .HasColumnType("datetime")
                .HasColumnName("ngayThanhToan");
            entity.Property(e => e.PhuongThuc)
                .HasMaxLength(30)
                .HasColumnName("phuongThuc");
            entity.Property(e => e.SoTien).HasColumnName("soTien");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasColumnName("trangThai");

            entity.HasOne(d => d.MaHoaDonNavigation).WithMany(p => p.ThanhToan)
                .HasForeignKey(d => d.MaHoaDon)
                .HasConstraintName("FK__THANH_TOA__maHoa__66603565");
        });

        modelBuilder.Entity<TraLoiDanhGia>(entity =>
        {
            entity.HasKey(e => e.MaTraLoi).HasName("PK__TRA_LOI___F2C812BC63877A40");

            entity.ToTable("TRA_LOI_DANH_GIA");

            entity.Property(e => e.MaTraLoi)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maTraLoi");
            entity.Property(e => e.MaDanhGia)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maDanhGia");
            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maKhachHang");
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maNhanVien");
            entity.Property(e => e.NgayGui)
                .HasColumnType("datetime")
                .HasColumnName("ngayGui");
            entity.Property(e => e.NguoiGui)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("nguoiGui");
            entity.Property(e => e.NoiDung).HasColumnName("noiDung");

            entity.HasOne(d => d.MaDanhGiaNavigation).WithMany(p => p.TraLoiDanhGia)
                .HasForeignKey(d => d.MaDanhGia)
                .HasConstraintName("FK__TRA_LOI_D__maDan__0E6E26BF");

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.TraLoiDanhGia)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__TRA_LOI_D__maKha__10566F31");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.TraLoiDanhGia)
                .HasForeignKey(d => d.MaNhanVien)
                .HasConstraintName("FK__TRA_LOI_D__maNha__114A936A");
        });

        modelBuilder.Entity<TraLoiPhanHoi>(entity =>
        {
            entity.HasKey(e => e.MaTraLoi).HasName("PK__TRA_LOI___F2C812BCAE50D14B");

            entity.ToTable("TRA_LOI_PHAN_HOI");

            entity.Property(e => e.MaTraLoi)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maTraLoi");
            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maKhachHang");
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maNhanVien");
            entity.Property(e => e.MaPhanHoi)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maPhanHoi");
            entity.Property(e => e.NgayGui)
                .HasColumnType("datetime")
                .HasColumnName("ngayGui");
            entity.Property(e => e.NguoiGui)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("nguoiGui");
            entity.Property(e => e.NoiDung).HasColumnName("noiDung");

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.TraLoiPhanHoi)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__TRA_LOI_P__maKha__17036CC0");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.TraLoiPhanHoi)
                .HasForeignKey(d => d.MaNhanVien)
                .HasConstraintName("FK__TRA_LOI_P__maNha__17F790F9");

            entity.HasOne(d => d.MaPhanHoiNavigation).WithMany(p => p.TraLoiPhanHoi)
                .HasForeignKey(d => d.MaPhanHoi)
                .HasConstraintName("FK__TRA_LOI_P__maPha__151B244E");
        });

        modelBuilder.Entity<TuyChonCauHoi>(entity =>
        {
            entity.HasKey(e => e.MaTuyChon).HasName("PK__TUY_CHON__30A8DE7DDBCA42C9");

            entity.ToTable("TUY_CHON_CAU_HOI");

            entity.Property(e => e.MaTuyChon)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maTuyChon");
            entity.Property(e => e.MaCauHoi)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maCauHoi");
            entity.Property(e => e.NoiDungTuyChon)
                .HasMaxLength(255)
                .HasColumnName("noiDungTuyChon");

            entity.HasOne(d => d.MaCauHoiNavigation).WithMany(p => p.TuyChonCauHoi)
                .HasForeignKey(d => d.MaCauHoi)
                .HasConstraintName("FK__TUY_CHON___maCau__7F2BE32F");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.MaVaiTro).HasName("PK__VAI_TRO__BFC88AB788DC447A");

            entity.ToTable("VAI_TRO");

            entity.Property(e => e.MaVaiTro)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maVaiTro");
            entity.Property(e => e.MoTa)
                .HasMaxLength(255)
                .HasColumnName("moTa");
            entity.Property(e => e.TenVaiTro)
                .HasMaxLength(100)
                .HasColumnName("tenVaiTro");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

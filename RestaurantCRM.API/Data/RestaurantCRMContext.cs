using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RestaurantCRM.API.Models;

namespace RestaurantCRM.API.Data;

public partial class RestaurantCRMContext : DbContext
{
    public RestaurantCRMContext(DbContextOptions<RestaurantCRMContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CauHoiKhaoSat> CauHoiKhaoSats { get; set; }

    public virtual DbSet<CauTraLoi> CauTraLois { get; set; }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<ChiTietKhuyenMaiMon> ChiTietKhuyenMaiMons { get; set; }

    public virtual DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }

    public virtual DbSet<ChuongTrinhKhuyenMai> ChuongTrinhKhuyenMais { get; set; }

    public virtual DbSet<DanhGia> DanhGias { get; set; }

    public virtual DbSet<DoiTuongKhaoSat> DoiTuongKhaoSats { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<HinhAnhMonAn> HinhAnhMonAns { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<KhaoSat> KhaoSats { get; set; }

    public virtual DbSet<LichSuTrangThai> LichSuTrangThais { get; set; }

    public virtual DbSet<LoaiMon> LoaiMons { get; set; }

    public virtual DbSet<MonAn> MonAns { get; set; }

    public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<PhanHoi> PhanHois { get; set; }

    public virtual DbSet<PhieuNhapHang> PhieuNhapHangs { get; set; }

    public virtual DbSet<PhieuTraLoi> PhieuTraLois { get; set; }

    public virtual DbSet<TaiKhoanAdmin> TaiKhoanAdmins { get; set; }

    public virtual DbSet<ThanhToan> ThanhToans { get; set; }

    public virtual DbSet<TuyChonCauHoi> TuyChonCauHois { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CauHoiKhaoSat>(entity =>
        {
            entity.HasKey(e => e.MaCauHoi).HasName("PK__CAU_HOI___82BDFBDCFBBAE45F");

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

            entity.HasOne(d => d.MaKhaoSatNavigation).WithMany(p => p.CauHoiKhaoSats)
                .HasForeignKey(d => d.MaKhaoSat)
                .HasConstraintName("FK__CAU_HOI_K__maKha__71D1E811");
        });

        modelBuilder.Entity<CauTraLoi>(entity =>
        {
            entity.HasKey(e => e.MaCauTraLoi).HasName("PK__CAU_TRA___51956E38F680767D");

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

            entity.HasOne(d => d.MaCauHoiNavigation).WithMany(p => p.CauTraLois)
                .HasForeignKey(d => d.MaCauHoi)
                .HasConstraintName("FK__CAU_TRA_L__maCau__00200768");

            entity.HasOne(d => d.MaPhieuTraLoiNavigation).WithMany(p => p.CauTraLois)
                .HasForeignKey(d => d.MaPhieuTraLoi)
                .HasConstraintName("FK__CAU_TRA_L__maPhi__7F2BE32F");

            entity.HasOne(d => d.MaTuyChonNavigation).WithMany(p => p.CauTraLois)
                .HasForeignKey(d => d.MaTuyChon)
                .HasConstraintName("FK__CAU_TRA_L__maTuy__01142BA1");
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => e.MaChiTiet).HasName("PK__CHI_TIET__99964888D57D2FA0");

            entity.ToTable("CHI_TIET_DON_HANG");

            entity.Property(e => e.MaChiTiet)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maChiTiet");
            entity.Property(e => e.DonGiaGoc).HasColumnName("donGiaGoc");
            entity.Property(e => e.DonGiaSauGiam).HasColumnName("donGiaSauGiam");
            entity.Property(e => e.MaChuongTrinhKm)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maChuongTrinhKM");
            entity.Property(e => e.MaDonHang)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maDonHang");
            entity.Property(e => e.MaMon)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maMon");
            entity.Property(e => e.SoLuong).HasColumnName("soLuong");
            entity.Property(e => e.ThanhTien).HasColumnName("thanhTien");
            entity.Property(e => e.TienGiamMon).HasColumnName("tienGiamMon");

            entity.HasOne(d => d.MaChuongTrinhKmNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaChuongTrinhKm)
                .HasConstraintName("FK__CHI_TIET___maChu__5DCAEF64");

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaDonHang)
                .HasConstraintName("FK__CHI_TIET___maDon__5BE2A6F2");

            entity.HasOne(d => d.MaMonNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaMon)
                .HasConstraintName("FK__CHI_TIET___maMon__5CD6CB2B");
        });

        modelBuilder.Entity<ChiTietKhuyenMaiMon>(entity =>
        {
            entity.HasKey(e => e.MaChiTietKm).HasName("PK__CHI_TIET__34810A8B48D153E3");

            entity.ToTable("CHI_TIET_KHUYEN_MAI_MON");

            entity.Property(e => e.MaChiTietKm)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maChiTietKM");
            entity.Property(e => e.MaChuongTrinh)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maChuongTrinh");
            entity.Property(e => e.MaMon)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maMon");
            entity.Property(e => e.SoLuongApDung).HasColumnName("soLuongApDung");

            entity.HasOne(d => d.MaChuongTrinhNavigation).WithMany(p => p.ChiTietKhuyenMaiMons)
                .HasForeignKey(d => d.MaChuongTrinh)
                .HasConstraintName("FK__CHI_TIET___maChu__534D60F1");

            entity.HasOne(d => d.MaMonNavigation).WithMany(p => p.ChiTietKhuyenMaiMons)
                .HasForeignKey(d => d.MaMon)
                .HasConstraintName("FK__CHI_TIET___maMon__5441852A");
        });

        modelBuilder.Entity<ChiTietPhieuNhap>(entity =>
        {
            entity.HasKey(e => e.MaChiTiet).HasName("PK__CHI_TIET__9996488823CEA1AE");

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

            entity.HasOne(d => d.MaMonNavigation).WithMany(p => p.ChiTietPhieuNhaps)
                .HasForeignKey(d => d.MaMon)
                .HasConstraintName("FK__CHI_TIET___maMon__4E88ABD4");

            entity.HasOne(d => d.MaPhieuNhapNavigation).WithMany(p => p.ChiTietPhieuNhaps)
                .HasForeignKey(d => d.MaPhieuNhap)
                .HasConstraintName("FK__CHI_TIET___maPhi__4D94879B");
        });

        modelBuilder.Entity<ChuongTrinhKhuyenMai>(entity =>
        {
            entity.HasKey(e => e.MaChuongTrinh).HasName("PK__CHUONG_T__4F35F615D774F629");

            entity.ToTable("CHUONG_TRINH_KHUYEN_MAI");

            entity.Property(e => e.MaChuongTrinh)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maChuongTrinh");
            entity.Property(e => e.GiaTriDonToiThieu).HasColumnName("giaTriDonToiThieu");
            entity.Property(e => e.GiaTriGiam).HasColumnName("giaTriGiam");
            entity.Property(e => e.LoaiGiam)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("loaiGiam");
            entity.Property(e => e.LoaiKhuyenMai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("loaiKhuyenMai");
            entity.Property(e => e.NgayBatDau).HasColumnName("ngayBatDau");
            entity.Property(e => e.NgayKetThuc).HasColumnName("ngayKetThuc");
            entity.Property(e => e.TenChuongTrinh)
                .HasMaxLength(150)
                .HasColumnName("tenChuongTrinh");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasColumnName("trangThai");
        });

        modelBuilder.Entity<DanhGia>(entity =>
        {
            entity.HasKey(e => e.MaDanhGia).HasName("PK__DANH_GIA__6B15DD9A3079D4AD");

            entity.ToTable("DANH_GIA");

            entity.Property(e => e.MaDanhGia)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maDanhGia");
            entity.Property(e => e.MaDonHang)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maDonHang");
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

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaDonHang)
                .HasConstraintName("FK__DANH_GIA__maDonH__6477ECF3");

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__DANH_GIA__maKhac__6383C8BA");

            entity.HasOne(d => d.MaMonNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaMon)
                .HasConstraintName("FK__DANH_GIA__maMon__656C112C");
        });

        modelBuilder.Entity<DoiTuongKhaoSat>(entity =>
        {
            entity.HasKey(e => e.MaDoiTuong).HasName("PK__DOI_TUON__8B6358B463E20179");

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

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.DoiTuongKhaoSats)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__DOI_TUONG__maKha__787EE5A0");

            entity.HasOne(d => d.MaKhaoSatNavigation).WithMany(p => p.DoiTuongKhaoSats)
                .HasForeignKey(d => d.MaKhaoSat)
                .HasConstraintName("FK__DOI_TUONG__maKha__778AC167");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.MaDonHang).HasName("PK__DON_HANG__871D3819E1E3FD70");

            entity.ToTable("DON_HANG");

            entity.Property(e => e.MaDonHang)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maDonHang");
            entity.Property(e => e.DiaChiGiao)
                .HasMaxLength(255)
                .HasColumnName("diaChiGiao");
            entity.Property(e => e.MaChuongTrinhVoucher)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maChuongTrinhVoucher");
            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maKhachHang");
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maNhanVien");
            entity.Property(e => e.NgayDat)
                .HasColumnType("datetime")
                .HasColumnName("ngayDat");
            entity.Property(e => e.TienGiamVoucher).HasColumnName("tienGiamVoucher");
            entity.Property(e => e.TongThanhToan).HasColumnName("tongThanhToan");
            entity.Property(e => e.TongTienHang).HasColumnName("tongTienHang");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasColumnName("trangThai");

            entity.HasOne(d => d.MaChuongTrinhVoucherNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaChuongTrinhVoucher)
                .HasConstraintName("FK__DON_HANG__maChuo__59063A47");

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__DON_HANG__maKhac__571DF1D5");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaNhanVien)
                .HasConstraintName("FK__DON_HANG__maNhan__5812160E");
        });

        modelBuilder.Entity<HinhAnhMonAn>(entity =>
        {
            entity.HasKey(e => e.MaHinhAnh).HasName("PK__HINH_ANH__134CD06CEA738CFF");

            entity.ToTable("HINH_ANH_MON_AN");

            entity.Property(e => e.MaHinhAnh)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maHinhAnh");
            entity.Property(e => e.DuongDanAnh)
                .HasMaxLength(255)
                .HasColumnName("duongDanAnh");
            entity.Property(e => e.LaAnhDaiDien).HasColumnName("laAnhDaiDien");
            entity.Property(e => e.MaMon)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maMon");
            entity.Property(e => e.ThuTuHienThi).HasColumnName("thuTuHienThi");

            entity.HasOne(d => d.MaMonNavigation).WithMany(p => p.HinhAnhMonAns)
                .HasForeignKey(d => d.MaMon)
                .HasConstraintName("FK__HINH_ANH___maMon__44FF419A");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKhachHang).HasName("PK__KHACH_HA__0CCB3D49C6D7540A");

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
            entity.HasKey(e => e.MaKhaoSat).HasName("PK__KHAO_SAT__AFDCED6E51036D90");

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

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.KhaoSats)
                .HasForeignKey(d => d.MaNhanVien)
                .HasConstraintName("FK__KHAO_SAT__maNhan__6EF57B66");
        });

        modelBuilder.Entity<LichSuTrangThai>(entity =>
        {
            entity.HasKey(e => e.MaLichSu).HasName("PK__LICH_SU___1F9C95FD4CDFDB92");

            entity.ToTable("LICH_SU_TRANG_THAI");

            entity.Property(e => e.MaLichSu)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maLichSu");
            entity.Property(e => e.GhiChu)
                .HasMaxLength(255)
                .HasColumnName("ghiChu");
            entity.Property(e => e.MaDonHang)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maDonHang");
            entity.Property(e => e.ThoiGian)
                .HasColumnType("datetime")
                .HasColumnName("thoiGian");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasColumnName("trangThai");

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.LichSuTrangThais)
                .HasForeignKey(d => d.MaDonHang)
                .HasConstraintName("FK__LICH_SU_T__maDon__68487DD7");
        });

        modelBuilder.Entity<LoaiMon>(entity =>
        {
            entity.HasKey(e => e.MaLoaiMon).HasName("PK__LOAI_MON__3A9F145591B16E2D");

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
            entity.HasKey(e => e.MaMon).HasName("PK__MON_AN__27547BFA10EDE5BC");

            entity.ToTable("MON_AN");

            entity.Property(e => e.MaMon)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maMon");
            entity.Property(e => e.DonGia).HasColumnName("donGia");
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

            entity.HasOne(d => d.MaLoaiMonNavigation).WithMany(p => p.MonAns)
                .HasForeignKey(d => d.MaLoaiMon)
                .HasConstraintName("FK__MON_AN__maLoaiMo__4222D4EF");
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.MaNhaCungCap).HasName("PK__NHA_CUNG__D0B4D6DE17DC7CD0");

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
            entity.HasKey(e => e.MaNhanVien).HasName("PK__NHAN_VIE__BDDEF20D09F31CA0");

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

            entity.HasOne(d => d.MaVaiTroNavigation).WithMany(p => p.NhanViens)
                .HasForeignKey(d => d.MaVaiTro)
                .HasConstraintName("FK__NHAN_VIEN__maVai__3B75D760");
        });

        modelBuilder.Entity<PhanHoi>(entity =>
        {
            entity.HasKey(e => e.MaPhanHoi).HasName("PK__PHAN_HOI__4E079CEC86E69264");

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

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.PhanHois)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__PHAN_HOI__maKhac__6B24EA82");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.PhanHois)
                .HasForeignKey(d => d.MaNhanVien)
                .HasConstraintName("FK__PHAN_HOI__maNhan__6C190EBB");
        });

        modelBuilder.Entity<PhieuNhapHang>(entity =>
        {
            entity.HasKey(e => e.MaPhieuNhap).HasName("PK__PHIEU_NH__E27639343859E31E");

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

            entity.HasOne(d => d.MaNhaCungCapNavigation).WithMany(p => p.PhieuNhapHangs)
                .HasForeignKey(d => d.MaNhaCungCap)
                .HasConstraintName("FK__PHIEU_NHA__maNha__49C3F6B7");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.PhieuNhapHangs)
                .HasForeignKey(d => d.MaNhanVien)
                .HasConstraintName("FK__PHIEU_NHA__maNha__4AB81AF0");
        });

        modelBuilder.Entity<PhieuTraLoi>(entity =>
        {
            entity.HasKey(e => e.MaPhieuTraLoi).HasName("PK__PHIEU_TR__528C694571FCE94A");

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

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.PhieuTraLois)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__PHIEU_TRA__maKha__7C4F7684");

            entity.HasOne(d => d.MaKhaoSatNavigation).WithMany(p => p.PhieuTraLois)
                .HasForeignKey(d => d.MaKhaoSat)
                .HasConstraintName("FK__PHIEU_TRA__maKha__7B5B524B");
        });

        modelBuilder.Entity<TaiKhoanAdmin>(entity =>
        {
            entity.HasKey(e => e.MaAdmin).HasName("PK__TAI_KHOA__B59817FA5423D20A");

            entity.ToTable("TAI_KHOAN_ADMIN");

            entity.Property(e => e.MaAdmin)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maAdmin");
            entity.Property(e => e.HoTen)
                .HasMaxLength(100)
                .HasColumnName("hoTen");
            entity.Property(e => e.MatKhau)
                .HasMaxLength(100)
                .HasColumnName("matKhau");
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .HasColumnName("tenDangNhap");
        });

        modelBuilder.Entity<ThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaThanhToan).HasName("PK__THANH_TO__7675FE6079BF84F1");

            entity.ToTable("THANH_TOAN");

            entity.Property(e => e.MaThanhToan)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maThanhToan");
            entity.Property(e => e.MaDonHang)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maDonHang");
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

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.ThanhToans)
                .HasForeignKey(d => d.MaDonHang)
                .HasConstraintName("FK__THANH_TOA__maDon__60A75C0F");
        });

        modelBuilder.Entity<TuyChonCauHoi>(entity =>
        {
            entity.HasKey(e => e.MaTuyChon).HasName("PK__TUY_CHON__30A8DE7D12E26AB7");

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

            entity.HasOne(d => d.MaCauHoiNavigation).WithMany(p => p.TuyChonCauHois)
                .HasForeignKey(d => d.MaCauHoi)
                .HasConstraintName("FK__TUY_CHON___maCau__74AE54BC");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.MaVaiTro).HasName("PK__VAI_TRO__BFC88AB75592EC7E");

            entity.ToTable("VAI_TRO");

            entity.Property(e => e.MaVaiTro)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maVaiTro");
            entity.Property(e => e.TenVaiTro)
                .HasMaxLength(100)
                .HasColumnName("tenVaiTro");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

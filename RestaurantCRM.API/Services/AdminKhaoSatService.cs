using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantCRM.BusinessLogic.Models;

namespace RestaurantCRM.API.Services
{
    public class KhaoSatAnswerItem
    {
        public string? MaCauHoi { get; set; }
        public string? MaTuyChon { get; set; }
        public string? MaPhieuTraLoi { get; set; }
        public string? NoiDungTuDien { get; set; }
        public string? TenKhachHang { get; set; }
        public DateTime? NgayTraLoi { get; set; }
    }

    /// <summary>
    /// Nghiệp vụ chuẩn hóa dữ liệu, xác thực câu hỏi và tổng hợp thống kê kết quả khảo sát.
    /// </summary>
    public static class AdminKhaoSatService
    {
        public const string Nhap = "Draft";
        public const string DangMo = "Open";
        public const string DaDong = "Closed";
        public const string TracNghiem = "Option";
        public const string TuLuan = "Text";

        /// <summary>Quy đổi trạng thái về giá trị hợp lệ của CSDL (Draft / Open / Closed).</summary>
        public static string? ChuanHoaTrangThai(string? s)
        {
            switch ((s ?? "").Trim().ToLowerInvariant())
            {
                case "draft": case "nhap": case "nháp": return Nhap;
                case "open": case "active": case "dangmo": case "đang mở": return DangMo;
                case "closed": case "close": case "dadong": case "đã đóng": return DaDong;
                default: return null;
            }
        }

        /// <summary>Quy đổi loại câu hỏi về giá trị hợp lệ của CSDL (Option / Text).</summary>
        public static string? ChuanHoaLoai(string? s)
        {
            switch ((s ?? "").Trim().ToLowerInvariant())
            {
                case "option": case "tracnghiem": case "trắc nghiệm": case "choice": return TracNghiem;
                case "text": case "tuluan": case "tự luận": return TuLuan;
                default: return null;
            }
        }

        public static string? KiemTraCauHoi(string? noiDung, string? loaiCauHoi, List<string>? tuyChons)
        {
            if (string.IsNullOrWhiteSpace(noiDung))
                return "Nội dung câu hỏi không được để trống.";
            if (noiDung.Trim().Length > 500)
                return "Nội dung câu hỏi tối đa 500 ký tự.";

            var loai = ChuanHoaLoai(loaiCauHoi);
            if (loai == null)
                return "Loại câu hỏi không hợp lệ. Chấp nhận 'Option' (trắc nghiệm) hoặc 'Text' (tự luận).";

            if (loai == TracNghiem)
            {
                var ds = LamSachTuyChon(tuyChons);
                if (ds.Count < 2)
                    return "Câu hỏi trắc nghiệm cần ít nhất 2 tùy chọn (không tính tùy chọn trống hoặc trùng nhau).";
                if (ds.Any(t => t.Length > 255))
                    return "Nội dung tùy chọn tối đa 255 ký tự.";
            }
            return null;
        }

        /// <summary>Bỏ tùy chọn trống, cắt khoảng trắng và bỏ trùng (giữ thứ tự).</summary>
        public static List<string> LamSachTuyChon(List<string>? tuyChons)
        {
            var kq = new List<string>();
            if (tuyChons == null) return kq;
            foreach (var t in tuyChons)
            {
                if (string.IsNullOrWhiteSpace(t)) continue;
                var v = t.Trim();
                if (!kq.Contains(v)) kq.Add(v);
            }
            return kq;
        }

        /// <summary>Sắp theo mã tự nhiên: CH2 trước CH10.</summary>
        public static IEnumerable<T> SapXep<T>(IEnumerable<T> items, Func<T, string> key)
        {
            return items.OrderBy(x => key(x).Length).ThenBy(key, StringComparer.Ordinal);
        }

        /// <summary>
        /// Tổng hợp và tính toán thống kê tỉ lệ lựa chọn, phân bổ lượt trả lời trắc nghiệm & tự luận.
        /// </summary>
        public static List<object> AggregateQuestionResults(IEnumerable<CauHoiKhaoSat> cauHois, IList<KhaoSatAnswerItem> cauTraLois)
        {
            var cauHoiResults = new List<object>();
            foreach (var ch in SapXep(cauHois, c => c.MaCauHoi))
            {
                var cuaCauHoi = cauTraLois.Where(c => c.MaCauHoi == ch.MaCauHoi).ToList();
                int soLuotTraLoi = cuaCauHoi.Select(c => c.MaPhieuTraLoi).Distinct().Count();
                bool laTuLuan = ChuanHoaLoai(ch.LoaiCauHoi) == TuLuan;

                if (!laTuLuan)
                {
                    var thongKeTuyChon = SapXep(ch.TuyChonCauHoi, t => t.MaTuyChon).Select(tc =>
                    {
                        int soNguoiChon = cuaCauHoi.Count(c => c.MaTuyChon == tc.MaTuyChon);
                        return new
                        {
                            tc.MaTuyChon,
                            tc.NoiDungTuyChon,
                            SoNguoiChon = soNguoiChon,
                            // Tỷ lệ trên số phiếu đã trả lời câu này (câu nhiều lựa chọn có thể vượt 100% khi cộng lại)
                            TyLe = soLuotTraLoi > 0 ? Math.Round((double)soNguoiChon / soLuotTraLoi * 100, 2) : 0.0
                        };
                    }).ToList();

                    cauHoiResults.Add(new
                    {
                        ch.MaCauHoi,
                        ch.NoiDungCauHoi,
                        LoaiCauHoi = "TracNghiem",
                        SoLuotTraLoi = soLuotTraLoi,
                        KetQuaTN = thongKeTuyChon,
                        KetQuaTuLuan = (object?)null,
                        TraLoiTuLuan = (object?)null
                    });
                }
                else
                {
                    var tuLuan = cuaCauHoi
                        .Where(c => !string.IsNullOrWhiteSpace(c.NoiDungTuDien))
                        .OrderByDescending(c => c.NgayTraLoi)
                        .ToList();

                    cauHoiResults.Add(new
                    {
                        ch.MaCauHoi,
                        ch.NoiDungCauHoi,
                        LoaiCauHoi = "TuLuan",
                        SoLuotTraLoi = soLuotTraLoi,
                        KetQuaTN = (object?)null,
                        KetQuaTuLuan = tuLuan.Select(c => c.NoiDungTuDien).ToList(),
                        TraLoiTuLuan = tuLuan.Select(c => new { NoiDung = c.NoiDungTuDien, c.TenKhachHang, c.NgayTraLoi }).ToList()
                    });
                }
            }

            return cauHoiResults;
        }
    }
}

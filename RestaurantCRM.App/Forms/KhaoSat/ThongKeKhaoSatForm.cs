using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RestaurantCRM.AdminApp.Forms.KhaoSat
{
    /// <summary>
    /// Form thống kê kết quả khảo sát:
    ///   - Hiển thị tổng số phiếu trả lời
    ///   - Câu trắc nghiệm: số lượt chọn + tỉ lệ % mỗi tùy chọn + thanh tiến trình
    ///   - Câu tự luận: danh sách câu trả lời
    /// </summary>
    public partial class ThongKeKhaoSatForm : Form
    {
        // ── Màu sắc chủ đề ────────────────────────────────────────────────────
        private static readonly Color _accent   = Color.FromArgb(139, 92, 246);
        private static readonly Color _bg       = Color.FromArgb(15, 15, 25);
        private static readonly Color _surface  = Color.FromArgb(30, 30, 45);
        private static readonly Color _surface2 = Color.FromArgb(45, 45, 65);
        private static readonly Color _textMain = Color.FromArgb(240, 240, 255);
        private static readonly Color _textSub  = Color.FromArgb(160, 160, 190);
        private static readonly Color _bar      = Color.FromArgb(99, 58, 210);
        private static readonly Color _barBg    = Color.FromArgb(55, 55, 75);

        private static readonly HttpClient _http = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5275")
        };

        private readonly string _maKhaoSat;
        private const int ContentWidth = 740;

        // Panel cuộn chứa các card câu hỏi
        private Panel _pnlScroll;

        public ThongKeKhaoSatForm(string maKhaoSat, string tieuDe)
        {
            _maKhaoSat = maKhaoSat;
            InitializeComponent();
            BuildLayout(tieuDe);
            this.Load += async (s, e) => await LoadThongKe();
        }

        // ── Xây dựng layout ──────────────────────────────────────────────────
        private void BuildLayout(string tieuDe)
        {
            this.BackColor = _bg;
            this.ForeColor = _textMain;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);

            // Header
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 64,
                BackColor = Color.FromArgb(25, 25, 40)
            };

            var lblTitle = new Label
            {
                Text      = $"📊  Thống kê khảo sát",
                AutoSize  = false,
                Location  = new Point(20, 8),
                Size      = new Size(500, 26),
                ForeColor = _accent,
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold)
            };
            var lblSub = new Label
            {
                Text      = tieuDe,
                AutoSize  = false,
                Location  = new Point(20, 36),
                Size      = new Size(680, 20),
                ForeColor = _textSub,
                Font      = new Font("Segoe UI", 9f)
            };

            var btnClose = new Button
            {
                Text      = "✕  Đóng",
                Size      = new Size(100, 32),
                Location  = new Point(this.ClientSize.Width - 120, 16),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                ForeColor = _textMain,
                BackColor = Color.FromArgb(60, 60, 80)
            };
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 100);
            btnClose.Click += (s, e) => this.Close();

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSub, btnClose });
            this.Controls.Add(pnlHeader);

            // Panel cuộn
            _pnlScroll = new Panel
            {
                Dock        = DockStyle.Fill,
                AutoScroll  = true,
                BackColor   = _bg,
                Padding     = new Padding(0)
            };
            this.Controls.Add(_pnlScroll);
        }

        // ── Load & render thống kê ────────────────────────────────────────────
        private async Task LoadThongKe()
        {
            _pnlScroll.Controls.Clear();
            _pnlScroll.Controls.Add(new Label
            {
                Text      = "⏳  Đang tải dữ liệu...",
                ForeColor = _textSub,
                AutoSize  = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI", 10f)
            });

            try
            {
                var result = await _http.GetFromJsonAsync<ThongKeKhaoSatDTO>(
                    $"/api/AdminKhaoSat/{_maKhaoSat}/results");

                _pnlScroll.Controls.Clear();

                if (result == null)
                {
                    ShowError("Không lấy được dữ liệu thống kê.");
                    return;
                }

                RenderThongKe(result);
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi kết nối API: {ex.Message}");
            }
        }

        private void ShowError(string msg)
        {
            _pnlScroll.Controls.Clear();
            _pnlScroll.Controls.Add(new Label
            {
                Text      = "⚠  " + msg,
                ForeColor = Color.FromArgb(248, 113, 113),
                AutoSize  = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI", 10f)
            });
        }

        // ── Render từng câu hỏi ──────────────────────────────────────────────
        private void RenderThongKe(ThongKeKhaoSatDTO data)
        {
            int y = 16;

            // Tổng phiếu
            var lblTong = MakeLabel(
                $"📋  Tổng số phiếu trả lời: {data.TongSoNguoiTraLoi}",
                new Point(16, y), ContentWidth - 32, 28,
                new Font("Segoe UI", 11f, FontStyle.Bold), _accent);
            _pnlScroll.Controls.Add(lblTong);
            y += 38;

            if (data.ChiTiet == null || data.ChiTiet.Count == 0)
            {
                _pnlScroll.Controls.Add(MakeLabel(
                    "Khảo sát chưa có câu hỏi hoặc chưa có phiếu trả lời.",
                    new Point(16, y), ContentWidth - 32, 24,
                    new Font("Segoe UI", 9.5f, FontStyle.Italic), _textSub));
                return;
            }

            int soTT = 1;
            foreach (var ch in data.ChiTiet)
            {
                int cardH = CalcCardHeight(ch);

                var card = new Panel
                {
                    Location  = new Point(16, y),
                    Size      = new Size(ContentWidth - 32, cardH),
                    BackColor = _surface
                };
                card.Paint += (s, e) => DrawRoundedCard(e.Graphics, card.ClientRectangle);
                _pnlScroll.Controls.Add(card);

                FillCard(card, soTT, ch, data.TongSoNguoiTraLoi);

                y += cardH + 16;
                soTT++;
            }

            _pnlScroll.AutoScrollMinSize = new Size(0, y + 16);
        }

        // Tính chiều cao card
        private int CalcCardHeight(ChiTietCauHoiDTO ch)
        {
            int h = 56; // header câu hỏi
            if (ch.LoaiCauHoi == "TracNghiem" && ch.KetQuaTN != null)
                h += ch.KetQuaTN.Count * 62 + 14;
            else if (ch.KetQuaTuLuan != null)
                h += Math.Max(ch.KetQuaTuLuan.Count, 1) * 36 + 14;
            return h;
        }

        // Điền nội dung vào card
        private void FillCard(Panel card, int soTT, ChiTietCauHoiDTO ch, int tongPhieu)
        {
            int cy = 12;

            string loaiLabel = ch.LoaiCauHoi == "TracNghiem" ? "Trắc nghiệm" : "Tự luận";
            card.Controls.Add(MakeLabel(
                $"Câu {soTT}:  {ch.NoiDungCauHoi}",
                new Point(14, cy), card.Width - 28, 22,
                new Font("Segoe UI", 10f, FontStyle.Bold), _textMain));
            cy += 24;

            card.Controls.Add(MakeLabel(
                $"[{loaiLabel}]",
                new Point(14, cy), 130, 18,
                new Font("Segoe UI", 8.5f, FontStyle.Italic), _accent));
            cy += 22;

            // ── Trắc nghiệm ───────────────────────────────────────────────
            if (ch.LoaiCauHoi == "TracNghiem" && ch.KetQuaTN != null)
            {
                foreach (var tc in ch.KetQuaTN)
                {
                    double pct = Math.Min(Math.Max(tc.TyLe, 0), 100);
                    int    barW = (int)((card.Width - 28) * pct / 100.0);

                    card.Controls.Add(MakeLabel(
                        $"▸  {tc.NoiDungTuyChon}",
                        new Point(14, cy), card.Width - 28, 18,
                        new Font("Segoe UI", 9f), _textMain));
                    cy += 20;

                    // Thanh tiến trình
                    var barBg = new Panel
                    {
                        Location  = new Point(14, cy),
                        Size      = new Size(card.Width - 28, 14),
                        BackColor = _barBg
                    };
                    if (barW > 0)
                    {
                        barBg.Controls.Add(new Panel
                        {
                            Location  = new Point(0, 0),
                            Size      = new Size(barW, 14),
                            BackColor = _bar
                        });
                    }
                    card.Controls.Add(barBg);
                    cy += 17;

                    card.Controls.Add(MakeLabel(
                        $"   {tc.SoNguoiChon} lượt  —  {tc.TyLe:0.#}%",
                        new Point(14, cy), 260, 16,
                        new Font("Segoe UI", 8.5f), _textSub));
                    cy += 20;
                }
            }
            // ── Tự luận ───────────────────────────────────────────────────
            else if (ch.KetQuaTuLuan != null)
            {
                if (ch.KetQuaTuLuan.Count == 0)
                {
                    card.Controls.Add(MakeLabel(
                        "  (Chưa có câu trả lời)",
                        new Point(14, cy), card.Width - 28, 18,
                        new Font("Segoe UI", 9f, FontStyle.Italic), _textSub));
                }
                else
                {
                    int idx = 1;
                    foreach (var ans in ch.KetQuaTuLuan)
                    {
                        card.Controls.Add(MakeLabel(
                            $"  {idx}. \"{ans}\"",
                            new Point(14, cy), card.Width - 28, 30,
                            new Font("Segoe UI", 9f, FontStyle.Italic), _textMain));
                        cy += 34;
                        idx++;
                    }
                }
            }
        }

        // ── Helper ───────────────────────────────────────────────────────────
        private Label MakeLabel(string text, Point loc, int width, int height,
            Font font, Color color)
        {
            return new Label
            {
                Text      = text,
                Location  = loc,
                Size      = new Size(width, height),
                ForeColor = color,
                BackColor = Color.Transparent,
                Font      = font,
                AutoSize  = false
            };
        }

        private void DrawRoundedCard(Graphics g, Rectangle rect)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = GetRoundedPath(rect, 10))
            {
                using (var fill = new SolidBrush(_surface))
                    g.FillPath(fill, path);
                using (var pen = new Pen(Color.FromArgb(60, 60, 90), 1f))
                    g.DrawPath(pen, path);
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X,          r.Y,           d, d, 180, 90);
            path.AddArc(r.Right - d,  r.Y,           d, d, 270, 90);
            path.AddArc(r.Right - d,  r.Bottom - d,  d, d,   0, 90);
            path.AddArc(r.X,          r.Bottom - d,  d, d,  90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // ── DTOs nội bộ form ─────────────────────────────────────────────────────
    internal class ThongKeKhaoSatDTO
    {
        public string MaKhaoSat           { get; set; }
        public string TieuDe              { get; set; }
        public int    TongSoNguoiTraLoi   { get; set; }
        public List<ChiTietCauHoiDTO> ChiTiet { get; set; }
    }

    internal class ChiTietCauHoiDTO
    {
        public string MaCauHoi      { get; set; }
        public string NoiDungCauHoi { get; set; }
        public string LoaiCauHoi    { get; set; }

        // Trắc nghiệm: dữ liệu mảng object từ API — map theo KetQua
        public List<TuyChonThongKeDTO> KetQuaTN     { get; set; }
        // Tự luận: list string
        public List<string>            KetQuaTuLuan { get; set; }
    }

    internal class TuyChonThongKeDTO
    {
        public string MaTuyChon      { get; set; }
        public string NoiDungTuyChon { get; set; }
        public int    SoNguoiChon    { get; set; }
        public double TyLe           { get; set; }
    }
}

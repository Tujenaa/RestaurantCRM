using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.DTOs.KhaoSat;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.KhaoSat
{
    /// <summary>
    /// Thống kê kết quả khảo sát: số phiếu trả lời, tỉ lệ phản hồi; với từng câu trắc nghiệm là số lượt chọn
    /// và tỉ lệ của mỗi tùy chọn; với câu tự luận là danh sách câu trả lời. Xuất được ra file CSV (mở bằng Excel).
    /// </summary>
    public partial class ThongKeKhaoSatForm : CrmDialog
    {
        private const int DlgWidth = 880;
        private readonly KhaoSatService _service = new KhaoSatService();
        private readonly string _maKhaoSat;
        private KhaoSatKetQuaDto _data;

        private readonly Panel _kpi = new Panel();
        private readonly Panel _scroll = new Panel { AutoScroll = true, BackColor = Color.White };
        private readonly Label _status = new Label();
        private readonly Button _btnCsv;
        private readonly Button _btnLamMoi;

        public ThongKeKhaoSatForm(string maKhaoSat, string tieuDe)
            : base("Thống kê khảo sát", (tieuDe ?? "") + "  ·  " + maKhaoSat, DlgWidth, 530, null, "Đóng")
        {
            InitializeComponent();
            _maKhaoSat = maKhaoSat;
            const int L = Gutter, W = DlgWidth - Gutter * 2;

            _kpi.SetBounds(L, 4, W, 84);
            _kpi.BackColor = Color.White;
            SkinUtil.EnableDoubleBuffer(_kpi);
            _kpi.Paint += PaintKpi;
            Body.Controls.Add(_kpi);

            _scroll.SetBounds(L - 4, 100, W + 8, 426);
            Body.Controls.Add(_scroll);
            _scroll.Resize += (s, e) => LayoutCards();

            _status.SetBounds(L, 100, W, 120);
            _status.TextAlign = ContentAlignment.MiddleCenter;
            _status.ForeColor = ThemeManager.Muted;
            _status.UseMnemonic = false;
            _status.Text = "Đang tải kết quả...";
            Body.Controls.Add(_status);
            _status.BringToFront();

            _btnCsv = AddLeftButton("Xuất CSV", ButtonSkin.Kind.Secondary);
            _btnLamMoi = AddLeftButton("Làm mới", ButtonSkin.Kind.Ghost);
            _btnCsv.Enabled = false;
            _btnCsv.Click += (s, e) => XuatCsv();
            _btnLamMoi.Click += async (s, e) => await LoadAsync();

            Shown += async (s, e) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            _btnLamMoi.Enabled = false;
            _status.Visible = true;
            _status.ForeColor = ThemeManager.Muted;
            _status.Text = "Đang tải kết quả...";
            try
            {
                _data = await _service.GetResultsAsync(_maKhaoSat);
                if (IsDisposed) return;
                Render();
            }
            catch (ApiException ex)
            {
                if (IsDisposed) return;
                _status.ForeColor = ThemeManager.Red;
                _status.Text = ex.Message;
            }
            finally
            {
                if (!IsDisposed) _btnLamMoi.Enabled = true;
            }
        }

        private void Render()
        {
            _scroll.SuspendLayout();
            var old = _scroll.Controls.Cast<Control>().ToArray();
            _scroll.Controls.Clear();
            foreach (var c in old) c.Dispose();

            var list = _data?.ChiTiet ?? new List<CauHoiKetQuaDto>();
            int stt = 1;
            foreach (var ch in list) _scroll.Controls.Add(new KetQuaCard(ch, stt++));

            _status.Visible = list.Count == 0;
            if (list.Count == 0) { _status.ForeColor = ThemeManager.Muted; _status.Text = "Khảo sát chưa có câu hỏi nào."; }
            _btnCsv.Enabled = list.Count > 0;
            LayoutCards();
            _scroll.ResumeLayout();
            _kpi.Invalidate();
        }

        private void LayoutCards()
        {
            // Luôn chừa chỗ thanh cuộn dọc để không phát sinh thanh cuộn ngang
            int w = _scroll.Width - SystemInformation.VerticalScrollBarWidth - 10;
            int y = 4 + _scroll.AutoScrollPosition.Y;
            foreach (Control c in _scroll.Controls)
            {
                var k = c as KetQuaCard;
                if (k != null) k.LayoutFor(w);
                c.Location = new Point(4, y);
                y += c.Height + 12;
            }
        }

        private void PaintKpi(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var d = _data;
            int phieu = d?.TongSoNguoiTraLoi ?? 0;
            int soCau = d?.ChiTiet?.Count ?? 0;
            int moi = d?.SoDoiTuong ?? 0;
            string tiLe = moi > 0 ? Math.Min(100.0, phieu * 100.0 / moi).ToString("0.#") + "%" : "—";
            var tiles = new[]
            {
                Tuple.Create("Phiếu trả lời", d == null ? "—" : phieu.ToString("N0"), ThemeManager.Primary),
                Tuple.Create("Số câu hỏi", d == null ? "—" : soCau.ToString("N0"), ThemeManager.Violet),
                Tuple.Create("Khách được mời", d == null ? "—" : (moi > 0 ? moi.ToString("N0") : "Tất cả khách"), ThemeManager.Teal),
                Tuple.Create("Tỉ lệ phản hồi", d == null ? "—" : tiLe, ThemeManager.Amber),
            };
            int gap = 12, tw = (_kpi.Width - gap * 3) / 4;
            for (int i = 0; i < 4; i++)
            {
                var r = new Rectangle(i * (tw + gap), 0, tw - 1, _kpi.Height - 1);
                using (var path = SkinUtil.Round(r, 12))
                {
                    using (var br = new SolidBrush(ThemeManager.Surface2)) g.FillPath(br, path);
                }
                using (var br = new SolidBrush(tiles[i].Item3)) g.FillEllipse(br, r.X + 16, r.Y + 20, 8, 8);
                using (var f = new Font(ThemeManager.FontFamily, 8.5F))
                    TextRenderer.DrawText(g, tiles[i].Item1, f, new Point(r.X + 30, r.Y + 15), ThemeManager.Muted, TextFormatFlags.NoPrefix);
                using (var f = new Font(ThemeManager.FontFamily, 17F, FontStyle.Bold))
                    TextRenderer.DrawText(g, tiles[i].Item2, f, new Rectangle(r.X + 14, r.Y + 36, r.Width - 20, 38), ThemeManager.Ink,
                        TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
            }
        }

        // ── Xuất CSV ─────────────────────────────────────────────────────────

        private void XuatCsv()
        {
            if (_data == null) return;
            using (var dlg = new SaveFileDialog
            {
                Filter = "CSV (mở bằng Excel)|*.csv",
                FileName = "ThongKe_" + _maKhaoSat + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv"
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(Csv("Khảo sát") + "," + Csv(_data.TieuDe) + "," + Csv(_data.MaKhaoSat));
                    sb.AppendLine(Csv("Tổng phiếu trả lời") + "," + _data.TongSoNguoiTraLoi);
                    sb.AppendLine();
                    sb.AppendLine("STT,Câu hỏi,Loại,Lượt trả lời câu,Tùy chọn / Câu trả lời,Số lượt chọn,Tỉ lệ (%),Khách hàng,Ngày trả lời");
                    int stt = 1;
                    foreach (var ch in _data.ChiTiet)
                    {
                        string head = stt + "," + Csv(ch.NoiDungCauHoi) + "," + Csv(ch.LaTracNghiem ? "Trắc nghiệm" : "Tự luận") + "," + ch.SoLuotTraLoi;
                        if (ch.LaTracNghiem)
                        {
                            var opts = ch.KetQuaTN ?? new List<TuyChonKetQuaDto>();
                            if (opts.Count == 0) sb.AppendLine(head + ",,,,,");
                            foreach (var t in opts)
                                sb.AppendLine(head + "," + Csv(t.NoiDungTuyChon) + "," + t.SoNguoiChon + "," + t.TyLe.ToString("0.##", CultureInfo.InvariantCulture) + ",,");
                        }
                        else
                        {
                            var ans = ch.TraLoiTuLuan ?? new List<TraLoiTuLuanDto>();
                            if (ans.Count == 0) sb.AppendLine(head + ",,,,,");
                            foreach (var a in ans)
                                sb.AppendLine(head + "," + Csv(a.NoiDung) + ",,," + Csv(a.TenKhachHang) + "," + (a.NgayTraLoi.HasValue ? a.NgayTraLoi.Value.ToString("dd/MM/yyyy HH:mm") : ""));
                        }
                        stt++;
                    }
                    File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));   // có BOM để Excel đọc đúng tiếng Việt
                    Notify.Success(this, "Đã xuất thống kê ra:\n" + dlg.FileName);
                }
                catch (Exception ex)
                {
                    Notify.Error(this, "Không ghi được file: " + ex.Message);
                }
            }
        }

        private static string Csv(string s)
        {
            if (s == null) return "";
            s = s.Replace("\r", " ").Replace("\n", " ");
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }
    }

    /// <summary>Thẻ kết quả một câu hỏi (tự vẽ): thanh tỉ lệ cho trắc nghiệm, danh sách trích dẫn cho tự luận.</summary>
    internal class KetQuaCard : Control
    {
        private const int Pad = 18, RowH = 34, MaxAnswers = 30;
        private static readonly TextFormatFlags Wrap = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding;
        private readonly CauHoiKetQuaDto _q;
        private readonly int _stt;
        private int _titleH;
        private readonly List<int> _answerH = new List<int>();

        public KetQuaCard(CauHoiKetQuaDto q, int stt)
        {
            _q = q; _stt = stt;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
            Font = new Font(ThemeManager.FontFamily, 9.5F);
        }

        private Font TitleFont { get { return new Font(ThemeManager.FontFamily, 10.5F, FontStyle.Bold); } }

        private List<TraLoiTuLuanDto> Answers { get { return (_q.TraLoiTuLuan ?? new List<TraLoiTuLuanDto>()).Take(MaxAnswers).ToList(); } }

        public void LayoutFor(int width)
        {
            int inner = width - Pad * 2;
            int h = Pad + 20;   // dòng "Câu n · loại · x lượt"
            using (var g = CreateGraphics())
            using (var tf = TitleFont)
            {
                _titleH = TextRenderer.MeasureText(g, _q.NoiDungCauHoi ?? "", tf, new Size(inner, int.MaxValue), Wrap).Height;
                h += _titleH + 14;
                _answerH.Clear();
                if (_q.LaTracNghiem)
                {
                    int n = (_q.KetQuaTN ?? new List<TuyChonKetQuaDto>()).Count;
                    h += Math.Max(1, n) * RowH;
                }
                else
                {
                    var ans = Answers;
                    if (ans.Count == 0) h += 30;
                    foreach (var a in ans)
                    {
                        int ah = TextRenderer.MeasureText(g, "“" + (a.NoiDung ?? "") + "”", Font, new Size(inner - 28, int.MaxValue), Wrap).Height + 22 + 16;
                        _answerH.Add(ah);
                        h += ah + 8;
                    }
                    if ((_q.TraLoiTuLuan?.Count ?? 0) > MaxAnswers) h += 26;
                }
            }
            Size = new Size(width, h + Pad - 4);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var card = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = SkinUtil.Round(card, 12))
            using (var pen = new Pen(ThemeManager.Line)) g.DrawPath(pen, path);

            int x = Pad, inner = Width - Pad * 2, y = Pad;
            const TextFormatFlags One = TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding;
            using (var fSmall = new Font(ThemeManager.FontFamily, 8.5F, FontStyle.Bold))
            {
                string head = "CÂU " + _stt + " · " + (_q.LaTracNghiem ? "TRẮC NGHIỆM" : "TỰ LUẬN");
                TextRenderer.DrawText(g, head, fSmall, new Point(x, y), ThemeManager.Primary, One);
                string luot = _q.SoLuotTraLoi.ToString("N0") + " lượt trả lời";
                var sz = TextRenderer.MeasureText(g, luot, fSmall, Size.Empty, TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, luot, fSmall, new Point(x + inner - sz.Width, y), ThemeManager.Muted, One);
            }
            y += 20;
            using (var tf = TitleFont)
                TextRenderer.DrawText(g, _q.NoiDungCauHoi ?? "", tf, new Rectangle(x, y, inner, _titleH), ThemeManager.Ink, Wrap);
            y += _titleH + 14;

            if (_q.LaTracNghiem) PaintOptions(g, x, y, inner);
            else PaintAnswers(g, x, y, inner);
        }

        private void PaintOptions(Graphics g, int x, int y, int inner)
        {
            var opts = _q.KetQuaTN ?? new List<TuyChonKetQuaDto>();
            if (opts.Count == 0)
            {
                TextRenderer.DrawText(g, "Câu hỏi chưa có tùy chọn.", Font, new Point(x, y + 8), ThemeManager.Muted, TextFormatFlags.NoPrefix);
                return;
            }
            int max = opts.Max(o => o.SoNguoiChon);
            int labelW = Math.Min(260, inner * 36 / 100), valueW = 110;
            int barX = x + labelW + 12, barW = inner - labelW - 12 - valueW - 8;
            using (var fVal = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold))
            using (var fPct = new Font(ThemeManager.FontFamily, 9F))
            {
                foreach (var o in opts)
                {
                    bool top = max > 0 && o.SoNguoiChon == max;
                    TextRenderer.DrawText(g, o.NoiDungTuyChon ?? "", Font, new Rectangle(x, y, labelW, RowH - 6), ThemeManager.Ink2,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
                    CrmPaint.Bar(g, new Rectangle(barX, y + (RowH - 6) / 2 - 5, Math.Max(20, barW), 10), o.TyLe / 100.0,
                        top ? ThemeManager.Primary : ThemeManager.Blend(ThemeManager.Primary, Color.White, 0.45f), ThemeManager.Surface2);
                    string v = o.SoNguoiChon.ToString("N0");
                    string p = "  " + o.TyLe.ToString("0.#") + "%";
                    var vs = TextRenderer.MeasureText(g, v, fVal, Size.Empty, TextFormatFlags.NoPadding);
                    var ps = TextRenderer.MeasureText(g, p, fPct, Size.Empty, TextFormatFlags.NoPadding);
                    int vx = x + inner - vs.Width - ps.Width;
                    TextRenderer.DrawText(g, v, fVal, new Rectangle(vx, y, vs.Width + 2, RowH - 6), ThemeManager.Ink, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(g, p, fPct, new Rectangle(vx + vs.Width, y, ps.Width + 2, RowH - 6), ThemeManager.Muted, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    y += RowH;
                }
            }
        }

        private void PaintAnswers(Graphics g, int x, int y, int inner)
        {
            var ans = Answers;
            if (ans.Count == 0)
            {
                TextRenderer.DrawText(g, "Chưa có câu trả lời nào.", Font, new Point(x, y + 4), ThemeManager.Muted, TextFormatFlags.NoPrefix);
                return;
            }
            using (var fMeta = new Font(ThemeManager.FontFamily, 8.5F))
            {
                for (int i = 0; i < ans.Count; i++)
                {
                    var a = ans[i];
                    int h = _answerH[i];
                    var r = new Rectangle(x, y, inner, h);
                    using (var path = SkinUtil.Round(r, 8))
                    using (var br = new SolidBrush(ThemeManager.Surface2)) g.FillPath(br, path);
                    using (var br = new SolidBrush(ThemeManager.Violet)) g.FillRectangle(br, x, y + 8, 3, h - 16);
                    TextRenderer.DrawText(g, "“" + (a.NoiDung ?? "") + "”", Font, new Rectangle(x + 16, y + 8, inner - 28, h - 30), ThemeManager.Ink, Wrap);
                    string meta = "— " + (string.IsNullOrWhiteSpace(a.TenKhachHang) ? "Khách ẩn danh" : a.TenKhachHang)
                                  + (a.NgayTraLoi.HasValue ? " · " + a.NgayTraLoi.Value.ToString("dd/MM/yyyy") : "");
                    TextRenderer.DrawText(g, meta, fMeta, new Rectangle(x + 16, y + h - 24, inner - 28, 18), ThemeManager.Muted,
                        TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
                    y += h + 8;
                }
                int con = (_q.TraLoiTuLuan?.Count ?? 0) - ans.Count;
                if (con > 0)
                    TextRenderer.DrawText(g, "và " + con + " câu trả lời khác — dùng \"Xuất CSV\" để xem đầy đủ.", fMeta, new Point(x, y + 4), ThemeManager.Muted, TextFormatFlags.NoPrefix);
            }
        }
    }
}
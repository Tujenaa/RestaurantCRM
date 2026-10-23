using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.DTOs.KhaoSat;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.KhaoSat
{
    /// <summary>
    /// Xem trước khảo sát như khách hàng sẽ thấy: tiêu đề, từng câu hỏi với lựa chọn (trắc nghiệm) hoặc ô nhập (tự luận).
    /// Có thể thử chọn / nhập và bấm "Gửi" để kiểm tra — câu trả lời không được lưu.
    /// </summary>
    public class KhaoSatXemTruocDialog : CrmDialog
    {
        private const int DlgWidth = 700;
        private readonly Panel _scroll = new Panel { AutoScroll = true, BackColor = Color.White };
        private readonly List<Func<bool>> _daTraLoi = new List<Func<bool>>();
        private readonly List<Panel> _cards = new List<Panel>();

        public KhaoSatXemTruocDialog(KhaoSatChiTietDto ks)
            : base("Xem trước khảo sát", "Giao diện khách hàng nhìn thấy. Câu trả lời ở đây không được lưu.", DlgWidth, 540, null, "Đóng")
        {
            const int L = Gutter, W = DlgWidth - Gutter * 2;
            _scroll.SetBounds(L - 6, 0, W + 12, 540);
            Body.Padding = new Padding(0);
            Body.Controls.Add(_scroll);

            int cw = W - 4;   // chừa chỗ cho thanh cuộn
            int y = 8;

            // Tiêu đề khảo sát
            var title = new Label
            {
                Text = string.IsNullOrWhiteSpace(ks.TieuDe) ? "(Chưa có tiêu đề)" : ks.TieuDe,
                UseMnemonic = false,
                AutoSize = true,
                MaximumSize = new Size(cw - 20, 0),
                Location = new Point(6, y),
                Font = new Font(ThemeManager.FontFamily, 15F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink
            };
            _scroll.Controls.Add(title);
            y += title.PreferredHeight + 6;
            int soCau = ks.CauHoi.Count;
            var meta = new Label
            {
                Text = soCau + " câu hỏi · khoảng " + Math.Max(1, (int)Math.Ceiling(soCau * 0.5)) + " phút · Trạng thái: " + ks.TrangThaiHienThi,
                AutoSize = true,
                Location = new Point(6, y),
                UseMnemonic = false,
                Font = new Font(ThemeManager.FontFamily, 9F),
                ForeColor = ThemeManager.Muted
            };
            _scroll.Controls.Add(meta);
            y += 30;

            if (soCau == 0)
            {
                _scroll.Controls.Add(new Label
                {
                    Text = "Khảo sát chưa có câu hỏi nào. Hãy thêm câu hỏi trước khi mở cho khách hàng.",
                    AutoSize = false,
                    Size = new Size(cw - 12, 60),
                    Location = new Point(6, y),
                    UseMnemonic = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = ThemeManager.Surface2,
                    ForeColor = ThemeManager.Ink2
                });
                return;
            }

            int stt = 1;
            foreach (var c in ks.CauHoi)
            {
                var card = BuildCard(c, stt++, cw - 12);
                card.Location = new Point(6, y);
                _scroll.Controls.Add(card);
                _cards.Add(card);
                y += card.Height + 12;
            }

            var btnGui = MakeButton("Gửi khảo sát", ButtonSkin.Kind.Primary, 140);
            btnGui.Location = new Point(6, y + 4);
            btnGui.Margin = new Padding(0);
            _scroll.Controls.Add(btnGui);
            var hint = new Label
            {
                Text = "(Bản xem trước — chỉ kiểm tra câu bỏ trống, không gửi đi)",
                AutoSize = true,
                UseMnemonic = false,
                Location = new Point(6 + 156, y + 14),
                Font = new Font(ThemeManager.FontFamily, 8.5F),
                ForeColor = ThemeManager.Muted
            };
            _scroll.Controls.Add(hint);
            _scroll.Controls.Add(new Panel { Location = new Point(0, y + 60), Size = new Size(1, 1) });   // lề dưới khi cuộn
            btnGui.Click += (s, e) => ThuGui();
            Shown += (s, e) => { if (CancelBtn != null) CancelBtn.Focus(); };
        }

        private Panel BuildCard(CauHoiDto c, int stt, int width)
        {
            var card = new Panel { Width = width, BackColor = Color.White };
            int y = 14;
            var lblSo = new Label
            {
                Text = "Câu " + stt + " · " + KhaoSatConst.TenLoai(c.LoaiCauHoi),
                AutoSize = true,
                Location = new Point(16, y),
                UseMnemonic = false,
                Font = new Font(ThemeManager.FontFamily, 8.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Primary,
                BackColor = Color.White
            };
            card.Controls.Add(lblSo);
            y += 22;
            var lblHoi = new Label
            {
                Text = c.NoiDungCauHoi + " *",
                AutoSize = true,
                MaximumSize = new Size(width - 32, 0),
                Location = new Point(16, y),
                UseMnemonic = false,
                Font = new Font(ThemeManager.FontFamily, 10.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                BackColor = Color.White
            };
            card.Controls.Add(lblHoi);
            y += lblHoi.PreferredHeight + 10;

            if (c.LaTracNghiem)
            {
                var radios = new List<RadioButton>();
                if (c.TuyChon.Count == 0)
                {
                    card.Controls.Add(new Label
                    {
                        Text = "(Câu trắc nghiệm này chưa có tùy chọn)",
                        AutoSize = true,
                        Location = new Point(18, y),
                        UseMnemonic = false,
                        ForeColor = ThemeManager.Red,
                        BackColor = Color.White
                    });
                    y += 26;
                }
                foreach (var t in c.TuyChon)
                {
                    var rb = new RadioButton
                    {
                        Text = t.NoiDungTuyChon,
                        AutoSize = true,
                        MaximumSize = new Size(width - 40, 0),
                        Location = new Point(18, y),
                        Font = new Font(ThemeManager.FontFamily, 9.5F),
                        ForeColor = ThemeManager.Ink,
                        BackColor = Color.White,
                        UseMnemonic = false,
                        Cursor = Cursors.Hand
                    };
                    card.Controls.Add(rb);
                    radios.Add(rb);
                    y += Math.Max(28, rb.PreferredSize.Height + 8);
                }
                _daTraLoi.Add(() => radios.Count == 0 || radios.Any(r => r.Checked));
            }
            else
            {
                var box = new TextBox
                {
                    Multiline = true,
                    Location = new Point(18, y),
                    Size = new Size(width - 36, 64),
                    ScrollBars = ScrollBars.Vertical,
                    Font = new Font(ThemeManager.FontFamily, 9.5F),
                    BorderStyle = BorderStyle.FixedSingle
                };
                card.Controls.Add(box);
                UiStyler.SetPlaceholder(box, "Nhập câu trả lời của bạn...");
                y += 64 + 8;
                _daTraLoi.Add(() => box.Text.Trim().Length > 0);
            }

            card.Height = y + 8;
            card.Tag = false;   // true = đang đánh dấu thiếu câu trả lời
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                bool thieu = (bool)card.Tag;
                var r = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                using (var path = SkinUtil.Round(r, 12))
                using (var pen = new Pen(thieu ? ThemeManager.Red : ThemeManager.Line, thieu ? 1.5f : 1f))
                    g.DrawPath(pen, path);
            };
            return card;
        }

        private void ThuGui()
        {
            int thieu = 0;
            Panel dau = null;
            for (int i = 0; i < _cards.Count; i++)
            {
                bool ok = _daTraLoi[i]();
                _cards[i].Tag = !ok;
                _cards[i].Invalidate();
                if (!ok) { thieu++; if (dau == null) dau = _cards[i]; }
            }
            if (thieu > 0)
            {
                _scroll.ScrollControlIntoView(dau);
                Notify.Warn(this, "Còn " + thieu + " câu hỏi chưa trả lời. Khách hàng sẽ thấy thông báo này khi gửi thiếu.", "Xem trước: kiểm tra câu trả lời");
            }
            else
                Notify.Info(this, "Đã trả lời đủ các câu hỏi.\nĐây là bản xem trước nên câu trả lời không được gửi đi.", "Xem trước");
        }
    }
}
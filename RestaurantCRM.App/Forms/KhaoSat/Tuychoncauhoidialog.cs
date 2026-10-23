using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.KhaoSat
{
    /// <summary>Sửa nội dung một tùy chọn trả lời (không trống, tối đa 255 ký tự, không trùng tùy chọn khác).</summary>
    public class TuyChonCauHoiDialog : CrmDialog
    {
        private readonly TextBox _txt = new TextBox { MaxLength = 255 };
        private readonly Label _lblError = new Label();
        private readonly List<string> _others;

        /// <summary>Nội dung sau khi người dùng bấm Lưu.</summary>
        public string Value { get; private set; }

        public TuyChonCauHoiDialog(string current, IEnumerable<string> others, int index)
            : base("Sửa tùy chọn", "Tùy chọn " + (index >= 0 && index < 26 ? ((char)('A' + index)).ToString() : (index + 1).ToString()) + " của câu hỏi", 480, 120, "Lưu")
        {
            _others = (others ?? Enumerable.Empty<string>()).Select(o => (o ?? "").Trim()).ToList();
            const int L = Gutter, W = 480 - Gutter * 2;
            Field(Body, "Nội dung tùy chọn", _txt, L, 4, W, true);
            _txt.Text = current ?? "";

            _lblError.SetBounds(L, 74, W, 36);
            _lblError.ForeColor = ThemeManager.Red;
            _lblError.Font = new Font(ThemeManager.FontFamily, 9F);
            _lblError.UseMnemonic = false;
            Body.Controls.Add(_lblError);

            _txt.TextChanged += (s, e) => _lblError.Text = "";
            PrimaryButton.Click += (s, e) => Luu();
            AcceptButton = PrimaryButton;
            Shown += (s, e) => { _txt.Focus(); _txt.SelectAll(); };
        }

        private void Luu()
        {
            string v = _txt.Text.Trim();
            if (v.Length == 0) { _lblError.Text = "Nội dung tùy chọn không được để trống."; _txt.Focus(); return; }
            if (_others.Any(o => string.Equals(o, v, StringComparison.CurrentCultureIgnoreCase)))
            {
                _lblError.Text = "Tùy chọn này trùng với một tùy chọn khác của câu hỏi.";
                _txt.SelectAll(); _txt.Focus();
                return;
            }
            Value = v;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
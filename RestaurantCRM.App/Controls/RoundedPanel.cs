using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    /// <summary>Panel bo góc, khử răng cưa, hỗ trợ nền gradient và viền mảnh.</summary>
    public class RoundedPanel : Panel
    {
        private int _radius = 14;
        private Color _fillColor = ThemeManager.Paper;
        private Color _borderColor = ThemeManager.Line;
        private Color _gradientEnd = Color.Empty;
        private float _gradientAngle = 0f;
        private int _borderWidth = 1;

        public RoundedPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        [DefaultValue(14)] public int Radius { get { return _radius; } set { _radius = value; Invalidate(); } }
        public Color FillColor { get { return _fillColor; } set { _fillColor = value; Invalidate(); } }
        public Color BorderColor { get { return _borderColor; } set { _borderColor = value; Invalidate(); } }
        public Color GradientEnd { get { return _gradientEnd; } set { _gradientEnd = value; Invalidate(); } }
        public float GradientAngle { get { return _gradientAngle; } set { _gradientAngle = value; Invalidate(); } }
        [DefaultValue(1)] public int BorderWidth { get { return _borderWidth; } set { _borderWidth = value; Invalidate(); } }

        private int _shadow = 0;
        /// <summary>Độ lan bóng đổ mềm (px). Khung thẻ sẽ thu vào trong để chừa chỗ vẽ bóng.</summary>
        [DefaultValue(0)] public int Shadow { get { return _shadow; } set { _shadow = value; Invalidate(); } }

        /// <summary>Vùng thẻ thực tế (đã trừ phần bóng đổ).</summary>
        public Rectangle CardRect
        {
            get
            {
                int s = _shadow;
                return new Rectangle(s, s / 2, Width - 1 - s * 2, Height - 1 - s / 2 - s * 2);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = CardRect;
            if (rect.Width <= 2 || rect.Height <= 2) return;
            if (_shadow > 0) ThemeManager.DrawSoftShadow(g, rect, _radius, _shadow);

            using (var path = ThemeManager.RoundedRect(rect, _radius))
            {
                if (_gradientEnd != Color.Empty)
                {
                    using (var brush = new LinearGradientBrush(rect, _fillColor, _gradientEnd, _gradientAngle))
                        g.FillPath(brush, path);
                }
                else
                {
                    using (var brush = new SolidBrush(_fillColor)) g.FillPath(brush, path);
                }
                if (_borderWidth > 0 && _borderColor != Color.Empty && _borderColor != Color.Transparent)
                {
                    using (var pen = new Pen(_borderColor, _borderWidth)) g.DrawPath(pen, path);
                }
            }
            base.OnPaint(e);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;
using RestaurantCRM.AdminApp.Services.BanHang;

namespace RestaurantCRM.AdminApp.Controls.BanHang
{
    /// <summary>
    /// Chuyên trách dựng giao diện và cập nhật hiển thị cho cột Thực đơn bên trái:
    /// - Thanh tìm kiếm và bộ lọc loại món / còn hàng
    /// - Thanh danh mục dạng nút bấm chuyển nhanh (Pill tabs)
    /// - Lưới thẻ món ăn và trạng thái tồn kho tức thời
    /// </summary>
    public class MenuPanelBuilder
    {
        private readonly Color _accent = ThemeManager.Primary;
        private readonly Font _baseFont;

        private FlowLayoutPanel _productsPanel;
        private TextBox _txtSearch;
        private ComboBox _cboCategory;
        private CheckBox _chkOnlyInStock;

        public event Action<MonAnDto> DishSelected;
        public event Action FilterChanged;

        public bool IsSearchFocused => _txtSearch?.Focused ?? false;
        public string SearchQuery => (_txtSearch?.Text == "Tìm món..." ? "" : _txtSearch?.Text)?.Trim()?.ToLowerInvariant() ?? "";
        public string SelectedCategory => _cboCategory?.SelectedItem?.ToString() ?? "Tất cả loại món";
        public bool OnlyInStock => _chkOnlyInStock?.Checked ?? false;

        public MenuPanelBuilder(Font baseFont)
        {
            _baseFont = baseFont ?? new Font("Segoe UI", 9.5F);
        }

        public Control BuildMenuColumn(Label lblNhanVien)
        {
            var menuPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(12, 10, 10, 10)
            };

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 84,
                BackColor = ThemeManager.Bg
            };

            var header = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = ThemeManager.Bg };
            var lblTitle = new Label
            {
                Text = "Thực đơn món ăn",
                Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(0, 4)
            };
            header.Controls.Add(lblTitle);

            if (lblNhanVien != null)
            {
                lblNhanVien.Location = new Point(190, 8);
                header.Controls.Add(lblNhanVien);
            }

            var filterBar = BuildFilterBar();

            topPanel.Controls.Add(filterBar);
            topPanel.Controls.Add(header);
            header.BringToFront();

            menuPanel.Controls.Add(topPanel);

            _productsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ThemeManager.Bg,
                Padding = new Padding(0, 4, 0, 0)
            };
            menuPanel.Controls.Add(_productsPanel);
            _productsPanel.BringToFront();

            return menuPanel;
        }

        private Control BuildFilterBar()
        {
            var bar = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = ThemeManager.Bg };

            var pnlSearch = new RoundedPanel
            {
                Size = new Size(240, 36),
                Location = new Point(0, 2),
                Radius = 8,
                BorderWidth = 1,
                BorderColor = ThemeManager.Line,
                FillColor = Color.White
            };

            var lblSearchIcon = new Label
            {
                Text = ThemeManager.Glyph.Search,
                Font = ThemeManager.Icon(9.5F),
                ForeColor = ThemeManager.Muted,
                Location = new Point(8, 9),
                Size = new Size(18, 18),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlSearch.Controls.Add(lblSearchIcon);

            _txtSearch = new TextBox
            {
                Text = "Tìm món...",
                ForeColor = ThemeManager.Muted,
                Font = _baseFont,
                Location = new Point(32, 8),
                Width = 200,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White
            };
            _txtSearch.GotFocus += (s, e) =>
            {
                if (_txtSearch.Text == "Tìm món...")
                {
                    _txtSearch.Text = "";
                    _txtSearch.ForeColor = ThemeManager.Ink;
                }
            };
            _txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtSearch.Text))
                {
                    _txtSearch.Text = "Tìm món...";
                    _txtSearch.ForeColor = ThemeManager.Muted;
                }
            };
            _txtSearch.TextChanged += (s, e) => FilterChanged?.Invoke();
            pnlSearch.Controls.Add(_txtSearch);
            bar.Controls.Add(pnlSearch);

            _cboCategory = new ComboBox
            {
                Location = new Point(248, 4),
                Size = new Size(160, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = _baseFont
            };
            _cboCategory.Items.Add("Tất cả loại món");
            _cboCategory.SelectedIndex = 0;
            _cboCategory.SelectedIndexChanged += (s, e) => FilterChanged?.Invoke();
            bar.Controls.Add(_cboCategory);

            _chkOnlyInStock = new CheckBox
            {
                Text = "Chỉ món còn hàng",
                Location = new Point(416, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.Ink2,
                BackColor = Color.Transparent,
                Checked = false,
                Cursor = Cursors.Hand
            };
            _chkOnlyInStock.CheckedChanged += (s, e) => FilterChanged?.Invoke();
            bar.Controls.Add(_chkOnlyInStock);

            return bar;
        }

        public void PopulateCategories(IEnumerable<LoaiMonDto> categories)
        {
            if (_cboCategory == null) return;
            string prev = _cboCategory.SelectedItem?.ToString();
            _cboCategory.Items.Clear();
            _cboCategory.Items.Add("Tất cả loại món");

            if (categories != null)
            {
                foreach (var c in categories)
                    _cboCategory.Items.Add(c.TenLoaiMon);
            }

            if (prev != null && _cboCategory.Items.Contains(prev))
                _cboCategory.SelectedItem = prev;
            else
                _cboCategory.SelectedIndex = 0;
        }

        public void RenderProducts(
            IEnumerable<MonAnDto> allMonAn,
            IEnumerable<KmMonDto> kmMonList,
            CartService cartService,
            OrderCalculationService calcService)
        {
            if (_productsPanel == null) return;

            Point scrollPos = _productsPanel.AutoScrollPosition;
            _productsPanel.SuspendLayout();
            var old = _productsPanel.Controls.Cast<Control>().ToList();
            _productsPanel.Controls.Clear();
            foreach (var c in old) c.Dispose();

            if (allMonAn == null || !allMonAn.Any())
            {
                _productsPanel.Controls.Add(new Label
                {
                    Text = "Không có món ăn nào trong thực đơn.",
                    ForeColor = ThemeManager.Muted,
                    AutoSize = true,
                    Margin = new Padding(12)
                });
                _productsPanel.ResumeLayout();
                return;
            }

            string q = SearchQuery;
            string selCat = SelectedCategory;
            bool onlyStock = OnlyInStock;

            var filtered = allMonAn.Where(mon =>
            {
                bool matchName = string.IsNullOrEmpty(q) ||
                                 (mon.TenMonAn != null && mon.TenMonAn.ToLowerInvariant().Contains(q)) ||
                                 (mon.MaMonAn != null && mon.MaMonAn.ToLowerInvariant().Contains(q));
                bool matchCat = selCat == "Tất cả loại món" || mon.TenLoaiMon == selCat;
                bool matchStock = !onlyStock || mon.SoLuongTon > 0;
                return matchName && matchCat && matchStock;
            }).ToList();

            if (!filtered.Any())
            {
                _productsPanel.Controls.Add(new Label
                {
                    Text = "Không có món ăn nào phù hợp với bộ lọc.",
                    ForeColor = ThemeManager.Muted,
                    AutoSize = true,
                    Margin = new Padding(12)
                });
                _productsPanel.ResumeLayout();
                return;
            }

            foreach (var mon in filtered)
            {
                var card = CreateDishCard(mon, kmMonList, cartService, calcService);
                _productsPanel.Controls.Add(card);
            }

            _productsPanel.ResumeLayout(true);
            _productsPanel.PerformLayout();
            try { _productsPanel.AutoScrollPosition = scrollPos; } catch { }
        }

        public void UpdateProductsInPlace(
            IEnumerable<MonAnDto> allMonAn,
            IEnumerable<KmMonDto> kmMonList,
            CartService cartService,
            OrderCalculationService calcService)
        {
            if (_productsPanel == null) return;
            var currentCards = _productsPanel.Controls.OfType<RoundedPanel>().ToList();
            if (!currentCards.Any())
            {
                RenderProducts(allMonAn, kmMonList, cartService, calcService);
                return;
            }

            string query = SearchQuery;
            string selCat = SelectedCategory;
            bool onlyStock = OnlyInStock;

            var monAnList = allMonAn?.ToList() ?? new List<MonAnDto>();

            foreach (var card in currentCards)
            {
                string maMon = card.Tag as string;
                if (string.IsNullOrEmpty(maMon)) continue;

                var mon = monAnList.FirstOrDefault(m => m.MaMonAn == maMon);
                if (mon == null)
                {
                    card.Visible = false;
                    continue;
                }

                bool matchName = string.IsNullOrEmpty(query) ||
                                 (mon.TenMonAn != null && mon.TenMonAn.ToLowerInvariant().Contains(query)) ||
                                 (mon.MaMonAn != null && mon.MaMonAn.ToLowerInvariant().Contains(query));
                bool matchCat = selCat == "Tất cả loại món" || mon.TenLoaiMon == selCat;
                bool matchStock = !onlyStock || mon.SoLuongTon > 0;
                bool shouldShow = matchName && matchCat && matchStock;

                if (!shouldShow)
                {
                    card.Visible = false;
                    continue;
                }
                card.Visible = true;

                int inCartQty = cartService.GetQuantityInCart(mon.MaMonAn);
                int virtualRemaining = Math.Max(0, mon.SoLuongTon - inCartQty);
                bool canAddMore = virtualRemaining > 0;
                bool rawInStock = mon.SoLuongTon > 0;

                var (effPrice, promoText) = calcService.GetDishPriceInfo(mon, kmMonList);
                bool hasPromo = promoText != null;

                card.BorderColor = rawInStock ? (hasPromo ? Color.FromArgb(254, 202, 202) : ThemeManager.Line) : Color.FromArgb(241, 245, 249);
                card.FillColor = rawInStock ? Color.White : Color.FromArgb(248, 250, 252);
                card.Cursor = canAddMore ? Cursors.Hand : Cursors.Default;

                var lblStock = card.Controls["lblStock"] as Label;
                if (lblStock != null)
                {
                    if (!rawInStock)
                    {
                        lblStock.Text = "● Hết món";
                        lblStock.ForeColor = ThemeManager.Red;
                        lblStock.BackColor = ThemeManager.RedSoft;
                    }
                    else if (virtualRemaining == 0)
                    {
                        lblStock.Text = inCartQty > 0 ? $"● Hết suất (Đã lấy {inCartQty})" : "● Hết món";
                        lblStock.ForeColor = ThemeManager.Red;
                        lblStock.BackColor = ThemeManager.RedSoft;
                    }
                    else if (virtualRemaining <= 5)
                    {
                        lblStock.Text = inCartQty > 0 ? $"● Còn: {virtualRemaining} (giỏ: {inCartQty})" : $"● Còn: {virtualRemaining}";
                        lblStock.ForeColor = ThemeManager.Amber;
                        lblStock.BackColor = ThemeManager.AmberSoft;
                    }
                    else
                    {
                        lblStock.Text = inCartQty > 0 ? $"● Còn: {virtualRemaining} (giỏ: {inCartQty})" : $"● Còn: {virtualRemaining}";
                        lblStock.ForeColor = ThemeManager.Green;
                        lblStock.BackColor = ThemeManager.GreenSoft;
                    }
                }

                var btnAdd = card.Controls["btnAdd"] as Button;
                if (btnAdd != null)
                {
                    if (canAddMore)
                    {
                        btnAdd.Text = "+ Thêm";
                        btnAdd.BackColor = _accent;
                        btnAdd.ForeColor = Color.White;
                        btnAdd.Cursor = Cursors.Hand;
                        btnAdd.Enabled = true;
                    }
                    else
                    {
                        btnAdd.Text = "Hết";
                        btnAdd.BackColor = Color.FromArgb(241, 245, 249);
                        btnAdd.ForeColor = ThemeManager.Muted;
                        btnAdd.Cursor = Cursors.Default;
                        btnAdd.Enabled = false;
                    }
                }
            }
        }

        private Control CreateDishCard(
            MonAnDto mon,
            IEnumerable<KmMonDto> kmMonList,
            CartService cartService,
            OrderCalculationService calcService)
        {
            int inCartQty = cartService.GetQuantityInCart(mon.MaMonAn);
            int virtualRemaining = Math.Max(0, mon.SoLuongTon - inCartQty);
            bool canAddMore = virtualRemaining > 0;
            bool rawInStock = mon.SoLuongTon > 0;

            var (effPrice, promoText) = calcService.GetDishPriceInfo(mon, kmMonList);
            bool hasPromo = promoText != null;

            var card = new RoundedPanel
            {
                Name = "card_" + mon.MaMonAn,
                Tag = mon.MaMonAn,
                Size = new Size(182, 114),
                Margin = new Padding(6),
                Radius = 10,
                BorderWidth = 1,
                BorderColor = rawInStock ? (hasPromo ? Color.FromArgb(254, 202, 202) : ThemeManager.Line) : Color.FromArgb(241, 245, 249),
                FillColor = rawInStock ? Color.White : Color.FromArgb(248, 250, 252),
                Cursor = canAddMore ? Cursors.Hand : Cursors.Default
            };

            var lblTen = new Label
            {
                Name = "lblTen",
                Text = mon.TenMonAn,
                Location = new Point(8, 6),
                Size = new Size(hasPromo ? 110 : 162, 34),
                AutoEllipsis = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = rawInStock ? ThemeManager.Ink : ThemeManager.Muted,
                BackColor = Color.Transparent,
                Cursor = canAddMore ? Cursors.Hand : Cursors.Default
            };
            card.Controls.Add(lblTen);

            if (hasPromo)
            {
                var lblSaleBadge = new Label
                {
                    Text = $"🔥 {promoText}",
                    Location = new Point(116, 6),
                    Size = new Size(58, 20),
                    Font = new Font("Segoe UI", 7F, FontStyle.Bold),
                    ForeColor = ThemeManager.Red,
                    BackColor = ThemeManager.RedSoft,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                card.Controls.Add(lblSaleBadge);
            }

            var lblGia = new Label
            {
                Name = "lblGia",
                Text = $"{effPrice:N0} đ",
                Location = new Point(8, 44),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = rawInStock ? (hasPromo ? ThemeManager.Red : _accent) : ThemeManager.Muted,
                BackColor = Color.Transparent,
                Cursor = canAddMore ? Cursors.Hand : Cursors.Default
            };
            card.Controls.Add(lblGia);

            if (hasPromo && effPrice < mon.Gia)
            {
                var lblGiaGoc = new Label
                {
                    Text = $"{mon.Gia:N0} đ",
                    Location = new Point(lblGia.PreferredWidth + 12, 46),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 8F, FontStyle.Strikeout),
                    ForeColor = ThemeManager.Muted,
                    BackColor = Color.Transparent
                };
                card.Controls.Add(lblGiaGoc);
            }

            var lblStock = new Label
            {
                Name = "lblStock",
                Location = new Point(8, 72),
                AutoSize = true,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Padding = new Padding(3, 2, 3, 2)
            };

            if (!rawInStock)
            {
                lblStock.Text = "● Hết món";
                lblStock.ForeColor = ThemeManager.Red;
                lblStock.BackColor = ThemeManager.RedSoft;
            }
            else if (virtualRemaining == 0)
            {
                lblStock.Text = inCartQty > 0 ? $"● Hết suất (Đã lấy {inCartQty})" : "● Hết món";
                lblStock.ForeColor = ThemeManager.Red;
                lblStock.BackColor = ThemeManager.RedSoft;
            }
            else if (virtualRemaining <= 5)
            {
                lblStock.Text = inCartQty > 0 ? $"● Còn: {virtualRemaining} (giỏ: {inCartQty})" : $"● Còn: {virtualRemaining}";
                lblStock.ForeColor = ThemeManager.Amber;
                lblStock.BackColor = ThemeManager.AmberSoft;
            }
            else
            {
                lblStock.Text = inCartQty > 0 ? $"● Còn: {virtualRemaining} (giỏ: {inCartQty})" : $"● Còn: {virtualRemaining}";
                lblStock.ForeColor = ThemeManager.Green;
                lblStock.BackColor = ThemeManager.GreenSoft;
            }
            card.Controls.Add(lblStock);

            var btnAdd = new Button
            {
                Name = "btnAdd",
                Location = new Point(112, 68),
                Size = new Size(60, 30),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            btnAdd.FlatAppearance.BorderSize = 0;

            if (canAddMore)
            {
                btnAdd.Text = "+ Thêm";
                btnAdd.BackColor = _accent;
                btnAdd.ForeColor = Color.White;
                btnAdd.Cursor = Cursors.Hand;
                btnAdd.Click += (s, e) => DishSelected?.Invoke(mon);
            }
            else
            {
                btnAdd.Text = "Hết";
                btnAdd.BackColor = Color.FromArgb(241, 245, 249);
                btnAdd.ForeColor = ThemeManager.Muted;
                btnAdd.Cursor = Cursors.Default;
                btnAdd.Enabled = false;
            }
            card.Controls.Add(btnAdd);

            Action handleCardClick = () =>
            {
                if (canAddMore) DishSelected?.Invoke(mon);
            };
            card.Click += (s, e) => handleCardClick();
            lblTen.Click += (s, e) => handleCardClick();
            lblGia.Click += (s, e) => handleCardClick();

            return card;
        }
    }
}

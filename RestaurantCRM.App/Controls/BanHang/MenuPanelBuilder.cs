using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
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
        private ComboBox _cboPriceFilter;
        private CheckBox _chkOnlyInStock;

        // "Tìm theo": mặc định tên món, có thể chọn mã món
        private readonly SearchScopes<MonAnDto> _scopes = new SearchScopes<MonAnDto>("BanHang.Menu")
            .Add("Tên món", "Tên", "Tìm theo tên món...", m => new[] { m.TenMonAn })
            .Add("Mã món", "Mã", "Tìm theo mã món...", m => new[] { m.MaMonAn });

        public event Action<MonAnDto> DishSelected;
        public event Action FilterChanged;

        public bool IsSearchFocused => _txtSearch?.Focused ?? false;
        public string SearchQuery => _txtSearch?.Text?.Trim()?.ToLowerInvariant() ?? "";
        public string SelectedCategory => _cboCategory?.SelectedItem?.ToString() ?? "Tất cả loại món";
        public string SelectedPriceFilter => _cboPriceFilter?.SelectedItem?.ToString() ?? "Tất cả mức giá";
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
                Padding = new Padding(10, 6, 8, 8)
            };

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = ThemeManager.Bg
            };

            var header = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = ThemeManager.Bg };
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
            var bar = new Panel { Dock = DockStyle.Top, Height = 38, BackColor = ThemeManager.Bg };

            var pnlSearch = new RoundedPanel
            {
                Size = new Size(205, 32),
                Location = new Point(0, 2),
                Radius = 8,
                BorderWidth = 1,
                BorderColor = ThemeManager.Line,
                FillColor = Color.White
            };

            var lblClear = new Label
            {
                Text = "✕",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Muted,
                Location = new Point(182, 8),
                Size = new Size(18, 18),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Visible = false
            };
            lblClear.Click += (s, e) =>
            {
                _txtSearch.Text = "";
                _txtSearch.Focus();
            };
            pnlSearch.Controls.Add(lblClear);

            // Nút "Tìm theo" nhỏ ở đầu ô (Tên ▾ / Mã ▾)
            var lblScope = new Label
            {
                Font = new Font(ThemeManager.FontFamily, 8.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Primary,
                Location = new Point(6, 7),
                Size = new Size(42, 18),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand,
                UseMnemonic = false
            };
            var scopeLine = new Panel { BackColor = ThemeManager.Line, Location = new Point(48, 8), Size = new Size(1, 16) };
            pnlSearch.Controls.Add(lblScope);
            pnlSearch.Controls.Add(scopeLine);

            _txtSearch = new TextBox
            {
                ForeColor = ThemeManager.Ink,
                Font = _baseFont,
                Location = new Point(54, 8),
                Width = 126,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White
            };
            Action refreshScope = () =>
            {
                lblScope.Text = _scopes.ShortNames[_scopes.SelectedIndex] + " ▾";
                UiStyler.SetPlaceholder(_txtSearch, _scopes.Placeholders[_scopes.SelectedIndex]);
            };
            refreshScope();
            lblScope.Click += (s, e) => PageKit.ShowScopeMenu(lblScope, new Point(0, lblScope.Height + 8), _scopes, _txtSearch);
            _scopes.Changed += (s, e) => { refreshScope(); FilterChanged?.Invoke(); };
            _txtSearch.TextChanged += (s, e) =>
            {
                lblClear.Visible = _txtSearch.TextLength > 0;
                FilterChanged?.Invoke();
            };
            pnlSearch.Controls.Add(_txtSearch);
            bar.Controls.Add(pnlSearch);

            _cboCategory = new ComboBox
            {
                Location = new Point(211, 4),
                Size = new Size(146, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = _baseFont
            };
            _cboCategory.Items.Add("Tất cả loại món");
            _cboCategory.SelectedIndex = 0;
            _cboCategory.SelectedIndexChanged += (s, e) => FilterChanged?.Invoke();
            bar.Controls.Add(_cboCategory);

            _cboPriceFilter = new ComboBox
            {
                Location = new Point(363, 4),
                Size = new Size(142, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = _baseFont
            };
            _cboPriceFilter.Items.AddRange(new object[]
            {
                "Tất cả mức giá",
                "Đang giảm giá",
                "Dưới 100.000 đ",
                "100.000 - 250.000 đ",
                "Trên 250.000 đ"
            });
            _cboPriceFilter.SelectedIndex = 0;
            _cboPriceFilter.SelectedIndexChanged += (s, e) => FilterChanged?.Invoke();
            bar.Controls.Add(_cboPriceFilter);

            _chkOnlyInStock = new CheckBox
            {
                Text = "Còn hàng",
                Location = new Point(513, 8),
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
            CartService cartService,
            OrderCalculationService calcService,
            IEnumerable<VoucherDto> vouchers = null)
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
            string selPrice = SelectedPriceFilter;
            bool onlyStock = OnlyInStock;

            var filtered = allMonAn.Where(mon =>
            {
                bool matchName = MatchesSearchQuery(mon, q);
                bool matchCat = selCat == "Tất cả loại món" || mon.TenLoaiMon == selCat;
                bool matchStock = !onlyStock || mon.SoLuongTon > 0;
                bool matchPrice = MatchesPriceFilter(mon, selPrice, calcService, vouchers);
                return matchName && matchCat && matchStock && matchPrice;
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
                var card = CreateDishCard(mon, cartService, calcService, vouchers);
                _productsPanel.Controls.Add(card);
            }

            _productsPanel.ResumeLayout(true);
            _productsPanel.PerformLayout();
            try { _productsPanel.AutoScrollPosition = scrollPos; } catch { }
        }

        public void UpdateProductsInPlace(
            IEnumerable<MonAnDto> allMonAn,
            CartService cartService,
            OrderCalculationService calcService,
            IEnumerable<VoucherDto> vouchers = null)
        {
            if (_productsPanel == null) return;
            var currentCards = _productsPanel.Controls.OfType<RoundedPanel>().ToList();
            if (!currentCards.Any())
            {
                RenderProducts(allMonAn, cartService, calcService, vouchers);
                return;
            }

            string query = SearchQuery;
            string selCat = SelectedCategory;
            string selPrice = SelectedPriceFilter;
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

                bool matchName = MatchesSearchQuery(mon, query);
                bool matchCat = selCat == "Tất cả loại món" || mon.TenLoaiMon == selCat;
                bool matchStock = !onlyStock || mon.SoLuongTon > 0;
                bool matchPrice = MatchesPriceFilter(mon, selPrice, calcService, vouchers);
                bool shouldShow = matchName && matchCat && matchStock && matchPrice;

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

                var (effPrice, promoText, _, _) = calcService.GetDishPriceInfo(mon, vouchers);
                bool hasPromo = promoText != null;

                card.BorderColor = rawInStock ? (hasPromo ? Color.FromArgb(254, 202, 202) : ThemeManager.Line) : Color.FromArgb(241, 245, 249);
                card.FillColor = rawInStock ? Color.White : Color.FromArgb(248, 250, 252);
                card.Cursor = canAddMore ? Cursors.Hand : Cursors.Default;

                var lblTen = card.Controls["lblTen"] as Label;
                if (lblTen != null)
                {
                    lblTen.Size = new Size(hasPromo ? 132 : 190, 34);
                    lblTen.ForeColor = rawInStock ? ThemeManager.Ink : ThemeManager.Muted;
                    lblTen.Cursor = canAddMore ? Cursors.Hand : Cursors.Default;
                }

                var lblSaleBadge = card.Controls["lblSaleBadge"] as Label;
                if (lblSaleBadge != null)
                {
                    lblSaleBadge.Text = hasPromo ? promoText : "";
                    lblSaleBadge.Visible = hasPromo;
                }

                var lblGia = card.Controls["lblGia"] as Label;
                if (lblGia != null)
                {
                    lblGia.Text = $"{effPrice:N0} đ";
                    lblGia.ForeColor = rawInStock ? (hasPromo ? ThemeManager.Red : _accent) : ThemeManager.Muted;
                    lblGia.Cursor = canAddMore ? Cursors.Hand : Cursors.Default;
                }

                var lblGiaGoc = card.Controls["lblGiaGoc"] as Label;
                if (lblGiaGoc != null)
                {
                    bool showGiaGoc = hasPromo && effPrice < mon.Gia;
                    lblGiaGoc.Text = $"{mon.Gia:N0} đ";
                    if (lblGia != null)
                    {
                        lblGiaGoc.Location = new Point(lblGia.PreferredWidth + 12, 46);
                    }
                    lblGiaGoc.Visible = showGiaGoc;
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

                var lblStock = card.Controls["lblStock"] as Label;
                if (lblStock != null)
                {
                    ApplyStockBadgeStyle(lblStock, btnAdd, rawInStock, virtualRemaining, inCartQty);
                }
            }
        }

        private Control CreateDishCard(
            MonAnDto mon,
            CartService cartService,
            OrderCalculationService calcService,
            IEnumerable<VoucherDto> vouchers = null)
        {
            int inCartQty = cartService.GetQuantityInCart(mon.MaMonAn);
            int virtualRemaining = Math.Max(0, mon.SoLuongTon - inCartQty);
            bool canAddMore = virtualRemaining > 0;
            bool rawInStock = mon.SoLuongTon > 0;

            var (effPrice, promoText, _, _) = calcService.GetDishPriceInfo(mon, vouchers);
            bool hasPromo = promoText != null;

            var card = new RoundedPanel
            {
                Name = "card_" + mon.MaMonAn,
                Tag = mon.MaMonAn,
                Size = new Size(206, 116),
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
                Size = new Size(hasPromo ? 132 : 190, 34),
                AutoEllipsis = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = rawInStock ? ThemeManager.Ink : ThemeManager.Muted,
                BackColor = Color.Transparent,
                Cursor = canAddMore ? Cursors.Hand : Cursors.Default
            };
            card.Controls.Add(lblTen);

            var lblSaleBadge = new Label
            {
                Name = "lblSaleBadge",
                Text = hasPromo ? promoText : "",
                Location = new Point(140, 6),
                Size = new Size(58, 20),
                Font = new Font("Segoe UI", 7F, FontStyle.Bold),
                ForeColor = ThemeManager.Red,
                BackColor = ThemeManager.RedSoft,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = hasPromo
            };
            card.Controls.Add(lblSaleBadge);

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

            var lblGiaGoc = new Label
            {
                Name = "lblGiaGoc",
                Text = $"{mon.Gia:N0} đ",
                Location = new Point(lblGia.PreferredWidth + 12, 46),
                AutoSize = true,
                Font = new Font("Segoe UI", 8F, FontStyle.Strikeout),
                ForeColor = ThemeManager.Muted,
                BackColor = Color.Transparent,
                Visible = hasPromo && effPrice < mon.Gia
            };
            card.Controls.Add(lblGiaGoc);

            var btnAdd = new Button
            {
                Name = "btnAdd",
                Location = new Point(138, 70),
                Size = new Size(58, 30),
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

            var lblStock = new Label
            {
                Name = "lblStock",
                Location = new Point(8, 74),
                AutoSize = true,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                Padding = new Padding(3, 2, 3, 2)
            };
            ApplyStockBadgeStyle(lblStock, btnAdd, rawInStock, virtualRemaining, inCartQty);
            card.Controls.Add(lblStock);

            Action handleCardClick = () =>
            {
                if (canAddMore) DishSelected?.Invoke(mon);
            };
            card.Click += (s, e) => handleCardClick();
            lblTen.Click += (s, e) => handleCardClick();
            lblGia.Click += (s, e) => handleCardClick();

            return card;
        }

        private static void ApplyStockBadgeStyle(
            Label lblStock,
            Button btnAdd,
            bool rawInStock,
            int virtualRemaining,
            int inCartQty)
        {
            if (lblStock == null) return;

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

            int btnLeft = btnAdd != null ? btnAdd.Left : 138;
            int btnTop = btnAdd != null ? btnAdd.Top : 70;
            int btnHeight = btnAdd != null ? btnAdd.Height : 30;
            int maxWidth = Math.Max(60, btnLeft - lblStock.Left - 6);
            lblStock.MaximumSize = new Size(maxWidth, 0);
            lblStock.Top = Math.Max(64, btnTop + (btnHeight - lblStock.PreferredHeight) / 2);
        }

        private static bool MatchesPriceFilter(
            MonAnDto mon,
            string selPrice,
            OrderCalculationService calcService,
            IEnumerable<VoucherDto> vouchers)
        {
            if (mon == null || string.IsNullOrEmpty(selPrice) || selPrice == "Tất cả mức giá")
                return true;

            var (effPrice, promoText, _, _) = calcService != null
                ? calcService.GetDishPriceInfo(mon, vouchers)
                : (mon.Gia, (string)null, 0m, (string)null);

            if (selPrice == "Đang giảm giá") return promoText != null;
            if (selPrice == "Dưới 100.000 đ") return effPrice < 100000m;
            if (selPrice == "100.000 - 250.000 đ") return effPrice >= 100000m && effPrice <= 250000m;
            if (selPrice == "Trên 250.000 đ") return effPrice > 250000m;
            return true;
        }

        private bool MatchesSearchQuery(MonAnDto mon, string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return true;
            if (mon == null) return false;

            // Theo lựa chọn "Tìm theo" (mặc định: tên món)
            return _scopes.Matches(query, mon);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Forms.BanHang
{
    public partial class TaoDonHangForm
    {
        private async Task RefreshStockContinuouslyAsync()
        {
            if (_isRefreshing || IsDisposed) return;
            // Nếu người dùng đang tập trung gõ tìm kiếm, không làm mới để tránh ngắt quãng thao tác
            if (_txtSearch.Focused) return;

            _isRefreshing = true;
            try
            {
                var list = await _monAnService.GetAllAsync();
                if (list == null || !list.Any() || IsDisposed) return;

                var available = list.Where(m => m.TrangThaiRaw != "Discontinued" && m.TrangThaiRaw != "Inactive").ToList();

                // Smart Refresh: Chỉ cập nhật khi tồn kho, giá hoặc trạng thái món thực sự thay đổi
                if (!HasMonAnStockChanged(_allMonAn, available))
                {
                    return; // Dữ liệu không đổi -> giữ nguyên 100% giao diện và thanh cuộn
                }

                _allMonAn = available;

                try
                {
                    _allVouchers = await _khuyenMaiService.GetVoucherListAsync();
                    _allKmMon = await _khuyenMaiService.GetKmMonListAsync();
                }
                catch { }

                bool cartAltered = false;
                string warningMsg = "";

                // Đồng bộ và kiểm tra ràng buộc kho với giỏ hàng
                for (int i = _cart.Count - 1; i >= 0; i--)
                {
                    var item = _cart[i];
                    var found = _allMonAn.FirstOrDefault(m => m.MaMonAn == item.MaMon);
                    int newStock = found != null ? found.SoLuongTon : 0;
                    item.MaxStock = newStock;

                    if (newStock <= 0)
                    {
                        _cart.RemoveAt(i);
                        cartAltered = true;
                        warningMsg += $"\n• Món '{item.Name}' vừa hết hàng trên hệ thống (đã gỡ khỏi giỏ).";
                    }
                    else if (item.Quantity > newStock)
                    {
                        item.Quantity = newStock;
                        cartAltered = true;
                        warningMsg += $"\n• Món '{item.Name}' chỉ còn {newStock} suất trong kho (đã điều chỉnh số lượng).";
                    }
                }

                // Cập nhật lại giao diện thực đơn tại chỗ (KHÔNG xóa controls, giữ nguyên 100% vị trí scroll)
                UpdateProductsInPlace();

                // Cập nhật lại giỏ hàng nếu số lượng bị thay đổi
                if (cartAltered)
                {
                    RenderCart();
                    if (!string.IsNullOrEmpty(warningMsg))
                    {
                        MessageBox.Show(this, "Thông báo đồng bộ tồn kho trong lúc bán:" + warningMsg, "Cập nhật tồn kho", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch
            {
                // Bỏ qua lỗi ngầm
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private bool HasMonAnStockChanged(List<MonAnDto> oldList, List<MonAnDto> newList)
        {
            if (oldList == null || newList == null) return true;
            if (oldList.Count != newList.Count) return true;
            for (int i = 0; i < oldList.Count; i++)
            {
                var a = oldList[i];
                var b = newList[i];
                if (a.MaMonAn != b.MaMonAn ||
                    a.SoLuongTon != b.SoLuongTon ||
                    a.Gia != b.Gia ||
                    a.TrangThaiRaw != b.TrangThaiRaw ||
                    a.TenMonAn != b.TenMonAn)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task InitData()
        {
            try
            {
                // Tải loại món
                _allLoaiMon = await _loaiMonService.GetAllAsync();
                _cboCategory.Items.Clear();
                _cboCategory.Items.Add("Tất cả loại món");
                foreach (var lm in _allLoaiMon)
                {
                    _cboCategory.Items.Add(lm.TenLoaiMon);
                }
                _cboCategory.SelectedIndex = 0;

                // Tải danh sách món ăn thực tế từ API (bao gồm số lượng tồn)
                var list = await _monAnService.GetAllAsync();
                // Lọc bỏ món đã ngừng kinh doanh
                _allMonAn = list.Where(m => m.TrangThaiRaw != "Discontinued" && m.TrangThaiRaw != "Inactive").ToList();

                // Tải danh sách khuyến mãi món và voucher hóa đơn
                _allVouchers = await _khuyenMaiService.GetVoucherListAsync();
                _allKmMon = await _khuyenMaiService.GetKmMonListAsync();

                // Cập nhật lại MaxStock trong giỏ hàng nếu món ăn đã thay đổi số lượng tồn
                foreach (var item in _cart)
                {
                    var found = _allMonAn.FirstOrDefault(m => m.MaMonAn == item.MaMon);
                    if (found != null)
                    {
                        item.MaxStock = found.SoLuongTon;
                    }
                }

                RenderProducts();
                RenderCart();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Không thể tải dữ liệu: " + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Lấy thông tin giá sau giảm của món nếu đang trong chương trình KM sản phẩm
        private (decimal effectivePrice, string promoText) GetDishPriceInfo(MonAnDto mon)
        {
            var promo = _allKmMon.FirstOrDefault(k => k.MaMon == mon.MaMonAn && k.TrangThai == "Đang áp dụng");
            if (promo != null)
            {
                decimal price = (decimal)promo.DonGiaSauGiam;
                return (price, promo.HinhThuc);
            }
            return (mon.Gia, null);
        }

        private void UpdateProductsInPlace()
        {
            var currentCards = _productsPanel.Controls.OfType<RoundedPanel>().ToList();
            if (!currentCards.Any())
            {
                RenderProducts();
                return;
            }

            string query = (_txtSearch.Text == "Tìm món..." ? "" : _txtSearch.Text).Trim().ToLowerInvariant();
            string selectedCatName = _cboCategory.SelectedItem?.ToString() ?? "Tất cả loại món";

            string selectedCatId = "";
            if (selectedCatName != "Tất cả loại món")
            {
                var foundCat = _allLoaiMon.FirstOrDefault(c => c.TenLoaiMon == selectedCatName);
                if (foundCat != null) selectedCatId = foundCat.MaLoaiMon;
            }

            var filtered = _allMonAn.Where(m => {
                bool matchName = string.IsNullOrEmpty(query) || (m.TenMonAn != null && m.TenMonAn.ToLowerInvariant().Contains(query));
                bool matchCat = string.IsNullOrEmpty(selectedCatId) || m.MaLoaiMon == selectedCatId;
                bool matchStock = !_chkOnlyInStock.Checked || m.SoLuongTon > 0;
                return matchName && matchCat && matchStock;
            }).ToList();

            var currentCardIds = new HashSet<string>(currentCards.Select(c => c.Tag as string).Where(id => !string.IsNullOrEmpty(id)));
            var filteredIds = new HashSet<string>(filtered.Select(m => m.MaMonAn));

            // Nếu tập hợp món ăn bị thay đổi (có món mới thêm vào hoặc bị lọc mất) => Gọi RenderProducts()
            if (!currentCardIds.SetEquals(filteredIds))
            {
                RenderProducts();
                return;
            }

            // Cập nhật tại chỗ: Giữ nguyên 100% controls và vị trí thanh cuộn
            _productsPanel.SuspendLayout();
            foreach (var card in currentCards)
            {
                string maMon = card.Tag as string;
                var mon = filtered.FirstOrDefault(m => m.MaMonAn == maMon);
                if (mon != null)
                {
                    UpdateDishCardState(card, mon);
                }
            }
            _productsPanel.ResumeLayout();
        }

        private void UpdateDishCardState(RoundedPanel card, MonAnDto mon)
        {
            int inCartQty = _cart.FirstOrDefault(x => x.MaMon == mon.MaMonAn)?.Quantity ?? 0;
            int virtualRemaining = Math.Max(0, mon.SoLuongTon - inCartQty);
            bool canAddMore = virtualRemaining > 0;
            bool rawInStock = mon.SoLuongTon > 0;

            var (effPrice, promoText) = GetDishPriceInfo(mon);
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
                    btnAdd.Enabled = true;
                    btnAdd.Cursor = Cursors.Hand;
                }
                else
                {
                    btnAdd.Text = inCartQty > 0 ? "Đầy" : "Hết";
                    btnAdd.BackColor = Color.FromArgb(226, 232, 240);
                    btnAdd.ForeColor = ThemeManager.Muted;
                    btnAdd.Enabled = false;
                    btnAdd.Cursor = Cursors.No;
                }
            }
        }

        private void RenderProducts()
        {
            Point scrollPos = new Point(Math.Abs(_productsPanel.AutoScrollPosition.X), Math.Abs(_productsPanel.AutoScrollPosition.Y));
            _productsPanel.SuspendLayout();
            _productsPanel.Controls.Clear();
            string query = (_txtSearch.Text == "Tìm món..." ? "" : _txtSearch.Text).Trim().ToLowerInvariant();
            string selectedCatName = _cboCategory.SelectedItem?.ToString() ?? "Tất cả loại món";

            string selectedCatId = "";
            if (selectedCatName != "Tất cả loại món")
            {
                var foundCat = _allLoaiMon.FirstOrDefault(c => c.TenLoaiMon == selectedCatName);
                if (foundCat != null) selectedCatId = foundCat.MaLoaiMon;
            }

            var filtered = _allMonAn.Where(m => {
                bool matchName = string.IsNullOrEmpty(query) || (m.TenMonAn != null && m.TenMonAn.ToLowerInvariant().Contains(query));
                bool matchCat = string.IsNullOrEmpty(selectedCatId) || m.MaLoaiMon == selectedCatId;
                bool matchStock = !_chkOnlyInStock.Checked || m.SoLuongTon > 0;
                return matchName && matchCat && matchStock;
            }).ToList();

            if (!filtered.Any())
            {
                _productsPanel.Controls.Add(new Label
                {
                    Text = "Không có món ăn nào phù hợp.",
                    ForeColor = ThemeManager.Muted,
                    AutoSize = true,
                    Margin = new Padding(12)
                });
                _productsPanel.ResumeLayout();
                return;
            }

            foreach (var mon in filtered)
            {
                var card = CreateDishCard(mon);
                _productsPanel.Controls.Add(card);
            }
            _productsPanel.ResumeLayout(true);
            _productsPanel.PerformLayout();
            try { _productsPanel.AutoScrollPosition = scrollPos; } catch { }
        }

        private Control CreateDishCard(MonAnDto mon)
        {
            int inCartQty = _cart.FirstOrDefault(x => x.MaMon == mon.MaMonAn)?.Quantity ?? 0;
            int virtualRemaining = Math.Max(0, mon.SoLuongTon - inCartQty);
            bool canAddMore = virtualRemaining > 0;
            bool rawInStock = mon.SoLuongTon > 0;

            var (effPrice, promoText) = GetDishPriceInfo(mon);
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

            // Badge sale nếu món có KM
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

            // Badge hiển thị số lượng còn (ảo: trừ theo số lượng đang có trong giỏ hàng)
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

            // Nút Thêm vào đơn
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
                btnAdd.Click += (s, e) => AddItem(mon);
            }
            else
            {
                btnAdd.Text = inCartQty > 0 ? "Đầy" : "Hết";
                btnAdd.BackColor = Color.FromArgb(226, 232, 240);
                btnAdd.ForeColor = ThemeManager.Muted;
                btnAdd.Enabled = false;
                btnAdd.Cursor = Cursors.No;
            }
            card.Controls.Add(btnAdd);

            Action onCardClick = () => {
                var currentMon = _allMonAn.FirstOrDefault(m => m.MaMonAn == mon.MaMonAn) ?? mon;
                int currentInCartQty = _cart.FirstOrDefault(x => x.MaMon == currentMon.MaMonAn)?.Quantity ?? 0;
                int currentVirtualRemaining = Math.Max(0, currentMon.SoLuongTon - currentInCartQty);
                bool currentCanAddMore = currentVirtualRemaining > 0;
                bool currentRawInStock = currentMon.SoLuongTon > 0;

                if (currentCanAddMore)
                {
                    AddItem(currentMon);
                }
                else if (!currentRawInStock)
                {
                    MessageBox.Show(this,
                        $"Món '{currentMon.TenMonAn}' hiện đã hết hàng trong kho (số lượng còn: 0).",
                        "Hết hàng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show(this,
                        $"Món '{currentMon.TenMonAn}' chỉ còn {currentMon.SoLuongTon} suất trong kho và bạn đã thêm tối đa ({currentInCartQty} suất) vào giỏ hàng.",
                        "Đã đạt số lượng tối đa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            card.Click += (s, e) => onCardClick();
            lblTen.Click += (s, e) => onCardClick();
            lblGia.Click += (s, e) => onCardClick();

            if (canAddMore)
            {
                card.MouseEnter += (s, e) => {
                    card.BorderColor = _accent;
                };
                card.MouseLeave += (s, e) => {
                    card.BorderColor = hasPromo ? Color.FromArgb(254, 202, 202) : ThemeManager.Line;
                };
            }

            return card;
        }
    }
}

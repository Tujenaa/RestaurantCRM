(() => {
  let toastTimer;
  const toast = message => {
    const element = document.getElementById('toast');
    if (!element) return;
    element.textContent = message;
    element.classList.add('show');
    window.clearTimeout(toastTimer);
    toastTimer = window.setTimeout(() => element.classList.remove('show'), 2800);
  };

  document.addEventListener('click', event => {
    const trigger = event.target.closest('[data-toast]');
    if (trigger) toast(trigger.dataset.toast);

    const accountButton = event.target.closest('[data-account]');
    if (accountButton) {
      const tab = accountButton.dataset.account;
      const root = accountButton.closest('[data-account-root]');
      if (root) {
        root.querySelectorAll('[data-account]').forEach(button => button.classList.toggle('active', button === accountButton));
        root.querySelectorAll('[data-account-panel]').forEach(panel => { panel.hidden = panel.dataset.accountPanel !== tab; });
        const url = new URL(window.location.href);
        url.searchParams.set('tab', tab);
        window.history.replaceState({}, '', url);
      }
    }

    const ratingButton = event.target.closest('#ratingStars [data-rating]');
    if (ratingButton) {
      const rating = Number(ratingButton.dataset.rating);
      const input = document.getElementById('DanhGia');
      if (input) input.value = String(rating);
      document.querySelectorAll('#ratingStars [data-rating]').forEach(button => button.classList.toggle('on', Number(button.dataset.rating) <= rating));
      const label = document.getElementById('ratingLabel');
      if (label) label.textContent = rating + '/5 — ' + ['', 'Rất không hài lòng', 'Chưa hài lòng', 'Bình thường', 'Hài lòng', 'Rất hài lòng'][rating];
    }
  });

  document.addEventListener('submit', async event => {
    const form = event.target;
    if (form && (form.classList.contains('add-cart-form') || form.classList.contains('dish-order-form'))) {
      event.preventDefault();
      try {
        const formData = new FormData(form);
        const response = await fetch(form.action, {
          method: form.method,
          body: formData,
          headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        
        if (response.ok) {
           const result = await response.json().catch(() => null);
           toast((result && result.message) ? result.message : 'Đã thêm món vào giỏ hàng.');
           if (result && result.cartCount !== undefined) {
               document.querySelectorAll('.count').forEach(el => el.textContent = result.cartCount);
           }
        } else {
           toast('Có lỗi xảy ra, không thể thêm vào giỏ.');
        }
      } catch (e) {
        toast('Đã thêm món vào giỏ hàng.'); // Fallback if redirect happened
      }
    }
  });
})();

// Global Quick View functions
window.openQuickView = function(dishId) {
    const modal = document.getElementById('quickViewModal');
    const content = document.getElementById('quickViewContent');
    if (!modal || !content) return;

    modal.classList.add('quickview-modal--open');
    modal.setAttribute('aria-hidden', 'false');
    document.body.style.overflow = 'hidden';

    content.innerHTML = `
        <div class="quickview-loading">
            <div class="spinner"></div>
            <p>Đang tải thông tin món ăn...</p>
        </div>
    `;

    fetch(`/Menu/QuickView/${encodeURIComponent(dishId)}`)
        .then(res => {
            if (!res.ok) throw new Error('Không thể tải món ăn');
            return res.text();
        })
        .then(html => {
            content.innerHTML = html;
            window.bindQuickViewActions();
        })
        .catch(err => {
            content.innerHTML = `
                <div class="quickview-error">
                    <p>⚠️ Không thể tải thông tin món lúc này. Vui lòng mở xem chi tiết trực tiếp.</p>
                    <a href="/Menu/ChiTiet/${encodeURIComponent(dishId)}" class="btn btn-primary">Xem trang chi tiết</a>
                </div>
            `;
        });
};

window.closeQuickView = function() {
    const modal = document.getElementById('quickViewModal');
    if (modal) {
        modal.classList.remove('quickview-modal--open');
        modal.setAttribute('aria-hidden', 'true');
    }
    document.body.style.overflow = '';
};

document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') window.closeQuickView();
});

window.bindQuickViewActions = function() {
    const minusBtn = document.getElementById('qvMinusBtn');
    const plusBtn = document.getElementById('qvPlusBtn');
    const qtyInput = document.getElementById('qvQuantityInput');
    const maxQty = parseInt(qtyInput?.getAttribute('max') || '99', 10);

    if (minusBtn && plusBtn && qtyInput) {
        minusBtn.addEventListener('click', () => {
            let val = parseInt(qtyInput.value || '1', 10);
            if (val > 1) qtyInput.value = val - 1;
        });
        plusBtn.addEventListener('click', () => {
            let val = parseInt(qtyInput.value || '1', 10);
            if (val < maxQty) qtyInput.value = val + 1;
        });
    }
};

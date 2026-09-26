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
})();

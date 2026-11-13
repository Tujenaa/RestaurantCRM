// Chấm sao từng món trong form đánh giá đơn hàng (Views/PhanHoi/_ChamMonTrongDon.cshtml).
// - Món chưa chấm riêng: hiển thị mờ, sao đi theo sao chung, nhãn "Theo điểm chung"; khi lưu là "không đánh giá riêng".
// - Bấm sao của món hoặc gõ nhận xét: món thành "chấm riêng", thôi đồng bộ theo sao chung.
// - Nút "Dùng điểm chung": bỏ chấm riêng, xóa nhận xét, sao quay lại theo sao chung.
// Trang gọi window.ChamMon.dongBoTheoDiemChung(soSao) mỗi khi khách đổi sao chung.
(function () {
    'use strict';

    var MAU_BAT = '#f59e0b';
    var MAU_TAT = '#d1d5db';

    function saoChungHienTai() {
        var input = document.getElementById('DanhGia');
        var val = input ? parseInt(input.value, 10) : NaN;
        return val >= 1 && val <= 5 ? val : 5;
    }

    function laChamRieng(item) {
        return item.getAttribute('data-cham-rieng') === 'true';
    }

    function veSao(item, soSao) {
        item.querySelector('.dish-rating-val').value = soSao;
        item.querySelector('.dish-rating-lbl').textContent = soSao + '★';
        item.querySelectorAll('.dish-star-btn').forEach(function (b, idx) {
            var bat = idx < soSao;
            b.style.color = bat ? MAU_BAT : MAU_TAT;
            b.classList.toggle('on', bat);
        });
    }

    function datCheDo(item, chamRieng) {
        item.setAttribute('data-cham-rieng', chamRieng ? 'true' : 'false');
        item.querySelector('.dish-cham-rieng').value = chamRieng ? 'true' : 'false';
        item.querySelector('.dish-mode-tag').textContent = chamRieng ? 'Bạn đã chấm riêng' : 'Theo điểm chung';
        item.querySelector('.dish-use-order').style.display = chamRieng ? 'inline-block' : 'none';
        item.querySelector('.dish-rating-row').style.opacity = chamRieng ? '1' : '0.5';
    }

    function dongBoTheoDiemChung(soSao) {
        document.querySelectorAll('.dish-review').forEach(function (item) {
            if (!laChamRieng(item)) veSao(item, soSao);
        });
    }

    function khoiTao() {
        document.querySelectorAll('.dish-review').forEach(function (item) {
            var comment = item.querySelector('.dish-comment');
            // Món được chấm riêng chỉ vì có nhận xét (chưa bấm sao) → xóa hết nhận xét thì quay lại theo điểm chung
            var daBamSao = laChamRieng(item);

            item.querySelectorAll('.dish-star-btn').forEach(function (btn) {
                btn.addEventListener('click', function () {
                    daBamSao = true;
                    datCheDo(item, true);
                    veSao(item, parseInt(btn.getAttribute('data-rating'), 10));
                });
            });

            if (comment) {
                comment.addEventListener('input', function () {
                    if (comment.value.trim() !== '') {
                        datCheDo(item, true);
                    } else if (!daBamSao) {
                        datCheDo(item, false);
                        veSao(item, saoChungHienTai());
                    }
                });
            }

            item.querySelector('.dish-use-order').addEventListener('click', function () {
                daBamSao = false;
                if (comment) comment.value = '';
                datCheDo(item, false);
                veSao(item, saoChungHienTai());
            });

            // Trạng thái ban đầu (kể cả khi form hiển thị lại sau lỗi)
            var chamRieng = laChamRieng(item) || (comment && comment.value.trim() !== '');
            datCheDo(item, chamRieng);
            if (!chamRieng) veSao(item, saoChungHienTai());
        });
    }

    window.ChamMon = { dongBoTheoDiemChung: dongBoTheoDiemChung };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', khoiTao);
    } else {
        khoiTao();
    }
})();

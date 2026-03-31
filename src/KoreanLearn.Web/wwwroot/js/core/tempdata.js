/* ═══════════════════════════════════════════════
   KoreanLearn — TempData 自動偵測（SweetAlert2 彈跳視窗）
   ═══════════════════════════════════════════════ */

document.addEventListener('DOMContentLoaded', function () {
    var s = document.querySelector('[data-tempdata-success]');
    if (s) showSuccess(s.getAttribute('data-tempdata-success'));

    var e = document.querySelector('[data-tempdata-error]');
    if (e) showError(e.getAttribute('data-tempdata-error'));

    var w = document.querySelector('[data-tempdata-warning]');
    if (w) showWarning(w.getAttribute('data-tempdata-warning'));

    var i = document.querySelector('[data-tempdata-info]');
    if (i) showInfo(i.getAttribute('data-tempdata-info'));
});

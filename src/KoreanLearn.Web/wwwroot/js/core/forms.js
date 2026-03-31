/* ═══════════════════════════════════════════════
   KoreanLearn — 通用表單互動
   ═══════════════════════════════════════════════ */

// ─── 通用確認操作 ───
document.addEventListener('click', function (e) {
    var btn = e.target.closest('[data-confirm]');
    if (!btn) return;
    e.preventDefault();
    e.stopPropagation();

    var message = btn.getAttribute('data-confirm') || '確定要執行此操作嗎？';
    var isDanger = btn.classList.contains('btn-danger') ||
                   btn.classList.contains('btn-outline-danger') ||
                   message.indexOf('刪除') !== -1;

    Swal.fire({
        icon: isDanger ? 'warning' : 'question',
        title: isDanger ? '確定要刪除？' : '操作確認',
        text: message,
        showCancelButton: true,
        confirmButtonColor: isDanger ? '#dc3545' : '#2B3A67',
        cancelButtonColor: '#6c757d',
        confirmButtonText: isDanger ? '確定刪除' : '確定',
        cancelButtonText: '取消'
    }).then(function (result) {
        if (!result.isConfirmed) return;
        var form = btn.closest('form');
        if (form) { form.submit(); return; }
        if (btn.tagName === 'A' && btn.href) window.location.href = btn.href;
    });
});

// ─── 通用刪除確認 ───
document.addEventListener('click', function (e) {
    var btn = e.target.closest('[data-confirm-delete]');
    if (!btn) return;
    e.preventDefault();
    e.stopPropagation();

    var itemName = btn.getAttribute('data-item-name') || '此項目';
    var formId = btn.getAttribute('data-form-id');

    showDeleteConfirm(itemName, function () {
        var form = formId ? document.getElementById(formId) : btn.closest('form');
        if (form) form.submit();
    });
});

// ─── 需要登入提示 ───
document.addEventListener('click', function (e) {
    var btn = e.target.closest('[data-require-auth]');
    if (!btn) return;
    e.preventDefault();
    e.stopPropagation();
    showLoginRequired();
});

// ─── 密碼顯示/隱藏切換 ───
document.addEventListener('click', function (e) {
    var btn = e.target.closest('.kl-toggle-pw');
    if (!btn) return;
    var input = btn.parentElement.querySelector('input');
    if (!input) return;
    var icon = btn.querySelector('i');
    if (input.type === 'password') {
        input.type = 'text';
        icon.className = 'bi bi-eye-slash';
    } else {
        input.type = 'password';
        icon.className = 'bi bi-eye';
    }
});

// ─── 表單送出防重複提交 ───
document.addEventListener('submit', function (e) {
    var form = e.target;
    if (form.getAttribute('data-no-disable')) return;
    if (form.querySelector('[data-confirm]')) return;
    if (form.querySelector('[data-confirm-delete]')) return;

    var submitBtns = form.querySelectorAll('button[type="submit"]');
    submitBtns.forEach(function (btn) {
        btn.disabled = true;
        var originalHtml = btn.innerHTML;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>處理中...';
        setTimeout(function () {
            btn.disabled = false;
            btn.innerHTML = originalHtml;
        }, 3000);
    });
});

// ─── 回到頂端按鈕 ───
(function () {
    var btn = document.getElementById('klBackToTop');
    if (!btn) return;
    window.addEventListener('scroll', function () {
        if (window.scrollY > 300) {
            btn.classList.add('show');
        } else {
            btn.classList.remove('show');
        }
    });
})();

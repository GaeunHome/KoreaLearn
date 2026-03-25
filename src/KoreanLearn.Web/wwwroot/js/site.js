/* ═══════════════════════════════════════════════
   KoreanLearn — 共用 JavaScript（SweetAlert2）
   ═══════════════════════════════════════════════ */

// ─── SweetAlert2 通知封裝 ────────────────────────
window.showSuccess = function (message, title) {
    Swal.fire({ icon: 'success', title: title || '成功', text: message, confirmButtonColor: '#2B3A67' });
};
window.showError = function (message, title) {
    Swal.fire({ icon: 'error', title: title || '錯誤', text: message, confirmButtonColor: '#2B3A67' });
};
window.showWarning = function (message, title) {
    Swal.fire({ icon: 'warning', title: title || '警告', text: message, confirmButtonColor: '#2B3A67' });
};
window.showInfo = function (message, title) {
    Swal.fire({ icon: 'info', title: title || '提示', text: message, confirmButtonColor: '#2B3A67' });
};

// 確認對話框
window.showConfirm = function (message, title, onConfirm) {
    Swal.fire({
        icon: 'question', title: title || '確認', text: message,
        showCancelButton: true, confirmButtonColor: '#2B3A67',
        cancelButtonColor: '#6c757d', confirmButtonText: '確定', cancelButtonText: '取消'
    }).then(function (result) { if (result.isConfirmed && onConfirm) onConfirm(); });
};

// 刪除確認
window.showDeleteConfirm = function (itemName, onConfirm) {
    Swal.fire({
        icon: 'warning',
        title: '確定要刪除？',
        html: '即將刪除「<strong>' + itemName + '</strong>」，此操作無法復原。',
        showCancelButton: true, confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d', confirmButtonText: '確定刪除', cancelButtonText: '取消'
    }).then(function (result) { if (result.isConfirmed && onConfirm) onConfirm(); });
};

// 需要登入
window.showLoginRequired = function () {
    Swal.fire({
        icon: 'info', title: '請先登入',
        text: '您需要登入才能使用此功能。',
        showCancelButton: true, confirmButtonColor: '#2B3A67',
        confirmButtonText: '前往登入', cancelButtonText: '取消'
    }).then(function (result) {
        if (result.isConfirmed) window.location.href = '/Identity/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
    });
};

// Toast（輕量通知，右下角自動消失）
window.showToast = function (message, type) {
    type = type || 'success';
    Swal.fire({
        toast: true, position: 'bottom-end', icon: type,
        title: message, showConfirmButton: false,
        timer: type === 'error' ? 3000 : 2000, timerProgressBar: true
    });
};

// ─── TempData 自動偵測 ───────────────────────────
document.addEventListener('DOMContentLoaded', function () {
    var s = document.querySelector('[data-tempdata-success]');
    if (s) showToast(s.getAttribute('data-tempdata-success'), 'success');
    var e = document.querySelector('[data-tempdata-error]');
    if (e) showToast(e.getAttribute('data-tempdata-error'), 'error');
    var w = document.querySelector('[data-tempdata-warning]');
    if (w) showToast(w.getAttribute('data-tempdata-warning'), 'warning');
    var i = document.querySelector('[data-tempdata-info]');
    if (i) showToast(i.getAttribute('data-tempdata-info'), 'info');
});

// ─── 通用確認操作（data-confirm） ────────────────
document.addEventListener('click', function (e) {
    var btn = e.target.closest('[data-confirm]');
    if (!btn) return;
    e.preventDefault();
    e.stopPropagation();

    var message = btn.getAttribute('data-confirm') || '確定要執行此操作嗎？';
    var isDanger = btn.classList.contains('btn-danger') ||
                   btn.classList.contains('btn-outline-danger') ||
                   message.indexOf('刪除') !== -1;

    var opts = {
        icon: isDanger ? 'warning' : 'question',
        title: isDanger ? '確定要刪除？' : '操作確認',
        text: message,
        showCancelButton: true,
        confirmButtonColor: isDanger ? '#dc3545' : '#2B3A67',
        cancelButtonColor: '#6c757d',
        confirmButtonText: isDanger ? '確定刪除' : '確定',
        cancelButtonText: '取消'
    };

    Swal.fire(opts).then(function (result) {
        if (!result.isConfirmed) return;
        var form = btn.closest('form');
        if (form) { form.submit(); return; }
        if (btn.tagName === 'A' && btn.href) window.location.href = btn.href;
    });
});

// ─── 通用刪除確認（data-confirm-delete） ─────────
document.addEventListener('click', function (e) {
    var btn = e.target.closest('[data-confirm-delete]');
    if (!btn) return;
    e.preventDefault();
    e.stopPropagation();

    var itemName = btn.getAttribute('data-item-name') || '此項目';
    var formId = btn.getAttribute('data-form-id');

    showDeleteConfirm(itemName, function () {
        if (formId) {
            var form = document.getElementById(formId);
            if (form) form.submit();
        } else {
            var form = btn.closest('form');
            if (form) form.submit();
        }
    });
});

// ─── 需要登入（data-require-auth） ───────────────
document.addEventListener('click', function (e) {
    var btn = e.target.closest('[data-require-auth]');
    if (!btn) return;
    e.preventDefault();
    e.stopPropagation();
    showLoginRequired();
});

// ─── 密碼顯示/隱藏切換 ──────────────────────────
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

// ─── 表單送出防重複提交 ─────────────────────────
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

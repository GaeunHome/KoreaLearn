/* ═══════════════════════════════════════════════
   KoreanLearn — SweetAlert2 通知封裝
   ═══════════════════════════════════════════════ */

(function (w) {
    'use strict';
    var PRIMARY = '#2B3A67';

    w.showSuccess = function (message, title) {
        Swal.fire({ icon: 'success', title: title || '成功', text: message, confirmButtonColor: PRIMARY });
    };

    w.showError = function (message, title) {
        Swal.fire({ icon: 'error', title: title || '錯誤', text: message, confirmButtonColor: PRIMARY });
    };

    w.showWarning = function (message, title) {
        Swal.fire({ icon: 'warning', title: title || '警告', text: message, confirmButtonColor: PRIMARY });
    };

    w.showInfo = function (message, title) {
        Swal.fire({ icon: 'info', title: title || '提示', text: message, confirmButtonColor: PRIMARY });
    };

    w.showConfirm = function (message, title, onConfirm) {
        Swal.fire({
            icon: 'question', title: title || '確認', text: message,
            showCancelButton: true, confirmButtonColor: PRIMARY,
            cancelButtonColor: '#6c757d', confirmButtonText: '確定', cancelButtonText: '取消'
        }).then(function (result) { if (result.isConfirmed && onConfirm) onConfirm(); });
    };

    w.showDeleteConfirm = function (itemName, onConfirm) {
        Swal.fire({
            icon: 'warning', title: '確定要刪除？',
            html: '即將刪除「<strong>' + itemName + '</strong>」，此操作無法復原。',
            showCancelButton: true, confirmButtonColor: '#dc3545',
            cancelButtonColor: '#6c757d', confirmButtonText: '確定刪除', cancelButtonText: '取消'
        }).then(function (result) { if (result.isConfirmed && onConfirm) onConfirm(); });
    };

    w.showLoginRequired = function () {
        Swal.fire({
            icon: 'info', title: '請先登入', text: '您需要登入才能使用此功能。',
            showCancelButton: true, confirmButtonColor: PRIMARY,
            confirmButtonText: '前往登入', cancelButtonText: '取消'
        }).then(function (result) {
            if (result.isConfirmed) w.location.href = '/Identity/Account/Login?returnUrl=' + encodeURIComponent(w.location.pathname);
        });
    };

    w.showToast = function (message, type) {
        type = type || 'success';
        Swal.fire({
            toast: true, position: 'bottom-end', icon: type,
            title: message, showConfirmButton: false,
            timer: type === 'error' ? 3000 : 2000, timerProgressBar: true
        });
    };
})(window);

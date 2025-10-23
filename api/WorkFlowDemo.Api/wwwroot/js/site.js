// 通用消息提示函数 - 使用 Toast
function showMessage(text, type = 'info') {
    const config = {
        'success': { icon: 'bi-check-circle-fill', bg: 'success', title: '成功' },
        'error': { icon: 'bi-x-circle-fill', bg: 'danger', title: '错误' },
        'warning': { icon: 'bi-exclamation-triangle-fill', bg: 'warning', title: '警告' },
        'info': { icon: 'bi-info-circle-fill', bg: 'info', title: '提示' }
    }[type] || { icon: 'bi-info-circle-fill', bg: 'info', title: '提示' };
    
    let container = document.querySelector('.toast-container');
    if (!container) {
        container = document.createElement('div');
        container.className = 'toast-container';
        document.body.appendChild(container);
    }
    
    const toastEl = document.createElement('div');
    toastEl.className = 'toast';
    toastEl.setAttribute('role', 'alert');
    toastEl.innerHTML = `
        <div class="toast-header text-white bg-${config.bg}">
            <i class="bi ${config.icon} me-2"></i>
            <strong class="me-auto">${config.title}</strong>
            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast"></button>
        </div>
        <div class="toast-body">${text}</div>
    `;
    
    container.appendChild(toastEl);
    const toast = new bootstrap.Toast(toastEl, { delay: 3000 });
    toast.show();
    
    toastEl.addEventListener('hidden.bs.toast', () => {
        toastEl.remove();
    });
}

// 显示加载状态
function showLoading(show = true) {
    const loading = document.getElementById('loading');
    if (loading) {
        loading.style.display = show ? 'block' : 'none';
    }
}

// 显示空状态
function showEmpty(show = true) {
    const empty = document.getElementById('empty');
    if (empty) {
        empty.style.display = show ? 'block' : 'none';
    }
}

// 格式化日期时间
function formatDateTime(dateString) {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
    });
}

// Bootstrap Modal 辅助函数
function showModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    }
}

function hideModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        const bsModal = bootstrap.Modal.getInstance(modal);
        if (bsModal) {
            bsModal.hide();
        }
    }
}
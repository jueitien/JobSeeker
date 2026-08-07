/* =============================================================
   employer.js — Employer role client-side interactions
   ============================================================= */

(function () {
    'use strict';

    // ── 1) Vacancy Detail Modal ──
    const detailModal = document.getElementById('vacancyDetailModal');
    if (detailModal) {
        detailModal.addEventListener('show.bs.modal', function (event) {
            const btn = event.relatedTarget;
            if (!btn) return;

            document.getElementById('vacancyDetailModalLabel').textContent = btn.getAttribute('data-title') || '—';
            document.getElementById('detailCompany').textContent           = btn.getAttribute('data-company') || '—';
            document.getElementById('detailLocation').textContent          = btn.getAttribute('data-location') || '—';
            document.getElementById('detailSalary').textContent            = btn.getAttribute('data-salary') || 'Not specified';
            document.getElementById('detailDeadline').textContent          = btn.getAttribute('data-deadline') || 'Open';
            document.getElementById('detailPostedBy').textContent          = btn.getAttribute('data-postedby') || '—';
            document.getElementById('detailDescription').textContent       = btn.getAttribute('data-description') || '';
            document.getElementById('detailRequirements').textContent      = btn.getAttribute('data-requirements') || '';

            document.getElementById('detailChips').innerHTML = `
                <span class="badge bg-primary">${btn.getAttribute('data-jobtype') || ''}</span>
                <span class="badge bg-secondary">${btn.getAttribute('data-experience') || ''}</span>
                <span class="badge bg-info text-dark">${btn.getAttribute('data-location') || ''}</span>
            `;
        });
    }

    // ── 2) Sort Dropdown ──
    const sortSelect = document.getElementById('sortSelect');
    const vacancyGrid = document.getElementById('vacancyGrid');

    if (sortSelect && vacancyGrid) {
        sortSelect.addEventListener('change', function () {
            const items = Array.from(vacancyGrid.querySelectorAll('.vacancy-item'));

            items.sort((a, b) => {
                if (this.value === 'newest') {
                    return new Date(b.getAttribute('data-posted')) - new Date(a.getAttribute('data-posted'));
                } else if (this.value === 'deadline') {
                    return new Date(a.getAttribute('data-deadline')) - new Date(b.getAttribute('data-deadline'));
                } else {
                    return a.getAttribute('data-title').toLowerCase()
                            .localeCompare(b.getAttribute('data-title').toLowerCase());
                }
            });

            items.forEach(item => vacancyGrid.appendChild(item));
        });
    }

    // ── 3) Textarea Character Counters ──
    const descField  = document.getElementById('NewVacancy_Description');
    const reqField   = document.getElementById('NewVacancy_Requirements');
    const descCounter = document.getElementById('descCount');
    const reqCounter  = document.getElementById('reqCount');

    if (descField && descCounter) {
        descField.addEventListener('input', function () {
            descCounter.textContent = `${this.value.length} / 5000`;
            descCounter.classList.toggle('text-danger', this.value.length > 5000);
        });
    }

    if (reqField && reqCounter) {
        reqField.addEventListener('input', function () {
            reqCounter.textContent = `${this.value.length} / 3000`;
            reqCounter.classList.toggle('text-danger', this.value.length > 3000);
        });
    }

    // ── 4) Reset form on modal close ──
    const postModal = document.getElementById('postVacancyModal');
    if (postModal) {
        postModal.addEventListener('hidden.bs.modal', function () {
            const form = postModal.querySelector('form');
            if (form) form.reset();
            if (descCounter) descCounter.textContent = '0 / 5000';
            if (reqCounter)  reqCounter.textContent  = '0 / 3000';
        });
    }

})();

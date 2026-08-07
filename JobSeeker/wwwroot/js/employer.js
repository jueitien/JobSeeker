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
            document.getElementById('detailPostedBy').textContent          = btn.getAttribute('data-postedon') || '—';
            document.getElementById('detailDescription').textContent       = btn.getAttribute('data-description') || '';
            document.getElementById('detailRequirements').textContent      = btn.getAttribute('data-requirements') || '';

            document.getElementById('detailChips').innerHTML = `
                <span class="badge bg-primary">${btn.getAttribute('data-employmenttype') || ''}</span>
                <span class="badge bg-secondary">${btn.getAttribute('data-workplacetype') || ''}</span>
                <span class="badge bg-info text-dark">${btn.getAttribute('data-status') || ''}</span>
            `;
        });
    }

    // ── 2) Textarea Character Counters ──
    const descField  = document.getElementById('NewVacancy_JobDescription');
    const reqField   = document.getElementById('NewVacancy_Responsibilities');
    const descCounter = document.getElementById('descCount');
    const reqCounter  = document.getElementById('reqCount');

    if (descField && descCounter) {
        descField.addEventListener('input', function () {
            descCounter.textContent = `${this.value.length} / 4000`;
            descCounter.classList.toggle('text-danger', this.value.length > 4000);
        });
    }

    if (reqField && reqCounter) {
        reqField.addEventListener('input', function () {
            reqCounter.textContent = `${this.value.length} / 4000`;
            reqCounter.classList.toggle('text-danger', this.value.length > 4000);
        });
    }

    // ── 3) Reset form on modal close ──
    const postModal = document.getElementById('postVacancyModal');
    if (postModal) {
        postModal.addEventListener('hidden.bs.modal', function () {
            const form = postModal.querySelector('form');
            if (form) form.reset();
            if (descCounter) descCounter.textContent = '0 / 4000';
            if (reqCounter)  reqCounter.textContent  = '0 / 4000';
        });
    }

})();

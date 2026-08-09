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
            resetSkillRows();
        });
    }

    // ── 4) Skills Requirement — dynamic add/remove rows + live 100% total ──
    const skillRowsContainer = document.getElementById('skillRowsContainer');
    const addSkillRowBtn = document.getElementById('addSkillRowBtn');
    const skillRowTemplate = document.getElementById('skillRowTemplate');
    const skillWeightTotal = document.getElementById('skillWeightTotal');

    function reindexSkillRows() {
        if (!skillRowsContainer) return;
        const rows = skillRowsContainer.querySelectorAll('.skill-requirement-row');
        rows.forEach(function (row, index) {
            row.querySelectorAll('[name]').forEach(function (field) {
                field.name = field.name.replace(/SkillRequirements\[\d+\]/, `SkillRequirements[${index}]`);
            });
        });
    }

    function calculateSkillWeightTotal() {
        if (!skillRowsContainer || !skillWeightTotal) return;
        const weightInputs = skillRowsContainer.querySelectorAll('.skill-weight');
        let total = 0;
        weightInputs.forEach(function (input) {
            total += parseFloat(input.value) || 0;
        });

        // Round to 2 decimals for display to avoid floating point artifacts.
        total = Math.round(total * 100) / 100;
        skillWeightTotal.textContent = `${total}%`;
        skillWeightTotal.classList.remove('text-success', 'text-danger');
        skillWeightTotal.classList.add(total === 100 ? 'text-success' : 'text-danger');
    }

    function resetSkillRows() {
        if (!skillRowsContainer) return;
        // Keep just one empty row after the modal closes/resets.
        const rows = skillRowsContainer.querySelectorAll('.skill-requirement-row');
        rows.forEach(function (row, index) {
            if (index === 0) {
                row.querySelectorAll('select').forEach(sel => sel.selectedIndex = 0);
                row.querySelectorAll('input.skill-weight').forEach(inp => inp.value = '');
            } else {
                row.remove();
            }
        });
        calculateSkillWeightTotal();
    }

    if (addSkillRowBtn && skillRowsContainer && skillRowTemplate) {
        addSkillRowBtn.addEventListener('click', function () {
            const nextIndex = skillRowsContainer.querySelectorAll('.skill-requirement-row').length;
            const fragment = skillRowTemplate.content.cloneNode(true);

            fragment.querySelectorAll('[name]').forEach(function (field) {
                field.name = field.name.replace('__index__', nextIndex);
            });

            skillRowsContainer.appendChild(fragment);
            calculateSkillWeightTotal();
        });
    }

    if (skillRowsContainer) {
        // Recalculate total whenever any weight input changes.
        skillRowsContainer.addEventListener('input', function (event) {
            if (event.target.classList.contains('skill-weight')) {
                calculateSkillWeightTotal();
            }
        });

        // Remove a row (delegated so it works for dynamically added rows too).
        skillRowsContainer.addEventListener('click', function (event) {
            const removeBtn = event.target.closest('.remove-skill-row');
            if (!removeBtn) return;

            const rows = skillRowsContainer.querySelectorAll('.skill-requirement-row');
            if (rows.length <= 1) {
                // Always keep at least one row — just clear it instead of removing.
                const row = removeBtn.closest('.skill-requirement-row');
                row.querySelectorAll('select').forEach(sel => sel.selectedIndex = 0);
                row.querySelectorAll('input.skill-weight').forEach(inp => inp.value = '');
            } else {
                removeBtn.closest('.skill-requirement-row').remove();
                reindexSkillRows();
            }

            calculateSkillWeightTotal();
        });

        // Initial calculation on page load (covers redisplay after validation errors).
        calculateSkillWeightTotal();
    }

    // Client-side guard: block submit with a clear message if weights don't total 100%.
    // (Server-side validation is authoritative; this is just faster feedback.)
    const postVacancyForm = postModal ? postModal.querySelector('form') : null;
    if (postVacancyForm && skillRowsContainer) {
        postVacancyForm.addEventListener('submit', function (event) {
            const weightInputs = skillRowsContainer.querySelectorAll('.skill-weight');
            let total = 0;
            let hasAnySkill = false;

            skillRowsContainer.querySelectorAll('.skill-requirement-row').forEach(function (row) {
                const skillSelect = row.querySelector('.skill-select');
                const weightInput = row.querySelector('.skill-weight');
                if (skillSelect && skillSelect.value !== '0' && skillSelect.value !== '') {
                    hasAnySkill = true;
                    total += parseFloat(weightInput.value) || 0;
                }
            });

            total = Math.round(total * 100) / 100;

            if (!hasAnySkill) {
                event.preventDefault();
                alert('Add at least one skill requirement before submitting.');
                return;
            }

            if (total !== 100) {
                event.preventDefault();
                alert(`The total importance weightage for all skills must equal exactly 100%. Current total: ${total}%.`);
            }
        });
    }

})();

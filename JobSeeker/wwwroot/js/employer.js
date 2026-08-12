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

            const jobId = btn.getAttribute('data-jobid');
            document.getElementById('vacancyDetailModalLabel').textContent = btn.getAttribute('data-title') || '—';
            document.getElementById('detailCompany').textContent           = btn.getAttribute('data-company') || '—';
            document.getElementById('detailLocation').textContent          = btn.getAttribute('data-location') || '—';
            document.getElementById('detailSalary').textContent            = btn.getAttribute('data-salary') || 'Not specified';
            document.getElementById('detailDeadline').textContent          = btn.getAttribute('data-deadline') || 'Open';
            document.getElementById('detailPostedBy').textContent          = btn.getAttribute('data-postedon') || '—';
            document.getElementById('detailDescription').textContent       = btn.getAttribute('data-description') || '';
            document.getElementById('detailRequirements').textContent      = btn.getAttribute('data-requirements') || '';

            // Update delete form with the job ID
            const deleteForm = document.getElementById('deleteVacancyForm');
            if (deleteForm && jobId) {
                deleteForm.innerHTML = '<input type="hidden" name="id" value="' + jobId + '" />';
                // Add the CSRF token if it exists
                const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]');
                if (csrfToken) {
                    const tokenClone = csrfToken.cloneNode(true);
                    deleteForm.appendChild(tokenClone);
                }
            }

            // Store job ID for edit button and add data attribute to trigger edit navigation
            const editBtn = document.getElementById('editVacancyBtn');
            if (editBtn && jobId) {
                editBtn.setAttribute('data-jobid', jobId);
                editBtn.onclick = function() {
                    // Navigate to edit page which will load form in edit mode
                    window.location.href = '/Vacancies/Edit/' + jobId;
                };
            }

            // Handle rejection reason section
            const rejectionReason = btn.getAttribute('data-rejectionreason') || '';
            const rejectionSection = document.getElementById('detailRejectionSection');
            if (rejectionReason.trim()) {
                document.getElementById('detailRejectionReason').textContent = rejectionReason;
                rejectionSection?.classList.remove('d-none');
            } else {
                rejectionSection?.classList.add('d-none');
            }

            document.getElementById('detailChips').innerHTML = `
                <span class="badge bg-primary">${btn.getAttribute('data-employmenttype') || ''}</span>
                <span class="badge bg-secondary">${btn.getAttribute('data-workplacetype') || ''}</span>
                <span class="badge bg-info text-dark">${btn.getAttribute('data-status') || ''}</span>
            `;

            const imagesSection = document.getElementById('detailImagesSection');
            const imagesContainer = document.getElementById('detailImages');
            const imagesRaw = btn.getAttribute('data-images') || '';
            const imageUrls = imagesRaw.split('|').filter(function (url) { return url.trim().length > 0; });

            if (imagesContainer) {
                if (imageUrls.length > 0) {
                    imagesContainer.innerHTML = imageUrls.map(function (url) {
                        return `<a href="${url}" target="_blank" rel="noopener">
                                    <img src="${url}" alt="Vacancy image" class="vac-detail-image" />
                                </a>`;
                    }).join('');
                    imagesSection?.classList.remove('d-none');
                } else {
                    imagesContainer.innerHTML = '';
                    imagesSection?.classList.add('d-none');
                }
            }
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
            resetVacancyImages();
        });
    }

    // ── 3b) Vacancy Images — max 3, accumulated across multiple picks ──
    // Native <input type="file" multiple> REPLACES the current selection
    // every time the picker is opened again, unless the user multi-selects
    // everything in one go. To let employers add images one at a time (or in
    // batches) up to the 3-image limit, we keep our own running list of
    // selected files and rebuild the input's FileList from it before submit.
    const vacancyImagesInput = document.getElementById('vacancyImagesInput');
    const vacancyImagePreview = document.getElementById('vacancyImagePreview');
    const MAX_VACANCY_IMAGES = 3;
    let selectedVacancyImages = [];

    function renderVacancyImagePreviews() {
        if (!vacancyImagePreview) return;
        vacancyImagePreview.innerHTML = '';

        selectedVacancyImages.forEach(function (file, index) {
            const reader = new FileReader();
            reader.onload = function (event) {
                const wrapper = document.createElement('div');
                wrapper.className = 'vac-image-thumb-wrapper';

                const img = document.createElement('img');
                img.src = event.target.result;
                img.alt = file.name;
                img.className = 'vac-image-thumb';

                const removeBtn = document.createElement('button');
                removeBtn.type = 'button';
                removeBtn.className = 'vac-image-thumb-remove';
                removeBtn.title = 'Remove image';
                removeBtn.innerHTML = '&times;';
                removeBtn.addEventListener('click', function () {
                    selectedVacancyImages.splice(index, 1);
                    syncVacancyImagesInput();
                    renderVacancyImagePreviews();
                });

                wrapper.appendChild(img);
                wrapper.appendChild(removeBtn);
                vacancyImagePreview.appendChild(wrapper);
            };
            reader.readAsDataURL(file);
        });
    }

    function syncVacancyImagesInput() {
        if (!vacancyImagesInput) return;
        const dataTransfer = new DataTransfer();
        selectedVacancyImages.forEach(function (file) {
            dataTransfer.items.add(file);
        });
        vacancyImagesInput.files = dataTransfer.files;
    }

    function resetVacancyImages() {
        selectedVacancyImages = [];
        if (vacancyImagePreview) vacancyImagePreview.innerHTML = '';
        if (vacancyImagesInput) vacancyImagesInput.value = '';
    }

    if (vacancyImagesInput && vacancyImagePreview) {
        vacancyImagesInput.addEventListener('change', function () {
            const newlyPicked = Array.from(vacancyImagesInput.files || []);
            if (newlyPicked.length === 0) return;

            let combined = selectedVacancyImages.concat(newlyPicked);

            if (combined.length > MAX_VACANCY_IMAGES) {
                alert(`You can upload a maximum of ${MAX_VACANCY_IMAGES} images. Only the first ${MAX_VACANCY_IMAGES} will be kept.`);
                combined = combined.slice(0, MAX_VACANCY_IMAGES);
            }

            selectedVacancyImages = combined;
            syncVacancyImagesInput();
            renderVacancyImagePreviews();
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

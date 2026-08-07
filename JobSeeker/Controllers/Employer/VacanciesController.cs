using JobSeeker.Data;
using JobSeeker.Models;
using JobSeeker.Models.Employer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Controllers.Employer
{
    [Authorize(Roles = "Employer")]
    public class VacanciesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VacanciesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Vacancies
        public async Task<IActionResult> Index(
            string? searchKeyword,
            string? filterEmploymentType,
            string? filterWorkplaceType,
            string? filterLocation)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            IQueryable<Job> query = _context.Jobs
                .Where(j => j.EmployerId == user.Id);

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                var term = searchKeyword.Trim();
                query = query.Where(j =>
                    j.JobTitle.Contains(term) ||
                    j.CompanyName.Contains(term) ||
                    j.JobDescription.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(filterEmploymentType) && filterEmploymentType != "All")
            {
                query = query.Where(j => j.EmploymentType == filterEmploymentType);
            }

            if (!string.IsNullOrWhiteSpace(filterWorkplaceType) && filterWorkplaceType != "All")
            {
                query = query.Where(j => j.WorkplaceType == filterWorkplaceType);
            }

            if (!string.IsNullOrWhiteSpace(filterLocation))
            {
                query = query.Where(j => j.Location != null && j.Location.Contains(filterLocation));
            }

            var vacancies = await query
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            var viewModel = new VacanciesPageViewModel
            {
                PublishedVacancies = vacancies,
                SearchKeyword = searchKeyword,
                FilterEmploymentType = filterEmploymentType,
                FilterWorkplaceType = filterWorkplaceType,
                FilterLocation = filterLocation
            };

            return View("~/Views/Employer/Vacancies/Index.cshtml", viewModel);
        }

        // POST: /Vacancies/Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Post([Bind(Prefix = "NewVacancy")] VacancyFormViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                var vacancies = await _context.Jobs
                    .Where(j => j.EmployerId == user.Id)
                    .OrderByDescending(j => j.CreatedAt)
                    .ToListAsync();

                var viewModel = new VacanciesPageViewModel
                {
                    NewVacancy = model,
                    PublishedVacancies = vacancies
                };
                return View("~/Views/Employer/Vacancies/Index.cshtml", viewModel);
            }

            var now = DateTime.UtcNow;

            var job = new Job
            {
                EmployerId = user.Id,
                CompanyName = model.CompanyName,
                JobTitle = model.JobTitle,
                JobDescription = model.JobDescription,
                Responsibilities = model.Responsibilities,
                MinimumQualification = model.MinimumQualification,
                PreferredFieldOfStudy = model.PreferredFieldOfStudy,
                MinimumExperienceYears = model.MinimumExperienceYears,
                EmploymentType = model.EmploymentType,
                WorkplaceType = model.WorkplaceType,
                Location = model.Location,
                MinimumSalary = model.MinimumSalary,
                MaximumSalary = model.MaximumSalary,
                VacancyCount = model.VacancyCount,
                ApplicationDeadline = model.ApplicationDeadline,
                ApprovalStatus = "PENDING",   // new postings must be approved by an Administrator
                JobStatus = "OPEN",
                IsTestData = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Job vacancy submitted successfully! It will be visible to job seekers once approved by an administrator.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Vacancies/Close/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(long id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.JobId == id && j.EmployerId == user.Id);

            if (job == null) return NotFound();

            job.JobStatus = "CLOSED";
            job.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"\"{job.JobTitle}\" has been closed.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Vacancies/Reopen/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reopen(long id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.JobId == id && j.EmployerId == user.Id);

            if (job == null) return NotFound();

            job.JobStatus = "OPEN";
            job.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"\"{job.JobTitle}\" has been reopened.";
            return RedirectToAction(nameof(Index));
        }
    }
}

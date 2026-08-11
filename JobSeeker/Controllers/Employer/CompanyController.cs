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
    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompanyController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Company
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var profile = await _context.EmployerProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployerId == user.Id);

            var model = profile == null
                ? new CompanyFormViewModel { IsNew = true }
                : new CompanyFormViewModel
                {
                    CompanyName = profile.CompanyName,
                    CompanyRegistrationNumber = profile.CompanyRegistrationNumber,
                    Industry = profile.Industry,
                    CompanySize = profile.CompanySize,
                    CompanyDescription = profile.CompanyDescription,
                    CompanyWebsite = profile.CompanyWebsite,
                    CompanyAddress = profile.CompanyAddress ?? string.Empty,
                    VerificationStatus = profile.VerificationStatus,
                    VerificationRemarks = profile.VerificationRemarks,
                    UpdatedAt = profile.UpdatedAt,
                    IsNew = false
                };

            return View("~/Views/Employer/Company/Index.cshtml", model);
        }

        // POST: /Company/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(CompanyFormViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Read-only / server-managed fields are not posted by the form.
            ModelState.Remove(nameof(CompanyFormViewModel.VerificationStatus));
            ModelState.Remove(nameof(CompanyFormViewModel.VerificationRemarks));
            ModelState.Remove(nameof(CompanyFormViewModel.UpdatedAt));

            var existing = await _context.EmployerProfiles
                .FirstOrDefaultAsync(e => e.EmployerId == user.Id);

            if (!ModelState.IsValid)
            {
                model.IsNew = existing == null;
                model.VerificationStatus = existing?.VerificationStatus ?? "PENDING";
                model.VerificationRemarks = existing?.VerificationRemarks;
                model.UpdatedAt = existing?.UpdatedAt;
                return View("~/Views/Employer/Company/Index.cshtml", model);
            }

            var now = DateTime.UtcNow;

            var registrationNumber = NullIfBlank(model.CompanyRegistrationNumber);
            var industry = NullIfBlank(model.Industry);
            var companySize = NullIfBlank(model.CompanySize);
            var description = NullIfBlank(model.CompanyDescription);
            var website = NullIfBlank(model.CompanyWebsite);

            if (existing == null)
            {
                _context.EmployerProfiles.Add(new EmployerProfile
                {
                    EmployerId = user.Id,
                    CompanyName = model.CompanyName.Trim(),
                    CompanyRegistrationNumber = registrationNumber,
                    Industry = industry,
                    CompanySize = companySize,
                    CompanyDescription = description,
                    CompanyWebsite = website,
                    CompanyAddress = model.CompanyAddress.Trim(),
                    VerificationStatus = "PENDING",
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            else
            {
                existing.CompanyName = model.CompanyName.Trim();
                existing.CompanyRegistrationNumber = registrationNumber;
                existing.Industry = industry;
                existing.CompanySize = companySize;
                existing.CompanyDescription = description;
                existing.CompanyWebsite = website;
                existing.CompanyAddress = model.CompanyAddress.Trim();
                existing.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Company details saved.";
            return RedirectToAction(nameof(Index));
        }

        private static string? NullIfBlank(string? value)
        {
            var trimmed = value?.Trim();
            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }
    }
}

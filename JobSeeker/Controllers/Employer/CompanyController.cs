using JobSeeker.Data;
using JobSeeker.Models;
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

            var company = await _context.CompanyDetails
                .FirstOrDefaultAsync(c => c.EmployerId == user.Id);

            return View("~/Views/Employer/Company/Index.cshtml", company ?? new CompanyDetail
            {
                EmployerId = user.Id
            });
        }

        // POST: /Company/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(CompanyDetail model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Navigation property / server-managed fields are not posted by the form.
            ModelState.Remove(nameof(CompanyDetail.Employer));
            ModelState.Remove(nameof(CompanyDetail.EmployerId));
            ModelState.Remove(nameof(CompanyDetail.CreatedAt));
            ModelState.Remove(nameof(CompanyDetail.UpdatedAt));

            if (!ModelState.IsValid)
            {
                model.EmployerId = user.Id;
                return View("~/Views/Employer/Company/Index.cshtml", model);
            }

            var now = DateTime.UtcNow;
            var company = await _context.CompanyDetails
                .FirstOrDefaultAsync(c => c.EmployerId == user.Id);

            if (company == null)
            {
                company = new CompanyDetail
                {
                    EmployerId = user.Id,
                    CompanyName = model.CompanyName.Trim(),
                    Address = model.Address.Trim(),
                    CreatedAt = now,
                    UpdatedAt = now
                };
                _context.CompanyDetails.Add(company);
            }
            else
            {
                company.CompanyName = model.CompanyName.Trim();
                company.Address = model.Address.Trim();
                company.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();

            // Keep employer_profiles in sync so admin can verify the employer.
            var profile = await _context.EmployerProfiles
                .FirstOrDefaultAsync(p => p.EmployerId == user.Id);

            if (profile == null)
            {
                _context.EmployerProfiles.Add(new EmployerProfile
                {
                    EmployerId         = user.Id,
                    CompanyName        = company.CompanyName,
                    CompanyAddress     = company.Address,
                    VerificationStatus = "PENDING",
                    CreatedAt          = now,
                    UpdatedAt          = now
                });
            }
            else
            {
                profile.CompanyName    = company.CompanyName;
                profile.CompanyAddress = company.Address;
                profile.UpdatedAt      = now;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Company details saved.";
            return RedirectToAction(nameof(Index));
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobSeeker.Models.Employer;

namespace JobSeeker.Controllers.Employer
{
    [Authorize(Roles = "Employer")]
    public class VacanciesController : Controller
    {
        // GET: /Vacancies
        public IActionResult Index(string? searchKeyword, string? filterJobType, string? filterExperienceLevel, string? filterLocation)
        {
            var sampleVacancies = GetSampleVacancies();

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                sampleVacancies = sampleVacancies.Where(v =>
                    v.Title.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase) ||
                    v.Company.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase) ||
                    v.Description.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(filterJobType) && filterJobType != "All")
                sampleVacancies = sampleVacancies.Where(v => v.JobType == filterJobType).ToList();

            if (!string.IsNullOrWhiteSpace(filterExperienceLevel) && filterExperienceLevel != "All")
                sampleVacancies = sampleVacancies.Where(v => v.ExperienceLevel == filterExperienceLevel).ToList();

            if (!string.IsNullOrWhiteSpace(filterLocation))
                sampleVacancies = sampleVacancies.Where(v =>
                    v.Location.Contains(filterLocation, StringComparison.OrdinalIgnoreCase)
                ).ToList();

            var viewModel = new VacanciesPageViewModel
            {
                PublishedVacancies = sampleVacancies,
                SearchKeyword = searchKeyword,
                FilterJobType = filterJobType,
                FilterExperienceLevel = filterExperienceLevel,
                FilterLocation = filterLocation
            };

            return View("~/Views/Employer/Vacancies/Index.cshtml", viewModel);
        }

        // POST: /Vacancies/Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Post(VacancyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = new VacanciesPageViewModel
                {
                    NewVacancy = model,
                    PublishedVacancies = GetSampleVacancies()
                };
                return View("~/Views/Employer/Vacancies/Index.cshtml", viewModel);
            }

            TempData["SuccessMessage"] = "Job vacancy posted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private List<VacancyViewModel> GetSampleVacancies()
        {
            return new List<VacancyViewModel>
            {
                new VacancyViewModel
                {
                    Id = 1,
                    Title = "Software Engineer",
                    Company = "Tech Solutions Inc.",
                    Location = "Kuala Lumpur",
                    JobType = "Full-Time",
                    ExperienceLevel = "Mid",
                    SalaryRange = "RM 6,000 - RM 10,000",
                    Description = "We are looking for a skilled software engineer to join our growing team. You will work on developing scalable web applications using modern technologies.",
                    Requirements = "3+ years experience in software development, proficiency in C# and .NET, experience with ASP.NET Core MVC, strong problem-solving skills.",
                    Deadline = DateTime.Now.AddDays(30),
                    PostedOn = DateTime.Now.AddDays(-5),
                    PostedBy = "HR Manager"
                },
                new VacancyViewModel
                {
                    Id = 2,
                    Title = "Frontend Developer",
                    Company = "Digital Creators Ltd.",
                    Location = "Penang",
                    JobType = "Full-Time",
                    ExperienceLevel = "Entry",
                    SalaryRange = "RM 3,500 - RM 5,500",
                    Description = "Join our creative team as a frontend developer. You'll be building beautiful, responsive user interfaces for our clients.",
                    Requirements = "1-2 years experience with HTML, CSS, JavaScript, React or Vue.js, understanding of responsive design principles.",
                    Deadline = DateTime.Now.AddDays(20),
                    PostedOn = DateTime.Now.AddDays(-3),
                    PostedBy = "Tech Lead"
                },
                new VacancyViewModel
                {
                    Id = 3,
                    Title = "Data Analyst Intern",
                    Company = "Analytics Pro",
                    Location = "Cyberjaya",
                    JobType = "Internship",
                    ExperienceLevel = "Entry",
                    SalaryRange = "RM 1,500 - RM 2,000",
                    Description = "Internship opportunity for students or fresh graduates interested in data analytics. Learn to work with large datasets and business intelligence tools.",
                    Requirements = "Currently pursuing or recently completed degree in Computer Science, Statistics, or related field. Basic knowledge of SQL and Excel.",
                    Deadline = DateTime.Now.AddDays(15),
                    PostedOn = DateTime.Now.AddDays(-7),
                    PostedBy = "Data Manager"
                },
                new VacancyViewModel
                {
                    Id = 4,
                    Title = "Senior DevOps Engineer",
                    Company = "Cloud Systems Malaysia",
                    Location = "Kuala Lumpur",
                    JobType = "Full-Time",
                    ExperienceLevel = "Senior",
                    SalaryRange = "RM 12,000 - RM 18,000",
                    Description = "Lead our DevOps initiatives and help build robust CI/CD pipelines. Work with cutting-edge cloud technologies.",
                    Requirements = "5+ years DevOps experience, strong knowledge of AWS/Azure, Docker, Kubernetes, Terraform, experience with monitoring and automation tools.",
                    Deadline = DateTime.Now.AddDays(45),
                    PostedOn = DateTime.Now.AddDays(-2),
                    PostedBy = "CTO"
                },
                new VacancyViewModel
                {
                    Id = 5,
                    Title = "Mobile App Developer",
                    Company = "AppWorks Studio",
                    Location = "Johor Bahru",
                    JobType = "Contract",
                    ExperienceLevel = "Mid",
                    SalaryRange = "RM 5,000 - RM 8,000",
                    Description = "6-month contract position to develop a cross-platform mobile application for retail industry client.",
                    Requirements = "Experience with Flutter or React Native, published apps on App Store/Play Store, knowledge of mobile UI/UX best practices.",
                    Deadline = DateTime.Now.AddDays(10),
                    PostedOn = DateTime.Now.AddDays(-1),
                    PostedBy = "Project Manager"
                },
                new VacancyViewModel
                {
                    Id = 6,
                    Title = "UI/UX Designer",
                    Company = "Creative Labs",
                    Location = "Remote",
                    JobType = "Part-Time",
                    ExperienceLevel = "Mid",
                    SalaryRange = "RM 4,000 - RM 6,000",
                    Description = "Part-time remote position for experienced UI/UX designer. Flexible hours, work on diverse client projects.",
                    Requirements = "3+ years UI/UX design experience, proficiency in Figma and Adobe XD, strong portfolio, excellent communication skills.",
                    Deadline = DateTime.Now.AddDays(25),
                    PostedOn = DateTime.Now.AddDays(-4),
                    PostedBy = "Design Director"
                }
            };
        }
    }
}

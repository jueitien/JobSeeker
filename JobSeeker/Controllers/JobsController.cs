using JobSeeker.Data;
using JobSeeker.Models;
using JobSeeker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Controllers
{
    [Authorize(Roles = UserRoles.JobSeeker)]
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly S3StorageService _s3Storage;
        private readonly ILogger<JobsController> _logger;

        public JobsController(
            ApplicationDbContext context,
            S3StorageService s3Storage,
            ILogger<JobsController> logger)
        {
            _context = context;
            _s3Storage = s3Storage;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index(
            string? keyword,
            string? location,
            string? employmentType,
            string? workplaceType,
            decimal? minimumSalary,
            string sort = "match")
        {
            return RedirectToAction("JobSeeker", "Home", new
            {
                keyword,
                location,
                employmentType,
                workplaceType,
                minimumSalary,
                sort
            });
        }

        // Displays a vacancy image to an authenticated Job Seeker.
        // The database stores only ImageS3Key. The actual file is stored at:
        // s3://<bucket>/vacancy-images/<job-id>/<generated-file-name>
        [HttpGet]
        public async Task<IActionResult> VacancyImage(long id)
        {
            var image = await _context.JobVacancyImages
                .AsNoTracking()
                .Include(x => x.Job)
                .FirstOrDefaultAsync(x => x.JobVacancyImageId == id);

            if (image == null ||
                image.Job.ApprovalStatus != "APPROVED" ||
                image.Job.JobStatus != "OPEN")
            {
                return NotFound();
            }

            try
            {
                var url = await _s3Storage.GetVacancyImagePresignedUrlAsync(
                    image.ImageS3Key, TimeSpan.FromMinutes(10));

                return Redirect(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Could not load vacancy image {VacancyImageId}.", id);
                return NotFound();
            }
        }
    }
}

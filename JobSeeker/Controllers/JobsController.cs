using JobSeeker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobSeeker.Controllers
{
    [Authorize(Roles = UserRoles.JobSeeker)]
    public class JobsController : Controller
    {
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
    }
}

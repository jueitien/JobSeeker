using JobSeeker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobSeeker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = UserRoles.JobSeeker)]
        public IActionResult JobSeeker()
        {
            return View();
        }

        [Authorize(Roles = UserRoles.Employer)]
        public IActionResult Employer()
        {
            return View();
        }

        [Authorize(Roles = UserRoles.CareerCounsellor)]
        public IActionResult CareerCounsellor()
        {
            return View();
        }

        [Authorize(Roles = UserRoles.Administrator)]
        public IActionResult Administrator()
        {
            return View();
        }
    }
}
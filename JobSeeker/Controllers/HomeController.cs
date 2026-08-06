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
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole(UserRoles.JobSeeker))
                    return RedirectToAction(nameof(JobSeeker));

                if (User.IsInRole(UserRoles.Employer))
                    return RedirectToAction(nameof(Employer));

                if (User.IsInRole(UserRoles.CareerCounsellor))
                    return RedirectToAction(nameof(CareerCounsellor));

                if (User.IsInRole(UserRoles.Administrator))
                    return RedirectToAction(nameof(Administrator));

                return Forbid();
            }

            // Public welcome page for users who are not logged in
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
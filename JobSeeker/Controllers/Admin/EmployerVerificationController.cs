using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobSeeker.Controllers.Admin
{
    [Authorize(Roles = "Administrator")]
    public class EmployerVerificationController : Controller
    {
        public IActionResult Pending()
        {
            return View("~/Views/Admin/EmployerVerification/Pending.cshtml");
        }

        public IActionResult Approved()
        {
            return View("~/Views/Admin/EmployerVerification/Approved.cshtml");
        }

        public IActionResult Rejected()
        {
            return View("~/Views/Admin/EmployerVerification/Rejected.cshtml");
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobSeeker.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class EmployerVerificationController : Controller
    {
        public IActionResult Pending()
        {
            return View();
        }

        public IActionResult Approved()
        {
            return View();
        }

        public IActionResult Rejected()
        {
            return View();
        }
    }
}

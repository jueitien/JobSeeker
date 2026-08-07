using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobSeeker.Controllers.Admin
{
    [Authorize(Roles = "Administrator")]
    public class JobApprovalController : Controller
    {
        public IActionResult Pending()
        {
            return View("~/Views/Admin/JobApproval/Pending.cshtml");
        }

        public IActionResult Approved()
        {
            return View("~/Views/Admin/JobApproval/Approved.cshtml");
        }

        public IActionResult Rejected()
        {
            return View("~/Views/Admin/JobApproval/Rejected.cshtml");
        }
    }
}

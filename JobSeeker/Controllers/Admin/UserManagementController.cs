using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobSeeker.Controllers.Admin
{
    [Authorize(Roles = "Administrator")]
    public class UserManagementController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Admin/UserManagement/Index.cshtml");
        }

        public IActionResult Suspended()
        {
            return View("~/Views/Admin/UserManagement/Suspended.cshtml");
        }
    }
}

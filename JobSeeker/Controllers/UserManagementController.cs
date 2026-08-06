using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobSeeker.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UserManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Suspended()
        {
            return View();
        }
    }
}

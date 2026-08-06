using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobSeeker.Controllers.Admin
{
    [Authorize(Roles = "Administrator")]
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Admin/Reports/Index.cshtml");
        }
    }
}

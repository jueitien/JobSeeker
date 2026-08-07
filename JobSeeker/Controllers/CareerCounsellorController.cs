using JobSeeker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Controllers
{
    [Authorize(Roles = UserRoles.CareerCounsellor)]
    public class CareerCounsellorController : Controller
    {
        [HttpGet]
        public IActionResult ResumeReview()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResumeFeedbackForm(int requestId)
        {
            ViewBag.RequestId = requestId;
            return View();
        }

        [HttpGet]
        public IActionResult CareerReview()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CareerRecommendations(int requestId)
        {
            ViewBag.RequestId = requestId;
            return View();
        }

        [HttpGet]
        public IActionResult SkillReview()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SkillRecommendations(long requestId)
        {
            ViewBag.RequestId = requestId;
            return View();
        }

        [HttpGet]
        public IActionResult CertificationReview()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CertificationRecommendations(int requestId)
        {
            ViewBag.RequestId = requestId;
            return View();
        }
    }
}
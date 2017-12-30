using Microsoft.AspNetCore.Mvc;

namespace Xmu.Crms.Mobile.Group1_7.Controllers
{
    public class CourseUIController : Controller
    {
        [Route("/CourseUI/CourseUI")]
        public IActionResult CourseUI()
        {
            return View();
        }
    }
}
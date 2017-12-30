using Microsoft.AspNetCore.Mvc;

namespace Xmu.Crms.Mobile.Group1_7.Controllers
{
    public class CourseInfoUIController : Controller
    {
        [Route("/CourseInfoUI/CourseInfoUI")]
        public IActionResult CourseInfoUI()
        {
            return View();
        }
    }
}
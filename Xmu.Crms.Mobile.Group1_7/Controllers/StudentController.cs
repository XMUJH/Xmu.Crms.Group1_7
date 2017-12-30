using Microsoft.AspNetCore.Mvc;

namespace Xmu.Crms.Mobile.Group1_7.Controllers
{
    public class StudentController : Controller
    {
        [Route("Student/StudentRollCallUI")]
        public IActionResult StudentRollCallUI()
        {
            return View();
        }

        [Route("Student/Score")]
        public IActionResult Score()
        {
            return View();
        }
    }
}
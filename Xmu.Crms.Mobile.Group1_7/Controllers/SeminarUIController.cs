using Microsoft.AspNetCore.Mvc;

namespace Xmu.Crms.Mobile.Group1_7.Controllers
{
    public class SeminarUIController : Controller
    {
        [Route("SeminarUI/Seminar")]
        public IActionResult Seminar()
        {
            return View();
        }

        [Route("SeminarUI/SeminarFixedGroupNoSelection")]
        public IActionResult SeminarFixedGroupNoSelection()
        {
            return View();
        }

        [Route("SeminarUI/SeminarRandomGroupNoSelection")]
        public IActionResult SeminarRandomGroupNoSelection()
        {
            return View();
        }
    }
}
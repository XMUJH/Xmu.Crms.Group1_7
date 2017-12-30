using Microsoft.AspNetCore.Mvc;

namespace Xmu.Crms.Mobile.Group1_7.Controllers
{
    public class GroupUIController : Controller
    {
        [Route("/GroupUI/GroupChooseTopicUI")]
        public IActionResult GroupChooseTopicUI()
        {
            return View();
        }

        [Route("/GroupUI/GroupUI")]
        public IActionResult GroupUI()
        {
            return View();
        }
    }
}
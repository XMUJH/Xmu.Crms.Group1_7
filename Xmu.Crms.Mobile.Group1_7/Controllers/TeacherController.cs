using Microsoft.AspNetCore.Mvc;

namespace Xmu.Crms.Mobile.Group1_7.Controllers
{
    public class TeacherController : Controller
    {
        [Route("Teacher/ClassManage")]
        public IActionResult ClassManage()
        {
            return View();
        }

        [Route("Teacher/RandomRollStartCallUI")]
        public IActionResult RandomRollStartCallUI()
        {
            return View();
        }

        [Route("Teacher/RandomRollCallUI")]
        public IActionResult RandomRollCallUI()
        {
            return View();
        }

        [Route("Teacher/RandomEndRollCallUI")]
        public IActionResult RandomEndRollCallUI()
        {
            return View();
        }

        [Route("Teacher/RollCallListUI")]
        public IActionResult RollCallListUI()
        {
            return View();
        }

        [Route("Teacher/GroupInfoUI")]
        public IActionResult GroupInfoUI()
        {
            return View();
        }

        [Route("Teacher/RollStartCallUI")]
        public IActionResult RollStartCallUI()
        {
            return View();
        }

        [Route("Teacher/RollCallUI")]
        public IActionResult RollCallUI()
        {
            return View();
        }

        [Route("Teacher/RollCallEndUI")]
        public IActionResult RollCallEndUI()
        {
            return View();
        }

        [Route("Teacher/EndRollCallUI")]
        public IActionResult EndRollCallUI()
        {
            return View();
        }

        [Route("Teacher/FixedGroupInfoUI")]
        public IActionResult FixedGroupInfoUI()
        {
            return View();
        }
    }
}
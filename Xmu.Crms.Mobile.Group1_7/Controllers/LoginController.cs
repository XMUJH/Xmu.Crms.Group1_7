using Microsoft.AspNetCore.Mvc;

namespace Xmu.Crms.Mobile.Group1_7.Controllers
{
    public class LoginController : Controller
    {
        [Route("/Login/ChooseSchool_province")]
        public IActionResult ChooseSchool_province()
        {
            return View();
        }

        [Route("/Login/ChooseSchool_city")]
        public IActionResult ChooseSchool_city()
        {
            return View();
        }

        [Route("/Login/ChooseSchool")]
        public IActionResult ChooseSchool()
        {
            return View();
        }

        [Route("/Login/ChooseSchool_school")]
        public IActionResult ChooseSchool_school()
        {
            return View();
        }

        [Route("/Login/ChooseSchool_school_create")]
        public IActionResult ChooseSchool_school_create()
        {
            return View();
        }

        [Route("/Login/ChooseCharacter")]
        public IActionResult ChooseCharacter()
        {
            return View();
        }

        [Route("/Login/Login")]
        public IActionResult LoginUI()
        {
            return View();
        }

        [Route("/Login/TeacherBindingUI")]
        public IActionResult TeacherBindingUI()
        {
            return View();
        }

        [Route("/Login/StudentBindingUI")]
        public IActionResult StudentBindingUI()
        {
            return View();
        }

        [Route("/Login/RegisterUI")]
        public IActionResult RegisterUI()
        {
            return View();
        }

        [Route("/Login/CreateSchool")]
        public IActionResult CreateSchool()
        {
            return View();
        }

        [Route("/Login/TeacherMainUI")]
        public IActionResult TeacherMainUI()
        {
            return View();
        }

        [Route("/Login/StudentMainUI")]
        public IActionResult StudentMainUI()
        {
            return View();
        }

        [Route("/Login/CheckStudentInfoUI")]
        public IActionResult CheckStudentInfoUI()
        {
            return View();
        }

        [Route("/Login/CheckTeacherInfoUI")]
        public IActionResult CheckTeacherInfoUI()
        {
            return View();
        }
    }
}
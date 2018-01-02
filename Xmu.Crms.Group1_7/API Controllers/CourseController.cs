using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using System.Linq;
using Xmu.Crms.Shared.Exceptions;
using System.Security.Claims;
using static Xmu.Crms.Group1_7.Utils;
using Microsoft.EntityFrameworkCore;

namespace Xmu.Crms.Group1_7
{
    [Route("")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CourseController : Controller
    {
        private CrmsContext _db;
        ICourseService _courseService;
        IClassService _classService;
        ISeminarService _seminarService;
        IGradeService _gradeService;
        ISeminarGroupService _seminarGroupService;
        IUserService _userService;
        private readonly JwtHeader _header;
        public CourseController(CrmsContext db,IUserService userService, ICourseService courseService, IClassService classService, ISeminarService seminarService, IGradeService gradeService,ISeminarGroupService seminarGroupService, JwtHeader header)
        {
            _db = db;
            _userService = userService;
            _courseService = courseService;
            _classService = classService;
            _seminarService = seminarService;
            _gradeService = gradeService;
            _seminarGroupService = seminarGroupService;
            _header = header;
        }
        //获取与当前用户相关联的课程列表
        //GET:/course
        [HttpGet("api/course")]
        public IActionResult GetCourse()
        {
            try
            {
                var courses = _courseService.ListCourseByUserId(User.Id());
                return Json(courses);
            }
            catch (CourseNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ArgumentException)
            {
                return StatusCode(400, "错误的ID格式");
            }
            //var result = new JsonResult();
            //var data = new object[]
            //{
            //    new { id = 1, name = "OOAD", numClass = 3, numStudent = 60, startTime = "2017-9-1", endTime = "2018-1-1" },
            //    new { id = 2, name = "J2EE", numClass = 1, numStudent = 60, startTime = "2017-9-1", endTime = "2018-1-1" },
            //};
            //result.Data = data;
            //return result;
        }



        //创建课程
        //POST:/course
        [HttpPost("api/course")]
        public IActionResult PostCourse([FromBody]dynamic json)
        {

            try
            {
                var courses = _courseService.InsertCourseByUserId(long.Parse(json.userId), long.Parse(json.course));
     
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { msg = "错误的ID格式" });
            }
            return Json(new
            {
               id= long.Parse(json.course).Id
            });
            //JsonResult result = new JsonResult();
            //var course = new { id = 23, name = json.name, description = json.description, startTime = json.startTime, endTime = json.endTime, proportions = json.proportions };
            //result.Data = course.id;
            //result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            //return result;
        }

        //按ID获取课程
        //GET:/course/{courseId}
        [HttpGet("api/course/{courseId}")]
        public IActionResult GetCourseByCourseId(int courseId)
        {
            try
            {
                var courses = _courseService.GetCourseByCourseId(courseId);
                return Json(new
                {
                    id = courses.Id,
                    name = courses.Name,


                    startdate = courses.StartDate,
                    enddate = courses.EndDate,

                    teacher = courses.Teacher,
                    description = courses.Description
                } );
            }
            catch (CourseNotFoundException)
            {
                return StatusCode(404, new { msg = "未找到课程" });
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { msg = "错误的ID格式" });
            }
            //JsonResult result = new JsonResult();
            //var data = new { id = 23, name = "OOAD", description = "面向对象分析与设计", teacherName = "邱明", teacherEmail = "mingqiu@xmu.edu.cn" };
            //result.Data = data;
            //return result;
        }

        //修改课程
        //PUT:/course/{courseId}
        [HttpPut("api/course/{courseId}")]
        public IActionResult ModifyCourse(int courseId, [FromBody]dynamic json)
        {
            try
            {
                _courseService.UpdateCourseByCourseId(courseId, json);
            }
            catch (Shared.Exceptions.InvalidOperationException)
            {
                return StatusCode(403, new { msg = "无法修改他人课程" });
            }
            return NoContent();
            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            //response.Content = new StringContent("成功", Encoding.UTF8);
            //return response;
        }

        //按Id删除课程
        //DELETE:/course/{courseId}
        [HttpDelete("api/course/{courseId}")]
        public IActionResult DeleteCourse(int courseId)
        {
            try
            {
                _courseService.DeleteCourseByCourseId(courseId);
            }
            catch (CourseNotFoundException)
            {
                return StatusCode(404, new { msg = "未找到课程" });
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { msg = "错误的ID格式" });
            }
            return NoContent();
            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            //response.Content = new StringContent("成功", Encoding.UTF8);
            //return response;
            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            //response.Content = new StringContent("成功", Encoding.UTF8);
            //return response;
        }

        //按ID获取课程的班级列表
        //GET:/course/{courseId}/class
        [HttpGet("api/course/{courseId}/class")]
        public IActionResult GetClass(int courseId)
        {
            try
            {
                var classes = _classService.ListClassByCourseId(courseId);
                return Json(classes.Select(c => new {
                    id = c.Id,
                    name = c.Name,
                    time = c.ClassTime,
                    site = c.Site,
                    number = _userService.ListUserByClassId(c.Id, "", "").Count
                //courseTeacher = c.Course.Teacher.Name,
                //courseName = c.Course.Name
            }));
            }
            catch (CourseNotFoundException)
            {
                return StatusCode(404, new { msg = "未找到课程" });
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { msg = "错误的ID格式" });
            }

            //JsonResult result = new JsonResult();
            //var data = new object[] {
            //    new { id = 45, name = "周三1-2节"},
            //    new { id = 48, name = "周三3-4节"}
            //};
            //result.Data = data;
            //return result;
        }

        //按ID获取学生所在课程的班级列表
        //GET:/course/{courseId}/class
        [HttpGet("api/course/{courseId}/class/{studentId}")]
        public IActionResult GetClassbyStudentId(int courseId,int studentId)
        {
            try
            {
                var classes = _classService.ListClassByCourseId(courseId);
                var selections = _classService.ListClassByUserId(studentId);
                //long classId = 0;
                foreach (var Class in classes)
                    foreach(var selection in selections)
                    {
                        if (selection.Id == Class.Id)
                            return Json(selection.Id);
                    }
                return NoContent();
            }
            catch (CourseNotFoundException)
            {
                return StatusCode(404, "未找到课程" );
            }
            catch (ArgumentException)
            {
                return StatusCode(400, "错误的ID格式" );
            }

            //JsonResult result = new JsonResult();
            //var data = new object[] {
            //    new { id = 45, name = "周三1-2节"},
            //    new { id = 48, name = "周三3-4节"}
            //};
            //result.Data = data;
            //return result;
        }
        //在指定ID的课程创建班级
        //POST:/course/{courseId}/class
        [HttpPost("api/course/{courseId}/class")]
        public IActionResult PostClass(int courseId, [FromBody]dynamic json)
        {

            try
            {
                var classes = _classService.InsertClassById(courseId,json);
                return Json(classes);
            }
            catch (CourseNotFoundException)
            {
                return StatusCode(404, new { msg = "未找到课程" });
            }
            catch (Shared.Exceptions.InvalidOperationException)
            {
                return StatusCode(403, new { msg = "学生无法创建课程" });
            }
            //JsonResult result = new JsonResult();
            //var Class = new { id = 45 };
            //result.Data = Class;
            //result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            //return result;
        }

        //按课程ID获取讨论课详情列表
        //GET: /course/{courseId}/seminar
        [HttpGet("api/course/{courseId}/seminar")]
        public IActionResult GetSeminar(long courseId)
        {
            try
            {
                DateTime dt = DateTime.Now;
                var seminars = _seminarService.ListSeminarByCourseId(courseId);
                return Json(seminars.Select(s => new
                {
                    id = s.Id,
                    name = s.Name,
                    description = s.Description,
                    groupingMethod = (s.IsFixed == true) ? "fixed" : "random",
                    startTime = s.StartTime.ToString("M"),
                    endTime = s.EndTime.ToString("M"),
                    Judge = ((DateTime.Compare(dt, s.StartTime) > 0) && (DateTime.Compare(s.EndTime, dt) > 0)) ? 1 : 0,
                    grade = (_seminarGroupService.GetSeminarGroupById(s.Id, User.Id()).FinalGrade > 0) ? _seminarGroupService.GetSeminarGroupById(s.Id, User.Id()).FinalGrade : 0
                }));
            }
            catch (CourseNotFoundException)
            {
                return StatusCode(404, new { msg = "未找到课程" });
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { msg = "错误的ID格式" });
            }
            //var result = new JsonResult();
            //var data = new object[] {
            //    new { id = 29, name = "界面原型设计", description = "界面原型设计", groupingMethod = "fixed", startTime = "2017-09-25", endTime = "2017-10-09", grade=4},
            //    new { id = 32, name = "概要设计", description = "模型层与数据库设计", groupingMethod = "fixed", startTime = "2017-10-10", endTime = "2017-10-24", grade=5}
            //};
            //result.Data = data;
            //return result;
        }

        //在指定ID的课程创建讨论课
        //POST: /course/{courseId}/seminar
        [HttpPost("api/course/{courseId}/seminar")]
        public IActionResult PostSeminar(int courseId, [FromBody]dynamic json)
        {
            try
            {
                var seminars = _seminarService.InsertSeminarByCourseId(courseId,json);
                return Json(seminars);
            }
            catch (Shared.Exceptions.InvalidOperationException)
            {
                return StatusCode(403, new { msg = "学生无法创建" });
            }
          
            //JsonResult result = new JsonResult();
            //var Class = new { id = 32 };
            //result.Data = Class;
            //result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            //return result;
        }

        //获取课程正在进行的讨论课
        ///???对应service未找到没有找到接口
        //GET: /course/{courseId}/seminar/current
        [HttpGet("api/course/{courseId}/seminar/current")]
        public IActionResult GetCurrentSeminar(long courseId)
        {
            try
            {
                IList<Seminar> seminars = _seminarService.ListSeminarByCourseId(courseId);
                DateTime dt = DateTime.Now;
                Seminar currentSeminar = new Seminar();
                bool flag = false;
                foreach (Seminar seminar in seminars)
                {
                    if((DateTime.Compare(dt, seminar.StartTime) > 0)&&(DateTime.Compare(seminar.EndTime,dt)>0))
                    {
                        currentSeminar = seminar;
                        flag = true;
                        break;
                    }
                }
                if (flag == false) throw new SeminarNotFoundException("没有正在进行的讨论课");
                IList<ClassInfo> classes = _classService.ListClassByCourseId(courseId);
                var groupingMethod = "fixed";
                if (currentSeminar.IsFixed != true) groupingMethod = "random";
                var result = new
                {
                    id = currentSeminar.Id,
                    courseName = currentSeminar.Course.Name,
                    name = currentSeminar.Name,
                    startTime = currentSeminar.StartTime.ToString("M"),
                    endTime = currentSeminar.EndTime.ToString("M"),
                    groupingMethod,
                    classes
                };
                return Json(result);
            }
            catch (CourseNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (SeminarNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ArgumentException)
            {
                return StatusCode(400, "错误的ID格式");
            }
            //var result = new JsonResult();
            //var data = new object[] {
            //    new { id = 29, name = "界面原型设计", description = "界面原型设计", groupingMethod = "fixed", startTime = "2017-09-25", endTime = "2017-10-09", grade=4},
            //    new { id = 32, name = "概要设计", description = "模型层与数据库设计", groupingMethod = "fixed", startTime = "2017-10-10", endTime = "2017-10-24", grade=5}
            //};
            //result.Data = data;
            //return result;
        }

        //按课程ID获取学生的所有讨论课成绩
        //GET: /course/{courseId}/grade
        [HttpGet("api/course/{courseId}/grade")]
        public IActionResult getGrade(long userId, long courseId)
        {
          try
            {
                var grades = _gradeService.ListSeminarGradeByCourseId(userId,courseId);
                return Json(grades.Select(c => new {
                    seminarName=c.Seminar.Name,
                    id = c.Id,
                    leaderName =c.Leader.Name,
                    presentationGrade=c.PresentationGrade,
                    reportGrade=c.ReportGrade,
                    grade=c.FinalGrade
                }));
            }
            catch (CourseNotFoundException)
            {
                return StatusCode(404, new { msg = "未找到课程" });
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { msg = "错误的ID格式" });
            }
            //var result = new JsonResult();
            //var data = new object[] {
            //    new { seminarName = "需求分析", groupName = "3A2",leaderName = "张三", presentationGrade = 3, reportGrade = 3,grade = 4 },
            //    new { seminarName = "界面原型设计", groupName = "3A3",leaderName = "张三", presentationGrade = 4, reportGrade = 4,grade = 4 }
            //};
            //result.Data = data;
            //return result;
        }
    }
}
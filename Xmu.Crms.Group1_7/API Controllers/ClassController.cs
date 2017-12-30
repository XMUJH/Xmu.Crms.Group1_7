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

namespace Xmu.Crms.Group1_7
{
    [Route("")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ClassController : Controller
    {
        private CrmsContext _db;
        IClassService _classService;
        ISeminarGroupService _seminarGroupService;

        public ClassController(CrmsContext db, IClassService classService, ISeminarGroupService seminarGroupService)
        {
            _db = db;
            _classService = classService;
            _seminarGroupService = seminarGroupService;
        }

        [Produces("application/json")]
        //获取与当前用户相关的或者符合条件的班级列表
        //GET:/class
        [HttpGet("api/class")]
        public IActionResult GetClass(int courseId)
        {
            try
            {
                var classes = _classService.ListClassByCourseId(courseId);
                return Json(classes.Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    time = c.ClassTime,
                    site = c.Site,
                    courseTeacher = c.Course.Teacher.Name,
                    courseName = c.Course.Name

                }));
            }
            catch (ClassNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ArgumentException)
            {
                return StatusCode(400, "错误的ID格式");
            }

            //var data = new object[]
            //{
            //    new { id = 23, name="周三1-2节", numStudent = 60,time="周三1-2、周五1-2",site="公寓405",courseName="OOAD",courseTeacher = "邱明"},
            //    new { id = 42, name="一班", numStudent = 60,time="周三34节、周五12节",site="海韵202",courseName=".Net",courseTeacher = "杨律青"}
            //};

            //return Json(data);
        }

        //按ID获取班级详情
        //GET: /class/{classId}
        [HttpGet("api/class/{classId}")]
        public IActionResult GetClassByClassId(int classId)
        {
            try
            {
                var classes = _classService.GetClassByClassId(classId);
                return Json(new
                {
                    id = classes.Id,
                    name = classes.Name,
                    time = classes.ClassTime,
                    site = classes.Site,
                    proportions = classes.PresentationPercentage
                });
            }
            catch (ClassNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ArgumentException)
            {
                return StatusCode(400, "错误的ID格式");
            }
            //var data = new { id = 23, name="周三1-2节", numStudent = 120,time="周三一二节",site="海韵201",calling=-1,roster="/roster/周三12班.xlsx",proportions=new { report=50,presentation=50,c=20,b=60,a=20} };
            //return Json(data);
        }

        //按ID修改班级
        //PUT:/class/{classId}
        [HttpPut("api/class/{classId}")]
        public IActionResult ModifyClass(long classId, ClassInfo newclass)
        {
            try
            {
                _classService.UpdateClassByClassId(classId, newclass);
            }
            catch (ClassNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ArgumentException)
            {
                return StatusCode(400, "错误的ID格式");
            }
            return NoContent();

        }

        //按Id删除班级
        //DELETE:/course/{classId}
        [HttpDelete("api/class/{classId}")]
        public IActionResult DeleteClass(long classId)
        {
            try
            {
                _classService.DeleteClassByClassId(classId);
            }
            catch (ClassNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ArgumentException)
            {
                return StatusCode(400, "错误的ID格式");
            }
            return NoContent();
        }

        //按班级ID查找学生列表（查询学号、姓名开头）
        //???未找到service
        /// <summary>
        /// 提问
        /// </summary>
        /// <param name="classId"></param>
        /// <param name="numBeginWith"></param>
        /// <param name="nameBeginWith"></param>
        /// <returns></returns>
        //GET:/class/{classId}/student
        [HttpGet("api/class/{classId}/student")]
        public IActionResult GetStudent(int classId, string numBeginWith, string nameBeginWith)
        {
            //try
            //{
            //    _classService.DeleteClassByClassId(classId);
            //    return;
            //}
            //catch (Shared.Exceptions.InvalidOperationException)
            //{
            //    return StatusCode(404, new { msg = "该操作不合法" });
            //}
            //catch (ClassNotFoundException)
            //{
            //    return StatusCode(404, new { msg = "未找到班级" });
            //}
            var data = new object[]
            {
                new { id = 233, name="张三", number="24320152202333"},
                new { id = 245, name="张八", number="24320152202334"}
            };
            return Json(data);
        }

        //学生按ID选择班级
        //POST:/class/{classId}/student
        [HttpPost("api/class/{classId}/student")]
        public IActionResult ChooseClass(int classId, [FromBody]dynamic json)
        {
            try
            {
                var classes = _classService.InsertCourseSelectionById(long.Parse(json.userId),classId);
                return Json(classes);
            }
            catch (ClassNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ArgumentException)
            {
                return StatusCode(400, "错误的ID格式");
            }

            //var Class = new { url="/class/34/student/2757" };
            //return Json(Class);
        }

        //学生按ID取消选择班级
        //DELETE:/class/{classId}/student/{studentId}
        [HttpDelete("api/class/{classId}/student/{studentId}")]
        public IActionResult DeleteClass(int classId, string studentId)
        {
            {
                try
                {
                    _classService.DeleteCourseSelectionById(long.Parse(studentId), classId);
                }
                catch (ClassNotFoundException e)
                {
                    return StatusCode(404, e.GetAlertInfo());
                }
                catch (ArgumentException)
                {
                    return StatusCode(400, "错误的ID格式");
                }
                return NoContent();
                //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
                //response.Content = new StringContent("成功", Encoding.UTF8);
                //return response;
            }
        }

        ////按ID获取自身所在班级小组
        ////GET:/class/{classId}/classgroup
        //[HttpGet("api/class/{classId}/classgroup")]
        //public IActionResult GetClassgroup(int classId)
        //{
        //    var members = new object[]
        //    {
        //        new {id=2756, name="李四", number="23320152202443"},
        //        new {id=2777, name="王五", number="23320152202433"}
        //    };
        //    var data = new { leader = new { id = 2757, name = "张三", number = "23320152202333" }, members };
        //    return Json(data);
        //}


        //班级小组组长辞职
        //PUT:/class/{classId}/classgroup/resign
        [HttpPut("api/class/{classId}/classgroup/resign")]
        public IActionResult ResignLeader([FromBody]dynamic json)
        {
            try
            {
                _seminarGroupService.ResignLeaderById(long.Parse(json.groupId), long.Parse(json.userId));
            }
            catch (UserNotFoundException e)
            {
                return StatusCode(400, e.GetAlertInfo());
            }
            catch (GroupNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (Shared.Exceptions.InvalidOperationException e)
            {
                return StatusCode(403, "学生不是组长");
            }
            catch (ArgumentException)
            {
                return StatusCode(400, "错误的ID格式");
            }

            return NoContent();
            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            //response.Content = new StringContent("成功", Encoding.UTF8);
            //return response;
        }


        //成为班级小组组长
        //PUT:/class/{classId}/classgroup/assign
        [HttpPut("api/class/{classId}/classgroup/asssign")]
        public IActionResult AssignLeader([FromBody]dynamic json)
        {
            try
            {
                _seminarGroupService.AssignLeaderById(long.Parse(json.groupId), long.Parse(json.userId));
            }
            catch (UserNotFoundException e)
            {
                return StatusCode(400, e.GetAlertInfo());
            }
            catch (GroupNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (Shared.Exceptions.InvalidOperationException)
            {
                return StatusCode(403, "已经有组长了");
            }
            catch (ArgumentException)
            {
                return StatusCode(400, "错误的ID格式");
            }

            return NoContent();
        }


        //添加班级小组成员
        //PUT:/class/{classId}/classgroup/add
        [HttpPut("api/class/{classId}/classgroup/add")]
        public IActionResult AddMember([FromBody]dynamic json)
        {
            try
            {
                _seminarGroupService.InsertSeminarGroupMemberById(long.Parse(json.userId), long.Parse(json.groupId));
            }
            catch (UserNotFoundException e)
            {
                return StatusCode(400, e.GetAlertInfo());
            }
            catch (GroupNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (Shared.Exceptions.InvalidOperationException)
            {
                return StatusCode(409, "待添加学生已经在小组里了");
            }
            catch (ArgumentException)
            {
                return StatusCode(400, "错误的ID格式");
            }

            return NoContent();
            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            //response.Content = new StringContent("成功", Encoding.UTF8);
            //return response;
        }


        //移除成员
        //PUT:/class/{classId}/classgroup/remove
        [HttpPut("api/class/{classId}/classgroup/remove")]
        public IActionResult RemoveMember([FromBody]dynamic json)
        {
            try
            {
                _seminarGroupService.DeleteSeminarGroupMemberBySeminarGroupId(long.Parse(json.seminarGroupId));
            }
            catch (UserNotFoundException e)
            {
                return StatusCode(400, e.GetAlertInfo());
            }
            catch (GroupNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (Shared.Exceptions.InvalidOperationException)
            {
                return StatusCode(409, "待添加学生已经在小组里了");
            }
            catch (ArgumentException)
            {
                return StatusCode(400, "错误的ID格式");
            }

            return NoContent();
            //    //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            //    //response.Content = new StringContent("成功", Encoding.UTF8);
            //    //return response;
            //}
        }
    }
}
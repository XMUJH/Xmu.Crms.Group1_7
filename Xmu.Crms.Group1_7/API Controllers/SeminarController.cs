using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xmu.Crms.Shared.Exceptions;
using Type = Xmu.Crms.Shared.Models.Type;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using static Xmu.Crms.Group1_7.Utils;

namespace Xmu.Crms.Group1_7
{
    [Route("")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SeminarController : Controller
    {
        private readonly ISeminarService _seminarService;
        private readonly ITopicService _topicService;
        private readonly IUserService _userService;
        private readonly CrmsContext _db;
        private readonly ISeminarGroupService _seminargroupService;
        private readonly IClassService _classService;

        public SeminarController(ISeminarService seminarService, ITopicService topicService, ISeminarGroupService seminargroupService, IUserService userService, IClassService classService, CrmsContext db)
        {
            _seminarService = seminarService;
            _topicService = topicService;
            _seminargroupService = seminargroupService;
            _userService = userService;
            _classService = classService;
            _db = db;
        }

        [HttpGet("api/seminar/{seminarId:long}")]
        public IActionResult GetSeminarById([FromRoute] long seminarId)
        {
            try
            {
                var sem = _seminarService.GetSeminarBySeminarId(seminarId);
                return Json(new
                {
                    id = sem.Id,
                    name = sem.Name,
                    description = sem.Description,
                    coursename = _db.Course.Find(sem.CourseId).Name,
                    startTime = sem.StartTime.ToString("yyyy-MM-dd"),
                    endTime = sem.EndTime.ToString("yyyy-MM-dd")
                });
            }
            catch (SeminarNotFoundException)
            {
                return StatusCode(404, new { msg = "讨论课不存在" });

            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { msg = "讨论课ID输入格式有误" });
            }
        }

        [HttpGet("api/seminar/{seminarId:long}/{classId}")]
        public IActionResult GetSeminarById([FromRoute] long seminarId,long classId)
        {
            try
            {
                return Json(new
                {
                    statu = _db.Location.SingleOrDefault(c => c.ClassInfoId == classId && c.SeminarId == seminarId).Status
                });
            }
            catch (SeminarNotFoundException)
            {
                return StatusCode(404, new { msg = "讨论课不存在" });

            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { msg = "讨论课ID输入格式有误" });
            }
        }

        [HttpPut("api/seminar/{seminarId:long}")]
        public IActionResult UpdateSeminarById([FromRoute] long seminarId, [FromBody] Seminar updated)
        {
            if (User.Type() != Type.Teacher)
            {
                return StatusCode(403, new { msg = "权限不足" });
            }
            try
            {
                _seminarService.UpdateSeminarBySeminarId(seminarId, updated);
                return NoContent();
            }
            catch (SeminarNotFoundException)
            {
                return StatusCode(404, new { msg = "讨论课不存在" });
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { msg = "讨论课ID输入格式有误" });
            }
        }
        [HttpDelete("api/seminar/{seminarId:long}")]
        public IActionResult DeleteSeminarById([FromRoute] long seminarId)
        {
            if (User.Type() != Type.Teacher)
            {
                return StatusCode(403, new { msg = "权限不足" });
            }
            try
            {
                _seminarService.DeleteSeminarBySeminarId(seminarId);
                return NoContent();
            }
            catch (SeminarNotFoundException)
            {
                return StatusCode(404, new { msg = "讨论课不存在" });
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { msg = "讨论课ID输入格式有误" });
            }
        }

        //groupLeft未加
        [HttpGet("api/seminar/{seminarId:long}/topic")]
        public IActionResult GetTopicsBySeminarId([FromRoute] long seminarId)
        {
            try
            {
                var topics = _topicService.ListTopicBySeminarId(seminarId);
                return Json(topics.Select(t => new
                {
                    id = t.Id,
                    serial = t.Serial,
                    name = t.Name,
                    description = t.Description,
                    groupLimit = t.GroupNumberLimit,
                    groupMemberLimit = t.GroupStudentLimit,
                }));
            }
            catch (SeminarNotFoundException)
            {
                return StatusCode(404, new { msg = "讨论课不存在" });
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { msg = "话题ID输入格式有误" });
            }
        }

        [HttpPost("api/seminar/{seminarId:long}/topic")]
        public IActionResult CreateTopicBySeminarId([FromRoute] long seminarId, [FromBody] Topic newTopic)
        {
            if (User.Type() != Type.Teacher)
            {
                return StatusCode(403, new { msg = "权限不足" });
            }

            var topicid = _topicService.InsertTopicBySeminarId(seminarId, newTopic);
            return Created("/topic/" + topicid, newTopic);
        }

        //没有小组成员 和 report
        [HttpGet("api/seminar/{seminarId:long}/group")]
        public IActionResult GetGroupsBySeminarId([FromRoute] long seminarId)
        {
            try
            {
                var groups = _seminargroupService.ListSeminarGroupBySeminarId(seminarId);
                return Json(groups.Select(t => new
                {
                    id = t.Id,
                    name = t.Id + "组"
                }));
            }
            catch (SeminarNotFoundException)
            {
                return StatusCode(404, new { msg = "讨论课不存在" });
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { msg = "讨论课ID输入格式有误" });
            }
        }

        [HttpGet("api/seminar/{seminarId:long}/group/my")]
        public IActionResult GetStudentGroupBySeminarId([FromRoute] long seminarId)
        {
            if (User.Type() != Type.Student)
            {
                return StatusCode(403, new { msg = "权限不足" });
            }

            try
            {
                var groups = _seminargroupService.ListSeminarGroupBySeminarId(seminarId);
                var group = groups.SelectMany(grp => _db.Entry(grp).Collection(gp => gp.SeminarGroupMembers).Query().Include(gm => gm.SeminarGroup)
                            .Where(gm => gm.StudentId == User.Id()).Select(gm => gm.SeminarGroup))
                    .SingleOrDefault(sg => sg.SeminarId == seminarId) ?? throw new GroupNotFoundException();
                var leader = group.Leader ?? _userService.GetUserByUserId(group.LeaderId);
                var members = _seminargroupService.ListSeminarGroupMemberByGroupId(group.Id);
                var topics = _topicService.ListSeminarGroupTopicByGroupId(group.Id)
                    .Select(gt => _topicService.GetTopicByTopicId(gt.TopicId));
                return Json(new
                {
                    id = group.Id,
                    name = group.Id + "组",
                    leader = new
                    {
                        id = leader.Id,
                        name = leader.Name
                    },
                    members = members.Select(u => new { id = u.Id, name = u.Name }),
                    topics = topics.Select(t => new { id = t.Id, name = t.Name })
                });
            }
            catch (SeminarNotFoundException)
            {
                return StatusCode(404, new { msg = "讨论课不存在" });
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { msg = "讨论课ID输入格式有误" });
            }
        }

        [HttpPut("api/seminar/{seminarId}/class/{classId}/startCall")]
        public IActionResult StartCallInRoll(long seminarId, long classId, [FromBody]dynamic json)
        {
            if (User.Type() != Type.Teacher)
            {
                return StatusCode(403, "权限不足");
            }

            try
            {
                Location location = new Location();
                location.SeminarId = seminarId;
                location.ClassInfoId = classId;
                location.ClassInfo = _classService.GetClassByClassId(classId);
                location.Seminar = _seminarService.GetSeminarBySeminarId(seminarId);
                location.Longitude = json.lng;
                location.Latitude = json.lat;
                long result = _classService.CallInRollById(location);
                return Json(result);
            }
            catch(ClassNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (SeminarNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch(ArgumentException)
            {
                return StatusCode(404, "错误的ID格式");
            }
        }

        [HttpPut("api/seminar/{seminarId}/class/{classId}/endCall")]
        public IActionResult EndCallInRoll(long seminarId, long classId)
        {
            if (User.Type() != Type.Teacher)
            {
                return StatusCode(403, "权限不足");
            }

            try
            {
                _classService.EndCallRollById(seminarId, classId);
                return NoContent();
            }
            catch (ClassNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (SeminarNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ArgumentException)
            {
                return StatusCode(404, "错误的ID格式");
            }
        }

        [HttpPut("api/seminar/{seminarId}/class/{classId}/finishAttendanceList")]
        public IActionResult FinishList(long seminarId, long classId)
        {
            if (User.Type() != Type.Teacher)
            {
                return StatusCode(403, "权限不足");
            }
            try
            {
                IList<UserInfo> users1 = _userService.ListPresentStudent(seminarId, classId);
                IList<UserInfo> users2 = _userService.ListLateStudent(seminarId, classId);
                IList<UserInfo> users3 = new List<UserInfo>();
                IList<long> usersId2 = new List<long>();
                foreach(UserInfo x in users1)
                {
                    users3.Add(x);
                }
                foreach (UserInfo x in users2)
                {
                    users3.Add(x);
                }
                foreach(UserInfo x in users3)
                {
                    usersId2.Add(x.Id);
                }
                IList<UserInfo> users = _userService.ListUserByClassId(classId, "", "");
                IList<long> usersId = new List<long>();
                foreach (UserInfo x in users)
                {
                    usersId.Add(x.Id);
                }
                Attendance tool = new Attendance();
                foreach(long x in usersId)
                {
                    if(usersId2.Contains(x)==false)
                    {
                        tool.ClassId = classId;
                        tool.SeminarId = seminarId;
                        tool.StudentId = x;
                        tool.AttendanceStatus = AttendanceStatus.Absent;
                        _db.Attendences.Add(tool);
                    }
                }
                _db.SaveChanges();
                return NoContent();
            }
            catch (ClassNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (SeminarNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ArgumentException)
            {
                return StatusCode(404, "错误的ID格式");
            }
        }

        [HttpGet("api/seminar/{seminarId}/class/{classId}/attendance/present")]
        public IActionResult GetPresentStudent(long seminarId, long classId)
        {
            if (User.Type() != Type.Teacher)
            {
                return StatusCode(403, "权限不足");
            }
            try
            {
                IList<UserInfo> attendances = _userService.ListPresentStudent(seminarId, classId);
                var result = new
                {
                    numPresent = attendances.Count(),
                    attendances
                };
                return Json(result);
            }
            catch (SeminarNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ClassNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ArgumentException)
            {
                return StatusCode(404, "错误的ID格式");
            }
        }

        [HttpGet("api/seminar/{seminarId}/class/{classId}/attendance/late")]
        public IActionResult GetLateStudent(long seminarId, long classId)
        {
            if (User.Type() != Type.Teacher)
            {
                return StatusCode(403, "权限不足");
            }
            try
            {
                IList<UserInfo> attendances = _userService.ListLateStudent(seminarId, classId);
                var result = new
                {
                    attendances
                };
                return Json(result);
            }
            catch (SeminarNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ClassNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ArgumentException)
            {
                return StatusCode(404, "错误的ID格式");
            }
        }

        [HttpGet("api/seminar/{seminarId}/class/{classId}/attendance/absent")]
        public IActionResult GetAbsentStudent(long seminarId, long classId)
        {
            if (User.Type() != Type.Teacher)
            {
                return StatusCode(403, "权限不足");
            }
            try
            {
                IList<UserInfo> attendances = _userService.ListAbsenceStudent(seminarId, classId);
                var result = new
                {
                    attendances
                };
                return Json(result);
            }
            catch (SeminarNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ClassNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ArgumentException)
            {
                return StatusCode(404, "错误的ID格式");
            }
        }
    }
}
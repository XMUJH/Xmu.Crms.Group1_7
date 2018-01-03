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
        private readonly ICourseService _courseService;
        private readonly IFixGroupService _fixGroupService;
        private readonly CrmsContext _db;
        private readonly ISeminarGroupService _seminargroupService;
        private readonly IClassService _classService;

        public SeminarController(ISeminarService seminarService,IFixGroupService fixGroupService, ICourseService courseService,ITopicService topicService, ISeminarGroupService seminargroupService, IUserService userService, IClassService classService, CrmsContext db)
        {
            _seminarService = seminarService;
            _topicService = topicService;
            _fixGroupService = fixGroupService;
            _courseService = courseService;
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

        //[HttpGet("api/seminar/{seminarId}/class/{classId}/group")]
        //public IActionResult GetGroupsBySeminarId(long seminarId, long classId)
        //{
        //    try
        //    {
        //        IList<SeminarGroup> rawGroups = _seminargroupService.ListSeminarGroupBySeminarId(seminarId);
        //        SeminarGroup tool;
        //        IList<SeminarGroup> groups = new List<SeminarGroup>();
        //        foreach (SeminarGroup x in rawGroups)
        //        {
        //            if (x.ClassId == classId)
        //            {
        //                tool = new SeminarGroup();
        //                tool = x;
        //                groups.Add(tool);
        //            }
        //        }
        //        return Json(groups);
        //    }
        //    catch (SeminarNotFoundException e)
        //    {
        //        return StatusCode(404, e.GetAlertInfo());
        //    }
        //    catch (ArgumentException)
        //    {
        //        return StatusCode(400, "讨论课ID输入格式有误");
        //    }
        //}

        //[HttpGet("api/seminar/group/{groupId}/groupTopic")]
        //public IActionResult GetTopicByGroupId(long groupId)
        //{
        //    try
        //    {
        //        IList<SeminarGroupTopic> topics = _topicService.ListSeminarGroupTopicByGroupId(groupId);
        //        return Json(topics);
        //    }
        //    catch (GroupNotFoundException e)
        //    {
        //        return StatusCode(404, e.GetAlertInfo());
        //    }
        //    catch (ArgumentException)
        //    {
        //        return StatusCode(400, "ID输入格式有误");
        //    }
        //}

        //[HttpGet("api/seminar/group/{groupId}/groupMember")]
        //public IActionResult GetMemberByGroupId(long groupId)
        //{
        //    try
        //    {
        //        IList<UserInfo> members = _seminargroupService.ListSeminarGroupMemberByGroupId(groupId);
        //        return Json(members);
        //    }
        //    catch (GroupNotFoundException e)
        //    {
        //        return StatusCode(404, e.GetAlertInfo());
        //    }
        //    catch (ArgumentException)
        //    {
        //        return StatusCode(400, "ID输入格式有误");
        //    }
        //}

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

        [HttpGet("api/seminar/{seminarId:long}/{classId}")]
        public IActionResult GetSeminarBytwoId([FromRoute] long seminarId,long classId)
        {
            try
            {
                //var att = _db.Attendences.SingleOrDefault(c => c.ClassId == classId && c.SeminarId == seminarId && c.StudentId == User.Id());
                //string atten;
                //if (att == null)
                    //atten = "no";
                //else
                    //atten = att.AttendanceStatus.ToString();
                return Json(new
                {
                    //attendance = atten,
                    statu = _db.Location.SingleOrDefault(c => c.ClassInfoId == classId && c.SeminarId == seminarId).Status
                });
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        [HttpGet("api/seminar/{seminarId:long}/{classId}/detail")]
        public IActionResult GetSeminarDetailById([FromRoute] long seminarId,long classId)
        {
            var seminar = _seminarService.GetSeminarBySeminarId(seminarId);
            var course = _courseService.GetCourseByCourseId(seminar.CourseId);
            var Class = _classService.GetClassByClassId(classId);
            var teacher = _userService.GetUserByUserId(course.TeacherId);
            try
            {
                return Json(new
                {
                    teacherName = teacher.Name,
                    teacherEmail = teacher.Email,
                    startTime = seminar.StartTime.ToString("yyyy-MM-dd"),
                    site = Class.Site
                });
            }
            catch (NullReferenceException)
            {
                return null;
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
                return StatusCode(404, "讨论课不存在");
            }
            catch (ArgumentException)
            {
                return StatusCode(400, "讨论课ID输入格式有误");
            }
        }

        [HttpPost("api/seminar/{seminarId:long}/{classId}/callinroll")]
        public IActionResult CreateTopicBySeminarId(long seminarId,long classId, [FromBody]dynamic json)
        {
            try
            {
                double longitude = (double)json.longitude;
                double latitude = (double)json.latitude;
                long attendanceId = _userService.InsertAttendanceById(classId, seminarId, User.Id(), longitude, latitude);
                return Json(attendanceId);
            }
            //catch (Shared.Exceptions.InvalidOperationException e)
            //{
            //    return StatusCode(250, e.GetAlertInfo());
            //}
            catch (UserNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
        }


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
                    groupMemberLimit = t.GroupStudentLimit
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

        //[HttpGet("api/seminar/{seminarId:long}/{classId:long}/topic")]
        //public IActionResult GetTopicsBySeminarId([FromRoute] long seminarId, [FromRoute] long classId)
        //{
        //    try
        //    {
        //        IList<SeminarGroup> rawGroups = _seminargroupService.ListSeminarGroupBySeminarId(seminarId);
        //        SeminarGroup tool;
        //        IList<SeminarGroup> groups = new List<SeminarGroup>();
        //        foreach (SeminarGroup x in rawGroups)
        //        {
        //            if (x.ClassId == classId)
        //            {
        //                tool = new SeminarGroup();
        //                tool = x;
        //                groups.Add(tool);
        //            }
        //        }
        //        return Json(groups);
        //    }
        //    catch (SeminarNotFoundException e)
        //    {
        //        return StatusCode(404, e.GetAlertInfo());
        //    }
        //    catch (ArgumentException)
        //    {
        //        return StatusCode(400, "讨论课ID输入格式有误");
        //    }
        //}

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

        [HttpGet("api/seminar/{seminarId}/class/{classId}/group")]
        public IActionResult GetGroupsBySeminarId2(long seminarId, long classId)
        {
            try
            {
                IList<SeminarGroup> rawGroups = _seminargroupService.ListSeminarGroupBySeminarId(seminarId);
                SeminarGroup tool;
                IList<SeminarGroup> groups = new List<SeminarGroup>();
                foreach (SeminarGroup x in rawGroups)
                {
                    if (x.ClassId == classId)
                    {
                        tool = new SeminarGroup();
                        tool = x;
                        groups.Add(tool);
                    }
                }
                return Json(groups);
            }
            catch (SeminarNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ArgumentException)
            {
                return StatusCode(400, "讨论课ID输入格式有误");
            }
        }

        [HttpGet("api/seminar/group/{groupId}/groupTopic")]
        public IActionResult GetTopicByGroupId2(long groupId)
        {
            try
            {
                IList<SeminarGroupTopic> topics = _topicService.ListSeminarGroupTopicByGroupId(groupId);
                return Json(topics);
            }
            catch (GroupNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ArgumentException)
            {
                return StatusCode(400, "ID输入格式有误");
            }
        }

        [HttpGet("api/seminar/group/{groupId}/groupMember")]
        public IActionResult GetMemberByGroupId2(long groupId)
        {
            try
            {
                IList<UserInfo> members = _seminargroupService.ListSeminarGroupMemberByGroupId(groupId);
                return Json(members);
            }
            catch (GroupNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ArgumentException)
            {
                return StatusCode(400, "ID输入格式有误");
            }
        }

        [HttpGet("api/seminar/{seminarId:long}/group/my")]
        public IActionResult GetStudentGroupBySeminarId2([FromRoute] long seminarId)
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
                // Attendance tool = new Attendance();
                IList<Attendance> tool = new List<Attendance>();
                Attendance tool1;
                foreach (long x in usersId)
                {
                    if(usersId2.Contains(x)==false)
                    {
                        tool1 = new Attendance();
                        tool1.ClassId = classId;
                        tool1.SeminarId = seminarId;
                        tool1.StudentId = x;
                        tool1.AttendanceStatus = AttendanceStatus.Absent;
                        tool.Add(tool1);
                    }
                }
                 for (int i = 0; i < tool.Count; i++)
                {
                    _db.Attendences.Add(tool[i]);
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
                    numLate=attendances.Count,
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
                IList<UserInfo> users1 = _userService.ListPresentStudent(seminarId, classId);
                IList<UserInfo> users2 = _userService.ListLateStudent(seminarId, classId);
                IList<UserInfo> users3 = new List<UserInfo>();
                IList<long> usersId2 = new List<long>();
                foreach (UserInfo x in users1)
                {
                    users3.Add(x);
                }
                foreach (UserInfo x in users2)
                {
                    users3.Add(x);
                }
                foreach (UserInfo x in users3)
                {
                    usersId2.Add(x.Id);
                }
                IList<UserInfo> users = _userService.ListUserByClassId(classId, "", "");
                IList<long> usersId = new List<long>();
                foreach (UserInfo x in users)
                {
                    usersId.Add(x.Id);
                }
                IList<UserInfo> attendances = new List<UserInfo>();
                UserInfo tool;
                foreach (long x in usersId)
                {
                    if (usersId2.Contains(x) == false)
                    {
                        tool = new UserInfo();
                        tool = _userService.GetUserByUserId(x);
                        attendances.Add(tool);
                    }
                }
                var result = new
                {
                    numAbsent = attendances.Count(),
                    attendances
                };
                return Json(result);
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

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("api/seminar/{seminarId:long}/group")]
        public IActionResult GetGroup(long seminarId, [FromQuery]bool isFixed, [FromQuery]bool gradeable, [FromQuery]long classid, [FromQuery]bool include)
        {
            var userId = User.Id();
            if (gradeable)
            {
                //  name = "组";
                var group = _seminargroupService.GetSeminarGroupById(seminarId, userId);
                var groupTopics = _topicService.ListSeminarGroupTopicByGroupId(group.Id);
                var allGroups = _seminargroupService.ListSeminarGroupBySeminarId(seminarId);
                List<SeminarGroup> groups = allGroups.ToList<SeminarGroup>();
                List<SeminarGroupTopic> topics = new List<SeminarGroupTopic>();
                var sg = _seminargroupService.GetSeminarGroupByGroupId(groupTopics.First().SeminarGroup.Id);
                if (groupTopics.Count == 1)
                {
                    foreach (var g in allGroups)
                    {
                        var t = _topicService.ListSeminarGroupTopicByGroupId(g.Id);
                        if (t.Count <= 0 || g.Id == group.Id || t.First().Id == groupTopics.First().Id)
                        {
                            continue;
                        }
                        else
                        {
                            var sg2 = _seminargroupService.GetSeminarGroupByGroupId(t.First().SeminarGroup.Id);
                            if (sg2.ClassInfo.Id != sg.ClassInfo.Id)
                                continue;
                        }
                        topics.Add(t.First());
                    }
                }
                else
                {
                    foreach (var g in allGroups)
                    {
                        if (g.Id != group.Id)
                        {
                            var t = _topicService.ListSeminarGroupTopicByGroupId(g.Id);
                            if (t.Count <= 0)
                            {
                                continue;
                            }
                            var sg2 = _seminargroupService.GetSeminarGroupByGroupId(t.First().SeminarGroup.Id);
                            if (sg2.ClassInfo.Id != sg.ClassInfo.Id)
                                continue;
                            topics.Add(t.First());
                        }
                    }
                }
                return Json(topics.GroupBy(g => g.Topic));
            }
            if (classid != 0)
            {
                if (!isFixed)
                {
                    var t = _seminargroupService.ListSeminarGroupBySeminarId(seminarId);
                    List<SeminarGroup> groups = new List<SeminarGroup>();
                    foreach (var g in t)
                    {
                        if (g.ClassInfo.Id == classid) groups.Add(g);
                    }
                    return Json(groups);
                }
                else
                {
                    var groups = _fixGroupService.ListFixGroupByClassId(classid);
                    return Json(groups);
                }
            }
            if (include)
            {
                try
                {
                    var group = _seminargroupService.GetSeminarGroupById(seminarId, userId);
                    var members = _seminargroupService.ListSeminarGroupMemberByGroupId(group.Id);
                    return Json(new { group = group, members = members });
                }
                catch (Exception e)
                {
                    return Json("no");
                }

            }
            else
                return Json(new { status = "false" });
        }

        [HttpPut("api/seminar/{seminarId}/class/{classId}/autoGroup")]
        public IActionResult AutoGrouping(long seminarId, long classId)
        {
            if (User.Type() != Type.Teacher)
            {
                return StatusCode(403, "权限不足");
            }
            try
            {
                if(seminarId<=0||classId<=0)
                {
                    throw new ArgumentException();
                }
                SeminarGroup x;
                for(int i=1;i<=5;i++)
                {
                    x = new SeminarGroup();
                    x.SeminarId = seminarId;
                    x.ClassId = classId;
                    x.LeaderId = 0;
                    _db.SeminarGroup.Add(x);
                }
                _db.SaveChanges();
                _seminargroupService.AutomaticallyGrouping(seminarId, classId);
                return NoContent();
            }
            catch (SeminarNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ClassNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch(Shared.Exceptions.InvalidOperationException e)
            {
                return StatusCode(401, e.GetAlertInfo());
            }
            catch (ArgumentException)
            {
                return StatusCode(404, "错误的ID格式");
            }
        }
    }

    //[HttpGet("api/seminar/{seminarId}/class/{classId}/group")]
    //public IActionResult GetGroupsBySeminarId(long seminarId, long classId)
    //{
    //    try
    //    {
    //        IList<SeminarGroup> rawGroups = _seminargroupService.ListSeminarGroupBySeminarId(seminarId);
    //        SeminarGroup tool;
    //        IList<SeminarGroup> groups = new List<SeminarGroup>();
    //        foreach (SeminarGroup x in rawGroups)
    //        {
    //            if (x.ClassId == classId)
    //            {
    //                tool = new SeminarGroup();
    //                tool = x;
    //                groups.Add(tool);
    //            }
    //        }
    //        return Json(groups);
    //    }
    //    catch (SeminarNotFoundException e)
    //    {
    //        return StatusCode(404, e.GetAlertInfo());
    //    }
    //    catch (ArgumentException)
    //    {
    //        return StatusCode(400, "讨论课ID输入格式有误");
    //    }
    //}

    //[HttpGet("apiminar/group/{groupId}/groupTopic")]
    //public IActionResult GetTopicByGroupId(long groupId)
    //{
    //    try
    //    {
    //        IList<SeminarGroupTopic> topics = _topicService.ListSeminarGroupTopicByGroupId(groupId);
    //        return Json(topics);
    //    }
    //    catch (GroupNotFoundException e)
    //    {
    //        return StatusCode(404, e.GetAlertInfo());
    //    }
    //    catch (ArgumentException)
    //    {
    //        return StatusCode(400, "ID输入格式有误");
    //    }
    //}

    //[HttpGet("apiminar/group/{groupId}/groupMember")]
    //public IActionResult GetMemberByGroupId(long groupId)
    //{
    //    try
    //    {
    //        IList<UserInfo> members = _seminargroupService.ListSeminarGroupMemberByGroupId(groupId);
    //        return Json(members);
    //    }
    //    catch (GroupNotFoundException e)
    //    {
    //        return StatusCode(404, e.GetAlertInfo());
    //    }
    //    catch (ArgumentException)
    //    {
    //        return StatusCode(400, "ID输入格式有误");
    //    }
    //}

}
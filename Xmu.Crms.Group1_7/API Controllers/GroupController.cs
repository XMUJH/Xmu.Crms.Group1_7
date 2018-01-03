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
    public class GroupController : Controller
    {
        private CrmsContext _db;
        ISeminarGroupService _seminarGroupService;
        IFixGroupService _fixGroupService;
        ITopicService _topicService;
        IGradeService _gradeService;
        ICourseService _courseService;
        IClassService _classService;
        ITopicService _topicService;
        public GroupController(CrmsContext db,ISeminarGroupService seminarGroupService,IFixGroupService fixGroupService,IGradeService gradeService, ICourseService courseService, IClassService classService, ITopicService topicService)
        {
            _db = db;
            _topicService = topicService;
            _seminarGroupService = seminarGroupService;
            _fixGroupService = fixGroupService;
            _gradeService = gradeService;
            _courseService = courseService;
            _classService = classService;
            _topicService = topicService;
        }
        
        //按小组ID获取小组详情
        //GET:/group/{groupId}
        [HttpGet("api/group/{groupId}")]
        public IActionResult GetGroupInfo(long groupId)
        {
            try
            {
                //var seminarGroup = _seminarGroupService.GetSeminarGroupByGroupId(groupId);
               var seminarGroup = _db.SeminarGroup
                    //包含Seminar
                    .Include(x => x.Seminar)
                    //包含Class
                    .Include(x => x.ClassInfo)
                    //包含Leader
                    // .Include(x => x.Leader)
                    //取出小组
                    .Single(x => x.Id == groupId);
                var seminarGroupmembers = _seminarGroupService.ListSeminarGroupMemberByGroupId(groupId);
                var seminarGroupTopics = _topicService.ListSeminarGroupTopicByGroupId(groupId);
                //foreach(var topic in seminarGroupTopics)
                //{
                //    topic.SeminarGroup = null;
                //}
                //_db.SaveChanges();
                //var a = seminarGroupTopics[0].Id;
                //var b = seminarGroupTopics[0].TopicId;
                //var c = seminarGroupTopics[0].Topic.Name;
                // var d = seminarGroup.Leader;
                return Json(new {
                    id = seminarGroup.Id,
                    seminarName = seminarGroup.Seminar.Name,
                    className = seminarGroup.ClassInfo.Name,
                    topics = seminarGroupTopics,
                    members = seminarGroupmembers,
                    report = seminarGroup.Report,
                    reportGrade = seminarGroup.ReportGrade,
                    presentationGrade = seminarGroup.PresentationGrade,
                    finalGrade = seminarGroup.FinalGrade,
                    leader = seminarGroup.Leader
                });
            }
            catch(GroupNotFoundException)
            {
                return StatusCode(404, new { msg = "未找到小组" });
            }
            catch(ArgumentException)
            {
                return StatusCode(400, new { msg = "错误的Id格式" });
            }

            //int id = 13;
            //var result = new JsonResult();
            //var members = new object[] { new { id = "5324", name = "李四" }, new { id = "5678", name = "王五" } };
            //var leader = new { id = "8888", name = "张三" };
            //string report = "";
            //var topics = new { id = "", name = "领域模型与模块" };
            //var data = new { id, leader, members, report, topics };
            //result.Data = data;
            //return result;
        }

        [HttpGet("api/group/{seminarId}/{studentId}")]
        public IActionResult GetMyGroupInfo(long seminarId,long studentId)
        {
            try
            {
                var seminarGroup = _seminarGroupService.GetSeminarGroupById(seminarId,studentId);
                return Json(new
                {
                    id = seminarGroup.Id,
                   // seminarName = seminarGroup.Seminar.Name,
                    //className = seminarGroup.ClassInfo.Name,
                    //report = seminarGroup.Report,
                   // reportGrade = seminarGroup.ReportGrade,
                    //presentationGrade = seminarGroup.PresentationGrade,
                   // finalGrade = seminarGroup.FinalGrade,
                    leaderId = seminarGroup.LeaderId
                });
            }
            catch (GroupNotFoundException)
            {
                return StatusCode(404,  "您未分组" );
            }
            catch (ArgumentException)
            {
                return StatusCode(400,  "错误的Id格式" );
            }

        }

        //组长辞职
        //PUT:/group/{groupId}/resign
        [HttpPut("api/group/{groupId}/resign")]
        public IActionResult LeaderResign(long groupId)
        {
            try
            {
                var group = _db.SeminarGroup.Find(groupId);
                group.LeaderId = 0;
                _db.SaveChanges();
            }
            catch(UserNotFoundException)
            {
                return StatusCode(400, new { msg = "不存在该学生" });
            }
            catch(GroupNotFoundException)
            {
                return StatusCode(404, new { msg = "未找到小组" });
            }
            catch(Xmu.Crms.Shared.Exceptions.InvalidOperationException)
            {
                return StatusCode(403, new { msg = "该学生不是组长" });
            }
            catch(ArgumentException)
            {
                return StatusCode(400, new { msg = "错误的Id格式" });
            }
            return NoContent();
            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            //response.Content = new StringContent("成功", Encoding.UTF8);
            //return response;
        }

        //成为组长
        //PUT:/group/{groupId}/assign
        [HttpPut("api/group/{groupId}/assign")]
        public IActionResult BecomeLeader(long groupId)
        {
            try
            {
                //_seminarGroupService.AssignLeaderById(groupId, id);
                var group = _db.SeminarGroup.Find(groupId);
                group.LeaderId = User.Id();
                _db.SaveChanges();
            }
            catch (UserNotFoundException)
            {
                return StatusCode(400, new { msg = "不存在该学生" });
            }
            catch (GroupNotFoundException)
            {
                return StatusCode(404, new { msg = "未找到小组" });
            }
            catch (Xmu.Crms.Shared.Exceptions.InvalidOperationException)
            {
                return StatusCode(409, new { msg = "已经有组长了" });
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { msg = "错误的Id格式" });
            }
            return NoContent();
            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            //response.Content = new StringContent("成功", Encoding.UTF8);
            //return response;
        }

        //添加成员
        //PUT:/group/{groupId}/add
        //???怎么区分固定小组和讨论课小组
        [HttpPut("api/group/{groupId}/add")]
        public IActionResult AddMember(long groupId, long id,int groupType)
        {
            long record;

            //固定小组
            if (groupType == 0)
            {
                try
                {
                    record = _fixGroupService.InsertStudentIntoGroup(id, groupId);
                }
                catch(UserNotFoundException)
                {
                    return StatusCode(400, new { msg = "不存在该学生" });
                }
                catch(FixGroupNotFoundException)
                {
                    return StatusCode(404, new { msg = "未找到小组" });
                }
                catch(Xmu.Crms.Shared.Exceptions.InvalidOperationException)
                {
                    return StatusCode(409, new { msg = "待添加学生已经在小组里了" });
                }
                catch (ArgumentException)
                {
                    return StatusCode(400, new { msg = "错误的Id格式" });
                }

            }
            //讨论课小组
            else if(groupType==1)
            {
                try
                {
                    record = _seminarGroupService.InsertSeminarGroupMemberById(id, groupId);
                }
                catch (UserNotFoundException)
                {
                    return StatusCode(400, new { msg = "不存在该学生" });
                }
                catch (GroupNotFoundException)
                {
                    return StatusCode(404, new { msg = "未找到小组" });
                }
                catch (Xmu.Crms.Shared.Exceptions.InvalidOperationException)
                {
                    return StatusCode(409, new { msg = "待添加学生已经在小组里了" });
                }
                catch (ArgumentException)
                {
                    return StatusCode(400, new { msg = "错误的Id格式" });
                }
            }
            return NoContent();
             
            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            //response.Content = new StringContent("成功", Encoding.UTF8);
            //return response;
        }

        //移除成员
        //PUT:/group/{groupId}/remove
        [HttpPut("api/group/{groupId}/remove")]
        public IActionResult RemoveMember(long groupId, long id,int groupType)
        {
            //固定小组
            if(groupType==0)
            {
                try
                {
                    _fixGroupService.DeleteFixGroupUserById(groupId, id);
                }
                catch (UserNotFoundException)
                {
                    return StatusCode(400, new { msg = "不存在该学生" });
                }
                catch (FixGroupNotFoundException)
                {
                    return StatusCode(404, new { msg = "未找到小组" });
                }
                catch (ArgumentException)
                {
                    return StatusCode(400, new { msg = "错误的Id格式" });
                }
            }
            //讨论课小组
            else if(groupType==1)
            {
                try
                {
                    _seminarGroupService.DeleteSeminarGroupMemberBySeminarGroupId(groupId);
                }
                catch (UserNotFoundException)
                {
                    return StatusCode(400, new { msg = "不存在该学生" });
                }
                catch (GroupNotFoundException)
                {
                    return StatusCode(404, new { msg = "未找到小组" });
                }
                catch (ArgumentException)
                {
                    return StatusCode(400, new { msg = "错误的Id格式" });
                }
            }
            return NoContent();

            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            //response.Content = new StringContent("成功", Encoding.UTF8);
            //return response;
        }

        //小组按ID选择话题
        //POST:/group/{groupId}/topic/{topicId}
        //[FromBody]dynamic json
        //???url
        [HttpPost("api/group/{groupId}/topic/{topicId}")]
        public IActionResult ChooseTopic(long groupId,long topicId)
        {
            string url = "";
            try
            {
               // url = _seminarGroupService.InsertTopicByGroupId(groupId, topicId);
            }
            catch(GroupNotFoundException)
            {
                return StatusCode(404, new { msg = "未找到小组" });
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { msg = "错误的Id格式" });
            }
            return Json(url);

            //JsonResult result = new JsonResult();
            //var Topic = new { url = "/group/27/topic/23" };
            //result.Data = Topic;
            //result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            //return result;
        }

        //小组按ID取消话题
        //DELETE:/group/{groupId}/topic/{topicId}
        //???没有对应service的方法
        [HttpDelete("api/group/{groupId}/topic/{topicId}")]
        public IActionResult DeleteTopic(int groupId, int topicId)
        {
            return Json(true);
            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            //response.Content = new StringContent("成功", Encoding.UTF8);
            //return response;
        }

        //按小组ID获取小组的讨论课成绩
        //GET:/group/{groupId}/grade
        [HttpGet("api/group/{groupId}/grade")]
        public IActionResult GetGroupGrade(int groupId)
        {
            try
            {
                var seminarGroup = _gradeService.GetSeminarGroupBySeminarGroupId(groupId);
                return Json(new
                {
                    presentationGrade = seminarGroup.PresentationGrade,
                    reportGrade = seminarGroup.ReportGrade,
                    finalGrade = seminarGroup.FinalGrade
                });
            }
            catch (GroupNotFoundException)
            {
                return StatusCode(404, new { msg = "未找到小组" });
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { msg = "错误的Id格式" });
            }         
            //var result = new JsonResult();
            //var presentationGrade = new object[] { new { topicId = "257", grade = "4" }, new { topicId = "258", grade = "5" } };
            //int reportGrade = 3;
            //int grade = 4;
            //var data = new { presentationGrade, reportGrade, grade };
            //result.Data = data;
            //return result;
        }

        //按ID设置小组的报告分数
        //PUT:/group/{groupId}/grade/report
        [HttpPut("api/group/{groupId}/grade/report")]
        public IActionResult GradeReport(int groupId, int reportGrade)
        {
            try
            {
                _gradeService.UpdateGroupByGroupId(groupId, reportGrade);
            }
            catch (GroupNotFoundException)
            {
                return StatusCode(404, new { msg = "未找到小组" });
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { msg = "错误的Id格式" });
            }
            return NoContent();
            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            //response.Content = new StringContent("成功", Encoding.UTF8);
            //return response;
        }
        //提交对其他小组的打分
        //PUT:/group/{groupId}/grade/presentation/{studentId}
        //[FromBody]dynamic presentation
        //[HttpPut("api/group/{groupId}/grade/presentation/{studentId}")]
        //public IActionResult SubmitPresentationGrade(long groupId, long studentId, long topicId, int grade)
        //{
        //    try
        //    {
        //        _gradeService.InsertGroupGradeByUserId(topicId, studentId, groupId, grade);
        //    }
        //    catch (GroupNotFoundException)
        //    {
        //        return StatusCode(404, new { msg = "未找到小组" });
        //    }
        //    catch (Xmu.Crms.Shared.Exceptions.InvalidOperationException)
        //    {
        //        return StatusCode(409, new { msg = "已评分，不能重复评分" });
        //    }
        //    catch (ArgumentException)
        //    {
        //        return StatusCode(400, new { msg = "错误的Id格式" });
        //    }
        //    return NoContent();
        //    //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
        //    //response.Content = new StringContent("成功",Encoding.UTF8);
        //    //return response;
        //}
        //提交对其他小组的打分
        //PUT:/group/{groupId}/grade/presentation/{studentId}
        //[FromBody]dynamic presentation
        [HttpPut("api/group/{groupId}/grade/presentation/{topicId}/{grade}")]
        public IActionResult SubmitPresentationGrade([FromRoute]long groupId, long topicId, int grade)
        {
            try
            {
                //if(groupId)
                _gradeService.InsertGroupGradeByUserId(topicId, User.Id(), groupId, grade);
            }
            catch (GroupNotFoundException)
            {
                return StatusCode(404, new { msg = "未找到小组" });
            }
            catch (Xmu.Crms.Shared.Exceptions.InvalidOperationException)
            {
                return StatusCode(409, new { msg = "已评分，不能重复评分" });
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { msg = "错误的Id格式" });
            }
            return NoContent();
            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            //response.Content = new StringContent("成功",Encoding.UTF8);
            //return response;
        }


        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //[HttpPut("api/group/{id}/grade/presentation")]
        //public IActionResult PostGrade(long id, [FromQuery]long topicId, [FromQuery]int grade)
        //{
        //    try
        //    {
        //        var userId = User.Id();
        //        _gradeService.InsertGroupGradeByUserId(topicId, userId, id, grade);
        //        return Json(new { status = 200 });
        //    }
        //    catch (GroupNotFoundException)
        //    {
        //        return StatusCode(404, new { msg = "没有找到该课程" });
        //    }
        //    catch (ArgumentException)
        //    {
        //        return StatusCode(400, new { msg = "组号格式错误" });
        //    }

        //}

        //[HttpPut("api/group/{groupId:long}/grade/presentation/{studentId:long}")]
        //public IActionResult SubmitStudentGradeByGroupId([FromBody] long groupId, [FromBody] long studentId,
        //   [FromBody] StudentScoreGroup updated)
        //{
        //    try
        //    {
        //        if (User.Type() != Shared.Models.Type.Student)
        //        {
        //            return StatusCode(403, new { msg = "权限不足" });
        //        }

        //        if (updated.Grade == null)
        //        {
        //            return NoContent();
        //        }

        //        _gradeService.InsertGroupGradeByUserId(updated.SeminarGroupTopic.Topic.Id, updated.Student.Id,
        //            groupId, (int)updated.Grade);
        //        return NoContent();
        //    }
        //    catch (GroupNotFoundException)
        //    {
        //        return StatusCode(404, new { msg = "没有找到该课程" });
        //    }
        //    catch (ArgumentException)
        //    {
        //        return StatusCode(400, new { msg = "组号格式错误" });
        //    }
        //}
    }
}

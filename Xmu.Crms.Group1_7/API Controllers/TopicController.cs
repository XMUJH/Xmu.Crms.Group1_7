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
    public class TopicController : Controller
    {
        private CrmsContext _db;
        ITopicService _topicService;
        ISeminarGroupService _seminarGroupService;

        public TopicController(CrmsContext db,ITopicService topicService, ISeminarGroupService seminarGroupService)
        {
            _db = db;
            _topicService = topicService;
            _seminarGroupService = seminarGroupService;
        }
        //按ID获取话题
        //GET: /topic/{topicId}
        [HttpGet("api/topic/{topicId}")]
        public IActionResult GetTopic(int topicId)
        {
            try
            {
                var topic = _topicService.GetTopicByTopicId(topicId);

                return Json(new
                {
                    id = topic.Id,
                    name = topic.Name,
                    description = topic.Description,
                    groupNumberLimit = topic.GroupNumberLimit,
                    groupStudentLimit = topic.GroupStudentLimit,
                    seminarName = topic.Seminar.Name
                });
            }
            catch(TopicNotFoundException)
            {
                return StatusCode(404, new { msg = "未找到话题" });
            }
            catch(ArgumentException)
            {
                return StatusCode(400, new { msg = "错误的Id格式" });
            }
            

            //var result = new JsonResult();
            //var data = new { id = 257, serial = "A", name = "领域模型与模块", description = "Domain model与模块划分",groupLimit = 5,groupMemberLimit = 6,groupLeft = 2};
            //result.Data = data;
            //return result;
        }

        //按ID修改话题
        //PUT:/topic/{topicId}
        // [FromBody]dynamic json
        [HttpPut("api/topic/{topicId}")]
        public IActionResult ModifyTopic(int topicId,Topic topic)
        {            
            try
            {
                _topicService.UpdateTopicByTopicId(topicId, topic);
            }
            catch (TopicNotFoundException)
            {
                return StatusCode(404, new { msg = "未找到话题" });
            }
            catch (ArgumentException)
            {
                return StatusCode(400,new { msg = "错误的Id格式" });
            }
            return NoContent();
            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);
            //response.Content = new StringContent("成功", Encoding.UTF8);
            //return response;
        }

        //按Id删除话题
        //DELETE:/topic/{topicId}
        [HttpDelete("api/topic/{topicId}")]
        public IActionResult DeleteTopic(int topicId)
        {
            try
            {
                _topicService.DeleteTopicByTopicId(topicId);
            }
            catch (TopicNotFoundException)
            {
                return StatusCode(404, new { msg = "未找到话题" });
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

        //按话题ID获取选择了该话题的小组
        //GET: /topic/{topicId}/group
        [HttpGet("api/topic/{topicId}/group")]
        public IActionResult GetGroup(int topicId)
        {
            try
            {
                var seminarGroups = _seminarGroupService.ListGroupByTopicId(topicId);
                return Json(seminarGroups.Select(c => new
                {
                    id = c.Id,
                    seminarName = c.Seminar.Name,
                    className = c.ClassInfo.Name,
                    report = c.Report,
                    reportGrade = c.ReportGrade,
                    presentaionGrade = c.PresentationGrade,
                    finalGrade = c.FinalGrade,
                    leaderName = c.Leader.Name
                }));
            }
            catch(SeminarNotFoundException)
            {
                return StatusCode(404, new { msg = "未找到讨论课" });
            }
            catch(ArgumentException)
            {
                return StatusCode(400, new { msg = "错误的Id格式" });
            }
            
            //var result = new JsonResult();
            //var data = new object[]
            //{
            //    new {id =23,name ="1A1"},
            //    new {id =26,name ="2A2"}
            //};
            //result.Data = data;
            //return result;
        }
    }
}

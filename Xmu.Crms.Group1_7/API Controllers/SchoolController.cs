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
    public class SchoolController : Controller
    {
        private CrmsContext _db;
        ISchoolService _schoolService;

        public SchoolController(CrmsContext db, ISchoolService schoolService)
        {
            _db = db;
            _schoolService = schoolService;
        }

        //获取学校列表（按照城市查找学校）
        //GET: /school?city={city}
        [HttpGet("api/school/city={city}")]
        public IActionResult GetSchool(string city)
        {
            try
            {
                var data = _schoolService.ListSchoolByCity(city);
                return Json(data.Select(c => new { id = c.Id, name = c.Name, province = c.Province, city = c.City }));
            }
            catch (NotImplementedException)
            {
                return StatusCode(404, "未找到学校");
            }
        }

        //添加学校
        //POST: /school
        [HttpPost("api/school")]
        public IActionResult PostSeminar([FromBody]dynamic json)
        {
            School school = new School { Name = json.schoolname, Province = json.province, City = json.city };
            long schoolId = 0;
            try
            {
                schoolId=_schoolService.InsertSchool(school);
                return Json(schoolId);
            }
            catch (NotImplementedException)
            {
                return StatusCode(404, "未找到添加的学校");
            }
        }

        //获取省份列表
        //GET: /school/province
        [HttpGet("api/school/province")]
        public IActionResult GetProvince([FromBody]dynamic json)
        {
            try
            {
                var data = _schoolService.ListProvince();
                return Json(data);
            }
            catch (NotImplementedException)
            {
                return StatusCode(404, "未找到省份");
            }
        }

        //获取城市列表
        //GET: /school/city?province={province}
        [HttpGet("api/school/city/province={province}")]
        public IActionResult GetCity(string province)
        {
            try
            {
                var data = _schoolService.ListCity(province);
                return Json(data);
            }
            catch (NotImplementedException)
            {
                return StatusCode(404, "未找到城市");
            }
        }
    }
}

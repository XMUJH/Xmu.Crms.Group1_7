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
    public class MeController : Controller
    {
        private CrmsContext _db;
        ILoginService _loginService;
        IUserService _userService;
        ISchoolService _schoolService;
        private readonly JwtHeader _header;

        public MeController(CrmsContext db, ILoginService loginService, IUserService userService, ISchoolService schoolService, JwtHeader header)
        {
            _db = db;
            _loginService = loginService;
            _userService = userService;
            _schoolService = schoolService;
            _header = header;
        }

        public class LoginModel
        {
            public string username { set; get; }
            public string password { set; get; }
        }


        [Route("/")]
        public IActionResult HomePage()
        {
            return Redirect("/GroupUI/GroupUI");
        }

        //获取当前用户
        // GET: /me
        [HttpGet("api/me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetCurrentUser()
        {
            try
            {
                UserInfo user = _userService.GetUserByUserId(User.Id());
                user.School = _schoolService.GetSchoolBySchoolId(user.SchoolId ?? -1);
                return Json(user);
            }
            catch (UserNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
        }

        //修改当前用户
        // PUT: /me
        [HttpPut("api/me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult PutID([FromBody]dynamic json)
        {
            try
            {
                if (json.userNumber != null)
                {
                    UserInfo bindInfo = new UserInfo();
                    bindInfo.Id = User.Id();
                    bindInfo.Number = json.userNumber;
                    bindInfo.Name = json.name;
                    bindInfo.Type = json.type;
                    bindInfo.Email = json.email;
                    bindInfo.Title = json.title;
                    bindInfo.Avatar = json.avatar;
                    string schoolName = json.school;
                    School school = _db.School.SingleOrDefault(temp => temp.Name == schoolName);
                    bindInfo.School = school;
                    bindInfo.SchoolId = school.Id;
                    _userService.UpdateUserByUserId(bindInfo.Id, bindInfo);
                    var userInfo = _userService.GetUserByUserId(User.Id());
                    return Json
                    (
                    new SigninResult
                    {
                        Id = userInfo.Id,
                        Name = userInfo.Name,
                        Type = userInfo.Type.ToString(),
                        Jwt = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(_header,
                        new JwtPayload
                        (
                            null,
                            null,
                            new[]
                            {
                                new Claim("id", userInfo.Id.ToString()),
                                new Claim("type", userInfo.Type.ToString().ToLower())
                            },
                            null,
                            DateTime.Now.AddDays(7)
                         )))
                    }
                    );
                }
                else
                {
                    if(User.Type() != Xmu.Crms.Shared.Models.Type.Teacher)
                    _loginService.DeleteStudentAccount(User.Id());
                    else _loginService.DeleteTeacherAccount(User.Id());
                    return NoContent();
                }
            }
            catch (UserNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
            catch (ArgumentException)
            {
                return StatusCode(400, "错误的ID格式");
            }
        }

        //.Net手机号密码登录
        // POST: /signin
        [HttpPost("api/me/signin")]
        public IActionResult PostSignin([FromBody]dynamic json)
        {
            UserInfo signInfo = new UserInfo();
            UserInfo userInfo = new UserInfo();
            signInfo.Phone = json.phone_id;
            signInfo.Password = json.password;
            try
            {
                userInfo = _loginService.SignInPhone(signInfo);
                HttpContext.SignInAsync(JwtBearerDefaults.AuthenticationScheme, new ClaimsPrincipal());
                return Json
                    (
                    new SigninResult
                    {
                        Id = userInfo.Id,
                        Name = userInfo.Name,
                        Type = userInfo.Type.ToString(),
                        Jwt = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(_header,
                        new JwtPayload
                        (
                            null,
                            null,
                            new[]
                            {
                                new Claim("id", userInfo.Id.ToString()),
                                new Claim("type", userInfo.Type.ToString().ToLower())
                            },
                            null,
                            DateTime.Now.AddDays(7)
                         )))
                    }
                    );
            }
            catch(PasswordErrorException e)
            {
                return StatusCode(401, e.GetAlertInfo());
            }
            catch (UserNotFoundException e)
            {
                return StatusCode(404, e.GetAlertInfo());
            }
        }

        //.Net手机号密码注册
        //POST:/me
        [HttpPost("api/me/register")]
        public IActionResult PostRegister([FromBody]dynamic json)
        {
            UserInfo signInfo = new UserInfo();
            UserInfo userInfo = new UserInfo();
            signInfo.Phone = json.phone_no;
            signInfo.Password = json.password_confirm;
            try
            {
                userInfo = _loginService.SignUpPhone(signInfo);
                HttpContext.SignInAsync(JwtBearerDefaults.AuthenticationScheme, new ClaimsPrincipal());
                return Json
                    (
                    new SigninResult
                    {
                        Id = userInfo.Id,
                        Name = userInfo.Name,
                        Type = userInfo.Type.ToString(),
                        Jwt = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(_header,
                        new JwtPayload
                        (
                            null,
                            null,
                            new[]
                            {
                                new Claim("id", userInfo.Id.ToString()),
                                new Claim("type", userInfo.Type.ToString().ToLower())
                            },
                            null,
                            DateTime.Now.AddDays(7)
                         )))
                    }
                    );
            }
            catch(PhoneAlreadyExistsException e)
            {
                return StatusCode(401, e.GetAlertInfo());
            }
        }

        public class SigninResult
        {
            public long Id { get; set; }

            public string Name { get; set; }

            public string Type { get; set; }

            public string Jwt { get; set; }
        }
    }
}

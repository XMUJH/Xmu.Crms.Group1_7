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
    public class UploadController : Controller
    {
        private CrmsContext _db;
        /*
        public UploadController(CrmsContext db)
        {
            _db = db;
        }
        */

        //上传头像
        //POST:/upload/avatar
        [HttpPost("api/upload/avatar")]
        public IActionResult ChoosePicture([FromBody]dynamic json)
        {
            try
            {
                var picture = new { url = json };
                return picture.url;
            }
            catch (NotImplementedException)
            {
                return StatusCode(404, new { msg = "未找到图片" });
            }
        }
    }
}

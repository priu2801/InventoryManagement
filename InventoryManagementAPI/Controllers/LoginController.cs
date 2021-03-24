using DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ServiceLayer.Interface;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Controllers
{
   // [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        #region Fields
        private IConfiguration _config;
        private readonly ILoggerService _loggerService;
        #endregion

        #region Ctor
        public LoginController(IConfiguration config, ILoggerService loggerService)
        {
            this._config = config;
            this._loggerService = loggerService;
        }
        #endregion

        [AllowAnonymous]
        [HttpPost]
        [SwaggerOperation(Tags = new[] { "Login Management" })]
        [Route("~/api/Login/Login")]
        public IActionResult Login([FromBody] User login)
        {
            IActionResult response = Unauthorized();
            try
            {
                var user = AuthenticateUser(login);

                if (user != null)
                {
                    var tokenString = GenerateJSONWebToken(user);
                    response = Ok(new { token = tokenString });
                }
            }
            catch (Exception ex)
            {
                _loggerService.Error(ex);
                return BadRequest();

            }
            return response;
        }

        private string GenerateJSONWebToken(User userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              null,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private User AuthenticateUser(User login)
        {
            User user = null;
            try
            {
                //Validate the User Credentials    
                //Demo Purpose, I have Passed HardCoded User Information    
                if (login.UserName == "Priya")
                {
                    user = new User { UserName = "Priya Chavadiya", Password = "test.btest@gmail.com" };
                }
            }
            catch (Exception ex)
            {
                _loggerService.Error(ex);
                return null;
            }
            return user;
        }
    }
}

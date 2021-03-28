using DataAccess;
using InventoryManagementAPI.Filter;
using Microsoft.AspNetCore.Authentication;
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
using System.Security.Claims;
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
        private readonly IJwtAuthManager _jwtAuthManager;
        private readonly IUserService _userService;

        #endregion

        #region Ctor
        public LoginController(IConfiguration config, ILoggerService loggerService, IJwtAuthManager jwtAuthManager, IUserService userService)
        {
            this._config = config;
            this._loggerService = loggerService;
            this._jwtAuthManager = jwtAuthManager;
            this._userService = userService;
        }
        #endregion

        [AllowAnonymous]
        [HttpPost]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [SwaggerOperation(Tags = new[] { "Login Management" })]
        [Route("~/api/v{version:apiVersion}/Login/Login")]
        public IActionResult Login([FromBody] User login)
        {
            IActionResult response = Unauthorized();
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                if (!_userService.IsValidUserCredentials(login.UserName, login.Password))
                {
                    return Unauthorized();
                }

                var claims = new[]
                {
                 new Claim("username", login.UserName),
                new Claim("userid", Convert.ToString(login.UserId))
                 };

                var jwtResult = _jwtAuthManager.GenerateTokens(login.UserName, claims, DateTime.Now);
                _loggerService.Info("User:" + login.UserName + "logged in the system.");
                return Ok(new
                {
                    UserName = login.UserName,
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = jwtResult.RefreshToken.TokenString
                });
            }
            catch (Exception ex)
            {
                _loggerService.Error(ex);
                return BadRequest();

            }
         
        }

        //private string GenerateJSONWebToken(User userInfo)
        //{
        //    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        //    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);


        //    var permClaims = new List<Claim>();
        //    permClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        //    permClaims.Add(new Claim("username", userInfo.UserName));
        //    permClaims.Add(new Claim("userid", Convert.ToString(userInfo.UserId)));
        

        //    var token = new JwtSecurityToken(_config["Jwt:Issuer"],
        //      _config["Jwt:Issuer"],
        //      permClaims,
        //      expires: DateTime.Now.AddMinutes(120),
        //      signingCredentials: credentials);

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}

        //private User AuthenticateUser(User login)
        //{
        //    User user = null;
        //    try
        //    {
        //        //Validate the User Credentials    
        //        //Demo Purpose, I have Passed HardCoded User Information    
        //        if (login.UserName == "Priya")
        //        {
        //            user = new User { UserName = "Priya Chavadiya", Password = "test.btest@gmail.com" };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _loggerService.Error(ex);
        //        return null;
        //    }
        //    return user;
        //}


        [HttpPost]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [SwaggerOperation(Tags = new[] { "Login Management" })]
        [Route("~/api/v{version:apiVersion}/Login/Logout")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult Logout()
        {
      
            var userName = User.Identity?.Name;
            _jwtAuthManager.RemoveRefreshTokenByUserName(userName);
            _loggerService.Info("User:" + userName  + "logged out from the system.");
            return Ok();
        }

        [HttpPost]
        [MapToApiVersion("1")]
        [MapToApiVersion("2")]
        [SwaggerOperation(Tags = new[] { "Login Management" })]
        [Route("~/api/v{version:apiVersion}/Login/RefreshToken")]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var userName = User.Identity?.Name;
                _loggerService.Info("User:" + userName + "is trying to refresh JWT token.");
              
                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return Unauthorized();
                }

                var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
                var jwtResult = _jwtAuthManager.Refresh(request.RefreshToken, accessToken, DateTime.Now);
                _loggerService.Info("User:" + userName + "has refreshed JWT token.");
                return Ok(new 
                {
                    UserName = userName,
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = jwtResult.RefreshToken.TokenString
                });
            }
            catch (SecurityTokenException e)
            {
                return Unauthorized(e.Message); // return 401 so that the client side can redirect the user to login page
            }
        }
    }
}

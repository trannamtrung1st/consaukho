using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CSK.Data;
using CSK.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CSK.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseController
    {
        private readonly UserManager<AppUsers> _userManager;
        public UsersController(CSKContext context,
            UserManager<AppUsers> userManager) : base(context)
        {
            _userManager = userManager;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginViewModel login)
        {
            try
            {
                AppUsers user;
                if (!_userManager.Users.Any())
                {
                    var result = _userManager.CreateAsync(new AppUsers
                    {
                        UserName = login.username
                    }, login.password).Result;
                    if (result.Succeeded)
                        user = _userManager.FindByNameAsync(login.username).Result;
                    else
                        return Error(new
                        {
                            message = "Có lỗi xảy ra. Vui lòng thử lại.",
                            data = result
                        });
                }
                else
                {
                    user = AuthenticateAsync(login.username, login.password).Result;
                    if (user == null)
                        return NotFound();
                }
                return Ok(GenerateTokenResponse(user).Result);
            }
            catch (Exception e)
            {
                return Error(new
                {
                    message = "Có lỗi xảy ra. Vui lòng thử lại.",
                    data = e
                });
            }
        }

        private async Task<IDictionary<string, object>> GenerateTokenResponse(AppUsers user)
        {
            var identity = new ClaimsIdentity("application");
            identity.AddClaim(new Claim(ClaimTypes.Name, user.Id));
            var claims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var r in roles)
                claims.Add(new Claim(ClaimTypes.Role, r));
            identity.AddClaims(claims);
            var principal = new ClaimsPrincipal(identity);
            var utcNow = DateTime.UtcNow;
            var props = new AuthenticationProperties()
            {
                IssuedUtc = utcNow,
                ExpiresUtc = utcNow.AddDays(7),
            };
            var ticket = new AuthenticationTicket(principal, props, "application");

            var resp = new Dictionary<string, object>();

            #region Generate JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.Default.GetBytes(JWT.SECRET_KEY);
            var issuer = JWT.ISSUER;
            var audience = JWT.AUDIENCE;
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, ticket.Principal.Identity.Name));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                Subject = identity,
                IssuedAt = ticket.Properties.IssuedUtc?.UtcDateTime,
                Expires = ticket.Properties.ExpiresUtc?.UtcDateTime,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                NotBefore = ticket.Properties.IssuedUtc?.UtcDateTime
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            #endregion

            resp["access_token"] = tokenString;
            resp["token_type"] = "bearer";
            if (ticket.Properties.ExpiresUtc != null)
                resp["expires_utc"] = ticket.Properties.ExpiresUtc;
            if (ticket.Properties.IssuedUtc != null)
                resp["issued_utc"] = ticket.Properties.IssuedUtc;
            resp["username"] = user.UserName;
            return resp;
        }

        private async Task<AppUsers> AuthenticateAsync(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null || !(await _userManager.CheckPasswordAsync(user, password)))
                return null;
            return user;
        }

    }

    public class LoginViewModel
    {
        public string username { get; set; }
        public string password { get; set; }
    }

}

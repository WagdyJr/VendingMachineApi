using ApiDemo.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VendingMachineAPI.Data;
using VendingMachineAPI.Models;

namespace VendingMachineAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;
        readonly ApiContext _context;
        public LoginController(IConfiguration configuration, ApiContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("Authenticate")]
        public async Task<ActionResult<LoginResponse>> Authenticate(LoginRequestDto login)
        {
            var loginResponse = new LoginResponse { };

            var user = _context.Users.FirstOrDefault(x => x.Username == login.Username);

            bool isUsernamePasswordValid = false;

            if (user != null)
            {
                isUsernamePasswordValid = user.Password == login.Password ? true : false;
            }
            if (isUsernamePasswordValid)
            {
                var role = user.Role.ToString().ToUpper();
                string token = CreateToken(login.Username, role);

                loginResponse.Token = token;

                return Ok(new { loginResponse });
            }
            else
            {
                return BadRequest("Username or Password Invalid!");
            }
        }

        private string CreateToken(string username, string role)
        {
            List<Claim> claims = new()
            {                    
                new Claim(ClaimTypes.Name, Convert.ToString(username)),
                new Claim("role", Convert.ToString(role))
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: cred
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}

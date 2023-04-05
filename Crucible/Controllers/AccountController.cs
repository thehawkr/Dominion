using System.Linq;
using System.Threading.Tasks;
using Crucible.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Crucible.Models;
using YourNamespace;

namespace Crucible.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AccountController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userExists = _context.Users.Any(u => u.Username == model.Username);
            if (userExists)
            {
                return Conflict(new { message = "User already exists" });
            }

            var user = new User
            {
                HostId = model.HostId,
                Username = model.Username,
                Domain = model.Domain,
                IsAdmin = model.IsAdmin,
            };
            user.SetPassword(model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User created successfully" });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);
            if (user == null)
            {
                return NotFound(new { message = "Invalid username or password" });
            }

            if (!user.ValidatePassword(model.Password))
            {
                return NotFound(new { message = "Invalid username or password" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(_config["JwtSettings:SecretKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("id", user.Id.ToString()) }),
                Issuer = _config["JwtSettings:Issuer"],
                Audience = _config["JwtSettings:Audience"],
                Expires = System.DateTime.UtcNow.AddMinutes(int.Parse(_config["JwtSettings:AccessTokenExpirationMinutes"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = RandomString(25),
                Expires = System.DateTime.UtcNow.AddDays(int.Parse(_config["JwtSettings:RefreshTokenExpirationDays"]))
            };
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken.Token });
        }

        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(System.Linq.Enumerable.Repeat(chars, length).Select(s => s[new System.Random().Next(s.Length)]).ToArray());
        }
    }
}

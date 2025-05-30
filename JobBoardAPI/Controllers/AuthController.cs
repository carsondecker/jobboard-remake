using JobBoardAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace JobBoardAPI.Controllers
{
    public record LoginCredentials(string Username, string Password);

    [ApiController]
    [Route("/api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] LoginCredentials creds)
        {
            var authResponse = await _authService.Register(creds.Username, creds.Password);

            switch (authResponse.Result)
            {
                case AuthResult.IllegalCredentials:
                    return BadRequest(new { message = "Login credentials do not fit requirements." });
                case AuthResult.UserAlreadyExists:
                    return BadRequest(new { message = "This username is already in use." });
                case AuthResult.Created:
                    // TODO: add a link to user with CreatedAtAction when that endpoint is created
                    return Created("", new
                    {
                        userId = authResponse.UserId,
                        username = authResponse.Username,
                        token = authResponse.Token
                    });
                case AuthResult.DBError:
                    return StatusCode(500, new { error = "A database server error occurred. Please try again later." });
                default:
                    return StatusCode(500, new { error = "A server error occurred. Please try again later." });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCredentials creds)
        {
            var authResponse = await _authService.Login(creds.Username, creds.Password);

            switch (authResponse.Result)
            {
                case AuthResult.UserNotFound:
                    return BadRequest(new { message = "User does not exist." });
                case AuthResult.IncorrectCredentials:
                    return BadRequest(new { message = "Incorrect login credentials." });
                case AuthResult.Ok:
                    return Ok(new
                    {
                        userId = authResponse.UserId,
                        username = authResponse.Username,
                        token = authResponse.Token
                    });
                case AuthResult.DBError:
                    return StatusCode(500, new { error = "A database server error occurred. Please try again later." });
                default:
                    return StatusCode(500, new { error = "A server error occurred. Please try again later." });
            }
        }
    }
}

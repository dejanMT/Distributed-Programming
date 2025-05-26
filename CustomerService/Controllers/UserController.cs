using CustomerService.Models;
using CustomerService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            var result = await _userService.Register(user);
            if (result == null) return BadRequest("User already exists.");
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User login)
        {
            var user = await _userService.Login(login.Email, login.Password);
            return user == null ? Unauthorized() : Ok(user);
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> GetUser(string email)
        {
            var user = await _userService.GetUserDetails(email);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpGet("{email}/notifications")]
        public async Task<IActionResult> GetUserNotifications(string email)
        {
            var user = await _userService.GetUserByEmail(email);
            if (user == null)
                return NotFound();

            if (user.Notifications == null)
            {
                return Ok(new List<Notification>());
            }
            return Ok(user.Notifications);
        }

    }
}

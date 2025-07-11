﻿using CustomerService.Models;
using CustomerService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Middleware;

namespace CustomerService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IJwtBuilder _jtwBuilder;
        private readonly IEncryptor _encryptor;

        public UserController(UserService userService, IJwtBuilder jwtbuilder, IEncryptor encryptor)
        {
            _userService = userService;
            _jtwBuilder = jwtbuilder;
            _encryptor = encryptor;
        }

        // tHIS is the endpoint for registering a new user
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            var result = await _userService.Register(user);
            if (result == null) return BadRequest("User already exists.");
            return Ok(result);
        }

        // This is the endpoint for logging in a user
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] UserDTO userDTO)
        {
            var user = await _userService.GetUserByEmail(userDTO.Email);

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Validate the password using the encryptor
            bool isValid = user.ValidatePassword(userDTO.Password, _encryptor);

            if (!isValid)
            {
                return BadRequest("Could not authenticate the user");
            }

            var token = _jtwBuilder.GetToken(user.Id);

            return Ok(token);
        }

        // This is the endpoint for getting user details by email
        [HttpGet("{email}")]
        public async Task<IActionResult> GetUser(string email)
        {
            var user = await _userService.GetUserDetails(email);
            return user == null ? NotFound() : Ok(user);
        }

        // This is the endpoint for getting user notifications by email
        [Authorize]
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

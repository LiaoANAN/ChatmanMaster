﻿using Chatman.Interfaces;
using Chatman.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Chatman.Helpers;
using Chatman.Models;
using Newtonsoft.Json.Linq;

namespace Chatman.Controllers
{
    public class UserController : WebController
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        #region //View
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
        #endregion

        #region //Get
        [HttpGet]
        public async Task<IActionResult> GetFriendsList()
        {
            try
            {
                var userInfo = WebHelper.GetCurrentUser(this.HttpContext);
                var friends = await _userService.GetFriendsByUserIdAsync(userInfo.UserId);

                return Ok(new
                {
                    success = true,
                    data = friends
                });
            }
            catch (Exception ex) 
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        #endregion

        #region //Add

        #endregion

        #region //Update

        #endregion

        #region //Delete

        #endregion

        #region //Login
        [HttpPost]
        public async Task<IActionResult> UserLogin([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _userService.LoginAsync(request);
                if (response.Success)
                {
                    return Ok(response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed");
                return StatusCode(500, new { message = "內部伺服器錯誤" });
            }
        }
        #endregion

        #region //Register
        [HttpPost]
        public async Task<IActionResult> UserRegister([FromBody] RegisterRequest request)
        {
            try
            {
                var response = await _userService.RegisterAsync(request);
                if (response.Success)
                {
                    return Ok(response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register failed");
                return StatusCode(500, new { message = "內部伺服器錯誤" });
            }
        }
        #endregion
    }
}

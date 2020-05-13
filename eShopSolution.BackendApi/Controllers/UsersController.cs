using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using eShopSolution.Application.System.Users;
using eShopSolution.ViewModel.Catalog.System.User;
using eShopSolution.ViewModel.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eShopSolution.BackendApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("authenticate")]
        [AllowAnonymous]
        public async Task<IActionResult> Authenticate([FromBody]LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = await _userService.Authenticate(request);

            if (string.IsNullOrEmpty(token))
                return BadRequest("Username or password is incorrect");

            return Ok(token);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody]RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.Register(request);

            if (!result.IsSuccess)
            {
                var validationErrors = ((ApiErrorResult<bool>)result).ValidationErrors;
                return BadRequest(JsonSerializer.Serialize(validationErrors));
            }
            return Ok(result);
        }

        // /users/paging
        [HttpGet("paging")]
        public async Task<IActionResult> GetUserPaging([FromQuery]GetUserPagingRequest request)
        {
            var users = await _userService.GetUserPaging(request);
            return Ok(users);
        }
    }
}
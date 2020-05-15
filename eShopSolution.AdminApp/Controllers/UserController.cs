using eShopSolution.AdminApp.Services;
using eShopSolution.ViewModel.Catalog.System.User;
using eShopSolution.ViewModel.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace eShopSolution.AdminApp.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserApiClient _userApiClient;
        private readonly IConfiguration _config;

        public UserController(
            IUserApiClient userApiClient,
            IConfiguration config
            )
        {
            _userApiClient = userApiClient;
            _config = config;
        }

        public async Task<IActionResult> Index(string keyword, int pageIndex = 1, int pageSize = 10)
        {
            var request = new GetUserPagingRequest
            {
                Keyword = keyword,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            var data = await _userApiClient.GetUserPaging(request);

            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var result = await _userApiClient.Create(request);

            if (result is ApiErrorResult<bool>)
            {
                var errors = ((ApiErrorResult<bool>)result).ValidationErrors;
                foreach (var item in errors)
                {
                    ModelState.AddModelError(item.Code, item.Message);
                }
                return View(request);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var result = await _userApiClient.GetById(id);
            if (!result.IsSuccessed)
            {
                return BadRequest(result.Message);
            }
            var user = result.ResultObject;
            var model = new UpdateUserRequest
            {
                Dob = user.Dob,
                Email = user.Email,
                FirstName = user.FirstName,
                Id = user.Id,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateUserRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var result = await _userApiClient.Update(request.Id, request);

            if (result is ApiErrorResult<bool>)
            {
                var errors = ((ApiErrorResult<bool>)result).ValidationErrors;
                if (errors != null)
                {
                    foreach (var item in errors)
                    {
                        ModelState.AddModelError(item.Code, item.Message);
                    }
                }
                ModelState.AddModelError("", ((ApiErrorResult<bool>)result).Message);

                return View(request);
            }

            return RedirectToAction("Index");
        }
    }
}
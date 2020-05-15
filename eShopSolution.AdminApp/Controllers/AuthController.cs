using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using eShopSolution.AdminApp.Services;
using eShopSolution.ViewModel.Catalog.System.User;
using eShopSolution.ViewModel.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

namespace eShopSolution.AdminApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserApiClient _userApiClient;
        private readonly IConfiguration _config;

        public AuthController(
            IUserApiClient userApiClient,
            IConfiguration config
            )
        {
            _userApiClient = userApiClient;
            _config = config;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var result = await _userApiClient.Authenticate(request);
            if (result is ApiErrorResult<string>)
            {
                var apiResult = (ApiErrorResult<string>)result;

                ModelState.AddModelError("", apiResult.Message);
                return View(request);
            }

            var token = result.ResultObject;

            var userPrincipal = ValidateToken(token);
            var authProperties = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                IsPersistent = true
            };

            HttpContext.Session.SetString("token", token);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal, authProperties);

            return RedirectToAction("index", "user");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.Session.Remove("token");

            return RedirectToAction("login", "auth");
        }

        private ClaimsPrincipal ValidateToken(string jwtToken)
        {
            IdentityModelEventSource.ShowPII = true;

            SecurityToken validatedToken;
            var validationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                ValidAudience = _config["Tokens:Issuer"],
                ValidIssuer = _config["Tokens:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]))
            };

            var principal = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out validatedToken);

            return principal;
        }
    }
}
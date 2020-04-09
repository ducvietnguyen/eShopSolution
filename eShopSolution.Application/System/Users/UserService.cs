using eShopSolution.Data.Entities;
using Microsoft.Extensions.Configuration;
using eShopSolution.ViewModel.Catalog.System;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace eShopSolution.Application.System.Users
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signinImanager;
        private readonly RoleManager<AppRole> _roleImanager;
        private readonly IConfiguration _config;

        public UserService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signinImanager,
            RoleManager<AppRole> roleImanager,
            IConfiguration config)
        {
            _signinImanager = signinImanager;
            _userManager = userManager;
            _roleImanager = roleImanager;
            _config = config;
        }

        public async Task<string> Authenticate(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null)
            {
                return null;
            }

            var signinResult = await _signinImanager.PasswordSignInAsync(user, request.Password, request.RememberMe, true);
            if (!signinResult.Succeeded)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new[] {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Role,roles!=null? string.Join(";",roles): null)
            };
            var secretKey = _config["Tokens:Key"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Tokens:Issuer"],
               _config["Tokens:Issuer"],
               claims,
               expires: DateTime.Now.AddHours(3),
               signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> Register(RegisterRequest request)
        {
            var user = new AppUser
            {
                Dob = request.Dob,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.UserName,
                PhoneNumber = request.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            return result.Succeeded;
        }
    }
}
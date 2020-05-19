using eShopSolution.Data.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using eShopSolution.ViewModel.Catalog.System.User;
using eShopSolution.ViewModel.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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

        public async Task<ApiResult<string>> Authenticate(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null)
            {
                return new ApiErrorResult<string>($"Username '{request.UserName}' is not found");
            }

            var signinResult = await _signinImanager.PasswordSignInAsync(user, request.Password, request.RememberMe, true);
            if (!signinResult.Succeeded)
            {
                return new ApiErrorResult<string>($"Username or password incorrect");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new[] {
                 new Claim(ClaimTypes.Name, user.UserName),
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

            return new ApiSuccessResult<string>(new JwtSecurityTokenHandler().WriteToken(token));
        }

        public async Task<ApiResult<UserVm>> GetById(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return new ApiErrorResult<UserVm>("User is not found");

            var userVm = new UserVm
            {
                Dob = user.Dob,
                Email = user.Email,
                FirstName = user.FirstName,
                Id = user.Id,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName
            };

            return new ApiSuccessResult<UserVm>(userVm);
        }

        public async Task<ApiResult<bool>> Delete(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
                return new ApiErrorResult<bool>("User is not found");

            await _userManager.DeleteAsync(user);

            return new ApiSuccessResult<bool>();
        }

        public async Task<PagedResult<UserVm>> GetUserPaging(GetUserPagingRequest request)
        {
            //1. select joint
            var query = _userManager.Users;

            //2. filter
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(
                    m => m.UserName.Contains(request.Keyword)
                    || m.Email.Contains(request.Keyword)
                    || m.FirstName.Contains(request.Keyword)
                    || m.LastName.Contains(request.Keyword)
                    || m.PhoneNumber.Contains(request.Keyword)
                    || m.UserName.Contains(request.Keyword));
            }

            //3. paging
            int totalRow = await query.CountAsync();
            var data = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(m => new UserVm()
                {
                    Id = m.Id,
                    Dob = m.Dob,
                    Email = m.Email,
                    FirstName = m.FirstName,
                    LastName = m.LastName,
                    PhoneNumber = m.PhoneNumber,
                    UserName = m.UserName
                }).ToListAsync();

            //4. select projection
            var pageResult = new PagedResult<UserVm>()
            {
                TotalRecords = totalRow,
                PageSize = request.PageSize,
                PageIndex = request.PageIndex,
                Items = data,
            };

            return pageResult;
        }

        public async Task<ApiResult<bool>> Register(RegisterRequest request)
        {
            if (await _userManager.Users.AnyAsync(m => m.Email == request.Email))
            {
                return new ApiErrorResult<bool>($"Email '{request.Email}' is already taken");
            }

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

            if (result.Errors.Any())
            {
                return new ApiErrorResult<bool>(result.Errors.Select(m => new ErrorValidationVm { Code = m.Code, Message = m.Description }).ToList());
            }
            return new ApiSuccessResult<bool>(true);
        }

        public async Task<ApiResult<bool>> Update(Guid id, UpdateUserRequest request)
        {
            if (await _userManager.Users.AnyAsync(m => m.Email == request.Email && m.Id != id))
            {
                return new ApiErrorResult<bool>($"Email '{request.Email}' is already taken");
            }
            var user = await _userManager.FindByIdAsync(id.ToString());

            user.Dob = request.Dob;
            user.Email = request.Email;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;

            user.PhoneNumber = request.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Errors.Any())
            {
                return new ApiErrorResult<bool>(result.Errors.Select(m => new ErrorValidationVm { Code = m.Code, Message = m.Description }).ToList());
            }
            return new ApiSuccessResult<bool>(true);
        }
    }
}
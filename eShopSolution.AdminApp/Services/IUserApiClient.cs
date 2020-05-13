using eShopSolution.ViewModel.Catalog.System.User;
using eShopSolution.ViewModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopSolution.AdminApp.Services
{
    public interface IUserApiClient
    {
        Task<string> Authenticate(LoginRequest request);

        Task<PagedResult<UserVm>> GetUserPaging(GetUserPagingRequest request);

        Task<ApiResult<bool>> Create(RegisterRequest request);
    }
}
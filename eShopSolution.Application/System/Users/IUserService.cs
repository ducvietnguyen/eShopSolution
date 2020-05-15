using eShopSolution.ViewModel.Catalog.System.User;
using eShopSolution.ViewModel.Common;
using System;
using System.Threading.Tasks;

namespace eShopSolution.Application.System.Users
{
    public interface IUserService
    {
        Task<ApiResult<string>> Authenticate(LoginRequest request);

        Task<ApiResult<bool>> Register(RegisterRequest request);

        Task<PagedResult<UserVm>> GetUserPaging(GetUserPagingRequest request);

        Task<ApiResult<bool>> Update(Guid id, UpdateUserRequest request);

        Task<ApiResult<UserVm>> GetById(Guid id);
    }
}
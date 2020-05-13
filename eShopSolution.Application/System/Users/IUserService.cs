using eShopSolution.ViewModel.Catalog.System.User;
using eShopSolution.ViewModel.Common;
using System.Threading.Tasks;

namespace eShopSolution.Application.System.Users
{
    public interface IUserService
    {
        Task<string> Authenticate(LoginRequest request);

        Task<ApiResult<bool>> Register(RegisterRequest request);

        Task<PagedResult<UserVm>> GetUserPaging(GetUserPagingRequest request);
    }
}
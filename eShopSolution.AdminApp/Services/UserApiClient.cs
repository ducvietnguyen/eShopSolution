using eShopSolution.ViewModel.Catalog.System.User;
using eShopSolution.ViewModel.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace eShopSolution.AdminApp.Services
{
    public class UserApiClient : IUserApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserApiClient(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResult<string>> Authenticate(LoginRequest request)
        {
            var jsonRequest = JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);

            var response = await client.PostAsync("/api/users/authenticate", httpContent);

            var resultContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<ApiErrorResult<string>>(resultContent);
            }
            return JsonConvert.DeserializeObject<ApiSuccessResult<string>>(resultContent);
        }

        public async Task<PagedResult<UserVm>> GetUserPaging(GetUserPagingRequest request)
        {
            var token = GetToken();
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"/api/users/paging?pageIndex={request.PageIndex}&pageSize={request.PageSize}&keyWord={request.Keyword}");
            var body = await response.Content.ReadAsStringAsync();

            var json = JsonConvert.DeserializeObject<PagedResult<UserVm>>(body);

            return json;
        }

        public async Task<ApiResult<bool>> Create(RegisterRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);

            var jsonRequest = JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/users", httpContent);

            var resultContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                var result = new ApiErrorResult<bool>(JsonConvert.DeserializeObject<List<ErrorValidationVm>>(errorMessage));
                return result;
            }

            return new ApiResult<bool>();
        }

        public async Task<ApiResult<bool>> Update(Guid id, UpdateUserRequest request)
        {
            var token = GetToken();
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonRequest = JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/users/{id}", httpContent);

            if (!response.IsSuccessStatusCode)
            {
                var resultContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ApiErrorResult<bool>>(resultContent);
            }
            return new ApiResult<bool>();
        }

        public async Task<ApiResult<UserVm>> GetById(Guid id)
        {
            var token = GetToken();

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"/api/users/{id}");
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<ApiErrorResult<UserVm>>(body);
            }

            var json = JsonConvert.DeserializeObject<ApiSuccessResult<UserVm>>(body);

            return json;
        }

        public async Task<ApiResult<bool>> Delete(Guid id)
        {
            var token = GetToken();

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"/api/users/{id}");
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<ApiErrorResult<bool>>(body);
            }

            var json = JsonConvert.DeserializeObject<ApiSuccessResult<bool>>(body);

            return json;
        }

        private string GetToken()
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("token");
            return token;
        }
    }
}
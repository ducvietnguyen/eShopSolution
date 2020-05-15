using System;
using System.Collections.Generic;
using System.Text;

namespace eShopSolution.ViewModel.Common
{
    public class ApiErrorResult<T> : ApiResult<T>
    {
        public List<ErrorValidationVm> ValidationErrors { get; set; }

        public ApiErrorResult(List<ErrorValidationVm> validationErrors)
        {
            ValidationErrors = validationErrors;
            IsSuccessed = false;
        }

        public ApiErrorResult(string message)
        {
            Message = message;
            IsSuccessed = false;
        }

        public ApiErrorResult()
        {
        }
    }
}
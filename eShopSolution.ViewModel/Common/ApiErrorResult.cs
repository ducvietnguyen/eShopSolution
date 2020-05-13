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
            IsSuccess = false;
        }

        public ApiErrorResult(string message)
        {
            Message = message;
            IsSuccess = false;
        }
    }
}
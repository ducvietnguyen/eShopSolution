using System;
using System.Collections.Generic;
using System.Text;

namespace eShopSolution.ViewModel.Common
{
    public class ApiSuccessResult<T> : ApiResult<T>
    {
        public ApiSuccessResult(T resultObject)
        {
            ResultObject = resultObject;
            IsSuccessed = true;
        }

        public ApiSuccessResult()
        {
            IsSuccessed = true;
        }
    }
}
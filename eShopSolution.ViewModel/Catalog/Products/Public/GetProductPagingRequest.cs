using eShopSolution.ViewModel.Common;

namespace eShopSolution.ViewModel.Catalog.Products.Public
{
    public class GetProductByCategoryPagingRequest : PagingRequestBase
    {
        public int? CategoryId { get; set; }
    }
}

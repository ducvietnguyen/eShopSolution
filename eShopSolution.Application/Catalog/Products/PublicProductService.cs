
using eShopSolution.Data.EF;
using eShopSolution.ViewModel.Catalog.Products;
using eShopSolution.ViewModel.Catalog.Products.Public;
using eShopSolution.ViewModel.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopSolution.Application.Catalog.Products
{
    public class PublicProductService : IPublicProductService
    {
        private readonly EShopDbContext _context;
        public PublicProductService(EShopDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ProductViewModel>> GetAllByCategoryId(GetProductByCategoryPagingRequest request)
        {
            //1. select joint
            var query = from p in _context.Products
                        join pt in _context.ProductTranslations on p.Id equals pt.ProductId
                        join pic in _context.ProductInCategories on p.Id equals pic.ProductId
                        join c in _context.Categories on pic.CategoryId equals c.Id
                        select new { p, pt, pic };

            //2. filter            
            if (request.CategoryId.HasValue && request.CategoryId > 0)
            {
                query = query.Where(m => m.pic.CategoryId == request.CategoryId);
            }

            //3. paging
            int totalRow = await query.CountAsync();
            var data = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(m => new ProductViewModel()
                {
                    Id = m.p.Id,
                    Name = m.pt.Name,
                    DateCreated = m.p.DateCreated,
                    Description = m.pt.Description,
                    Details = m.pt.Details,
                    LanguageId = m.pt.LanguageId,
                    Price = m.p.Price,
                    OriginalPrice = m.p.OriginalPrice,
                    SeoAlias = m.pt.SeoAlias,
                    SeoDescription = m.pt.SeoDescription,
                    SeoTitle = m.pt.SeoTitle,
                    Stock = m.p.Stock,
                    ViewCount = m.p.ViewCount

                }).ToListAsync();

            //4. select projection
            var pageResult = new PagedResult<ProductViewModel>()
            {
                TotalRecords = totalRow,
                Items = data
            };

            return pageResult;
        }
    }
}

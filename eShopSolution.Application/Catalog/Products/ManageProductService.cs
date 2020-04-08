using eShopSolution.Application.Common;
using eShopSolution.Data.EF;
using eShopSolution.Data.Entities;
using eShopSolution.Utilities.Exceptions;
using eShopSolution.ViewModel.Catalog.Products;
using eShopSolution.ViewModel.Catalog.Products.Manage;
using eShopSolution.ViewModel.Catalog.Products.Public;
using eShopSolution.ViewModel.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace eShopSolution.Application.Catalog.Products
{
    public class ManageProductService : IManageProductService
    {
        private readonly EShopDbContext _context;
        private readonly IStorageService _storageService;
        public ManageProductService(EShopDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        public async Task AddViewCount(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            product.ViewCount++;
            await _context.SaveChangesAsync();
        }

        public async Task<int> Create(ProductCreateRequest request)
        {
            var product = new Product
            {
                Price = request.Price,
                OriginalPrice = request.OriginalPrice,
                Stock = request.Stock,
                ViewCount = 0,
                DateCreated = DateTime.Now,
                ProductTranslations = new List<ProductTranslation>{
                    new ProductTranslation
                    {
                        Name= request.Name,
                        Description= request.Description,
                        Details = request.Details,
                        SeoDescription = request.SeoDescription,
                        SeoAlias = request.SeoAlias,
                        SeoTitle = request.SeoTitle,
                        LanguageId = request.LanguageId
                    }
                }
            };

            // save images
            if (request.ThumbnailImage != null)
            {
                product.ProductImages = new List<ProductImage> {
                    new ProductImage
                    {
                        Caption= "Thumbnail image",
                        DateCreated = DateTime.Now,
                        FileSize = request.ThumbnailImage.Length,
                        ImagePath = await this.SaveFile(request.ThumbnailImage)
                    }
                };
            }
            _context.Products.Add(product);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                throw new EShopException($"Cannot find a product: {id}");

            var images = _context.ProductImages.Where(m => m.ProductId == id);

            foreach (var image in images)
            {
                await _storageService.DeleteFileAsync(image.ImagePath);
            }

            _context.Products.Remove(product);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> Update(ProductUpdateRequest request)
        {
            var product = await _context.Products.FindAsync(request.Id);
            var productTranslation = await _context.ProductTranslations.FirstOrDefaultAsync(m => m.ProductId == request.Id && m.LanguageId == request.LanguageId);

            if (product == null || productTranslation == null)
                throw new EShopException($"Cannot find a product: {request.Id}");


            productTranslation.Name = request.Name;
            productTranslation.SeoAlias = request.SeoAlias;
            productTranslation.SeoDescription = request.SeoDescription;
            productTranslation.SeoTitle = request.SeoTitle;
            productTranslation.Description = request.Description;
            productTranslation.Details = request.Details;

            // save images
            if (request.ThumbnailImage != null)
            {
                var thumbnailsImage = await _context.ProductImages.FirstOrDefaultAsync(m => m.ProductId == request.Id && m.IsDefault);
                if (thumbnailsImage != null)
                {
                    thumbnailsImage.FileSize = request.ThumbnailImage.Length;
                    thumbnailsImage.ImagePath = await this.SaveFile(request.ThumbnailImage);
                    _context.ProductImages.Update(thumbnailsImage);
                }

            }

            return await _context.SaveChangesAsync();

        }


        public async Task<PagedResult<ProductViewModel>> GetAllPaging(GetProductPagingRequest pagingRequest)
        {
            //1. select joint
            var query = from p in _context.Products
                        join pt in _context.ProductTranslations on p.Id equals pt.ProductId
                        join pic in _context.ProductInCategories on p.Id equals pic.ProductId
                        join c in _context.Categories on pic.CategoryId equals c.Id
                        select new { p, pt, pic };

            //2. filter
            if (!string.IsNullOrEmpty(pagingRequest.Keyword))
            {
                query = query.Where(m => m.pt.Name.Contains(pagingRequest.Keyword));
            }

            if (pagingRequest.CategoryIds.Count > 0)
            {
                query = query.Where(m => pagingRequest.CategoryIds.Contains(m.pic.CategoryId));
            }

            //3. paging
            int totalRow = await query.CountAsync();
            var data = await query
                .Skip((pagingRequest.PageIndex - 1) * pagingRequest.PageSize)
                .Take(pagingRequest.PageSize)
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

        public async Task<bool> UpdatePrice(int productId, decimal price)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new EShopException($"Cannot find a product: {productId}");

            product.Price = price;
            _context.Products.Update(product);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateStock(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new EShopException($"Cannot find a product: {productId}");

            product.Stock += quantity;
            _context.Products.Update(product);
            return await _context.SaveChangesAsync() > 0;
        }


        private async Task<string> SaveFile(IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            return fileName;
        }

        public Task<int> AddImages(int productId, List<IFormFile> files)
        {
            throw new NotImplementedException();
        }

        public async Task<int> RemoveImage(int imageId)
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image == null)
                throw new EShopException($"Cannot find image: {imageId}");
            
            await _storageService.DeleteFileAsync(image.ImagePath);
           
            _context.ProductImages.Remove(image);

            return await _context.SaveChangesAsync();

        }

        public async Task<int> UpdateImage(int imageId, string caption, bool isDefault)
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image == null)
                throw new EShopException($"Cannot find image: {imageId}");


            image.Caption = caption;
            image.IsDefault = isDefault;

            _context.ProductImages.Update(image);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<ProductImageViewModel>> GetImages(int productId)
        {
            var images = _context.ProductImages.Where(m => m.ProductId == productId);
            
            if (images == null)
                throw new EShopException($"Cannot find images of product: {productId}");
            
            return await images.Select(m => new ProductImageViewModel
            {
                Id = m.Id,
                FilePath = m.ImagePath,
                FileSize = m.FileSize,
                IsDefault = m.IsDefault
            }).ToListAsync();

        }

    }
}

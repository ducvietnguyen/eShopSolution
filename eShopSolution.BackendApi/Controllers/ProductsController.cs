using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eShopSolution.Application.Catalog.Products;
using eShopSolution.ViewModel.Catalog.ProductImages;
using eShopSolution.ViewModel.Catalog.Products;
using Microsoft.AspNetCore.Mvc;

namespace eShopSolution.BackendApi.Controllers
{
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        private readonly IPublicProductService _publicProductService;
        private readonly IManageProductService _manageProductService;

        public ProductsController(IPublicProductService publicProductService, IManageProductService manageProductService)
        {
            _publicProductService = publicProductService;
            _manageProductService = manageProductService;
        }

        // /product/public-paging
        [HttpGet("{languageId}")]
        public async Task<IActionResult> GetAllPaging(string languageId, [FromForm]GetPublicProductPagingRequest request)
        {
            var products = await _publicProductService.GetAllByCategoryId(languageId, request);
            return Ok(products);
        }

        // ../product/1
        [HttpGet("{productId}/{languageId}")]
        public async Task<IActionResult> GetById(int productId, string languageId)
        {
            var products = await _manageProductService.GetById(productId, languageId);
            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm]ProductCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var productId = await _manageProductService.Create(request);
            if (productId == 0)
                return BadRequest();
            var product = await _manageProductService.GetById(productId, request.LanguageId);
            return CreatedAtAction(nameof(GetById), productId, product);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromForm]ProductUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var effectedResult = await _manageProductService.Update(request);
            if (effectedResult == 0)
                return BadRequest();
            return Ok();
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> Delete(int productId)
        {
            var effectedResult = await _manageProductService.Delete(productId);
            if (effectedResult == 0)
                return BadRequest();
            return Ok();
        }

        [HttpPatch("price/{productId}/{newPrice}")]
        public async Task<IActionResult> UpdatePrice(int productId, decimal newPrice)
        {
            var isSuccessful = await _manageProductService.UpdatePrice(productId, newPrice);
            if (!isSuccessful)
                return BadRequest();
            return Ok();
        }

        #region Images

        [HttpPost("{productId}/images")]
        public async Task<IActionResult> CreateImage(int productId, [FromForm]ProductImageCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var imageId = await _manageProductService.AddImage(productId, request);
            if (imageId == 0)
                return BadRequest();
            var image = await _manageProductService.GetImageById(imageId);
            return CreatedAtAction(nameof(GetImageById), productId, image);
        }

        [HttpGet("{productId}/images/{imageId}")]
        public async Task<IActionResult> GetImageById(int imageId)
        {
            var image = await _manageProductService.GetImageById(imageId);
            return Ok(image);
        }

        [HttpPut("{productId}/images/{imageId}")]
        public async Task<IActionResult> UpdateImage(int imageId, [FromForm]ProductImageUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var effectedResult = await _manageProductService.UpdateImage(imageId, request);
            if (effectedResult == 0)
                return BadRequest();
            return Ok();
        }

        [HttpDelete("{productId}/images/{imageId}")]
        public async Task<IActionResult> RemoveImage(int imageId)
        {
            var effectedResult = await _manageProductService.RemoveImage(imageId);
            if (effectedResult == 0)
                return BadRequest();
            return Ok();
        }

        [HttpGet("{productId}/images")]
        public async Task<IActionResult> GetImagesByProductId(int productId)
        {
            var data = await _manageProductService.GetImagesByProductId(productId);

            return Ok(data);
        }

        #endregion Images
    }
}
using EdunextG1.DTO;
using EdunextG1.Models;
using EdunextG1.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EdunextG1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IBlobService _blobService;
        private readonly string _containerName;

        public ProductController(IProductService productService, IBlobService blobService)
        {
            _productService = productService;
            _blobService = blobService;
        }

        [HttpGet]
        [AllowAnonymous] // Cho phép tất cả mọi người truy cập đường dẫn này
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                return Ok(new
                {
                    data = products,
                    status = 200,
                    message = "Success ! Show all Products"
                });
            } catch(Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = "Product not found" });
                }
                return Ok(new
                {
                    data = product,
                    status = 200,
                    message = "Success ! Show Product by Id"
                });
            } catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] ProductCreateDto productDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var imageUrl = await _blobService.UploadBlobAsync(productDto.Image);
                    var product = new Product
                    {
                        Name = productDto.Name,
                        Description = productDto.Description,
                        Price = productDto.Price,
                        ImageUrl = imageUrl,
                    };
                    var createdProduct = await _productService.CreateProductAsync(product);
                    return CreatedAtAction
                        (
                            nameof(GetProductById), 
                            new { id = createdProduct.Id }, 
                            createdProduct
                        );
                }
                return BadRequest(new { message = "Invalid model" });
            } catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductUpdateDto productDto)
        {
            try
            {
                if (id != productDto.Id)
                {
                    return BadRequest(new { message = "Id does not match" });
                }

                if (ModelState.IsValid)
                {
                    var product = await _productService.GetProductByIdAsync(id);
                    if (product == null)
                    {
                        return NotFound(new { message = "Product not found" });
                    }

                    if (productDto.Image != null)
                    {
                        var imageUrl = await _blobService.UploadBlobAsync(productDto.Image);
                        product.ImageUrl = imageUrl;
                    }
                    product.Name = productDto.Name;
                    product.Description = productDto.Description;
                    product.Price = productDto.Price;

                    var updateProduct = await _productService.UpdateProductAsync(product);
                    return Ok(new
                    {
                        data = updateProduct,
                        status = 201,
                        message = "Success ! Update Product"
                    });
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = "Product not found" });
                }

                await _productService.DeleteProductAsync(id);
                await _blobService.DeleteBlobAsync(product.ImageUrl);
                return Ok(new
                {
                    message = "Product deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Can not find ID or Error to DELETE" });
            }
        }
    }
}

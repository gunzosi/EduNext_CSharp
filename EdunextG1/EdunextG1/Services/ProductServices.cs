using EdunextG1.Models;
using EdunextG1.Repository.IRepository;
using EdunextG1.Services.IServices;

namespace EdunextG1.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllProducts();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _productRepository.GetProductById(id);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            return await _productRepository.AddProduct(product);
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            return await _productRepository.UpdateProduct(product);
        }

        public async Task DeleteProductAsync(int id)
        {
            await _productRepository.DeleteProduct(id);
        }
    }
}

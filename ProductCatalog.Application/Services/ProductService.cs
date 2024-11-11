using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Interfaces.Repositories;
using ProductCatalog.Application.Interfaces.Services;
using ProductCatalog.Common.Helpers;
using ProductCatalog.Common.Mappings;
using ProductCatalog.Domain.Entities;
using System.Data;
using System.Globalization;

namespace ProductCatalog.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;

        }

        public async Task<List<string>> ImportUpsertProductsAsync(IFormFile file)
        {
            var errors = new List<string>();

            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            }))
            {
                csv.Context.RegisterClassMap<ProductMap>();

                var validationErrors = ProcessAndValidateRecords(csv);

                if (validationErrors.errors.Any())
                {
                    return validationErrors.errors;
                }

                var dataTable = DataTableHelper.ToDataTable(validationErrors.validProducts);
                await _productRepository.ImportUpsertProductsAsync(dataTable);
            }

            return errors;
        }

        private (List<Product> validProducts, List<string> errors) ProcessAndValidateRecords(CsvReader csv)
        {
            var validProducts = new List<Product>();
            var errors = new List<string>();

            foreach (var product in csv.GetRecords<Product>())
            {
                var validationErrors = ValidateProduct(product);
                if (!validationErrors.Any())
                {
                    validProducts.Add(product);
                }
                else
                {
                    errors.Add($"Product '{product.Name ?? "Unnamed"}': {string.Join(", ", validationErrors)}");
                }
            }

            return (validProducts, errors);
        }

        private List<string> ValidateProduct(Product product)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(product.Name))
                errors.Add("Product name is required.");

            if (product.Price < 0)
                errors.Add("Price must be non-negative.");

            if (product.InventoryLevel < 0)
                errors.Add("Inventory level must be non-negative.");

            if (product.CategoryId == Guid.Empty)
                errors.Add("CategoryId is invalid.");

            return errors;
        }

        public async Task<IEnumerable<ProductDto>> GetProductsAsync(ProductFilterDto filter)
        {
            var products = await _productRepository.GetFilteredProductsAsync(
                filter.Name, filter.MinPrice, filter.MaxPrice, filter.CategoryId,
                filter.PageNumber, filter.PageSize);

            return products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                InventoryLevel = p.InventoryLevel,
                CategoryId = p.CategoryId,
                ImageUrl = p.ImageUrl,
                RowVersion = p.RowVersion
            });
        }

        public async Task<ProductDto> GetProductByIdAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) throw new KeyNotFoundException("Product not found");

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                InventoryLevel = product.InventoryLevel,
                CategoryId = product.CategoryId,
                ImageUrl = product.ImageUrl,
                RowVersion = product.RowVersion
            };
        }

        public async Task<ProductDto?> CreateProductAsync(ProductDto productDto)
        {
            if (productDto == null) throw new ArgumentNullException(nameof(productDto));

            var category = await _categoryRepository.GetByIdAsync(productDto.CategoryId);
            if (category == null)
            {
                return null;
            }

            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                InventoryLevel = productDto.InventoryLevel,
                CategoryId = productDto.CategoryId,
                ImageUrl = productDto.ImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            var createdProduct = await _productRepository.AddAsync(product);

            if (createdProduct == null)
            {
                return null;
            }

            return new ProductDto
            {
                Id = createdProduct.Id,
                Name = createdProduct.Name,
                Description = createdProduct.Description,
                Price = createdProduct.Price,
                InventoryLevel = createdProduct.InventoryLevel,
                CategoryId = createdProduct.CategoryId,
                ImageUrl = createdProduct.ImageUrl,
                RowVersion = createdProduct.RowVersion
            };
        }

        public async Task UpdateProductAsync(ProductDto productDto)
        {
            if (productDto == null) throw new ArgumentNullException(nameof(productDto));

            var product = await _productRepository.GetByIdAsync(productDto.Id);
            if (product == null) throw new KeyNotFoundException("Product not found");

            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.Price = productDto.Price;
            product.InventoryLevel = productDto.InventoryLevel;
            product.CategoryId = productDto.CategoryId;
            product.ImageUrl = productDto.ImageUrl;
            product.UpdatedAt = DateTime.UtcNow;
            product.RowVersion = productDto.RowVersion;

            await _productRepository.UpdateAsync(product);
        }

        public async Task DeleteProductAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) throw new KeyNotFoundException("Product not found");

            await _productRepository.DeleteAsync(product);
        }
    }
}

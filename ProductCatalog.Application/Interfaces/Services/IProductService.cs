﻿using Microsoft.AspNetCore.Http;
using ProductCatalog.Application.DTOs;

namespace ProductCatalog.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<List<string>> ImportUpsertProductsAsync(IFormFile file);
        Task<IEnumerable<ProductDto>> GetProductsAsync(ProductFilterDto filter);
        Task<ProductDto> GetProductByIdAsync(Guid id);
        Task<ProductDto?> CreateProductAsync(ProductDto productDto);
        Task UpdateProductAsync(ProductDto productDto);
        Task DeleteProductAsync(Guid id);
    }
}

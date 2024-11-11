﻿using ProductCatalog.Application.DTOs;

namespace ProductCatalog.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProductsAsync(ProductFilterDto filter);
        Task<ProductDto> GetProductByIdAsync(Guid id);
        Task<ProductDto?> CreateProductAsync(ProductDto productDto);
        Task UpdateProductAsync(ProductDto productDto);
        Task DeleteProductAsync(Guid id);
    }
}

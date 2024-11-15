﻿using Microsoft.EntityFrameworkCore;
using ProductCatalog.Application.Interfaces.Repositories;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Infrastructure.Data;

namespace ProductCatalog.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ProductCatalogContext context) : base(context)
        {
        }

        public async Task<Category?> GetCategoryWithProductsAsync(Guid id)
        {
            var categories = Include(c => c.Products);

            return await categories.FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}

using FluentAssertions;
using Newtonsoft.Json;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Tests.IntegrationTests.Constants;
using System.Net;
using System.Text;

namespace ProductCatalog.Tests.IntegrationTests.Helpers
{
    public static class TestHelpers
    {
        public static async Task<CategoryDto> CreateCategoryAndVerifyAsync(HttpClient client, CategoryDto category)
        {
            // Act
            var response = await client.PostAsync(BaseUrl.CategoryAPI, new StringContent(
                JsonConvert.SerializeObject(category),
                Encoding.UTF8,
                "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var content = await response.Content.ReadAsStringAsync();
            var createdCategory = JsonConvert.DeserializeObject<CategoryDto>(content);

            createdCategory.Should().NotBeNull();
            createdCategory!.Name.Should().Be(category.Name);
            createdCategory.Description.Should().Be(category.Description);
            createdCategory.Id.Should().NotBeEmpty();

            return createdCategory;
        }

        public static async Task<ProductDto> CreateProductAndVerifyAsync(HttpClient client, ProductDto product)
        {
            var response = await client.PostAsync(BaseUrl.ProductAPI, new StringContent(
                JsonConvert.SerializeObject(product),
                Encoding.UTF8,
                "application/json"));

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var content = await response.Content.ReadAsStringAsync();
            var createdProduct = JsonConvert.DeserializeObject<ProductDto>(content);

            createdProduct.Should().NotBeNull();
            createdProduct!.Name.Should().Be(product.Name);
            createdProduct.Price.Should().Be(product.Price);
            createdProduct.InventoryLevel.Should().Be(product.InventoryLevel);
            createdProduct.CategoryId.Should().Be(product.CategoryId);
            createdProduct.Id.Should().NotBeEmpty();

            return createdProduct;
        }
    }
}

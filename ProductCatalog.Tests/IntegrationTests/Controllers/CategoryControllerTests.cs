using FluentAssertions;
using Newtonsoft.Json;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Tests.IntegrationTests.Constants;
using ProductCatalog.Tests.IntegrationTests.Helpers;
using ProductCatalog.Tests.IntegrationTests.Infrastructure;
using System.Net;
using System.Text;
using System.Transactions;

namespace ProductCatalog.Tests.IntegrationTests.Controllers
{
    [TestClass]
    public class CategoryControllerTests
    {
        private CustomWebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;
        private TransactionScope _transaction = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _factory = new CustomWebApplicationFactory<Program>();
            _client = _factory.CreateClient();
            _transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _transaction.Dispose();
            _factory.Dispose();
            _client.Dispose();
        }

        [TestMethod]
        public async Task GetCategories_ShouldReturnCategoryList()
        {
            // Act
            var response = await _client.GetAsync(BaseUrl.CategoryAPI);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var categories = JsonConvert.DeserializeObject<IEnumerable<CategoryDto>>(content);
            categories.Should().NotBeNull();
        }

        [TestMethod]
        public async Task GetCategory_ShouldReturnCategory_WhenCategoryExists()
        {
            // Arrange
            var createdCategory = await TestHelpers.CreateCategoryAndVerifyAsync(_client, new CategoryDto
            {
                Name = "Test Category",
                Description = "A test category"
            });

            // Act
            var response = await _client.GetAsync($"{BaseUrl.CategoryAPI}/{createdCategory.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var category = JsonConvert.DeserializeObject<CategoryDto>(content);
            category.Should().NotBeNull();
            category!.Id.Should().Be(createdCategory.Id);
            category.Name.Should().Be(createdCategory.Name);
        }

        [TestMethod]
        public async Task GetCategoryWithProducts_ShouldReturnCategoryWithProducts()
        {
            // Arrange
            var createdCategory = await TestHelpers.CreateCategoryAndVerifyAsync(_client, new CategoryDto
            {
                Name = "Category with Products",
                Description = "A test category with products"
            });

            var createdProduct = await TestHelpers.CreateProductAndVerifyAsync(_client, new ProductDto
            {
                Name = "Test Product",
                Price = 100,
                InventoryLevel = 10,
                CategoryId = createdCategory.Id,
            });
            
            // Act
            var response = await _client.GetAsync($"{BaseUrl.CategoryAPI}/{createdCategory.Id}/products");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var categoryWithProducts = JsonConvert.DeserializeObject<CategoryWithProductsDto>(content);
            categoryWithProducts.Should().NotBeNull();
            categoryWithProducts!.Id.Should().Be(createdCategory.Id);
        }

        [TestMethod]
        public async Task CreateCategory_ShouldReturnCreatedCategory()
        {
            // Arrange
            var categoryDto = new CategoryDto
            {
                Name = "New Category",
                Description = "A new category for testing"
            };

            // Act
            var response = await _client.PostAsync(BaseUrl.CategoryAPI, new StringContent(
                JsonConvert.SerializeObject(categoryDto),
                Encoding.UTF8,
                "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var content = await response.Content.ReadAsStringAsync();
            var createdCategory = JsonConvert.DeserializeObject<CategoryDto>(content);
            createdCategory.Should().NotBeNull();
            createdCategory!.Name.Should().Be(categoryDto.Name);
        }

        [TestMethod]
        public async Task UpdateCategory_ShouldReturnNoContent()
        {
            // Arrange
            var createdCategory = await TestHelpers.CreateCategoryAndVerifyAsync(_client, new CategoryDto
            {
                Name = "Updatable Category",
                Description = "A category to update"
            });

            var updatedCategoryDto = new CategoryDto
            {
                Id = createdCategory.Id,
                Name = "Updated Category Name",
                Description = "Updated Description",
                RowVersion = createdCategory.RowVersion
            };

            // Act
            var response = await _client.PutAsync($"{BaseUrl.CategoryAPI}/{createdCategory.Id}", new StringContent(
                JsonConvert.SerializeObject(updatedCategoryDto),
                Encoding.UTF8,
                "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var getResponse = await _client.GetAsync($"{BaseUrl.CategoryAPI}/{createdCategory.Id}");
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var updatedCategory = JsonConvert.DeserializeObject<CategoryDto>(getContent);
            updatedCategory.Should().NotBeNull();
            updatedCategory!.Name.Should().Be(updatedCategoryDto.Name);
            updatedCategory.Description.Should().Be(updatedCategoryDto.Description);
        }

        [TestMethod]
        public async Task DeleteCategory_ShouldReturnNoContent()
        {
            // Arrange
            var createdCategory = await TestHelpers.CreateCategoryAndVerifyAsync(_client, new CategoryDto
            {
                Name = "Deletable Category",
                Description = "A category to delete"
            });

            // Act
            var response = await _client.DeleteAsync($"{BaseUrl.CategoryAPI}/{createdCategory.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var getResponse = await _client.GetAsync($"{BaseUrl.CategoryAPI}/{createdCategory.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task UpdateCategory_ShouldThrowConcurrencyException()
        {
            // Arrange
            var createdCategory = await TestHelpers.CreateCategoryAndVerifyAsync(_client, new CategoryDto
            {
                Name = "Test Category",
                Description = "testing concurrency"
            });

            // Act
            createdCategory.Name = "Updated Name";
            createdCategory.Description = "Updated Description";

            var response1 = await _client.PutAsync($"{BaseUrl.CategoryAPI}/{createdCategory.Id}", new StringContent(
                JsonConvert.SerializeObject(createdCategory),
                Encoding.UTF8,
                "application/json"));

            response1.StatusCode.Should().Be(HttpStatusCode.NoContent);

            createdCategory.Name = "Conflicting Update Name";

            var response2 = await _client.PutAsync($"{BaseUrl.CategoryAPI}/{createdCategory.Id}", new StringContent(
                JsonConvert.SerializeObject(createdCategory),
                Encoding.UTF8,
                "application/json"));

            // Assert
            response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
    }
}

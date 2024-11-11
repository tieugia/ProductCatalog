using FluentAssertions;
using Newtonsoft.Json;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Tests.IntegrationTests.Constants;
using ProductCatalog.Tests.IntegrationTests.Helpers;
using ProductCatalog.Tests.IntegrationTests.Infrastructure;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Transactions;

namespace ProductCatalog.Tests.IntegrationTests.Controllers
{
    [TestClass]
    public class ProductControllerTests
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
        public async Task ImportProductData_ShouldImport100000Records()
        {
            // Arrange
            var filePath = "../../../Products_100000.csv";
            if (!File.Exists(filePath))
            {
                Assert.Fail("Test CSV file not found.");
            }

            using var formContent = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(filePath);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

            formContent.Add(fileContent, "file", Path.GetFileName(filePath));

            var stopwatch = Stopwatch.StartNew();

            // Act
            var response = await _client.PostAsync($"{BaseUrl.ProductAPI}/import", formContent);

            stopwatch.Stop();
            var elapsedTime = stopwatch.Elapsed;

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Log the elapsed time
            Console.WriteLine($"Import operation completed in: {elapsedTime.TotalSeconds} seconds");
        }

        [TestMethod]
        public async Task GetProducts_ShouldReturnFilteredAndPaginatedList()
        {
            // Arrange
            var categoryId = await SeedTestDataAsync();

            var filter = new ProductFilterDto
            {
                Name = "Test Product",
                MinPrice = 50,
                MaxPrice = 200,
                CategoryId = categoryId,
                PageNumber = 1,
                PageSize = 3
            };

            var query = $"?Name={filter.Name}&MinPrice={filter.MinPrice}&MaxPrice={filter.MaxPrice}" +
                        $"&CategoryId={filter.CategoryId}&PageNumber={filter.PageNumber}&PageSize={filter.PageSize}";

            // Act
            var response = await _client.GetAsync(BaseUrl.ProductAPI + query);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var products = JsonConvert.DeserializeObject<IEnumerable<ProductDto>>(content);

            products.Should().NotBeNull();
            products.Should().NotBeEmpty();
            products.Should().OnlyContain(p => p.Price >= filter.MinPrice && p.Price <= filter.MaxPrice);
            products.Should().OnlyContain(p => p.Name.Contains(filter.Name));
            products.Should().HaveCountLessOrEqualTo(filter.PageSize);
        }


        [TestMethod]
        public async Task GetProduct_ShouldReturnProduct_WhenProductExists()
        {
            // Arrange
            var createdCategory = await TestHelpers.CreateCategoryAndVerifyAsync(_client, new CategoryDto
            {
                Name = "Test Category",
                Description = "A test category"
            });

            var createdProduct = await TestHelpers.CreateProductAndVerifyAsync(_client, new ProductDto
            {
                Name = "Test Product",
                Price = 100,
                InventoryLevel = 10,
                CategoryId = createdCategory.Id,
            });

            // Act
            var response = await _client.GetAsync($"{BaseUrl.ProductAPI}/{createdProduct.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var returnedProduct = JsonConvert.DeserializeObject<ProductDto>(content);
            returnedProduct.Should().NotBeNull();
            returnedProduct!.Name.Should().Be(createdProduct.Name);
            returnedProduct.Price.Should().Be(createdProduct.Price);
        }

        [TestMethod]
        public async Task CreateProduct_ShouldReturnCreatedProduct()
        {
            // Arrange
            var createdCategory = await TestHelpers.CreateCategoryAndVerifyAsync(_client, new CategoryDto
            {
                Name = "Test Category",
                Description = "A test category"
            });

            // Act - Assert
            await TestHelpers.CreateProductAndVerifyAsync(_client, new ProductDto
            {
                Name = "Test Product",
                Price = 100,
                InventoryLevel = 10,
                CategoryId = createdCategory.Id,
            });
        }

        [TestMethod]
        public async Task UpdateProduct_ShouldReturnNoContent()
        {
            // Arrange
            var createdCategory = await TestHelpers.CreateCategoryAndVerifyAsync(_client, new CategoryDto
            {
                Name = "Test Category",
                Description = "A test category"
            });

            var createdProduct = await TestHelpers.CreateProductAndVerifyAsync(_client, new ProductDto
            {
                Name = "Test Product",
                Price = 100,
                InventoryLevel = 10,
                CategoryId = createdCategory.Id,
            });

            // Act
            createdProduct.Id = createdProduct.Id;
            createdProduct.Name = "Updated Product";
            createdProduct.Price = 200;

            var responseUpdate = await _client.PutAsync($"{BaseUrl.ProductAPI}/{createdProduct.Id}", new StringContent(
                JsonConvert.SerializeObject(createdProduct),
                Encoding.UTF8,
                "application/json"));

            // Assert
            responseUpdate.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [TestMethod]
        public async Task DeleteProduct_ShouldReturnNoContent()
        {
            // Arrange
            var createdCategory = await TestHelpers.CreateCategoryAndVerifyAsync(_client, new CategoryDto
            {
                Name = "Test Category",
                Description = "A test category"
            });

            var createdProduct = await TestHelpers.CreateProductAndVerifyAsync(_client, new ProductDto
            {
                Name = "Test Product",
                Price = 100,
                InventoryLevel = 10,
                CategoryId = createdCategory.Id,
            });

            // Act
            var responseDelete = await _client.DeleteAsync($"{BaseUrl.ProductAPI}/{createdProduct.Id}");

            // Assert
            responseDelete.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [TestMethod]
        public async Task UpdateProduct_ShouldThrowConcurrencyException()
        {
            // Arrange
            var category = await TestHelpers.CreateCategoryAndVerifyAsync(_client, new CategoryDto
            {
                Name = "Test Category",
                Description = "A test category"
            });

            var product = new ProductDto
            {
                Name = "Test Product",
                Price = 100,
                InventoryLevel = 10,
                CategoryId = category.Id
            };

            var createdProduct = await TestHelpers.CreateProductAndVerifyAsync(_client, product);

            createdProduct.Name = "Updated Product 1st";
            var responseUpdate1 = await _client.PutAsync($"{BaseUrl.ProductAPI}/{createdProduct.Id}", new StringContent(
                JsonConvert.SerializeObject(createdProduct),
                Encoding.UTF8,
                "application/json"));

            responseUpdate1.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Act - Use old row verion -> Concurrency Exception
            createdProduct.Name = "Updated Product 2nd";
            var responseUpdate2 = await _client.PutAsync($"{BaseUrl.ProductAPI}/{createdProduct.Id}", new StringContent(
                JsonConvert.SerializeObject(createdProduct),
                Encoding.UTF8,
                "application/json"));

            // Assert
            responseUpdate2.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        private async Task<Guid> SeedTestDataAsync()
        {
            // Create category
            var category = new CategoryDto
            {
                Name = "Category Test",
                Description = "category for testing"
            };

            var createdCategory = await TestHelpers.CreateCategoryAndVerifyAsync(_client, category);

            // Create products
            var products = new List<ProductDto>
            {
                new ProductDto { Name = "Test Product A", Price = 50, InventoryLevel = 100, CategoryId = createdCategory.Id },
                new ProductDto { Name = "Test Product B", Price = 100, InventoryLevel = 200, CategoryId = createdCategory.Id },
                new ProductDto { Name = "Test Product C", Price = 150, InventoryLevel = 150, CategoryId = createdCategory.Id },
                new ProductDto { Name = "Test Product D", Price = 200, InventoryLevel = 300, CategoryId = createdCategory.Id },
                new ProductDto { Name = "Test Product E", Price = 250, InventoryLevel = 50, CategoryId = createdCategory.Id },
            };

            foreach (var product in products)
            {
                await TestHelpers.CreateProductAndVerifyAsync(_client, product);
            }

            return createdCategory.Id;
        }
    }
}

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
    public class CategoryHierarchyControllerTests
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
        public async Task AddHierarchy_ShouldReturnCreated()
        {
            // Arrange
            var tasks = await Task.WhenAll(
                CreateCategoryAsync("Parent Category", "Parent category description"),
                CreateCategoryAsync("Child Category", "Child category description")
            );

            var parentCategory = tasks[0];
            var childCategory = tasks[1];

            var hierarchyDto = new CategoryHierarchyDto
            {
                ParentId = parentCategory.Id,
                ChildId = childCategory.Id
            };

            // Act
            var response = await _client.PostAsync(BaseUrl.CategoryHierachyAPI, new StringContent(
                JsonConvert.SerializeObject(hierarchyDto),
                Encoding.UTF8,
                "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var content = await response.Content.ReadAsStringAsync();
            var createdHierarchy = JsonConvert.DeserializeObject<CategoryHierarchyDto>(content);
            createdHierarchy.Should().NotBeNull();
            createdHierarchy!.ParentId.Should().Be(parentCategory.Id);
            createdHierarchy.ChildId.Should().Be(childCategory.Id);
        }

        [TestMethod]
        public async Task RemoveHierarchy_ShouldReturnNoContent()
        {
            // Arrange
            var tasks = await Task.WhenAll(
                CreateCategoryAsync("Parent Category", "Parent category description"),
                CreateCategoryAsync("Child Category", "Child category description")
            );

            var parentCategory = tasks[0];
            var childCategory = tasks[1];

            var createdHierarchy = await AddCategoryHierarchyAsync(parentCategory.Id, childCategory.Id);

            // Act: Xóa liên kết cha-con đã tạo
            var response = await _client.DeleteAsync($"{BaseUrl.CategoryHierachyAPI}/{createdHierarchy!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [TestMethod]
        public async Task GetChildren_ShouldReturnChildrenList()
        {
            // Arrange
            var tasks = await Task.WhenAll(
                CreateCategoryAsync("Parent Category", "Parent category with children"),
                CreateCategoryAsync("Child Category 1", "First child category"),
                CreateCategoryAsync("Child Category 2", "Second child category")
            );

            var parentCategory = tasks[0];
            var childCategory1 = tasks[1];
            var childCategory2 = tasks[2];

            await AddCategoryHierarchyAsync(parentCategory.Id, childCategory1.Id);
            await AddCategoryHierarchyAsync(parentCategory.Id, childCategory2.Id);

            // Act
            var response = await _client.GetAsync($"{BaseUrl.CategoryHierachyAPI}/parent/{parentCategory.Id}/children");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var children = JsonConvert.DeserializeObject<IEnumerable<CategoryHierarchyDto>>(content);
            children.Should().NotBeNull();
            children.Should().HaveCount(2);
        }

        [TestMethod]
        public async Task GetParents_ShouldReturnParentsList()
        {
            // Arrange
            var tasks = await Task.WhenAll(
                CreateCategoryAsync("Parent Category 1", "First parent category"),
                CreateCategoryAsync("Parent Category 2", "Second parent category"),
                CreateCategoryAsync("Child Category", "Child category with multiple parents")
            );

            var parentCategory1 = tasks[0];
            var parentCategory2 = tasks[1];
            var childCategory = tasks[2];

            await Task.WhenAll(
                AddCategoryHierarchyAsync(parentCategory1.Id, childCategory.Id),
                AddCategoryHierarchyAsync(parentCategory2.Id, childCategory.Id)
            );

            // Act
            var response = await _client.GetAsync($"{BaseUrl.CategoryHierachyAPI}/child/{childCategory.Id}/parents");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var parents = JsonConvert.DeserializeObject<IEnumerable<CategoryHierarchyDto>>(content);
            parents.Should().NotBeNull();
            parents.Should().HaveCount(2);
        }

        [TestMethod]
        public async Task UpdateHierarchy_ShouldThrowConcurrencyException()
        {
            // Arrange
            var tasks = await Task.WhenAll(
                CreateCategoryAsync("Parent Category", "Parent category description"),
                CreateCategoryAsync("Child Category", "Child category description")
            );

            var parentCategory = tasks[0];
            var childCategory = tasks[1];

            var createdHierarchy = await AddCategoryHierarchyAsync(parentCategory.Id, childCategory.Id);

            // Act
            createdHierarchy!.ParentId = parentCategory.Id;
            createdHierarchy.ChildId = childCategory.Id;

            var initialUpdateResponse = await _client.PutAsync($"{BaseUrl.CategoryHierachyAPI}/{createdHierarchy.Id}", new StringContent(
                JsonConvert.SerializeObject(createdHierarchy),
                Encoding.UTF8,
                "application/json"));

            initialUpdateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            createdHierarchy.ParentId = parentCategory.Id;

            var conflictingUpdateResponse = await _client.PutAsync($"{BaseUrl.CategoryHierachyAPI}/{createdHierarchy.Id}", new StringContent(
                JsonConvert.SerializeObject(createdHierarchy),
                Encoding.UTF8,
                "application/json"));

            // Assert
            conflictingUpdateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        private async Task<CategoryDto> CreateCategoryAsync(string name, string description)
        {
            return await TestHelpers.CreateCategoryAndVerifyAsync(_client, new CategoryDto { Name = name, Description = description });
        }

        private async Task<CategoryHierarchyDto?> AddCategoryHierarchyAsync(Guid parentId, Guid childId)
        {
            var hierarchyDto = new CategoryHierarchyDto
            {
                ParentId = parentId,
                ChildId = childId
            };

            var response = await _client.PostAsync(BaseUrl.CategoryHierachyAPI, new StringContent(
                JsonConvert.SerializeObject(hierarchyDto),
                Encoding.UTF8,
                "application/json"));

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var content = await response.Content.ReadAsStringAsync();
            var categoryHierarchy = JsonConvert.DeserializeObject<CategoryHierarchyDto>(content);
            categoryHierarchy.Should().NotBeNull();
            return categoryHierarchy;
        }
    }
}

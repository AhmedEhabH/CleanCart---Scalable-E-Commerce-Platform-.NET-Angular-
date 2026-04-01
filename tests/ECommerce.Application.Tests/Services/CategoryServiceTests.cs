using ECommerce.Application.Categories.DTOs;
using ECommerce.Application.Categories.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ECommerce.Application.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _sut = new CategoryService(_categoryRepositoryMock.Object);
    }

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSuccess_WhenCategoryExists()
    {
        var category = CreateCategory();
        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepositoryMock.Setup(r => r.GetProductCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());
        _categoryRepositoryMock.Setup(r => r.GetSubcategoriesAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        var result = await _sut.GetByIdAsync(category.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(category.Id);
        result.Value.Name.Should().Be(category.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFailure_WhenCategoryDoesNotExist()
    {
        var categoryId = Guid.NewGuid();
        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await _sut.GetByIdAsync(categoryId);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("CATEGORY_NOT_FOUND");
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ShouldReturnTreeStructure()
    {
        var rootCategory = CreateCategory(name: "Root", parentId: null);
        var childCategory = CreateCategory(name: "Child", parentId: rootCategory.Id);
        var categories = new List<Category> { rootCategory, childCategory };

        _categoryRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);
        _categoryRepositoryMock.Setup(r => r.GetProductCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());

        var result = await _sut.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Children.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoCategoriesExist()
    {
        _categoryRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());
        _categoryRepositoryMock.Setup(r => r.GetProductCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());

        var result = await _sut.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    #endregion

    #region GetRootCategoriesAsync

    [Fact]
    public async Task GetRootCategoriesAsync_ShouldReturnRootCategories()
    {
        var rootCategories = new List<Category> { CreateCategory(name: "Root 1"), CreateCategory(name: "Root 2") };
        _categoryRepositoryMock.Setup(r => r.GetRootCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rootCategories);
        _categoryRepositoryMock.Setup(r => r.GetProductCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());

        var result = await _sut.GetRootCategoriesAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    #endregion

    #region GetSubcategoriesAsync

    [Fact]
    public async Task GetSubcategoriesAsync_ShouldReturnSubcategories_WhenParentExists()
    {
        var parentId = Guid.NewGuid();
        var subcategories = new List<Category> { CreateCategory(parentId: parentId), CreateCategory(parentId: parentId) };
        _categoryRepositoryMock.Setup(r => r.ExistsAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _categoryRepositoryMock.Setup(r => r.GetSubcategoriesAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subcategories);
        _categoryRepositoryMock.Setup(r => r.GetProductCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());

        var result = await _sut.GetSubcategoriesAsync(parentId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSubcategoriesAsync_ShouldReturnFailure_WhenParentDoesNotExist()
    {
        var parentId = Guid.NewGuid();
        _categoryRepositoryMock.Setup(r => r.ExistsAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.GetSubcategoriesAsync(parentId);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("CATEGORY_NOT_FOUND");
    }

    #endregion

    #region GetBySlugAsync

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnSuccess_WhenCategoryExists()
    {
        var category = CreateCategory(slug: "test-category");
        _categoryRepositoryMock.Setup(r => r.GetBySlugAsync(category.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepositoryMock.Setup(r => r.GetProductCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());

        var result = await _sut.GetBySlugAsync(category.Slug);

        result.IsSuccess.Should().BeTrue();
        result.Value.Slug.Should().Be(category.Slug);
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnFailure_WhenCategoryDoesNotExist()
    {
        _categoryRepositoryMock.Setup(r => r.GetBySlugAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await _sut.GetBySlugAsync("nonexistent");

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("CATEGORY_NOT_FOUND");
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess_WhenValidRequest()
    {
        var request = new CreateCategoryRequest("New Category", "new-category", "Description");
        _categoryRepositoryMock.Setup(r => r.GetBySlugAsync(request.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);
        _categoryRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category c, CancellationToken _) => c);

        var result = await _sut.CreateAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Category");
        result.Value.Slug.Should().Be("new-category");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess_WithParent_WhenParentExists()
    {
        var parentId = Guid.NewGuid();
        var request = new CreateCategoryRequest("Child Category", "child-category", null, parentId);
        _categoryRepositoryMock.Setup(r => r.GetBySlugAsync(request.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);
        _categoryRepositoryMock.Setup(r => r.ExistsAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _categoryRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category c, CancellationToken _) => c);

        var result = await _sut.CreateAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.ParentId.Should().Be(parentId);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenSlugAlreadyExists()
    {
        var request = new CreateCategoryRequest("Existing", "existing-slug");
        _categoryRepositoryMock.Setup(r => r.GetBySlugAsync(request.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCategory(slug: "existing-slug"));

        var result = await _sut.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("CATEGORY_SLUG_EXISTS");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenParentDoesNotExist()
    {
        var parentId = Guid.NewGuid();
        var request = new CreateCategoryRequest("Child", "child", null, parentId);
        _categoryRepositoryMock.Setup(r => r.GetBySlugAsync(request.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);
        _categoryRepositoryMock.Setup(r => r.ExistsAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("CATEGORY_NOT_FOUND");
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldReturnSuccess_WhenCategoryExists()
    {
        var category = CreateCategory();
        var request = new UpdateCategoryRequest("Updated Name");
        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepositoryMock.Setup(r => r.UpdateAsync(category, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _categoryRepositoryMock.Setup(r => r.GetProductCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());

        var result = await _sut.UpdateAsync(category.Id, request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenCategoryDoesNotExist()
    {
        var categoryId = Guid.NewGuid();
        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await _sut.UpdateAsync(categoryId, new UpdateCategoryRequest());

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("CATEGORY_NOT_FOUND");
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ShouldReturnSuccess_WhenValidCategory()
    {
        var category = CreateCategory();
        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepositoryMock.Setup(r => r.HasSubcategoriesAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _categoryRepositoryMock.Setup(r => r.HasProductsAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _categoryRepositoryMock.Setup(r => r.DeleteAsync(category, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.DeleteAsync(category.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenCategoryDoesNotExist()
    {
        var categoryId = Guid.NewGuid();
        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await _sut.DeleteAsync(categoryId);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("CATEGORY_NOT_FOUND");
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenCategoryHasSubcategories()
    {
        var category = CreateCategory();
        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepositoryMock.Setup(r => r.HasSubcategoriesAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.DeleteAsync(category.Id);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("CATEGORY_HAS_SUBCATEGORIES");
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenCategoryHasProducts()
    {
        var category = CreateCategory();
        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepositoryMock.Setup(r => r.HasSubcategoriesAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _categoryRepositoryMock.Setup(r => r.HasProductsAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.DeleteAsync(category.Id);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("CATEGORY_HAS_PRODUCTS");
    }

    #endregion

    #region Helper Methods

    private static Category CreateCategory(
        Guid? id = null,
        string? name = null,
        string? slug = null,
        Guid? parentId = null)
    {
        var category = Category.Create(
            name ?? "Test Category",
            slug ?? "test-category",
            parentId: parentId
        );

        if (id.HasValue)
        {
            typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))?.SetValue(category, id.Value);
        }

        return category;
    }

    #endregion
}

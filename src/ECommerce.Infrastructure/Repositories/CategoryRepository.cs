using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category> AddAsync(Category entity, CancellationToken cancellationToken = default)
    {
        _context.Categories.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Category entity, CancellationToken cancellationToken = default)
    {
        _context.Categories.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Category entity, CancellationToken cancellationToken = default)
    {
        _context.Categories.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories.AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.ParentId == null)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetSubcategoriesAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.ParentId == parentId)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetProductCountsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => p.IsActive)
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);
    }

    public async Task<bool> HasSubcategoriesAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories.AnyAsync(c => c.ParentId == categoryId, cancellationToken);
    }

    public async Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Products.AnyAsync(p => p.CategoryId == categoryId, cancellationToken);
    }
}

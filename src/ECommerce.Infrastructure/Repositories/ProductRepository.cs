using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Images)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        _context.Products.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Product entity, CancellationToken cancellationToken = default)
    {
        _context.Products.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);
    }

    public async Task<Product?> GetWithImagesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Product?> GetBySKUAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.SKU == sku, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Images)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetFeaturedAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Images)
            .Where(p => p.IsFeatured && p.IsActive)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLowerInvariant();
        return await _context.Products
            .Include(p => p.Images)
            .Where(p => p.IsActive &&
                (p.Name.ToLowerInvariant().Contains(term) ||
                 (p.Description != null && p.Description.ToLowerInvariant().Contains(term))))
            .ToListAsync(cancellationToken);
    }
}

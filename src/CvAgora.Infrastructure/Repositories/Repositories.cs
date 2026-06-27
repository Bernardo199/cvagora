using CvAgora.Core.Entities;
using CvAgora.Core.Interfaces;
using CvAgora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CvAgora.Infrastructure.Repositories;

public class ArticleRepository : IArticleRepository
{
    private readonly AppDbContext _db;
    public ArticleRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Article>> GetPublishedAsync(int page = 1, int pageSize = 9)
        => await _db.Articles
            .Where(a => a.Published)
            .Include(a => a.Category)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IEnumerable<Article>> GetFeaturedAsync(int count = 6)
        => await _db.Articles
            .Where(a => a.Published && a.Featured)
            .Include(a => a.Category)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Article?> GetBySlugAsync(string slug)
        => await _db.Articles
            .Include(a => a.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Slug == slug && a.Published);

    public async Task<IEnumerable<Article>> GetByCategoryAsync(string categorySlug, int page = 1, int pageSize = 9)
        => await _db.Articles
            .Where(a => a.Published && a.Category.Slug == categorySlug)
            .Include(a => a.Category)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

    public async Task<int> GetTotalCountAsync(bool publishedOnly = true)
        => await _db.Articles.CountAsync(a => !publishedOnly || a.Published);

    public async Task<Article> CreateAsync(Article article)
    {
        _db.Articles.Add(article);
        await _db.SaveChangesAsync();
        return article;
    }

    public async Task<Article> UpdateAsync(Article article)
    {
        article.UpdatedAt = DateTime.UtcNow;
        _db.Articles.Update(article);
        await _db.SaveChangesAsync();
        return article;
    }

    public async Task DeleteAsync(int id)
    {
        var article = await _db.Articles.FindAsync(id);
        if (article != null) { _db.Articles.Remove(article); await _db.SaveChangesAsync(); }
    }

    public async Task IncrementViewCountAsync(int id)
        => await _db.Articles.Where(a => a.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.ViewCount, a => a.ViewCount + 1));

    public async Task<IEnumerable<Article>> SearchAsync(string query)
        => await _db.Articles
            .Where(a => a.Published && (a.Title.Contains(query) || a.Summary.Contains(query)))
            .Include(a => a.Category)
            .OrderByDescending(a => a.CreatedAt)
            .Take(20)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IEnumerable<Article>> GetAllAsync()
        => await _db.Articles.Include(a => a.Category).OrderByDescending(a => a.CreatedAt).AsNoTracking().ToListAsync();
}

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;
    public CategoryRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Category>> GetAllAsync()
        => await _db.Categories.OrderBy(c => c.SortOrder).AsNoTracking().ToListAsync();

    public async Task<Category?> GetBySlugAsync(string slug)
        => await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Slug == slug);

    public async Task<Category> CreateAsync(Category category)
    { _db.Categories.Add(category); await _db.SaveChangesAsync(); return category; }

    public async Task<Category> UpdateAsync(Category category)
    { _db.Categories.Update(category); await _db.SaveChangesAsync(); return category; }

    public async Task DeleteAsync(int id)
    { var c = await _db.Categories.FindAsync(id); if (c != null) { _db.Categories.Remove(c); await _db.SaveChangesAsync(); } }
}

public class SiteConfigRepository : ISiteConfigRepository
{
    private readonly AppDbContext _db;
    public SiteConfigRepository(AppDbContext db) => _db = db;

    public async Task<string?> GetValueAsync(string key)
        => (await _db.SiteConfigs.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key))?.Value;

    public async Task SetValueAsync(string key, string value)
    {
        var config = await _db.SiteConfigs.FirstOrDefaultAsync(s => s.Key == key);
        if (config != null) config.Value = value;
        else _db.SiteConfigs.Add(new SiteConfig { Key = key, Value = value });
        await _db.SaveChangesAsync();
    }

    public async Task<Dictionary<string, string>> GetAllAsync()
        => await _db.SiteConfigs.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value);
}

public class NewsletterRepository : INewsletterRepository
{
    private readonly AppDbContext _db;
    public NewsletterRepository(AppDbContext db) => _db = db;

    public async Task<bool> SubscribeAsync(string email)
    {
        if (await _db.Newsletters.AnyAsync(n => n.Email == email)) return false;
        _db.Newsletters.Add(new Newsletter { Email = email });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Newsletter>> GetAllAsync()
        => await _db.Newsletters.OrderByDescending(n => n.SubscribedAt).ToListAsync();

    public async Task<int> GetCountAsync()
        => await _db.Newsletters.CountAsync(n => n.Active);
}

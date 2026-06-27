using CvAgora.Core.Entities;

namespace CvAgora.Core.Interfaces;

public interface IArticleRepository
{
    Task<IEnumerable<Article>> GetPublishedAsync(int page = 1, int pageSize = 9);
    Task<IEnumerable<Article>> GetFeaturedAsync(int count = 6);
    Task<Article?> GetBySlugAsync(string slug);
    Task<IEnumerable<Article>> GetByCategoryAsync(string categorySlug, int page = 1, int pageSize = 9);
    Task<int> GetTotalCountAsync(bool publishedOnly = true);
    Task<Article> CreateAsync(Article article);
    Task<Article> UpdateAsync(Article article);
    Task DeleteAsync(int id);
    Task IncrementViewCountAsync(int id);
    Task<IEnumerable<Article>> SearchAsync(string query);
    Task<IEnumerable<Article>> GetAllAsync();
}

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetBySlugAsync(string slug);
    Task<Category> CreateAsync(Category category);
    Task<Category> UpdateAsync(Category category);
    Task DeleteAsync(int id);
}

public interface ISiteConfigRepository
{
    Task<string?> GetValueAsync(string key);
    Task SetValueAsync(string key, string value);
    Task<Dictionary<string, string>> GetAllAsync();
}

public interface INewsletterRepository
{
    Task<bool> SubscribeAsync(string email);
    Task<IEnumerable<Newsletter>> GetAllAsync();
    Task<int> GetCountAsync();
}

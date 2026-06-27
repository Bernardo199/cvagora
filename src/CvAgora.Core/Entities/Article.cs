namespace CvAgora.Core.Entities;

public class Article
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Body { get; set; } = "";
    public string Summary { get; set; } = "";
    public string? ImageUrl { get; set; }
    public bool Published { get; set; }
    public bool Featured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public int ViewCount { get; set; }
}

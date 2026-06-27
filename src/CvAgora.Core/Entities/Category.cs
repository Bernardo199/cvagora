namespace CvAgora.Core.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? ColorClass { get; set; } = "c-azul";  // c-azul, c-sol, c-hibisco, c-verde
    public string? TagLabel { get; set; }
    public int SortOrder { get; set; }
    public ICollection<Article> Articles { get; set; } = new List<Article>();
}

public class SiteConfig
{
    public int Id { get; set; }
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public string? Description { get; set; }
}

public class Newsletter
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    public bool Active { get; set; } = true;
}

public class CultureItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string IconSvg { get; set; } = "";
    public int SortOrder { get; set; }
    public bool Active { get; set; } = true;
}

using CvAgora.Core.Entities;

namespace CvAgora.Web.ViewModels;

public class HomeViewModel
{
    public List<Article> AllArticles { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public string HeroTitle { get; set; } = "";
    public string HeroSubtitle { get; set; } = "";
    public string HeroEyebrow { get; set; } = "";
    public string TrendingTitle { get; set; } = "";
    public string AdSenseClient { get; set; } = "";
    public string AdSenseSlot1 { get; set; } = "";
    public string AdSenseSlot2 { get; set; } = "";
    public string GoogleAnalyticsId { get; set; } = "";
    public List<Article> FeaturedArticles { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<CultureItemVm> CultureItems { get; set; } = new();
}

public class ArticleViewModel
{
    public Article Article { get; set; } = null!;
    public string AdSenseClient { get; set; } = "";
    public string AdSenseSlot1 { get; set; } = "";
    public string GoogleAnalyticsId { get; set; } = "";
}

public class CategoryViewModel
{
    public Category Category { get; set; } = null!;
    public List<Article> Articles { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public string AdSenseClient { get; set; } = "";
    public string GoogleAnalyticsId { get; set; } = "";
}

public class SearchViewModel
{
    public string Query { get; set; } = "";
    public List<Article> Results { get; set; } = new();
}

public class CultureItemVm
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string IconKey { get; set; } = "";  // music, food, festival, nature
}

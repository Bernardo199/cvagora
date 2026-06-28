using CvAgora.Core.Interfaces;
using CvAgora.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CvAgora.Web.Controllers;

public class HomeController : Controller
{
    private readonly IArticleRepository _articles;
    private readonly ICategoryRepository _categories;
    private readonly ISiteConfigRepository _config;
    private readonly INewsletterRepository _newsletter;
    private readonly IMemoryCache _cache;

    public HomeController(
        IArticleRepository articles,
        ICategoryRepository categories,
        ISiteConfigRepository config,
        INewsletterRepository newsletter,
        IMemoryCache cache)
    {
        _articles = articles;
        _categories = categories;
        _config = config;
        _newsletter = newsletter;
        _cache = cache;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        const string cacheKey = "home_vm_p{page}";
        if (!_cache.TryGetValue(cacheKey, out HomeViewModel? vm))
        {
            var configs = await _config.GetAllAsync();
            var featured = await _articles.GetFeaturedAsync(3);
            var allArticles = await _articles.GetPublishedAsync(page, 9);
            var totalArticles = await _articles.GetTotalCountAsync(true);
            var categories = await _categories.GetAllAsync();
            var cultureItems = await GetCultureItemsAsync();

            vm = new HomeViewModel
            {
                HeroTitle = configs.GetValueOrDefault("hero_title", "Cabo Verde\nestá no mapa."),
                HeroSubtitle = configs.GetValueOrDefault("hero_subtitle", "Dez ilhas no meio do Atlântico estão a dar que falar ao mundo."),
                HeroEyebrow = configs.GetValueOrDefault("hero_eyebrow", $"EDIÇÃO DE HOJE · {DateTime.Now:d 'DE' MMMM 'DE' yyyy}".ToUpper()),
                TrendingTitle = configs.GetValueOrDefault("trending_section_title", "O que todos andam a pesquisar"),
                AdSenseClient = configs.GetValueOrDefault("adsense_client", ""),
                AdSenseSlot1 = configs.GetValueOrDefault("adsense_slot_1", ""),
                AdSenseSlot2 = configs.GetValueOrDefault("adsense_slot_2", ""),
                GoogleAnalyticsId = configs.GetValueOrDefault("google_analytics_id", ""),
                FeaturedArticles = featured.ToList(),
                AllArticles = allArticles.ToList(),
                Categories = categories.ToList(),
                CultureItems = cultureItems,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalArticles / 9.0)
            };

            _cache.Set(cacheKey, vm, TimeSpan.FromMinutes(5));
        }

        return View(vm);
    }

    public async Task<IActionResult> Article(string slug)
    {
        var cacheKey = $"article_{slug}";
        if (!_cache.TryGetValue(cacheKey, out ArticleViewModel? vm))
        {
            var article = await _articles.GetBySlugAsync(slug);
            if (article == null) return NotFound();

            var configs = await _config.GetAllAsync();
            vm = new ArticleViewModel
            {
                Article = article,
                AdSenseClient = configs.GetValueOrDefault("adsense_client", ""),
                AdSenseSlot1 = configs.GetValueOrDefault("adsense_slot_1", ""),
                GoogleAnalyticsId = configs.GetValueOrDefault("google_analytics_id", "")
            };

            _cache.Set(cacheKey, vm, TimeSpan.FromMinutes(10));
        }

        // Incrementar vistas de forma async sem bloquear a resposta
        _ = _articles.IncrementViewCountAsync(vm!.Article.Id);

        return View(vm);
    }

    public async Task<IActionResult> Category(string slug, int page = 1)
    {
        var category = await _categories.GetBySlugAsync(slug);
        if (category == null) return NotFound();

        var articles = await _articles.GetByCategoryAsync(slug, page);
        var configs = await _config.GetAllAsync();

        var vm = new CategoryViewModel
        {
            Category = category,
            Articles = articles.ToList(),
            CurrentPage = page,
            AdSenseClient = configs.GetValueOrDefault("adsense_client", ""),
            GoogleAnalyticsId = configs.GetValueOrDefault("google_analytics_id", "")
        };

        return View(vm);
    }

    public async Task<IActionResult> Search(string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return RedirectToAction("Index");
        var results = await _articles.SearchAsync(q);
        return View(new SearchViewModel { Query = q, Results = results.ToList() });
    }

    [HttpPost]
    [Route("subscribe")]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return Json(new { success = false, message = "Email inválido." });

        var ok = await _newsletter.SubscribeAsync(email.Trim().ToLower());
        return Json(new
        {
            success = ok,
            message = ok ? "Subscrito com sucesso! Obrigado." : "Este email já está subscrito."
        });
    }

    [Route("erro")]
    public IActionResult Error() => View();

    // Helper privado para items de cultura (poderiam vir da BD também)
    private async Task<List<CultureItemVm>> GetCultureItemsAsync()
    {
        // Puxa da BD se existir tabela; caso contrário, valores default
        await Task.CompletedTask;
        return new List<CultureItemVm>
        {
            new() { Title = "Morna & Funaná", Description = "A morna, melancólica e poética, e o funaná, dançante e de acordeão, são os dois grandes géneros que carregam a alma cabo-verdiana pelo mundo.", IconKey = "music" },
            new() { Title = "Cachupa", Description = "Cozinhada lentamente com milho, feijão e o que a despensa tiver, a cachupa é o prato nacional e o símbolo da partilha à mesa cabo-verdiana.", IconKey = "food" },
            new() { Title = "São João", Description = "Em junho, Porto Novo e outras vilas enchem-se de romarias, tambores e bandeiras, numa das festas populares mais esperadas do ano.", IconKey = "festival" },
            new() { Title = "Sete Maravilhas", Description = "Da Praia de Santa Maria ao Deserto de Viana, sete paisagens naturais resumem a diversidade de um arquipélago vulcânico no Atlântico.", IconKey = "nature" }
        };
    }
}

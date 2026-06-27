using CvAgora.Core.Entities;
using CvAgora.Core.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace CvAgora.Web.Controllers;

[Authorize]
[Route("admin")]
public class AdminController : Controller
{
    private readonly IArticleRepository _articles;
    private readonly ICategoryRepository _categories;
    private readonly ISiteConfigRepository _config;
    private readonly INewsletterRepository _newsletter;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _appConfig;

    public AdminController(
        IArticleRepository articles,
        ICategoryRepository categories,
        ISiteConfigRepository config,
        INewsletterRepository newsletter,
        IMemoryCache cache,
        IConfiguration appConfig)
    {
        _articles = articles;
        _categories = categories;
        _config = config;
        _newsletter = newsletter;
        _cache = cache;
        _appConfig = appConfig;
    }

    // ── Dashboard ──────────────────────────────────────────
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var totalArticles = await _articles.GetTotalCountAsync(false);
        var publishedArticles = await _articles.GetTotalCountAsync(true);
        var newsletterCount = await _newsletter.GetCountAsync();
        var recentArticles = await _articles.GetAllAsync();

        ViewBag.TotalArticles = totalArticles;
        ViewBag.PublishedArticles = publishedArticles;
        ViewBag.NewsletterCount = newsletterCount;
        ViewBag.RecentArticles = recentArticles.Take(10).ToList();

        return View();
    }

    // ── Login ──────────────────────────────────────────────
    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login() => User.Identity?.IsAuthenticated == true
        ? RedirectToAction("Index")
        : View();

    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password)
    {
        var validUser = _appConfig["AdminCredentials:Username"];
        var validPass = _appConfig["AdminCredentials:Password"];

        if (username != validUser || password != validPass)
        {
            ViewBag.Error = "Credenciais inválidas.";
            return View();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return RedirectToAction("Index");
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    // ── Artigos ────────────────────────────────────────────
    [HttpGet("artigos")]
    public async Task<IActionResult> Articles()
    {
        var articles = await _articles.GetAllAsync();
        return View(articles);
    }

    [HttpGet("artigos/novo")]
    public async Task<IActionResult> ArticleCreate()
    {
        ViewBag.Categories = await _categories.GetAllAsync();
        return View(new Article { CreatedAt = DateTime.UtcNow });
    }

    [HttpPost("artigos/novo")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArticleCreate(Article model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _categories.GetAllAsync();
            return View(model);
        }

        model.Slug = GenerateSlug(model.Title);
        model.CreatedAt = DateTime.UtcNow;
        model.UpdatedAt = DateTime.UtcNow;

        await _articles.CreateAsync(model);
        InvalidateCache();
        TempData["Success"] = "Artigo criado com sucesso.";
        return RedirectToAction("Articles");
    }

    [HttpGet("artigos/{id:int}/editar")]
    public async Task<IActionResult> ArticleEdit(int id)
    {
        var allArticles = await _articles.GetAllAsync();
        var article = allArticles.FirstOrDefault(a => a.Id == id);
        if (article == null) return NotFound();
        ViewBag.Categories = await _categories.GetAllAsync();
        return View(article);
    }

    [HttpPost("artigos/{id:int}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArticleEdit(int id, Article model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _categories.GetAllAsync();
            return View(model);
        }

        model.Id = id;
        if (string.IsNullOrWhiteSpace(model.Slug))
            model.Slug = GenerateSlug(model.Title);

        await _articles.UpdateAsync(model);
        InvalidateCache();
        TempData["Success"] = "Artigo actualizado.";
        return RedirectToAction("Articles");
    }

    [HttpPost("artigos/{id:int}/apagar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArticleDelete(int id)
    {
        await _articles.DeleteAsync(id);
        InvalidateCache();
        TempData["Success"] = "Artigo eliminado.";
        return RedirectToAction("Articles");
    }

    // ── Categorias ─────────────────────────────────────────
    [HttpGet("categorias")]
    public async Task<IActionResult> Categories()
    {
        var cats = await _categories.GetAllAsync();
        return View(cats);
    }

    [HttpGet("categorias/nova")]
    public IActionResult CategoryCreate() => View(new Core.Entities.Category());

    [HttpPost("categorias/nova")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoryCreate(Core.Entities.Category model)
    {
        model.Slug = GenerateSlug(model.Name);
        await _categories.CreateAsync(model);
        InvalidateCache();
        TempData["Success"] = "Categoria criada.";
        return RedirectToAction("Categories");
    }

    // ── Configurações ──────────────────────────────────────
    [HttpGet("configuracoes")]
    public async Task<IActionResult> Settings()
    {
        var configs = await _config.GetAllAsync();
        return View(configs);
    }

    [HttpPost("configuracoes")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(Dictionary<string, string> configs)
    {
        foreach (var kv in configs)
            await _config.SetValueAsync(kv.Key, kv.Value ?? "");

        InvalidateCache();
        TempData["Success"] = "Configurações guardadas.";
        return RedirectToAction("Settings");
    }

    // ── Newsletter ─────────────────────────────────────────
    [HttpGet("newsletter")]
    public async Task<IActionResult> Newsletter()
    {
        var subscribers = await _newsletter.GetAllAsync();
        return View(subscribers);
    }

    // ── Helpers ───────────────────────────────────────────
    private void InvalidateCache()
    {
        // Remove entradas de cache relevantes
        _cache.Remove("home_vm");
        // Artigos individuais serão invalidados por TTL
    }

    private static string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return Guid.NewGuid().ToString("N")[..8];

        var slug = title.ToLowerInvariant()
            .Replace("á", "a").Replace("à", "a").Replace("ã", "a").Replace("â", "a")
            .Replace("é", "e").Replace("ê", "e").Replace("í", "i").Replace("ó", "o")
            .Replace("ô", "o").Replace("õ", "o").Replace("ú", "u").Replace("ç", "c")
            .Replace("ñ", "n");

        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-").Trim('-');

        return slug.Length > 80 ? slug[..80] : slug;
    }
}

using CvAgora.Core.Interfaces;
using CvAgora.Infrastructure.Data;
using CvAgora.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC + Razor Pages
builder.Services.AddControllersWithViews();

// MySQL via EF Core (Pomelo)
var conn = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' não encontrada.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(conn, ServerVersion.AutoDetect(conn),
        o => o.MigrationsAssembly("CvAgora.Infrastructure")));

// Repositórios
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ISiteConfigRepository, SiteConfigRepository>();
builder.Services.AddScoped<INewsletterRepository, NewsletterRepository>();

// Cache em memória
builder.Services.AddMemoryCache();

// Auth por cookie (painel admin)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/admin/login";
        options.LogoutPath = "/admin/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.Name = "cvagora.admin";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Aplicar migrations e seed na inicialização
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/erro");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Rotas principais
app.MapControllerRoute("article", "artigo/{slug}", new { controller = "Home", action = "Article" });
app.MapControllerRoute("category", "categoria/{slug}", new { controller = "Home", action = "Category" });
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();

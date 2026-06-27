using CvAgora.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CvAgora.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SiteConfig> SiteConfigs => Set<SiteConfig>();
    public DbSet<Newsletter> Newsletters => Set<Newsletter>();
    public DbSet<CultureItem> CultureItems => Set<CultureItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>(e =>
        {
            e.HasIndex(a => a.Slug).IsUnique();
            e.HasIndex(a => a.Published);
            e.HasIndex(a => a.CreatedAt);
            e.Property(a => a.Title).HasMaxLength(300);
            e.Property(a => a.Slug).HasMaxLength(300);
            e.Property(a => a.Summary).HasMaxLength(600);
            e.HasOne(a => a.Category).WithMany(c => c.Articles).HasForeignKey(a => a.CategoryId);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasIndex(c => c.Slug).IsUnique();
            e.Property(c => c.Name).HasMaxLength(100);
            e.Property(c => c.Slug).HasMaxLength(100);
        });

        modelBuilder.Entity<SiteConfig>(e =>
        {
            e.HasIndex(s => s.Key).IsUnique();
            e.Property(s => s.Key).HasMaxLength(100);
        });

        modelBuilder.Entity<Newsletter>(e =>
        {
            e.HasIndex(n => n.Email).IsUnique();
            e.Property(n => n.Email).HasMaxLength(254);
        });

        // Seed data inicial
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Mundial 2026", Slug = "mundial-2026", ColorClass = "c-azul", TagLabel = "Mundial 2026", SortOrder = 1 },
            new Category { Id = 2, Name = "Fenómeno Viral", Slug = "fenomeno-viral", ColorClass = "c-sol", TagLabel = "Fenómeno viral", SortOrder = 2 },
            new Category { Id = 3, Name = "Política", Slug = "politica", ColorClass = "c-hibisco", TagLabel = "Política", SortOrder = 3 },
            new Category { Id = 4, Name = "Cultura", Slug = "cultura", ColorClass = "c-verde", TagLabel = "Cultura", SortOrder = 4 },
            new Category { Id = 5, Name = "Economia", Slug = "economia", ColorClass = "c-azul", TagLabel = "Economia", SortOrder = 5 },
            new Category { Id = 6, Name = "Turismo", Slug = "turismo", ColorClass = "c-sol", TagLabel = "Turismo", SortOrder = 6 }
        );

        modelBuilder.Entity<SiteConfig>().HasData(
            new SiteConfig { Id = 1, Key = "hero_title", Value = "Cabo Verde\nestá no mapa.", Description = "Título principal do hero (usar \\n para quebra de linha)" },
            new SiteConfig { Id = 2, Key = "hero_subtitle", Value = "Dez ilhas no meio do Atlântico estão a dar que falar ao mundo esta semana.", Description = "Subtítulo do hero" },
            new SiteConfig { Id = 3, Key = "hero_eyebrow", Value = "EDIÇÃO DE HOJE", Description = "Eyebrow do hero (data/edição)" },
            new SiteConfig { Id = 4, Key = "adsense_client", Value = "ca-pub-XXXXXXXXXX", Description = "Google AdSense publisher ID" },
            new SiteConfig { Id = 5, Key = "adsense_slot_1", Value = "XXXXXXXXXX", Description = "AdSense slot ID — entre tendências" },
            new SiteConfig { Id = 6, Key = "adsense_slot_2", Value = "XXXXXXXXXX", Description = "AdSense slot ID — antes do footer" },
            new SiteConfig { Id = 7, Key = "google_analytics_id", Value = "G-XXXXXXXXXX", Description = "Google Analytics 4 Measurement ID" },
            new SiteConfig { Id = 8, Key = "site_name", Value = "CV Agora", Description = "Nome do site" },
            new SiteConfig { Id = 9, Key = "trending_section_title", Value = "O que todos andam a pesquisar", Description = "Título da secção de tendências" }
        );

        modelBuilder.Entity<CultureItem>().HasData(
            new CultureItem { Id = 1, Title = "Morna & Funaná", Description = "A morna, melancólica e poética, e o funaná, dançante e de acordeão, são os dois grandes géneros que carregam a alma cabo-verdiana pelo mundo.", IconSvg = "<circle cx=\"24\" cy=\"24\" r=\"20\" fill=\"none\" stroke=\"#2E7DB8\" stroke-width=\"2.5\"/><path d=\"M14 28c3-8 17-8 20 0\" stroke=\"#2E7DB8\" stroke-width=\"2.5\" fill=\"none\" stroke-linecap=\"round\"/><circle cx=\"18\" cy=\"19\" r=\"2\" fill=\"#2E7DB8\"/><circle cx=\"30\" cy=\"19\" r=\"2\" fill=\"#2E7DB8\"/>", SortOrder = 1 },
            new CultureItem { Id = 2, Title = "Cachupa", Description = "Cozinhada lentamente com milho, feijão e o que a despensa tiver, a cachupa é o prato nacional e o símbolo da partilha à mesa cabo-verdiana.", IconSvg = "<path d=\"M10 30c0-8 6-14 14-14s14 6 14 14\" fill=\"none\" stroke=\"#C8392B\" stroke-width=\"2.5\"/><line x1=\"8\" y1=\"30\" x2=\"40\" y2=\"30\" stroke=\"#C8392B\" stroke-width=\"2.5\"/>", SortOrder = 2 },
            new CultureItem { Id = 3, Title = "São João", Description = "Em junho, Porto Novo e outras vilas enchem-se de romarias, tambores e bandeiras, numa das festas populares mais esperadas do ano.", IconSvg = "<rect x=\"14\" y=\"10\" width=\"20\" height=\"28\" rx=\"3\" fill=\"none\" stroke=\"#3F7A56\" stroke-width=\"2.5\"/><line x1=\"14\" y1=\"18\" x2=\"34\" y2=\"18\" stroke=\"#3F7A56\" stroke-width=\"2.5\"/><line x1=\"14\" y1=\"26\" x2=\"34\" y2=\"26\" stroke=\"#3F7A56\" stroke-width=\"2.5\"/>", SortOrder = 3 },
            new CultureItem { Id = 4, Title = "Sete Maravilhas", Description = "Da Praia de Santa Maria ao Deserto de Viana, sete paisagens naturais resumem a diversidade de um arquipélago vulcânico no Atlântico.", IconSvg = "<path d=\"M6 34c8-18 28-18 36 0\" fill=\"none\" stroke=\"#F2B705\" stroke-width=\"2.5\"/><circle cx=\"24\" cy=\"16\" r=\"5\" fill=\"none\" stroke=\"#F2B705\" stroke-width=\"2.5\"/>", SortOrder = 4 }
        );
    }
}

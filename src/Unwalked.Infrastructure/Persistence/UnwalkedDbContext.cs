using Microsoft.EntityFrameworkCore;
using Unwalked.Domain.Feedback;
using Unwalked.Domain.Places;
using Unwalked.Domain.Regions;
using Unwalked.Domain.Sharing;
using Unwalked.Domain.Users;

namespace Unwalked.Infrastructure.Persistence;

/// <summary>
/// EF Core context for the Unwalked database. Mapping lives in the Configurations folder.
/// </summary>
public sealed class UnwalkedDbContext(DbContextOptions<UnwalkedDbContext> options) : DbContext(options)
{
    public DbSet<Place> Places => Set<Place>();

    public DbSet<Building> Buildings => Set<Building>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Region> Regions => Set<Region>();

    public DbSet<RegionPack> RegionPacks => Set<RegionPack>();

    public DbSet<FeedbackDailyTotal> FeedbackDailyTotals => Set<FeedbackDailyTotal>();

    public DbSet<UserAccount> Users => Set<UserAccount>();

    public DbSet<AchievementShare> Shares => Set<AchievementShare>();

    public DbSet<RecommendationFeedback> Feedback => Set<RecommendationFeedback>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UnwalkedDbContext).Assembly);
    }
}

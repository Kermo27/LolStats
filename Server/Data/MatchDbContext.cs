using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LolStatsTracker.API.Data;

public class MatchDbContext : DbContext
{
    public MatchDbContext(DbContextOptions<MatchDbContext> options) : base(options) {}
    
    public DbSet<MatchEntry> Matches => Set<MatchEntry>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<ChampionPool> ChampionPools => Set<ChampionPool>();
    public DbSet<RankMilestone> RankMilestones => Set<RankMilestone>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MatchEntry>()
            .HasOne<UserProfile>()
            .WithMany()
            .HasForeignKey(m => m.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChampionPool>()
            .HasOne<UserProfile>()
            .WithMany()
            .HasForeignKey(c => c.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RankMilestone>()
            .HasOne<UserProfile>()
            .WithMany()
            .HasForeignKey(r => r.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Performance indexes
        modelBuilder.Entity<MatchEntry>()
            .HasIndex(m => m.ProfileId)
            .HasDatabaseName("IX_Matches_ProfileId");

        modelBuilder.Entity<MatchEntry>()
            .HasIndex(m => m.Date)
            .HasDatabaseName("IX_Matches_Date");

        modelBuilder.Entity<MatchEntry>()
            .HasIndex(m => new { m.ProfileId, m.Date })
            .HasDatabaseName("IX_Matches_ProfileId_Date");

        modelBuilder.Entity<Season>().HasData(
            new Season 
            { 
                Id = 1, 
                Number = 15, 
                StartDate = new DateTime(2025, 1, 9), 
                EndDate = new DateTime(2026, 1, 6) 
            }
        );
    }
}
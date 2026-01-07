using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LolStatsTracker.API.Data;

public class MatchDbContext : DbContext
{
    public MatchDbContext(DbContextOptions<MatchDbContext> options) : base(options) {}
    
    public DbSet<User> Users => Set<User>();
    public DbSet<MatchEntry> Matches => Set<MatchEntry>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User configuration
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("IX_Users_Username");

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .HasDatabaseName("IX_Users_Email");

        // RefreshToken -> User relationship
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_Token");

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.UserId)
            .HasDatabaseName("IX_RefreshTokens_UserId");

        // UserProfile -> User relationship (nullable for legacy profiles)
        modelBuilder.Entity<UserProfile>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<MatchEntry>()
            .HasOne<UserProfile>()
            .WithMany()
            .HasForeignKey(m => m.ProfileId)
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
        
        modelBuilder.Entity<MatchEntry>()
            .HasIndex(m => new { m.ProfileId, m.Champion })
            .HasDatabaseName("IX_Matches_ProfileId_Champion");

        modelBuilder.Entity<MatchEntry>()
            .HasIndex(m => new { m.ProfileId, m.Date, m.GameMode })
            .HasDatabaseName("IX_Matches_ProfileId_Date_GameMode");

        modelBuilder.Entity<MatchEntry>()
            .HasIndex(m => new { m.ProfileId, m.Role })
            .HasDatabaseName("IX_Matches_ProfileId_Role");

        modelBuilder.Entity<Season>().HasData(
            new Season 
            { 
                Id = 1, 
                Number = 15, 
                StartDate = new DateTime(2025, 1, 9, 0, 0, 0, DateTimeKind.Utc), 
                EndDate = new DateTime(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc) 
            }
        );
    }
}
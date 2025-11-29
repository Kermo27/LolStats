using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LolStatsTracker.API.Data;

public class MatchDbContext : DbContext
{
    public MatchDbContext(DbContextOptions<MatchDbContext> options) : base(options) {}
    
    public DbSet<MatchEntry> Matches => Set<MatchEntry>();
}
using LogisticsTrackingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LogisticsTrackingAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<Trip> Trips { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Relacionamento 1..N: Um jogador tem várias trips
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Player)
                .WithMany(p => p.Trips)
                .HasForeignKey(t => t.PlayerId);
        }
    }
}

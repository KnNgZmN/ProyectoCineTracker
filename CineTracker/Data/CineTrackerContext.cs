using Microsoft.EntityFrameworkCore;
using CineTracker.Models;

namespace CineTracker.Data
{
    public class CineTrackerContext : DbContext
    {
        public CineTrackerContext(DbContextOptions<CineTrackerContext> options)
            : base(options)
        {
        }

        // Tablas de la base de datos
        public DbSet<Movie> Movies { get; set; }
        public DbSet<WatchListItem> WatchlistItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de la tabla Movies
            modelBuilder.Entity<Movie>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Overview).HasMaxLength(2000);
                entity.Property(e => e.PosterPath).HasMaxLength(500);
            });

            // Configuración de la tabla WatchlistItems
            modelBuilder.Entity<WatchListItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PosterPath).HasMaxLength(500);
            });
        }
    }
}

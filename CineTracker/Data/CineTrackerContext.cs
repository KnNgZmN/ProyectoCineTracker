using Microsoft.EntityFrameworkCore;
using CineTracker.Models;

namespace CineTracker.Data
{
    // Contexto principal de EF Core: representa la base de datos y sus tablas
    public class CineTrackerContext : DbContext
    {
        public CineTrackerContext(DbContextOptions<CineTrackerContext> options)
            : base(options) { }

        // Cada DbSet representa una tabla en PostgreSQL
        public DbSet<Movie> Movies { get; set; }
        public DbSet<WatchListItem> WatchlistItems { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Movie>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Overview).HasMaxLength(2000);
                entity.Property(e => e.PosterPath).HasMaxLength(500);
            });

            modelBuilder.Entity<WatchListItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PosterPath).HasMaxLength(500);
                // Si se elimina el usuario, se eliminan sus items en cascada
                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.NombreUsuario).IsUnique();
                entity.Property(e => e.NombreUsuario).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PasswordHash).IsRequired();
            });
        }
    }
}

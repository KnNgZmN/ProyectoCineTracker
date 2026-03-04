using Microsoft.EntityFrameworkCore;
using CineTracker.Models;

namespace CineTracker.Data
{
    /// <summary>
    /// Contexto principal de Entity Framework para la aplicación CineTracker.
    /// Define las tablas y la configuración de las entidades.
    /// </summary>
    public class CineTrackerContext : DbContext
    {
        /// <summary>
        /// Constructor que recibe las opciones de configuración de EF Core.
        /// </summary>
        /// <param name="options">Opciones de DbContext</param>
        public CineTrackerContext(DbContextOptions<CineTrackerContext> options)
            : base(options)
        {
        }

        #region Tablas de la base de datos

        /// <summary>
        /// Tabla que almacena las películas (información general de TMDB u otra fuente).
        /// </summary>
        public DbSet<Movie> Movies { get; set; }

        /// <summary>
        /// Tabla que almacena los elementos de la lista de seguimiento del usuario.
        /// </summary>
        public DbSet<WatchListItem> WatchlistItems { get; set; }

        #endregion

        /// <summary>
        /// Configuración de las entidades y restricciones de la base de datos.
        /// </summary>
        /// <param name="modelBuilder">Constructor de modelos de EF Core</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de la entidad Movie
            modelBuilder.Entity<Movie>(entity =>
            {
                entity.HasKey(e => e.Id);  // Clave primaria
                entity.Property(e => e.Title)
                      .IsRequired()        // Campo obligatorio
                      .HasMaxLength(200);  // Longitud máxima 200 caracteres
                entity.Property(e => e.Overview)
                      .HasMaxLength(2000); // Longitud máxima 2000 caracteres
                entity.Property(e => e.PosterPath)
                      .HasMaxLength(500);  // Longitud máxima 500 caracteres para la URL del poster
            });

            // Configuración de la entidad WatchlistItems
            modelBuilder.Entity<WatchListItem>(entity =>
            {
                entity.HasKey(e => e.Id);  // Clave primaria
                entity.Property(e => e.Title)
                      .IsRequired()        // Campo obligatorio
                      .HasMaxLength(200);  // Longitud máxima 200 caracteres
                entity.Property(e => e.PosterPath)
                      .HasMaxLength(500);  // Longitud máxima 500 caracteres para la URL del poster
            });
        }
    }
}
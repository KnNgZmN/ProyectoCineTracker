namespace CineTracker.Models
{
    // Película en la watchlist de un usuario → tabla "watch_list_items" en PostgreSQL
    public class WatchListItem
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }    // FK → usuarios.id (con CASCADE DELETE)
        public Usuario? Usuario { get; set; } // propiedad de navegación, puede no estar cargada

        public int TmdbId { get; set; }       // ID en la API de TMDB
        public string Title { get; set; } = string.Empty;
        public string PosterPath { get; set; } = string.Empty;
        public string ReleaseDate { get; set; } = string.Empty;
        public double VoteAverage { get; set; }

        public bool IsFavorite { get; set; } = false;
        public bool IsWatched { get; set; } = false;

        public DateTime DateAdded { get; set; } = DateTime.UtcNow; // UTC requerido por PostgreSQL/Npgsql
    }
}

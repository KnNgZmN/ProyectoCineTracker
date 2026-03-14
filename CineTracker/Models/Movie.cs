namespace CineTracker.Models
{
    // Película almacenada localmente (datos traídos desde la API de TMDB)
    public class Movie
    {
        public int Id { get; set; }
        public string TmdbId { get; set; } = string.Empty;   // ID en la API de TMDB
        public string Title { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string PosterPath { get; set; } = string.Empty; // ruta relativa del poster en TMDB
        public string ReleaseDate { get; set; } = string.Empty;
        public double VoteAverage { get; set; }
        public string Genres { get; set; } = string.Empty;    // géneros separados por coma
    }
}

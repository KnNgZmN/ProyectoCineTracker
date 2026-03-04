namespace CineTracker.Models
{
    /// <summary>
    /// Representa un elemento de la lista de seguimiento del usuario (watchlist).
    /// Contiene información básica de la película y estados de seguimiento/favorito.
    /// </summary>
    public class WatchListItem
    {
        /// <summary>
        /// Identificador único de la base de datos para este elemento.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador de la película en TMDB (The Movie Database).
        /// </summary>
        public int TmdbId { get; set; }

        /// <summary>
        /// Título de la película.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Ruta del poster de la película según TMDB.
        /// </summary>
        public string PosterPath { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de estreno de la película (formato yyyy-MM-dd).
        /// </summary>
        public string ReleaseDate { get; set; } = string.Empty;

        /// <summary>
        /// Promedio de votos de la película en TMDB.
        /// </summary>
        public double VoteAverage { get; set; }

        /// <summary>
        /// Indica si la película está marcada como favorita por el usuario.
        /// </summary>
        public bool IsFavorite { get; set; } = false;

        /// <summary>
        /// Indica si la película ha sido marcada como vista por el usuario.
        /// </summary>
        public bool IsWatched { get; set; } = false;

        /// <summary>
        /// Fecha en que la película fue agregada a la lista.
        /// </summary>
        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}
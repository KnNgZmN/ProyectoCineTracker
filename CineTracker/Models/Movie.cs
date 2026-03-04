namespace CineTracker.Models
{
    /// <summary>
    /// Representa una película en la aplicación CineTracker.
    /// Esta clase se utiliza tanto para almacenar información local
    /// en la base de datos como para mapear datos obtenidos desde TMDB.
    /// </summary>
    public class Movie
    {
        /// <summary>
        /// Identificador único interno de la película en la base de datos.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la película según TMDB.
        /// Se puede usar para consultas externas o integraciones con la API de TMDB.
        /// </summary>
        public string TmdbId { get; set; } = string.Empty;

        /// <summary>
        /// Título de la película.
        /// Campo obligatorio.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Sinopsis o descripción de la película.
        /// </summary>
        public string Overview { get; set; } = string.Empty;

        /// <summary>
        /// URL parcial del poster de la película en TMDB.
        /// Se combina con la URL base para obtener la imagen completa.
        /// </summary>
        public string PosterPath { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de estreno en formato YYYY-MM-DD.
        /// </summary>
        public string ReleaseDate { get; set; } = string.Empty;

        /// <summary>
        /// Promedio de votos de la película en TMDB.
        /// </summary>
        public double VoteAverage { get; set; }

        /// <summary>
        /// Géneros de la película como texto separado por comas.
        /// Ejemplo: "Acción, Aventura, Ciencia ficción".
        /// </summary>
        public string Genres { get; set; } = string.Empty;
    }
}

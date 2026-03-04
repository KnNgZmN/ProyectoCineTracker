using System.Text.Json.Serialization;

namespace CineTracker.Models
{
    /// <summary>
    /// Representa la respuesta general de TMDB al buscar o listar películas.
    /// Contiene información de paginación y los resultados en forma de <see cref="TmdbMovie"/>.
    /// </summary>
    public class TmdbResponse
    {
        /// <summary>
        /// Página actual de resultados devuelta por la API.
        /// </summary>
        [JsonPropertyName("page")]
        public int Page { get; set; }

        /// <summary>
        /// Lista de películas devueltas en esta página.
        /// </summary>
        [JsonPropertyName("results")]
        public List<TmdbMovie> Results { get; set; } = new();

        /// <summary>
        /// Total de páginas disponibles según la búsqueda o listado.
        /// </summary>
        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }

        /// <summary>
        /// Total de resultados encontrados en la búsqueda.
        /// </summary>
        [JsonPropertyName("total_results")]
        public int TotalResults { get; set; }
    }

    /// <summary>
    /// Modelo que representa una película básica devuelta por TMDB.
    /// Se utiliza en listados como populares, mejor valoradas o búsqueda.
    /// </summary>
    public class TmdbMovie
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("overview")]
        public string Overview { get; set; } = string.Empty;

        [JsonPropertyName("poster_path")]
        public string PosterPath { get; set; } = string.Empty;

        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; } = string.Empty;

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        [JsonPropertyName("genre_ids")]
        public List<int> GenreIds { get; set; } = new();

        [JsonPropertyName("backdrop_path")]
        public string BackdropPath { get; set; } = string.Empty;

        [JsonPropertyName("popularity")]
        public double Popularity { get; set; }
    }

    /// <summary>
    /// Modelo que representa el detalle completo de una película.
    /// Incluye información adicional como duración, tagline y géneros completos.
    /// </summary>
    public class TmdbMovieDetail
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("overview")]
        public string Overview { get; set; } = string.Empty;

        [JsonPropertyName("poster_path")]
        public string PosterPath { get; set; } = string.Empty;

        [JsonPropertyName("backdrop_path")]
        public string BackdropPath { get; set; } = string.Empty;

        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; } = string.Empty;

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        /// <summary>
        /// Duración de la película en minutos.
        /// </summary>
        [JsonPropertyName("runtime")]
        public int Runtime { get; set; }

        /// <summary>
        /// Lista de géneros de la película con información completa.
        /// </summary>
        [JsonPropertyName("genres")]
        public List<TmdbGenre> Genres { get; set; } = new();

        /// <summary>
        /// Frase o tagline promocional de la película.
        /// </summary>
        [JsonPropertyName("tagline")]
        public string Tagline { get; set; } = string.Empty;
    }

    /// <summary>
    /// Representa un género de película.
    /// Se utiliza dentro de <see cref="TmdbMovieDetail"/> para mostrar los nombres de géneros.
    /// </summary>
    public class TmdbGenre
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
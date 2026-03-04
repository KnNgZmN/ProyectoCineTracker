namespace CineTracker
{
    /// <summary>
    /// Configuración para la API de TMDB (The Movie Database).
    /// Esta clase se carga desde appsettings.json y se inyecta en TmdbService
    /// usando IOptions&lt;TmdbSettings&gt;.
    /// </summary>
    public class TmdbSettings
    {
        /// <summary>
        /// Clave de API proporcionada por TMDB para autenticar las peticiones.
        /// Debe mantenerse secreta y no compartirse públicamente.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// URL base de la API de TMDB, por ejemplo: "https://api.themoviedb.org/3".
        /// Se utiliza para construir las URLs de todas las peticiones HTTP.
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// URL base para obtener imágenes (posters) de películas,
        /// por ejemplo: "https://image.tmdb.org/t/p/w500".
        /// Se combina con la ruta del poster proporcionada por la API.
        /// </summary>
        public string ImageBaseUrl { get; set; } = string.Empty;
    }
}

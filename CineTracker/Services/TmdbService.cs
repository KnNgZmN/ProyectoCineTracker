using CineTracker.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CineTracker.Services
{
    public class TmdbService
    {
        // =========================
        // VARIABLES PRIVADAS
        // =========================
        private readonly HttpClient _httpClient;   // Cliente HTTP para realizar las peticiones a TMDB
        private readonly string _apiKey;           // Clave de API de TMDB
        private readonly string _baseUrl;          // URL base de la API de TMDB
        private readonly string _imageBaseUrl;     // URL base para las imágenes de TMDB
        private readonly TmdbSettings _settings;   // Configuración inyectada desde appsettings.json

        // =========================
        // CONSTRUCTOR
        // =========================
        public TmdbService(HttpClient httpClient, IOptions<TmdbSettings> options)
        {
            _httpClient = httpClient;       // Inyecta HttpClient para llamadas HTTP
            _settings = options.Value;      // Obtiene la configuración TMDB desde IOptions

            // Asignación de variables de configuración
            _apiKey = _settings.ApiKey;
            _baseUrl = _settings.BaseUrl;
            _imageBaseUrl = _settings.ImageBaseUrl;
        }

        // =========================
        // OBTENER URL COMPLETA DE IMAGEN
        // =========================
        public string GetImageUrl(string posterPath)
        {
            if (string.IsNullOrEmpty(posterPath))
                return "/images/no-poster.png"; // Imagen por defecto si no existe poster
            return $"{_imageBaseUrl}{posterPath}"; // URL completa del poster
        }

        // =========================
        // OBTENER PELÍCULAS POPULARES
        // =========================
        public async Task<List<TmdbMovie>> GetPopularMoviesAsync(int page = 1)
        {
            // Construye la URL de la API para películas populares
            var url = $"{_baseUrl}/movie/popular?api_key={_apiKey}&language=es-ES&page={page}";

            // Realiza la petición HTTP
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return new List<TmdbMovie>(); // Retorna lista vacía si falla

            var json = await response.Content.ReadAsStringAsync(); // Lee el contenido JSON
            var result = JsonSerializer.Deserialize<TmdbResponse>(json); // Deserializa a TmdbResponse
            return result?.Results ?? new List<TmdbMovie>(); // Retorna la lista de películas
        }

        // =========================
        // BUSCAR PELÍCULAS POR NOMBRE
        // =========================
        public async Task<List<TmdbMovie>> SearchMoviesAsync(string query, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<TmdbMovie>();

            // URL de búsqueda de películas con query codificado
            var url = $"{_baseUrl}/search/movie?api_key={_apiKey}&language=es-ES&query={Uri.EscapeDataString(query)}&page={page}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return new List<TmdbMovie>();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbResponse>(json);
            return result?.Results ?? new List<TmdbMovie>();
        }

        // =========================
        // OBTENER DETALLE COMPLETO DE UNA PELÍCULA
        // =========================
        public async Task<TmdbMovieDetail?> GetMovieDetailAsync(int tmdbId)
        {
            // URL de detalle de película
            var url = $"{_baseUrl}/movie/{tmdbId}?api_key={_apiKey}&language=es-ES";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return null; // Retorna null si falla la petición

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TmdbMovieDetail>(json); // Deserializa a TmdbMovieDetail
        }

        // =========================
        // OBTENER PELÍCULAS MEJOR VALORADAS
        // =========================
        public async Task<List<TmdbMovie>> GetTopRatedMoviesAsync(int page = 1)
        {
            var url = $"{_baseUrl}/movie/top_rated?api_key={_apiKey}&language=es-ES&page={page}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return new List<TmdbMovie>();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbResponse>(json);
            return result?.Results ?? new List<TmdbMovie>();
        }

        // =========================
        // OBTENER PELÍCULAS EN CARTELERA
        // =========================
        public async Task<List<TmdbMovie>> GetNowPlayingAsync(int page = 1)
        {
            var url = $"{_baseUrl}/movie/now_playing?api_key={_apiKey}&language=es-ES&page={page}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return new List<TmdbMovie>();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbResponse>(json);
            return result?.Results ?? new List<TmdbMovie>();
        }
    }
}
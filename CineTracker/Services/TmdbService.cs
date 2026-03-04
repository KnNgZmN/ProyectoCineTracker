using System.Text.Json;
using CineTracker.Models;

namespace CineTracker.Services
{
    public class TmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _imageBaseUrl;

        public TmdbService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["TmdbSettings:ApiKey"]!;
            _baseUrl = configuration["TmdbSettings:BaseUrl"]!;
            _imageBaseUrl = configuration["TmdbSettings:ImageBaseUrl"]!;
        }

        // Obtener URL completa de imagen
        public string GetImageUrl(string posterPath)
        {
            if (string.IsNullOrEmpty(posterPath))
                return "/images/no-poster.png";
            return $"{_imageBaseUrl}{posterPath}";
        }

        // Películas populares
        public async Task<List<TmdbMovie>> GetPopularMoviesAsync(int page = 1)
        {
            var url = $"{_baseUrl}/movie/popular?api_key={_apiKey}&language=es-ES&page={page}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return new List<TmdbMovie>();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbResponse>(json);
            return result?.Results ?? new List<TmdbMovie>();
        }

        // Buscar películas por nombre
        public async Task<List<TmdbMovie>> SearchMoviesAsync(string query, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<TmdbMovie>();

            var url = $"{_baseUrl}/search/movie?api_key={_apiKey}&language=es-ES&query={Uri.EscapeDataString(query)}&page={page}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return new List<TmdbMovie>();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbResponse>(json);
            return result?.Results ?? new List<TmdbMovie>();
        }

        // Detalle completo de una película
        public async Task<TmdbMovieDetail?> GetMovieDetailAsync(int tmdbId)
        {
            var url = $"{_baseUrl}/movie/{tmdbId}?api_key={_apiKey}&language=es-ES";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TmdbMovieDetail>(json);
        }

        // Películas mejor valoradas
        public async Task<List<TmdbMovie>> GetTopRatedMoviesAsync(int page = 1)
        {
            var url = $"{_baseUrl}/movie/top_rated?api_key={_apiKey}&language=es-ES&page={page}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return new List<TmdbMovie>();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbResponse>(json);
            return result?.Results ?? new List<TmdbMovie>();
        }

        // Películas en cartelera
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
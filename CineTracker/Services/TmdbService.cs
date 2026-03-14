using CineTracker.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CineTracker.Services
{
    // Consume la API pública de TMDB (The Movie Database)
    public class TmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _imageBaseUrl;

        public TmdbService(HttpClient httpClient, IOptions<TmdbSettings> options)
        {
            _httpClient = httpClient;
            var settings = options.Value;
            _apiKey = settings.ApiKey;
            _baseUrl = settings.BaseUrl;
            _imageBaseUrl = settings.ImageBaseUrl;
        }

        // Construye la URL completa del poster. Retorna imagen por defecto si no hay poster
        public string GetImageUrl(string posterPath)
        {
            if (string.IsNullOrEmpty(posterPath))
                return "/images/no-poster.png";
            return $"{_imageBaseUrl}{posterPath}";
        }

        public async Task<List<TmdbMovie>> GetPopularMoviesAsync(int page = 1)
        {
            var url = $"{_baseUrl}/movie/popular?api_key={_apiKey}&language=es-ES&page={page}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return [];

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbResponse>(json);
            return result?.Results ?? [];
        }

        public async Task<List<TmdbMovie>> SearchMoviesAsync(string query, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query)) return [];

            // Uri.EscapeDataString codifica caracteres especiales para que la URL sea válida
            var url = $"{_baseUrl}/search/movie?api_key={_apiKey}&language=es-ES&query={Uri.EscapeDataString(query)}&page={page}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return [];

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbResponse>(json);
            return result?.Results ?? [];
        }

        public async Task<TmdbMovieDetail?> GetMovieDetailAsync(int tmdbId)
        {
            var url = $"{_baseUrl}/movie/{tmdbId}?api_key={_apiKey}&language=es-ES";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TmdbMovieDetail>(json);
        }

        public async Task<List<TmdbMovie>> GetTopRatedMoviesAsync(int page = 1)
        {
            var url = $"{_baseUrl}/movie/top_rated?api_key={_apiKey}&language=es-ES&page={page}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return [];

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbResponse>(json);
            return result?.Results ?? [];
        }

        public async Task<List<TmdbMovie>> GetNowPlayingAsync(int page = 1)
        {
            var url = $"{_baseUrl}/movie/now_playing?api_key={_apiKey}&language=es-ES&page={page}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return [];

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbResponse>(json);
            return result?.Results ?? [];
        }
    }
}

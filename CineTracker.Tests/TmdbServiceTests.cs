using CineTracker.Services;
using CineTracker.Tests.Helpers;
using Microsoft.Extensions.Options;
using System.Net;

namespace CineTracker.Tests
{
    /// <summary>
    /// Pruebas unitarias para TmdbService.
    ///
    /// TmdbService consume la API externa de TMDB. En pruebas unitarias
    /// usamos FakeHttpMessageHandler para simular las respuestas HTTP
    /// sin hacer llamadas reales a internet.
    ///
    /// Qué probamos:
    ///   - GetImageUrl         → construcción de URLs de imágenes
    ///   - SearchMoviesAsync   → búsqueda (caso query vacía = sin HTTP)
    ///   - GetPopularMoviesAsync → llamada HTTP con respuesta exitosa y fallida
    ///   - GetMovieDetailAsync  → parseo de detalle de película
    /// </summary>
    public class TmdbServiceTests
    {
        // ─────────────────────────────────────────────────────────
        // Configuración compartida de TmdbSettings para los tests
        // ─────────────────────────────────────────────────────────

        private static readonly TmdbSettings ConfigPrueba = new()
        {
            ApiKey = "test-api-key-123",
            BaseUrl = "https://api.themoviedb.org/3",
            ImageBaseUrl = "https://image.tmdb.org/t/p/w500"
        };

        /// <summary>
        /// Crea un TmdbService con un HttpClient que devuelve la respuesta simulada.
        /// </summary>
        /// <param name="statusCode">Código HTTP de la respuesta (200, 404, 500...)</param>
        /// <param name="jsonContent">Cuerpo JSON de la respuesta simulada</param>
        private static TmdbService CrearServicio(HttpStatusCode statusCode, string jsonContent)
        {
            var handler = new FakeHttpMessageHandler(statusCode, jsonContent);
            var httpClient = new HttpClient(handler);
            var options = Options.Create(ConfigPrueba); // simula IOptions<TmdbSettings>
            return new TmdbService(httpClient, options);
        }

        // ═══════════════════════════════════════════════════════════
        // GET IMAGE URL (sin llamadas HTTP — solo lógica interna)
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public void GetImageUrl_PosterPathVacio_RetornaImagenPorDefecto()
        {
            // Cuando una película no tiene poster, debe retornar la imagen placeholder
            var servicio = CrearServicio(HttpStatusCode.OK, "{}");

            var url = servicio.GetImageUrl(string.Empty);

            Assert.Equal("/images/no-poster.png", url);
        }

        [Fact]
        public void GetImageUrl_PosterPathNulo_RetornaImagenPorDefecto()
        {
            var servicio = CrearServicio(HttpStatusCode.OK, "{}");

            var url = servicio.GetImageUrl(null!);

            Assert.Equal("/images/no-poster.png", url);
        }

        [Fact]
        public void GetImageUrl_PosterPathValido_RetornaUrlCompleta()
        {
            // Verifica que concatena correctamente ImageBaseUrl + posterPath
            var servicio = CrearServicio(HttpStatusCode.OK, "{}");

            var url = servicio.GetImageUrl("/abc123.jpg");

            Assert.Equal("https://image.tmdb.org/t/p/w500/abc123.jpg", url);
        }

        // ═══════════════════════════════════════════════════════════
        // SEARCH MOVIES (caso sin HTTP: query vacía)
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task SearchMoviesAsync_QueryVacia_RetornaListaVaciaSinLlamarHttp()
        {
            // Si la query está vacía, el servicio retorna inmediatamente
            // sin hacer ninguna llamada HTTP (optimización)
            var servicio = CrearServicio(HttpStatusCode.OK, "{}");

            var resultado = await servicio.SearchMoviesAsync(string.Empty);

            Assert.Empty(resultado);
        }

        [Fact]
        public async Task SearchMoviesAsync_QuerySoloEspacios_RetornaListaVacia()
        {
            var servicio = CrearServicio(HttpStatusCode.OK, "{}");

            var resultado = await servicio.SearchMoviesAsync("   ");

            Assert.Empty(resultado);
        }

        [Fact]
        public async Task SearchMoviesAsync_RespuestaExitosa_RetornaPeliculas()
        {
            // JSON que simula la respuesta real de TMDB al buscar películas
            var jsonRespuesta = """
                {
                    "page": 1,
                    "results": [
                        {
                            "id": 101,
                            "title": "Batman Begins",
                            "overview": "El origen de Batman",
                            "poster_path": "/batman.jpg",
                            "release_date": "2005-06-15",
                            "vote_average": 7.9,
                            "genre_ids": [28, 18],
                            "backdrop_path": "/bg.jpg",
                            "popularity": 50.0
                        }
                    ],
                    "total_pages": 1,
                    "total_results": 1
                }
                """;

            var servicio = CrearServicio(HttpStatusCode.OK, jsonRespuesta);

            var resultado = await servicio.SearchMoviesAsync("Batman");

            Assert.Single(resultado);
            Assert.Equal(101, resultado[0].Id);
            Assert.Equal("Batman Begins", resultado[0].Title);
        }

        // ═══════════════════════════════════════════════════════════
        // GET POPULAR MOVIES
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task GetPopularMoviesAsync_RespuestaExitosa_RetornaPeliculas()
        {
            var jsonRespuesta = """
                {
                    "page": 1,
                    "results": [
                        {
                            "id": 550,
                            "title": "Fight Club",
                            "overview": "Un clásico",
                            "poster_path": "/fightclub.jpg",
                            "release_date": "1999-10-15",
                            "vote_average": 8.4,
                            "genre_ids": [18, 53],
                            "backdrop_path": "/bg.jpg",
                            "popularity": 120.5
                        },
                        {
                            "id": 238,
                            "title": "El Padrino",
                            "overview": "La mafia italiana",
                            "poster_path": "/godfather.jpg",
                            "release_date": "1972-03-24",
                            "vote_average": 9.2,
                            "genre_ids": [18, 80],
                            "backdrop_path": "/bg2.jpg",
                            "popularity": 98.3
                        }
                    ],
                    "total_pages": 10,
                    "total_results": 200
                }
                """;

            var servicio = CrearServicio(HttpStatusCode.OK, jsonRespuesta);

            var resultado = await servicio.GetPopularMoviesAsync();

            Assert.Equal(2, resultado.Count);
            Assert.Equal("Fight Club", resultado[0].Title);
            Assert.Equal("El Padrino", resultado[1].Title);
        }

        [Fact]
        public async Task GetPopularMoviesAsync_RespuestaFallida_RetornaListaVacia()
        {
            // Si la API devuelve error (500), el servicio retorna lista vacía
            // en lugar de lanzar una excepción que rompa la UI
            var servicio = CrearServicio(HttpStatusCode.InternalServerError, "{}");

            var resultado = await servicio.GetPopularMoviesAsync();

            Assert.Empty(resultado);
        }

        [Fact]
        public async Task GetPopularMoviesAsync_ApiNoEncontrada_RetornaListaVacia()
        {
            var servicio = CrearServicio(HttpStatusCode.NotFound, "Not Found");

            var resultado = await servicio.GetPopularMoviesAsync();

            Assert.Empty(resultado);
        }

        // ═══════════════════════════════════════════════════════════
        // GET MOVIE DETAIL
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task GetMovieDetailAsync_RespuestaExitosa_RetornaDetalle()
        {
            var jsonRespuesta = """
                {
                    "id": 550,
                    "title": "Fight Club",
                    "overview": "El primer paso...",
                    "poster_path": "/fightclub.jpg",
                    "backdrop_path": "/bg.jpg",
                    "release_date": "1999-10-15",
                    "vote_average": 8.4,
                    "runtime": 139,
                    "tagline": "Mischief. Mayhem. Soap.",
                    "genres": [
                        { "id": 18, "name": "Drama" },
                        { "id": 53, "name": "Thriller" }
                    ]
                }
                """;

            var servicio = CrearServicio(HttpStatusCode.OK, jsonRespuesta);

            var detalle = await servicio.GetMovieDetailAsync(550);

            Assert.NotNull(detalle);
            Assert.Equal(550, detalle.Id);
            Assert.Equal("Fight Club", detalle.Title);
            Assert.Equal(139, detalle.Runtime);
            Assert.Equal(2, detalle.Genres.Count);
            Assert.Equal("Drama", detalle.Genres[0].Name);
        }

        [Fact]
        public async Task GetMovieDetailAsync_PeliculaNoEncontrada_RetornaNull()
        {
            // Si la película no existe en TMDB (404), el servicio retorna null
            var servicio = CrearServicio(HttpStatusCode.NotFound, "{}");

            var detalle = await servicio.GetMovieDetailAsync(999999);

            Assert.Null(detalle);
        }
    }
}

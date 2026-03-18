using CineTracker.Models;
using CineTracker.Services;
using CineTracker.Tests.Helpers;

namespace CineTracker.Tests
{
    /// <summary>
    /// Pruebas unitarias para WatchlistService.
    ///
    /// WatchlistService maneja la lista de películas de cada usuario:
    ///   - GetAllAsync          → obtiene todos los items del usuario
    ///   - GetFavoritesAsync    → obtiene solo los favoritos
    ///   - IsInWatchlistAsync   → verifica si una película ya está en la lista
    ///   - AddToWatchlistAsync  → agrega una película (ignora duplicados)
    ///   - ToggleFavoriteAsync  → alterna el estado favorito (true/false)
    ///   - ToggleWatchedAsync   → alterna el estado "vista" (true/false)
    ///   - RemoveFromWatchlistAsync → elimina una película de la lista
    /// </summary>
    public class WatchlistServiceTests
    {
        // ─────────────────────────────────────────────────────────
        // Helpers: crean objetos reutilizables en los tests
        // ─────────────────────────────────────────────────────────

        private static WatchlistService CrearServicio(out string dbName)
        {
            dbName = Guid.NewGuid().ToString();
            var factory = new TestDbContextFactory(dbName);
            return new WatchlistService(factory);
        }

        /// <summary>Crea una película TMDB de prueba con datos mínimos.</summary>
        private static TmdbMovie CrearPelicula(int id = 1, string titulo = "Película de prueba")
        {
            return new TmdbMovie
            {
                Id = id,
                Title = titulo,
                PosterPath = "/poster.jpg",
                ReleaseDate = "2024-01-01",
                VoteAverage = 7.5
            };
        }

        // ═══════════════════════════════════════════════════════════
        // GET ALL
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task GetAllAsync_BDVacia_RetornaListaVacia()
        {
            var servicio = CrearServicio(out _);

            var resultado = await servicio.GetAllAsync(usuarioId: 1);

            Assert.Empty(resultado); // lista vacía cuando no hay películas
        }

        [Fact]
        public async Task GetAllAsync_ConPeliculas_RetornaTodasDelUsuario()
        {
            var servicio = CrearServicio(out _);
            int usuarioId = 1;

            // Agregar dos películas al mismo usuario
            await servicio.AddToWatchlistAsync(CrearPelicula(id: 10, "Inception"), usuarioId);
            await servicio.AddToWatchlistAsync(CrearPelicula(id: 20, "Interstellar"), usuarioId);

            var resultado = await servicio.GetAllAsync(usuarioId);

            Assert.Equal(2, resultado.Count);
        }

        [Fact]
        public async Task GetAllAsync_SoloRetornaPeliculasDelUsuarioCorrecto()
        {
            // Verifica que cada usuario solo ve SUS películas, no las de otros
            var servicio = CrearServicio(out _);

            await servicio.AddToWatchlistAsync(CrearPelicula(id: 1), usuarioId: 1);
            await servicio.AddToWatchlistAsync(CrearPelicula(id: 2), usuarioId: 2);

            var listaUsuario1 = await servicio.GetAllAsync(usuarioId: 1);
            var listaUsuario2 = await servicio.GetAllAsync(usuarioId: 2);

            Assert.Single(listaUsuario1);  // usuario 1 solo tiene 1 película
            Assert.Single(listaUsuario2);  // usuario 2 solo tiene 1 película
        }

        // ═══════════════════════════════════════════════════════════
        // ADD TO WATCHLIST
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task AddToWatchlistAsync_NuevaPelicula_SeGuardaCorrectamente()
        {
            var servicio = CrearServicio(out _);
            var pelicula = CrearPelicula(id: 5, "Matrix");

            await servicio.AddToWatchlistAsync(pelicula, usuarioId: 1);

            var lista = await servicio.GetAllAsync(usuarioId: 1);
            Assert.Single(lista);
            Assert.Equal("Matrix", lista[0].Title);
            Assert.Equal(5, lista[0].TmdbId);
        }

        [Fact]
        public async Task AddToWatchlistAsync_PeliculaDuplicada_NoSeAgreganDos()
        {
            // Si el usuario intenta agregar la misma película dos veces,
            // el servicio debe ignorar el duplicado
            var servicio = CrearServicio(out _);
            var pelicula = CrearPelicula(id: 99);

            await servicio.AddToWatchlistAsync(pelicula, usuarioId: 1);
            await servicio.AddToWatchlistAsync(pelicula, usuarioId: 1); // intento duplicado

            var lista = await servicio.GetAllAsync(usuarioId: 1);
            Assert.Single(lista); // sigue siendo 1, no 2
        }

        // ═══════════════════════════════════════════════════════════
        // IS IN WATCHLIST
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task IsInWatchlistAsync_PeliculaExistente_RetornaTrue()
        {
            var servicio = CrearServicio(out _);
            await servicio.AddToWatchlistAsync(CrearPelicula(id: 42), usuarioId: 1);

            var existe = await servicio.IsInWatchlistAsync(tmdbId: 42, usuarioId: 1);

            Assert.True(existe);
        }

        [Fact]
        public async Task IsInWatchlistAsync_PeliculaNoExistente_RetornaFalse()
        {
            var servicio = CrearServicio(out _);
            // No agregamos ninguna película

            var existe = await servicio.IsInWatchlistAsync(tmdbId: 999, usuarioId: 1);

            Assert.False(existe);
        }

        [Fact]
        public async Task IsInWatchlistAsync_PeliculaDeOtroUsuario_RetornaFalse()
        {
            // Película del usuario 1 NO debe aparecer en la lista del usuario 2
            var servicio = CrearServicio(out _);
            await servicio.AddToWatchlistAsync(CrearPelicula(id: 7), usuarioId: 1);

            var existe = await servicio.IsInWatchlistAsync(tmdbId: 7, usuarioId: 2);

            Assert.False(existe);
        }

        // ═══════════════════════════════════════════════════════════
        // TOGGLE FAVORITE
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task ToggleFavoriteAsync_PeliculaNoFavorita_PasaAFavorita()
        {
            var servicio = CrearServicio(out _);
            await servicio.AddToWatchlistAsync(CrearPelicula(id: 3), usuarioId: 1);

            // Al agregar, IsFavorite = false por defecto
            await servicio.ToggleFavoriteAsync(tmdbId: 3, usuarioId: 1);

            var lista = await servicio.GetAllAsync(usuarioId: 1);
            Assert.True(lista[0].IsFavorite); // ahora es favorita
        }

        [Fact]
        public async Task ToggleFavoriteAsync_PeliculaFavorita_PasaANoFavorita()
        {
            var servicio = CrearServicio(out _);
            await servicio.AddToWatchlistAsync(CrearPelicula(id: 3), usuarioId: 1);

            // Toggle 2 veces: false → true → false
            await servicio.ToggleFavoriteAsync(tmdbId: 3, usuarioId: 1);
            await servicio.ToggleFavoriteAsync(tmdbId: 3, usuarioId: 1);

            var lista = await servicio.GetAllAsync(usuarioId: 1);
            Assert.False(lista[0].IsFavorite); // vuelve a no favorita
        }

        [Fact]
        public async Task ToggleFavoriteAsync_PeliculaInexistente_NoLanzaExcepcion()
        {
            // El servicio debe ignorar silenciosamente si la película no existe
            var servicio = CrearServicio(out _);

            // No debe lanzar ninguna excepción
            await servicio.ToggleFavoriteAsync(tmdbId: 999, usuarioId: 1);
        }

        // ═══════════════════════════════════════════════════════════
        // TOGGLE WATCHED
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task ToggleWatchedAsync_PeliculaNoVista_PasaAVista()
        {
            var servicio = CrearServicio(out _);
            await servicio.AddToWatchlistAsync(CrearPelicula(id: 8), usuarioId: 1);

            await servicio.ToggleWatchedAsync(tmdbId: 8, usuarioId: 1);

            var lista = await servicio.GetAllAsync(usuarioId: 1);
            Assert.True(lista[0].IsWatched);
        }

        [Fact]
        public async Task ToggleWatchedAsync_PeliculaVista_PasaANoVista()
        {
            var servicio = CrearServicio(out _);
            await servicio.AddToWatchlistAsync(CrearPelicula(id: 8), usuarioId: 1);

            await servicio.ToggleWatchedAsync(tmdbId: 8, usuarioId: 1);
            await servicio.ToggleWatchedAsync(tmdbId: 8, usuarioId: 1);

            var lista = await servicio.GetAllAsync(usuarioId: 1);
            Assert.False(lista[0].IsWatched);
        }

        // ═══════════════════════════════════════════════════════════
        // REMOVE FROM WATCHLIST
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task RemoveFromWatchlistAsync_PeliculaExistente_SeElimina()
        {
            var servicio = CrearServicio(out _);
            await servicio.AddToWatchlistAsync(CrearPelicula(id: 15), usuarioId: 1);

            await servicio.RemoveFromWatchlistAsync(tmdbId: 15, usuarioId: 1);

            var lista = await servicio.GetAllAsync(usuarioId: 1);
            Assert.Empty(lista); // la lista debe quedar vacía
        }

        [Fact]
        public async Task RemoveFromWatchlistAsync_PeliculaInexistente_NoLanzaExcepcion()
        {
            var servicio = CrearServicio(out _);

            // Intentar eliminar algo que no existe no debe lanzar excepción
            await servicio.RemoveFromWatchlistAsync(tmdbId: 999, usuarioId: 1);
        }

        // ═══════════════════════════════════════════════════════════
        // GET FAVORITES
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task GetFavoritesAsync_ConMezclaFavoritosYNormales_RetornaSoloFavoritos()
        {
            var servicio = CrearServicio(out _);
            int usuarioId = 1;

            // Agregar 3 películas
            await servicio.AddToWatchlistAsync(CrearPelicula(id: 1, "Favorita A"), usuarioId);
            await servicio.AddToWatchlistAsync(CrearPelicula(id: 2, "Normal"), usuarioId);
            await servicio.AddToWatchlistAsync(CrearPelicula(id: 3, "Favorita B"), usuarioId);

            // Marcar como favoritas solo id=1 e id=3
            await servicio.ToggleFavoriteAsync(tmdbId: 1, usuarioId);
            await servicio.ToggleFavoriteAsync(tmdbId: 3, usuarioId);

            var favoritos = await servicio.GetFavoritesAsync(usuarioId);

            Assert.Equal(2, favoritos.Count);
            Assert.All(favoritos, item => Assert.True(item.IsFavorite));
        }

        [Fact]
        public async Task GetFavoritesAsync_SinFavoritos_RetornaListaVacia()
        {
            var servicio = CrearServicio(out _);
            await servicio.AddToWatchlistAsync(CrearPelicula(id: 1), usuarioId: 1);
            // No se marca ninguna como favorita

            var favoritos = await servicio.GetFavoritesAsync(usuarioId: 1);

            Assert.Empty(favoritos);
        }
    }
}

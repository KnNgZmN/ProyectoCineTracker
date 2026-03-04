using Microsoft.EntityFrameworkCore;
using CineTracker.Data;
using CineTracker.Models;

namespace CineTracker.Services
{
    /// <summary>
    /// Servicio para manejar la watchlist del usuario.
    /// Permite agregar, eliminar, marcar como vista/favorita y consultar películas.
    /// </summary>
    public class WatchlistService
    {
        private readonly IDbContextFactory<CineTrackerContext> _contextFactory;

        /// <summary>
        /// Constructor que inyecta la factoría de DbContext para Blazor Server.
        /// </summary>
        /// <param name="contextFactory">Factoría de DbContext</param>
        public WatchlistService(IDbContextFactory<CineTrackerContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Obtiene todos los elementos de la watchlist, ordenados por fecha de adición descendente.
        /// </summary>
        /// <returns>Lista de <see cref="WatchListItem"/></returns>
        public async Task<List<WatchListItem>> GetAllAsync()
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.WatchlistItems
                .OrderByDescending(w => w.DateAdded)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene únicamente las películas marcadas como favoritas.
        /// </summary>
        /// <returns>Lista de <see cref="WatchListItem"/> favoritos</returns>
        public async Task<List<WatchListItem>> GetFavoritesAsync()
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.WatchlistItems
                .Where(w => w.IsFavorite)
                .OrderByDescending(w => w.DateAdded)
                .ToListAsync();
        }

        /// <summary>
        /// Verifica si una película ya está en la watchlist.
        /// </summary>
        /// <param name="tmdbId">ID de TMDB de la película</param>
        /// <returns>True si existe, false en caso contrario</returns>
        public async Task<bool> IsInWatchlistAsync(int tmdbId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.WatchlistItems.AnyAsync(w => w.TmdbId == tmdbId);
        }

        /// <summary>
        /// Agrega una película a la watchlist.
        /// </summary>
        /// <param name="movie">Objeto <see cref="TmdbMovie"/> con los datos de la película</param>
        public async Task AddToWatchlistAsync(TmdbMovie movie)
        {
            await using var context = _contextFactory.CreateDbContext();

            if (await context.WatchlistItems.AnyAsync(w => w.TmdbId == movie.Id))
                return;

            var item = new WatchListItem
            {
                TmdbId = movie.Id,
                Title = movie.Title,
                PosterPath = movie.PosterPath,
                ReleaseDate = movie.ReleaseDate,
                VoteAverage = movie.VoteAverage,
                DateAdded = DateTime.UtcNow
            };

            context.WatchlistItems.Add(item);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Marca o desmarca una película como favorita.
        /// </summary>
        /// <param name="tmdbId">ID de TMDB de la película</param>
        public async Task ToggleFavoriteAsync(int tmdbId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var item = await context.WatchlistItems.FirstOrDefaultAsync(w => w.TmdbId == tmdbId);
            if (item is null) return;

            item.IsFavorite = !item.IsFavorite;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Marca o desmarca una película como vista.
        /// </summary>
        /// <param name="tmdbId">ID de TMDB de la película</param>
        public async Task ToggleWatchedAsync(int tmdbId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var item = await context.WatchlistItems.FirstOrDefaultAsync(w => w.TmdbId == tmdbId);
            if (item is null) return;

            item.IsWatched = !item.IsWatched;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Elimina una película de la watchlist.
        /// </summary>
        /// <param name="tmdbId">ID de TMDB de la película</param>
        public async Task RemoveFromWatchlistAsync(int tmdbId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var item = await context.WatchlistItems.FirstOrDefaultAsync(w => w.TmdbId == tmdbId);
            if (item is null) return;

            context.WatchlistItems.Remove(item);
            await context.SaveChangesAsync();
        }
    }
}
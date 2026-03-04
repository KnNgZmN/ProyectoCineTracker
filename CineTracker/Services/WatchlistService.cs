using Microsoft.EntityFrameworkCore;
using CineTracker.Data;
using CineTracker.Models;

namespace CineTracker.Services
{
using Microsoft.EntityFrameworkCore;

    public class WatchlistService
    {
        private readonly IDbContextFactory<CineTrackerContext> _contextFactory;

        public WatchlistService(IDbContextFactory<CineTrackerContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // Obtener toda la watchlist
        public async Task<List<WatchListItem>> GetAllAsync()
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.WatchlistItems
                .OrderByDescending(w => w.DateAdded)
                .ToListAsync();
        }

        // Obtener solo favoritos
        public async Task<List<WatchListItem>> GetFavoritesAsync()
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.WatchlistItems
                .Where(w => w.IsFavorite)
                .OrderByDescending(w => w.DateAdded)
                .ToListAsync();
        }

        // Verificar si una película ya está en la lista
        public async Task<bool> IsInWatchlistAsync(int tmdbId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.WatchlistItems
                .AnyAsync(w => w.TmdbId == tmdbId);
        }

        // Agregar película a la watchlist
        public async Task AddToWatchlistAsync(TmdbMovie movie)
        {
            await using var context = _contextFactory.CreateDbContext();

            var exists = await context.WatchlistItems.AnyAsync(w => w.TmdbId == movie.Id);
            if (exists) return;

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

        // Marcar/desmarcar como favorito
        public async Task ToggleFavoriteAsync(int tmdbId)
        {
            await using var context = _contextFactory.CreateDbContext();

            var item = await context.WatchlistItems
                .FirstOrDefaultAsync(w => w.TmdbId == tmdbId);

            if (item is null) return;

            item.IsFavorite = !item.IsFavorite;
            await context.SaveChangesAsync();
        }

        // Marcar como vista
        public async Task ToggleWatchedAsync(int tmdbId)
        {
            await using var context = _contextFactory.CreateDbContext();

            var item = await context.WatchlistItems
                .FirstOrDefaultAsync(w => w.TmdbId == tmdbId);

            if (item is null) return;

            item.IsWatched = !item.IsWatched;
            await context.SaveChangesAsync();
        }

        // Eliminar de la watchlist
        public async Task RemoveFromWatchlistAsync(int tmdbId)
        {
            await using var context = _contextFactory.CreateDbContext();

            var item = await context.WatchlistItems
                .FirstOrDefaultAsync(w => w.TmdbId == tmdbId);

            if (item is null) return;

            context.WatchlistItems.Remove(item);
            await context.SaveChangesAsync();
        }
    }
}

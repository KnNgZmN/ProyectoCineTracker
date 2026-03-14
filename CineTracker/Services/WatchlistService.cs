using Microsoft.EntityFrameworkCore;
using CineTracker.Data;
using CineTracker.Models;

namespace CineTracker.Services
{
    // Maneja las operaciones CRUD de la watchlist de cada usuario
    public class WatchlistService
    {
        // IDbContextFactory crea un contexto por operación → obligatorio en Blazor Server (thread-safe)
        private readonly IDbContextFactory<CineTrackerContext> _contextFactory;

        public WatchlistService(IDbContextFactory<CineTrackerContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // Devuelve todos los items del usuario ordenados por fecha de agregado
        public async Task<List<WatchListItem>> GetAllAsync(int usuarioId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.WatchlistItems
                .Where(w => w.UsuarioId == usuarioId)
                .OrderByDescending(w => w.DateAdded)
                .ToListAsync();
        }

        // Devuelve solo los items marcados como favoritos
        public async Task<List<WatchListItem>> GetFavoritesAsync(int usuarioId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.WatchlistItems
                .Where(w => w.UsuarioId == usuarioId && w.IsFavorite)
                .OrderByDescending(w => w.DateAdded)
                .ToListAsync();
        }

        // Verifica si una película ya está en la lista (usa EXISTS en SQL, más eficiente que COUNT)
        public async Task<bool> IsInWatchlistAsync(int tmdbId, int usuarioId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.WatchlistItems
                .AnyAsync(w => w.TmdbId == tmdbId && w.UsuarioId == usuarioId);
        }

        // Agrega una película a la watchlist del usuario. Ignora si ya existe
        public async Task AddToWatchlistAsync(TmdbMovie movie, int usuarioId)
        {
            await using var context = _contextFactory.CreateDbContext();

            if (await context.WatchlistItems.AnyAsync(w => w.TmdbId == movie.Id && w.UsuarioId == usuarioId))
                return;

            var item = new WatchListItem
            {
                TmdbId = movie.Id,
                Title = movie.Title,
                PosterPath = movie.PosterPath,
                ReleaseDate = movie.ReleaseDate,
                VoteAverage = movie.VoteAverage,
                UsuarioId = usuarioId,
                DateAdded = DateTime.UtcNow
            };

            context.WatchlistItems.Add(item);
            await context.SaveChangesAsync();
        }

        // Alterna el estado de favorito (true → false, false → true)
        public async Task ToggleFavoriteAsync(int tmdbId, int usuarioId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var item = await context.WatchlistItems
                .FirstOrDefaultAsync(w => w.TmdbId == tmdbId && w.UsuarioId == usuarioId);
            if (item is null) return;

            item.IsFavorite = !item.IsFavorite;
            await context.SaveChangesAsync();
        }

        // Alterna el estado de "vista" (true → false, false → true)
        public async Task ToggleWatchedAsync(int tmdbId, int usuarioId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var item = await context.WatchlistItems
                .FirstOrDefaultAsync(w => w.TmdbId == tmdbId && w.UsuarioId == usuarioId);
            if (item is null) return;

            item.IsWatched = !item.IsWatched;
            await context.SaveChangesAsync();
        }

        // Elimina una película de la lista del usuario
        public async Task RemoveFromWatchlistAsync(int tmdbId, int usuarioId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var item = await context.WatchlistItems
                .FirstOrDefaultAsync(w => w.TmdbId == tmdbId && w.UsuarioId == usuarioId);
            if (item is null) return;

            context.WatchlistItems.Remove(item);
            await context.SaveChangesAsync();
        }
    }
}

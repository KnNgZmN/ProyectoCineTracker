using Microsoft.EntityFrameworkCore;
using CineTracker.Data;
using CineTracker.Models;

namespace CineTracker.Services
{
    /// <summary>
    /// Servicio para manejar la watchlist de cada usuario por separado.
    /// Todos los métodos reciben el ID del usuario autenticado para que
    /// cada persona solo vea y modifique sus propios datos en la base de datos.
    /// Se registra como Scoped en Program.cs.
    /// </summary>
    public class WatchlistService
    {
        // IDbContextFactory es necesario en Blazor Server porque el DbContext normal
        // no es thread-safe. El factory crea una instancia nueva por operación,
        // lo que permite que varios usuarios interactúen sin interferirse.
        private readonly IDbContextFactory<CineTrackerContext> _contextFactory;

        public WatchlistService(IDbContextFactory<CineTrackerContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Obtiene todas las películas de la lista del usuario indicado,
        /// ordenadas de más reciente a más antigua.
        /// </summary>
        /// <param name="usuarioId">ID del usuario autenticado, viene del claim de la cookie</param>
        public async Task<List<WatchListItem>> GetAllAsync(int usuarioId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.WatchlistItems
                // WHERE UsuarioId = @usuarioId — filtra solo los registros del usuario actual
                .Where(w => w.UsuarioId == usuarioId)
                // ORDER BY DateAdded DESC — más recientes primero
                .OrderByDescending(w => w.DateAdded)
                // Ejecuta la consulta y trae los resultados a memoria como List<>
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene solo las películas marcadas como favoritas por el usuario.
        /// </summary>
        /// <param name="usuarioId">ID del usuario autenticado</param>
        public async Task<List<WatchListItem>> GetFavoritesAsync(int usuarioId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.WatchlistItems
                // WHERE UsuarioId = @usuarioId AND IsFavorite = 1
                .Where(w => w.UsuarioId == usuarioId && w.IsFavorite)
                .OrderByDescending(w => w.DateAdded)
                .ToListAsync();
        }

        /// <summary>
        /// Verifica si una película ya está en la lista del usuario.
        /// Se usa para mostrar el botón "Agregada" en las tarjetas de películas.
        /// </summary>
        /// <param name="tmdbId">ID de la película en la API de TMDB</param>
        /// <param name="usuarioId">ID del usuario autenticado</param>
        public async Task<bool> IsInWatchlistAsync(int tmdbId, int usuarioId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.WatchlistItems
                // AnyAsync genera un EXISTS en SQL, que es más eficiente que COUNT o SELECT *
                // SQL: SELECT CASE WHEN EXISTS(SELECT 1 FROM WatchlistItems WHERE TmdbId=@t AND UsuarioId=@u) ...
                .AnyAsync(w => w.TmdbId == tmdbId && w.UsuarioId == usuarioId);
        }

        /// <summary>
        /// Agrega una película a la lista del usuario.
        /// Los datos de la película vienen del objeto TmdbMovie, que fue obtenido
        /// previamente desde la API externa de TMDB (The Movie Database).
        /// Se guarda una copia local en SQL Server para no depender siempre de la API.
        /// </summary>
        /// <param name="movie">
        /// Objeto TmdbMovie con datos de la API de TMDB:
        /// - Id: identificador único de TMDB (diferente al Id local de SQL Server)
        /// - Title: título original de la película
        /// - PosterPath: ruta relativa del poster en los servidores de TMDB
        ///   (ej: "/abc123.jpg", se combina con "https://image.tmdb.org/t/p/w500")
        /// - ReleaseDate: fecha de estreno en formato "YYYY-MM-DD"
        /// - VoteAverage: promedio de calificación de los usuarios de TMDB (0.0 - 10.0)
        /// </param>
        /// <param name="usuarioId">ID del usuario que agrega la película</param>
        public async Task AddToWatchlistAsync(TmdbMovie movie, int usuarioId)
        {
            await using var context = _contextFactory.CreateDbContext();

            // Verifica primero si ya existe para evitar duplicados del mismo usuario
            if (await context.WatchlistItems.AnyAsync(w => w.TmdbId == movie.Id && w.UsuarioId == usuarioId))
                return;

            // Crea el registro local copiando los datos relevantes de TMDB
            // y asociándolo al usuario mediante UsuarioId (FK)
            var item = new WatchListItem
            {
                TmdbId = movie.Id,           // ID de TMDB, para poder volver a consultar la API después
                Title = movie.Title,
                PosterPath = movie.PosterPath, // Solo guardamos la ruta relativa, no la URL completa
                ReleaseDate = movie.ReleaseDate,
                VoteAverage = movie.VoteAverage,
                UsuarioId = usuarioId,         // Vincula el item al usuario que lo agrega
                DateAdded = DateTime.UtcNow
            };

            context.WatchlistItems.Add(item);
            // SaveChangesAsync ejecuta: INSERT INTO WatchlistItems (...) VALUES (...)
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Activa o desactiva el estado de favorito de una película en la lista del usuario.
        /// </summary>
        /// <param name="tmdbId">ID de TMDB de la película</param>
        /// <param name="usuarioId">ID del usuario autenticado</param>
        public async Task ToggleFavoriteAsync(int tmdbId, int usuarioId)
        {
            await using var context = _contextFactory.CreateDbContext();
            // Busca el registro exacto: película + usuario (evita modificar datos de otro usuario)
            var item = await context.WatchlistItems
                .FirstOrDefaultAsync(w => w.TmdbId == tmdbId && w.UsuarioId == usuarioId);
            if (item is null) return;

            // Invierte el valor booleano: true->false o false->true
            item.IsFavorite = !item.IsFavorite;
            // SaveChangesAsync ejecuta: UPDATE WatchlistItems SET IsFavorite=@val WHERE Id=@id
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Activa o desactiva el estado de "vista" de una película en la lista del usuario.
        /// </summary>
        /// <param name="tmdbId">ID de TMDB de la película</param>
        /// <param name="usuarioId">ID del usuario autenticado</param>
        public async Task ToggleWatchedAsync(int tmdbId, int usuarioId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var item = await context.WatchlistItems
                .FirstOrDefaultAsync(w => w.TmdbId == tmdbId && w.UsuarioId == usuarioId);
            if (item is null) return;

            item.IsWatched = !item.IsWatched;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Elimina una película de la lista del usuario.
        /// </summary>
        /// <param name="tmdbId">ID de TMDB de la película</param>
        /// <param name="usuarioId">ID del usuario autenticado</param>
        public async Task RemoveFromWatchlistAsync(int tmdbId, int usuarioId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var item = await context.WatchlistItems
                .FirstOrDefaultAsync(w => w.TmdbId == tmdbId && w.UsuarioId == usuarioId);
            if (item is null) return;

            // Remove marca la entidad para eliminar
            context.WatchlistItems.Remove(item);
            // SaveChangesAsync ejecuta: DELETE FROM WatchlistItems WHERE Id=@id
            await context.SaveChangesAsync();
        }
    }
}

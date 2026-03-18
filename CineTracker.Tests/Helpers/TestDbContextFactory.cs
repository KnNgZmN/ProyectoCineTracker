using CineTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace CineTracker.Tests.Helpers
{
    /// <summary>
    /// Implementación de IDbContextFactory para pruebas unitarias.
    ///
    /// En lugar de conectarse a PostgreSQL real, usa una base de datos
    /// EN MEMORIA que vive solo durante la ejecución del test.
    ///
    /// Cada test crea una instancia con un nombre de BD único (Guid),
    /// así los tests no se contaminan entre sí.
    /// </summary>
    public class TestDbContextFactory : IDbContextFactory<CineTrackerContext>
    {
        private readonly DbContextOptions<CineTrackerContext> _options;

        /// <param name="databaseName">Nombre único para la BD en memoria (usa Guid.NewGuid().ToString())</param>
        public TestDbContextFactory(string databaseName)
        {
            _options = new DbContextOptionsBuilder<CineTrackerContext>()
                .UseInMemoryDatabase(databaseName)  // BD en memoria, no requiere PostgreSQL
                .Options;
        }

        /// <summary>
        /// Crea y retorna un nuevo DbContext apuntando a la misma BD en memoria.
        /// Cada llamada devuelve un contexto distinto pero comparten los mismos datos.
        /// </summary>
        public CineTrackerContext CreateDbContext()
        {
            return new CineTrackerContext(_options);
        }
    }
}

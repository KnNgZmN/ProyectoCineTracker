// PasswordHasher<T> viene de Microsoft.AspNetCore.Identity
// Es parte del framework de ASP.NET Core y no requiere paquete extra.
// Implementa el algoritmo PBKDF2 con sal aleatoria para proteger contraseñas.
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CineTracker.Data;
using CineTracker.Models;

namespace CineTracker.Services
{
    /// <summary>
    /// Servicio de autenticación: maneja el registro e inicio de sesión de usuarios.
    /// Se registra como Scoped en Program.cs, lo que significa que se crea una instancia
    /// por cada solicitud HTTP y se destruye al terminar.
    /// </summary>
    public class AuthService
    {
        // IDbContextFactory permite crear instancias del DbContext de forma segura
        // en entornos concurrentes como Blazor Server, donde múltiples usuarios
        // comparten el mismo servidor pero cada operación necesita su propio contexto.
        private readonly IDbContextFactory<CineTrackerContext> _contextFactory;

        // PasswordHasher<Usuario> es la clase de ASP.NET Core que convierte
        // una contraseña de texto plano en un hash seguro (y viceversa para verificar).
        // El tipo genérico <Usuario> es solo para contexto; no accede a sus propiedades.
        private readonly PasswordHasher<Usuario> _hasher = new();

        /// <summary>
        /// Constructor: recibe el factory del DbContext mediante inyección de dependencias.
        /// ASP.NET Core lo inyecta automáticamente porque está registrado en Program.cs.
        /// </summary>
        public AuthService(IDbContextFactory<CineTrackerContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Registra un nuevo usuario en la base de datos.
        /// Retorna una tupla (bool Success, string Error) para comunicar el resultado.
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario visible</param>
        /// <param name="email">Email usado para iniciar sesión</param>
        /// <param name="password">Contraseña en texto plano (se hashea antes de guardar)</param>
        public async Task<(bool Success, string Error)> RegistrarAsync(string nombreUsuario, string email, string password)
        {
            // "await using" garantiza que el DbContext se libere correctamente
            // aunque ocurra una excepción (equivalente a using + Dispose).
            await using var context = _contextFactory.CreateDbContext();

            // AnyAsync genera: SELECT CASE WHEN EXISTS(SELECT 1 FROM Usuarios WHERE Email = @email) THEN 1 ELSE 0 END
            // Es más eficiente que traer el registro completo solo para verificar si existe.
            if (await context.Usuarios.AnyAsync(u => u.Email == email))
                return (false, "El email ya está registrado.");

            if (await context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario))
                return (false, "El nombre de usuario ya está en uso.");

            // Se crea el objeto sin PasswordHash para poder pasarlo al hasher
            var usuario = new Usuario
            {
                NombreUsuario = nombreUsuario,
                Email = email,
                FechaCreacion = DateTime.UtcNow
            };

            // HashPassword genera un hash seguro que incluye:
            // - Algoritmo: PBKDF2 con HMAC-SHA512
            // - Una "sal" aleatoria única para este usuario
            // - 100.000 iteraciones para dificultar ataques de fuerza bruta
            // El resultado es una cadena base64 que se guarda en PasswordHash.
            usuario.PasswordHash = _hasher.HashPassword(usuario, password);

            // Add marca la entidad como "pendiente de insertar"
            context.Usuarios.Add(usuario);

            // SaveChangesAsync ejecuta el INSERT en SQL Server y confirma la transacción
            await context.SaveChangesAsync();

            return (true, string.Empty);
        }

        /// <summary>
        /// Verifica las credenciales de un usuario.
        /// Retorna el objeto Usuario si son correctas, o null si fallan.
        /// </summary>
        /// <param name="email">Email ingresado en el formulario</param>
        /// <param name="password">Contraseña en texto plano ingresada en el formulario</param>
        public async Task<Usuario?> LoginAsync(string email, string password)
        {
            await using var context = _contextFactory.CreateDbContext();

            // Busca el usuario por email. FirstOrDefaultAsync retorna null si no existe,
            // evitando así revelar si el email existe o no (buena práctica de seguridad
            // aunque aquí mostramos un mensaje genérico de error).
            var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario is null) return null;

            // VerifyHashedPassword compara la contraseña ingresada con el hash guardado.
            // Internamente repite el proceso de hasheo con la misma sal y compara.
            // Retorna PasswordVerificationResult.Success, Failed, o SuccessRehashNeeded.
            var result = _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, password);

            // Solo retorna el usuario si la verificación fue exitosa
            return result == PasswordVerificationResult.Success ? usuario : null;
        }
    }
}

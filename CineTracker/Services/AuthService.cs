using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CineTracker.Data;
using CineTracker.Models;

namespace CineTracker.Services
{
    // Maneja registro e inicio de sesión de usuarios
    public class AuthService
    {
        private readonly IDbContextFactory<CineTrackerContext> _contextFactory;
        private readonly PasswordHasher<Usuario> _hasher = new(); // PBKDF2 con sal aleatoria

        public AuthService(IDbContextFactory<CineTrackerContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // Registra un nuevo usuario. Retorna (true, "") si fue exitoso o (false, "mensaje") si hubo error
        public async Task<(bool Success, string Error)> RegistrarAsync(string nombreUsuario, string email, string password)
        {
            await using var context = _contextFactory.CreateDbContext();

            // Verifica duplicados antes de insertar
            if (await context.Usuarios.AnyAsync(u => u.Email == email))
                return (false, "El email ya está registrado.");

            if (await context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario))
                return (false, "El nombre de usuario ya está en uso.");

            var usuario = new Usuario
            {
                NombreUsuario = nombreUsuario,
                Email = email,
                FechaCreacion = DateTime.UtcNow
            };

            // Genera el hash seguro antes de guardar en la base de datos
            usuario.PasswordHash = _hasher.HashPassword(usuario, password);

            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            return (true, string.Empty);
        }

        // Verifica credenciales. Retorna el Usuario si son correctas, o null si fallan
        public async Task<Usuario?> LoginAsync(string email, string password)
        {
            await using var context = _contextFactory.CreateDbContext();

            var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario is null) return null;

            // Compara la contraseña ingresada con el hash guardado
            var result = _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, password);
            return result == PasswordVerificationResult.Success ? usuario : null;
        }
    }
}

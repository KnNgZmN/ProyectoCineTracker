using CineTracker.Services;
using CineTracker.Tests.Helpers;

namespace CineTracker.Tests
{
    /// <summary>
    /// Pruebas unitarias para AuthService.
    ///
    /// AuthService maneja:
    ///   - RegistrarAsync → crea un nuevo usuario en la BD
    ///   - LoginAsync     → verifica email + contraseña y retorna el usuario
    ///
    /// Cada prueba usa su propia BD en memoria (nombre único con Guid)
    /// para que los datos de un test no afecten a otro.
    /// </summary>
    public class AuthServiceTests
    {
        // ─────────────────────────────────────────────────────────
        // Helper: crea un AuthService con BD en memoria vacía
        // ─────────────────────────────────────────────────────────
        private static AuthService CrearServicio(out string dbName)
        {
            dbName = Guid.NewGuid().ToString(); // nombre único por test
            var factory = new TestDbContextFactory(dbName);
            return new AuthService(factory);
        }

        // ═══════════════════════════════════════════════════════════
        // REGISTRO
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task RegistrarAsync_DatosValidos_RetornaExito()
        {
            // ARRANGE: preparar el servicio y los datos de entrada
            var servicio = CrearServicio(out _);

            // ACT: llamar al método que queremos probar
            var (exito, error) = await servicio.RegistrarAsync("juan123", "juan@email.com", "Password123");

            // ASSERT: verificar que el resultado es el esperado
            Assert.True(exito);           // el registro debe haber sido exitoso
            Assert.Equal("", error);      // no debe haber mensaje de error
        }

        [Fact]
        public async Task RegistrarAsync_EmailDuplicado_RetornaError()
        {
            // ARRANGE: registrar un usuario primero
            var servicio = CrearServicio(out _);
            await servicio.RegistrarAsync("usuario1", "duplicado@email.com", "Password123");

            // ACT: intentar registrar OTRO usuario con el mismo email
            var (exito, error) = await servicio.RegistrarAsync("usuario2", "duplicado@email.com", "OtraPassword");

            // ASSERT: debe fallar con el mensaje apropiado
            Assert.False(exito);
            Assert.Contains("email", error, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RegistrarAsync_NombreUsuarioDuplicado_RetornaError()
        {
            // ARRANGE: registrar un usuario primero
            var servicio = CrearServicio(out _);
            await servicio.RegistrarAsync("nombreduplic", "primer@email.com", "Password123");

            // ACT: intentar registrar OTRO usuario con el mismo nombre de usuario
            var (exito, error) = await servicio.RegistrarAsync("nombreduplic", "segundo@email.com", "Password123");

            // ASSERT: debe fallar indicando que el nombre ya está en uso
            Assert.False(exito);
            Assert.Contains("usuario", error, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RegistrarAsync_DosUsuariosDistintos_AmbosTienenExito()
        {
            // Verifica que se pueden registrar múltiples usuarios con datos diferentes
            var servicio = CrearServicio(out _);

            var (exito1, _) = await servicio.RegistrarAsync("usuario_a", "a@email.com", "Pass1");
            var (exito2, _) = await servicio.RegistrarAsync("usuario_b", "b@email.com", "Pass2");

            Assert.True(exito1);
            Assert.True(exito2);
        }

        // ═══════════════════════════════════════════════════════════
        // LOGIN
        // ═══════════════════════════════════════════════════════════

        [Fact]
        public async Task LoginAsync_CredencialesCorrectas_RetornaUsuario()
        {
            // ARRANGE: registrar un usuario para luego intentar iniciar sesión
            var servicio = CrearServicio(out _);
            await servicio.RegistrarAsync("maria99", "maria@email.com", "MiPassword");

            // ACT: iniciar sesión con las credenciales correctas
            var usuario = await servicio.LoginAsync("maria@email.com", "MiPassword");

            // ASSERT: debe retornar el objeto Usuario (no null)
            Assert.NotNull(usuario);
            Assert.Equal("maria99", usuario.NombreUsuario);
            Assert.Equal("maria@email.com", usuario.Email);
        }

        [Fact]
        public async Task LoginAsync_EmailInexistente_RetornaNull()
        {
            // ARRANGE: servicio con BD vacía (ningún usuario registrado)
            var servicio = CrearServicio(out _);

            // ACT: intentar iniciar sesión con un email que no existe
            var usuario = await servicio.LoginAsync("noexiste@email.com", "cualquierpass");

            // ASSERT: debe retornar null porque el email no está registrado
            Assert.Null(usuario);
        }

        [Fact]
        public async Task LoginAsync_PasswordIncorrecta_RetornaNull()
        {
            // ARRANGE: registrar un usuario con una contraseña conocida
            var servicio = CrearServicio(out _);
            await servicio.RegistrarAsync("carlos", "carlos@email.com", "PasswordCorrecta");

            // ACT: intentar iniciar sesión con la contraseña INCORRECTA
            var usuario = await servicio.LoginAsync("carlos@email.com", "PasswordIncorrecta");

            // ASSERT: debe retornar null porque la contraseña no coincide
            Assert.Null(usuario);
        }

        [Fact]
        public async Task LoginAsync_PasswordVacia_RetornaNull()
        {
            // Caso borde: contraseña en blanco nunca debe pasar la verificación
            var servicio = CrearServicio(out _);
            await servicio.RegistrarAsync("luis", "luis@email.com", "PasswordReal");

            var usuario = await servicio.LoginAsync("luis@email.com", "");

            Assert.Null(usuario);
        }
    }
}

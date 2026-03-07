namespace CineTracker.Models
{
    /// <summary>
    /// Representa un usuario registrado en la aplicación.
    /// Esta clase se mapea directamente a la tabla "Usuarios" en SQL Server mediante Entity Framework.
    /// Cada propiedad pública se convierte en una columna de la tabla.
    /// </summary>
    public class Usuario
    {
        /// <summary>
        /// Clave primaria del usuario en la base de datos.
        /// EF Core la detecta automáticamente por convención de nombre (Id).
        /// SQL Server la genera como IDENTITY (autoincremento).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre visible del usuario dentro de la aplicación.
        /// Debe ser único — se configura un índice UNIQUE en CineTrackerContext.
        /// Máximo 50 caracteres (configurado en OnModelCreating).
        /// </summary>
        public string NombreUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico del usuario, usado como identificador de login.
        /// Debe ser único — se configura un índice UNIQUE en CineTrackerContext.
        /// Máximo 200 caracteres.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario almacenada como hash seguro.
        /// NUNCA se guarda la contraseña en texto plano.
        /// Se genera con PasswordHasher de ASP.NET Core Identity (algoritmo PBKDF2).
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora en que se creó la cuenta.
        /// Se usa DateTime.UtcNow para registrar en tiempo universal coordinado
        /// y evitar problemas con zonas horarias.
        /// </summary>
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}

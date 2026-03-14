namespace CineTracker.Models
{
    // Usuario registrado en la aplicación → tabla "usuarios" en PostgreSQL
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty; // único, máx 50 caracteres
        public string Email { get; set; } = string.Empty;         // único, usado para login
        public string PasswordHash { get; set; } = string.Empty;  // hash PBKDF2, nunca texto plano
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow; // UTC para evitar problemas de zona horaria
    }
}

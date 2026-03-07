using CineTracker;
using CineTracker.Components;
using CineTracker.Data;
using CineTracker.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;


/// <summary>
/// Punto de entrada de la aplicación CineTracker.
/// Configura servicios, base de datos, API externa (TMDB) y componentes Razor interactivos.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Configuración de la aplicación desde varias fuentes:
/// - appsettings.json (configuración obligatoria)
/// - User Secrets (para claves sensibles durante desarrollo)
/// - Variables de entorno (útil para producción)
/// </summary>
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();

/// <summary>
/// Configura los ajustes de TMDB (The Movie Database) inyectables mediante IOptions.
/// Se leen desde la sección "TmdbSettings" de appsettings.json o User Secrets.
/// </summary>
builder.Services.Configure<TmdbSettings>(
    builder.Configuration.GetSection("TmdbSettings"));

/// <summary>
/// Configuración de la base de datos:
/// - Se utiliza DbContextFactory para Blazor Server y operaciones concurrentes seguras.
/// - CineTrackerContext representa las tablas y relaciones de la base de datos.
/// - UseSqlServer conecta a SQL Server con la cadena de conexión "CineTrackerDB".
/// </summary>
builder.Services.AddDbContextFactory<CineTrackerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CineTrackerDB")));

/// <summary>
/// Registro de servicios relacionados con TMDB:
/// - AddHttpClient<TmdbService>() crea un cliente HTTP inyectable para consumir la API de TMDB.
/// - WatchlistService se registra como Scoped para manejar listas de seguimiento de películas/series.
/// </summary>
builder.Services.AddHttpClient<TmdbService>();
builder.Services.AddScoped<WatchlistService>();
builder.Services.AddScoped<AuthService>();

/// <summary>
/// Registro de Razor Components y configuración interactiva del lado del servidor.
/// Permite renderizado dinámico y componentes reutilizables en Blazor Server.
/// </summary>
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddCascadingAuthenticationState();

/// <summary>
/// Construye la aplicación con toda la configuración de servicios y pipeline.
/// </summary>
var app = builder.Build();

/// <summary>
/// Configuración del pipeline HTTP:
/// - En producción, se maneja un error global mediante /Error.
/// - HSTS asegura que el navegador use HTTPS automáticamente.
/// </summary>
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

/// <summary>
/// Manejo de páginas de error por código HTTP (como 404):
/// - Redirige a /not-found con acceso a servicios inyectables.
/// </summary>
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

/// <summary>
/// Redirige automáticamente todo HTTP a HTTPS.
/// </summary>
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

/// <summary>
/// Habilita protección antifalsificación (CSRF) en formularios y acciones sensibles.
/// </summary>
app.UseAntiforgery();

/// <summary>
/// Mapea los archivos estáticos (CSS, JS, imágenes).
/// </summary>
app.MapStaticAssets();

app.MapGet("/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
}).AllowAnonymous();

/// <summary>
/// Mapea el componente raíz Razor <App> para renderizado interactivo del servidor.
/// </summary>
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

/// <summary>
/// Inicia la aplicación y comienza a escuchar solicitudes HTTP.
/// </summary>
app.Run();

using CineTracker;
using CineTracker.Components;
using CineTracker.Data;
using CineTracker.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Fuentes de configuración: appsettings.json → User Secrets → variables de entorno
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();

// Inyecta TmdbSettings como IOptions<TmdbSettings> en los servicios que lo necesiten
builder.Services.Configure<TmdbSettings>(
    builder.Configuration.GetSection("TmdbSettings"));

// DbContextFactory es obligatorio en Blazor Server: permite crear contextos por operación (thread-safe)
builder.Services.AddDbContextFactory<CineTrackerContext>(options =>
    options
        .UseNpgsql(
            builder.Configuration.GetConnectionString("CineTrackerDB"),
            npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3) // reintenta si Supabase está pausado
        )
        .UseSnakeCaseNamingConvention()); // convierte nombres a snake_case para PostgreSQL

// Servicios de la aplicación
builder.Services.AddHttpClient<TmdbService>(); // cliente HTTP tipado para la API de TMDB
builder.Services.AddScoped<WatchlistService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Autenticación por cookie: redirige a /login si no está autenticado
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true; // renueva la cookie si el usuario está activo
    });

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts(); // fuerza HTTPS en el navegador
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true); // maneja 404
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery(); // protección CSRF

app.MapStaticAssets();

app.MapGet("/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
}).AllowAnonymous();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

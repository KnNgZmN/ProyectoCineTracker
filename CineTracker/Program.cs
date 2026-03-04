using CineTracker;
using CineTracker.Components;
using CineTracker.Data;
using CineTracker.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();

builder.Services.Configure<TmdbSettings>(
    builder.Configuration.GetSection("TmdbSettings"));

// Base de datos: usar una fábrica de DbContext para evitar operaciones concurrentes en Blazor Server
builder.Services.AddDbContextFactory<CineTrackerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CineTrackerDB")));

// Servicios TMDB
builder.Services.AddHttpClient<TmdbService>();
builder.Services.AddScoped<WatchlistService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

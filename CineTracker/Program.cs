using CineTracker.Components;
using CineTracker.Data;
using Microsoft.EntityFrameworkCore;
using CineTracker.Services;

var builder = WebApplication.CreateBuilder(args);

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

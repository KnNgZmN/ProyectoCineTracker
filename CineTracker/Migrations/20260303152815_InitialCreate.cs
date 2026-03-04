using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineTracker.Migrations
{
    /// <summary>
    /// Migración inicial de la base de datos.
    /// Crea las tablas Movies y WatchlistItems con sus columnas y restricciones.
    /// </summary>
    public partial class InitialCreate : Migration
    {
        /// <summary>
        /// Método que se ejecuta al aplicar la migración.
        /// Crea las tablas y define las columnas, tipos y claves primarias.
        /// </summary>
        /// <param name="migrationBuilder">Constructor de migraciones de EF Core</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tabla Movies: información general de películas
            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    // Clave primaria autoincremental
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    // ID externo de TMDB
                    TmdbId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    // Título de la película, obligatorio y con longitud máxima de 200
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    // Sinopsis de la película, longitud máxima 2000
                    Overview = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    // Ruta del poster, longitud máxima 500
                    PosterPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    // Fecha de estreno
                    ReleaseDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    // Promedio de votos
                    VoteAverage = table.Column<double>(type: "float", nullable: false),
                    // Géneros de la película como texto
                    Genres = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    // Define la clave primaria
                    table.PrimaryKey("PK_Movies", x => x.Id);
                });

            // Tabla WatchlistItems: elementos de la lista de seguimiento del usuario
            migrationBuilder.CreateTable(
                name: "WatchlistItems",
                columns: table => new
                {
                    // Clave primaria autoincremental
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    // ID de la película en TMDB
                    TmdbId = table.Column<int>(type: "int", nullable: false),
                    // Título de la película, obligatorio
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    // Poster de la película, opcional longitud máxima 500
                    PosterPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    // Fecha de estreno
                    ReleaseDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    // Promedio de votos
                    VoteAverage = table.Column<double>(type: "float", nullable: false),
                    // Indica si la película es favorita del usuario
                    IsFavorite = table.Column<bool>(type: "bit", nullable: false),
                    // Indica si la película ya ha sido vista
                    IsWatched = table.Column<bool>(type: "bit", nullable: false),
                    // Fecha en que se agregó a la lista
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    // Define la clave primaria
                    table.PrimaryKey("PK_WatchlistItems", x => x.Id);
                });
        }

        /// <summary>
        /// Método que se ejecuta al revertir la migración.
        /// Elimina las tablas creadas por esta migración.
        /// </summary>
        /// <param name="migrationBuilder">Constructor de migraciones de EF Core</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Elimina la tabla Movies
            migrationBuilder.DropTable(
                name: "Movies");

            // Elimina la tabla WatchlistItems
            migrationBuilder.DropTable(
                name: "WatchlistItems");
        }
    }
}
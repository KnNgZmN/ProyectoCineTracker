using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioIdToWatchlist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Elimina registros huérfanos que no pertenecen a ningún usuario
            migrationBuilder.Sql("DELETE FROM WatchlistItems");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "WatchlistItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistItems_UsuarioId",
                table: "WatchlistItems",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_WatchlistItems_Usuarios_UsuarioId",
                table: "WatchlistItems",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WatchlistItems_Usuarios_UsuarioId",
                table: "WatchlistItems");

            migrationBuilder.DropIndex(
                name: "IX_WatchlistItems_UsuarioId",
                table: "WatchlistItems");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "WatchlistItems");
        }
    }
}

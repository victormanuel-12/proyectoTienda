using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proyectoTienda.Data.Migrations
{
    /// <inheritdoc />
    public partial class MLMigracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Etiqueta",
                table: "Contacto",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Puntuacion",
                table: "Contacto",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Etiqueta",
                table: "Contacto");

            migrationBuilder.DropColumn(
                name: "Puntuacion",
                table: "Contacto");
        }
    }
}

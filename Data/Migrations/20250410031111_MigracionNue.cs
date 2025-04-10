using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proyectoTienda.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigracionNue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Complemento",
                table: "Direcciones",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Complemento",
                table: "Direcciones");
        }
    }
}

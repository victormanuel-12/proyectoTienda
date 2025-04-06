using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proyectoTienda.Data.Migrations
{
    /// <inheritdoc />
    public partial class carrito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Precio",
                table: "Productos");

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioActual",
                table: "Productos",
                type: "decimal(10, 1)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioOriginal",
                table: "Productos",
                type: "decimal(10, 1)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "Pedidos",
                type: "decimal(10, 2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecioActual",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PrecioOriginal",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "Pedidos");

            migrationBuilder.AddColumn<decimal>(
                name: "Precio",
                table: "Productos",
                type: "decimal(10, 2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proyectoTienda.Data.Migrations
{
    /// <inheritdoc />
    public partial class carrit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemsCarrito",
                columns: table => new
                {
                    IDItem = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductoIDProducto = table.Column<int>(type: "INTEGER", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", nullable: true),
                    Precio = table.Column<decimal>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsCarrito", x => x.IDItem);
                    table.ForeignKey(
                        name: "FK_ItemsCarrito_Productos_ProductoIDProducto",
                        column: x => x.ProductoIDProducto,
                        principalTable: "Productos",
                        principalColumn: "IDProducto");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemsCarrito_ProductoIDProducto",
                table: "ItemsCarrito",
                column: "ProductoIDProducto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemsCarrito");
        }
    }
}

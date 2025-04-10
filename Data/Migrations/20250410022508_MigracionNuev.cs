using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proyectoTienda.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigracionNuev : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Direcciones_DireccionID",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_DireccionID",
                table: "Pedidos");

            migrationBuilder.AddColumn<int>(
                name: "IdDireccion",
                table: "Pedidos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_IdDireccion",
                table: "Pedidos",
                column: "IdDireccion");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Direcciones_IdDireccion",
                table: "Pedidos",
                column: "IdDireccion",
                principalTable: "Direcciones",
                principalColumn: "IdDireccion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Direcciones_IdDireccion",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_IdDireccion",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "IdDireccion",
                table: "Pedidos");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_DireccionID",
                table: "Pedidos",
                column: "DireccionID");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Direcciones_DireccionID",
                table: "Pedidos",
                column: "DireccionID",
                principalTable: "Direcciones",
                principalColumn: "IdDireccion",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

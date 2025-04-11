using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proyectoTienda.Data.Migrations
{
    /// <inheritdoc />
    public partial class Direccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Direcciones_Usuarios_IDCliente",
                table: "Direcciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Direcciones_IdDireccion",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Direcciones_IDCliente",
                table: "Direcciones");

            migrationBuilder.DropColumn(
                name: "DireccionID",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "IDCliente",
                table: "Direcciones");

            migrationBuilder.AlterColumn<int>(
                name: "IdDireccion",
                table: "Pedidos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Direcciones_IdDireccion",
                table: "Pedidos",
                column: "IdDireccion",
                principalTable: "Direcciones",
                principalColumn: "IdDireccion",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Direcciones_IdDireccion",
                table: "Pedidos");

            migrationBuilder.AlterColumn<int>(
                name: "IdDireccion",
                table: "Pedidos",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "DireccionID",
                table: "Pedidos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IDCliente",
                table: "Direcciones",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Direcciones_IDCliente",
                table: "Direcciones",
                column: "IDCliente");

            migrationBuilder.AddForeignKey(
                name: "FK_Direcciones_Usuarios_IDCliente",
                table: "Direcciones",
                column: "IDCliente",
                principalTable: "Usuarios",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Direcciones_IdDireccion",
                table: "Pedidos",
                column: "IdDireccion",
                principalTable: "Direcciones",
                principalColumn: "IdDireccion");
        }
    }
}

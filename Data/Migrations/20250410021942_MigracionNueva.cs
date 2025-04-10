using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proyectoTienda.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigracionNueva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Usuarios");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Pedidos",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "DireccionID",
                table: "Pedidos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TipoEntrega",
                table: "Pedidos",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Departamentos",
                columns: table => new
                {
                    DepartamentoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departamentos", x => x.DepartamentoId);
                });

            migrationBuilder.CreateTable(
                name: "Direcciones",
                columns: table => new
                {
                    IdDireccion = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IDCliente = table.Column<string>(type: "TEXT", nullable: false),
                    DireccionTexto = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Distrito = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Provincia = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Departamento = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Direcciones", x => x.IdDireccion);
                    table.ForeignKey(
                        name: "FK_Direcciones_Usuarios_IDCliente",
                        column: x => x.IDCliente,
                        principalTable: "Usuarios",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Provincias",
                columns: table => new
                {
                    ProvinciaId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DepartamentoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provincias", x => x.ProvinciaId);
                    table.ForeignKey(
                        name: "FK_Provincias_Departamentos_DepartamentoId",
                        column: x => x.DepartamentoId,
                        principalTable: "Departamentos",
                        principalColumn: "DepartamentoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Distritos",
                columns: table => new
                {
                    DistritoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ProvinciaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Distritos", x => x.DistritoId);
                    table.ForeignKey(
                        name: "FK_Distritos_Provincias_ProvinciaId",
                        column: x => x.ProvinciaId,
                        principalTable: "Provincias",
                        principalColumn: "ProvinciaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_DireccionID",
                table: "Pedidos",
                column: "DireccionID");

            migrationBuilder.CreateIndex(
                name: "IX_Direcciones_IDCliente",
                table: "Direcciones",
                column: "IDCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Distritos_ProvinciaId",
                table: "Distritos",
                column: "ProvinciaId");

            migrationBuilder.CreateIndex(
                name: "IX_Provincias_DepartamentoId",
                table: "Provincias",
                column: "DepartamentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Direcciones_DireccionID",
                table: "Pedidos",
                column: "DireccionID",
                principalTable: "Direcciones",
                principalColumn: "IdDireccion",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Direcciones_DireccionID",
                table: "Pedidos");

            migrationBuilder.DropTable(
                name: "Direcciones");

            migrationBuilder.DropTable(
                name: "Distritos");

            migrationBuilder.DropTable(
                name: "Provincias");

            migrationBuilder.DropTable(
                name: "Departamentos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_DireccionID",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "DireccionID",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "TipoEntrega",
                table: "Pedidos");

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "Pedidos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}

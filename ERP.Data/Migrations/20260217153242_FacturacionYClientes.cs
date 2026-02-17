using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class FacturacionYClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articulos_FamiliaArticulo_FamiliaId",
                table: "Articulos");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroDocumento",
                table: "Documentos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRecepcion",
                table: "Documentos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsContabilizado",
                table: "Documentos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MetodoPago",
                table: "Documentos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumeroAlbaran",
                table: "Documentos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "Documentos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FamiliaId1",
                table: "Articulos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProveedorHabitualId",
                table: "Articulos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StockMinimo",
                table: "Articulos",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "CierresCaja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Terminal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalVentasEfectivo = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalVentasTarjeta = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalIva = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ImporteRealEnCaja = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsProcesado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CierresCaja", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Familias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CodigoInterno = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsActiva = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Familias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosStock",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArticuloId = table.Column<int>(type: "int", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    StockResultante = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReferenciaDocumento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosStock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosStock_Articulos_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "Articulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articulos_FamiliaId1",
                table: "Articulos",
                column: "FamiliaId1");

            migrationBuilder.CreateIndex(
                name: "IX_Articulos_ProveedorHabitualId",
                table: "Articulos",
                column: "ProveedorHabitualId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_ArticuloId",
                table: "MovimientosStock",
                column: "ArticuloId");

            migrationBuilder.AddForeignKey(
                name: "FK_Articulos_FamiliaArticulo_FamiliaId",
                table: "Articulos",
                column: "FamiliaId",
                principalTable: "FamiliaArticulo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Articulos_Familias_FamiliaId1",
                table: "Articulos",
                column: "FamiliaId1",
                principalTable: "Familias",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Articulos_Proveedores_ProveedorHabitualId",
                table: "Articulos",
                column: "ProveedorHabitualId",
                principalTable: "Proveedores",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articulos_FamiliaArticulo_FamiliaId",
                table: "Articulos");

            migrationBuilder.DropForeignKey(
                name: "FK_Articulos_Familias_FamiliaId1",
                table: "Articulos");

            migrationBuilder.DropForeignKey(
                name: "FK_Articulos_Proveedores_ProveedorHabitualId",
                table: "Articulos");

            migrationBuilder.DropTable(
                name: "CierresCaja");

            migrationBuilder.DropTable(
                name: "Familias");

            migrationBuilder.DropTable(
                name: "MovimientosStock");

            migrationBuilder.DropIndex(
                name: "IX_Articulos_FamiliaId1",
                table: "Articulos");

            migrationBuilder.DropIndex(
                name: "IX_Articulos_ProveedorHabitualId",
                table: "Articulos");

            migrationBuilder.DropColumn(
                name: "FechaRecepcion",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "IsContabilizado",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "MetodoPago",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "NumeroAlbaran",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "FamiliaId1",
                table: "Articulos");

            migrationBuilder.DropColumn(
                name: "ProveedorHabitualId",
                table: "Articulos");

            migrationBuilder.DropColumn(
                name: "StockMinimo",
                table: "Articulos");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroDocumento",
                table: "Documentos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_Articulos_FamiliaArticulo_FamiliaId",
                table: "Articulos",
                column: "FamiliaId",
                principalTable: "FamiliaArticulo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

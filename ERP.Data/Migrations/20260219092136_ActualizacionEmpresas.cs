using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionEmpresas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articulos_FamiliaArticulo_FamiliaId",
                table: "Articulos");

            migrationBuilder.DropForeignKey(
                name: "FK_Articulos_Familias_FamiliaId1",
                table: "Articulos");

            migrationBuilder.DropTable(
                name: "FamiliaArticulo");

            migrationBuilder.DropIndex(
                name: "IX_Articulos_FamiliaId1",
                table: "Articulos");

            migrationBuilder.DropColumn(
                name: "FamiliaId1",
                table: "Articulos");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Proveedores",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Proveedores",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAlta",
                table: "Proveedores",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "MovimientosStock",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Familias",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaModificacion",
                table: "Familias",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Empresas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Base10",
                table: "CierresCaja",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Base21",
                table: "CierresCaja",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Base4",
                table: "CierresCaja",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "CierresCaja",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Iva10",
                table: "CierresCaja",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Iva21",
                table: "CierresCaja",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Iva4",
                table: "CierresCaja",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosStock_EmpresaId",
                table: "MovimientosStock",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CierresCaja_EmpresaId",
                table: "CierresCaja",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Articulos_Familias_FamiliaId",
                table: "Articulos",
                column: "FamiliaId",
                principalTable: "Familias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CierresCaja_Empresas_EmpresaId",
                table: "CierresCaja",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosStock_Empresas_EmpresaId",
                table: "MovimientosStock",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articulos_Familias_FamiliaId",
                table: "Articulos");

            migrationBuilder.DropForeignKey(
                name: "FK_CierresCaja_Empresas_EmpresaId",
                table: "CierresCaja");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosStock_Empresas_EmpresaId",
                table: "MovimientosStock");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosStock_EmpresaId",
                table: "MovimientosStock");

            migrationBuilder.DropIndex(
                name: "IX_CierresCaja_EmpresaId",
                table: "CierresCaja");

            migrationBuilder.DropColumn(
                name: "FechaAlta",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "MovimientosStock");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Familias");

            migrationBuilder.DropColumn(
                name: "UltimaModificacion",
                table: "Familias");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Base10",
                table: "CierresCaja");

            migrationBuilder.DropColumn(
                name: "Base21",
                table: "CierresCaja");

            migrationBuilder.DropColumn(
                name: "Base4",
                table: "CierresCaja");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "CierresCaja");

            migrationBuilder.DropColumn(
                name: "Iva10",
                table: "CierresCaja");

            migrationBuilder.DropColumn(
                name: "Iva21",
                table: "CierresCaja");

            migrationBuilder.DropColumn(
                name: "Iva4",
                table: "CierresCaja");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Proveedores",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Proveedores",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FamiliaId1",
                table: "Articulos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FamiliaArticulo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActiva = table.Column<bool>(type: "bit", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamiliaArticulo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articulos_FamiliaId1",
                table: "Articulos",
                column: "FamiliaId1");

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
        }
    }
}

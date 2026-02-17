using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionMaestrosERP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstaPagado",
                table: "Vencimientos");

            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "Clientes");

            migrationBuilder.RenameColumn(
                name: "Precio",
                table: "Articulos",
                newName: "PrecioVenta");

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Vencimientos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Vencimientos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CodigoPostal",
                table: "Empresas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorHex",
                table: "Empresas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Eslogan",
                table: "Empresas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAlta",
                table: "Empresas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "IvaDefecto",
                table: "Empresas",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "LogoBase64",
                table: "Empresas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Poblacion",
                table: "Empresas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provincia",
                table: "Empresas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistroMercantil",
                table: "Empresas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Empresas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaModificacion",
                table: "Empresas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Web",
                table: "Empresas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cargo",
                table: "Empleados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Departamento",
                table: "Empleados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "Empleados",
                type: "nvarchar(34)",
                maxLength: 34,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Empleados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoCliente",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CodigoPostal",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DescuentoFijo",
                table: "Clientes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DiaPagoHabitual",
                table: "Clientes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAlta",
                table: "Clientes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FormaPago",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBloqueado",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MotivoBloqueo",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreComercial",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Poblacion",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provincia",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazonSocial",
                table: "Clientes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "TieneRecargoEquivalencia",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "FamiliaId",
                table: "Articulos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajeIva",
                table: "Articulos",
                type: "decimal(18,2)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioCompra",
                table: "Articulos",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "FamiliaArticulo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActiva = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamiliaArticulo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vencimientos_EmpresaId",
                table: "Vencimientos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Articulos_FamiliaId",
                table: "Articulos",
                column: "FamiliaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Articulos_FamiliaArticulo_FamiliaId",
                table: "Articulos",
                column: "FamiliaId",
                principalTable: "FamiliaArticulo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vencimientos_Empresas_EmpresaId",
                table: "Vencimientos",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articulos_FamiliaArticulo_FamiliaId",
                table: "Articulos");

            migrationBuilder.DropForeignKey(
                name: "FK_Vencimientos_Empresas_EmpresaId",
                table: "Vencimientos");

            migrationBuilder.DropTable(
                name: "FamiliaArticulo");

            migrationBuilder.DropIndex(
                name: "IX_Vencimientos_EmpresaId",
                table: "Vencimientos");

            migrationBuilder.DropIndex(
                name: "IX_Articulos_FamiliaId",
                table: "Articulos");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Vencimientos");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Vencimientos");

            migrationBuilder.DropColumn(
                name: "CodigoPostal",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "ColorHex",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Eslogan",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "FechaAlta",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "IvaDefecto",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "LogoBase64",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Poblacion",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Provincia",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "RegistroMercantil",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "UltimaModificacion",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Web",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Cargo",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "Departamento",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "CodigoCliente",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "CodigoPostal",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "DescuentoFijo",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "DiaPagoHabitual",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "FechaAlta",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "FormaPago",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "IsBloqueado",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "MotivoBloqueo",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "NombreComercial",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Poblacion",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Provincia",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "RazonSocial",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TieneRecargoEquivalencia",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "FamiliaId",
                table: "Articulos");

            migrationBuilder.DropColumn(
                name: "PorcentajeIva",
                table: "Articulos");

            migrationBuilder.DropColumn(
                name: "PrecioCompra",
                table: "Articulos");

            migrationBuilder.RenameColumn(
                name: "PrecioVenta",
                table: "Articulos",
                newName: "Precio");

            migrationBuilder.AddColumn<bool>(
                name: "EstaPagado",
                table: "Vencimientos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}

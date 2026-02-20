using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionModelosERP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NombreArchivoPdf",
                table: "Empleados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutaDocumentoPdf",
                table: "Empleados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VacacionesDisfrutadas",
                table: "Empleados",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VacacionesTotales",
                table: "Empleados",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NotasInternas",
                table: "Documentos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioNombre",
                table: "Documentos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PorcentajeIva",
                table: "DocumentoLineas",
                type: "decimal(5,2)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "ArticuloId",
                table: "DocumentoLineas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "CategoriaNombre",
                table: "DocumentoLineas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataCategoriasJson",
                table: "CierresCaja",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataUsuariosJson",
                table: "CierresCaja",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StockReservado",
                table: "Articulos",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NombreArchivoPdf",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "RutaDocumentoPdf",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "VacacionesDisfrutadas",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "VacacionesTotales",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "NotasInternas",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "UsuarioNombre",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "CategoriaNombre",
                table: "DocumentoLineas");

            migrationBuilder.DropColumn(
                name: "DataCategoriasJson",
                table: "CierresCaja");

            migrationBuilder.DropColumn(
                name: "DataUsuariosJson",
                table: "CierresCaja");

            migrationBuilder.DropColumn(
                name: "StockReservado",
                table: "Articulos");

            migrationBuilder.AlterColumn<double>(
                name: "PorcentajeIva",
                table: "DocumentoLineas",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<int>(
                name: "ArticuloId",
                table: "DocumentoLineas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}

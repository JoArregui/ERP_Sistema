using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class IntegridadEstrictaERP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ControlesHorarios_Empleados_EmpleadoId",
                table: "ControlesHorarios");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosStock_Articulos_ArticuloId",
                table: "MovimientosStock");

            migrationBuilder.DropForeignKey(
                name: "FK_Nominas_Empleados_EmpleadoId",
                table: "Nominas");

            migrationBuilder.AddForeignKey(
                name: "FK_ControlesHorarios_Empleados_EmpleadoId",
                table: "ControlesHorarios",
                column: "EmpleadoId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosStock_Articulos_ArticuloId",
                table: "MovimientosStock",
                column: "ArticuloId",
                principalTable: "Articulos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Nominas_Empleados_EmpleadoId",
                table: "Nominas",
                column: "EmpleadoId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ControlesHorarios_Empleados_EmpleadoId",
                table: "ControlesHorarios");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosStock_Articulos_ArticuloId",
                table: "MovimientosStock");

            migrationBuilder.DropForeignKey(
                name: "FK_Nominas_Empleados_EmpleadoId",
                table: "Nominas");

            migrationBuilder.AddForeignKey(
                name: "FK_ControlesHorarios_Empleados_EmpleadoId",
                table: "ControlesHorarios",
                column: "EmpleadoId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosStock_Articulos_ArticuloId",
                table: "MovimientosStock",
                column: "ArticuloId",
                principalTable: "Articulos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Nominas_Empleados_EmpleadoId",
                table: "Nominas",
                column: "EmpleadoId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

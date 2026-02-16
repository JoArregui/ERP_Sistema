using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AjusteModeladoFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentoLineas_Articulos_ArticuloId",
                table: "DocumentoLineas");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentoLineas_Articulos_ArticuloId",
                table: "DocumentoLineas",
                column: "ArticuloId",
                principalTable: "Articulos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentoLineas_Articulos_ArticuloId",
                table: "DocumentoLineas");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentoLineas_Articulos_ArticuloId",
                table: "DocumentoLineas",
                column: "ArticuloId",
                principalTable: "Articulos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

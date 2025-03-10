using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiGym.Migrations
{
    /// <inheritdoc />
    public partial class CambioDeDatos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Membresias_Usuario_usuarioId",
                table: "Membresias");

            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Usuario_usuarioId",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_usuarioId",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Membresias_usuarioId",
                table: "Membresias");

            migrationBuilder.AlterColumn<string>(
                name: "usuarioId",
                table: "Pagos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "usuarioId",
                table: "Membresias",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "usuarioId1",
                table: "Membresias",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Membresias_usuarioId1",
                table: "Membresias",
                column: "usuarioId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Membresias_Usuario_usuarioId1",
                table: "Membresias",
                column: "usuarioId1",
                principalTable: "Usuario",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Membresias_Usuario_usuarioId1",
                table: "Membresias");

            migrationBuilder.DropIndex(
                name: "IX_Membresias_usuarioId1",
                table: "Membresias");

            migrationBuilder.DropColumn(
                name: "usuarioId1",
                table: "Membresias");

            migrationBuilder.AlterColumn<Guid>(
                name: "usuarioId",
                table: "Pagos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "usuarioId",
                table: "Membresias",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_usuarioId",
                table: "Pagos",
                column: "usuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Membresias_usuarioId",
                table: "Membresias",
                column: "usuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Membresias_Usuario_usuarioId",
                table: "Membresias",
                column: "usuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Usuario_usuarioId",
                table: "Pagos",
                column: "usuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

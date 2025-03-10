using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiGym.Migrations
{
    /// <inheritdoc />
    public partial class EliminandoPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Usuario",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

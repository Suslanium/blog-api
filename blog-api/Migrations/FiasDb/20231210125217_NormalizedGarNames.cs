using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace blog_api.Migrations.FiasDb
{
    /// <inheritdoc />
    public partial class NormalizedGarNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedHouseNum",
                schema: "fias",
                table: "as_houses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                schema: "fias",
                table: "as_addr_obj",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NormalizedHouseNum",
                schema: "fias",
                table: "as_houses");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                schema: "fias",
                table: "as_addr_obj");
        }
    }
}

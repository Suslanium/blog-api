using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace blog_api.Migrations
{
    /// <inheritdoc />
    public partial class TokenValidationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TokenValidation",
                table: "TokenValidation");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "TokenValidation");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "TokenValidation",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TokenValidation",
                table: "TokenValidation",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TokenValidation",
                table: "TokenValidation");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TokenValidation");

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "TokenValidation",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TokenValidation",
                table: "TokenValidation",
                column: "UserEmail");
        }
    }
}

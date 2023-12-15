using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace blog_api.Migrations
{
    /// <inheritdoc />
    public partial class CommentTopLevelParent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TopLevelParentCommentId",
                table: "Comments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TopLevelParentCommentId",
                table: "Comments",
                column: "TopLevelParentCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_TopLevelParentCommentId",
                table: "Comments",
                column: "TopLevelParentCommentId",
                principalTable: "Comments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_TopLevelParentCommentId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_TopLevelParentCommentId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "TopLevelParentCommentId",
                table: "Comments");
        }
    }
}

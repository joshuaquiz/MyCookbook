using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCookbook.API.Migrations
{
    /// <inheritdoc />
    public partial class AddHtml : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Html",
                table: "RecipeUrls",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Html",
                table: "RecipeUrls");
        }
    }
}

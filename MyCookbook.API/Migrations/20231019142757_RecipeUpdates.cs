using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCookbook.API.Migrations
{
    /// <inheritdoc />
    public partial class RecipeUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>("Image", "Ingredients", "TEXT", nullable: true);

            migrationBuilder.AddColumn<string>("ParserVersion", "RecipeUrls", "INTEGER", nullable: false);
            migrationBuilder.AddColumn<string>("Host", "RecipeUrls", "TEXT", nullable: false);

            migrationBuilder.AddColumn<string>("BackgroundImage", "Authors", "TEXT", nullable: true);

            migrationBuilder.AddColumn<string>("RecipeUrlGuid", "Recipes", "TEXT", nullable: true);
            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_RecipeUrls_RecipeUrlGuid",
                table: "Recipes",
                column: "RecipeUrlGuid",
                principalTable: "RecipeUrls",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.CreateIndex(
                name: "IX_Recipes_RecipeUrlGuid",
                table: "Recipes",
                column: "RecipeUrlGuid");

            migrationBuilder.RenameTable("IngredientRecipe", newName: "RecipeIngredients");
            migrationBuilder.DropPrimaryKey("PK_IngredientRecipe", "RecipeIngredients");
            migrationBuilder.AddColumn<Guid>("Guid", "RecipeIngredients", "TEXT", nullable: false);
            migrationBuilder.AddColumn<string>("Quantity", "RecipeIngredients", "TEXT", nullable: false);
            migrationBuilder.AddColumn<int>("MeasurementUnit", "RecipeIngredients", "INTEGER", nullable: false);
            migrationBuilder.AddColumn<string>("Notes", "RecipeIngredients", "TEXT", nullable: false);
            migrationBuilder.AddPrimaryKey("PK_RecipeIngredients", "RecipeIngredients", "Guid");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_IngredientGuid",
                table: "RecipeIngredients",
                column: "IngredientGuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_RecipeIngredients_IngredientGuid", "RecipeIngredients");

            migrationBuilder.DropColumn("Image", "Ingredients");

            migrationBuilder.DropColumn("ParserVersion", "RecipeUrls");
            migrationBuilder.DropColumn("Host", "RecipeUrls");

            migrationBuilder.DropColumn("BackgroundImage", "Authors");

            migrationBuilder.DropForeignKey("FK_Recipes_RecipeUrls_RecipeUrlGuid", "Recipes");
            migrationBuilder.DropColumn("RecipeUrlGuid", "Recipes");
            migrationBuilder.DropIndex("IX_Recipes_RecipeUrlGuid", "Recipes");

            migrationBuilder.DropPrimaryKey("PK_RecipeIngredients", "RecipeIngredients");
            migrationBuilder.DropColumn("Guid", "RecipeIngredients");
            migrationBuilder.DropColumn("Quantity", "RecipeIngredients");
            migrationBuilder.DropColumn("MeasurementUnit", "RecipeIngredients");
            migrationBuilder.DropColumn("Notes", "RecipeIngredients");
            migrationBuilder.AddPrimaryKey("PK_IngredientRecipe", "RecipeIngredients", ["IngredientsGuid", "RecipesGuid"]);
            migrationBuilder.RenameTable("RecipeIngredients", newName: "IngredientRecipe");
        }
    }
}

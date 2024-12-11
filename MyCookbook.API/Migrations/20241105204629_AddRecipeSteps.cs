using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCookbook.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeSteps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeIngredients");

            migrationBuilder.CreateTable(
                name: "RecipeSteps",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    StepNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    RecipeStepType = table.Column<int>(type: "INTEGER", nullable: false),
                    Instructions = table.Column<string>(type: "TEXT", nullable: true),
                    RecipeGuid = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeSteps", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_RecipeSteps_Recipes_RecipeGuid",
                        column: x => x.RecipeGuid,
                        principalTable: "Recipes",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeStepIngredients",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Quantity = table.Column<string>(type: "TEXT", nullable: false),
                    Measurement = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    IngredientGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecipeStepGuid = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeStepIngredients", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_RecipeStepIngredients_Ingredients_IngredientGuid",
                        column: x => x.IngredientGuid,
                        principalTable: "Ingredients",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeStepIngredients_RecipeSteps_RecipeStepGuid",
                        column: x => x.RecipeStepGuid,
                        principalTable: "RecipeSteps",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeStepIngredients_IngredientGuid",
                table: "RecipeStepIngredients",
                column: "IngredientGuid");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeStepIngredients_RecipeStepGuid",
                table: "RecipeStepIngredients",
                column: "RecipeStepGuid");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeSteps_RecipeGuid",
                table: "RecipeSteps",
                column: "RecipeGuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeStepIngredients");

            migrationBuilder.DropTable(
                name: "RecipeSteps");

            migrationBuilder.CreateTable(
                name: "RecipeIngredients",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    IngredientGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecipeGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Measurement = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Quantity = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeIngredients", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Ingredients_IngredientGuid",
                        column: x => x.IngredientGuid,
                        principalTable: "Ingredients",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Recipes_RecipeGuid",
                        column: x => x.RecipeGuid,
                        principalTable: "Recipes",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_IngredientGuid",
                table: "RecipeIngredients",
                column: "IngredientGuid");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_RecipeGuid",
                table: "RecipeIngredients",
                column: "RecipeGuid");
        }
    }
}

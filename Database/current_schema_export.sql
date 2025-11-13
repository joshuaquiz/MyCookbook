CREATE TABLE IF NOT EXISTS "__EFMigrationsLock" (

    "Id" INTEGER NOT NULL CONSTRAINT "PK___EFMigrationsLock" PRIMARY KEY,

    "Timestamp" TEXT NOT NULL

);
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (

    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,

    "ProductVersion" TEXT NOT NULL

);
CREATE TABLE IF NOT EXISTS "Ingredients" (

    "Guid" TEXT NOT NULL CONSTRAINT "PK_Ingredients" PRIMARY KEY,

    "Name" TEXT NOT NULL

, "Image" TEXT NULL);
CREATE TABLE IF NOT EXISTS "RecipeUrls" (

    "Guid" TEXT NOT NULL CONSTRAINT "PK_RecipeUrls" PRIMARY KEY,

    "ProcessingStatus" INTEGER NOT NULL,

    "Uri" TEXT NOT NULL,

    "StatusCode" INTEGER NULL,

    "LdJson" TEXT NULL,

    "Exception" TEXT NULL,

    "CompletedAt" TEXT NULL

, "ParserVersion" INTEGER NOT NULL, "Host" TEXT NOT NULL, "Html" TEXT NULL);
CREATE TABLE IF NOT EXISTS "Users" (

    "Guid" TEXT NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY,

    "Name" TEXT NOT NULL,

    "Image" TEXT NULL

);
CREATE TABLE IF NOT EXISTS "Authors" (

    "Guid" TEXT NOT NULL CONSTRAINT "PK_Authors" PRIMARY KEY,

    "Name" TEXT NOT NULL,

    "Image" TEXT NULL,

    "UserGuid" TEXT NULL, "BackgroundImage" TEXT NULL,

    CONSTRAINT "FK_Authors_Users_UserGuid" FOREIGN KEY ("UserGuid") REFERENCES "Users" ("Guid")

);
CREATE INDEX "IX_Authors_UserGuid" ON "Authors" ("UserGuid");
CREATE TABLE IF NOT EXISTS "Recipes" (

    "Guid" TEXT NOT NULL CONSTRAINT "PK_Recipes" PRIMARY KEY,

    "AuthorGuid" TEXT NOT NULL,

    "Image" TEXT NULL,

    "Name" TEXT NOT NULL,

    "RecipeUrlGuid" TEXT NOT NULL,

    "TotalTime" TEXT NOT NULL, "Description" TEXT NULL,

    CONSTRAINT "FK_Recipes_Authors_AuthorGuid" FOREIGN KEY ("AuthorGuid") REFERENCES "Authors" ("Guid") ON DELETE CASCADE,

    CONSTRAINT "FK_Recipes_RecipeUrls_RecipeUrlGuid" FOREIGN KEY ("RecipeUrlGuid") REFERENCES "RecipeUrls" ("Guid") ON DELETE CASCADE

);
CREATE INDEX "IX_Recipes_AuthorGuid" ON "Recipes" ("AuthorGuid");
CREATE INDEX "IX_Recipes_RecipeUrlGuid" ON "Recipes" ("RecipeUrlGuid");
CREATE TABLE IF NOT EXISTS "RecipeSteps" (

    "Guid" TEXT NOT NULL CONSTRAINT "PK_RecipeSteps" PRIMARY KEY,

    "StepNumber" INTEGER NOT NULL,

    "RecipeStepType" INTEGER NOT NULL,

    "Instructions" TEXT NULL,

    "RecipeGuid" TEXT NOT NULL,

    CONSTRAINT "FK_RecipeSteps_Recipes_RecipeGuid" FOREIGN KEY ("RecipeGuid") REFERENCES "Recipes" ("Guid") ON DELETE CASCADE

);
CREATE TABLE IF NOT EXISTS "RecipeStepIngredients" (

    "Guid" TEXT NOT NULL CONSTRAINT "PK_RecipeStepIngredients" PRIMARY KEY,

    "Quantity" TEXT NOT NULL,

    "Measurement" INTEGER NOT NULL,

    "Notes" TEXT NULL,

    "IngredientGuid" TEXT NOT NULL,

    "RecipeStepGuid" TEXT NOT NULL,

    CONSTRAINT "FK_RecipeStepIngredients_Ingredients_IngredientGuid" FOREIGN KEY ("IngredientGuid") REFERENCES "Ingredients" ("Guid") ON DELETE CASCADE,

    CONSTRAINT "FK_RecipeStepIngredients_RecipeSteps_RecipeStepGuid" FOREIGN KEY ("RecipeStepGuid") REFERENCES "RecipeSteps" ("Guid") ON DELETE CASCADE

);
CREATE INDEX "IX_RecipeStepIngredients_IngredientGuid" ON "RecipeStepIngredients" ("IngredientGuid");
CREATE INDEX "IX_RecipeStepIngredients_RecipeStepGuid" ON "RecipeStepIngredients" ("RecipeStepGuid");
CREATE INDEX "IX_RecipeSteps_RecipeGuid" ON "RecipeSteps" ("RecipeGuid");

-- Script to convert TEXT columns to UUID type in PostgreSQL
-- This fixes the "Reading as 'System.Guid' is not supported for fields having DataTypeName 'text'" error

-- IMPORTANT: Run this script on the PostgreSQL database
-- Make sure to backup your database before running this script!

BEGIN;

-- Step 1: Drop all foreign key constraints (both old and new naming conventions)
ALTER TABLE "Recipes" DROP CONSTRAINT IF EXISTS "Recipes_recipe_url_id_fkey";
ALTER TABLE "Recipes" DROP CONSTRAINT IF EXISTS "Recipes_author_id_fkey";
ALTER TABLE "Recipes" DROP CONSTRAINT IF EXISTS "FK_Recipes_Authors_author_id";
ALTER TABLE "Recipes" DROP CONSTRAINT IF EXISTS "FK_Recipes_RawDataSources_recipe_url_id";
ALTER TABLE "RecipeSteps" DROP CONSTRAINT IF EXISTS "RecipeSteps_recipe_id_fkey";
ALTER TABLE "RecipeSteps" DROP CONSTRAINT IF EXISTS "FK_RecipeSteps_Recipes_recipe_id";
ALTER TABLE "RecipeStepIngredients" DROP CONSTRAINT IF EXISTS "RecipeStepIngredients_recipe_step_id_fkey";
ALTER TABLE "RecipeStepIngredients" DROP CONSTRAINT IF EXISTS "RecipeStepIngredients_ingredient_id_fkey";
ALTER TABLE "RecipeStepIngredients" DROP CONSTRAINT IF EXISTS "FK_RecipeStepIngredients_RecipeSteps_recipe_step_id";
ALTER TABLE "RecipeStepIngredients" DROP CONSTRAINT IF EXISTS "FK_RecipeStepIngredients_Ingredients_ingredient_id";
ALTER TABLE "EntityImages" DROP CONSTRAINT IF EXISTS "EntityImages_image_id_fkey";
ALTER TABLE "EntityImages" DROP CONSTRAINT IF EXISTS "FK_EntityImages_Images_image_id";
ALTER TABLE "RecipeTags" DROP CONSTRAINT IF EXISTS "RecipeTags_recipe_id_fkey";
ALTER TABLE "RecipeTags" DROP CONSTRAINT IF EXISTS "RecipeTags_tag_id_fkey";
ALTER TABLE "RecipeTags" DROP CONSTRAINT IF EXISTS "FK_RecipeTags_Recipes_recipe_id";
ALTER TABLE "RecipeTags" DROP CONSTRAINT IF EXISTS "FK_RecipeTags_Tags_tag_id";
ALTER TABLE "RecipeCategories" DROP CONSTRAINT IF EXISTS "RecipeCategories_recipe_id_fkey";
ALTER TABLE "RecipeCategories" DROP CONSTRAINT IF EXISTS "RecipeCategories_category_id_fkey";
ALTER TABLE "RecipeCategories" DROP CONSTRAINT IF EXISTS "FK_RecipeCategories_Recipes_recipe_id";
ALTER TABLE "RecipeCategories" DROP CONSTRAINT IF EXISTS "FK_RecipeCategories_Categories_category_id";
ALTER TABLE "RecipeHearts" DROP CONSTRAINT IF EXISTS "RecipeHearts_author_id_fkey";
ALTER TABLE "RecipeHearts" DROP CONSTRAINT IF EXISTS "RecipeHearts_recipe_id_fkey";
ALTER TABLE "RecipeHearts" DROP CONSTRAINT IF EXISTS "FK_RecipeHearts_Authors_author_id";
ALTER TABLE "RecipeHearts" DROP CONSTRAINT IF EXISTS "FK_RecipeHearts_Recipes_recipe_id";
ALTER TABLE "AuthorLinks" DROP CONSTRAINT IF EXISTS "AuthorLinks_author_id_fkey";
ALTER TABLE "AuthorLinks" DROP CONSTRAINT IF EXISTS "FK_AuthorLinks_Authors_author_id";
ALTER TABLE "UserCalendars" DROP CONSTRAINT IF EXISTS "UserCalendars_author_id_fkey";
ALTER TABLE "UserCalendars" DROP CONSTRAINT IF EXISTS "UserCalendars_recipe_id_fkey";
ALTER TABLE "UserCalendars" DROP CONSTRAINT IF EXISTS "FK_UserCalendars_Authors_author_id";
ALTER TABLE "UserCalendars" DROP CONSTRAINT IF EXISTS "FK_UserCalendars_Recipes_recipe_id";
ALTER TABLE "ShoppingListItems" DROP CONSTRAINT IF EXISTS "ShoppingListItems_author_id_fkey";
ALTER TABLE "ShoppingListItems" DROP CONSTRAINT IF EXISTS "ShoppingListItems_ingredient_id_fkey";
ALTER TABLE "ShoppingListItems" DROP CONSTRAINT IF EXISTS "ShoppingListItems_recipe_step_id_fkey";
ALTER TABLE "ShoppingListItems" DROP CONSTRAINT IF EXISTS "FK_ShoppingListItems_Authors_author_id";
ALTER TABLE "ShoppingListItems" DROP CONSTRAINT IF EXISTS "FK_ShoppingListItems_Ingredients_ingredient_id";

-- Step 2: Convert columns to UUID type

-- 1. RawDataSources table
ALTER TABLE "RawDataSources"
    ALTER COLUMN source_id TYPE uuid USING source_id::uuid,
    ALTER COLUMN same_as TYPE uuid USING same_as::uuid;

-- 2. Authors table
ALTER TABLE "Authors"
    ALTER COLUMN author_id TYPE uuid USING author_id::uuid;

-- 3. Recipes table
ALTER TABLE "Recipes"
    ALTER COLUMN recipe_id TYPE uuid USING recipe_id::uuid,
    ALTER COLUMN recipe_url_id TYPE uuid USING recipe_url_id::uuid,
    ALTER COLUMN author_id TYPE uuid USING author_id::uuid,
    ALTER COLUMN original_recipe_id TYPE uuid USING original_recipe_id::uuid;

-- 4. RecipeSteps table
ALTER TABLE "RecipeSteps"
    ALTER COLUMN step_id TYPE uuid USING step_id::uuid,
    ALTER COLUMN recipe_id TYPE uuid USING recipe_id::uuid;

-- 5. Ingredients table
ALTER TABLE "Ingredients" 
    ALTER COLUMN ingredient_id TYPE uuid USING ingredient_id::uuid;

-- 6. RecipeStepIngredients table
ALTER TABLE "RecipeStepIngredients" 
    ALTER COLUMN step_ingredient_id TYPE uuid USING step_ingredient_id::uuid,
    ALTER COLUMN recipe_step_id TYPE uuid USING recipe_step_id::uuid,
    ALTER COLUMN ingredient_id TYPE uuid USING ingredient_id::uuid;

-- 7. Images table
ALTER TABLE "Images" 
    ALTER COLUMN image_id TYPE uuid USING image_id::uuid;

-- 8. EntityImages table
ALTER TABLE "EntityImages" 
    ALTER COLUMN entity_image_id TYPE uuid USING entity_image_id::uuid,
    ALTER COLUMN image_id TYPE uuid USING image_id::uuid,
    ALTER COLUMN entity_id TYPE uuid USING entity_id::uuid;

-- 9. AuthorLinks table
ALTER TABLE "AuthorLinks"
    ALTER COLUMN link_id TYPE uuid USING link_id::uuid,
    ALTER COLUMN author_id TYPE uuid USING author_id::uuid;

-- 10. Tags table
ALTER TABLE "Tags"
    ALTER COLUMN tag_id TYPE uuid USING tag_id::uuid;

-- 10. RecipeTags table (composite key)
ALTER TABLE "RecipeTags" 
    ALTER COLUMN recipe_id TYPE uuid USING recipe_id::uuid,
    ALTER COLUMN tag_id TYPE uuid USING tag_id::uuid;

-- 11. Categories table
ALTER TABLE "Categories" 
    ALTER COLUMN category_id TYPE uuid USING category_id::uuid;

-- 12. RecipeCategories table (composite key)
ALTER TABLE "RecipeCategories" 
    ALTER COLUMN recipe_id TYPE uuid USING recipe_id::uuid,
    ALTER COLUMN category_id TYPE uuid USING category_id::uuid;

-- 13. RecipeHearts table (composite key)
ALTER TABLE "RecipeHearts" 
    ALTER COLUMN author_id TYPE uuid USING author_id::uuid,
    ALTER COLUMN recipe_id TYPE uuid USING recipe_id::uuid;

-- 14. Popularity table
ALTER TABLE "Popularity"
    ALTER COLUMN popularity_id TYPE uuid USING popularity_id::uuid,
    ALTER COLUMN entity_id TYPE uuid USING entity_id::uuid;

-- 15. UserCalendars table
ALTER TABLE "UserCalendars"
    ALTER COLUMN id TYPE uuid USING id::uuid,
    ALTER COLUMN author_id TYPE uuid USING author_id::uuid,
    ALTER COLUMN recipe_id TYPE uuid USING recipe_id::uuid;

-- 16. ShoppingListItems table
ALTER TABLE "ShoppingListItems"
    ALTER COLUMN shopping_list_item_id TYPE uuid USING shopping_list_item_id::uuid,
    ALTER COLUMN author_id TYPE uuid USING author_id::uuid,
    ALTER COLUMN ingredient_id TYPE uuid USING ingredient_id::uuid,
    ALTER COLUMN recipe_step_id TYPE uuid USING recipe_step_id::uuid;

-- Step 3: Recreate foreign key constraints
ALTER TABLE "Recipes"
    ADD CONSTRAINT "FK_Recipes_RawDataSources_recipe_url_id"
    FOREIGN KEY (recipe_url_id) REFERENCES "RawDataSources"(source_id);

ALTER TABLE "Recipes"
    ADD CONSTRAINT "FK_Recipes_Authors_author_id"
    FOREIGN KEY (author_id) REFERENCES "Authors"(author_id);

ALTER TABLE "RecipeSteps"
    ADD CONSTRAINT "FK_RecipeSteps_Recipes_recipe_id"
    FOREIGN KEY (recipe_id) REFERENCES "Recipes"(recipe_id);

ALTER TABLE "RecipeStepIngredients"
    ADD CONSTRAINT "FK_RecipeStepIngredients_RecipeSteps_recipe_step_id"
    FOREIGN KEY (recipe_step_id) REFERENCES "RecipeSteps"(step_id);

ALTER TABLE "RecipeStepIngredients"
    ADD CONSTRAINT "FK_RecipeStepIngredients_Ingredients_ingredient_id"
    FOREIGN KEY (ingredient_id) REFERENCES "Ingredients"(ingredient_id);

ALTER TABLE "EntityImages"
    ADD CONSTRAINT "FK_EntityImages_Images_image_id"
    FOREIGN KEY (image_id) REFERENCES "Images"(image_id);

ALTER TABLE "RecipeTags"
    ADD CONSTRAINT "FK_RecipeTags_Recipes_recipe_id"
    FOREIGN KEY (recipe_id) REFERENCES "Recipes"(recipe_id);

ALTER TABLE "RecipeTags"
    ADD CONSTRAINT "FK_RecipeTags_Tags_tag_id"
    FOREIGN KEY (tag_id) REFERENCES "Tags"(tag_id);

ALTER TABLE "RecipeCategories"
    ADD CONSTRAINT "FK_RecipeCategories_Recipes_recipe_id"
    FOREIGN KEY (recipe_id) REFERENCES "Recipes"(recipe_id);

ALTER TABLE "RecipeCategories"
    ADD CONSTRAINT "FK_RecipeCategories_Categories_category_id"
    FOREIGN KEY (category_id) REFERENCES "Categories"(category_id);

ALTER TABLE "RecipeHearts"
    ADD CONSTRAINT "FK_RecipeHearts_Authors_author_id"
    FOREIGN KEY (author_id) REFERENCES "Authors"(author_id);

ALTER TABLE "RecipeHearts"
    ADD CONSTRAINT "FK_RecipeHearts_Recipes_recipe_id"
    FOREIGN KEY (recipe_id) REFERENCES "Recipes"(recipe_id);

ALTER TABLE "AuthorLinks"
    ADD CONSTRAINT "FK_AuthorLinks_Authors_author_id"
    FOREIGN KEY (author_id) REFERENCES "Authors"(author_id);

ALTER TABLE "UserCalendars"
    ADD CONSTRAINT "FK_UserCalendars_Authors_author_id"
    FOREIGN KEY (author_id) REFERENCES "Authors"(author_id);

ALTER TABLE "UserCalendars"
    ADD CONSTRAINT "FK_UserCalendars_Recipes_recipe_id"
    FOREIGN KEY (recipe_id) REFERENCES "Recipes"(recipe_id);

ALTER TABLE "ShoppingListItems"
    ADD CONSTRAINT "FK_ShoppingListItems_Authors_author_id"
    FOREIGN KEY (author_id) REFERENCES "Authors"(author_id);

ALTER TABLE "ShoppingListItems"
    ADD CONSTRAINT "FK_ShoppingListItems_Ingredients_ingredient_id"
    FOREIGN KEY (ingredient_id) REFERENCES "Ingredients"(ingredient_id);

COMMIT;

-- Verify the changes
SELECT
    table_name,
    column_name,
    data_type
FROM information_schema.columns
WHERE table_schema = 'public'
    AND data_type = 'uuid'
ORDER BY table_name, column_name;


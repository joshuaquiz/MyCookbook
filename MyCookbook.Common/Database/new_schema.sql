CREATE TABLE "RawDataSources" (
    source_id TEXT PRIMARY KEY NOT NULL, -- GUID
    same_as TEXT,
    url TEXT NOT NULL UNIQUE,
    url_host TEXT,
    processing_status INTEGER NOT NULL DEFAULT 0,
    page_type INTEGER NOT NULL DEFAULT 0,
    parser_version INTEGER NOT NULL DEFAULT 0,
    ld_json_data TEXT,
    raw_html TEXT,
    processed_datetime TIMESTAMP,
    error TEXT
);

-- 1. Authors Table: Manages public profiles and user authentication
CREATE TABLE "Authors" (
    author_id TEXT PRIMARY KEY NOT NULL, -- GUID
    name TEXT NOT NULL,
    location TEXT,
    bio TEXT,
    is_visible BOOLEAN NOT NULL DEFAULT FALSE, -- FALSE: default for users, TRUE: visible authors
    author_type INTEGER NOT NULL DEFAULT 0,
    -- User authentication fields
    email TEXT UNIQUE,
    password_hash TEXT NULL,
    email_verified BOOLEAN NOT NULL DEFAULT FALSE,
    provider_user_id TEXT,
    auth_provider TEXT NOT NULL DEFAULT 'local',
    cognito_sub TEXT UNIQUE,
    last_login_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 3. Recipes Table: Primary entity for a recipe
CREATE TABLE "Recipes" (
    recipe_id TEXT PRIMARY KEY NOT NULL, -- GUID
    recipe_url_id TEXT UNIQUE, -- GUID of the RawDataSource it came from
    author_id TEXT NOT NULL, -- GUID of the Author (could be a user or a public source)
    title TEXT NOT NULL,
    type TEXT NOT NULL,
    description TEXT,
    prep_time INTEGER,
    cook_time INTEGER,
    servings TEXT,
    rating REAL,
    is_public BOOLEAN NOT NULL DEFAULT TRUE,
    original_recipe_id TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (recipe_url_id) REFERENCES "RawDataSources"(source_id),
    FOREIGN KEY (author_id) REFERENCES "Authors"(author_id)
);

-- 4. Ingredients Table: Master list of ingredients
CREATE TABLE "Ingredients" (
    ingredient_id TEXT PRIMARY KEY NOT NULL, -- GUID
    name TEXT NOT NULL UNIQUE
);

-- 6. RecipeSteps Table: Instructions for a recipe
CREATE TABLE "RecipeSteps" (
    step_id TEXT PRIMARY KEY NOT NULL, -- GUID
    recipe_id TEXT NOT NULL,
    step_number INTEGER NOT NULL,
    step_type INTEGER NOT NULL DEFAULT 0,
    instructions TEXT,

    FOREIGN KEY (recipe_id) REFERENCES "Recipes"(recipe_id),

    UNIQUE (recipe_id, step_number)
);

-- 7. RecipeStepIngredients Table: Linking table for specific ingredients used in a step
CREATE TABLE "RecipeStepIngredients" (
    step_ingredient_id TEXT PRIMARY KEY NOT NULL, -- GUID
    recipe_step_id TEXT NOT NULL,
    ingredient_id TEXT NOT NULL,
    raw_text TEXT NOT NULL,
    quantity_type INTEGER DEFAULT 1,
    min_value REAL,
    max_value REAL,
    number_value REAL,
    measurement_type INTEGER NOT NULL DEFAULT 0,
    notes TEXT,

    FOREIGN KEY (recipe_step_id) REFERENCES "RecipeSteps"(step_id),
    FOREIGN KEY (ingredient_id) REFERENCES "Ingredients"(ingredient_id)
);

-- 9. Images Table: Master list of image URLs and types
CREATE TABLE "Images" (
    image_id TEXT PRIMARY KEY NOT NULL, -- GUID
    url TEXT NOT NULL,
    image_type INTEGER NOT NULL DEFAULT 0
);

-- 10. EntityImages Table: Polymorphic linking table for all images
CREATE TABLE "EntityImages" (
    entity_image_id TEXT PRIMARY KEY NOT NULL, -- GUID
    image_id TEXT NOT NULL,
    entity_type INTEGER NOT NULL DEFAULT 0,
    entity_id TEXT NOT NULL, -- References the respective GUID of the entity (Recipe, Step, Author, or Ingredient)

    FOREIGN KEY (image_id) REFERENCES "Images"(image_id)
    -- Note: We cannot create a strict FOREIGN KEY constraint on entity_id
    -- as it could refer to multiple tables. This link is enforced in the application logic.
);

-- 12. Popularity Table: Generic metric tracking for sorting
CREATE TABLE "Popularity" (
    popularity_id TEXT PRIMARY KEY NOT NULL, -- GUID
    entity_type INTEGER NOT NULL DEFAULT 0,
    entity_id TEXT NOT NULL, -- GUID of the entity
    metric_type INTEGER NOT NULL DEFAULT 0,
    count INTEGER NOT NULL DEFAULT 0,

    UNIQUE (entity_type, entity_id, metric_type)
);

CREATE TABLE "AuthorLinks" (
    link_id TEXT PRIMARY KEY NOT NULL, -- GUID
    author_id TEXT NOT NULL,
    url TEXT NOT NULL,

    FOREIGN KEY (author_id) REFERENCES "Authors"(author_id)
);

-- 13. Tags Table: Master list of tags
CREATE TABLE "Tags" (
    tag_id TEXT PRIMARY KEY NOT NULL, -- GUID
    tag_name TEXT NOT NULL UNIQUE
);

-- 14. RecipeTags Table: Many-to-many relationship between recipes and tags
CREATE TABLE "RecipeTags" (
    recipe_id TEXT NOT NULL,
    tag_id TEXT NOT NULL,

    PRIMARY KEY (recipe_id, tag_id),
    FOREIGN KEY (recipe_id) REFERENCES "Recipes"(recipe_id),
    FOREIGN KEY (tag_id) REFERENCES "Tags"(tag_id)
);

-- 15. Categories Table: Master list of categories
CREATE TABLE "Categories" (
    category_id TEXT PRIMARY KEY NOT NULL, -- GUID
    category_name TEXT NOT NULL UNIQUE
);

-- 16. RecipeCategories Table: Many-to-many relationship between recipes and categories
CREATE TABLE "RecipeCategories" (
    recipe_id TEXT NOT NULL,
    category_id TEXT NOT NULL,

    PRIMARY KEY (recipe_id, category_id),
    FOREIGN KEY (recipe_id) REFERENCES "Recipes"(recipe_id),
    FOREIGN KEY (category_id) REFERENCES "Categories"(category_id)
);

-- 17. RecipeHearts Table: Tracks which authors have hearted which recipes
CREATE TABLE "RecipeHearts" (
    author_id TEXT NOT NULL,
    recipe_id TEXT NOT NULL,
    hearted_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (author_id, recipe_id),
    FOREIGN KEY (author_id) REFERENCES "Authors"(author_id),
    FOREIGN KEY (recipe_id) REFERENCES "Recipes"(recipe_id)
);

CREATE TABLE "UserCalendars" (
    id TEXT PRIMARY KEY NOT NULL, -- GUID
    author_id TEXT NOT NULL,
    recipe_id TEXT NOT NULL,
    date TIMESTAMP NOT NULL,
    meal_type INTEGER NOT NULL DEFAULT 0,
    servings_multiplier REAL NOT NULL DEFAULT 1.0,

    FOREIGN KEY (author_id) REFERENCES "Authors"(author_id),
    FOREIGN KEY (recipe_id) REFERENCES "Recipes"(recipe_id)
);

CREATE TABLE "ShoppingListItems" (
    shopping_list_item_id TEXT PRIMARY KEY NOT NULL, -- GUID
    author_id TEXT NOT NULL,
    ingredient_id TEXT NOT NULL,
    recipe_step_id TEXT NOT NULL,
    multiplier REAL NOT NULL DEFAULT 1.0,
    notes TEXT,
    is_purchased BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (author_id) REFERENCES "Authors"(author_id),
    FOREIGN KEY (ingredient_id) REFERENCES "Ingredients"(ingredient_id),
    FOREIGN KEY (recipe_step_id) REFERENCES "RecipeSteps"(step_id)
);

CREATE INDEX idx_shoppinglistitems_author_id ON "ShoppingListItems"(author_id);
CREATE INDEX idx_shoppinglistitems_ingredient_id ON "ShoppingListItems"(ingredient_id);
CREATE INDEX idx_shoppinglistitems_recipe_step_id ON "ShoppingListItems"(recipe_step_id);
CREATE INDEX idx_shoppinglistitems_is_purchased ON "ShoppingListItems"(is_purchased);
CREATE INDEX idx_shoppinglistitems_author_purchased ON "ShoppingListItems"(author_id, is_purchased);
CREATE INDEX idx_authorlinks_author_id ON "AuthorLinks"(author_id);
CREATE INDEX idx_usercalendars_author_id ON "UserCalendars"(author_id);
CREATE INDEX idx_usercalendars_recipe_id ON "UserCalendars"(recipe_id);
CREATE INDEX idx_usercalendars_date ON "UserCalendars"(date);
CREATE INDEX idx_usercalendars_author_date ON "UserCalendars"(author_id, date);
CREATE INDEX idx_usercalendars_meal_type ON "UserCalendars"(meal_type);
CREATE INDEX idx_authors_email ON "Authors"(email);
CREATE INDEX idx_authors_cognito_sub ON "Authors"(cognito_sub);
CREATE INDEX idx_authors_provider_user_id ON "Authors"(provider_user_id);
CREATE INDEX idx_authors_provider_lookup ON "Authors"(auth_provider, provider_user_id);
CREATE INDEX idx_recipes_author_id ON "Recipes"(author_id);
CREATE INDEX idx_recipes_recipe_url_id ON "Recipes"(recipe_url_id);
CREATE INDEX idx_recipesteps_recipe_id ON "RecipeSteps"(recipe_id);
CREATE INDEX idx_recipestepingredients_recipe_step_id ON "RecipeStepIngredients"(recipe_step_id);
CREATE INDEX idx_recipestepingredients_ingredient_id ON "RecipeStepIngredients"(ingredient_id);
CREATE INDEX idx_entityimages_entity_id ON "EntityImages"(entity_id);
CREATE INDEX idx_entityimages_image_id ON "EntityImages"(image_id);
CREATE INDEX idx_popularity_entity_id_type ON "Popularity"(entity_id, entity_type);
CREATE INDEX idx_recipetags_recipe_id ON "RecipeTags"(recipe_id);
CREATE INDEX idx_recipetags_tag_id ON "RecipeTags"(tag_id);
CREATE INDEX idx_recipecategories_recipe_id ON "RecipeCategories"(recipe_id);
CREATE INDEX idx_recipecategories_category_id ON "RecipeCategories"(category_id);
CREATE INDEX idx_recipehearts_recipe_id ON "RecipeHearts"(recipe_id);
CREATE INDEX idx_recipehearts_author_id ON "RecipeHearts"(author_id);

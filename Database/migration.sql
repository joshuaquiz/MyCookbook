-- 1. Preparation: Disable foreign key checks and start a transaction for safety
PRAGMA foreign_keys = OFF;

-- 2. Remove all old application-specific tables (retains RecipeUrls and EF Core metadata tables)
-- The tables to drop are identified from the old schema export.
DROP TABLE IF EXISTS "Ingredients";
DROP TABLE IF EXISTS "Users";
DROP TABLE IF EXISTS "Authors";
DROP TABLE IF EXISTS "Recipes";
DROP TABLE IF EXISTS "RecipeIngredients";
DROP TABLE IF EXISTS "RecipeSteps";
DROP TABLE IF EXISTS "RecipeStepIngredients";

-- Optional: Clear the EF Migrations History to allow for a fresh start with new schema migrations
DELETE FROM "__EFMigrationsHistory";

-- 3. Create the new tables, indexes, and constraints
-- Script generated from the content of new_schema.sql.

-- 11. RawDataSources Table: Tracking processed web data
CREATE TABLE RawDataSources (
    source_id TEXT PRIMARY KEY NOT NULL, -- GUID
    same_as TEXT,
    url TEXT NOT NULL UNIQUE,
    url_host TEXT,
    processing_status INTEGER NOT NULL DEFAULT 0,
    page_type INTEGER NOT NULL DEFAULT 0,
    parser_version INTEGER NOT NULL DEFAULT 0,
    ld_json_data TEXT,
    raw_html TEXT,
    processed_datetime DATETIME,
    error TEXT
);

-- 1. Authors Table: Manages public profiles
CREATE TABLE Authors (
    author_id TEXT PRIMARY KEY NOT NULL, -- GUID
    name TEXT NOT NULL,
    location TEXT,
    bio TEXT,
    is_visible BOOLEAN NOT NULL DEFAULT 0, -- 0: false (default for users), 1: true
    author_type INTEGER NOT NULL DEFAULT 0
);

-- 2. Users Table: Manages login and links to a specific Author profile
CREATE TABLE Users (
    user_id TEXT PRIMARY KEY NOT NULL, -- GUID
    username TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    author_id TEXT NOT NULL UNIQUE, -- Every user MUST have a unique author profile
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (author_id) REFERENCES Authors(author_id)
);

-- 3. Recipes Table: Primary entity for a recipe
CREATE TABLE Recipes (
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
    is_public BOOLEAN NOT NULL DEFAULT 1,
    original_recipe_id TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (recipe_url_id) REFERENCES RawDataSources(source_id),
    FOREIGN KEY (author_id) REFERENCES Authors(author_id)
);

-- 4. Ingredients Table: Master list of ingredients
CREATE TABLE Ingredients (
    ingredient_id TEXT PRIMARY KEY NOT NULL, -- GUID
    name TEXT NOT NULL UNIQUE
);

-- 6. RecipeSteps Table: Instructions for a recipe
CREATE TABLE RecipeSteps (
    step_id TEXT PRIMARY KEY NOT NULL, -- GUID
    recipe_id TEXT NOT NULL,
    step_number INTEGER NOT NULL,
    step_type INTEGER NOT NULL DEFAULT 0,
    instructions TEXT,

    FOREIGN KEY (recipe_id) REFERENCES Recipes(recipe_id),

    UNIQUE (recipe_id, step_number)
);

-- 7. RecipeStepIngredients Table: Linking table for specific ingredients used in a step
CREATE TABLE RecipeStepIngredients (
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

    FOREIGN KEY (recipe_step_id) REFERENCES RecipeSteps(step_id),
    FOREIGN KEY (ingredient_id) REFERENCES Ingredients(ingredient_id)
);

-- 9. Images Table: Master list of image URLs and types
CREATE TABLE Images (
    image_id TEXT PRIMARY KEY NOT NULL, -- GUID
    url TEXT NOT NULL,
    image_type INTEGER NOT NULL DEFAULT 0
);

-- 10. EntityImages Table: Polymorphic linking table for all images
CREATE TABLE EntityImages (
    entity_image_id TEXT PRIMARY KEY NOT NULL, -- GUID
    image_id TEXT NOT NULL,
    entity_type INTEGER NOT NULL DEFAULT 0,
    entity_id TEXT NOT NULL, -- References the respective GUID of the entity (Recipe, Step, Author, or Ingredient)

    FOREIGN KEY (image_id) REFERENCES Images(image_id)
    -- Note: We cannot create a strict FOREIGN KEY constraint on entity_id
    -- as it could refer to multiple tables. This link is enforced in the application logic.
);

-- 12. Popularity Table: Generic metric tracking for sorting
CREATE TABLE Popularity (
    popularity_id TEXT PRIMARY KEY NOT NULL, -- GUID
    entity_type INTEGER NOT NULL DEFAULT 0,
    entity_id TEXT NOT NULL, -- GUID of the entity
    metric_type INTEGER NOT NULL DEFAULT 0,
    count INTEGER NOT NULL DEFAULT 0,

    UNIQUE (entity_type, entity_id, metric_type)
);

CREATE TABLE AuthorLinks (
    link_id TEXT PRIMARY KEY NOT NULL, -- GUID
    author_id TEXT NOT NULL,
    url TEXT NOT NULL,

    FOREIGN KEY (author_id) REFERENCES Authors(author_id)
);

-- Index for efficient lookup of links by author
CREATE INDEX idx_authorlinks_author_id ON AuthorLinks(author_id);

-- Performance Indexes
CREATE INDEX idx_users_author_id ON Users(author_id);
CREATE INDEX idx_recipes_author_id ON Recipes(author_id);
CREATE INDEX idx_recipes_recipe_url_id ON Recipes(recipe_url_id);
CREATE INDEX idx_recipesteps_recipe_id ON RecipeSteps(recipe_id);
CREATE INDEX idx_recipestepingredients_recipe_step_id ON RecipeStepIngredients(recipe_step_id);
CREATE INDEX idx_recipestepingredients_ingredient_id ON RecipeStepIngredients(ingredient_id);
CREATE INDEX idx_entityimages_entity_id ON EntityImages(entity_id);
CREATE INDEX idx_entityimages_image_id ON EntityImages(image_id);
CREATE INDEX idx_popularity_entity_id_type ON Popularity(entity_id, entity_type);


-- 4. Migrate old RecipeUrls data to the new RawDataSources table
INSERT INTO RawDataSources (
    source_id,
    url,
    url_host,
    processing_status,
    parser_version,
    ld_json_data,
    raw_html,
    processed_datetime,
    error,
    page_type
)
SELECT
    "Guid",
    "Uri",
    "Host",
    "ProcessingStatus",
    "ParserVersion",
    "LdJson",
    "Html",
    "CompletedAt",
    "Exception",
    0 -- Default value for the new page_type column
FROM
    "RecipeUrls";

-- 5. Delete the old RecipeUrls table
DROP TABLE IF EXISTS "RecipeUrls";

-- 6. Cleanup: Commit the transaction and re-enable foreign key constraints
PRAGMA foreign_keys = ON;

-- Optional: Reclaim space in the database file
--VACUUM;
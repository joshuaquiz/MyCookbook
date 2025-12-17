--
-- Add sharing functionality for recipes and cookbooks
--

-- Recipe Shares Table
CREATE TABLE public."RecipeShares" (
    share_id uuid NOT NULL DEFAULT gen_random_uuid(),
    recipe_id uuid NOT NULL,
    shared_by_author_id uuid NOT NULL,
    share_token text NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    expires_at timestamp without time zone,
    access_count integer DEFAULT 0 NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    CONSTRAINT "RecipeShares_pkey" PRIMARY KEY (share_id),
    CONSTRAINT "RecipeShares_recipe_id_fkey" FOREIGN KEY (recipe_id) 
        REFERENCES public."Recipes"(recipe_id) ON DELETE CASCADE,
    CONSTRAINT "RecipeShares_shared_by_author_id_fkey" FOREIGN KEY (shared_by_author_id) 
        REFERENCES public."Authors"(author_id) ON DELETE CASCADE
);

-- Create unique index on share_token for fast lookups
CREATE UNIQUE INDEX "RecipeShares_share_token_idx" ON public."RecipeShares"(share_token);

-- Create index on recipe_id for finding all shares of a recipe
CREATE INDEX "RecipeShares_recipe_id_idx" ON public."RecipeShares"(recipe_id);

-- Cookbook Shares Table
CREATE TABLE public."CookbookShares" (
    share_id uuid NOT NULL DEFAULT gen_random_uuid(),
    author_id uuid NOT NULL,
    share_token text NOT NULL,
    share_name text NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    expires_at timestamp without time zone,
    access_count integer DEFAULT 0 NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    CONSTRAINT "CookbookShares_pkey" PRIMARY KEY (share_id),
    CONSTRAINT "CookbookShares_author_id_fkey" FOREIGN KEY (author_id) 
        REFERENCES public."Authors"(author_id) ON DELETE CASCADE
);

-- Create unique index on share_token for fast lookups
CREATE UNIQUE INDEX "CookbookShares_share_token_idx" ON public."CookbookShares"(share_token);

-- Create index on author_id for finding all shares by a user
CREATE INDEX "CookbookShares_author_id_idx" ON public."CookbookShares"(author_id);

-- Cookbook Share Recipes (many-to-many relationship)
CREATE TABLE public."CookbookShareRecipes" (
    cookbook_share_id uuid NOT NULL,
    recipe_id uuid NOT NULL,
    CONSTRAINT "CookbookShareRecipes_pkey" PRIMARY KEY (cookbook_share_id, recipe_id),
    CONSTRAINT "CookbookShareRecipes_cookbook_share_id_fkey" FOREIGN KEY (cookbook_share_id) 
        REFERENCES public."CookbookShares"(share_id) ON DELETE CASCADE,
    CONSTRAINT "CookbookShareRecipes_recipe_id_fkey" FOREIGN KEY (recipe_id) 
        REFERENCES public."Recipes"(recipe_id) ON DELETE CASCADE
);

-- Create index on recipe_id for finding which shares include a recipe
CREATE INDEX "CookbookShareRecipes_recipe_id_idx" ON public."CookbookShareRecipes"(recipe_id);


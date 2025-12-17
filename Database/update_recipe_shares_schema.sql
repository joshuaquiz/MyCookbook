--
-- Update RecipeShares table to support sharing to specific users
--

-- Remove ExpiresAt column
ALTER TABLE public."RecipeShares" 
DROP COLUMN IF EXISTS expires_at;

-- Add SharedToAuthorId column (nullable to support both URL shares and user shares)
ALTER TABLE public."RecipeShares" 
ADD COLUMN shared_to_author_id uuid;

-- Add foreign key constraint
ALTER TABLE public."RecipeShares"
ADD CONSTRAINT fk_recipe_shares_shared_to_author
FOREIGN KEY (shared_to_author_id) 
REFERENCES public."Authors"(author_id)
ON DELETE CASCADE;

-- Create index for querying shares by recipient
CREATE INDEX idx_recipe_shares_shared_to_author ON public."RecipeShares"(shared_to_author_id);

-- Create index for querying share history (for sorting by share frequency)
CREATE INDEX idx_recipe_shares_shared_by_to ON public."RecipeShares"(shared_by_author_id, shared_to_author_id);


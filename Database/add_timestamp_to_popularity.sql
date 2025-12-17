--
-- Add timestamp column to Popularity table for time-based analytics
--

-- Add created_at column with default value
ALTER TABLE public."Popularity" 
ADD COLUMN created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL;

-- Create index for time-based queries
CREATE INDEX idx_popularity_created_at ON public."Popularity"(created_at);

-- Create composite index for entity queries with time filtering
CREATE INDEX idx_popularity_entity_time ON public."Popularity"(entity_type, entity_id, metric_type, created_at);


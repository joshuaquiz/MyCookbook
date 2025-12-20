--
-- Remove unique constraint from Popularity table to allow multiple time-stamped entries
-- This enables time-based analytics (e.g., "views in last 24 hours")
--

-- Drop the existing unique constraint
-- PostgreSQL constraint name format: IX_TableName_Column1_Column2_...
DROP INDEX IF EXISTS public."IX_Popularity_EntityType_EntityId_MetricType";

-- Create a non-unique index for efficient queries
CREATE INDEX IF NOT EXISTS idx_popularity_entity_metric 
ON public."Popularity"(entity_type, entity_id, metric_type);

-- The idx_popularity_entity_time index was already created in add_timestamp_to_popularity.sql
-- CREATE INDEX idx_popularity_entity_time ON public."Popularity"(entity_type, entity_id, metric_type, created_at);


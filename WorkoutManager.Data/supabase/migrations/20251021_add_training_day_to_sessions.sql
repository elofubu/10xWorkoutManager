-- Migration: Add training_day_id to sessions table
-- This allows tracking which specific training day was performed in each session
-- Every session MUST be linked to a training day

-- Add training_day_id column to sessions table
ALTER TABLE public.sessions 
ADD COLUMN training_day_id BIGINT NOT NULL
REFERENCES public.training_days(id) ON DELETE RESTRICT;

-- Create an index for better query performance
CREATE INDEX idx_sessions_training_day_id ON public.sessions(training_day_id);

-- Add comment for documentation
COMMENT ON COLUMN public.sessions.training_day_id IS 'The specific training day performed in this session. Required - every session must be linked to a training day.';


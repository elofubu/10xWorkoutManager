-- Migration: Update plan_day_exercises to use composite primary key
-- This enables proper many-to-many relationship handling in Supabase C# library

-- Step 1: Drop the old primary key constraint
ALTER TABLE public.plan_day_exercises
  DROP CONSTRAINT plan_day_exercises_pkey;

-- Step 2: Add composite primary key (id, training_day_id, exercise_id)
ALTER TABLE public.plan_day_exercises
  ADD PRIMARY KEY (id, training_day_id, exercise_id);

-- Step 3: Add unique constraint to prevent duplicate exercises in the same training day
ALTER TABLE public.plan_day_exercises
  ADD CONSTRAINT unique_training_day_exercise
  UNIQUE (training_day_id, exercise_id);

-- Step 4: Create index for better query performance on training_day_id
CREATE INDEX idx_plan_day_exercises_training_day_id
  ON public.plan_day_exercises(training_day_id);

-- Step 5: Create index for better query performance on exercise_id
CREATE INDEX idx_plan_day_exercises_exercise_id
  ON public.plan_day_exercises(exercise_id);

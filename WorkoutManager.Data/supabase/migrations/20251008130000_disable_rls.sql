-- Migration to disable all previously defined RLS policies and disable RLS on tables.
-- This is useful for development or specific testing scenarios where RLS is not desired.

-- Drop RLS policies for muscle_groups
DROP POLICY IF EXISTS "Allow read access to all authenticated users" ON public.muscle_groups;

-- Drop RLS policies for exercises
DROP POLICY IF EXISTS "Allow read access for predefined and user-owned exercises" ON public.exercises;
DROP POLICY IF EXISTS "Allow insert for user-owned exercises" ON public.exercises;
DROP POLICY IF EXISTS "Allow update for user-owned exercises" ON public.exercises;
DROP POLICY IF EXISTS "Allow delete for user-owned exercises" ON public.exercises;

-- Drop RLS policies for workout_plans
DROP POLICY IF EXISTS "Allow full access for user-owned plans" ON public.workout_plans;

-- Drop RLS policies for training_days
DROP POLICY IF EXISTS "Allow full access for plan owners" ON public.training_days;

-- Drop RLS policies for plan_day_exercises
DROP POLICY IF EXISTS "Allow full access for plan owners" ON public.plan_day_exercises;

-- Drop RLS policies for sessions
DROP POLICY IF EXISTS "Allow full access for user-owned sessions" ON public.sessions;

-- Drop RLS policies for session_exercises
DROP POLICY IF EXISTS "Allow full access for session owners" ON public.session_exercises;

-- Drop RLS policies for exercise_sets
DROP POLICY IF EXISTS "Allow full access for session owners" ON public.exercise_sets;


-- Disable RLS on all relevant tables
ALTER TABLE public.muscle_groups DISABLE ROW LEVEL SECURITY;
ALTER TABLE public.exercises DISABLE ROW LEVEL SECURITY;
ALTER TABLE public.workout_plans DISABLE ROW LEVEL SECURITY;
ALTER TABLE public.training_days DISABLE ROW LEVEL SECURITY;
ALTER TABLE public.plan_day_exercises DISABLE ROW LEVEL SECURITY;
ALTER TABLE public.sessions DISABLE ROW LEVEL SECURITY;
ALTER TABLE public.session_exercises DISABLE ROW LEVEL SECURITY;
ALTER TABLE public.exercise_sets DISABLE ROW LEVEL SECURITY;

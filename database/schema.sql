-- Sailwind Regatta — Supabase schema

-- ============================================================
-- Tables
-- ============================================================

CREATE TABLE race (
    id   int  PRIMARY KEY,
    name text NOT NULL
);

CREATE TABLE player (
    id         uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    key        text        UNIQUE NOT NULL,  -- SHA256(steamId + salt), computed client-side
    name       text        NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE run (
    id          uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id   uuid        NOT NULL REFERENCES player(id),
    race_id     int         NOT NULL REFERENCES race(id),
    started_at  timestamptz NOT NULL,
    finished_at timestamptz,   -- NULL while in progress
    duration    int,           -- real-world seconds, NULL until finish
    created_at  timestamptz NOT NULL DEFAULT now()
);

-- ============================================================
-- Seed data
-- ============================================================

INSERT INTO race (id, name) VALUES
    (1, 'The Capital Circuit');


-- ============================================================
-- RPC functions
-- ============================================================
CREATE OR REPLACE FUNCTION upsert_player(key text, name text)
RETURNS uuid LANGUAGE sql SECURITY DEFINER
SET search_path = ''
AS $$
    INSERT INTO public.player (key, name)
    VALUES ($1, $2)
    ON CONFLICT (key) DO UPDATE SET name = EXCLUDED.name
    RETURNING id;
$$;

CREATE OR REPLACE FUNCTION start_run(player_id uuid, race_id int, started_at timestamptz)
RETURNS uuid LANGUAGE sql SECURITY DEFINER
SET search_path = ''
AS $$
    INSERT INTO public.run (player_id, race_id, started_at)
    VALUES ($1, $2, $3)
    RETURNING id;
$$;

CREATE OR REPLACE FUNCTION finish_run(run_id uuid, finished_at timestamptz, duration int)
RETURNS void LANGUAGE plpgsql SECURITY DEFINER
SET search_path = ''
AS $$
DECLARE
    n int;
BEGIN
    UPDATE public.run AS t
    SET finished_at = $2,
        duration = $3
    WHERE t.id = $1 AND t.finished_at IS NULL;
    GET DIAGNOSTICS n = ROW_COUNT;
    IF n = 0 THEN
        RAISE EXCEPTION 'run not found or already finished';
    END IF;
END;
$$;

-- ============================================================
-- RLS policies
-- ============================================================

ALTER TABLE race ENABLE ROW LEVEL SECURITY;
ALTER TABLE player ENABLE ROW LEVEL SECURITY;
ALTER TABLE run ENABLE ROW LEVEL SECURITY;

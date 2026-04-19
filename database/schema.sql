-- Sailwind Regatta — Supabase schema

-- ============================================================
-- Tables
-- ============================================================

CREATE TABLE player (
    id         uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    key        text        UNIQUE NOT NULL,  -- SHA256(steamId + salt), computed client-side
    name       text        NOT NULL,
    dev        boolean     NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE run (
    id           uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id    uuid        NOT NULL REFERENCES player(id),
    race_id      int,
    boat_type_id int,             -- SaveableObject.sceneIndex, NULL until first steering-wheel use
    started_at   timestamptz NOT NULL,
    finished_at  timestamptz,     -- NULL while in progress
    aborted_at   timestamptz,     -- NULL unless aborted
    duration_minutes bigint,          -- in-game minutes, NULL until finish
    created_at   timestamptz NOT NULL DEFAULT now()
);

-- ============================================================
-- RPC functions
-- ============================================================
CREATE OR REPLACE FUNCTION upsert_player(key text, name text, dev boolean DEFAULT false)
RETURNS uuid LANGUAGE sql SECURITY DEFINER
SET search_path = ''
AS $$
    INSERT INTO public.player (key, name, dev)
    VALUES ($1, $2, $3)
    ON CONFLICT (key) DO UPDATE SET name = EXCLUDED.name, dev = EXCLUDED.dev
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

CREATE OR REPLACE FUNCTION finish_run(run_id uuid, finished_at timestamptz, duration_minutes bigint, boat_type_id int DEFAULT NULL)
RETURNS void LANGUAGE plpgsql SECURITY DEFINER
SET search_path = ''
AS $$
DECLARE
    n int;
BEGIN
    UPDATE public.run AS t
    SET finished_at      = $2,
        duration_minutes = $3,
        boat_type_id     = $4
    WHERE t.id = $1 AND t.finished_at IS NULL AND t.aborted_at IS NULL;
    GET DIAGNOSTICS n = ROW_COUNT;
    IF n = 0 THEN
        RAISE EXCEPTION 'run not found or already finished';
    END IF;
END;
$$;

CREATE OR REPLACE FUNCTION abort_run(run_id uuid, aborted_at timestamptz)
RETURNS void LANGUAGE plpgsql SECURITY DEFINER
SET search_path = ''
AS $$
DECLARE
    n int;
BEGIN
    UPDATE public.run AS t
    SET aborted_at = $2
    WHERE t.id = $1 AND t.finished_at IS NULL AND t.aborted_at IS NULL;
    GET DIAGNOSTICS n = ROW_COUNT;
    IF n = 0 THEN
        RAISE EXCEPTION 'run not found or already finished';
    END IF;
END;
$$;

CREATE OR REPLACE FUNCTION get_leaderboard(race_id int, max_results int DEFAULT 5, dev boolean DEFAULT false, player_id uuid DEFAULT NULL)
RETURNS TABLE (rank int, player_name text, duration_minutes bigint, boat_type_id int)
LANGUAGE sql SECURITY DEFINER
SET search_path = ''
AS $$
    WITH best_runs AS (
        SELECT DISTINCT ON (r.player_id)
            r.player_id,
            p.name AS player_name,
            r.duration_minutes,
            r.boat_type_id
        FROM public.run r
        JOIN public.player p ON p.id = r.player_id
        WHERE r.race_id     = get_leaderboard.race_id
          AND r.finished_at IS NOT NULL
          AND r.aborted_at  IS NULL
          AND p.dev         = get_leaderboard.dev
        ORDER BY r.player_id, r.duration_minutes ASC
    ),
    ranked AS (
        SELECT
            r.player_id,
            ROW_NUMBER() OVER (ORDER BY r.duration_minutes ASC) AS rank,
            r.player_name,
            r.duration_minutes,
            r.boat_type_id
        FROM best_runs r
    )
    SELECT rank, player_name, duration_minutes, boat_type_id FROM ranked WHERE rank <= get_leaderboard.max_results
    UNION ALL
    SELECT rank, player_name, duration_minutes, boat_type_id FROM ranked
        WHERE get_leaderboard.player_id IS NOT NULL
          AND player_id = get_leaderboard.player_id
          AND rank > get_leaderboard.max_results
    ORDER BY rank;
$$;


-- ============================================================
-- RLS policies
-- ============================================================

ALTER TABLE player ENABLE ROW LEVEL SECURITY;
ALTER TABLE run ENABLE ROW LEVEL SECURITY;

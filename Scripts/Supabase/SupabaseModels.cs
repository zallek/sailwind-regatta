using System;

// JsonUtility assigns these fields via reflection — suppress the "never assigned" warning.
#pragma warning disable CS0649

namespace SailwindRegatta
{
    // Field names must exactly match PostgREST column names (snake_case).

    // Used by the upsert_player RPC — field names must match the SQL function parameter names.
    [Serializable]
    internal class UpsertPlayerRpcRequest
    {
        public string key;
        public string name;
    }
    // RPC returns a scalar UUID string directly — no wrapper needed.

    // Used by the start_run RPC — field names must match the SQL function parameter names.
    [Serializable]
    internal class StartRunRpcRequest
    {
        public string player_id;
        public int    race_id;
        public string started_at;  // ISO 8601 UTC string
    }

    // Used by the finish_run RPC when boat type is known.
    [Serializable]
    internal class FinishRunRpcRequest
    {
        public string run_id;
        public string finished_at;   // ISO 8601 UTC string
        public int    duration;      // real-world seconds
        public int    boat_type_id;  // SaveableObject.sceneIndex
    }

    // Used by the finish_run RPC when boat type is unknown (omits boat_type_id so the DB default NULL applies).
    [Serializable]
    internal class FinishRunNoBoatRpcRequest
    {
        public string run_id;
        public string finished_at;
        public int    duration;
    }

    // Used by the abort_run RPC — field names must match the SQL function parameter names.
    [Serializable]
    internal class AbortRunRpcRequest
    {
        public string run_id;
        public string aborted_at;  // ISO 8601 UTC string
    }

    // Used by the get_leaderboard RPC.
    [Serializable]
    internal class GetLeaderboardRpcRequest
    {
        public int race_id;
        public int max_results;
    }

    // One row returned by the get_leaderboard RPC.
    [Serializable]
    internal class LeaderboardEntryResponse
    {
        public int    rank;
        public string player_name;
        public int    duration;    // real-world seconds
    }

    // JsonUtility cannot deserialise a root-level JSON array.
    // Wrap the raw response string as {"items":[...]} before parsing.
    [Serializable]
    internal class LeaderboardWrapper
    {
        public LeaderboardEntryResponse[] items;
    }
}

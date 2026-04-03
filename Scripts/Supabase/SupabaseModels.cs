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

    // Used by the finish_run RPC — field names must match the SQL function parameter names.
    [Serializable]
    internal class FinishRunRpcRequest
    {
        public string run_id;
        public string finished_at;  // ISO 8601 UTC string
        public int    duration;     // real-world seconds
    }
}

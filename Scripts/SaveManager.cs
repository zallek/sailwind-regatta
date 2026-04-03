using System;
using System.Globalization;
using UnityEngine;

namespace SailwindRegatta
{

    [Serializable]
    internal class RunSaveEntry
    {
        public int    raceId;
        public int    checkpointIndex;
        public int    startDay;
        public string id;              // Supabase run id; empty if StartRunAsync hasn't resolved yet
        public string startedAtUtc;    // ISO 8601 round-trip string of Run.StartedAt
    }

    [Serializable]
    internal class SRSaveData
    {
        // JsonUtility doesn't handle null object fields well, so we use an explicit flag.
        public bool hasActiveRace;
        public RunSaveEntry activeRace = new RunSaveEntry();
    }

    internal static class SaveManager
    {
        public static void Save()
        {
            var payload = new SRSaveData();

            var active = RaceManager.Instance?.ActiveRun;
            if (active != null)
            {
                payload.hasActiveRace = true;
                payload.activeRace = new RunSaveEntry
                {
                    raceId          = active.Race.Id,
                    checkpointIndex = active.NextCheckpointIndex,
                    startDay        = active.StartDay,
                    id              = active.Id ?? string.Empty,
                    startedAtUtc    = active.StartedAt.ToString("o")
                };
            }

            GameState.modData["SailwindRegatta"] = JsonUtility.ToJson(payload);
        }

        public static void Load()
        {
            if (!GameState.modData.ContainsKey("SailwindRegatta")) return;

            var payload = JsonUtility.FromJson<SRSaveData>(GameState.modData["SailwindRegatta"]);
            if (payload == null || !payload.hasActiveRace) return;

            var entry = payload.activeRace;
            var race = RaceRegistry.GetById(entry.raceId);
            if (race == null) return;

            DateTime startedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(entry.startedAtUtc))
                DateTime.TryParse(entry.startedAtUtc, null, DateTimeStyles.RoundtripKind, out startedAt);

            var active = new Run(race, entry.startDay, startedAt)
            {
                NextCheckpointIndex = entry.checkpointIndex,
                Id                  = string.IsNullOrEmpty(entry.id) ? null : entry.id
            };

            if (RaceManager.Instance != null)
                RaceManager.Instance.ActiveRun = active;
        }
    }
}

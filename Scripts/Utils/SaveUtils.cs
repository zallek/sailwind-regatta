using System;
using System.Globalization;
using UnityEngine;

namespace SailwindRegatta
{

    // NOTE: JsonUtility silently drops nested [Serializable] objects — keep this class flat.
    [Serializable]
    internal class SRSaveData
    {
        public bool   hasActiveRace;
        public int    raceId;
        public int    checkpointIndex;
        public int    startDay;
        public string id;           // Supabase run id; empty if StartRunAsync hasn't resolved yet
        public string startedAtUtc; // ISO 8601 round-trip string of Run.StartedAt
        public int    boatTypeId;    // -1 when BoatTypeId is null (JsonUtility cannot serialize int?)
        public float  elapsedSeconds;
    }

    internal static class SaveUtils
    {
        public static void Save()
        {
            var payload = new SRSaveData();

            var active = RaceManager.Instance?.ActiveRun;
            if (active != null)
            {
                payload.hasActiveRace   = true;
                payload.raceId          = active.Race.Id;
                payload.checkpointIndex = active.NextCheckpointIndex;
                payload.startDay        = active.StartDay;
                payload.id              = active.Id ?? string.Empty;
                payload.startedAtUtc    = active.StartedAt.ToString("o");
                payload.boatTypeId      = active.BoatTypeId ?? -1;
                payload.elapsedSeconds  = active.ElapsedSeconds;
            }

            string json = JsonUtility.ToJson(payload);

            if (GameState.modData.ContainsKey(Plugin.PLUGIN_GUID))
                GameState.modData[Plugin.PLUGIN_GUID] = json;
            else
                GameState.modData.Add(Plugin.PLUGIN_GUID, json);
        }

        public static void Load()
        {
            if (!GameState.modData.ContainsKey(Plugin.PLUGIN_GUID)) return;

            string json = GameState.modData[Plugin.PLUGIN_GUID];
            var payload = JsonUtility.FromJson<SRSaveData>(json);
            if (payload == null)
            {
                Plugin.Log.LogError("Failed to parse mod data from save.");
                return;
            }
            if (!payload.hasActiveRace) return;

            var race = RaceRegistry.GetById(payload.raceId);
            if (race == null)
            {
                Plugin.Log.LogError($"Saved run references unknown race id {payload.raceId}.");
                return;
            }

            DateTime startedAt;
            if (string.IsNullOrEmpty(payload.startedAtUtc) ||
                !DateTime.TryParse(payload.startedAtUtc, null, DateTimeStyles.RoundtripKind, out startedAt))
            {
                Plugin.Log.LogWarning($"Saved run has invalid startedAtUtc '{payload.startedAtUtc}'; discarding run.");
                return;
            }

            int? boatTypeId = payload.boatTypeId >= 0 ? payload.boatTypeId : (int?)null;
            string id = string.IsNullOrEmpty(payload.id) ? null : payload.id;

            var active = new Run(race, payload.startDay, startedAt)
            {
                NextCheckpointIndex = payload.checkpointIndex,
                Id                  = id,
                BoatTypeId          = boatTypeId,
                ElapsedSeconds      = payload.elapsedSeconds
            };

            if (RaceManager.Instance == null)
            {
                Plugin.Log.LogError("RaceManager not ready when loading save.");
                return;
            }

            RaceManager.Instance.ActiveRun = active;
            Plugin.Log.LogInfo($"Active run restored from save (race {race.DisplayName}, run id: {id}).");
        }
    }
}

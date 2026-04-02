using System;
using HarmonyLib;
using UnityEngine;

namespace SailwindRegatta
{
    [HarmonyPatch(typeof(SaveLoadManager), "SaveModData")]
    internal class SaveModDataPatch
    {
        static void Postfix()
        {
            var payload = new SRSaveData();

            var active = RaceManager.Instance?.ActiveRace;
            if (active != null)
            {
                payload.hasActiveRace = true;
                payload.activeRace = new ActiveRaceSaveEntry
                {
                    raceId            = active.Definition.Id,
                    checkpointIndex   = active.NextCheckpointIndex,
                    startDay          = active.StartDay,
                    startGameTime     = active.StartGameTime
                };
            }

            GameState.modData["SailwindRegatta"] = JsonUtility.ToJson(payload);
        }
    }

    [HarmonyPatch(typeof(SaveLoadManager), "LoadModData")]
    internal class LoadModDataPatch
    {
        static void Postfix()
        {
            if (!GameState.modData.ContainsKey("SailwindRegatta")) return;

            var payload = JsonUtility.FromJson<SRSaveData>(GameState.modData["SailwindRegatta"]);
            if (payload == null || !payload.hasActiveRace) return;

            var entry = payload.activeRace;
            var race = RaceRegistry.GetById(entry.raceId);
            if (race == null) return;

            var active = new ActiveRace(race, entry.startDay, entry.startGameTime)
            {
                NextCheckpointIndex = entry.checkpointIndex
            };

            if (RaceManager.Instance != null)
                RaceManager.Instance.ActiveRace = active;
        }
    }

    [Serializable]
    internal class ActiveRaceSaveEntry
    {
        public int raceId;
        public int checkpointIndex;
        public int startDay;
        public float startGameTime;
    }

    [Serializable]
    internal class SRSaveData
    {
        // JsonUtility doesn't handle null object fields well, so we use an explicit flag.
        public bool hasActiveRace;
        public ActiveRaceSaveEntry activeRace = new ActiveRaceSaveEntry();
    }
}

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace SailwindRegatta
{
    internal class RaceManager : MonoBehaviour
    {
        public static RaceManager Instance { get; private set; }

        // Set by SaveLoadPatches when a save is loaded.
        internal Run ActiveRun { get; set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (ActiveRun != null && !Sun.SunPaused())
                ActiveRun.ElapsedSeconds += Time.unscaledDeltaTime;
        }

        internal void OnSteeringWheelActivated(Rudder rudder)
        {
            if (ActiveRun == null)
                return;
            
            int? boatTypeId = BoatTypeUtils.TryGetBoatTypeIdFromRudder(rudder);
            if (boatTypeId == null) {
                Plugin.Log.LogError("Could not resolve boat type id for this rudder.");
                return;
            }

            if (ActiveRun.BoatTypeId == null)
            {
                ActiveRun.BoatTypeId = boatTypeId;
                Plugin.Log.LogInfo($"Run locked to boat type id {boatTypeId.Value}");
                return;
            }

            if (ActiveRun.BoatTypeId.Value != boatTypeId.Value)
                AbortRace("You switched to a different boat.");
        }

        // Called by the Port.Start() patch when any port initialises.
        // Injects a CheckpointArea child if this port is part of a race.
        internal void TryInjectCheckpointArea(Port port)
        {
            Checkpoint checkpoint = null;
            foreach (var c in CheckpointRegistry.Checkpoints)
            {
                if (c.PortName.ToPortString() == port.GetPortName())
                {
                    checkpoint = c;
                    break;
                }
            }
            if (checkpoint == null) return;

            // Avoid duplicates if the scene reloads (old components are destroyed with it).
            if (port.GetComponentInChildren<CheckpointArea>() != null) return;

            var child = new GameObject("CheckpointArea");
            child.transform.SetParent(port.transform, worldPositionStays: false);
            child.AddComponent<CheckpointArea>().Init(checkpoint);

            Plugin.Log.LogDebug($"CheckpointArea injected on port: {port.GetPortName()}");
        }

        // Called by the Port.Start() patch when any port initialises.
        // Injects a RaceMasterNPC child for every registry entry matching this port.
        internal void TryInjectRaceMasterNPC(Port port)
        {
            foreach (var raceMaster in RaceMasterRegistry.RaceMasters)
            {
                if (raceMaster.PortName.ToPortString() != port.GetPortName()) continue;
                if (port.GetComponentInChildren<RaceMasterNPC>() != null) continue;

                var race = RaceRegistry.Races.Find(r => r.Id == raceMaster.RaceId);
                if (race == null) continue;

                var go = new GameObject("RaceMasterNPC");
                go.transform.SetParent(port.transform, worldPositionStays: false);
                go.AddComponent<RaceMasterNPC>().Init(race, raceMaster);

                Plugin.Log.LogDebug($"RaceMasterNPC injected on port: {port.GetPortName()}");
            }
        }

        // Called by CheckpointArea when the player enters a checkpoint zone.
        // Race start is now handled exclusively by RaceMasterNPC — checkpoint zones
        // only advance an already-active race.
        internal void OnPlayerEnteredCheckpoint(Checkpoint checkpoint)
        {
            if (!GameState.playing) return;

            if (ActiveRun != null)
                TryAdvanceCheckpoint(checkpoint);
        }

        // Called by RaceMasterNPC when the player clicks the NPC to start a race.
        internal void StartRace(Race race)
        {
            var startedAt = DateTime.UtcNow;
            ActiveRun = new Run(race, GameState.day, startedAt);

            string raceName = ActiveRun.Race.DisplayName;
            string nextCheckpointName = ActiveRun.NextCheckpointName.ToDisplayName();
            Plugin.Log.LogInfo($"Race started: {raceName}");
            NotificationUi.instance.ShowNotification($"{raceName}\nRace started!\nHead to: {nextCheckpointName}", 15f);

            if (Plugin.Session != null)
                _ = SaveRunStartedAsync(race.Id, startedAt);
        }

        private async Task SaveRunStartedAsync(int raceId, DateTime startedAt)
        {
            string id = await SupabaseClient.StartRunAsync(Plugin.Session, raceId, startedAt);
            if (id != null && ActiveRun != null)
            {
                ActiveRun.Id = id;
                Plugin.Log.LogDebug($"Run started on Supabase. Run id: {id}");
            }
        }

        private void TryAdvanceCheckpoint(Checkpoint checkpoint)
        {
            if (checkpoint.Name != ActiveRun.NextCheckpointName) return;

            ActiveRun.NextCheckpointIndex++;

            if (ActiveRun.IsFinished)
            {
                FinishRace();
            }
            else
            {
                string raceName = ActiveRun.Race.DisplayName;
                string nextCheckpointName = ActiveRun.NextCheckpointName.ToDisplayName();
                int reached = ActiveRun.NextCheckpointIndex;
                int total = ActiveRun.Race.RouteCheckpoints.Length - 1;
                Plugin.Log.LogInfo($"Race checkpoint: {raceName} - {reached}/{total}");
                NotificationUi.instance.ShowNotification($"{raceName}\nCheckpoint {reached} / {total}\nHead to: {nextCheckpointName}", 15f);
            }
        }

        private void FinishRace()
        {
            string raceName = ActiveRun.Race.DisplayName;
            int raceId = ActiveRun.Race.Id;
            string runId = ActiveRun.Id;
            int? boatTypeId = ActiveRun.BoatTypeId;
            var startedAt = ActiveRun.StartedAt;
            var finishedAt = DateTime.UtcNow;
            int duration = (int)ActiveRun.ElapsedSeconds;

            Plugin.Log.LogInfo($"Race finished: {raceName}");
            NotificationUi.instance.ShowNotification($"{raceName}\nRace finished in {duration}s!", 15f);

            ActiveRun = null;

            if (Plugin.Session != null)
            {
                if (runId != null)
                    _ = SaveRunFinishedAsync(runId, finishedAt, duration, boatTypeId);
                else
                    _ = SaveRunRetroactiveAsync(raceId, startedAt, finishedAt, duration, boatTypeId);
            }
        }

        private async Task SaveRunFinishedAsync(string runId, DateTime finishedAt, int duration, int? boatTypeId)
        {
            var success = await SupabaseClient.FinishRunAsync(runId, finishedAt, duration, boatTypeId);
            if (success)
                Plugin.Log.LogDebug($"Run finished on Supabase. Run id: {runId}");
        }

        private async Task SaveRunRetroactiveAsync(int raceId, DateTime startedAt, DateTime finishedAt, int duration, int? boatTypeId)
        {
            string runId = await SupabaseClient.StartRunAsync(Plugin.Session, raceId, startedAt);
            if (runId == null)
            {
                Plugin.Log.LogError("Retroactive run start failed; result not saved online.");
                return;
            }
            var success = await SupabaseClient.FinishRunAsync(runId, finishedAt, duration, boatTypeId);
            if (success)
                Plugin.Log.LogDebug($"Run saved retroactively on Supabase. Run id: {runId}");
        }

        internal void AbortRace(string reason)
        {
            if (ActiveRun == null)
                return;

            string runId = ActiveRun.Id;
            var abortedAt = DateTime.UtcNow;
            string raceName = ActiveRun.Race.DisplayName;
            Plugin.Log.LogInfo($"Race aborted: {raceName} - {reason}");
            NotificationUi.instance.ShowNotification($"{raceName}\nRace aborted\n{reason}", 15f);
            ActiveRun = null;

            if (Plugin.Session != null && runId != null)
                _ = SaveRunAbortedAsync(runId, abortedAt);
        }

        private async Task SaveRunAbortedAsync(string runId, DateTime abortedAt)
        {
            var success = await SupabaseClient.AbortRunAsync(runId, abortedAt);
            if (success)
                Plugin.Log.LogDebug($"Run aborted on Supabase. Run id: {runId}");
        }
    }
}

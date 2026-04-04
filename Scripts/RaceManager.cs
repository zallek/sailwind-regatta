using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SailwindRegatta
{
    internal class RaceManager : MonoBehaviour
    {
        public static RaceManager Instance { get; private set; }

        // Set by SaveLoadPatches when a save is loaded.
        internal Run ActiveRun { get; set; }

        // Checkpoints keyed by port string (for injection lookup), built from all races.
        // When the same port appears in multiple checkpoints, last definition wins (one trigger per port).
        private readonly Dictionary<string, Checkpoint> _checkpointsByPort = new Dictionary<string, Checkpoint>();

        private void Awake()
        {
            Instance = this;

            foreach (var race in RaceRegistry.Races)
                foreach (var checkpoint in race.Checkpoints)
                    _checkpointsByPort[checkpoint.Name.ToPortString()] = checkpoint;
        }

        private void Update()
        {
            if (ActiveRun != null && !Sun.SunPaused())
                ActiveRun.ElapsedSeconds += Time.unscaledDeltaTime;
        }

        internal void OnSteeringWheelActivated(Rudder rudder)
        {
            int? boatTypeId = BoatTypeHelper.TryGetBoatTypeIdFromRudder(rudder);
            if (boatTypeId != null)
                Plugin.Log.LogInfo($"Steering wheel activated — boat type id: {boatTypeId.Value}");
            else
                Plugin.Log.LogWarning("Steering wheel activated — could not resolve boat type id for this rudder.");

            if (ActiveRun == null)
                return;

            if (boatTypeId == null)
                return;

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
            if (!_checkpointsByPort.TryGetValue(port.GetPortName(), out var checkpoint)) return;

            // Avoid duplicates if the scene reloads (old components are destroyed with it).
            if (port.GetComponentInChildren<CheckpointArea>() != null) return;

            var child = new GameObject("CheckpointArea");
            child.transform.SetParent(port.transform, worldPositionStays: false);
            child.AddComponent<CheckpointArea>().Init(checkpoint);

            Plugin.Log.LogInfo($"CheckpointArea injected on port: {port.GetPortName()}");
        }

        // Called by CheckpointArea when the player enters a checkpoint zone.
        internal void OnPlayerEnteredCheckpoint(CheckpointName name)
        {
            if (!GameState.playing) return;

            if (ActiveRun == null)
            {
                TryStartRace(name);
            }
            else
            {
                TryAdvanceCheckpoint(name);
            }
        }

        private void TryStartRace(CheckpointName name)
        {
            var race = RaceRegistry.Races.Find(r => r.Checkpoints[0].Name == name);
            if (race == null) return;

            var startedAt = DateTime.UtcNow;
            ActiveRun = new Run(race, GameState.day, startedAt);

            Plugin.Log.LogInfo($"Race started: {race.DisplayName}");
            NotificationUi.instance.ShowNotification(
                $"{race.DisplayName}\nRace started!\nHead to: {ActiveRun.NextCheckpointName.ToPortString()}", 15f);

            if (Plugin.Session != null)
                _ = SaveRunStartedAsync(race.Id, startedAt);
        }

        private async Task SaveRunStartedAsync(int raceId, DateTime startedAt)
        {
            string id = await SupabaseClient.StartRunAsync(Plugin.Session, raceId, startedAt);
            if (id != null && ActiveRun != null)
            {
                ActiveRun.Id = id;

                Plugin.Log.LogInfo($"Run started on Supabase. Run id: {id}");
            }
        }

        private void TryAdvanceCheckpoint(CheckpointName name)
        {
            if (name != ActiveRun.NextCheckpointName) return;

            ActiveRun.NextCheckpointIndex++;

            if (ActiveRun.IsFinished)
            {
                FinishRace();
            }
            else
            {
                int reached = ActiveRun.NextCheckpointIndex;
                int total = ActiveRun.Race.RouteCheckpoints.Length - 1;
                Plugin.Log.LogInfo($"Checkpoint {reached}/{total}: {name}");
                NotificationUi.instance.ShowNotification(
                    $"Checkpoint {reached} / {total}\n{name.ToPortString()}\nHead to: {ActiveRun.NextCheckpointName.ToPortString()}", 15f);
            }
        }

        private void FinishRace()
        {
            string raceName   = ActiveRun.Race.DisplayName;
            string runId      = ActiveRun.Id;
            int?   boatTypeId = ActiveRun.BoatTypeId;
            var    finishedAt = DateTime.UtcNow;
            int    duration   = (int)ActiveRun.ElapsedSeconds;

            Plugin.Log.LogInfo($"Race finished: {raceName} in {duration}s");
            NotificationUi.instance.ShowNotification(
                $"{raceName}\nFinished in {duration}s!", 15f);

            ActiveRun = null;

            if (Plugin.Session != null && runId != null)
                _ = SaveRunFinishedAsync(runId, finishedAt, duration, boatTypeId);
        }

        private async Task SaveRunFinishedAsync(string runId, DateTime finishedAt, int duration, int? boatTypeId)
        {
            await SupabaseClient.FinishRunAsync(runId, finishedAt, duration, boatTypeId);
            Plugin.Log.LogInfo($"Run finished on Supabase. Run id: {runId}");
        }

        internal void AbortRace(string reason)
        {
            if (ActiveRun == null)
                return;

            string runId     = ActiveRun.Id;
            var    abortedAt = DateTime.UtcNow;

            Plugin.Log.LogInfo($"Race aborted: {reason}");
            NotificationUi.instance.ShowNotification($"Race aborted\n{reason}", 15f);
            ActiveRun = null;

            if (Plugin.Session != null && runId != null)
                _ = SaveRunAbortedAsync(runId, abortedAt);
        }

        private async Task SaveRunAbortedAsync(string runId, DateTime abortedAt)
        {
            await SupabaseClient.AbortRunAsync(runId, abortedAt);
            Plugin.Log.LogInfo($"Run aborted on Supabase. Run id: {runId}");
        }
    }
}

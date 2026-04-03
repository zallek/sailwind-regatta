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

        // Ports that need a trigger injected, keyed by port name.
        // Built from all unique port names across all races.
        private readonly HashSet<string> _racePortNames = new HashSet<string>();

        private void Awake()
        {
            Instance = this;

            foreach (var race in RaceRegistry.Races)
                foreach (var name in race.PortNames)
                    _racePortNames.Add(name);
        }

        // Called by the Port.Start() patch when any port initialises.
        // Injects a trigger collider if this port is part of a race.
        internal void TryInjectTrigger(Port port)
        {
            if (!_racePortNames.Contains(port.GetPortName())) return;

            // Avoid duplicates if the scene reloads (old components are destroyed with it).
            if (port.GetComponent<PortArrivalTrigger>() != null) return;

            var col = port.gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 800f;

            var trigger = port.gameObject.AddComponent<PortArrivalTrigger>();
            trigger.Port = port;

            Plugin.Log.LogInfo($"Trigger injected on port: {port.GetPortName()}");
        }

        // Called by PortArrivalTrigger when the player enters a port zone.
        internal void OnPlayerEnteredPort(Port port)
        {
            if (!GameState.playing) return;

            var portName = port.GetPortName();

            if (ActiveRun == null)
            {
                TryStartRace(portName);
            }
            else
            {
                TryAdvanceCheckpoint(portName);
            }
        }

        private void TryStartRace(string portName)
        {
            var race = RaceRegistry.Races.Find(r => r.StartPortName == portName);
            if (race == null) return;

            var startedAt = DateTime.UtcNow;
            ActiveRun = new Run(race, GameState.day, startedAt);

            Plugin.Log.LogInfo($"Race started: {race.DisplayName}");
            NotificationUi.instance.ShowNotification(
                $"{race.DisplayName}\nRace started!\nHead to: {ActiveRun.NextPortName}", 15f);

            if (Plugin.Session != null)
                _ = SaveRunStartedAsync(race.Id, startedAt);
        }

        private async Task SaveRunStartedAsync(int raceId, DateTime startedAt)
        {
            string id = await SupabaseClient.StartRunAsync(Plugin.Session, raceId, startedAt);
            if (id != null && ActiveRun != null)
            {
                ActiveRun.Id = id;
                SaveManager.Save();
                Plugin.Log.LogInfo($"Run started on Supabase. Run id: {id}");
            }
        }

        private void TryAdvanceCheckpoint(string portName)
        {
            if (portName != ActiveRun.NextPortName) return;

            ActiveRun.NextCheckpointIndex++;

            if (ActiveRun.IsFinished)
            {
                FinishRace();
            }
            else
            {
                int reached = ActiveRun.NextCheckpointIndex;
                int total = ActiveRun.Race.CheckpointPortNames.Length - 1;
                Plugin.Log.LogInfo($"Checkpoint {reached}/{total}: {portName}");
                NotificationUi.instance.ShowNotification(
                    $"Checkpoint {reached} / {total}\n{portName}\nHead to: {ActiveRun.NextPortName}", 15f);
            }
        }

        private void FinishRace()
        {
            string raceName  = ActiveRun.Race.DisplayName;
            string runId   = ActiveRun.Id;
            var    finishedAt = DateTime.UtcNow;
            int    duration  = (int)(finishedAt - ActiveRun.StartedAt).TotalSeconds;

            Plugin.Log.LogInfo($"Race finished: {raceName} in {duration}s");
            NotificationUi.instance.ShowNotification(
                $"{raceName}\nFinished in {duration}s!", 15f);

            ActiveRun = null;
            SaveManager.Save();

            if (Plugin.Session != null && runId != null)
                _ = SaveRunFinishedAsync(runId, finishedAt, duration);
        }

        private async Task SaveRunFinishedAsync(string runId, DateTime finishedAt, int duration)
        {
            await SupabaseClient.FinishRunAsync(runId, finishedAt, duration);
            SaveManager.Save();
            Plugin.Log.LogInfo($"Run finished on Supabase. Run id: {runId}");
        }
    }
}

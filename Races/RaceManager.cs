using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SailwindRegatta
{
    internal class RaceManager : MonoBehaviour
    {
        public static RaceManager Instance { get; private set; }

        // Set by SaveLoadPatches when a save is loaded.
        internal ActiveRace ActiveRace { get; set; }

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

            if (ActiveRace == null)
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

            ActiveRace = new ActiveRace(race, GameState.day, Sun.sun.globalTime);

            Plugin.Log.LogInfo($"Race started: {race.DisplayName}");
            NotificationUi.instance.ShowNotification(
                $"{race.DisplayName}\nRace started!\nHead to: {ActiveRace.NextPortName}", 15f);
        }

        private void TryAdvanceCheckpoint(string portName)
        {
            if (portName != ActiveRace.NextPortName) return;

            ActiveRace.NextCheckpointIndex++;

            if (ActiveRace.IsFinished)
            {
                FinishRace();
            }
            else
            {
                int reached = ActiveRace.NextCheckpointIndex;
                int total = ActiveRace.Definition.CheckpointPortNames.Length - 1;
                Plugin.Log.LogInfo($"Checkpoint {reached}/{total}: {portName}");
                NotificationUi.instance.ShowNotification(
                    $"Checkpoint {reached} / {total}\n{portName}\nHead to: {ActiveRace.NextPortName}", 15f);
            }
        }

        private void FinishRace()
        {
            float elapsed = ActiveRace.ElapsedGameHours(GameState.day, Sun.sun.globalTime);
            string raceName = ActiveRace.Definition.DisplayName;

            Plugin.Log.LogInfo($"Race finished: {raceName} in {elapsed:F1} game hours");
            NotificationUi.instance.ShowNotification(
                $"{raceName}\nFinished! {elapsed:F1} game hours", 15f);

            ActiveRace = null;
        }
    }

    // Patch Port.Start() to inject arrival triggers as island scenes load.
    [HarmonyPatch(typeof(Port), "Start")]
    internal class PortStartPatch
    {
        static void Postfix(Port __instance)
        {
            RaceManager.Instance?.TryInjectTrigger(__instance);
        }
    }
}

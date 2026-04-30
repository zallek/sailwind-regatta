using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace SailwindRegatta
{
    internal class RaceManager : MonoBehaviour
    {
        public static RaceManager Instance { get; private set; }

        // Set by SaveLoadPatches when a save is loaded.
        internal Run ActiveRun { get; set; }

        private Vector3 _lastPlayerPosition;
        private Vector3 _lastOriginOffset;
        private bool _positionInitialized;

        private GUIStyle _timerStyle;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void OnGUI()
        {
            if (ActiveRun == null || !Plugin.ShowTimer.Value)
                return;

            if (_timerStyle == null)
            {
                _timerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleRight,
                    normal = { textColor = Color.white },
                };
            }

            long minutes = (long)(ActiveRun.ElapsedHours * 60.0);
            string text = $"{ActiveRun.Race.DisplayName}\n{TimeUtils.FormatDuration(minutes)}";
            float width = 220f;
            float height = 50f;
            GUI.Label(new Rect(Screen.width - width - 12f, Screen.height - height - 12f, width, height), text, _timerStyle);
        }

        private void Update()
        {
            try
            {
                CheckTeleport();
                CheckTimescale();
                UpdateElapsedHours();
            }
            catch (Exception e)
            {
                AbortRace("Error while updating race timer");
                Plugin.Log.LogError($"Error while updating race timer: {e.Message}");
            }
        }

        private void UpdateElapsedHours()
        {
            // Mirror the Sun clock: Time.deltaTime * timescale = in-game hours per frame.
            // This ensures sleep fast-forward is counted fairly — the timer advances
            // proportionally to the in-game hours that pass, not real wall-clock time.
            if (ActiveRun != null && !Sun.SunPaused())
                ActiveRun.ElapsedHours += Time.deltaTime * Sun.sun.timescale;
        }

        private void CheckTeleport()
        {
            if (ActiveRun == null)
                return;

            // Several game events legitimately move the player large distances in a single
            // frame. We guard against each one to avoid false positives:
            //
            // - ovrCameraRig / FloatingOriginManager not ready:
            //     Happens on the very first frames after scene load before all managers
            //     initialize. Skip and reset so we re-seed once everything is stable.
            //
            // - GameState.sleeping:
            //     When the player sleeps in a port or on a boat the game fast-forwards time
            //     (Time.timeScale = 16, Sun.timescale *= 9). The CharacterController may be
            //     repositioned as the boat drifts. Not a teleport.
            //
            // - GameState.currentlyLoading / justStarted:
            //     Save loading restores the player to their saved world position in one frame,
            //     which can be hundreds of metres from the previous position. Not a teleport.
            //
            // Camera position instead of CharacterController position:
            //     Using ovrCameraRig instead of charController is the key insight. When the
            //     player embarks or disembarks a boat, PlayerEmbarkerNew reparents the
            //     CharacterController to/from boat.walkCol across several frames, causing its
            //     world-space coordinates to jump discontinuously (we measured ~293 m).
            //     The camera rig is never reparented — it always sits in the same hierarchy —
            //     so its position only ever changes by what the player actually moved.
            //
            // FloatingOriginManager origin compensation:
            //     FloatingOriginManager periodically recenters the world by translating every
            //     child of shiftingWorld (including the player and all boats) by a shift
            //     vector, then accumulates that shift in outCurrentOffset. Without compensation
            //     this shift looks like a large teleport. Subtracting the delta of
            //     outCurrentOffset isolates true player movement from world recentering.
            //     This also covers the "transition between port sea and open sea" zones, which
            //     trigger a recentering when the player crosses the boundary.
            if (
                Refs.ovrCameraRig == null
                || FloatingOriginManager.instance == null
                || GameState.sleeping
                || GameState.currentlyLoading
                || GameState.justStarted
            )
            {
                _positionInitialized = false;
                return;
            }

            Vector3 currentPosition = Refs.ovrCameraRig.transform.position;
            Vector3 currentOriginOffset = FloatingOriginManager.instance.outCurrentOffset;

            if (!_positionInitialized)
            {
                _lastPlayerPosition = currentPosition;
                _lastOriginOffset = currentOriginOffset;
                _positionInitialized = true;
                return;
            }

            Vector3 originDelta = currentOriginOffset - _lastOriginOffset;
            float distanceMoved = Vector3.Distance(currentPosition, _lastPlayerPosition + originDelta);

            _lastPlayerPosition = currentPosition;
            _lastOriginOffset = currentOriginOffset;

            if (distanceMoved > 100f)
            {
                Plugin.Log.LogWarning($"Teleport detected: {distanceMoved:F1}m in one frame. Aborting race.");
                AbortRace("Teleportation detected.");
            }
        }

        internal void ResetPositionTracking()
        {
            _positionInitialized = false;
        }

        private void CheckTimescale()
        {
            if (ActiveRun == null)
                return;

            if (!IsTimescaleValid())
            {
                Plugin.Log.LogWarning($"Timescale tampering detected: initialTimescale={Sun.sun.initialTimescale:F4}. Aborting race.");
                AbortRace("Time speed was modified.");
            }
        }

        private static bool IsTimescaleValid()
        {
            // Epsilon of 0.001f guards against floating-point drift while catching any real modification.
            return Mathf.Abs(Sun.sun.initialTimescale - 0.008f) < 0.001f;
        }

        internal void OnSteeringWheelActivated(Rudder rudder)
        {
            if (ActiveRun == null)
                return;

            int? boatTypeId = BoatTypeUtils.TryGetBoatTypeIdFromRudder(rudder);
            if (boatTypeId == null)
            {
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
            if (checkpoint == null)
                return;

            // Avoid duplicates if the scene reloads (old components are destroyed with it).
            if (port.GetComponentInChildren<CheckpointArea>() != null)
                return;

            var child = new GameObject($"CheckpointArea {checkpoint.Name.ToDisplayName()}");
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
                if (raceMaster.PortName.ToPortString() != port.GetPortName())
                    continue;

                var race = RaceRegistry.Races.Find(r => r.Id == raceMaster.RaceId);
                if (race == null)
                    continue;

                var goName = $"RaceMasterNPC {raceMaster.PortName.ToPortString()} {race.DisplayName}";
                if (port.transform.Find(goName) != null)
                    continue;
                var go = new GameObject(goName);
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
            if (!GameState.playing)
                return;

            if (ActiveRun != null)
                TryAdvanceCheckpoint(checkpoint);
        }

        // Called by RaceMasterNPC when the player clicks the NPC to start a race.
        internal void StartRace(Race race)
        {
            if (!IsTimescaleValid())
            {
                NotificationUi.instance.ShowNotification("Cannot start race\nTime speed is modified.", 8f);
                return;
            }

            var startedAt = DateTime.UtcNow;
            ActiveRun = new Run(race, GameState.day, startedAt);
            ResetPositionTracking();

            string raceName = ActiveRun.Race.DisplayName;
            string nextCheckpointName = ActiveRun.NextCheckpointName.ToDisplayName();
            Plugin.Log.LogInfo($"Race started: {raceName}");
            NotificationUi.instance.ShowNotification($"{raceName}\nRace started!\nHead to: {nextCheckpointName}", 15f);

            _ = SaveRunStartedAsync(race.Id, startedAt);
        }

        private async Task SaveRunStartedAsync(int raceId, DateTime startedAt)
        {
            if (Plugin.Session == null)
                return;

            string runId = await SupabaseClient.StartRunAsync(Plugin.Session, raceId, startedAt);
            if (runId != null && ActiveRun != null)
            {
                ActiveRun.Id = runId;
                Plugin.Log.LogDebug($"Run started on Supabase. Run id: {runId}");
                await SaveRunModsAsync(runId);
            }
        }

        internal async Task TrySaveRunModsOnLoadAsync()
        {
            if (Plugin.Session == null || ActiveRun?.Id == null)
                return;

            await SaveRunModsAsync(ActiveRun.Id);
        }

        private void TryAdvanceCheckpoint(Checkpoint checkpoint)
        {
            if (checkpoint.Name != ActiveRun.NextCheckpointName)
                return;

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
                int total = ActiveRun.Race.RouteCheckpoints.Length;
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
            long duration = (long)(ActiveRun.ElapsedHours * 60.0);

            Plugin.Log.LogInfo($"Race finished: {raceName}");
            NotificationUi.instance.ShowNotification($"{raceName}\nRace finished!\n{TimeUtils.FormatDuration(duration)}", 15f);

            ActiveRun = null;

            if (runId != null)
                _ = SaveRunFinishedAsync(runId, finishedAt, duration, boatTypeId);
            else
                _ = SaveRunRetroactiveAsync(raceId, startedAt, finishedAt, duration, boatTypeId);
        }

        private async Task SaveRunFinishedAsync(string runId, DateTime finishedAt, long duration, int? boatTypeId)
        {
            if (Plugin.Session == null)
                return;

            var success = await SupabaseClient.FinishRunAsync(Plugin.Session, runId, finishedAt, duration, boatTypeId);
            if (success)
            {
                Plugin.Log.LogDebug($"Run finished on Supabase. Run id: {runId}");
                await SaveRunModsAsync(runId);
            }
        }

        private async Task SaveRunRetroactiveAsync(int raceId, DateTime startedAt, DateTime finishedAt, long duration, int? boatTypeId)
        {
            if (Plugin.Session == null)
                return;

            string runId = await SupabaseClient.StartRunAsync(Plugin.Session, raceId, startedAt);
            if (runId == null)
            {
                Plugin.Log.LogError("Retroactive run start failed; result not saved online.");
                return;
            }
            var success = await SupabaseClient.FinishRunAsync(Plugin.Session, runId, finishedAt, duration, boatTypeId);
            if (success)
            {
                Plugin.Log.LogDebug($"Run saved retroactively on Supabase. Run id: {runId}");
                await SaveRunModsAsync(runId);
            }
        }

        private static async Task SaveRunModsAsync(string runId)
        {
            string[] modGuids = BepInEx.Bootstrap.Chainloader.PluginInfos.Keys.ToArray();
            await SupabaseClient.SaveRunModsAsync(runId, modGuids);
            Plugin.Log.LogDebug($"Saved {modGuids.Length} mods for run {runId}");
        }

        internal void AbortRace(string reason = null)
        {
            if (ActiveRun == null)
                return;

            string runId = ActiveRun.Id;
            var abortedAt = DateTime.UtcNow;
            string raceName = ActiveRun.Race.DisplayName;
            Plugin.Log.LogInfo($"Race aborted: {raceName} - {reason}");

            var notificationMessage = $"{raceName}\nRace aborted";
            if (!string.IsNullOrWhiteSpace(reason))
            {
                notificationMessage += $"\n{reason}";
            }
            NotificationUi.instance.ShowNotification(notificationMessage, 10f);
            ActiveRun = null;

            if (runId != null)
                _ = SaveRunAbortedAsync(runId, abortedAt);
        }

        private async Task SaveRunAbortedAsync(string runId, DateTime abortedAt)
        {
            if (Plugin.Session == null)
                return;

            var success = await SupabaseClient.AbortRunAsync(Plugin.Session, runId, abortedAt);
            if (success)
                Plugin.Log.LogDebug($"Run aborted on Supabase. Run id: {runId}");
        }
    }
}

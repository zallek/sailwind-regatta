using System;

namespace SailwindRegatta
{
    internal class Run
    {
        public Race Race { get; }

        // Index into Race.RouteCheckpoints — the checkpoint the player must visit next.
        public int NextCheckpointIndex { get; set; }

        public int StartDay { get; }

        // Real-world UTC time when the race started. Used to compute run duration.
        public DateTime StartedAt { get; internal set; }

        // Supabase row id (UUID string). Null until StartRunAsync resolves.
        // Persisted in save data so the run can be finished after a game restart.
        public string Id { get; set; }

        // SaveableObject.sceneIndex on the hull; null until first steering-wheel use this run.
        public int? BoatTypeId { get; set; }

        // Accumulated real-world play time in seconds. Excludes paused and closed-game time.
        public float ElapsedSeconds { get; set; }

        public CheckpointName NextCheckpointName => Race.RouteCheckpoints[NextCheckpointIndex];
        public bool IsFinished => NextCheckpointIndex >= Race.RouteCheckpoints.Length;

        public Run(Race race, int startDay, DateTime startedAt)
        {
            Race = race;
            NextCheckpointIndex = 0;
            StartDay = startDay;
            StartedAt = startedAt;
        }
    }
}

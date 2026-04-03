using System;

namespace SailwindRegatta
{
    internal class Run
    {
        public Race Race { get; }

        // Index into Race.CheckpointPortNames — the port the player must visit next.
        public int NextCheckpointIndex { get; set; }

        public int StartDay { get; }

        // Real-world UTC time when the race started. Used to compute run duration.
        public DateTime StartedAt { get; internal set; }

        // Supabase row id (UUID string). Null until StartRunAsync resolves.
        // Persisted in save data so the run can be finished after a game restart.
        public string Id { get; set; }

        public string NextPortName => Race.CheckpointPortNames[NextCheckpointIndex];
        public bool IsFinished => NextCheckpointIndex >= Race.CheckpointPortNames.Length;

        public Run(Race race, int startDay, DateTime startedAt)
        {
            Race = race;
            NextCheckpointIndex = 0;
            StartDay = startDay;
            StartedAt = startedAt;
        }
    }
}

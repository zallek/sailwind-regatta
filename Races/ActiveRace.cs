namespace SailwindRegatta
{
    internal class ActiveRace
    {
        public RaceDefinition Definition { get; }

        // Index into Definition.CheckpointPortNames — the port the player must visit next.
        public int NextCheckpointIndex { get; set; }

        public int StartDay { get; }
        public float StartGameTime { get; }  // Sun.sun.globalTime (0–24) at race start

        public string NextPortName => Definition.CheckpointPortNames[NextCheckpointIndex];
        public bool IsFinished => NextCheckpointIndex >= Definition.CheckpointPortNames.Length;

        public ActiveRace(RaceDefinition definition, int startDay, float startGameTime)
        {
            Definition = definition;
            NextCheckpointIndex = 0;
            StartDay = startDay;
            StartGameTime = startGameTime;
        }

        // Returns elapsed in-game hours since the race started.
        public float ElapsedGameHours(int currentDay, float currentGameTime)
        {
            return (currentDay - StartDay) * 24f + (currentGameTime - StartGameTime);
        }
    }
}

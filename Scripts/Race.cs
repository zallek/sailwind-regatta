using System.Linq;

namespace SailwindRegatta
{
    internal class Race
    {
        public int Id { get; }
        public string DisplayName { get; }

        // Full ordered route: [startCheckpoint, checkpoint1, ..., finishCheckpoint]
        // For a circuit race the first and last entry target the same checkpoint name.
        public Checkpoint[] Checkpoints { get; }

        // Everything after the start: intermediate checkpoints + finish destination.
        public Checkpoint[] RouteCheckpoints => Checkpoints.Skip(1).ToArray();

        public Race(int id, string displayName, Checkpoint[] checkpoints)
        {
            Id = id;
            DisplayName = displayName;
            Checkpoints = checkpoints;
        }
    }
}

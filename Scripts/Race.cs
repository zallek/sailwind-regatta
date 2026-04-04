using System.Linq;

namespace SailwindRegatta
{
    internal class Race
    {
        public int Id { get; }
        public string DisplayName { get; }

        // Full ordered route: [start, checkpoint1, ..., finish]
        // For a circuit race the first and last entry are the same checkpoint name.
        public CheckpointName[] Checkpoints { get; }

        // Everything after the start: intermediate checkpoints + finish destination.
        public CheckpointName[] RouteCheckpoints => Checkpoints.Skip(1).ToArray();

        public Race(int id, string displayName, CheckpointName[] checkpoints)
        {
            Id = id;
            DisplayName = displayName;
            Checkpoints = checkpoints;
        }
    }
}

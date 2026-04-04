using System.Linq;

namespace SailwindRegatta
{
    internal class Race
    {
        public int Id { get; }
        public string DisplayName { get; }

        // Full ordered route: [startCheckpoint, checkpoint1, ..., finishCheckpoint]
        // For a circuit race the first and last entry target the same port name.
        public RaceCheckpoint[] Checkpoints { get; }

        public string StartPortName => Checkpoints[0].PortName;

        // Everything after the start: intermediate checkpoints + finish destination.
        public RaceCheckpoint[] RouteCheckpoints => Checkpoints.Skip(1).ToArray();

        public Race(int id, string displayName, RaceCheckpoint[] checkpoints)
        {
            Id = id;
            DisplayName = displayName;
            Checkpoints = checkpoints;
        }
    }
}

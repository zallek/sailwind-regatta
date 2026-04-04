using System.Collections.Generic;
using UnityEngine;

namespace SailwindRegatta
{
    // Single source of truth for checkpoint physical config (position offset and detection radius).
    // RaceManager reads this to know where to place and size each CheckpointArea.
    internal static class CheckpointRegistry
    {
        public static readonly List<Checkpoint> Checkpoints = new List<Checkpoint>
        {
            new Checkpoint(CheckpointName.GoldRockCity, PortName.GoldRockCity, radius: 100f, offset: new Vector3(100, 0, -20)),
            new Checkpoint(CheckpointName.FortAestrin, PortName.FortAestrin),
            new Checkpoint(CheckpointName.DragonCliffs, PortName.DragonCliffs),
        };
    }
}

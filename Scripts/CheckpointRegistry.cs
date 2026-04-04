using System.Collections.Generic;
using UnityEngine;

namespace SailwindRegatta
{
    // Single source of truth for checkpoint physical config (position offset and detection radius).
    // RaceManager reads this to know where to place and size each CheckpointArea.
    internal static class CheckpointRegistry
    {
        public static readonly Dictionary<CheckpointName, Checkpoint> Checkpoints =
            new Dictionary<CheckpointName, Checkpoint>
            {
                { CheckpointName.GoldRockCity, new Checkpoint(CheckpointName.GoldRockCity, radius: 200f, offset: new Vector3(100, 0, -20)) },
                { CheckpointName.FortAestrin,  new Checkpoint(CheckpointName.FortAestrin) },
                { CheckpointName.DragonCliffs, new Checkpoint(CheckpointName.DragonCliffs) },
            };
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace SailwindRegatta
{
    // Single source of truth for Race Master NPC placement.
    // RaceManager reads this to know where to inject RaceMasterNPC components at runtime.
    internal static class RaceMasterRegistry
    {
        public static readonly List<RaceMasterConfig> NPCs = new List<RaceMasterConfig>
        {
            // Position is relative to the port transform — needs in-game tuning.
            new RaceMasterConfig(
                port:        CheckpointName.GoldRockCity,
                position:    new Vector3(5f, 0f, -5f),
                eulerAngles: new Vector3(0f, 180f, 0f)
            ),
        };
    }
}

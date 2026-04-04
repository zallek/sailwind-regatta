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
            // position/eulerAngles are relative to the port transform — tune in-game as needed.
            // avatar: index into Port.ports[] — determines which port's dude mesh is cloned.
            new RaceMasterConfig(
                port:        CheckpointName.GoldRockCity,
                position:    new Vector3(5f, -0.78f, -5f),
                eulerAngles: new Vector3(0f, 30f, 0f),
                avatar:      2
            ),
        };
    }
}

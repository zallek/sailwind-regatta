using System.Collections.Generic;
using UnityEngine;

namespace SailwindRegatta
{
    // Single source of truth for Race Scroll spawn points.
    // RaceManager reads this to inject RaceScrollSpawner components at runtime.
    internal static class RaceScrollRegistry
    {
        public static readonly List<RaceScrollConfig> Scrolls = new List<RaceScrollConfig>
        {
            // position/eulerAngles are relative to the port transform — tune in-game as needed.
            new RaceScrollConfig(
                portName:    PortName.GoldRockCity,
                raceId:      1,
                position:    new Vector3(5f, -0.78f, -3.5f),
                eulerAngles: new Vector3(0f, 0f, 0f)
            )
        };
    }
}

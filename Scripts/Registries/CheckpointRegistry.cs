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
            // Al'Ankh
            new Checkpoint(CheckpointName.GoldRockCity, PortName.GoldRockCity, radius: 150f, offset: new Vector3(140f, 0, -50f)), // 1
            // Aestrin
            new Checkpoint(CheckpointName.FortAestrin, PortName.FortAestrin, radius: 150f, offset: new Vector3(80f, 0, 100f)), // 15
            new Checkpoint(CheckpointName.FireflyGrotto, PortName.FireflyGrotto, radius: 80f, offset: new Vector3(0, 0, 80f)),
            new Checkpoint(CheckpointName.FeyValley, PortName.FeyValley, radius: 100f, offset: new Vector3(0, 0, 50f)),
            new Checkpoint(CheckpointName.SirenSong, PortName.SirenSong, radius: 80f, offset: new Vector3(-80f, 0, 0)), // 21
            new Checkpoint(CheckpointName.Eastwind, PortName.Eastwind, radius: 60f), // 19
            new Checkpoint(CheckpointName.Sunspire, PortName.Sunspire, radius: 100f), // 16
            // Emerald
            new Checkpoint(CheckpointName.DragonCliffs, PortName.DragonCliffs, radius: 150f, offset: new Vector3(0, 0, 100f)), // 9
        };
    }
}

using System.Collections.Generic;

namespace SailwindRegatta
{
    internal static class RaceRegistry
    {
        public static readonly List<Race> Races = new List<Race>
        {
            new Race(
                id: 1,
                displayName: "The Grand Tour",
                checkpoints: new[]
                {
                    CheckpointName.GoldRockCity,
                    CheckpointName.FortAestrin,
                    CheckpointName.DragonCliffs,
                    CheckpointName.GoldRockCity,
                }
            ),
            new Race(
                id: 2,
                displayName: "Aestrin Shores",
                checkpoints: new[]
                {
                    CheckpointName.FortAestrin,
                    CheckpointName.FireflyGrotto,
                    CheckpointName.FeyValley,
                    CheckpointName.SirenSong,
                    CheckpointName.Eastwind,
                    CheckpointName.Sunspire,
                    CheckpointName.FortAestrin,
                }
            ),
        };

        public static Race GetById(int id)
        {
            return Races.Find(r => r.Id == id);
        }
    }
}

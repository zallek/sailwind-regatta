using System.Collections.Generic;

namespace SailwindRegatta
{
    internal static class RaceRegistry
    {
        public static readonly List<Race> Races = new List<Race>
        {
            new Race(
                id: 1,
                displayName: "The Capital Circuit",
                checkpoints: new[]
                {
                    CheckpointName.GoldRockCity,
                    CheckpointName.FortAestrin,
                    CheckpointName.DragonCliffs,
                    CheckpointName.GoldRockCity,
                }
            ),
        };

        public static Race GetById(int id)
        {
            return Races.Find(r => r.Id == id);
        }
    }
}

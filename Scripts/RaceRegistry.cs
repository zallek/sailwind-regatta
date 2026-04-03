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
                portNames: new[] { "Gold Rock City", "Fort Aestrin", "Dragon Cliffs", "Gold Rock City" }
            )
        };

        public static Race GetById(int id)
        {
            return Races.Find(r => r.Id == id);
        }
    }
}

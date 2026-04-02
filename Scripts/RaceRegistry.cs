using System.Collections.Generic;

namespace SailwindRegatta
{
    internal static class RaceRegistry
    {
        public static readonly List<RaceDefinition> Races = new List<RaceDefinition>
        {
            new RaceDefinition(
                id: 1,
                displayName: "The Capital Circuit",
                portNames: new[] { "Gold Rock City", "Fort Aestrin", "Dragon Cliffs", "Gold Rock City" }
            )
        };

        public static RaceDefinition GetById(int id)
        {
            return Races.Find(r => r.Id == id);
        }
    }
}

using System.Linq;

namespace SailwindRegatta
{
    internal class RaceDefinition
    {
        public int Id { get; }
        public string DisplayName { get; }

        // Full ordered route: [startPort, checkpoint1, ..., finishPort]
        // For a circuit race the first and last entry are the same port name.
        /*
        Aestra Abbey
        Al'Ankh Academy
        Al'Nilem
        Albacore Town
        Alchemist's Island
        Chronos
        Crab Beach
        Dead Cove
        Dragon Cliffs
        Eastwind
        Fey Valley
        Fire Fish Town
        Firefly Grotto
        Fort Aestrin
        Gold Rock City
        Happy Bay
        Kicia Bay
        Mirage Mountain
        Mount Malefic
        Neverdin
        New Port
        Oasis
        Old Ankh Town
        On'na
        Saffron Island
        Sage Hills
        Sanctuary
        Sen'na
        Serpent Isle
        Siren Song
        Sunspire
        Test Port
        Turtle Island
        */
        public string[] PortNames { get; }

        public string StartPortName => PortNames[0];

        // Everything after the start: intermediate checkpoints + finish destination.
        public string[] CheckpointPortNames { get; }

        public RaceDefinition(int id, string displayName, string[] portNames)
        {
            Id = id;
            DisplayName = displayName;
            PortNames = portNames;
            CheckpointPortNames = portNames.Skip(1).ToArray();
        }
    }
}

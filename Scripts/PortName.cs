namespace SailwindRegatta
{
    internal enum PortName
    {
        AestraAbbey,
        AlAnkhAcademy,
        AlNilem,
        AlbacoreTown,
        AlchemistsIsland,
        Chronos,
        CrabBeach,
        DeadCove,
        DragonCliffs,
        Eastwind,
        FeyValley,
        FireFishTown,
        FireflyGrotto,
        FortAestrin,
        GoldRockCity,
        HappyBay,
        KiciaBay,
        MirageMountain,
        MountMalefic,
        Neverdin,
        NewPort,
        Oasis,
        OldAnkhTown,
        Onna,
        SaffronIsland,
        SageHills,
        Sanctuary,
        Senna,
        SerpentIsle,
        SirenSong,
        Sunspire,
        TestPort,
        TurtleIsland,
    }

    internal static class PortNameExtensions
    {
        internal static string ToPortString(this PortName port)
        {
            switch (port)
            {
                case PortName.AestraAbbey:      return "Aestra Abbey";
                case PortName.AlAnkhAcademy:    return "Al'Ankh Academy";
                case PortName.AlNilem:          return "Al'Nilem";
                case PortName.AlbacoreTown:     return "Albacore Town";
                case PortName.AlchemistsIsland: return "Alchemist's Island";
                case PortName.Chronos:          return "Chronos";
                case PortName.CrabBeach:        return "Crab Beach";
                case PortName.DeadCove:         return "Dead Cove";
                case PortName.DragonCliffs:     return "Dragon Cliffs";
                case PortName.Eastwind:         return "Eastwind";
                case PortName.FeyValley:        return "Fey Valley";
                case PortName.FireFishTown:     return "Fire Fish Town";
                case PortName.FireflyGrotto:    return "Firefly Grotto";
                case PortName.FortAestrin:      return "Fort Aestrin";
                case PortName.GoldRockCity:     return "Gold Rock City";
                case PortName.HappyBay:         return "Happy Bay";
                case PortName.KiciaBay:         return "Kicia Bay";
                case PortName.MirageMountain:   return "Mirage Mountain";
                case PortName.MountMalefic:     return "Mount Malefic";
                case PortName.Neverdin:         return "Neverdin";
                case PortName.NewPort:          return "New Port";
                case PortName.Oasis:            return "Oasis";
                case PortName.OldAnkhTown:      return "Old Ankh Town";
                case PortName.Onna:             return "On'na";
                case PortName.SaffronIsland:    return "Saffron Island";
                case PortName.SageHills:        return "Sage Hills";
                case PortName.Sanctuary:        return "Sanctuary";
                case PortName.Senna:            return "Sen'na";
                case PortName.SerpentIsle:      return "Serpent Isle";
                case PortName.SirenSong:        return "Siren Song";
                case PortName.Sunspire:         return "Sunspire";
                case PortName.TestPort:         return "Test Port";
                case PortName.TurtleIsland:     return "Turtle Island";
                default:                        return port.ToString();
            }
        }
    }
}

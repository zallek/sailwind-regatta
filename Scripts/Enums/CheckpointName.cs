namespace SailwindRegatta
{
    internal enum CheckpointName
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
        TurtleIsland,
    }

    internal static class CheckpointNameExtensions
    {
        internal static string ToDisplayName(this CheckpointName name)
        {
            switch (name)
            {
                case CheckpointName.AestraAbbey:
                    return "Aestra Abbey";
                case CheckpointName.AlAnkhAcademy:
                    return "Al'Ankh Academy";
                case CheckpointName.AlNilem:
                    return "Al'Nilem";
                case CheckpointName.AlbacoreTown:
                    return "Albacore Town";
                case CheckpointName.AlchemistsIsland:
                    return "Alchemist's Island";
                case CheckpointName.Chronos:
                    return "Chronos";
                case CheckpointName.CrabBeach:
                    return "Crab Beach";
                case CheckpointName.DeadCove:
                    return "Dead Cove";
                case CheckpointName.DragonCliffs:
                    return "Dragon Cliffs";
                case CheckpointName.Eastwind:
                    return "Eastwind";
                case CheckpointName.FeyValley:
                    return "Fey Valley";
                case CheckpointName.FireFishTown:
                    return "Fire Fish Town";
                case CheckpointName.FireflyGrotto:
                    return "Firefly Grotto";
                case CheckpointName.FortAestrin:
                    return "Fort Aestrin";
                case CheckpointName.GoldRockCity:
                    return "Gold Rock City";
                case CheckpointName.HappyBay:
                    return "Happy Bay";
                case CheckpointName.KiciaBay:
                    return "Kicia Bay";
                case CheckpointName.MirageMountain:
                    return "Mirage Mountain";
                case CheckpointName.MountMalefic:
                    return "Mount Malefic";
                case CheckpointName.Neverdin:
                    return "Neverdin";
                case CheckpointName.NewPort:
                    return "New Port";
                case CheckpointName.Oasis:
                    return "Oasis";
                case CheckpointName.OldAnkhTown:
                    return "Old Ankh Town";
                case CheckpointName.Onna:
                    return "On'na";
                case CheckpointName.SaffronIsland:
                    return "Saffron Island";
                case CheckpointName.SageHills:
                    return "Sage Hills";
                case CheckpointName.Sanctuary:
                    return "Sanctuary";
                case CheckpointName.Senna:
                    return "Sen'na";
                case CheckpointName.SerpentIsle:
                    return "Serpent Isle";
                case CheckpointName.SirenSong:
                    return "Siren Song";
                case CheckpointName.Sunspire:
                    return "Sunspire";
                case CheckpointName.TurtleIsland:
                    return "Turtle Island";
                default:
                    return name.ToString();
            }
        }
    }
}

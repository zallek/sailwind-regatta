namespace SailwindRegatta
{
    internal enum CheckpointName
    {
        DragonCliffs,
        Eastwind,
        FeyValley,
        FortAestrin,
        FireflyGrotto,
        GoldRockCity,
        SirenSong,
        Sunspire,
    }

    internal static class CheckpointNameExtensions
    {
        internal static string ToDisplayName(this CheckpointName name)
        {
            switch (name)
            {
                case CheckpointName.DragonCliffs:
                    return "Dragon Cliffs";
                case CheckpointName.Eastwind:
                    return "Eastwind";
                case CheckpointName.FeyValley:
                    return "Fey Valley";
                case CheckpointName.FireflyGrotto:
                    return "Firefly Grotto";
                case CheckpointName.FortAestrin:
                    return "Fort Aestrin";
                case CheckpointName.GoldRockCity:
                    return "Gold Rock City";
                case CheckpointName.SirenSong:
                    return "Siren Song";
                case CheckpointName.Sunspire:
                    return "Sunspire";
                default:
                    return name.ToString();
            }
        }
    }
}

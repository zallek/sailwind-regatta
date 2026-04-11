namespace SailwindRegatta
{
    internal static class TimeUtils
    {
        internal static string FormatDuration(long seconds)
        {
            if (Plugin.UseInGameTime.Value)
            {
                float inGameHours = seconds * Sun.sun.timescale;
                int days = (int)(inGameHours / 24f);
                int hours = (int)(inGameHours % 24f);
                int minutes = (int)((inGameHours % 1f) * 60f);
                return $"{days}d {hours:D2}h {minutes:D2}m";
            }
            int m = (int)(seconds / 60);
            int s = (int)(seconds % 60);
            return $"{m}:{s:D2}";
        }
    }
}

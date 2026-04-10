namespace SailwindRegatta
{
    internal static class TimeUtils
    {
        internal static string FormatDuration(int seconds)
        {
            if (Plugin.UseInGameTime.Value)
            {
                float inGameHours = seconds * Sun.sun.timescale;
                int days = (int)(inGameHours / 24f);
                int hours = (int)(inGameHours % 24f);
                int minutes = (int)((inGameHours % 1f) * 60f);
                return $"{days} days {hours} hours {minutes} minutes";
            }
            int m = seconds / 60;
            int s = seconds % 60;
            return $"{m}:{s:D2}";
        }
    }
}

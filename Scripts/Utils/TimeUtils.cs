namespace SailwindRegatta
{
    internal static class TimeUtils
    {
        /// <summary>Formats an in-game duration (minutes) as "Xd XXh XXm".</summary>
        internal static string FormatDuration(long minutes)
        {
            long days = minutes / (24 * 60);
            long hours = (minutes / 60) % 24;
            long mins = minutes % 60;
            return $"{days}d {hours:D2}h {mins:D2}m";
        }
    }
}

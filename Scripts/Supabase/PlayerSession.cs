namespace SailwindRegatta
{
    internal class PlayerSession
    {
        // Stored in save data so run finish can be PATCHed after a game restart.
        internal string PlayerUuid { get; }

        internal PlayerSession(string playerUuid)
        {
            PlayerUuid = playerUuid;
        }
    }
}

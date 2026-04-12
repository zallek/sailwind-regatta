using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace SailwindRegatta
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.sailwindregatta.mod";
        public const string PLUGIN_NAME = "Sailwind Regatta";
        public const string PLUGIN_VERSION = "0.1.0";

        internal static Plugin Instance { get; private set; }
        internal static ManualLogSource Log { get; private set; }
        internal static PlayerSession Session { get; set; }
        internal static ConfigEntry<bool> ShowTimer { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Log = Logger;

            ShowTimer = Config.Bind("Display", "ShowTimer", true, "Show the active race timer in the bottom-right corner of the screen.");

            // Required for HTTPS on Mono/.NET 4.8 in Unity.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PLUGIN_GUID);

            gameObject.AddComponent<RaceManager>();

            _ = InitOnlineSessionAsync();
        }

        private static async Task InitOnlineSessionAsync()
        {
            try
            {
                var steamUser = SteamUtils.GetCurrentUser();
                if (steamUser == null)
                {
                    Log.LogWarning("Could not retrieve Steam user. Online mode disabled.");
                    return;
                }

                var playerSession = await SupabaseClient.UpsertPlayerAsync(steamUser);
                if (playerSession == null)
                {
                    Log.LogWarning("Could not create online session. Online mode disabled.");
                    return;
                }

                Session = playerSession;
                Log.LogInfo($"Online session initialized. Player name: {steamUser.PersonaName}");
            }
            catch (System.Exception ex)
            {
                Log.LogError($"InitOnlineSessionAsync exception: {ex.Message}");
            }
        }
    }
}

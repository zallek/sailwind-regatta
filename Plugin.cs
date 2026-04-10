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
        internal static ConfigEntry<bool> ShowCheckpointZones { get; private set; }
        internal static ConfigEntry<bool> DevMode { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Log = Logger;

            DevMode = Config.Bind("Dev", "DevMode", false,
                "When true, runs are saved under a separate dev player (name suffixed with '-dev', different key salt). Keeps dev runs off the main leaderboard.");
            ShowCheckpointZones = Config.Bind("Dev", "ShowCheckpointZones", false,
                "Render checkpoint detection zones as semi-transparent red spheres.");

            // Required for HTTPS on Mono/.NET 4.8 in Unity.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PLUGIN_GUID);

            gameObject.AddComponent<RaceManager>();

            _ = InitOnlineSessionAsync();

            Log.LogInfo($"{PLUGIN_NAME} v{PLUGIN_VERSION} loaded.");
        }

        private static async Task InitOnlineSessionAsync()
        {
            var steamUser = SteamUtils.GetCurrentUser();
            if (steamUser == null) {
                Log.LogWarning("Could not retrieve Steam user. Online mode disabled.");
                return;
            }

            var playerUuid = await SupabaseClient.UpsertPlayerAsync(steamUser);
            if (playerUuid == null) {
                Log.LogWarning("Could not create online player. Online mode disabled.");
                return;
            }
        
            Session = new PlayerSession(playerUuid);
            Log.LogInfo($"Online session initialized. PlayerUUID: {Session.PlayerUuid}");
        }
    }
}

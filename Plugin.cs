using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
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
        internal static SteamUser LocalPlayer { get; private set; }
        internal static PlayerSession Session { get; set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Log = Logger;

            // Required for HTTPS on Mono/.NET 4.8 in Unity.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PLUGIN_GUID);

            gameObject.AddComponent<RaceManager>();

            LocalPlayer = SteamUtils.GetCurrentUser();
            if (LocalPlayer != null)
            {
                _ = InitSessionAsync();
            }
            else
            {
                Log.LogWarning("Could not retrieve Steam user. Leaderboard disabled.");
            }

            Log.LogInfo($"{PLUGIN_NAME} v{PLUGIN_VERSION} loaded.");
        }

        private static async Task InitSessionAsync()
        {
            await SupabaseClient.InitPlayerAsync(LocalPlayer);
            if (Session == null)
                Log.LogWarning("Supabase player init failed. Leaderboard disabled.");
        }
    }
}

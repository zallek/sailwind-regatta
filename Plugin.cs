using System.Reflection;
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

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Log = Logger;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PLUGIN_GUID);

            gameObject.AddComponent<RaceManager>();

            LocalPlayer = SteamUtils.GetCurrentUser();
            if (LocalPlayer != null)
                Log.LogInfo($"Player: {LocalPlayer.PersonaName} (SteamId: {LocalPlayer.SteamId})");
            else
                Log.LogWarning("Could not retrieve Steam user.");

            Log.LogInfo($"{PLUGIN_NAME} v{PLUGIN_VERSION} loaded.");
        }
    }
}

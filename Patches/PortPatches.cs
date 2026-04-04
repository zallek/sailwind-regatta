using HarmonyLib;

namespace SailwindRegatta
{
    [HarmonyPatch(typeof(Port), "Start")]
    internal class PortStartPatch
    {
        static void Postfix(Port __instance)
        {
            RaceManager.Instance?.TryInjectCheckpointArea(__instance);
            RaceManager.Instance?.TryInjectRaceMasterNPC(__instance);
        }
    }
}
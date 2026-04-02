using HarmonyLib;

namespace SailwindRegatta
{
    [HarmonyPatch(typeof(Port), "Start")]
    internal class PortStartPatch
    {
        static void Postfix(Port __instance)
        {
            RaceManager.Instance?.TryInjectTrigger(__instance);
        }
    }
}
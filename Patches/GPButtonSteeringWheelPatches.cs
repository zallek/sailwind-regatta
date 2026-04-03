using HarmonyLib;

namespace SailwindRegatta
{
    [HarmonyPatch(typeof(GPButtonSteeringWheel), "OnActivate")]
    internal static class GPButtonSteeringWheelOnActivatePatch
    {
        static void Postfix(GPButtonSteeringWheel __instance)
        {
            if (__instance == null || __instance.attachedRudder == null)
                return;

            var rudder = __instance.attachedRudder.GetComponent<Rudder>();
            if (rudder == null)
                return;

            RaceManager.Instance?.OnSteeringWheelActivated(rudder);
        }
    }
}

using HarmonyLib;

namespace SailwindRegatta
{
    // All patches are null-safe: GetComponent<RaceScroll>() returns null on
    // ordinary ShipItemScrolls, so they are completely unaffected.

    [HarmonyPatch(typeof(ShipItemScroll), "OnPickup")]
    internal class ShipItemScrollOnPickupPatch
    {
        static void Postfix(ShipItemScroll __instance)
        {
            __instance.GetComponent<RaceScroll>()?.OnScrollPickup();
        }
    }

    [HarmonyPatch(typeof(ShipItemScroll), "OnDrop")]
    internal class ShipItemScrollOnDropPatch
    {
        static void Postfix(ShipItemScroll __instance)
        {
            __instance.GetComponent<RaceScroll>()?.OnScrollDrop();
        }
    }

    // Suppress ShipItemScroll's own page-flip logic for race scrolls and delegate
    // page navigation to RaceScroll instead.
    [HarmonyPatch(typeof(ShipItemScroll), "OnScroll")]
    internal class ShipItemScrollOnScrollPatch
    {
        static bool Prefix(ShipItemScroll __instance, float input)
        {
            var raceScroll = __instance.GetComponent<RaceScroll>();
            if (raceScroll == null) return true;   // allow normal scrolls through

            raceScroll.OnScroll(input);
            return false;   // skip ShipItemScroll's built-in FlipPage call
        }
    }
}

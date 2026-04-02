
using HarmonyLib;

namespace SailwindRegatta
{
    [HarmonyPatch(typeof(SaveLoadManager), "SaveModData")]
    internal class SaveModDataPatch
    {
        static void Postfix()
        {
            SaveManager.Save();
        }
    }

    [HarmonyPatch(typeof(SaveLoadManager), "LoadModData")]
    internal class LoadModDataPatch
    {
        static void Postfix()
        {
            SaveManager.Load();
        }
    }
}

using HarmonyLib;

namespace SailwindRegatta
{
    [HarmonyPatch(typeof(SaveLoadManager), "SaveModData")]
    internal class SaveLoadManagerSaveModDataPatch
    {
        static void Postfix()
        {
            SaveUtils.Save();
        }
    }

    [HarmonyPatch(typeof(SaveLoadManager), "LoadModData")]
    internal class SaveLoadManagerLoadModDataPatch
    {
        static void Postfix()
        {
            SaveUtils.Load();
        }
    }
}

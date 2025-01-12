using HarmonyLib;
namespace StartCreditsPlus.Patches.StartCredits;

[HarmonyPatch(typeof(Terminal))]
internal class TerminalPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    private static void StartPatch()
    {
        StartCredits.calculateStartGroupCredits();
    }
}
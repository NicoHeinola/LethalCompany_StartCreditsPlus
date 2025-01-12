using HarmonyLib;
namespace StartCreditsPlus.Patches.StartCredits;

[HarmonyPatch(typeof(StartOfRound))]
internal class StartOfRoundPatch
{
    [HarmonyPatch("OnClientConnect")]
    [HarmonyPostfix]
    private static void OnClientConnectPatch()
    {
        StartCredits.calculateStartGroupCredits();
    }

    [HarmonyPatch("ResetShip")]
    [HarmonyPostfix]
    private static void ResetShipPatch()
    {
        StartCredits.reset();
    }
}

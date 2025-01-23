using HarmonyLib;
namespace StartCreditsPlus.Patches.StartCredits;

[HarmonyPatch(typeof(StartOfRound))]
internal class StartOfRoundPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    private static void StartPatch()
    {
        StartCredits.loadSaveFileConfigurations();
    }

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
        StartCredits.calculateStartGroupCredits();
    }

    [HarmonyPatch("StartGame")]
    [HarmonyPrefix]
    private static void StartGamePatch()
    {
        StartCredits.calculateStartGroupCredits();
    }
}

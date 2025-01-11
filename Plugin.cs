using BepInEx;
using HarmonyLib;

namespace StartCreditsPlus;

[BepInPlugin(modGUID, modName, modVersion)]
public class StartCreditsPlusPlugin : BaseUnityPlugin
{
    private const string modGUID = "Pizza.StartCreditsPlus";
    private const string modName = "Start Credits Plus";
    private const string modVersion = "1.0.0";

    private readonly Harmony harmony = new Harmony(modGUID);

    private static StartCreditsPlusPlugin instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        harmony.PatchAll(typeof(StartCreditsPlusPlugin));
    }
}

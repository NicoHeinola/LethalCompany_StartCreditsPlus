using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using StartCreditsPlus.Patches.StartCredits;

namespace StartCreditsPlus;

[BepInPlugin(modGUID, modName, modVersion)]
public class StartCreditsPlusPlugin : BaseUnityPlugin
{
    public const string modGUID = "pizzagamer777.StartCreditsPlus";
    private const string modName = "Start Credits Plus";
    private const string modVersion = "1.2.1";

    private readonly Harmony harmony = new Harmony(modGUID);

    private static StartCreditsPlusPlugin _instance;
    internal static StartCreditsPlusPlugin Instance
    {
        get => _instance;
        set => _instance = value;
    }

    internal static ManualLogSource logger;

    internal static ConfigManager configManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        StartCreditsPlusPlugin.logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);

        StartCreditsPlusPlugin.configManager = new ConfigManager();

        harmony.PatchAll(typeof(StartCreditsPlusPlugin));
        StartCredits.patch(harmony);
    }
}

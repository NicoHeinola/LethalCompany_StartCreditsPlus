using HarmonyLib;
using UnityEngine;
namespace StartCreditsPlus.Patches.StartCredits;

internal class StartCredits
{
    private static Terminal _terminal;

    public static Terminal Terminal
    {
        get
        {
            if (_terminal == null)
            {
                _terminal = GameObject.FindObjectOfType<Terminal>();
            }

            return _terminal;
        }

        set
        {
            _terminal = value;
        }
    }

    private static bool appliedStaticCredits = false;
    private static bool appliedRandomCredits = false;
    private static int dynamicCreditsPlayerAmount = 0;

    internal static void patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(TerminalPatch));
        harmony.PatchAll(typeof(StartOfRoundPatch));
    }

    public static void loadSaveFileConfigurations()
    {
        if (StartCreditsPlusPlugin.configManager.resetOnFirstDayUponReHost.Value)
        {
            reset();
            return;
        }

        string saveKey = $"{StartCreditsPlusPlugin.modGUID}.dynamicCreditsPlayerAmount";
        dynamicCreditsPlayerAmount = ES3.Load<int>(saveKey, GameNetworkManager.Instance.currentSaveFileName, 0);

        saveKey = $"{StartCreditsPlusPlugin.modGUID}.appliedStaticCredits";
        appliedStaticCredits = ES3.Load<bool>(saveKey, GameNetworkManager.Instance.currentSaveFileName, false);

        saveKey = $"{StartCreditsPlusPlugin.modGUID}.appliedRandomCredits";
        appliedRandomCredits = ES3.Load<bool>(saveKey, GameNetworkManager.Instance.currentSaveFileName, false);
    }

    private static int getAppliedDynamicCreditPlayerAmount()
    {
        return dynamicCreditsPlayerAmount;
    }

    public static bool canModifyStartCredits()
    {
        // You need to be a host to modify the credits
        if (!GameNetworkManager.Instance.isHostingGame) return false;

        // If not the very first day.
        if (StartOfRound.Instance.gameStats.daysSpent != 0)
        {
            return false;
        }

        // If players are going to land on a planet
        if (!StartOfRound.Instance.inShipPhase)
        {
            return false;
        }

        // We use terminal to modify credits
        if (!Terminal)
        {
            StartCreditsPlusPlugin.logger.LogWarning("Terminal not found! Can't modify start credits.");
            return false;
        };

        return true;
    }

    private static bool canApplyStaticCredits()
    {
        if (!StartCreditsPlusPlugin.configManager.enableStartingCredits.Value)
        {
            return false;
        }


        return !appliedStaticCredits;
    }
    private static bool canApplyRandomCredits()
    {
        if (!StartCreditsPlusPlugin.configManager.enableRandomStartCredits.Value)
        {
            return false;
        }

        return !appliedRandomCredits;
    }

    private static bool canApplyDynamicCredits()
    {
        if (!StartCreditsPlusPlugin.configManager.enableDynamicStartCredits.Value)
        {
            return false;
        }

        // If applied already to the current players
        if (getAppliedDynamicCreditPlayerAmount() >= GameNetworkManager.Instance.connectedPlayers)
        {
            return false;
        }

        // If too little players in the lobby
        int minPlayers = StartCreditsPlusPlugin.configManager.minPlayersForDynamicCredits.Value;
        if (GameNetworkManager.Instance.connectedPlayers <= minPlayers)
        {
            return false;
        }

        // If too many players in the lobby
        int maxPlayers = StartCreditsPlusPlugin.configManager.maxPlayersForDynamicCredits.Value;
        if (GameNetworkManager.Instance.connectedPlayers > maxPlayers && maxPlayers > -1)
        {
            return false;
        }

        return true;
    }

    internal static void reset()
    {
        // Only hosts can manage credits
        if (!GameNetworkManager.Instance.isHostingGame) return;

        appliedStaticCredits = false;
        appliedRandomCredits = false;
        dynamicCreditsPlayerAmount = 0;

        ES3.Save($"{StartCreditsPlusPlugin.modGUID}.appliedStaticCredits", appliedStaticCredits, GameNetworkManager.Instance.currentSaveFileName);
        ES3.Save($"{StartCreditsPlusPlugin.modGUID}.appliedRandomCredits", appliedRandomCredits, GameNetworkManager.Instance.currentSaveFileName);
        ES3.Save($"{StartCreditsPlusPlugin.modGUID}.dynamicCreditsPlayerAmount", dynamicCreditsPlayerAmount, GameNetworkManager.Instance.currentSaveFileName);
    }

    private static int applyStaticCredits(int creditsToModify)
    {
        int startingCredits = StartCreditsPlusPlugin.configManager.startingCredits.Value;
        creditsToModify = startingCredits;

        appliedStaticCredits = true;

        ES3.Save($"{StartCreditsPlusPlugin.modGUID}.appliedStaticCredits", appliedStaticCredits, GameNetworkManager.Instance.currentSaveFileName);

        return creditsToModify;
    }

    private static int applyRandomCredits(int creditsToModify)
    {
        int minRandomStartCredits = StartCreditsPlusPlugin.configManager.minRandomStartCredits.Value;
        int maxRandomStartCredits = StartCreditsPlusPlugin.configManager.maxRandomStartCredits.Value;

        int randomCredits = Random.Range(minRandomStartCredits, maxRandomStartCredits);

        creditsToModify += randomCredits;

        appliedRandomCredits = true;

        ES3.Save($"{StartCreditsPlusPlugin.modGUID}.appliedRandomCredits", appliedRandomCredits, GameNetworkManager.Instance.currentSaveFileName);

        return creditsToModify;
    }

    private static int applyDynamicCredits(int creditsToModify)
    {
        int dynamicStartCreditIncreasePerPlayer = StartCreditsPlusPlugin.configManager.dynamicStartCreditIncreasePerPlayer.Value;
        int appliedDynamicCreditPlayerAmount = getAppliedDynamicCreditPlayerAmount();
        int playersLeftToApply = GameNetworkManager.Instance.connectedPlayers - appliedDynamicCreditPlayerAmount;
        creditsToModify += dynamicStartCreditIncreasePerPlayer * playersLeftToApply;

        dynamicCreditsPlayerAmount = appliedDynamicCreditPlayerAmount + playersLeftToApply;

        ES3.Save($"{StartCreditsPlusPlugin.modGUID}.dynamicCreditsPlayerAmount", dynamicCreditsPlayerAmount, GameNetworkManager.Instance.currentSaveFileName);

        return creditsToModify;
    }

    internal static void calculateStartGroupCredits()
    {
        if (!canModifyStartCredits())
        {
            return;
        }

        int newGroupCredits = Terminal.groupCredits;

        if (canApplyStaticCredits())
        {
            newGroupCredits = applyStaticCredits(newGroupCredits);
        }

        if (canApplyRandomCredits())
        {
            newGroupCredits = applyRandomCredits(newGroupCredits);
        }


        if (canApplyDynamicCredits())
        {
            newGroupCredits = applyDynamicCredits(newGroupCredits);
        }

        if (newGroupCredits != Terminal.groupCredits)
        {
            Terminal.SyncGroupCreditsServerRpc(newGroupCredits, Terminal.numberOfItemsInDropship);
        }
    }
}
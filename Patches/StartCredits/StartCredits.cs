using HarmonyLib;
using UnityEngine;
namespace StartCreditsPlus.Patches.StartCredits;

internal class StartCredits
{
    private static Terminal _terminal;

    private static Terminal Terminal
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

    internal static void patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(TerminalPatch));
        harmony.PatchAll(typeof(StartOfRoundPatch));
    }

    private static int getAppliedDynamicCreditPlayerAmount()
    {
        string saveKey = $"{StartCreditsPlusPlugin.modGUID}.dynamicCreditsPlayerAmount";
        int appliedDynamicCreditPlayerAmount = ES3.Load<int>(saveKey, GameNetworkManager.Instance.currentSaveFileName, 0);
        return appliedDynamicCreditPlayerAmount;
    }

    private static bool canModifyStartCredits()
    {
        // If not the very first day.
        if (StartOfRound.Instance.gameStats.daysSpent != 0)
        {
            return false;
        }

        // We use terminal to modify credits
        if (!Terminal)
        {
            StartCreditsPlusPlugin.logger.LogWarning("Terminal not found! Can't modify start credits.");
            return false;
        };

        // You need to be a host to modify the credits
        if (!GameNetworkManager.Instance.isHostingGame) return false;

        return true;
    }

    private static bool canApplyStaticCredits()
    {
        if (!StartCreditsPlusPlugin.configManager.enableStartingCredits.Value)
        {
            return false;
        }

        string saveKey = $"{StartCreditsPlusPlugin.modGUID}.appliedStaticCredits";
        bool hasAppliedCredits = ES3.Load<bool>(saveKey, GameNetworkManager.Instance.currentSaveFileName, false);

        return !hasAppliedCredits;
    }
    private static bool canApplyRandomCredits()
    {
        if (!StartCreditsPlusPlugin.configManager.enableRandomStartCredits.Value)
        {
            return false;
        }

        string saveKey = $"{StartCreditsPlusPlugin.modGUID}.appliedRandomCredits";
        bool hasAppliedCredits = ES3.Load<bool>(saveKey, GameNetworkManager.Instance.currentSaveFileName, false);

        return !hasAppliedCredits;
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

        ES3.Save($"{StartCreditsPlusPlugin.modGUID}.appliedStaticCredits", false, GameNetworkManager.Instance.currentSaveFileName);
        ES3.Save($"{StartCreditsPlusPlugin.modGUID}.appliedRandomCredits", false, GameNetworkManager.Instance.currentSaveFileName);
        ES3.Save($"{StartCreditsPlusPlugin.modGUID}.dynamicCreditsPlayerAmount", 0, GameNetworkManager.Instance.currentSaveFileName);
        calculateStartGroupCredits();
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
            int startingCredits = StartCreditsPlusPlugin.configManager.startingCredits.Value;
            newGroupCredits = startingCredits;

            ES3.Save($"{StartCreditsPlusPlugin.modGUID}.appliedStaticCredits", true, GameNetworkManager.Instance.currentSaveFileName);
        }

        if (canApplyRandomCredits())
        {
            int minRandomStartCredits = StartCreditsPlusPlugin.configManager.minRandomStartCredits.Value;
            int maxRandomStartCredits = StartCreditsPlusPlugin.configManager.maxRandomStartCredits.Value;

            int randomCredits = Random.Range(minRandomStartCredits, maxRandomStartCredits);

            newGroupCredits += randomCredits;

            ES3.Save($"{StartCreditsPlusPlugin.modGUID}.appliedRandomCredits", true, GameNetworkManager.Instance.currentSaveFileName);
        }


        if (canApplyDynamicCredits())
        {
            int dynamicStartCreditIncreasePerPlayer = StartCreditsPlusPlugin.configManager.dynamicStartCreditIncreasePerPlayer.Value;
            int appliedDynamicCreditPlayerAmount = getAppliedDynamicCreditPlayerAmount();
            int playersLeftToApply = GameNetworkManager.Instance.connectedPlayers - appliedDynamicCreditPlayerAmount;
            newGroupCredits += dynamicStartCreditIncreasePerPlayer * playersLeftToApply;

            int newAppliedDynamicCreditPlayerAmount = appliedDynamicCreditPlayerAmount + playersLeftToApply;
            ES3.Save($"{StartCreditsPlusPlugin.modGUID}.dynamicCreditsPlayerAmount", newAppliedDynamicCreditPlayerAmount, GameNetworkManager.Instance.currentSaveFileName);
        }

        if (newGroupCredits != Terminal.groupCredits)
        {
            Terminal.SyncGroupCreditsServerRpc(newGroupCredits, Terminal.numberOfItemsInDropship);
        }
    }
}
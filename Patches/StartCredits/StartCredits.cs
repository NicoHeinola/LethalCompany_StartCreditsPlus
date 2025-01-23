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
        }


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

        // ----- If applied already to the current players
        int minPlayers = StartCreditsPlusPlugin.configManager.minPlayersForDynamicCredits.Value;
        int maxPlayers = StartCreditsPlusPlugin.configManager.maxPlayersForDynamicCredits.Value;
        bool maxPlayersEnabled = maxPlayers > -1;

        int playersToApplyTo = Mathf.Max(0, GameNetworkManager.Instance.connectedPlayers - minPlayers);

        // If max player limit is enabled
        if (maxPlayersEnabled)
        {
            playersToApplyTo = Mathf.Min(maxPlayers - minPlayers, playersToApplyTo);
        }

        if (getAppliedDynamicCreditPlayerAmount() >= playersToApplyTo)
        {
            return false;
        }
        // -----

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

    private static int applyStaticCredits(int _)
    {
        int startingCredits = StartCreditsPlusPlugin.configManager.startingCredits.Value;
        int creditsToModify = startingCredits;

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

    private static float getNextDynamicCreditAmount(float currentAmount)
    {
        int additiveChange = StartCreditsPlusPlugin.configManager.dynamicStartCreditAdditiveChange.Value;
        float multiplicativeChange = StartCreditsPlusPlugin.configManager.dynamicStartCreditMultiplicativeChange.Value;
        float newAmount = (currentAmount + additiveChange) * multiplicativeChange;

        return newAmount;
    }

    private static int applyDynamicCredits(int creditsToModify)
    {
        int increasePerPlayer = StartCreditsPlusPlugin.configManager.dynamicStartCreditIncreasePerPlayer.Value;

        int minPlayers = StartCreditsPlusPlugin.configManager.minPlayersForDynamicCredits.Value;
        int maxPlayers = StartCreditsPlusPlugin.configManager.maxPlayersForDynamicCredits.Value;

        bool maxPlayersEnabled = maxPlayers > -1;

        int playersToApplyTo = Mathf.Max(0, GameNetworkManager.Instance.connectedPlayers - minPlayers);

        // If max player limit is enabled
        if (maxPlayersEnabled)
        {
            playersToApplyTo = Mathf.Min(maxPlayers - minPlayers, playersToApplyTo);
        }

        int minIncrease = StartCreditsPlusPlugin.configManager.dynamicStartCreditMinIncrease.Value;
        int maxIncrease = StartCreditsPlusPlugin.configManager.dynamicStartCreditMaxIncrease.Value;

        bool maxIncreaseEnabled = maxIncrease != -1;

        int appliedToPlayersCount = getAppliedDynamicCreditPlayerAmount();

        float nextIncrease = increasePerPlayer;

        StartCreditsPlusPlugin.logger.LogDebug($"Applying dynamic credits to {playersToApplyTo} players. Applied to {appliedToPlayersCount} players.");

        for (int i = 1; i <= playersToApplyTo; i++)
        {
            // Calculate base next increase
            if (i <= appliedToPlayersCount)
            {
                nextIncrease = getNextDynamicCreditAmount(nextIncrease);
                continue;
            }

            // Apply limits
            int roundedNextIncrease = Mathf.RoundToInt(nextIncrease);


            roundedNextIncrease = Mathf.Max(minIncrease, roundedNextIncrease);


            if (maxIncreaseEnabled)
            {
                roundedNextIncrease = Mathf.Min(maxIncrease, roundedNextIncrease);
            }

            // Increase credits
            creditsToModify += roundedNextIncrease;

            // Increment next increase
            nextIncrease = getNextDynamicCreditAmount(nextIncrease);
        }

        // Fixes a situation where user first has maxPlayers off but then turns it on
        dynamicCreditsPlayerAmount = appliedToPlayersCount + playersToApplyTo;

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
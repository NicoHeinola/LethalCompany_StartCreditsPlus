namespace StartCreditsPlus.Patches.StartCredits;

internal class TerminalCommandManager
{
    private static string terminalMessageOverride = "";
    private static string terminalMessageAddition = "";
    public static bool canRunTerminalCommands()
    {
        bool terminalCommandsEnabled = StartCreditsPlusPlugin.configManager.terminalCommandsEnabled.Value;
        if (!terminalCommandsEnabled) return false;

        return true;
    }

    public static void handleTerminalCommand(string[] typedWords)
    {
        if (typedWords.Length == 0)
        {
            return;
        }


        if (!canRunTerminalCommands())
        {
            return;
        }

        string prefix = StartCreditsPlusPlugin.configManager.terminalCommandPrefix.Value.ToLower();
        string typedWord1 = typedWords[0].ToLower();

        string helpCommandKey = StartCreditsPlusPlugin.configManager.terminalCommandHelp.Value.ToLower();

        if (typedWords.Length < 2)
        {
            if (typedWord1 == "help")
            {
                handleVanillaHelpCommand();
            }

            return;
        }

        if (prefix != "" && typedWord1 != prefix)
        {
            return;
        }

        string typedWord2 = typedWords[1].ToLower();
        string reloadCommandKey = StartCreditsPlusPlugin.configManager.terminalCommandReload.Value.ToLower();

        if (helpCommandKey != "" && typedWord2 == helpCommandKey)
        {
            handleHelpCommand();
            return;
        }

        if (reloadCommandKey != "" && typedWord2 == reloadCommandKey)
        {
            handleReloadCommand();
            return;
        }

        // Debug commands

        /*if (typedWord2 == "d2")
        {
            StartCredits.calculateStartGroupCredits();
            terminalMessageOverride = $"Applied credit changes!";
            return;
        }

        if (typedWord2 == "d1+")
        {
            GameNetworkManager.Instance.connectedPlayers += 1;
            terminalMessageOverride = $"Players in lobby {GameNetworkManager.Instance.connectedPlayers}";
            return;
        }

        if (typedWord2 == "d1m")
        {
            GameNetworkManager.Instance.connectedPlayers -= 1;
            terminalMessageOverride = $"Players in lobby {GameNetworkManager.Instance.connectedPlayers}";
            return;
        }*/
    }

    private static void handleReloadCommand()
    {
        StartCreditsPlusPlugin.logger.LogDebug("\"reload\" command was typed...");

        if (!StartCredits.canModifyStartCredits())
        {
            terminalMessageOverride = $"You are not allowed to reset start credits!";

            return;
        }


        StartCreditsPlusPlugin.logger.LogDebug("Reloading start credits...");

        // Vanilla starting credits
        StartCredits.Terminal.groupCredits = TimeOfDay.Instance.quotaVariables.startingCredits;
        StartCredits.Terminal.ClearBoughtItems();
        StartCredits.Terminal.SyncGroupCreditsServerRpc(StartCredits.Terminal.groupCredits, StartCredits.Terminal.numberOfItemsInDropship);

        StartCredits.reset();

        StartCredits.calculateStartGroupCredits();

        terminalMessageOverride = $"Start credits have been reset!";
    }

    private static void handleHelpCommand()
    {
        StartCreditsPlusPlugin.logger.LogDebug("\"help\" command was typed...");

        string prefix = StartCreditsPlusPlugin.configManager.terminalCommandPrefix.Value;
        string helpCommandKey = StartCreditsPlusPlugin.configManager.terminalCommandHelp.Value;
        string reloadCommandKey = StartCreditsPlusPlugin.configManager.terminalCommandReload.Value;

        terminalMessageOverride = $"Available commands:\n\n>{prefix} {helpCommandKey}\nShows all commands\n\n>{prefix} {reloadCommandKey}\nResets start credits and bought items";
    }

    public static void handleVanillaHelpCommand()
    {
        StartCreditsPlusPlugin.logger.LogDebug("Handling vanilla help command...");

        string prefix = StartCreditsPlusPlugin.configManager.terminalCommandPrefix.Value;
        string helpCommandKey = StartCreditsPlusPlugin.configManager.terminalCommandHelp.Value;

        terminalMessageAddition = $">{prefix} {helpCommandKey}\nShows all commands from \"Start Credits Plus\"!";
    }

    public static void showPendingTerminalMessage()
    {
        string newCurrentText = StartCredits.Terminal.currentText ?? "";
        bool changedTerminalText = false;

        if (terminalMessageOverride != "")
        {
            newCurrentText = $"\n\n\n{terminalMessageOverride}";
            terminalMessageOverride = "";

            changedTerminalText = true;
        }

        if (terminalMessageAddition != "")
        {
            newCurrentText += $"{terminalMessageAddition}";
            terminalMessageAddition = "";

            changedTerminalText = true;
        }

        if (!changedTerminalText)
        {
            return;
        }

        if (!newCurrentText.EndsWith("\n\n"))
        {
            newCurrentText += "\n\n";
        }

        StartCredits.Terminal.currentText = newCurrentText;
        StartCredits.Terminal.screenText.text = StartCredits.Terminal.currentText;
    }
}

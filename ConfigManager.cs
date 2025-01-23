using BepInEx.Configuration;

namespace StartCreditsPlus;

internal class ConfigManager
{
    // Declare config entries
    internal ConfigEntry<bool> enableStartingCredits;
    internal ConfigEntry<int> startingCredits;

    internal ConfigEntry<bool> enableRandomStartCredits;
    internal ConfigEntry<int> minRandomStartCredits;
    internal ConfigEntry<int> maxRandomStartCredits;

    internal ConfigEntry<bool> enableDynamicStartCredits;
    internal ConfigEntry<int> dynamicStartCreditIncreasePerPlayer;
    internal ConfigEntry<int> minPlayersForDynamicCredits;
    internal ConfigEntry<int> maxPlayersForDynamicCredits;

    internal ConfigEntry<int> dynamicStartCreditMinIncrease;
    internal ConfigEntry<int> dynamicStartCreditMaxIncrease;

    internal ConfigEntry<int> dynamicStartCreditAdditiveChange;
    internal ConfigEntry<float> dynamicStartCreditMultiplicativeChange;

    internal ConfigEntry<bool> resetOnFirstDayUponReHost;

    internal ConfigEntry<bool> terminalCommandsEnabled;
    internal ConfigEntry<string> terminalCommandPrefix;
    internal ConfigEntry<string> terminalCommandHelp;
    internal ConfigEntry<string> terminalCommandReload;

    internal ConfigEntry<bool> allocateStartCreditsAfterLanding;

    // Strings to define sections and keys
    private const string generalSection = "General";
    private const string staticStartCreditsSection = "Static Start Credits";
    private const string dynamicStartCreditsSection = "Dynamic Start Credits";
    private const string changingDynamicStartCreditsSection = "Changing Dynamic Start Credits";
    private const string randomStartCreditsSection = "Random Start Credits";
    private const string terminalCommandsSection = "Terminal Commands";

    private const string enabledKey = "Enabled";
    private const string resetOnFirstDayUponReHostKey = "Reset on First Day Upon Re-Host";
    private const string allocateStartCreditsAfterLandingKey = "Allocate Start Credits After Landing";
    private const string startCreditsKey = "Start Credits";
    private const string startCreditIncreasePerPlayerKey = "Start Credit Increase Per Player";
    private const string minDynamicCreditPlayersKey = "Min Players";
    private const string maxDynamicCreditPlayersKey = "Max Players";
    private const string minRandomCreditsKey = "Min Additional Start Credits";
    private const string maxRandomCreditsKey = "Max Additional Start Credits";
    private const string dynamicStartCreditMinIncreaseKey = "Min Increase";
    private const string dynamicStartCreditMaxIncreaseKey = "Max Increase";
    private const string dynamicStartCreditAdditiveChangeKey = "Additive Change";
    private const string dynamicStartCreditMultiplicativeChangeKey = "Multiplicative Change";
    private const string terminalCommandPrefixKey = "Prefix For All Commands";
    private const string terminalCommandHelpKey = "Help Command Keyword";
    private const string terminalCommandReloadKey = "Reload Command Keyword";

    // Description variables
    private readonly string staticStartCreditsEnabledDescription = "If true, \"Static Start Credits\" is applied. Turn this off if you want some other mod to manage start credits.";

    private readonly string staticStartCreditsAmountDescription = "Modifies how many credits the crew starts with.\n\nEx. \"1000\" means that you have 1000 credits when you start a new run.\n\nDynamic Start Credits are applied on top of this.\n\nEx. if this setting is 60, Dynamic Start Credits is 15 and there are 3 players, then you would start with 60 + 15 * 3 = 105 credits. \n\nBy default you start with 60 credits in vanilla Lethal Company.";

    private readonly string dynamicStartCreditsEnabledDescription = "If true, \"Dynamic Start Credits\" is applied. Turn this off if you want some other mod to manage start credits.";

    private readonly string dynamicStartCreditIncreasePerPlayerDescription = "Gives extra credits when someone joins the game for the first time and the crew hasn't landed yet.\n\nHost is included.\n\nThis is based on player count rather than WHO joins. If 2 people join, then it adds the bonus twice and until there are more than 3 people (host + 2), it won't apply the bonus again.";

    private readonly string minPlayersForDynamicCreditsDescription = "How many players have to be present in the lobby for dynamic credits to be applied.\n\nEx. 0 means that host counts and this will be applied to the host and any other who joins.\n\nEx. 2 means that 2 players other than the host would need to join before we apply the \"Start Credit Increase Per Player\" increase at all. This would also mean that the counting starts from the third player with host being ignored meaning the bonus is applied ONCE in this scenario.";

    private readonly string maxPlayersForDynamicCreditsDescription = "How many players can be in the lobby before we stop applying the \"Start Credit Increase Per Player\" increase.\n\nEx. 4 would mean that the bonus is applied 4 times, host included. If a 5th person joined, host included, then the bonus would NOT be applied.\n\n-1 Means there is no maximum cap.";

    private readonly string dynamicStartCreditMinIncreaseDescription = "Minimum start credit increase value per player.\n\nUsed for when reducing/increasing player based start credits.\n\nIf this is less than 0, the starting credits could go DOWN if too many join.";

    private readonly string dynamicStartCreditMaxIncreaseDescription = "Maximum start credit increase value per player.\n\nUsed for when reducing/increasing player based start credits.\n\n-1 means there is no maximum cap.";

    private readonly string dynamicStartCreditAdditiveChangeDescription = "When someone joins, this is added to the Dynamic Start Credits per player.\n\nThis is applied BEFORE the multiplicative change.\n\nEx. Dynamic Start Credits = 100. Additive Change = -10. Multiplicative Change = 0.5. 1 players join. First the credits are 100 for the first player, aka. host. Then the Dynamic Start Credits are applied again for the second player. (100 + (-10)) * 0.5 = 45. The next credit increase would be 45 and we would have 145 credits with host and 1 other.";

    private readonly string dynamicStartCreditMultiplicativeChangeDescription = "When someone joins, Dynamic Start Credits are multiplied by this PER PLAYER.\n\nThis is applied BEFORE the multiplicative change.\n\nEx. Dynamic Start Credits = 100. Additive Change = -10. Multiplicative Change = 0.5. 2 players join. First the credits are 100 for the first player, aka. host. Then the Dynamic Start Credits are applied again for the second player. (100 + (-10)) * 0.5 = 45. The next credit increase would be 45 and we would have 145 credits with host and 1 other. Then the Dynamic Start Credits are applied again for the third player. (45 + (-10)) * 0.5) = 17.5. Now we have 162.5 credits with host and 2 others.";

    private readonly string randomCreditsEnabledDescription = "If true, the crew will get a random amount of credits.\n\nThis will be applied IN ADDITION to any other credits like \"Static Start Credits\".\n\nIf you just want any random amount, you should set \"Static Start Credits\" to 0.\n\nEx. if \"Static Start Credits\" is 200 and this is in range of -100 to 150, then the start credits could be from 100 (200 - 100) to 350 (200 + 150).";

    private readonly string minRandomCreditsDescription = "Minimum amount of credits the crew starts with.\n\nOnly applies to random credits.";

    private readonly string maxRandomCreditsDescription = "Maximum amount of credits the crew starts with.\n\nOnly applies to random credits.";

    private readonly string resetOnFirstDayUponReHostDescription = "When you create a lobby (or get fired), you can buy items and then save and exit. When you re-host, the bought items are not saved but you have still lost the credits. This is a \"fix\" for that.\n\nIf this is enabled, you will get your start credits back if you re-host the game on your first day (day 0).\n\nThis is disabled by default because some mods still save purchases on day 0 so this setting could be \"abused\" in that sense.";

    private readonly string terminalCommandsEnabledDescription = "If any of the terminal commands from this mod are enabled. \n\nCurrent terminal commands are:\n\"reload\"\n\"help\"\n\nSpaces are NOT allowed.\n\nYou can disable individual commands by leaving them empty.";

    private readonly string terminalCommandPrefixDescription = "Every terminal command from this mod needs to have this keyword before them. Case doesn't matter.\n\nEx. if this setting is StartCreditsPlus, you can reload credits by typing \"STARTcreditsPLUS REload\" or \"StartCreditsPlus reload\", etc.\n\nYou can leave the prefix empty and still run commands but it isn't recommended due to possible overlapping commands.";

    private readonly string terminalCommandHelpDescription = "Shows all available terminal commands and their descriptions.\n\nThis is useful if you forget what a command does or if you want to see all available commands.";

    private readonly string terminalCommandReloadDescription = "Resets start credits. Removes every bought item. Only works on day 0.\n\nThis is useful is some other mod overwrites some aspect of this mod. This is a sort of manual activation of this mod's features.";

    private readonly string allocateStartCreditsAfterLandingDescription = "If true, starting credits will be allocated after landing instead of at the start.\n\nStart credits will be 0 before you land.";

    public ConfigManager()
    {
        StartCreditsPlusPlugin.logger.LogDebug("Loading config...");

        ConfigFile config = StartCreditsPlusPlugin.Instance.Config;

        resetOnFirstDayUponReHost = config.Bind(generalSection, resetOnFirstDayUponReHostKey, false, resetOnFirstDayUponReHostDescription);
        allocateStartCreditsAfterLanding = config.Bind(generalSection, allocateStartCreditsAfterLandingKey, false, allocateStartCreditsAfterLandingDescription);

        // Static start credits
        enableStartingCredits = config.Bind(staticStartCreditsSection, enabledKey, true, staticStartCreditsEnabledDescription);
        startingCredits = config.Bind(staticStartCreditsSection, startCreditsKey, 60, staticStartCreditsAmountDescription);

        // Random start credits
        enableRandomStartCredits = config.Bind(randomStartCreditsSection, enabledKey, false, randomCreditsEnabledDescription);
        minRandomStartCredits = config.Bind(randomStartCreditsSection, minRandomCreditsKey, -60, minRandomCreditsDescription);
        maxRandomStartCredits = config.Bind(randomStartCreditsSection, maxRandomCreditsKey, 60, maxRandomCreditsDescription);

        // Dynamic start credits
        enableDynamicStartCredits = config.Bind(dynamicStartCreditsSection, enabledKey, true, dynamicStartCreditsEnabledDescription);
        dynamicStartCreditIncreasePerPlayer = config.Bind(dynamicStartCreditsSection, startCreditIncreasePerPlayerKey, 15, dynamicStartCreditIncreasePerPlayerDescription);
        minPlayersForDynamicCredits = config.Bind(dynamicStartCreditsSection, minDynamicCreditPlayersKey, 0, minPlayersForDynamicCreditsDescription);
        maxPlayersForDynamicCredits = config.Bind(dynamicStartCreditsSection, maxDynamicCreditPlayersKey, -1, maxPlayersForDynamicCreditsDescription);

        dynamicStartCreditMinIncrease = config.Bind(changingDynamicStartCreditsSection, dynamicStartCreditMinIncreaseKey, 0, dynamicStartCreditMinIncreaseDescription);
        dynamicStartCreditMaxIncrease = config.Bind(changingDynamicStartCreditsSection, dynamicStartCreditMaxIncreaseKey, -1, dynamicStartCreditMaxIncreaseDescription);

        dynamicStartCreditAdditiveChange = config.Bind(changingDynamicStartCreditsSection, dynamicStartCreditAdditiveChangeKey, -5, dynamicStartCreditAdditiveChangeDescription);
        dynamicStartCreditMultiplicativeChange = config.Bind(changingDynamicStartCreditsSection, dynamicStartCreditMultiplicativeChangeKey, 1f, dynamicStartCreditMultiplicativeChangeDescription);

        // Terminal commands
        terminalCommandsEnabled = config.Bind(terminalCommandsSection, enabledKey, true, terminalCommandsEnabledDescription);
        terminalCommandPrefix = config.Bind(terminalCommandsSection, terminalCommandPrefixKey, "StartCreditsPlus", terminalCommandPrefixDescription);
        terminalCommandHelp = config.Bind(terminalCommandsSection, terminalCommandHelpKey, "help", terminalCommandHelpDescription);
        terminalCommandReload = config.Bind(terminalCommandsSection, terminalCommandReloadKey, "reload", terminalCommandReloadDescription);

        // Subscribe to the SettingChanged event
        StartCreditsPlusPlugin.Instance.Config.SettingChanged += OnSettingChanged;

        StartCreditsPlusPlugin.logger.LogDebug("Config loaded!");
    }

    // Event handler for dynamic config reloading
    private void OnSettingChanged(object sender = null, SettingChangedEventArgs args = null)
    {
        StartCreditsPlusPlugin.logger.LogDebug($"Reloading a config value... [Section: {args.ChangedSetting.Definition.Section}, Value: {args.ChangedSetting.BoxedValue}]");

        if (args == null || args.ChangedSetting == null)
        {
            return;
        }

        // Dynamically update the values when settings change
        if (args.ChangedSetting.Definition.Section == staticStartCreditsSection)
        {
            switch (args.ChangedSetting.Definition.Key)
            {
                case enabledKey:
                    enableStartingCredits.Value = (bool)args.ChangedSetting.BoxedValue;
                    break;
                case startCreditsKey:
                    startingCredits.Value = (int)args.ChangedSetting.BoxedValue;
                    break;
            }
        }
        else if (args.ChangedSetting.Definition.Section == dynamicStartCreditsSection)
        {
            switch (args.ChangedSetting.Definition.Key)
            {
                case enabledKey:
                    enableDynamicStartCredits.Value = (bool)args.ChangedSetting.BoxedValue;
                    break;
                case startCreditIncreasePerPlayerKey:
                    dynamicStartCreditIncreasePerPlayer.Value = (int)args.ChangedSetting.BoxedValue;
                    break;
                case minDynamicCreditPlayersKey:
                    minPlayersForDynamicCredits.Value = (int)args.ChangedSetting.BoxedValue;
                    break;
                case maxDynamicCreditPlayersKey:
                    maxPlayersForDynamicCredits.Value = (int)args.ChangedSetting.BoxedValue;
                    break;
                case dynamicStartCreditMinIncreaseKey:
                    dynamicStartCreditMinIncrease.Value = (int)args.ChangedSetting.BoxedValue;
                    break;
                case dynamicStartCreditMaxIncreaseKey:
                    dynamicStartCreditMaxIncrease.Value = (int)args.ChangedSetting.BoxedValue;
                    break;
                case dynamicStartCreditAdditiveChangeKey:
                    dynamicStartCreditAdditiveChange.Value = (int)args.ChangedSetting.BoxedValue;
                    break;
                case dynamicStartCreditMultiplicativeChangeKey:
                    dynamicStartCreditMultiplicativeChange.Value = (float)args.ChangedSetting.BoxedValue;
                    break;
            }
        }
        else if (args.ChangedSetting.Definition.Section == randomStartCreditsSection)
        {
            switch (args.ChangedSetting.Definition.Key)
            {
                case enabledKey:
                    enableRandomStartCredits.Value = (bool)args.ChangedSetting.BoxedValue;
                    break;
                case minRandomCreditsKey:
                    minRandomStartCredits.Value = (int)args.ChangedSetting.BoxedValue;
                    break;
                case maxRandomCreditsKey:
                    maxRandomStartCredits.Value = (int)args.ChangedSetting.BoxedValue;
                    break;
            }
        }
        else if (args.ChangedSetting.Definition.Section == generalSection)
        {
            switch (args.ChangedSetting.Definition.Key)
            {
                case resetOnFirstDayUponReHostKey:
                    resetOnFirstDayUponReHost.Value = (bool)args.ChangedSetting.BoxedValue;
                    break;
                case allocateStartCreditsAfterLandingKey:
                    allocateStartCreditsAfterLanding.Value = (bool)args.ChangedSetting.BoxedValue;
                    break;
            }

        }
        else if (args.ChangedSetting.Definition.Section == terminalCommandsSection)
        {
            switch (args.ChangedSetting.Definition.Key)
            {
                case enabledKey:
                    terminalCommandsEnabled.Value = (bool)args.ChangedSetting.BoxedValue;
                    break;
                case terminalCommandPrefixKey:
                    terminalCommandPrefix.Value = (string)args.ChangedSetting.BoxedValue;
                    break;
                case terminalCommandHelpKey:
                    terminalCommandHelp.Value = (string)args.ChangedSetting.BoxedValue;
                    break;
                case terminalCommandReloadKey:
                    terminalCommandReload.Value = (string)args.ChangedSetting.BoxedValue;
                    break;
            }
        }

        StartCreditsPlusPlugin.logger.LogDebug("Reloading complete!");
    }
}

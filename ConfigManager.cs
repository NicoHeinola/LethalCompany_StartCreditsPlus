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

    internal ConfigEntry<bool> resetOnFirstDayUponReHost;

    // Strings to define sections and keys
    private const string generalSection = "General";
    private const string staticStartCreditsSection = "Static Start Credits";
    private const string dynamicStartCreditsSection = "Dynamic Start Credits";
    private const string randomStartCreditsSection = "Random Start Credits";

    private const string enabledKey = "Enabled";
    private const string startCreditsKey = "Start Credits";
    private const string startCreditIncreasePerPlayerKey = "Start Credit Increase Per Player";
    private const string minDynamicCreditPlayersKey = "Min Players";
    private const string maxDynamicCreditPlayersKey = "Max Players";
    private const string minRandomCreditsKey = "Min Additional Start Credits";
    private const string maxRandomCreditsKey = "Max Additional Start Credits";
    private const string resetOnFirstDayUponReHostKey = "Reset on First Day Upon Re-Host";

    // Description variables
    private readonly string staticStartCreditsEnabledDescription = "If true, \"Static Start Credits\" is applied. Turn this off if you want some other mod to manage start credits.";

    private readonly string staticStartCreditsAmountDescription = "Modifies how many credits the crew starts with.\n\nEx. \"1000\" means that you have 1000 credits when you start a new run.\n\nDynamic Start Credits are applied on top of this.\n\nEx. if this setting is 60, Dynamic Start Credits is 15 and there are 3 players, then you would start with 60 + 15 * 3 = 105 credits. \n\nBy default you start with 60 credits in unmodded Lethal Company.";

    private readonly string dynamicStartCreditsEnabledDescription = "If true, \"Dynamic Start Credits\" is applied. Turn this off if you want some other mod to manage start credits.";

    private readonly string dynamicStartCreditIncreasePerPlayerDescription = "Gives extra credits when someone joins the game for the first time and the crew hasn't landed yet.\n\nHost is included.\n\nThis is based on player count rather than WHO joins. If 2 people join, then it adds the bonus twice and until there are more than 3 people (host + 2), it won't apply the bonus again.";

    private readonly string minPlayersForDynamicCreditsDescription = "How many players have to be present in the lobby for dynamic credits to be applied.\n\nEx. 0 means that host counts and this will be applied to the host and any other who joins.\n\nEx. 1 means that someone else other than the host would need to join before we apply the \"Start Credit Increase Per Player\" increase.";

    private readonly string maxPlayersForDynamicCreditsDescription = "How many players can be in the lobby before we stop applying the \"Start Credit Increase Per Player\" increase.\n\nEx. 4 would mean that the bonus is applied 4 times, host included. If a 5th person joined, host included, then the bonus would NOT be applied.\n\n-1 Means there is no maximum cap.";

    private readonly string randomCreditsEnabledDescription = "If true, the crew will get a random amount of credits.\n\nThis will be applied IN ADDITION to any other credits like \"Static Start Credits\".\n\nIf you just want any random amount, you should set \"Static Start Credits\" to 0.\n\nEx. if \"Static Start Credits\" is 200 and this is in range of -100 to 150, then the starting credits could be from 100 (200 - 100) to 350 (200 + 150).";

    private readonly string minRandomCreditsDescription = "Minimum amount of credits the crew starts with.\n\nOnly applies to random credits.";

    private readonly string maxRandomCreditsDescription = "Maximum amount of credits the crew starts with.\n\nOnly applies to random credits.";

    private readonly string resetOnFirstDayUponReHostDescription = "When you create a lobby (or get fired), you can buy items and then save and exit. When you re-host, the bought items are not saved but you have still lost the credits. This is a \"fix\" for that.\n\nIf this is enabled, you will get your starting credits back if you re-host the game on your first day (day 0).\n\nThis is disabled by default because some mods still save purchases on day 0 so this setting could be \"abused\" in that sense.";
    public ConfigManager()
    {
        StartCreditsPlusPlugin.logger.LogDebug("Loading config...");

        ConfigFile config = StartCreditsPlusPlugin.Instance.Config;

        resetOnFirstDayUponReHost = config.Bind(generalSection, resetOnFirstDayUponReHostKey, false, resetOnFirstDayUponReHostDescription);

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
            }

        }

        StartCreditsPlusPlugin.logger.LogDebug("Reloading complete!");
    }
}

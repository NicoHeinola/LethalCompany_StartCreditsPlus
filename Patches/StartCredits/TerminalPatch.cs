using HarmonyLib;
using System;
using System.Reflection;
namespace StartCreditsPlus.Patches.StartCredits;

[HarmonyPatch(typeof(Terminal))]
internal class TerminalPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    private static void StartPatch()
    {
        StartCredits.calculateStartGroupCredits();
    }

    [HarmonyPatch("OnSubmit")]
    [HarmonyPrefix]
    private static void OnSubmitPrePatch(ref Terminal __instance)
    {
        if (!TerminalCommandManager.canRunTerminalCommands()) return;

        string s = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded);

        MethodInfo removePunctuationMethod = typeof(Terminal).GetMethod("RemovePunctuation", BindingFlags.NonPublic | BindingFlags.Instance);
        if (removePunctuationMethod == null)
        {
            StartCreditsPlusPlugin.logger.LogError("Could not find RemovePunctuation method in Terminal class! Can't run terminal commands.");
            return;
        }

        s = (string)removePunctuationMethod.Invoke(__instance, new object[] { s });

        string[] array = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        TerminalCommandManager.handleTerminalCommand(array);
    }

    [HarmonyPatch("OnSubmit")]
    [HarmonyPostfix]
    private static void OnSubmitPostPatch()
    {
        if (!TerminalCommandManager.canRunTerminalCommands()) return;

        TerminalCommandManager.showPendingTerminalMessage();
    }

    [HarmonyPatch("BeginUsingTerminal")]
    [HarmonyPostfix]
    private static void BeginUsingTerminalPatch()
    {
        if (!TerminalCommandManager.canRunTerminalCommands()) return;

        TerminalCommandManager.handleVanillaHelpCommand();
        TerminalCommandManager.showPendingTerminalMessage();
    }
}

using System;
using System.Text;
using HarmonyLib;
using LevelGeneration;

namespace GTFO.EZ.Patches;

/// <summary>
/// SPIKE (GE-T10), diagnostic only: investigates a level-wide terminal PING (ping any item from
/// any terminal, not just same-zone). A prior attempt used this same technique - look up the item
/// in LG_LevelInteractionManager.Current.m_terminalItemsByKeyString, then call
/// iTerminalItem.PlayPing() + GuiManager.AttemptSetLocatorBeacon/AttemptSetTerminalPing directly
/// (what AlexanderMakunin/BetterTerminal, MIT, does for its own consumables) - but broke the
/// terminal in play and was fully reverted before ever reaching version control, so its exact code
/// isn't available to diff against here; this is a fresh instrumented reconstruction.
///
/// It's built defensive-by-construction against two of the three suspected causes: it never skips
/// the original EvaluateInput call (the Prefix always lets Harmony continue to the real method, so
/// "skipping EvaluateInput breaks interpreter state" can't apply here), and it catches every
/// exception instead of letting one escape the prefix (Harmony does not swallow prefix exceptions,
/// so an unhandled one here would propagate up through EvaluateInput's own call site and could well
/// be what "broke the terminal" - the original method would simply never run). What's left
/// un-ruled-out is "wrong key format" - this logs the raw input and the parsed key on every
/// attempt specifically so that can be confirmed or ruled out from LogOutput.log.
///
/// Off by default. Enabling it never changes what EvaluateInput itself does - it only observes the
/// input and best-effort pings alongside it. Needs an actual in-game PING attempt to find out
/// whether it still breaks anything; the command parsing itself lives in native IL2CPP code this
/// project can't unit-test or decompile past its interop stubs.
/// </summary>
[HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.EvaluateInput))]
internal static class TerminalPing_EvaluateInput_Patch
{
    private const string LogPrefix = "GTFO.EZ [GE-T10 terminal ping spike]";
    private const string PingCommand = "PING";

    [HarmonyPrefix]
    private static void Prefix(string inputString)
    {
        if (!Plugin.ExperimentalGlobalTerminalPing.Value) return;

        // Deliberately the only thing this prefix does outside the try/catch: if everything below
        // throws, we still know EvaluateInput was reached with this exact input text.
        Plugin.Logger.LogInfo($"{LogPrefix} EvaluateInput input: \"{inputString}\"");

        try
        {
            HandlePing(inputString);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"{LogPrefix} exception while handling input \"{inputString}\": {e}");
        }
    }

    private static void HandlePing(string? inputString)
    {
        var trimmed = inputString?.Trim() ?? string.Empty;
        if (!trimmed.StartsWith(PingCommand, StringComparison.OrdinalIgnoreCase)) return;

        var key = trimmed.Length > PingCommand.Length ? trimmed[PingCommand.Length..].Trim() : string.Empty;
        if (key.Length == 0)
        {
            Plugin.Logger.LogInfo($"{LogPrefix} PING with no key argument, nothing to look up.");
            return;
        }

        var manager = LG_LevelInteractionManager.Current;
        if (manager == null)
        {
            Plugin.Logger.LogWarning($"{LogPrefix} LG_LevelInteractionManager.Current is null, cannot look up terminal items.");
            return;
        }

        var itemsByKey = manager.m_terminalItemsByKeyString;
        if (itemsByKey == null)
        {
            Plugin.Logger.LogWarning($"{LogPrefix} m_terminalItemsByKeyString is null.");
            return;
        }

        Plugin.Logger.LogInfo($"{LogPrefix} parsed key \"{key}\" from input, {itemsByKey.Count} known terminal item keys.");

        if (!itemsByKey.TryGetValue(key, out var item))
        {
            var sample = new StringBuilder();
            var count = 0;
            foreach (var knownKey in itemsByKey.Keys)
            {
                if (count++ >= 10) break;
                if (sample.Length > 0) sample.Append(", ");
                sample.Append(knownKey);
            }

            Plugin.Logger.LogWarning($"{LogPrefix} key \"{key}\" not found. Sample known keys: {sample}");
            return;
        }

        Plugin.Logger.LogInfo($"{LogPrefix} found item for key \"{key}\" (TerminalItemId={item.TerminalItemId}), triggering ping.");

        item.PlayPing();
        GuiManager.AttemptSetLocatorBeacon(true, item.LocatorBeaconPosition, item.TerminalItemId);
        GuiManager.AttemptSetTerminalPing(true, item.LocatorBeaconPosition, item.TerminalItemId);

        Plugin.Logger.LogInfo($"{LogPrefix} ping triggered for key \"{key}\" without exception.");
    }
}

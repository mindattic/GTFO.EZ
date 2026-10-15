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
/// be what "broke the terminal" - the original method would simply never run). "Wrong key format"
/// was also ruled in as a real bug (not the root cause, but a confirmed defect in this spike's own
/// parsing) and fixed: it used to mistake "PING KEY -t" for an unknown-key-format input.
///
/// A fourth hypothesis, found by decompiling the game's IL2CPP interop stubs (Modules-ASM.dll) with
/// ilspycmd rather than by play-testing: LG_ComputerTerminalCommandInterpreter has its own *private*
/// `int Ping(string, string)`, plus private GetPingStatus(iTerminalItem)/GetDefaultDetails(item,
/// beaconStatus) helpers. (A later pass of this same decompile corrected two names that had been
/// misattributed to the interpreter: TryGetTerminalInterface(key-or-id, eDimensionIndex, out item) is
/// `public static` on LG_LevelInteractionManager, not a private interpreter helper, and
/// OnGotListedCallback is a public `Il2CppSystem.Action` property on iTerminalItem itself (backed by
/// LG_GenericTerminalItem) - both already reachable with no reflection needed.) Vanilla's real PING
/// path almost certainly does eligibility/listed-state bookkeeping through Ping's private helpers
/// before (or instead of) touching PlayPing()/AttemptSetLocatorBeacon/AttemptSetTerminalPing directly
/// - the three raw calls this spike (and the original broken attempt) make skip all of that. That
/// bookkeeping gap, not a networking/RPC one, is the leading suspect: GuiManager separately exposes a
/// distinctly-named SpawnSyncedNavMarker, implying these two calls are intentionally local-only
/// rather than RPC-gated, so a replication precondition is unlikely.
///
/// Ping's signature - `(string, string)`, not `(string, eDimensionIndex)` as first guessed - lines up
/// with vanilla's own "PING key [-t]" parsing, meaning this private method is almost certainly the
/// exact entry point EvaluateInput's own command dispatch already reaches for a normal, same-zone
/// PING. That cuts the other way for "reflect into it instead" as a next step: Ping's body is (
/// reconfirmed on this later pass) a pure il2cpp_runtime_invoke trampoline with no recoverable
/// managed IL, so whether its native implementation enforces the same-zone restriction internally -
/// the exact thing a level-wide ping needs to bypass - can't be read off decompiled code, and
/// reflecting into it would most likely just reproduce ordinary same-zone PING rather than a
/// level-wide one. Interop stubs being native trampolines means this can't be narrowed further by
/// static analysis alone - the open bookkeeping-gap hypothesis still needs an actual in-game PING
/// attempt to confirm or rule out, which no agent session so far has had a way to perform.
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
        var tokens = (inputString ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0 || !string.Equals(tokens[0], PingCommand, StringComparison.OrdinalIgnoreCase)) return;

        // Vanilla PING takes a "-t" (repeat) flag, before or after the key - skip flags so
        // "PING KEY_123 -t" looks up KEY_123 rather than misreporting a key-format problem.
        var key = string.Empty;
        for (var i = 1; i < tokens.Length; i++)
        {
            if (tokens[i].StartsWith("-")) continue;
            key = tokens[i];
            break;
        }
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

        // Registry keys are uppercase; fall back so typed lowercase isn't misread as a format bug.
        if (!itemsByKey.TryGetValue(key, out var item) && !itemsByKey.TryGetValue(key.ToUpperInvariant(), out item))
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

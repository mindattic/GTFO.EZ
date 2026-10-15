using System;
using System.Collections.Generic;

namespace GTFO.EZ.Core;

/// <summary>
/// Pure multiplier math shared by the weapon and melee tweak passes in GameDataTweaks.cs - no
/// BepInEx/Il2Cpp dependency, so it can be unit tested directly (docs/USER_STORIES.md GE-T4).
/// </summary>
public static class BalanceMath
{
    // Mirrors GameDataInit_Initialize_Patch.ScaleInt: Math.Round has no float overload, so the
    // product widens to double before rounding - this is also where Math.Round's default
    // MidpointRounding.ToEven ("banker's rounding") applies (3 * 1.5 = 4, 5 * 0.5 = 2), which is a
    // deliberate, open product question (see GE-T4) rather than something this method decides.
    public static int ScaleInt(int value, float multiplier) => (int)Math.Round(value * multiplier);

    // A missing key defaults to 1 (no-op multiplier) rather than 0, so an archetype/field
    // combination nobody configured yet - or one the game adds later - is left at vanilla instead
    // of silently zeroing out a stat.
    public static float LookupMultiplierOrDefault(IReadOnlyDictionary<string, float> table, string key) =>
        table.TryGetValue(key, out var value) ? value : 1f;

    // Weapon stats compose a type-level slider (shared by every fire-mode in a weapon family) with
    // an individual-archetype slider; melee stats only ever use the individual-style lookup, so
    // ComposeWeaponMultiplier's two-table shape only applies to ranged weapons/sentries.
    public static float ComposeWeaponMultiplier(
        IReadOnlyDictionary<string, float> typeTable, string typeKey,
        IReadOnlyDictionary<string, float> individualTable, string individualKey) =>
        LookupMultiplierOrDefault(typeTable, typeKey) * LookupMultiplierOrDefault(individualTable, individualKey);
}

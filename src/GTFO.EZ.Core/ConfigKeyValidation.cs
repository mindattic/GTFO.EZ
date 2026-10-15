using System.Collections.Generic;
using System.Linq;

namespace GTFO.EZ.Core;

/// <summary>
/// Pure checks against <see cref="BalanceTables"/> and the (section, key) pairs Plugin.cs's
/// BindConfig generates from it - no BepInEx dependency, so these run as plain unit tests
/// (docs/USER_STORIES.md GE-T3). A failure here means BindConfig would throw at startup (BepInEx
/// rejects a duplicate section+key pair) or silently mis-tag an archetype.
/// </summary>
public static class ConfigKeyValidation
{
    public static IReadOnlyCollection<uint> FindDuplicateArchetypeIds(
        IEnumerable<(uint ArchetypeId, string TypeCategory, string Label)> archetypes) =>
        archetypes.GroupBy(a => a.ArchetypeId).Where(g => g.Count() > 1).Select(g => g.Key).ToArray();

    public static IReadOnlyCollection<uint> FindArchetypesWithUnknownCategory(
        IEnumerable<(uint ArchetypeId, string TypeCategory, string Label)> archetypes,
        IEnumerable<string> categories)
    {
        var known = new HashSet<string>(categories);
        return archetypes.Where(a => !known.Contains(a.TypeCategory)).Select(a => a.ArchetypeId).ToArray();
    }

    public static IReadOnlyCollection<string> FindCategoriesWithNoArchetypes(
        IEnumerable<string> categories,
        IEnumerable<(uint ArchetypeId, string TypeCategory, string Label)> archetypes)
    {
        var used = new HashSet<string>(archetypes.Select(a => a.TypeCategory));
        return categories.Where(c => !used.Contains(c)).ToArray();
    }

    // BepInEx's config file is an INI-style "[Section]" / "Key = Value" format: a section
    // containing '[' or ']', or a key containing '=' or a newline, would corrupt the generated
    // .cfg on write or fail to round-trip back on the next load. An empty/whitespace-only name is
    // rejected by BepInEx's own ConfigDefinition outright.
    public static bool IsLegalSectionName(string section) =>
        !string.IsNullOrWhiteSpace(section) && !section.Contains('[') && !section.Contains(']') && !ContainsNewline(section);

    public static bool IsLegalKeyName(string key) =>
        !string.IsNullOrWhiteSpace(key) && !key.Contains('=') && !ContainsNewline(key);

    private static bool ContainsNewline(string s) => s.Contains('\n') || s.Contains('\r');

    public static IEnumerable<(string Section, string Key)> GenerateMeleeConfigKeys(
        IEnumerable<string> meleeArchetypes,
        IEnumerable<(string FieldKey, string ActionType, string DisplayName, string Description)> meleeStatDefs)
    {
        foreach (var archetype in meleeArchetypes)
            foreach (var stat in meleeStatDefs)
                yield return ($"Melee - {archetype} - {stat.ActionType}", stat.DisplayName);
    }

    public static IEnumerable<(string Section, string Key)> GenerateWeaponTypeConfigKeys(
        IEnumerable<string> weaponTypeCategories,
        IEnumerable<(string FieldKey, string DisplayName, string Description)> weaponCoreStatDefs)
    {
        foreach (var category in weaponTypeCategories)
            foreach (var stat in weaponCoreStatDefs)
                yield return ($"Weapon Type - {category.Replace('_', ' ')}", stat.DisplayName);
    }

    public static IEnumerable<(string Section, string Key)> GenerateWeaponIndividualConfigKeys(
        IEnumerable<(uint ArchetypeId, string TypeCategory, string Label)> weaponArchetypes,
        IEnumerable<(string FieldKey, string DisplayName, string Description)> weaponCoreStatDefs)
    {
        foreach (var weapon in weaponArchetypes)
            foreach (var stat in weaponCoreStatDefs)
                yield return ($"Weapon - {weapon.Label}", stat.DisplayName);
    }
}

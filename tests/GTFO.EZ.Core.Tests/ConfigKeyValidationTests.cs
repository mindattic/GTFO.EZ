using System.Linq;
using GTFO.EZ.Core;
using NUnit.Framework;

namespace GTFO.EZ.Core.Tests;

// GE-T3: a duplicate or illegal generated config key/section makes BindConfig throw and the whole
// mod fail to load, so these run against the real shipped BalanceTables data, not synthetic
// fixtures - a regression here is exactly the failure mode the ticket describes.
[TestFixture]
public class ConfigKeyValidationTests
{
    [Test]
    public void WeaponArchetypes_HaveNoDuplicateIds()
    {
        var duplicates = ConfigKeyValidation.FindDuplicateArchetypeIds(BalanceTables.WeaponArchetypes);

        Assert.That(duplicates, Is.Empty, $"Duplicate archetype IDs: {string.Join(", ", duplicates)}");
    }

    [Test]
    public void WeaponArchetypes_EveryTypeCategoryIsListed()
    {
        var unknown = ConfigKeyValidation.FindArchetypesWithUnknownCategory(
            BalanceTables.WeaponArchetypes, BalanceTables.WeaponTypeCategories);

        Assert.That(unknown, Is.Empty, $"Archetype IDs with an unlisted TypeCategory: {string.Join(", ", unknown)}");
    }

    [Test]
    public void WeaponTypeCategories_EveryCategoryHasAtLeastOneArchetype()
    {
        var unused = ConfigKeyValidation.FindCategoriesWithNoArchetypes(
            BalanceTables.WeaponTypeCategories, BalanceTables.WeaponArchetypes);

        Assert.That(unused, Is.Empty, $"Categories with zero archetypes: {string.Join(", ", unused)}");
    }

    [Test]
    public void GeneratedConfigKeys_AreGloballyUnique_AcrossMeleeAndWeaponSections()
    {
        var allKeys = ConfigKeyValidation.GenerateMeleeConfigKeys(BalanceTables.MeleeArchetypes, BalanceTables.MeleeStatDefs)
            .Concat(ConfigKeyValidation.GenerateWeaponTypeConfigKeys(BalanceTables.WeaponTypeCategories, BalanceTables.WeaponCoreStatDefs))
            .Concat(ConfigKeyValidation.GenerateWeaponIndividualConfigKeys(BalanceTables.WeaponArchetypes, BalanceTables.WeaponCoreStatDefs))
            .ToList();

        var duplicates = allKeys.GroupBy(k => k).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

        Assert.That(duplicates, Is.Empty, $"Duplicate (section, key) pairs: {string.Join("; ", duplicates)}");
    }

    [Test]
    public void GeneratedConfigKeys_AreAllBepInExLegal()
    {
        var allKeys = ConfigKeyValidation.GenerateMeleeConfigKeys(BalanceTables.MeleeArchetypes, BalanceTables.MeleeStatDefs)
            .Concat(ConfigKeyValidation.GenerateWeaponTypeConfigKeys(BalanceTables.WeaponTypeCategories, BalanceTables.WeaponCoreStatDefs))
            .Concat(ConfigKeyValidation.GenerateWeaponIndividualConfigKeys(BalanceTables.WeaponArchetypes, BalanceTables.WeaponCoreStatDefs));

        foreach (var (section, key) in allKeys)
        {
            Assert.That(ConfigKeyValidation.IsLegalSectionName(section), Is.True, $"Illegal section name: \"{section}\"");
            Assert.That(ConfigKeyValidation.IsLegalKeyName(key), Is.True, $"Illegal key name: \"{key}\"");
        }
    }

    [TestCase("Weapon - Rifle Semi", true)]
    [TestCase("Weapon [Rifle]", false)]
    [TestCase("", false)]
    [TestCase("   ", false)]
    public void IsLegalSectionName_RejectsBracketsAndBlankNames(string section, bool expected)
    {
        Assert.That(ConfigKeyValidation.IsLegalSectionName(section), Is.EqualTo(expected));
    }

    [TestCase("Damage Multiplier", true)]
    [TestCase("Damage = Multiplier", false)]
    [TestCase("", false)]
    public void IsLegalKeyName_RejectsEqualsSignAndBlankNames(string key, bool expected)
    {
        Assert.That(ConfigKeyValidation.IsLegalKeyName(key), Is.EqualTo(expected));
    }
}

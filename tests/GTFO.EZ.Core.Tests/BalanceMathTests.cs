using System.Collections.Generic;
using GTFO.EZ.Core;
using NUnit.Framework;

namespace GTFO.EZ.Core.Tests;

[TestFixture]
public class BalanceMathTests
{
    [TestCase(3, 1.5f, 5)]
    [TestCase(5, 0.5f, 3)]
    [TestCase(1, 0.4f, 1)]
    [TestCase(3, 1.6f, 5)]
    [TestCase(4, 1.0f, 4)]
    [TestCase(10, 0.3f, 3)] // 0.3 isn't exact in binary - must not become 4
    public void ScaleIntUp_RoundsUp_ForStatsWhereMoreHelpsThePlayer(int value, float multiplier, int expected)
    {
        Assert.That(BalanceMath.ScaleIntUp(value, multiplier), Is.EqualTo(expected));
    }

    [TestCase(3, 1.5f, 4)]
    [TestCase(5, 0.5f, 2)]
    [TestCase(1, 0.4f, 0)]
    [TestCase(3, 1.6f, 4)]
    [TestCase(4, 1.0f, 4)]
    [TestCase(10, 0.7f, 7)] // 0.7 isn't exact in binary - must not become 6
    public void ScaleIntDown_RoundsDown_ForStatsWhereFewerHelpsThePlayer(int value, float multiplier, int expected)
    {
        Assert.That(BalanceMath.ScaleIntDown(value, multiplier), Is.EqualTo(expected));
    }

    [Test]
    public void LookupMultiplierOrDefault_ReturnsOne_WhenKeyMissing()
    {
        var table = new Dictionary<string, float> { ["Rifle.Damage"] = 2f };

        Assert.That(BalanceMath.LookupMultiplierOrDefault(table, "Shotgun.Damage"), Is.EqualTo(1f));
    }

    [Test]
    public void LookupMultiplierOrDefault_ReturnsStoredValue_WhenKeyPresent()
    {
        var table = new Dictionary<string, float> { ["Rifle.Damage"] = 2f };

        Assert.That(BalanceMath.LookupMultiplierOrDefault(table, "Rifle.Damage"), Is.EqualTo(2f));
    }

    [Test]
    public void ComposeWeaponMultiplier_MultipliesTypeAndIndividualSliders()
    {
        var typeTable = new Dictionary<string, float> { ["Rifle.Damage"] = 2f };
        var individualTable = new Dictionary<string, float> { ["1.Damage"] = 1.5f };

        var result = BalanceMath.ComposeWeaponMultiplier(typeTable, "Rifle.Damage", individualTable, "1.Damage");

        Assert.That(result, Is.EqualTo(3f));
    }

    [Test]
    public void ComposeWeaponMultiplier_DefaultsMissingSideToOne_ActingAsNoOp()
    {
        var typeTable = new Dictionary<string, float> { ["Rifle.Damage"] = 2f };
        var individualTable = new Dictionary<string, float>(); // archetype never configured

        var result = BalanceMath.ComposeWeaponMultiplier(typeTable, "Rifle.Damage", individualTable, "1.Damage");

        Assert.That(result, Is.EqualTo(2f));
    }
}

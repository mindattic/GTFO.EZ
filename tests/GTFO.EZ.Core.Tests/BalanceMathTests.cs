using System.Collections.Generic;
using GTFO.EZ.Core;
using NUnit.Framework;

namespace GTFO.EZ.Core.Tests;

[TestFixture]
public class BalanceMathTests
{
    [TestCase(3, 1.6f, 5)] // 4.8, unambiguous regardless of rounding mode
    [TestCase(1, 0.4f, 0)] // 0.4, unambiguous
    [TestCase(4, 0.25f, 1)] // exactly 1.0, no rounding involved
    public void ScaleInt_RoundsToNearestInteger_ForNonMidpointCases(int value, float multiplier, int expected)
    {
        Assert.That(BalanceMath.ScaleInt(value, multiplier), Is.EqualTo(expected));
    }

    // GE-T4: Math.Round defaults to MidpointRounding.ToEven ("banker's rounding"), so exact
    // .5 products round to the nearest EVEN integer, not always up - 3*1.5=4.5 rounds to 4
    // (even), while 5*0.5=2.5 rounds to 2 (even). This documents the current, actual behavior;
    // whether that's the intended behavior for clip sizes/pellet counts is a still-open product
    // decision (pin AwayFromZero deliberately, or accept ToEven), not something these tests
    // should silently redefine.
    [TestCase(3, 1.5f, 4)]
    [TestCase(5, 0.5f, 2)]
    public void ScaleInt_UsesBankersRounding_AtExactMidpoints(int value, float multiplier, int expected)
    {
        Assert.That(BalanceMath.ScaleInt(value, multiplier), Is.EqualTo(expected));
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

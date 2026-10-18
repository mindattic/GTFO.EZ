using System;
using HarmonyLib;
using UnityEngine;

namespace GTFO.EZ.Patches;

// The I2-LP's heal is hardcoded in native code (vanilla rolls 5-15% of max health) - no datablock
// carries it, so it can't be scaled like everything in GameDataTweaks. It reaches the player
// through Dam_SyncedDamageBase.AddHealth, which med kits use too, so the syringe's own apply
// coroutine flags the call window and only AddHealth calls made inside it are rewritten.
[HarmonyPatch(typeof(SyringeFirstPerson._ApplySyringe_d__18), nameof(SyringeFirstPerson._ApplySyringe_d__18.MoveNext))]
internal static class SyringeApply_MoveNext_Patch
{
    [ThreadStatic] internal static bool ApplyingHealthSyringe;

    [HarmonyPrefix]
    private static void Prefix(SyringeFirstPerson._ApplySyringe_d__18 __instance)
    {
        var syringe = __instance.__4__this;
        ApplyingHealthSyringe = syringe != null && syringe.m_type == eSyringeType.Health;
    }

    // A finalizer, not a postfix, so the flag clears even if MoveNext throws - otherwise the next
    // med kit heal would be rewritten as a syringe heal.
    [HarmonyFinalizer]
    private static void Finalizer() => ApplyingHealthSyringe = false;
}

[HarmonyPatch(typeof(Dam_SyncedDamageBase), nameof(Dam_SyncedDamageBase.AddHealth))]
internal static class SyringeAddHealth_Patch
{
    [HarmonyPrefix]
    private static void Prefix(Dam_SyncedDamageBase __instance, ref float addHealth)
    {
        if (!SyringeApply_MoveNext_Patch.ApplyingHealthSyringe)
        {
            SyringeLifeBuff.OnOtherHeal(__instance, addHealth);
            return;
        }

        var fraction = Plugin.HealthSyringeHealFraction.Value;
        if (fraction <= 0f) return;

        var vanillaAmount = addHealth;
        var maxHealth = SyringeLifeBuff.OriginalMaxHealth(__instance);
        addHealth = maxHealth * fraction;
        Plugin.Logger.LogInfo($"I2-LP syringe heal: {vanillaAmount:0.##} -> {addHealth:0.##} HP (max {maxHealth:0.##}).");

        SyringeLifeBuff.Grant(__instance, addHealth);
    }
}

[HarmonyPatch(typeof(Dam_PlayerDamageLocal))]
internal static class SyringeLifeBuff_Tick_Patch
{
    [HarmonyPatch(nameof(Dam_PlayerDamageLocal.Update))]
    [HarmonyPostfix]
    private static void Update_Postfix(Dam_PlayerDamageLocal __instance) => SyringeLifeBuff.Tick(__instance);

    // A new level means a new local player, so a buff still running from the last one is dropped.
    // Passes the instance along so a HealthMax raised for the buff that just ended gets restored
    // on it specifically, rather than left inflated forever on a player object the game reuses
    // across levels within the same expedition (see SyringeLifeBuff.Clear).
    [HarmonyPatch(nameof(Dam_PlayerDamageLocal.Setup))]
    [HarmonyPostfix]
    private static void Setup_Postfix(Dam_PlayerDamageLocal __instance) => SyringeLifeBuff.Clear(__instance);
}

// Turns the I2-LP into a temporary life buff: the health you had when injecting is the baseline,
// the syringe adds on top of it (overheal past max allowed), and everything above the baseline
// drains back down at a fixed rate. Falling below the baseline cancels the buff on the spot.
internal static class SyringeLifeBuff
{
    // Health is set through the synced SendSetHealth, so it's stepped once a second rather than
    // every frame to keep network traffic down.
    private const float TickSeconds = 1f;
    private const float Epsilon = 0.01f;

    // AddHealth caps at max natively, so overheal is pushed with SendSetHealth afterwards. If the
    // game caps that too, max health is raised for the buff's duration; if even that doesn't
    // stick, the buff carries on without overheal.
    private const int OverhealAttempts = 2;

    private static IntPtr _owner;
    private static bool _active;
    private static float _baseline;
    private static float _target;
    private static float _originalMax;
    private static bool _maxRaised;
    private static int _overhealAttempt;
    private static float _lastSent = float.NaN;
    private static float _untilTick;

    // Set when a heal is about to land: the baseline has already moved up, health hasn't yet, and
    // that gap mustn't be read as damage.
    private static bool _cancelGrace;

    internal static float OriginalMaxHealth(Dam_SyncedDamageBase target) =>
        _active && _maxRaised && target.Pointer == _owner ? _originalMax : target.HealthMax;

    internal static void Grant(Dam_SyncedDamageBase target, float amount)
    {
        if (Plugin.HealthSyringeBuffDrainPercentPerSecond.Value <= 0f) return;

        if (!_active || _owner != target.Pointer)
        {
            Clear();
            _owner = target.Pointer;
            _active = true;
            _baseline = target.Health;
            _originalMax = target.HealthMax;
        }

        _target = CurrentHealth(target.Health) + amount;
        _overhealAttempt = 0;
        _lastSent = float.NaN;
        _untilTick = TickSeconds;
        _cancelGrace = true;
        Plugin.Logger.LogInfo($"I2-LP life buff: {_baseline:0.##} HP baseline, overhealing to {_target:0.##} HP (max {_originalMax:0.##}).");
    }

    internal static void OnOtherHeal(Dam_SyncedDamageBase target, float amount)
    {
        if (!_active || target.Pointer != _owner || amount <= 0f) return;

        _baseline = Math.Min(_originalMax, _baseline + amount);
        _lastSent = float.NaN;
        _cancelGrace = true;
        Plugin.Logger.LogInfo($"I2-LP life buff: heal of {amount:0.##} HP kept, baseline now {_baseline:0.##} HP.");
    }

    // Drops the buff from tracking. If the buff had raised HealthMax and the caller can hand back
    // the exact live instance that was raised, restores it first - otherwise an inflated max would
    // stick on that object forever, since nothing else ever lowers it back once tracking stops.
    // (Grant's own "different owner" call site never has that instance to hand back, but it also
    // never needs to: this patch only ever tracks the local player's own Dam_SyncedDamageBase, so
    // there's no real scenario where the owner changes out from under a still-raised buff.)
    internal static void Clear(Dam_SyncedDamageBase? sameInstance = null)
    {
        if (_maxRaised && sameInstance != null && sameInstance.Pointer == _owner)
        {
            sameInstance.HealthMax = _originalMax;
        }

        _owner = IntPtr.Zero;
        _active = false;
        _maxRaised = false;
        _lastSent = float.NaN;
        _cancelGrace = false;
    }

    internal static void Tick(Dam_PlayerDamageLocal local)
    {
        if (!_active || local.Pointer != _owner) return;

        if (!_cancelGrace && local.Health < _baseline - Epsilon)
        {
            End(local, "health fell below the pre-syringe level, cancelled");
            return;
        }

        _untilTick -= Time.deltaTime;
        if (_untilTick > 0f) return;
        var elapsed = TickSeconds - _untilTick;
        _untilTick = TickSeconds;
        _cancelGrace = false;

        var health = local.Health;
        if (health < _baseline - Epsilon)
        {
            End(local, "health fell below the pre-syringe level, cancelled");
            return;
        }

        if (_overhealAttempt <= OverhealAttempts && health < _target - Epsilon)
        {
            if (_overhealAttempt < OverhealAttempts)
            {
                if (_overhealAttempt == 1 && _target > local.HealthMax + Epsilon)
                {
                    local.HealthMax = _target;
                    _maxRaised = true;
                    Plugin.Logger.LogInfo($"I2-LP life buff: health capped at {health:0.##}, raising max to {_target:0.##} for the buff.");
                }
                local.SendSetHealth(_target);
                _lastSent = _target;
                _overhealAttempt++;
                return;
            }
            Plugin.Logger.LogWarning($"I2-LP life buff: overheal didn't stick (health {health:0.##}, wanted {_target:0.##}), draining from here.");
        }
        _overhealAttempt = OverhealAttempts + 1;

        var step = _originalMax * Plugin.HealthSyringeBuffDrainPercentPerSecond.Value / 100f * elapsed;
        var next = Math.Max(_baseline, CurrentHealth(health) - step);
        if (_maxRaised) local.HealthMax = Math.Max(_originalMax, next);
        local.SendSetHealth(next);
        _lastSent = next;

        if (next <= _baseline + Epsilon) End(local, "drained back to the pre-syringe level");
    }

    // On a client, Health can briefly read the pre-SendSetHealth value; trusting the last value
    // sent stops a stale read from undoing a drain step.
    private static float CurrentHealth(float health) => float.IsNaN(_lastSent) ? health : Math.Min(health, _lastSent);

    private static void End(Dam_PlayerDamageLocal local, string reason)
    {
        if (_maxRaised)
        {
            local.HealthMax = _originalMax;
            if (local.Health > _originalMax) local.SendSetHealth(_originalMax);
        }
        Plugin.Logger.LogInfo($"I2-LP life buff ended: {reason}.");
        Clear();
    }
}

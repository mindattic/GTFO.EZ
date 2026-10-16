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
        if (!SyringeApply_MoveNext_Patch.ApplyingHealthSyringe) return;

        var fraction = Plugin.HealthSyringeHealFraction.Value;
        if (fraction <= 0f) return;

        var vanillaAmount = addHealth;
        addHealth = __instance.HealthMax * fraction;
        Plugin.Logger.LogInfo($"I2-LP syringe heal: {vanillaAmount:0.##} -> {addHealth:0.##} HP (max {__instance.HealthMax:0.##}).");

        SyringeLifeBuff.Grant(__instance, Math.Min(addHealth, __instance.HealthMax - __instance.Health));
    }
}

[HarmonyPatch(typeof(Dam_PlayerDamageLocal))]
internal static class SyringeLifeBuff_Tick_Patch
{
    [HarmonyPatch(nameof(Dam_PlayerDamageLocal.Update))]
    [HarmonyPostfix]
    private static void Update_Postfix(Dam_PlayerDamageLocal __instance) => SyringeLifeBuff.Tick(__instance);

    // A new level means a new local player, so a buff still running from the last one is dropped.
    [HarmonyPatch(nameof(Dam_PlayerDamageLocal.Setup))]
    [HarmonyPostfix]
    private static void Setup_Postfix() => SyringeLifeBuff.Clear();
}

// Turns the I2-LP's heal into temporary health: whatever it actually added drains back out
// linearly over the configured duration. Damage taken meanwhile is paid out of the buff first,
// so less is left to drain, and the drain itself never takes the player below MinHealth.
internal static class SyringeLifeBuff
{
    // Health is set through the synced SendSetHealth, so it's stepped once a second rather than
    // every frame to keep network traffic down.
    private const float TickSeconds = 1f;
    private const float MinHealth = 1f;

    private static IntPtr _owner;
    private static float _remaining;
    private static float _drainPerSecond;
    private static float _lastHealth = float.NaN;
    private static float _untilTick;

    internal static void Grant(Dam_SyncedDamageBase target, float gained)
    {
        var duration = Plugin.HealthSyringeBuffDurationSeconds.Value;
        if (duration <= 0f || gained <= 0f) return;

        if (_owner != target.Pointer) Clear();
        _owner = target.Pointer;
        _remaining += gained;
        _drainPerSecond = _remaining / duration;
        // AddHealth hasn't landed yet (and may round-trip the network on a client), so the next
        // tick re-reads health rather than mistaking the pre-heal value for damage taken.
        _lastHealth = float.NaN;
        _untilTick = TickSeconds;
        Plugin.Logger.LogInfo($"I2-LP life buff: {gained:0.##} HP draining over {duration:0.#}s ({_remaining:0.##} HP buffed in total).");
    }

    internal static void Clear()
    {
        _owner = IntPtr.Zero;
        _remaining = 0f;
        _lastHealth = float.NaN;
    }

    internal static void Tick(Dam_PlayerDamageLocal local)
    {
        if (_remaining <= 0f || local.Pointer != _owner) return;

        _untilTick -= Time.deltaTime;
        if (_untilTick > 0f) return;
        var elapsed = TickSeconds - _untilTick;
        _untilTick = TickSeconds;

        var health = local.Health;
        if (health <= 0f)
        {
            Clear();
            return;
        }

        if (!float.IsNaN(_lastHealth) && health < _lastHealth) _remaining -= _lastHealth - health;
        if (_remaining <= 0f)
        {
            Plugin.Logger.LogInfo("I2-LP life buff absorbed by damage.");
            Clear();
            return;
        }

        var drain = Math.Min(_remaining, _drainPerSecond * elapsed);
        var newHealth = Math.Max(MinHealth, health - drain);
        _remaining = newHealth > MinHealth ? _remaining - drain : 0f;
        if (newHealth < health) local.SendSetHealth(newHealth);
        _lastHealth = newHealth;

        if (_remaining <= 0f)
        {
            Plugin.Logger.LogInfo("I2-LP life buff ended.");
            Clear();
        }
    }
}

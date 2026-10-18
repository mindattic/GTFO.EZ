using System;
using System.Collections.Generic;
using HarmonyLib;
using Player;
using UnityEngine;

namespace GTFO.EZ.Patches;

// G.EZ-T34 spike: Shift+E deploys the first bot with an undeployed tool at wherever the local
// player is currently aiming their own tool's placement ghost - reusing the exact validated
// pose/validity SentryGunFirstPerson already computes every frame for the player's own placement,
// rather than guessing a pose ourselves. There's no vanilla order for "deploy now" to route
// through instead (PlayerBotManager's order enum only has Guard/Follow/Stop/Sneak/Roam/Invite/
// Activate/Deactivate - no Deploy, no item reference), so this calls the bot AI's own placement
// method directly (exposed as public by the IL2CPP interop stubs). Picking a bot-placed deployable
// back up is left to whatever the normal vanilla E-interact already does with a placed item -
// confirmed to compile against the real interop assemblies, not confirmed safe by decompiling
// alone - every call here is guarded so a failure logs and moves on instead of breaking the mod.
[HarmonyPatch(typeof(Dam_PlayerDamageLocal), nameof(Dam_PlayerDamageLocal.Update))]
internal static class DeployPickupHotkeys_Update_Patch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        if (!Plugin.BotsDeployOnShiftE.Value) return;

        var shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (shiftHeld && Input.GetKeyDown(KeyCode.E)) BotDeployAtAim.TryDeployNext();
    }
}

internal static class Bots
{
    internal static List<PlayerAIBot> List()
    {
        var result = new List<PlayerAIBot>();
        foreach (var agent in PlayerManager.PlayerAgentsInLevel)
        {
            if (agent == null || agent.Owner == null || !agent.Owner.IsBot) continue;
            var bot = agent.GetComponent<PlayerAIBot>();
            if (bot != null) result.Add(bot);
        }
        return result;
    }
}

internal static class BotDeployAtAim
{
    internal static void TryDeployNext()
    {
        Pose pose;
        try
        {
            if (!TryGetAimPose(out pose))
            {
                Plugin.Logger.LogInfo("Shift+E: aim your own equipped tool at a valid placement spot first - that's the aim this reuses.");
                return;
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"Shift+E: couldn't read your placement aim. {e}");
            return;
        }

        List<PlayerAIBot> bots;
        try
        {
            bots = Bots.List();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"Shift+E: couldn't list bots. {e}");
            return;
        }

        foreach (var bot in bots)
        {
            try
            {
                var backpack = bot?.Backpack;
                if (backpack == null || !backpack.TryGetBackpackItem(InventorySlot.GearClass, out var backpackItem) || backpackItem == null) continue;
                if (backpack.IsDeployed(InventorySlot.GearClass)) continue;

                var desc = new PlayerBotActionDeploySentryGun.Descriptor(bot) { InstallationPose = pose };
                var action = new PlayerBotActionDeploySentryGun(desc) { m_backpackItem = backpackItem };
                action.PlaceSentryGunOnGround();
                Plugin.Logger.LogInfo("Shift+E: deployed a bot's tool at your aim.");
                return;
            }
            catch (Exception e)
            {
                Plugin.Logger.LogWarning($"Shift+E: deploy attempt failed for one bot, trying the next. {e}");
            }
        }

        Plugin.Logger.LogInfo("Shift+E: no bot has an undeployed tool to place.");
    }

    // Mirrors exactly what the player's own SentryGunFirstPerson.Update() already computes every
    // frame for their own placement ghost - m_placePos/m_cameraLookRot are the validated spot,
    // m_lastCanPlace is the same green/red check the player sees. No separate raycast of our own.
    private static bool TryGetAimPose(out Pose pose)
    {
        pose = default;

        var backpack = PlayerBackpackManager.LocalBackpack;
        if (backpack == null) return false;
        if (!backpack.TryGetBackpackItem(InventorySlot.GearClass, out var backpackItem) || backpackItem?.Instance == null) return false;

        var sentry = backpackItem.Instance.TryCast<SentryGunFirstPerson>();
        if (sentry == null || !sentry.m_lastCanPlace) return false;

        pose = new Pose(sentry.m_placePos, sentry.m_cameraLookRot);
        return true;
    }
}

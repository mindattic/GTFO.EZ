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

        var altHeld = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        if (altHeld && Input.GetKeyDown(KeyCode.E)) PickupAllBotTools.Activate();
    }
}

// The player's own placement already goes through the normal vanilla flow, but switches weapons
// back to whatever was equipped before pulling out the tool - vanilla leaves you empty-handed in
// the Tool slot afterward, which this treats as a missed step rather than desired behaviour.
[HarmonyPatch(typeof(SentryGunFirstPerson), nameof(SentryGunFirstPerson.PlaceOnGround))]
internal static class SentryGunFirstPerson_PlaceOnGround_Patch
{
    [HarmonyPostfix]
    private static void Postfix(SentryGunFirstPerson __instance)
    {
        try
        {
            var localBackpack = PlayerBackpackManager.LocalBackpack;
            if (localBackpack == null) return;
            if (!localBackpack.TryGetBackpackItem(InventorySlot.GearClass, out var backpackItem) || backpackItem?.Instance != __instance) return;

            localBackpack.TryClearSlot(InventorySlot.GearClass);

            if (!PlayerManager.TryGetLocalPlayerAgent(out var localAgent) || localAgent == null) return;
            var lastSlot = InventorySlot.None;
            if (!localBackpack.GetLastEquippedGear(ref lastSlot) || lastSlot == InventorySlot.None) return;
            if (!localBackpack.TryGetBackpackItem(lastSlot, out var previousItem) || previousItem?.Instance == null) return;
            var previousEquippable = previousItem.Instance.TryCast<ItemEquippable>();
            if (previousEquippable == null) return;

            localAgent.Inventory.SyncWieldItem(previousEquippable);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"Instant Sentry Placement: couldn't switch back to your previous weapon after placing. {e}");
        }
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
                backpack.TryClearSlot(InventorySlot.GearClass);
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

// Alt+E: instantly recalls every bot-placed tool still down, straight back into the bot that
// placed it, all at once - the reverse of Shift+E. Your own placed tool is untouched; walk up and
// interact with it as normal.
internal static class PickupAllBotTools
{
    internal static void Activate()
    {
        List<PlayerAIBot> bots;
        try
        {
            bots = Bots.List();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"Alt+E: couldn't list bots. {e}");
            return;
        }

        var recalled = 0;
        foreach (var bot in bots)
        {
            Il2CppSystem.Collections.Generic.List<ItemEquippable> deployedItems;
            try
            {
                if (!PlayerBotActionGatherDeployables.Descriptor.Evaluate(bot, out deployedItems) || deployedItems == null) continue;
            }
            catch (Exception e)
            {
                Plugin.Logger.LogWarning($"Alt+E: couldn't check a bot's deployed items, skipping it. {e}");
                continue;
            }

            foreach (var item in deployedItems)
            {
                try
                {
                    var desc = new PlayerBotActionGatherDeployables.Descriptor(bot);
                    var action = new PlayerBotActionGatherDeployables(desc) { m_targetItem = item };
                    action.PickUpItem();
                    recalled++;
                }
                catch (Exception e)
                {
                    Plugin.Logger.LogWarning($"Alt+E: couldn't recall one item, skipping it. {e}");
                }
            }
        }

        Plugin.Logger.LogInfo($"Alt+E: recalled {recalled} bot tool(s).");
    }
}

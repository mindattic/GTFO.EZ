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
        if (!Plugin.BotsDeployOnShiftE.Value || !Hotkeys.InPlay) return;

        if (Hotkeys.ShiftHeld && Input.GetKeyDown(KeyCode.E)) BotDeployAtAim.TryDeployNext();

        if ((Hotkeys.AltHeld && Input.GetKeyDown(KeyCode.E)) || Input.GetKeyDown(KeyCode.Z)) PickupAllBotTools.Activate();

        if (Input.GetKeyDown(KeyCode.X)) OwnSentryAutoPlace.Place();
    }
}

internal static class Hotkeys
{
    // Only in normal first-person play - not while typing at a terminal or in chat, in the Q comms
    // menu, on the map, etc., where these letters are just text or menu input.
    internal static bool InPlay => FocusStateManager.CurrentState == eFocusState.FPS;
    internal static bool ShiftHeld => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    internal static bool AltHeld => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
}

// With sentry placement made instant, the plain E inside Shift+E / Alt+E would also complete the
// vanilla placement of your own held sentry in the same press. Hold that back for your own sentry
// only - a bot's placement and X's own call go through.
[HarmonyPatch(typeof(SentryGunFirstPerson), nameof(SentryGunFirstPerson.PlaceOnGround))]
internal static class SentryGunFirstPerson_PlaceOnGround_ModifierGuard
{
    [HarmonyPrefix]
    private static bool Prefix(SentryGunFirstPerson __instance)
    {
        if (OwnSentryAutoPlace.Placing || !Plugin.BotsDeployOnShiftE.Value) return true;
        if (!Hotkeys.ShiftHeld && !Hotkeys.AltHeld) return true;
        var own = BotDeployAtAim.LocalSentry();
        return own == null || own.Pointer != __instance.Pointer;
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
            // Pointer, not ==: two Il2Cpp wrappers around the same native object aren't reference-equal.
            if (!localBackpack.TryGetBackpackItem(InventorySlot.GearClass, out var backpackItem) || backpackItem?.Instance?.Pointer != __instance.Pointer) return;

            if (!PlayerManager.TryGetLocalPlayerAgent(out var localAgent) || localAgent?.Inventory == null) return;
            // Only when you were actually holding the sentry - X places it from the backpack while
            // you hold something else, and that weapon must stay in your hands.
            if (localAgent.Inventory.WieldedSlot != InventorySlot.GearClass) return;
            var lastSlot = InventorySlot.None;
            if (!localBackpack.GetLastEquippedGear(ref lastSlot) || lastSlot == InventorySlot.None || lastSlot == InventorySlot.GearClass) return;
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
                // Marks it placed-but-still-owned, the bookkeeping the bot's own AI does after this
                // call. Clearing the slot instead (an earlier version) orphaned the turret, which
                // broke bot item commands and gave Alt+E nothing to recall.
                backpack.SetDeployed(InventorySlot.GearClass, true);
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
    internal static bool TryGetAimPose(out Pose pose)
    {
        pose = default;

        var sentry = LocalSentry();
        // Only while it's actually in hand: m_placePos/m_lastCanPlace stop updating once it's put
        // away, so reading them then would reuse wherever it was last aimed.
        if (sentry == null || !sentry.m_lastCanPlace || !IsWielded(InventorySlot.GearClass)) return false;

        pose = new Pose(sentry.m_placePos, sentry.m_cameraLookRot);
        return true;
    }

    internal static SentryGunFirstPerson? LocalSentry()
    {
        var backpack = PlayerBackpackManager.LocalBackpack;
        if (backpack == null) return null;
        if (!backpack.TryGetBackpackItem(InventorySlot.GearClass, out var backpackItem) || backpackItem?.Instance == null) return null;
        return backpackItem.Instance.TryCast<SentryGunFirstPerson>();
    }

    private static bool IsWielded(InventorySlot slot) =>
        PlayerManager.TryGetLocalPlayerAgent(out var agent) && agent?.Inventory != null && agent.Inventory.WieldedSlot == slot;
}

// X: places your own sentry without pulling it out first - at your current placement aim if you're
// already holding it on a valid spot, otherwise on the ground a step in front of you, facing where
// you look. Goes through the sentry's own PlaceOnGround, the same call a normal placement ends in.
internal static class OwnSentryAutoPlace
{
    private const float StepAhead = 1.5f;
    private const float GroundProbeHeight = 1.5f;
    private const float GroundProbeDepth = 4f;

    // Lets X's own PlaceOnGround call past the Shift/Alt modifier guard.
    internal static bool Placing;

    internal static void Place()
    {
        try
        {
            var backpack = PlayerBackpackManager.LocalBackpack;
            var sentry = BotDeployAtAim.LocalSentry();
            if (backpack == null || sentry == null)
            {
                Plugin.Logger.LogInfo("X: you don't have a sentry gun equipped.");
                return;
            }
            if (backpack.IsDeployed(InventorySlot.GearClass))
            {
                Plugin.Logger.LogInfo("X: your sentry is already placed.");
                return;
            }

            if (!BotDeployAtAim.TryGetAimPose(out var pose) && !TryGroundAhead(out pose))
            {
                Plugin.Logger.LogInfo("X: no ground in front of you to place on.");
                return;
            }

            sentry.m_placePos = pose.position;
            sentry.m_cameraLookRot = pose.rotation;
            sentry.m_lastCanPlace = true;
            Placing = true;
            sentry.PlaceOnGround();
            // PlaceOnGround leaves the yellow ground arrow showing; the vanilla flow only hides it
            // when you put the sentry away, which never happens when X places it from the backpack.
            sentry.m_placementIndicator?.SetVisible(false);
            Plugin.Logger.LogInfo("X: placed your sentry.");
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"X: couldn't place your sentry. {e}");
        }
        finally
        {
            Placing = false;
        }
    }

    private static bool TryGroundAhead(out Pose pose)
    {
        pose = default;
        if (!PlayerManager.TryGetLocalPlayerAgent(out var agent) || agent == null) return false;

        var forward = agent.FPSCamera != null ? agent.FPSCamera.FlatForward : agent.transform.forward;
        var origin = agent.transform.position + forward * StepAhead + Vector3.up * GroundProbeHeight;
        if (!Physics.Raycast(origin, Vector3.down, out var hit, GroundProbeDepth, LayerManager.MASK_WORLD)) return false;

        pose = new Pose(hit.point, Quaternion.LookRotation(forward, Vector3.up));
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
        // Placed sentries are found in the level and matched to their bot through the backpack each
        // one records it belongs to. Neither backpack lookup works for this: the game's
        // GatherDeployables.Descriptor.Evaluate wrapper throws a TypeLoadException on Il2CppSystem
        // List`1, and TryGetDeployedItem finds no replicator for a sentry Shift+E placed (the game
        // logs "Non replicatior found with key 0").
        UnityEngine.Object[] placed;
        try
        {
            placed = UnityEngine.Object.FindObjectsOfType(Il2CppInterop.Runtime.Il2CppType.Of<SentryGunInstance>());
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"Alt+E: couldn't list placed sentries. {e}");
            return;
        }

        foreach (var obj in placed)
        {
            try
            {
                var sentry = obj?.TryCast<SentryGunInstance>();
                var owner = sentry?.m_belongsToBackpack;
                if (sentry == null || owner == null) continue;

                PlayerAIBot? bot = null;
                foreach (var candidate in bots)
                    if (candidate?.Backpack != null && candidate.Backpack.Pointer == owner.Pointer) bot = candidate;
                if (bot == null) continue; // yours, or a teammate's - not a bot's

                var desc = new PlayerBotActionGatherDeployables.Descriptor(bot);
                var action = new PlayerBotActionGatherDeployables(desc) { m_targetItem = sentry };
                action.PickUpItem();
                recalled++;
            }
            catch (Exception e)
            {
                Plugin.Logger.LogWarning($"Alt+E: couldn't recall a bot's sentry, skipping it. {e}");
            }
        }

        Plugin.Logger.LogInfo($"Alt+E: recalled {recalled} bot sentry(s) of {placed.Length} placed in the level.");
    }
}

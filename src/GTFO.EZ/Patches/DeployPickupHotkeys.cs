using System;
using System.Collections.Generic;
using HarmonyLib;
using Player;
using SNetwork;
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
    private static void Postfix(Dam_PlayerDamageLocal __instance)
    {
        if (!LocalPlayerDamage.IsMine(__instance)) return;
        if (!Plugin.BotsDeployOnShiftE.Value || !Hotkeys.InPlay) return;

        // Bots belong to the host: a client running these would only move its own copy of the bot's
        // turret, leaving host and client disagreeing about it. X (your own sentry) works anywhere.
        // The guard is armed on a client too, so Shift+E means the same thing for everyone: never
        // your own placement.
        if (Hotkeys.ShiftHeld && Input.GetKeyDown(KeyCode.E))
        {
            Hotkeys.ShiftEPressedAt = Time.time;
            if (SNet.IsMaster) BotDeployAtAim.TryDeployNext();
        }

        if (SNet.IsMaster && Input.GetKeyDown(KeyCode.Z)) PickupAllBotTools.Activate();

        if (Input.GetKeyDown(KeyCode.X)) OwnSentryAutoPlace.Place();
    }
}

internal static class Hotkeys
{
    // Only in normal first-person play - not while typing at a terminal or in chat, in the Q comms
    // menu, on the map, etc., where these letters are just text or menu input.
    internal static bool InPlay => FocusStateManager.CurrentState == eFocusState.FPS;
    internal static bool ShiftHeld => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

    // When the last Shift+E bot deploy was pressed. Only the vanilla placement that same press
    // triggers is held back - plain E with Shift held for sprinting still places your own sentry.
    internal static float ShiftEPressedAt = -10f;
    internal const float ShiftEPressWindow = 0.25f;
}

// With sentry placement made instant, the plain E inside Shift+E would also complete the
// vanilla placement of your own held sentry in the same press. Hold that back for your own sentry
// only - a bot's placement and X's own call go through.
[HarmonyPatch(typeof(SentryGunFirstPerson), nameof(SentryGunFirstPerson.PlaceOnGround))]
internal static class SentryGunFirstPerson_PlaceOnGround_ModifierGuard
{
    [HarmonyPrefix]
    private static bool Prefix(SentryGunFirstPerson __instance)
    {
        if (OwnSentryAutoPlace.Placing || !Plugin.BotsDeployOnShiftE.Value) return true;
        // The press itself counts too: the sentry's own update can place it this frame before the
        // hotkey postfix has stamped ShiftEPressedAt (Unity doesn't order the two components).
        var pressedNow = Input.GetKeyDown(KeyCode.E);
        if (!Hotkeys.ShiftHeld || (!pressedNow && Time.time - Hotkeys.ShiftEPressedAt > Hotkeys.ShiftEPressWindow)) return true;
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
    private static void Postfix(SentryGunFirstPerson __instance, bool __runOriginal)
    {
        // Postfixes run even when a prefix skipped the original: when the Shift+E guard held your
        // own placement back, nothing was placed and you're still aiming the sentry for the bot.
        if (!__runOriginal) return;
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
                // broke bot item commands and gave Z nothing to recall.
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
    // Steeper than this (stairs edges, railings, crate corners) isn't ground a sentry stands on.
    private const float MaxGroundSlope = 30f;
    // Aims the sight check just above the ground so it isn't blocked by the floor it's checking.
    private const float SightLift = 0.2f;

    // Lets X's own PlaceOnGround call past the Shift modifier guard.
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

            // Downed (or dead), you can't place a tool the normal way either.
            if (!PlayerManager.TryGetLocalPlayerAgent(out var agent) || agent == null || !agent.Alive)
            {
                Plugin.Logger.LogInfo("X: you can't place your sentry while downed.");
                return;
            }

            // The aim pose is the game's own validated spot (m_lastCanPlace is already true there);
            // the fallback only counts once its probe has proved a flat spot you can see.
            if (!BotDeployAtAim.TryGetAimPose(out var pose) && !TryGroundAhead(agent, out pose))
            {
                Plugin.Logger.LogInfo("X: no clear ground in front of you to place on.");
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

    private static bool TryGroundAhead(PlayerAgent agent, out Pose pose)
    {
        pose = default;
        if (agent.FPSCamera == null) return false;

        var forward = agent.FPSCamera.FlatForward;
        var origin = agent.transform.position + forward * StepAhead + Vector3.up * GroundProbeHeight;
        if (!Physics.Raycast(origin, Vector3.down, out var hit, GroundProbeDepth, LayerManager.MASK_WORLD)) return false;
        if (Vector3.Angle(hit.normal, Vector3.up) > MaxGroundSlope) return false;

        // The probe starts a step ahead, so on its own it finds floor on the far side of a wall or
        // closed door. The spot only counts if your camera can see it, blocked by the same mask the
        // game's own sentry placement aim uses, plus world geometry.
        var eye = agent.FPSCamera.Position;
        var sightMask = LayerManager.MASK_WORLD | LayerManager.MASK_SENTRYGUN_CAMERARAY;
        if (Physics.Linecast(eye, origin, sightMask)) return false;
        if (Physics.Linecast(eye, hit.point + Vector3.up * SightLift, sightMask)) return false;

        pose = new Pose(hit.point, Quaternion.LookRotation(forward, Vector3.up));
        return true;
    }
}

// Z: instantly recalls every bot-placed tool still down, straight back into the bot that
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
            Plugin.Logger.LogWarning($"Z: couldn't list bots. {e}");
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
            Plugin.Logger.LogWarning($"Z: couldn't list placed sentries. {e}");
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
                Plugin.Logger.LogWarning($"Z: couldn't recall a bot's sentry, skipping it. {e}");
            }
        }

        Plugin.Logger.LogInfo($"Z: recalled {recalled} bot sentry(s) of {placed.Length} placed in the level.");
    }
}

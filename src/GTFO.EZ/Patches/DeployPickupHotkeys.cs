using System;
using System.Collections.Generic;
using HarmonyLib;
using Player;
using UnityEngine;

namespace GTFO.EZ.Patches;

// GE-T34 spike: two experimental hotkeys. Neither touches the local human player's own normal
// deploy/pickup - those stay vanilla. Both call private bot-AI methods (exposed as public by the
// IL2CPP interop stubs) directly, bypassing the bot's own Idle/Move/Interact travel-and-wait cycle,
// since there's no vanilla "order a bot to deploy/pick up right now" command to route through
// instead (PlayerBotManager's order enum only has Guard/Follow/Stop/Sneak/Roam/Invite/
// Activate/Deactivate - no Deploy, no Pickup, no item reference). Only confirmed to compile against
// the real interop assemblies, not confirmed safe by any decompile - every call here is guarded so
// a failure logs and moves on instead of breaking the mod or the game.
[HarmonyPatch(typeof(Dam_PlayerDamageLocal), nameof(Dam_PlayerDamageLocal.Update))]
internal static class DeployPickupHotkeys_Update_Patch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        if (Input.GetKeyDown(Plugin.BotsDeployKey.Value)) BotDeploySequence.Start();
        BotDeploySequence.Tick(Time.deltaTime);

        if (Input.GetKeyDown(Plugin.PickupAllDeployablesKey.Value)) PickupAllDeployables.Activate();
    }
}

// Shared by both hotkeys: the local human player's own PlayerAgent has no SNet_Player.IsBot, so a
// plain foreach over PlayerAgentsInLevel (an Il2Cpp list - LINQ doesn't bind against it cleanly)
// picks out just the bot companions.
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

// O: deploys bot1's equipped tool at the calling player's own position/facing, waits for
// confirmation it's actually down, then bot2, then bot3 - one bot at a time, same shape as the
// vanilla COMS order flow the user already relies on, except the "wait for confirmation" check
// reads real backpack state instead of watching the menu.
internal static class BotDeploySequence
{
    private const float PerBotTimeoutSeconds = 6f;

    private static List<PlayerAIBot> _queue = new();
    private static int _index;
    private static bool _waiting;
    private static float _timeLeft;

    internal static void Start()
    {
        if (_waiting) return; // a sequence is already running - let it finish before starting another

        try
        {
            _queue = Bots.List();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"Bots Deploy Key: couldn't list bots, aborting. {e}");
            _queue = new List<PlayerAIBot>();
            return;
        }

        if (_queue.Count == 0)
        {
            Plugin.Logger.LogInfo("Bots Deploy Key: no bots in the level.");
            return;
        }

        _index = 0;
        TryDeployCurrent();
    }

    internal static void Tick(float deltaTime)
    {
        if (!_waiting) return;

        var bot = _queue[_index];
        bool deployed;
        try
        {
            deployed = bot == null || (bot.Backpack?.IsDeployed(InventorySlot.GearClass) ?? true);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"Bots Deploy Key: couldn't check deploy state, moving on. {e}");
            deployed = true;
        }

        _timeLeft -= deltaTime;
        if (!deployed && _timeLeft > 0f) return;

        if (!deployed) Plugin.Logger.LogWarning("Bots Deploy Key: timed out waiting for a bot to deploy, moving to the next one.");
        Advance();
    }

    private static void Advance()
    {
        _waiting = false;
        _index++;
        if (_index >= _queue.Count)
        {
            Plugin.Logger.LogInfo("Bots Deploy Key: sequence finished.");
            return;
        }
        TryDeployCurrent();
    }

    private static void TryDeployCurrent()
    {
        var bot = _queue[_index];
        try
        {
            var backpack = bot.Backpack;
            if (backpack == null || !backpack.TryGetBackpackItem(InventorySlot.GearClass, out var backpackItem) || backpackItem == null)
            {
                Plugin.Logger.LogInfo("Bots Deploy Key: a bot has no tool equipped, skipping it.");
                Advance();
                return;
            }

            if (backpack.IsDeployed(InventorySlot.GearClass))
            {
                Advance(); // already placed from an earlier press - nothing to do
                return;
            }

            // Placed at the calling player's own position/facing, not the bot's - the bot doesn't
            // walk there first, so a pose from wherever it happens to be standing could be
            // anywhere; the player aiming the spot is the one piece of human judgement in this
            // otherwise-instant flow.
            Pose pose;
            if (PlayerManager.TryGetLocalPlayerAgent(out var localAgent) && localAgent != null)
            {
                pose = new Pose(localAgent.transform.position, localAgent.transform.rotation);
            }
            else
            {
                Plugin.Logger.LogWarning("Bots Deploy Key: couldn't find the local player, skipping this bot.");
                Advance();
                return;
            }

            var desc = new PlayerBotActionDeploySentryGun.Descriptor(bot) { InstallationPose = pose };
            var action = new PlayerBotActionDeploySentryGun(desc) { m_backpackItem = backpackItem };
            action.PlaceSentryGunOnGround();

            _waiting = true;
            _timeLeft = PerBotTimeoutSeconds;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"Bots Deploy Key: deploy attempt failed, skipping this bot. {e}");
            Advance();
        }
    }
}

// P: instantly recalls every deployable a bot has placed and hasn't retrieved yet, all bots, one
// press. The local human player's own pickup is untouched - still the normal walk-up-and-interact.
internal static class PickupAllDeployables
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
            Plugin.Logger.LogWarning($"Pickup All Deployables Key: couldn't list bots, aborting. {e}");
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
                Plugin.Logger.LogWarning($"Pickup All Deployables Key: couldn't check a bot's deployed items, skipping it. {e}");
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
                    Plugin.Logger.LogWarning($"Pickup All Deployables Key: couldn't recall one item, skipping it. {e}");
                }
            }
        }

        Plugin.Logger.LogInfo($"Pickup All Deployables Key: recalled {recalled} deployable(s).");
    }
}

using System;
using HarmonyLib;
using Player;
using UnityEngine;

namespace GTFO.EZ.Patches;

// Hold Shift and double-tap W/A/S/D for a quick dash that way - mainly to duck out of the way of a
// homing projectile on short notice. Moves the player through the game's own character controller
// with a wall-stopping SphereCast each step, the same approach already proven in GTFO.AbsurdWeapons'
// Katana dash (src/GTFO.AbsurdWeapons/KatanaDash.cs), so it collides and syncs like any other
// movement rather than teleporting through geometry.
[HarmonyPatch(typeof(Dam_PlayerDamageLocal), nameof(Dam_PlayerDamageLocal.Update))]
internal static class DashHotkey_Update_Patch
{
    // Key, then its direction relative to the camera: (right, forward).
    private static readonly (KeyCode Key, Vector2 Dir)[] DirectionKeys =
    {
        (KeyCode.W, new Vector2(0f, 1f)),
        (KeyCode.S, new Vector2(0f, -1f)),
        (KeyCode.A, new Vector2(-1f, 0f)),
        (KeyCode.D, new Vector2(1f, 0f)),
    };

    private static readonly float[] LastTap = { -10f, -10f, -10f, -10f };

    [HarmonyPostfix]
    private static void Postfix()
    {
        DashRunner.Tick();

        if (!Plugin.DashEnabled.Value || !Hotkeys.InPlay) return;

        for (var i = 0; i < DirectionKeys.Length; i++)
        {
            if (!Input.GetKeyDown(DirectionKeys[i].Key)) continue;

            var sincePrevious = Time.time - LastTap[i];
            LastTap[i] = Time.time;
            if (Hotkeys.ShiftHeld && sincePrevious <= Plugin.DashDoubleTapWindowSeconds.Value)
            {
                LastTap[i] = -10f; // a third tap starts a fresh pair rather than chaining off this one
                DashRunner.TryStart(DirectionKeys[i].Dir);
            }
        }
    }
}

internal static class DashRunner
{
    private const float WallProbeRadius = 0.3f;
    private const float WallSkin = 0.15f;

    private static float _cooldownUntil;
    private static PlayerAgent? _player;
    private static Vector3 _direction;
    private static float _timeLeft;

    // local: x = right, y = forward, relative to where you're looking (flattened to the ground).
    internal static void TryStart(Vector2 local)
    {
        if (_player != null || Time.time < _cooldownUntil) return;
        // A zero duration would divide distance by 0 into a NaN step and move the player to NaN.
        if (Plugin.DashSeconds.Value <= 0f || Plugin.DashDistance.Value <= 0f) return;

        try
        {
            if (!PlayerManager.TryGetLocalPlayerAgent(out var agent) || agent == null || !agent.Alive) return;

            var forward = agent.FPSCamera != null ? agent.FPSCamera.FlatForward : agent.transform.forward;
            var right = agent.FPSCamera != null ? agent.FPSCamera.FlatRight : agent.transform.right;

            var direction = forward * local.y + right * local.x;
            if (direction.sqrMagnitude < 0.0001f) return;

            _player = agent;
            _direction = direction.normalized;
            _timeLeft = Plugin.DashSeconds.Value;
            _cooldownUntil = Time.time + Plugin.DashCooldownSeconds.Value;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"Dash: couldn't start. {e}");
            _player = null;
        }
    }

    internal static void Tick()
    {
        if (_player == null) return;

        try
        {
            if (!_player.Alive)
            {
                _player = null;
                return;
            }

            var dt = Mathf.Min(Time.deltaTime, _timeLeft);
            _timeLeft -= dt;
            var from = _player.Position;
            var step = Plugin.DashDistance.Value / Plugin.DashSeconds.Value * dt;

            // A wall or prop ahead within this step stops the dash short, same probe shape as the
            // Katana dash - looked ahead rather than measured after moving, since the agent's
            // position only catches up later.
            var blocked = Physics.SphereCast(from + Vector3.up, WallProbeRadius, _direction, out var wall, step + WallSkin, LayerManager.MASK_WORLD);
            if (blocked) step = Mathf.Max(0f, wall.distance - WallSkin);

            _player.PlayerCharacterController?.Move(_direction * step);

            if (blocked || _timeLeft <= 0f) _player = null;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"Dash: step failed. {e}");
            _player = null;
        }
    }
}

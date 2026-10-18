using System;
using HarmonyLib;
using Player;
using UnityEngine;

namespace GTFO.EZ.Patches;

// Double-tap Shift (+ a movement direction) for a quick dash - mainly to duck out of the way of a
// homing projectile on short notice. Moves the player through the game's own character controller
// with a wall-stopping SphereCast each step, the same approach already proven in GTFO.AbsurdWeapons'
// Katana dash (src/GTFO.AbsurdWeapons/KatanaDash.cs), so it collides and syncs like any other
// movement rather than teleporting through geometry.
[HarmonyPatch(typeof(Dam_PlayerDamageLocal), nameof(Dam_PlayerDamageLocal.Update))]
internal static class DashHotkey_Update_Patch
{
    private static float _lastShiftPressTime = -10f;

    [HarmonyPostfix]
    private static void Postfix()
    {
        DashRunner.Tick();

        if (!Plugin.DashEnabled.Value) return;

        var shiftDown = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
        if (!shiftDown) return;

        var sincePrevious = Time.time - _lastShiftPressTime;
        _lastShiftPressTime = Time.time;
        if (sincePrevious <= Plugin.DashDoubleTapWindowSeconds.Value) DashRunner.TryStart();
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

    internal static void TryStart()
    {
        if (_player != null || Time.time < _cooldownUntil) return;

        try
        {
            if (!PlayerManager.TryGetLocalPlayerAgent(out var agent) || agent == null || !agent.Alive) return;

            var forward = agent.FPSCamera != null ? agent.FPSCamera.FlatForward : agent.transform.forward;
            var right = agent.FPSCamera != null ? agent.FPSCamera.FlatRight : agent.transform.right;

            var moveX = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
            var moveZ = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);

            var direction = forward * moveZ + right * moveX;
            if (direction.sqrMagnitude < 0.0001f) direction = forward; // no direction held - dash forward

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

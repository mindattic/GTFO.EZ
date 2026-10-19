using Player;

namespace GTFO.EZ.Patches;

// The host's bots have a Dam_PlayerDamageLocal too, so its Update and Setup run once per bot as
// well as for you - which made one Shift+E deploy several bots' tools, sped up Fog Clearing's ramp,
// and let a bot joining mid-level reset the fog sphere or clear a ping. Every hook on it checks this
// first so only your own component counts.
internal static class LocalPlayerDamage
{
    internal static bool IsMine(Dam_PlayerDamageLocal damage)
    {
        var agent = damage.Owner;
        return agent != null && agent.Owner != null && !agent.Owner.IsBot;
    }
}

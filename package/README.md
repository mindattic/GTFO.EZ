# GtfoEZ

A small, config-driven BepInEx plugin for GTFO that tunes player stats for an easier, more
relaxed co-op experience. No datablock JSON files are shipped — it patches the game's own data
in memory at load time, so it keeps working across game patches that only change base values.

## What it changes

Defaults follow community-consensus balance values (several independent GTFO easy-mode mods'
settled-on numbers), not arbitrary multipliers:

| Stat | Vanilla | Default |
|---|---|---|
| Max health | 25 | 50 (x2) |
| Health regen rate | 0.2/s | 1.0/s (x5) |
| Health regen cap (no med kit) | 20% | 40% (x2) |
| Health regen delay after damage | 5s | unchanged |
| Stamina regen while in combat | lower than out-of-combat | equal to out-of-combat |
| Fall damage amount | 2-15 | unchanged |
| Fall height before any damage | 4m | 8m (x2) |
| Fall height where damage maxes out | 20m | 30m (x1.5) |
| Weapon/tool ammo (reserve, starting, refill packs) | x1 | x1.5 |
| Flashlight angle & intensity | x1 | x1.25 |
| Enemy movement-noise detection distance | 8m | 6m (x0.75) |

A few deliberate non-changes: regen delay after damage is left at vanilla (fast regen with no
delay makes you nearly unkillable in a slow fight), ammo isn't paired with a damage buff (that
combination was reported as overtuned by another mod's author), and flashlight range is untouched
(a longer beam reaches past where the level's lighting was designed to work - angle/intensity
improve the same cone without that side effect).

Every value above is a multiplier (or on/off switch) you can retune in the config file BepInEx
generates after first launch (`BepInEx/config/MindAttic.GtfoEZ.cfg`), without recompiling anything.

## Install

Everyone in the lobby needs the same mod and config. Use Thunderstore Mod Manager or r2modman,
or import the zip manually and share a profile code.

## Credit

Default values were cross-referenced against several existing GTFO easy-mode mods' changelogs and
settled-on balance choices, including Mendu's [EasyMode](https://thunderstore.io/c/gtfo/p/Mendu/EasyMode/),
GTFriendlyO, and Friendly GTFO. This mod is an independent reimplementation as a code plugin rather
than datablock JSON, built to survive game updates without needing a rebuild for every patch.

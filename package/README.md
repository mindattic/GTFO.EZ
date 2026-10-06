# GTFO.EZ

*(Listed on Thunderstore as `GtfoEZ` — package names there are restricted to letters, numbers,
and underscores, so they can't contain a period.)*

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
generates after first launch (`BepInEx/config/MindAttic.GTFO.EZ.cfg`), without recompiling anything.

## Install

Install via [Thunderstore Mod Manager or r2modman](https://thunderstore.io/c/gtfo/p/MindAttic/GtfoEZ/),
or import the zip manually and share a profile code. Everyone in the lobby needs the same mod and
config.

## Attributions

This mod is an independent, from-scratch implementation (no code or data copied from any of the
projects below) built as a code plugin rather than datablock JSON, specifically so it survives
game updates without needing a rebuild for every patch. Its design still owes a real debt to
others' prior work:

- **[Mendu](https://thunderstore.io/c/gtfo/p/Mendu/EasyMode/)** — creator of EasyMode, the original
  datablock-JSON mod whose feature list (health, regen, stamina, fall damage, ammo, flashlights,
  detection range) this mod reimplements.
- **GTFriendlyO** (Carb_Crusaders / Heaveness) and **Friendly GTFO** (EcoLight) — their published
  changelogs and balance iterations (including a health multiplier that was tried higher and
  walked back) are what this mod's default values are tuned against, rather than guessing.
- **Team_Chicken**, creator of *Health And Melee Tweaks* — demonstrated the core technique this
  mod is built on: editing the game's own loaded `PlayerDataBlock` from a plugin instead of
  shipping replacement JSON, so the game's built-in systems keep doing the work.
- **atime1pm**, creator of *LowSpecGaming* — reference example for iterating every
  `FlashlightSettingsDataBlock` via `GameDataBlockBase<T>.GetAllBlocks()`.
- **The [GTFO-Modding](https://github.com/GTFO-Modding) community** — the GTDO wiki's datablock
  reference and the GTFO-API source are what made it possible to find the real field names
  (`health`, `movementDetectionDistance`, etc.) and the `GameDataInit.Initialize` hook point this
  mod relies on, instead of guessing at an undocumented API.
- **The [BepInEx](https://github.com/BepInEx/BepInEx) team** — the modding framework and loader
  this mod (and essentially every other GTFO mod) runs on.

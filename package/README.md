# GTFO.EZ

*(Listed on Thunderstore as `GtfoEZ` — package names there are restricted to letters, numbers,
and underscores, so they can't contain a period.)*

A small, config-driven BepInEx plugin for GTFO that tunes player stats for an easier, more
relaxed co-op experience. No datablock JSON files are shipped — it patches the game's own data
in memory at load time, so it keeps working across game patches that only change base values.

## What it changes

Defaults follow community-consensus balance values (several independent GTFO easy-mode mods'
settled-on numbers), not arbitrary multipliers. "Default" is the value after this mod's default
config is applied; `-` means left at vanilla:

| Stat | Vanilla | Default |
|---|---|---|
| Max health | 25 | 50 (+100%) |
| Health regen rate | 0.2/s | 1.0/s (+400%) |
| Health regen cap (no med kit) | 20% | 40% (+100%) |
| Health regen delay after damage | 5s | - |
| Stamina regen while in combat | lower than out-of-combat | equal to out-of-combat |
| Fall damage amount | 2-15 | - |
| Fall height before any damage | 4m | 8m (+100%) |
| Fall height where damage maxes out | 20m | 30m (+50%) |
| Weapon/tool ammo (reserve, starting, refill packs) | x1 | x1.5 (+50%) |
| Enemy movement-noise detection distance | 8m | 6m (-25%) |

A few deliberate non-changes among the tuned stats: regen delay after damage is left at vanilla
(fast regen with no delay makes you nearly unkillable in a slow fight), fall damage amount itself
is untouched (the height changes above already do most of the work), and ammo isn't paired with a
damage buff (that combination was reported as overtuned by another mod's author).

Every value above is a multiplier (or on/off switch) you can retune in the config file BepInEx
generates after first launch (`BepInEx/config/MindAttic.GTFO.EZ.cfg`), without recompiling anything.

## Also exposed, but left at vanilla by default

These are tunable in the same config file, but ship unchanged — either because vanilla was
already fine (flashlights), or because there's no community consensus they need adjusting for an
easier game, unlike the stats above:

| Stat | Vanilla | Default |
|---|---|---|
| Flashlight angle | x1 | - |
| Flashlight intensity | x1 | - |
| Walk speed | 3.5 m/s | - |
| Run speed | 6 m/s | - |
| Air (mid-air) move speed | 3 m/s | - |
| Crouch move speed | 2 m/s | - |
| Ladder climb speed | 1.5 m/s | - |
| Jump height (initial jump velocity) | 9 m/s | - |
| Friendly fire damage multiplier | x0.5 | - |
| Battery capacity (flashlights/tools) | 100 | - |
| Small tool battery drain rate | 0.25/s | - |
| Medium tool battery drain rate | 0.5/s | - |
| Large tool battery drain rate | 3/s | - |
| No-air (suffocation) damage rate | 0.15 | - |
| No-air meter depletion time | 90s | - |

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

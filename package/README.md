# GTFO.EZ

*(Listed on Thunderstore as `GtfoEZ` — package names there are restricted to letters, numbers,
and underscores, so they can't contain a period.)*

A small, config-driven BepInEx plugin for GTFO that tunes player stats for an easier, more
relaxed co-op experience. No datablock JSON files are shipped — it patches the game's own data
in memory at load time, so it keeps working across game patches that only change base values.
A curated set of defaults covers the core experience, with 155 customizable fields underneath
for anyone who wants to retune things further.

## What it changes

Defaults follow community-consensus balance values (several independent GTFO easy-mode mods'
settled-on numbers), not arbitrary multipliers. "Default" is the value after this mod's default
config is applied; `-` means left at vanilla:

| Stat | Vanilla | Default |
|---|---|---|
| Max health | 25 | 50 (+100%) |
| Health regen rate | 0.2/s | 1.0/s (+400%) |
| Health regen cap (no med kit) | 20% | 40% (+100%) |
| Stamina regen rate while resting in combat | 0.15/s (vs 0.25/s out of combat) | 0.25/s, equalized |
| Stamina cap while in combat | 90% | 100%, cap removed |
| Fall damage amount | 2-15 | 0.2-1.5 (-90%) |
| Fall height where damage maxes out | 20m | 30m (+50%) |
| Weapon/tool ammo (reserve, starting, refill packs) | x1 | x1.5 (+50%) |
| Enemy movement-noise detection distance | 8m (sleepers: 20m) | 6m (sleepers: 15m) (-25%) |
| Glow stick spawn odds (both colors) | enabled | disabled (-100%) |

A few deliberate non-changes among the tuned stats: regen delay after damage is left at vanilla
(fast regen with no delay makes you nearly unkillable in a slow fight), the no-damage fall height
itself is left at vanilla (cutting fall damage to 10% already does the work, so raising the safe
height on top of that was redundant), and ammo isn't paired with a damage buff (that combination
was reported as overtuned by another mod's author).

Every value above is a multiplier (or on/off switch) you can retune in the config file BepInEx
generates after first launch (`BepInEx/config/MindAttic.GTFO.EZ.cfg`), without recompiling anything.

## Also exposed, but left at vanilla by default

These are tunable in the same config file, but ship unchanged — either because vanilla was
already fine (flashlights), or because there's no community consensus they need adjusting for an
easier game, unlike the stats above. Grouped here the same way they're grouped in the config file
itself:

**Fall Damage**

| Stat | Vanilla | Default |
|---|---|---|
| Fall height before any damage | 4m | - |

**Lighting**

| Stat | Vanilla | Default |
|---|---|---|
| Flashlight angle | varies by light type (35-70 degrees) | - |
| Flashlight intensity | varies by light type (0.25-0.9) | - |

**Movement**

| Stat | Vanilla | Default |
|---|---|---|
| Walk speed | 3.5 m/s | - |
| Run speed | 6 m/s | - |
| Air (mid-air) move speed | 3 m/s | - |
| Crouch move speed | 2 m/s | - |
| Ladder climb speed | 1.5 m/s | - |
| Jump height (initial jump velocity) | 9 m/s | - |

**Combat**

| Stat | Vanilla | Default |
|---|---|---|
| Friendly fire damage multiplier | x0.5 | - |

**Utility**

| Stat | Vanilla | Default |
|---|---|---|
| Battery capacity (flashlights/tools) | 100 | - |
| Small tool battery drain rate | 0.25/s | - |
| Medium tool battery drain rate | 0.5/s | - |
| Large tool battery drain rate | 3/s | - |

**Environment**

| Stat | Vanilla | Default |
|---|---|---|
| No-air (suffocation) damage rate | 0.15 | - |
| No-air meter depletion time | 90s | - |

**Enemy Balance**

| Stat | Vanilla | Default |
|---|---|---|
| Enemy max health | varies by enemy type | - |
| Enemy melee attack damage | varies by enemy type | - |
| Enemy tentacle attack damage (Striker/Tank grabs, Scout tentacles, etc.) | varies by enemy type | - |

**Loot**

| Stat | Vanilla | Default |
|---|---|---|
| Consumable spawns per zone (med/ammo packs, syringes) | varies by zone | - |
| Big pickup spawns per zone (fog turbines, artifacts, etc.) | varies by zone | - |

**Interactions**

| Stat | Vanilla | Default |
|---|---|---|
| Revive duration (time to revive a downed teammate) | 4s | - |

## Miscellaneous (exposed, but not part of the core tuning)

Everything below was added after the mod's core feature set (health/regen/fall damage/ammo/
detection/glow sticks, above) was already settled. It's real, functional, and fully configurable,
but less curated and less load-bearing for the mod's "easy mode" identity than the stats above —
expect it to matter more for players who want to go deeper than the defaults, not for a first install.

**Consumable spawn odds** — Glow Stick is covered above (it's disabled by default); these other
items share the same weighted lottery but are all left at vanilla:

| Stat | Default |
|---|---|
| Long Range Flashlight spawn odds | - |
| C-Foam Grenade spawn odds | - |
| Fog Repeller spawn odds | - |
| Lock Melter spawn odds | - |
| Explosive Trip Mine spawn odds | - |
| Melee Buff (IIx) Syringe spawn odds | - |
| Health (I2-LP) Syringe spawn odds | - |
| C-Foam Tripmine spawn odds | - |

These spawn-odds stats (glow stick included) all retune the same weighted lottery that decides
*which* consumable fills a spawn slot — they don't change *how many* consumables spawn (that's
the `Consumable Spawn Multiplier` above). Setting one to 0 removes that item from the loot pool
entirely; the slots it would have used go to whatever's left in the lottery instead.

**Expedition balance** — `ExpeditionBalanceDataBlock` is the game's own per-difficulty-tier budget
table (one entry per tier: Easy/Normal/Hard/..., scaled uniformly). This is by far the largest
surface in the mod and none of it is retuned by default:

*Resources*

| Stat | Default |
|---|---|
| Health-pack resource budget per zone | - |
| Infection-cure resource budget per zone | - |
| Weapon ammo-pack resource budget per zone | - |
| Tool ammo (sentries, C-foam launcher, etc.) budget per zone | - |
| Commodity (crafting material) value per zone | - |
| Chance a container holds a commodity | - |
| Chance a container holds an artifact | - |
| Chance a commodity spawn is a large pack | - |
| Chance a commodity spawn is a medium pack | - |
| Chance a resource container gets reused for a second pack | - |
| Max packs per resource container | - |
| Empty weak (cardboard box) containers per zone | - |
| Empty secure (locker) containers per zone | - |
| General loot budget per zone | - |
| Air (breathable) budget per zone | - |
| Air budget per zone inside no-air areas | - |
| Terminals per zone | - |
| Chance a weak container holding a pack is locked | - |
| Resource pack size (small/medium/large tiers) | - |

*Enemies*

| Stat | Default |
|---|---|
| Roaming enemy patrol groups per zone | - |
| Max stationary (sleeper) enemies per zone | - |
| Max stationary enemies in a small area | - |
| Max stationary enemies in a medium area | - |
| Max stationary enemies in a large area | - |
| Max stationary enemies in a huge area | - |
| Overall enemy population budget per zone | - |

*Level generation* (internal tuning, not difficulty — changes how levels are built)

| Stat | Default |
|---|---|
| Voxel coverage area factor | - |
| Voxel coverage area randomness factor | - |
| Artifacts per level segment | - |
| Artifacts per level layer | - |

*Doors*

| Stat | Default |
|---|---|
| Small (4x4) weak door health | - |
| Large (8x4) weak door health | - |
| Odds a weak door has no lock | - |
| Odds a weak door has a melee-breakable lock | - |
| Odds a weak door has a hackable lock | - |
| Chance an unlocked weak door starts open | - |
| Chance a door starts open after a wall-remover charge | - |
| Weak door lock health | - |
| C-Foam volume -> door health conversion rate | - |
| C-Foam volume needed to fully seal a door | - |

Not yet exposed: `TentacleTraps` and `ParasiteNests` on the same datablock are nested structs
that need their own field-by-field investigation.

**Melee weapons** — `MeleeArchetypeDataBlock` has one entry per weapon type: **Bat**, **Hammer**
(the sledgehammer), **Knife**, and **Spear** — real in-game names, confirmed from the data rather
than assumed. Each weapon gets its own independent set of config entries, grouped by action type
(Light Attack / Charged Attack / Push / General) the same way they're grouped in the config file.
Vanilla numbers below are the same for all four stat *categories* (every weapon has a Light Attack
Damage, a Stagger Multiplier, etc.) but the actual *values* differ per weapon — a Knife's light
attack does 2 damage, a Hammer's does 3, for example:

*Light Attack*

| Stat | Default |
|---|---|
| Damage | - |
| Stagger Multiplier | - |
| Precision Damage Multiplier | - |
| Environment Damage Multiplier | - |
| Backstab Damage Multiplier | - |
| Sleeper Damage Multiplier | - |
| Stamina Cost Multiplier | - |

*Charged Attack*

| Stat | Default |
|---|---|
| Damage | - |
| Stagger Multiplier | - |
| Precision Damage Multiplier | - |
| Environment Damage Multiplier | - |
| Backstab Damage Multiplier | - |
| Sleeper Damage Multiplier | - |
| Stamina Cost Multiplier | - |

*Push*

| Stat | Default |
|---|---|
| Hit Detection Radius Multiplier | - |
| Stamina Cost Multiplier | - |

*General*

| Stat | Default |
|---|---|
| Reach Multiplier | - |
| Hit Detection Radius Multiplier | - |
| Charge Move Speed Multiplier | - |

That's 19 stats x 4 weapons = 76 config entries, all left at vanilla. For reference, here's what
"vanilla" actually is per weapon (light / charged attack damage only, the two most legible
numbers):

| Weapon | Light Attack Damage | Charged Attack Damage |
|---|---|---|
| Bat | 3 | 12 |
| Hammer | 3 | 20 |
| Knife | 2 | 5.5 |
| Spear | 2 | 17.5 |

Not yet exposed: `PublicName`, `NoiseLevel`, the animation/SFX set references, and the boolean
behavior flags (`SkipLimbDestruction`, `CanHitMultipleEnemies`, etc.) aren't multiplier-friendly
balance values, so they're left alone.

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

## Changelog

### v5

- **Bug fix:** Remove Combat Stamina Penalty previously equalized a pair of fields that are both
  0 at vanilla regardless of combat state, so it did nothing. It now correctly equalizes the real
  penalty (0.15/s vs 0.25/s regen while resting) and removes the 90% stamina cap while in combat.
- Fall damage amount is now actually cut to 10% of normal, on top of the unchanged no-damage
  height (previously only the height was touched, not the damage itself).
- Exposed more stats as config options, all left at vanilla by default unless noted: enemy max
  health, enemy melee/tentacle attack damage, consumable spawn rate per zone, big pickup spawn
  rate per zone, revive duration, 8 more individual consumable spawn-odds stats, the full
  per-difficulty-tier `ExpeditionBalanceDataBlock` (~40 stats covering resources, enemies, level
  generation, and doors), and every melee weapon stat (76 entries: 4 weapons x 19 stats each,
  organized by weapon then by action type).
- Glow Stick spawn odds now default to disabled (0) instead of vanilla — a real tuned default,
  not just exposed.
- Corrected several "vanilla" values in the docs that were placeholders or incomplete: revive
  duration (4s), flashlight angle/intensity (vary by light type, not a flat x1), and enemy
  detection distance (there's a second, 20m sleeper profile alongside the 8m normal one).
- Trimmed two config descriptions (Health Regen Cap Multiplier, Weapon Ammo Multiplier) that
  exceeded the in-game config editor's 200-character limit and were getting cut off.
- Pre-release cleanup: removed a leftover debug log line that fired on every revive, consolidated
  a redundant double-iteration over the consumable loot table, and brought both READMEs and the
  manifest description up to date with everything above.

### v4

- In-level warden-intel confirmation message now shows 90 seconds after spawn instead of 30, so
  it lands once you're actually in the level rather than right at the drop-in.

### v3

- Flashlight angle/intensity tweak now defaults to vanilla (x1.0) instead of x1.25 — vanilla
  flashlights were already fine as-is — but the config option is still there to retune if you want.
- Exposed more stats as config options, all left at vanilla by default: walk/run/air/crouch/ladder
  move speed, jump height, friendly fire damage, battery capacity and drain rate (small/medium/
  large tools), and no-air (suffocation) damage rate and depletion time.
- The in-game confirmation message now includes the mod's major version (e.g. "GTFO.EZ v3
  Active...").

### v2

- Added an in-game confirmation message: a line on the rundown-select screen, and a warden-intel
  message 30 seconds after you spawn into a level.

### v1

- Initial release: health, regen, stamina, fall damage, ammo, flashlight, and enemy detection
  tweaks, all config-driven multipliers applied at game-data load time.

# GTFO.EZ

*(Listed on Thunderstore as `GtfoEZ` — package names there are restricted to letters, numbers,
and underscores, so they can't contain a period.)*

A small, config-driven BepInEx plugin for GTFO that tunes player stats for an easier, more
relaxed co-op experience. No datablock JSON files are shipped — it patches the game's own data
in memory at load time, so it keeps working across game patches that only change base values.
A curated set of defaults covers the core experience, with 530+ customizable fields underneath
for anyone who wants to retune things further.

## What it changes

Defaults follow community-consensus balance values (several independent GTFO easy-mode mods'
settled-on numbers), not arbitrary multipliers. "Default" is the value after this mod's default
config is applied; `-` means left at vanilla:

| Stat | Vanilla | Default |
|---|---|---|
| Max health | 25 | 50 (+100%) |
| Health regen rate | 0.2/s | 0.1/s (-50%) |
| Max regenerated health (no med kit) | 20% | 40% (+100%) |
| Stamina regen rate while resting in combat | 0.15/s (vs 0.25/s out of combat) | 0.25/s, equalized |
| Stamina cap while in combat | 90% | 100%, cap removed |
| Fall damage amount | 2-15 | 0.2-1.5 (-90%) |
| Fall height where damage maxes out | 20m | 30m (+50%) |
| Weapon/tool ammo (reserve, starting, refill packs) | x1 | x1.5 (+50%) |
| Enemy movement-noise detection distance | 8m (sleepers: 20m) | 6m (sleepers: 15m) (-25%) |
| Consumables (boxes, lockers, lying around) | mixed items | health, tool and ammo kits only, 20-40% of a pack each |
| Revive duration (time to revive a downed teammate) | 4s | 1.32s (-67%) |
| Fog (visibility-limiting fog) | enabled | disabled |
| Sentry gun deploy/pickup interaction time | vanilla hold time | instant (-100%) |
| Dash (hold Shift, double-tap W/A/S/D) | none | 6 m in 0.25 s, 1.5 s cooldown |
| Artifact pickups (Bold / Aggressive / Muted) | count toward boosters only | +5% max health / +5% damage / 5% harder to detect, each, for the level |
| Fog clearing around you | none (Fog Repeller item only, 70 s) | always on, 4 m widening to 25 m over 90 s |

A few deliberate non-changes among the tuned stats: regen delay after damage is left at vanilla
(fast regen with no delay makes you nearly unkillable in a slow fight), the no-damage fall height
itself is left at vanilla (cutting fall damage to 10% already does the work, so raising the safe
height on top of that was redundant), and ammo isn't paired with a damage buff (that combination
was reported as overtuned by another mod's author).

**Controls** — on first launch the mod also switches on four of the game's own control options:
the comms menu opens and closes with a tap of Q instead of having to be held, sprint and aim are
toggles, and jumping stands you up from a crouch. It does this once only; after that they're
ordinary game settings you can change back in Settings → Gameplay.

Every value above is a multiplier (or on/off switch) you can retune in the config file BepInEx
generates after first launch (`BepInEx/config/MindAttic.GTFO.EZ.cfg`), without recompiling anything.

If you're upgrading from an older version: BepInEx keeps whatever value is already in your `.cfg`,
so when a *default* listed above changes between versions (see `CHANGELOG.md`), your install won't
pick up the new number on its own. This mod corrects that once per affected setting, the first
time you launch the version that fixes it - if you'd deliberately set one of those same settings
to its old value yourself, that gets reset to the new default too, since the two cases look
identical from the config file alone.

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
| Consumable spawns per zone (kits, or vanilla items with Consumables Are Kits off) | varies by zone | - |
| Big pickup spawns per zone (fog turbines, artifacts, etc.) | varies by zone | - |

## Miscellaneous (exposed, but not part of the core tuning)

Everything below was added after the mod's core feature set (health/regen/fall damage/ammo/
detection/glow sticks, above) was already settled. It's real, functional, and fully configurable,
but less curated and less load-bearing for the mod's "easy mode" identity than the stats above —
expect it to matter more for players who want to go deeper than the defaults, not for a first install.

**Consumable spawn odds** — only used with `Consumables Are Kits` turned off (it's on by default,
and replaces every consumable with a kit). They retune the weighted lottery that decides *which*
consumable fills a spawn slot, not *how many* spawn (that's the `Consumable Spawn Multiplier`
above). Setting one to 0 removes that item, and its share goes to the rest; a loot table with
every item disabled spawns nothing.

| Stat | Vanilla | Default |
|---|---|---|
| Glow stick spawn odds (both colors) | enabled | disabled (-100%) |
| Lock Melter spawn odds | varies by zone | disabled (-100%) |
| C-Foam Grenade spawn odds | varies by zone | disabled (-100%) |
| Explosive Trip Mine spawn odds | varies by zone | disabled (-100%) |
| C-Foam Tripmine spawn odds | varies by zone | disabled (-100%) |
| Fog Repeller spawn odds | varies by zone | disabled (-100%) |
| Long Range Flashlight spawn odds | varies by zone | - |

**Expedition balance** — `ExpeditionBalanceDataBlock` is the game's own per-difficulty-tier budget
table (one entry per tier: Easy/Normal/Hard/..., scaled uniformly). This is by far the largest
surface in the mod and none of it is retuned by default:

*Resources*

| Stat | Vanilla | Default |
|---|---|---|
| Health-pack resource budget per zone | varies by tier | - |
| Infection-cure resource budget per zone | varies by tier | - |
| Weapon ammo-pack resource budget per zone | varies by tier | - |
| Tool ammo (sentries, C-foam launcher, etc.) budget per zone | varies by tier | - |
| Commodity (crafting material) value per zone | varies by tier | - |
| Chance a container holds a commodity | varies by tier | - |
| Chance a container holds an artifact | varies by tier | - |
| Chance a commodity spawn is a large pack | varies by tier | - |
| Chance a commodity spawn is a medium pack | varies by tier | - |
| Chance a resource container gets reused for a second pack | varies by tier | - |
| Max packs per resource container | varies by tier | - |
| Empty weak (cardboard box) containers per zone | varies by tier | - |
| Empty secure (locker) containers per zone | varies by tier | - |
| General loot budget per zone | varies by tier | - |
| Air (breathable) budget per zone | varies by tier | - |
| Air budget per zone inside no-air areas | varies by tier | - |
| Terminals per zone | varies by tier | - |
| Chance a weak container holding a pack is locked | varies by tier | - |
| Resource pack size (small/medium/large tiers) | varies by tier | - |

*Enemies*

| Stat | Vanilla | Default |
|---|---|---|
| Roaming enemy patrol groups per zone | varies by tier | - |
| Max stationary (sleeper) enemies per zone | varies by tier | - |
| Max stationary enemies in a small area | varies by tier | - |
| Max stationary enemies in a medium area | varies by tier | - |
| Max stationary enemies in a large area | varies by tier | - |
| Max stationary enemies in a huge area | varies by tier | - |
| Overall enemy population budget per zone | varies by tier | - |
| Tentacle Trap health | varies by tier | - |
| Tentacle Trap attack damage | varies by tier | - |
| Tentacle Trap max per zone | varies by tier | - |
| Tentacle Trap max in a small area | varies by tier | - |
| Tentacle Trap max in a medium area | varies by tier | - |
| Tentacle Trap max in a large area | varies by tier | - |
| Tentacle Trap max in a huge area | varies by tier | - |
| Parasite Nest health | varies by tier | - |
| Parasite Nest attack damage | varies by tier | - |
| Parasite Nest max per zone | varies by tier | - |
| Parasite Nest max in a small area | varies by tier | - |
| Parasite Nest max in a medium area | varies by tier | - |
| Parasite Nest max in a large area | varies by tier | - |
| Parasite Nest max in a huge area | varies by tier | - |

Tentacle Trap and Parasite Nest each expose the same 7-stat shape as roaming/sleeper enemies
above (Health, Attack Damage, Max Per Zone, Max Small/Medium/Large/Huge Area) via their own
`StaticEnemyData` reference on the datablock — confirmed by IL-inspecting `Modules-ASM.dll` rather
than guessing at an undocumented nested type.

*Level generation* (internal tuning, not difficulty — changes how levels are built)

| Stat | Vanilla | Default |
|---|---|---|
| Voxel coverage area factor | varies by tier | - |
| Voxel coverage area randomness factor | varies by tier | - |
| Artifacts per level segment | varies by tier | - |
| Artifacts per level layer | varies by tier | - |

*Doors*

| Stat | Vanilla | Default |
|---|---|---|
| Small (4x4) weak door health | varies by tier | - |
| Large (8x4) weak door health | varies by tier | - |
| Odds a weak door has no lock | varies by tier | - |
| Odds a weak door has a melee-breakable lock | varies by tier | - |
| Odds a weak door has a hackable lock | varies by tier | - |
| Chance an unlocked weak door starts open | varies by tier | - |
| Chance a door starts open after a wall-remover charge | varies by tier | - |
| Weak door lock health | varies by tier | - |
| C-Foam volume -> door health conversion rate | varies by tier | - |
| C-Foam volume needed to fully seal a door | varies by tier | - |

**Melee weapons** — `MeleeArchetypeDataBlock` has one entry per weapon type: **Bat**, **Hammer**
(the sledgehammer), **Knife**, and **Spear** — real in-game names, confirmed from the data rather
than assumed. Each weapon gets its own independent set of config entries, grouped by action type
(Light Attack / Charged Attack / Push / General) the same way they're grouped in the config file.
Vanilla numbers below are the same for all four stat *categories* (every weapon has a Light Attack
Damage, a Stagger Multiplier, etc.) but the actual *values* differ per weapon — a Knife's light
attack does 2 damage, a Hammer's does 3, for example:

*Light Attack*

| Stat | Vanilla | Default |
|---|---|---|
| Damage | varies by weapon (see table below) | - |
| Stagger Multiplier | varies by weapon | - |
| Precision Damage Multiplier | varies by weapon | - |
| Environment Damage Multiplier | varies by weapon | - |
| Backstab Damage Multiplier | varies by weapon | - |
| Sleeper Damage Multiplier | varies by weapon | - |
| Stamina Cost Multiplier | varies by weapon | - |

*Charged Attack*

| Stat | Vanilla | Default |
|---|---|---|
| Damage | varies by weapon (see table below) | - |
| Stagger Multiplier | varies by weapon | - |
| Precision Damage Multiplier | varies by weapon | - |
| Environment Damage Multiplier | varies by weapon | - |
| Backstab Damage Multiplier | varies by weapon | - |
| Sleeper Damage Multiplier | varies by weapon | - |
| Stamina Cost Multiplier | varies by weapon | - |

*Push*

| Stat | Vanilla | Default |
|---|---|---|
| Hit Detection Radius Multiplier | varies by weapon | - |
| Stamina Cost Multiplier | varies by weapon | - |

*General*

| Stat | Vanilla | Default |
|---|---|---|
| Reach Multiplier | varies by weapon | - |
| Hit Detection Radius Multiplier | varies by weapon | - |
| Charge Move Speed Multiplier | varies by weapon | - |

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

**Ranged weapons and sentries** — `ArchetypeDataBlock` is the ranged-weapon equivalent of melee's
archetype block, covering every gun's fire modes and every sentry. There are far more of these
(~50 individual fire-mode archetypes across ~20 weapon types) than melee's 4, so the five core
combat stats get a **two-tier** multiplier instead of melee's single tier: a **Weapon Type**
slider shared by every fire mode in that family (e.g. all of "Rifle") multiplied by an
**Individual Weapon** slider for one specific fire mode (e.g. "Rifle - Burst"), both defaulting to
1.0 and stacking multiplicatively - adjust either one, or both.

| Stat | Vanilla | Default |
|---|---|---|
| Damage | varies by weapon (see table below) | - |
| Stagger Multiplier | varies by weapon | - |
| Precision (headshot) Multiplier | varies by weapon | - |
| Clip Size | varies by weapon | - |
| Reload Time | varies by weapon | - |

Weapon type and archetype ownership were confirmed by logging `GearCategoryDataBlock` and
`ArchetypeDataBlock` at boot - not documented anywhere official. A few types share an archetype
across more than one category (e.g. "SMG_Special" reuses plain "SMG"'s stats), in which case only
the base type gets its own slider. Sentries don't go through this category system at all, so they
aren't covered by the two-tier sliders - see the flat Sentry stats further below.

The **Weapon** column is the actual gun you pick in the loadout screen, resolved from the game's own
gear data (and checked against the GTFO wiki). The Weapon Type / Individual Weapon columns are the
names of the config sections you'd edit. 16 rows have no player weapon - every "SemiBurst" slot
plus a few unused ones - so treat those as inert. The fire-mode word in a section name is the
internal slot, which doesn't always match how the gun fires: the Bataldo Custom K330 is a slug
shotgun filed under "HEL Shotgun - Semi", and the Bataldo J 300 fires full-auto from the "Burst"
slot.

| Weapon | Weapon Type | Individual Weapon | Damage | Stagger Multiplier | Precision Multiplier | Clip Size | Reload Time |
|---|---|---|---|---|---|---|---|
| — (no player weapon) | Bullpup | Semi | 3 | 1 | 1 | 8 | 3 |
| — (no player weapon) | Bullpup | Burst | 1.8 | 1 | 1 | 24 | 3 |
| Accrat Golok DA Bullpup Rifle | Bullpup | Auto | 2.1 | 1 | 1 | 40 | 2.3 |
| — (no player weapon) | Bullpup | SemiBurst | 1.5 | 1 | 1 | 25 | 3 |
| TR22 Hanaway DMR | DMR | Semi | 7.51 | 1 | 0.87 | 12 | 2.3 |
| Hanaway PSB Double Tap Rifle | DMR | Burst | 5.51 | 1 | 0.9 | 14 | 2.15 |
| — (no player weapon) | DMR | Auto (Long gun) | 4 | 1 | 1 | 16 | 1.9 |
| — (no player weapon) | DMR | SemiBurst | 20 | 1 | 1 | 10 | 1.9 |
| Omneco LRG HEL Rifle | Gauss Rifle | Semi | 30.01 | 1 | 0.73 | 5 | 3.2 |
| Bataldo Custom K330 Slug Shotgun | HEL Shotgun | Semi | 21 | 2 | 0.7413 | 6 | 2.5 |
| Bataldo J 300 HEL Shotgun | HEL Shotgun | Burst | 1.67 | 2 | 0.7413 | 14 | 3 |
| Shelling Arid 5 High Caliber Pistol | HighCalPistol | - | 30.1 | 1 | 0.7413 | 6 | 2.5 |
| TechMan Veruta XII Machine Gun | MachineGun | Auto | 3.51 | 1.45 | 0.7413 | 60 | 4 |
| Omneco EXP1 HEL Gun | MachineGun LowRPM | Semi | 16.25 | 1 | 0.8 | 9 | 3.3 |
| TechMan Klust 6 Burst Cannon | MachineGun LowRPM | Burst | 22 | 1 | 0.734 | 20 | 3.7 |
| TechMan Arbalist V Machine Gun | MachineGun LowRPM | Auto | 5.35 | 1 | 0.7413 | 40 | 3.5 |
| — (no player weapon) | MachineGun LowRPM | SemiBurst | 20 | 1 | 1 | 10 | 2.1 |
| Shelling S49 Pistol | Pistol | Semi | 4.64 | 1.5 | 0.8667 | 15 | 1.3 |
| Shelling Nano Burst Pistol | Pistol | Burst | 3.2 | 1 | 0.8667 | 28 | 1.25 |
| Raptus Treffen 2 Machine Pistol | Pistol | Auto | 1.9 | 1 | 0.7413 | 50 | 1.25 |
| — (no player weapon) | Pistol | SemiBurst | 20 | 1 | 1 | 10 | 1.25 |
| Raptus Steigro HEL Autopistol | Pistol Stagger | Auto | 1.5 | 5 | 0.7413 | 60 | 1.3 |
| Drekker Del P1 Precision Rifle | Precision Rifle | - | 13 | 1 | 1.3 | 10 | 3.3 |
| Mastaba R66 Revolver | Revolver | Semi | 14.21 | 1 | 0.7413 | 6 | 1.75 |
| Bataldo 3RB HEL Revolver | Revolver | Burst | 8.01 | 1 | 0.7413 | 8 | 2.3 |
| — (no player weapon) | Revolver | Auto (Autorevolver) | 7 | 1 | 1 | 10 | 2 |
| — (no player weapon) | Revolver | SemiBurst | 20 | 1 | 1 | 10 | 2 |
| Drekker Pres MOD 556 Rifle | Rifle | Semi | 5.08 | 1.75 | 0.8667 | 14 | 1.9 |
| Malatack CH 4 Burst Rifle | Rifle | Burst | 2.71 | 1 | 0.8667 | 30 | 2 |
| Malatack LX Assault Rifle | Rifle | Auto | 2.1875 | 1 | 0.8 | 30 | 1.8 |
| — (no player weapon) | Rifle | SemiBurst (Semi Burst Rifle) | 20 | 1 | 1 | 10 | 1.8 |
| — (no player weapon) | RifleCQB | - | 4.31 | 1 | 0.7 | 24 | 1.5 |
| Malatack HXC Heavy Assault Rifle | RifleHeavy | Auto | 5.01 | 1 | 0.8 | 30 | 2.3 |
| Drekker CLR Short Rifle | SMG | Semi | 4.81 | 0.5 | 0.7413 | 30 | 1.7 |
| Van Auken CAB F4 Carbine | SMG | Burst | 2.38 | 1 | 0.7413 | 48 | 1.6 |
| Van Auken LTC5 SMG | SMG | Auto | 1.98 | 1 | 0.91 | 50 | 1.4 |
| — (no player weapon) | SMG | SemiBurst | 20 | 1 | 1 | 10 | 1.5 |
| Accrat ND6 Heavy SMG | SMGHeavy | Auto | 2.45 | 1 | 0.8 | 33 | 1.55 |
| Accrat STB PDW | SMG PDW | - | 2.15 | 1 | 0.7413 | 32 | 1.4 |
| Buckland SBS III Sawed-off Shotgun | SawedOffShotgun | Semi | 3.8 | 2 | 0.7413 | 4 | 1.6 |
| Drekker INEX Drei Scattergun | Scattergun | Burst | 4.8 | 2 | 0.7333 | 2 | 2.66 |
| Buckland S870 Shotgun | Shotgun | Semi | 3.01 | 1 | 1 | 8 | 3 |
| Buckland XDIST2 Choke Mod Shotgun | Shotgun | Burst | 3.76 | 1.35 | 1 | 4 | 2.72 |
| Buckland AF6 Combat Shotgun | Shotgun | Auto | 2.51 | 2 | 0.875 | 12 | 2.83 |
| — (no player weapon) | Shotgun | SemiBurst | 20 | 1 | 1 | 10 | 1 |
| Köning PR 11 Sniper | Sniper | Semi | 40.01 | 1 | 2.0025 | 2 | 3.5 |
| — (no player weapon) | Sniper | Burst (Railgun) | 15 | 1 | 1 | 9 | 3.5 |
| — (no player weapon) | Sniper | Auto (Coilgun) | 12 | 1 | 1 | 10 | 3.5 |
| — (no player weapon) | Sniper | SemiBurst | 20 | 1 | 1 | 10 | 2.5 |

Every other `ArchetypeDataBlock` stat (ammo cost, fire rate, spread, burst/shotgun-pellet counts,
charge-up/cooldown timing, equip/aim speed, shell casing size) is left as one flat multiplier
shared by every weapon and sentry alike, rather than tripling the config size again for stats
nobody asked to tune per-gun:

| Stat | Vanilla | Default |
|---|---|---|
| Ammo cost per shot | varies by weapon | - |
| Shot delay (fire rate) | varies by weapon | - |
| Hip-fire spread | varies by weapon | - |
| Aim spread | varies by weapon | - |
| Piercing target count | varies by weapon | - |
| Burst delay | varies by weapon | - |
| Burst shot count | varies by weapon | - |
| Shotgun pellet count | varies by weapon | - |
| Shotgun cone size | varies by weapon | - |
| Shotgun pellet spread | varies by weapon | - |
| Special charge-up time | varies by weapon | - |
| Special cooldown time | varies by weapon | - |
| Semi-burst timeout | varies by weapon | - |
| Equip transition time | varies by weapon | - |
| Aim transition time | varies by weapon | - |
| Shell casing size (cosmetic) | varies by weapon | - |

Sentries get their own flat multiplier set for the targeting/combat behavior that's unique to them
(not shared with handheld weapons), including a separate set applied only once a sentry has
"tagged" (scanned) a target:

| Stat | Vanilla | Default |
|---|---|---|
| Start fire delay | varies by sentry | - |
| Rotation speed | varies by sentry | - |
| Detection max range | varies by sentry | - |
| Detection max angle | varies by sentry | - |
| Long range threshold | varies by sentry | - |
| Short range threshold | varies by sentry | - |
| Tagged-target start fire delay multiplier | varies by sentry | - |
| Tagged-target rotation speed multiplier | varies by sentry | - |
| Tagged-target damage multiplier | varies by sentry | - |
| Tagged-target stagger multiplier | varies by sentry | - |
| Tagged-target ammo cost multiplier | varies by sentry | - |
| Tagged-target shot delay multiplier | varies by sentry | - |

Not yet exposed: `PublicName`/`Description`, `FireMode`, `RecoilDataID`, `DamageBoosterEffect`, the
animation sequence references, `PiercingBullets` (on/off flag), and `ShellCasingSpeedRange` -
either not multiplier-friendly or purely cosmetic/technical.

## Compatibility

- **GTFO.AbsurdWeapons:** compatible. Everything it changes in the game's data is applied after this
  mod's, so where the two clash it wins - notably loot, which follows its `[Loot]` settings (kits at
  its odds and size, plus Hand Grenades) instead of *Consumables Are Kits* here. Its
  keys (Home/PgUp/PgDn) don't overlap with this mod's (E, X, Z, Shift, W/A/S/D). It leaves artifacts
  in place, so artifact buffs work with it installed.
- **GTFO.CrashTestDummies** and **GTFO.SkipDrop:** compatible - they patch level building and the
  elevator drop, nothing this mod touches.

## Install

Install via [Thunderstore Mod Manager or r2modman](https://thunderstore.io/c/gtfo/p/MindAttic/GtfoEZ/),
or import the zip manually and share a profile code. Everyone in the lobby needs the same mod and
config.

## Attributions

This mod is an independent, from-scratch implementation (no code or data copied from any of the
projects below, except where noted) built as a code plugin rather than datablock JSON, specifically so it survives
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
- **AlexanderMakunin**, creator of
  *[BetterTerminal](https://github.com/AlexanderMakunin/BetterTerminal)* (Thunderstore:
  Daem0n/BetterTerminal, MIT, Copyright (c) 2026 AlexanderMakunin) — Ping Anywhere uses its
  technique for owning a terminal command: writing the command's output straight into the
  terminal's buffers so the game doesn't also run it, and expiring the ping marker afterwards. The
  output-writing helper follows BetterTerminal's `TerminalCommandOutput.WriteImmediate` closely.
- **The [BepInEx](https://github.com/BepInEx/BepInEx) team** — the modding framework and loader
  this mod (and essentially every other GTFO mod) runs on.
- **Claude Opus 5.5** (Anthropic) — AI assistant used throughout development: implementing the
  patches, reverse-engineering datablock fields, and writing/auditing this documentation.

## Changelog

### v14

- New **artifact buffs**: picking up an artifact buffs whoever picked it up for the rest of the
  level - each **Bold** +5% max health (healed too), each **Aggressive** +5% damage to enemies, each
  **Muted** 5% harder for enemies to detect you (capped at 75%). A checkpoint restart that puts an
  artifact back takes its buff off again. Tunable under the new Artifacts section.
- **Moved out:** the Stim Pack - the I2-LP's overheal (+50% health past max, draining back down)
  and its "Stim Pack" rename - now lives in GTFO.AbsurdWeapons, as a consumable of its own. The
  I2-LP is a vanilla heal again here, and the Consumables settings for it (heal amount, buff
  drain, display name) are gone.
- New **Consumables Are Kits** (Loot, on by default): every consumable found in boxes, lockers or
  lying around is a health, tool or ammo kit holding 20-40% of a pack (weights and contents
  configurable); the zone's normal resource packs still spawn as usual. GTFO.EZ no longer turns
  disabled consumables into syringes, and its two syringe spawn-odds settings are gone. With
  GTFO.AbsurdWeapons installed its loot settings take precedence: kits at its odds and size, plus
  Hand Grenades.
- New **Ping Anywhere** (Experimental, on by default): `PING <KEY>` from any terminal reaches an
  item anywhere in the level, not just the terminal's own zone; the marker shows for 10 s.
  Same-zone pings are left to the game's own PING, and any error falls back to it. Technique
  credit: BetterTerminal by AlexanderMakunin (MIT).
- **Bug fix:** X (place your own sentry) left the yellow placement arrow on screen; it's hidden
  once the sentry is down.
- **Bug fix:** recalling bot sentries found nothing - a sentry placed with Shift+E isn't linked
  back to its bot the way the recall looked it up. It now finds the placed sentries in the level
  and matches each to its bot. Z is the recall key; Alt+E is gone.
- With GTFO.AbsurdWeapons installed, everything it changes in the game's data applies after this
  mod's and wins where they clash - loot follows its own settings. See Compatibility.

### v13

- **Bug fix:** Fog Density Multiplier (0.1 / -90%) turned out to have no visible effect in-game no
  matter how low it was set - whatever actually drives fog's look isn't gated purely on that field.
  Replaced with a Disable Fog toggle (default on) that turns fog off via its Enabled flag instead
  of scaling a density value that didn't render differently.
- **Bug fix:** if the I2-LP life buff was still holding max health raised (the overheal case where
  native code's own cap had to be lifted) when a level ended, that raised cap stuck forever -
  nothing restored it going into the next level, so max health could creep upward for the rest of
  the expedition. Starting a new level now restores it first if it was still raised.
- **Experimental (SPIKE), on by default:** while aiming your own equipped tool at a valid
  placement spot (the normal green ghost/arrow), Shift+E deploys the first bot with an undeployed
  tool there instead, reusing that same aim - your own plain-E placement is untouched. Placing
  your own also switches you straight back to whatever you had equipped before pulling the tool
  out, instead of leaving you empty-handed. Z instantly recalls every bot-placed tool
  still down, straight back into whichever bot placed it. X places your own sentry without
  pulling it out - at your aim if you're holding it, otherwise on the ground just ahead of you.
  There's no vanilla order for "deploy now" to route through, so this
  calls the bot AI's own placement/pickup methods directly; only confirmed to compile, not
  confirmed safe by decompiling alone - set Shift+E deploy to false under Experimental to disable,
  or report back if it needs reverting.
- Sentry guns place and pick back up instantly now - the hold-to-confirm charge is gone by
  default. New "Deploy/Pickup Interaction Duration Multiplier" under Sentries, default 0; set to
  1.0 to restore the vanilla hold time.
- New **dash**: hold Shift and double-tap W/A/S/D for a quick 6 m dash that way, mainly to
  sidestep homing projectiles. Moves you through the game's own character
  controller, so walls stop it short; 1.5 s cooldown. Distance, duration, cooldown, double-tap
  window and an on/off toggle are under the new Dash config section.
- New **fog clearing**: Disable Fog alone still left fog in play, so a silent, invisible
  fog-clearing sphere now follows you - the same fog-system sphere the Fog Repeller item carves
  out, without its light, sound or 70 s limit. It starts at 4 m and widens to 25 m over 90 s
  each level, so the longer you're in, the less fog there is. Its strength copies the vanilla
  Fog Repeller's own; tunable under the new Fog Clearing section.

### v11

- **Bug fix:** Health Regen Cap Multiplier's default of 0.5 (10%), introduced in v8, turned out to
  have no stated rationale anywhere in that commit or the changelog - unlike every other default
  change in this mod, which cites a reason. It also never matched how the mod was actually played:
  because of the stale-`.cfg` bug below, any install that predates v8 was never actually affected
  by it in the first place, silently staying at the original 2.0 (40%) the whole time. Reverted
  the default back to 2.0 (40%) to match that original, actually-played-with value, and extended
  the v10 migration to also catch anyone who *did* get moved onto the short-lived 0.5.
- Whole-number stats (ammo counts, clip size, pellet count, enemy caps, loot counts, etc.) now
  always round in the player's favor when a multiplier leaves a fraction, instead of rounding to
  nearest-even: anything where more helps you rounds up, anything where fewer helps you (enemy
  counts, shotgun spread) rounds down. At default settings this can add 1 to an ammo value where
  the x1.5 ammo multiplier lands on a half.
- Every individual-weapon setting now names the actual gun it affects (e.g. "Weapon - Rifle Auto"
  is the Malatack LX Assault Rifle), both in the config file's descriptions and in a new Weapon
  column on the README's weapon table. Section names are unchanged, so existing settings carry
  over. 16 of the 49 fire-mode slots turned out to have no player weapon at all - now labelled.
- **Bug fix:** each mod update re-ran every past default-migration, not just the new ones, so if
  you'd deliberately set something back after an earlier update (say, re-enabled Lock Melters, or
  put fall damage back to vanilla) the next update quietly reset it again. Each migration now runs
  once per install and never again.
- Fog Repeller spawn odds now default to disabled (0), joining the other disabled consumables -
  with fog already cut 90%, they mostly took up loot slots.
- I2-LP (health) syringes are now a temporary life buff: they add 50% of max health (vanilla: a
  random 5-15%) and can overheal past 100% - inject at 65% and you're at 115%. Everything above
  the health you had when injecting then drains away at 1% of max health per second. Drop below
  that pre-syringe health (say, from a hit) and the buff cancels on the spot. A second syringe
  stacks on top, and a med kit used mid-buff is kept rather than drained. Configurable under
  Consumables as "I2-LP Syringe Heal Amount" (0 keeps vanilla) and "I2-LP Buff Drain (% of max
  health per second)" (0 makes it a normal heal capped at max health).
- The I2-LP is renamed "Stim Pack" in game - pickup, inventory and terminal, where its key becomes
  STIM_PACK_123. Configurable as "I2-LP Display Name" under Consumables; empty keeps the vanilla name.
- On first launch, turns on four of the game's own control options: the comms menu toggles open/
  closed with Q instead of having to hold it, sprint toggle, aim toggle, and stand when jumping.
  Applied once only - change any of them back in the game's settings and they stay changed.
- Disabled consumables now turn into syringes instead of just vanishing: each disabled item's loot
  slot becomes an I2-LP (health) or IIx (melee buff) syringe at the same odds, alternating within
  each loot table. Previously their share went to whatever was left - so 23 tables, including
  every common "Baseline" zone, dropped nothing but Long Range Flashlights, and 25 tables had
  nothing left to drop at all. Med/ammo/tool packs aren't used here: those get filled by the
  level's resource budget and would likely spawn empty from a consumable table.
- **Bug fix:** with one syringe type's spawn odds set to 0, disabled consumables still alternated
  onto it, so half their loot slots spawned nothing. They now all go to the syringe that's enabled.

### v10

- **Bug fix:** A full audit of every config default's git history turned up more entries hit by
  the same stale-`.cfg` bug as v9's loot fix: Health Regen Rate Multiplier (5.0->0.5) and Health
  Regen Cap Multiplier (2.0->0.5) from v8, Fall Damage Multiplier (1.0->0.1) and Fall Damage Min
  Height Multiplier (2.0->1.0) from v5, Flashlight Angle/Intensity Multiplier (1.25->1.0) from v2,
  and Revive Duration Multiplier (1.0->0.33) from v8 all changed their default without a
  migration, so upgraders from before the relevant version could still be stuck on values as old
  as v1. The one-time migration added here now covers all of these too.
- **Bug fix:** a single try/catch wrapped all 9 of the mod's tweak categories (player stats, fog,
  loot, weapons, etc.), so one category throwing an exception - e.g. a future game patch renaming
  a field - silently skipped every other, unrelated category too. Each category now fails
  independently and logs which one broke, instead of one bad category taking the rest down with it.
- The in-level "Goo goo. Gah gah." warden-intel message now stays up for 30 seconds instead of 15,
  still starting 90 seconds into a level - easier to actually notice.
- Added a real test suite: the weapon/melee config-table data and multiplier math that used to
  live only in `Plugin.cs`/`GameDataTweaks.cs` moved to a new `GTFO.EZ.Core` project with no
  BepInEx/Il2Cpp dependency, so it can be unit tested (`tests/GTFO.EZ.Core.Tests`, 21 NUnit tests)
  without the game installed. Dev-facing only - no gameplay change.

### v9

- Explosive Trip Mine and C-Foam Tripmine spawn odds now default to disabled (0), joining Glow
  Stick, Lock Melter, and C-Foam Grenade - vanilla frequency was judged too cluttered, same call as
  the others.
- **Bug fix:** Glow Stick (v5), and Lock Melter/C-Foam Grenade (v8), were meant to default to
  disabled (0) spawn odds, but BepInEx preserves whatever value already exists in an upgrader's
  `.cfg` file - so anyone who'd installed before the relevant version stayed stuck at the old
  vanilla (1.0) default forever and kept seeing them at full frequency. Added a one-time migration
  that nudges those settings (now including the two above) to their intended default on first
  launch of this version, gated so it never re-applies - if you'd deliberately re-enabled one of
  them since, your choice is kept.

### v8

- Added a Vanilla column to every remaining table that was missing one (consumable spawn odds,
  all four Expedition Balance subsections, all four melee weapon action types), so every exposed
  stat now shows what it's being compared against instead of just its tuned/default value.
- Exposed `TentacleTraps` and `ParasiteNests` (on `ExpeditionBalanceDataBlock`) as 14 new config
  options, all left at vanilla (1.0) by default: both are `StaticEnemyData` references with the
  same 7-field shape already exposed for roaming/sleeper enemies (Health, Attack Damage, Max Per
  Zone, Max Small/Medium/Large/Huge Area), confirmed via IL inspection of the game's own assembly.
- Credited Claude Opus 5.5 in Attributions for its role throughout development.
- Added a Fog Density Multiplier (new, default 0.1 / -90%) patched onto every
  `FogSettingsDataBlock` - vanilla fog was judged more annoying than fun.
- Health Regen Rate Multiplier default lowered from 5.0 to 0.5 (0.2/s -> 0.1/s) and Health Regen
  Cap Multiplier default lowered from 2.0 to 0.5 (20% -> 10%).
- Revive Duration Multiplier default lowered from 1.0 to 0.33 (4s -> 1.32s), moving it from
  "exposed but vanilla" into the core tuned defaults.
- Lock Melter and C-Foam Grenade spawn odds now default to disabled (0), joining Glow Stick.
- **Bug fix:** `Plugin.VERSION` was still hardcoded `5.0.0` from the last release despite all of
  the above - the in-game confirmation message would have shown the wrong major version. Fixed to
  match the real package version before release.
- Added a **Bots Always Equip Sentry Gun** option (off by default): forces a bot's Tool slot to the
  basic Sentry Gun the moment it gets gear, whether filling an empty slot or replacing a player.
  Resolved through the game's own `GD.Gear.Basic_SentryGun` name lookup rather than a hardcoded ID.
- Exposed `ArchetypeDataBlock` (every ranged weapon's fire modes, plus sentries) as ~380 new config
  options. Core combat stats (Damage, Stagger, Precision, Clip Size, Reload Time) get a two-tier
  multiplier - a Weapon Type slider shared by every fire mode in that family, times an Individual
  Weapon slider for one specific fire mode - covering ~50 archetypes across ~20 weapon types, all
  left at vanilla (1.0) by default. Every other archetype stat (ammo cost, fire rate, spread, burst
  timing, equip speed, etc.) plus sentry-specific targeting/combat stats get one flat multiplier
  shared by every weapon instead. IDs and ownership confirmed by logging
  `GearCategoryDataBlock`/`ArchetypeDataBlock` at boot.
- **Bug fix:** `package/README.md`'s embedded changelog copy had drifted out of sync with
  `package/CHANGELOG.md` - still headed `v6` and missing the two bullets above. Pre-release audit
  caught it; the two files are back in sync.
- **Bug fix:** the root `README.md` and `package/README.md` both still advertised "155 config
  entries" after the weapon/sentry exposure above added ~380 more. Updated to 530+ throughout.

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

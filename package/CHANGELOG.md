# Changelog

## v14

- New **Insert** (infinite ammo) and **Delete** (infinite health) toggles, announced as warden
  messages, shared by every GTFO.* mod so they never clash when several are installed.
- **Bug fix:** a toggle message from one GTFO.* mod and a message from another (GTFO.AbsurdWeapons'
  gear swap) replace each other without cutting the game's own objective messages short, even after
  a level change; Insert/Delete keep working next to a mod build from before the shared toggles; and
  with every kit weight at 0, *Consumables Are Kits* spawns no kits instead of the vanilla consumables.
- **Bug fix:** with GTFO.AbsurdWeapons installed and *Consumables Are Kits* off, a loot table where
  every item was disabled here stayed empty instead of getting AbsurdWeapons' mix. With its Loot Mix
  on, this mod now leaves consumable loot entirely to it.
- **Moved out:** the dash (hold Shift, double-tap W/A/S/D) now lives in GTFO.AbsurdMoves as its
  **Dodge**, with a 0.5 s double-tap window (was 0.3 s here). This mod's Dash config section is gone;
  install GTFO.AbsurdMoves to keep dashing.
- New **artifact buffs**: picking up an artifact buffs whoever picked it up for the rest of the
  level - each **Bold** +5% max health (healed too), each **Aggressive** +5% damage to enemies, each
  **Muted** 5% harder for enemies to detect you (capped at 75%). A checkpoint restart that puts an
  artifact back takes its buff off again. Tunable under the new Artifacts section.
- **Moved out:** the Stim Pack - the I2-LP's overheal (+50% health past max, draining back down)
  and its "Stim Pack" rename - now lives in GTFO.AbsurdWeapons, as a consumable of its own. The
  I2-LP is a vanilla heal again here, and the Consumables settings for it (heal amount, buff
  drain, display name) are gone.
- New **Consumables Are Kits** (Loot, on by default): consumables found in lockers, boxes or lying
  around are a few small health, tool and ammo kits of 1-2 charges each, adding about 20% to the
  zone's normal resource packs (which spawn as usual), split the way the zone's resource budget
  is. *Kit Loot (%)*, *Kit Charges Min/Max* and the per-kind weights are configurable. GTFO.EZ no longer turns
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
- **Bug fix:** Shift+E switched you off your sentry to your previous weapon even though your own
  placement was held back for the bot's; you now keep aiming it.
- **Bug fix:** on a client, Shift+E placed your own sentry. Shift+E and Z are host-only (bots
  belong to the host); on a client Shift+E now does nothing instead.
- **Bug fix:** X with your sentry put away could place it on floor behind a wall or closed door,
  on a steep edge, or while you were downed. The spot ahead of you now has to be flat ground you
  can see, and X does nothing while you're downed.
- **Bug fix:** a negative multiplier, range or percentage in the config applied as-is (negative
  health, damage or spawn counts); every such setting is now held at 0 or more. Defaults are
  unchanged.
- **Bug fix:** if no Fog Repeller was loaded the first time fog clearing looked for one, it used
  the fallback density for the rest of the session; it now looks again in the next level.
- With GTFO.AbsurdWeapons installed, everything it changes in the game's data applies after this
  mod's and wins where they clash - loot follows its own settings. See Compatibility.

## v13

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

## v11

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

## v10

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

## v9

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

## v8

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

## v5

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

## v4

- In-level warden-intel confirmation message now shows 90 seconds after spawn instead of 30, so
  it lands once you're actually in the level rather than right at the drop-in.

## v3

- Flashlight angle/intensity tweak now defaults to vanilla (x1.0) instead of x1.25 — vanilla
  flashlights were already fine as-is — but the config option is still there to retune if you want.
- Exposed more stats as config options, all left at vanilla by default: walk/run/air/crouch/ladder
  move speed, jump height, friendly fire damage, battery capacity and drain rate (small/medium/
  large tools), and no-air (suffocation) damage rate and depletion time.
- The in-game confirmation message now includes the mod's major version (e.g. "GTFO.EZ v3
  Active...").

## v2

- Added an in-game confirmation message: a line on the rundown-select screen, and a warden-intel
  message 30 seconds after you spawn into a level.

## v1

- Initial release: health, regen, stamina, fall damage, ammo, flashlight, and enemy detection
  tweaks, all config-driven multipliers applied at game-data load time.

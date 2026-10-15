---
codex: 1
project: GTFO.EZ
code: GE
layer: stories
status: living
updated: 2026-10-12
---

# GTFO.EZ — User Stories
> ✅ done (shipped & tested) · 🟡 partial · ⬜ planned. Every ✅ cites the test.

## Priority backlog
_Nothing recorded yet._ (GE-T5 closed — see Tickets below.)

## Tickets
> Permanent log of every change and fix — this repo's ticket system. Rows are append-only: never
> delete or reword one; only move its Status (open → closed / wont-do) and fill in Ref. Status is
> a word, not a glyph, so tickets don't count toward the story index. Keep this section last.

| ID | Date | Type | Status | Summary | Ref |
|---|---|---|---|---|---|
| GE-T1 | 2026-10-12 | docs | closed | Start the permanent ticket log in USER_STORIES.md | — |
| GE-T2 | 2026-10-14 | chore | wont-do | NUnit tests for GTFO.EZ multiplier math (src/GTFO.EZ/Patches/GameDataTweaks.cs WeaponMult/MeleeMult/ScaleInt): type x individual multipliers compose, a missing '<category>.<field>' or '<archetypeId>.<field>' key defaults to 1, ScaleInt rounds half away correctly for clip/burst/pellet counts (e.g. 3 x 1.5 = 5, 1 x 0.4 = 0); needs the pure helpers pulled into a class with no BepInEx/Il2Cpp interop dependency and a new test project, since the game DLLs only exist in a local mod profile | superseded by GE-T4 (wrong rounding example) |
| GE-T3 | 2026-10-14 | chore | open | NUnit tests for GTFO.EZ config tables (src/GTFO.EZ/Plugin.cs WeaponArchetypes, weapon type list, MeleeStatDefs, WeaponCoreStatDefs): archetype IDs are unique, every archetype's TypeCategory is a listed weapon type, every type has at least one archetype, and the generated config keys/section names are unique and BepInEx-legal - a duplicate or illegal key makes BindConfig throw and the whole mod fail to load | — |
| GE-T4 | 2026-10-14 | chore | open | NUnit tests for GTFO.EZ multiplier math (src/GTFO.EZ/Patches/GameDataTweaks.cs WeaponMult/MeleeMult/ScaleInt): type x individual multipliers compose, a missing '<category>.<field>' or '<archetypeId>.<field>' key defaults to 1, ScaleInt uses Math.Round's default banker's rounding (3 x 1.5 = 4, 5 x 0.5 = 2, 1 x 0.4 = 0) - pin that or switch to AwayFromZero deliberately; needs the pure helpers pulled into a class with no BepInEx/Il2Cpp dependency and a new test project, since the game DLLs only exist in a local mod profile | — |
| GE-T5 | 2026-10-15 | fix | closed | HIGHEST PRIORITY - Lock Melters, Explosive Trip Mines, and fog are still showing up constantly in actual play despite mod settings meant to tame them: LockMelterSpawnWeightMultiplier defaults to 0 (src/GTFO.EZ/Plugin.cs:412, disabled) and FogDensityMultiplier defaults to 0.1 (src/GTFO.EZ/Plugin.cs:376, 90% reduction), yet both keep appearing; ExplosiveTripMineSpawnWeightMultiplier is left at vanilla 1.0 (src/GTFO.EZ/Plugin.cs:415), so it was never reduced in the first place. First rule out a stale DEV .cfg (known gotcha - redeploying to DEV must wipe the old BepInEx config or new defaults never take effect); if a clean .cfg still reproduces it, audit ApplyFogTweaks/ApplyLootTweaks (src/GTFO.EZ/Patches/GameDataTweaks.cs:156-207) to confirm the Lock Melter weight-zero and fog density patches actually reach every FogSettingsDataBlock/consumable pool in play, and consider lowering ExplosiveTripMineSpawnWeightMultiplier's default if vanilla frequency is itself the complaint | Root cause found: LockMelterSpawnWeightMultiplier's default changed 1.0->0.0 in v8 (and C-Foam Grenade likewise, Glow Stick in v5), but BepInEx preserves an existing .cfg value across upgrades, so any pre-v8/pre-v5 install stayed stuck at the old vanilla default forever - confirmed via git history of Plugin.cs. Fixed in v9 with a one-time ConfigMigrationVersion-gated migration (src/GTFO.EZ/Plugin.cs: ApplyLootDefaultMigrations/ResetIfAtOldDefault) that nudges all three to their intended default on first v9 launch. FogDensityMultiplier was never changed (always 0.1 since introduction in v8) so it isn't affected by this bug. User then asked (2026-10-15) to also disable Explosive Trip Mine/C-Foam Tripmine spawn odds (previously vanilla 1.0 by design) - done in the same v9: both now default to 0, folded into the same ResetIfAtOldDefault migration (bumped to migration version 2) so existing installs upgrading past v9 get nudged off the old vanilla default too |

# GTFO.EZ

Dev repo for **GTFO.EZ**, a BepInEx IL2CPP plugin for GTFO that tunes player stats (health, regen,
stamina, fall damage, ammo, enemy detection range) for an easier co-op experience. A curated set
of defaults covers that core experience; underneath it, 530+ config entries expose nearly every
other per-zone, per-enemy, and per-weapon balance value the game has, for players who want to go
further — see `package/README.md` for the full breakdown.

Published on Thunderstore: https://thunderstore.io/c/gtfo/p/MindAttic/GtfoEZ/

Unlike Mendu's EasyMode (a datablock-JSON mod via MTFO), this ships no copies of the game's data
files. It patches `GameDataInit.Initialize` and adjusts fields on the game's own loaded datablocks
at runtime as multipliers, so it keeps working across patches that rebalance base values instead
of going stale.

## Layout

```
src/GTFO.EZ/          the plugin project (BepInEx/Il2Cpp - needs the game installed to build)
src/GTFO.EZ.Core/     pure balance data/math the plugin uses, with no BepInEx/Il2Cpp dependency -
                      split out so it's unit-testable; ships alongside GTFO.EZ.dll at runtime
tests/                GTFO.EZ.Core.Tests - NUnit, runs with plain `dotnet test`, no game needed
package/              exactly what ships to Thunderstore / gets zipped for sharing
  manifest.json       Thunderstore package name is "GtfoEZ" (no period allowed there)
  README.md           becomes the Thunderstore page
  CHANGELOG.md
  icon.png            256x256, resized from reference/logo-1254x1254.png
reference/            (gitignored) local-only: EasyMode's files, vanilla data dumps, notes
build.ps1             builds the plugin and packages dist/GTFO.EZ-<version>.zip
```

## Building

Requires BepInExPack_GTFO already installed, and launched at least once ("Modded" start), in a
Thunderstore Mod Manager or r2modman profile.

```powershell
.\build.ps1                      # uses the Thunderstore Mod Manager profile "Easy Mode DEV"
.\build.ps1 -Profile MyProfile
```

If you're on the older r2modman app instead of Thunderstore Mod Manager, its profiles live under
a different path — pass it directly instead:
```powershell
dotnet build .\src\GTFO.EZ\GTFO.EZ.csproj -p:BepInEx="$env:AppData\r2modmanPlus-local\GTFO\profiles\Default\BepInEx"
```

This builds straight into that profile's `BepInEx/plugins/GTFO.EZ/` folder (so you can launch the
game and test immediately) and also produces `dist/GTFO.EZ-<version>.zip` for sharing or upload.

## Testing

```powershell
dotnet test tests/GTFO.EZ.Core.Tests/GTFO.EZ.Core.Tests.csproj
```

No game install or BepInEx profile needed - `GTFO.EZ.Core` holds the plugin's pure balance data
and math (weapon/melee config-table generation, the `ScaleInt`/multiplier-composition logic) with
no BepInEx/Il2Cpp dependency, so it's unit-tested directly. The actual datablock patches in
`src/GTFO.EZ/Patches/` still can't be unit tested - they only run against the game's own loaded
data - so verify those by launching the game after `build.ps1`.

## Config

All tweaks are multipliers, tunable after first launch in
`BepInEx/config/MindAttic.GTFO.EZ.cfg` — no rebuild needed to retune values.

BepInEx keeps whatever value is already in a player's `.cfg`, so when this mod changes a default
between versions, an existing install won't pick up the new number on its own — see
`Plugin.ApplyConfigDefaultMigrations` for the one-time, per-setting correction this mod applies
instead of leaving upgraders silently stuck on a stale default.

## Versioning

v1, v2, v3... — major-only bumps, matching the convention across other MindAttic projects.
Thunderstore requires `manifest.json`'s `version_number` to be strict `x.y.z`, so "v1" is
`1.0.0`, "v2" is `2.0.0`, and so on (minor/patch stay 0). `package/CHANGELOG.md` headers use the
short `vN` form; the manifest keeps the `N.0.0` form Thunderstore requires.

## Sharing with friends

- **Private:** Import the zip from `dist/` as a local mod in your mod manager, then
  `Settings -> Profile -> Export profile as code` and send friends the code.
- **Public:** Published on Thunderstore at https://thunderstore.io/c/gtfo/p/MindAttic/GtfoEZ/.
  Everyone in a lobby needs the identical mod + config.

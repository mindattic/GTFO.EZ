# GTFO-EZ

Dev repo for **GtfoEZ**, a BepInEx IL2CPP plugin for GTFO that tunes player stats (health, regen,
stamina, fall damage, ammo, flashlights, enemy detection range) for an easier co-op experience.

Unlike Mendu's EasyMode (a datablock-JSON mod via MTFO), this ships no copies of the game's data
files. It patches `GameDataInit.Initialize` and adjusts fields on the game's own loaded datablocks
at runtime as multipliers, so it keeps working across patches that rebalance base values instead
of going stale.

## Layout

```
src/GtfoEZ/           the plugin project
package/              exactly what ships to Thunderstore / gets zipped for sharing
  manifest.json
  README.md           becomes the Thunderstore page
  CHANGELOG.md
  icon.png            256x256, resized from reference/logo-1254x1254.png
reference/            (gitignored) local-only: EasyMode's files, vanilla data dumps, notes
build.ps1             builds the plugin and packages dist/GtfoEZ-<version>.zip
```

## Building

Requires BepInExPack_GTFO already installed, and launched at least once ("Modded" start), in a
Thunderstore Mod Manager or r2modman profile.

```powershell
.\build.ps1                      # uses the Thunderstore Mod Manager profile "Easy Mode"
.\build.ps1 -Profile MyProfile
```

If you're on the older r2modman app instead of Thunderstore Mod Manager, its profiles live under
a different path — pass it directly instead:
```powershell
dotnet build .\src\GtfoEZ\GtfoEZ.csproj -p:BepInEx="$env:AppData\r2modmanPlus-local\GTFO\profiles\Default\BepInEx"
```

This builds straight into that profile's `BepInEx/plugins/GtfoEZ/` folder (so you can launch the
game and test immediately) and also produces `dist/GtfoEZ-<version>.zip` for sharing or upload.

## Config

All tweaks are multipliers, tunable after first launch in
`BepInEx/config/MindAttic.GtfoEZ.cfg` — no rebuild needed to retune values.

## Sharing with friends

- **Private:** Import the zip from `dist/` as a local mod in your mod manager, then
  `Settings -> Profile -> Export profile as code` and send friends the code.
- **Public:** Upload the zip to Thunderstore under your own team. Everyone in a lobby needs the
  identical mod + config.

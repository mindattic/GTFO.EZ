# GtfoEZ

A small, config-driven BepInEx plugin for GTFO that tunes player stats for an easier, more
relaxed co-op experience. No datablock JSON files are shipped — it patches the game's own data
in memory at load time, so it keeps working across game patches that only change base values.

## What it changes

| Tweak | Default |
|---|---|
| Max health | x2 |
| Health regen rate | x2 (and starts sooner) |
| Stamina regen rate | x1.5 |
| Fall damage | x0.5 |
| Fall height before damage | x1.5 |
| Weapon/tool ammo (reserve, starting, refill packs) | x1.5 |
| Headlight & weapon light range/intensity | x1.3 |
| Enemy movement-noise detection distance | x0.75 (8m -> 6m by default) |

Every value above is a multiplier you can retune in the config file BepInEx generates after
first launch (`BepInEx/config/MindAttic.GtfoEZ.cfg`), without recompiling anything.

## Install

Everyone in the lobby needs the same mod and config. Use Thunderstore Mod Manager or r2modman,
or import the zip manually and share a profile code.

## Credit

Tweak values inspired by Mendu's [EasyMode](https://thunderstore.io/c/gtfo/p/Mendu/EasyMode/).
This mod is an independent reimplementation as a code plugin rather than datablock JSON, built
to survive game updates without needing a rebuild for every patch.

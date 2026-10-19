# Model catalog

Lists every mesh and every top-level object (prefab or scene root) in GTFO's game data, read offline
with [UnityPy](https://github.com/K0lb3/UnityPy). Use it to find a model worth borrowing in a mod.
The output goes to `reference/gtfo-models.csv`, the one file in `reference/` that's committed (the
rest of that folder is git-ignored). It never ships: `build.ps1` zips `package\*` only.

It can't list load paths such as `Assets/AssetPrefabs/Items/Consumables/LockMelter/...prefab`, which
`AssetShardManager.GetLoadedAsset` needs. The shard manifests holding those paths have no type
information in this IL2CPP build, so only the running game can list them.

Rerun after a game update, from a scratch folder:

```powershell
python -m venv <scratch>\unitypy-venv
<scratch>\unitypy-venv\Scripts\python.exe -m pip install UnityPy
<scratch>\unitypy-venv\Scripts\python.exe -I catalog.py "<Steam>\steamapps\common\GTFO\GTFO_Data" <scratch>\catalog.json
<scratch>\unitypy-venv\Scripts\python.exe -I build_page.py <scratch>\catalog.json ..\..\reference\gtfo-models.csv <scratch>\gtfo-model-catalog.html
<scratch>\unitypy-venv\Scripts\python.exe -I search.py <scratch>\catalog.json flare tube canister
```

`build_page.py` also writes the searchable HTML page from `page_template.html`.

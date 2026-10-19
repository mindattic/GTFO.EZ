"""Offline catalog of GTFO's models: every Mesh and every root GameObject (a prefab or scene root) in
GTFO_Data, with the file it lives in and, for meshes, vertex count and bounding-box size.

usage: python -I catalog.py <GTFO_Data> <out.json>
"""
import json
import os
import sys
import time

import UnityPy

data, out = sys.argv[1], sys.argv[2]
files = sorted(f for f in os.listdir(data)
               if f.endswith(".assets") or f.startswith("level") or f in ("globalgamemanagers", "resources.assets"))
meshes, roots, failed = [], [], []
start = time.time()
for i, name in enumerate(files, 1):
    try:
        env = UnityPy.load(os.path.join(data, name))
        for obj in env.objects:
            kind = obj.type.name
            if kind == "Mesh":
                try:
                    mesh = obj.read()
                    ext = mesh.m_LocalAABB.m_Extent
                    size = [round(2 * float(v), 3) for v in (ext.x, ext.y, ext.z)]
                    verts = getattr(getattr(mesh, "m_VertexData", None), "m_VertexCount", 0)
                    meshes.append({"name": mesh.m_Name, "file": name, "vertices": verts, "size": size})
                except Exception:
                    meshes.append({"name": obj.peek_name() or "", "file": name})
            elif kind == "Transform":
                try:
                    t = obj.read()
                    if t.m_Father.path_id != 0:
                        continue
                    go_name = t.m_GameObject.read().m_Name
                    roots.append({"name": go_name, "file": name, "children": len(t.m_Children or [])})
                except Exception:
                    continue
    except Exception as e:
        failed.append({"file": name, "error": f"{type(e).__name__}: {e}"[:200]})
    if i % 50 == 0:
        print(f"{i}/{len(files)} files, {len(meshes)} meshes, {len(roots)} roots, {time.time() - start:.0f}s", flush=True)

with open(out, "w", encoding="utf-8") as f:
    json.dump({"meshes": meshes, "roots": roots, "failed": failed, "files": len(files)}, f)
print(f"done: {len(files)} files, {len(meshes)} meshes, {len(roots)} roots, {len(failed)} failed, {time.time() - start:.0f}s")

"""Search the model catalog: python -I search.py <catalog.json> <regex> [...]"""
import json
import re
import sys

sys.stdout.reconfigure(encoding="utf-8")
data = json.load(open(sys.argv[1], encoding="utf-8"))
print(f"{len(data['meshes'])} meshes ({sum(1 for m in data['meshes'] if 'size' in m)} with size), {len(data['roots'])} roots")
pattern = re.compile("|".join(sys.argv[2:]), re.I)
seen = set()
for m in data["meshes"]:
    if not pattern.search(m["name"]):
        continue
    key = (m["name"], tuple(m.get("size", [])))
    if key in seen:
        continue
    seen.add(key)
    print(f"mesh  {m['name']:<55} {str(m.get('size')):<26} v={m.get('vertices', '?'):<6} {m['file']}")
seen = set()
for r in data["roots"]:
    if pattern.search(r["name"]) and r["name"] not in seen:
        seen.add(r["name"])
        print(f"root  {r['name']:<55} children={r['children']:<4} {r['file']}")

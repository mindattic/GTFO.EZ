"""Turn catalog.json into a CSV and the browsable HTML page.

usage: python -I build_page.py <catalog.json> <out.csv> <out.html>
"""
import csv
import json
import sys

src, out_csv, out_html = sys.argv[1:4]
data = json.load(open(src, encoding="utf-8"))

meshes, seen = [], set()
for m in data["meshes"]:
    # A mesh catalog.py couldn't read has no size or vertex count: blank, not a real 0 x 0 x 0.
    size = m.get("size") or ["", "", ""]
    key = (m["name"], tuple(size), m["file"])
    if key in seen:
        continue
    seen.add(key)
    meshes.append([m["name"], m["file"], m.get("vertices", ""), *size])
roots, seen = [], set()
for r in data["roots"]:
    key = (r["name"], r["file"])
    if key in seen or not r["name"]:
        continue
    seen.add(key)
    roots.append([r["name"], r["file"], r["children"]])
meshes.sort(key=lambda m: m[0].lower())
roots.sort(key=lambda r: r[0].lower())

with open(out_csv, "w", encoding="utf-8", newline="") as f:
    w = csv.writer(f)
    w.writerow(["kind", "name", "file", "vertices", "size_x_m", "size_y_m", "size_z_m", "children"])
    for m in meshes:
        w.writerow(["mesh", m[0], m[1], m[2], m[3], m[4], m[5], ""])
    for r in roots:
        w.writerow(["root", r[0], r[1], "", "", "", "", r[2]])

payload = json.dumps({"meshes": meshes, "roots": roots, "files": data["files"]}, separators=(",", ":"), ensure_ascii=False)
payload = payload.replace("</", "<\\/")
template = open(sys.argv[0].replace("build_page.py", "page_template.html"), encoding="utf-8").read()
open(out_html, "w", encoding="utf-8").write(template.replace("/*__DATA__*/null", payload))
print(f"{len(meshes)} meshes, {len(roots)} roots -> {out_csv}, {out_html}")

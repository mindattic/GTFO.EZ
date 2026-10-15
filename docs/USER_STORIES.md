---
codex: 1
project: GTFO.EZ
code: GE
layer: stories
status: living
updated: 2026-10-15
---
<!-- workflow-db: this repo's stories and tickets live in the MindAttic.Workflow database -->

# GTFO.EZ — User Stories

This repo's stories and tickets (GE-US-…, GE-T…) live in the MindAttic.Workflow database
(`MindAttic.Workflow/data/workflow.db`), not in this file. Use the `workflow` CLI:

- Read: `workflow story list --repo GTFO.EZ`, `workflow ticket list --repo GTFO.EZ`, `workflow ticket show <ID>`
- Change: `workflow ticket new` / `close`, `workflow story status <ID> done|partial|planned`, `workflow ticket link <A> requires <B>`
- This file as it was, rebuilt from the database: `workflow db export --repo GTFO.EZ`

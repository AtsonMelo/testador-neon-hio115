---
name: issue-24-layout-alvo-2
description: Use only when reviewing the historical Issue #24 Layout Alvo 2 work; current production UI guidance lives in docs/design/ui-industrial-2.md.
---

# Issue 24: Layout Alvo 2

Use this skill only to understand or review the historical Issue #24 work.

## Reference Rules

- Treat the archived Layout Alvo 2 documents as historical context, not as the
  current production reference.
- Use `docs/design/ui-industrial-2.md` for the current production UI.
- Do not restore controls removed by Project Cleanup 1 unless a future task
  proves a new functional requirement independently.

## Integration Rules

- Keep `MainForm.cs` focused on composition, events, and services.
- Do not move detailed layout logic into `MainForm.cs`.
- Do not apply old stashes unless the user explicitly asks for it.
- Treat existing stashes only as backup material, not as the active source for integration.

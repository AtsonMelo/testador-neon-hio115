---
name: issue-24-layout-alvo-2
description: Use for Issue #24 work where the approved Layout Alvo 2 is the visual reference and the integration should remain gradual, componentized, and non-disruptive.
---

# Issue 24: Layout Alvo 2

Use this skill only for Issue #24 work.

## Reference Rules

- Treat the approved visual state of Layout Alvo 2 as the primary reference.
- Integrate the layout gradually through separated components.
- Preserve `IndustrialMainContentControl` and `IndustrialManualIoPanelControl` as working references and components.

## Integration Rules

- Keep `MainForm.cs` focused on composition, events, and services.
- Do not move detailed layout logic into `MainForm.cs`.
- Do not apply old stashes unless the user explicitly asks for it.
- Treat existing stashes only as backup material, not as the active source for integration.

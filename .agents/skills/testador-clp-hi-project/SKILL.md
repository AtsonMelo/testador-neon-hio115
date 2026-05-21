---
name: testador-clp-hi-project
description: Use for work in the TestadorCLPHI.App repository, especially context about the HI/NEON HIO115 CLP test application, component/service architecture, and HIstudio integration constraints.
---

# TestadorCLPHI.App Context

Use this skill when working on the TestadorCLPHI.App project.

## Project Context

- This is a Windows application used to test the HI/NEON HIO115 PLC.
- Preserve the component/service-oriented architecture already used by the app.
- Avoid unnecessary growth in `MainForm.cs`.
- Prefer moving behavior into components, controls, or services instead of expanding the form class.

## HIstudio Constraints

- The `.dpk` file from HIstudio is not updated automatically by Git, ST, or CSV-based workflows.
- If changes affect executable HIstudio artifacts, they must be made, compiled, and loaded in HIstudio when applicable.
- Do not assume repository files are the full source of truth for HIstudio runtime behavior.

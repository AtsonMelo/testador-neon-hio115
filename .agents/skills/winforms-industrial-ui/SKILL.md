---
name: winforms-industrial-ui
description: Use for WinForms UI work in an industrial dark-theme application, especially when preserving existing controls, layout structure, responsiveness, and reusable UI components.
---

# WinForms Industrial UI

Use this skill for UI work in the WinForms app.

## UI Rules

- Prefer isolated components under `app/TestadorCLPHI.App/Ui/Industrial` or `Ui/Controls`.
- Preserve the dark industrial theme.
- Reuse existing industrial PNG assets when they fit the task.
- Validate layout and usability for notebook screens and high-DPI setups.
- Avoid blind visual trial and error.
- Do not rewrite the application wholesale.

## Layout Guidance

- Keep UI changes modular and composable.
- Favor reusable controls over hard-coded layout inside `MainForm.cs`.
- Make sure controls remain readable and stable when resized.

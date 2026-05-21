---
name: safe-git-powershell
description: Use for safe PowerShell-based work in Git repositories, especially when making small reversible changes, validating diffs, and handling command output carefully.
---

# Safe Git PowerShell

Use this skill when working in PowerShell on Git-tracked changes.

## Workflow

- Check `git status --short` before editing and again before finishing.
- Keep changes small, scoped, and easy to revert.
- Do not mix refactor, UI, bugfix, and feature work in the same change unless the user explicitly asks for that mix.
- Prefer one narrow change at a time.
- Run `git diff --check` before considering a change complete.
- If the change touches C#, run the relevant validation or test command available for the repo before finishing.
- Do not commit until the change is validated.

## PowerShell Output Handling

- When command output matters, capture it in `$output`.
- Copy important output with `$output | Set-Clipboard` when you need to preserve it.
- Prefer variable capture over re-running commands just to inspect or reuse their output.

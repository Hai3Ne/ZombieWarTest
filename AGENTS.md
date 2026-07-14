# TSC Unity Workflow for Codex

The Unity project is in `ZombieWar/`. The shared workflow kit is pinned in
`tsc-unity-kit/` as a Git submodule.

## Working rules

After the direct user request, follow:

1. `.tsc_workspace/rules/agent-principles.md`
2. `.tsc_workspace/rules/coding-standard.md`
3. `.tsc_workspace/rules/definition-of-done.md`

Use `.tsc_workspace/` for plans and documentation, and use the
adapters under `tsc-unity-kit/.agents/skills/` for command-specific guidance.

## Commands

- `$init`: load the shared project context.
- `$code [Feature]`: generate a TSC-style MonoBehaviour.
- `$ui [Panel]`: generate a UI controller.
- `$fix`: analyze, repair, and verify a problem.
- `$check`: audit code against the TSC rules.
- `$docs [Topic]`: read project documentation.
- `$plan`: read or update the active plan.

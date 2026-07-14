# TSC Unity Workflow for Claude

The Unity project is in `ZombieWar/`. The shared workflow kit is pinned in
`tsc-unity-kit/` as a Git submodule.

Before non-trivial work, load the relevant context, choose the smallest viable
solution, define successful completion, make only the required change, and
verify it.

After the direct user request, follow:

1. `.tsc_workspace/rules/agent-principles.md`
2. `.tsc_workspace/rules/coding-standard.md`
3. `.tsc_workspace/rules/definition-of-done.md`

Use `.tsc_workspace/` for plans and documentation, and use the
adapters under `tsc-unity-kit/.claude/skills/` for command-specific guidance.

Commands: `$init`, `$code [Feature]`, `$ui [Panel]`, `$fix`, `$check`,
`$docs [Topic]`, `$plan`, `$rules`, and `$promote`.

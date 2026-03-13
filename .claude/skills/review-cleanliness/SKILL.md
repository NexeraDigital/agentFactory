---
context: fork
allowed-tools: Read, Glob, Grep, Bash
---

# Code Cleanliness Batch Review

Review staged or recently changed files against code cleanliness rules.

## Procedure

1. Run `git diff --cached --name-only` to find staged files. If none staged, run `git diff --name-only` for unstaged changes.
2. For each changed file, read the full file content.
3. Check each file against the code cleanliness rules loaded in context (from `.claude/rules/`):
   - **CC-001**: Method length ≤ 20 executable lines
   - **CC-002**: No nested if statements
   - **CC-003**: Class cohesion (fields used by most methods)
   - **CC-004**: Constructor parameters ≤ 4
   - **CC-005**: Anti-abstraction bias
   - **CC-006**: Prefer declarative style
   - **CC-007**: Components ≤ 200 lines (frontend)
   - Plus universal rules: UNI-001 through UNI-005
4. Report findings grouped by file, using this format:

```
## [filename]

### Issues (must fix)
- [RULE-ID] [SEVERITY] description — file:line

### Warnings (should fix)
- [RULE-ID] [SEVERITY] description — file:line

### Praise
- [description of good patterns found]
```

5. End with a summary: total issues, total warnings, overall cleanliness assessment.

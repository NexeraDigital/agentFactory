---
description: "Run sentinel security review on staged or changed files"
allowed-tools: Read, Glob, Grep, Bash, Agent
---

# Security Review (Sentinel)

Run the sentinel agent to perform a security audit on changed files.

## Procedure

1. Run `git diff --cached --name-only` to find staged files. If none, run `git diff --name-only` for unstaged changes. If still none, ask the user which files to review.
2. Filter to security-relevant files (code files, not docs/config).
3. Invoke the **sentinel** agent with:
   - The list of changed files as targets
   - Instructions to trace dependencies and produce the full structured report
4. Present the sentinel report to the user.

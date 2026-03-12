# CLAUDE.md Architecture: Lean Index + Reference Files

## The Problem: CLAUDE.md Bloat

CLAUDE.md is loaded into **every** conversation's context. If we pack all rules, design systems, layer definitions, project knowledge, and agent workflow instructions into it, it becomes a massive token tax on every interaction -- including ones that have nothing to do with those rules.

Consider what the migration wants to put in CLAUDE.md:
- ~15-20 inlined rules across 8 domains (~60 lines)
- IDesign layer definitions and dependency table (~30 lines, now that idesign-architect is HYBRID)
- Modern UI design system reference (~20 lines minimum, 337 lines if full)
- Project knowledge promoted from agent memory (~40-80 lines)
- Agent workflow instructions (~30 lines)
- Existing project context (tech stack, build commands, etc.)

That easily reaches 200-400 lines before any project-specific content. A developer asking "fix this CSS bug" pays the token cost of loading IDesign layer definitions and SQL anti-patterns they'll never need.

## The Solution: CLAUDE.md as Lean Index

CLAUDE.md should contain only two things:

1. **Universal rules** -- the 5-10 rules that apply to literally every edit regardless of domain (e.g., "no silent fallbacks," "no nested ifs"). These are worth the always-loaded cost.
2. **Pointers to reference files** -- short descriptions with file paths that tell Claude where to look when working in a specific domain. Claude reads these on demand using the Read tool.

Everything else lives in reference files that Claude reads **only when relevant**.

## Reference File Structure

```
docs/
├── review-rules/                    # Hook-triggered review rules
│   ├── backend-rules.md             # BE-001 through BE-008
│   ├── react-rules.md               # RC-001 through RC-008
│   ├── security-rules.md            # SEC-001 through SEC-006
│   ├── idesign-rules.md             # ID-001 through ID-007 + layer definitions
│   ├── sql-rules.md                 # SQL-001 through SQL-007
│   ├── table-storage-rules.md       # TS-001 through TS-006
│   ├── code-cleanliness-rules.md    # CC-001 through CC-007
│   └── ui-design-rules.md           # UI-001 through UI-008
├── design-system.md                 # Full design system (from modern-ui-agent)
└── architecture.md                  # IDesign layers, dependency direction, project architecture
```

## CLAUDE.md Template (Lean Version)

```markdown
## Universal Rules (All Code)

- NEVER use silent fallbacks (`??` with defaults, catch-return-default, TrySomething patterns)
- Methods must be under 20 lines of executable code; no nested if statements
- Constructor parameters must be 4 or fewer
- Every query on user-owned entities MUST include a user identity predicate
- UserId must come from authenticated identity, NEVER from request input

## Reference Files

Read these files when working in the relevant domain. Do NOT read files for
domains you are not currently working in.

| Domain | Reference File | When to Read |
|--------|---------------|--------------|
| Backend (.cs) | `docs/review-rules/backend-rules.md` | Writing or reviewing C# code |
| React (.tsx/.ts) | `docs/review-rules/react-rules.md` | Writing or reviewing React components |
| Security | `docs/review-rules/security-rules.md` | Writing controllers, auth, data access |
| Architecture | `docs/review-rules/idesign-rules.md` | Creating or modifying Manager/Engine/Accessor classes |
| SQL / EF Core | `docs/review-rules/sql-rules.md` | Writing migrations, queries, DbContext changes |
| Table Storage | `docs/review-rules/table-storage-rules.md` | Writing Table Storage entities or accessors |
| Code cleanliness | `docs/review-rules/code-cleanliness-rules.md` | Any code authoring (all languages) |
| UI Design | `docs/review-rules/ui-design-rules.md` | Styling, layout, component visual design |
| Design System | `docs/design-system.md` | Creating new UI components or pages |
| Architecture Overview | `docs/architecture.md` | Understanding layer boundaries and project structure |
```

## How This Works with the Three Layers

**Layer 1 (CLAUDE.md)**: Only the ~5 universal rules are always loaded. Claude follows these during generation with zero extra cost.

**Layer 2 (PostToolUse hooks)**: The hook's stderr prompt tells Claude which rules file to read: *"Review this edit against docs/review-rules/backend-rules.md."* Claude reads the file on demand -- tokens are spent only when the hook fires on a relevant file.

**Layer 3 (PreCommit hook)**: The PreCommit prompt tells Claude to read all rules files relevant to the staged changes. Tokens are spent once per commit.

**On-demand (agents)**: HYBRID agents for planning sessions read the reference files they need. The agent prompt can say *"Read docs/architecture.md for project context before designing."*

## Token Impact

| Approach | CLAUDE.md Size | Per-Conversation Cost |
|----------|---------------|----------------------|
| Everything inlined | ~300-400 lines | ~6-8K tokens on every conversation |
| Lean index + references | ~30-40 lines | ~600-800 tokens + on-demand reads only when relevant |

A developer fixing a CSS bug loads ~600 tokens of CLAUDE.md instead of ~8K. A developer writing a new Manager loads ~600 tokens of CLAUDE.md + ~2K tokens of idesign-rules.md (read on demand) = ~2.6K total. Both are significantly cheaper than the inlined approach.

## What the Hooks Need to Change

The PostToolUse hook stderr message must explicitly tell Claude to read the rules file, since it's no longer inlined in CLAUDE.md:

```bash
# Before (rules inlined in CLAUDE.md -- Claude already knows them):
echo "Review this edit for backend rule violations." >&2

# After (rules in reference file -- Claude needs to read it):
echo "Read docs/review-rules/backend-rules.md then review the edit you just made to $FILE. Report any violations with rule IDs." >&2
```

This adds one Read tool call per hook trigger -- a small cost that's far outweighed by the savings from not loading all rules on every conversation.

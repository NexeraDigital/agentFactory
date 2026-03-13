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
.claude/
├── rules/                           # Auto-loading rules with paths: frontmatter
│   ├── universal.md                 # Always loaded (no paths: frontmatter)
│   ├── security-universal.md        # Always loaded (no paths: frontmatter)
│   ├── backend.md                   # paths: ["**/*.cs", "!**/*Test*.cs"]
│   ├── react.md                     # paths: ["**/*.tsx", "**/*.ts"]
│   ├── idesign.md                   # paths: ["**/*Manager.cs", "**/*Engine.cs", "**/*Accessor.cs"]
│   ├── sql.md                       # paths: ["**/*Accessor.cs", "**/Migrations/**", "**/*.sql"]
│   ├── table-storage.md             # paths: ["**/TableStorage/**"]
│   ├── ui-design.md                 # paths: ["**/*.tsx", "**/*.css", "**/*.scss"]
│   └── code-cleanliness.md          # paths: ["**/*.cs", "**/*.tsx", "**/*.ts"]
├── settings.json                    # Hook configuration (prompt/agent handlers)
├── skills/                          # On-demand batch review commands
│   ├── review-backend/SKILL.md
│   ├── review-security/SKILL.md
│   └── review-cleanliness/SKILL.md
└── agents/                          # HYBRID agents for planning sessions
docs/
├── design-system.md                 # Full design system (from modern-ui-agent)
└── architecture.md                  # IDesign layers + project structure
```

## CLAUDE.md Template (Lean Version)

```markdown
## Universal Rules (All Code)

- NEVER use silent fallbacks (`??` with defaults, catch-return-default, TrySomething patterns)
- Methods must be under 20 lines of executable code; no nested if statements
- Constructor parameters must be 4 or fewer
- Every query on user-owned entities MUST include a user identity predicate
- UserId must come from authenticated identity, NEVER from request input

## Project Context

@docs/architecture.md
@docs/design-system.md

## Domain Rules

Domain-specific rules auto-load from `.claude/rules/` when you access relevant files.
No manual reading required -- rules with `paths:` frontmatter load conditionally.
```

## The `@path` Import Syntax

CLAUDE.md supports `@path` import syntax (up to 5 hops deep) for automatic inclusion of referenced documents. When CLAUDE.md contains `@docs/architecture.md`, Claude Code automatically inlines that file's content into the context at the start of every conversation -- no Read tool call required.

Use `@path` imports for large reference docs that should always be available: architecture overviews, design systems, and similar project-wide references. Do not use `@path` imports for domain rules -- those belong in `.claude/rules/` where `paths:` frontmatter handles conditional loading automatically.

## How This Works with the Three Layers

**Layer 1 (CLAUDE.md + `.claude/rules/`)**: Rules files in `.claude/rules/` without `paths:` frontmatter (e.g., `universal.md`, `security-universal.md`) load on every session alongside CLAUDE.md. Rules files with `paths:` frontmatter load conditionally -- only when the current conversation involves files matching those patterns. Zero extra tool calls for either case.

**Layer 2 (`prompt`/`agent` hooks in `.claude/settings.json`)**: Hooks check edits against rules already in context from the auto-loaded `.claude/rules/` files. Because the relevant rules are already present, no Read tool call is needed -- the hook simply asks Claude to verify the edit against the rules it already has.

**Layer 3 (Husky pre-commit hook)**: Runs outside Claude Code via standard git hooks. Can invoke the Agent SDK for a full rules review of staged changes, or run static linters directly. Tokens are spent once per commit.

**On-demand (agents)**: HYBRID agents for planning sessions read the reference files they need. The agent prompt can say *"Read docs/architecture.md for project context before designing."*

## Token Impact

| Approach | Context Size | Per-Conversation Cost |
|----------|-------------|----------------------|
| Everything inlined in CLAUDE.md | ~300-400 lines | ~6-8K tokens on every conversation |
| `.claude/rules/` with `paths:` frontmatter | ~30-40 lines CLAUDE.md + relevant rules only | ~600-800 tokens base + only matching domain rules loaded |

Universal rules (`universal.md`, `security-universal.md`) are always loaded alongside CLAUDE.md. Domain rules load only when the conversation touches files that match their `paths:` patterns. A developer fixing a CSS bug loads the base ~600-800 tokens plus `ui-design.md` and `code-cleanliness.md` -- not SQL rules, not IDesign rules. A developer writing a new Manager loads the base plus `backend.md`, `idesign.md`, and `code-cleanliness.md`. Both are significantly cheaper than the fully-inlined approach, and neither requires any Read tool calls to access rules.

## What the Hooks Need to Change

With `.claude/rules/` and `paths:` frontmatter, hooks no longer need to instruct Claude to read rules files. The relevant rules are already in context by the time any hook fires, because Claude Code loaded them automatically when the relevant files were accessed.

The `prompt` hook type in `.claude/settings.json` simply asks Claude to check the edit against rules that are already loaded -- no Read tool call is required:

```bash
# Old approach (rules in reference file -- hook had to tell Claude to read it):
echo "Read docs/review-rules/backend-rules.md then review the edit you just made to $FILE. Report any violations with rule IDs." >&2

# New approach (rules auto-loaded via .claude/rules/ -- already in context):
echo "Review the edit you just made to $FILE against the backend and cleanliness rules already in your context. Report any violations with rule IDs." >&2
```

This eliminates the Read tool call overhead entirely. Rules arrive in context as part of the session setup -- before any hook fires -- so the hook's only job is to trigger the review, not to fetch the rules.

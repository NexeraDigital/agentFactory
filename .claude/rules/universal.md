---
description: "Universal code quality rules applied to all files. Enforces core principles that prevent the most common quality issues."
---

# Universal Code Quality Rules

These rules apply to **all code** regardless of language or framework. They represent the highest-value quality checks that prevent the most common defects.

| ID | Severity | Rule | Details |
|----|----------|------|---------|
| UNI-001 | **Critical** | No silent fallbacks | Never swallow exceptions, return defaults on failure, or use `??` with fallback values that hide errors. Failures must be explicit — throw, log and rethrow, or return a meaningful error. |
| UNI-002 | **High** | Method length ≤ 20 lines | Methods over 20 executable lines (skip blanks, braces, comments) are a red flag for SRP/SoC violation. Extract and compose. |
| UNI-003 | **High** | No nested if statements | Use early returns (guard clauses), pattern matching, or switch expressions to flatten logic. If nesting seems unavoidable, the method is doing too much — extract. |
| UNI-004 | **Medium** | Constructor parameters ≤ 4 | More than 4 constructor parameters is a coupling smell. Consider splitting the class, introducing a parameter object, or re-evaluating responsibilities. |
| UNI-005 | **Medium** | High cohesion, low coupling | Every field in a class should be used by most methods. If methods cluster into groups using different field subsets, the class should be split. Prefer composition over inheritance. |

## Bad Examples

```
// UNI-001 VIOLATION: Silent fallback hides failure
var result = await TryGetFromCacheAsync() ?? await GetFromDatabaseAsync();

// UNI-001 VIOLATION: Swallowing exception
try { return await GetDataAsync(); }
catch { return DefaultData(); }

// UNI-003 VIOLATION: Nested conditionals
if (user != null) {
    if (user.IsActive) {
        if (user.HasPermission("edit")) {
            // do work
        }
    }
}
```

## Good Examples

```
// UNI-001 CORRECT: Explicit failure
var result = await GetFromDatabaseAsync();
if (result == null) throw new NotFoundException($"Entity {id} not found");

// UNI-001 CORRECT: Log and rethrow
try { return await GetDataAsync(); }
catch (Exception ex) { _logger.LogError(ex, "Failed"); throw; }

// UNI-003 CORRECT: Guard clauses flatten logic
if (user == null) return NotFound();
if (!user.IsActive) return Forbid();
if (!user.HasPermission("edit")) return Forbid();
// do work
```

---
description: "Universal code quality rules applied to all files. Enforces core principles that prevent the most common quality issues."
---

# Universal Code Quality Rules

These rules apply to **all code** regardless of language or framework. They represent the highest-value quality checks that prevent the most common defects.

| ID | Severity | Rule | Details |
|----|----------|------|---------|
| UNI-001 | **Critical** | No silent fallbacks | Never swallow exceptions, return defaults on failure, or use fallback operators (`??`, `||`, `or`, `.unwrap_or()`) to hide errors. Failures must be explicit — throw, log and rethrow, or return a meaningful error. |
| UNI-002 | **High** | Method length ≤ 20 lines | Methods over 20 executable lines (skip blanks, braces, comments) are a red flag for SRP/SoC violation. Extract and compose. |
| UNI-003 | **High** | No nested if statements | Use early returns (guard clauses), pattern matching, or match/switch expressions to flatten logic. If nesting seems unavoidable, the method is doing too much — extract. |
| UNI-004 | **Medium** | Constructor parameters ≤ 4 | More than 4 constructor parameters is a coupling smell. Consider splitting the class, introducing a parameter object, or re-evaluating responsibilities. |
| UNI-005 | **Medium** | High cohesion, low coupling | Every field in a class should be used by most methods. If methods cluster into groups using different field subsets, the class should be split. Prefer composition over inheritance. |

## Bad Examples

```
// UNI-001 VIOLATION: Silent fallback hides failure
result = tryGetFromCache() ?? getFromDatabase()

// UNI-001 VIOLATION: Swallowing exception
try {
    return getData()
} catch (error) {
    return defaultData()        // failure is invisible to caller
}

// UNI-003 VIOLATION: Nested conditionals
if (user != null) {
    if (user.isActive) {
        if (user.hasPermission("edit")) {
            // do work
        }
    }
}
```

## Good Examples

```
// UNI-001 CORRECT: Explicit failure
result = getFromDatabase()
if (result == null) throw NotFoundError("Entity " + id + " not found")

// UNI-001 CORRECT: Log and rethrow
try {
    return getData()
} catch (error) {
    logger.error("Failed to get data", error)
    throw                       // re-raise, don't swallow
}

// UNI-003 CORRECT: Guard clauses flatten logic
if (user == null) return NotFound()
if (!user.isActive) return Forbidden()
if (!user.hasPermission("edit")) return Forbidden()
// do work
```

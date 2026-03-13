---
description: "Code cleanliness rules for all authored code. Enforces method length, class cohesion, coupling limits, nesting elimination, and declarative style."
globs: <!-- ADAPT: e.g. "**/*.cs", "**/*.tsx", "**/*.ts" — set to your source file patterns -->
---

# Code Cleanliness Rules

| ID | Severity | Rule | Details |
|----|----------|------|---------|
| CC-001 | **High** | Method length ≤ 20 lines | Count executable lines only (skip blanks, braces, comments). Methods over 20 lines are a red flag for SRP/SoC violation. Suggest specific extraction points. |
| CC-002 | **High** | No nested if statements | Use guard clauses (early returns), pattern matching, switch expressions, or LINQ predicates. If nesting seems unavoidable, the method is doing too much — extract. |
| CC-003 | **High** | Class cohesion | Every field should be used by most methods. If methods cluster into groups using different field subsets, split the class. Large classes are coupling magnets. |
| CC-004 | **Medium** | Constructor parameters ≤ 4 | More than 3-4 constructor parameters indicates a class with too many responsibilities. Split, introduce parameter objects, or re-evaluate. |
| CC-005 | **Medium** | Anti-abstraction bias | Be skeptical of interfaces with single implementations, abstract base classes that add indirection, and deep inheritance hierarchies. Prefer declarative, functional-style code: LINQ, expression-bodied members, switch expressions, pattern matching, immutable records. |
| CC-006 | **Medium** | Prefer declarative style | Replace imperative loops with LINQ where clearer. Use expression-bodied members for simple methods. Prefer immutable records over mutable classes. Replace verbose conditionals with switch expressions. |
| CC-007 | **Low** | Components ≤ 200 lines (frontend) | React/frontend components over 200 lines likely need splitting into smaller, focused components with clear single responsibilities. |

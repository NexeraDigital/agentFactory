---
description: "Code cleanliness rules beyond universal basics. Enforces abstraction skepticism, declarative style, and frontend component size."
---

# Code Cleanliness Rules

Rules CC-001 through CC-003 complement the universal rules (UNI-001 through UNI-005) with additional style and structure guidance.

| ID | Severity | Rule | Details |
|----|----------|------|---------|
| CC-001 | **Medium** | Anti-abstraction bias | Be skeptical of interfaces with single implementations, abstract base classes that add indirection, and deep inheritance hierarchies. Prefer declarative, functional-style code: functional constructs (lambdas, comprehensions, pattern matching), immutable data types (records, dataclasses, readonly/frozen types). |
| CC-002 | **Medium** | Prefer declarative style | Replace imperative loops with declarative alternatives (`.map()`/`.filter()`, comprehensions, LINQ). Use concise function syntax where the language supports it. Prefer immutable data structures over mutable ones. Replace verbose conditionals with pattern matching or match/switch expressions. |
| CC-003 | **Low** | Components ≤ 200 lines (frontend) | React/frontend components over 200 lines likely need splitting into smaller, focused components with clear single responsibilities. |

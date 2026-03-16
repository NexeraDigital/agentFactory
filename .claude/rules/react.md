---
description: "React/TypeScript architecture rules. Enforces component patterns, state management, and frontend best practices."
paths: # ADAPT: adjust to your frontend file patterns
  - "**/*.tsx"
  - "**/*.jsx"
---

# React Architecture Rules

| ID | Severity | Rule | Details |
|----|----------|------|---------|
| RC-001 | **High** | No prop drilling beyond 2 levels | If props pass through more than 2 intermediate components, use composition, context, or a state management solution. |
| RC-002 | **High** | No god components | Components over 200 lines likely need splitting. Each component should have ONE clear reason to change. |
| RC-003 | **High** | No useEffect for derived state | Use `useMemo` for values computable from existing state/props. `useEffect` is for side effects (fetching, subscriptions, DOM manipulation), not derivation. |
| RC-004 | **High** | No state synchronization | If you're syncing state between two sources, the architecture is wrong. Pick one source of truth. Never duplicate server state in client stores. |
| RC-005 | **High** | No business logic in components | Extract business logic to custom hooks or utility functions. Components should handle rendering and user interaction, not domain calculations. |
| RC-006 | **Medium** | State proximity principle | State lives as close to where it's used as possible. Local UI → useState. Form → controlled/RHF. Server → custom hooks. URL → router params. Cross-component → context/store. |
| RC-007 | **Medium** | TypeScript strict — no `any` | Avoid `any` types. Use `unknown` for truly unknown types, discriminated unions for complex state, and proper generics. Document any unavoidable `any` with a comment explaining why. |
| RC-008 | **Medium** | Unidirectional data flow | Props down, callbacks up. Always. No bidirectional bindings or mutation of parent state from children. |

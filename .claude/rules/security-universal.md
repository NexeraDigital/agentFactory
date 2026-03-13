---
description: "Universal security rules applied to all files. Covers the highest-risk vulnerability patterns from OWASP Top 10."
---

# Security Rules (Universal)

These rules apply to **all code**. They catch the most dangerous vulnerability patterns that appear regardless of framework or layer.

| ID | Severity | Rule | Details |
|----|----------|------|---------|
| SEC-001 | **Critical** | No direct entity lookups without user scoping | Any query on user-owned entities must include a user identity predicate at the data access layer. Lookups by ID alone on user-owned data are BOLA/IDOR vulnerabilities. |
| SEC-002 | **Critical** | Never trust user-supplied identity | UserId must come from the authenticated session/token, never from request body, query string, or route parameters. |
| SEC-003 | **Critical** | User scoping on all data operations | Every create, read, update, delete, list, search, export, report, and download of user-owned data must enforce user scoping. |
| SEC-004 | **High** | No unauthenticated access to user data | Any endpoint touching user data must require authentication. Flag "auth optional" patterns where nullable user context leads to permissive defaults. |
| SEC-005 | **High** | Parameterized queries always | Never concatenate user input into SQL, NoSQL, or command strings. Use parameterized queries, ORM parameters, or prepared statements. |
| SEC-006 | **High** | No secrets in responses or logs | Never include tokens, passwords, connection strings, API keys, or internal IDs in API responses, URLs, or log entries. Sanitize error messages — no stack traces or SQL fragments in production responses. |

## Scary Patterns (Treat as High Severity Unless Clearly Safe)

1. Direct entity lookups by ID on user-owned entities without user scoping
2. Bypassing user scoping filters on user-owned entities
3. Any create/update path where UserId comes from request input
4. Any ID-based read/update/delete without user/owner check
5. Any list/search/export/report/download without user scoping
6. Any endpoint touching user data that is unauthenticated or weakly authorized

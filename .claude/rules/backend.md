---
description: "Backend code quality rules. Enforces SOLID principles, error handling patterns, and API design standards for server-side code."
# ADAPT: add paths: to scope to your backend file patterns (e.g. ["**/*.cs"])
---

# Backend Rules

These rules apply to all server-side code. They enforce architectural patterns and code quality standards that prevent the most common backend defects.

| ID | Severity | Rule | Details |
|----|----------|------|---------|
| BE-001 | **Critical** | No fallback logic | Avoid `??` with default values, `try/catch` returning defaults, and `TrySomethingSafely()` patterns. Failures must surface explicitly. See universal rule UNI-001 for examples. |
| BE-002 | **High** | Dependencies flow downward only | Controllers call services/managers. Services/managers call engines/repositories. Never call upward or sideways within the same layer. |
| BE-003 | **High** | Design interfaces first | For new features, define interfaces before implementations. Register in DI container. Depend on abstractions, not concretions. |
| BE-004 | **High** | Async/await consistency | Use async/await for all I/O operations. Never block on async code (.Result, .Wait()). Always pass CancellationToken through the call chain. |
| BE-005 | **Medium** | Validate at boundaries only | Validate inputs at API controllers and public methods. Internal code trusts validated data flowing through the pipeline. |
| BE-006 | **Medium** | Proper disposal patterns | Classes that own IDisposable resources must implement IDisposable/IAsyncDisposable. Use `using` statements or DI lifetime management. |
| BE-007 | **Medium** | Meaningful exception types | Use domain-specific exceptions (NotFoundException, ValidationException) rather than generic Exception. Include context in messages. |
| BE-008 | **Low** | Document public APIs | Public methods on service interfaces should have XML documentation comments describing parameters, return values, and exceptions thrown. |

## Forbidden Patterns

```csharp
// BE-001: Silent fallback
result ?? defaultValue;                        // VIOLATION
catch { return default; }                      // VIOLATION
catch (Exception) { /* ignore */ }             // VIOLATION
public T? TryGetSafely()                       // VIOLATION

// BE-004: Blocking on async
var result = GetDataAsync().Result;            // VIOLATION — deadlock risk
task.Wait();                                   // VIOLATION
```

## Required Patterns

```csharp
// BE-001: Explicit failure
var config = settings.Value ?? throw new InvalidOperationException("Settings not configured");

// BE-004: Proper async
var result = await GetDataAsync(cancellationToken);

// BE-007: Domain exceptions
throw new NotFoundException($"Order {orderId} not found");
```

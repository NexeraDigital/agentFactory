# IDesign Implementation Guide

Coding standards and patterns for implementing IDesign architecture.

---

## Project Structure

```
src/
├── Clients/                    # Presentation layer
│   ├── Web/
│   └── Api/
├── Managers/                   # Use-case orchestration
│   ├── OrderManager/
│   └── CustomerManager/
├── Engines/                    # Business rules (rare)
│   └── PricingEngine/
├── ResourceAccess/             # Data access
│   ├── OrderAccess/
│   └── CustomerAccess/
├── Contracts/                  # Shared data contracts
│   ├── DataContracts/
│   └── ServiceContracts/
└── Utilities/                  # Cross-cutting infrastructure
    ├── Logging/
    └── Configuration/
```

---

## Interface Design

### Service Contracts

Each component exposes a service contract (interface). Contracts live in a shared `Contracts` assembly.

```csharp
// Contracts/ServiceContracts/IOrderManager.cs
public interface IOrderManager
{
    OrderResult PlaceOrder(PlaceOrderRequest request);
    OrderResult GetOrderStatus(Guid orderId);
}
```

**Rules:**
- One interface per component
- Methods accept/return data contracts or primitives only
- No entity types in signatures
- No `IQueryable` or `IEnumerable<T>` leaking out

### Data Contracts

```csharp
// Contracts/DataContracts/OrderResult.cs
public class OrderResult
{
    public Guid OrderId { get; set; }
    public string Status { get; set; }
    public DateTime Created { get; set; }
}
```

**Rules:**
- POCOs only—no behavior
- No inheritance hierarchies
- Serialization-friendly (no circular references)
- Immutable where practical

---

## Layer Implementation Patterns

### Managers

Managers orchestrate workflows. They are thin—delegate to engines and resource access.

```csharp
public class OrderManager : IOrderManager
{
    private readonly IPricingEngine _pricingEngine;
    private readonly IOrderAccess _orderAccess;
    private readonly IInventoryAccess _inventoryAccess;

    public OrderManager(
        IPricingEngine pricingEngine,
        IOrderAccess orderAccess,
        IInventoryAccess inventoryAccess)
    {
        _pricingEngine = pricingEngine;
        _orderAccess = orderAccess;
        _inventoryAccess = inventoryAccess;
    }

    public OrderResult PlaceOrder(PlaceOrderRequest request)
    {
        // 1. Validate (may delegate to engine)
        // 2. Calculate pricing
        var price = _pricingEngine.CalculatePrice(request.Items);
        
        // 3. Reserve inventory
        _inventoryAccess.Reserve(request.Items);
        
        // 4. Persist order
        var orderId = _orderAccess.Create(request, price);
        
        // 5. Return result
        return new OrderResult { OrderId = orderId, Status = "Placed" };
    }
}
```

**Manager Rules:**
- No business logic—delegate to engines
- No direct data access—delegate to resource access
- Orchestrate sequence only
- Keep methods short (workflow steps visible at a glance)
- Near-expendable: if use cases change, manager changes

### Engines

Engines encapsulate reusable business rules. They are stateless.

```csharp
public class PricingEngine : IPricingEngine
{
    private readonly IDiscountAccess _discountAccess;

    public PricingEngine(IDiscountAccess discountAccess)
    {
        _discountAccess = discountAccess;
    }

    public PriceResult CalculatePrice(IEnumerable<LineItem> items)
    {
        var subtotal = items.Sum(i => i.Quantity * i.UnitPrice);
        var discount = _discountAccess.GetApplicableDiscount(items);
        var total = subtotal - discount;
        
        return new PriceResult
        {
            Subtotal = subtotal,
            Discount = discount,
            Total = total
        };
    }
}
```

**Engine Rules:**
- Stateless—no instance state between calls
- Pure business logic and rules
- May call resource access (for rule data)
- Do NOT call other engines
- Do NOT publish or subscribe to events
- Create only when truly reusable across managers

### Resource Access

Resource access encapsulates data operations with business verbs.

```csharp
public class OrderAccess : IOrderAccess
{
    private readonly DbContext _context;

    public OrderAccess(DbContext context)
    {
        _context = context;
    }

    // Business verb, not CRUD
    public Guid Create(PlaceOrderRequest request, PriceResult pricing)
    {
        var entity = new OrderEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Total = pricing.Total,
            Status = OrderStatus.Placed,
            Created = DateTime.UtcNow
        };

        _context.Orders.Add(entity);
        _context.SaveChanges();

        return entity.Id;
    }

    // Business verb
    public void MarkShipped(Guid orderId, string trackingNumber)
    {
        var order = _context.Orders.Find(orderId);
        order.Status = OrderStatus.Shipped;
        order.TrackingNumber = trackingNumber;
        order.ShippedAt = DateTime.UtcNow;
        _context.SaveChanges();
    }
}
```

**Resource Access Rules:**
- Business verbs, NOT generic CRUD (prefer `MarkShipped` over `Update`)
- Encapsulate entity/ORM details—callers see data contracts only
- May access multiple tables in one operation
- Do NOT call other resource access services
- Do NOT publish events
- Shareable across managers and engines

---

## Dependency Injection

### Registration Pattern

```csharp
public static class ServiceRegistration
{
    public static IServiceCollection AddManagers(this IServiceCollection services)
    {
        services.AddScoped<IOrderManager, OrderManager>();
        services.AddScoped<ICustomerManager, CustomerManager>();
        return services;
    }

    public static IServiceCollection AddEngines(this IServiceCollection services)
    {
        services.AddScoped<IPricingEngine, PricingEngine>();
        return services;
    }

    public static IServiceCollection AddResourceAccess(this IServiceCollection services)
    {
        services.AddScoped<IOrderAccess, OrderAccess>();
        services.AddScoped<ICustomerAccess, CustomerAccess>();
        services.AddScoped<IInventoryAccess, InventoryAccess>();
        return services;
    }
}
```

### Lifetime Guidelines

| Layer | Lifetime | Reason |
|-------|----------|--------|
| Manager | Scoped | Per-request workflow |
| Engine | Scoped or Singleton | Stateless; singleton if no scoped dependencies |
| Resource Access | Scoped | Tied to DbContext/unit of work |
| Utilities | Singleton | Shared infrastructure |

---

## Error Handling

### Fault Contracts

Define explicit fault types for expected errors.

```csharp
public class OrderFault
{
    public string Code { get; set; }
    public string Message { get; set; }
}

public class OrderResult
{
    public bool Success { get; set; }
    public Guid? OrderId { get; set; }
    public OrderFault Fault { get; set; }
}
```

### Exception Strategy

| Layer | Strategy |
|-------|----------|
| Resource Access | Catch data exceptions, throw typed faults or return failure |
| Engine | Let validation failures bubble as typed faults |
| Manager | Catch faults, translate to result contracts |
| Client | Receive result contracts, display appropriately |

**Rules:**
- Don't swallow exceptions silently
- Log at the boundary where you handle
- Don't expose stack traces to clients
- Use correlation IDs for tracing

---

## Validation

### Where to Validate

| Type | Location |
|------|----------|
| Input format (nulls, types) | Manager entry point |
| Business rules | Engine |
| Data integrity | Resource Access (constraints) |

```csharp
public class OrderManager : IOrderManager
{
    public OrderResult PlaceOrder(PlaceOrderRequest request)
    {
        // Input validation at entry
        if (request == null)
            return OrderResult.Fail("NULL_REQUEST", "Request required");
        
        if (request.Items == null || !request.Items.Any())
            return OrderResult.Fail("NO_ITEMS", "Order must have items");

        // Business validation delegated to engine
        var validation = _validationEngine.ValidateOrder(request);
        if (!validation.IsValid)
            return OrderResult.Fail(validation.Fault);

        // Proceed with workflow...
    }
}
```

---

## Testing Strategy

### Unit Tests by Layer

| Layer | Test Focus | Mocking |
|-------|------------|---------|
| Manager | Workflow orchestration | Mock engines and resource access |
| Engine | Business logic correctness | Mock resource access |
| Resource Access | Data operations | Use in-memory DB or repository pattern |

### Test Structure

```csharp
[TestClass]
public class OrderManagerTests
{
    private Mock<IPricingEngine> _pricingEngine;
    private Mock<IOrderAccess> _orderAccess;
    private OrderManager _manager;

    [TestInitialize]
    public void Setup()
    {
        _pricingEngine = new Mock<IPricingEngine>();
        _orderAccess = new Mock<IOrderAccess>();
        _manager = new OrderManager(_pricingEngine.Object, _orderAccess.Object);
    }

    [TestMethod]
    public void PlaceOrder_ValidRequest_CreatesOrder()
    {
        // Arrange
        var request = new PlaceOrderRequest { /* ... */ };
        _pricingEngine.Setup(x => x.CalculatePrice(It.IsAny<IEnumerable<LineItem>>()))
            .Returns(new PriceResult { Total = 100 });
        _orderAccess.Setup(x => x.Create(It.IsAny<PlaceOrderRequest>(), It.IsAny<PriceResult>()))
            .Returns(Guid.NewGuid());

        // Act
        var result = _manager.PlaceOrder(request);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.OrderId);
    }
}
```

### Integration Tests

- Test manager → engine → resource access flows
- Use real database (containerized)
- Focus on critical paths identified during design

---

## Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Manager interface | `I{Noun}Manager` | `IOrderManager` |
| Manager class | `{Noun}Manager` | `OrderManager` |
| Engine interface | `I{Verb}Engine` | `IPricingEngine` |
| Engine class | `{Verb}Engine` | `PricingEngine` |
| Resource Access interface | `I{Noun}Access` | `IOrderAccess` |
| Resource Access class | `{Noun}Access` | `OrderAccess` |
| Data contract | `{Noun}` or `{Noun}Result/Request` | `OrderResult` |
| Entity (internal) | `{Noun}Entity` | `OrderEntity` |

---

## Anti-Patterns to Avoid

| Anti-Pattern | Problem | Fix |
|--------------|---------|-----|
| Fat manager | Business logic in manager | Extract to engine |
| Anemic engine | Engine just passes through | Remove; call RA directly |
| CRUD resource access | Generic `Update(entity)` methods | Use business verbs |
| Leaking entities | Entities in service contracts | Map to data contracts |
| Cross-layer calls | Engine calling manager | Restructure; respect layers |
| Chatty interfaces | Many small calls per operation | Batch into meaningful operations |
| Shared mutable state | Static state in engines | Keep stateless; inject dependencies |

---

## Async Considerations

When using async:

```csharp
public interface IOrderManager
{
    Task<OrderResult> PlaceOrderAsync(PlaceOrderRequest request);
}

public class OrderManager : IOrderManager
{
    public async Task<OrderResult> PlaceOrderAsync(PlaceOrderRequest request)
    {
        var price = await _pricingEngine.CalculatePriceAsync(request.Items);
        await _inventoryAccess.ReserveAsync(request.Items);
        var orderId = await _orderAccess.CreateAsync(request, price);
        return new OrderResult { OrderId = orderId, Status = "Placed" };
    }
}
```

**Rules:**
- Async all the way down (avoid `.Result` or `.Wait()`)
- Suffix async methods with `Async`
- Use `ConfigureAwait(false)` in library code
- Consider cancellation tokens for long operations

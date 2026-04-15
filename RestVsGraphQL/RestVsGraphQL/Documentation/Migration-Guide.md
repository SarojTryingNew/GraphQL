# Migration Guide: REST to GraphQL

## Strategy: Incremental Coexistence

This guide outlines a **zero-downtime, low-risk migration** from REST to GraphQL based on the POC validation.

---

## Table of Contents

1. [Migration Patterns](#migration-patterns)
2. [Phase-by-Phase Plan](#phase-by-phase-plan)
3. [Technical Implementation](#technical-implementation)
4. [UI Migration](#ui-migration)
5. [Testing Strategy](#testing-strategy)
6. [Rollback Plan](#rollback-plan)
7. [Monitoring & Metrics](#monitoring--metrics)

---

## Migration Patterns

### Pattern 1: Side-by-Side Deployment ✅

**Architecture:**
```
┌─────────────────┐
│   UI Client     │
└────────┬────────┘
         │
    ┌────▼────────────────┐
    │  REST API   GraphQL │  ← Both available
    │  /api/*     /graphql│
    └────┬────────────┬───┘
         │            │
         ▼            ▼
    ┌────────────────────┐
    │    DataStore       │
    └────────────────────┘
```

**Pros:**
- ✅ Zero risk to existing functionality
- ✅ Gradual UI migration
- ✅ Easy rollback

**Cons:**
- ⚠️ Code duplication (mitigated by shared services)
- ⚠️ Two APIs to maintain temporarily

**When to Use:** Initial rollout, pilot testing

---

### Pattern 2: GraphQL Backend Layer ✅

**Architecture:**
```
┌─────────────────┐
│   UI Client     │
└────────┬────────┘
         │
    ┌────▼─────────────────────┐
    │  REST API                │  ← No UI changes needed
    │  (uses GraphQL internal) │
    └────┬─────────────────────┘
         │
    ┌────▼────────────────┐
    │  GraphQL Executor   │  ← Internal layer
    │  Service            │
    └────┬────────────────┘
         │
    ┌────▼────────────────┐
    │    DataStore        │
    └─────────────────────┘
```

**Pros:**
- ✅ No UI changes required
- ✅ Gradual backend consolidation
- ✅ Shared GraphQL schema benefits (DataLoaders, etc.)

**Cons:**
- ⚠️ Extra layer of indirection
- ⚠️ Not leveraging GraphQL's full potential yet

**When to Use:** Complex endpoints, gradual refactoring

**Implementation:**
```csharp
[ApiController]
[Route("api/graphql-backend/orders")]
public class OrdersGraphQLBackendController : ControllerBase
{
    private readonly GraphQLExecutorService _graphQLExecutor;

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _graphQLExecutor.GetOrderByIdAsync(id);
        return order == null ? NotFound() : Ok(order);
    }
}
```

---

### Pattern 3: Direct GraphQL Migration ✅

**Architecture:**
```
┌─────────────────┐
│   UI Client     │
│ (Apollo/urql)   │
└────────┬────────┘
         │
    ┌────▼─────────┐
    │  GraphQL API │
    └────┬─────────┘
         │
    ┌────▼────────────────┐
    │  Shared Services    │
    └────┬────────────────┘
         │
    ┌────▼────────────────┐
    │    DataStore        │
    └─────────────────────┘
```

**Pros:**
- ✅ Full GraphQL benefits (field selection, batching, single request)
- ✅ Better performance for complex queries
- ✅ Simplified backend (one API)

**Cons:**
- ⚠️ UI code changes required
- ⚠️ Team learning curve

**When to Use:** New features, modernized components

---

## Phase-by-Phase Plan

### Phase 0: Preparation (Week 1-2)

**Tasks:**
1. ☑️ Install HotChocolate NuGet package
2. ☑️ Set up GraphQL endpoint (`/graphql`)
3. ☑️ Configure Banana Cake Pop (GraphQL IDE)
4. ☑️ Create basic schema (Query, Mutation types)
5. ☑️ Deploy to development environment

**Deliverables:**
- GraphQL endpoint accessible alongside REST
- Basic "Hello World" query working
- Team can access GraphQL IDE

**Validation:**
```graphql
query {
  customers {
    id
    name
  }
}
```

**Effort:** 8-16 hours (1 developer)

---

### Phase 1: Core Schema & DataLoaders (Week 3-6)

**Tasks:**
1. ☑️ Define GraphQL types for core entities (Customer, Order, Product)
2. ☑️ Implement DataLoaders for all relations
3. ☑️ Create shared service layer (OrderService, CustomerService)
4. ☑️ Implement read-only queries
5. ☑️ Set up YAML-based testing

**Deliverables:**
- Complete GraphQL schema for queries
- All N+1 queries eliminated via DataLoaders
- Side-by-side REST vs GraphQL tests passing

**Validation:**
```yaml
# TestSuites/validation-tests.yaml
- name: "GraphQL - Nested Order Query"
  type: "GraphQL"
  query: |
    {
      order(id: 1) {
        customer { name }
        items { product { category { name } } }
      }
    }
  assertions:
    statusCode: 200
```

**Effort:** 60-80 hours (2 developers)

---

### Phase 2: Mutations & Bulk Operations (Week 7-10)

**Tasks:**
1. ☑️ Implement create/update/delete mutations
2. ☑️ Implement bulk operations (create, update, delete)
3. ☑️ Share business logic with REST (OrderService)
4. ☑️ Add validation and error handling
5. ☑️ Performance testing

**Deliverables:**
- Full CRUD operations via GraphQL
- Bulk operations with partial success support
- Performance benchmarks (REST vs GraphQL)

**Validation:**
```graphql
mutation {
  bulkCreateOrders(request: {
    orders: [...]
  }) {
    successCount
    failureCount
    createdIds
    errors
  }
}
```

**Effort:** 60-80 hours (2 developers)

---

### Phase 3: Hybrid REST-GraphQL Layer (Week 11-14)

**Tasks:**
1. ☑️ Implement GraphQLExecutorService
2. ☑️ Create hybrid REST controllers
3. ☑️ Migrate 5-10 complex REST endpoints
4. ☑️ Regression testing

**Deliverables:**
- REST endpoints internally using GraphQL
- No UI changes required
- All existing tests still passing

**Example:**
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<Order>> GetOrder(int id)
{
    // REST endpoint internally calls GraphQL
    var order = await _graphQLExecutor.GetOrderByIdAsync(id);
    return order == null ? NotFound() : Ok(order);
}
```

**Effort:** 40-60 hours (2 developers)

---

### Phase 4: UI Client Setup (Week 15-18)

**Tasks:**
1. Install GraphQL client (Apollo Client / urql)
2. Configure GraphQL code generation
3. Create sample components using GraphQL
4. Pilot migration of 2-3 simple views

**Deliverables:**
- GraphQL client integrated in UI
- TypeScript types auto-generated from schema
- 2-3 components successfully migrated

**Angular Example:**
```typescript
// Install Apollo Client
npm install @apollo/client graphql

// Configure Apollo
import { ApolloClient, InMemoryCache } from '@apollo/client';

const client = new ApolloClient({
  uri: 'http://localhost:5000/graphql',
  cache: new InMemoryCache()
});
```

**React Example:**
```typescript
// Install urql
npm install urql graphql

// Configure urql
import { createClient, Provider } from 'urql';

const client = createClient({
  url: 'http://localhost:5000/graphql'
});
```

**Effort:** 40-60 hours (2 frontend developers)

---

### Phase 5: Gradual UI Migration (Week 19-40)

**Strategy:** Migrate incrementally by feature area

**Priority Order:**
1. **New Features** (write directly in GraphQL)
2. **Complex Multi-Query Views** (dashboard, reports)
3. **Frequently Accessed Pages** (performance benefit)
4. **Low-Traffic Pages** (lower risk)
5. **Legacy Views** (migrate when touched)

**Week 19-24: Pilot (20% of UI)**
- Dashboard views
- Product catalog
- Customer details

**Week 25-32: Main Migration (50% of UI)**
- Order management
- Inventory management
- Search/filtering

**Week 33-40: Completion (30% of UI)**
- Reports
- Settings pages
- Admin panels

**Example Migration (Before/After):**

**Before (REST):**
```typescript
// Multiple API calls
const customer = await fetch(`/api/customers/${id}`);
const orders = await fetch(`/api/customers/${id}/orders`);
const products = await fetch(`/api/products`);
```

**After (GraphQL):**
```typescript
// Single GraphQL query
const { data } = await client.query({
  query: gql`
    query GetCustomerData($id: Int!) {
      customer(id: $id) {
        id
        name
        email
        orders {
          id
          orderDate
          totalAmount
        }
      }
      products {
        id
        name
        price
      }
    }
  `,
  variables: { id }
});
```

**Effort:** 240-360 hours (3 frontend developers)

---

### Phase 6: Optimization & Cleanup (Week 41-48)

**Tasks:**
1. Remove unused REST endpoints
2. Performance tuning (caching, query optimization)
3. Documentation update
4. External API client support decision

**Deliverables:**
- Deprecated REST endpoints documented
- Performance optimization report
- Updated API documentation

**Decision Point: External API Clients**

**Option A: Keep Both APIs**
- REST for external clients (backward compatibility)
- GraphQL for internal UI
- Minimal additional effort

**Option B: Migrate External Clients**
- Provide migration guide
- Deprecation timeline (6-12 months)
- Support both during transition

**Effort:** 40-80 hours (team)

---

## Technical Implementation

### Shared Service Layer

**Key Principle:** Business logic shared between REST and GraphQL

```csharp
// Services/OrderService.cs
public class OrderService
{
    private readonly DataStore _dataStore;

    public BulkOperationResult BulkCreateOrders(BulkOrderCreateRequest request)
    {
        // Shared logic used by BOTH REST and GraphQL
        var result = new BulkOperationResult();
        
        foreach (var orderDto in request.Orders)
        {
            try
            {
                var order = CreateOrder(orderDto);
                result.SuccessCount++;
                result.CreatedIds.Add(order.Id);
            }
            catch (Exception ex)
            {
                result.FailureCount++;
                result.Errors.Add(ex.Message);
            }
        }
        
        return result;
    }
}
```

**REST Controller:**
```csharp
[HttpPost("bulk")]
public ActionResult<BulkOperationResult> BulkCreate(
    [FromBody] BulkOrderCreateRequest request)
{
    return Ok(_orderService.BulkCreateOrders(request));
}
```

**GraphQL Mutation:**
```csharp
public BulkOperationResult BulkCreateOrders(
    BulkOrderCreateRequest request,
    [Service] OrderService orderService)
{
    return orderService.BulkCreateOrders(request);
}
```

---

### GraphQL Executor Service (Hybrid Pattern)

```csharp
// Services/GraphQLExecutorService.cs
public class GraphQLExecutorService
{
    private readonly IRequestExecutorResolver _executorResolver;

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        var executor = await _executorResolver.GetRequestExecutorAsync();

        var query = $@"
            query {{
                order(id: {id}) {{
                    id
                    orderDate
                    totalAmount
                    customer {{ id name email }}
                    items {{
                        id
                        quantity
                        product {{ id name price }}
                    }}
                }}
            }}";

        var result = await executor.ExecuteAsync(query);
        var resultData = await GetResultDataAsync<JsonElement>(result);

        if (resultData.TryGetProperty("order", out var orderElement))
        {
            return JsonSerializer.Deserialize<Order>(orderElement.GetRawText());
        }

        return null;
    }
}
```

**Benefits:**
- REST endpoint returns same data structure
- UI doesn't need changes
- Backend benefits from GraphQL (DataLoaders, etc.)

---

## UI Migration

### Apollo Client Setup (Angular)

```typescript
// app.config.ts
import { Apollo, APOLLO_OPTIONS } from 'apollo-angular';
import { HttpLink } from 'apollo-angular/http';
import { InMemoryCache } from '@apollo/client/core';

export const graphqlProvider: ApplicationConfig['providers'] = [
  Apollo,
  {
    provide: APOLLO_OPTIONS,
    useFactory: (httpLink: HttpLink) => ({
      cache: new InMemoryCache(),
      link: httpLink.create({
        uri: 'http://localhost:5000/graphql',
      }),
    }),
    deps: [HttpLink],
  },
];
```

### GraphQL Code Generation

```bash
# Install GraphQL Code Generator
npm install --save-dev @graphql-codegen/cli @graphql-codegen/typescript @graphql-codegen/typescript-operations @graphql-codegen/typescript-apollo-angular

# Create codegen.yml
schema: http://localhost:5000/graphql
documents: './src/**/*.graphql'
generates:
  ./src/generated/graphql.ts:
    plugins:
      - typescript
      - typescript-operations
      - typescript-apollo-angular

# Generate types
npx graphql-codegen
```

### Example Component Migration

**Before (REST):**
```typescript
// customer-detail.component.ts
export class CustomerDetailComponent implements OnInit {
  customer: Customer;
  orders: Order[];

  async ngOnInit() {
    this.customer = await this.http.get<Customer>(
      `/api/customers/${this.customerId}`
    ).toPromise();
    
    this.orders = await this.http.get<Order[]>(
      `/api/customers/${this.customerId}/orders`
    ).toPromise();
  }
}
```

**After (GraphQL):**
```typescript
// customer-detail.component.ts
import { Apollo, gql } from 'apollo-angular';

const GET_CUSTOMER = gql`
  query GetCustomer($id: Int!) {
    customer(id: $id) {
      id
      name
      email
      phone
      orders {
        id
        orderDate
        totalAmount
        status
      }
    }
  }
`;

export class CustomerDetailComponent implements OnInit {
  customer: Customer;

  constructor(private apollo: Apollo) {}

  ngOnInit() {
    this.apollo
      .watchQuery<{ customer: Customer }>({
        query: GET_CUSTOMER,
        variables: { id: this.customerId }
      })
      .valueChanges.subscribe(({ data }) => {
        this.customer = data.customer;
      });
  }
}
```

**Benefits:**
- 1 request instead of 2
- Type-safe (with code generation)
- Automatic updates (watchQuery)

---

## Testing Strategy

### Side-by-Side YAML Tests

```yaml
# TestSuites/migration-validation.yaml
name: "Migration Validation Tests"

tests:
  # Verify identical behavior
  - name: "REST - Get Order"
    type: "REST"
    method: "GET"
    endpoint: "/api/orders/1"
    assertions:
      statusCode: 200
      contains: "totalAmount"

  - name: "GraphQL - Get Order"
    type: "GraphQL"
    query: |
      {
        order(id: 1) {
          id
          totalAmount
        }
      }
    assertions:
      statusCode: 200
      contains: "totalAmount"

  # Verify bulk operations
  - name: "REST - Bulk Create"
    type: "REST"
    method: "POST"
    endpoint: "/api/orders/bulk"
    body: { orders: [...] }
    assertions:
      statusCode: 200

  - name: "GraphQL - Bulk Create"
    type: "GraphQL"
    query: "mutation { bulkCreateOrders(...) { successCount } }"
    assertions:
      statusCode: 200
```

### Run Tests

```bash
dotnet run --project TestRunner
```

### Regression Testing

After each migration phase:
1. ✅ Run full YAML test suite
2. ✅ Compare REST vs GraphQL responses
3. ✅ Performance benchmarks
4. ✅ UI smoke tests

---

## Rollback Plan

### Rollback Triggers

Initiate rollback if:
- ❌ >5% increase in error rate
- ❌ >20% performance degradation
- ❌ Critical bug blocking production

### Rollback Steps

**Phase 1-3 (Backend Only):**
1. Disable GraphQL endpoint (`app.MapGraphQL()` commented out)
2. No UI changes needed
3. Zero downtime

**Phase 4-5 (UI Migration):**
1. Revert UI deployment to previous version
2. GraphQL endpoint remains (no impact)
3. Feature flags to disable GraphQL in UI

### Feature Flags

```typescript
// Feature flag in UI
const USE_GRAPHQL = environment.featureFlags.graphql;

if (USE_GRAPHQL) {
  // GraphQL query
} else {
  // REST API call
}
```

---

## Monitoring & Metrics

### Key Metrics to Track

**Performance:**
- ⏱️ Average response time (REST vs GraphQL)
- 📊 Payload size comparison
- 🔢 Number of API calls per page load

**Reliability:**
- ❌ Error rate
- ⚠️ 4xx/5xx responses
- 🔄 Retry rate

**Adoption:**
- 📈 GraphQL query count vs REST
- 👥 % of UI using GraphQL
- 🆕 New features using GraphQL

### Monitoring Implementation

```csharp
// Middleware/MetricsMiddleware.cs
public class MetricsMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        await _next(context);
        
        stopwatch.Stop();
        
        _metricsCollector.RecordRequest(new RequestMetric
        {
            Path = context.Request.Path,
            Method = context.Request.Method,
            StatusCode = context.Response.StatusCode,
            Duration = stopwatch.ElapsedMilliseconds,
            ResponseSize = context.Response.ContentLength ?? 0
        });
    }
}
```

### Dashboard Queries

```graphql
query GetMetrics {
  metrics {
    restApiCalls
    graphqlApiCalls
    avgRestResponseTime
    avgGraphqlResponseTime
    errorRate
  }
}
```

---

## Success Criteria

### Phase 1-2 Success
- ✅ GraphQL endpoint available
- ✅ All queries/mutations functional
- ✅ DataLoaders eliminate N+1
- ✅ Shared service layer working
- ✅ Tests passing (REST & GraphQL)

### Phase 3 Success
- ✅ 5-10 REST endpoints using GraphQL backend
- ✅ No UI changes required
- ✅ Performance equal or better

### Phase 4-5 Success
- ✅ 80%+ UI components using GraphQL
- ✅ <5% error rate increase
- ✅ 20-40% reduction in API calls
- ✅ Developer satisfaction (survey)

### Phase 6 Success
- ✅ Deprecated REST endpoints documented
- ✅ External client migration plan
- ✅ Performance optimized
- ✅ Documentation complete

---

## Risk Mitigation

### Risk 1: Performance Regression

**Mitigation:**
- Comprehensive benchmarking
- DataLoaders for all relations
- Query complexity limits
- Caching strategy

### Risk 2: Learning Curve

**Mitigation:**
- Training sessions (2-day workshop)
- Documentation (this guide + examples)
- Pair programming during migration
- GraphQL champions in each team

### Risk 3: Breaking Changes

**Mitigation:**
- Schema evolution guidelines
- Never remove fields (deprecate)
- Versioning strategy (if needed)
- Side-by-side deployment

### Risk 4: External API Clients

**Mitigation:**
- Keep REST API for external clients
- 12-month deprecation notice
- Migration guide for partners
- Support during transition

---

## Timeline Summary

| Phase | Duration | Team | Effort | Status |
|-------|----------|------|--------|--------|
| 0: Preparation | 1-2 weeks | 1 dev | 8-16h | ✅ POC Complete |
| 1: Core Schema | 3-6 weeks | 2 devs | 60-80h | Ready to Start |
| 2: Mutations | 7-10 weeks | 2 devs | 60-80h | Planned |
| 3: Hybrid Layer | 11-14 weeks | 2 devs | 40-60h | Planned |
| 4: UI Setup | 15-18 weeks | 2 FE devs | 40-60h | Planned |
| 5: UI Migration | 19-40 weeks | 3 FE devs | 240-360h | Planned |
| 6: Optimization | 41-48 weeks | Team | 40-80h | Planned |
| **TOTAL** | **~11 months** | **Team** | **500-736h** | **In Progress** |

**Critical Path:** UI Migration (longest phase)

**Recommended Approach:** Start UI migration earlier (overlap with Phase 3)

---

## Conclusion

This migration strategy has been **validated by the POC** and provides:

✅ **Zero Downtime**: All phases can be deployed incrementally  
✅ **Low Risk**: Rollback possible at any stage  
✅ **Proven Patterns**: Three coexistence patterns demonstrated  
✅ **Clear Timeline**: 11-month gradual migration  
✅ **Measurable Success**: Comprehensive metrics and validation  

**Recommended Next Step:** Proceed to Phase 1 (Core Schema & DataLoaders)

---

**Related Documents:**
- [README.md](./README.md) - POC Overview & Results
- [How-to-GraphQL.md](./How-to-GraphQL.md) - Implementation Guide
- [GraphQL-Query-Model.md](./GraphQL-Query-Model.md) - Schema Reference

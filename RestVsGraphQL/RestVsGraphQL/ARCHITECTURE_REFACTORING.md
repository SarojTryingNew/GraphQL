# Architecture Refactoring Summary

## Overview
The application has been refactored to implement the following architecture:

```
Angular Client (Frontend)
    ↓ (parallel calls)
    ├─→ REST API Direct (/api/orders/*)
    │       ↓
    │   DataStore (in-memory database)
    │
    └─→ REST API with GraphQL Backend (/api/graphql-backend/orders/*)
            ↓
        GraphQL (internal layer)
            ↓
        DataStore (in-memory database)
```

## Key Changes

### 1. Backend (.NET 9 API)

#### New Services
- **`GraphQLExecutorService.cs`**: Service that executes GraphQL queries and mutations internally (in-process, not via HTTP). This allows REST APIs to use GraphQL as an internal data access layer.

#### New Controllers
- **`OrdersGraphQLBackendController.cs`**: New REST API controller at route `/api/graphql-backend/orders/*` that uses `GraphQLExecutorService` to communicate with GraphQL internally.
- **`OrdersController.cs`** (existing): Continues to work at route `/api/orders/*` accessing DataStore directly.

#### Updated Metrics System
- **`ApiType` enum** - Extended with new values:
  - `RESTDirect` - REST API calling DataStore directly
  - `RESTWithGraphQL` - REST API using GraphQL as internal layer
  
- **`MetricsMiddleware.cs`** - Updated to detect and categorize requests:
  - `/api/orders/*` → `RESTDirect`
  - `/api/graphql-backend/orders/*` → `RESTWithGraphQL`
  - `/graphql` → `GraphQL`

- **`MetricsCollector.cs`** - Updated to track and compare both REST approaches:
  - Added `RESTDirectMetrics` and `RESTWithGraphQLMetrics` properties to `ComparisonReport`
  - Updated `GetComparisonReport()` to compare the two REST approaches

#### Program.cs
- Registered `GraphQLExecutorService` as a singleton service

### 2. Frontend (Angular Client)

#### New Service
- **`rest-graphql-backend.service.ts`**: New Angular service that calls the REST API with GraphQL backend (`/api/graphql-backend/orders/*`)

#### Existing Services (Unchanged)
- **`rest.service.ts`**: Calls REST API directly (`/api/orders/*`)
- **`graphql.service.ts`**: Calls GraphQL endpoint directly (`/graphql`)

#### Usage in Components (Next Step)
Components should inject both `RestService` and `RestGraphQLBackendService` and call them in parallel to compare performance.

### 3. Performance Tests (PowerShell Scripts)

#### Updated Test Script
- **`load-test-bulk-create.ps1`**: Updated to test both approaches:
  - Test 1: REST Direct (`/api/orders/bulk`)
  - Test 2: REST with GraphQL Backend (`/api/graphql-backend/orders/bulk`)
  - Updated metrics display to show comparison between both approaches

#### Other Test Scripts (To Be Updated)
The following scripts should be updated similarly:
- `load-test-bulk-update.ps1`
- `load-test-bulk-delete.ps1`
- `load-test-bulk-get.ps1`
- `load-test.ps1`
- `load-test-nested.ps1`
- `load-test-dashboard.ps1`
- `load-test-multiple.ps1`

## API Endpoints

### REST Direct (DataStore)
- `GET /api/orders` - Get all orders
- `GET /api/orders/{id}` - Get order by ID
- `GET /api/orders/bulk?ids=1,2,3` - Get orders by IDs
- `POST /api/orders/bulk` - Bulk create orders
- `PUT /api/orders/bulk` - Bulk update orders
- `DELETE /api/orders/bulk` - Bulk delete orders

### REST with GraphQL Backend
- `GET /api/graphql-backend/orders` - Get all orders
- `GET /api/graphql-backend/orders/{id}` - Get order by ID
- `GET /api/graphql-backend/orders/bulk?ids=1,2,3` - Get orders by IDs
- `POST /api/graphql-backend/orders/bulk` - Bulk create orders
- `PUT /api/graphql-backend/orders/bulk` - Bulk update orders
- `DELETE /api/graphql-backend/orders/bulk` - Bulk delete orders

### GraphQL (Internal/Optional Direct Access)
- `POST /graphql` - GraphQL endpoint (kept for backward compatibility)

## Metrics and Comparison

The metrics system now tracks three types of API calls:
1. **RESTDirect** - Direct REST API calls to DataStore
2. **RESTWithGraphQL** - REST API calls that use GraphQL internally
3. **GraphQL** - Direct GraphQL calls (legacy/optional)

### Comparison Report
Access at: `http://localhost:5072/api/metrics/report`

The report shows:
- **Winners**: Which approach performed better for:
  - Response Time
  - Payload Size
  - Throughput
  - Reliability
  - Memory Efficiency

- **Improvements**: Percentage improvements between approaches

- **Detailed Metrics** for both approaches:
  - Total Requests
  - Success Rate
  - Average/P95 Response Times
  - Average Payload Size
  - Throughput (req/s)

## Next Steps

### For Angular Frontend:
1. Update components to inject both `RestService` and `RestGraphQLBackendService`
2. Implement parallel calls to both services
3. Display side-by-side metrics comparison in the UI

### For PowerShell Tests:
1. Update remaining test scripts to test both REST approaches
2. Ensure all tests reset metrics before running
3. Verify metrics display correctly compares both approaches

### For Metrics Page:
1. Update the HTML metrics report to display three-way comparison
2. Add charts showing performance differences
3. Highlight which approach wins for each scenario

## Benefits of This Architecture

1. **Fair Comparison**: Both REST approaches use the same underlying business logic and data store
2. **GraphQL as Internal Layer**: Demonstrates using GraphQL for internal data access while maintaining REST interface
3. **Performance Insights**: Shows overhead of GraphQL abstraction layer vs direct data access
4. **Flexibility**: Clients can choose between approaches based on their needs
5. **Educational**: Clearly demonstrates architectural patterns and trade-offs

## Testing the Changes

1. **Start the API**: `dotnet run` in RestVsGraphQL directory
2. **Run a test**: `.\launch-tests.ps1` and select option 3 (Bulk CREATE Operations)
3. **View metrics**: Navigate to `http://localhost:5072/api/metrics/report`
4. **Compare**: Check the KPI comparison between REST Direct and REST with GraphQL

The metrics will show how adding GraphQL as an internal layer affects:
- Response times (likely slightly slower due to additional layer)
- Payload size (should be similar)
- Throughput (may be slightly lower)
- Memory usage (may be slightly higher due to GraphQL execution overhead)

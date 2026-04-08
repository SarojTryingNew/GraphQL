# Visual Guide: Request/Response Examples Page

## Page URL
```
http://localhost:5072/api/metrics/examples
```

## Page Layout

```
╔═══════════════════════════════════════════════════════════════════════╗
║                                                                       ║
║  🔍 Request/Response Examples                                        ║
║  ═══════════════════════════════════════════════════════════════     ║
║                                                                       ║
║  📋 About These Examples:                                            ║
║  This page shows actual HTTP requests and responses captured         ║
║  during your performance tests. The system captures only the         ║
║  first 2 requests per endpoint to minimize performance impact.       ║
║                                                                       ║
║  ─────────────────────────────────────────────────────────────────   ║
║                                                                       ║
║  📍 Customers                                                         ║
║  ╔════════════════════════════════╦════════════════════════════════╗ ║
║  ║  🔵 REST API                   ║  🔴 GraphQL API               ║ ║
║  ║                                ║                               ║ ║
║  ║  GET /api/customers/1 [200]    ║  POST /graphql [200]          ║ ║
║  ║                                ║                               ║ ║
║  ║  📤 Request                    ║  📤 Request (GraphQL Query)   ║ ║
║  ║  ┌──────────────────────────┐ ║  ┌──────────────────────────┐ ║ ║
║  ║  │ No request body          │ ║  │ {                        │ ║ ║
║  ║  │ (GET request)            │ ║  │   "query": "query {      │ ║ ║
║  ║  │                          │ ║  │     customer(id: \"1\") { │ ║ ║
║  ║  │                          │ ║  │       id                 │ ║ ║
║  ║  │                          │ ║  │       name               │ ║ ║
║  ║  └──────────────────────────┘ ║  │       email              │ ║ ║
║  ║                                ║  │     }                    │ ║ ║
║  ║  📥 Response                   ║  │   }"                     │ ║ ║
║  ║  ┌──────────────────────────┐ ║  └──────────────────────────┘ ║ ║
║  ║  │ {                        │ ║                               ║ ║
║  ║  │   "id": 1,               │ ║  📥 Response                  ║ ║
║  ║  │   "name": "John Doe",    │ ║  ┌──────────────────────────┐ ║ ║
║  ║  │   "email": "john@...",   │ ║  │ {                        │ ║ ║
║  ║  │   "phone": "555-0100"    │ ║  │   "data": {              │ ║ ║
║  ║  │ }                        │ ║  │     "customer": {        │ ║ ║
║  ║  └──────────────────────────┘ ║  │       "id": "1",         │ ║ ║
║  ║  Size: 245 B | Time: 12.5ms  ║  │       "name": "John Doe",│ ║ ║
║  ║                                ║  │       "email": "john@.." │ ║ ║
║  ╠════════════════════════════════╬═══════════════════════════════╣ ║
║  ║  GET /api/customers/2 [200]    ║  POST /graphql [200]          ║ ║
║  ║                                ║                               ║ ║
║  ║  📤 Request                    ║  📤 Request (GraphQL Query)   ║ ║
║  ║  No request body (GET)         ║  { "query": "..." }           ║ ║
║  ║                                ║                               ║ ║
║  ║  📥 Response                   ║  📥 Response                  ║ ║
║  ║  { ... customer 2 data ... }   ║  { "data": { ... } }          ║ ║
║  ╚════════════════════════════════╩════════════════════════════════╝ ║
║                                                                       ║
║  ─────────────────────────────────────────────────────────────────   ║
║                                                                       ║
║  📍 Orders                                                            ║
║  ╔════════════════════════════════╦════════════════════════════════╗ ║
║  ║  🔵 REST API                   ║  🔴 GraphQL API               ║ ║
║  ║                                ║                               ║ ║
║  ║  GET /api/orders/1 [200]       ║  POST /graphql [200]          ║ ║
║  ║                                ║                               ║ ║
║  ║  📤 Request                    ║  📤 Request (GraphQL Query)   ║ ║
║  ║  No request body (GET)         ║  ┌──────────────────────────┐ ║ ║
║  ║                                ║  │ {                        │ ║ ║
║  ║  📥 Response                   ║  │   "query": "query {      │ ║ ║
║  ║  ┌──────────────────────────┐ ║  │     order(id: \"1\") {    │ ║ ║
║  ║  │ {                        │ ║  │       id                 │ ║ ║
║  ║  │   "id": 1,               │ ║  │       customerId         │ ║ ║
║  ║  │   "customerId": 5,       │ ║  │       items {            │ ║ ║
║  ║  │   "total": 299.99,       │ ║  │         id               │ ║ ║
║  ║  │   "items": [             │ ║  │         productId        │ ║ ║
║  ║  │     {                    │ ║  │         quantity         │ ║ ║
║  ║  │       "id": 101,         │ ║  │       }                  │ ║ ║
║  ║  │       "productId": 5,    │ ║  │     }                    │ ║ ║
║  ║  │       "quantity": 2      │ ║  │   }"                     │ ║ ║
║  ║  │     }                    │ ║  └──────────────────────────┘ ║ ║
║  ║  │   ]                      │ ║                               ║ ║
║  ║  │ }                        │ ║  📥 Response                  ║ ║
║  ║  └──────────────────────────┘ ║  ┌──────────────────────────┐ ║ ║
║  ║  Size: 1.2 KB | Time: 45.3ms ║  │ {                        │ ║ ║
║  ║                                ║  │   "data": {              │ ║ ║
║  ║                                ║  │     "order": {           │ ║ ║
║  ║                                ║  │       "id": "1",         │ ║ ║
║  ║                                ║  │       "customerId": "5", │ ║ ║
║  ║                                ║  │       "items": [...]     │ ║ ║
║  ║                                ║  │     }                    │ ║ ║
║  ║                                ║  │   }                      │ ║ ║
║  ║                                ║  │ }                        │ ║ ║
║  ║                                ║  └──────────────────────────┘ ║ ║
║  ║                                ║  Size: 987 B | Time: 38.1ms   ║ ║
║  ╚════════════════════════════════╩════════════════════════════════╝ ║
║                                                                       ║
║  ─────────────────────────────────────────────────────────────────   ║
║                                                                       ║
║  📍 Bulk Operations                                                   ║
║  ╔════════════════════════════════╦════════════════════════════════╗ ║
║  ║  🔵 REST API                   ║  🔴 GraphQL API               ║ ║
║  ║                                ║                               ║ ║
║  ║  POST /api/orders/bulk [200]   ║  POST /graphql [200]          ║ ║
║  ║                                ║                               ║ ║
║  ║  📤 Request                    ║  📤 Request (Mutation)        ║ ║
║  ║  ┌──────────────────────────┐ ║  ┌──────────────────────────┐ ║ ║
║  ║  │ {                        │ ║  │ {                        │ ║ ║
║  ║  │   "orders": [            │ ║  │   "query": "mutation {   │ ║ ║
║  ║  │     {                    │ ║  │     createOrders(        │ ║ ║
║  ║  │       "customerId": 1,   │ ║  │       input: [           │ ║ ║
║  ║  │       "items": [...]     │ ║  │         {                │ ║ ║
║  ║  │     },                   │ ║  │           customerId: 1  │ ║ ║
║  ║  │     { ... }              │ ║  │           items: [...]   │ ║ ║
║  ║  │   ]                      │ ║  │         }                │ ║ ║
║  ║  │ }                        │ ║  │       ]                  │ ║ ║
║  ║  └──────────────────────────┘ ║  │     ) { ... }            │ ║ ║
║  ║                                ║  │   }"                     │ ║ ║
║  ║  📥 Response                   ║  └──────────────────────────┘ ║ ║
║  ║  ┌──────────────────────────┐ ║                               ║ ║
║  ║  │ {                        │ ║  📥 Response                  ║ ║
║  ║  │   "created": 10,         │ ║  ┌──────────────────────────┐ ║ ║
║  ║  │   "orderIds": [1,2,...]  │ ║  │ {                        │ ║ ║
║  ║  │ }                        │ ║  │   "data": {              │ ║ ║
║  ║  └──────────────────────────┘ ║  │     "createOrders": {...}│ ║ ║
║  ║  Size: 2.5 KB | Time: 125ms  ║  │   }                      │ ║ ║
║  ║                                ║  │ }                        │ ║ ║
║  ║                                ║  └──────────────────────────┘ ║ ║
║  ║                                ║  Size: 2.1 KB | Time: 98ms    ║ ║
║  ╚════════════════════════════════╩════════════════════════════════╝ ║
║                                                                       ║
║  ─────────────────────────────────────────────────────────────────   ║
║                                                                       ║
║  ← Back to Performance Report                                        ║
║                                                                       ║
╚═══════════════════════════════════════════════════════════════════════╝
```

## Key Visual Features

### Color Coding
- **🔵 REST API** - Red border, light red background
- **🔴 GraphQL API** - Pink border, light pink background
- **Code Blocks** - Dark theme with light text
- **Status 200** - Green badge
- **Status 4xx/5xx** - Red badge

### Layout
- **Two-column grid** - Side-by-side comparison
- **Responsive** - Adapts to screen size
- **Scrollable code** - Long JSON can be scrolled
- **Grouped by operation** - Related endpoints together

### Information Density
- **Endpoint path** - Full URL shown
- **HTTP method** - GET, POST, etc.
- **Status code** - Color-coded badge
- **Request body** - Pretty-printed JSON
- **Response body** - Pretty-printed JSON
- **Metadata** - Size and time below each response

## Real-World Example

After running Quick Test, you might see:

### Customers Section
```
REST Side:
- GET /api/customers/1 → { id: 1, name: "John Doe", ... }
- GET /api/customers/2 → { id: 2, name: "Jane Smith", ... }

GraphQL Side:
- POST /graphql → query { customer(id: "1") { ... } }
- POST /graphql → query { customer(id: "2") { ... } }
```

### Orders Section
```
REST Side:
- GET /api/orders/1 → { id: 1, items: [...], total: 299.99 }
- GET /api/orders/2 → { id: 2, items: [...], total: 149.99 }

GraphQL Side:
- POST /graphql → query { order(id: "1") { id items { ... } } }
- POST /graphql → query { order(id: "2") { id items { ... } } }
```

### Bulk Operations Section
```
REST Side:
- POST /api/orders/bulk → { orders: [10 orders] }

GraphQL Side:
- POST /graphql → mutation { createOrders(input: [...]) { ... } }
```

## Navigation

### From Main Report:
1. Open metrics report (auto-opens after test)
2. Look at the top section
3. Click: **"📋 View Request/Response Examples"**
4. Examples page opens

### Direct Access:
- Just visit: `http://localhost:5072/api/metrics/examples`

### Return to Report:
- Click **"← Back to Performance Report"** at bottom of examples page

## When to Use This Page

### ✅ Use When:
- You want to see actual API requests
- You need to understand REST vs GraphQL differences
- You're debugging request/response structure
- You want to show stakeholders what's happening
- You need examples for documentation
- You're learning GraphQL queries

### ❌ Don't Need When:
- You only care about performance metrics (use main report)
- You're looking at aggregated statistics (use main report)
- You need all requests, not just samples (use browser DevTools)

## Tips

1. **Run tests first** - Examples only appear after running tests
2. **Reset clears examples** - Calling `/api/metrics/reset` removes all examples
3. **2 samples per endpoint** - You'll see max 2 examples of each operation
4. **JSON is formatted** - Raw JSON is automatically pretty-printed
5. **Scrollable** - Long responses have scrollbars

---

This visual guide shows exactly what you'll see when you visit the examples page! 🎉

# REST vs GraphQL Architecture Overview

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         CLIENT LAYER                             │
├─────────────────────────────────────────────────────────────────┤
│  Browser / Postman / Curl / PowerShell Scripts / Test Runner    │
└────────────┬────────────────────────────────────┬────────────────┘
             │                                    │
    ┌────────▼────────┐                  ┌────────▼────────┐
    │   REST API      │                  │  GraphQL API    │
    │  /api/*         │                  │  /graphql       │
    └────────┬────────┘                  └────────┬────────┘
             │                                    │
             └────────────┬───────────────────────┘
                          │
              ┌───────────▼──────────┐
              │   Business Logic     │
              │   DataStore Service  │
              └───────────┬──────────┘
                          │
              ┌───────────▼──────────┐
              │   In-Memory Data     │
              │  - Customers         │
              │  - Orders            │
              │  - Products          │
              │  - Categories        │
              └──────────────────────┘
```

## Data Model Hierarchy

```
Category
  └── Product (Many-to-One)
        └── OrderItem (Many-to-Many via OrderItem)
              ├── Product (One-to-Many)
              ├── Order (Many-to-One)
              └── OrderItemNote (One-to-Many)

Customer
  └── Order (One-to-Many)
        └── OrderItem (One-to-Many)
              └── OrderItemNote (One-to-Many)

Example Flow:
Customer "John Doe"
  └── Order #1 (Dec 2024, $1500)
        ├── OrderItem #1
        │     ├── Product: Laptop ($1200)
        │     │     └── Category: Electronics
        │     └── Notes: ["Express shipping required"]
        └── OrderItem #2
              ├── Product: Mouse ($25)
              │     └── Category: Electronics
              └── Notes: ["Gift wrap", "Blue color preferred"]
```

## REST API Endpoints

```
┌─────────────────────────────────────────────────────────────────┐
│                      REST ENDPOINTS                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  CUSTOMERS                                                       │
│  ├── GET    /api/customers              → List all customers    │
│  ├── GET    /api/customers/{id}         → Get single customer   │
│  └── GET    /api/customers/{id}/orders  → Get customer orders   │
│                                                                  │
│  ORDERS                                                          │
│  ├── GET    /api/orders                 → List all orders       │
│  ├── GET    /api/orders/{id}            → Get single order      │
│  ├── GET    /api/orders/{id}/nested     → Order + full nesting  │
│  ├── POST   /api/orders/bulk            → Bulk create orders    │
│  └── PUT    /api/orders/bulk            → Bulk update orders    │
│                                                                  │
│  PRODUCTS                                                        │
│  ├── GET    /api/products               → List all products     │
│  └── GET    /api/products/{id}          → Get single product    │
│                                                                  │
│  DASHBOARD                                                       │
│  ├── GET    /api/dashboard              → Full dashboard data   │
│  └── GET    /api/dashboard/stats        → Multiple statistics   │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## GraphQL Schema

```graphql
type Query {
  # Single entities
  customer(id: Int!): Customer
  order(id: Int!): Order
  product(id: Int!): Product
  
  # Collections
  customers: [Customer!]!
  orders: [Order!]!
  products: [Product!]!
  categories: [Category!]!
  
  # Aggregations
  dashboard: DashboardViewModel!
}

type Mutation {
  # Single operations
  createOrder(orderDto: OrderCreateDtoInput!): Order!
  
  # Bulk operations
  bulkCreateOrders(request: BulkOrderCreateRequestInput!): BulkOperationResult!
  bulkUpdateOrders(request: BulkOrderUpdateRequestInput!): BulkOperationResult!
}

type Customer {
  id: Int!
  name: String!
  email: String!
  phone: String!
  createdAt: DateTime!
  orders: [Order!]!
}

type Order {
  id: Int!
  orderDate: DateTime!
  status: String!
  totalAmount: Decimal!
  customerId: Int!
  customer: Customer
  items: [OrderItem!]!
}

type OrderItem {
  id: Int!
  orderId: Int!
  productId: Int!
  quantity: Int!
  unitPrice: Decimal!
  discount: Decimal!
  product: Product
  notes: [OrderItemNote!]!
}

type OrderItemNote {
  id: Int!
  orderItemId: Int!
  content: String!
  createdAt: DateTime!
}

type Product {
  id: Int!
  name: String!
  description: String!
  price: Decimal!
  stockQuantity: Int!
  categoryId: Int!
  category: Category
}

type Category {
  id: Int!
  name: String!
  description: String!
  products: [Product!]!
}

type DashboardViewModel {
  totalCustomers: Int!
  totalOrders: Int!
  totalRevenue: Decimal!
  pendingOrders: Int!
  completedOrders: Int!
  topProducts: [TopProductDto!]!
  recentOrders: [RecentOrderDto!]!
  topCustomers: [CustomerStatsDto!]!
  revenueByMonth: [KeyValuePair!]!
}
```

## Comparison Scenarios

### Scenario 1: Simple Query
```
┌──────────────────────────┬──────────────────────────┐
│         REST             │        GraphQL           │
├──────────────────────────┼──────────────────────────┤
│ GET /api/customers/1     │ {                        │
│                          │   customer(id: 1) {      │
│ Returns:                 │     id                   │
│ {                        │     name                 │
│   "id": 1,              │     email                │
│   "name": "John",       │   }                      │
│   "email": "j@ex.com",  │ }                        │
│   "phone": "555-0101",  │                          │
│   "createdAt": "...",   │ Returns only requested   │
│   "orders": []          │ fields                   │
│ }                        │                          │
│                          │                          │
│ Over-fetching: phone,   │ No over-fetching         │
│ createdAt, orders       │                          │
└──────────────────────────┴──────────────────────────┘
```

### Scenario 2: Nested Data (3 Levels)
```
┌──────────────────────────┬──────────────────────────┐
│         REST             │        GraphQL           │
├──────────────────────────┼──────────────────────────┤
│ GET /api/orders/1/nested │ {                        │
│                          │   order(id: 1) {         │
│ 1 Request                │     customer { name }    │
│ Returns ALL fields at    │     items {              │
│ all levels               │       product {          │
│                          │         category {       │
│ Payload: ~5KB            │           name           │
│                          │         }                │
│                          │       }                  │
│                          │       notes { content }  │
│                          │     }                    │
│                          │   }                      │
│                          │ }                        │
│                          │                          │
│                          │ 1 Request                │
│                          │ Returns ONLY requested   │
│                          │ fields                   │
│                          │                          │
│                          │ Payload: ~2KB (60% less) │
└──────────────────────────┴──────────────────────────┘
```

### Scenario 3: Multiple Resources
```
┌──────────────────────────┬──────────────────────────┐
│         REST             │        GraphQL           │
├──────────────────────────┼──────────────────────────┤
│ 3 Separate Requests:     │ 1 Request:               │
│                          │                          │
│ 1. GET /api/customers/1  │ {                        │
│    (50ms)                │   customer(id: 1) {      │
│                          │     name                 │
│ 2. GET /api/customers/   │     orders { id }        │
│    1/orders              │   }                      │
│    (45ms)                │   products {             │
│                          │     name                 │
│ 3. GET /api/products     │     price                │
│    (60ms)                │   }                      │
│                          │ }                        │
│ Total: 155ms             │                          │
│ (network latency x3)     │ Total: 55ms              │
│                          │ (network latency x1)     │
│                          │                          │
│ Bandwidth: Higher        │ Bandwidth: Lower         │
│ Waterfall: Sequential    │ Waterfall: Single        │
└──────────────────────────┴──────────────────────────┘
```

### Scenario 4: Dashboard Aggregation
```
┌──────────────────────────┬──────────────────────────┐
│         REST             │        GraphQL           │
├──────────────────────────┼──────────────────────────┤
│ GET /api/dashboard       │ {                        │
│                          │   dashboard {            │
│ Returns:                 │     totalOrders          │
│ - totalCustomers         │     totalRevenue         │
│ - totalOrders           │     topProducts {        │
│ - totalRevenue          │       productName        │
│ - pendingOrders         │       revenue            │
│ - completedOrders       │     }                    │
│ - topProducts (all)     │   }                      │
│ - recentOrders (all)    │ }                        │
│ - topCustomers (all)    │                          │
│ - revenueByMonth (all)  │ Mobile app can request   │
│                          │ only needed metrics      │
│ Desktop: Uses all        │                          │
│ Mobile: Uses 30%         │ No wasted bandwidth      │
│ Tablet: Uses 60%         │                          │
│                          │                          │
│ Waste: 40-70% for        │ Waste: 0%                │
│ non-desktop clients      │                          │
└──────────────────────────┴──────────────────────────┘
```

### Scenario 5: Bulk Operations
```
┌──────────────────────────┬──────────────────────────┐
│         REST             │        GraphQL           │
├──────────────────────────┼──────────────────────────┤
│ POST /api/orders/bulk    │ mutation {               │
│                          │   bulkCreateOrders(      │
│ {                        │     request: {           │
│   "orders": [            │       orders: [          │
│     {                    │         {                │
│       "customerId": 1,   │           customerId: 1  │
│       "items": [...]     │           items: [...]   │
│     },                   │         }                │
│     ...                  │       ]                  │
│   ]                      │     }                    │
│ }                        │   ) {                    │
│                          │     successCount         │
│ Returns:                 │     failureCount         │
│ {                        │     createdIds           │
│   "successCount": 10,    │   }                      │
│   "failureCount": 0,     │ }                        │
│   "createdIds": [...]    │                          │
│ }                        │                          │
│                          │                          │
│ Performance: Similar     │ Performance: Similar     │
│ Syntax: JSON in body     │ Syntax: GraphQL mutation │
└──────────────────────────┴──────────────────────────┘
```

## Testing Strategy Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    START API                                 │
│            dotnet run or .\start-api.ps1                    │
└────────────────┬────────────────────────────────────────────┘
                 │
       ┌─────────┴─────────┐
       │                   │
┌──────▼──────┐    ┌──────▼──────┐
│ YAML Tests  │    │  Manual     │
│ (Automated) │    │  Testing    │
└──────┬──────┘    └──────┬──────┘
       │                   │
       │           ┌───────┴────────┬────────────┐
       │           │                │            │
       │    ┌──────▼──────┐  ┌─────▼─────┐  ┌──▼──────┐
       │    │  Browser    │  │PowerShell │  │ Postman │
       │    │  GraphQL    │  │  Scripts  │  │         │
       │    │    IDE      │  │           │  │         │
       │    └─────────────┘  └───────────┘  └─────────┘
       │
┌──────▼────────────────────────┐
│   Test Results & Metrics      │
│   - Response times            │
│   - Payload sizes             │
│   - Success rates             │
│   - Assertion results         │
└───────────────────────────────┘
```

## Performance Metrics to Collect

```
┌────────────────────────────────────────────────────────────┐
│                   COMPARISON MATRIX                         │
├─────────────────┬──────────────┬──────────────┬───────────┤
│   Scenario      │     REST     │   GraphQL    │  Winner   │
├─────────────────┼──────────────┼──────────────┼───────────┤
│ Single Query    │   ~15ms     │    ~17ms     │   REST    │
│ Nested (3-lvl)  │   ~35ms     │    ~30ms     │  GraphQL  │
│ Dashboard       │   ~45ms     │    ~40ms     │  GraphQL  │
│ Bulk (10)       │   ~120ms    │    ~125ms    │    Tie    │
│ Multi-resource  │   ~155ms    │    ~55ms     │  GraphQL  │
│                 │  (3 calls)  │   (1 call)   │           │
├─────────────────┼──────────────┼──────────────┼───────────┤
│ Payload Size    │              │              │           │
│  Simple         │    2.1 KB    │    0.8 KB    │  GraphQL  │
│  Nested         │    5.3 KB    │    2.1 KB    │  GraphQL  │
│  Dashboard      │    8.5 KB    │    3.2 KB    │  GraphQL  │
├─────────────────┼──────────────┼──────────────┼───────────┤
│ Requests        │    Higher    │    Lower     │  GraphQL  │
│ Cacheability    │   Excellent  │     Good     │   REST    │
│ Flexibility     │     Fixed    │   Dynamic    │  GraphQL  │
│ Complexity      │   Simple     │   Learning   │   REST    │
└─────────────────┴──────────────┴──────────────┴───────────┘

* Actual results will vary based on your system and network
```

## Decision Matrix

```
Use GraphQL when:
  ✓ Complex UI with varied data needs
  ✓ Mobile apps (bandwidth critical)
  ✓ Multiple client types
  ✓ Rapid frontend iteration
  ✓ Reducing roundtrips is critical

Use REST when:
  ✓ Simple CRUD operations
  ✓ Heavy caching requirements
  ✓ File uploads/downloads
  ✓ Team lacks GraphQL skills
  ✓ Standard HTTP patterns needed

Use Both when:
  ✓ Large organization
  ✓ Different client needs
  ✓ Gradual migration
  ✓ Testing/experimentation
```

---

This visual guide provides a comprehensive overview of the REST vs GraphQL comparison project architecture and expected outcomes.

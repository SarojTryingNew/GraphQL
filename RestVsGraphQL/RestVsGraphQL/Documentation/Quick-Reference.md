# GraphQL Quick Reference Card

Quick reference for common GraphQL operations in this POC implementation.

---

## Quick Start

### Run the Application
```bash
cd RestVsGraphQL
dotnet run
# Navigate to http://localhost:5000/graphql/
```

### Run Tests
```bash
dotnet run --project TestRunner
```

---

## Common Queries

### Get All Customers
```graphql
{
  customers {
    id
    name
    email
    phone
    createdAt
  }
}
```

### Get Single Customer
```graphql
{
  customer(id: 1) {
    id
    name
    email
  }
}
```

### Get Customer with Orders
```graphql
{
  customer(id: 1) {
    id
    name
    email
    orders {
      id
      orderDate
      totalAmount
      status
    }
  }
}
```

### Get Order with Full Nesting (4 levels)
```graphql
{
  order(id: 1) {
    id
    orderDate
    status
    totalAmount
    customer {
      id
      name
      email
    }
    items {
      id
      quantity
      unitPrice
      discount
      product {
        id
        name
        price
        category {
          id
          name
        }
      }
      notes {
        id
        content
        createdAt
      }
    }
  }
}
```

### Get Multiple Orders
```graphql
{
  ordersByIds(ids: [1, 2, 3]) {
    id
    orderDate
    totalAmount
    customer {
      name
    }
  }
}
```

### Get Dashboard
```graphql
{
  dashboard {
    totalCustomers
    totalOrders
    totalRevenue
    pendingOrders
    completedOrders
    topProducts {
      productId
      productName
      quantitySold
      revenue
    }
    topCustomers {
      customerId
      customerName
      totalOrders
      totalSpent
    }
    recentOrders {
      orderId
      orderDate
      customerName
      totalAmount
      status
    }
  }
}
```

### Get All Products with Categories
```graphql
{
  products {
    id
    name
    description
    price
    stockQuantity
    category {
      id
      name
      description
    }
  }
}
```

---

## Common Mutations

### Create Single Order
```graphql
mutation {
  createOrder(orderDto: {
    customerId: 1
    status: "Pending"
    items: [
      {
        productId: 5
        quantity: 10
        discount: 5
        notes: ["Urgent delivery", "Fragile"]
      },
      {
        productId: 8
        quantity: 2
        discount: 0
        notes: []
      }
    ]
  }) {
    id
    orderDate
    totalAmount
    status
    items {
      id
      quantity
      unitPrice
      product {
        name
        price
      }
    }
  }
}
```

### Bulk Create Orders
```graphql
mutation {
  bulkCreateOrders(request: {
    orders: [
      {
        customerId: 1
        status: "Pending"
        items: [
          { productId: 5, quantity: 10, discount: 5, notes: ["Test 1"] }
        ]
      },
      {
        customerId: 2
        status: "Processing"
        items: [
          { productId: 3, quantity: 5, discount: 0, notes: [] },
          { productId: 7, quantity: 2, discount: 10, notes: ["Gift wrap"] }
        ]
      }
    ]
  }) {
    successCount
    failureCount
    createdIds
    errors
  }
}
```

### Bulk Update Orders
```graphql
mutation {
  bulkUpdateOrders(request: {
    orders: [
      {
        id: 1
        status: "Completed"
      },
      {
        id: 2
        status: "Shipped"
      }
    ]
  }) {
    successCount
    failureCount
    errors
  }
}
```

### Bulk Delete Orders
```graphql
mutation {
  bulkDeleteOrders(request: {
    orderIds: [10, 11, 12]
  }) {
    successCount
    failureCount
    deletedIds
    errors
  }
}
```

---

## Variables Example

### Query with Variables
```graphql
query GetCustomerOrders($customerId: Int!) {
  customer(id: $customerId) {
    id
    name
    email
    orders {
      id
      orderDate
      totalAmount
      status
    }
  }
}
```

**Variables:**
```json
{
  "customerId": 1
}
```

### Mutation with Variables
```graphql
mutation CreateOrder($orderInput: OrderCreateDto!) {
  createOrder(orderDto: $orderInput) {
    id
    orderDate
    totalAmount
  }
}
```

**Variables:**
```json
{
  "orderInput": {
    "customerId": 1,
    "status": "Pending",
    "items": [
      {
        "productId": 5,
        "quantity": 10,
        "discount": 5,
        "notes": ["Test"]
      }
    ]
  }
}
```

---

## Advanced Queries

### Multiple Queries in One Request
```graphql
query GetDashboardData {
  customers {
    id
    name
  }
  
  recentOrders: orders {
    id
    orderDate
    totalAmount
  }
  
  products {
    id
    name
    price
    stockQuantity
  }
  
  dashboard {
    totalRevenue
    totalOrders
  }
}
```

### Aliases
```graphql
{
  customer1: customer(id: 1) {
    name
  }
  
  customer2: customer(id: 2) {
    name
  }
}
```

### Fragments
```graphql
query {
  customer(id: 1) {
    ...CustomerDetails
    orders {
      id
      totalAmount
    }
  }
}

fragment CustomerDetails on Customer {
  id
  name
  email
  phone
  createdAt
}
```

---

## REST Equivalents

### REST: Get Customer
```http
GET /api/customers/1
```

**GraphQL Equivalent:**
```graphql
{ customer(id: 1) { id name email } }
```

---

### REST: Get Order with Nested Data
```http
GET /api/orders/1/nested
```

**GraphQL Equivalent:**
```graphql
{
  order(id: 1) {
    id
    customer { name }
    items {
      product { name }
    }
  }
}
```

---

### REST: Bulk Create Orders
```http
POST /api/orders/bulk
Content-Type: application/json

{
  "orders": [...]
}
```

**GraphQL Equivalent:**
```graphql
mutation {
  bulkCreateOrders(request: { orders: [...] }) {
    successCount
  }
}
```

---

### REST: Multiple Calls
```http
GET /api/customers/1        # Call 1
GET /api/customers/1/orders # Call 2
GET /api/products           # Call 3
```

**GraphQL Equivalent (Single Call):**
```graphql
{
  customer(id: 1) {
    id
    name
    orders { id orderDate }
  }
  products { id name price }
}
```

---

## C# Implementation Snippets

### Basic Query
```csharp
public class Query
{
    public IEnumerable<Customer> GetCustomers([Service] DataStore dataStore)
        => dataStore.Customers;
    
    public Customer? GetCustomer(int id, [Service] DataStore dataStore)
        => dataStore.Customers.FirstOrDefault(c => c.Id == id);
}
```

### Basic Mutation
```csharp
public class Mutation
{
    public Order CreateOrder(
        OrderCreateDto orderDto, 
        [Service] DataStore dataStore)
    {
        var order = new Order
        {
            Id = dataStore.GetNextOrderId(),
            CustomerId = orderDto.CustomerId,
            OrderDate = DateTime.UtcNow
        };
        
        dataStore.Orders.Add(order);
        return order;
    }
}
```

### DataLoader
```csharp
public class CustomerByIdDataLoader : BatchDataLoader<int, Customer>
{
    private readonly DataStore _dataStore;

    protected override Task<IReadOnlyDictionary<int, Customer>> LoadBatchAsync(
        IReadOnlyList<int> keys, 
        CancellationToken ct)
    {
        var customers = _dataStore.Customers
            .Where(c => keys.Contains(c.Id))
            .ToDictionary(c => c.Id);
        
        return Task.FromResult<IReadOnlyDictionary<int, Customer>>(customers);
    }
}
```

### Use DataLoader
```csharp
public class OrderType : ObjectType<Order>
{
    protected override void Configure(IObjectTypeDescriptor<Order> descriptor)
    {
        descriptor
            .Field(o => o.Customer)
            .ResolveWith<OrderResolvers>(r => r.GetCustomer(default!, default!))
            .UseDataLoader<CustomerByIdDataLoader>();
    }
}

public class OrderResolvers
{
    public async Task<Customer?> GetCustomer(
        [Parent] Order order,
        CustomerByIdDataLoader dataLoader)
    {
        return await dataLoader.LoadAsync(order.CustomerId);
    }
}
```

---

## Testing Snippets

### YAML Test (REST)
```yaml
- name: "Get Customer"
  type: "REST"
  method: "GET"
  endpoint: "/api/customers/1"
  assertions:
    statusCode: 200
    contains: "name"
```

### YAML Test (GraphQL)
```yaml
- name: "Get Customer"
  type: "GraphQL"
  query: |
    {
      customer(id: 1) {
        id
        name
        email
      }
    }
  assertions:
    statusCode: 200
    contains: "customer"
```

---

## Error Handling

### Not Found (null)
```graphql
{
  customer(id: 99999) {
    id
  }
}
```

**Response:**
```json
{
  "data": {
    "customer": null
  }
}
```

### Validation Error
```graphql
mutation {
  createOrder(orderDto: {
    customerId: 99999
    items: []
  }) {
    id
  }
}
```

**Response:**
```json
{
  "errors": [
    {
      "message": "Customer 99999 not found",
      "path": ["createOrder"]
    }
  ],
  "data": {
    "createOrder": null
  }
}
```

### Bulk Operation Partial Success
```graphql
mutation {
  bulkCreateOrders(request: {
    orders: [
      { customerId: 1, items: [...] },
      { customerId: 99999, items: [...] }
    ]
  }) {
    successCount
    failureCount
    createdIds
    errors
  }
}
```

**Response:**
```json
{
  "data": {
    "bulkCreateOrders": {
      "successCount": 1,
      "failureCount": 1,
      "createdIds": [101],
      "errors": ["Customer 99999 not found"]
    }
  }
}
```

---

## Performance Tips

### ✅ Use Field Selection
```graphql
# ❌ Over-fetching
{ customers { id name email phone createdAt address city zip } }

# ✅ Request only what you need
{ customers { id name email } }
```

### ✅ Use DataLoaders for Relations
```csharp
// Prevents N+1 queries
descriptor
    .Field(o => o.Customer)
    .UseDataLoader<CustomerByIdDataLoader>();
```

### ✅ Use Fragments for Reusable Selections
```graphql
fragment OrderSummary on Order {
  id
  orderDate
  totalAmount
  status
}

query {
  order(id: 1) {
    ...OrderSummary
  }
}
```

### ✅ Batch Multiple Queries
```graphql
# Instead of 3 separate HTTP calls:
{
  customer(id: 1) { ... }
  products { ... }
  dashboard { ... }
}
```

---

## Common Mistakes

### ❌ Forgetting to Register DataLoader
```csharp
// Program.cs
builder.Services
    .AddGraphQLServer()
    .AddDataLoader<CustomerByIdDataLoader>()  // ← Don't forget!
```

### ❌ Not Using [Service] Attribute
```csharp
// ❌ Wrong
public IEnumerable<Customer> GetCustomers(DataStore dataStore)

// ✅ Correct
public IEnumerable<Customer> GetCustomers([Service] DataStore dataStore)
```

### ❌ Circular References in Types
```csharp
// Can cause infinite loops - use DataLoaders or configure carefully
public class Customer
{
    public List<Order> Orders { get; set; }
}

public class Order
{
    public Customer Customer { get; set; }
}
```

---

## Useful Commands

### Build Project
```bash
dotnet build
```

### Run Application
```bash
dotnet run
```

### Run Tests
```bash
dotnet run --project TestRunner
```

### Restore Packages
```bash
dotnet restore
```

### Clean Build
```bash
dotnet clean
dotnet build
```

---

## Endpoint Reference

| Endpoint | Purpose |
|----------|---------|
| `http://localhost:5000/graphql/` | GraphQL Playground (IDE) |
| `http://localhost:5000/graphql` | GraphQL API endpoint |
| `http://localhost:5000/swagger` | Swagger UI (REST API docs) |
| `http://localhost:5000/api/*` | REST API endpoints |

---

## Schema Introspection

### Get Full Schema
```graphql
{
  __schema {
    types {
      name
      description
    }
  }
}
```

### Get Type Details
```graphql
{
  __type(name: "Order") {
    name
    fields {
      name
      type {
        name
      }
    }
  }
}
```

---

## Next Steps

1. **Read Full Documentation**: [README.md](./README.md)
2. **Learn Implementation**: [How-to-GraphQL.md](./How-to-GraphQL.md)
3. **Study Schema**: [GraphQL-Query-Model.md](./GraphQL-Query-Model.md)
4. **Plan Migration**: [Migration-Guide.md](./Migration-Guide.md)

---

**Bookmark This Page** for quick reference during development!

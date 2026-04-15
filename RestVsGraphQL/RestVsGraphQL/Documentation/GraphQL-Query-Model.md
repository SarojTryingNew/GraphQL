# GraphQL Query Model - Early Draft

## Schema Definition

This document provides a complete GraphQL schema for the ECO system, demonstrating queries, mutations, types, and relationships.

---

## Core Types

### Customer

```graphql
type Customer {
  """Unique customer identifier"""
  id: Int!
  
  """Customer full name"""
  name: String!
  
  """Customer email address"""
  email: String!
  
  """Customer phone number (optional)"""
  phone: String
  
  """Account creation timestamp"""
  createdAt: DateTime!
  
  """All orders placed by this customer"""
  orders: [Order!]!
}
```

**C# Implementation:**
```csharp
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<Order> Orders { get; set; } = new();
}
```

---

### Order

```graphql
type Order {
  """Unique order identifier"""
  id: Int!
  
  """Order creation date"""
  orderDate: DateTime!
  
  """Customer ID who placed the order"""
  customerId: Int!
  
  """Customer details"""
  customer: Customer
  
  """Order status (Pending, Processing, Completed, Cancelled)"""
  status: String!
  
  """Total order amount (calculated)"""
  totalAmount: Decimal!
  
  """Order line items"""
  items: [OrderItem!]!
}
```

**C# Implementation:**
```csharp
public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    
    public void RecalculateTotal()
    {
        TotalAmount = Items.Sum(item => 
            item.Quantity * item.UnitPrice * (1 - item.Discount / 100));
    }
}
```

---

### OrderItem

```graphql
type OrderItem {
  """Unique order item identifier"""
  id: Int!
  
  """Parent order ID"""
  orderId: Int!
  
  """Product ID"""
  productId: Int!
  
  """Product details"""
  product: Product
  
  """Quantity ordered"""
  quantity: Int!
  
  """Unit price at time of order"""
  unitPrice: Decimal!
  
  """Discount percentage (0-100)"""
  discount: Decimal!
  
  """Notes attached to this order item"""
  notes: [OrderItemNote!]!
}
```

---

### OrderItemNote

```graphql
type OrderItemNote {
  """Unique note identifier"""
  id: Int!
  
  """Parent order item ID"""
  orderItemId: Int!
  
  """Note content"""
  content: String!
  
  """Note creation timestamp"""
  createdAt: DateTime!
}
```

---

### Product

```graphql
type Product {
  """Unique product identifier"""
  id: Int!
  
  """Product name"""
  name: String!
  
  """Product description"""
  description: String
  
  """Current price"""
  price: Decimal!
  
  """Available stock quantity"""
  stockQuantity: Int!
  
  """Category ID"""
  categoryId: Int!
  
  """Product category"""
  category: Category
}
```

---

### Category

```graphql
type Category {
  """Unique category identifier"""
  id: Int!
  
  """Category name"""
  name: String!
  
  """Category description"""
  description: String
  
  """Products in this category"""
  products: [Product!]!
}
```

---

### Dashboard (Aggregation Type)

```graphql
type DashboardViewModel {
  """Total number of customers"""
  totalCustomers: Int!
  
  """Total number of orders"""
  totalOrders: Int!
  
  """Total revenue across all orders"""
  totalRevenue: Decimal!
  
  """Number of pending orders"""
  pendingOrders: Int!
  
  """Number of completed orders"""
  completedOrders: Int!
  
  """Top selling products"""
  topProducts: [TopProductViewModel!]!
  
  """Top spending customers"""
  topCustomers: [TopCustomerViewModel!]!
  
  """Most recent orders"""
  recentOrders: [RecentOrderViewModel!]!
}

type TopProductViewModel {
  productId: Int!
  productName: String!
  quantitySold: Int!
  revenue: Decimal!
}

type TopCustomerViewModel {
  customerId: Int!
  customerName: String!
  totalOrders: Int!
  totalSpent: Decimal!
}

type RecentOrderViewModel {
  orderId: Int!
  orderDate: DateTime!
  customerName: String!
  totalAmount: Decimal!
  status: String!
}
```

---

## Input Types

### OrderCreateDto

```graphql
input OrderCreateDto {
  """Customer ID placing the order"""
  customerId: Int!
  
  """Order status (default: Pending)"""
  status: String = "Pending"
  
  """Order line items"""
  items: [OrderItemCreateDto!]!
}
```

---

### OrderItemCreateDto

```graphql
input OrderItemCreateDto {
  """Product ID to order"""
  productId: Int!
  
  """Quantity to order"""
  quantity: Int!
  
  """Discount percentage (0-100)"""
  discount: Decimal!
  
  """Optional notes"""
  notes: [String!]!
}
```

---

### BulkOrderCreateRequest

```graphql
input BulkOrderCreateRequest {
  """List of orders to create"""
  orders: [OrderCreateDto!]!
}
```

---

### BulkOrderUpdateRequest

```graphql
input BulkOrderUpdateRequest {
  """List of orders to update"""
  orders: [OrderUpdateDto!]!
}

input OrderUpdateDto {
  """Order ID to update"""
  id: Int!
  
  """New status (optional)"""
  status: String
  
  """Updated items (optional)"""
  items: [OrderItemUpdateDto!]
}

input OrderItemUpdateDto {
  """Order item ID (null for new items)"""
  id: Int
  
  """Product ID"""
  productId: Int!
  
  """New quantity"""
  quantity: Int!
  
  """New discount"""
  discount: Decimal!
}
```

---

### BulkOrderDeleteRequest

```graphql
input BulkOrderDeleteRequest {
  """List of order IDs to delete"""
  orderIds: [Int!]!
}
```

---

## Output Types

### BulkOperationResult

```graphql
type BulkOperationResult {
  """Number of successful operations"""
  successCount: Int!
  
  """Number of failed operations"""
  failureCount: Int!
  
  """List of error messages"""
  errors: [String!]!
  
  """IDs of created entities"""
  createdIds: [Int!]!
  
  """IDs of deleted entities"""
  deletedIds: [Int!]!
}
```

**C# Implementation:**
```csharp
public class BulkOperationResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<int> CreatedIds { get; set; } = new();
    public List<int> DeletedIds { get; set; } = new();
}
```

---

## Query Operations

### Root Query Type

```graphql
type Query {
  """Get all customers"""
  customers: [Customer!]!
  
  """Get a single customer by ID"""
  customer(id: Int!): Customer
  
  """Get all orders"""
  orders: [Order!]!
  
  """Get a single order by ID"""
  order(id: Int!): Order
  
  """Get multiple orders by IDs"""
  ordersByIds(ids: [Int!]!): [Order!]!
  
  """Get all products"""
  products: [Product!]!
  
  """Get a single product by ID"""
  product(id: Int!): Product
  
  """Get all categories"""
  categories: [Category!]!
  
  """Get dashboard aggregation data"""
  dashboard: DashboardViewModel!
}
```

**C# Implementation:**
```csharp
public class Query
{
    public IEnumerable<Customer> GetCustomers([Service] DataStore dataStore)
        => dataStore.Customers;

    public Customer? GetCustomer(int id, [Service] DataStore dataStore)
        => dataStore.Customers.FirstOrDefault(c => c.Id == id);

    public IEnumerable<Order> GetOrders([Service] DataStore dataStore)
        => dataStore.Orders;

    public Order? GetOrder(int id, [Service] DataStore dataStore)
    {
        var order = dataStore.Orders.FirstOrDefault(o => o.Id == id);
        order?.LoadRelations(dataStore);
        return order;
    }

    public IEnumerable<Order> GetOrdersByIds(
        List<int> ids, 
        [Service] DataStore dataStore)
    {
        var orders = dataStore.Orders
            .Where(o => ids.Contains(o.Id))
            .ToList();
        orders.LoadRelations(dataStore);
        return orders;
    }

    public IEnumerable<Product> GetProducts([Service] DataStore dataStore)
        => dataStore.Products;

    public Product? GetProduct(int id, [Service] DataStore dataStore)
        => dataStore.Products.FirstOrDefault(p => p.Id == id);

    public IEnumerable<Category> GetCategories([Service] DataStore dataStore)
        => dataStore.Categories;

    public DashboardViewModel GetDashboard()
        => new DashboardViewModel();
}
```

---

## Mutation Operations

### Root Mutation Type

```graphql
type Mutation {
  """Create a single order"""
  createOrder(orderDto: OrderCreateDto!): Order!
  
  """Bulk create multiple orders"""
  bulkCreateOrders(request: BulkOrderCreateRequest!): BulkOperationResult!
  
  """Bulk update multiple orders"""
  bulkUpdateOrders(request: BulkOrderUpdateRequest!): BulkOperationResult!
  
  """Bulk delete multiple orders"""
  bulkDeleteOrders(request: BulkOrderDeleteRequest!): BulkOperationResult!
}
```

**C# Implementation:**
```csharp
public class Mutation
{
    public Order CreateOrder(
        OrderCreateDto orderDto, 
        [Service] DataStore dataStore)
    {
        // Implementation in How-to-GraphQL.md
    }

    public BulkOperationResult BulkCreateOrders(
        BulkOrderCreateRequest request,
        [Service] OrderService orderService)
    {
        return orderService.BulkCreateOrders(request);
    }

    public BulkOperationResult BulkUpdateOrders(
        BulkOrderUpdateRequest request,
        [Service] OrderService orderService)
    {
        return orderService.BulkUpdateOrders(request);
    }

    public BulkOperationResult BulkDeleteOrders(
        BulkOrderDeleteRequest request,
        [Service] OrderService orderService)
    {
        return orderService.BulkDeleteOrders(request);
    }
}
```

---

## Example Queries

### Simple Query: Get All Customers

```graphql
query GetAllCustomers {
  customers {
    id
    name
    email
    phone
    createdAt
  }
}
```

**Expected Response:**
```json
{
  "data": {
    "customers": [
      {
        "id": 1,
        "name": "John Doe",
        "email": "john.doe@example.com",
        "phone": "+1-555-0101",
        "createdAt": "2024-01-15T10:30:00Z"
      },
      {
        "id": 2,
        "name": "Jane Smith",
        "email": "jane.smith@example.com",
        "phone": "+1-555-0102",
        "createdAt": "2024-01-16T11:20:00Z"
      }
    ]
  }
}
```

---

### Nested Query: Order with Relations

```graphql
query GetOrderWithDetails {
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

**Expected Response (4-level nesting):**
```json
{
  "data": {
    "order": {
      "id": 1,
      "orderDate": "2024-01-20T14:30:00Z",
      "status": "Completed",
      "totalAmount": 285.50,
      "customer": {
        "id": 1,
        "name": "John Doe",
        "email": "john.doe@example.com"
      },
      "items": [
        {
          "id": 101,
          "quantity": 5,
          "unitPrice": 29.99,
          "discount": 10,
          "product": {
            "id": 5,
            "name": "Wireless Mouse",
            "price": 29.99,
            "category": {
              "id": 1,
              "name": "Electronics"
            }
          },
          "notes": [
            {
              "id": 1001,
              "content": "Gift wrap requested",
              "createdAt": "2024-01-20T14:35:00Z"
            }
          ]
        }
      ]
    }
  }
}
```

---

### Dashboard Query

```graphql
query GetDashboard {
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

---

### Multiple Queries in One Request

```graphql
query GetCustomerAndProducts {
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
  
  products {
    id
    name
    price
    stockQuantity
    category {
      name
    }
  }
}
```

**Advantage**: Replaces 2-3 REST API calls with **one GraphQL request**.

---

## Example Mutations

### Create Single Order

```graphql
mutation CreateNewOrder {
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
      product {
        name
        price
      }
    }
  }
}
```

**Expected Response:**
```json
{
  "data": {
    "createOrder": {
      "id": 150,
      "orderDate": "2024-01-25T16:45:00Z",
      "totalAmount": 342.10,
      "status": "Pending",
      "items": [
        {
          "id": 201,
          "quantity": 10,
          "product": {
            "name": "Wireless Mouse",
            "price": 29.99
          }
        },
        {
          "id": 202,
          "quantity": 2,
          "product": {
            "name": "USB-C Cable",
            "price": 12.99
          }
        }
      ]
    }
  }
}
```

---

### Bulk Create Orders

```graphql
mutation BulkCreateOrders {
  bulkCreateOrders(request: {
    orders: [
      {
        customerId: 1
        status: "Pending"
        items: [
          { productId: 5, quantity: 10, discount: 5, notes: ["Urgent"] }
        ]
      },
      {
        customerId: 2
        status: "Processing"
        items: [
          { productId: 3, quantity: 5, discount: 0, notes: [] },
          { productId: 7, quantity: 2, discount: 10, notes: ["Gift wrap"] }
        ]
      },
      {
        customerId: 999
        status: "Pending"
        items: [
          { productId: 1, quantity: 1, discount: 0, notes: [] }
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

**Expected Response:**
```json
{
  "data": {
    "bulkCreateOrders": {
      "successCount": 2,
      "failureCount": 1,
      "createdIds": [151, 152],
      "errors": [
        "Customer 999 not found"
      ]
    }
  }
}
```

**Key Feature**: Partial success support - valid orders created, errors reported for invalid ones.

---

### Bulk Delete Orders

```graphql
mutation BulkDeleteOrders {
  bulkDeleteOrders(request: {
    orderIds: [10, 11, 12, 999, 998]
  }) {
    successCount
    failureCount
    deletedIds
    errors
  }
}
```

**Expected Response:**
```json
{
  "data": {
    "bulkDeleteOrders": {
      "successCount": 3,
      "failureCount": 2,
      "deletedIds": [10, 11, 12],
      "errors": [
        "Order 999 not found",
        "Order 998 not found"
      ]
    }
  }
}
```

---

## DataLoader Optimizations

### Problem: N+1 Queries

**Without DataLoaders:**
```graphql
query {
  orders {                # 1 query: fetch all orders
    id
    customer {            # N queries: one per order!
      name
    }
  }
}
```
**Result**: 1 + N database queries (e.g., 101 queries for 100 orders)

---

### Solution: Batched DataLoaders

**With DataLoaders:**
```graphql
query {
  orders {                # 1 query: fetch all orders
    id
    customer {            # 1 BATCHED query: fetch all customers at once
      name
    }
  }
}
```
**Result**: Only **2 queries** total, regardless of N

**Implementation:**
```csharp
// GraphQL/DataLoaders/CustomerByIdDataLoader.cs
public class CustomerByIdDataLoader : BatchDataLoader<int, Customer>
{
    protected override async Task<IReadOnlyDictionary<int, Customer>> LoadBatchAsync(
        IReadOnlyList<int> keys, 
        CancellationToken ct)
    {
        // Batch load all customers in ONE query
        var customers = _dataStore.Customers
            .Where(c => keys.Contains(c.Id))
            .ToList();

        return customers.ToDictionary(c => c.Id);
    }
}
```

---

## Pagination (Future Enhancement)

### Cursor-Based Pagination

```graphql
type Query {
  ordersPaginated(
    first: Int
    after: String
  ): OrderConnection!
}

type OrderConnection {
  edges: [OrderEdge!]!
  pageInfo: PageInfo!
  totalCount: Int!
}

type OrderEdge {
  node: Order!
  cursor: String!
}

type PageInfo {
  hasNextPage: Boolean!
  hasPreviousPage: Boolean!
  startCursor: String
  endCursor: String
}
```

**Example Query:**
```graphql
query {
  ordersPaginated(first: 20, after: "cursor123") {
    edges {
      node {
        id
        orderDate
        totalAmount
      }
      cursor
    }
    pageInfo {
      hasNextPage
      endCursor
    }
    totalCount
  }
}
```

---

## Filtering & Sorting (Future Enhancement)

```graphql
type Query {
  ordersFiltered(
    status: String
    minAmount: Decimal
    maxAmount: Decimal
    orderBy: OrderSortInput
  ): [Order!]!
}

input OrderSortInput {
  field: OrderSortField!
  direction: SortDirection!
}

enum OrderSortField {
  ORDER_DATE
  TOTAL_AMOUNT
  STATUS
}

enum SortDirection {
  ASC
  DESC
}
```

**Example:**
```graphql
query {
  ordersFiltered(
    status: "Completed"
    minAmount: 100
    orderBy: { field: TOTAL_AMOUNT, direction: DESC }
  ) {
    id
    totalAmount
  }
}
```

---

## Subscriptions (Future Enhancement)

```graphql
type Subscription {
  """Subscribe to new order events"""
  orderCreated: Order!
  
  """Subscribe to order status changes"""
  orderStatusChanged(orderId: Int!): Order!
}
```

**Example:**
```graphql
subscription {
  orderCreated {
    id
    orderDate
    customer {
      name
    }
    totalAmount
  }
}
```

---

## Schema Evolution Guidelines

### ✅ Safe Changes (Non-Breaking)

- Add new types
- Add new fields to existing types
- Add new optional arguments
- Deprecate fields (don't remove)

```graphql
type Customer {
  id: Int!
  name: String!
  email: String!
  oldField: String @deprecated(reason: "Use newField instead")
  newField: String
}
```

### ❌ Breaking Changes (Avoid)

- Remove types
- Remove fields
- Change field types
- Make optional arguments required
- Change argument types

---

## Error Handling Patterns

### 1. Null for Not Found

```graphql
query {
  customer(id: 999) {
    id
    name
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

---

### 2. Exceptions for Domain Errors

```graphql
mutation {
  createOrder(orderDto: {
    customerId: 999
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
      "message": "Customer 999 not found",
      "path": ["createOrder"]
    }
  ],
  "data": {
    "createOrder": null
  }
}
```

---

### 3. Structured Results for Bulk Operations

```graphql
mutation {
  bulkCreateOrders(request: { orders: [...] }) {
    successCount
    failureCount
    createdIds
    errors
  }
}
```

**Response (Partial Success):**
```json
{
  "data": {
    "bulkCreateOrders": {
      "successCount": 8,
      "failureCount": 2,
      "createdIds": [101, 102, 103, ...],
      "errors": [
        "Customer 999 not found",
        "Product 777 out of stock"
      ]
    }
  }
}
```

---

## Performance Considerations

### Query Complexity

**Limit nested depth:**
```csharp
builder.Services
    .AddGraphQLServer()
    .ModifyRequestOptions(opt => opt.MaxAllowedExecutionDepth = 10);
```

### Query Cost Analysis (Future)

Assign costs to fields and limit total query cost:

```graphql
type Query {
  orders: [Order!]! @cost(weight: 10)
  dashboard: DashboardViewModel! @cost(weight: 50)
}
```

---

## Complete Schema Summary

```graphql
# Scalars
scalar DateTime
scalar Decimal

# Query Root
type Query {
  customers: [Customer!]!
  customer(id: Int!): Customer
  orders: [Order!]!
  order(id: Int!): Order
  ordersByIds(ids: [Int!]!): [Order!]!
  products: [Product!]!
  product(id: Int!): Product
  categories: [Category!]!
  dashboard: DashboardViewModel!
}

# Mutation Root
type Mutation {
  createOrder(orderDto: OrderCreateDto!): Order!
  bulkCreateOrders(request: BulkOrderCreateRequest!): BulkOperationResult!
  bulkUpdateOrders(request: BulkOrderUpdateRequest!): BulkOperationResult!
  bulkDeleteOrders(request: BulkOrderDeleteRequest!): BulkOperationResult!
}

# Entity Types
type Customer { ... }
type Order { ... }
type OrderItem { ... }
type OrderItemNote { ... }
type Product { ... }
type Category { ... }

# Aggregation Types
type DashboardViewModel { ... }
type TopProductViewModel { ... }
type TopCustomerViewModel { ... }
type RecentOrderViewModel { ... }

# Input Types
input OrderCreateDto { ... }
input OrderItemCreateDto { ... }
input BulkOrderCreateRequest { ... }
input BulkOrderUpdateRequest { ... }
input OrderUpdateDto { ... }
input OrderItemUpdateDto { ... }
input BulkOrderDeleteRequest { ... }

# Result Types
type BulkOperationResult { ... }
```

---

**Next**: [Migration Guide](./Migration-Guide.md)

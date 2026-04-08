# GraphQL Schema Reference

Complete reference for the GraphQL API implementation, including all queries, mutations, and types.

---

## 🌐 GraphQL Endpoint

**URL**: `http://localhost:5072/graphql`  
**GraphQL IDE**: Open `http://localhost:5072/graphql` in your browser (Banana Cake Pop)

---

## 📋 Table of Contents

1. [Queries](#queries)
2. [Mutations](#mutations)
3. [Types](#types)
4. [Example Queries](#example-queries)
5. [Example Mutations](#example-mutations)

---

## 🔍 Queries

### Get All Customers
```graphql
query {
  customers {
    id
    name
    email
    phone
    address
  }
}
```

### Get Customer by ID
```graphql
query {
  customer(id: 1) {
    id
    name
    email
    phone
    address
  }
}
```

### Get All Orders
```graphql
query {
  orders {
    id
    customerId
    orderDate
    totalAmount
    status
  }
}
```

### Get Order by ID (with nested data)
```graphql
query {
  order(id: 1) {
    id
    orderDate
    totalAmount
    status
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

### Get All Products
```graphql
query {
  products {
    id
    name
    description
    price
    stock
    categoryId
  }
}
```

### Get Product by ID (with category)
```graphql
query {
  product(id: 1) {
    id
    name
    description
    price
    stock
    category {
      id
      name
      description
    }
  }
}
```

### Get All Categories
```graphql
query {
  categories {
    id
    name
    description
  }
}
```

### Get Dashboard Data
```graphql
query {
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
    recentOrders {
      orderId
      orderDate
      customerName
      totalAmount
      status
    }
    topCustomers {
      customerId
      customerName
      orderCount
      totalSpent
    }
    revenueByMonth {
      month
      revenue
    }
  }
}
```

---

## ✏️ Mutations

### Bulk Create Orders
```graphql
mutation BulkCreateOrders($request: BulkOrderCreateRequestInput!) {
  bulkCreateOrders(request: $request) {
    successCount
    failureCount
    createdIds
    errors
  }
}
```

**Variables:**
```json
{
  "request": {
    "orders": [
      {
        "customerId": 1,
        "status": "Pending",
        "items": [
          {
            "productId": 1,
            "quantity": 2,
            "discount": 10,
            "notes": ["Express delivery", "Gift wrap"]
          }
        ]
      }
    ]
  }
}
```

### Bulk Update Orders
```graphql
mutation BulkUpdateOrders($request: BulkOrderUpdateRequestInput!) {
  bulkUpdateOrders(request: $request) {
    successCount
    failureCount
    errors
  }
}
```

**Variables:**
```json
{
  "request": {
    "orders": [
      {
        "id": 1,
        "status": "Completed"
      },
      {
        "id": 2,
        "status": "Shipped"
      }
    ]
  }
}
```

### Bulk Delete Orders
```graphql
mutation BulkDeleteOrders($request: BulkOrderDeleteRequestInput!) {
  bulkDeleteOrders(request: $request) {
    successCount
    failureCount
    errors
    deletedIds
  }
}
```

**Variables:**
```json
{
  "request": {
    "orderIds": [1, 2, 3, 4, 5]
  }
}
```

---

## 📦 Types

### Customer
```graphql
type Customer {
  id: Int!
  name: String!
  email: String!
  phone: String
  address: String
}
```

### Order
```graphql
type Order {
  id: Int!
  customerId: Int!
  customer: Customer
  orderDate: DateTime!
  totalAmount: Decimal!
  status: String!
  items: [OrderItem!]!
}
```

### OrderItem
```graphql
type OrderItem {
  id: Int!
  orderId: Int!
  productId: Int!
  product: Product
  quantity: Int!
  unitPrice: Decimal!
  discount: Decimal!
  notes: [OrderItemNote!]!
}
```

### OrderItemNote
```graphql
type OrderItemNote {
  id: Int!
  orderItemId: Int!
  content: String!
  createdAt: DateTime!
}
```

### Product
```graphql
type Product {
  id: Int!
  name: String!
  description: String
  price: Decimal!
  stock: Int!
  categoryId: Int!
  category: Category
}
```

### Category
```graphql
type Category {
  id: Int!
  name: String!
  description: String
}
```

### DashboardViewModel
```graphql
type DashboardViewModel {
  totalCustomers: Int!
  totalOrders: Int!
  totalRevenue: Decimal!
  pendingOrders: Int!
  completedOrders: Int!
  topProducts: [TopProductDto!]!
  recentOrders: [RecentOrderDto!]!
  topCustomers: [CustomerStatsDto!]!
  revenueByMonth: [MonthlyRevenueDto!]!
}
```

### TopProductDto
```graphql
type TopProductDto {
  productId: Int!
  productName: String!
  quantitySold: Int!
  revenue: Decimal!
}
```

### RecentOrderDto
```graphql
type RecentOrderDto {
  orderId: Int!
  orderDate: DateTime!
  customerName: String!
  totalAmount: Decimal!
  status: String!
}
```

### CustomerStatsDto
```graphql
type CustomerStatsDto {
  customerId: Int!
  customerName: String!
  orderCount: Int!
  totalSpent: Decimal!
}
```

### MonthlyRevenueDto
```graphql
type MonthlyRevenueDto {
  month: String!
  revenue: Decimal!
}
```

### BulkOperationResult
```graphql
type BulkOperationResult {
  successCount: Int!
  failureCount: Int!
  createdIds: [Int!]!
  errors: [String!]!
}
```

---

## 📚 Example Queries

### 1. Simple Customer List
```graphql
{
  customers {
    id
    name
    email
  }
}
```

### 2. Nested Order with Product and Category
```graphql
{
  order(id: 1) {
    id
    orderDate
    customer {
      name
    }
    items {
      quantity
      product {
        name
        category {
          name
        }
      }
      notes {
        content
      }
    }
  }
}
```
**Nesting depth**: 4 levels (Order → Items → Product → Category)

### 3. Dashboard with Aggregations
```graphql
{
  dashboard {
    totalOrders
    totalRevenue
    topProducts {
      productName
      revenue
    }
    topCustomers {
      customerName
      totalSpent
    }
  }
}
```

### 4. Multiple Resources in One Query
```graphql
{
  customer(id: 1) {
    name
    orders {
      id
      totalAmount
    }
  }
  products {
    id
    name
    price
  }
}
```
**Advantage**: Replaces 3 REST calls with 1 GraphQL query

### 5. Product with Category Details
```graphql
{
  product(id: 1) {
    name
    price
    category {
      name
      description
    }
  }
}
```

---

## 🔧 Example Mutations

### 1. Bulk Create Multiple Orders
```graphql
mutation {
  bulkCreateOrders(request: {
    orders: [
      {
        customerId: 1,
        status: "Pending",
        items: [
          {
            productId: 1,
            quantity: 5,
            discount: 10,
            notes: ["Priority shipping"]
          },
          {
            productId: 2,
            quantity: 3,
            discount: 0,
            notes: []
          }
        ]
      },
      {
        customerId: 2,
        status: "Pending",
        items: [
          {
            productId: 3,
            quantity: 2,
            discount: 15,
            notes: ["Gift wrap"]
          }
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

### 2. Bulk Update Order Status
```graphql
mutation {
  bulkUpdateOrders(request: {
    orders: [
      { id: 1, status: "Processing" },
      { id: 2, status: "Shipped" },
      { id: 3, status: "Completed" }
    ]
  }) {
    successCount
    failureCount
    errors
  }
}
```

---

## 💡 Tips & Best Practices

### Selective Field Fetching
Only request fields you need to reduce payload size:

```graphql
# Bad: Over-fetching
{
  products {
    id
    name
    description
    price
    stock
    categoryId
    category {
      id
      name
      description
    }
  }
}

# Good: Only what you need
{
  products {
    id
    name
    price
  }
}
```

### Aliases for Multiple Queries
```graphql
{
  customer1: customer(id: 1) { name }
  customer2: customer(id: 2) { name }
}
```

### Fragments for Reusability
```graphql
fragment CustomerFields on Customer {
  id
  name
  email
}

{
  customer(id: 1) {
    ...CustomerFields
  }
}
```

---

## 🔗 Related Documentation

- **[PERFORMANCE_TESTING.md](PERFORMANCE_TESTING.md)** - Performance testing with GraphQL
- **[SCRIPTS.md](SCRIPTS.md)** - Test scripts using GraphQL
- **[README.md](../README.md)** - Main project overview

---

## 📊 Performance Comparisons

See [PERFORMANCE_TESTING.md](PERFORMANCE_TESTING.md) for detailed comparisons showing:

- **Response Time**: GraphQL typically 60-80% faster for multi-resource queries
- **Payload Size**: GraphQL typically 90-96% smaller (no over-fetching)
- **HTTP Efficiency**: 67% fewer HTTP calls for multiple resource scenarios

---

*Last Updated: 2024*
*GraphQL Server: Hot Chocolate*

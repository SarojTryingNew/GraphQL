# User Guide - REST vs GraphQL Performance Comparison

**Complete guide for using all features, running tests, and interpreting results.**

---

## 📑 Table of Contents

1. [Interactive Front-End Demo](#interactive-front-end-demo)
2. [Performance Testing](#performance-testing)
3. [GraphQL API Reference](#graphql-api-reference)
4. [Test Scripts Reference](#test-scripts-reference)
5. [Understanding Results](#understanding-results)
6. [Best Practices](#best-practices)

---

## 🎨 Interactive Front-End Demo

### Accessing the Demo

```powershell
# Start API
.\start-api.ps1

# Open browser
http://localhost:5072/
```

### Features

#### 1. Side-by-Side Comparison

**REST API (Left Panel - Pink)**
- Bulk create orders
- Bulk delete orders
- Response viewer
- Performance stats

**GraphQL API (Right Panel - Blue)**
- Same operations as REST
- Shows single-request advantage
- Real-time performance metrics

#### 2. Bulk Create Orders

**Steps:**
1. Enter number of orders (1-100)
2. Click "Create Orders (REST)" or "Create Orders (GraphQL)"
3. View response with created order IDs
4. Compare performance metrics

**Example Response:**
```json
{
  "successCount": 10,
  "failureCount": 0,
  "errors": [],
  "createdIds": [101, 102, 103, 104, 105, 106, 107, 108, 109, 110],
  "deletedIds": []
}
```

#### 3. Bulk Delete Orders

**Steps:**
1. Copy order IDs from create response
2. Paste into delete field (comma-separated)
3. Click "Delete Orders (REST)" or "Delete Orders (GraphQL)"
4. View deleted IDs confirmation

**Example:**
```
Input: 101,102,103,104,105
```

#### 4. GraphQL Query Explorer

**Pre-Built Templates:**

**Simple Query** - List customers:
```graphql
query GetCustomers {
  customers {
    id
    name
    email
    city
  }
}
```

**Nested Query** - 4-level object graph:
```graphql
query GetOrderWithNesting {
  order(id: 1) {
    id
    orderDate
    customer { name email }
    items {
      quantity
      product {
        name
        category { name }
      }
      notes { content }
    }
  }
}
```

**Dashboard Query** - Aggregations:
```graphql
query GetDashboard {
  dashboard {
    totalCustomers
    totalOrders
    totalRevenue
    averageOrderValue
    topProducts { name price }
    recentOrders { id orderDate totalAmount }
  }
}
```

**Multiple Resources** - Combined query:
```graphql
query GetMultipleResources {
  customers(take: 5) { id name }
  products(take: 5) { id name price }
  orders(take: 5) { id orderDate }
}
```

#### 5. Performance Metrics

Each operation shows:
- **Response Time**: Milliseconds (ms)
- **Requests Made**: Number of HTTP calls
- **Data Size**: Payload size in bytes/KB

---

## 🧪 Performance Testing

### Test Execution Modes

#### 1. Quick Test (10 iterations)
**Duration:** ~30 seconds  
**Use Case:** Quick validation, smoke testing

```powershell
.\PerformanceTests\quick-test.ps1
```

**What it tests:**
- All 4 scenarios (a, b, c, d)
- Minimal iterations for fast feedback

#### 2. Standard Load Test (100 iterations)
**Duration:** ~5-10 minutes  
**Use Case:** Comprehensive performance benchmarking

```powershell
.\PerformanceTests\load-test.ps1

# Or with custom iterations
.\PerformanceTests\load-test.ps1 -Iterations 50
```

#### 3. Bulk Operations Test
**Duration:** Varies by preset  
**Use Case:** Testing bulk create/update/delete operations

```powershell
.\PerformanceTests\load-test-bulk.ps1 -Iterations 20 -OrdersPerBulk 15
```

**Presets Available:**
- **Quick Bulk Test**: 10 iterations × 10 orders = 100 orders
- **Standard Bulk Test**: 20 iterations × 20 orders = 400 orders
- **Heavy Load Bulk Test**: 50 iterations × 20 orders = 1,000 orders
- **Stress Test**: 100 iterations × 50 orders = 5,000 orders
- **Custom**: Specify your own values

#### 4. Individual Scenario Tests

**Nested Object Graph Test:**
```powershell
.\PerformanceTests\load-test-nested.ps1 -Iterations 100
```

**Dashboard Aggregation Test:**
```powershell
.\PerformanceTests\load-test-dashboard.ps1 -Iterations 100
```

**Multiple Dependent Calls Test:**
```powershell
.\PerformanceTests\load-test-multiple.ps1 -Iterations 100
```

#### 5. Interactive Launcher

**Best for:** Choosing tests interactively

```powershell
.\launch-tests.ps1
```

**Menu Options:**
```
1. Quick Test (10 iterations)
2. Standard Load Test (100 iterations)
3. Bulk Operations (submenu with presets)
4. Individual Scenarios (submenu)
5. Run All Tests
6. Exit
```

### Understanding Test Scenarios

#### Scenario (a): Bulk Operations
**What it tests:** Creating multiple orders in one request

**REST Implementation:**
```http
POST /api/orders/bulk
Content-Type: application/json

{
  "orders": [
    {
      "customerId": 1,
      "status": "Pending",
      "items": [
        { "productId": 1, "quantity": 2, "discount": 10, "notes": ["Express"] }
      ]
    }
  ]
}
```

**GraphQL Implementation:**
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

**Performance:** Both use **1 request** (equal)

#### Scenario (b): Nested Object Graphs
**What it tests:** Retrieving deeply nested data (4 levels)

**REST Implementation (4 requests):**
```http
GET /api/orders/1           # Get order
GET /api/customers/1        # Get customer
GET /api/products/1         # Get product
GET /api/categories/1       # Get category
```

**GraphQL Implementation (1 request):**
```graphql
query {
  order(id: 1) {
    id
    customer { name }
    items {
      product {
        name
        category { name }
      }
    }
  }
}
```

**Performance:** GraphQL **75% faster** (1 vs 4 requests)

#### Scenario (c): Dashboard Aggregation
**What it tests:** Complex aggregations and computed metrics

**REST Implementation (4 requests):**
```http
GET /api/dashboard/customers    # Customer count
GET /api/dashboard/orders       # Order stats
GET /api/dashboard/revenue      # Revenue calculations
GET /api/dashboard/top-products # Top products
```

**GraphQL Implementation (1 request):**
```graphql
query {
  dashboard {
    totalCustomers
    totalOrders
    totalRevenue
    averageOrderValue
    topProducts { name price }
  }
}
```

**Performance:** GraphQL **75% faster** (1 vs 4 requests)

#### Scenario (d): Multiple Dependent Calls
**What it tests:** Fetching multiple related resources

**REST Implementation (3 requests):**
```http
GET /api/customers?take=10
GET /api/orders?customerId=1
GET /api/products
```

**GraphQL Implementation (1 request):**
```graphql
query {
  customers(take: 10) { id name }
  orders(where: { customerId: 1 }) { id }
  products { id name }
}
```

**Performance:** GraphQL **67% faster** (1 vs 3 requests)

---

## 🔍 GraphQL API Reference

### Available Queries

#### Get All Customers
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

#### Get Customer by ID
```graphql
query {
  customer(id: 1) {
    id
    name
    email
    orders {
      id
      orderDate
      totalAmount
    }
  }
}
```

#### Get All Orders
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

#### Get Order with Nested Data
```graphql
query {
  order(id: 1) {
    id
    orderDate
    customer {
      name
      email
    }
    items {
      quantity
      unitPrice
      product {
        name
        category {
          name
        }
      }
      notes {
        content
        createdAt
      }
    }
  }
}
```

#### Get All Products
```graphql
query {
  products {
    id
    name
    description
    price
    stockQuantity
    category {
      name
    }
  }
}
```

#### Get Dashboard Data
```graphql
query {
  dashboard {
    totalCustomers
    totalOrders
    totalRevenue
    averageOrderValue
    topProducts {
      name
      price
      category { name }
    }
    recentOrders {
      id
      orderDate
      totalAmount
      customer { name }
    }
  }
}
```

### Available Mutations

#### Bulk Create Orders
```graphql
mutation BulkCreateOrders($request: BulkOrderCreateRequestInput!) {
  bulkCreateOrders(request: $request) {
    successCount
    failureCount
    errors
    createdIds
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
            "notes": ["Express delivery"]
          }
        ]
      }
    ]
  }
}
```

#### Bulk Update Orders
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
      { "id": 1, "status": "Completed" },
      { "id": 2, "status": "Shipped" }
    ]
  }
}
```

#### Bulk Delete Orders
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
    "orderIds": [101, 102, 103, 104, 105]
  }
}
```

### GraphQL Tips & Best Practices

#### 1. Request Only What You Need
```graphql
# Bad - Over-fetching
query {
  customers {
    id name email phone address createdAt
  }
}

# Good - Precise fields
query {
  customers {
    id
    name
    email
  }
}
```

#### 2. Use Aliases for Multiple Queries
```graphql
query {
  activeCustomers: customers(where: { isActive: true }) {
    id
    name
  }
  inactiveCustomers: customers(where: { isActive: false }) {
    id
    name
  }
}
```

#### 3. Use Fragments for Reusability
```graphql
fragment CustomerDetails on Customer {
  id
  name
  email
  phone
}

query {
  customer(id: 1) {
    ...CustomerDetails
    orders {
      id
      totalAmount
    }
  }
}
```

---

## 📜 Test Scripts Reference

### Command-Line Parameters

#### load-test.ps1
```powershell
# Basic usage
.\PerformanceTests\load-test.ps1

# Custom iterations
.\PerformanceTests\load-test.ps1 -Iterations 50

# Specific scenario
.\PerformanceTests\load-test.ps1 -Scenario "nested" -Iterations 100
```

#### load-test-bulk.ps1
```powershell
# Default (20 iterations × 10 orders)
.\PerformanceTests\load-test-bulk.ps1

# Custom configuration
.\PerformanceTests\load-test-bulk.ps1 -Iterations 50 -OrdersPerBulk 20
```

#### Individual Scenario Scripts
```powershell
# Nested objects
.\PerformanceTests\load-test-nested.ps1 -Iterations 100

# Dashboard aggregation
.\PerformanceTests\load-test-dashboard.ps1 -Iterations 100

# Multiple dependent calls
.\PerformanceTests\load-test-multiple.ps1 -Iterations 100
```

---

## 📊 Understanding Results

### HTML Performance Report

After tests complete, view at: `http://localhost:5072/api/metrics/report`

#### Executive Summary Section

Shows winner for each metric:
- **Response Time**: Lower is better
- **Throughput**: Higher is better
- **HTTP Calls**: Lower is better (fewer round-trips)
- **Payload Size**: Lower is better (less bandwidth)
- **Memory Usage**: Lower is better

**Tie Detection:** If metrics are within 0.01%, shows "REST & GraphQL (Tie)"

#### Detailed Metrics Section

**For Each API (REST & GraphQL):**
- Average response time
- P50, P95, P99 percentiles
- Min/Max response times
- Total requests
- Success rate
- Error count
- Average payload size
- Total bandwidth

#### Request/Response Examples

Shows actual HTTP requests and responses for transparency:
- REST API examples (headers, body, response)
- GraphQL query examples (query, variables, response)
- Helps understand what's being measured

### Key Metrics Explained

#### Response Time
- **What:** Time from request sent to response received
- **Units:** Milliseconds (ms)
- **Good:** < 100ms for simple queries, < 500ms for complex
- **GraphQL Advantage:** Single request reduces network latency

#### Throughput
- **What:** Requests processed per second
- **Units:** req/sec
- **Good:** > 100 req/sec for bulk operations
- **GraphQL Advantage:** Efficient processing of complex queries

#### HTTP Calls
- **What:** Number of HTTP requests needed
- **Units:** Count
- **Good:** Fewer is better (reduces network overhead)
- **GraphQL Advantage:** Combines multiple resources in one request

#### Payload Size
- **What:** Size of response data
- **Units:** Bytes/KB/MB
- **Good:** Smaller is better (saves bandwidth)
- **GraphQL Advantage:** Client specifies exact fields needed

### Interpreting Improvements

**Example Results:**
```
Scenario (b) - Nested Object Graph:
  REST:    4 requests, 250ms avg, 45KB payload
  GraphQL: 1 request, 180ms avg, 12KB payload
  
  Improvement:
  - 75% reduction in HTTP calls (4 → 1)
  - 28% faster response time
  - 73% smaller payload size
```

**What this means:**
- GraphQL eliminates 3 network round-trips
- Faster overall user experience
- Less data transferred (saves mobile data costs)

---

## 🎯 Best Practices

### For Performance Testing

1. **Run Standard Tests First**
   - Start with quick test to verify setup
   - Run standard load test for baseline
   - Run specific scenarios as needed

2. **Clean Environment**
   - Restart API before major test runs
   - Clear browser cache
   - Close unnecessary applications

3. **Consistent Conditions**
   - Same machine/network for comparisons
   - Run tests multiple times
   - Average results for accuracy

4. **Interpret Results Carefully**
   - Look at trends, not single data points
   - Consider P95/P99 for outliers
   - Check success rates (should be 100%)

### For GraphQL Queries

1. **Start Simple**
   - Use pre-built templates
   - Add complexity gradually
   - Test in Banana Cake Pop IDE first

2. **Optimize Queries**
   - Request only needed fields
   - Use pagination for large lists
   - Avoid deep nesting when not needed

3. **Use Variables**
   - Don't hardcode values in queries
   - Use variables for reusability
   - Easier testing with different inputs

### For Demonstrations

1. **Front-End Demo**
   - Great for stakeholders and business users
   - Show side-by-side comparison
   - Use bulk operations for impact

2. **GraphQL IDE**
   - Best for technical audiences
   - Show schema exploration
   - Demonstrate flexible queries

3. **Performance Reports**
   - Use for data-driven discussions
   - Highlight specific metrics
   - Show request/response examples

---

## 🆘 Troubleshooting Guide

### Common Issues

#### Tests Report Errors
**Symptom:** High failure count in results  
**Solution:**
- Check API is running (`.\start-api.ps1`)
- Verify port 5072 is available
- Check PowerShell execution policy

#### Slow Performance
**Symptom:** Tests take much longer than expected  
**Solution:**
- Check CPU/memory usage on machine
- Close other applications
- Reduce iteration count for quick tests

#### Front-End Not Loading
**Symptom:** Blank page at http://localhost:5072/  
**Solution:**
- Verify API started successfully
- Check browser console for errors
- Clear browser cache and reload

#### GraphQL Errors
**Symptom:** Query returns error messages  
**Solution:**
- Verify query syntax in Banana Cake Pop
- Check variable types match schema
- Ensure IDs exist in DataStore (1-5 for customers, 1-8 for products)

---

## 📚 Additional Resources

### Documentation Files
- `GETTING_STARTED.md` - Quick start guide
- `DEVELOPER_GUIDE.md` - Technical implementation details
- `README.md` (root) - Project overview

### Online Resources
- GraphQL IDE: http://localhost:5072/graphql
- Swagger API: http://localhost:5072/swagger
- Performance Report: http://localhost:5072/api/metrics/report

### Seeded Test Data
- **5 Customers**: IDs 1-5 (John Doe, Jane Smith, Bob Johnson, Alice Williams, Charlie Brown)
- **8 Products**: IDs 1-8 (Laptop, Smartphone, Headphones, T-Shirt, Jeans, Novel, Textbook, Garden Tools)
- **4 Categories**: IDs 1-4 (Electronics, Clothing, Books, Home & Garden)
- **20 Orders**: Pre-seeded sample orders with items and notes

---

**Last Updated:** December 2024  
**Version:** 2.0 (Consolidated)

# REST vs GraphQL - Performance Testing Documentation

## Overview

This document provides comprehensive details about the performance testing suite for comparing REST and GraphQL APIs. The test suite is designed to measure and compare Key Performance Indicators (KPIs) and Non-Functional Requirements (NFRs) across different scenarios.

---

## 🚀 Quick Start

### Prerequisites
- .NET 9 SDK installed
- PowerShell (comes with Windows)
- Running API server on `http://localhost:5072`

### Starting the API
```powershell
.\start-api.ps1
```

### Running Tests
```powershell
.\launch-tests.ps1
```

---

## 📊 Test Suite Structure

### Main Test Options

The `launch-tests.ps1` provides an interactive menu with the following options:

#### **1. Quick Test**
- **Duration**: ~30 seconds
- **Purpose**: Fast validation during development
- **Configuration**:
  - Standard Tests: 10 iterations per scenario
  - Bulk Operations: 10 iterations × 10 orders = 100 orders
- **Total API Calls**: ~60 REST + ~40 GraphQL + Bulk operations

#### **2. Standard Load Test**
- **Duration**: ~5-10 minutes
- **Purpose**: Comprehensive performance comparison
- **Configuration**:
  - Standard Tests: 100 iterations per scenario
  - Bulk Operations: 10 iterations × 50 orders = 500 orders
- **Total API Calls**: ~600 REST + ~400 GraphQL + Bulk operations

#### **3. Bulk Operations Test (with presets)**
- Quick Bulk: 10 iterations × 10 orders = 100 orders
- Standard Bulk: 50 iterations × 10 orders = 500 orders
- Heavy Load: 50 iterations × 20 orders = 1,000 orders
- Stress Test: 200 iterations × 50 orders = 10,000 orders
- Custom: User-defined configuration

#### **4. Nested Object Graph Test**
- **Default**: 100 iterations (configurable)
- **Purpose**: Test deep object traversal (4 levels)
- **Structure**: Order → Items → Product → Category

#### **5. Dashboard Aggregation Test**
- **Default**: 100 iterations (configurable)
- **Purpose**: Test complex aggregations and computed metrics
- **Metrics**: Total orders, revenue, top products, top customers

#### **6. Multiple Dependent Calls Test**
- **Default**: 100 iterations (configurable)
- **Purpose**: Compare multiple REST calls vs single GraphQL query
- **REST**: 3 HTTP calls per user (customer, orders, products)
- **GraphQL**: 1 HTTP call per user (combined query)

---

## 🎯 Test Scenarios Coverage

### Scenario (a): Bulk Operations
**Covered in**: Options 1, 2, 3

**What it tests**:
- Bulk CREATE operations (multiple orders in single request)
- Bulk UPDATE operations (updating multiple orders)

**REST Implementation**:
```
POST /api/orders/bulk
PUT /api/orders/bulk
```

**GraphQL Implementation**:
```graphql
mutation {
  bulkCreateOrders(request: $request) {
    successCount
    failureCount
    createdIds
  }
  bulkUpdateOrders(request: $request) {
    successCount
    failureCount
  }
}
```

**Key Metrics**:
- Orders per request efficiency
- Response time for batch operations
- Payload size comparison
- Success/failure handling

---

### Scenario (b): Nested Object Graph Operations
**Covered in**: Options 1, 2, 4

**What it tests**:
- Deep object traversal (parent → child → grandchild → great-grandchild)
- Complex relationship fetching

**Structure**:
```
Order (Level 1)
  ├── Customer (Level 2)
  └── Items (Level 2)
      ├── Product (Level 3)
      │   └── Category (Level 4)
      └── Notes (Level 3)
```

**REST Implementation**:
```
GET /api/orders/1/nested
```
Returns all related data in a single endpoint with nested objects.

**GraphQL Implementation**:
```graphql
{
  order(id: 1) {
    id
    orderDate
    customer { name }
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

**Key Metrics**:
- Response time for complex queries
- Payload size with nested data
- Over-fetching comparison (REST returns all fields vs GraphQL selective)

---

### Scenario (c): UI Aggregation / Dashboard
**Covered in**: Options 1, 2, 5

**What it tests**:
- Complex aggregations (SUM, COUNT, GROUP BY)
- Computed metrics
- Top-N queries

**REST Implementation**:
```
GET /api/dashboard
```

**GraphQL Implementation**:
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

**Aggregations Tested**:
- Total order count
- Total revenue calculation
- Top 5 products by revenue
- Top 5 customers by spending

**Key Metrics**:
- Response time for aggregations
- Payload efficiency
- Query optimization comparison

---

### Scenario (d): Multiple Dependent REST Calls
**Covered in**: Options 1, 2, 6

**What it tests**:
- Multiple sequential HTTP requests (N+1 problem)
- Network round-trip overhead
- GraphQL's advantage in fetching related resources

**REST Implementation** (3 sequential calls):
```
1. GET /api/customers/1        # Get customer
2. GET /api/customers/1/orders # Get customer's orders
3. GET /api/products           # Get product catalog
```
**Total**: 3 × Network Latency

**GraphQL Implementation** (1 combined call):
```graphql
{
  customer(id: 1) {
    name
    orders { id totalAmount }
  }
  products { id name price }
}
```
**Total**: 1 × Network Latency

**For 100 iterations**:
- REST: **300 HTTP calls** (100 users × 3 calls)
- GraphQL: **100 HTTP calls** (100 users × 1 call)
- **Result**: 67% reduction in HTTP requests

**Key Metrics**:
- Total HTTP call count
- Cumulative response time
- Bandwidth efficiency
- Server load reduction

---

## 📈 Key Performance Indicators (KPIs) Measured

### 1. Response Time
- **Average Response Time** (ms)
- **P50 (Median)** - 50th percentile
- **P95** - 95th percentile (worst case for 95% of requests)
- **P99** - 99th percentile (worst case for 99% of requests)
- **Min Response Time** (best case)
- **Max Response Time** (worst case)

### 2. Throughput
- **Requests Per Second** (req/s)
- Total requests processed
- Success rate percentage

### 3. Payload Efficiency
- **Average Payload Size** (bytes/KB/MB)
- Min/Max payload sizes
- **Total Bandwidth** consumed
- Comparison of over-fetching

### 4. Memory Usage
- **Average Memory per Request**
- Min/Max memory consumption
- Total memory used during test

### 5. Reliability
- **Success Rate** (percentage)
- Failed request count
- Error rate

### 6. HTTP Efficiency
- **Total HTTP Calls** (REST vs GraphQL)
- Calls per operation
- Network round-trip comparison

---

## 🎨 Test Execution Flow

### Example: Standard Load Test (Option 2)

```
1. User selects Option 2
2. Test configuration displayed:
   - Standard Tests: 100 iterations
   - Bulk Operations: 10 iterations × 50 orders
3. API health check
4. Metrics reset
5. Test scenario set to "Standard Load Test (100 iterations)"

PART 1: Standard Load Tests
├── Test 1: Simple GET (100 iterations)
│   ├── REST: GET /api/customers
│   └── GraphQL: { customers { id name email } }
├── Test 2: Nested Objects (100 iterations)
│   ├── REST: GET /api/orders/1/nested
│   └── GraphQL: { order(id:1) { ... nested fields ... } }
├── Test 3: Dashboard (100 iterations)
│   ├── REST: GET /api/dashboard
│   └── GraphQL: { dashboard { ... aggregations ... } }
└── Test 4: Multiple Calls (100 iterations)
    ├── REST: 3 calls × 100 = 300 HTTP requests
    └── GraphQL: 1 call × 100 = 100 HTTP requests

PART 2: Bulk Operations
├── REST Bulk Create (10 iterations)
├── GraphQL Bulk Create (10 iterations)
├── REST Bulk Update (5 iterations)
└── GraphQL Bulk Update (5 iterations)

6. Metrics collected automatically
7. HTML report generated
8. Browser opens with results
```

---

## 📊 HTML Report Contents

After each test, an interactive HTML report is generated at:
`http://localhost:5072/api/metrics/report`

### Report Sections:

#### 1. Header
- Test Scenario Name
- Test Start Time
- Report Generation Time

#### 2. Executive Summary
- Response Time Winner (with % improvement)
- Payload Size Winner (with % improvement)
- Memory Efficiency Winner (with % improvement)
- Throughput Winner (with % improvement)
- Reliability Winner

**Note**: If metrics are equal (within 0.01%), displays "REST & GraphQL (Tie)"

#### 3. Key Performance Indicators (KPIs)
Visual cards showing:
- Total Requests (REST vs GraphQL)
- Success Rate (REST vs GraphQL)
- Average Response Time (REST vs GraphQL)
- Throughput (REST vs GraphQL)

Winners highlighted in green.

#### 4. Response Time Analysis
Detailed table with:
- Average, P50, P95, P99, Min, Max
- Side-by-side REST vs GraphQL comparison
- Winner for each metric

#### 5. Bandwidth Efficiency
- Average Payload Size
- Min/Max Payload
- Total Bandwidth consumed
- Winner comparison

#### 6. Memory Usage
- Average Memory per Request
- Min/Max Memory
- Total Memory Used
- Winner comparison

#### 7. Endpoint Analysis
Breakdown by endpoint showing:
- Total calls per endpoint
- Average response time
- Average payload size
- Success rate

#### 8. Non-Functional Requirements Assessment
Comprehensive table mapping:
- **Performance NFR**: Latency, Throughput
- **Reliability NFR**: Success Rate
- **Efficiency NFR**: Bandwidth, Memory Usage

---

## 🔧 Configuration Details

### Bulk Operations Presets

| Preset | Iterations | Orders/Bulk | Total Orders | Duration | Use Case |
|--------|-----------|-------------|--------------|----------|----------|
| **Quick** | 10 | 10 | 100 | ~10s | Quick validation |
| **Standard** | 50 | 10 | 500 | ~30s | Regular testing |
| **Heavy Load** | 50 | 20 | 1,000 | ~1min | Performance testing |
| **Stress** | 200 | 50 | 10,000 | ~5-10min | Stress testing |
| **Custom** | User-defined | User-defined | Variable | Variable | Specific scenarios |

### Excluded from Metrics

The following endpoints are **NOT** included in KPI calculations:
- `/api/metrics/summary`
- `/api/metrics/comparison`
- `/api/metrics/rest`
- `/api/metrics/graphql`
- `/api/metrics/report`
- `/api/metrics/reset`
- `/api/metrics/scenario`

This ensures that checking metrics doesn't affect the test results.

---

## 🎯 Test Goals & Success Criteria

### Performance Goals
- ✅ Response time < 100ms for simple queries
- ✅ P95 response time < 500ms
- ✅ Success rate > 99%
- ✅ Throughput > 100 req/s (under load)

### Comparison Goals
- 📊 Identify which approach (REST vs GraphQL) is better for:
  - Simple queries
  - Complex nested queries
  - Aggregations
  - Bulk operations
  - Multiple resource fetching

### NFR Validation
- **Performance**: Acceptable latency and throughput
- **Reliability**: High success rate
- **Efficiency**: Optimal bandwidth and memory usage
- **Scalability**: Consistent performance under load

---

## 📁 Test File Structure

```
RestVsGraphQL/
├── launch-tests.ps1                  # Main test launcher (interactive menu)
├── start-api.ps1                     # API startup utility
│
└── PerformanceTests/
    ├── quick-test.ps1                # Quick test (10 iterations)
    ├── load-test.ps1                 # Standard/comprehensive test
    ├── load-test-bulk.ps1            # Bulk operations test
    ├── load-test-nested.ps1          # Nested object graph test
    ├── load-test-dashboard.ps1       # Dashboard aggregation test
    └── load-test-multiple.ps1        # Multiple dependent calls test
```

---

## 🔍 Understanding Test Results

### Response Time Analysis

**What to look for**:
- GraphQL typically faster for multiple resource queries (no N+1 problem)
- REST typically comparable for simple single-resource queries
- P95/P99 show worst-case scenarios (important for SLAs)

### Payload Size Comparison

**What to look for**:
- GraphQL typically smaller (no over-fetching)
- REST may send unnecessary fields
- Bandwidth savings with GraphQL especially visible in mobile scenarios

### HTTP Call Efficiency

**Key insight for Test 4 (Multiple Calls)**:
- REST: 300 calls (100 iterations × 3 endpoints)
- GraphQL: 100 calls (100 iterations × 1 query)
- **67% reduction in network round-trips with GraphQL**

### When to Use Each

**Use REST when**:
- Simple CRUD operations
- Caching is critical (HTTP caching)
- Public API with strict versioning
- File uploads/downloads

**Use GraphQL when**:
- Complex queries with multiple relations
- Mobile applications (bandwidth concerns)
- Frequent schema changes
- Need for selective field fetching
- Multiple resources in single call

---

## 🛠️ Troubleshooting

### API Not Running
```
Error: Cannot connect to API at http://localhost:5072
Solution: Run .\start-api.ps1 first
```

### Port Already in Use
```
Error: Address already in use
Solution: Change port in launchSettings.json or stop other service
```

### Test Fails Midway
```
Solution: 
1. Check API is still running
2. Reset metrics: POST /api/metrics/reset
3. Restart tests
```

### PowerShell Execution Policy Error
```
Error: Execution policy prevents script
Solution: Run PowerShell as Administrator
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

---

## 📞 Support & Contribution

### Running Individual Test Files

You can also run test files directly:

```powershell
# Quick test (10 iterations)
.\PerformanceTests\quick-test.ps1

# Standard load test with custom iterations
.\PerformanceTests\load-test.ps1 -Iterations 50

# Bulk operations with custom config
.\PerformanceTests\load-test-bulk.ps1 -Iterations 20 -OrdersPerBulk 15

# Individual scenario tests
.\PerformanceTests\load-test-nested.ps1 -Iterations 100
.\PerformanceTests\load-test-dashboard.ps1 -Iterations 100
.\PerformanceTests\load-test-multiple.ps1 -Iterations 100
```

### Custom Test Scenarios

To create custom tests, follow this template:

```powershell
# Set base URL
$BaseUrl = "http://localhost:5072"

# Reset metrics
Invoke-RestMethod -Uri "$BaseUrl/api/metrics/reset" -Method Post

# Set test scenario name
$scenarioBody = @{ scenarioName = "My Custom Test" } | ConvertTo-Json
Invoke-RestMethod -Uri "$BaseUrl/api/metrics/scenario" -Method Post -Body $scenarioBody -ContentType "application/json"

# Run your tests...

# View report
Start-Process "$BaseUrl/api/metrics/report"
```

---

## 📚 Additional Resources

### Related Files
- `RestVsGraphQL\Metrics\MetricsCollector.cs` - Metrics collection logic
- `RestVsGraphQL\Middleware\MetricsMiddleware.cs` - Automatic metric recording
- `RestVsGraphQL\Controllers\MetricsController.cs` - HTML report generation
- **[GRAPHQL_SCHEMA.md](GRAPHQL_SCHEMA.md)** - Complete GraphQL API reference

### API Endpoints
- REST APIs: `http://localhost:5072/api/*`
- GraphQL Endpoint: `http://localhost:5072/graphql`
- GraphQL IDE: Open `http://localhost:5072/graphql` in browser (Banana Cake Pop)
- Metrics Report: `http://localhost:5072/api/metrics/report`
- **GraphQL Schema Documentation**: [GRAPHQL_SCHEMA.md](GRAPHQL_SCHEMA.md)

---

## ✅ Summary

This comprehensive test suite provides:

✅ **4 Core Scenarios**: Bulk Operations, Nested Objects, Aggregations, Multiple Calls  
✅ **6 Test Execution Modes**: Quick, Standard, Individual Scenarios, Bulk Presets  
✅ **10+ KPIs**: Response time, throughput, payload size, memory, reliability  
✅ **Automated HTML Reports**: Visual comparison with winner highlighting  
✅ **Flexible Configuration**: Custom iterations and bulk sizes  
✅ **Production-Ready**: NFR validation and performance benchmarking  

**Result**: Complete performance comparison between REST and GraphQL APIs to make informed architectural decisions! 🚀

---

*Last Updated: 2024*
*Version: 1.0*

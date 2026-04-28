# Developer Guide - Technical Reference

**Technical documentation for developers working on or extending the REST vs GraphQL Performance Comparison project.**

---

## 📑 Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Project Structure](#project-structure)
3. [Core Components](#core-components)
4. [Performance Metrics Implementation](#performance-metrics-implementation)
5. [GraphQL Schema Design](#graphql-schema-design)
6. [Testing Infrastructure](#testing-infrastructure)
7. [Extending the Project](#extending-the-project)
8. [Performance Optimization](#performance-optimization)

---

## 🏗️ Architecture Overview

### Technology Stack

- **.NET 9** - Target framework (C# 13.0)
- **ASP.NET Core** - Web API framework
- **Hot Chocolate** - GraphQL server implementation
- **Entity Framework Core In-Memory** - Data persistence for testing
- **YamlDotNet** - YAML test file parsing
- **PowerShell 7+** - Test automation scripts

### High-Level Architecture

```
┌─────────────────────────────────────────────────────┐
│              Client Layer                           │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────┐ │
│  │  Web UI      │  │  PowerShell  │  │  Swagger  │ │
│  │  (wwwroot)   │  │  Scripts     │  │  UI       │ │
│  └──────────────┘  └──────────────┘  └───────────┘ │
└─────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────┐
│           Middleware Layer                          │
│  ┌──────────────────────────────────────────────┐   │
│  │  MetricsMiddleware (Request/Response Tracking)│  │
│  └──────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────┐
│              API Layer                              │
│  ┌────────────────┐         ┌──────────────────┐   │
│  │  REST          │         │  GraphQL         │   │
│  │  Controllers   │         │  Schema          │   │
│  │  - Orders      │         │  - Query         │   │
│  │  - Customers   │         │  - Mutation      │   │
│  │  - Products    │         │  - Types         │   │
│  │  - Dashboard   │         │                  │   │
│  └────────────────┘         └──────────────────┘   │
└─────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────┐
│            Service Layer                            │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────┐  │
│  │  DataStore   │  │  Metrics     │  │  YAML    │  │
│  │  (In-Memory) │  │  Collector   │  │  Runner  │  │
│  └──────────────┘  └──────────────┘  └──────────┘  │
└─────────────────────────────────────────────────────┘
```

### Design Patterns Used

1. **Repository Pattern** - DataStore acts as in-memory repository
2. **Middleware Pattern** - MetricsMiddleware for request/response interception
3. **DTO Pattern** - Separate data transfer objects for API contracts
4. **Singleton Pattern** - DataStore and MetricsCollector as singletons
5. **Factory Pattern** - Test data generation in test scripts

---

## 📂 Project Structure

```
RestVsGraphQL/
│
├── README.md                          # Project overview
├── start-api.ps1                      # Quick API startup
├── launch-tests.ps1                   # Interactive test launcher
│
├── Documentation/                     # 📚 Documentation
│   ├── GETTING_STARTED.md             # Quick start guide
│   ├── USER_GUIDE.md                  # Complete usage guide
│   └── DEVELOPER_GUIDE.md             # This file
│
├── PerformanceTests/                  # 🧪 Test automation
│   ├── quick-test.ps1                 # Quick validation
│   ├── load-test.ps1                  # Standard/custom load tests
│   ├── load-test-bulk.ps1             # Bulk operations test
│   ├── load-test-nested.ps1           # Nested object graph test
│   ├── load-test-dashboard.ps1        # Dashboard aggregation test
│   └── load-test-multiple.ps1         # Multiple dependent calls test
│
└── RestVsGraphQL/                     # 🎯 Main application
    │
    ├── Program.cs                     # Application entry point
    │
    ├── wwwroot/                       # Static files
    │   └── index.html                 # Front-end demo
    │
    ├── Controllers/                   # REST API endpoints
    │   ├── OrdersController.cs        # Order CRUD + bulk operations
    │   ├── CustomersController.cs     # Customer operations
    │   ├── ProductsController.cs      # Product operations
    │   ├── DashboardController.cs     # Dashboard aggregations
    │   └── MetricsController.cs       # Metrics and reports
    │
    ├── GraphQL/                       # GraphQL schema
    │   ├── Query.cs                   # Query root type
    │   └── Mutation.cs                # Mutation root type
    │
    ├── Models/                        # Domain models
    │   ├── Customer.cs
    │   ├── Order.cs
    │   ├── OrderItem.cs
    │   ├── OrderItemNote.cs
    │   ├── Product.cs
    │   └── Category.cs
    │
    ├── DTOs/                          # Data transfer objects
    │   ├── BulkOperationDtos.cs       # Bulk operation requests
    │   └── DashboardViewModel.cs      # Dashboard aggregations
    │
    ├── Services/                      # Business services
    │   └── DataStore.cs               # In-memory data storage
    │
    ├── Metrics/                       # Performance tracking
    │   └── MetricsCollector.cs        # Metrics collection & aggregation
    │
    ├── Middleware/                    # ASP.NET middleware
    │   └── MetricsMiddleware.cs       # Request/response interception
    │
    └── Testing/                       # Test utilities
        └── YamlTestRunner.cs          # YAML test execution
```

---

## 🔧 Core Components

### 1. DataStore.cs

**Purpose:** In-memory data storage with seeded test data

**Key Methods:**
```csharp
public class DataStore
{
    // Collections
    public List<Customer> Customers { get; }
    public List<Order> Orders { get; }
    public List<Product> Products { get; }
    public List<Category> Categories { get; }
    public List<OrderItem> OrderItems { get; }
    public List<OrderItemNote> OrderItemNotes { get; }
    
    // ID generators
    public int GetNextCustomerId();
    public int GetNextOrderId();
    public int GetNextProductId();
    // ... etc
    
    // Data seeding
    private void SeedData();
}
```

**Seeded Data:**
- 5 Customers (IDs 1-5)
- 8 Products (IDs 1-8)
- 4 Categories (IDs 1-4)
- 20 Orders with items and notes

**Design Decisions:**
- In-memory for fast testing (no database required)
- Thread-safe (singleton in DI container)
- Automatic ID generation (simulates database auto-increment)

### 2. MetricsMiddleware.cs

**Purpose:** Intercept all HTTP requests/responses to collect performance metrics

**Implementation:**
```csharp
public class MetricsMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip metrics endpoints (avoid self-monitoring)
        if (context.Request.Path.StartsWithSegments("/api/metrics"))
        {
            await _next(context);
            return;
        }
        
        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;
        
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        
        await _next(context);
        
        stopwatch.Stop();
        
        // Collect metrics
        var metric = new RequestMetric
        {
            Method = context.Request.Method,
            Path = context.Request.Path,
            ResponseTime = stopwatch.ElapsedMilliseconds,
            StatusCode = context.Response.StatusCode,
            RequestSize = context.Request.ContentLength ?? 0,
            ResponseSize = responseBody.Length,
            Timestamp = DateTime.UtcNow
        };
        
        _metricsCollector.RecordMetric(metric);
        
        // Copy response back
        await responseBody.CopyToAsync(originalBodyStream);
    }
}
```

**Key Features:**
- Two-layer protection to exclude /api/metrics endpoints
- Captures request/response sizes
- Measures response time with high precision
- Non-blocking (doesn't affect API performance)

### 3. MetricsCollector.cs

**Purpose:** Collect, aggregate, and report performance metrics

**Key Methods:**
```csharp
public class MetricsCollector
{
    // Record individual metric
    public void RecordMetric(RequestMetric metric);
    
    // Set current test scenario name
    public void SetTestScenario(string scenarioName);
    
    // Get current test scenario
    public string GetCurrentTestScenario();
    
    // Get aggregated summary
    public MetricsSummary GetSummary();
    
    // Get comparison report (REST vs GraphQL)
    public ComparisonReport GetComparisonReport();
    
    // Clear all metrics
    public void ClearMetrics();
}
```

**Metric Aggregations:**
- Average, Min, Max response times
- P50, P95, P99 percentiles
- Total requests, success rate, error rate
- Total bandwidth (request + response sizes)
- Memory usage tracking

### 4. GraphQL Schema (Query.cs & Mutation.cs)

**Query.cs - Read Operations:**
```csharp
public class Query
{
    // Get all customers (with optional filtering)
    public IQueryable<Customer> GetCustomers([Service] DataStore dataStore);
    
    // Get single customer by ID
    public Customer? GetCustomer(int id, [Service] DataStore dataStore);
    
    // Get all orders
    public IQueryable<Order> GetOrders([Service] DataStore dataStore);
    
    // Get single order (with nested data loading)
    public Order? GetOrder(int id, [Service] DataStore dataStore);
    
    // Get all products
    public IQueryable<Product> GetProducts([Service] DataStore dataStore);
    
    // Get dashboard aggregations
    public DashboardViewModel GetDashboard([Service] DataStore dataStore);
}
```

**Mutation.cs - Write Operations:**
```csharp
public class Mutation
{
    // Bulk create multiple orders
    public BulkOperationResult BulkCreateOrders(
        BulkOrderCreateRequest request,
        [Service] DataStore dataStore);
    
    // Bulk update orders
    public BulkOperationResult BulkUpdateOrders(
        BulkOrderUpdateRequest request,
        [Service] DataStore dataStore);
    
    // Bulk delete orders
    public BulkOperationResult BulkDeleteOrders(
        BulkOrderDeleteRequest request,
        [Service] DataStore dataStore);
}
```

**Schema Features:**
- Lazy loading of nested relationships
- Automatic projection (HotChocolate handles field selection)
- Input validation
- Comprehensive error handling

---

## 📊 Performance Metrics Implementation

### Metric Collection Flow

```
1. Request arrives → MetricsMiddleware intercepts
2. Start stopwatch, capture request details
3. Execute API logic (REST or GraphQL)
4. Capture response details, stop stopwatch
5. Create RequestMetric object
6. Send to MetricsCollector
7. Continue response to client
```

### Metrics Exclusion Strategy

**Problem:** Metrics endpoints being included in performance measurements

**Solution:** Two-layer protection
1. **Middleware Layer**: Skip /api/metrics/* endpoints entirely
2. **Collector Layer**: Filter out metrics paths before aggregation

```csharp
// Layer 1: Middleware
if (context.Request.Path.StartsWithSegments("/api/metrics"))
{
    await _next(context);
    return; // Skip metrics collection
}

// Layer 2: Collector
public MetricsSummary GetSummary()
{
    var filtered = _metrics
        .Where(m => !m.Path.StartsWith("/api/metrics"))
        .ToList();
    // ... aggregate filtered metrics
}
```

### Test Scenario Tracking

**Purpose:** Identify which test is currently running

**Implementation:**
```csharp
private string _currentTestScenario = "unknown";

public void SetTestScenario(string scenarioName)
{
    _currentTestScenario = scenarioName;
}

public ComparisonReport GetComparisonReport()
{
    return new ComparisonReport
    {
        TestScenario = _currentTestScenario,
        TestStartTime = _testStartTime,
        // ... other metrics
    };
}
```

**Usage in Test Scripts:**
```powershell
# Set scenario before tests
Invoke-RestMethod -Uri "$apiBase/api/metrics/scenario" `
    -Method Post `
    -Body '"Scenario (a) - Bulk Operations"' `
    -ContentType "application/json"

# Run tests
# ...

# Get report (includes scenario name)
Start-Process "$apiBase/api/metrics/report"
```

### HTML Report Generation

**MetricsController.cs:**
```csharp
[HttpGet("report")]
public ContentResult GetHtmlReport()
{
    var report = _metricsCollector.GetComparisonReport();
    var html = GenerateHtmlReport(report);
    return Content(html, "text/html");
}

private string GenerateHtmlReport(ComparisonReport report)
{
    // Executive Summary
    var winner = DetermineWinner(report.RestMetrics, report.GraphQLMetrics);
    
    // Detailed tables with all metrics
    // Request/response examples
    // Performance charts
    
    return htmlBuilder.ToString();
}
```

**Tie Detection Logic:**
```csharp
private string DetermineWinner(decimal restValue, decimal graphqlValue)
{
    var improvement = Math.Abs((restValue - graphqlValue) / restValue * 100);
    
    if (improvement < 0.01m)
        return "REST & GraphQL (Tie)";
    
    return restValue < graphqlValue ? "REST" : "GraphQL";
}
```

---

## 🎨 GraphQL Schema Design

### Type Definitions

**Customer Type:**
```csharp
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Order Type (with relationships):**
```csharp
public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }        // Navigation property
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
    public List<OrderItem> Items { get; set; }     // Collection navigation
}
```

### Resolver Implementation

**Simple Resolver (no lazy loading):**
```csharp
public IQueryable<Customer> GetCustomers([Service] DataStore dataStore)
{
    return dataStore.Customers.AsQueryable();
}
```

**Complex Resolver (with lazy loading):**
```csharp
public Order? GetOrder(int id, [Service] DataStore dataStore)
{
    var order = dataStore.Orders.FirstOrDefault(o => o.Id == id);
    if (order == null) return null;
    
    // Lazy load customer
    order.Customer = dataStore.Customers
        .FirstOrDefault(c => c.Id == order.CustomerId);
    
    // Lazy load items
    order.Items = dataStore.OrderItems
        .Where(oi => oi.OrderId == order.Id)
        .ToList();
    
    // Lazy load nested product and category
    foreach (var item in order.Items)
    {
        item.Product = dataStore.Products
            .FirstOrDefault(p => p.Id == item.ProductId);
        
        if (item.Product != null)
        {
            item.Product.Category = dataStore.Categories
                .FirstOrDefault(c => c.Id == item.Product.CategoryId);
        }
        
        item.Notes = dataStore.OrderItemNotes
            .Where(n => n.OrderItemId == item.Id)
            .ToList();
    }
    
    return order;
}
```

### Input Types

**Bulk Create Request:**
```csharp
public class BulkOrderCreateRequest
{
    public List<OrderCreateDto> Orders { get; set; } = new();
}

public class OrderCreateDto
{
    public int CustomerId { get; set; }
    public string Status { get; set; } = "Pending";
    public List<OrderItemCreateDto> Items { get; set; } = new();
}

public class OrderItemCreateDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Discount { get; set; }
    public List<string> Notes { get; set; } = new();
}
```

### Error Handling

```csharp
public BulkOperationResult BulkCreateOrders(
    BulkOrderCreateRequest request,
    [Service] DataStore dataStore)
{
    var result = new BulkOperationResult();
    
    foreach (var orderDto in request.Orders)
    {
        try
        {
            // Validate customer exists
            var customer = dataStore.Customers
                .FirstOrDefault(c => c.Id == orderDto.CustomerId);
            
            if (customer == null)
            {
                result.FailureCount++;
                result.Errors.Add($"Customer {orderDto.CustomerId} not found");
                continue; // Skip this order
            }
            
            // Create order...
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
```

---

## 🧪 Testing Infrastructure

### YAML Test Runner

**Purpose:** Execute declarative API tests from YAML files

**YamlTestRunner.cs:**
```csharp
public class YamlTestRunner
{
    public async Task<TestResults> RunTestsFromFile(string filePath)
    {
        // Parse YAML file
        var yaml = File.ReadAllText(filePath);
        var deserializer = new DeserializerBuilder().Build();
        var testSuite = deserializer.Deserialize<TestSuite>(yaml);
        
        var results = new TestResults();
        
        // Execute each test
        foreach (var test in testSuite.Tests)
        {
            var result = await ExecuteTest(test);
            results.Add(result);
        }
        
        return results;
    }
    
    private async Task<TestResult> ExecuteTest(TestCase test)
    {
        // Send HTTP request
        // Validate response
        // Check assertions
        // Return result
    }
}
```

**YAML Test Format:**
```yaml
name: "GraphQL Bulk Create Test"
tests:
  - name: "Create 5 orders"
    type: "graphql"
    endpoint: "/graphql"
    query: |
      mutation BulkCreateOrders($request: BulkOrderCreateRequestInput!) {
        bulkCreateOrders(request: $request) {
          successCount
          failureCount
        }
      }
    variables:
      request:
        orders:
          - customerId: 1
            status: "Pending"
    assertions:
      - field: "data.bulkCreateOrders.successCount"
        expected: 5
```

### PowerShell Test Scripts

**Common Pattern:**
```powershell
param(
    [int]$Iterations = 100,
    [string]$ApiBase = "http://localhost:5072"
)

# Clear previous metrics
Invoke-RestMethod -Uri "$ApiBase/api/metrics/clear" -Method Post

# Set test scenario
Invoke-RestMethod -Uri "$ApiBase/api/metrics/scenario" `
    -Method Post `
    -Body '"Scenario (a) - Bulk Operations"' `
    -ContentType "application/json"

# Run test iterations
for ($i = 1; $i -le $Iterations; $i++) {
    Write-Progress -Activity "Running tests" `
        -Status "$i of $Iterations" `
        -PercentComplete (($i / $Iterations) * 100)
    
    # REST API call
    $restResponse = Invoke-RestMethod `
        -Uri "$ApiBase/api/orders/bulk" `
        -Method Post `
        -Body $jsonBody `
        -ContentType "application/json"
    
    # GraphQL call
    $graphqlResponse = Invoke-RestMethod `
        -Uri "$ApiBase/graphql" `
        -Method Post `
        -Body $graphqlBody `
        -ContentType "application/json"
}

# Open report
Start-Process "$ApiBase/api/metrics/report"
```

---

## 🚀 Extending the Project

### Adding New GraphQL Query

**1. Add to Query.cs:**
```csharp
public List<Customer> GetTopCustomers(
    int count,
    [Service] DataStore dataStore)
{
    return dataStore.Orders
        .GroupBy(o => o.CustomerId)
        .OrderByDescending(g => g.Sum(o => o.TotalAmount))
        .Take(count)
        .Select(g => dataStore.Customers.First(c => c.Id == g.Key))
        .ToList();
}
```

**2. Test in Banana Cake Pop:**
```graphql
query {
  topCustomers(count: 10) {
    id
    name
    email
  }
}
```

### Adding New REST Endpoint

**1. Add to Controller:**
```csharp
[HttpGet("top-customers")]
public ActionResult<List<Customer>> GetTopCustomers([FromQuery] int count = 10)
{
    var topCustomers = _dataStore.Orders
        .GroupBy(o => o.CustomerId)
        .OrderByDescending(g => g.Sum(o => o.TotalAmount))
        .Take(count)
        .Select(g => _dataStore.Customers.First(c => c.Id == g.Key))
        .ToList();
    
    return Ok(topCustomers);
}
```

**2. Test in Swagger:**
```
GET /api/orders/top-customers?count=10
```

### Adding New Test Script

**1. Create new PowerShell file:**
```powershell
# PerformanceTests/load-test-top-customers.ps1
param(
    [int]$Iterations = 100,
    [int]$CustomerCount = 10
)

$apiBase = "http://localhost:5072"

# Clear metrics
Invoke-RestMethod -Uri "$apiBase/api/metrics/clear" -Method Post

# Set scenario
Invoke-RestMethod -Uri "$apiBase/api/metrics/scenario" `
    -Method Post `
    -Body '"Top Customers Test"' `
    -ContentType "application/json"

# Run tests
for ($i = 1; $i -le $Iterations; $i++) {
    # REST
    Invoke-RestMethod -Uri "$apiBase/api/orders/top-customers?count=$CustomerCount"
    
    # GraphQL
    $query = @{
        query = "query { topCustomers(count: $CustomerCount) { id name } }"
    }
    Invoke-RestMethod -Uri "$apiBase/graphql" `
        -Method Post `
        -Body ($query | ConvertTo-Json) `
        -ContentType "application/json"
}

# Show report
Start-Process "$apiBase/api/metrics/report"
```

### Adding Front-End Feature

**Example: Add new query template**

Edit `wwwroot/index.html`:
```javascript
function loadQuery(type) {
    const queries = {
        simple: `...existing...`,
        nested: `...existing...`,
        topCustomers: `query GetTopCustomers {
  topCustomers(count: 10) {
    id
    name
    email
    totalSpent
  }
}`
    };
    
    document.getElementById('customQuery').value = queries[type];
}
```

Add button:
```html
<button class="graphql-btn" onclick="loadQuery('topCustomers')">Top Customers</button>
```

---

## ⚡ Performance Optimization

### Best Practices

#### 1. Minimize Data Loading
```csharp
// Bad - Loads all data
public IEnumerable<Customer> GetCustomers([Service] DataStore dataStore)
{
    return dataStore.Customers.ToList(); // Materializes entire list
}

// Good - Uses IQueryable for deferred execution
public IQueryable<Customer> GetCustomers([Service] DataStore dataStore)
{
    return dataStore.Customers.AsQueryable(); // HotChocolate handles projection
}
```

#### 2. Use Pagination
```csharp
[UsePaging]
public IQueryable<Order> GetOrders([Service] DataStore dataStore)
{
    return dataStore.Orders.AsQueryable();
}

// Query with pagination:
// query { orders(first: 10) { nodes { id } } }
```

#### 3. Optimize Nested Loading
```csharp
// Only load nested data if requested by GraphQL query
public Order? GetOrder(int id, [Service] DataStore dataStore)
{
    var order = dataStore.Orders.FirstOrDefault(o => o.Id == id);
    if (order == null) return null;
    
    // HotChocolate will only resolve these if fields are requested
    return order;
}
```

#### 4. Cache Frequently Accessed Data
```csharp
private static readonly ConcurrentDictionary<int, Customer> _customerCache = new();

public Customer? GetCustomer(int id, [Service] DataStore dataStore)
{
    return _customerCache.GetOrAdd(id, 
        id => dataStore.Customers.FirstOrDefault(c => c.Id == id));
}
```

### Monitoring Performance

**Built-in Metrics:**
- Response time tracking (automatic via middleware)
- Payload size measurement
- Request count
- Success/failure rates

**Adding Custom Metrics:**
```csharp
public class CustomMetric
{
    public string Name { get; set; }
    public long Value { get; set; }
    public DateTime Timestamp { get; set; }
}

// In MetricsCollector:
public void RecordCustomMetric(CustomMetric metric)
{
    _customMetrics.Add(metric);
}
```

---

## 🔐 Security Considerations

### Input Validation

```csharp
public BulkOperationResult BulkCreateOrders(
    BulkOrderCreateRequest request,
    [Service] DataStore dataStore)
{
    // Validate input
    if (request?.Orders == null || !request.Orders.Any())
    {
        throw new ArgumentException("Orders list cannot be empty");
    }
    
    if (request.Orders.Count > 100)
    {
        throw new ArgumentException("Cannot create more than 100 orders at once");
    }
    
    // Process...
}
```

### Rate Limiting (Future Enhancement)

```csharp
// Add to Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        context => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

---

## 📚 Additional Resources

### Official Documentation
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Hot Chocolate GraphQL](https://chillicream.com/docs/hotchocolate)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)

### GraphQL Best Practices
- [GraphQL.org](https://graphql.org/learn/best-practices/)
- [Apollo GraphQL Guide](https://www.apollographql.com/docs/)

### Performance Testing
- [k6 Load Testing](https://k6.io/) - Alternative to PowerShell scripts
- [BenchmarkDotNet](https://benchmarkdotnet.org/) - .NET benchmarking library

---

**Last Updated:** December 2024  
**Version:** 2.0 (Consolidated)  
**Maintainer:** GraphQL POC Team

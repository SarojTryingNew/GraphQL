# YAML-Based Testing Guide

## Overview

This POC uses **declarative YAML testing** to validate both REST and GraphQL APIs side-by-side. This approach enables:

✅ **Non-technical readable tests**  
✅ **Easy maintenance** (no code changes for new tests)  
✅ **Consistent validation** across both API types  
✅ **Automated regression testing**  

---

## Test Suite Structure

### YAML Format

```yaml
name: "Test Suite Name"
description: "Brief description of what these tests validate"

tests:
  - name: "Individual Test Name"
    type: "REST" | "GraphQL"
    method: "GET" | "POST" | "PUT" | "DELETE"  # REST only
    endpoint: "/api/endpoint"                   # REST only
    query: "GraphQL query string"               # GraphQL only
    body: { ... }                               # Optional request body
    variables: { ... }                          # GraphQL variables
    assertions:
      statusCode: 200
      contains: "expectedString"
```

---

## Example Test Suites

### 1. REST API Tests

```yaml
# TestSuites/rest-tests.yaml
name: "REST API Tests"
description: "Comprehensive REST endpoint validation"

tests:
  - name: "Get All Customers"
    type: "REST"
    method: "GET"
    endpoint: "/api/customers"
    assertions:
      statusCode: 200
      contains: "customers"

  - name: "Get Single Customer"
    type: "REST"
    method: "GET"
    endpoint: "/api/customers/1"
    assertions:
      statusCode: 200
      contains: "name"

  - name: "Get Non-Existent Customer"
    type: "REST"
    method: "GET"
    endpoint: "/api/customers/99999"
    assertions:
      statusCode: 404

  - name: "Bulk Create Orders"
    type: "REST"
    method: "POST"
    endpoint: "/api/orders/bulk"
    body:
      orders:
        - customerId: 1
          status: "Pending"
          items:
            - productId: 5
              quantity: 10
              discount: 5
              notes: ["Test order"]
    assertions:
      statusCode: 200
      contains: "successCount"

  - name: "Bulk Delete Orders"
    type: "REST"
    method: "DELETE"
    endpoint: "/api/orders/bulk"
    body:
      orderIds: [10, 11, 12]
    assertions:
      statusCode: 200
      contains: "deletedIds"
```

---

### 2. GraphQL API Tests

```yaml
# TestSuites/graphql-tests.yaml
name: "GraphQL API Tests"
description: "Comprehensive GraphQL query and mutation validation"

tests:
  - name: "Get All Customers"
    type: "GraphQL"
    query: |
      {
        customers {
          id
          name
          email
          phone
        }
      }
    assertions:
      statusCode: 200
      contains: "customers"

  - name: "Get Customer with Orders"
    type: "GraphQL"
    query: |
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
    assertions:
      statusCode: 200
      contains: "customer"

  - name: "Get Order with 4-Level Nesting"
    type: "GraphQL"
    query: |
      {
        order(id: 1) {
          id
          orderDate
          totalAmount
          customer {
            name
            email
          }
          items {
            quantity
            unitPrice
            discount
            product {
              name
              price
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
    assertions:
      statusCode: 200
      contains: "order"

  - name: "Get Dashboard Data"
    type: "GraphQL"
    query: |
      {
        dashboard {
          totalCustomers
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
    assertions:
      statusCode: 200
      contains: "dashboard"

  - name: "Create Single Order"
    type: "GraphQL"
    query: |
      mutation {
        createOrder(orderDto: {
          customerId: 1
          status: "Pending"
          items: [
            {
              productId: 5
              quantity: 10
              discount: 5
              notes: ["GraphQL test order"]
            }
          ]
        }) {
          id
          orderDate
          totalAmount
          items {
            id
            quantity
            product {
              name
            }
          }
        }
      }
    assertions:
      statusCode: 200
      contains: "createOrder"

  - name: "Bulk Create Orders"
    type: "GraphQL"
    query: |
      mutation {
        bulkCreateOrders(request: {
          orders: [
            {
              customerId: 1
              status: "Pending"
              items: [
                { productId: 5, quantity: 10, discount: 5, notes: ["Bulk test"] }
              ]
            },
            {
              customerId: 2
              status: "Processing"
              items: [
                { productId: 3, quantity: 2, discount: 0, notes: [] }
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
    assertions:
      statusCode: 200
      contains: "bulkCreateOrders"

  - name: "Bulk Delete Orders"
    type: "GraphQL"
    query: |
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
    assertions:
      statusCode: 200
      contains: "bulkDeleteOrders"
```

---

### 3. Comparison Tests (REST vs GraphQL)

```yaml
# TestSuites/comparison-tests.yaml
name: "REST vs GraphQL Comparison Tests"
description: "Side-by-side validation of identical operations"

tests:
  # Simple retrieval
  - name: "REST - Get Customer"
    type: "REST"
    method: "GET"
    endpoint: "/api/customers/1"
    assertions:
      statusCode: 200

  - name: "GraphQL - Get Customer"
    type: "GraphQL"
    query: |
      {
        customer(id: 1) {
          id
          name
          email
          phone
          createdAt
        }
      }
    assertions:
      statusCode: 200

  # Nested data
  - name: "REST - Order with Nested Data"
    type: "REST"
    method: "GET"
    endpoint: "/api/orders/1/nested"
    assertions:
      statusCode: 200

  - name: "GraphQL - Order with Nested Data"
    type: "GraphQL"
    query: |
      {
        order(id: 1) {
          id
          orderDate
          status
          totalAmount
          customer { id name email }
          items {
            id
            quantity
            unitPrice
            product { id name category { id name } }
            notes { id content }
          }
        }
      }
    assertions:
      statusCode: 200

  # Dashboard
  - name: "REST - Dashboard"
    type: "REST"
    method: "GET"
    endpoint: "/api/dashboard"
    assertions:
      statusCode: 200

  - name: "GraphQL - Dashboard"
    type: "GraphQL"
    query: |
      {
        dashboard {
          totalCustomers
          totalOrders
          totalRevenue
          topProducts { productName revenue }
          topCustomers { customerName totalSpent }
        }
      }
    assertions:
      statusCode: 200

  # Bulk operations
  - name: "REST - Bulk Create Orders"
    type: "REST"
    method: "POST"
    endpoint: "/api/orders/bulk"
    body:
      orders:
        - customerId: 1
          status: "Pending"
          items:
            - productId: 1
              quantity: 2
              discount: 5
              notes: ["Comparison test"]
    assertions:
      statusCode: 200

  - name: "GraphQL - Bulk Create Orders"
    type: "GraphQL"
    query: |
      mutation {
        bulkCreateOrders(request: {
          orders: [
            {
              customerId: 1
              status: "Pending"
              items: [
                { productId: 1, quantity: 2, discount: 5, notes: ["Comparison test"] }
              ]
            }
          ]
        }) {
          successCount
          failureCount
          createdIds
        }
      }
    assertions:
      statusCode: 200

  # Multiple dependent calls
  - name: "REST - Customer Info (Call 1 of 3)"
    type: "REST"
    method: "GET"
    endpoint: "/api/customers/1"
    assertions:
      statusCode: 200

  - name: "REST - Customer Orders (Call 2 of 3)"
    type: "REST"
    method: "GET"
    endpoint: "/api/customers/1/orders"
    assertions:
      statusCode: 200

  - name: "REST - All Products (Call 3 of 3)"
    type: "REST"
    method: "GET"
    endpoint: "/api/products"
    assertions:
      statusCode: 200

  - name: "GraphQL - All Data in Single Call"
    type: "GraphQL"
    query: |
      {
        customer(id: 1) {
          id
          name
          email
          orders { id orderDate totalAmount status }
        }
        products { id name price stockQuantity }
      }
    assertions:
      statusCode: 200
```

---

## Test Runner Implementation

### C# Test Runner

```csharp
// Testing/YamlTestRunner.cs
using System.Net.Http.Json;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RestVsGraphQL.Testing;

public class YamlTestRunner
{
    private readonly HttpClient _httpClient;
    private readonly IDeserializer _yamlDeserializer;

    public YamlTestRunner(string baseUrl)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    public async Task<TestResults> RunTestsFromFile(string yamlFilePath)
    {
        var yamlContent = await File.ReadAllTextAsync(yamlFilePath);
        var testSuite = _yamlDeserializer.Deserialize<TestSuite>(yamlContent);

        var results = new TestResults { TestSuiteName = testSuite.Name };

        Console.WriteLine($"Running: {testSuite.Name}");
        Console.WriteLine($"Description: {testSuite.Description}");
        Console.WriteLine($"Tests: {testSuite.Tests.Count}\n");

        foreach (var test in testSuite.Tests)
        {
            var result = await ExecuteTest(test);
            results.Results.Add(result);

            // Print result
            var status = result.Success ? "✅ PASS" : "❌ FAIL";
            Console.WriteLine($"{status} - {result.TestName} ({result.Duration.TotalMilliseconds}ms)");
            
            if (!result.Success)
            {
                Console.WriteLine($"   Error: {result.ErrorMessage}");
            }
        }

        Console.WriteLine($"\nSummary: {results.PassCount}/{results.Results.Count} passed");
        return results;
    }

    private async Task<TestResult> ExecuteTest(ApiTest test)
    {
        var result = new TestResult
        {
            TestName = test.Name,
            ApiType = test.Type,
            StartTime = DateTime.UtcNow
        };

        try
        {
            HttpResponseMessage response;

            if (test.Type.Equals("REST", StringComparison.OrdinalIgnoreCase))
            {
                response = test.Method.ToUpper() switch
                {
                    "GET" => await _httpClient.GetAsync(test.Endpoint),
                    "POST" => await _httpClient.PostAsJsonAsync(test.Endpoint, test.Body),
                    "PUT" => await _httpClient.PutAsJsonAsync(test.Endpoint, test.Body),
                    "DELETE" => await SendDeleteWithBody(test.Endpoint, test.Body),
                    _ => throw new InvalidOperationException($"Unsupported HTTP method: {test.Method}")
                };
            }
            else if (test.Type.Equals("GraphQL", StringComparison.OrdinalIgnoreCase))
            {
                var graphQLRequest = new
                {
                    query = test.Query,
                    variables = test.Variables
                };
                response = await _httpClient.PostAsJsonAsync("/graphql", graphQLRequest);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported API type: {test.Type}");
            }

            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime - result.StartTime;
            result.StatusCode = (int)response.StatusCode;
            result.ResponseBody = await response.Content.ReadAsStringAsync();
            result.ResponseSize = result.ResponseBody.Length;
            result.Success = response.IsSuccessStatusCode;

            // Validate assertions
            if (test.Assertions != null)
            {
                result.AssertionResults = ValidateAssertions(
                    test.Assertions, 
                    result.ResponseBody, 
                    result.StatusCode);
                result.Success = result.Success && result.AssertionResults.All(a => a.Passed);
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime - result.StartTime;
        }

        return result;
    }

    private async Task<HttpResponseMessage> SendDeleteWithBody(string endpoint, object? body)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri(_httpClient.BaseAddress!, endpoint),
            Content = body != null ? JsonContent.Create(body) : null
        };
        return await _httpClient.SendAsync(request);
    }

    private List<AssertionResult> ValidateAssertions(
        Dictionary<string, object> assertions, 
        string responseBody, 
        int statusCode)
    {
        var results = new List<AssertionResult>();

        foreach (var assertion in assertions)
        {
            var assertionResult = new AssertionResult
            {
                AssertionType = assertion.Key,
                ExpectedValue = assertion.Value.ToString()
            };

            switch (assertion.Key.ToLower())
            {
                case "statuscode":
                    var expectedStatus = Convert.ToInt32(assertion.Value);
                    assertionResult.Passed = statusCode == expectedStatus;
                    assertionResult.ActualValue = statusCode.ToString();
                    break;

                case "contains":
                    var expectedString = assertion.Value.ToString();
                    assertionResult.Passed = responseBody.Contains(expectedString!);
                    assertionResult.ActualValue = assertionResult.Passed 
                        ? "Found" 
                        : "Not found";
                    break;

                default:
                    assertionResult.Passed = false;
                    assertionResult.ActualValue = "Unknown assertion type";
                    break;
            }

            results.Add(assertionResult);
        }

        return results;
    }
}
```

---

## Running Tests

### Console Test Runner

```csharp
// TestRunner/Program.cs
using RestVsGraphQL.Testing;

var baseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
var runner = new YamlTestRunner(baseUrl);

Console.WriteLine("=== GraphQL POC Test Runner ===\n");

// Run all test suites
var testSuites = new[]
{
    "TestSuites/rest-tests.yaml",
    "TestSuites/graphql-tests.yaml",
    "TestSuites/comparison-tests.yaml"
};

var allResults = new List<TestResults>();

foreach (var testSuite in testSuites)
{
    if (File.Exists(testSuite))
    {
        var results = await runner.RunTestsFromFile(testSuite);
        allResults.Add(results);
        Console.WriteLine();
    }
    else
    {
        Console.WriteLine($"⚠️  Test suite not found: {testSuite}\n");
    }
}

// Overall summary
Console.WriteLine("=== Overall Summary ===");
var totalTests = allResults.Sum(r => r.Results.Count);
var totalPassed = allResults.Sum(r => r.PassCount);
Console.WriteLine($"Total: {totalPassed}/{totalTests} tests passed");

if (totalPassed == totalTests)
{
    Console.WriteLine("✅ All tests passed!");
    Environment.Exit(0);
}
else
{
    Console.WriteLine("❌ Some tests failed");
    Environment.Exit(1);
}
```

### Run from Command Line

```bash
# Start the API (in one terminal)
cd RestVsGraphQL
dotnet run

# Run tests (in another terminal)
cd RestVsGraphQL
dotnet run --project TestRunner

# Or specify custom base URL
dotnet run --project TestRunner https://myapi.com
```

### Expected Output

```
=== GraphQL POC Test Runner ===

Running: REST API Tests
Description: Comprehensive REST endpoint validation
Tests: 8

✅ PASS - Get All Customers (45ms)
✅ PASS - Get Single Customer (12ms)
✅ PASS - Get Non-Existent Customer (8ms)
✅ PASS - Bulk Create Orders (123ms)
✅ PASS - Bulk Delete Orders (67ms)

Summary: 5/5 passed

Running: GraphQL API Tests
Description: Comprehensive GraphQL query and mutation validation
Tests: 12

✅ PASS - Get All Customers (38ms)
✅ PASS - Get Customer with Orders (52ms)
✅ PASS - Get Order with 4-Level Nesting (71ms)
✅ PASS - Get Dashboard Data (145ms)
✅ PASS - Create Single Order (89ms)
✅ PASS - Bulk Create Orders (156ms)
✅ PASS - Bulk Delete Orders (78ms)

Summary: 7/7 passed

Running: REST vs GraphQL Comparison Tests
Description: Side-by-side validation of identical operations
Tests: 10

✅ PASS - REST - Get Customer (15ms)
✅ PASS - GraphQL - Get Customer (42ms)
✅ PASS - REST - Order with Nested Data (87ms)
✅ PASS - GraphQL - Order with Nested Data (65ms)
✅ PASS - REST - Dashboard (134ms)
✅ PASS - GraphQL - Dashboard (128ms)
✅ PASS - REST - Bulk Create Orders (145ms)
✅ PASS - GraphQL - Bulk Create Orders (138ms)
✅ PASS - REST - Customer Info (Call 1 of 3) (18ms)
✅ PASS - REST - Customer Orders (Call 2 of 3) (22ms)
✅ PASS - REST - All Products (Call 3 of 3) (31ms)
✅ PASS - GraphQL - All Data in Single Call (56ms)

Summary: 12/12 passed

=== Overall Summary ===
Total: 24/24 tests passed
✅ All tests passed!
```

---

## CI/CD Integration

### GitHub Actions

```yaml
# .github/workflows/test.yml
name: API Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Start API
        run: |
          dotnet run --project RestVsGraphQL &
          sleep 10
      
      - name: Run Tests
        run: dotnet run --project TestRunner
      
      - name: Upload Test Results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: test-results
          path: TestResults/
```

---

## Best Practices

### ✅ Test Organization

1. **Separate test suites by purpose**
   - `rest-tests.yaml` - REST API validation
   - `graphql-tests.yaml` - GraphQL validation
   - `comparison-tests.yaml` - Side-by-side comparison

2. **Use descriptive names**
   ```yaml
   - name: "GraphQL - Bulk Create Orders with Partial Failure"
   ```

3. **Group related tests**
   ```yaml
   # Query tests
   - name: "Get All Customers"
   - name: "Get Single Customer"
   - name: "Get Non-Existent Customer"
   
   # Mutation tests
   - name: "Create Order"
   - name: "Update Order"
   - name: "Delete Order"
   ```

### ✅ Assertion Strategies

1. **Check status codes**
   ```yaml
   assertions:
     statusCode: 200  # Success
     statusCode: 404  # Not found
     statusCode: 400  # Bad request
   ```

2. **Verify response content**
   ```yaml
   assertions:
     contains: "successCount"
     contains: "customer"
   ```

3. **Multiple assertions**
   ```yaml
   assertions:
     statusCode: 200
     contains: "totalAmount"
     contains: "customer"
   ```

### ✅ Performance Testing

Add timing to tests:

```yaml
- name: "Performance - Complex Nested Query"
  type: "GraphQL"
  query: |
    { order(id: 1) { ... deeply nested ... } }
  assertions:
    statusCode: 200
  # Compare duration in test results
```

---

## Advanced Testing Patterns

### Variables in GraphQL Tests

```yaml
- name: "Parameterized GraphQL Query"
  type: "GraphQL"
  query: |
    query GetCustomer($id: Int!) {
      customer(id: $id) {
        id
        name
      }
    }
  variables:
    id: 1
  assertions:
    statusCode: 200
```

### Error Case Testing

```yaml
- name: "GraphQL - Invalid Customer ID"
  type: "GraphQL"
  query: |
    mutation {
      createOrder(orderDto: {
        customerId: 99999
        items: []
      }) {
        id
      }
    }
  assertions:
    statusCode: 200
    contains: "not found"
```

### Bulk Operation Validation

```yaml
- name: "Bulk Operation - Partial Success"
  type: "GraphQL"
  query: |
    mutation {
      bulkCreateOrders(request: {
        orders: [
          { customerId: 1, items: [...] },
          { customerId: 99999, items: [...] }
        ]
      }) {
        successCount
        failureCount
        errors
      }
    }
  assertions:
    statusCode: 200
    contains: "successCount"
    contains: "failureCount"
```

---

## Summary

**YAML-based testing provides:**

✅ **Declarative**: Tests as data, not code  
✅ **Maintainable**: Easy to add/modify tests  
✅ **Comprehensive**: Cover REST, GraphQL, and comparisons  
✅ **Automated**: CI/CD integration ready  
✅ **Performance**: Track response times automatically  

**Use for:**
- ✅ Regression testing
- ✅ Migration validation
- ✅ Performance benchmarking
- ✅ API contract verification

---

**Related Documentation:**
- [README.md](./README.md) - POC Overview
- [Migration-Guide.md](./Migration-Guide.md) - Migration Strategy

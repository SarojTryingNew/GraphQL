# Quick Start Guide

## Step 1: Start the API (5 minutes)

Open a terminal and run:
```powershell
cd RestVsGraphQL
dotnet restore
dotnet build
dotnet run
```

The API will start at `http://localhost:5072`

## Step 2: Test REST API (2 minutes)

Open another terminal and test with curl or your browser:

```powershell
# Get all customers
curl http://localhost:5072/api/customers

# Get order with nested data
curl http://localhost:5072/api/orders/1/nested

# Get dashboard
curl http://localhost:5072/api/dashboard
```

## Step 3: Test GraphQL API (2 minutes)

### Option A: Use the GraphQL IDE
Open your browser to: `http://localhost:5072/graphql`

Try this query:
```graphql
{
  customer(id: 1) {
    name
    email
    orders {
      id
      totalAmount
      items {
        quantity
        product {
          name
        }
      }
    }
  }
}
```

### Option B: Use curl
```powershell
curl -X POST http://localhost:5072/graphql `
  -H "Content-Type: application/json" `
  -d '{\"query\":\"{ customers { id name email } }\"}'
```

## Step 4: Run YAML Tests (5 minutes)

In a new terminal:
```powershell
cd RestVsGraphQL

# You can use the C# Interactive or create a simple runner
# For now, let's use PowerShell to call the testing code
```

Or create a simple test file:
```csharp
// TestRunner.cs
using RestVsGraphQL.Testing;

var runner = new YamlTestRunner("http://localhost:5072");

// Run REST tests
var restResults = await runner.RunTestsFromFile("TestSuites/rest-tests.yaml");
runner.PrintResults(restResults);

// Run GraphQL tests
var graphqlResults = await runner.RunTestsFromFile("TestSuites/graphql-tests.yaml");
runner.PrintResults(graphqlResults);

// Run comparison tests
var comparisonResults = await runner.RunTestsFromFile("TestSuites/comparison-tests.yaml");
runner.PrintResults(comparisonResults);
```

## Step 5: Run Performance Benchmarks (10 minutes)

**Important**: Make sure the API is running first!

```powershell
# Build in Release mode
dotnet build -c Release

# Run benchmarks (this will take a few minutes)
# Note: You'll need to set up a proper benchmark runner
# For now, you can test manually with tools like:
# - Apache Bench (ab)
# - wrk
# - k6
```

### Quick Performance Test with PowerShell

```powershell
# Time 100 REST requests
Measure-Command {
    1..100 | ForEach-Object {
        Invoke-RestMethod http://localhost:5072/api/orders/1/nested
    }
}

# Time 100 GraphQL requests
$query = @{
    query = "{ order(id: 1) { id orderDate customer { name } items { quantity product { name } } } }"
} | ConvertTo-Json

Measure-Command {
    1..100 | ForEach-Object {
        Invoke-RestMethod -Uri http://localhost:5072/graphql `
                         -Method Post `
                         -Body $query `
                         -ContentType "application/json"
    }
}
```

## Step 6: Compare Results

### Key Metrics to Compare:

1. **Response Time**
   - Single queries
   - Nested queries
   - Bulk operations

2. **Payload Size**
   - REST returns all fields
   - GraphQL returns only requested fields

3. **Number of Requests**
   - REST: Multiple endpoints
   - GraphQL: Single endpoint

4. **Flexibility**
   - REST: Fixed responses
   - GraphQL: Custom queries

## Sample Comparison Test

### Scenario: Get customer with their orders and products

**REST Approach** (3 requests):
```powershell
$customer = Invoke-RestMethod http://localhost:5072/api/customers/1
$orders = Invoke-RestMethod http://localhost:5072/api/customers/1/orders
$products = Invoke-RestMethod http://localhost:5072/api/products
```

**GraphQL Approach** (1 request):
```powershell
$query = @{
    query = @"
    {
        customer(id: 1) {
            id
            name
            email
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
"@
} | ConvertTo-Json

Invoke-RestMethod -Uri http://localhost:5072/graphql -Method Post -Body $query -ContentType "application/json"
```

## Expected Results Summary

| Scenario | REST | GraphQL | Winner |
|----------|------|---------|--------|
| Single record | Fast | Fast | Tie |
| Nested data (3 levels) | 1 request, over-fetching | 1 request, exact data | GraphQL |
| Multiple resources | 3+ requests | 1 request | GraphQL |
| Dashboard aggregation | 1 request | 1 request | Tie |
| Bulk operations | Similar | Similar | Tie |
| Caching | Excellent | Good | REST |
| Payload size | Larger | Smaller | GraphQL |

## Troubleshooting

### API won't start
```powershell
# Check if port 5000 is in use
netstat -ano | findstr :5000

# Kill the process if needed
taskkill /PID <PID> /F
```

### Cannot connect to API
- Ensure API is running in another terminal
- Check firewall settings
- Try `http://localhost:5072` instead of `https`

### GraphQL errors
- Check query syntax in Banana Cake Pop IDE
- Verify field names match the schema
- Check for required arguments

## Next Steps

1. ✅ You've tested both REST and GraphQL
2. 📊 Run the performance benchmarks
3. 📈 Analyze the results
4. 🎯 Decide: REST, GraphQL, or both?
5. 🚀 Build your application!

## Quick Reference

**REST Base URL**: `http://localhost:5072/api`
**GraphQL Endpoint**: `http://localhost:5072/graphql`
**GraphQL IDE**: `http://localhost:5072/graphql` (in browser)

**Key REST Endpoints**:
- `GET /api/customers`
- `GET /api/orders/{id}/nested`
- `GET /api/dashboard`
- `POST /api/orders/bulk`

**Key GraphQL Queries**:
- `{ customers { id name } }`
- `{ order(id: 1) { id customer { name } items { product { name } } } }`
- `{ dashboard { totalOrders totalRevenue topProducts { productName revenue } } }`

Enjoy exploring REST vs GraphQL! 🚀


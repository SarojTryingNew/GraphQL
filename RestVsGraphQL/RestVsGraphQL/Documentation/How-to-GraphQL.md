# How-to GraphQL in .NET 9

## Complete Implementation Guide

This guide demonstrates how to implement GraphQL in a .NET 9 application using HotChocolate, based on the POC implementation.

---

## Table of Contents

1. [Project Setup](#1-project-setup)
2. [Schema Design](#2-schema-design)
3. [Queries Implementation](#3-queries-implementation)
4. [Mutations Implementation](#4-mutations-implementation)
5. [DataLoaders (N+1 Prevention)](#5-dataloaders-n1-prevention)
6. [Bulk Operations](#6-bulk-operations)
7. [Testing GraphQL APIs](#7-testing-graphql-apis)
8. [Best Practices](#8-best-practices)

---

## 1. Project Setup

### Install NuGet Packages

```bash
dotnet add package HotChocolate.AspNetCore --version 14.2.0
```

### Configure Program.cs

```csharp
using RestVsGraphQL.GraphQL;
using RestVsGraphQL.Services;

var builder = WebApplication.CreateBuilder(args);

// Register your services
builder.Services.AddSingleton<DataStore>();
builder.Services.AddSingleton<OrderService>();

// Add GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()              // Root query type
    .AddMutationType<Mutation>()        // Root mutation type
    // Register DataLoaders for efficient batching
    .AddDataLoader<CustomerByIdDataLoader>()
    .AddDataLoader<ProductByIdDataLoader>()
    .AddDataLoader<OrdersByCustomerIdDataLoader>();

var app = builder.Build();

// Map GraphQL endpoint
app.MapGraphQL("/graphql");  // Serves GraphQL at /graphql

app.Run();
```

### Verify Installation

Navigate to `http://localhost:5000/graphql/` to access **Banana Cake Pop** (GraphQL IDE).

---

## 2. Schema Design

### Define Models

```csharp
// Models/Customer.cs
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<Order> Orders { get; set; } = new();
}

// Models/Order.cs
public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

// Models/OrderItem.cs
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public List<OrderItemNote> Notes { get; set; } = new();
}
```

### Create GraphQL Types (Optional)

HotChocolate can auto-generate types from your C# models, but you can customize them:

```csharp
// GraphQL/Types/CustomerType.cs
using HotChocolate.Types;

public class CustomerType : ObjectType<Customer>
{
    protected override void Configure(IObjectTypeDescriptor<Customer> descriptor)
    {
        descriptor.Description("Represents a customer in the system");
        
        descriptor
            .Field(c => c.Orders)
            .Description("Orders placed by this customer")
            .UseDataLoader<OrdersByCustomerIdDataLoader>();
    }
}
```

---

## 3. Queries Implementation

### Basic Query Type

```csharp
// GraphQL/Query.cs
using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL;

public class Query
{
    /// <summary>
    /// Get all customers
    /// </summary>
    public IEnumerable<Customer> GetCustomers([Service] DataStore dataStore)
        => dataStore.Customers;

    /// <summary>
    /// Get a single customer by ID
    /// </summary>
    public Customer? GetCustomer(int id, [Service] DataStore dataStore)
        => dataStore.Customers.FirstOrDefault(c => c.Id == id);

    /// <summary>
    /// Get all orders
    /// </summary>
    public IEnumerable<Order> GetOrders([Service] DataStore dataStore)
        => dataStore.Orders;

    /// <summary>
    /// Get a single order by ID with nested relations
    /// </summary>
    public Order? GetOrder(int id, [Service] DataStore dataStore)
    {
        var order = dataStore.Orders.FirstOrDefault(o => o.Id == id);
        if (order == null) return null;

        // Load relations (handled better by DataLoaders)
        order.LoadRelations(dataStore);
        
        return order;
    }

    /// <summary>
    /// Get multiple orders by IDs
    /// </summary>
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
}
```

### Example Queries

**Get all customers:**
```graphql
query {
  customers {
    id
    name
    email
  }
}
```

**Get customer with orders:**
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
      status
    }
  }
}
```

**Get order with nested data:**
```graphql
query {
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
```

---

## 4. Mutations Implementation

### Basic Mutation Type

```csharp
// GraphQL/Mutation.cs
using RestVsGraphQL.DTOs;
using RestVsGraphQL.Models;
using RestVsGraphQL.Services;

namespace RestVsGraphQL.GraphQL;

public class Mutation
{
    /// <summary>
    /// Create a single order with nested items
    /// </summary>
    public Order CreateOrder(
        OrderCreateDto orderDto, 
        [Service] DataStore dataStore)
    {
        // Input validation
        if (orderDto == null)
            throw new ArgumentNullException(nameof(orderDto));

        if (!orderDto.Items.Any())
            throw new ArgumentException("Order must contain at least one item");

        var customer = dataStore.Customers
            .FirstOrDefault(c => c.Id == orderDto.CustomerId);
        
        if (customer == null)
            throw new Exception($"Customer {orderDto.CustomerId} not found");

        // Create order
        var order = new Order
        {
            Id = dataStore.GetNextOrderId(),
            CustomerId = orderDto.CustomerId,
            Customer = customer,
            OrderDate = DateTime.UtcNow,
            Status = orderDto.Status
        };

        // Add items
        foreach (var itemDto in orderDto.Items)
        {
            var product = dataStore.Products
                .FirstOrDefault(p => p.Id == itemDto.ProductId);
            
            if (product == null)
                throw new Exception($"Product {itemDto.ProductId} not found");

            var orderItem = new OrderItem
            {
                Id = dataStore.GetNextOrderItemId(),
                OrderId = order.Id,
                ProductId = itemDto.ProductId,
                Product = product,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price,
                Discount = itemDto.Discount
            };

            // Add notes
            foreach (var noteContent in itemDto.Notes)
            {
                var note = new OrderItemNote
                {
                    Id = dataStore.GetNextOrderItemNoteId(),
                    OrderItemId = orderItem.Id,
                    Content = noteContent,
                    CreatedAt = DateTime.UtcNow
                };
                orderItem.Notes.Add(note);
                dataStore.OrderItemNotes.Add(note);
            }

            order.Items.Add(orderItem);
            dataStore.OrderItems.Add(orderItem);
        }

        order.RecalculateTotal();
        dataStore.Orders.Add(order);

        return order;
    }
}
```

### Input DTOs

```csharp
// DTOs/BulkOperationDtos.cs
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

### Example Mutation

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
        productId: 3
        quantity: 2
        discount: 0
        notes: []
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
```

---

## 5. DataLoaders (N+1 Prevention)

### The N+1 Problem

**Without DataLoaders:**
```graphql
query {
  orders {           # 1 query: get all orders
    id
    customer {       # N queries: one per order!
      name
    }
  }
}
```
This generates **1 + N** database queries (N = number of orders).

**With DataLoaders:**
- 1 query to get orders
- 1 **batched** query to get all customers
- **Total: 2 queries** regardless of N

### Implementing a DataLoader

```csharp
// GraphQL/DataLoaders/CustomerByIdDataLoader.cs
using RestVsGraphQL.Models;
using RestVsGraphQL.Services;
using GreenDonut;

namespace RestVsGraphQL.GraphQL.DataLoaders;

public class CustomerByIdDataLoader : BatchDataLoader<int, Customer>
{
    private readonly DataStore _dataStore;

    public CustomerByIdDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override async Task<IReadOnlyDictionary<int, Customer>> LoadBatchAsync(
        IReadOnlyList<int> keys, 
        CancellationToken cancellationToken)
    {
        // Batch load all customers in one query
        var customers = _dataStore.Customers
            .Where(c => keys.Contains(c.Id))
            .ToList();

        return customers.ToDictionary(c => c.Id);
    }
}
```

### Register DataLoader

```csharp
// Program.cs
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddDataLoader<CustomerByIdDataLoader>()
    .AddDataLoader<ProductByIdDataLoader>()
    .AddDataLoader<OrdersByCustomerIdDataLoader>();
```

### Use DataLoader in Type

```csharp
// GraphQL/Types/OrderType.cs
using HotChocolate.Types;

public class OrderType : ObjectType<Order>
{
    protected override void Configure(IObjectTypeDescriptor<Order> descriptor)
    {
        descriptor
            .Field(o => o.Customer)
            .ResolveWith<OrderResolvers>(r => r.GetCustomerAsync(default!, default!))
            .UseDataLoader<CustomerByIdDataLoader>();
    }
}

public class OrderResolvers
{
    public async Task<Customer?> GetCustomerAsync(
        [Parent] Order order,
        CustomerByIdDataLoader customerLoader)
    {
        return await customerLoader.LoadAsync(order.CustomerId);
    }
}
```

### DataLoader for Lists

```csharp
// GraphQL/DataLoaders/OrdersByCustomerIdDataLoader.cs
using GreenDonut;

public class OrdersByCustomerIdDataLoader 
    : GroupedDataLoader<int, Order>
{
    private readonly DataStore _dataStore;

    public OrdersByCustomerIdDataLoader(
        DataStore dataStore,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _dataStore = dataStore;
    }

    protected override async Task<ILookup<int, Order>> LoadGroupedBatchAsync(
        IReadOnlyList<int> keys, 
        CancellationToken cancellationToken)
    {
        var orders = _dataStore.Orders
            .Where(o => keys.Contains(o.CustomerId))
            .ToList();

        return orders.ToLookup(o => o.CustomerId);
    }
}
```

**Usage:**
```csharp
public class CustomerType : ObjectType<Customer>
{
    protected override void Configure(IObjectTypeDescriptor<Customer> descriptor)
    {
        descriptor
            .Field(c => c.Orders)
            .ResolveWith<CustomerResolvers>(r => r.GetOrdersAsync(default!, default!))
            .UseDataLoader<OrdersByCustomerIdDataLoader>();
    }
}
```

---

## 6. Bulk Operations

### Bulk Create Implementation

```csharp
// GraphQL/Mutation.cs
public class Mutation
{
    public BulkOperationResult BulkCreateOrders(
        BulkOrderCreateRequest request,
        [Service] OrderService orderService)
    {
        return orderService.BulkCreateOrders(request);
    }

    public BulkOperationResult BulkDeleteOrders(
        BulkOrderDeleteRequest request,
        [Service] OrderService orderService)
    {
        return orderService.BulkDeleteOrders(request);
    }
}
```

### Shared Service Layer

```csharp
// Services/OrderService.cs
public class OrderService
{
    private readonly DataStore _dataStore;

    public OrderService(DataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public BulkOperationResult BulkCreateOrders(BulkOrderCreateRequest request)
    {
        var result = new BulkOperationResult();

        foreach (var orderDto in request.Orders)
        {
            try
            {
                // Validation
                if (!orderDto.Items.Any())
                {
                    result.FailureCount++;
                    result.Errors.Add($"Order for customer {orderDto.CustomerId} has no items");
                    continue;
                }

                // Create order (reuse mutation logic or extract)
                var order = CreateSingleOrder(orderDto);
                
                result.SuccessCount++;
                result.CreatedIds.Add(order.Id);
            }
            catch (Exception ex)
            {
                result.FailureCount++;
                result.Errors.Add($"Customer {orderDto.CustomerId}: {ex.Message}");
            }
        }

        return result;
    }

    public BulkOperationResult BulkDeleteOrders(BulkOrderDeleteRequest request)
    {
        var result = new BulkOperationResult();

        foreach (var orderId in request.OrderIds)
        {
            try
            {
                var order = _dataStore.Orders.FirstOrDefault(o => o.Id == orderId);
                if (order == null)
                {
                    result.FailureCount++;
                    result.Errors.Add($"Order {orderId} not found");
                    continue;
                }

                // Cascade delete
                var itemsToRemove = _dataStore.OrderItems
                    .Where(oi => oi.OrderId == orderId)
                    .ToList();

                foreach (var item in itemsToRemove)
                {
                    var notesToRemove = _dataStore.OrderItemNotes
                        .Where(n => n.OrderItemId == item.Id)
                        .ToList();
                    
                    notesToRemove.ForEach(n => _dataStore.OrderItemNotes.Remove(n));
                    _dataStore.OrderItems.Remove(item);
                }

                _dataStore.Orders.Remove(order);

                result.SuccessCount++;
                result.DeletedIds.Add(orderId);
            }
            catch (Exception ex)
            {
                result.FailureCount++;
                result.Errors.Add($"Order {orderId}: {ex.Message}");
            }
        }

        return result;
    }
}
```

### Example Bulk Mutation

```graphql
mutation {
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

**Response:**
```json
{
  "data": {
    "bulkCreateOrders": {
      "successCount": 2,
      "failureCount": 0,
      "createdIds": [101, 102],
      "errors": []
    }
  }
}
```

---

## 7. Testing GraphQL APIs

### YAML-Based Declarative Testing

```yaml
# TestSuites/graphql-tests.yaml
name: "GraphQL API Tests"
description: "Automated test suite for GraphQL endpoints"

tests:
  - name: "Get All Customers"
    type: "GraphQL"
    query: |
      {
        customers {
          id
          name
          email
        }
      }
    assertions:
      statusCode: 200
      contains: "customers"

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
                { productId: 1, quantity: 5, discount: 0, notes: [] }
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
      contains: "successCount"
```

### Test Runner

```csharp
// Testing/YamlTestRunner.cs
public class YamlTestRunner
{
    private readonly HttpClient _httpClient;

    public async Task<TestResult> ExecuteGraphQLTest(ApiTest test)
    {
        var graphQLRequest = new
        {
            query = test.Query,
            variables = test.Variables
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/graphql", 
            graphQLRequest);

        var result = new TestResult
        {
            TestName = test.Name,
            StatusCode = (int)response.StatusCode,
            ResponseBody = await response.Content.ReadAsStringAsync(),
            Success = response.IsSuccessStatusCode
        };

        return result;
    }
}
```

### Run Tests

```bash
dotnet run --project TestRunner
```

---

## 8. Best Practices

### ✅ Schema Design

1. **Use nullable types appropriately**
   ```graphql
   type Customer {
     id: Int!           # Required
     name: String!      # Required
     phone: String      # Optional
   }
   ```

2. **Design for evolution**
   - Never remove fields (deprecate instead)
   - Use `@deprecated` directive
   ```csharp
   descriptor.Field(c => c.OldField)
       .Deprecated("Use newField instead");
   ```

3. **Use Input Types for mutations**
   ```csharp
   public Order CreateOrder(OrderCreateDto input, [Service] DataStore ds)
   ```

### ✅ Performance

1. **Always use DataLoaders for relations**
   - Prevents N+1 queries
   - Automatic batching

2. **Implement pagination for large lists**
   ```csharp
   public IEnumerable<Order> GetOrders(
       int skip = 0, 
       int take = 20, 
       [Service] DataStore ds)
   {
       return ds.Orders.Skip(skip).Take(take);
   }
   ```

3. **Use projection to minimize data fetching**
   - HotChocolate automatically projects only requested fields

### ✅ Error Handling

1. **Use exceptions for domain errors**
   ```csharp
   if (customer == null)
       throw new Exception($"Customer {id} not found");
   ```

2. **Return structured errors in bulk operations**
   ```csharp
   public class BulkOperationResult
   {
       public int SuccessCount { get; set; }
       public int FailureCount { get; set; }
       public List<string> Errors { get; set; } = new();
   }
   ```

### ✅ Security

1. **Validate all inputs**
   ```csharp
   if (orderDto.Items.Any(i => i.Quantity <= 0))
       throw new ArgumentException("Quantity must be > 0");
   ```

2. **Implement authorization**
   ```csharp
   descriptor.Field(c => c.SensitiveData)
       .Authorize("AdminPolicy");
   ```

3. **Limit query depth/complexity**
   ```csharp
   builder.Services
       .AddGraphQLServer()
       .ModifyRequestOptions(opt => opt.MaxAllowedExecutionDepth = 10);
   ```

### ✅ Code Organization

1. **Share business logic between REST and GraphQL**
   ```
   Services/
   ├── OrderService.cs        # Shared by REST & GraphQL
   ├── CustomerService.cs
   └── ValidationService.cs
   ```

2. **Separate concerns**
   ```
   GraphQL/
   ├── Query.cs               # All queries
   ├── Mutation.cs            # All mutations
   ├── Types/                 # Type customizations
   └── DataLoaders/           # Batching logic
   ```

---

## Quick Reference

### Common GraphQL Operations

**Query**
```graphql
query GetOrder($id: Int!) {
  order(id: $id) {
    id
    totalAmount
  }
}
```

**Mutation**
```graphql
mutation CreateOrder($input: OrderCreateDto!) {
  createOrder(orderDto: $input) {
    id
    totalAmount
  }
}
```

**Variables**
```json
{
  "id": 1,
  "input": {
    "customerId": 1,
    "status": "Pending",
    "items": []
  }
}
```

### HotChocolate Attributes

| Attribute | Purpose |
|-----------|---------|
| `[Service]` | Inject dependency |
| `[Parent]` | Access parent object in resolver |
| `[GraphQLName("customName")]` | Override field name |
| `[GraphQLDescription("...")]` | Add description |
| `[Authorize]` | Require authentication |

---

## Further Reading

- **HotChocolate Docs**: https://chillicream.com/docs/hotchocolate
- **GraphQL Spec**: https://spec.graphql.org/
- **DataLoader Pattern**: https://github.com/graphql/dataloader

---

**Next**: [GraphQL Query Model](./GraphQL-Query-Model.md)

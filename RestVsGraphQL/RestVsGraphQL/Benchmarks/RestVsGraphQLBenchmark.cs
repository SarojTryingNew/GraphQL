using BenchmarkDotNet.Attributes;
using RestVsGraphQL.DTOs;
using RestVsGraphQL.Services;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace RestVsGraphQL.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class RestVsGraphQLBenchmark
{
    private HttpClient _httpClient = null!;
    private const string BaseUrl = "http://localhost:5000";

    [GlobalSetup]
    public void Setup()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _httpClient?.Dispose();
    }

    [Benchmark(Description = "REST: Get Single Order with Nested Data")]
    public async Task<string> RestGetOrderNested()
    {
        var response = await _httpClient.GetAsync("/api/orders/1/nested");
        return await response.Content.ReadAsStringAsync();
    }

    [Benchmark(Description = "GraphQL: Get Single Order with Nested Data")]
    public async Task<string> GraphQLGetOrderNested()
    {
        var query = """
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
        """;

        var request = new { query };
        var response = await _httpClient.PostAsJsonAsync("/graphql", request);
        return await response.Content.ReadAsStringAsync();
    }

    [Benchmark(Description = "REST: Get Dashboard (Aggregated Data)")]
    public async Task<string> RestGetDashboard()
    {
        var response = await _httpClient.GetAsync("/api/dashboard");
        return await response.Content.ReadAsStringAsync();
    }

    [Benchmark(Description = "GraphQL: Get Dashboard (Aggregated Data)")]
    public async Task<string> GraphQLGetDashboard()
    {
        var query = """
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
            }
        }
        """;

        var request = new { query };
        var response = await _httpClient.PostAsJsonAsync("/graphql", request);
        return await response.Content.ReadAsStringAsync();
    }

    [Benchmark(Description = "REST: Bulk Create Orders (10 orders)")]
    public async Task<string> RestBulkCreateOrders()
    {
        var bulkRequest = new BulkOrderCreateRequest
        {
            Orders = Enumerable.Range(1, 10).Select(i => new OrderCreateDto
            {
                CustomerId = (i % 5) + 1,
                Status = "Pending",
                Items = new List<OrderItemCreateDto>
                {
                    new() { ProductId = 1, Quantity = 2, Discount = 5, Notes = new List<string> { "Test note" } },
                    new() { ProductId = 2, Quantity = 1, Discount = 0, Notes = new List<string>() }
                }
            }).ToList()
        };

        var response = await _httpClient.PostAsJsonAsync("/api/orders/bulk", bulkRequest);
        return await response.Content.ReadAsStringAsync();
    }

    [Benchmark(Description = "GraphQL: Bulk Create Orders (10 orders)")]
    public async Task<string> GraphQLBulkCreateOrders()
    {
        var mutation = """
        mutation($request: BulkOrderCreateRequestInput!) {
            bulkCreateOrders(request: $request) {
                successCount
                failureCount
                errors
                createdIds
            }
        }
        """;

        var variables = new
        {
            request = new
            {
                orders = Enumerable.Range(1, 10).Select(i => new
                {
                    customerId = (i % 5) + 1,
                    status = "Pending",
                    items = new[]
                    {
                        new { productId = 1, quantity = 2, discount = 5.0, notes = new[] { "Test note" } },
                        new { productId = 2, quantity = 1, discount = 0.0, notes = Array.Empty<string>() }
                    }
                }).ToArray()
            }
        };

        var request = new { query = mutation, variables };
        var response = await _httpClient.PostAsJsonAsync("/graphql", request);
        return await response.Content.ReadAsStringAsync();
    }

    [Benchmark(Description = "REST: Multiple Dependent Calls (Customer + Orders + Products)")]
    public async Task<string> RestMultipleDependentCalls()
    {
        var customer = await _httpClient.GetAsync("/api/customers/1");
        var orders = await _httpClient.GetAsync("/api/customers/1/orders");
        var products = await _httpClient.GetAsync("/api/products");

        return await customer.Content.ReadAsStringAsync() + 
               await orders.Content.ReadAsStringAsync() + 
               await products.Content.ReadAsStringAsync();
    }

    [Benchmark(Description = "GraphQL: Single Call for Multiple Resources")]
    public async Task<string> GraphQLSingleCallMultipleResources()
    {
        var query = """
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
            products {
                id
                name
                price
                stockQuantity
            }
        }
        """;

        var request = new { query };
        var response = await _httpClient.PostAsJsonAsync("/graphql", request);
        return await response.Content.ReadAsStringAsync();
    }
}
